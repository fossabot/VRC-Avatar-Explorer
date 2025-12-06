using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AvatarExplorer.UI.Localization;

internal class Localizer
{
    private readonly Dictionary<string, string> _map;

    internal static Localizer Instance { get; private set; } = new Localizer();

    private Localizer()
    {
        _map = new Dictionary<string, string>();
    }

    internal void LoadFromFile(string path)
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

    internal string GetDisplayName(string internalId)
    {
        return _map.TryGetValue(internalId, out var name) ? name : internalId;
    }

    internal string GetDisplayName(string internalId, string[] args)
    {
        var localizedText = _map.TryGetValue(internalId, out var name) ? name : internalId;
        return args.Length > 0 ? string.Format(localizedText, args) : localizedText;
    }

    internal string? GetInternalId(string displayName)
    {
        foreach (var kv in _map)
        {
            if (kv.Value == displayName) return kv.Key;
        }

        return null;
    }

    internal IReadOnlyDictionary<string, string> All => _map;
}
