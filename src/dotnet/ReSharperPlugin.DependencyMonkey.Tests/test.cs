using FluentAssertions;
using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions.ActionManager;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ProjectModel.Features.SolutionBuilders;
using JetBrains.ReSharper.TestFramework;
using JetBrains.Util;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NUnit.Framework;
using ReSharperPlugin.DependencyMonkey.Actions;
using ReSharperPlugin.DependencyMonkey.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ReSharperPlugin.DependencyMonkey.Tests;

[TestFixture]
public class Test01 : BaseTestWithExistingSolution
{
    protected override IEnumerable<PackageDependency> GetTestDataPackages()
    {
        return Enumerable.Empty<PackageDependency>();
    }
    
    public static string GetPathRelativeToSolution(params string[] path)
    {
        return Path.Combine(new[] { GetDirectoryOfThisSoureFile(), "..", "..", ".." }.Concat(path).ToArray());
    }

    private static string GetDirectoryOfThisSoureFile([CallerFilePath] string filePath = null)
    {
        return Path.GetDirectoryName(filePath).NotNull();
    }

    [Test]
    public void Test()
    {
        string pathToTestSolution = GetPathRelativeToSolution("test\\data\\Test01\\ClassLibrary1.sln");
        FileSystemPath solutionFilePath = FileSystemPath.Parse(pathToTestSolution);

        ExecuteWithinSettingsTransaction(ctx =>
        {
            DoTestSolution(solutionFilePath, (lifetime, solution) =>
            {
                var runner = solution.GetComponents<ISolutionBuilderRunner>().ToList();
                var sb = solution.GetComponents<ISolutionBuilder>().ToList();
                IProject project = solution.GetProjectsByName("ClassLibrary1").First();
                var am = solution.GetComponent<IActionManager>();

                IList<IDataRule> rules = DataRules.AddRule("TEST", ProjectModelDataConstants.PROJECT_MODEL_ELEMENT, _ => project);

                var action = new UpgradeMajorVersionAction();

                action.Execute(am.DataContexts.CreateWithDataRules(lifetime, rules), null!);

                var a = new ProjectToUpgrade(project);

                a.OriginalVersion.Should().BeEquivalentTo(new NuGetVersion(2, 0, 0));
            });
        });
    }
}