// This code is borrowed from Avalonia
// Github Code URL: https://github.com/AvaloniaUI/AvaloniaUI.QuickGuides/blob/main/ClipboardOps/ViewModels/MainWindowViewModel.cs

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Services.System;
using AvatarExplorer.UI.Localization;
using ErrorOr;

namespace AvatarExplorer.UI.Services.Utilities;

internal static class ClipboardService
{
    internal static async Task<ErrorOr<Success>> SetText(string text)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow?.Clipboard is not { } provider)
            {
                return Error.Failure("Failed to get os provider.");
            }

            await provider.SetTextAsync(text);

            return Result.Success;
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError("Failed to set text to clipboard.", ex, Localizer.Instance[LocalizationKey.Error.ClipboardFailed]);
            return Error.Failure("Failed to set text to clipboard.");
        }
    }
}
