using System;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void Main_ShowProgress(string title)
    {
        if (ProgressBarTitle == null || ProgressOverlay == null) return;
        
        ProgressBarTitle.Text = title;
        ProgressOverlay.IsVisible = true;
    }
    private void Main_HideProgress()
    {
        if (ProgressOverlay == null) return;

        ProgressOverlay.IsVisible = false;
    }
    private void Main_UpdateProgress(int value)
    {
        if (ProgressOverlay == null) return;

        ProgressBar.IsIndeterminate = value == 0;
        ProgressBar.Value = Math.Clamp(value, 0, 100);
    }
}
