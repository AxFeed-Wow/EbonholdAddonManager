using System.Net.Http.Headers;
using System.Text.Json;
using EbonholdAddonManager.Models;

namespace EbonholdAddonManager.Services;

public sealed class CatalogService
{
    // The catalog is fetched from the repository so that approved
    // addons reach every user without shipping a new release.
    private const string RemoteCatalogUrl =
        "https://raw.githubusercontent.com/AxFeed-Wow/EbonholdAddonManager/master/addons.json";

    private static readonly JsonSerializerOptions ReadOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly string _cachePath;

    public CatalogService()
    {
        _cachePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                ),
                "EbonholdAddonManager",
                "catalog.cache.json"
            );
    }

    public async Task<List<AddonDefinition>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        // 1. Remote catalog (source of truth).
        List<AddonDefinition>? remote =
            await TryLoadRemoteAsync(cancellationToken);

        if (remote is { Count: > 0 })
        {
            await TrySaveCacheAsync(remote, cancellationToken);
            return remote;
        }

        // 2. Last catalog successfully fetched (offline fallback).
        List<AddonDefinition>? cached =
            await TryLoadFileAsync(_cachePath, cancellationToken);

        if (cached is { Count: > 0 })
            return cached;

        // 3. Catalog shipped with the application.
        string bundledPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "addons.json"
            );

        List<AddonDefinition>? bundled =
            await TryLoadFileAsync(bundledPath, cancellationToken);

        if (bundled is { Count: > 0 })
            return bundled;

        // 4. Minimal last-resort catalog.
        return GetDefaultCatalog();
    }

    private static async Task<List<AddonDefinition>?> TryLoadRemoteAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpClient client = new()
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(
                    "EbonholdAddonManager",
                    "1.0"
                )
            );

            string json =
                await client.GetStringAsync(
                    RemoteCatalogUrl,
                    cancellationToken
                );

            return Parse(json);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<List<AddonDefinition>?> TryLoadFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(path))
                return null;

            string json =
                await File.ReadAllTextAsync(
                    path,
                    cancellationToken
                );

            return Parse(json);
        }
        catch
        {
            return null;
        }
    }

    private async Task TrySaveCacheAsync(
        List<AddonDefinition> catalog,
        CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(_cachePath)!
            );

            string json =
                JsonSerializer.Serialize(
                    catalog,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

            await File.WriteAllTextAsync(
                _cachePath,
                json,
                cancellationToken
            );
        }
        catch
        {
        }
    }

    private static List<AddonDefinition>? Parse(
        string json)
    {
        return JsonSerializer.Deserialize<
            List<AddonDefinition>
        >(
            json,
            ReadOptions
        );
    }

    private static List<AddonDefinition> GetDefaultCatalog()
    {
        return
        [
            new AddonDefinition
            {
                Id = "ebon-affix-alert",
                Name = "Ebon Affix Alert",
                Folder = "EbonAffixAlert",
                Repository = "Kebbie/EbonAffixAlert",
                Branch = "main"
            }
        ];
    }
}
