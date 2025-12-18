using System;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void ProgressOverlay_Show(string title)
    {
        ProgressBarTitle.Text = title;
        ProgressOverlay.IsVisible = true;
    }
    private void ProgressOverlay_Hide()
    {
        ProgressOverlay.IsVisible = false;
    }
    private void ProgressOverlay_Update(int value)
    {
        ProgressBar.IsIndeterminate = value == 0;
        ProgressBar.Value = Math.Clamp(value, 0, 100);
    }
}
