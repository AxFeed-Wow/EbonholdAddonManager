using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using EbonholdAddonManager.Models;
using EbonholdAddonManager.Services;

namespace EbonholdAddonManager;

public sealed class MainForm : Form
{
    private const string SubmitAddonUrl =
        "https://github.com/AxFeed-Wow/EbonholdAddonManager/issues/new?template=addon_submission.yml";

    private readonly CatalogService _catalogService;
    private readonly SettingsService _settingsService;
    private readonly AddonManagerService _addonManagerService;
    private readonly AppUpdateService _appUpdateService;

    private string? _ebonholdPath;

    private List<AddonDefinition> _catalog = [];
    private List<AddonInfo> _addons = [];

    private bool _loading;
    private bool _updating;
    private bool _suppressLanguageChange;

    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Label _installationTitleLabel = null!;
    private Label _pathLabel = null!;
    private Label _connectionLabel = null!;
    private Label _summaryLabel = null!;
    private Label _statusLabel = null!;
    private Label _languageLabel = null!;
    private Label _warningLabel = null!;
    private LinkLabel _versionLink = null!;

    private Button _browseButton = null!;
    private Button _refreshButton = null!;
    private Button _creditsButton = null!;
    private Button _updateAllButton = null!;
    private Button _proposeButton = null!;

    private ComboBox _languageComboBox = null!;

    private TextBox _searchBox = null!;
    private string _filter = "";

    private FlowLayoutPanel _addonsPanel = null!;

    public MainForm()
    {
        _catalogService =
            new CatalogService();

        _settingsService =
            new SettingsService();

        _addonManagerService =
            new AddonManagerService();

        _appUpdateService =
            new AppUpdateService();

        InitializeComponent();

        FormClosing += MainForm_FormClosing;

        Shown += async (_, _) =>
        {
            await InitializeAsync();
        };
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Text =
            LocalizationService.Get(
                "app.title"
            );

        StartPosition =
            FormStartPosition.CenterScreen;

        MinimumSize =
            new Size(
                900,
                650
            );

        Size =
            new Size(
                1200,
                800
            );

        BackColor =
            Color.FromArgb(
                18,
                20,
                26
            );

        Font =
            new Font(
                "Segoe UI",
                9F
            );

        BuildInterface();

        ResumeLayout(
            false
        );

        PerformLayout();
    }

