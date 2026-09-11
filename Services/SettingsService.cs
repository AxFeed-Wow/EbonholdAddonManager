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

    public int? WindowWidth { get; set; }
    public int? WindowHeight { get; set; }
    public int? WindowX { get; set; }
    public int? WindowY { get; set; }
    public bool WindowMaximized { get; set; }

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

            WindowWidth = data.WindowWidth;
            WindowHeight = data.WindowHeight;
            WindowX = data.WindowX;
            WindowY = data.WindowY;
            WindowMaximized = data.WindowMaximized;

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

            await File.WriteAllTextAsync(
                _settingsPath,
                Serialize(),
                cancellationToken
            );
        }
        catch
        {
        }
    }

    // Synchronous save for the form-closing path.
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(
                _settingsDirectory
            );

            File.WriteAllText(
                _settingsPath,
                Serialize()
            );
        }
        catch
        {
        }
    }

    private string Serialize()
    {
        SettingsData data = new()
        {
            EbonholdPath = EbonholdPath,
            Language = Language.ToString(),
            WindowWidth = WindowWidth,
            WindowHeight = WindowHeight,
            WindowX = WindowX,
            WindowY = WindowY,
            WindowMaximized = WindowMaximized
        };

        return JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }
        );
    }

    private sealed class SettingsData
    {
        public string? EbonholdPath { get; set; }

        public string Language { get; set; } =
            nameof(AppLanguage.French);

        public int? WindowWidth { get; set; }
        public int? WindowHeight { get; set; }
        public int? WindowX { get; set; }
        public int? WindowY { get; set; }
        public bool WindowMaximized { get; set; }
    }
}
