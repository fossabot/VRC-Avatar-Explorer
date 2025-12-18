using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class RuntimeSettingsService
{
    internal static RuntimeSettings Load(string path)
    {
        try
        {
            if (!File.Exists(path)) return new();
            return FileSystemService.DeserializeClass<RuntimeSettings>(path) ?? new();
        }
        catch
        {
            return new();
        }
    }

    internal static void Save(RuntimeSettings settings)
    {
        try
        {
            FileSystemService.SerializeClass(settings, SystemPath.RuntimeSettingsFilePath);
        }
        catch
        {
            // Ignored
        }
    }

    internal static bool TrySetDataRootDirectory(RuntimeSettings settings, string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            settings.SetDataRootDirectory(SystemPath.DefaultItemsFolderPath);
            return false;
        }

        settings.SetDataRootDirectory(Path.GetFullPath(path));
        return true;
    }
}
