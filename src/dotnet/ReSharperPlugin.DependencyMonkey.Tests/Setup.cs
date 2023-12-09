using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.Environment;
using JetBrains.Debugger.Host;
using JetBrains.IDE.Debugger;
using JetBrains.Platform.RdFramework;
using JetBrains.Platform.RdFramework.Actions.Backend;
using JetBrains.ProjectModel.NuGet;
using JetBrains.ProjectModel.NuGet.Searching;
using JetBrains.RdBackend.Common.Env;
using JetBrains.ReSharper.ExternalSources.ILViewer;
using JetBrains.ReSharper.Feature.Services.AI;
using JetBrains.ReSharper.Feature.Services.Breadcrumbs;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.ExternalSource;
using JetBrains.ReSharper.Feature.Services.ExternalSources;
using JetBrains.ReSharper.Feature.Services.PackageChecker;
using JetBrains.ReSharper.Features.ReSpeller;
using JetBrains.ReSharper.Features.Running;
using JetBrains.ReSharper.Features.SolBuilderDuo;
using JetBrains.ReSharper.Features.Xaml.Previewer.Host;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Impl.Reflection2;
using JetBrains.ReSharper.Psi.Razor;
using JetBrains.ReSharper.Psi.VB;
using JetBrains.ReSharper.Psi.Xaml;
using JetBrains.ReSharper.TestFramework;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Rider.Backend.Env;
using JetBrains.Rider.Backend.Product;
using JetBrains.Rider.Model;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;
using ReSharperPlugin.DependencyMonkey.Tests;
using System.Threading;

[assembly: Apartment(ApartmentState.STA)]

namespace ReSharperPlugin.DependencyMonkey.Tests
{
    // riderIncrementalRunner
    [ZoneDefinition]
    public interface IDependencyMonkeyTestEnvironmentZone : ITestsEnvZone,
    //         IRequire<JetBrains.Rider.Backend.Env.ZonesActivator>,
    //
    IRequire<PsiFeatureTestZone>,
    // IRequire<ISinceClr4HostZone>,
    // IRequire<IResharperHostCoreFeatureZone>,
    // IRequire<IRiderModelZone>,
    // IRequire<IRiderPlatformZone>,
    // IRequire<INuGetZone>,
    // IRequire<INuGetSearchZone>,
    // IRequire<IDebuggerZone>,
    // IRequire<IRiderDebuggerZone>,
    // IRequire<IOuterWorldConnectEnvZone>,
    // IRequire<IPsiAssemblyFileLoaderImplZone>,
    // IRequire<MetadataTreeZone>,
    // IRequire<SolutionBuilderDuoZone>,
    // IRequire<RunnableProjectsZone>,
    // IRequire<IPackageCheckerZone>,
    IRequire<IDependencyMonkeyZone>
    //         IRequire<ISolutionBuilderFeatureZone>,
    //         IRequire<IRiderPlatformZone>,
    //         IRequire<IRdActionsBackendZone>,
    //         IRequire<IRiderModelZone>,
    //         IRequire<SolutionBuilderDuoZone>,
    //         IRequire<IRdFrameworkZone>

        // IRequire<IRiderFeatureZone>,
        // IRequire<IRiderFeatureEnvironmentZone>
        // IActivate<PsiFeatureTestZone>,
        // IActivate<IRiderPlatformZone>,
        // IActivate<IRiderFeatureZone>,
        // IActivate<IRiderFeatureEnvironmentZone>,
        // IActivate<DaemonZone>,
        // IActivate<IPsiLanguageZone>,
        // IActivate<ILanguageCSharpZone>
        // IRequire<ISolutionBuilderFeatureZone>, IRequire<IRiderPlatformZone>
    {
    }

    [ZoneMarker]
    public class ZoneMarker : IRequire<IDependencyMonkeyTestEnvironmentZone>
    {
    }
}

// Note: Global namespace to workaround (or hide) https://youtrack.jetbrains.com/issue/RSRP-464493.
[SetUpFixture]
public class TestEnvironmentSetUpFixture : ExtensionTestEnvironmentAssembly<IDependencyMonkeyTestEnvironmentZone>
{
}