using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Controls;
using JetBrains.DataFlow;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Features.SolutionBuilders;
using JetBrains.ProjectModel.NuGet.Configs;
using JetBrains.ProjectModel.NuGet.Operations;
using JetBrains.ProjectModel.NuGet.PackageManagement;
using JetBrains.ProjectModel.NuGet.Packaging;
using JetBrains.ProjectModel.NuGet.Protocol;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Threading;
using JetBrains.Util;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using ReSharperPlugin.DependencyMonkey.Model;
using ReSharperPlugin.DependencyMonkey.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReSharperPlugin.DependencyMonkey;

[SolutionComponent]
public class NugetUpgrader
{
    private readonly Lifetime _componentLifetime;
    private readonly DependencyMonkeyUserNotifications _notifications;
    private readonly ISolutionBuilder _solutionBuilder;
    private readonly IShellLocks _shellLocks;
    private readonly ISolution _solution;

    private SolutionGraph _graph;
    private DependencyMonkeySettings _settings;

    private ProjectToUpgrade _currentProjectToUpgrade;

    private uint _expectedBuildSessionId;
    private IProgressIndicator _backgroundProgress;
    private LifetimeDefinition _upgradeLifetime;

    private readonly IBackgroundProgressIndicatorManager _backgroundProgressIndicatorManager;

    private VersionBumper _versionBumper;

    public NugetUpgrader(Lifetime componentLifetime,
        ISolutionBuilder solutionBuilder,
        IShellLocks shellLocks,
        DependencyMonkeyUserNotifications notifications,
        ISolution solution, IBackgroundProgressIndicatorManager backgroundProgressIndicatorManager)
    {
        _componentLifetime = componentLifetime;
        _backgroundProgressIndicatorManager = backgroundProgressIndicatorManager;

        _notifications = notifications;
        _solutionBuilder = solutionBuilder;
        _shellLocks = shellLocks;
        _solution = solution;
    }

    public bool IsAvailable => !_solutionBuilder.IsRunning();

    private void HandleRunningRequestChange(Lifetime lifetime, PropertyChangedEventArgs<SolutionBuilderRequest> change)
    {
        SolutionBuilderRequest request = change.Property.Value;
        if (_currentProjectToUpgrade == null || request == null || _expectedBuildSessionId != request.SessionId)
            return;

        var handler = CreateHandler(request);
        if (handler == null) return;

        request.AfterBuildCompleted.Advise(request.Lifetime, handler);

        // request?.State.Change.Advise(lifetime, state =>
        // {
        //     if (!state.HasNew || !state.New.HasFlag(BuildRunState.Completed))
        //         return;
        //
        //     handler();
        // });
    }

    private Action CreateHandler(SolutionBuilderRequest request)
    {
        if (request.BuildSessionTarget == BuildSessionTarget.Build)
        {
            return () =>
            {
                if (request.Suceeded.Value)
                {
                    HandleBuildCompletion(_currentProjectToUpgrade);
                }
                else
                {
                    HandleError();
                }
            };
        }

        return null;
    }

    private void Reset()
    {
        _currentProjectToUpgrade = null;
        _graph = null;
        _expectedBuildSessionId = 0;
        _upgradeLifetime?.Terminate();
    }

    private void PerformNextUpgrade()
    {
        if (!_graph.UpgradeOrder.IsEmpty())
        {
            _currentProjectToUpgrade = _graph.UpgradeOrder.Dequeue();
            _backgroundProgress.CurrentItemText = _currentProjectToUpgrade.Project.Name;
            _backgroundProgress.Advance(1);
            InstallDependenciesInBackground(_currentProjectToUpgrade);
        }
        else
        {
            _notifications.ShowInfoNotification($"Upgraded {_graph.AmountOfItemsToUpgrade} projects.");
            Reset();
        }
    }

