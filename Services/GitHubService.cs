using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace EbonholdAddonManager.Services;

public sealed class GitHubService
{
    private readonly HttpClient _httpClient;

    public GitHubService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(
                "EbonholdAddonManager",
                "1.0"
            )
        );
    }

    public async Task<RepositoryMetadata> GetRepositoryMetadataAsync(
        string repository,
        string branch,
        string folder,
        CancellationToken cancellationToken = default)
    {
        RepositoryMetadata metadata = new();

        // README
        string? readme =
            await TryGetRawFileAsync(
                repository,
                branch,
                "README.md",
                cancellationToken
            );

        if (string.IsNullOrWhiteSpace(readme))
        {
            readme =
                await TryGetRawFileAsync(
                    repository,
                    branch,
                    "Readme.md",
                    cancellationToken
                );
        }

        if (!string.IsNullOrWhiteSpace(readme))
        {
            metadata.Description =
                ExtractDescriptionFromReadme(readme);

            metadata.Author =
                ExtractAuthorFromReadme(readme);

            metadata.License =
                ExtractLicenseFromReadme(readme);
        }

        // TOC
        string tocFileName =
            $"{folder}.toc";

        string? toc =
            await TryGetRawFileAsync(
                repository,
                branch,
                tocFileName,
                cancellationToken
            );

        if (!string.IsNullOrWhiteSpace(toc))
        {
            string tocAuthor =
                ExtractTocField(
                    toc,
                    "Author"
                );

            if (!string.IsNullOrWhiteSpace(tocAuthor))
            {
                metadata.Author = tocAuthor;
            }

            string tocVersion =
                ExtractTocField(
                    toc,
                    "Version"
                );

            if (!string.IsNullOrWhiteSpace(tocVersion))
            {
                metadata.Version = tocVersion;
            }
        }

        // License files
        if (string.IsNullOrWhiteSpace(metadata.License))
        {
            string[] licenseFiles =
            [
                "LICENSE",
                "LICENSE.md",
                "LICENSE.txt",
                "license",
                "license.md",
                "license.txt"
            ];

            foreach (string licenseFile in licenseFiles)
            {
                string? licenseContent =
                    await TryGetRawFileAsync(
                        repository,
                        branch,
                        licenseFile,
                        cancellationToken
                    );

                if (string.IsNullOrWhiteSpace(
                        licenseContent))
                {
                    continue;
                }

                metadata.License =
                    DetectLicense(
                        licenseFile,
                        licenseContent
                    );

                if (!string.IsNullOrWhiteSpace(
                        metadata.License))
                {
                    break;
                }
            }
        }

        // GitHub repository owner is a useful fallback
        // when no author is explicitly declared.
        if (string.IsNullOrWhiteSpace(metadata.Author))
        {
            metadata.Author =
                ExtractRepositoryOwner(repository);
        }

        if (string.IsNullOrWhiteSpace(metadata.License))
        {
            metadata.License =
                "License not specified";
        }

        return metadata;
    }

    public async Task<string> GetRepositoryDescriptionAsync(
        string repository,
        CancellationToken cancellationToken = default)
    {
        string[] branches =
        [
            "main",
            "master"
        ];

        foreach (string branch in branches)
        {
            string? readme =
                await TryGetRawFileAsync(
                    repository,
                    branch,
                    "README.md",
                    cancellationToken
                );

            if (string.IsNullOrWhiteSpace(readme))
            {
                readme =
                    await TryGetRawFileAsync(
                        repository,
                        branch,
                        "Readme.md",
                        cancellationToken
                    );
            }

            if (string.IsNullOrWhiteSpace(readme))
                continue;

            string description =
                ExtractDescriptionFromReadme(
                    readme
                );

            if (!string.IsNullOrWhiteSpace(
                    description))
            {
                return description;
            }
        }

        return "";
    }

    public async Task<string> GetRemoteVersionAsync(
        string repository,
        string branch,
        string folder,
        CancellationToken cancellationToken = default)
    {
        string tocFileName =
            $"{folder}.toc";

        string? content =
            await TryGetRawFileAsync(
                repository,
                branch,
                tocFileName,
                cancellationToken
            );

        if (string.IsNullOrWhiteSpace(content))
            return "";

        return ExtractTocField(
            content,
            "Version"
        );
    }

    public async Task<string> DownloadRepositoryAsync(
        string repository,
        string branch,
        string destination,
        CancellationToken cancellationToken = default)
    {
        string url =
            $"https://github.com/{repository}/archive/refs/heads/{Uri.EscapeDataString(branch)}.zip";

        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

        response.EnsureSuccessStatusCode();

        await using Stream input =
            await response.Content.ReadAsStreamAsync(
                cancellationToken
            );

        await using FileStream output =
            File.Create(destination);

        await input.CopyToAsync(
            output,
            cancellationToken
        );

        return destination;
    }

    private async Task<string?> TryGetRawFileAsync(
        string repository,
        string branch,
        string file,
        CancellationToken cancellationToken)
    {
        try
        {
            string url =
                $"https://raw.githubusercontent.com/" +
                $"{repository}/" +
                $"{Uri.EscapeDataString(branch)}/" +
                $"{Uri.EscapeDataString(file)}";

            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    url,
                    cancellationToken
                );

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync(
                cancellationToken
            );
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractTocField(
        string content,
        string field)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "";

        Match match =
            Regex.Match(
                content,
                $@"^##\s*{Regex.Escape(field)}\s*:\s*(.+)$",
                RegexOptions.IgnoreCase |
                RegexOptions.Multiline
            );

        return match.Success
            ? match.Groups[1].Value.Trim()
            : "";
    }

    private static string ExtractDescriptionFromReadme(
        string readme)
    {
        if (string.IsNullOrWhiteSpace(readme))
            return "";

        string[] lines =
            readme.Split(
                [
                    "\r\n",
                    "\n",
                    "\r"
                ],
                StringSplitOptions.None
            );

        bool passedTitle = false;

        foreach (string rawLine in lines)
        {
            string line =
                rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("#"))
            {
                passedTitle = true;
                continue;
            }

            if (!passedTitle)
                continue;

            if (line.StartsWith("[![") ||
                line.StartsWith("<img") ||
                line.StartsWith("!["))
            {
                continue;
            }

            if (Regex.IsMatch(
                    line,
                    @"^[-_*]{3,}$"))
            {
                continue;
            }

            string description =
                Regex.Replace(
                    line,
                    @"\[(.*?)\]\(.*?\)",
                    "$1"
                );

            description =
                Regex.Replace(
                    description,
                    @"[*_`~]",
                    ""
                );

            description =
                Regex.Replace(
                    description,
                    @"\s+",
                    " "
                )
                .Trim();

            if (description.Length < 10)
                continue;

            return description;
        }

        return "";
    }

    private static string ExtractAuthorFromReadme(
        string readme)
    {
        if (string.IsNullOrWhiteSpace(readme))
            return "";

        string[] patterns =
        [
            @"(?im)^\s*(?:author|auteur)\s*:\s*(.+)$",
            @"(?im)^\s*(?:by|par)\s+(.+)$",
            @"(?im)^\s*created\s+by\s+(.+)$",
            @"(?im)^\s*maintained\s+by\s+(.+)$"
        ];

        foreach (string pattern in patterns)
        {
            Match match =
                Regex.Match(
                    readme,
                    pattern
                );

            if (!match.Success)
                continue;

            string value =
                StripMarkdown(
                    match.Groups[1].Value
                );

            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return "";
    }

    private static string ExtractLicenseFromReadme(
        string readme)
    {
        if (string.IsNullOrWhiteSpace(readme))
            return "";

        string[] licenses =
        [
            "MIT",
            "Apache License 2.0",
            "Apache-2.0",
            "GPL-3.0",
            "GPLv3",
            "GPL-2.0",
            "GPLv2",
            "AGPL-3.0",
            "AGPLv3",
            "LGPL-3.0",
            "BSD-3-Clause",
            "BSD-2-Clause",
            "MPL-2.0",
            "Unlicense"
        ];

        foreach (string license in licenses)
        {
            if (readme.Contains(
                    license,
                    StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeLicenseName(license);
            }
        }

        return "";
    }

    private static string DetectLicense(
        string filename,
        string content)
    {
        string lowerFile =
            filename.ToLowerInvariant();

        string firstPart =
            content.Length > 5000
                ? content[..5000]
                : content;

        if (firstPart.Contains(
                "permission is hereby granted, free of charge",
                StringComparison.OrdinalIgnoreCase))
        {
            return "MIT";
        }

        if (firstPart.Contains(
                "apache license",
                StringComparison.OrdinalIgnoreCase) &&
            firstPart.Contains(
                "version 2.0",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Apache-2.0";
        }

        if (firstPart.Contains(
                "gnu general public license",
                StringComparison.OrdinalIgnoreCase))
        {
            if (firstPart.Contains(
                    "version 3",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "GPL-3.0";
            }

            if (firstPart.Contains(
                    "version 2",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "GPL-2.0";
            }

            return "GPL";
        }

        if (firstPart.Contains(
                "gnu affero general public license",
                StringComparison.OrdinalIgnoreCase))
        {
            return "AGPL";
        }

        if (firstPart.Contains(
                "redistribution and use in source and binary forms",
                StringComparison.OrdinalIgnoreCase))
        {
            if (firstPart.Contains(
                    "3.0",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "BSD";
            }

            return "BSD";
        }

        if (firstPart.Contains(
                "mozilla public license",
                StringComparison.OrdinalIgnoreCase))
        {
            return "MPL-2.0";
        }

        if (lowerFile.Contains("unlicense") ||
            firstPart.Contains(
                "this is free and unencumbered software",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Unlicense";
        }

        return "License specified";
    }

    private static string ExtractRepositoryOwner(
        string repository)
    {
        string[] parts =
            repository.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries
            );

        return parts.Length > 0
            ? parts[0]
            : "";
    }

    private static string StripMarkdown(
        string value)
    {
        string result =
            Regex.Replace(
                value,
                @"\[(.*?)\]\(.*?\)",
                "$1"
            );

        result =
            Regex.Replace(
                result,
                @"[*_`~]",
                ""
            );

        return result.Trim();
    }

    private static string NormalizeLicenseName(
        string license)
    {
        return license switch
        {
            "Apache License 2.0" => "Apache-2.0",
            "GPLv3" => "GPL-3.0",
            "GPLv2" => "GPL-2.0",
            "AGPLv3" => "AGPL-3.0",
            _ => license
        };
    }
}

public sealed class RepositoryMetadata
{
    public string Author { get; set; } = "";
    public string License { get; set; } = "";
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
}