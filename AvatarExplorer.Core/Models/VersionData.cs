using System.Text.Json.Serialization;

namespace AvatarExplorer.Core.Models;

public class VersionData
{
    public string LatestVersion { get; set; } = string.Empty;

    [JsonPropertyName("ChangeLog")]
    public string[] ChangeLogs { get; set; } = Array.Empty<string>();
}
