using System.Diagnostics;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Models;

public class SearchFilter
{
    /// <summary>
    /// 検索するアイテムのタイトルを取得または設定します。
    /// </summary>
    public List<string> Titles { get; set; } = new List<string>();

    /// <summary>
    /// 検索する作者の名前を取得または設定します。
    /// </summary>
    public List<string> Authors { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのIDを取得または設定します。
    /// </summary>
    public List<string> BoothIds { get; set; } = new List<string>();

    /// <summary>
    /// 検索する対応アバターを取得または設定します。
    /// </summary>
    public List<string> SupportedAvatars { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのカテゴリ、またはカスタムカテゴリを取得または設定します。
    /// </summary>
    public List<string> Categories { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのメモを取得または設定します。
    /// </summary>
    public List<string> ItemMemos { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのフォルダ名を取得または設定します。
    /// </summary>
    public List<string> FolderNames { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのファイル名を取得または設定します。
    /// </summary>
    public List<string> FileNames { get; set; } = new List<string>();

    /// <summary>
    /// 検索する実装済みのアバターを取得または設定します。
    /// </summary>
    public List<string> ImplementedAvatars { get; set; } = new List<string>();

    /// <summary>
    /// 検索する未実装のアバターを取得または設定します。
    /// </summary>
    public List<string> NotImplementedAvatars { get; set; } = new List<string>();

    /// <summary>
    /// 検索するタグを取得または設定します。
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// 検索する共通素体グループを取得または設定します。
    /// </summary>
    public List<string> CommonAvatars { get; set; } = new List<string>();
    
    /// <summary>
    /// OR検索かどうかを取得または設定します。
    /// </summary>
    public bool IsOrSearch { get; set; } = false;

    /// <summary>
    /// アイテムの対応パスなどが破損しているかどうかを取得または設定します。
    /// </summary>
    public bool BrokenItems { get; set; } = false;

    /// <summary>
    /// 検索するアイテムの文字列を取得または設定します。
    /// </summary>
    public List<string> SearchWords { get; set; } = new List<string>();

    public bool Matches(Dictionary<string, string> avatarNameMaps, List<CommonAvatar> commonAvatars, Item item) {
        bool matchTitle = Titles.Count == 0 || MatchesFilter(
            [item.Title], Titles,
            IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchAuthor = Authors.Count == 0 || MatchesFilter(
            [item.Author], Authors,
            IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchBooth = BoothIds.Count == 0 || MatchesFilter(
            [item.BoothId.ToString()], BoothIds,
            IsOrSearch,
            (target, filter) => target == filter
        );

        bool matchAvatar = SupportedAvatars.Count == 0 || MatchesFilter(
            item.SupportedAvatars.Select(avatar => ItemUtils.GetAvatarNameFromDictionary(avatarNameMaps, avatar)), SupportedAvatars,
            IsOrSearch,
            (target, filter) => !string.IsNullOrEmpty(target) && target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchCategory = Categories.Count == 0 || MatchesFilter(
            [item.Type == ItemType.Custom ? item.CustomCategory : item.Type.GetInternalId()], Categories,
            IsOrSearch,
            (target, filter) => target != null && target.Contains(filter!)
        );

        bool matchMemo = ItemMemos.Count == 0 || MatchesFilter(
            [item.ItemMemo], ItemMemos,
            IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchPath = FolderNames.Count == 0 || MatchesFilter(
            [Path.GetFileName(item.ItemPath), Path.GetFileName(item.MaterialPath)], FolderNames,
            IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        //TODO: マテリアルフォルダも追加
        bool matchFile = FileNames.Count == 0
            || MatchesFilter(
                FileSystemUtils.EnumerateFiles(ItemUtils.GetItemPath(item.ItemPath)), FileNames,
                IsOrSearch,
                (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
            );
        
        var implementedAvatarNames = item.ImplementedAvatars.Select(avatar => ItemUtils.GetAvatarNameFromDictionary(avatarNameMaps, avatar));

        bool matchImplemented = ImplementedAvatars.Count == 0 || MatchesFilter(
            implementedAvatarNames, ImplementedAvatars,
            IsOrSearch,
            (target, filter) => !string.IsNullOrEmpty(target) && target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchNotImplemented = NotImplementedAvatars.Count == 0
            || (NotImplementedAvatars.Count > 0 && IsOrSearch
                ? NotImplementedAvatars.Any(filter => !implementedAvatarNames.Any(name => name.Contains(filter, StringComparison.CurrentCultureIgnoreCase)))
                : NotImplementedAvatars.All(filter => !implementedAvatarNames.Any(name => name.Contains(filter, StringComparison.CurrentCultureIgnoreCase))));

        bool matchTag = Tags.Count == 0 || MatchesFilter(
            item.Tags, Tags,
            IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchCommon;
        if (CommonAvatars.Count == 0)
        {
            matchCommon = true;
        }
        else
        {
            List<CommonAvatar?> filterCommonAvatars = CommonAvatars
                .Select(name => CommonAvatarUtils.GetCommonAvatarFromName(commonAvatars, name))
                .ToList();

            matchCommon = IsOrSearch
                ? item.SupportedAvatars.Any(avatar => filterCommonAvatars.Any(ca => ca != null && ca.Avatars.Contains(avatar)))
                : filterCommonAvatars.All(ca => ca != null && item.SupportedAvatars.Any(avatar => ca.Avatars.Contains(avatar)));
        }

        bool matchBroken = !BrokenItems || (BrokenItems && !(item.SupportedAvatars.Contains(item.ItemPath) || item.ImplementedAvatars.Contains(item.ItemPath)));

        bool matchWord = SearchWords.Count == 0 || SearchWords.Any(w => GetWordSearchResult(avatarNameMaps, item, w));

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
}

