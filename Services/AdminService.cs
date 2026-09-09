using System.Diagnostics;
using System.Security.Principal;

namespace EbonholdAddonManager.Services;

public static class AdminService
{
    public static bool IsRunningAsAdministrator()
    {
        using WindowsIdentity identity =
            WindowsIdentity.GetCurrent();

        WindowsPrincipal principal =
            new WindowsPrincipal(identity);

        return principal.IsInRole(
            WindowsBuiltInRole.Administrator
        );
    }

    public static bool TryRestartAsAdministrator(
        string[] arguments)
    {
        try
        {
            string executable =
                Environment.ProcessPath
                ?? throw new InvalidOperationException(
                    "Impossible de déterminer le chemin du launcher."
                );

            string argumentString =
                BuildArgumentString(arguments);

            ProcessStartInfo startInfo = new()
            {
                FileName = executable,
                Arguments = argumentString,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(startInfo);

            return true;
        }
        catch (System.ComponentModel.Win32Exception ex)
            when (ex.NativeErrorCode == 1223)
        {
            // L'utilisateur a refusé l'UAC.
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool NeedsAdministrator(
        string path)
    {
        try
        {
            string testFile = Path.Combine(
                path,
                $".ebonhold_write_test_{Guid.NewGuid():N}.tmp"
            );

            File.WriteAllText(
                testFile,
                "test"
            );

            File.Delete(
                testFile
            );

            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
        catch (IOException)
        {
            // Une erreur IO n'est pas nécessairement
            // un problème de droits.
            return false;
        }
    }

    private static string BuildArgumentString(
        string[] arguments)
    {
        return string.Join(
            " ",
            arguments.Select(
                QuoteArgument
            )
        );
    }

    private static string QuoteArgument(
        string argument)
    {
        if (string.IsNullOrEmpty(argument))
            return "\"\"";

        return "\"" +
               argument.Replace(
                   "\\",
                   "\\\\"
               ).Replace(
                   "\"",
                   "\\\""
               ) +
               "\"";
    }
}