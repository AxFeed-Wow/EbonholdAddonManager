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

    // These fields are populated automatically from the repository.
    public string Author { get; set; } = "";
    public string License { get; set; } = "";

    public string RepositoryUrl =>
        $"https://github.com/{Repository}";
}