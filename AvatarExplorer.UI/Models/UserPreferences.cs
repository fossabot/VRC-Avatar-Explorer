using System.Text.Json.Serialization;

namespace AvatarExplorer.UI.Models;

public class UserPreferences
{
    [JsonInclude]
    public int DefaultLanguage { get; private set; } = 0;

    [JsonInclude]
    public Theme Theme { get; private set; } = Theme.Auto;

    [JsonInclude]
    public int ItemsPerPage { get; private set; } = 30;

    internal void FromOther(UserPreferences userPreferences)
    {
        DefaultLanguage = userPreferences.DefaultLanguage;
        Theme = userPreferences.Theme;
        ItemsPerPage = userPreferences.ItemsPerPage;
    }

    internal void SetLanguage(int index)
        => DefaultLanguage = index;

    internal void SetTheme(Theme theme)
        => Theme = theme;

    internal void SetItemsPerPage(int value)
        => ItemsPerPage = value;
}
