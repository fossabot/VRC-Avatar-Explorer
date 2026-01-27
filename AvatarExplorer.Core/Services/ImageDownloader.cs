namespace AvatarExplorer.Core.Services;

internal static class ImageDownloader
{
    internal static async Task<bool> Fetch(string url, string filePath, bool overwrite = false)
    {
        if (string.IsNullOrEmpty(filePath)) return false;
        
        if (!overwrite && File.Exists(filePath)) return true;

        try
        {
            byte[] imageBytes = await GetBytes(url);
            FileSystemService.PrepareDirectory(filePath);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            return true;
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostInternalError(string.Format("Failed to download image: '{0}'.", url), ex);
            return false;
        }
    }
    private static async Task<byte[]> GetBytes(string url) => await HttpService.Client.GetByteArrayAsync(url);
}
