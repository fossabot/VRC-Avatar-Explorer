using Avalonia.Interactivity;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void ShowDialog(string title, string content)
    {
        if (DialogTitle == null || DialogContent == null) return;

        DialogTitle.Text = title;
        DialogContent.Text = content;

        DialogOverlay.IsVisible = true;
    }
    private void Dialog_OKButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DialogOverlay == null) return;

        DialogOverlay.IsVisible = false;
    }
}
