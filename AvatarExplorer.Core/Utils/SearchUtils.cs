using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

internal static class SearchUtils
{
    internal static bool Matches(SearchFilter searchFilter, Dictionary<string, string> avatarNameMaps, List<CommonAvatar> commonAvatars, Item item, string parentFolder) {
        bool matchTitle = searchFilter.Titles.Count == 0 || MatchesFilter(
            [item.Title], searchFilter.Titles,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchAuthor = searchFilter.Authors.Count == 0 || MatchesFilter(
            [item.Author], searchFilter.Authors,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchBooth = searchFilter.BoothIds.Count == 0 || MatchesFilter(
            [item.BoothId.ToString()], searchFilter.BoothIds,
            searchFilter.IsOrSearch,
            (target, filter) => target == filter
        );

        bool matchAvatar = searchFilter.SupportedAvatars.Count == 0 || MatchesFilter(
            item.SupportedAvatars.Select(avatar => ItemUtils.GetAvatarNameFromDictionary(avatarNameMaps, avatar)), searchFilter.SupportedAvatars,
            searchFilter.IsOrSearch,
            (target, filter) => !string.IsNullOrEmpty(target) && target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchCategory = searchFilter.Categories.Count == 0 || MatchesFilter(
            [item.Type == ItemType.Custom ? item.CustomCategory : item.Type.GetLocalizationKey()], searchFilter.Categories,
            searchFilter.IsOrSearch,
            (target, filter) => target != null && target.Contains(filter!)
        );

        bool matchMemo = searchFilter.ItemMemos.Count == 0 || MatchesFilter(
            [item.ItemMemo], searchFilter.ItemMemos,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchPath = searchFilter.FolderNames.Count == 0 || MatchesFilter(
            [Path.GetFileName(item.ItemPath), Path.GetFileName(item.MaterialPath)], searchFilter.FolderNames,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchFile;
        if (searchFilter.FileNames.Count == 0)
        {
            matchFile = true;
        }
        else
        {
            string itemPath = ItemUtils.GetItemPath(parentFolder, item.ItemPath);
            string materialPath = ItemUtils.GetItemPath(parentFolder, item.MaterialPath);

            List<string> files = new();
            if (!string.IsNullOrEmpty(itemPath) && Directory.Exists(itemPath)) files.AddRange(FileSystemUtils.EnumerateFiles(itemPath));
            if (!string.IsNullOrEmpty(materialPath) && Directory.Exists(materialPath)) files.AddRange(FileSystemUtils.EnumerateFiles(materialPath));

            matchFile = MatchesFilter(
                files, searchFilter.FileNames,
                searchFilter.IsOrSearch,
                (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
            );
        }
        
        IEnumerable<string> implementedAvatarNames = item.ImplementedAvatars.Select(avatar => ItemUtils.GetAvatarNameFromDictionary(avatarNameMaps, avatar));

        bool matchImplemented = searchFilter.ImplementedAvatars.Count == 0 || MatchesFilter(
            implementedAvatarNames, searchFilter.ImplementedAvatars,
            searchFilter.IsOrSearch,
            (target, filter) => !string.IsNullOrEmpty(target) && target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchNotImplemented = searchFilter.NotImplementedAvatars.Count == 0
            || (searchFilter.NotImplementedAvatars.Count > 0 && searchFilter.IsOrSearch
                ? searchFilter.NotImplementedAvatars.Any(filter => !implementedAvatarNames.Any(name => name.Contains(filter, StringComparison.CurrentCultureIgnoreCase)))
                : searchFilter.NotImplementedAvatars.All(filter => !implementedAvatarNames.Any(name => name.Contains(filter, StringComparison.CurrentCultureIgnoreCase))));

        bool matchTag = searchFilter.Tags.Count == 0 || MatchesFilter(
            item.Tags, searchFilter.Tags,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchCommon;
        if (searchFilter.CommonAvatars.Count == 0)
        {
            matchCommon = true;
        }
        else
        {
            List<CommonAvatar?> filterCommonAvatars = searchFilter.CommonAvatars
                .Select(name => CommonAvatarUtils.GetCommonAvatarFromName(commonAvatars, name))
                .ToList();

            matchCommon = searchFilter.IsOrSearch
                ? item.SupportedAvatars.Any(avatar => filterCommonAvatars.Any(ca => ca != null && ca.Avatars.Contains(avatar)))
                : filterCommonAvatars.All(ca => ca != null && item.SupportedAvatars.Any(avatar => ca.Avatars.Contains(avatar)));
        }

        bool matchBroken = !searchFilter.BrokenItems || (searchFilter.BrokenItems && !(item.SupportedAvatars.Contains(item.ItemPath) || item.ImplementedAvatars.Contains(item.ItemPath)));

        bool matchWord = searchFilter.SearchWords.Count == 0
            || (searchFilter.IsOrSearch
                ? searchFilter.SearchWords.Any(w => GetWordSearchResult(avatarNameMaps, item, w))
                : searchFilter.SearchWords.All(w => GetWordSearchResult(avatarNameMaps, item, w)));

        return matchTitle
            && matchAuthor
            && matchBooth
            && matchAvatar
            && matchCategory
            && matchMemo
            && matchPath
            && matchFile
            && matchImplemented
            && matchNotImplemented
            && matchTag
            && matchCommon
            && matchBroken
            && matchWord;
    }

    private static bool MatchesFilter<T>(IEnumerable<T> targets, IEnumerable<T> filters, bool isOrSearch, Func<T, T, bool> comparer)
    {
        if (!filters.Any()) return true;

        if (isOrSearch)
        {
            return filters.Any(filter => targets.Any(target => comparer(target, filter)));
        }
        else
        {
            return filters.All(filter => targets.Any(target => comparer(target, filter)));
        }
    }

    private static bool GetWordSearchResult(Dictionary<string, string> avatarNameMaps, Item item, string word)
    {
        if (item.SearchIndex == string.Empty) item.BuildSearchIndex(avatarNameMaps);
        return item.SearchIndex.Contains(word, StringComparison.CurrentCultureIgnoreCase);
    }

    internal static int GetScore(Item item, IEnumerable<string> words)
    {
        int count = 0;

        foreach (string word in words)
        {
            int index = 0;

            while ((index = item.SearchIndex.IndexOf(word, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += word.Length;
            }
        }

        return count;
    }
}
