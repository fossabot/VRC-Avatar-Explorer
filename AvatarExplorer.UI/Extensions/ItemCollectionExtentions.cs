using System;
using System.Collections;
using Avalonia.Controls;

namespace AvatarExplorer.UI.Extensions;

internal static class ItemCollectionExtentions
{
    internal static void AddRange(this ItemCollection itemCollection, IEnumerable values)
    {
        foreach (string value in values)
        {
            itemCollection.Add(value);
        }
    }
}
