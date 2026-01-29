using System;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void ProgressOverlay_Show(string title)
    {
        if (ProgressOverlay.IsVisible)
        {
            ProgressBarTitle.Text = title;
            return;
        }

        ProgressBarTitle.Text = title;
        ProgressOverlay.IsVisible = true;
    }
    private void ProgressOverlay_Hide()
    {
        ProgressOverlay.IsVisible = false;
    }
    private void ProgressOverlay_Update(int value)
    {
        if (!ProgressOverlay.IsVisible) return;
        ProgressBar.IsIndeterminate = value == 0;
        ProgressBar.Value = Math.Clamp(value, 0, 100);
    }
}
