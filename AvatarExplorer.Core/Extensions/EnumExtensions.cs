using AvatarExplorer.Core.Attributes;

namespace AvatarExplorer.Core.Extensions;

public static class EnumExtensions
{
    public static string? GetLocalizationKey(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(LocalizationKeyAttribute), false).FirstOrDefault() as LocalizationKeyAttribute;
        return attribute?.Key ?? null;
    }

    internal static string[]? GetExtensionFilters(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(ExtensionsFilterAttribute), false).FirstOrDefault() as ExtensionsFilterAttribute;
        return attribute?.Filter.Split('|') ?? null;
    }
}
