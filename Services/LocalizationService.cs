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
            ["propose_addon"] = "Propose an addon",
            ["search_placeholder"] = "Search addons...",

            ["update_available_title"] = "Update available",
            ["update_available_intro"] =
                "A new version of Ebonhold Addon Manager is available.",
            ["update_no_notes"] = "No release notes provided.",
            ["update_now"] = "Update now",
            ["update_later"] = "Later",
            ["update_downloading"] = "Downloading update...",
            ["update_verifying"] = "Verifying...",
            ["update_restarting"] = "Restarting...",
            ["update_rollback_title"] = "Ebonhold Addon Manager - Update",
            ["update_rollback_ok"] =
                "The update could not be applied and the previous version was restored.",
            ["update_rollback_fail"] =
                "The update could not be applied.",
            ["update_check_now"] = "Check for updates",
            ["update_up_to_date"] = "You are running the latest version.",

            ["warning_notice"] =
                "⚠  Some addons may need dependencies that are not listed here, and some may stop working after a server or addon update. If something looks off, ping me on Discord.",

            ["scanning"] = "Scanning addons...",
            ["scanning_addon"] = "Scanning {0}...",
            ["scanning_progress"] = "Scanning addons... {0}/{1}",
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
            ["uninstall"] = "Uninstall",
            ["uninstall_confirm_title"] = "Uninstall addon",
            ["uninstall_confirm"] =
                "Remove {0}?\r\n\r\nThis deletes the addon folder. Your saved settings (WTF) are kept.",
            ["uninstall_complete"] = "uninstalled.",
            ["elevation_title"] = "Administrator rights required",
            ["elevation_required"] =
                "This Ebonhold installation is in a protected location, so changing addons requires administrator rights.\r\n\r\nRestart Ebonhold Addon Manager as administrator?",
            ["elevation_failed"] = "Could not restart as administrator.",
            ["requires_label"] = "⚠ Requires {0}",
            ["requires_confirm_title"] = "Dependency required",
            ["requires_confirm"] =
                "{0} requires:\r\n\r\n{1}\r\n\r\nMake sure you have it set up, then continue with the installation?",
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
            ["propose_addon"] = "Proposer un addon",
            ["search_placeholder"] = "Rechercher un addon...",

            ["update_available_title"] = "Mise à jour disponible",
            ["update_available_intro"] =
                "Une nouvelle version d'Ebonhold Addon Manager est disponible.",
            ["update_no_notes"] = "Aucune note de version fournie.",
            ["update_now"] = "Mettre à jour maintenant",
            ["update_later"] = "Plus tard",
            ["update_downloading"] = "Téléchargement de la mise à jour...",
            ["update_verifying"] = "Vérification...",
            ["update_restarting"] = "Redémarrage...",
            ["update_rollback_title"] = "Ebonhold Addon Manager - Mise à jour",
            ["update_rollback_ok"] =
                "La mise à jour n'a pas pu être appliquée et la version précédente a été restaurée.",
            ["update_rollback_fail"] =
                "La mise à jour n'a pas pu être appliquée.",
            ["update_check_now"] = "Vérifier les mises à jour",
            ["update_up_to_date"] = "Vous utilisez la dernière version.",

            ["warning_notice"] =
                "⚠  Certains addons peuvent nécessiter des dépendances non listées ici, et d'autres peuvent cesser de fonctionner après une mise à jour du serveur ou de l'addon. Si quelque chose cloche, ping-moi sur Discord.",

            ["scanning"] = "Analyse des addons...",
            ["scanning_addon"] = "Analyse de {0}...",
            ["scanning_progress"] = "Analyse des addons... {0}/{1}",
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
            ["uninstall"] = "Désinstaller",
            ["uninstall_confirm_title"] = "Désinstaller l'addon",
            ["uninstall_confirm"] =
                "Supprimer {0} ?\r\n\r\nCela supprime le dossier de l'addon. Tes réglages sauvegardés (WTF) sont conservés.",
            ["uninstall_complete"] = "désinstallé.",
            ["elevation_title"] = "Droits administrateur requis",
            ["elevation_required"] =
                "Cette installation Ebonhold est dans un emplacement protégé ; modifier les addons nécessite les droits administrateur.\r\n\r\nRedémarrer Ebonhold Addon Manager en administrateur ?",
            ["elevation_failed"] = "Impossible de redémarrer en administrateur.",
            ["requires_label"] = "⚠ Nécessite {0}",
            ["requires_confirm_title"] = "Dépendance requise",
            ["requires_confirm"] =
                "{0} nécessite :\r\n\r\n{1}\r\n\r\nAssure-toi de l'avoir configuré, puis continuer l'installation ?",
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