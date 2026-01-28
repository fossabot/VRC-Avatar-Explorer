using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Services;

namespace AvatarExplorer.UI.Localization;

public class Localizer : INotifyPropertyChanged
{
    private readonly List<Dictionary<string, string>> _map;
    private int _selectedLanguageIndex = -1;
    private bool IsValidIndex => _selectedLanguageIndex >= 0 && _selectedLanguageIndex < _map.Count;

    public static Localizer Instance { get; private set; } = new Localizer();

    public int CurrentLanguageIndex
    {
        get => _selectedLanguageIndex;
        private set
        {
            _selectedLanguageIndex = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private Localizer()
    {
        _map = new();
    }

    public void LoadFromFolder(string path)
    {
        if (!Directory.Exists(path)) return;

        List<Dictionary<string, string>> languageMaps = new();

        foreach (string filePath in FileSystemService.EnumerateFiles(path))
        {
            Dictionary<string, string>? dictionary = FileSystemService.DeserializeClass<Dictionary<string, string>>(filePath);
            if (dictionary != null) languageMaps.Add(dictionary);
        }

        _map.Clear();
        List<Dictionary<string, string>> sortedMaps = languageMaps.OrderBy(i =>
        {
            string priorityString = i.TryGetValue("LanguagePriority", out string? value) ? value : string.Empty;
            return int.TryParse(priorityString, out int priority) ? priority : int.MaxValue;
        }).ToList();
        _map.AddRange(sortedMaps);
    }
    
    public string[] GetLanguageList() => _map.Select(i => i[LocalizationKey.LanguageName]).ToArray();

    public void SetLanguage(int index) => CurrentLanguageIndex = index;

    public string GetDisplayName(string localizationKey) => this[localizationKey];
    public string GetDisplayName(string localizationKey, string arg)
    {
        if (!IsValidIndex) return localizationKey;
        string localizedText = _map[_selectedLanguageIndex].TryGetValue(localizationKey, out var value) ? value : localizationKey;
        return string.Format(localizedText, arg);
    }
    public string GetDisplayName(string localizationKey, string[] args)
    {
        if (!IsValidIndex) return localizationKey;
        string localizedText = _map[_selectedLanguageIndex].TryGetValue(localizationKey, out var value) ? value : localizationKey;
        return args.Length > 0 ? string.Format(localizedText, args) : localizedText;
    }

    public string this[string key]
    {
        get
        {
            if (!IsValidIndex) return key;
            return _map[_selectedLanguageIndex].TryGetValue(key, out string? value) ? value : key;
        }
    }

    public string? GetLocalizationKey(string displayName)
    {
        foreach (var localizationMap in _map[_selectedLanguageIndex])
        {
            if (localizationMap.Value == displayName) return localizationMap.Key;
        }

        return null;
    }
}
