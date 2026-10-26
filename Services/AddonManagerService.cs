using System.Diagnostics;
using EbonholdAddonManager.Models;

namespace EbonholdAddonManager.Services;

public sealed class AddonManagerService
{
    // Limits how many addons are queried from GitHub at once.
    private const int MaxScanConcurrency = 8;

    private readonly GitHubService _gitHubService;
    private readonly AddonUpdater _addonUpdater;

    public AddonManagerService()
    {
        _gitHubService = new GitHubService();

        _addonUpdater = new AddonUpdater(
            _gitHubService
        );
    }

    public async Task<List<AddonInfo>> ScanAsync(
        string addonsFolder,
        IReadOnlyList<AddonDefinition> catalog,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        AddonInfo[] results = new AddonInfo[catalog.Count];

        using SemaphoreSlim gate = new(MaxScanConcurrency);
        int completed = 0;

        async Task ProcessAsync(int index)
        {
            await gate.WaitAsync(cancellationToken);

            try
            {
                results[index] =
                    await ScanAddonAsync(
                        addonsFolder,
                        catalog[index],
                        cancellationToken
                    );
            }
            finally
            {
                gate.Release();

                int done = Interlocked.Increment(ref completed);

                progress?.Report(
                    LocalizationService.Get("scanning_progress")
                        .Replace("{0}", done.ToString())
                        .Replace("{1}", catalog.Count.ToString())
                );
            }
        }

        List<Task> tasks = [];

        for (int i = 0; i < catalog.Count; i++)
        {
            tasks.Add(ProcessAsync(i));
        }

        await Task.WhenAll(tasks);

        return [.. results];
    }

    private async Task<AddonInfo> ScanAddonAsync(
        string addonsFolder,
        AddonDefinition definition,
        CancellationToken cancellationToken)
    {
        string localPath =
            Path.Combine(
                addonsFolder,
                definition.Folder
            );

        AddonInfo info = new()
        {
            Definition = definition,
            LocalPath = localPath
        };

        // A single metadata call already returns the author, license,
        // description and the remote .toc version, so the version is
        // reused instead of fetching the .toc a second time.
        RepositoryMetadata? metadata = null;

        try
        {
            metadata =
                await _gitHubService.GetRepositoryMetadataAsync(
                    definition.Repository,
                    definition.Branch,
                    definition.Folder,
                    cancellationToken
                );

            if (!string.IsNullOrWhiteSpace(metadata.Author))
                definition.Author = metadata.Author;

            if (!string.IsNullOrWhiteSpace(metadata.License))
                definition.License = metadata.License;

            if (!string.IsNullOrWhiteSpace(metadata.Description))
                definition.Description = metadata.Description;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GitHub Metadata] {definition.Name}");
            Debug.WriteLine(ex.ToString());
        }

        GitHubRelease? release = null;

        if (!string.IsNullOrWhiteSpace(definition.ReleaseAsset))
        {
            try
            {
                release =
                    await _gitHubService.GetLatestReleaseAsync(
                        definition.Repository,
                        definition.ReleaseAsset,
                        definition.IncludePrerelease,
                        cancellationToken
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GitHub Release] {definition.Name}");
                Debug.WriteLine(ex.ToString());
            }
        }

        // Addons installed from a release are compared with the
        // release tag, never with the branch .toc.
        info.RemoteVersion =
            string.IsNullOrWhiteSpace(definition.ReleaseAsset)
                ? metadata?.Version ?? ""
                : release?.Tag ?? "";

        string? legacyPath =
            definition.LegacyFolders
                .Select(folder => Path.Combine(addonsFolder, folder))
                .FirstOrDefault(Directory.Exists);

        // Installed under a previous folder name: the update
        // installs the new folder and removes the old one.
        if (legacyPath != null)
        {
            info.LocalVersion =
                TocReader.ReadVersion(
                    Directory.Exists(localPath)
                        ? localPath
                        : legacyPath
                );

            info.Status = AddonStatus.UpdateAvailable;
            info.Message =
                LocalizationService.Get("update_available_message");

            return info;
        }

        if (!Directory.Exists(localPath))
        {
            info.Status = AddonStatus.NotInstalled;
            info.Message =
                LocalizationService.Get("not_installed_message");

            return info;
        }

        info.LocalVersion =
            TocReader.ReadVersion(
                localPath,
                definition.Folder
            );

        bool packageIncomplete =
            definition.ExtraFolders.Any(folder =>
                !Directory.Exists(
                    Path.Combine(
                        addonsFolder,
                        folder
                    )
                )
            );

