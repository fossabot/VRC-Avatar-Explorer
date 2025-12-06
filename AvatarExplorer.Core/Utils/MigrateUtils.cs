namespace AvatarExplorer.Core.Utils;

internal static class MigrateUtils
{
    private const string V1ItemsFolderPrefix = "Datas\\Items\\";
    private const string V1ThumbnailFolderPrefix = "Datas\\Thumbnail\\";
    private const string V1AuthorThumbnailFolderPrefix = "Datas\\AuthorImage\\";
    
    internal static void MigrateItemPaths(List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            paths[i] = MigrateItemPath(paths[i]);
        }
    }

    internal static string MigrateItemPath(string path)
    {
        if (path.StartsWith(V1ItemsFolderPrefix))
            return path.Replace(V1ItemsFolderPrefix, "<sys>"); // フルパスとアプリフォルダの区別をつけるため

        if (path.StartsWith(V1ThumbnailFolderPrefix))
            return path.Replace(V1ThumbnailFolderPrefix, "");

        if (path.StartsWith(V1AuthorThumbnailFolderPrefix))
            return path.Replace(V1AuthorThumbnailFolderPrefix, "");

        return path;
    }
}
