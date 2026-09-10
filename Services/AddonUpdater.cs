using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace EbonholdAddonManager.Services;

public sealed class AddonUpdater
{
    private readonly GitHubService _gitHubService;

    public AddonUpdater(GitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    public async Task UpdateAsync(
        string addonsFolder,
        string addonFolder,
        string repository,
        string branch,
        CancellationToken cancellationToken = default)
    {
        string targetFolder =
            Path.Combine(
                addonsFolder,
                addonFolder
            );

        bool wasAlreadyInstalled =
            Directory.Exists(targetFolder);

        string tempRoot =
            Path.Combine(
                Path.GetTempPath(),
                "EbonholdAddonManager",
                Guid.NewGuid().ToString("N")
            );

        string zipPath =
            Path.Combine(
                tempRoot,
                "addon.zip"
            );

        string extractPath =
            Path.Combine(
                tempRoot,
                "extracted"
            );

        string backupPath =
            Path.Combine(
                tempRoot,
                "backup"
            );

        string newAddonPath =
            Path.Combine(
                tempRoot,
                "new-addon"
            );

        string workingAddonPath =
            Path.Combine(
                tempRoot,
                "working-addon"
            );

        // IMPORTANT:
        // This folder must be on the same drive as the installation.
        // Directory.Move() cannot move a folder between
        // two different roots.
        string liveOldPath =
            Path.Combine(
                addonsFolder,
                $".EbonholdAddonManagerBackup_{Guid.NewGuid():N}"
            );

        Directory.CreateDirectory(
            tempRoot
        );

        bool installationStarted = false;
        bool oldInstallationMoved = false;

        try
        {
            // =========================================================
            // 1. Retrieve the GitHub version
            // =========================================================

            string remoteVersion =
                await _gitHubService.GetRemoteVersionAsync(
                    repository,
                    branch,
                    addonFolder,
                    cancellationToken
                );

            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(remoteVersion))
            {
                throw new InvalidOperationException(
                    "Unable to determine the remote version " +
                    "from the GitHub .toc file."
                );
            }

            // =========================================================
            // 2. Download
            // =========================================================

            await _gitHubService.DownloadRepositoryAsync(
                repository,
                branch,
                zipPath,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(zipPath))
            {
                throw new InvalidOperationException(
                    "The download failed: " +
                    "the ZIP file was not found."
                );
            }

            FileInfo zipInfo =
                new(zipPath);

            if (zipInfo.Length == 0)
            {
                throw new InvalidOperationException(
                    "The downloaded ZIP file is empty."
                );
            }

            // =========================================================
            // 3. Extraction
            // =========================================================

            ZipFile.ExtractToDirectory(
                zipPath,
                extractPath
            );

            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(extractPath))
            {
                throw new InvalidOperationException(
                    "Extraction of the GitHub repository failed."
                );
            }

            // =========================================================
            // 4. Locate the repository root
            // =========================================================

            string repositoryRoot =
                FindRepositoryRoot(
                    extractPath
                );

            if (!Directory.Exists(repositoryRoot))
            {
                throw new InvalidOperationException(
                    "Unable to determine the repository root."
                );
            }

            string tocPath =
                FindTocFile(
                    repositoryRoot,
                    addonFolder
                );

            if (string.IsNullOrWhiteSpace(tocPath))
            {
                throw new InvalidOperationException(
                    $"Unable to find " +
                    $"{addonFolder}.toc in the GitHub repository."
                );
            }

            string sourceFolder =
                Path.GetDirectoryName(tocPath)
                ?? repositoryRoot;

            if (!Directory.Exists(sourceFolder))
            {
                throw new InvalidOperationException(
                    "The addon source folder was not found."
                );
            }

            // =========================================================
            // 5. Validate the downloaded package
            // =========================================================

            ValidateAddon(
                sourceFolder,
                addonFolder,
                "GitHub source"
            );

            cancellationToken.ThrowIfCancellationRequested();

            CopyDirectory(
                sourceFolder,
                newAddonPath
            );

            ValidateAddon(
                newAddonPath,
                addonFolder,
                "new version"
            );

            string packagedVersion =
                ReadTocVersion(
                    newAddonPath,
                    addonFolder
                );

            if (string.IsNullOrWhiteSpace(
                    packagedVersion))
            {
                throw new InvalidOperationException(
                    $"Unable to read the version of " +
                    $"{addonFolder}.toc in the downloaded package."
                );
            }

            if (!VersionsEqual(
                    packagedVersion,
                    remoteVersion))
            {
                throw new InvalidOperationException(
                    "The downloaded package does not match " +
                    "the GitHub version.\n\n" +
                    $"GitHub: {remoteVersion}\n" +
                    $"Package: {packagedVersion}"
                );
            }

            // =========================================================
            // 6. Back up the current installation
            // =========================================================

            if (wasAlreadyInstalled)
            {
                CopyDirectory(
                    targetFolder,
                    backupPath
                );

                cancellationToken.ThrowIfCancellationRequested();

                if (!Directory.Exists(backupPath))
                {
                    throw new InvalidOperationException(
                        "Unable to create the backup " +
                        "of the previous version."
                    );
                }
            }

            // =========================================================
            // 7. Create the working version
            // =========================================================
            //
            // Start from the existing installation to preserve
            // the user's additional files.
            // =========================================================

            if (wasAlreadyInstalled)
            {
                CopyDirectory(
                    targetFolder,
                    workingAddonPath
                );
            }
            else
            {
                Directory.CreateDirectory(
                    workingAddonPath
                );
            }

            cancellationToken.ThrowIfCancellationRequested();

            // =========================================================
            // 8. Inject the new package
            // =========================================================
            //
            // The package files replace the matching
            // files.
            //
            // Existing additional files are preserved.
            // =========================================================

            OverlayDirectory(
                newAddonPath,
                workingAddonPath,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();

            // =========================================================
            // 9. Full SHA-256 validation
            // =========================================================

            ValidatePackageFiles(
                newAddonPath,
                workingAddonPath,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();

            ValidateAddon(
                workingAddonPath,
                addonFolder,
                "prepared installation"
            );

            string workingVersion =
                ReadTocVersion(
                    workingAddonPath,
                    addonFolder
                );

            if (string.IsNullOrWhiteSpace(
                    workingVersion))
            {
                throw new InvalidOperationException(
                    $"Unable to read the prepared version " +
                    $"of {addonFolder}."
                );
            }

            if (!VersionsEqual(
                    workingVersion,
                    remoteVersion))
            {
                throw new InvalidOperationException(
                    "The prepared version is incorrect.\n\n" +
                    $"GitHub: {remoteVersion}\n" +
                    $"Prepared: {workingVersion}"
                );
            }

            // =========================================================
            // 10. Replace the real installation
            // =========================================================
            //
            // WARNING:
            //
            // We do NOT do:
            //
            // Directory.Move(C:\Temp, E:\...)
            //
            // because the roots are different.
            //
            // We first move the old folder within
            // the same drive, then copy the new folder.
            // =========================================================

            installationStarted = true;

            if (Directory.Exists(targetFolder))
            {
                Directory.Move(
                    targetFolder,
                    liveOldPath
                );

                oldInstallationMoved = true;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // CopyDirectory works across two different drives.
            CopyDirectory(
                workingAddonPath,
                targetFolder
            );

            cancellationToken.ThrowIfCancellationRequested();

            // =========================================================
            // 11. SHA-256 validation of the real installation
            // =========================================================

            ValidatePackageFiles(
                newAddonPath,
                targetFolder,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();

            ValidateAddon(
                targetFolder,
                addonFolder,
                "installation"
            );

            string installedVersion =
                ReadTocVersion(
                    targetFolder,
                    addonFolder
                );

            if (string.IsNullOrWhiteSpace(
                    installedVersion))
            {
                throw new InvalidOperationException(
                    $"Unable to read the installed version " +
                    $"of {addonFolder}."
                );
            }

            if (!VersionsEqual(
                    installedVersion,
                    remoteVersion))
            {
                throw new InvalidOperationException(
                    "The installed version is incorrect.\n\n" +
                    $"GitHub: {remoteVersion}\n" +
                    $"Installed: {installedVersion}"
                );
            }

            // =========================================================
            // 12. Installation succeeded
            // =========================================================

            if (oldInstallationMoved)
            {
                DeleteDirectorySafe(
                    liveOldPath
                );

                oldInstallationMoved = false;
            }

            DeleteDirectorySafe(
                backupPath
            );
        }
        catch
        {
            // =========================================================
            // ROLLBACK
            // =========================================================

            if (installationStarted)
            {
                // Remove only the new installation
                // created during this operation.
                DeleteDirectorySafe(
                    targetFolder
                );

                // Restore first from the folder moved
                // on the same drive.
                if (oldInstallationMoved &&
                    Directory.Exists(liveOldPath))
                {
                    try
                    {
                        Directory.Move(
                            liveOldPath,
                            targetFolder
                        );

                        oldInstallationMoved = false;

                        if (!Directory.Exists(
                                targetFolder))
                        {
                            throw new InvalidOperationException(
                                "The rollback failed."
                            );
                        }
                    }
                    catch (Exception rollbackException)
                    {
                        throw new InvalidOperationException(
                            "The update failed and the " +
                            "rollback of the previous version " +
                            "also failed.\n\n" +
                            $"Rollback error: " +
                            $"{rollbackException.Message}"
                        );
                    }
                }
                else if (wasAlreadyInstalled &&
                         Directory.Exists(backupPath))
                {
                    // Fall back to the backup copy.
                    try
                    {
                        CopyDirectory(
                            backupPath,
                            targetFolder
                        );

                        if (!Directory.Exists(
                                targetFolder))
                        {
                            throw new InvalidOperationException(
                                "The rollback failed."
                            );
                        }
                    }
                    catch (Exception rollbackException)
                    {
                        throw new InvalidOperationException(
                            "The update failed and the " +
                            "rollback of the previous version " +
                            "also failed.\n\n" +
                            $"Rollback error: " +
                            $"{rollbackException.Message}"
                        );
                    }
                }
            }
        }
        finally
        {
            DeleteDirectorySafe(
                tempRoot
            );

            // Extra safety:
            // if the old folder is still present here,
            // we only try to delete it after success.
            //
            // If deletion fails, it stays on disk
            // rather than risking data loss.
            if (!oldInstallationMoved)
            {
                DeleteDirectorySafe(
                    liveOldPath
                );
            }
        }
    }

    private static string FindRepositoryRoot(
        string extractPath)
    {
        string[] directories =
            Directory.GetDirectories(
                extractPath,
                "*",
                SearchOption.TopDirectoryOnly
            );

        if (directories.Length == 1)
        {
            return directories[0];
        }

        return extractPath;
    }

    private static string FindTocFile(
        string repositoryRoot,
        string addonFolder)
    {
        string rootToc =
            Path.Combine(
                repositoryRoot,
                $"{addonFolder}.toc"
            );

        if (File.Exists(rootToc))
        {
            return rootToc;
        }

        string subFolderToc =
            Path.Combine(
                repositoryRoot,
                addonFolder,
                $"{addonFolder}.toc"
            );

        if (File.Exists(subFolderToc))
        {
            return subFolderToc;
        }

        string? toc =
            Directory
                .EnumerateFiles(
                    repositoryRoot,
                    $"{addonFolder}.toc",
                    SearchOption.AllDirectories
                )
                .FirstOrDefault();

        return toc ?? "";
    }

    private static void ValidateAddon(
        string folder,
        string addonFolder,
        string description)
    {
        if (!Directory.Exists(folder))
        {
            throw new InvalidOperationException(
                $"The {description} folder does not exist."
            );
        }

        string tocPath =
            Path.Combine(
                folder,
                $"{addonFolder}.toc"
            );

        if (!File.Exists(tocPath))
        {
            throw new InvalidOperationException(
                $"The {addonFolder}.toc file " +
                $"is missing from the {description}."
            );
        }

        string content;

        try
        {
            content =
                File.ReadAllText(tocPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Unable to read {addonFolder}.toc " +
                $"in the {description}.",
                ex
            );
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                $"{addonFolder}.toc is empty."
            );
        }

        string[] files =
            Directory.GetFiles(
                folder,
                "*",
                SearchOption.AllDirectories
            );

        if (files.Length == 0)
        {
            throw new InvalidOperationException(
                $"The {description} contains no files."
            );
        }
    }

    private static void ValidatePackageFiles(
        string packageFolder,
        string installedFolder,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(packageFolder))
        {
            throw new InvalidOperationException(
                "The package folder was not found."
            );
        }

        if (!Directory.Exists(installedFolder))
        {
            throw new InvalidOperationException(
                "The installation folder was not found."
            );
        }

        string[] packageFiles =
            Directory.GetFiles(
                packageFolder,
                "*",
                SearchOption.AllDirectories
            );

        if (packageFiles.Length == 0)
        {
            throw new InvalidOperationException(
                "The package contains no files."
            );
        }

        foreach (string packageFile in packageFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string relativePath =
                Path.GetRelativePath(
                    packageFolder,
                    packageFile
                );

            string installedFile =
                Path.Combine(
                    installedFolder,
                    relativePath
                );

            if (!File.Exists(installedFile))
            {
                throw new InvalidOperationException(
                    "Missing file after installation:\n" +
                    relativePath
                );
            }

            FileInfo packageInfo =
                new(packageFile);

            FileInfo installedInfo =
                new(installedFile);

            if (packageInfo.Length !=
                installedInfo.Length)
            {
                throw new InvalidOperationException(
                    "Incorrect file size after installation:\n" +
                    $"{relativePath}\n\n" +
                    $"Package: {packageInfo.Length} bytes\n" +
                    $"Installed: {installedInfo.Length} bytes"
                );
            }

            string packageHash =
                ComputeSha256(
                    packageFile
                );

            string installedHash =
                ComputeSha256(
                    installedFile
                );

            if (!string.Equals(
                    packageHash,
                    installedHash,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "A file's content does not match " +
                    "the downloaded package:\n" +
                    $"{relativePath}\n\n" +
                    $"SHA-256 package: {packageHash}\n" +
                    $"SHA-256 installed: {installedHash}"
                );
            }
        }
    }

    private static string ComputeSha256(
        string filePath)
    {
        using FileStream stream =
            new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

        byte[] hash =
            SHA256.HashData(
                stream
            );

        return Convert.ToHexString(
            hash
        );
    }

    private static void OverlayDirectory(
        string source,
        string destination,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException(
                $"Source folder not found: {source}"
            );
        }

        Directory.CreateDirectory(
            destination
        );

        foreach (string file in
                 Directory.GetFiles(source))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string destinationFile =
                Path.Combine(
                    destination,
                    Path.GetFileName(file)
                );

            File.Copy(
                file,
                destinationFile,
                true
            );
        }