        // The .toc has no version: the installed commit, recorded at
        // install time, is compared with the branch instead.
        if (string.IsNullOrWhiteSpace(definition.ReleaseAsset) &&
            metadata is { HasToc: true } &&
            string.IsNullOrWhiteSpace(metadata.Version))
        {
            try
            {
                info.RemoteCommit =
                    await _gitHubService.GetBranchCommitAsync(
                        definition.Repository,
                        definition.Branch,
                        cancellationToken
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GitHub Commit] {definition.Name}");
                Debug.WriteLine(ex.ToString());
            }

            info.LocalCommit =
                ReadCommitMarker(localPath);

            info.RemoteVersion =
                ShortCommit(info.RemoteCommit);

            if (string.IsNullOrWhiteSpace(info.LocalVersion))
            {
                info.LocalVersion =
                    ShortCommit(info.LocalCommit);
            }

            if (string.IsNullOrWhiteSpace(info.RemoteCommit))
            {
                info.Status = AddonStatus.Unknown;
                info.Message =
                    LocalizationService.Get("remote_version_unknown");
            }
            else if (packageIncomplete ||
                     !string.Equals(
                         info.LocalCommit,
                         info.RemoteCommit,
                         StringComparison.OrdinalIgnoreCase))
            {
                info.Status = AddonStatus.UpdateAvailable;
                info.Message =
                    LocalizationService.Get("update_available_message");
            }
            else
            {
                info.Status = AddonStatus.UpToDate;
                info.Message =
                    LocalizationService.Get("up_to_date_message");
            }

            return info;
        }

        if (string.IsNullOrWhiteSpace(info.LocalVersion))
        {
            info.Status = AddonStatus.Unknown;
            info.Message =
                LocalizationService.Get("local_version_unknown");
        }
        else if (string.IsNullOrWhiteSpace(info.RemoteVersion))
        {
            info.Status = AddonStatus.Unknown;
            info.Message =
                LocalizationService.Get("remote_version_unknown");
        }
        // Only a strictly newer remote is an update, so an older
        // remote never replaces a newer installation.
        else if (packageIncomplete ||
                 VersionComparer.Compare(
                     info.LocalVersion,
                     info.RemoteVersion) < 0)
        {
            info.Status = AddonStatus.UpdateAvailable;
            info.Message =
                LocalizationService.Get("update_available_message");
        }
        else
        {
            info.Status = AddonStatus.UpToDate;
            info.Message =
                LocalizationService.Get("up_to_date_message");
        }

        return info;
    }

    // Runs on the thread pool: the file copies and SHA-256 checks
    // are synchronous and would otherwise freeze the window.
    public Task InstallOrUpdateAsync(
        string addonsFolder,
        AddonInfo addon,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(
            () =>
                _addonUpdater.UpdateAsync(
                    addonsFolder,
                    addon.Definition,
                    cancellationToken
                ),
            cancellationToken
        );
    }

    // The catalog addon named by addon.Definition.Requires (by name,
    // id or folder), or null when it names something else, such as
    // a website.
    public static AddonInfo? FindDependency(
        AddonInfo addon,
        IEnumerable<AddonInfo> addons)
    {
        string requires =
            addon.Definition.Requires.Trim();

        if (string.IsNullOrWhiteSpace(requires))
            return null;

        return addons.FirstOrDefault(other =>
            other != addon &&
            (string.Equals(other.Definition.Name, requires, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(other.Definition.Id, requires, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(other.Definition.Folder, requires, StringComparison.OrdinalIgnoreCase)));
    }

    private static string ReadCommitMarker(
        string addonPath)
    {
        try
        {
            return File.ReadAllText(
                    Path.Combine(
                        addonPath,
                        AddonUpdater.CommitMarkerFile
                    )
                )
                .Trim();
        }
        catch
        {
            return "";
        }
    }

    private static string ShortCommit(
        string commit)
    {
        return commit.Length > 7
            ? commit[..7]
            : commit;
    }

    public void Uninstall(
        string addonsFolder,
        AddonInfo addon)
    {
        foreach (string folder in
                 addon.Definition.AllFolders.Concat(
                     addon.Definition.LegacyFolders))
        {
            string path =
                Path.Combine(
                    addonsFolder,
                    folder
                );

            if (Directory.Exists(path))
            {
                Directory.Delete(
                    path,
                    true
                );
            }
        }
    }
}
