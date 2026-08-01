using System.Reactive.Linq;
using EdgarWatcher.Configuration;
using EdgarWatcher.Features.Datastore;
using EdgarWatcher.Features.SecApi;
using EdgarWatcher.Features.Webhook;
using Microsoft.Extensions.Options;

namespace EdgarWatcher.Features;

public class WatcherService : BackgroundService
{
    private readonly SecApiService _secApi;
    private readonly DiscordSecMessenger _discord;
    private readonly EdgarWatcherSettings _settings;
    private readonly ILogger<WatcherService> _logger;
    private readonly List<TickerStore> _tickerStores;

    public WatcherService(
        SecApiService secApi,
        DiscordSecMessenger discord,
        IOptions<EdgarWatcherSettings> settings,
        ILogger<WatcherService> logger)
    {
        _secApi = secApi;
        _discord = discord;
        _settings = settings.Value;
        _logger = logger;
        _tickerStores = _settings.Tickers
            .Select(ticker => new TickerStore(ticker))
            .ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _secApi.LoadTickerListAsync();
        _logger.LogInformation("Loaded ticker list from SEC");

        _discord.HealthPost("starting up").Subscribe(
            _ => { },
            ex => _logger.LogError(ex, "Failed to post startup health check"));

        _logger.LogInformation("Watching tickers: {Tickers}", string.Join(", ", _settings.Tickers));
        _logger.LogInformation(
            "Edgar Watcher started - polling {Count} tickers every {Interval}ms",
            _tickerStores.Count,
            _settings.IntervalInMilliseconds);

        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_settings.IntervalInMilliseconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (TickerStore store in _tickerStores)
                CheckUpdates(store);
        }
    }

    private void CheckUpdates(TickerStore store)
    {
        _secApi.FindCik(store.Name).Subscribe(
            cik => FetchSubmissions(cik, store),
            HandleError);
    }

    private void FetchSubmissions(int cik, TickerStore store)
    {
        _secApi.GetSubmissions(cik)
            .Retry(3)
            .Subscribe(
                submission => CompareSubmission(submission, store, cik),
                HandleError);
    }

    private void CompareSubmission(Models.Submission submission, TickerStore store, int cik)
    {
        if (!store.Compare(submission))
        {
            _discord.PostForm(submission, cik).Subscribe(
                _ => _logger.LogInformation("Posted new filing for {Ticker}", store.Name),
                ex => _logger.LogError(ex, "Failed to post form for {Ticker}", store.Name));
        }
        store.Store(submission);
    }

    private void HandleError(Exception error)
    {
        _logger.LogError(error, "Error during update check");
        _discord.HealthPost(error.Message).Subscribe(
            _ => { },
            ex => _logger.LogError(ex, "Failed to post error to Discord"));
    }
}
