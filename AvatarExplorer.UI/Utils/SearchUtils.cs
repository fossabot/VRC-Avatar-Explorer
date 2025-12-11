using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI.Utils;

internal static partial class SearchUtils
{
    [GeneratedRegex(@"(?<key>Title|Author|Booth|Avatar|Category|Memo|Folder|File|Implemented|NotImplemented|Tag|Common|OR|BrokenItems)=(?:""(?<value>.*?)""|(?<value>[^\s]+))|(?<word>[^\s]+)")]
    private static partial Regex SearchFilterRegex();

    private static readonly string[] CategoryLocalizationKeys = Enum.GetValues<ItemType>().Select(i => i.GetLocalizationKey()).Where(i => i != null).ToArray()!;

    internal static List<RawSearchToken> ParseSearchText(string text)
    {
        MatchCollection matches = SearchFilterRegex().Matches(text);
        List<RawSearchToken> rawSearchTokens = new();

        foreach (GroupCollection groupCollection in matches.Select(m => m.Groups))
        {
            if (groupCollection["key"].Success && groupCollection["value"].Success)
            {
                rawSearchTokens.Add(new RawSearchToken
                {
                    Key = groupCollection["key"].Value,
                    Value = groupCollection["value"].Value
                });
            }
            else if (groupCollection["word"].Success)
            {
                rawSearchTokens.Add(new RawSearchToken
                {
                    Key = "FreeWord",
                    Value = groupCollection["word"].Value
                });
            }
        }

        return rawSearchTokens;
    }

    internal static string ParseCategory(string text)
    {
        string? parsedResult = Localizer.Instance.GetLocalizationKey(text);
        if (parsedResult == null || !CategoryLocalizationKeys.Contains(parsedResult)) return text;

        return parsedResult;
    }

    internal static SearchFilter BuildFilter(string searchText)
    {
        List<RawSearchToken> rawSearchTokens = ParseSearchText(searchText);
        SearchFilter filter = new();

        foreach (RawSearchToken token in rawSearchTokens)
        {
            switch (token.Key)
            {
                case "Title":
                    filter.Titles.Add(token.Value);
                    break;
                case "Author":
                    filter.Authors.Add(token.Value);
                    break;
                case "Booth":
                    filter.BoothIds.Add(token.Value);
                    break;
                case "Avatar":
                    filter.SupportedAvatars.Add(token.Value);
                    break;
                case "Category":
                    filter.Categories.Add(ParseCategory(token.Value));
                    break;
                case "Memo":
                    filter.ItemMemos.Add(token.Value);
                    break;
                case "Folder":
                    filter.FolderNames.Add(token.Value);
                    break;
                case "File":
                    filter.FileNames.Add(token.Value);
                    break;
                case "Implemented":
                    filter.ImplementedAvatars.Add(token.Value);
                    break;
                case "NotImplemented":
                    filter.NotImplementedAvatars.Add(token.Value);
                    break;
                case "Tag":
                    filter.Tags.Add(token.Value);
                    break;
                case "Common":
                    filter.CommonAvatars.Add(token.Value);
                    break;
                case "OR":
                    filter.IsOrSearch = token.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    break;
                case "BrokenItems":
                    filter.BrokenItems = token.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    break;
                case "FreeWord":
                    filter.SearchWords.Add(token.Value);
                    break;
            }
        }

        return filter;
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
