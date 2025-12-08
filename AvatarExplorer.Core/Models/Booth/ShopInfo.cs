using System.Text.Json.Serialization;

namespace AvatarExplorer.Core.Models.Booth;

public class ShopInfo
{
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("thumbnail_url")]
    public string ThumbnailUrl { get; set; } = string.Empty;
}
