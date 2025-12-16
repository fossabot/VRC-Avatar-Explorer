using System;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void Main_ShowProgress(string title)
    {
        ProgressBarTitle.Text = title;
        ProgressOverlay.IsVisible = true;
    }
    private void Main_HideProgress()
    {
        ProgressOverlay.IsVisible = false;
    }
    private void Main_UpdateProgress(int value)
    {
        ProgressBar.IsIndeterminate = value == 0;
        ProgressBar.Value = Math.Clamp(value, 0, 100);
    }
}
