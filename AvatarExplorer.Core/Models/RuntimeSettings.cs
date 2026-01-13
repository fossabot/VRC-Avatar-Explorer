using System.Text.Json.Serialization;
using AvatarExplorer.Core.Data.Paths;

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
    
    internal void SetDataRootDirectory(string path)
        => DataRootDirectory = path;

    internal void SetSortOrder(SortOrder sortOrder)
        => ItemSortOrder = sortOrder;

    internal void SetRemoveOriginal(bool value)
        => RemoveOriginal = value;

    internal void SetRemoveBrackets(bool value)
        => RemoveBrackets = value;
}
