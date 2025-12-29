using AvatarExplorer.Core.Attributes;
using AvatarExplorer.Core.Localization;

namespace AvatarExplorer.Core.Models;

[Flags]
public enum ItemTagState
{
    None = 0,

    // RootだけはPrefixがあるため、翻訳キーを追加している。その他はそのままで大丈夫
    [LocalizationKey(LocalizationKey.Path.SearchResult)]
    SearchItem = 1 << 0,

    [LocalizationKey(LocalizationKey.Path.Root.Avatar)]
    RootAvatar = 1 << 1,

    [LocalizationKey(LocalizationKey.Path.Root.Author)]
    RootAuthor = 1 << 2,

    [LocalizationKey(LocalizationKey.Path.Root.Category)]
    RootCategory = 1 << 3,
    
    RootSelectedCategory = 1 << 4,
    RootSelectedItem = 1 << 5,
    ItemFileCategory = 1 << 6,
    ItemFileCategoryOpen = 1 << 7
}
