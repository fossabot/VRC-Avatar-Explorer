using System.Diagnostics;
using AvatarExplorer.Test.Localization;
using AvatarExplorer.Test.Utils;

var avatarExplorer = new AvatarExplorer.Core.AvatarExplorer();
avatarExplorer.LoadItemDatabase("./ItemsData.json");

Localizer.Instance.LoadFromFile("./ja-JP.json");

var searchFilter = SearchUtils.BuildFilter("Avatar=キプフェル まめひなた");

Stopwatch stopwatch = Stopwatch.StartNew();
avatarExplorer.SearchItems(searchFilter);
stopwatch.Stop();

Console.WriteLine("{0}ms", stopwatch.ElapsedMilliseconds);
