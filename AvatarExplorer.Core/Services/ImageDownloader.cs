namespace AvatarExplorer.Core.Services;

internal static class ImageDownloader
{
    private static readonly HttpClient HttpClient = new();

    internal static async Task Fetch(string url, string filePath, bool overwrite = false)
    {
        if ((!overwrite && File.Exists(filePath)) || string.IsNullOrEmpty(url)) return;

        try
        {
            byte[] imageBytes = await GetBytes(url);
            FileSystemService.PrepareDirectory(filePath);
            await File.WriteAllBytesAsync(filePath, imageBytes);
        }
        catch
        {
            // Ignored
        }
    }
    private static async Task<byte[]> GetBytes(string url) => await HttpClient.GetByteArrayAsync(url);
}
