using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Models;

public static class SystemPath
{
    public static readonly string SoftwareDataPath = DatabaseUtils.GetSoftwareFolderPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    public static readonly string DatabaseFolderPath = DatabaseUtils.GetDataFolderPath(SoftwareDataPath);
    public static readonly string ImagesFolderPath = DatabaseUtils.GetImagesFolderPath(SoftwareDataPath);
    public static readonly string ItemsFolderPath = DatabaseUtils.GetItemsFolderPath(SoftwareDataPath);
    public static readonly string TempFolderPath = DatabaseUtils.GetTempFolderPath(SoftwareDataPath);

    public static readonly string AuthorThumbnailsPath = DatabaseUtils.GetAuthorThumbnailsFolderPath(SoftwareDataPath);
    public static readonly string ItemThumbnailsPath = DatabaseUtils.GetItemThumbnailsFolderPath(SoftwareDataPath);

    public static readonly string ItemDatabasePath = Path.Join(DatabaseFolderPath, SystemFile.DatabaseFile);
    public static readonly string CommonAvatarDatabasePath = Path.Join(DatabaseFolderPath, SystemFile.CommonAvatarDatabaseFile);
}
