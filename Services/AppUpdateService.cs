using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace EbonholdAddonManager.Services;

public sealed class AppUpdateInfo
{
    public required Version Version { get; init; }
    public required string TagName { get; init; }
    public required string Changelog { get; init; }
    public required string ZipUrl { get; init; }
    public string? Sha256Url { get; init; }
}

public sealed class AppUpdateService
{
    private const string LatestReleaseUrl =
        "https://api.github.com/repos/AxFeed-Wow/EbonholdAddonManager/releases/latest";

    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version
        ?? new Version(0, 0, 0);

    public async Task<AppUpdateInfo?> CheckForUpdateAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient client = CreateClient();

            using HttpResponseMessage response =
                await client.GetAsync(
                    LatestReleaseUrl,
                    cancellationToken
                );

            if (!response.IsSuccessStatusCode)
                return null;

            string json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken
                );

            using JsonDocument document =
                JsonDocument.Parse(json);

            JsonElement root = document.RootElement;

            string tag =
                GetString(root, "tag_name");

            if (!TryParseVersion(tag, out Version? latest) ||
                latest is null ||
                latest <= CurrentVersion)
            {
                return null;
            }

            string zipUrl = "";
            string? shaUrl = null;

            if (root.TryGetProperty("assets", out JsonElement assets) &&
                assets.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement asset in assets.EnumerateArray())
                {
                    string name = GetString(asset, "name");
                    string url = GetString(asset, "browser_download_url");

                    if (string.IsNullOrEmpty(url))
                        continue;

                    if (name.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase))
                        shaUrl = url;
                    else if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        zipUrl = url;
                }
            }

            if (string.IsNullOrWhiteSpace(zipUrl))
                return null;

            return new AppUpdateInfo
            {
                Version = latest,
                TagName = tag,
                Changelog = GetString(root, "body"),
                ZipUrl = zipUrl,
                Sha256Url = shaUrl
            };
        }
        catch
        {
            return null;
        }
    }

    // Downloads the update zip, verifies it and extracts it.
    // Returns the folder that contains the new executable.
    public async Task<string> DownloadAsync(
        AppUpdateInfo info,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        string tempRoot =
            Path.Combine(
                Path.GetTempPath(),
                "EbonholdAddonManager",
                "update",
                Guid.NewGuid().ToString("N")
            );

        Directory.CreateDirectory(tempRoot);

        string zipPath = Path.Combine(tempRoot, "update.zip");
        string newDir = Path.Combine(tempRoot, "new");

        using HttpClient client = CreateClient();

        progress?.Report(
            LocalizationService.Get("update_downloading")
        );

        using (HttpResponseMessage response =
               await client.GetAsync(
                   info.ZipUrl,
                   HttpCompletionOption.ResponseHeadersRead,
                   cancellationToken))
        {
            response.EnsureSuccessStatusCode();

            await using Stream input =
                await response.Content.ReadAsStreamAsync(cancellationToken);

            await using FileStream output =
                File.Create(zipPath);

            await input.CopyToAsync(output, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(info.Sha256Url))
        {
            progress?.Report(
                LocalizationService.Get("update_verifying")
            );

            string sha =
                await client.GetStringAsync(
                    info.Sha256Url,
                    cancellationToken
                );

            string expected =
                sha.Split(
                        [' ', '\t', '\r', '\n'],
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    .FirstOrDefault()
                ?? "";

            string actual = ComputeSha256(zipPath);

            if (!string.IsNullOrWhiteSpace(expected) &&
                !string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The downloaded update failed SHA-256 verification."
                );
            }
        }

        ZipFile.ExtractToDirectory(zipPath, newDir);

        return FindExecutableDirectory(newDir);
    }

    private static HttpClient CreateClient()
    {
        HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(
                "EbonholdAddonManager",
                "1.0"
            )
        );

        return client;
    }

    private static string FindExecutableDirectory(string root)
    {
        string direct =
            Path.Combine(root, "EbonholdAddonManager.exe");

        if (File.Exists(direct))
            return root;

        string? found =
            Directory
                .EnumerateFiles(
                    root,
                    "EbonholdAddonManager.exe",
                    SearchOption.AllDirectories
                )
                .FirstOrDefault();

        return found != null
            ? Path.GetDirectoryName(found) ?? root
            : root;
    }

    private static string ComputeSha256(string path)
    {
        using FileStream stream = File.OpenRead(path);
        byte[] hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }

    private static string GetString(
        JsonElement element,
        string property)
    {
        return element.TryGetProperty(property, out JsonElement value) &&
               value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";
    }

    private static bool TryParseVersion(
        string tag,
        out Version? version)
    {
        version = null;

        if (string.IsNullOrWhiteSpace(tag))
            return false;

        string cleaned = tag.Trim().TrimStart('v', 'V');

        return Version.TryParse(cleaned, out version);
    }
}
