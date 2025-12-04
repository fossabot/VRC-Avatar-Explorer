using System.Text.RegularExpressions;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Test.Localization;
using AvatarExplorer.Test.Models;

namespace AvatarExplorer.Test.Utils;

public static partial class SearchUtils
{
    [GeneratedRegex(@"(?<key>Title|Author|Booth|Avatar|Category|Memo|Folder|File|Implemented|NotImplemented|Tag|Common|OR|BrokenItems)=(?:""(?<value>.*?)""|(?<value>[^\s]+))|(?<word>[^\s]+)")]
    private static partial Regex SearchFilterRegex();

    private static readonly string[] CategoryKeys = Enum.GetValues<ItemType>().Select(i => i.GetInternalId()).Where(i => i != null).ToArray()!;

    public static List<RawSearchToken> ParseSearchText(string text)
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

    public static string ParseCategory(string text)
    {
        var parseResult = Localizer.Instance.GetInternalId(text);
        if (parseResult == null || !CategoryKeys.Contains(parseResult)) return text;

        return parseResult;
    }

    public static SearchFilter BuildFilter(string searchText)
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
}
