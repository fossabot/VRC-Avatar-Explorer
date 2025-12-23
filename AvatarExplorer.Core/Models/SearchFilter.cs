namespace AvatarExplorer.Core.Models;

public class SearchFilter
{
    public List<string> Titles { get; } = new List<string>();
    public List<string> Authors { get; } = new List<string>();
    public List<string> BoothIds { get; } = new List<string>();
    public List<string> SupportedAvatars { get; } = new List<string>();
    public List<string> Categories { get; } = new List<string>();
    public List<string> ItemMemos { get; } = new List<string>();
    public List<string> FolderNames { get; } = new List<string>();
    public List<string> FileNames { get; } = new List<string>();
    public List<string> ImplementedAvatars { get; } = new List<string>();
    public List<string> NotImplementedAvatars { get; } = new List<string>();
    public List<string> Tags { get; } = new List<string>();
    public List<string> CommonAvatars { get; } = new List<string>();
    public bool IsOrSearch { get; set; } = false;
    public bool BrokenItems { get; set; } = false;
    public List<string> SearchWords { get; } = new List<string>();
}
