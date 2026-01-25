using System.Collections.Generic;
using System.Linq;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Services;

internal static class PathService
{
    internal static string BuildPath(IEnumerable<Item> items, SelectionNode selectionNode, bool removeBrackets)
    {
        ItemTagState state = selectionNode.State;
        string value = selectionNode.Key;

        if (StateFlagUtils.IsItemState(state))
        {
            Item? item = items.FirstOrDefault(item => item.Id == value);
            if (item != null) value = removeBrackets ? ItemUtils.RemoveBrackets(item.Title) : item.Title; // アイテムはパスからタイトルに変換する
        }

        if (StateFlagUtils.IsCategoryState(state))
        {
            // カテゴリはValue自体を翻訳する
            // カテゴリ: Search.Category.Textureのような感じで入っているため
            value = Localizer.Instance[value];
        }

        // 翻訳できないタグ(Root以外)はここがnullになるため、valueがパスになる。ある場合はPrefixが翻訳される。
        string? localizationKey = state.GetLocalizationKey();

        return localizationKey == null ? value : Localizer.Instance.GetDisplayName(localizationKey, value);
    }
}
