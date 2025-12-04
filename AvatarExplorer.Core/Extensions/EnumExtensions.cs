using System;
using AvatarExplorer.Core.Attributes;

namespace AvatarExplorer.Core.Extensions;

public static class EnumExtensions
{
    public static string? GetInternalId(this Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());
        var attr = fi?.GetCustomAttributes(typeof(InternalIdAttribute), false).FirstOrDefault() as InternalIdAttribute;
        return attr?.Id ?? null;
    }
}
