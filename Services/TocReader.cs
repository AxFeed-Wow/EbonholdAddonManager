using System.Text;
using System.Text.RegularExpressions;

namespace EbonholdAddonManager.Services;

public static class TocReader
{
    private static readonly Regex VersionRegex = new(
        @"^##\s*Version\s*:\s*(.+)$",
        RegexOptions.IgnoreCase |
        RegexOptions.Multiline
    );

    // Reads <tocName>.toc (the folder name by default), falling back
    // to the first .toc of the folder.
    public static string ReadVersion(
        string addonFolder,
        string? tocName = null)
    {
        if (!Directory.Exists(addonFolder))
            return "";

        string namedToc =
            Path.Combine(
                addonFolder,
                $"{tocName ?? Path.GetFileName(addonFolder)}.toc"
            );

        string? tocFile = File.Exists(namedToc)
            ? namedToc
            : Directory
                .EnumerateFiles(
                    addonFolder,
                    "*.toc",
                    SearchOption.TopDirectoryOnly
                )
                .FirstOrDefault();

        if (tocFile == null)
            return "";

        try
        {
            string content = File.ReadAllText(
                tocFile,
                Encoding.UTF8
            );

            Match match =
                VersionRegex.Match(content);

            if (!match.Success)
                return "";

            return match.Groups[1].Value.Trim();
        }
        catch
        {
            return "";
        }
    }
}