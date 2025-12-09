using System.Text.Json.Serialization;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Models;


public class RuntimeSettings
{
    [JsonInclude]
    public string DataRootDirectory { get; private set; } = SystemPath.DefaultItemsFolderPath;
    
    [JsonInclude]
    public SortOrder ItemSortOrder { get; private set; } = SortOrder.Title;
    
    [JsonInclude]
    public bool RemoveOriginal { get; private set; } = false;
    
    [JsonInclude]
    public bool RemoveBrackets { get; private set; } = false;

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

    internal void SetRemoveOriginal(bool value)
    {
        RemoveOriginal = value;
    }

    internal void SetRemoveBrackets(bool value)
    {
        RemoveBrackets = value;
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
