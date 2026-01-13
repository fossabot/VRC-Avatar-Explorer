using System.Text.Json.Serialization;

namespace  AvatarExplorer.Core.Models.KonoAsset;

public class KonoAssetDescription
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("creator")]
    public string Creator { get; set; } = "";

    [JsonPropertyName("imageFilename")]
    public string? ImageFilename { get; set; } = null;

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; } = Array.Empty<string>();

    [JsonPropertyName("memo")]
    public string? Memo { get; set; } = null;

    [JsonPropertyName("boothItemId")]
    public int? BoothItemId { get; set; } = null;

    [JsonPropertyName("dependencies")]
    public string[] Dependencies { get; set; } = Array.Empty<string>();

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; } = 0;

    // [JsonPropertyName("publishedAt")]
    // public long? PublishedAt { get; set; } = null;
}
