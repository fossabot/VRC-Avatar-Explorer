using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models.KonoAsset;

namespace AvatarExplorer.Core.Services;

internal static class ItemCreator
{
    internal static async Task<(Item? newItem, List<string> processingFailedPaths)> FromItemCreationContext(ItemCreationContext itemCreationContext, RuntimeSettings runtimeSettings)
    {
        (string itemPath, List<string> processingFailedPaths) = await ExtractInternal(itemCreationContext, runtimeSettings);
        if (string.IsNullOrEmpty(itemPath)) return (null, processingFailedPaths);
        
        Item newItem = new()
        {
            Title = itemCreationContext.Title,
            Author = itemCreationContext.Author,
            AuthorId = itemCreationContext.AuthorId,
            BoothId = itemCreationContext.BoothId,
            ItemPath = itemPath,
            Type = itemCreationContext.ItemType,
            CustomCategory = itemCreationContext.CustomCategory
        };
        
        if (itemCreationContext.BoothId != -1) // Boothの情報を取得している状態が確定している
        {
            string itemThumbnailFileName = itemCreationContext.BoothId + ".png";
            await ImageDownloader.Download(itemCreationContext.ThumbnailUrl, Path.Combine(SystemPath.ItemThumbnailsPath, itemThumbnailFileName), false);
            newItem.ThumbnmailFileName = itemThumbnailFileName;

            string authorThumbnailFileName = itemCreationContext.AuthorId + ".png";
            await ImageDownloader.Download(itemCreationContext.AuthorThumbnailUrl, Path.Combine(SystemPath.AuthorThumbnailsPath, authorThumbnailFileName), false);
            newItem.AuthorThumbnmailFileName = authorThumbnailFileName;
        }

        newItem.UpdateSupportedAvatars(itemCreationContext.SupportedAvatars);

        return (newItem, processingFailedPaths);
    }
    private static async Task<(string itemPath, List<string> processingFailedPaths)> ExtractInternal(ItemCreationContext itemCreationContext, RuntimeSettings runtimeSettings)
    {
        string extractDestinationFolderPath = Path.Combine(runtimeSettings.DataRootDirectory, itemCreationContext.LocalizedItemTypeName);
        return await FileSystemService.ExtractItemFolders(itemCreationContext, runtimeSettings.DataRootDirectory, extractDestinationFolderPath, runtimeSettings.RemoveOriginal);
    }

    internal static Item FromKonoAssetDescription(KonoAssetDescription konoAssetDescription)
    {
        Item newItem = new()
        {
            Title = konoAssetDescription.Name,
            Author = konoAssetDescription.Creator,
            ThumbnmailFileName = konoAssetDescription.ImageFilename ?? string.Empty,
            ItemMemo = konoAssetDescription.Memo ?? string.Empty,
            BoothId = konoAssetDescription.BoothItemId ?? -1,
            CreatedDate = konoAssetDescription.CreatedAt.ToString(),
            UpdatedDate = konoAssetDescription.CreatedAt.ToString()
        };

        newItem.UpdateTags(konoAssetDescription.Tags);

        return newItem;
    }
}
