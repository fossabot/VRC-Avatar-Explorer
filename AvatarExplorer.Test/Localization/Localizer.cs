using System.Text.Json;

namespace AvatarExplorer.Test.Localization;

public class Localizer
{
    private readonly Dictionary<string, string> _map;
    
    public static Localizer Instance { get; private set; } = new Localizer();

    private Localizer()
    {
        _map = new Dictionary<string, string>();
    }

    public void LoadFromFile(string path)
    {
        if (!File.Exists(path)) return;

        var json = File.ReadAllText(path);
        _map.Clear();

        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (dict != null)
        {
            foreach (var kv in dict)
            {
                _map[kv.Key] = kv.Value;
            }
        }
    }

    public string GetDisplayName(string internalId)
    {
        return _map.TryGetValue(internalId, out var name) ? name : internalId;
    }

    public string? GetInternalId(string displayName)
    {
        foreach (var kv in _map)
        {
            if (kv.Value == displayName) return kv.Key;
        }

        return null;
    }

    public IReadOnlyDictionary<string, string> All => _map;
}
