using System.Text.Json.Serialization;

namespace EdgarWatcher.Models;

public class Submission
{
    [JsonPropertyName("cik")]
    public string Cik { get; set; } = "";

    [JsonPropertyName("entityType")]
    public string EntityType { get; set; } = "";

    [JsonPropertyName("sic")]
    public string Sic { get; set; } = "";

    [JsonPropertyName("sicDescription")]
    public string SicDescription { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("tickers")]
    public List<string> Tickers { get; set; } = [];

    [JsonPropertyName("exchanges")]
    public List<string> Exchanges { get; set; } = [];

    [JsonPropertyName("ein")]
    public string Ein { get; set; } = "";

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = "";

    [JsonPropertyName("filings")]
    public FilingsContainer Filings { get; set; } = new();
}

public class FilingsContainer
{
    [JsonPropertyName("recent")]
    public RecentFiling Recent { get; set; } = new();
}
