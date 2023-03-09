using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using ReSharperPlugin.DependencyMonkey.Options;

namespace ReSharperPlugin.DependencyMonkey.Actions;

public abstract class UpgradeVersionAction : IExecutableAction
{
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
        var projectElement = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);
        var upgrader = context.GetComponent<NugetUpgrader>();

        var project = projectElement as ProjectImpl;
        var visible = project != null && upgrader.IsAvailable;
        presentation.Visible = visible;

        return visible;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
        var upgrader = context.GetComponent<NugetUpgrader>();

        IProjectModelElement projectElement = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);
        if (projectElement is IProject project)
        {
            upgrader.UpgradeProjectAndDependencies(project, Strategy);
        }
    }

    protected abstract VersionUpdateOperation Strategy { get; }
}

[Action(Id, "Upgrade Prerelease Version")]
public class UpgradePrereleaseVersionAction : UpgradeVersionAction
{
    public const string Id = nameof(UpgradePrereleaseVersionAction);
    protected override VersionUpdateOperation Strategy => VersionUpdateOperation.IncrementPrerelease;
}

[Action(Id, "Upgrade Major Version")]
public class UpgradeMajorVersionAction : UpgradeVersionAction
{
    public const string Id = nameof(UpgradeMajorVersionAction);
    protected override VersionUpdateOperation Strategy => VersionUpdateOperation.NextMajor;
}

[Action(Id, "Upgrade Minor Version")]
public class UpgradeMinorVersionAction : UpgradeVersionAction
{
    public const string Id = nameof(UpgradeMinorVersionAction);
    protected override VersionUpdateOperation Strategy => VersionUpdateOperation.NextMinor;
}

[Action(Id, "Upgrade Patch Version")]
public class UpgradePatchVersionAction : UpgradeVersionAction
{
    public const string Id = nameof(UpgradePatchVersionAction);
    protected override VersionUpdateOperation Strategy => VersionUpdateOperation.NextPatch;
}

[Action(Id, "Remove Prerelease tag")]
public class RemovePrereleaseAction : UpgradeVersionAction
{
    public const string Id = nameof(RemovePrereleaseAction);
    protected override VersionUpdateOperation Strategy => VersionUpdateOperation.RemovePrerelease;
}
