using System.Text.Json.Serialization;
using AvatarExplorer.Core.Interfaces.KonoAsset;

namespace AvatarExplorer.Core.Models.KonoAsset.Items;

public class KonoAssetWorldItem : IKonoAssetItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public KonoAssetDescription Description { get; set; } = new KonoAssetDescription();

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";
}
