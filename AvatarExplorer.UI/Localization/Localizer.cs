using System.Collections.Generic;
using System.IO;
using AvatarExplorer.Core.Services;

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

        _map.Clear();

        var dictionary = FileSystemService.DeserializeClass<Dictionary<string, string>>(path);
        if (dictionary != null)
        {
            foreach (var localizationMap in dictionary)
            {
                _map[localizationMap.Key] = localizationMap.Value;
            }
        }
    }

    internal string GetDisplayName(string localizationKey)
        => this[localizationKey];

    internal string GetDisplayName(string localizationKey, string[] args)
    {
        string localizedText = _map.TryGetValue(localizationKey, out var value) ? value : localizationKey;
        return args.Length > 0 ? string.Format(localizedText, args) : localizedText;
    }

    internal string GetDisplayName(string localizationKey, string arg)
    {
        string localizedText = _map.TryGetValue(localizationKey, out var value) ? value : localizationKey;
        return string.Format(localizedText, arg);
    }

    internal string this[string key]
        => _map.TryGetValue(key, out string? value) ? value : key;

    internal string? GetLocalizationKey(string displayName)
    {
        foreach (var localizationMap in _map)
        {
            if (localizationMap.Value == displayName) return localizationMap.Key;
        }

        return null;
    }

    internal IReadOnlyDictionary<string, string> All => _map;
}
