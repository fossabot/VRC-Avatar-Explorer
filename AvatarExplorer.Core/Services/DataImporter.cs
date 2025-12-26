using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Data.Paths.KonoAsset;
using AvatarExplorer.Core.Data.Paths.V1;
using AvatarExplorer.Core.Interfaces.KonoAsset;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.Booth;
using AvatarExplorer.Core.Models.KonoAsset.Databases;
using AvatarExplorer.Core.Models.V1;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

internal static class DataImporter
{
    internal static async Task<(List<Item>, List<CommonAvatar>)> FromV1(string dataFolderPath, RuntimeSettings runtimeSettings, Dictionary<ItemType, string> localizedItemTypesMapping, IProgress<(string, int, string)>? progress = null)
    {
        progress?.Report((LocalizationKey.Processing.Import.Copying, 0, string.Empty));

        List<ItemV1> v1Items = FileSystemService.DeserializeClass<List<ItemV1>>(SystemPathV1.ItemDatabasePath(dataFolderPath)) ?? [];
        List<CommonAvatarV1> v1CommonAvatars = FileSystemService.DeserializeClass<List<CommonAvatarV1>>(SystemPathV1.CommonAvatarDatabasePath(dataFolderPath)) ?? [];

        progress?.Report((LocalizationKey.Processing.Import.Copying, 10, string.Empty));
        await FileSystemService.CopyDirectory(SystemPathV1.AuthorThumbnailsPath(dataFolderPath), SystemPath.AuthorThumbnailsPath);

        progress?.Report((LocalizationKey.Processing.Import.Copying, 20, string.Empty));
        await FileSystemService.CopyDirectory(SystemPathV1.ItemThumbnailsPath(dataFolderPath), SystemPath.ItemThumbnailsPath);

        List<Item> items = new();

        Dictionary<string, string> pathMapping = new();

        // データ移行処理
        int lastPercent = -1;
        for (int i = 0; i < v1Items.Count; i++)
        {
            ItemV1 item = v1Items[i];
            string previousItemPath = item.ItemPath;

            string LocalizedCategoryName = item.Type == ItemType.Custom ? item.CustomCategory : localizedItemTypesMapping[item.Type];
            string safeItemTitle = ItemUtils.GetSafeTitle(item.Title) ?? Path.GetFileNameWithoutExtension(item.ItemPath);
            string newItemPath = Path.Combine(runtimeSettings.DataRootDirectory, LocalizedCategoryName, safeItemTitle);
            string newItemMaterialPath = Path.Combine(newItemPath, "AE_Materials");

            await FileSystemService.CopyDirectory(ItemUtils.GetItemPath(SystemPathV1.ItemsPath(dataFolderPath), MigrateUtils.MigrateItemPath(item.ItemPath)), newItemPath);
            if (!string.IsNullOrEmpty(item.MaterialPath)) await FileSystemService.CopyDirectory(ItemUtils.GetItemPath(SystemPathV1.ItemsPath(dataFolderPath), MigrateUtils.MigrateItemPath(item.MaterialPath)), newItemMaterialPath);

            item.ItemPath = $"<sys>{Path.GetRelativePath(runtimeSettings.DataRootDirectory, newItemPath)}";
            pathMapping[previousItemPath] = item.ItemPath;
            
            items.Add(Item.FromV1(item));

            int percent = 20 + (int)(80.0 * i / v1Items.Count);
            if (percent != lastPercent)
            {
                lastPercent = percent;
                progress?.Report((LocalizationKey.Processing.Import.Copying, percent, string.Empty));
            }
        }

        // SupportedAvatarsとImplementedAvatarsのパスを更新する
        foreach (Item item in items)
        {
            IEnumerable<string> supportedAvatars = item.SupportedAvatars
                .Select(a => pathMapping.TryGetValue(a, out string? value) ? value : a)
                .ToArray();
            item.SetSupportedAvatars(supportedAvatars, true);

            IEnumerable<string> implementedAvatars = item.ImplementedAvatars
                .Select(a => pathMapping.TryGetValue(a, out string? value) ? value : a)
                .ToArray();
            item.SetImplementedAvatars(implementedAvatars, true);
        }

        List<CommonAvatar> commonAvatars = new();
        ListUtils.Add(commonAvatars, v1CommonAvatars.Select(CommonAvatar.FromV1), true);

        // 共通素体のパスを更新する
        foreach (CommonAvatar commonAvatar in commonAvatars)
        {
            IEnumerable<string> avatarPaths = commonAvatar.Avatars
                .Select(a => pathMapping.TryGetValue(a, out string? value) ? value : a)
                .ToArray();
            commonAvatar.SetAvatars(avatarPaths, true);
        }

        progress?.Report((LocalizationKey.Processing.Import.Copying, 100, string.Empty));

        return (items, commonAvatars);
    }