        foreach (string directory in
                 Directory.GetDirectories(source))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string destinationDirectory =
                Path.Combine(
                    destination,
                    Path.GetFileName(directory)
                );

            Directory.CreateDirectory(
                destinationDirectory
            );

            OverlayDirectory(
                directory,
                destinationDirectory,
                cancellationToken
            );
        }
    }

    private static string ReadTocVersion(
        string addonFolderPath,
        string addonFolder)
    {
        string tocPath =
            Path.Combine(
                addonFolderPath,
                $"{addonFolder}.toc"
            );

        if (!File.Exists(tocPath))
            return "";

        try
        {
            string content =
                File.ReadAllText(
                    tocPath
                );

            Match match =
                Regex.Match(
                    content,
                    @"^##\s*Version\s*:\s*(.+)$",
                    RegexOptions.IgnoreCase |
                    RegexOptions.Multiline
                );

            if (!match.Success)
                return "";

            return match.Groups[1]
                .Value
                .Trim();
        }
        catch
        {
            return "";
        }
    }

    private static bool VersionsEqual(
        string versionA,
        string versionB)
    {
        return string.Equals(
            NormalizeVersion(versionA),
            NormalizeVersion(versionB),
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static string NormalizeVersion(
        string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return "";

        return Regex.Replace(
                version.Trim(),
                @"\s+",
                ""
            )
            .TrimStart(
                'v',
                'V'
            );
    }

    private static void CopyDirectory(
        string source,
        string destination)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException(
                $"Source folder not found: {source}"
            );
        }

        Directory.CreateDirectory(
            destination
        );

        foreach (string file in
                 Directory.GetFiles(source))
        {
            string destinationFile =
                Path.Combine(
                    destination,
                    Path.GetFileName(file)
                );

            File.Copy(
                file,
                destinationFile,
                true
            );
        }

        foreach (string directory in
                 Directory.GetDirectories(source))
        {
            string destinationDirectory =
                Path.Combine(
                    destination,
                    Path.GetFileName(directory)
                );

            CopyDirectory(
                directory,
                destinationDirectory
            );
        }
    }

    private static void DeleteDirectorySafe(
        string path)
    {
        if (!Directory.Exists(path))
            return;

        try
        {
            Directory.Delete(
                path,
                true
            );
        }
        catch
        {
        }
    }
}
