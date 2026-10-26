using System.Text.RegularExpressions;

namespace EbonholdAddonManager.Services;

// Orders addon versions so an older remote never replaces a newer
// installation. Loosely follows SemVer: 1.20.0 > 1.20.0-beta.2 >
// 1.20.0-beta.1 > 1.19.4. Build info (a "+..." suffix or a trailing
// commit hash such as "-b9c9332") is ignored and whitespace counts
// as "-", so the .toc version "1.20.0-beta.1 test.9095-b9c9332"
// equals the release tag "v1.20.0-beta.1-test.9095".
public static class VersionComparer
{
    public static int Compare(
        string a,
        string b)
    {
        (string[] coreA, string[] preA) = Split(a);
        (string[] coreB, string[] preB) = Split(b);

        for (int i = 0; i < Math.Max(coreA.Length, coreB.Length); i++)
        {
            int result =
                CompareIdentifiers(
                    i < coreA.Length ? coreA[i] : "0",
                    i < coreB.Length ? coreB[i] : "0"
                );

            if (result != 0)
                return result;
        }

        // A release ranks above any of its prereleases.
        if (preA.Length == 0 || preB.Length == 0)
            return preB.Length.CompareTo(preA.Length);

        for (int i = 0; i < Math.Min(preA.Length, preB.Length); i++)
        {
            int result =
                CompareIdentifiers(
                    preA[i],
                    preB[i]
                );

            if (result != 0)
                return result;
        }

        return preA.Length.CompareTo(preB.Length);
    }

    private static (string[] Core, string[] Pre) Split(
        string version)
    {
        string value =
            Regex.Replace(
                version.Trim().TrimStart('v', 'V'),
                @"\s+",
                "-"
            );

        value = Regex.Replace(value, @"\+.*$", "");

        value =
            Regex.Replace(
                value,
                @"-(?=[0-9a-f]*[a-f])[0-9a-f]{7,40}$",
                "",
                RegexOptions.IgnoreCase
            );

        int dash = value.IndexOf('-');

        string core = dash < 0 ? value : value[..dash];
        string pre = dash < 0 ? "" : value[(dash + 1)..];

        return (
            core.Split('.', StringSplitOptions.RemoveEmptyEntries),
            pre.Split(['.', '-'], StringSplitOptions.RemoveEmptyEntries)
        );
    }

    private static int CompareIdentifiers(
        string a,
        string b)
    {
        bool aIsNumber = long.TryParse(a, out long aNumber);
        bool bIsNumber = long.TryParse(b, out long bNumber);

        if (aIsNumber && bIsNumber)
            return aNumber.CompareTo(bNumber);

        // SemVer: numeric identifiers rank below alphanumeric ones.
        if (aIsNumber != bIsNumber)
            return aIsNumber ? -1 : 1;

        return string.Compare(
            a,
            b,
            StringComparison.OrdinalIgnoreCase
        );
    }
}
