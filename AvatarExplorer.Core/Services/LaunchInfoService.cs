using System.Collections.Specialized;
using System.Web;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

public class LaunchInfoService
{
    public static LaunchInfo GetLaunchInfo(string url)
    {
        Uri uri = new(url);
        NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);

        string[] dir = query.GetValues("dir") ?? [];
        string id = query.Get("id") ?? string.Empty;

        return new LaunchInfo
        {
            AssetDirs = dir,
            AssetId = id
        };
    }
}
