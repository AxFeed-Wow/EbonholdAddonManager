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

    public static string ReadVersion(string addonFolder)
    {
        if (!Directory.Exists(addonFolder))
            return "";

        string? tocFile = Directory
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