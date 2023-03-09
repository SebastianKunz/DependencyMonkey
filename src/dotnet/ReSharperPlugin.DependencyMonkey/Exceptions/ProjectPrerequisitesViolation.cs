using System;

namespace ReSharperPlugin.DependencyMonkey.Exceptions;

public class ProjectPrerequisitesViolation : Exception
{
    public string ProjectName { get; }
    public readonly string _message;

    public ProjectPrerequisitesViolation(string projectName, string message)
    {
        ProjectName = projectName;
        _message = message;
    }

    public override string Message => $"Invalid Project prequisites for {ProjectName}, because {_message}.";
}