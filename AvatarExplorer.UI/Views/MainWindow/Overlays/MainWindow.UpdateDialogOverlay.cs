using System.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Data.Links;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private async Task UpdateDialogOverlay_Check()
    {
        VersionData? latestVersionData = await UpdateChecker.CheckUpdate();
        if (latestVersionData != null && latestVersionData.LatestVersion != AvatarExplorerApp.CurrentVersion) UpdateDialogOverlay_Show(latestVersionData.LatestVersion, latestVersionData.ChangeLogs);
    }
    private void UpdateDialogOverlay_Show(string latestVersion, string[] changeLogs)
    {
        UpdateDialogOverlay_VersionText.Text = Localizer.Instance.GetDisplayName(LocalizationKey.UI.Dialog.Update.VersionText, [latestVersion, AvatarExplorerApp.CurrentVersion]);
        UpdateDialogOverlay_UpdateContentText.Text = string.Join("\n", changeLogs.Select(i => $"・{i}"));
        UpdateDialogOverlay.IsVisible = true;
    }
    private void UpdateDialogOverlay_Hide() => UpdateDialogOverlay.IsVisible = false;

    #region Event Handler
    private void UpdateDialogOverlay_Later_Click(object? sender, RoutedEventArgs e) => UpdateDialogOverlay_Hide();
    private async void UpdateDialogOverlay_UpdateNow_Click(object? sender, RoutedEventArgs e)
    {
        await LauncherService.OpenUri(this, SoftwareLink.LatestReleasePageURL);
        UpdateDialogOverlay_Hide();
    }
    #endregion
}
