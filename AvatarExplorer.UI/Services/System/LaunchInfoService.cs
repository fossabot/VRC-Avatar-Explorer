using System;
using System.Collections.Specialized;
using System.Web;
using AvatarExplorer.Core.Services.System;
using AvatarExplorer.UI.Models.System;

namespace AvatarExplorer.UI.Services.System;

public static class LaunchInfoService
{
    public static LaunchInfo? GetLaunchInfo(string url)
    {
        try
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
        catch (Exception ex)
        {
            ErrorManager.Instance.PostInternalError(string.Format("Failed to parse url: '{0}.'", url), ex);
            return null;
        }
    }
}
