namespace AvatarExplorer.Core.Utils;

public static class FileSystemUtils
{
    public static IEnumerable<string> EnumerateFiles(string root)
    {
        if (!Directory.Exists(root))
            yield break;

        var directories = new Stack<string>();
        directories.Push(root);

        while (directories.Count > 0)
        {
            var dir = directories.Pop();

            string[] subDirectories;

            try { subDirectories = Directory.GetDirectories(dir); }
            catch { continue; }

            foreach (var subDirectory in subDirectories)
            {
                directories.Push(subDirectory);
            }

            string[] files;

            try { files = Directory.GetFiles(dir); }
            catch { continue; }

            foreach (var f in files)
            {
                yield return f;
            }
        }
    }
}
