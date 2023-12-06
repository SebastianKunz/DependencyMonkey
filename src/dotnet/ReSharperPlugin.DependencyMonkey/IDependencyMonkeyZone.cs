using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;
using JetBrains.Rider.Backend.Env;

namespace ReSharperPlugin.DependencyMonkey
{
    [ZoneDefinition]
    // [ZoneDefinitionConfigurableFeature("Title", "Description", IsInProductSection: false)]
    public interface IDependencyMonkeyZone : IPsiLanguageZone, IRequire<IRiderPlatformZone>
    {
    }
}