using System.Text;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

internal static class DataExporter
{
    internal static async Task ToCsv(List<Item> items, List<CommonAvatar> commonAvatars, Dictionary<ItemType, string> localizedItemTypesMapping, string filePath, bool includeCommonToSupported)
    {
        using StreamWriter sw = new(filePath, false, Encoding.UTF8);
        await sw.WriteLineAsync("Id,Title,AuthorName,AuthorImageFilePath,ImagePath,Type,Memo,SupportedAvatars,ImplementedAvatars,BoothId,ItemPath,Tags");

        foreach (Item item in items)
        {
            List<string> supportedAvatarNames = new();
            foreach (string supportedAvatarId in SupportedAvatarService.GetAllAvatarIds(item.SupportedAvatarsView, commonAvatars, includeCommonToSupported))
            {
                string avatarName = ItemUtils.GetAvatarNameFromId(items, supportedAvatarId);
                if (avatarName == null) continue;

                supportedAvatarNames.Add(avatarName);
            }

            List<string> implementedAvatarNames = new();
            foreach (string implementedAvatarId in item.ImplementedAvatarsView.Distinct())
            {
                string avatarName = ItemUtils.GetAvatarNameFromId(items, implementedAvatarId);
                if (avatarName == null) continue;

                implementedAvatarNames.Add(avatarName);
            }

            string itemId = CsvUtils.EscapeCsv(item.Id);
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
            string tags = CsvUtils.EscapeCsv(string.Join(Environment.NewLine, item.TagsView));

            await sw.WriteLineAsync($"{itemId},{itemTitle},{authorName},{authorImageFilePath},{imagePath},{type},{memo},{supportedAvatarsList},{implementedAvatarsList},{boothId},{itemPath},{tags}");
        }
    }
}
