namespace EbonholdAddonManager.Models;

public sealed class AddonDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Folder { get; set; } = "";
    public string Repository { get; set; } = "";
    public string Branch { get; set; } = "main";
    public string Description { get; set; } = "";
    public string ReleaseAsset { get; set; } = "";
    public string ReleaseTag { get; set; } = "";
    public bool PreferRelease { get; set; } = true;

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