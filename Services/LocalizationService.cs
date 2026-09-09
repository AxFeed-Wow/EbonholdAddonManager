using System.Collections.Generic;

namespace EbonholdAddonManager.Services;

public enum AppLanguage
{
    English,
    French
}

public static class LocalizationService
{
    private static AppLanguage _currentLanguage =
        AppLanguage.English;

    private static readonly Dictionary<string, string> English =
        new()
        {
            ["app.title"] = "Ebonhold Addon Manager",
            ["app.subtitle"] =
                "Manage, install and update your Ebonhold addons",

            ["language"] = "Language",

            ["installation"] = "Ebonhold installation",
            ["change_folder"] = "Change folder",
            ["select_ebonhold"] =
                "Select your Ebonhold installation folder",

            ["searching"] = "Searching for Ebonhold...",
            ["ready"] = "Ready",
            ["not_found"] = "Ebonhold installation not found",

            ["refresh"] = "Refresh",
            ["update_all"] = "Update all",
            ["credits"] = "Credits",

            ["scanning"] = "Scanning addons...",
            ["scanning_addon"] = "Scanning {0}...",
            ["scan_complete"] = "Scan complete.",
            ["cancelled"] = "Operation cancelled.",

            ["processing"] = "Processing",
            ["status_complete"] = "completed.",
            ["all_updates_complete"] =
                "All updates completed.",

            ["addons_folder_not_found"] =
                "The Ebonhold AddOns folder could not be found.",

            ["invalid_ebonhold"] =
                "The selected folder does not appear to be a valid Ebonhold installation.",

            ["not_installed"] = "Not installed",
            ["up_to_date"] = "Up to date",
            ["update_available"] = "Update available",
            ["unknown"] = "Unknown",
            ["error"] = "Error",

            ["version"] = "Version",

            ["addons"] = "addons",
            ["installed"] = "installed",
            ["update_available_short"] =
                "updates available",
            ["errors"] = "errors",

            ["install"] = "Install",
            ["update"] = "Update",
            ["github"] = "GitHub",

            ["description_unavailable"] =
                "No description available.",

            ["not_installed_message"] =
                "This addon is not installed.",

            ["local_version_unknown"] =
                "The installed version could not be determined.",

            ["remote_version_unknown"] =
                "The remote version could not be determined.",

            ["up_to_date_message"] =
                "The installed version is up to date.",

            ["update_available_message"] =
                "A newer version is available.",

            ["error_prefix"] = "Error: ",

            ["error_title"] =
                "Ebonhold Addon Manager",

            ["credits_title"] =
                "Addon Credits",

            ["credits_disclaimer"] =
                "Addons are developed and maintained by their respective authors.\r\n" +
                "Ebonhold Addon Manager is an independent community project and is not affiliated with the addon authors.",

            ["unknown_author"] =
                "Unknown author",

            ["license_not_specified"] =
                "License not specified",

            ["by"] = "by",

            ["close"] = "Close"
        };

    private static readonly Dictionary<string, string> French =
        new()
        {
            ["app.title"] = "Ebonhold Addon Manager",
            ["app.subtitle"] =
                "Gérez, installez et mettez à jour vos addons Ebonhold",

            ["language"] = "Langue",

            ["installation"] = "Installation Ebonhold",
            ["change_folder"] = "Changer de dossier",
            ["select_ebonhold"] =
                "Sélectionnez votre dossier d'installation Ebonhold",

            ["searching"] = "Recherche d'Ebonhold...",
            ["ready"] = "Prêt",
            ["not_found"] =
                "Installation Ebonhold introuvable",

            ["refresh"] = "Actualiser",
            ["update_all"] = "Tout mettre à jour",
            ["credits"] = "Crédits",

            ["scanning"] = "Analyse des addons...",
            ["scanning_addon"] = "Analyse de {0}...",
            ["scan_complete"] = "Analyse terminée.",
            ["cancelled"] = "Opération annulée.",

            ["processing"] = "Traitement de",
            ["status_complete"] = "terminé.",
            ["all_updates_complete"] =
                "Toutes les mises à jour sont terminées.",

            ["addons_folder_not_found"] =
                "Le dossier AddOns d'Ebonhold est introuvable.",

            ["invalid_ebonhold"] =
                "Le dossier sélectionné ne semble pas être une installation Ebonhold valide.",

            ["not_installed"] = "Non installé",
            ["up_to_date"] = "À jour",
            ["update_available"] = "Mise à jour disponible",
            ["unknown"] = "Inconnu",
            ["error"] = "Erreur",

            ["version"] = "Version",

            ["addons"] = "addons",
            ["installed"] = "installés",
            ["update_available_short"] =
                "mises à jour disponibles",
            ["errors"] = "erreurs",

            ["install"] = "Installer",
            ["update"] = "Mettre à jour",
            ["github"] = "GitHub",

            ["description_unavailable"] =
                "Aucune description disponible.",

            ["not_installed_message"] =
                "Cet addon n'est pas installé.",

            ["local_version_unknown"] =
                "La version installée n'a pas pu être déterminée.",

            ["remote_version_unknown"] =
                "La version distante n'a pas pu être déterminée.",

            ["up_to_date_message"] =
                "La version installée est à jour.",

            ["update_available_message"] =
                "Une nouvelle version est disponible.",

            ["error_prefix"] = "Erreur : ",

            ["error_title"] =
                "Ebonhold Addon Manager",

            ["credits_title"] =
                "Crédits des addons",

            ["credits_disclaimer"] =
                "Les addons sont développés et maintenus par leurs auteurs respectifs.\r\n" +
                "Ebonhold Addon Manager est un projet communautaire indépendant et n'est pas affilié aux auteurs des addons.",

            ["unknown_author"] =
                "Auteur inconnu",

            ["license_not_specified"] =
                "Licence non spécifiée",

            ["by"] = "par",

            ["close"] = "Fermer"
        };

    public static AppLanguage CurrentLanguage =>
        _currentLanguage;

    public static void SetLanguage(
        AppLanguage language)
    {
        _currentLanguage = language;
    }

    public static string Get(
        string key)
    {
        Dictionary<string, string> dictionary =
            _currentLanguage == AppLanguage.French
                ? French
                : English;

        if (dictionary.TryGetValue(
                key,
                out string? value))
        {
            return value;
        }

        // Fallback to English if a translation is missing.
        if (English.TryGetValue(
                key,
                out string? englishValue))
        {
            return englishValue;
        }

        return key;
    }
}