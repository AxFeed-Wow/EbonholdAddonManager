using System.IO.Compression;
using System.Security.Cryptography;
using EbonholdAddonManager.Models;

namespace EbonholdAddonManager.Services;

public sealed class AddonUpdater
{
    // Written in the addon folder when its .toc has no version, so
    // the installed commit can be compared with the branch.
    public const string CommitMarkerFile = ".ebonhold-commit";

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
            //
            // A branch whose .toc has no version is downloaded at its
            // current commit, which is then recorded in the addon.
            // =========================================================

            string remoteVersion = "";
            string remoteCommit = "";

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
                    remoteCommit =
                        await _gitHubService.GetBranchCommitAsync(
                            definition.Repository,
                            definition.Branch,
                            cancellationToken
                        );
                }

                if (string.IsNullOrWhiteSpace(remoteVersion) &&
                    string.IsNullOrWhiteSpace(remoteCommit))
                {
                    throw new InvalidOperationException(
                        "Unable to determine the remote version " +
                        "from the GitHub .toc file."
                    );
                }

                await _gitHubService.DownloadRepositoryAsync(
                    definition.Repository,
                    definition.Branch,
                    remoteCommit,
                    zipPath,
                    cancellationToken
                );
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(zipPath) ||
                new FileInfo(zipPath).Length == 0)
            {
                throw new InvalidOperationException(
                    "The downloaded ZIP file is missing or empty."
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
            // 3. Locate and validate every folder of the package
            // =========================================================
            //
            // Done before the installation is touched, so a package
            // missing a folder or carrying the wrong version changes
            // nothing.
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

            if (!string.IsNullOrWhiteSpace(remoteVersion))
            {
                string packagedVersion =
                    TocReader.ReadVersion(
                        folders[0].Source,
                        definition.Folder
                    );

                if (VersionComparer.Compare(
                        packagedVersion,
                        remoteVersion) != 0)
                {
                    throw new InvalidOperationException(
                        "The downloaded package does not match " +
                        "the GitHub version.\n\n" +
                        $"GitHub: {remoteVersion}\n" +
                        $"Package: {packagedVersion}"
                    );
                }
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
                    cancellationToken
                );
            }

            if (!string.IsNullOrWhiteSpace(remoteCommit))
            {
                File.WriteAllText(
                    Path.Combine(
                        addonsFolder,
                        definition.Folder,
                        CommitMarkerFile
                    ),
                    remoteCommit
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

    // Replaces addonsFolder/addonFolder with the package, keeping the
    // files the user added that the package does not ship.
    //
    // The old folder is first renamed aside, which is instant because
    // it stays on the same drive, and renamed back if anything fails.
    // A file locked by the game makes that rename fail before anything
    // has changed.
    private static void InstallFolder(
        string addonsFolder,
        string addonFolder,
        string sourceFolder,
        CancellationToken cancellationToken)
    {
        string targetFolder =
            Path.Combine(
                addonsFolder,
                addonFolder
            );

        string oldPath =
            Path.Combine(
                addonsFolder,
                $".EbonholdAddonManagerBackup_{Guid.NewGuid():N}"
            );

        bool wasAlreadyInstalled =
            Directory.Exists(targetFolder);

        bool installationStarted = false;

        try
        {
            if (wasAlreadyInstalled)
            {
                Directory.Move(
                    targetFolder,
                    oldPath
                );
            }

            installationStarted = true;

            cancellationToken.ThrowIfCancellationRequested();

            // CopyDirectory works across two different drives.
            CopyDirectory(
                sourceFolder,
                targetFolder
            );

            if (wasAlreadyInstalled)
            {
                CopyMissingFiles(
                    oldPath,
                    targetFolder
                );
            }

            // Full SHA-256 validation of the real installation.
            ValidatePackageFiles(
                sourceFolder,
                targetFolder,
                cancellationToken
            );

            ValidateAddon(
                targetFolder,
                addonFolder,
                "installation"
            );
        }
        catch
        {
            if (installationStarted)
            {
                DeleteDirectorySafe(
                    targetFolder
                );

                if (wasAlreadyInstalled)
                {
                    try
                    {
                        Directory.Move(
                            oldPath,
                            targetFolder
                        );
                    }
                    catch (Exception rollbackException)
                    {
                        throw new InvalidOperationException(
                            $"The update of {addonFolder} failed and " +
                            "the previous version could not be " +
                            "restored. It is kept in:\n" +
                            $"{oldPath}\n\n" +
                            $"Rollback error: {rollbackException.Message}",
                            rollbackException
                        );
                    }
                }
            }

            throw;
        }

        if (wasAlreadyInstalled)
        {
            DeleteDirectorySafe(
                oldPath
            );
        }
    }

    private static void CopyMissingFiles(
        string source,
        string destination)
    {
        foreach (string file in
                 Directory.EnumerateFiles(
                     source,
                     "*",
                     SearchOption.AllDirectories))
        {
            string destinationFile =
                Path.Combine(
                    destination,
                    Path.GetRelativePath(
                        source,
                        file
                    )
                );

            if (File.Exists(destinationFile))
                continue;

            Directory.CreateDirectory(
                Path.GetDirectoryName(destinationFile)!
            );

            File.Copy(
                file,
                destinationFile
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
