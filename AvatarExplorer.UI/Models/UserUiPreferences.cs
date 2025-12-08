using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.UI.Models;

public class UserUiPreferences
{
    public int DefaultLanguage { get; set; } = 0;
    public Theme Theme { get; set; } = Theme.Auto;
    public int ItemsPerPage { get; set; } = 30;

    internal void FromOther(UserUiPreferences userUiPreferences)
    {
        DefaultLanguage = userUiPreferences.DefaultLanguage;
        Theme = userUiPreferences.Theme;
        ItemsPerPage = userUiPreferences.ItemsPerPage;
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
