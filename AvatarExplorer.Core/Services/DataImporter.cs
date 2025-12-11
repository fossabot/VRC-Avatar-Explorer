using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.V1;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

internal static class DataImporter
{
    internal static async Task<(List<Item>, List<CommonAvatar>)> FromV1(string dataFolderPath, RuntimeSettings runtimeSettings, Dictionary<ItemType, string> localizedItemTypesMapping, IProgress<(string, int, string)>? progress = null)
    {
        progress?.Report((LocalizationKey.Processing.Import.Copying, 0, string.Empty));

        List<Item> items = DatabaseUtils.LoadItemsDataFromV1(SystemPathV1.ItemDatabasePath(dataFolderPath));
        List<CommonAvatar> commonAvatars = DatabaseUtils.LoadCommonAvatarsDataFromV1(SystemPathV1.CommonAvatarDatabasePath(dataFolderPath));

        progress?.Report((LocalizationKey.Processing.Import.Copying, 10, string.Empty));
        await FileSystemUtils.CopyDirectory(SystemPathV1.AuthorThumbnailsPath(dataFolderPath), SystemPath.AuthorThumbnailsPath);

        progress?.Report((LocalizationKey.Processing.Import.Copying, 20, string.Empty));
        await FileSystemUtils.CopyDirectory(SystemPathV1.ItemThumbnailsPath(dataFolderPath), SystemPath.ItemThumbnailsPath);

        // データ移行処理
        int lastPercent = -1;
        for (int i = 0; i < items.Count; i++)
        {
            Item item = items[i];
            string LocalizedCategoryName = item.Type == ItemType.Custom ? item.CustomCategory : localizedItemTypesMapping[item.Type];
            string safeItemTitle = ItemUtils.GetSafeTitle(item.Title) ?? Path.GetFileNameWithoutExtension(item.ItemPath);
            string newItemPath = Path.Combine(runtimeSettings.DataRootDirectory, LocalizedCategoryName, safeItemTitle);
            string newItemMaterialPath = Path.Combine(newItemPath, "AE_Materials");

            await FileSystemUtils.CopyDirectory(ItemUtils.GetItemPath(SystemPathV1.ItemsPath(dataFolderPath), item.ItemPath), newItemPath);
            if (!string.IsNullOrEmpty(item.MaterialPath)) await FileSystemUtils.CopyDirectory(ItemUtils.GetItemPath(SystemPathV1.ItemsPath(dataFolderPath), item.MaterialPath), newItemMaterialPath);

            item.ItemPath = newItemPath;

            int percent = 20 + (int)(80.0 * i / items.Count);
            if (percent != lastPercent)
            {
                lastPercent = percent;
                progress?.Report((LocalizationKey.Processing.Import.Copying, percent, string.Empty));
            }
        }

        progress?.Report((LocalizationKey.Processing.Import.Copying, 100, string.Empty));

        return (items, commonAvatars);
    }

    internal static async Task<List<Item>> FromKonoAsset(string dataFolderPath, RuntimeSettings runtimeSettings, Dictionary<ItemType, string> localizedItemTypesMapping, IProgress<(string, int, string)>? progress = null)
    {
        throw new NotImplementedException();
    }
}
