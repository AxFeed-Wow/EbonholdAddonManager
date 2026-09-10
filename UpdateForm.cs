using System.Drawing;
using System.Windows.Forms;
using EbonholdAddonManager.Services;

namespace EbonholdAddonManager;

public sealed class UpdateForm : Form
{
    private readonly AppUpdateService _service;
    private readonly AppUpdateInfo _info;

    private Button _updateButton = null!;
    private Button _laterButton = null!;
    private Label _statusLabel = null!;

    public UpdateForm(
        AppUpdateService service,
        AppUpdateInfo info)
    {
        _service = service;
        _info = info;

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text =
            LocalizationService.Get("update_available_title");

        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(560, 460);
        Size = new Size(620, 520);
        BackColor = Color.FromArgb(18, 20, 26);
        Font = new Font("Segoe UI", 9F);
        ShowIcon = false;
        MaximizeBox = false;
        MinimizeBox = false;

        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.FromArgb(18, 20, 26)
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

        Controls.Add(root);

        Label title = new()
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
            ForeColor = Color.White,
            Text = LocalizationService.Get("update_available_intro"),
            TextAlign = ContentAlignment.MiddleLeft
        };

        root.Controls.Add(title, 0, 0);

        Label versions = new()
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = Color.FromArgb(120, 170, 255),
            Text = $"{AppUpdateService.CurrentVersion}  →  {_info.Version}",
            TextAlign = ContentAlignment.MiddleLeft
        };

        root.Controls.Add(versions, 0, 1);

        Panel changelogPanel = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 4, 0, 8),
            BackColor = Color.Transparent
        };

        TextBox changelog = new()
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            WordWrap = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(27, 30, 38),
            ForeColor = Color.FromArgb(210, 214, 222),
            Font = new Font("Segoe UI", 9F),
            Text = string.IsNullOrWhiteSpace(_info.Changelog)
                ? LocalizationService.Get("update_no_notes")
                : _info.Changelog.Replace("\r\n", "\n").Replace("\n", "\r\n")
        };

        changelogPanel.Controls.Add(changelog);
        root.Controls.Add(changelogPanel, 0, 2);

        Panel buttons = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };

        root.Controls.Add(buttons, 0, 3);

        _statusLabel = new Label
        {
            AutoSize = false,
            Location = new Point(0, 14),
            Size = new Size(240, 24),
            ForeColor = Color.FromArgb(150, 158, 175),
            Font = new Font("Segoe UI", 8.5F),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = ""
        };

        buttons.Controls.Add(_statusLabel);

        _updateButton =
            MakeButton(LocalizationService.Get("update_now"), 210);

        _laterButton =
            MakeButton(LocalizationService.Get("update_later"), 110);

        buttons.Controls.Add(_updateButton);
        buttons.Controls.Add(_laterButton);

        buttons.Resize += (_, _) => LayoutButtons(buttons);
        LayoutButtons(buttons);

        _laterButton.Click += (_, _) => Close();
        _updateButton.Click += UpdateButton_Click;
    }

    private void LayoutButtons(Panel panel)
    {
        _updateButton.Location =
            new Point(
                panel.ClientSize.Width - _updateButton.Width,
                10
            );

        _laterButton.Location =
            new Point(
                _updateButton.Left - _laterButton.Width - 8,
                10
            );

        _statusLabel.Width =
            Math.Max(120, _laterButton.Left - 12);
    }

    private async void UpdateButton_Click(
        object? sender,
        EventArgs e)
    {
        _updateButton.Enabled = false;
        _laterButton.Enabled = false;

        Progress<string> progress =
            new(message => _statusLabel.Text = message);

        try
        {
            string newDir =
                await _service.DownloadAsync(_info, progress);

            _statusLabel.Text =
                LocalizationService.Get("update_restarting");

            AppUpdater.LaunchAndExit(newDir);

            Application.Exit();
        }
        catch (Exception ex)
        {
            _statusLabel.Text =
                $"{LocalizationService.Get("error_prefix")}{ex.Message}";

            _updateButton.Enabled = true;
            _laterButton.Enabled = true;
        }
    }

    private static Button MakeButton(string text, int width)
    {
        Button button = new()
        {
            Text = text,
            Width = width,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(37, 42, 53),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 9F),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderColor =
            Color.FromArgb(65, 72, 88);

        return button;
    }
}
