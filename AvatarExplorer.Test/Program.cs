using AvatarExplorer.Core.Services;

// このプロジェクトは、UIを作る前に一度機能をテストするためのプレイグラウンドです。.gitignoreで無視されているため、テスト用にお使いください。
AvatarExplorerApp avatarExplorer = new();

#region Initializing
avatarExplorer.LoadItemDatabase();
avatarExplorer.LoadCommonAvatarDatabase();
avatarExplorer.LoadRuntimeSettings();

Console.WriteLine(string.Format("VRC Avatar Explorer ({0}) Launched!", AvatarExplorerApp.CurrentVersion));
Console.WriteLine("--------------------------------------------------------------------------------------------------");
Console.WriteLine(string.Format("Current Items count: {0}", avatarExplorer.GetAllItems().Count));
Console.WriteLine(string.Format("Current Common Avatars count: {0}", avatarExplorer.GetCommonAvatars().Count));
Console.WriteLine("--------------------------------------------------------------------------------------------------");
Console.WriteLine("");
#endregion

// ここからコードを書くことが出来ます
