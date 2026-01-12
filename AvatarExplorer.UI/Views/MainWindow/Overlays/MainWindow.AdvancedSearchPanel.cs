using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    internal void AdvancedSearchPanel_ApplyValues(SearchFilter searchFilter)
    {
        searchFilter.Titles.AddRange(TextParser.Parse(AdvancedSearchPanel_Title.Text ?? ""));
        searchFilter.Authors.AddRange(TextParser.Parse(AdvancedSearchPanel_Author.Text ?? ""));
        searchFilter.BoothIds.AddRange(TextParser.Parse(AdvancedSearchPanel_Booth.Text ?? ""));
        searchFilter.SupportedAvatars.AddRange(TextParser.Parse(AdvancedSearchPanel_Avatar.Text ?? ""));
        searchFilter.Categories.AddRange(TextParser.Parse(AdvancedSearchPanel_Category.Text ?? ""));
        searchFilter.ItemMemos.AddRange(TextParser.Parse(AdvancedSearchPanel_Memo.Text ?? ""));
        searchFilter.FolderNames.AddRange(TextParser.Parse(AdvancedSearchPanel_Folder.Text ?? ""));
        searchFilter.FileNames.AddRange(TextParser.Parse(AdvancedSearchPanel_File.Text ?? ""));
        searchFilter.ImplementedAvatars.AddRange(TextParser.Parse(AdvancedSearchPanel_Implemented.Text ?? ""));
        searchFilter.NotImplementedAvatars.AddRange(TextParser.Parse(AdvancedSearchPanel_NotImplemented.Text ?? ""));
        searchFilter.Tags.AddRange(TextParser.Parse(AdvancedSearchPanel_Tag.Text ?? ""));
        searchFilter.CommonAvatars.AddRange(TextParser.Parse(AdvancedSearchPanel_CommonAvatar.Text ?? ""));
        searchFilter.Tags.AddRange(TextParser.Parse(AdvancedSearchPanel_Tag.Text ?? ""));
        searchFilter.IsOrSearch = AdvancedSearchPanel_OrSearch.IsChecked ?? false;
    }
}
