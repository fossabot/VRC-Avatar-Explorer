using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Data.Paths;

public static class SystemPath
{
    public static readonly string SoftwareDataPath = DatabaseUtils.GetSoftwareFolderPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    public static readonly string DocumentPath = DatabaseUtils.GetSoftwareFolderPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)); // デフォルトのアイテム保存先
    public static readonly string DatabaseFolderPath = DatabaseUtils.GetDataFolderPath(SoftwareDataPath);
    public static readonly string ImagesFolderPath = DatabaseUtils.GetImagesFolderPath(SoftwareDataPath);
    public static readonly string DefaultItemsFolderPath = DatabaseUtils.GetItemsFolderPath(DocumentPath);
    public static readonly string SettingsFolderPath = DatabaseUtils.GetSettingsFolderPath(SoftwareDataPath);

    public static readonly string TempFolderPath = DatabaseUtils.GetSoftwareFolderPath(Path.GetTempPath());

    public static readonly string AuthorThumbnailsPath = DatabaseUtils.GetAuthorThumbnailsFolderPath(SoftwareDataPath);
    public static readonly string ItemThumbnailsPath = DatabaseUtils.GetItemThumbnailsFolderPath(SoftwareDataPath);

    public static readonly string ItemDatabasePath = Path.Join(DatabaseFolderPath, SystemFileName.Database.Items);
    public static readonly string CommonAvatarDatabasePath = Path.Join(DatabaseFolderPath, SystemFileName.Database.CommonAvatars);

    public static readonly string RuntimeSettingsFilePath = Path.Join(SettingsFolderPath, SystemFileName.Settings.Runtime);
    public static readonly string UserPreferencesFilePath = Path.Join(SettingsFolderPath, SystemFileName.Settings.Preferences);
}
