using System.Text.Json.Serialization;

namespace EdgarWatcher.Models;

public class CompanyTicker
{
    [JsonPropertyName("cik_str")]
    public int CikStr { get; set; }

    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
}
