using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace AvatarExplorer.Core.Utils;

public static class FileSystemUtils
{
    public static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true
    };
    
    public static void SerializeClass<T>(T values, string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(values, jsonSerializerOptions);
        File.WriteAllText(filePath, json);
    }

    public static IEnumerable<string> EnumerateFiles(string rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            yield break;

        Stack<string> directories = new();
        directories.Push(rootDirectory);

        while (directories.Count > 0)
        {
            string directory = directories.Pop();

            string[] subDirectories;

            try { subDirectories = Directory.GetDirectories(directory); }
            catch { continue; }

            foreach (string subDirectory in subDirectories)
            {
                directories.Push(subDirectory);
            }

            string[] files;

            try { files = Directory.GetFiles(directory); }
            catch { continue; }

            foreach (string file in files)
            {
                yield return file;
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

        string saveFolder = getNextFolder(SystemPath.TempFolderPath);
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

    internal static async Task<(string, string, List<string>)> ExtractItemFolders(ItemCreationContext itemCreationContext, string dataRootDirectory, string destinationDirectory, bool removeOriginal = false)
    {
        List<string> processingFailedPaths = new();

        string parentFolder = string.Empty;
        string othersFolder = string.Empty;
        string materialsFolder = string.Empty;
    
        for (int i = 0; i < itemCreationContext.Folders.Count; i++)
        {
            try
            {
                var extractedFolderPath = await ProcessExtractItemFoldersInternal(
                    itemCreationContext.Folders[i],
                    string.IsNullOrEmpty(parentFolder) ? destinationDirectory : othersFolder,
                    string.IsNullOrEmpty(parentFolder) ? (itemCreationContext.GetSafeTitle() ?? Path.GetFileNameWithoutExtension(itemCreationContext.Folders[i])) : Path.GetFileNameWithoutExtension(itemCreationContext.Folders[i]), // 親フォルダだけフォルダ名をタイトルに変換する
                    removeOriginal
                );

                if (string.IsNullOrEmpty(parentFolder))
                {
                    parentFolder = extractedFolderPath;
                    othersFolder = Path.Combine(parentFolder, "AE_Others");
                    materialsFolder = Path.Combine(parentFolder, "AE_Materials");
                }
            }
            catch
            {
                processingFailedPaths.Add(itemCreationContext.Folders[i]);
            }
        }

        try
        {
            if (!string.IsNullOrEmpty(parentFolder) && !string.IsNullOrEmpty(itemCreationContext.MaterialFolder))
            {
                await ProcessExtractItemFoldersInternal(
                    itemCreationContext.MaterialFolder,
                    materialsFolder,
                    Path.GetFileNameWithoutExtension(itemCreationContext.MaterialFolder),
                    removeOriginal
                );
            }
        }
        catch
        {
            processingFailedPaths.Add(itemCreationContext.MaterialFolder);
        }

        if (string.IsNullOrEmpty(parentFolder)) // 展開全てに失敗した時
        {
            return (string.Empty, string.Empty, processingFailedPaths);
        }

        return ($"<sys>{Path.GetRelativePath(dataRootDirectory, parentFolder)}", $"<sys>{Path.GetRelativePath(dataRootDirectory, materialsFolder)}", processingFailedPaths);
    }

    private const int BufferSize = 1024 * 1024;
    private static async Task<string> ProcessExtractItemFoldersInternal(string filePath, string destinationFolderPath, string folderName, bool removeOriginal)
    {
        var (extractedDestinationFolderPath, isDirectory) = FileExtractorInternal(filePath, destinationFolderPath, folderName, removeOriginal);
        if (isDirectory)
        {
            var copiedFolderPath = PrepareDestinationDirectoryInternal(destinationFolderPath, folderName);
            await CopyDirectory(filePath, copiedFolderPath); // フォルダが返された場合は、フォルダにコピーする
            extractedDestinationFolderPath = copiedFolderPath;
        }

        return extractedDestinationFolderPath;
    }
    private static async Task CopyDirectory(string sourceDirectory, string destinationDirectory, IProgress<(string, int, string)>? progress = null, int maxDegreeOfParallelism = 4)
    {
        var allFiles = EnumerateFiles(sourceDirectory).ToList();
        int totalFiles = allFiles.Count;

        int copiedFiles = 0;
        int lastPercent = -1;

        await Task.Run(async () =>
        {
            Parallel.ForEach(allFiles, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
            file =>
            {
                try
                {
                    string relativePath = Path.GetRelativePath(sourceDirectory, file);
                    string destPath = Path.Combine(destinationDirectory, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

                    using var sourceStream = File.OpenRead(file);
                    using var destStream = File.Create(destPath);
                    sourceStream.CopyTo(destStream, BufferSize);

                    copiedFiles++;
                    int percent = (int)(copiedFiles / (double)totalFiles * 100);
                    if (percent != lastPercent)
                    {
                        lastPercent = percent;
                        progress?.Report((LocalizationKey.Processing.DirectoryCopy.Copying, percent, string.Empty));
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            });
        });
    }
    private static (string extractedFolderPath, bool isDirectory) FileExtractorInternal(string filePath, string extractDirectory, string folderName, bool removeOriginalFile)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new InvalidOperationException("Path is Null Or Empty.");
        }

        if (Directory.Exists(filePath))
        {
            return (filePath, true); // フォルダが渡された場合はそのフォルダをそのまま返して上げる
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(filePath);
        }
        
        string extractedFolderPath;
        if (filePath.ToLower().EndsWith(".zip")) extractedFolderPath = ZipExtractor(filePath, extractDirectory, folderName);
        else if (filePath.ToLower().EndsWith(".rar")) extractedFolderPath = RarExtractor(filePath, extractDirectory, folderName);
        else if (filePath.ToLower().EndsWith(".7z")) extractedFolderPath = SevenZipExtractor(filePath, extractDirectory, folderName);
        else if (filePath.ToLower().EndsWith(".gz")) extractedFolderPath = GzipExtractor(filePath, extractDirectory, folderName);
        else throw new NotImplementedException();

        if (removeOriginalFile)
        {
            try { File.Delete(filePath); }
            catch{ }
        }

        return (extractedFolderPath, false);
    }
    private static string ZipExtractor(string filePath, string extractDirectory, string folderName)
    {
        var extractDirectoryFolder = PrepareDestinationDirectoryInternal(extractDirectory, folderName);

        using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open(filePath))
        EntriesProcessorInternal(extractDirectoryFolder, archive.Entries);

        return extractDirectoryFolder;
    }
    private static string RarExtractor(string filePath, string extractDirectory, string folderName)
    {
        var extractDirectoryFolder = PrepareDestinationDirectoryInternal(extractDirectory, folderName);
        
        using (var archive = SharpCompress.Archives.Rar.RarArchive.Open(filePath))
        EntriesProcessorInternal(extractDirectoryFolder, archive.Entries);

        return extractDirectoryFolder;
    }
    private static string SevenZipExtractor(string filePath, string extractDirectory, string folderName)
    {
        var extractDirectoryFolder = PrepareDestinationDirectoryInternal(extractDirectory, folderName);
        
        using (var archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(filePath))
        EntriesProcessorInternal(extractDirectoryFolder, archive.Entries);

        return extractDirectoryFolder;
    }
    private static string GzipExtractor(string filePath, string extractDirectory, string folderName)
    {
        var extractDirectoryFolder = PrepareDestinationDirectoryInternal(extractDirectory, folderName);
        
        using (var archive = SharpCompress.Archives.GZip.GZipArchive.Open(filePath))
        EntriesProcessorInternal(extractDirectoryFolder, archive.Entries);

        return extractDirectoryFolder;
    }
    private static string PrepareDestinationDirectoryInternal(string extractDirectory, string folderName)
    {
        var extractDirectoryFolder = Path.Combine(extractDirectory, folderName);

        if (Directory.Exists(extractDirectoryFolder))
        {
            int i = 1;
            while (Directory.Exists(extractDirectoryFolder + " - " + i)) i++;
            extractDirectoryFolder += " - " + i;
        }

        Directory.CreateDirectory(extractDirectoryFolder);

        return extractDirectoryFolder;
    }
    private static void EntriesProcessorInternal<T>(string extractDirectoryFolder, ICollection<T> entries)
        where T: Entry, IArchiveEntry
    {
        byte[] buffer = new byte[BufferSize];

        foreach (var entry in entries)
        {
            if (!entry.IsDirectory)
            {
                string fullPath = Path.Combine(extractDirectoryFolder, entry.Key!);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                using var inStream = entry.OpenEntryStream();
                using var outStream = File.Create(fullPath);

                int read;
                while ((read = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, read);
                }
            }
            else if (entry.Key != null)
            {
                Directory.CreateDirectory(Path.Combine(extractDirectoryFolder, entry.Key));
            }
        }
    }
}
