using System.Text.Json;

namespace EbonholdAddonManager.Services;

public sealed class SettingsService
{
    private readonly string _settingsDirectory;
    private readonly string _settingsPath;

    public SettingsService()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ),
            "EbonholdAddonManager"
        );

        _settingsPath = Path.Combine(
            _settingsDirectory,
            "settings.json"
        );
    }

    public string? EbonholdPath { get; set; }

    public AppLanguage Language { get; set; } =
        AppLanguage.French;

    public async Task LoadAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsPath))
            return;

        try
        {
            string json =
                await File.ReadAllTextAsync(
                    _settingsPath,
                    cancellationToken
                );

            SettingsData? data =
                JsonSerializer.Deserialize<SettingsData>(
                    json
                );

            if (data == null)
                return;

            if (!string.IsNullOrWhiteSpace(
                    data.EbonholdPath) &&
                InstallationDetector.IsValidEbonholdFolder(
                    data.EbonholdPath
                ))
            {
                EbonholdPath =
                    data.EbonholdPath;
            }

            if (Enum.TryParse<AppLanguage>(
                    data.Language,
                    true,
                    out AppLanguage language))
            {
                Language = language;
            }

            LocalizationService.SetLanguage(
                Language
            );
        }
        catch
        {
        }
    }

    public async Task SaveAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(
                _settingsDirectory
            );

            SettingsData data = new()
            {
                EbonholdPath = EbonholdPath,
                Language = Language.ToString()
            };

            string json =
                JsonSerializer.Serialize(
                    data,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

            await File.WriteAllTextAsync(
                _settingsPath,
                json,
                cancellationToken
            );
        }
        catch
        {
        }
    }

    private sealed class SettingsData
    {
        public string? EbonholdPath { get; set; }

        public string Language { get; set; } =
            nameof(AppLanguage.French);
    }
}