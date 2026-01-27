using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace AvatarExplorer.Core.Services;

public static class FileSystemService
{
    #region Serialize / Deserialize
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };
    public static void SerializeClass<T>(T values, string filePath)
    {
        try
        {
            PrepareDirectory(filePath);
            string json = JsonSerializer.Serialize(values, JsonSerializerOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Type elementType = typeof(T).GetGenericArguments().FirstOrDefault() ?? typeof(T);
            string typeName = elementType.Name;
            ErrorManager.Instance.PostInternalError(string.Format("Failed to serialize class: '{0}'.", typeName), ex);
        }
    }
    public static T? DeserializeClass<T>(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            Type elementType = typeof(T).GetGenericArguments().FirstOrDefault() ?? typeof(T);
            string typeName = elementType.Name;
            ErrorManager.Instance.PostInternalError(string.Format("Failed to deserialize class '{0}'. Returning default value.", typeName), ex);

            return default;
        }
    }
    #endregion

    #region Unitypackage Modifier
    internal static async Task<string?> ModifyUnityPackageFilePathsAsync(string[] filePaths, string[] itemCategoryNames, Func<(string, int), Task>? reportProgress = null)
    {
        List<string> unitypackagePaths = new();
        
        foreach (string filePath in filePaths)
        {
            bool isUnitypackage = filePath.ToLower().EndsWith(".unitypackage");
            if (!isUnitypackage) continue;

            unitypackagePaths.Add(filePath);
        }

        if (unitypackagePaths.Count == 0) return null;
        
        return await ModifyUnityPackageFilePathsAsyncInternal(unitypackagePaths, itemCategoryNames, reportProgress);
    }
    private static async Task<string?> ModifyUnityPackageFilePathsAsyncInternal(List<string> itemPaths, string[] itemCategoryNames, Func<(string, int), Task>? reportProgress = null)
    {
        if (itemPaths.Count == 0) return null;

        string? unityPackagePath = await Task.Run(async () =>
        {
            try
            {
                if (reportProgress != null) await reportProgress.Invoke((LocalizationKey.Processing.Unitypackage.Status.Preparing, 0));

                var (saveFolder, saveFilePath, unityPackagePath) = PrepareSavePaths();
                PrepareSaveDirectory(saveFolder);

                if (reportProgress != null) await reportProgress.Invoke((LocalizationKey.Processing.Unitypackage.Status.Extracting, 10));

                int totalEntries = 0;
                foreach (string itemPath in itemPaths)
                {
                    totalEntries += await CountTarEntriesAsync(itemPath);
                }

                int currentProcessedEntries = 0;
                for (int i = 0; i < itemPaths.Count; i++)
                {
                    string itemPath = itemPaths[i];
                    currentProcessedEntries = await ExtractTarToFolderAsync(itemPath, saveFilePath, itemCategoryNames[i], totalEntries, currentProcessedEntries, reportProgress);
                }

                reportProgress?.Invoke((LocalizationKey.Processing.Unitypackage.Status.Creating, 90));

                await CreateTarArchive(saveFilePath, unityPackagePath);

                Directory.Delete(saveFilePath, true);
                
                // ここの3つ目の引数で出力先のアイテムパスをUI側に返してあげる
                reportProgress?.Invoke((LocalizationKey.Processing.Unitypackage.Status.Completed, 100));

                await Task.Delay(750); // すぐ閉じるのではなく、100%の表記を出してから0.75秒経って返すようにする

                return unityPackagePath;
            }
            catch (Exception ex)
            {
                ErrorManager.Instance.PostInternalError("Error occured while processing unitypackage.", ex);
                return null;
            }
        });

        return unityPackagePath;
    }
    private static (string saveFolder, string saveFilePath, string unityPackagePath) PrepareSavePaths()
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
        string saveFilePath = Path.Combine(saveFolder, "Unitypackages Modified by Avatar Explorer");
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
        await using Stream fileStream = File.OpenRead(filePath);
        await using GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        await using TarReader tarReader = new TarReader(gzipStream);
        while (await tarReader.GetNextEntryAsync() is { })
            count++;
        return count;
    }
    private static async Task<int> ExtractTarToFolderAsync(string tarGzFilePath, string saveFilePath, string category, int totalEntries, int currentProcessedEntries = 0, Func<(string, int), Task>? reportProgress = null)
    {
        int processedEntries = currentProcessedEntries;

        await using Stream fileStream = File.OpenRead(tarGzFilePath);
        await using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
        await using TarReader tarReader = new(gzipStream);

        int lastProgress = -1;

        while (await tarReader.GetNextEntryAsync() is { } entry)
        {
            try
            {
                if (Path.GetFileName(entry.Name) == "pathname" && entry.DataStream != null)
                {
                    using StreamReader reader = new(entry.DataStream);
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
                    await using Stream entryStream = File.Create(entryPath);
                    await entry.DataStream.CopyToAsync(entryStream);
                }
            }
            catch (Exception ex)
            {
                ErrorManager.Instance.PostInternalError(string.Format("An error occurred while processing entry '{0}'.", entry.Name), ex);
            }

            processedEntries++;
            int currentProgress = 10 + (int)(80.0 * processedEntries / totalEntries);

            if (currentProgress != lastProgress)
            {
                if (reportProgress != null) await reportProgress.Invoke((LocalizationKey.Processing.Unitypackage.Status.Extracting, currentProgress));
                lastProgress = currentProgress;
            }
        }

        return processedEntries;
    }
    private static async Task CreateTarArchive(string sourceFolder, string outputTarFile)
    {
        if (!Directory.Exists(sourceFolder)) throw new DirectoryNotFoundException(sourceFolder);

        using TarArchive archive = TarArchive.Create();

        foreach (string filePath in EnumerateFiles(sourceFolder))
        {
            string relativePath = Path.GetRelativePath(sourceFolder, filePath);
            archive.AddEntry(relativePath, filePath);
        }

        using FileStream fileStream = new(outputTarFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1024 * 1024, FileOptions.SequentialScan);
        await archive.SaveToAsync(fileStream, new WriterOptions(CompressionType.None));
    }
    #endregion

    #region Extract Item Folders
    internal static async Task<(string, List<string>)> ExtractItemFolders(ItemCreationContext itemCreationContext, string dataRootDirectory, bool removeOriginal = false)
    {
        List<string> processingFailedPaths = new();

        string parentFolder = string.Empty;
        string othersFolder = string.Empty;
    
        for (int i = 0; i < itemCreationContext.ItemPaths.Count; i++)
        {
            try
            {
                string extractedFolderPath = await ExtractItemInternalAsync(
                    itemCreationContext.ItemPaths[i],
                    string.IsNullOrEmpty(parentFolder) ? dataRootDirectory : othersFolder,
                    string.IsNullOrEmpty(parentFolder) ? (ItemUtils.GetSafeTitle(itemCreationContext.Title) ?? Path.GetFileNameWithoutExtension(itemCreationContext.ItemPaths[i])) : Path.GetFileNameWithoutExtension(itemCreationContext.ItemPaths[i]), // 親フォルダだけフォルダ名をタイトルに変換する
                    removeOriginal
                );

                if (string.IsNullOrEmpty(parentFolder))
                {
                    parentFolder = extractedFolderPath;
                    othersFolder = Path.Combine(parentFolder, "AE_Others");
                }
            }
            catch (Exception ex)
            {
                ErrorManager.Instance.PostInternalError(string.Format("An error occurred while processing folder '{0}'.", itemCreationContext.ItemPaths[i]), ex);
                processingFailedPaths.Add(itemCreationContext.ItemPaths[i]);
            }
        }

        if (string.IsNullOrEmpty(parentFolder)) // 展開全てに失敗した時
        {
            return (string.Empty, processingFailedPaths);
        }

        return ($"<sys>{Path.GetRelativePath(dataRootDirectory, parentFolder)}", processingFailedPaths);
    }
    internal static async Task<List<string>> ExtractItemPaths(string parentFolderPath, string[] itemPaths, bool removeOriginal = false)
    {
        List<string> processingFailedPaths = new();

        string destinationDirectory = Path.Combine(parentFolderPath, "AE_Others");

        foreach (string itemPath in itemPaths)
        {
            try
            {
                await ExtractItemInternalAsync(
                    itemPath,
                    destinationDirectory,
                    Path.GetFileNameWithoutExtension(itemPath),
                    removeOriginal
                );
            }
            catch (CryptographicException cryptographicException)
            {
                ErrorManager.Instance.PostInternalError(string.Format("File '{0}' is password-protected and is not supported.", itemPath), cryptographicException);
                processingFailedPaths.Add(itemPath);
            }
            catch (Exception ex)
            {
                ErrorManager.Instance.PostInternalError(string.Format("An error occurred while processing folder '{0}'.", itemPath), ex);
                processingFailedPaths.Add(itemPath);
            }
        }

        return processingFailedPaths;
    }
    private static async Task<string> ExtractItemInternalAsync(string filePath, string destinationFolderPath, string folderName, bool removeOriginal)
    {
        var (extractedDestinationFolderPath, isDirectory) = await FileExtractorInternalAsync(filePath, destinationFolderPath, folderName, removeOriginal);
        if (isDirectory)
        {
            string copiedFolderPath = GetUniquePath(destinationFolderPath, folderName, true);
            await CopyDirectory(filePath, copiedFolderPath);
            extractedDestinationFolderPath = copiedFolderPath;
        }

        return extractedDestinationFolderPath;
    }
    #endregion
    
    #region Extractor
    private static async Task<(string extractedFolderPath, bool isDirectory)> FileExtractorInternalAsync(string filePath, string extractDirectory, string folderName, bool removeOriginalFile)
    {
        if (Directory.Exists(filePath)) return (filePath, true); // フォルダが渡された場合はそのフォルダをそのまま返して上げる
        
        string extractedFolderPath;
        if (filePath.ToLower().EndsWith(".zip")) extractedFolderPath = await ZipExtractorAsync(filePath, extractDirectory, folderName);
        else if (filePath.ToLower().EndsWith(".rar")) extractedFolderPath = await RarExtractorAsync(filePath, extractDirectory, folderName);
        else if (filePath.ToLower().EndsWith(".7z")) extractedFolderPath = await SevenZipExtractorAsync(filePath, extractDirectory, folderName);
        else if (filePath.ToLower().EndsWith(".gz")) extractedFolderPath = await GzipExtractorAsync(filePath, extractDirectory, folderName);
        else if (filePath.ToLower().EndsWith(".tar")) extractedFolderPath = await TarExtractorAsync(filePath, extractDirectory, folderName);
        else throw new NotImplementedException(string.Format("Unsupported File Extension: {0}", Path.GetFileName(filePath)));

        if (removeOriginalFile)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                ErrorManager.Instance.PostInternalError(string.Format("Failed to remove original file: '{0}'", filePath), ex);
            }
        }

        return (extractedFolderPath, false);
    }

    private const int BufferSize = 1024 * 1024;
    private static async Task<string> ZipExtractorAsync(string filePath, string extractDirectory, string folderName)
    {
        string extractDirectoryFolder = GetUniquePath(extractDirectory, folderName, true);

        using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open(filePath))
        await ExtractEntriesAsync(extractDirectoryFolder, archive.Entries);

        return extractDirectoryFolder;
    }
    private static async Task<string> RarExtractorAsync(string filePath, string extractDirectory, string folderName)
    {
        string extractDirectoryFolder = GetUniquePath(extractDirectory, folderName, true);
        
        using (var archive = SharpCompress.Archives.Rar.RarArchive.Open(filePath))
        await ExtractEntriesAsync(extractDirectoryFolder, archive.Entries);

        return extractDirectoryFolder;
    }
    private static async Task<string> SevenZipExtractorAsync(string filePath, string extractDirectory, string folderName)
    {
        string extractDirectoryFolder = GetUniquePath(extractDirectory, folderName, true);
        
        using (var archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(filePath))
        await ExtractEntriesAsync(extractDirectoryFolder, archive.Entries);

        return extractDirectoryFolder;
    }
    private static async Task<string> GzipExtractorAsync(string filePath, string extractDirectory, string folderName)
    {
        string extractDirectoryFolder = GetUniquePath(extractDirectory, folderName, true);
        
        using (var archive = SharpCompress.Archives.GZip.GZipArchive.Open(filePath))
        await ExtractEntriesAsync(extractDirectoryFolder, archive.Entries);

        return extractDirectoryFolder;
    }
    private static async Task<string> TarExtractorAsync(string filePath, string extractDirectory, string folderName)
    {
        string extractDirectoryFolder = GetUniquePath(extractDirectory, folderName, true);
        
        using (var archive = SharpCompress.Archives.Tar.TarArchive.Open(filePath))
        await ExtractEntriesAsync(extractDirectoryFolder, archive.Entries);

        return extractDirectoryFolder;
    }
    private static async Task ExtractEntriesAsync<T>(string extractDirectoryFolder, ICollection<T> entries)
        where T: Entry, IArchiveEntry
    {
        byte[] buffer = new byte[BufferSize];

        foreach (T entry in entries)
        {
            if (!entry.IsDirectory)
            {
                string fullPath = Path.Combine(extractDirectoryFolder, entry.Key!);
                PrepareDirectory(fullPath);

                using Stream inStream = await entry.OpenEntryStreamAsync();
                using Stream outStream = File.Create(fullPath);

                int read;
                while ((read = await inStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await outStream.WriteAsync(buffer, 0, read);
                }
            }
            else if (entry.Key != null)
            {
                Directory.CreateDirectory(Path.Combine(extractDirectoryFolder, entry.Key));
            }
        }
    }
    #endregion

    #region Copy
    public static async Task CopyDirectory(string sourceDirectory, string destinationDirectory, Action<(string, int)>? reportProgress = null, int maxDegreeOfParallelism = 4)
    {
        if (sourceDirectory == destinationDirectory) return; // sourceとdestinationが同じ場合は無視

        Directory.CreateDirectory(destinationDirectory); // ファイル数が0の時、フォルダが生成されないため

        List<string> allFiles = EnumerateFiles(sourceDirectory).ToList();
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
                    PrepareDirectory(destPath);

                    using Stream sourceStream = File.OpenRead(file);
                    using Stream destStream = File.Create(destPath);
                    sourceStream.CopyTo(destStream, BufferSize);

                    copiedFiles++;
                    int percent = (int)(copiedFiles / (double)totalFiles * 100);
                    if (percent != lastPercent)
                    {
                        lastPercent = percent;
                        reportProgress?.Invoke((LocalizationKey.Processing.DirectoryCopy.Copying, percent));
                    }
                }
                catch (Exception ex)
                {
                    ErrorManager.Instance.PostInternalError(string.Format("Failed to copy file: '{0}'.", file), ex);
                }
            });
        });
    }
    public static async Task<string?> CopyFile(string sourceFile, string destinationFile, bool unique = false)
    {
        try
        {
            string uniqueFilePath = unique ? GetUniquePath(Path.GetDirectoryName(destinationFile) ?? destinationFile, Path.GetFileName(destinationFile)) : destinationFile;

            using Stream sourceStream = File.OpenRead(sourceFile);
            using Stream destStream = File.Create(uniqueFilePath);
            await sourceStream.CopyToAsync(destStream, BufferSize);

            return uniqueFilePath;
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostInternalError(string.Format("Failed to copy file: '{0}'.", sourceFile), ex);
            return null;
        }
    }
    #endregion

    public static void PrepareDirectory(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath) ?? filePath;
        Directory.CreateDirectory(directory);
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

            try
            {
                subDirectories = Directory.GetDirectories(directory);
            }
            catch (Exception ex)
            {
                ErrorManager.Instance.PostInternalError(string.Format("Failed to retrieve subdirectories for '{0}'.", directory), ex);
                continue;
            }

            foreach (string subDirectory in subDirectories)
            {
                directories.Push(subDirectory);
            }

            string[] files;

            try
            {
                files = Directory.GetFiles(directory);
            }
            catch (Exception ex)
            {
                ErrorManager.Instance.PostInternalError(string.Format("Failed to retrieve files for '{0}'.", directory), ex);
                continue;
            }

            foreach (string file in files)
            {
                yield return file;
            }
        }
    }

    public static string GetUniquePath(string directory, string fileName, bool isFolder = false)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);

        string path = Path.Combine(directory, fileName);
        if ((!isFolder && !File.Exists(path)) || (isFolder && !Directory.Exists(path))) return path;

        int index = 1;

        while (true)
        {
            string newName = $"{fileNameWithoutExtension} - {index}{extension}";
            string newPath = Path.Combine(directory, newName);

            if ((!isFolder && !File.Exists(newPath)) || (isFolder && !Directory.Exists(newPath)))
                return newPath;

            index++;
        }
    }

    public static bool DeleteDirectory(string path, bool recursive = true)
    {
        try
        {
            if (!Directory.Exists(path)) return false;
            Directory.Delete(path, recursive);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
