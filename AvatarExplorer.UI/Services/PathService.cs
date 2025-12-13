using System.Collections.Generic;
using System.Linq;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Services;

internal class PathService
{
    internal static string BuildPathTextFromSelectionNode(IEnumerable<Item> items, SelectionNode selectionNode)
    {
        ItemTagState state = selectionNode.State;
        string value = selectionNode.Key;

        if (StateFlagUtils.ItemsFlag.HasFlag(state))
        {
            Item? item = items.FirstOrDefault(item => item.ItemPath == value);
            if (item != null) value = item.Title; // アイテムはパスからタイトルに変換する
        }

        if (StateFlagUtils.CategoriesFlag.HasFlag(state))
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
