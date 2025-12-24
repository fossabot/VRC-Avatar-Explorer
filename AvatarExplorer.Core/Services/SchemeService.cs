using System.Diagnostics;
using System.Security.Principal;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Utils;
using Microsoft.Win32;

namespace AvatarExplorer.Core.Services;

public static class SchemeService
{
    private static readonly string REG_PROTCOL = "VRCAE";
    private static readonly string SKIPPED_TEXT = "<sys>SKIPPED";

    /// <summary>
    /// カスタムURLスキームの登録用のヘルパー関数です。
    /// </summary>
    public static string? GetInternalSchemePath()
    {
        try
        {
            if (ProcessUtils.GetCurrentProcessPath() == null) return null;

            if (!File.Exists(SystemPath.SchemeFilePath)) return null;
            else return File.ReadAllText(SystemPath.SchemeFilePath);
        }
        catch
        {
            return null;
        }
    }

    public static bool IsSkipped(string text)
        => text == SKIPPED_TEXT;

    public static bool IsSchemeRegistered()
        => IsSchemeRegistered(REG_PROTCOL);

    public static void RegisterScheme()
    {
        if (!IsRunAsAdmin()) return;

        string? processPath = ProcessUtils.GetCurrentProcessPath();
        if (string.IsNullOrEmpty(processPath)) return;

        RegisterCustomScheme(REG_PROTCOL, processPath);
        File.WriteAllText(SystemPath.SchemeFilePath, processPath);
    }
    public static void MarkSchemeSkipped()
    {
        File.WriteAllText(SystemPath.SchemeFilePath, SKIPPED_TEXT);
    }
    
    public static bool IsRunAsAdmin()
    {
        if (!ProcessUtils.IsWindows()) return false;

#pragma warning disable CA1416 // プラットフォームの互換性を検証

        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);

#pragma warning restore CA1416 // プラットフォームの互換性を検証
    }

    public static void RestartAsAdmin()
    {
        string? processPath = ProcessUtils.GetCurrentProcessPath();
        if (string.IsNullOrEmpty(processPath)) return;

        ProcessStartInfo processStartInfo = new()
        {
            FileName = processPath,
            UseShellExecute = true,
            Verb = "runas"
        };

        Process.Start(processStartInfo);
        Environment.Exit(0);
    }

    private static void RegisterCustomScheme(string protocol, string processPath)
    {
        if (!ProcessUtils.IsWindows()) return;

#pragma warning disable CA1416 // プラットフォームの互換性を検証

        using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(protocol))
        {
            key.SetValue(string.Empty, "URL:" + protocol + " Protocol");
            key.SetValue("URL Protocol", string.Empty);
        }

        string commandKey = $@"{protocol}\shell\open\command";
        using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(commandKey))
        {
            key.SetValue(string.Empty, $"\"{processPath}\" \"%1\"");
        }

#pragma warning restore CA1416 // プラットフォームの互換性を検証
    }
    private static bool IsSchemeRegistered(string protocol)
    {
        if (!ProcessUtils.IsWindows()) return false;

#pragma warning disable CA1416 // プラットフォームの互換性を検証

        using RegistryKey? key = Registry.ClassesRoot.OpenSubKey(protocol);
        return key != null;

#pragma warning restore CA1416 // プラットフォームの互換性を検証
    }
}
