using System.Diagnostics;
using System.Windows.Forms;

namespace EbonholdAddonManager.Services;

// Applies an application update on Windows, where a running executable
// cannot overwrite itself. A copy of the current exe is launched in
// "updater mode" to replace the installed files once the app has exited.
public static class AppUpdater
{
    public const string ApplyUpdateArg = "--apply-update";

    private const string ExeName = "EbonholdAddonManager.exe";

    // Called from the running app. Stages a runner copy of this exe and
    // launches it in updater mode. The caller must then exit.
    public static void LaunchAndExit(string newVersionDir)
    {
        string exePath =
            Environment.ProcessPath
            ?? throw new InvalidOperationException(
                "Unable to determine the current executable path."
            );

        string installDir =
            (Path.GetDirectoryName(exePath) ?? AppContext.BaseDirectory)
            .TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            );

        int pid = Environment.ProcessId;

        string runnerDir =
            Path.Combine(
                Path.GetTempPath(),
                "EbonholdAddonManager",
                "runner",
                Guid.NewGuid().ToString("N")
            );

        Directory.CreateDirectory(runnerDir);

        string runnerExe =
            Path.Combine(runnerDir, Path.GetFileName(exePath));

        File.Copy(exePath, runnerExe, true);

        ProcessStartInfo startInfo = new()
        {
            FileName = runnerExe,
            UseShellExecute = true
        };

        startInfo.ArgumentList.Add(ApplyUpdateArg);
        startInfo.ArgumentList.Add(installDir);
        startInfo.ArgumentList.Add(newVersionDir);
        startInfo.ArgumentList.Add(pid.ToString());

        // Elevate only if the install folder is not writable.
        if (AdminService.NeedsAdministrator(installDir))
            startInfo.Verb = "runas";

        Process.Start(startInfo);
    }

    // Runs inside the temp runner copy (dispatched from Program.Main when
    // started with --apply-update <installDir> <newVersionDir> <pid>).
    public static void RunUpdaterMode(string[] args)
    {
        if (args.Length < 4)
            return;

        string installDir = args[1];
        string newVersionDir = args[2];

        int pid =
            int.TryParse(args[3], out int parsed)
                ? parsed
                : -1;

        WaitForProcessExit(pid);

        string backupDir =
            Path.Combine(
                Path.GetTempPath(),
                "EbonholdAddonManager",
                "backup",
                Guid.NewGuid().ToString("N")
            );

        string targetExe =
            Path.Combine(installDir, ExeName);

        try
        {
            // Back up the current installation for rollback.
            CopyDirectory(installDir, backupDir);

            // Copy the new files over the installation.
            OverlayDirectory(newVersionDir, installDir);

            if (!File.Exists(targetExe))
            {
                throw new InvalidOperationException(
                    "The updated executable is missing after copy."
                );
            }

            Relaunch(targetExe, installDir);
        }
        catch (Exception ex)
        {
            bool restored = TryRestore(backupDir, installDir);

            MessageBox.Show(
                "The update could not be applied" +
                (restored
                    ? " and the previous version was restored."
                    : ".") +
                "\n\n" + ex.Message,
                "Ebonhold Addon Manager - Update",
                MessageBoxButtons.OK,
                restored
                    ? MessageBoxIcon.Warning
                    : MessageBoxIcon.Error
            );

            if (File.Exists(targetExe))
                Relaunch(targetExe, installDir);
        }
        finally
        {
            DeleteDirectorySafe(backupDir);
            DeleteDirectorySafe(newVersionDir);
        }
    }

    private static void Relaunch(string exePath, string workingDir)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = workingDir,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    private static bool TryRestore(string backupDir, string installDir)
    {
        try
        {
            if (Directory.Exists(backupDir))
            {
                OverlayDirectory(backupDir, installDir);
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    private static void WaitForProcessExit(int pid)
    {
        if (pid <= 0)
            return;

        try
        {
            Process process = Process.GetProcessById(pid);
            process.WaitForExit(30000);
        }
        catch
        {
            // Already exited or not accessible.
        }

        // Give the OS a moment to release file handles.
        Thread.Sleep(500);
    }

    private static void OverlayDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (string file in Directory.GetFiles(source))
        {
            File.Copy(
                file,
                Path.Combine(destination, Path.GetFileName(file)),
                true
            );
        }

        foreach (string directory in Directory.GetDirectories(source))
        {
            OverlayDirectory(
                directory,
                Path.Combine(destination, Path.GetFileName(directory))
            );
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (string file in Directory.GetFiles(source))
        {
            File.Copy(
                file,
                Path.Combine(destination, Path.GetFileName(file)),
                true
            );
        }

        foreach (string directory in Directory.GetDirectories(source))
        {
            CopyDirectory(
                directory,
                Path.Combine(destination, Path.GetFileName(directory))
            );
        }
    }

    private static void DeleteDirectorySafe(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        catch
        {
        }
    }
}
