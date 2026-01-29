using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Data.Paths.External.KonoAsset;
using AvatarExplorer.Core.Data.Paths.External.V1;
using AvatarExplorer.Core.Interfaces.External.KonoAsset;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models.External.Booth;
using AvatarExplorer.Core.Models.External.KonoAsset.Databases;
using AvatarExplorer.Core.Models.External.V1;
using AvatarExplorer.Core.Models.Items;
using AvatarExplorer.Core.Models.System;
using AvatarExplorer.Core.Services.IO;
using AvatarExplorer.Core.Services.Network;
using AvatarExplorer.Core.Services.System;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

internal static class DataImporter
{
    internal static async Task<(List<Item>, List<CommonAvatar>)> FromV1(string dataFolderPath, RuntimeSettings runtimeSettings, Func<(string, int), Task>? reportProgress = null)
    {
        if (reportProgress != null) await reportProgress.Invoke((LocalizationKey.Processing.Import.Copying, 0));

        if (Directory.Exists(Path.Combine(dataFolderPath, "Datas"))) dataFolderPath = Path.Combine(dataFolderPath, "Datas");

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

            try
            {
                string safeItemTitle = ItemUtils.GetSafeTitle(item.Title) ?? Path.GetFileNameWithoutExtension(item.ItemPath);
                string newItemPath = FileSystemService.GetUniquePath(runtimeSettings.DataRootDirectory, safeItemTitle, isFolder: true) ?? throw new DirectoryNotFoundException("Counldn't get unique item path");
                string newItemMaterialPath = Path.Combine(newItemPath, "AE_Materials");

                await FileSystemService.CopyDirectory(ItemUtils.GetItemPath(SystemPathV1.ItemsPath(dataFolderPath), MigrateV1Path(item.ItemPath)), newItemPath);
                if (!string.IsNullOrEmpty(item.MaterialPath)) await FileSystemService.CopyDirectory(ItemUtils.GetItemPath(SystemPathV1.ItemsPath(dataFolderPath), MigrateV1Path(item.MaterialPath)), newItemMaterialPath);

                item.ItemPath = $"<sys>{Path.GetRelativePath(runtimeSettings.DataRootDirectory, newItemPath)}";

                Item newItem = FromV1(item);

                pathMapping[previousItemPath] = newItem.Id;

                items.Add(newItem);
            }
            catch (Exception ex)
            {
                ErrorManager.Instance.PostInternalError(string.Format("Failed to process item: '{0}'.", item.Title), ex);
            }

            int percent = 20 + (int)(80.0 * i / v1Items.Count);
            if (percent != lastPercent)
            {
                lastPercent = percent;
                if (reportProgress != null) await reportProgress.Invoke((LocalizationKey.Processing.Import.Copying, percent));
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

        if (reportProgress != null) await reportProgress.Invoke((LocalizationKey.Processing.Import.Copying, 100));

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
            ThumbnmailFileName = MigrateV1Path(item.ImagePath),
            AuthorThumbnmailFileName = MigrateV1Path(item.AuthorImageFilePath),
            Type = item.Type,
            CustomCategory = item.CustomCategory,
            ItemMemo = item.ItemMemo,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate
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
    private static string MigrateV1Path(string path)
    {
        const string V1ItemsFolderPrefix = "Datas\\Items\\";
        const string V1ThumbnailFolderPrefix = "Datas\\Thumbnail\\";
        const string V1AuthorThumbnailFolderPrefix = "Datas\\AuthorImage\\";

        if (path.StartsWith(V1ItemsFolderPrefix))
            return path.Replace(V1ItemsFolderPrefix, "<sys>"); // フルパスとアプリフォルダの区別をつけるため

        if (path.StartsWith(V1ThumbnailFolderPrefix))
            return path.Replace(V1ThumbnailFolderPrefix, string.Empty);

        if (path.StartsWith(V1AuthorThumbnailFolderPrefix))
            return path.Replace(V1AuthorThumbnailFolderPrefix, string.Empty);

        return path;
    }

    internal static async Task<List<Item>> FromKonoAsset(string dataFolderPath, RuntimeSettings runtimeSettings, Func<(string, int), Task>? reportProgress = null)
    {
        if (reportProgress != null) await reportProgress.Invoke((LocalizationKey.Processing.Import.Copying, 0));

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

            try
            {
                string safeItemTitle = ItemUtils.GetSafeTitle(item.Title) ?? Path.GetFileNameWithoutExtension(item.ItemPath);
                string newItemPath = FileSystemService.GetUniquePath(runtimeSettings.DataRootDirectory, safeItemTitle, isFolder: true);

                await FileSystemService.CopyDirectory(ItemUtils.GetItemPath(KonoAssetPath.ItemsPath(dataFolderPath), item.ItemPath), newItemPath);
                item.ItemPath = newItemPath;

                if (item.BoothId != -1)
                {
                    BoothItem? boothItem = await BoothService.GetItem(item.BoothId.ToString()); // もう一度取得してあげる

                    if (boothItem != null)
                    {
                        item.AuthorId = boothItem.AuthorId; // IKonoAssetItem.ToItem()ではAuthorIdは移行されないためここで設定する必要がある。

                        string itemThumbnailFileName = item.BoothId + ".png";
                        await ImageDownloader.Fetch(boothItem.Thumbnails.Count > 0 ? boothItem.Thumbnails[0].Original : string.Empty, Path.Combine(SystemPath.ItemThumbnailsPath, itemThumbnailFileName), false);
                        item.ThumbnmailFileName = itemThumbnailFileName;

                        string authorThumbnailFileName = item.AuthorId + ".png";
                        await ImageDownloader.Fetch(boothItem.Shop.ThumbnailUrl, Path.Combine(SystemPath.AuthorThumbnailsPath, authorThumbnailFileName), false);
                        item.AuthorThumbnmailFileName = authorThumbnailFileName;
                    }

                    await Task.Delay(2250); // (750 * 3)ms
                }

                items.Add(item);
            }
            catch (Exception ex)
            {
                ErrorManager.Instance.PostInternalError(string.Format("Failed to process item: '{0}'.", item.Title), ex);
            }

            int percent = (int)(100.0 * i / konoAssetItems.Count);
            if (percent != lastPercent)
            {
                lastPercent = percent;
                if (reportProgress != null) await reportProgress.Invoke((LocalizationKey.Processing.Import.Copying, percent));
            }
        }

        if (reportProgress != null) await reportProgress.Invoke((LocalizationKey.Processing.Import.Copying, 100));

        return items;
    }
}
