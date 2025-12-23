namespace AvatarExplorer.Core.Utils;

public static class ListUtils
{
    public static void Add<T>(List<T> list, IEnumerable<T> items, bool clear)
    {
        if (clear) list.Clear();
        list.AddRange(items);
    }
}