    internal static async Task<List<Item>> FromKonoAsset(string dataFolderPath, RuntimeSettings runtimeSettings, Dictionary<ItemType, string> localizedItemTypesMapping, IProgress<(string, int, string)>? progress = null)
    {
        progress?.Report((LocalizationKey.Processing.Import.Copying, 0, string.Empty));

        List<IKonoAssetItem> konoAssetItems = new();
        ListUtils.Add(konoAssetItems, (FileSystemService.DeserializeClass<KonoAssetAvatarDatabase>(KonoAssetPath.AvatarsDatabasePath(dataFolderPath)) ?? new()).Data, true);
        ListUtils.Add(konoAssetItems, (FileSystemService.DeserializeClass<KonoAssetWearableDatabase>(KonoAssetPath.AvatarWearablesDatabasePath(dataFolderPath)) ?? new()).Data, false);
        ListUtils.Add(konoAssetItems, (FileSystemService.DeserializeClass<KonoAssetWorldDatabase>(KonoAssetPath.WorldObjectsDatabasePath(dataFolderPath)) ?? new()).Data, false);

        List<Item> items = new();

        int lastPercent = -1;
        for (int i = 0; i < konoAssetItems.Count; i++)
        {
            Item item = konoAssetItems[i].ToItem();

            string LocalizedCategoryName = item.Type == ItemType.Custom ? item.CustomCategory : localizedItemTypesMapping[item.Type];
            string safeItemTitle = ItemUtils.GetSafeTitle(item.Title) ?? Path.GetFileNameWithoutExtension(item.ItemPath);
            string newItemPath = Path.Combine(runtimeSettings.DataRootDirectory, LocalizedCategoryName, safeItemTitle);

            await FileSystemService.CopyDirectory(ItemUtils.GetItemPath(KonoAssetPath.ItemsPath(dataFolderPath), item.ItemPath), newItemPath);
            item.ItemPath = newItemPath;

            if (item.BoothId != -1)
            {
                BoothItem? boothItem = await BoothService.GetItem(item.BoothId.ToString()); // もう一度取得してあげる

                if (boothItem != null)
                {
                    item.AuthorId = boothItem.AuthorId; // IKonoAssetItem.ToItem()ではAuthorIdは移行されないためここで設定する必要がある。

                    string itemThumbnailFileName = item.BoothId + ".png";
                    await ImageDownloader.Download(boothItem.Thumbnails.Count > 0 ? boothItem.Thumbnails[0].Original : string.Empty, Path.Combine(SystemPath.ItemThumbnailsPath, itemThumbnailFileName), false);
                    item.ThumbnmailFileName = itemThumbnailFileName;

                    string authorThumbnailFileName = item.AuthorId + ".png";
                    await ImageDownloader.Download(boothItem.Shop.ThumbnailUrl, Path.Combine(SystemPath.AuthorThumbnailsPath, authorThumbnailFileName), false);
                    item.AuthorThumbnmailFileName = authorThumbnailFileName;
                }

                await Task.Delay(5000);
            }

            items.Add(item);

            int percent = (int)(100.0 * i / konoAssetItems.Count);
            if (percent != lastPercent)
            {
                lastPercent = percent;
                progress?.Report((LocalizationKey.Processing.Import.Copying, percent, string.Empty));
            }
        }

        progress?.Report((LocalizationKey.Processing.Import.Copying, 100, string.Empty));

        return items;
    }
}
