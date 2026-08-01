using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using EdgarWatcher.Configuration;
using EdgarWatcher.Features.SecApi;
using EdgarWatcher.Models;
using Microsoft.Extensions.Options;

namespace EdgarWatcher.Features.Webhook;

public class DiscordSecMessenger
{
    private readonly HttpClient _httpClient;
    private readonly EdgarWatcherSettings _edgarSettings;
    private readonly NotificationSettings _notificationSettings;
    private readonly ILogger<DiscordSecMessenger> _logger;

    public DiscordSecMessenger(
        HttpClient httpClient,
        IOptions<EdgarWatcherSettings> edgarSettings,
        IOptions<NotificationSettings> notificationSettings,
        ILogger<DiscordSecMessenger> logger)
    {
        _httpClient = httpClient;
        _edgarSettings = edgarSettings.Value;
        _notificationSettings = notificationSettings.Value;
        _logger = logger;
    }

    public IObservable<bool> PostForm(Submission submission, int cik)
    {
        return Observable.FromAsync(async () =>
        {
            List<Filing> filingsArr = ApiHelper.FixFilingObjectAndSort(submission.Filings.Recent);
            if (filingsArr.Count == 0) return false;

            Filing latestFiling = filingsArr[0];
            string accession = latestFiling.AccessionNumber.Replace("-", "");
            string docUrl = $"https://www.sec.gov/Archives/edgar/data/{cik}/{accession}/{latestFiling.PrimaryDocument}";

            await StatusPost($"New {latestFiling.Form}", docUrl);
            return true;
        });
    }

    public IObservable<bool> HealthPost(string message)
    {
        return Observable.FromAsync(async () =>
        {
            await PostToWebhook(_notificationSettings.HealthCheckWebhook, $"[{_edgarSettings.ServiceName}] {message}");
            return true;
        });
    }

    private async Task StatusPost(string title, string url)
    {
        var payload = new
        {
            content = $"@everyone {title}\n{url}"
        };

        await PostToWebhook(_notificationSettings.DiscordWebhook, payload);
    }

    private async Task PostToWebhook(string webhookUrl, object payload)
    {
        string json = JsonSerializer.Serialize(payload is string s ? new { content = s } : payload);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(webhookUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Discord webhook returned {StatusCode}", response.StatusCode);
        }
    }
}
