using System.Text.Json.Serialization;
using AvatarExplorer.Core.Interfaces.KonoAsset;
using AvatarExplorer.Core.Services;

namespace AvatarExplorer.Core.Models.KonoAsset.Items;

public class KonoAssetWorldItem : IKonoAssetItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public KonoAssetDescription Description { get; set; } = new KonoAssetDescription();

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    public Item ToItem()
    {
        Item migratedItem = ItemCreator.FromKonoAssetDescription(Description);
        migratedItem.ItemPath = $"<sys>{Id}";
        migratedItem.Type = ItemType.Custom;
        migratedItem.CustomCategory = string.IsNullOrEmpty(Category) ? "Worlds" : Category;

        return migratedItem;
    }
}
