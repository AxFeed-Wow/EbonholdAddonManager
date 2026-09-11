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

        if (!Directory.Exists(localPath))
        {
            info.Status = AddonStatus.NotInstalled;
            info.Message =
                LocalizationService.Get("not_installed_message");

            return info;
        }

        info.LocalVersion = TocReader.ReadVersion(localPath);
        info.RemoteVersion = metadata?.Version ?? "";

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
        else if (VersionsEqual(info.LocalVersion, info.RemoteVersion))
        {
            info.Status = AddonStatus.UpToDate;
            info.Message =
                LocalizationService.Get("up_to_date_message");
        }
        else
        {
            info.Status = AddonStatus.UpdateAvailable;
            info.Message =
                LocalizationService.Get("update_available_message");
        }

        return info;
    }

    public async Task InstallOrUpdateAsync(
        string addonsFolder,
        AddonInfo addon,
        CancellationToken cancellationToken = default)
    {
        await _addonUpdater.UpdateAsync(
            addonsFolder,
            addon.Definition.Folder,
            addon.Definition.Repository,
            addon.Definition.Branch,
            cancellationToken
        );
    }

    public void Uninstall(
        string addonsFolder,
        AddonInfo addon)
    {
        string path =
            Path.Combine(
                addonsFolder,
                addon.Definition.Folder
            );

        if (Directory.Exists(path))
        {
            Directory.Delete(
                path,
                true
            );
        }
    }

    private static bool VersionsEqual(
        string a,
        string b)
    {
        return NormalizeVersion(a)
            .Equals(
                NormalizeVersion(b),
                StringComparison.OrdinalIgnoreCase
            );
    }

    private static string NormalizeVersion(
        string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return "";

        return version
            .Trim()
            .TrimStart('v', 'V')
            .Replace(" ", "");
    }
}