    public void UpgradeProjectAndDependencies(IProject project, VersionUpdateOperation operation)
    {
        try
        {
            if (_solutionBuilder.IsRunning())
                return;
            Reset();

            _upgradeLifetime = _componentLifetime.CreateNested();
            _backgroundProgress = _backgroundProgressIndicatorManager.CreateBackgroundProgress(_upgradeLifetime.Lifetime, "Dependency Monkey", Reset);
            _solutionBuilder.RunningRequest.Change.Advise(_upgradeLifetime.Lifetime,
                (change) =>
                {
                    try
                    {
                        HandleRunningRequestChange(_upgradeLifetime.Lifetime, change);
                    }
                    catch (LifetimeCanceledException ex)
                    {
                    }
                });

            ISolution solution = project.GetSolution();
            _graph = new SolutionGraph(solution, project);
            _graph.Build();

            _backgroundProgress.Start(_graph.UpgradeOrder.Count);

            var settingsStore = solution.GetComponent<ISettingsStore>();
            _settings = settingsStore.BindToContextTransient(ContextRange.ApplicationWide)
                .GetKey<DependencyMonkeySettings>(SettingsOptimization.OptimizeDefault);
            _versionBumper = new VersionBumper(_settings.VersionIncreaseStrategy, operation, _settings.PreReleaseTag);

            PerformNextUpgrade();
        }
        catch (LifetimeCanceledException ex)
        {
        }
    }

    private void IncreaseProjectVersionAndBuild(ProjectToUpgrade projectToUpgrade)
    {
        _shellLocks.ExecuteOrQueueEx(_upgradeLifetime.Lifetime, "NugetUpgrader::PerformUpgrade", () =>
        {
            _backgroundProgress.TaskName = "Adjusting project version";
            if (projectToUpgrade.CanIncreaseVersion)
            {
                IncreaseVersion(projectToUpgrade);
            }

            if (projectToUpgrade.DoesGeneratePackageOnBuild)
            {
                SendBuildRequest(projectToUpgrade);
            }
            else
            {
                PerformNextUpgrade();
            }
        });
    }

    private void IncreaseVersion(ProjectToUpgrade projectToUpgrade)
    {
        _shellLocks.ExecuteWithReadLock(() =>
        {
            try
            {
                var newVersion = _versionBumper.UpdateVersion(projectToUpgrade.OriginalVersion);
                projectToUpgrade.SetVersion(newVersion);
            }
            catch (Exception e)
            {
                HandleError($"{projectToUpgrade.Project.Name}: {e.Message}");
            }
        });
    }

    private void SendBuildRequest(ProjectToUpgrade projectToUpgrade)
    {
        _backgroundProgress.TaskName = "Building project";
        SolutionBuilderRequest buildRequest = _solutionBuilder.CreateBuildRequest(BuildSessionTarget.Build,
            new[] { projectToUpgrade.Project },
            SolutionBuilderRequestSilentMode.Default);
        _expectedBuildSessionId = buildRequest.SessionId;

        _solutionBuilder.ExecuteBuildRequest(buildRequest);
    }

    private void HandleBuildCompletion(ProjectToUpgrade ptu)
    {
        _backgroundProgress.TaskName = "Moving nuget packages";
        MoveNugetPackage(ptu);

        PerformNextUpgrade();
    }

    private void InstallDependenciesInBackground(ProjectToUpgrade ptu)
    {
        _shellLocks.StartBackground(_upgradeLifetime.Lifetime, async () =>
        {
            _backgroundProgress.TaskName = "Installing NuGet packages";
            await InstallNugetDependenciesForProject(ptu);
            IncreaseProjectVersionAndBuild(ptu);
        }).NoAwait();
    }

    private void HandleError([CanBeNull] string message = null)
    {
        Reset();
        if (!string.IsNullOrEmpty(message))
        {
            _notifications.ShowErrorNotification(message);
        }
    }

    private string GetOutputPathForUpgradeOperation(IProject project)
    {
        if (_versionBumper.UpdateOperation == VersionUpdateOperation.IncrementPrerelease)
            return Path.Combine(project.Location.FullPath, "bin", "Debug");

        return Path.Combine(project.Location.FullPath, "bin", "Release");
    }

