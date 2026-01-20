using System.Text.Json;
using AvatarExplorer.Core.Data.Links;
using AvatarExplorer.Core.Data.Mappings;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.Booth;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

internal static class BoothService
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    internal static async Task<BoothItem?> GetItem(string boothId)
    {
        try
        {
            string url = string.Format(BoothLink.ItemJsonURLFormat, boothId);
            string response = await HttpClient.GetStringAsync(url);

            BoothItem? boothItem = JsonSerializer.Deserialize<BoothItem>(response, JsonSerializerOptions);
            if (boothItem == null) return null;

            return boothItem with
            {
                EstimatedCategory = SuggestItemType(boothItem.Title, boothItem.Category.Name),
                AuthorId = BoothUtils.GetAuthorIdFromUrl(boothItem.Shop.Url)
            };
        }
        catch
        {
            return null;
        }
    }
    private static ItemType SuggestItemType(string title, string type)
    {
        if (!BoothMapping.CategoryMappings.TryGetValue(type, out ItemType categorySuggestedType))
            categorySuggestedType = ItemType.None;
        
        IEnumerable<ItemType> titleSuggestedTypes = BoothMapping.TitleMappings
            .Where(mapping => mapping.Key.Any(title.Contains))
            .Select(mapping => mapping.Value);
        
        return titleSuggestedTypes.Any() ? titleSuggestedTypes.First() : categorySuggestedType;
    }
}
