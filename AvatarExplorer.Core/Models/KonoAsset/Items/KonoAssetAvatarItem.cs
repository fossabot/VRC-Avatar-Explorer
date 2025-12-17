using System.Text.Json.Serialization;
using AvatarExplorer.Core.Interfaces.KonoAsset;
using AvatarExplorer.Core.Services;

namespace AvatarExplorer.Core.Models.KonoAsset.Items;

public class KonoAssetAvatarItem : IKonoAssetItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public KonoAssetDescription Description { get; set; } = new KonoAssetDescription();

    public Item ToItem()
    {
        Item migratedItem = ItemCreator.FromKonoAssetDescription(Description);
        migratedItem.ItemPath = $"<sys>{Id}";
        migratedItem.Type = ItemType.Avatar;

        return migratedItem;
    }
}
