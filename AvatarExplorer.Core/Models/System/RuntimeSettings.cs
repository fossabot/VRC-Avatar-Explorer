using System.Text.Json.Serialization;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models.Items;

namespace AvatarExplorer.Core.Models.System;

public class RuntimeSettings
{
    [JsonInclude]
    public string DataRootDirectory { get; private set; } = SystemPath.DefaultItemsFolderPath;

    [JsonInclude]
    public string AutoBackupRootDirectory { get; private set; } = SystemPath.BackupFolderPath;

    [JsonInclude]
    public ItemSortOrder ItemSortOrder { get; private set; } = ItemSortOrder.Title;

    [JsonInclude]
    public bool RemoveOriginal { get; private set; } = false;

    [JsonInclude]
    public bool RemoveBrackets { get; private set; } = false;

    [JsonInclude]
    public int AutoBackupInterval { get; private set; } = 5;

    [JsonInclude]
    public int MaxDegreeOfParallelism { get; private set; } = 4;

    internal void SetDataRootDirectory(string path) => DataRootDirectory = path;
    internal void SetAutoBackupRootDirectory(string path) => AutoBackupRootDirectory = path;
    internal void SetSortOrder(ItemSortOrder sortOrder) => ItemSortOrder = sortOrder;
    internal void SetRemoveOriginal(bool value) => RemoveOriginal = value;
    internal void SetRemoveBrackets(bool value) => RemoveBrackets = value;
    internal void SetAutoBackupInterval(int value) => AutoBackupInterval = value;
    internal void SetMaxDegreeOfParallelism(int value) => MaxDegreeOfParallelism = value;
}
