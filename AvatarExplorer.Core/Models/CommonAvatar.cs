using System.Text.Json.Serialization;
using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

public class CommonAvatar : ISelectableItem
{
    public string GroupName { get; set; } = string.Empty;
    [JsonInclude] private List<string> Avatars { get; set; } = new List<string>();

    [JsonIgnore] public IReadOnlyList<string> AvatarsView => Avatars;

    public void UpdateAvatars(IEnumerable<string> avatars) => Avatars = avatars.ToList();

    [JsonIgnore] public static readonly string InternalPathPrefix = "<sys:commonavatar>";
    public string GetInternalPath() => InternalPathPrefix + GroupName;
    public static string? GetGroupName(string internalPath)
    {
        if (string.IsNullOrEmpty(internalPath))
            return null;

        if (!internalPath.StartsWith(InternalPathPrefix))
            return null;

        return internalPath[InternalPathPrefix.Length..];
    }
}
