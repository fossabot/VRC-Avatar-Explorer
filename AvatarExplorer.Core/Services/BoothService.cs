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
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal static async Task<BoothItem?> GetBoothItemAsync(string boothId)
    {
        try
        {
            string url = string.Format(BoothLink.ItemJsonURLFormat, boothId);
            string response = await HttpClient.GetStringAsync(url);

            BoothItem? boothItem = JsonSerializer.Deserialize<BoothItem>(response, JsonSerializerOptions);
            if (boothItem == null) return null;
            
            boothItem.EstimatedCategory = SuggestItemTypeFromTitle(boothItem.Title, boothItem.Category.Name);
            boothItem.AuthorId = BoothUtils.GetAuthorIdFromUrl(boothItem.Shop.Url);

            return boothItem;
        }
        catch
        {
            return null;
        }
    }

    private static ItemType SuggestItemTypeFromTitle(string title, string type)
    {
        if (!BoothMapping.CategoryMappings.TryGetValue(type, out ItemType categorySuggestedType))
            categorySuggestedType = ItemType.Unknown;
        
        ItemType titleMatchedType = BoothMapping.TitleMappings.FirstOrDefault(mapping => mapping.Key.Any(title.Contains)).Value;
        return titleMatchedType != ItemType.Unknown && titleMatchedType != default ? titleMatchedType : categorySuggestedType;
    }
}
