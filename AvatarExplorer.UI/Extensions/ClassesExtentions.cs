using Avalonia.Controls;

namespace AvatarExplorer.UI.Extensions;

internal static class ClassesExtentions
{
    internal static void AddRange(this Classes classes, string[] values)
    {
        foreach (string value in values)
        {
            classes.Add(value);
        }
    }
}
