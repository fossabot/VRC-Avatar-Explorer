using System;
using System.IO;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void SettingsOverlay_Show()
    {
        SettingsOverlay_SetUiValueFromCurrentSettings();
        SettingsOverlay.IsVisible = true;
    }
    private void SettingsOverlay_Hide()
        => SettingsOverlay.IsVisible = false;

    private void SettingsOverlay_SetUiValueFromCurrentSettings() // 設定画面を読み込んだ時に値をセットするための関数
    {
        if (!_main_initialized) return;

        RuntimeSettings runtimeSettings = _avatarExplorerApp.GetRuntimeSettings();
        UserPreferences userPreferences = _userPreferences;

        SettingsOverlay_ItemsFolderPathTextBox.Text = runtimeSettings.DataRootDirectory;
        SettingsOverlay_RemoveBracketsCheckBox.IsChecked = runtimeSettings.RemoveBrackets;
        SettingsOverlay_RemoveOriginalCheckBox.IsChecked = runtimeSettings.RemoveOriginal;
        SettingsOverlay_NormalIconSizeSlider.Value = userPreferences.NormalIconSize;
        SettingsOverlay_HoverIconSizeSlider.Value = userPreferences.HoverIconSize;
        SettingsOverlay_UseBackgroundImageCheckBox.IsChecked = userPreferences.UseBackgroundImage;
        SettingsOverlay_BackgroundImagePathTextBox.Text = userPreferences.BackgroundImage;
        SettingsOverlay_BackgroundImageOpacitySlider.Value = userPreferences.BackgroundOpacity;
        SettingsOverlay_ItemsPerPageTextBox.Text = userPreferences.ItemsPerPage.ToString();
        SettingsOverlay_ThemeComboBox.SelectedIndex = (int)userPreferences.Theme;
        SettingsOverlay_DefaultLanguageComboBox.SelectedIndex = userPreferences.DefaultLanguage;
        SettingsOverlay_DefaultSortOrderComboBox.SelectedIndex = (int)runtimeSettings.ItemSortOrder;
    }
    private void SettingsOverlay_ApplySettingsValues() // 設定の適用ボタンが押されたときのみ
    {
        if (!_main_initialized) return;

        _avatarExplorerApp.SetDataRootDirectory(SettingsOverlay_ItemsFolderPathTextBox.Text ?? "");
        _avatarExplorerApp.SetRemoveBrackets(SettingsOverlay_RemoveBracketsCheckBox.IsChecked ?? false);
        _avatarExplorerApp.SetRemoveOriginal(SettingsOverlay_RemoveOriginalCheckBox.IsChecked ?? false);
        _userPreferences.SetIconSize((int)SettingsOverlay_NormalIconSizeSlider.Value, (int)SettingsOverlay_HoverIconSizeSlider.Value);
        _userPreferences.UseBackground(SettingsOverlay_UseBackgroundImageCheckBox.IsChecked ?? false);
        _userPreferences.SetBackground(SettingsOverlay_BackgroundImagePathTextBox.Text ?? "");
        _userPreferences.SetBackgroundOpacity(Math.Clamp((int)SettingsOverlay_BackgroundImageOpacitySlider.Value, 0, 100));
        _userPreferences.SetItemsPerPage(int.TryParse(SettingsOverlay_ItemsPerPageTextBox.Text, out var count) ? count : 30);
        _userPreferences.SetTheme((Theme)SettingsOverlay_ThemeComboBox.SelectedIndex);
        _userPreferences.SetLanguage(SettingsOverlay_DefaultLanguageComboBox.SelectedIndex);
        _avatarExplorerApp.SetItemsSortOrder((SortOrder)SettingsOverlay_DefaultSortOrderComboBox.SelectedIndex);

        SettingsOverlay_ApplyPreferenceSettingsToUi();
        SettingsOverlay_ApplyRuntimeSettingsToUi();

        // 適用時は自動で保存する
        _avatarExplorerApp.SaveRuntimeSettings();
        UserPreferencesService.Save(_userPreferences);
    }
    
    private void SettingsOverlay_ApplyPreferenceSettingsToUi()
    {
        Application? currentApplication = Application.Current;
        if (currentApplication != null)
        {
            SettingsOverlay_SetApplicationTheme(currentApplication, _userPreferences.Theme);
            SettingsOverlay_SetBackground(_userPreferences.Theme);
            SettingsOverlay_ApplyBackgroundImage(_userPreferences);
        }
        
        Main_LanguageComboBox.SelectedIndex = _userPreferences.DefaultLanguage;
    }
    private void SettingsOverlay_SetApplicationTheme(Application application, Theme theme)
    {
        if (theme == Models.Theme.Dark) application.RequestedThemeVariant = ThemeVariant.Dark;
        else if (theme == Models.Theme.Light) application.RequestedThemeVariant = ThemeVariant.Light;
    }
    private void SettingsOverlay_SetBackground(Theme theme)
    {
        if (theme == Models.Theme.Dark) Background = new SolidColorBrush(new Color(255, 32, 32, 32));
        else if (theme == Models.Theme.Light) Background = new SolidColorBrush(new Color(255, 249, 249, 249));
    }
    private void SettingsOverlay_ApplyBackgroundImage(UserPreferences userPreferences)
    {
        if (userPreferences.UseBackgroundImage && File.Exists(userPreferences.BackgroundImage))
        {
            try
            {
                WindowGrid.Background = new ImageBrush()
                {
                    Source = new Bitmap(userPreferences.BackgroundImage),
                    Opacity = Math.Clamp(userPreferences.BackgroundOpacity / 100.0, 0, 1),
                    Stretch = Stretch.UniformToFill
                };
            }
            catch
            {
                WindowGrid.Background = null;
            }
        }
        else
        {
            WindowGrid.Background = null;
        }
    }
    private void SettingsOverlay_ApplyRuntimeSettingsToUi()
    {
        Main_SortOrderComboBox.SelectedIndex = (int)RuntimeSettings.ItemSortOrder;
    }

    #region Event Handler
    private async void SettingsOverlay_OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, "フォルダを選択してください", false);
        if (folders == null || folders.Length == 0) return;

        SettingsOverlay_ItemsFolderPathTextBox.Text = folders[0];
    }
    private async void SettingsOverlay_OpenBackgroundFile_Click(object? sender, RoutedEventArgs e)
    {
        string[]? files = await StorageService.OpenFileDialog(this, "ファイルを選択してください", false);
        if (files == null || files.Length == 0) return;

        SettingsOverlay_BackgroundImagePathTextBox.Text = files[0];
    }
    private void SettingsOverlay_Border_Click(object? sender, RoutedEventArgs e)
        => SettingsOverlay_Hide();
    private void SettingsOverlay_Close_Click(object? sender, RoutedEventArgs e)
        => SettingsOverlay_Hide();
    private void SettingsOverlay_Apply_Click(object? sender, RoutedEventArgs e)
    {
        SettingsOverlay_ApplySettingsValues();
        Main_ReloadCurrentWindow();
    }
    #endregion
}
