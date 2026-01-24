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
    internal static async Task<(List<Item>, List<CommonAvatar>)> FromV1(string dataFolderPath, RuntimeSettings runtimeSettings, Dictionary<ItemType, string> localizedItemTypesMapping, Action<(string, int)>? reportProgress = null)
    {
        reportProgress?.Invoke((LocalizationKey.Processing.Import.Copying, 0));

        List<ItemV1> v1Items = FileSystemService.DeserializeClass<List<ItemV1>>(SystemPathV1.ItemDatabasePath(dataFolderPath)) ?? [];
        List<CommonAvatarV1> v1CommonAvatars = FileSystemService.DeserializeClass<List<CommonAvatarV1>>(SystemPathV1.CommonAvatarDatabasePath(dataFolderPath)) ?? [];

        reportProgress?.Invoke((LocalizationKey.Processing.Import.Copying, 10));
        await FileSystemService.CopyDirectory(SystemPathV1.AuthorThumbnailsPath(dataFolderPath), SystemPath.AuthorThumbnailsPath);

        reportProgress?.Invoke((LocalizationKey.Processing.Import.Copying, 20));
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
            
            items.Add(FromV1(item));

            int percent = 20 + (int)(80.0 * i / v1Items.Count);
            if (percent != lastPercent)
            {
                lastPercent = percent;
                reportProgress?.Invoke((LocalizationKey.Processing.Import.Copying, percent));
            }
        }

        foreach (Item item in items)
        {
            IEnumerable<string> supportedAvatars = item.SupportedAvatarsView
                .Select(a => pathMapping.TryGetValue(a, out string? value) ? value : a);
            item.UpdateSupportedAvatars(supportedAvatars);

            IEnumerable<string> implementedAvatars = item.ImplementedAvatarsView
                .Select(a => pathMapping.TryGetValue(a, out string? value) ? value : a);
            item.UpdateImplementedAvatars(implementedAvatars);
        }

        List<CommonAvatar> commonAvatars = v1CommonAvatars.Select(FromV1).ToList();

        foreach (CommonAvatar commonAvatar in commonAvatars)
        {
            IEnumerable<string> avatarPaths = commonAvatar.AvatarsView
                .Select(a => pathMapping.TryGetValue(a, out string? value) ? value : a);
            commonAvatar.UpdateAvatars(avatarPaths);
        }

        reportProgress?.Invoke((LocalizationKey.Processing.Import.Copying, 100));

        return (items, commonAvatars);
    }
    private static Item FromV1(ItemV1 item)
    {
        Item migratedItem = new()
        {
            Title = item.Title,
            Author = item.AuthorName,
            AuthorId = item.AuthorId,
            BoothId = item.BoothId,
            ItemPath = item.ItemPath,
            ThumbnmailFileName = MigrateUtils.MigrateItemPath(item.ImagePath),
            AuthorThumbnmailFileName = MigrateUtils.MigrateItemPath(item.AuthorImageFilePath),
            Type = item.Type,
            CustomCategory = item.CustomCategory,
            ItemMemo = item.ItemMemo,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate,
        };

        migratedItem.UpdateSupportedAvatars(item.SupportedAvatar);
        migratedItem.UpdateImplementedAvatars(item.ImplementedAvatars);
        migratedItem.UpdateTags(item.Tags);

        return migratedItem;
    }
    private static CommonAvatar FromV1(CommonAvatarV1 commonAvatar)
    {
        CommonAvatar migratedCommonAvatar = new()
        {
            GroupName = commonAvatar.Name
        };

        migratedCommonAvatar.UpdateAvatars(commonAvatar.Avatars);

        return migratedCommonAvatar;
    }

    internal static async Task<List<Item>> FromKonoAsset(string dataFolderPath, RuntimeSettings runtimeSettings, Dictionary<ItemType, string> localizedItemTypesMapping, Action<(string, int)>? reportProgress = null)
    {
        reportProgress?.Invoke((LocalizationKey.Processing.Import.Copying, 0));

        List<IKonoAssetItem> konoAssetItems =
        [
            .. (FileSystemService.DeserializeClass<KonoAssetAvatarDatabase>(KonoAssetPath.AvatarsDatabasePath(dataFolderPath)) ?? new()).Data,
            .. (FileSystemService.DeserializeClass<KonoAssetWearableDatabase>(KonoAssetPath.AvatarWearablesDatabasePath(dataFolderPath)) ?? new()).Data,
            .. (FileSystemService.DeserializeClass<KonoAssetWorldDatabase>(KonoAssetPath.WorldObjectsDatabasePath(dataFolderPath)) ?? new()).Data,
        ];

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

                await Task.Delay(2250); // 750 * 3ms
            }

            items.Add(item);

            int percent = (int)(100.0 * i / konoAssetItems.Count);
            if (percent != lastPercent)
            {
                lastPercent = percent;
                reportProgress?.Invoke((LocalizationKey.Processing.Import.Copying, percent));
            }
        }

        reportProgress?.Invoke((LocalizationKey.Processing.Import.Copying, 100));

        return items;
    }
}
