using System.Text.Json.Serialization;
using AvatarExplorer.Core.Models.Updates;
using AvatarExplorer.UI.Models.Common;

namespace AvatarExplorer.UI.Models.Settings;

public class UserPreferences
{
    [JsonInclude]
    public int Language { get; private set; } = 0;

    [JsonInclude]
    public int NormalIconSize { get; private set; } = 70;

    [JsonInclude]
    public bool EnableHoverIconSize { get; private set; } = true;

    [JsonInclude]
    public int HoverIconSize { get; private set; } = 200;

    [JsonInclude]
    public bool UseBackgroundImage { get; private set; } = false;

    [JsonInclude]
    public string BackgroundImage { get; private set; } = string.Empty;

    [JsonInclude]
    public int BackgroundOpacity { get; private set; } = 20;

    [JsonInclude]
    public Theme Theme { get; private set; } = Theme.Dark;

    [JsonInclude]
    public int ItemsPerPage { get; private set; } = 30;

    [JsonInclude]
    public BitmapAntiAliasingMode AntiAliasingMode { get; private set; } = BitmapAntiAliasingMode.None;

    [JsonInclude]
    public bool CheckForUpdate { get; private set; } = true;

    [JsonInclude]
    public UpdateChannel UpdateChannel { get; private set; } = UpdateChannel.Stable;

    internal void FromOther(UserPreferences userPreferences)
    {
        Language = userPreferences.Language;
        NormalIconSize = userPreferences.NormalIconSize;
        HoverIconSize = userPreferences.HoverIconSize;
        EnableHoverIconSize = userPreferences.EnableHoverIconSize;
        UseBackgroundImage = userPreferences.UseBackgroundImage;
        BackgroundImage = userPreferences.BackgroundImage;
        BackgroundOpacity = userPreferences.BackgroundOpacity;
        Theme = userPreferences.Theme;
        ItemsPerPage = userPreferences.ItemsPerPage;
        AntiAliasingMode = userPreferences.AntiAliasingMode;
        CheckForUpdate = userPreferences.CheckForUpdate;
        UpdateChannel = userPreferences.UpdateChannel;
    }

    internal void SetLanguage(int index) => Language = index;
    internal void SetIconSize(int normalIconSize, int hoverIconSize)
    {
        NormalIconSize = normalIconSize;
        HoverIconSize = hoverIconSize;
    }
    internal void UseHoverIconSize(bool value) => EnableHoverIconSize = value;
    internal void UseBackground(bool value) => UseBackgroundImage = value;
    internal void SetBackground(string path) => BackgroundImage = path;
    internal void SetBackgroundOpacity(int value) => BackgroundOpacity = value;
    internal void SetTheme(Theme theme) => Theme = theme;
    internal void SetItemsPerPage(int value) => ItemsPerPage = value;
    internal void SetAntialiasing(BitmapAntiAliasingMode value) => AntiAliasingMode = value;
    internal void SetCheckForUpdate(bool value) => CheckForUpdate = value;
    internal void SetUpdateChannel(UpdateChannel value) => UpdateChannel = value;
}
