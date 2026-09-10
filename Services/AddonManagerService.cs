using System.Diagnostics;
using EbonholdAddonManager.Models;

namespace EbonholdAddonManager.Services;

public sealed class AddonManagerService
{
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
        List<AddonInfo> results = [];

        foreach (AddonDefinition definition in catalog)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report(
                LocalizationService.Get("scanning_addon")
                    .Replace("{0}", definition.Name)
            );

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

            try
            {
                RepositoryMetadata metadata =
                    await _gitHubService.GetRepositoryMetadataAsync(
                        definition.Repository,
                        definition.Branch,
                        definition.Folder,
                        cancellationToken
                    );

                if (!string.IsNullOrWhiteSpace(
                        metadata.Author))
                {
                    definition.Author =
                        metadata.Author;
                }

                if (!string.IsNullOrWhiteSpace(
                        metadata.License))
                {
                    definition.License =
                        metadata.License;
                }

                if (!string.IsNullOrWhiteSpace(
                        metadata.Description))
                {
                    definition.Description =
                        metadata.Description;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[GitHub Metadata] {definition.Name}"
                );

                Debug.WriteLine(
                    ex.ToString()
                );
            }

            if (!Directory.Exists(localPath))
            {
                info.Status =
                    AddonStatus.NotInstalled;

                info.Message =
                    LocalizationService.Get(
                        "not_installed_message"
                    );

                results.Add(info);

                continue;
            }

            info.LocalVersion =
                TocReader.ReadVersion(
                    localPath
                );

            try
            {
                info.RemoteVersion =
                    await _gitHubService.GetRemoteVersionAsync(
                        definition.Repository,
                        definition.Branch,
                        definition.Folder,
                        cancellationToken
                    );

                if (string.IsNullOrWhiteSpace(
                        info.LocalVersion))
                {
                    info.Status =
                        AddonStatus.Unknown;

                    info.Message =
                        LocalizationService.Get(
                            "local_version_unknown"
                        );
                }
                else if (string.IsNullOrWhiteSpace(
                             info.RemoteVersion))
                {
                    info.Status =
                        AddonStatus.Unknown;

                    info.Message =
                        LocalizationService.Get(
                            "remote_version_unknown"
                        );
                }
                else if (VersionsEqual(
                             info.LocalVersion,
                             info.RemoteVersion))
                {
                    info.Status =
                        AddonStatus.UpToDate;

                    info.Message =
                        LocalizationService.Get(
                            "up_to_date_message"
                        );
                }
                else
                {
                    info.Status =
                        AddonStatus.UpdateAvailable;

                    info.Message =
                        LocalizationService.Get(
                            "update_available_message"
                        );
                }
            }
            catch (Exception ex)
            {
                info.Status =
                    AddonStatus.Error;

                info.Message =
                    $"{LocalizationService.Get("error_prefix")}{ex.GetBaseException().Message}";

                Debug.WriteLine(
                    "=================================================="
                );

                Debug.WriteLine(
                    $"[GitHub ERROR] {definition.Name}"
                );

                Debug.WriteLine(
                    $"Repository : {definition.Repository}"
                );

                Debug.WriteLine(
                    $"Branch : {definition.Branch}"
                );

                Debug.WriteLine(
                    ex.ToString()
                );
            }

            results.Add(info);
        }

        return results;
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