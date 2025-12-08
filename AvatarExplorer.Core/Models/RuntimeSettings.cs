namespace AvatarExplorer.Core.Models;

internal class RuntimeSettings
{
    internal string DataRootDirectory { get; private set; } = SystemPath.DefaultItemsFolderPath;
    internal SortOrder ItemSortOrder { get; private set; } = SortOrder.Title;

    internal void SetSortOrder(SortOrder sortOrder)
    {
        ItemSortOrder = sortOrder;
    }

    internal bool SetDataRootDirectory(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            DataRootDirectory = SystemPath.DefaultItemsFolderPath;
            return false;
        }
        else
        {
            DataRootDirectory = Path.GetFullPath(path);
            return true;
        }
    }
}
