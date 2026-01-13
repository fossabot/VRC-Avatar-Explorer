using System.Text.Json.Serialization;
using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Models.V1;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Models;

public class CommonAvatar : ISelectableItem
{
    public string GroupName { get; set; } = string.Empty;
    [JsonInclude] public List<string> Avatars { get; private set; } = new List<string>();

    public void SetAvatars(IEnumerable<string> avatars, bool clear)
        => ListUtils.Add(Avatars, avatars, clear);

    public static CommonAvatar FromV1(CommonAvatarV1 commonAvatar)
    {
        CommonAvatar migratedCommonAvatar = new()
        {
            GroupName = commonAvatar.Name
        };

        migratedCommonAvatar.SetAvatars(commonAvatar.Avatars, true);

        return migratedCommonAvatar;
    }

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
