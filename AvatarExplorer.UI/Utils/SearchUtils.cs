using System;
using System.Linq;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Utils;

internal static class SearchUtils
{
    private static readonly string[] CategoryLocalizationKeys = Enum.GetValues<ItemType>().Select(i => i.GetLocalizationKey()).Where(i => i != null).ToArray()!;

    internal static string ParseCategory(string text)
    {
        string? parsedResult = Localizer.Instance.GetLocalizationKey(text);
        if (parsedResult == null || !CategoryLocalizationKeys.Contains(parsedResult)) return text;

        return parsedResult;
    }
}
