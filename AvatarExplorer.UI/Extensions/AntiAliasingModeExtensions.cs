using Avalonia.Media.Imaging;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI.Extensions;

internal static class AntiAliasingModeExtensions
{
    internal static BitmapInterpolationMode GetInterpolationMode(this BitmapAntiAliasingMode bitmapAntiAliasingMode)
    {
        return bitmapAntiAliasingMode switch
        {
            BitmapAntiAliasingMode.None => BitmapInterpolationMode.None,
            BitmapAntiAliasingMode.Low => BitmapInterpolationMode.LowQuality,
            BitmapAntiAliasingMode.Medium => BitmapInterpolationMode.MediumQuality,
            BitmapAntiAliasingMode.High => BitmapInterpolationMode.HighQuality,
            _ => BitmapInterpolationMode.Unspecified
        };
    }
}
