using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils
{
    internal static class CommonAvatarUtils
    {
        internal static CommonAvatar? GetCommonAvatarFromName(IReadOnlyList<CommonAvatar> commonAvatars, string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            return commonAvatars.FirstOrDefault(commonAvatar => commonAvatar.GroupName == name);
        }
    }
}
