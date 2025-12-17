using System.Text;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

internal static class DataExporter
{
    internal static async Task ToCsv(List<Item> items, List<CommonAvatar> commonAvatars, Dictionary<ItemType, string> localizedItemTypesMapping, string filePath, bool includeImplementedToSupported)
    {
        using StreamWriter sw = new(filePath, false, Encoding.UTF8);
        await sw.WriteLineAsync("Title,AuthorName,AuthorImageFilePath,ImagePath,Type,Memo,SupportedAvatars,ImplementedAvatars,BoothId,ItemPath,Tags");

        foreach (Item item in items)
        {
            List<string> supportedAvatarNames = new();
            List<string> supportedAvatarPaths = new();

            foreach (string avatar in item.SupportedAvatars)
            {
                string avatarName = ItemUtils.GetAvatarNameFromPath(items, avatar);
                if (avatarName == null) continue;

                supportedAvatarNames.Add(avatarName);
                supportedAvatarPaths.Add(avatar);

                if (!includeImplementedToSupported) continue;

                IEnumerable<CommonAvatar> commonAvatarGroup = commonAvatars.Where(commonAvatar => commonAvatar.Avatars.Contains(avatar));
                foreach (CommonAvatar commonAvatar in commonAvatarGroup)
                {
                    foreach (string commonAvatarPath in commonAvatar.Avatars)
                    {
                        if (supportedAvatarPaths.Contains(commonAvatarPath)) continue;

                        string name = ItemUtils.GetAvatarNameFromPath(items, commonAvatarPath);
                        if (name == null) continue;

                        supportedAvatarNames.Add(name);
                        supportedAvatarPaths.Add(commonAvatarPath);
                    }
                }
            }

            List<string> implementedAvatarNames = new();
            foreach (string implementedAvatar in item.ImplementedAvatars)
            {
                string avatarName = ItemUtils.GetAvatarNameFromPath(items, implementedAvatar);
                if (avatarName == null) continue;

                implementedAvatarNames.Add(avatarName);
            }

            string itemTitle = CsvUtils.EscapeCsv(item.Title);
            string authorName = CsvUtils.EscapeCsv(item.Author);
            string authorImageFilePath = CsvUtils.EscapeCsv(item.AuthorThumbnmailFileName);
            string imagePath = CsvUtils.EscapeCsv(item.ThumbnmailFileName);
            string type = CsvUtils.EscapeCsv(item.Type == ItemType.Custom ? item.CustomCategory : localizedItemTypesMapping[item.Type]);
            string memo = CsvUtils.EscapeCsv(item.ItemMemo);
            string supportedAvatarsList = CsvUtils.EscapeCsv(string.Join(Environment.NewLine, supportedAvatarNames));
            string implementedAvatarsList = CsvUtils.EscapeCsv(string.Join(Environment.NewLine, implementedAvatarNames));
            string boothId = CsvUtils.EscapeCsv(item.BoothId.ToString());
            string itemPath = CsvUtils.EscapeCsv(item.ItemPath);
            string tags = CsvUtils.EscapeCsv(string.Join(Environment.NewLine, item.Tags));

            await sw.WriteLineAsync($"{itemTitle},{authorName},{authorImageFilePath},{imagePath},{type},{memo},{supportedAvatarsList},{implementedAvatarsList},{boothId},{itemPath},{tags}");
        }
    }
}
