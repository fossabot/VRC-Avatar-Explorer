namespace AvatarExplorer.Core.Models;

public class SearchFilter
{
    public List<string> Titles { get; set; } = new List<string>();
    public List<string> Authors { get; set; } = new List<string>();
    public List<string> BoothIds { get; set; } = new List<string>();
    public List<string> SupportedAvatars { get; set; } = new List<string>();
    public List<string> Categories { get; set; } = new List<string>();
    public List<string> ItemMemos { get; set; } = new List<string>();
    public List<string> FolderNames { get; set; } = new List<string>();
    public List<string> FileNames { get; set; } = new List<string>();
    public List<string> ImplementedAvatars { get; set; } = new List<string>();
    public List<string> NotImplementedAvatars { get; set; } = new List<string>();
    public List<string> Tags { get; set; } = new List<string>();
    public List<string> CommonAvatars { get; set; } = new List<string>();
    public bool IsOrSearch { get; set; } = false;
    public bool BrokenItems { get; set; } = false;
    public List<string> SearchWords { get; set; } = new List<string>();
}
