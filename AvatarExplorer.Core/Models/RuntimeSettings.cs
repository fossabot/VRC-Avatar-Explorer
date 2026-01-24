using System.Text.Json.Serialization;
using AvatarExplorer.Core.Data.Paths;

namespace AvatarExplorer.Core.Models;

public class RuntimeSettings
{
    [JsonInclude]
    public string DataRootDirectory { get; private set; } = SystemPath.DefaultItemsFolderPath;

    [JsonInclude]
    public string AutoBackupRootDirectory { get; private set; } = SystemPath.BackupFolderPath;
    
    [JsonInclude]
    public SortOrder ItemSortOrder { get; private set; } = SortOrder.Title;
    
    [JsonInclude]
    public bool RemoveOriginal { get; private set; } = false;
    
    [JsonInclude]
    public bool RemoveBrackets { get; private set; } = false;

    [JsonInclude]
    public int AutoBackupInterval { get; private set; } = 5;
    
    internal void SetDataRootDirectory(string path) => DataRootDirectory = path;
    internal void SetAutoBackupRootDirectory(string path) => AutoBackupRootDirectory = path;
    internal void SetSortOrder(SortOrder sortOrder) => ItemSortOrder = sortOrder;
    internal void SetRemoveOriginal(bool value) => RemoveOriginal = value;
    internal void SetRemoveBrackets(bool value) => RemoveBrackets = value;
    internal void SetAutoBackupInterval(int value) => AutoBackupInterval = value;
}
