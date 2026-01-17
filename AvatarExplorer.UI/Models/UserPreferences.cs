using System.Text.Json.Serialization;

namespace AvatarExplorer.UI.Models;

public class UserPreferences
{
    [JsonInclude]
    public int Language { get; private set; } = 0;

    [JsonInclude]
    public int NormalIconSize { get; private set; } = 70;

    [JsonInclude]
    public int HoverIconSize { get; private set; } = 200;

    [JsonInclude]
    public bool UseBackgroundImage { get; private set; } = false;

    [JsonInclude]
    public string BackgroundImage { get; private set; } = string.Empty;

    [JsonInclude]
    public int BackgroundOpacity { get; private set; } = 20;
    
    [JsonInclude]
    public Theme Theme { get; private set; } = Theme.Dark;

    [JsonInclude]
    public int ItemsPerPage { get; private set; } = 30;

    internal void FromOther(UserPreferences userPreferences)
    {
        Language = userPreferences.Language;
        NormalIconSize = userPreferences.NormalIconSize;
        HoverIconSize = userPreferences.HoverIconSize;
        UseBackgroundImage = userPreferences.UseBackgroundImage;
        BackgroundImage = userPreferences.BackgroundImage;
        BackgroundOpacity = userPreferences.BackgroundOpacity;
        Theme = userPreferences.Theme;
        ItemsPerPage = userPreferences.ItemsPerPage;
    }

    internal void SetLanguage(int index) => Language = index;

    internal void SetIconSize(int normal, int hover)
    {
        NormalIconSize = normal;
        HoverIconSize = hover;
    }

    internal void UseBackground(bool value) => UseBackgroundImage = value;

    internal void SetBackground(string path) => BackgroundImage = path;

    internal void SetBackgroundOpacity(int value) => BackgroundOpacity = value;

    internal void SetTheme(Theme theme) => Theme = theme;

    internal void SetItemsPerPage(int value) => ItemsPerPage = value;
}
