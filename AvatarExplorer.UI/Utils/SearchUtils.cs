using System;
using System.Collections.Generic;
using System.Linq;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Utils;

internal static partial class SearchUtils
{
    private static readonly string[] CategoryLocalizationKeys = Enum.GetValues<ItemType>().Select(i => i.GetLocalizationKey()).Where(i => i != null).ToArray()!;

    internal static string ParseCategory(string text)
    {
        string? parsedResult = Localizer.Instance.GetLocalizationKey(text);
        if (parsedResult == null || !CategoryLocalizationKeys.Contains(parsedResult)) return text;

        return parsedResult;
    }

    internal static string ToPathString(this SearchFilter searchFilter)
    {
        List<string> searchFilterStrings = new();

        string localize(string key, IEnumerable<string> values)
            => Localizer.Instance.GetDisplayName(key, toSeparatedString(values));

        void addKey(string key, IEnumerable<string> values)
            => searchFilterStrings.Add(localize(key, values));

        string toSeparatedString(IEnumerable<string> values, string separateString = ", ")
            => string.Join(separateString, values);

        if (searchFilter.IsOrSearch) searchFilterStrings.Add(Localizer.Instance[LocalizationKey.SearchFilter.IsOrSearch]);
        if (searchFilter.Titles.Count != 0) addKey(LocalizationKey.SearchFilter.Title, searchFilter.Titles);
        if (searchFilter.Authors.Count != 0) addKey(LocalizationKey.SearchFilter.Author, searchFilter.Authors);
        if (searchFilter.BoothIds.Count != 0) addKey(LocalizationKey.SearchFilter.Booth, searchFilter.BoothIds);
        if (searchFilter.SupportedAvatars.Count != 0) addKey(LocalizationKey.SearchFilter.SupportedAvatar, searchFilter.SupportedAvatars);
        if (searchFilter.Categories.Count != 0) addKey(LocalizationKey.SearchFilter.Category, searchFilter.Categories.Select(Localizer.Instance.GetDisplayName));
        if (searchFilter.ItemMemos.Count != 0) addKey(LocalizationKey.SearchFilter.ItemMemo, searchFilter.ItemMemos);
        if (searchFilter.FolderNames.Count != 0) addKey(LocalizationKey.SearchFilter.FolderName, searchFilter.FolderNames);
        if (searchFilter.FileNames.Count != 0) addKey(LocalizationKey.SearchFilter.FileName, searchFilter.FileNames);
        if (searchFilter.ImplementedAvatars.Count != 0) addKey(LocalizationKey.SearchFilter.ImplementedAvatar, searchFilter.ImplementedAvatars);
        if (searchFilter.NotImplementedAvatars.Count != 0) addKey(LocalizationKey.SearchFilter.NotImplementedAvatar, searchFilter.NotImplementedAvatars);
        if (searchFilter.Tags.Count != 0) addKey(LocalizationKey.SearchFilter.Tag, searchFilter.Tags);
        if (searchFilter.CommonAvatars.Count != 0) addKey(LocalizationKey.SearchFilter.CommonAvatar, searchFilter.CommonAvatars);
        if (searchFilter.SearchWords.Count != 0) addKey(LocalizationKey.SearchFilter.SearchWord, searchFilter.SearchWords);

        string result = toSeparatedString(searchFilterStrings, " / ");
        return Localizer.Instance.GetDisplayName(LocalizationKey.SearchFilter.Default, result);
    }
}
