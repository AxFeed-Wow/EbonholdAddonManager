namespace EbonholdAddonManager.Models;

public sealed class AddonDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Folder { get; set; } = "";
    public string Repository { get; set; } = "";
    public string Branch { get; set; } = "main";
    public string Description { get; set; } = "";
    // File name pattern of a GitHub release asset (e.g. "Addon-*.zip").
    // When set, installs and updates come from the newest matching
    // release instead of the branch source ZIP.
    public string ReleaseAsset { get; set; } = "";
    public bool PreferRelease { get; set; } = true;

    // Lets ReleaseAsset be taken from a GitHub prerelease.
    public bool IncludePrerelease { get; set; }

    // Other addon folders shipped in the same package and installed,
    // updated and uninstalled together with Folder.
    public List<string> ExtraFolders { get; set; } = [];

    // Folders the addon used under a previous name. An installation
    // found there is offered as an update; installing removes them.
    public List<string> LegacyFolders { get; set; } = [];

    public IEnumerable<string> AllFolders =>
        [Folder, .. ExtraFolders];

    // Optional dependency the user must have (e.g. a companion addon or an
    // external site/account). Shown on the card and confirmed on install.
    public string Requires { get; set; } = "";

    // These fields are populated automatically from the repository.
    public string Author { get; set; } = "";
    public string License { get; set; } = "";

    public string RepositoryUrl =>
        $"https://github.com/{Repository}";

    public bool RequiresIsLink =>
        Requires.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
        Requires.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
}