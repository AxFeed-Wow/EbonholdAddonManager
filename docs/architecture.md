# Architecture

**English** | [Français](#architecture-fr)

## Overview

Ebonhold Addon Manager is a Windows desktop application built with C# and .NET 8 using Windows Forms. It keeps addon-management logic separate from the user interface while staying lightweight.

## Project structure

```text
EbonholdAddonManager/
  Models/
    AddonDefinition.cs
    AddonInfo.cs
    RepositoryMetadata.cs
  Services/
    AddonManagerService.cs
    AddonUpdater.cs
    AdminService.cs
    AppUpdateService.cs
    AppUpdater.cs
    CatalogService.cs
    GitHubService.cs
    InstallationDetector.cs
    LocalizationService.cs
    SettingsService.cs
    TocReader.cs
  .github/
  docs/
  addons.json
  CreditsForm.cs
  MainForm.cs
  UpdateForm.cs
  Program.cs
```

## Main components

* **`MainForm`** — the main interface: addon list, versions, install / update / update-all / **uninstall**, refresh, the **Propose an addon** button, language selection, and the update prompt.
* **`AddonManagerService`** — coordinates scanning, version comparison, install/update, and uninstall (removing the addon folder).
* **`AddonUpdater`** — the conservative update process: download, extract, validate, back up, install, verify with SHA-256, and roll back on failure. Local files not part of the package are preserved.
* **`GitHubService`** — repository access: raw files (`.toc`, README, LICENSE, resolving a `.toc` at the repo root or in a same-named subfolder), archive download, and author/license/version metadata.
* **`CatalogService`** — loads `addons.json` **remotely from the project repository**, with a local cache and the bundled file as fallback.
* **`InstallationDetector`** — locates and validates the Ebonhold installation (and the `Interface/AddOns` folder).
* **`TocReader`** — reads the addon `.toc` version.
* **`AdminService`** — elevation helper (used by the self-updater when the install folder is not writable).
* **`SettingsService`** — stores the selected language and installation path.
* **`LocalizationService`** — English and French strings.
* **`CreditsForm`** — author / license / repository information per addon.
* **`AppUpdateService`** — checks the latest GitHub release, compares versions, and downloads + verifies (SHA-256) the update package.
* **`AppUpdater`** — applies the application update on Windows: a temp copy of the exe replaces the installed files once the app exits, then relaunches (rollback on failure).
* **`UpdateForm`** — the update dialog (changelog + confirm).

## Data flow

A typical addon update: `MainForm` → `AddonManagerService` → `CatalogService` / `InstallationDetector` / `TocReader` / `GitHubService` → `AddonUpdater` (download → validate → back up → install → verify → rollback on failure).

The application self-update: on launch, `AppUpdateService` checks `releases/latest`; `UpdateForm` shows the changelog; on confirmation the package is downloaded and verified, then `AppUpdater` replaces the files and restarts.

The community catalogue: submissions arrive through a GitHub issue form; a maintainer applies the `approved` label; a GitHub Action validates the entry and commits it to `addons.json`, which the app then loads live.

## Design principles

* Keep the UI separate from addon-management logic.
* Prefer explicit validation over assumptions.
* Preserve existing local files whenever possible.
* Do not require administrator privileges unless necessary.
* Keep third-party addon code outside this repository.
* Prefer simple, maintainable implementations.

---

<a id="architecture-fr"></a>

# Architecture (Français)

[English](#architecture) | **Français**

## Vue d'ensemble

Ebonhold Addon Manager est une application de bureau Windows en C# et .NET 8 (Windows Forms). Elle sépare la logique de gestion des addons de l'interface tout en restant légère.

## Structure du projet

```text
EbonholdAddonManager/
  Models/
    AddonDefinition.cs
    AddonInfo.cs
    RepositoryMetadata.cs
  Services/
    AddonManagerService.cs
    AddonUpdater.cs
    AdminService.cs
    AppUpdateService.cs
    AppUpdater.cs
    CatalogService.cs
    GitHubService.cs
    InstallationDetector.cs
    LocalizationService.cs
    SettingsService.cs
    TocReader.cs
  .github/
  docs/
  addons.json
  CreditsForm.cs
  MainForm.cs
  UpdateForm.cs
  Program.cs
```

## Composants principaux

* **`MainForm`** — l'interface principale : liste des addons, versions, installer / mettre à jour / tout mettre à jour / **désinstaller**, actualiser, le bouton **Proposer un addon**, la sélection de langue et l'invite de mise à jour.
* **`AddonManagerService`** — coordonne l'analyse, la comparaison de versions, l'installation/mise à jour et la désinstallation (suppression du dossier de l'addon).
* **`AddonUpdater`** — le processus de mise à jour prudent : téléchargement, extraction, validation, sauvegarde, installation, vérification SHA-256 et rollback en cas d'échec. Les fichiers locaux hors package sont préservés.
* **`GitHubService`** — accès au dépôt : fichiers bruts (`.toc`, README, LICENSE, résolution d'un `.toc` à la racine ou dans un sous-dossier du même nom), téléchargement de l'archive, et métadonnées auteur/licence/version.
* **`CatalogService`** — charge `addons.json` **à distance depuis le dépôt du projet**, avec un cache local et le fichier embarqué en secours.
* **`InstallationDetector`** — localise et valide l'installation Ebonhold (et le dossier `Interface/AddOns`).
* **`TocReader`** — lit la version dans le `.toc` de l'addon.
* **`AdminService`** — utilitaire d'élévation (utilisé par l'auto-updater quand le dossier d'installation n'est pas accessible en écriture).
* **`SettingsService`** — stocke la langue choisie et le chemin d'installation.
* **`LocalizationService`** — chaînes anglaises et françaises.
* **`CreditsForm`** — informations auteur / licence / dépôt par addon.
* **`AppUpdateService`** — vérifie la dernière release GitHub, compare les versions, télécharge et vérifie (SHA-256) le package de mise à jour.
* **`AppUpdater`** — applique la mise à jour de l'application sous Windows : une copie temporaire de l'exe remplace les fichiers installés une fois l'app fermée, puis relance (rollback en cas d'échec).
* **`UpdateForm`** — le dialogue de mise à jour (changelog + confirmation).

## Flux de données

Une mise à jour d'addon typique : `MainForm` → `AddonManagerService` → `CatalogService` / `InstallationDetector` / `TocReader` / `GitHubService` → `AddonUpdater` (téléchargement → validation → sauvegarde → installation → vérification → rollback en cas d'échec).

La mise à jour de l'application : au lancement, `AppUpdateService` vérifie `releases/latest` ; `UpdateForm` affiche le changelog ; après confirmation le package est téléchargé et vérifié, puis `AppUpdater` remplace les fichiers et redémarre.

Le catalogue communautaire : les soumissions arrivent via un formulaire d'issue GitHub ; un mainteneur applique le label `approved` ; une GitHub Action valide l'entrée et la commit dans `addons.json`, que l'app charge ensuite en direct.

## Principes de conception

* Séparer l'UI de la logique de gestion des addons.
* Préférer la validation explicite aux suppositions.
* Préserver les fichiers locaux existants autant que possible.
* Ne pas exiger de privilèges administrateur sauf nécessité.
* Garder le code des addons tiers hors de ce dépôt.
* Préférer des implémentations simples et maintenables.
