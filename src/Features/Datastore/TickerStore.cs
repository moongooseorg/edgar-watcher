using System.Text.Json;

namespace EdgarWatcher.Features.Datastore;

public class TickerStore
{
    private readonly string _ticker;
    private string? _storedJson;

    public TickerStore(string ticker)
    {
        _ticker = ticker;
    }

    public string Name => _ticker;

    public void Store(object value)
    {
        _storedJson = JsonSerializer.Serialize(value);
    }

    public bool Compare(object? newObj)
    {
        if (newObj is null || _storedJson is null)
            return true;

        string newJson = JsonSerializer.Serialize(newObj);
        return string.Equals(_storedJson, newJson, StringComparison.Ordinal);
    }
}