    private async Task InitializeAsync()
    {
        if (_loading)
            return;

        _loading = true;

        try
        {
            SetStatus(
                LocalizationService.Get(
                    "searching"
                )
            );

            await _settingsService.LoadAsync();

            ApplyWindowSettings();

            LocalizationService.SetLanguage(
                _settingsService.Language
            );

            ApplyLocalization();

            UpdateLanguageSelector();

            string? detectedPath =
                _settingsService.EbonholdPath;

            if (string.IsNullOrWhiteSpace(
                    detectedPath))
            {
                detectedPath =
                    InstallationDetector.FindEbonholdFolder();
            }

            if (!string.IsNullOrWhiteSpace(
                    detectedPath))
            {
                _ebonholdPath =
                    detectedPath;

                _settingsService.EbonholdPath =
                    detectedPath;

                await _settingsService.SaveAsync();

                UpdatePathDisplay();

                SetConnectionState(
                    true
                );
            }
            else
            {
                SetConnectionState(
                    false
                );

                SetStatus(
                    LocalizationService.Get(
                        "not_found"
                    )
                );
            }

            _catalog =
                await _catalogService.LoadAsync();

            ApplyLocalization();

            if (!string.IsNullOrWhiteSpace(
                    _ebonholdPath))
            {
                await RefreshAddonsAsync();
            }

            await CheckForAppUpdateAsync();
        }
        catch (Exception ex)
        {
            SetStatus(
                $"{LocalizationService.Get("error_prefix")}{ex.Message}"
            );

            MessageBox.Show(
                ex.ToString(),
                LocalizationService.Get(
                    "error_title"
                ),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task CheckForAppUpdateAsync(
        bool manual = false)
    {
        try
        {
            AppUpdateInfo? update =
                await _appUpdateService.CheckForUpdateAsync();

            if (update == null)
            {
                if (manual)
                {
                    MessageBox.Show(
                        LocalizationService.Get(
                            "update_up_to_date"
                        ),
                        LocalizationService.Get(
                            "app.title"
                        ),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                return;
            }

            using UpdateForm form =
                new(_appUpdateService, update);

            form.ShowDialog(this);
        }
        catch
        {
        }
    }

    private void ApplyWindowSettings()
    {
        if (_settingsService.WindowWidth is int w &&
            _settingsService.WindowHeight is int h &&
            w >= MinimumSize.Width &&
            h >= MinimumSize.Height)
        {
            StartPosition = FormStartPosition.Manual;
            Size = new Size(w, h);

            if (_settingsService.WindowX is int x &&
                _settingsService.WindowY is int y &&
                IsOnScreen(new Rectangle(x, y, w, h)))
            {
                Location = new Point(x, y);
            }
            else
            {
                Rectangle area =
                    Screen.PrimaryScreen!.WorkingArea;

                Location = new Point(
                    area.X + (area.Width - w) / 2,
                    area.Y + (area.Height - h) / 2
                );
            }
        }

        if (_settingsService.WindowMaximized)
            WindowState = FormWindowState.Maximized;
    }

    private static bool IsOnScreen(Rectangle bounds)
    {
        foreach (Screen screen in Screen.AllScreens)
        {
            if (screen.WorkingArea.IntersectsWith(bounds))
                return true;
        }

        return false;
    }

    private void MainForm_FormClosing(
        object? sender,
        FormClosingEventArgs e)
    {
        try
        {
            _settingsService.WindowMaximized =
                WindowState == FormWindowState.Maximized;

            Rectangle bounds =
                WindowState == FormWindowState.Normal
                    ? Bounds
                    : RestoreBounds;

            _settingsService.WindowWidth = bounds.Width;
            _settingsService.WindowHeight = bounds.Height;
            _settingsService.WindowX = bounds.X;
            _settingsService.WindowY = bounds.Y;

            _settingsService.Save();
        }
        catch
        {
        }
    }

    private static string GetVersionLinkText()
    {
        return $"v{AppUpdateService.CurrentVersion.ToString(3)} — " +
               LocalizationService.Get("update_check_now");
    }

    private void BuildInterface()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            BackColor =
                Color.FromArgb(
                    18,
                    20,
                    26
                ),
            Padding =
                new Padding(
                    24
                ),
            ColumnCount = 1,
            RowCount = 7
        };

        root.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                100F
            )
        );

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                100F
            )
        );

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                82F
            )
        );

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                60F
            )
        );

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                36F
            )
        );

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                56F
            )
        );

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                100F
            )
        );

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                28F
            )
        );

        Controls.Add(root);

        // -------------------------------------------------
        // HEADER
        // -------------------------------------------------

        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        header.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                100F
            )
        );

        header.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                155F
            )
        );

        root.Controls.Add(
            header,
            0,
            0
        );

        Panel titlePanel = new()
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        header.Controls.Add(
            titlePanel,
            0,
            0
        );

        _titleLabel = new Label
        {
            AutoSize = true,
            Location =
                new Point(
                    0,
                    0
                ),
            Font =
                new Font(
                    "Segoe UI Semibold",
                    23F,
                    FontStyle.Bold
                ),
            ForeColor = Color.White,
            Text =
                LocalizationService.Get(
                    "app.title"
                )
        };

        titlePanel.Controls.Add(
            _titleLabel
        );

        _subtitleLabel = new Label
        {
            AutoSize = true,
            Location =
                new Point(
                    2,
                    43
                ),
            Font =
                new Font(
                    "Segoe UI",
                    10F
                ),
            ForeColor =
                Color.FromArgb(
                    150,
                    158,
                    175
                ),
            Text =
                LocalizationService.Get(
                    "app.subtitle"
                )
        };

        titlePanel.Controls.Add(
            _subtitleLabel
        );

        TableLayoutPanel languagePanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        languagePanel.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                24F
            )
        );

        languagePanel.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                38F
            )
        );

        header.Controls.Add(
            languagePanel,
            1,
            0
        );

        _languageLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign =
                ContentAlignment.MiddleRight,
            Font =
                new Font(
                    "Segoe UI Semibold",
                    8.5F,
                    FontStyle.Bold
                ),
            ForeColor =
                Color.FromArgb(
                    150,
                    158,
                    175
                ),
            Text =
                LocalizationService.Get(
                    "language"
                )
        };

        languagePanel.Controls.Add(
            _languageLabel,
            0,
            0
        );

        _languageComboBox = new ComboBox
        {
            DropDownStyle =
                ComboBoxStyle.DropDownList,
            Width = 130,
            Height = 32,
            Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right,
            FlatStyle =
                FlatStyle.Flat,
            BackColor =
                Color.FromArgb(
                    35,
                    39,
                    49
                ),
            ForeColor = Color.White
        };

        _languageComboBox.Items.Add(
            "🇬🇧 English"
        );

        _languageComboBox.Items.Add(
            "🇫🇷 Français"
        );

        _languageComboBox.SelectedIndexChanged +=
            LanguageComboBox_SelectedIndexChanged;

        languagePanel.Controls.Add(
            _languageComboBox,
            0,
            1
        );

        // -------------------------------------------------
        // INSTALLATION
        // -------------------------------------------------

        Panel installationPanel = new()
        {
            Dock = DockStyle.Fill,
            BackColor =
                Color.FromArgb(
                    27,
                    30,
                    38
                ),
            Padding =
                new Padding(
                    16
                ),
            Margin = Padding.Empty
        };

        root.Controls.Add(
            installationPanel,
            0,
            1
        );

        _installationTitleLabel = new Label
        {
            AutoSize = true,
            Location =
                new Point(
                    16,
                    8
                ),
            Font =
                new Font(
                    "Segoe UI Semibold",
                    8.5F,
                    FontStyle.Bold
                ),
            ForeColor =
                Color.FromArgb(
                    150,
                    158,
                    175
                ),
            Text =
                LocalizationService.Get(
                    "installation"
                )
        };

        installationPanel.Controls.Add(
            _installationTitleLabel
        );

        _pathLabel = new Label
        {
            AutoEllipsis = true,
            Location =
                new Point(
                    16,
                    34
                ),
            Size =
                new Size(
                    600,
                    25
                ),
            Font =
                new Font(
                    "Segoe UI",
                    9.5F
                ),
            ForeColor = Color.White,
            Text = "—"
        };

        installationPanel.Controls.Add(
            _pathLabel
        );

        _browseButton =
            CreateButton(
                LocalizationService.Get(
                    "change_folder"
                ),
                125
            );

        _browseButton.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Right;

        _browseButton.Click +=
            BrowseButton_Click;

        installationPanel.Controls.Add(
            _browseButton
        );

        installationPanel.Resize +=
            (_, _) =>
            {
                _browseButton.Location =
                    new Point(
                        installationPanel.ClientSize.Width -
                        _browseButton.Width -
                        16,
                        26
                    );

                _pathLabel.Width =
                    Math.Max(
                        200,
                        _browseButton.Left -
                        _pathLabel.Left -
                        16
                    );
            };

        // -------------------------------------------------
        // TOOLBAR
        // -------------------------------------------------

        Panel toolbar = new()
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding =
                new Padding(
                    0,
                    10,
                    0,
                    8
                )
        };

        root.Controls.Add(
            toolbar,
            0,
            2
        );

        _connectionLabel = new Label
        {
            AutoSize = true,
            Location =
                new Point(
                    0,
                    17
                ),
            Font =
                new Font(
                    "Segoe UI",
                    9F
                ),
            ForeColor =
                Color.FromArgb(
                    120,
                    170,
                    255
                ),
            Text =
                LocalizationService.Get(
                    "searching"
                )
        };

        toolbar.Controls.Add(
            _connectionLabel
        );

        _updateAllButton =
            CreateButton(
                LocalizationService.Get(
                    "update_all"
                ),
                145
            );

        _updateAllButton.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Right;

        _updateAllButton.Enabled = false;

        _updateAllButton.Click +=
            UpdateAllButton_Click;

        toolbar.Controls.Add(
            _updateAllButton
        );

        _creditsButton =
            CreateButton(
                LocalizationService.Get(
                    "credits"
                ),
                95
            );

        _creditsButton.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Right;

        _creditsButton.Click +=
            (_, _) =>
            {
                using CreditsForm form =
                    new(_catalog);

                form.ShowDialog(this);
            };

        toolbar.Controls.Add(
            _creditsButton
        );

        _refreshButton =
            CreateButton(
                LocalizationService.Get(
                    "refresh"
                ),
                105
            );

        _refreshButton.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Right;

        _refreshButton.Click +=
            async (_, _) =>
            {
                await RefreshAddonsAsync();
            };

        toolbar.Controls.Add(
            _refreshButton
        );

        _proposeButton =
            CreateButton(
                LocalizationService.Get(
                    "propose_addon"
                ),
                150
            );

        _proposeButton.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Right;

        _proposeButton.Click +=
            (_, _) =>
            {
                OpenUrl(SubmitAddonUrl);
            };

        toolbar.Controls.Add(
            _proposeButton
        );

        toolbar.Resize +=
            (_, _) =>
            {
                _updateAllButton.Location =
                    new Point(
                        toolbar.ClientSize.Width -
                        _updateAllButton.Width,
                        8
                    );

                _creditsButton.Location =
                    new Point(
                        _updateAllButton.Left -
                        _creditsButton.Width -
                        8,
                        8
                    );

                _refreshButton.Location =
                    new Point(
                        _creditsButton.Left -
                        _refreshButton.Width -
                        8,
                        8
                    );

                _proposeButton.Location =
                    new Point(
                        _refreshButton.Left -
                        _proposeButton.Width -
                        8,
                        8
                    );
            };

        // -------------------------------------------------
        // SUMMARY
        // -------------------------------------------------

        TableLayoutPanel summaryRow = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        summaryRow.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                280F
            )
        );

        summaryRow.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                100F
            )
        );

        _searchBox = new TextBox
        {
            Width = 260,
            Anchor = AnchorStyles.Left,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor =
                Color.FromArgb(
                    35,
                    39,
                    49
                ),
            ForeColor = Color.White,
            Font =
                new Font(
                    "Segoe UI",
                    9.5F
                ),
            PlaceholderText =
                LocalizationService.Get(
                    "search_placeholder"
                )
        };

        _searchBox.TextChanged +=
            (_, _) =>
            {
                _filter = _searchBox.Text;
                DisplayAddons();
            };

        summaryRow.Controls.Add(
            _searchBox,
            0,
            0
        );

        _summaryLabel = new Label
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Font =
                new Font(
                    "Segoe UI Semibold",
                    9F
                ),
            ForeColor =
                Color.FromArgb(
                    180,
                    185,
                    198
                ),
            TextAlign =
                ContentAlignment.MiddleLeft,
            Text = ""
        };

        summaryRow.Controls.Add(
            _summaryLabel,
            1,
            0
        );

        root.Controls.Add(
            summaryRow,
            0,
            3
        );

        // -------------------------------------------------
        // WARNING
        // -------------------------------------------------

        Panel warningPanel = new()
        {
            Dock = DockStyle.Fill,
            Margin =
                new Padding(
                    0,
                    4,
                    0,
                    4
                ),
            Padding =
                new Padding(
                    12,
                    4,
                    12,
                    4
                ),
            BackColor =
                Color.FromArgb(
                    38,
                    32,
                    20
                )
        };

        _warningLabel = new Label
        {
            Dock = DockStyle.Fill,
            Font =
                new Font(
                    "Segoe UI",
                    8.5F
                ),
            ForeColor =
                Color.FromArgb(
                    255,
                    190,
                    80
                ),
            TextAlign =
                ContentAlignment.MiddleLeft,
            Text =
                LocalizationService.Get(
                    "warning_notice"
                )
        };

        warningPanel.Controls.Add(
            _warningLabel
        );

        root.Controls.Add(
            warningPanel,
            0,
            4
        );

        // -------------------------------------------------
        // ADDONS
        // -------------------------------------------------

        _addonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            AutoSize = false,
            FlowDirection =
                FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding =
                new Padding(
                    0,
                    6,
                    20,
                    30
                ),
            Margin = Padding.Empty
        };

        root.Controls.Add(
            _addonsPanel,
            0,
            5
        );

        _addonsPanel.Resize +=
            (_, _) =>
            {
                ResizeAddonCards();
            };

        // -------------------------------------------------
        // STATUS
        // -------------------------------------------------

        TableLayoutPanel statusRow = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        statusRow.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                100F
            )
        );

        statusRow.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.AutoSize
            )
        );

        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            TextAlign =
                ContentAlignment.MiddleLeft,
            Font =
                new Font(
                    "Segoe UI",
                    8.5F
                ),
            ForeColor =
                Color.FromArgb(
                    120,
                    128,
                    145
                ),
            Text = ""
        };

        statusRow.Controls.Add(
            _statusLabel,
            0,
            0
        );

        _versionLink = new LinkLabel
        {
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Margin =
                new Padding(
                    8,
                    0,
                    0,
                    0
                ),
            Font =
                new Font(
                    "Segoe UI",
                    8.5F
                ),
            LinkColor =
                Color.FromArgb(
                    120,
                    128,
                    145
                ),
            ActiveLinkColor = Color.White,
            Text = GetVersionLinkText()
        };

        _versionLink.LinkClicked +=
            async (_, _) =>
            {
                await CheckForAppUpdateAsync(true);
            };

        statusRow.Controls.Add(
            _versionLink,
            1,
            0
        );

        root.Controls.Add(
            statusRow,
            0,
            6
        );
    }

    private static Button CreateButton(
        string text,
        int width)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = 34,
            FlatStyle =
                FlatStyle.Flat,
            BackColor =
                Color.FromArgb(
                    37,
                    42,
                    53
                ),
            ForeColor = Color.White,
            Font =
                new Font(
                    "Segoe UI Semibold",
                    9F
                ),
            FlatAppearance =
            {
                BorderColor =
                    Color.FromArgb(
                        65,
                        72,
                        88
                    ),
                BorderSize = 1
            },
            Cursor = Cursors.Hand
        };
    }

    private void ApplyLocalization()
    {
        Text =
            LocalizationService.Get(
                "app.title"
            );

        _titleLabel.Text =
            LocalizationService.Get(
                "app.title"
            );

        _subtitleLabel.Text =
            LocalizationService.Get(
                "app.subtitle"
            );

        _languageLabel.Text =
            LocalizationService.Get(
                "language"
            );

        _installationTitleLabel.Text =
            LocalizationService.Get(
                "installation"
            );

        _browseButton.Text =
            LocalizationService.Get(
                "change_folder"
            );

        _refreshButton.Text =
            LocalizationService.Get(
                "refresh"
            );

        _creditsButton.Text =
            LocalizationService.Get(
                "credits"
            );

        _proposeButton.Text =
            LocalizationService.Get(
                "propose_addon"
            );

        if (_searchBox != null)
        {
            _searchBox.PlaceholderText =
                LocalizationService.Get(
                    "search_placeholder"
                );
        }

        if (_versionLink != null)
        {
            _versionLink.Text = GetVersionLinkText();
        }

        _updateAllButton.Text =
            LocalizationService.Get(
                "update_all"
            );

        _warningLabel.Text =
            LocalizationService.Get(
                "warning_notice"
            );

        UpdateConnectionLabel();

        UpdateSummary();

        if (_addons.Count > 0)
        {
            DisplayAddons();
        }
    }

    private void UpdateLanguageSelector()
    {
        if (_languageComboBox == null)
            return;

        _suppressLanguageChange = true;

        try
        {
            _languageComboBox.SelectedIndex =
                LocalizationService.CurrentLanguage ==
                AppLanguage.English
                    ? 0
                    : 1;
        }
        finally
        {
            _suppressLanguageChange = false;
        }
    }

    private async void LanguageComboBox_SelectedIndexChanged(
        object? sender,
        EventArgs e)
    {
        if (_suppressLanguageChange)
            return;

        AppLanguage language =
            _languageComboBox.SelectedIndex == 1
                ? AppLanguage.French
                : AppLanguage.English;

        LocalizationService.SetLanguage(
            language
        );

        _settingsService.Language =
            language;

        await _settingsService.SaveAsync();

        ApplyLocalization();

        UpdateLanguageSelector();
    }

    private void UpdatePathDisplay()
    {
        _pathLabel.Text =
            string.IsNullOrWhiteSpace(
                _ebonholdPath)
                ? "—"
                : _ebonholdPath;
    }

    private void SetConnectionState(
        bool connected)
    {
        if (connected)
        {
            _connectionLabel.Text =
                "● " +
                LocalizationService.Get(
                    "ready"
                );

            _connectionLabel.ForeColor =
                Color.FromArgb(
                    90,
                    210,
                    130
                );
        }
        else
        {
            _connectionLabel.Text =
                "● " +
                LocalizationService.Get(
                    "not_found"
                );

            _connectionLabel.ForeColor =
                Color.FromArgb(
                    230,
                    100,
                    100
                );
        }
    }

    private void UpdateConnectionLabel()
    {
        SetConnectionState(
            !string.IsNullOrWhiteSpace(
                _ebonholdPath
            )
        );
    }

    private async void BrowseButton_Click(
        object? sender,
        EventArgs e)
    {
        using FolderBrowserDialog dialog =
            new()
            {
                Description =
                    LocalizationService.Get(
                        "select_ebonhold"
                    ),

                UseDescriptionForTitle = true,

                ShowNewFolderButton = false
            };

        if (!string.IsNullOrWhiteSpace(
                _ebonholdPath) &&
            Directory.Exists(
                _ebonholdPath))
        {
            dialog.SelectedPath =
                _ebonholdPath;
        }

        if (dialog.ShowDialog(this) !=
            DialogResult.OK)
        {
            return;
        }

        string? resolved =
            InstallationDetector.ResolveEbonholdFolder(
                dialog.SelectedPath
            );

        if (resolved == null)
        {
            MessageBox.Show(
                LocalizationService.Get(
                    "invalid_ebonhold"
                ),
                LocalizationService.Get(
                    "error_title"
                ),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return;
        }

        _ebonholdPath =
            resolved;

        _settingsService.EbonholdPath =
            resolved;

        await _settingsService.SaveAsync();

        UpdatePathDisplay();

        SetConnectionState(
            true
        );

        await RefreshAddonsAsync();
    }

    private async Task RefreshAddonsAsync()
    {
        // Refresh is blocked only while an actual
        // install/update operation is in progress.
        //
        // Startup scanning is NOT blocked by _loading.
        // Post-update scanning is performed after
        // _updating has been reset.

        if (_updating)
            return;

        if (string.IsNullOrWhiteSpace(
                _ebonholdPath))
        {
            return;
        }

        string? addonsFolder =
            InstallationDetector.ResolveAddonsFolder(
                _ebonholdPath
            );

        if (addonsFolder == null)
        {
            SetStatus(
                LocalizationService.Get(
                    "addons_folder_not_found"
                )
            );

            return;
        }

        try
        {
            _refreshButton.Enabled = false;
            _browseButton.Enabled = false;
            _creditsButton.Enabled = false;

            SetStatus(
                LocalizationService.Get(
                    "scanning"
                )
            );

            Progress<string> progress =
                new(
                    message =>
                    {
                        SetStatus(message);
                    }
                );

            _addons =
                await _addonManagerService.ScanAsync(
                    addonsFolder,
                    _catalog,
                    progress
                );

            DisplayAddons();

            UpdateSummary();

            SetStatus(
                LocalizationService.Get(
                    "scan_complete"
                )
            );
        }
        catch (OperationCanceledException)
        {
            SetStatus(
                LocalizationService.Get(
                    "cancelled"
                )
            );
        }
        catch (Exception ex)
        {
            SetStatus(
                $"{LocalizationService.Get("error_prefix")}{ex.Message}"
            );
        }
        finally
        {
            _refreshButton.Enabled = true;
            _browseButton.Enabled = true;
            _creditsButton.Enabled = true;
        }
    }

    private void DisplayAddons()
    {
        _addonsPanel.SuspendLayout();

        try
        {
            _addonsPanel.Controls.Clear();

            IEnumerable<AddonInfo> visible = _addons;

            if (!string.IsNullOrWhiteSpace(_filter))
            {
                string term = _filter.Trim();

                visible = _addons.Where(
                    a =>
                        a.Definition.Name.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
            }

            foreach (AddonInfo addon in visible)
            {
                Panel card =
                    CreateAddonCard(
                        addon
                    );

                _addonsPanel.Controls.Add(
                    card
                );
            }

            ResizeAddonCards();

            _addonsPanel.AutoScrollPosition =
                new Point(
                    0,
                    0
                );
        }
        finally
        {
            _addonsPanel.ResumeLayout(
                true
            );
        }

        if (_addonsPanel.Controls.Count > 0)
        {
            _addonsPanel.ScrollControlIntoView(
                _addonsPanel.Controls[0]
            );
        }
    }

    private void ResizeAddonCards()
    {
        if (_addonsPanel == null)
            return;

        int availableWidth =
            _addonsPanel.ClientSize.Width -
            _addonsPanel.Padding.Left -
            _addonsPanel.Padding.Right;

        if (_addonsPanel.VerticalScroll.Visible)
        {
            availableWidth -=
                SystemInformation.VerticalScrollBarWidth;
        }

        availableWidth -= 4;

        if (availableWidth < 500)
        {
            availableWidth = 500;
        }

        foreach (Control control
                 in _addonsPanel.Controls)
        {
            control.Width =
                availableWidth;
        }
    }

    private Panel CreateAddonCard(
        AddonInfo addon)
    {
        const int cardHeight = 185;

        Panel card = new()
        {
            Height = cardHeight,

            Margin =
                new Padding(
                    0,
                    0,
                    0,
                    10
                ),

            Padding =
                new Padding(
                    18
                ),

            BackColor =
                Color.FromArgb(
                    27,
                    30,
                    38
                )
        };

        Label nameLabel = new()
        {
            AutoSize = true,

            Location =
                new Point(
                    18,
                    14
                ),

            Font =
                new Font(
                    "Segoe UI Semibold",
                    12F,
                    FontStyle.Bold
                ),

            ForeColor =
                Color.White,

            Text =
                addon.Definition.Name
        };

        card.Controls.Add(
            nameLabel
        );

        Label statusLabel = new()
        {
            AutoSize = true,

            Location =
                new Point(
                    18,
                    43
                ),

            Font =
                new Font(
                    "Segoe UI Semibold",
                    8.5F,
                    FontStyle.Bold
                ),

            ForeColor =
                GetStatusColor(
                    addon.Status
                ),

            Text =
                GetStatusText(
                    addon
                )
        };

        card.Controls.Add(
            statusLabel
        );

        Label descriptionLabel = new()
        {
            AutoEllipsis = true,

            Location =
                new Point(
                    18,
                    68
                ),

            Height = 38,

            Font =
                new Font(
                    "Segoe UI",
                    8.5F
                ),

            ForeColor =
                Color.FromArgb(
                    150,
                    158,
                    175
                ),

            Text =
                string.IsNullOrWhiteSpace(
                    addon.Definition.Description)
                    ? LocalizationService.Get(
                        "description_unavailable"
                    )
                    : addon.Definition.Description
        };

        descriptionLabel.Width =
            Math.Max(
                300,
                card.ClientSize.Width -
                36
            );

        card.Controls.Add(
            descriptionLabel
        );

        if (!string.IsNullOrWhiteSpace(
                addon.Definition.Requires))
        {
            string requiresText =
                LocalizationService.Get("requires_label")
                    .Replace(
                        "{0}",
                        addon.Definition.Requires
                    );

            if (addon.Definition.RequiresIsLink)
            {
                LinkLabel requiresLink = new()
                {
                    AutoSize = true,
                    Location =
                        new Point(
                            18,
                            108
                        ),
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F,
                            FontStyle.Bold
                        ),
                    LinkColor =
                        Color.FromArgb(
                            255,
                            190,
                            80
                        ),
                    ActiveLinkColor = Color.White,
                    Text = requiresText
                };

                requiresLink.LinkClicked +=
                    (_, _) =>
                    {
                        OpenUrl(
                            addon.Definition.Requires
                        );
                    };

                card.Controls.Add(
                    requiresLink
                );
            }
            else
            {
                Label requiresLabel = new()
                {
                    AutoSize = true,
                    Location =
                        new Point(
                            18,
                            108
                        ),
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F,
                            FontStyle.Bold
                        ),
                    ForeColor =
                        Color.FromArgb(
                            255,
                            190,
                            80
                        ),
                    Text = requiresText
                };

                card.Controls.Add(
                    requiresLabel
                );
            }
        }

        Button githubButton =
            CreateButton(
                LocalizationService.Get(
                    "github"
                ),
                85
            );

        githubButton.Anchor =
            AnchorStyles.Bottom |
            AnchorStyles.Left;

        githubButton.Location =
            new Point(
                18,
                cardHeight -
                githubButton.Height -
                18
            );

        githubButton.Click +=
            (_, _) =>
            {
                OpenUrl(
                    addon.Definition.RepositoryUrl
                );
            };

        card.Controls.Add(
            githubButton
        );

        Button actionButton =
            CreateButton(
                GetActionText(
                    addon
                ),
                110
            );

        actionButton.Anchor =
            AnchorStyles.Bottom |
            AnchorStyles.Right;

        actionButton.Location =
            new Point(
                card.ClientSize.Width -
                actionButton.Width -
                18,

                card.ClientSize.Height -
                actionButton.Height -
                18
            );

        actionButton.Enabled =
            addon.CanInstall &&
            !_updating;

        actionButton.Click +=
            async (_, _) =>
            {
                await InstallAddonAsync(
                    addon
                );
            };

        card.Controls.Add(
            actionButton
        );

        Button? uninstallButton = null;

        if (addon.IsInstalled)
        {
            uninstallButton =
                CreateButton(
                    LocalizationService.Get(
                        "uninstall"
                    ),
                    110
                );

            uninstallButton.Anchor =
                AnchorStyles.Bottom |
                AnchorStyles.Right;

            uninstallButton.ForeColor =
                Color.FromArgb(
                    226,
                    120,
                    120
                );

            uninstallButton.Enabled = !_updating;

            uninstallButton.Location =
                new Point(
                    actionButton.Left -
                    uninstallButton.Width -
                    8,

                    card.ClientSize.Height -
                    uninstallButton.Height -
                    18
                );

            uninstallButton.Click +=
                async (_, _) =>
                {
                    await UninstallAddonAsync(
                        addon
                    );
                };

            card.Controls.Add(
                uninstallButton
            );
        }

        card.Resize +=
            (_, _) =>
            {
                githubButton.Location =
                    new Point(
                        18,
                        card.ClientSize.Height -
                        githubButton.Height -
                        18
                    );

                actionButton.Location =
                    new Point(
                        card.ClientSize.Width -
                        actionButton.Width -
                        18,

                        card.ClientSize.Height -
                        actionButton.Height -
                        18
                    );

                if (uninstallButton != null)
                {
                    uninstallButton.Location =
                        new Point(
                            actionButton.Left -
                            uninstallButton.Width -
                            8,

                            card.ClientSize.Height -
                            uninstallButton.Height -
                            18
                        );
                }

                descriptionLabel.Width =
                    Math.Max(
                        300,
                        card.ClientSize.Width -
                        36
                    );
            };

        return card;
    }

    private static Color GetStatusColor(
        AddonStatus status)
    {
        return status switch
        {
            AddonStatus.UpToDate =>
                Color.FromArgb(
                    90,
                    210,
                    130
                ),

            AddonStatus.UpdateAvailable =>
                Color.FromArgb(
                    255,
                    190,
                    80
                ),

            AddonStatus.NotInstalled =>
                Color.FromArgb(
                    150,
                    158,
                    175
                ),

            AddonStatus.Error =>
                Color.FromArgb(
                    230,
                    100,
                    100
                ),

            _ =>
                Color.FromArgb(
                    170,
                    175,
                    190
                )
        };
    }

    private static string GetStatusText(
        AddonInfo addon)
    {
        return addon.Status switch
        {
            AddonStatus.UpToDate =>
                "● " +
                LocalizationService.Get(
                    "up_to_date"
                ) +
                GetVersionSuffix(
                    addon
                ),

            AddonStatus.UpdateAvailable =>
                "● " +
                LocalizationService.Get(
                    "update_available"
                ) +
                GetVersionSuffix(
                    addon
                ),

            AddonStatus.NotInstalled =>
                "● " +
                LocalizationService.Get(
                    "not_installed"
                ),

            AddonStatus.Unknown =>
                "● " +
                LocalizationService.Get(
                    "unknown"
                ),

            AddonStatus.Error =>
                "● " +
                LocalizationService.Get(
                    "error"
                ),

            _ =>
                "● " +
                LocalizationService.Get(
                    "unknown"
                )
        };
    }

    private static string GetVersionSuffix(
        AddonInfo addon)
    {
        if (string.IsNullOrWhiteSpace(
                addon.LocalVersion))
        {
            return "";
        }

        if (string.IsNullOrWhiteSpace(
                addon.RemoteVersion))
        {
            return
                $"  •  {LocalizationService.Get("version")} {addon.LocalVersion}";
        }

        return
            $"  •  {addon.LocalVersion} → {addon.RemoteVersion}";
    }

    private static string GetActionText(
        AddonInfo addon)
    {
        return addon.Status ==
               AddonStatus.NotInstalled
            ? LocalizationService.Get(
                "install"
            )
            : LocalizationService.Get(
                "update"
            );
    }

    private void UpdateSummary()
    {
        if (_summaryLabel == null)
            return;

        int installed =
            _addons.Count(
                x =>
                    x.Status !=
                    AddonStatus.NotInstalled
            );

        int updates =
            _addons.Count(
                x =>
                    x.Status ==
                    AddonStatus.UpdateAvailable
            );

        int errors =
            _addons.Count(
                x =>
                    x.Status ==
                    AddonStatus.Error
            );

        string addonsText =
            $"{_addons.Count} " +
            LocalizationService.Get(
                "addons"
            );

        string installedText =
            $"{installed} " +
            LocalizationService.Get(
                "installed"
            );

        string updateText =
            $"{updates} " +
            LocalizationService.Get(
                "update_available_short"
            );

        string errorText =
            $"{errors} " +
            LocalizationService.Get(
                "errors"
            );

        _summaryLabel.Text =
            $"{addonsText}   •   " +
            $"{installedText}   •   " +
            $"{updateText}   •   " +
            $"{errorText}";

        _updateAllButton.Enabled =
            updates > 0 &&
            !_updating;
    }

    private bool EnsureCanWrite(
        string addonsFolder)
    {
        if (!AdminService.NeedsAdministrator(
                addonsFolder))
        {
            return true;
        }

        DialogResult result =
            MessageBox.Show(
                LocalizationService.Get(
                    "elevation_required"
                ),
                LocalizationService.Get(
                    "elevation_title"
                ),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

        if (result == DialogResult.Yes)
        {
            if (AdminService.TryRestartAsAdministrator([]))
            {
                Application.Exit();
            }
            else
            {
                MessageBox.Show(
                    LocalizationService.Get(
                        "elevation_failed"
                    ),
                    LocalizationService.Get(
                        "error_title"
                    ),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        return false;
    }

    private async Task UninstallAddonAsync(
        AddonInfo addon)
    {
        if (_updating)
            return;

        if (string.IsNullOrWhiteSpace(
                _ebonholdPath))
        {
            return;
        }

        string? addonsFolder =
            InstallationDetector.ResolveAddonsFolder(
                _ebonholdPath
            );

        if (addonsFolder == null)
        {
            MessageBox.Show(
                LocalizationService.Get(
                    "addons_folder_not_found"
                ),
                LocalizationService.Get(
                    "error_title"
                ),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            return;
        }

        DialogResult confirm =
            MessageBox.Show(
                LocalizationService.Get(
                    "uninstall_confirm"
                ).Replace(
                    "{0}",
                    addon.Definition.Name
                ),
                LocalizationService.Get(
                    "uninstall_confirm_title"
                ),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

        if (confirm != DialogResult.Yes)
            return;

        if (!EnsureCanWrite(addonsFolder))
            return;

        _updating = true;

        _refreshButton.Enabled = false;
        _browseButton.Enabled = false;
        _creditsButton.Enabled = false;
        _updateAllButton.Enabled = false;

        try
        {
            SetStatus(
                $"{LocalizationService.Get("processing")} {addon.Definition.Name}..."
            );

            await Task.Run(
                () =>
                    _addonManagerService.Uninstall(
                        addonsFolder,
                        addon
                    )
            );

            SetStatus(
                $"{addon.Definition.Name} — " +
                LocalizationService.Get(
                    "uninstall_complete"
                )
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                LocalizationService.Get(
                    "error_title"
                ),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            SetStatus(
                $"{LocalizationService.Get("error_prefix")}{ex.Message}"
            );

            return;
        }
        finally
        {
            _updating = false;

            _refreshButton.Enabled = true;
            _browseButton.Enabled = true;
            _creditsButton.Enabled = true;
        }

        await RefreshAddonsAsync();

        UpdateSummary();
    }

    private async Task InstallAddonAsync(
        AddonInfo addon)
    {
        if (_updating)
            return;

        if (addon.Status == AddonStatus.NotInstalled &&
            !string.IsNullOrWhiteSpace(
                addon.Definition.Requires))
        {
            DialogResult dependency =
                MessageBox.Show(
                    LocalizationService.Get("requires_confirm")
                        .Replace("{0}", addon.Definition.Name)
                        .Replace("{1}", addon.Definition.Requires),
                    LocalizationService.Get(
                        "requires_confirm_title"
                    ),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (dependency != DialogResult.Yes)
                return;
        }

        if (string.IsNullOrWhiteSpace(
                _ebonholdPath))
        {
            return;
        }

        string? addonsFolder =
            InstallationDetector.ResolveAddonsFolder(
                _ebonholdPath
            );

        if (addonsFolder == null)
        {
            MessageBox.Show(
                LocalizationService.Get(
                    "addons_folder_not_found"
                ),
                LocalizationService.Get(
                    "error_title"
                ),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            return;
        }

        if (!EnsureCanWrite(addonsFolder))
            return;

        _updating = true;

        _refreshButton.Enabled = false;
        _browseButton.Enabled = false;
        _creditsButton.Enabled = false;
        _updateAllButton.Enabled = false;

        try
        {
            SetStatus(
                $"{LocalizationService.Get("processing")} {addon.Definition.Name}..."
            );

            await _addonManagerService.InstallOrUpdateAsync(
                addonsFolder,
                addon
            );

            SetStatus(
                $"{addon.Definition.Name} — " +
                LocalizationService.Get(
                    "status_complete"
                )
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                LocalizationService.Get(
                    "error_title"
                ),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            SetStatus(
                $"{LocalizationService.Get("error_prefix")}{ex.Message}"
            );

            return;
        }
        finally
        {
            _updating = false;

            _refreshButton.Enabled = true;
            _browseButton.Enabled = true;
            _creditsButton.Enabled = true;
        }

        // IMPORTANT:
        // _updating is false here, so RefreshAddonsAsync()
        // actually performs the scan.
        await RefreshAddonsAsync();

        UpdateSummary();
    }

    private async void UpdateAllButton_Click(
        object? sender,
        EventArgs e)
    {
        if (_updating)
            return;

        List<AddonInfo> updates =
            _addons
                .Where(
                    x =>
                        x.Status ==
                        AddonStatus.UpdateAvailable
                )
                .ToList();

        if (updates.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(
                _ebonholdPath))
        {
            return;
        }

        string? addonsFolder =
            InstallationDetector.ResolveAddonsFolder(
                _ebonholdPath
            );

        if (addonsFolder == null)
            return;

        if (!EnsureCanWrite(addonsFolder))
            return;

        _updating = true;

        _updateAllButton.Enabled = false;
        _refreshButton.Enabled = false;
        _browseButton.Enabled = false;
        _creditsButton.Enabled = false;

        try
        {
            foreach (AddonInfo addon in updates)
            {
                SetStatus(
                    $"{LocalizationService.Get("processing")} {addon.Definition.Name}..."
                );

                await _addonManagerService.InstallOrUpdateAsync(
                    addonsFolder,
                    addon
                );
            }

            SetStatus(
                LocalizationService.Get(
                    "all_updates_complete"
                )
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                LocalizationService.Get(
                    "error_title"
                ),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            SetStatus(
                $"{LocalizationService.Get("error_prefix")}{ex.Message}"
            );
        }
        finally
        {
            _updating = false;

            _refreshButton.Enabled = true;
            _browseButton.Enabled = true;
            _creditsButton.Enabled = true;
        }

        // Only one scan after the whole batch.
        await RefreshAddonsAsync();

        UpdateSummary();
    }

    private void SetStatus(
        string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(
                new Action(
                    () =>
                        SetStatus(
                            message
                        )
                )
            );

            return;
        }

        _statusLabel.Text =
            message;
    }

    private static void OpenUrl(
        string url)
    {
        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                }
            );
        }
        catch
        {
        }
    }
}