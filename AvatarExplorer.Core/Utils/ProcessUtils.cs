using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AvatarExplorer.Core.Utils;

public static class ProcessUtils
{
    public static bool IsWindows() => CheckPlatForm(OSPlatform.Windows);
    public static bool IsLinux() => CheckPlatForm(OSPlatform.Linux);
    public static bool IsOSX() => CheckPlatForm(OSPlatform.OSX);
    public static bool IsFreeBSD() => CheckPlatForm(OSPlatform.FreeBSD);
    private static bool CheckPlatForm(OSPlatform platform) => RuntimeInformation.IsOSPlatform(platform);

    public static string? GetCurrentProcessPath() => Process.GetCurrentProcess()?.MainModule?.FileName;
}
