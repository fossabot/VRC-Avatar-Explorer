using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Models;

public class RuntimeSettings
{
    public string DataRootDirectory { get; set; } = SystemPath.DefaultItemsFolderPath;
    public SortOrder ItemSortOrder { get; set; } = SortOrder.Title;
    public bool RemoveOriginal { get; set; } = false;

    internal  void SetSortOrder(SortOrder sortOrder)
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

    internal void SetRemoveOriginal(bool value)
    {
        RemoveOriginal = value;
    }

    internal void Save()
    {
        try
        {
            FileSystemUtils.SerializeClass(this, SystemPath.RuntimeSettingsFilePath);
        }
        catch
        {
            // Ignored
        }
    }
}
