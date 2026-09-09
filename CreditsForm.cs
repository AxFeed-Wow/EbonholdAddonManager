using System.Drawing;
using System.Windows.Forms;
using EbonholdAddonManager.Models;
using EbonholdAddonManager.Services;

namespace EbonholdAddonManager;

public sealed class CreditsForm : Form
{
    private readonly IReadOnlyList<AddonDefinition> _addons;

    public CreditsForm(
        IReadOnlyList<AddonDefinition> addons)
    {
        _addons = addons;

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text =
            LocalizationService.Get(
                "credits_title"
            );

        StartPosition =
            FormStartPosition.CenterParent;

        MinimumSize =
            new Size(
                650,
                500
            );

        Size =
            new Size(
                760,
                650
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

        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.FromArgb(18, 20, 26)
        };

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                45F
            )
        );

        root.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                65F
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
                45F
            )
        );

        Controls.Add(root);

        Label title = new()
        {
            Dock = DockStyle.Fill,
            Font =
                new Font(
                    "Segoe UI Semibold",
                    18F,
                    FontStyle.Bold
                ),
            ForeColor = Color.White,
            Text =
                LocalizationService.Get(
                    "credits_title"
                ),
            TextAlign =
                ContentAlignment.MiddleLeft
        };

        root.Controls.Add(
            title,
            0,
            0
        );

        Label disclaimer = new()
        {
            Dock = DockStyle.Fill,
            Font =
                new Font(
                    "Segoe UI",
                    9F
                ),
            ForeColor =
                Color.FromArgb(
                    155,
                    162,
                    178
                ),
            Text =
                LocalizationService.Get(
                    "credits_disclaimer"
                ),
            TextAlign =
                ContentAlignment.MiddleLeft
        };

        root.Controls.Add(
            disclaimer,
            0,
            1
        );

        FlowLayoutPanel addonPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection =
                FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor =
                Color.FromArgb(
                    24,
                    27,
                    34
                ),
            Padding =
                new Padding(
                    12
                )
        };

        root.Controls.Add(
            addonPanel,
            0,
            2
        );

        foreach (AddonDefinition addon in _addons)
        {
            Panel card =
                CreateAddonCard(
                    addon,
                    addonPanel.ClientSize.Width - 30
                );

            addonPanel.Controls.Add(card);
        }

        addonPanel.Resize +=
            (_, _) =>
            {
                int width =
                    addonPanel.ClientSize.Width -
                    addonPanel.Padding.Left -
                    addonPanel.Padding.Right -
                    20;

                foreach (Control control
                         in addonPanel.Controls)
                {
                    control.Width =
                        Math.Max(
                            400,
                            width
                        );
                }
            };

        Button closeButton =
            new()
            {
                Text =
                    LocalizationService.Get(
                        "close"
                    ),
                Width = 100,
                Height = 32,
                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Right,
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
                Cursor = Cursors.Hand
            };

        closeButton.FlatAppearance.BorderColor =
            Color.FromArgb(
                65,
                72,
                88
            );

        closeButton.Click +=
            (_, _) =>
            {
                Close();
            };

        Panel buttonPanel = new()
        {
            Dock = DockStyle.Fill
        };

        buttonPanel.Controls.Add(
            closeButton
        );

        buttonPanel.Resize +=
            (_, _) =>
            {
                closeButton.Location =
                    new Point(
                        buttonPanel.ClientSize.Width -
                        closeButton.Width,
                        5
                    );
            };

        root.Controls.Add(
            buttonPanel,
            0,
            3
        );
    }

    private static Panel CreateAddonCard(
        AddonDefinition addon,
        int width)
    {
        Panel card = new()
        {
            Width =
                Math.Max(
                    400,
                    width
                ),
            Height = 82,
            Margin =
                new Padding(
                    0,
                    0,
                    0,
                    8
                ),
            Padding =
                new Padding(
                    12
                ),
            BackColor =
                Color.FromArgb(
                    31,
                    35,
                    44
                )
        };

        Label name = new()
        {
            AutoSize = true,
            Location =
                new Point(
                    12,
                    10
                ),
            Font =
                new Font(
                    "Segoe UI Semibold",
                    10F,
                    FontStyle.Bold
                ),
            ForeColor = Color.White,
            Text = addon.Name
        };

        card.Controls.Add(name);

        string author =
            string.IsNullOrWhiteSpace(
                addon.Author)
                ? LocalizationService.Get(
                    "unknown_author"
                )
                : addon.Author;

        string license =
            string.IsNullOrWhiteSpace(
                addon.License)
                ? LocalizationService.Get(
                    "license_not_specified"
                )
                : addon.License;

        Label info = new()
        {
            AutoSize = true,
            Location =
                new Point(
                    12,
                    38
                ),
            Font =
                new Font(
                    "Segoe UI",
                    8.5F
                ),
            ForeColor =
                Color.FromArgb(
                    160,
                    168,
                    182
                ),
            Text =
                $"{LocalizationService.Get("by")} {author}   •   {license}"
        };

        card.Controls.Add(info);

        Button github = new()
        {
            Text =
                LocalizationService.Get(
                    "github"
                ),
            Width = 75,
            Height = 28,
            Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right,
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
                    8F
                ),
            Cursor = Cursors.Hand
        };

        github.FlatAppearance.BorderColor =
            Color.FromArgb(
                65,
                72,
                88
            );

        card.Controls.Add(github);

        github.Click +=
            (_, _) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName =
                                addon.RepositoryUrl,
                            UseShellExecute = true
                        }
                    );
                }
                catch
                {
                }
            };

        card.Resize +=
            (_, _) =>
            {
                github.Location =
                    new Point(
                        card.ClientSize.Width -
                        github.Width -
                        12,
                        27
                    );
            };

        github.Location =
            new Point(
                card.ClientSize.Width -
                github.Width -
                12,
                27
            );

        return card;
    }
}