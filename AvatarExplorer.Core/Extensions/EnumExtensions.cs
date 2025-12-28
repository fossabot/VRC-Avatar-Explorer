using System.Reflection;
using AvatarExplorer.Core.Attributes;

namespace AvatarExplorer.Core.Extensions;

public static class EnumExtensions
{
    public static string? GetLocalizationKey(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        LocalizationKeyAttribute? attribute = field?.GetCustomAttributes(typeof(LocalizationKeyAttribute), false).FirstOrDefault() as LocalizationKeyAttribute;
        return attribute?.Key ?? null;
    }

    internal static string[]? GetExtensionFilters(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        ExtensionsFilterAttribute? attribute = field?.GetCustomAttributes(typeof(ExtensionsFilterAttribute), false).FirstOrDefault() as ExtensionsFilterAttribute;
        return attribute?.Filter.Split('|') ?? null;
    }

    internal static string[]? GetFileNameFilters(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        FileNamesFilterAttribute? attribute = field?.GetCustomAttributes(typeof(FileNamesFilterAttribute), false).FirstOrDefault() as FileNamesFilterAttribute;
        return attribute?.Filter.Split('|') ?? null;
    }
}
