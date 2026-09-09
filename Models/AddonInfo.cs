namespace EbonholdAddonManager.Models;

public enum AddonStatus
{
    NotInstalled,
    UpToDate,
    UpdateAvailable,
    Unknown,
    Error
}

public sealed class AddonInfo
{
    public required AddonDefinition Definition { get; init; }

    public AddonStatus Status { get; set; }

    public string LocalVersion { get; set; } = "";
    public string RemoteVersion { get; set; } = "";
    public string LocalPath { get; set; } = "";

    public string LocalCommit { get; set; } = "";
    public string RemoteCommit { get; set; } = "";

    public string Message { get; set; } = "";

    public bool IsInstalled =>
        Status != AddonStatus.NotInstalled;

    public bool HasUpdate =>
        Status == AddonStatus.UpdateAvailable;

    public bool CanInstall =>
        Status == AddonStatus.NotInstalled ||
        Status == AddonStatus.UpdateAvailable;
}