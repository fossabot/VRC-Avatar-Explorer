using System.Text.Json.Serialization;

namespace AvatarExplorer.UI.Models;

public class UserPreferences
{
    [JsonInclude]
    public int DefaultLanguage { get; private set; } = 0;

    [JsonInclude]
    public string BackgroundImage { get; private set; } = string.Empty;

    [JsonInclude]
    public int BackgroundOpacity { get; private set; } = 15;
    
    [JsonInclude]
    public Theme Theme { get; private set; } = Theme.Auto;

    [JsonInclude]
    public int ItemsPerPage { get; private set; } = 30;

    internal void FromOther(UserPreferences userPreferences)
    {
        DefaultLanguage = userPreferences.DefaultLanguage;
        BackgroundImage = userPreferences.BackgroundImage;
        BackgroundOpacity = userPreferences.BackgroundOpacity;
        Theme = userPreferences.Theme;
        ItemsPerPage = userPreferences.ItemsPerPage;
    }

    internal void SetLanguage(int index)
        => DefaultLanguage = index;

    internal void SetBackground(string path)
        => BackgroundImage = path;

    internal void SetBackgroundOpacity(int value)
        => BackgroundOpacity = value;

    internal void SetTheme(Theme theme)
        => Theme = theme;

    internal void SetItemsPerPage(int value)
        => ItemsPerPage = value;
}
