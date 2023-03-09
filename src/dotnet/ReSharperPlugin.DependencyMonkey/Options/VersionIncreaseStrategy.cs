namespace ReSharperPlugin.DependencyMonkey.Options;

public enum VersionIncreaseStrategy
{
    Major,
    Minor,
    Patch,
}

public enum VersionUpdateOperation
{
    IncrementPrerelease,
    RemovePrerelease,
    NextMajor,
    NextMinor,
    NextPatch,
}