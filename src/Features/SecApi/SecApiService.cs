using System.Reactive.Linq;
using System.Text.Json;
using EdgarWatcher.Configuration;
using EdgarWatcher.Models;
using Microsoft.Extensions.Options;

namespace EdgarWatcher.Features.SecApi;

public class SecApiService
{
    private readonly HttpClient _httpClient;
    private readonly EdgarWatcherSettings _settings;
    private readonly SemaphoreSlim _rateLimiter;
    private Dictionary<string, CompanyTicker>? _tickerList;

    public SecApiService(HttpClient httpClient, IOptions<EdgarWatcherSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _rateLimiter = new SemaphoreSlim(_settings.MaxServiceCallsInARow, _settings.MaxServiceCallsInARow);
    }

    public async Task LoadTickerListAsync()
    {
        string response = await GetStringAsync("https://www.sec.gov/files/company_tickers.json");
        _tickerList = JsonSerializer.Deserialize<Dictionary<string, CompanyTicker>>(response) ?? [];
    }

    public IObservable<int> FindCik(string ticker)
    {
        return Observable.Create<int>(observer =>
        {
            int? result = FindCikByTicker(ticker);
            if (result.HasValue)
            {
                observer.OnNext(result.Value);
            }
            else
            {
                observer.OnError(new InvalidOperationException(
                    $"Couldn't find Ticker '{ticker}' - Please update the list"));
            }
            observer.OnCompleted();
            return () => { };
        });
    }

    public IObservable<Submission> GetSubmissions(int cik)
    {
        string paddedCik = cik.ToString().PadLeft(10, '0');
        string url = $"https://data.sec.gov/submissions/CIK{paddedCik}.json";

        return Observable.FromAsync(async ct =>
        {
            await _rateLimiter.WaitAsync(ct);
            try
            {
                string response = await GetStringAsync(url, ct);
                return JsonSerializer.Deserialize<Submission>(response)
                    ?? throw new InvalidOperationException($"Failed to deserialize submission for CIK {cik}");
            }
            finally
            {
                _ = ReleaseAfterDelay();
            }
        });
    }

    private async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent", _settings.UserAgent);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task ReleaseAfterDelay()
    {
        await Task.Delay(_settings.ServiceCallsResetInMilliseconds);
        _rateLimiter.Release();
    }

    private int? FindCikByTicker(string tickerToSearch)
    {
        if (_tickerList is null) return null;

        foreach (CompanyTicker entry in _tickerList.Values)
        {
            if (string.Equals(entry.Ticker, tickerToSearch, StringComparison.OrdinalIgnoreCase))
                return entry.CikStr;
        }

        return null;
    }
}
