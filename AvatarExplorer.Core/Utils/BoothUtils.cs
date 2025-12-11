using System.Text.Json;
using System.Text.RegularExpressions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.Booth;

namespace AvatarExplorer.Core.Utils;

internal static partial class BoothUtils
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly Dictionary<string[], ItemType> BoothTitleMappings = new()
    {
        { new[] { "オリジナル3Dモデル", "オリジナル", "Avatar", "Original" }, ItemType.Avatar },
        { new[] { "アニメーション", "Animation" }, ItemType.Animation },
        { new[] { "衣装", "Clothing" }, ItemType.Clothing },
        { new[] { "ギミック", "Gimmick" }, ItemType.Gimmick },
        { new[] { "アクセサリ", "Accessory" }, ItemType.Accessory },
        { new[] { "髪", "Hair" }, ItemType.HairStyle },
        { new[] { "テクスチャ", "Eye", "Texture" }, ItemType.Texture },
        { new[] { "ツール", "システム", "Tool", "System" }, ItemType.Tool },
        { new[] { "シェーダー", "Shader" }, ItemType.Shader }
    };
    private static readonly Dictionary<string, ItemType> BoothCategoryMappings = new()
    {
        { "3Dキャラクター", ItemType.Avatar },
        { "3Dモデル（その他）" , ItemType.Avatar },
        { "3Dモーション・アニメーション", ItemType.Animation },
        { "3D小道具", ItemType.Gimmick },
        { "3D装飾品", ItemType.Accessory },
        { "3Dテクスチャ", ItemType.Texture },
        { "3Dツール・システム", ItemType.Tool }
    };

    [GeneratedRegex(@"https://(.*)\.booth\.pm/")]
    private static partial Regex BoothAuthorURLRegex();

    internal static async Task<BoothItem?> GetBoothItemAsync(string boothId)
    {
        try
        {
            string url = string.Format(BoothLink.ItemJsonURLFormat, boothId);
            string response = await HttpClient.GetStringAsync(url);

            BoothItem? boothItem = JsonSerializer.Deserialize<BoothItem>(response, JsonSerializerOptions);
            if (boothItem == null) return null;
            
            boothItem.EstimatedCategory = SuggestItemType(boothItem.Title, boothItem.Category.Name);
            boothItem.AuthorId = GetAuthorIdFromUrl(boothItem.Shop.Url);

            return boothItem;
        }
        catch
        {
            return null;
        }
    }

    private static string GetAuthorIdFromUrl(string url)
    {
        Match match = BoothAuthorURLRegex().Match(url);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static ItemType SuggestItemType(string title, string type)
    {
        if (!BoothCategoryMappings.TryGetValue(type, out ItemType categorySuggestedType))
            categorySuggestedType = ItemType.Unknown;
        
        ItemType titleMatchedType = BoothTitleMappings.FirstOrDefault(mapping => mapping.Key.Any(title.Contains)).Value;
        return titleMatchedType != ItemType.Unknown && titleMatchedType != default ? titleMatchedType : categorySuggestedType;
    }
}
