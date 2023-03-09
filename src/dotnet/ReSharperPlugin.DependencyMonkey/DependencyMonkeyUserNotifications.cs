using JetBrains.Application;
using JetBrains.Application.Notifications;
using JetBrains.Application.Settings;
using JetBrains.Lifetimes;
using ReSharperPlugin.DependencyMonkey.Options;

namespace ReSharperPlugin.DependencyMonkey;

[ShellComponent]
public class DependencyMonkeyUserNotifications
{
    private readonly UserNotifications _notifications;
    private readonly Lifetime _lifetime;
    private readonly ISettingsStore _settingsStore;

    private const string NotificationTitle = "DependencyMonkey";

    public DependencyMonkeyUserNotifications(UserNotifications notifications, Lifetime lifetime, ISettingsStore settingsStore)
    {
        _notifications = notifications;
        _lifetime = lifetime;
        _settingsStore = settingsStore;
    }

    private void ShowNotificationCore(NotificationSeverity severity, string title, string body, bool forceEnable = false)
    {
        if (forceEnable)
        {
            var settings = _settingsStore.BindToContextTransient(ContextRange.ApplicationWide)
                .GetKey<DependencyMonkeySettings>(SettingsOptimization.OptimizeDefault);
            if (!settings.ShowAdditionalNotifications)
                return;
        }

        _notifications.CreateNotification(
            _lifetime,
            severity,
            title: title,
            body: body);
    }

    public void ShowWarningNotification(string body)
    {
        ShowNotificationCore(NotificationSeverity.WARNING, NotificationTitle, body, true);
    }

    public void ShowErrorNotification(string body)
    {
        ShowNotificationCore(NotificationSeverity.CRITICAL, NotificationTitle, body, true);
    }

    public void ShowInfoNotification(string body, string title = null)
    {
        ShowNotificationCore(
            NotificationSeverity.INFO,
            title == null ? NotificationTitle : $"{NotificationTitle} - {title}",
            body);
    }
}