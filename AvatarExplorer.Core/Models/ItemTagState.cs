using AvatarExplorer.Core.Attributes;
using AvatarExplorer.Core.Localization;

namespace AvatarExplorer.Core.Models;

public enum ItemTagState
{
    Unknown,

    // RootだけはPrefixがあるため、翻訳キーを追加している。その他はそのままで大丈夫
    [LocalizationKey(LocalizationKey.Path.SearchResult)]
    SearchItem,

    [LocalizationKey(LocalizationKey.Path.Root.Avatar)]
    RootAvatar,

    [LocalizationKey(LocalizationKey.Path.Root.Author)]
    RootAuthor,

    [LocalizationKey(LocalizationKey.Path.Root.Category)]
    RootCategory,
    
    RootSelectedCategory,
    RootSelectedItem,
    ItemFileCategory,
    ItemFileCategoryOpen
}
