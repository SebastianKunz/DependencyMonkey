using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;

namespace ReSharperPlugin.DependencyMonkey.Options;

[SettingsKey(Parent: typeof(EnvironmentSettings), Description: "DependencyMonkey – Settings")]
public class DependencyMonkeySettings
{
    [SettingsEntry(DefaultValue: "beta", Description: "Prerelease tag")]
    public string PreReleaseTag;

    [SettingsEntry(DefaultValue: default(string), Description: "Private description")]
    public string FolderPath;

    [SettingsEntry(DefaultValue: true, Description: "Show notifications")]
    public bool ShowAdditionalNotifications;

    [SettingsEntry(DefaultValue: VersionIncreaseStrategy.Patch, Description: "What version should be upgraded when upgrading from a non prerelease to a prerelease version.")]
    public VersionIncreaseStrategy VersionIncreaseStrategy;
}