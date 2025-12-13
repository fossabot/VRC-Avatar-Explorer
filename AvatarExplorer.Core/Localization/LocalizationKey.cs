namespace AvatarExplorer.Core.Localization;

public static class LocalizationKey
{
    public static class ItemCategory
    {
        public const string Avatar = "ItemCategory.Avatar";
        public const string Clothing = "ItemCategory.Clothing";
        public const string Texture = "ItemCategory.Texture";
        public const string Gimmick = "ItemCategory.Gimmick";
        public const string Accessory = "ItemCategory.Accessory";
        public const string HairStyle = "ItemCategory.HairStyle";
        public const string Animation = "ItemCategory.Animation";
        public const string Tool = "ItemCategory.Tool";
        public const string Shader = "ItemCategory.Shader";
    }

    public static class UI
    {
        public static class Button
        {
            public static class Description
            {
                public static class Item
                {
                    public const string Author = "UI.Button.Description.Item.Author";
                    public const string Count = "UI.Button.Description.Item.Count";
                }

                public static class File
                {
                    public const string Extension = "UI.Button.Description.File.Extension";
                }
            }

            public static class Tag
            {
                public const string CommonAvatar = "UI.Button.Tag.CommonAvatar";
            }

            public static class ToolTip
            {
                public const string CreatedDate = "UI.Button.ToolTip.CreatedDate";
                public const string UpdatedDate = "UI.Button.ToolTip.UpdatedDate";
            }
        }

        public static class ItemWindow
        {
            public const string Page = "UI.ItemWindow.Page";
            public const string PageItemCount = "UI.ItemWindow.PageItemCount";
        }

        public static class Dialog
        {
            public static class Success
            {
                public const string Default = "UI.Dialog.Success.Default";
                public const string Import = "UI.Dialog.Success.Import";
                public const string Export = "UI.Dialog.Success.Export";
                public const string ItemAdd = "UI.Dialog.Success.ItemAdd";
                public const string ItemEdit = "UI.Dialog.Success.ItemEdit";
            }

            public static class Failed
            {
                public const string Default = "UI.Dialog.Failed.Default";
                public const string Import = "UI.Dialog.Faileds.Import";
                public const string Export = "UI.Dialog.Failed.Export";
                public const string ItemAdd = "UI.Dialog.Failed.ItemAdd";
                public const string ItemEdit = "UI.Dialog.Failed.ItemEdit";
            }

            public const string SelectSaveFilePath = "UI.Dialog.SelectSaveFilePath";
            public const string SelectFolderPath = "UI.Dialog.SelectFolderPath";
            public const string SelectFilePath = "UI.Dialog.SelectFilePath";
        }

        public static class Overlay
        {
            public static class EditFolder
            {
                public const string RemoveFolder = "UI.Overlay.EditFolder.RemoveFolder";
            }
        }
    }

    public static class FileCategory
    {
        public const string Modification = "FileCategory.Modification";
        public const string Texture = "FileCategory.Texture";
        public const string Document = "FileCategory.Document";
        public const string Unitypackage = "FileCategory.Unitypackage";
        public const string Material = "FileCategory.Material";
    }

    public static class Processing
    {
        public static class Unitypackage
        {
            public static class Status
            {
                public const string Preparing = "Processing.Unitypackage.Status.Preparing";
                public const string Extracting = "Processing.Unitypackage.Status.Extracting";
                public const string Creating = "Processing.Unitypackage.Status.Creating";
                public const string Completed = "Processing.Unitypackage.Status.Completed";
            }
        }

        public static class Booth
        {
            public static class Status
            {
                public const string Fetching = "Processing.Booth.Status.Fetching";
            }
        }

        public static class DirectoryCopy
        {
            public const string Copying = "Processing.DirectoryCopy.Copying";
        }

        public static class Import
        {
            public const string Copying = "Processing.Import.Copying";
        }

        public static class ItemAdd
        {
            public const string Copying = "Processing.ItemAdd.Copying";
        }
    }

    public static class Path
    {
        public const string Default = "Path.Default";
        public const string SearchResult = "Path.SearchResult";

        public static class Root
        {
            public const string Avatar = "Path.Root.Avatar";
            public const string Author = "Path.Root.Avatar";
            public const string Category = "Path.Root.Category";
        }
    }

    public static class SearchFilter
    {
        public const string Default = "SearchFilter.Default";
        public const string Title = "SearchFilter.Title";
        public const string Author = "SearchFilter.Author";
        public const string Booth = "SearchFilter.Booth";
        public const string SupportedAvatar = "SearchFilter.SupportedAvatar";
        public const string Category = "SearchFilter.Category";
        public const string ItemMemo = "SearchFilter.ItemMemo";
        public const string FolderName = "SearchFilter.FolderName";
        public const string FileName = "SearchFilter.FileName";
        public const string ImplementedAvatar = "SearchFilter.ImplementedAvatar";
        public const string NotImplementedAvatar = "SearchFilter.NotImplementedAvatar";
        public const string Tag = "SearchFilter.Tag";
        public const string CommonAvatar = "SearchFilter.CommonAvatar";
        public const string SearchWord = "SearchFilter.SearchWord";
        public const string IsOrSearch = "SearchFilter.IsOrSearch";
    }

    public static class Error
    {
        public const string Default = "Error.Default";
        public const string NotImplemented = "Error.NotImplemented";
        public const string Nothing = "Error.Nothing";
        public const string BoothItemNotFound = "Error.BoothItemNotFound";
        public const string BoothApiCooldown = "Error.BoothApiCooldown";
        public const string ItemNotFound = "Error.ItemNotFound";

        public static class Validation
        {
            public const string NoFolders = "Error.Validation.NoFolders";
            public const string EmptyTitle = "Error.Validation.EmptyTitle";
            public const string EmptyAuthor = "Error.Validation.Author";
        }

        public const string ItemAddFailed = "Error.Item.Add.Failed";
        public const string ItemFolderProcessingFailedPaths = "Error.Item.Folder.ProcessingFailedPaths";

    }

    public static class ContextMenu
    {
        public static class Item
        {
            public const string OpenFolder = "ContextMenu.Item.OpenFolder";

            public static class Booth
            {
                public const string Copy = "ContextMenu.Item.Booth.Copy";
                public const string Open = "ContextMenu.Item.Booth.Open";
            }

            public const string ShowOtherItemsByAuthor = "ContextMenu.Item.ShowOtherItemsByAuthor";
            
            public static class Thumbnail
            {
                public const string Change = "ContextMenu.Item.Thumbnail.Change";
                public const string Fetch = "ContextMenu.Item.Thumbnail.Fetch";
            }

            public static class Edit
            {
                public const string Default = "ContextMenu.Item.Edit.Default";
                public const string Implemented = "ContextMenu.Item.Edit.Implemented";
                public const string Tag = "ContextMenu.Item.Edit.Tag";
            }

            public static class Add
            {
                public const string Memo = "ContextMenu.Item.Add.Memo";
                public const string Folder = "ContextMenu.Item.Add.Folder";
            }

            public const string Remove = "ContextMenu.Item.Remove";
        }
    }
}
