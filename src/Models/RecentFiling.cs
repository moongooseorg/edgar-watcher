using System.Text.Json.Serialization;

namespace EdgarWatcher.Models;

public class RecentFiling
{
    [JsonPropertyName("accessionNumber")]
    public List<string> AccessionNumber { get; set; } = [];

    [JsonPropertyName("filingDate")]
    public List<string> FilingDate { get; set; } = [];

    [JsonPropertyName("reportDate")]
    public List<string> ReportDate { get; set; } = [];

    [JsonPropertyName("acceptanceDateTime")]
    public List<string> AcceptanceDateTime { get; set; } = [];

    [JsonPropertyName("act")]
    public List<string> Act { get; set; } = [];

    [JsonPropertyName("form")]
    public List<string> Form { get; set; } = [];

    [JsonPropertyName("fileNumber")]
    public List<string> FileNumber { get; set; } = [];

    [JsonPropertyName("filmNumber")]
    public List<string> FilmNumber { get; set; } = [];

    [JsonPropertyName("items")]
    public List<string> Items { get; set; } = [];

    [JsonPropertyName("size")]
    public List<int> Size { get; set; } = [];

    [JsonPropertyName("isXBRL")]
    public List<int> IsXBRL { get; set; } = [];

    [JsonPropertyName("isInlineXBRL")]
    public List<int> IsInlineXBRL { get; set; } = [];

    [JsonPropertyName("primaryDocument")]
    public List<string> PrimaryDocument { get; set; } = [];

    [JsonPropertyName("primaryDocDescription")]
    public List<string> PrimaryDocDescription { get; set; } = [];
}
