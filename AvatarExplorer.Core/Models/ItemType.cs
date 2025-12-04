using AvatarExplorer.Core.Attributes;

namespace AvatarExplorer.Core.Models;

/// <summary>
/// アイテムの種類を表します。
/// </summary>
public enum ItemType
{
    None = -1,

    [InternalId("Search.Category.Avatar")]
    Avatar = 0,

    [InternalId("Search.Category.Clothing")]
    Clothing = 1,

    [InternalId("Search.Category.Texture")]
    Texture = 2,

    [InternalId("Search.Category.Gimmick")]
    Gimmick = 3,

    [InternalId("Search.Category.Accessory")]
    Accessory = 4,

    [InternalId("Search.Category.HairStyle")]
    HairStyle = 5,

    [InternalId("Search.Category.Animation")]
    Animation = 6,

    [InternalId("Search.Category.Tool")]
    Tool = 7,

    [InternalId("Search.Category.Shader")]
    Shader = 8,

    Custom = 9,

    Unknown = 10
}
