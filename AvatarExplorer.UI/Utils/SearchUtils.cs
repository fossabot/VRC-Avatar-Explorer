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
        var matches = SearchFilterRegex().Matches(text);
        var tokens = new List<RawSearchToken>();

        foreach (GroupCollection gc in matches.Select(m => m.Groups))
        {
            if (gc["key"].Success && gc["value"].Success)
            {
                tokens.Add(new RawSearchToken
                {
                    Key = gc["key"].Value,
                    Value = gc["value"].Value
                });
            }
            else if (gc["word"].Success)
            {
                tokens.Add(new RawSearchToken
                {
                    Key = "FreeWord",
                    Value = gc["word"].Value
                });
            }
        }

        return tokens;
    }

    internal static string ParseCategory(string text)
    {
        var parsedResult = Localizer.Instance.GetLocalizationKey(text);
        if (parsedResult == null || !CategoryLocalizationKeys.Contains(parsedResult)) return text;

        return parsedResult;
    }

    internal static SearchFilter BuildFilter(string searchText)
    {
        var rawTokens = ParseSearchText(searchText);
        var filter = new SearchFilter();

        foreach (var t in rawTokens)
        {
            switch (t.Key)
            {
                case "Title":
                    filter.Titles.Add(t.Value);
                    break;
                case "Author":
                    filter.Authors.Add(t.Value);
                    break;
                case "Booth":
                    filter.BoothIds.Add(t.Value);
                    break;
                case "Avatar":
                    filter.SupportedAvatars.Add(t.Value);
                    break;
                case "Category":
                    filter.Categories.Add(ParseCategory(t.Value));
                    break;
                case "Memo":
                    filter.ItemMemos.Add(t.Value);
                    break;
                case "Folder":
                    filter.FolderNames.Add(t.Value);
                    break;
                case "File":
                    filter.FileNames.Add(t.Value);
                    break;
                case "Implemented":
                    filter.ImplementedAvatars.Add(t.Value);
                    break;
                case "NotImplemented":
                    filter.NotImplementedAvatars.Add(t.Value);
                    break;
                case "Tag":
                    filter.Tags.Add(t.Value);
                    break;
                case "Common":
                    filter.CommonAvatars.Add(t.Value);
                    break;
                case "OR":
                    filter.IsOrSearch = t.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    break;
                case "BrokenItems":
                    filter.BrokenItems = t.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    break;
                case "FreeWord":
                    filter.SearchWords.Add(t.Value);
                    break;
            }
        }

        return filter;
    }

    internal static string ToPathString(this SearchFilter searchFilter)
    {
        List<string> searchFilterStrings = new();

        void addKey(string key, IEnumerable<string> values)
            => searchFilterStrings.Add(Localizer.Instance.GetDisplayName(key, [toSeparatedString(values)]));

        string toSeparatedString(IEnumerable<string> values, string separateString = ", ")
            => string.Join(separateString, values);

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
        return Localizer.Instance.GetDisplayName(LocalizationKey.SearchFilter.Default, [result]);
    }
}
