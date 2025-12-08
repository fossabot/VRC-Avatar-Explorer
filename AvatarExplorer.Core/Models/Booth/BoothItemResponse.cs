using System.Text.Json.Serialization;

namespace AvatarExplorer.Core.Models.Booth;

public class BoothItem
{
    [JsonPropertyName("name")]
    public string Title { get; set; } = string.Empty;

    public ShopInfo Shop { get; set; } = new();

    [JsonPropertyName("id")]
    public int BoothId { get; set; } = -1;

    [JsonPropertyName("images")]
    public List<ImageInfo> Thumbnails { get; set; } = new();

    public CategoryInfo Category { get; set; } = new();

    // これより下はAEの値
    [JsonIgnore]
    public ItemType EstimatedCategory { get; set; } = ItemType.None;

    [JsonIgnore]
    public string AuthorId { get; set; } = string.Empty;
}
