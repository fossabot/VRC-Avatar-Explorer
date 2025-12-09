using System.Text.Json.Serialization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.UI.Models;

public class UserUiPreferences
{
    [JsonInclude]
    public int DefaultLanguage { get; private set; } = 0;

    [JsonInclude]
    public Theme Theme { get; private set; } = Theme.Auto;

    [JsonInclude]
    public int ItemsPerPage { get; private set; } = 30;

    internal void FromOther(UserUiPreferences userUiPreferences)
    {
        DefaultLanguage = userUiPreferences.DefaultLanguage;
        Theme = userUiPreferences.Theme;
        ItemsPerPage = userUiPreferences.ItemsPerPage;
    }

    internal void SetLanguage(int index)
    {
        DefaultLanguage = index;
    }

    internal void SetTheme(Theme theme)
    {
        Theme = theme;
    }

    internal void SetItemsPerPage(int value)
    {
        ItemsPerPage = value;
    }
    
    internal void Save()
    {
        try
        {
            FileSystemUtils.SerializeClass(this, SystemPath.UserPreferencesFilePath);
        }
        catch
        {
            // Ignored
        }
    }
}
