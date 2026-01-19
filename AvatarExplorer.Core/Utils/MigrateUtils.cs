namespace AvatarExplorer.Core.Utils;

internal static class MigrateUtils
{
    private const string V1ItemsFolderPrefix = "Datas\\Items\\";
    private const string V1ThumbnailFolderPrefix = "Datas\\Thumbnail\\";
    private const string V1AuthorThumbnailFolderPrefix = "Datas\\AuthorImage\\";

    internal static string MigrateItemPath(string path)
    {
        if (path.StartsWith(V1ItemsFolderPrefix))
            return path.Replace(V1ItemsFolderPrefix, "<sys>"); // フルパスとアプリフォルダの区別をつけるため

        if (path.StartsWith(V1ThumbnailFolderPrefix))
            return path.Replace(V1ThumbnailFolderPrefix, string.Empty);

        if (path.StartsWith(V1AuthorThumbnailFolderPrefix))
            return path.Replace(V1AuthorThumbnailFolderPrefix, string.Empty);

        return path;
    }
}
