using JetBrains.ProjectModel;
using JetBrains.Util;
using ReSharperPlugin.DependencyMonkey.QuikGraph.Structures.Edges;
using ReSharperPlugin.DependencyMonkey.QuikGraph.Structures.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReSharperPlugin.DependencyMonkey.Model;

public class SolutionGraph
{
    private readonly ISolution _solution;
    private readonly IProject _startingPoint;
    private readonly BidirectionalGraph<ProjectToUpgrade, Edge<ProjectToUpgrade>> _graph;

    public ProjectToUpgrade StartProject { get; private set; }

    private readonly Dictionary<Guid, ProjectToUpgrade> _projectMap;

    public Queue<ProjectToUpgrade> UpgradeOrder { get; private set; }

    public int AmountOfItemsToUpgrade { get; private set; }

    private bool IsCsproj(IProject project)
    {
        if (project?.ProjectFile is null)
            return false;

        return project.ProjectFile.Name.EndsWith(".csproj");
    }

    public SolutionGraph(ISolution solution, IProject startingPoint)
    {
        _solution = solution;
        _startingPoint = startingPoint;
        _projectMap = new Dictionary<Guid, ProjectToUpgrade>();
        _graph = new BidirectionalGraph<ProjectToUpgrade, Edge<ProjectToUpgrade>>(false);
    }

    public void Build()
    {
        BuildDependencyGraphFromSolution(_solution);
        StartProject = _projectMap[_startingPoint.Guid];
        BuildUpgradeOrder();
    }

    private void AddDependenciesRecursively(ProjectToUpgrade source, BidirectionalGraph<ProjectToUpgrade, Edge<ProjectToUpgrade>> graph)
    {
        foreach (IProjectToPackageReference dep in source.Project.GetAllPackagesReferences())
        {
            var depAssemblyName = dep.Name;
            // ignore self dependency, just so we dont die, in case it does happen.
            if (source.AssemblyName == dep.Name)
            {
                continue;
            }

            var project = _projectMap.Values.FirstOrDefault(x => x.AssemblyName == depAssemblyName);

            if (project != null)
            {
                graph.AddEdge(new ProjectToUpgradeEdge(source, project));
                AddDependenciesRecursively(project, graph);
            }
        }
    }

    public IEnumerable<ProjectToUpgrade> GetDependents(ProjectToUpgrade project)
    {
        return _graph.InEdges(project).Select(x => x.Source);
    }

    public IEnumerable<ProjectToUpgrade> GetDependencies(ProjectToUpgrade projectToUpgrade)
    {
        return _graph.OutEdges(projectToUpgrade).Select(x => x.Target);
    }

    /// <summary>
    /// Analyses all csharp projects inside a solution and builds a dependency graph out of it.
    /// Only considers package references, which source files are in the same solution.
    /// </summary>
    /// <param name="solution">The solution to analyze.</param>
    private void BuildDependencyGraphFromSolution(ISolution solution)
    {
        var projects = solution.GetAllProjects();

        foreach (IProject project in projects)
        {
            if (!IsCsproj(project))
                continue;
            AddVertex(project);
        }


        foreach (IProject project in projects)
        {
            if (!IsCsproj(project))
                continue;

            var ptu = _projectMap[project.Guid];

            AddDependenciesRecursively(ptu, _graph);
        }
    }

    private void AddVertex(IProject project)
    {
        ProjectToUpgrade ptu = new ProjectToUpgrade(project);
        _projectMap[ptu.Project.Guid] = ptu;

        _graph.AddVertex(ptu);
    }

    /// <summary>
    /// Uses the dependency graph to build the correct order in which the packages need to be updated.
    /// The algorithm used is a mix between BFS and DFS.
    /// The algorithm can be described in the following steps:
    /// The BFS part boils down to:
    /// 1. Mark the project as 'needUpgrade'.
    /// 2. Mark all dependent projects as 'needsUpgrade' and add them to the 'explorerQueue'
    /// The DFS part of the algorithm boils down to:
    /// 1. Grab a project from the queue.
    /// 2. Explore all dependencies of the project.
    /// 3. Perform 2. until we reached a project which has dependencies on a project which was set to 'needsUpgrade'.
    /// 4. Mark the project as 'needsUpgrade' and add it to the 'upgradeQueue'.
    /// </summary>
    private void BuildUpgradeOrder()
    {
        UpgradeOrder = new Queue<ProjectToUpgrade>();

        var explorerQueue = new Queue<ProjectToUpgrade>();

        UpgradeOrder.Enqueue(StartProject);
        StartProject.WasVisited = true;
        StartProject.NeedsToBeUpdated = true;
        foreach (var dep in GetDependents(StartProject))
        {
            dep.NeedsToBeUpdated = true;
            explorerQueue.Enqueue(dep);
        }

        while (!explorerQueue.IsEmpty())
        {
            ProjectToUpgrade p = explorerQueue.Dequeue();
            Enqueue(p, UpgradeOrder, explorerQueue);
        }

        AmountOfItemsToUpgrade = UpgradeOrder.Count;
    }

    private void Enqueue(ProjectToUpgrade ptu, Queue<ProjectToUpgrade> orderQueue, Queue<ProjectToUpgrade> explorerQueue)
    {
        if (ptu.WasVisited)
        {
            return;
        }

        // DFS part
        var dependencies = GetDependencies(ptu).ToArray();
        foreach (var depDependency in GetDependencies(ptu))
        {
            Enqueue(depDependency, orderQueue, explorerQueue);
        }

        ptu.WasVisited = true;
        // projects without dependencies, cannot be changed, and therefore never have to be updated
        if (dependencies.Length == 0)
            return;

        // projects which dependencies were visited (changed) do not need to be included.
        if (!dependencies.Any(x => x.NeedsToBeUpdated))
        {
            return;
        }

        ptu.NeedsToBeUpdated = true;
        orderQueue.Enqueue(ptu);

        // BFS part
        IEnumerable<ProjectToUpgrade> dependents = GetDependents(ptu);
        foreach (ProjectToUpgrade dep in dependents)
        {
            dep.NeedsToBeUpdated = true;
            explorerQueue.Enqueue(dep);
        }
    }
}