using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class RuntimeSettingsService
{
    internal static RuntimeSettings Load(string path)
    {
        return FileSystemService.DeserializeClass<RuntimeSettings>(path) ?? new();
    }

    internal static void Save(RuntimeSettings settings)
    {
        FileSystemService.SerializeClass(settings, SystemPath.RuntimeSettingsFilePath);
    }
}
