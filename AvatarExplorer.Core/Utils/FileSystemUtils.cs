using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace AvatarExplorer.Core.Utils;

public static class FileSystemUtils
{
    public static IEnumerable<string> EnumerateFiles(string root)
    {
        if (!Directory.Exists(root))
            yield break;

        var directories = new Stack<string>();
        directories.Push(root);

        while (directories.Count > 0)
        {
            var dir = directories.Pop();

            string[] subDirectories;

            try { subDirectories = Directory.GetDirectories(dir); }
            catch { continue; }

            foreach (var subDirectory in subDirectories)
            {
                directories.Push(subDirectory);
            }

            string[] files;

            try { files = Directory.GetFiles(dir); }
            catch { continue; }

            foreach (var f in files)
            {
                yield return f;
            }
        }
    }

    internal static void ModifyUnityPackageFilePathAsync(string itemPath, string itemCategoryName = "", IProgress<(string, int, string)>? progress = null)
    {
        bool isUnitypackage = itemPath.ToLower().EndsWith(".unitypackage");
        if (!isUnitypackage) return;

        _ = ModifyUnityPackageFilePathAsyncInternal(itemPath, itemCategoryName, progress);
    }

    private static async Task ModifyUnityPackageFilePathAsyncInternal(string itemPath, string itemCategoryName, IProgress<(string, int, string)>? progress = null)
    {
        await Task.Run(async () =>
        {
            progress?.Report((LocalizationKey.Processing.Unitypackage.Status.Preparing, 0, string.Empty));

            var (saveFolder, saveFilePath, unityPackagePath) = PrepareSavePaths(itemPath);
            PrepareSaveDirectory(saveFolder);

            progress?.Report((LocalizationKey.Processing.Unitypackage.Status.Extracting, 10, string.Empty));

            int totalEntries = await CountTarEntriesAsync(itemPath);
            await ExtractTarToFolderAsync(itemPath, saveFilePath, itemCategoryName, totalEntries, progress);

            progress?.Report((LocalizationKey.Processing.Unitypackage.Status.Creating, 90, string.Empty));

            CreateTarArchive(saveFilePath, unityPackagePath);

            Directory.Delete(saveFilePath, true);
            
            // ここの3つ目の引数で出力先のアイテムパスをUI側に返してあげる
            progress?.Report((LocalizationKey.Processing.Unitypackage.Status.Completed, 100, unityPackagePath));
        });
    }
    private static (string saveFolder, string saveFilePath, string unityPackagePath) PrepareSavePaths(string itemPath)
    {
        static string getNextFolder(string basePath)
        {
            int i = 1;
            while (Directory.Exists(Path.Combine(basePath, i.ToString())))
            {
                i++;
            }
            
            return Path.Combine(basePath, i.ToString());
        }

        var saveFolder = getNextFolder(SystemPath.TempFolderPath);
        string saveFilePath = Path.Combine(saveFolder, $"{Path.GetFileNameWithoutExtension(itemPath)}_export");
        string unityPackagePath = saveFilePath + ".unitypackage";

        return (saveFolder, saveFilePath, unityPackagePath);
    }
    private static void PrepareSaveDirectory(string tempFolder)
    {
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
    }
    private static async Task<int> CountTarEntriesAsync(string filePath)
    {
        int count = 0;
        await using var fileStream = File.OpenRead(filePath);
        await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        await using var tarReader = new TarReader(gzipStream);
        while (await tarReader.GetNextEntryAsync() is { })
            count++;
        return count;
    }
    private static async Task ExtractTarToFolderAsync(string tarGzFilePath, string saveFilePath, string category, int totalEntries, IProgress<(string, int, string)>? progress = null)
    {
        int processedEntries = 0;

        await using var fileStream = File.OpenRead(tarGzFilePath);
        await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        await using var tarReader = new TarReader(gzipStream);

        int lastProgress = -1;

        while (await tarReader.GetNextEntryAsync() is { } entry)
        {
            if (Path.GetFileName(entry.Name) == "pathname" && entry.DataStream != null)
            {
                using var reader = new StreamReader(entry.DataStream);
                string assetPath = await reader.ReadToEndAsync();

                // 親フォルダがAssetsのものだけ変更するようにする (Packagesフォルダは変更しない)
                if (assetPath.StartsWith("Assets"))
                    assetPath = assetPath.Insert(7, $"{category}/");

                entry.DataStream = new MemoryStream(Encoding.UTF8.GetBytes(assetPath));
            }

            string entryPath = Path.Combine(saveFilePath, entry.Name);
            if (entryPath.EndsWith('/'))
            {
                Directory.CreateDirectory(entryPath);
            }
            else
            {
                entry.DataStream ??= new MemoryStream();
                Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);
                await using var entryStream = File.Create(entryPath);
                await entry.DataStream.CopyToAsync(entryStream);
            }

            processedEntries++;
            int currentProgress = 10 + (int)(80.0 * processedEntries / totalEntries);

            if (currentProgress != lastProgress)
            {
                progress?.Report((LocalizationKey.Processing.Unitypackage.Status.Extracting, currentProgress, string.Empty));
                lastProgress = currentProgress;
            }
        }
    }
    private static void CreateTarArchive(string sourceFolder, string outputTarFile)
    {
        if (!Directory.Exists(sourceFolder)) throw new DirectoryNotFoundException(sourceFolder);

        using var archive = TarArchive.Create();

        foreach (string filePath in EnumerateFiles(sourceFolder))
        {
            string relativePath = Path.GetRelativePath(sourceFolder, filePath);
            archive.AddEntry(relativePath, filePath);
        }

        using var fileStream = new FileStream(outputTarFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1024 * 1024, FileOptions.SequentialScan);
        archive.SaveTo(fileStream, new WriterOptions(CompressionType.None));
    }
}
