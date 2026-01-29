using System;
using System.IO;
using System.Threading;
using Avalonia;
using AvatarExplorer.Core.Services.System;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.UI;

static class Program
{
    private const string MutexName = "AvatarExplorerV2.SingleInstance";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Single Instance Check
        Mutex _ = new(true, MutexName, out bool isNew);

        if (!isNew)
        {
            SingleInstanceService.SendToServer(args);
            return;
        }

        // Set Current Directory
        string? currentProcessPath = Path.GetDirectoryName(ProcessUtils.GetCurrentProcessPath());
        if (currentProcessPath != null) Directory.SetCurrentDirectory(currentProcessPath);

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
