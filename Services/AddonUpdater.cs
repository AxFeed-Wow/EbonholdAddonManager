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

        // IMPORTANT :
        // Ce dossier doit être sur le même disque que l'installation.
        // Directory.Move() ne peut pas déplacer un dossier entre
        // deux racines différentes.
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
            // 1. Récupération de la version GitHub
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
                    "Impossible de déterminer la version distante " +
                    "depuis le fichier .toc GitHub."
                );
            }

            // =========================================================
            // 2. Téléchargement
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
                    "Le téléchargement a échoué : " +
                    "le fichier ZIP est introuvable."
                );
            }

            FileInfo zipInfo =
                new(zipPath);

            if (zipInfo.Length == 0)
            {
                throw new InvalidOperationException(
                    "Le fichier ZIP téléchargé est vide."
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
                    "L'extraction du dépôt GitHub a échoué."
                );
            }

            // =========================================================
            // 4. Recherche de la racine du dépôt
            // =========================================================

            string repositoryRoot =
                FindRepositoryRoot(
                    extractPath
                );

            if (!Directory.Exists(repositoryRoot))
            {
                throw new InvalidOperationException(
                    "Impossible de déterminer la racine du dépôt."
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
                    $"Impossible de trouver " +
                    $"{addonFolder}.toc dans le dépôt GitHub."
                );
            }

            string sourceFolder =
                Path.GetDirectoryName(tocPath)
                ?? repositoryRoot;

            if (!Directory.Exists(sourceFolder))
            {
                throw new InvalidOperationException(
                    "Le dossier source de l'addon est introuvable."
                );
            }

            // =========================================================
            // 5. Validation du package téléchargé
            // =========================================================

            ValidateAddon(
                sourceFolder,
                addonFolder,
                "source GitHub"
            );

            cancellationToken.ThrowIfCancellationRequested();

            CopyDirectory(
                sourceFolder,
                newAddonPath
            );

            ValidateAddon(
                newAddonPath,
                addonFolder,
                "nouvelle version"
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
                    $"Impossible de lire la version de " +
                    $"{addonFolder}.toc dans le package téléchargé."
                );
            }

            if (!VersionsEqual(
                    packagedVersion,
                    remoteVersion))
            {
                throw new InvalidOperationException(
                    "Le package téléchargé ne correspond " +
                    "pas à la version GitHub.\n\n" +
                    $"GitHub : {remoteVersion}\n" +
                    $"Package : {packagedVersion}"
                );
            }

            // =========================================================
            // 6. Sauvegarde de l'installation actuelle
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
                        "Impossible de créer la sauvegarde " +
                        "de l'ancienne version."
                    );
                }
            }

            // =========================================================
            // 7. Création de la version de travail
            // =========================================================
            //
            // On part de l'installation existante afin de conserver
            // les fichiers supplémentaires de l'utilisateur.
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
            // 8. Injection du nouveau package
            // =========================================================
            //
            // Les fichiers du package remplacent les fichiers
            // correspondants.
            //
            // Les fichiers supplémentaires existants sont conservés.
            // =========================================================

            OverlayDirectory(
                newAddonPath,
                workingAddonPath,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();

            // =========================================================
            // 9. Validation SHA-256 complète
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
                "installation préparée"
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
                    $"Impossible de lire la version préparée " +
                    $"de {addonFolder}."
                );
            }

            if (!VersionsEqual(
                    workingVersion,
                    remoteVersion))
            {
                throw new InvalidOperationException(
                    "La version préparée est incorrecte.\n\n" +
                    $"GitHub : {remoteVersion}\n" +
                    $"Préparée : {workingVersion}"
                );
            }

            // =========================================================
            // 10. Remplacement de l'installation réelle
            // =========================================================
            //
            // ATTENTION :
            //
            // On ne fait PAS :
            //
            // Directory.Move(C:\Temp, E:\...)
            //
            // car les racines sont différentes.
            //
            // On déplace d'abord l'ancien dossier à l'intérieur
            // du même disque, puis on copie le nouveau dossier.
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

            // CopyDirectory fonctionne entre deux disques différents.
            CopyDirectory(
                workingAddonPath,
                targetFolder
            );

            cancellationToken.ThrowIfCancellationRequested();

            // =========================================================
            // 11. Validation SHA-256 de l'installation réelle
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
                    $"Impossible de lire la version installée " +
                    $"de {addonFolder}."
                );
            }

            if (!VersionsEqual(
                    installedVersion,
                    remoteVersion))
            {
                throw new InvalidOperationException(
                    "La version installée est incorrecte.\n\n" +
                    $"GitHub : {remoteVersion}\n" +
                    $"Installée : {installedVersion}"
                );
            }

            // =========================================================
            // 12. Installation réussie
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
                // Supprime uniquement la nouvelle installation
                // créée pendant cette opération.
                DeleteDirectorySafe(
                    targetFolder
                );

                // Restauration prioritaire depuis le dossier déplacé
                // sur le même disque.
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
                                "Le rollback a échoué."
                            );
                        }
                    }
                    catch (Exception rollbackException)
                    {
                        throw new InvalidOperationException(
                            "La mise à jour a échoué et le " +
                            "rollback de l'ancienne version " +
                            "a également échoué.\n\n" +
                            $"Erreur rollback : " +
                            $"{rollbackException.Message}"
                        );
                    }
                }
                else if (wasAlreadyInstalled &&
                         Directory.Exists(backupPath))
                {
                    // Fallback sur la copie de sauvegarde.
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
                                "Le rollback a échoué."
                            );
                        }
                    }
                    catch (Exception rollbackException)
                    {
                        throw new InvalidOperationException(
                            "La mise à jour a échoué et le " +
                            "rollback de l'ancienne version " +
                            "a également échoué.\n\n" +
                            $"Erreur rollback : " +
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

            // Sécurité supplémentaire :
            // si l'ancien dossier est encore présent à cet endroit,
            // on tente de le supprimer uniquement après succès.
            //
            // En cas d'échec de suppression, il reste sur le disque
            // plutôt que de risquer une perte de données.
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
                $"Le dossier de la {description} n'existe pas."
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
                $"Le fichier {addonFolder}.toc " +
                $"est absent de la {description}."
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
                $"Impossible de lire {addonFolder}.toc " +
                $"dans la {description}.",
                ex
            );
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                $"{addonFolder}.toc est vide."
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
                $"La {description} ne contient aucun fichier."
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
                "Le dossier du package est introuvable."
            );
        }

        if (!Directory.Exists(installedFolder))
        {
            throw new InvalidOperationException(
                "Le dossier d'installation est introuvable."
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
                "Le package ne contient aucun fichier."
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
                    "Fichier manquant après installation :\n" +
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
                    "Taille de fichier incorrecte après installation :\n" +
                    $"{relativePath}\n\n" +
                    $"Package : {packageInfo.Length} octets\n" +
                    $"Installé : {installedInfo.Length} octets"
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
                    "Le contenu d'un fichier ne correspond pas " +
                    "au package téléchargé :\n" +
                    $"{relativePath}\n\n" +
                    $"SHA-256 package : {packageHash}\n" +
                    $"SHA-256 installé : {installedHash}"
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
                $"Dossier source introuvable : {source}"
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
                $"Dossier source introuvable : {source}"
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