    private void MoveNugetPackage(ProjectToUpgrade ptu)
    {
        var project = ptu.Project;
        var outputPath = GetOutputPathForUpgradeOperation(project);
        var assemblyName = project.GetOutputAssemblyName(project.TargetFrameworkIds.First());

        var version = ptu.UpgradedVersion.ToNormalizedString();
        var nugetFile = $"{assemblyName}.{version}.nupkg";
        var absolutePathToNugetPackage = Path.Combine(outputPath, nugetFile);

        if (File.Exists(absolutePathToNugetPackage))
        {
            var settingsStore = project.GetComponent<ISettingsStore>();
            var sampleSettings = settingsStore.BindToContextTransient(ContextRange.ApplicationWide)
                .GetKey<DependencyMonkeySettings>(SettingsOptimization.OptimizeDefault);

            if (string.IsNullOrEmpty(sampleSettings.FolderPath))
            {
                HandleError(
                    $"Unable to copy generated nuget package {nugetFile}, because there is no copy directory set. Please set it in the settings.");
                return;
            }

            var localNugetFolder = sampleSettings.FolderPath;
            string newAbsolutePathToNugetPackage = Path.Combine(localNugetFolder, nugetFile);

            if (File.Exists(newAbsolutePathToNugetPackage))
            {
                _notifications.ShowWarningNotification($"Failed to move {nugetFile} file, because {newAbsolutePathToNugetPackage} already exists.");
            }
            else
            {
                File.Move(absolutePathToNugetPackage, newAbsolutePathToNugetPackage);

                _notifications.ShowInfoNotification($"Successfully upgraded {assemblyName} to version {version} and moved it to {localNugetFolder}.",
                    assemblyName);
            }
        }
        else
        {
            HandleError(
                $"Unable to find the package {absolutePathToNugetPackage}. Are you sure that the build results in the generation of a nuget package?");
        }
    }

    private async Task InstallNugetDependenciesForProject(ProjectToUpgrade ptu)
    {
        var dependencies = _graph.GetDependencies(ptu).Where(x => x.WasChanged).ToArray();
        if (dependencies.Length == 0)
            return;

        List<NuGetProjectWithIdentity> packages = dependencies.Select(dep =>
        {
            var packageIdentity = new PackageIdentity(dep.AssemblyName, dep.UpgradedVersion);

            return new NuGetProjectWithIdentity(ptu.Project, packageIdentity);
        }).ToList();
        // TODO: if we are really cool, we can grab this from the nuget settings in rider
        var resolutionContext = NuGetResolutionContextFactory.Create(DependencyBehavior.Lowest, true, false, VersionConstraints.None);

        NuGetFeedContext context = CreateNuGetFeedContext();
        if (context == null)
            return;

        var nuGetOperator = _solution.GetComponent<NuGetOperator>();

        NuGetOperationAggregatedResult result = await nuGetOperator.InstallMultiAsync(packages,
            resolutionContext, context);

        if (result.Errors.Count > 0)
        {
            HandleError(string.Join(";", result.Errors));
        }
    }

    private NuGetFeedContext CreateNuGetFeedContext()
    {
        var factory = Shell.Instance.GetComponent<NuGetResourceProviderFactory>();
        NuGetFeedManager nuGetFeedManager = _solution.GetComponent<NuGetFeedManager>();

        var allFeeds = nuGetFeedManager.GetAllKnownFeeds();

        var localFeed = allFeeds.FirstOrDefault(x => x.Url == _settings.FolderPath);

        if (localFeed is null)
        {
            HandleError($"Your specified output folder {_settings.FolderPath} is not a valid NuGet feed!");
            return null;
        }


        // TODO: I don't know if nullsettings.instance is correct, or what effect it will have.
        ISourceRepositoryProvider repository = new NuGetSourceRepositoryProvider(factory, NullSettings.Instance);
        var context = NuGetFeedContext.FromTargetFeed(repository, localFeed, allFeeds);
        return context;
    }
}