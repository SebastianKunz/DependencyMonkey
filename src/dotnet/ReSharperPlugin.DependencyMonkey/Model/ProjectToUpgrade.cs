using JetBrains.Application.Threading;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Transactions;
using JetBrains.ReSharper.Psi.Xml.Impl.Tree;
using JetBrains.ReSharper.Psi.Xml.Parsing;
using JetBrains.ReSharper.Psi.Xml.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;
using NuGet.Versioning;
using ReSharperPlugin.DependencyMonkey.Exceptions;
using System;
using System.Diagnostics;

namespace ReSharperPlugin.DependencyMonkey.Model;

[DebuggerDisplay("{Project.Name}")]
public class ProjectToUpgrade
{
    public IProject Project { get; }

    public ProjectToUpgrade(IProject project)
    {
        Project = project;
        Parse();
    }

    public bool CanIncreaseVersion => OriginalVersion != null;

    public bool DoesGeneratePackageOnBuild => GeneratesPackageOnBuildAttr && IsPackableAttr;

    public bool WasVisited { get; set; }
    
    public bool NeedsToBeUpdated { get; set; }

    public bool WasChanged { get; private set; }

    public NuGetVersion UpgradedVersion { get; private set; }

    public string AssemblyName { get; private set; }

    public bool IsPackableAttr { get; private set; }

    public bool GeneratesPackageOnBuildAttr { get; private set; }

    public NuGetVersion OriginalVersion { get; private set; }

    public override bool Equals(object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }

        ProjectToUpgrade p = (ProjectToUpgrade)obj;

        return p.Project.Guid == Project.Guid;
    }

    public override int GetHashCode()
    {
        return Project.Guid.GetHashCode();
    }

    private IXmlTag RetrievePropertyGroup()
    {
        var projectFile = Project.ProjectFile;
        IPsiSourceFile psiSourceFile = projectFile.ToSourceFile();

        XmlFile xmlFile = psiSourceFile.GetPrimaryPsiFile() as XmlFile;
        var projectGroup = xmlFile?.InnerTags.FirstOrDefault();

        if (projectGroup is null)
        {
            throw new Exception($"Expected project {Project.Name} first xml tag to be <Project ...>, but its null!");
        }

        IXmlTag propertyGroup = projectGroup.InnerTags.FirstOrDefault(x => x.Header.ContainerName == "PropertyGroup");

        return propertyGroup;
    }

    public void Parse()
    {
        // TODO: I have no idea if this is the correct lock to use
        using IShellLocksEx.UndoUsingReadLock readLock = Project.ProjectFile.Locks.UsingReadLock();
        var propertyGroup = RetrievePropertyGroup();
        if (propertyGroup is null)
        {
            throw new ProjectPrerequisitesViolation(Project.Name, $"the project xml file does not contain a Project tag");
        }

        IXmlTag generatePackageOnBuildTag = null;
        IXmlTag isPackableTag = null;
        IXmlTag versionTag = null;
        foreach (var tag in propertyGroup.InnerTags)
        {
            if (tag.Header.ContainerName == "GeneratePackageOnBuild")
            {
                generatePackageOnBuildTag = tag;
            }
            else if (tag.Header.ContainerName == "IsPackable")
            {
                isPackableTag = tag;
            }
            else if (tag.Header.ContainerName == "Version")
            {
                versionTag = tag;
            }
            else if (tag.Header.ContainerName == "AssemblyName")
            {
                AssemblyName = tag.InnerText; 
            }
        }

        OriginalVersion = versionTag == null ? null : new NuGetVersion(versionTag.InnerText);
        // ReSharper disable once SimplifyConditionalTernaryExpression
        GeneratesPackageOnBuildAttr = generatePackageOnBuildTag == null ? false : bool.Parse(generatePackageOnBuildTag.InnerText);
        // ReSharper disable once SimplifyConditionalTernaryExpression
        // the default value for IsPackagable is true
        IsPackableAttr = isPackableTag == null ? true : bool.Parse(isPackableTag.InnerText);

        if (AssemblyName is null)
        {
            AssemblyName = Project.Name;
        }
    }

    public void SetVersion(NuGetVersion version)
    {
        using var writeLock = Project.ProjectFile.Locks.UsingWriteLock();
        var propertyGroup = RetrievePropertyGroup();
        var versionTag = propertyGroup.InnerTags.FirstOrDefault(x => x.Header.ContainerName == "Version");
        if (versionTag == null)
        {
            throw new ProjectPrerequisitesViolation(Project.Name, "failed to find a <Version> tag when setting the version.");
        }
        UpgradedVersion = version;

        var psiServices = Project.GetComponent<IPsiServices>();
        using (new PsiTransactionCookie(psiServices, DefaultAction.Commit, "UpdateXmlVersionTag"))
        {
            UpdateXmlVersionTag(versionTag, UpgradedVersion.ToNormalizedString());
        }

        WasChanged = true;
    }

    private void UpdateXmlVersionTag(IXmlTag tag, string text)
    {
        using (WriteLockCookie.Create(tag.IsPhysical()))
        {
            // Get an instance of XmlElementFactory, NOT IXmlElementFactory
            XmlElementFactory elementFactory = XmlElementFactory.GetInstance(tag);

            // Create the text to be used to create the tag
            var tagText = $"<Version>{text}</Version>";
            IXmlTag newTag = elementFactory.CreateRootTag(tagText);

            ModificationUtil.ReplaceChild(tag, newTag);
        }
    }

}