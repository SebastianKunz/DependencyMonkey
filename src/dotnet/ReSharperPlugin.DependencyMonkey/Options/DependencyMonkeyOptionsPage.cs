using JetBrains.Application.Settings;
using JetBrains.Application.UI.Controls.FileSystem;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.DataFlow;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Daemon.OptionPages;
using JetBrains.ReSharper.UnitTestFramework.Resources;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.Util;
using System;
using System.Linq.Expressions;

namespace ReSharperPlugin.DependencyMonkey.Options;

[OptionsPage(Pid, PageTitle, typeof(UnitTestingThemedIcons.Session), ParentId = CodeInspectionPage.PID)]
public class DependencyMonkeyOptionsPage : BeSimpleOptionsPage
{
    private const string Pid = nameof(DependencyMonkeyOptionsPage);
    private const string PageTitle = "DependencyMonkey";

    private readonly Lifetime _lifetime;

    public DependencyMonkeyOptionsPage(Lifetime lifetime,
        OptionsPageContext optionsPageContext,
        OptionsSettingsSmartContext optionsSettingsSmartContext,
        IconHostBase iconHost,
        ICommonFileDialogs dialogs)
        : base(lifetime, optionsPageContext, optionsSettingsSmartContext)
    {
        _lifetime = lifetime;

        // Add additional search keywords
        AddKeyword("Sample", "Example", "Preferences"); // TODO: only works for ReSharper?

        AddText("The specified folder HAS to be a nuget feed!");
        AddFolderChooserOption(
            (DependencyMonkeySettings x) => x.FolderPath,
            id: nameof(DependencyMonkeySettings.FolderPath),
            initialValue: FileSystemPath.Empty,
            iconHost,
            dialogs);
        AddSpacer();
        
        AddText("The prerelease tag to use when upgrading to a prerelease version.");
        AddTextBox((DependencyMonkeySettings x) => x.PreReleaseTag, "Prerelease Tag");
        AddSpacer();

        AddText("Whether to show additional notifications.");
        AddBoolOption((DependencyMonkeySettings x) => x.ShowAdditionalNotifications, "Show additional notifications");
        AddSpacer();

        AddText("Determines what part of the semantic version should be increased when increasing the version from a non prerelease version to a prerelease version. " +
                "Example: With the default value 'Patch' the version will go from 1.2.3 -> 1.2.4-beta.1 when upgrading to the next prerelease version.");
        AddComboOptionFromEnum((DependencyMonkeySettings x) => x.VersionIncreaseStrategy, (strategy) => strategy.ToString());
    }

    private BeTextBox AddTextBox<TKeyClass>(Expression<Func<TKeyClass, string>> lambdaExpression, string description)
    {
        var property = new Property<string>(description);
        OptionsSettingsSmartContext.SetBinding(_lifetime, lambdaExpression, property);
        var control = property.GetBeTextBox(_lifetime);
        AddControl(control.WithDescription(description, _lifetime));
        return control;
    }
}