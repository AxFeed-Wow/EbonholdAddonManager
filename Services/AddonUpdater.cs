using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using EbonholdAddonManager.Models;

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
        AddonDefinition definition,
        CancellationToken cancellationToken = default)
    {
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

        Directory.CreateDirectory(
            tempRoot
        );

        try
        {
            // =========================================================
            // 1. Download the release asset or the branch source
            // =========================================================
            //
            // A release asset is the source of truth: its .toc may
            // carry build info (e.g. a commit hash) that the branch
            // .toc lacks, so it is not checked against the branch.
            // =========================================================

            string remoteVersion = "";

            if (!string.IsNullOrWhiteSpace(definition.ReleaseAsset))
            {
                GitHubRelease release =
                    await _gitHubService.GetLatestReleaseAsync(
                        definition.Repository,
                        definition.ReleaseAsset,
                        definition.IncludePrerelease,
                        cancellationToken
                    )
                    ?? throw new InvalidOperationException(
                        $"No GitHub release of {definition.Repository} " +
                        $"has an asset matching {definition.ReleaseAsset}."
                    );

                await _gitHubService.DownloadFileAsync(
                    release.AssetUrl,
                    zipPath,
                    cancellationToken
                );
            }
            else
            {
                remoteVersion =
                    await _gitHubService.GetRemoteVersionAsync(
                        definition.Repository,
                        definition.Branch,
                        definition.Folder,
                        cancellationToken
                    );

                if (string.IsNullOrWhiteSpace(remoteVersion))
                {
                    throw new InvalidOperationException(
                        "Unable to determine the remote version " +
                        "from the GitHub .toc file."
                    );
                }

                await _gitHubService.DownloadRepositoryAsync(
                    definition.Repository,
                    definition.Branch,
                    zipPath,
                    cancellationToken
                );
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(zipPath))
            {
                throw new InvalidOperationException(
                    "The download failed: " +
                    "the ZIP file was not found."
                );
            }

            if (new FileInfo(zipPath).Length == 0)
            {
                throw new InvalidOperationException(
                    "The downloaded ZIP file is empty."
                );
            }

            // =========================================================
            // 2. Extraction
            // =========================================================

            ZipFile.ExtractToDirectory(
                zipPath,
                extractPath
            );

            cancellationToken.ThrowIfCancellationRequested();

            string repositoryRoot =
                FindRepositoryRoot(
                    extractPath
                );

            // =========================================================
            // 3. Locate every folder of the package
            // =========================================================
            //
            // All folders are found and validated before the
            // installation is touched, so a package missing one of
            // them changes nothing.
            // =========================================================

            List<(string Folder, string Source)> folders = [];

            foreach (string addonFolder in definition.AllFolders)
            {
                string tocPath =
                    FindTocFile(
                        repositoryRoot,
                        addonFolder
                    );

                if (string.IsNullOrWhiteSpace(tocPath))
                {
                    throw new InvalidOperationException(
                        $"Unable to find " +
                        $"{addonFolder}.toc in the downloaded package."
                    );
                }

                string sourceFolder =
                    Path.GetDirectoryName(tocPath)
                    ?? repositoryRoot;

                ValidateAddon(
                    sourceFolder,
                    addonFolder,
                    "downloaded package"
                );

                folders.Add((addonFolder, sourceFolder));
            }

            // =========================================================
            // 4. Install each folder
            // =========================================================

            // ponytail: each folder is installed and rolled back on its
            // own; a disk error on a later folder leaves the earlier
            // ones updated. Share one rollback if that ever matters.
            foreach ((string addonFolder, string sourceFolder) in folders)
            {
                InstallFolder(
                    addonsFolder,
                    addonFolder,
                    sourceFolder,
                    addonFolder == definition.Folder
                        ? remoteVersion
                        : "",
                    Path.Combine(
                        tempRoot,
                        addonFolder
                    ),
                    cancellationToken
                );
            }

            // =========================================================
            // 5. Remove folders left from a previous addon name
            // =========================================================
            //
            // Only after the new folders are in place, so a failed
            // install leaves the old addon working. SavedVariables
            // live in WTF and are never touched.
            // =========================================================

            foreach (string legacyFolder in definition.LegacyFolders)
            {
                DeleteDirectorySafe(
                    Path.Combine(
                        addonsFolder,
                        legacyFolder
                    )
                );
            }
        }
        finally
        {
            DeleteDirectorySafe(
                tempRoot
            );
        }
    }

    // expectedVersion: when not empty, the .toc version every copy
    // of the addon must carry.
    private static void InstallFolder(
        string addonsFolder,
        string addonFolder,
        string sourceFolder,
        string expectedVersion,
        string workRoot,
        CancellationToken cancellationToken)
    {
        string targetFolder =
            Path.Combine(
                addonsFolder,
                addonFolder
            );

        bool wasAlreadyInstalled =
            Directory.Exists(targetFolder);

        string backupPath =
            Path.Combine(
                workRoot,
                "backup"
            );

        string newAddonPath =
            Path.Combine(
                workRoot,
                "new-addon"
            );

        string workingAddonPath =
            Path.Combine(
                workRoot,
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

        bool installationStarted = false;
        bool oldInstallationMoved = false;

        try
        {
            // =========================================================
            // 5. Copy and validate the downloaded package
            // =========================================================

            CopyDirectory(
                sourceFolder,
                newAddonPath
            );

            CheckVersion(
                newAddonPath,
                addonFolder,
                expectedVersion,
                "new version"
            );

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

            CheckVersion(
                workingAddonPath,
                addonFolder,
                expectedVersion,
                "prepared installation"
            );

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

            CheckVersion(
                targetFolder,
                addonFolder,
                expectedVersion,
                "installation"
            );

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

            throw;
        }
        finally
        {
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

    private static void CheckVersion(
        string folder,
        string addonFolder,
        string expectedVersion,
        string description)
    {
        ValidateAddon(
            folder,
            addonFolder,
            description
        );

        string version =
            ReadTocVersion(
                folder,
                addonFolder
            );

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidOperationException(
                $"Unable to read the version of " +
                $"{addonFolder}.toc in the {description}."
            );
        }

        if (!string.IsNullOrWhiteSpace(expectedVersion) &&
            !VersionsEqual(
                version,
                expectedVersion))
        {
            throw new InvalidOperationException(
                $"The {description} does not match " +
                "the GitHub version.\n\n" +
                $"GitHub: {expectedVersion}\n" +
                $"Found: {version}"
            );
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
