using System;
using System.IO;

namespace EbonholdAddonManager.Services;

public static class InstallationDetector
{
    public static string? FindEbonholdFolder()
    {
        string[] candidates =
        [
            // Installed directly on C:
            @"C:\Ebonhold",
            @"C:\Ebonhold\Ebonhold",

            // Installed directly on D:
            @"D:\Ebonhold",
            @"D:\Ebonhold\Ebonhold",

            // Current installation
            @"E:\ebonhold\Ebonhold",
            @"E:\Ebonhold",
            @"E:\Ebonhold\Ebonhold",

            // Program Files
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles),
                "Ebonhold"),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles),
                "Ebonhold",
                "Ebonhold"),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFilesX86),
                "Ebonhold"),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFilesX86),
                "Ebonhold",
                "Ebonhold")
        ];

        foreach (string candidate in candidates)
        {
            if (IsValidEbonholdFolder(candidate))
                return candidate;
        }

        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady)
                continue;

            string root = drive.RootDirectory.FullName;

            string[] rootCandidates =
            [
                Path.Combine(root, "Ebonhold"),
                Path.Combine(root, "ebonhold", "Ebonhold"),
                Path.Combine(root, "Games", "Ebonhold"),
                Path.Combine(root, "Games", "Ebonhold", "Ebonhold"),
                Path.Combine(root, "World of Warcraft"),
                Path.Combine(root, "World of Warcraft", "Ebonhold")
            ];

            foreach (string candidate in rootCandidates)
            {
                if (IsValidEbonholdFolder(candidate))
                    return candidate;
            }
        }

        return null;
    }

    public static string? ResolveEbonholdFolder(string selectedPath)
    {
        if (string.IsNullOrWhiteSpace(selectedPath))
            return null;

        try
        {
            selectedPath = Path.GetFullPath(selectedPath);
        }
        catch
        {
            return null;
        }

        if (IsValidEbonholdFolder(selectedPath))
            return selectedPath;

        string nestedEbonhold =
            Path.Combine(selectedPath, "Ebonhold");

        if (IsValidEbonholdFolder(nestedEbonhold))
            return nestedEbonhold;

        string? fromAddons =
            FindEbonholdFromAddonsFolder(selectedPath);

        if (fromAddons != null)
            return fromAddons;

        return null;
    }

    public static string? ResolveAddonsFolder(string selectedPath)
    {
        if (string.IsNullOrWhiteSpace(selectedPath))
            return null;

        try
        {
            selectedPath = Path.GetFullPath(selectedPath);
        }
        catch
        {
            return null;
        }

        if (IsValidAddonsFolder(selectedPath))
            return selectedPath;

        string addonsFromEbonhold =
            GetAddonsFolder(selectedPath);

        if (IsValidAddonsFolder(addonsFromEbonhold))
            return addonsFromEbonhold;

        string nestedEbonhold =
            Path.Combine(selectedPath, "Ebonhold");

        if (IsValidEbonholdFolder(nestedEbonhold))
        {
            string nestedAddons =
                GetAddonsFolder(nestedEbonhold);

            if (IsValidAddonsFolder(nestedAddons))
                return nestedAddons;
        }

        string? ebonholdFolder =
            FindEbonholdFromAddonsFolder(selectedPath);

        if (ebonholdFolder != null)
        {
            string addons =
                GetAddonsFolder(ebonholdFolder);

            if (IsValidAddonsFolder(addons))
                return addons;
        }

        return null;
    }

    private static string? FindEbonholdFromAddonsFolder(
        string path)
    {
        try
        {
            DirectoryInfo? current =
                new DirectoryInfo(path);

            for (int i = 0; i < 5 && current != null; i++)
            {
                if (IsValidEbonholdFolder(current.FullName))
                    return current.FullName;

                current = current.Parent;
            }
        }
        catch
        {
        }

        return null;
    }

    public static string GetAddonsFolder(string ebonholdFolder)
    {
        return Path.Combine(
            ebonholdFolder,
            "Interface",
            "AddOns");
    }

    public static string GetWtfFolder(string ebonholdFolder)
    {
        return Path.Combine(
            ebonholdFolder,
            "WTF");
    }

    public static bool IsValidEbonholdFolder(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return false;

            string wowExe =
                Path.Combine(path, "wow.exe");

            return File.Exists(wowExe);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidAddonsFolder(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return false;

            DirectoryInfo directory =
                new DirectoryInfo(path);

            if (!string.Equals(
                    directory.Name,
                    "AddOns",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            DirectoryInfo? interfaceFolder =
                directory.Parent;

            if (interfaceFolder == null)
                return false;

            if (!string.Equals(
                    interfaceFolder.Name,
                    "Interface",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            DirectoryInfo? ebonholdFolder =
                interfaceFolder.Parent;

            if (ebonholdFolder == null)
                return false;

            return IsValidEbonholdFolder(
                ebonholdFolder.FullName);
        }
        catch
        {
            return false;
        }
    }
}