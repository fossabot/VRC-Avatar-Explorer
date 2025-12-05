using AvatarExplorer.Core.Attributes;

namespace AvatarExplorer.Core.Models;

public enum FileCategory
{
    None,

    [ExtensionsFilter(".psd|.clip|.blend|.fbx")]
    [InternalId("FileCategory.Modification")]
    Modification,

    [ExtensionsFilter(".png|.jpg")]
    [InternalId("FileCategory.Texture")]
    Texture,

    [ExtensionsFilter(".txt|.md|.pdf")]
    [InternalId("FileCategory.Document")]
    Document,

    [ExtensionsFilter(".unitypackage")]
    [InternalId("FileCategory.Unitypackage")]
    Unitypackage,

    [InternalId("FileCategory.Modification")]
    Material,
    Unknown
}
