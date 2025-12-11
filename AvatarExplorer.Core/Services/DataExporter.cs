using System.Text;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

internal static class DataExporter
{
    internal static async Task ExportToCsv(List<Item> items, List<CommonAvatar> commonAvatars, Dictionary<ItemType, string> localizedItemTypesMapping, string filePath, bool includeImplementedToSupported)
    {
        using var sw = new StreamWriter(filePath, false, Encoding.UTF8);
        await sw.WriteLineAsync("Title,AuthorName,AuthorImageFilePath,ImagePath,Type,Memo,SupportedAvatars,ImplementedAvatars,BoothId,ItemPath,Tags");

        foreach (var item in items)
        {
            List<string> supportedAvatarNames = new();
            List<string> supportedAvatarPaths = new();

            foreach (var avatar in item.SupportedAvatars)
            {
                var avatarName = ItemUtils.GetAvatarNameFromPath(items, avatar);
                if (avatarName == null) continue;

                supportedAvatarNames.Add(avatarName);
                supportedAvatarPaths.Add(avatar);

                if (!includeImplementedToSupported) continue;

                var commonAvatarGroup = commonAvatars.Where(commonAvatar => commonAvatar.Avatars.Contains(avatar));
                foreach (var commonAvatar in commonAvatarGroup)
                {
                    foreach (var commonAvatarPath in commonAvatar.Avatars)
                    {
                        if (supportedAvatarPaths.Contains(commonAvatarPath)) continue;

                        var name = ItemUtils.GetAvatarNameFromPath(items, commonAvatarPath);
                        if (name == null) continue;

                        supportedAvatarNames.Add(name);
                        supportedAvatarPaths.Add(commonAvatarPath);
                    }
                }
            }

            List<string> implementedAvatarNames = new();
            foreach (var implementedAvatar in item.ImplementedAvatars)
            {
                var avatarName = ItemUtils.GetAvatarNameFromPath(items, implementedAvatar);
                if (avatarName == null) continue;

                implementedAvatarNames.Add(avatarName);
            }

            var itemTitle = CsvUtils.EscapeCsv(item.Title);
            var authorName = CsvUtils.EscapeCsv(item.Author);
            var authorImageFilePath = CsvUtils.EscapeCsv(item.AuthorThumbnmailFileName);
            var imagePath = CsvUtils.EscapeCsv(item.ThumbnmailFileName);
            var type = CsvUtils.EscapeCsv(item.Type == ItemType.Custom ? item.CustomCategory : localizedItemTypesMapping[item.Type]);
            var memo = CsvUtils.EscapeCsv(item.ItemMemo);
            var supportedAvatarsList = CsvUtils.EscapeCsv(string.Join(Environment.NewLine, supportedAvatarNames));
            var implementedAvatarsList = CsvUtils.EscapeCsv(string.Join(Environment.NewLine, implementedAvatarNames));
            var boothId = CsvUtils.EscapeCsv(item.BoothId.ToString());
            var itemPath = CsvUtils.EscapeCsv(item.ItemPath);
            var tags = CsvUtils.EscapeCsv(string.Join(Environment.NewLine, item.Tags));

            await sw.WriteLineAsync($"{itemTitle},{authorName},{authorImageFilePath},{imagePath},{type},{memo},{supportedAvatarsList},{implementedAvatarsList},{boothId},{itemPath},{tags}");
        }
    }
}
