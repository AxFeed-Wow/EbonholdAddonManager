using System.Text.Json;
using EbonholdAddonManager.Models;

namespace EbonholdAddonManager.Services;

public sealed class CatalogService
{
    public async Task<List<AddonDefinition>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        string path =
            Path.Combine(
                AppContext.BaseDirectory,
                "addons.json"
            );

        if (!File.Exists(path))
        {
            return GetDefaultCatalog();
        }

        try
        {
            string json =
                await File.ReadAllTextAsync(
                    path,
                    cancellationToken
                );

            return
                JsonSerializer.Deserialize<
                    List<AddonDefinition>
                >(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                )
                ?? GetDefaultCatalog();
        }
        catch
        {
            return GetDefaultCatalog();
        }
    }

    private static List<AddonDefinition>
        GetDefaultCatalog()
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