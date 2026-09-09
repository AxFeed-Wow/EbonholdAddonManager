using System;
using System.Windows.Forms;

namespace EbonholdAddonManager;

internal static class Program
{
    [STAThread]
    static void Main()
    {
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
                "ERREUR Ebonhold Addon Manager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}