using System.Globalization;
using AvatarExplorer.Core.Data.Paths;

namespace AvatarExplorer.Core.Services;

internal class BackupManager
{
    private int _backupInterval = 300000;
    private CancellationTokenSource? _backupCts;
    private Task? _backupTask;
    private DateTime _lastBackupDate = DateTime.MinValue;
    private string _backupRootFolderPath = string.Empty;

    internal void StartAutoBackup(int interval, string backupRootFolderPath)
    {
        SetAutoBackupPath(backupRootFolderPath);
        SetAutoBackupInterval(interval * 60 * 1000); // min to ms

        if (_backupTask != null) return;

        _backupCts = new CancellationTokenSource();
        _backupTask = Task.Run(() => AutoBackupLoop(_backupCts.Token));
    }

    internal async Task StopAutoBackup()
    {
        if (_backupCts != null)
        {
            await _backupCts.CancelAsync();

            try
            {
                if (_backupTask != null) await _backupTask;
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            finally
            {
                _backupCts.Dispose();
                _backupCts = null;
                _backupTask = null;
            }
        }
    }

    internal DateTime LastBackupTime => _lastBackupDate;

    public void SetAutoBackupInterval(int interval)
    {
        if (interval < 0) return;
        _backupInterval = interval * 60 * 1000;
    }

    public void SetAutoBackupPath(string path)
    {
        _backupRootFolderPath = path;
    }

    private async Task AutoBackupLoop(CancellationToken token)
    {
        await Task.Delay(60 * 1000, token); // 1分は待機する

        while (!token.IsCancellationRequested)
        {
            try
            {
                await ExecuteBackup(_backupRootFolderPath, token);
                await Task.Delay(_backupInterval, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
    private static readonly string[] _backupFiles =
    [
        SystemPath.ItemDatabasePath,
        SystemPath.CommonAvatarDatabasePath,
        SystemPath.RuntimeSettingsFilePath,
        SystemPath.UserPreferencesFilePath
    ];

    internal async Task ExecuteBackup(string backupRootFolderPath, CancellationToken token = default)
    {
        string now = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture);
        string backupFolderPath = Path.Combine(backupRootFolderPath, now);
        Directory.CreateDirectory(backupFolderPath);

        foreach (string filePath in _backupFiles.Where(File.Exists))
        {
            if (token.IsCancellationRequested) return;

            string fileName = Path.GetFileName(filePath);
            string backupPath = Path.Combine(backupFolderPath, fileName);

            await FileSystemService.CopyFile(filePath, backupPath);
        }

        _lastBackupDate = DateTime.Now;
    }
}
