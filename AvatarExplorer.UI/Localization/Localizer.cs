using System.Collections.Generic;
using System.IO;
using System.Linq;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Services;

namespace AvatarExplorer.UI.Localization;

internal class Localizer
{
    private readonly List<Dictionary<string, string>> _map;
    private int _selectedLanguageIndex = -1;
    private bool IsValidIndex => _selectedLanguageIndex >= 0 && _selectedLanguageIndex < _map.Count;

    internal static Localizer Instance { get; private set; } = new Localizer();

    private Localizer()
    {
        _map = new();
    }

    internal void LoadFromFolder(string path)
    {
        if (!Directory.Exists(path)) return;
        _map.Clear();

        foreach (string filePath in FileSystemService.EnumerateFiles(path))
        {
            var dictionary = FileSystemService.DeserializeClass<Dictionary<string, string>>(filePath);
            if (dictionary != null) _map.Add(dictionary);
        }
    }
    
    internal string[] GetLanguageList()
    {
        return _map.Select(i => i[LocalizationKey.LanguageName]).ToArray();
    }

    internal void SetLanguage(int index)
    {
        _selectedLanguageIndex = index;
    }

    internal string GetDisplayName(string localizationKey)
        => this[localizationKey];
    internal string GetDisplayName(string localizationKey, string arg)
    {
        if (!IsValidIndex) return localizationKey;
        string localizedText = _map[_selectedLanguageIndex].TryGetValue(localizationKey, out var value) ? value : localizationKey;
        return string.Format(localizedText, arg);
    }
    internal string GetDisplayName(string localizationKey, string[] args)
    {
        if (!IsValidIndex) return localizationKey;
        string localizedText = _map[_selectedLanguageIndex].TryGetValue(localizationKey, out var value) ? value : localizationKey;
        return args.Length > 0 ? string.Format(localizedText, args) : localizedText;
    }
    internal string this[string key]
    {
        get
        {
            if (!IsValidIndex) return key;
            return _map[_selectedLanguageIndex].TryGetValue(key, out string? value) ? value : key;
        }
    }

    internal string? GetLocalizationKey(string displayName)
    {
        foreach (var localizationMap in _map[_selectedLanguageIndex])
        {
            if (localizationMap.Value == displayName) return localizationMap.Key;
        }

        return null;
    }
}
