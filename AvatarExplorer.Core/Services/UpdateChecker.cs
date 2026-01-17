using System.Text.Json;
using AvatarExplorer.Core.Data.Links;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

public static class UpdateChecker
{
    private static readonly HttpClient _httpClient = new();
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async static Task<VersionData?> CheckUpdate()
    {
        try
        {
            string response = await _httpClient.GetStringAsync(SoftwareLink.UpdateCheckURL);
            return JsonSerializer.Deserialize<VersionData>(response, JsonSerializerOptions);
        }
        catch
        {
            return null;
        }
    }
}
