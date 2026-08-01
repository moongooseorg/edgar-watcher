using OpenBaoConfiguration;

namespace EdgarWatcher.Configuration;

[ConfigurationSection("EdgarWatcher")]
public class EdgarWatcherSettings
{
    public int IntervalInMilliseconds { get; set; }
    public int MaxServiceCallsInARow { get; set; }
    public int ServiceCallsResetInMilliseconds { get; set; }
    public string ServiceName { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public string[] Tickers { get; set; } = [];
}

[ConfigurationSection("Notification")]
public class NotificationSettings
{
    public string HealthCheckWebhook { get; set; } = "";
    public string DiscordWebhook { get; set; } = "";
}
