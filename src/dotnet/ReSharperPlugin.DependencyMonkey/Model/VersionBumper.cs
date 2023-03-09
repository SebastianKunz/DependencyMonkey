using NuGet.Versioning;
using ReSharperPlugin.DependencyMonkey.Options;
using System;
using System.Linq;

namespace ReSharperPlugin.DependencyMonkey.Model;

public class VersionBumper
{
    private readonly VersionIncreaseStrategy _preReleaseVersionIncreaseStrategy;
    public VersionUpdateOperation UpdateOperation { get; }
    private readonly string _prereleaseTag;

    public VersionBumper(VersionIncreaseStrategy preReleaseVersionIncreaseStrategy,
        VersionUpdateOperation updateOperation,
        string prereleaseTag)
    {
        _preReleaseVersionIncreaseStrategy = preReleaseVersionIncreaseStrategy;
        UpdateOperation = updateOperation;
        _prereleaseTag = prereleaseTag;
    }

    public NuGetVersion UpdateVersion(NuGetVersion version)
    {
        int major = version.Major;
        int minor = version.Minor;
        int patch = version.Patch;
        if (UpdateOperation == VersionUpdateOperation.RemovePrerelease)
        {
            return new NuGetVersion(major, minor, patch, Array.Empty<string>(), version.Metadata);
        }

        var labels = version.ReleaseLabels.ToArray();

        if (UpdateOperation == VersionUpdateOperation.IncrementPrerelease)
        {
            if (!version.IsPrerelease && labels.Length == 0)
            {
                (major, minor, patch) = IncreaseMajorMinorPatchByStrategy(_preReleaseVersionIncreaseStrategy, version.Major, version.Minor, version.Patch);
                // The package does not have a prerelease version yet, so we add it and return.
                return new NuGetVersion(major, minor, patch, new[] { _prereleaseTag, "1" }, version.Metadata);
            }

            if (labels[0] != _prereleaseTag)
            {
                // TODO: warning 
            }

            if (int.TryParse(labels[1], out int versionInt))
            {
                labels[1] = (versionInt + 1).ToString();
            }

            return new NuGetVersion(major, minor, patch, labels, version.Metadata);
        }
        
        (major, minor, patch) = IncreaseMajorMinorPatchByStrategy(ToStrategy(UpdateOperation), version.Major, version.Minor, version.Patch);
        labels = Array.Empty<string>();

        return new NuGetVersion(major, minor, patch, labels, version.Metadata);
    }

    private VersionIncreaseStrategy ToStrategy(VersionUpdateOperation operation)
    {
        switch (operation)
        {
            case VersionUpdateOperation.NextMajor:
                return VersionIncreaseStrategy.Major;
            case VersionUpdateOperation.NextMinor:
                return VersionIncreaseStrategy.Minor;
            case VersionUpdateOperation.NextPatch:
                return VersionIncreaseStrategy.Patch;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        } 
    }

    private (int, int, int) IncreaseMajorMinorPatchByStrategy(VersionIncreaseStrategy strategy, int major, int minor, int patch)
    {
        if (strategy == VersionIncreaseStrategy.Major)
        {
            major++;
            minor = 0;
            patch = 0;
        }
        else if (strategy == VersionIncreaseStrategy.Minor)
        {
            minor++;
            patch = 0;
        }
        else if (strategy == VersionIncreaseStrategy.Patch)
        {
            patch++;
        }

        return (major, minor, patch);
    }
}