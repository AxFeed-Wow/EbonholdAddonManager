using System;
using System.Windows.Forms;
using EbonholdAddonManager.Services;

namespace EbonholdAddonManager;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // When relaunched as the updater, apply the staged update
        // instead of starting the user interface.
        if (args.Length > 0 &&
            args[0] == AppUpdater.ApplyUpdateArg)
        {
            AppUpdater.RunUpdaterMode(args);
            return;
        }

        try
        {
            ApplicationConfiguration.Initialize();

            Application.Run(
                new MainForm()
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "Ebonhold Addon Manager - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
