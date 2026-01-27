using System.Text.Json;
using AvatarExplorer.Core.Data.Links;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

public static class UpdateChecker
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async static Task<VersionData?> CheckUpdate()
    {
        try
        {
            string response = await HttpService.Client.GetStringAsync(SoftwareLink.UpdateCheckURL);
            return JsonSerializer.Deserialize<VersionData>(response, JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostInternalError(string.Format("Failed to retrieve update information: '{0}'.", SoftwareLink.UpdateCheckURL), ex);
            return null;
        }
    }
}
