namespace AvatarExplorer.Core.Services;

internal static class ImageDownloader
{
    private static readonly HttpClient HttpClient = new();

    internal static async Task DownloadImageAsync(string url, string filePath, bool overwrite = false)
    {
        try
        {
            if (!overwrite && File.Exists(filePath)) return;

            byte[] imageBytes = await GetBytes(url);
            FileSystemService.PrepareDirectory(filePath);
            await File.WriteAllBytesAsync(filePath, imageBytes);
        }
        catch
        {
            // Ignored
        }
    }
    private static async Task<byte[]> GetBytes(string url)
    {
        return await HttpClient.GetByteArrayAsync(url);
    }
}
