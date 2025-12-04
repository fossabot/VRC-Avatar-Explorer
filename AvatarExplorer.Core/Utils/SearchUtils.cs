using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

internal static class SearchUtils
{
    internal static int GetScore(Item item, IEnumerable<string> words)
    {
        int count = 0;

        foreach (var word in words)
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
