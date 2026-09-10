# Development Guide

**English** | [Français](#guide-de-développement)

## Requirements

* Windows
* .NET 8 SDK (or a compatible newer SDK)
* Git
* Visual Studio 2022 or another C#/.NET environment

The project targets `net8.0-windows` and uses Windows Forms.

## Build and run

```powershell
git clone https://github.com/AxFeed-Wow/EbonholdAddonManager.git
cd EbonholdAddonManager
dotnet restore EbonholdAddonManager.slnx
dotnet build EbonholdAddonManager.slnx --configuration Release
dotnet run --project EbonholdAddonManager.csproj
```

## Project organization

* `Models/` — data models
* `Services/` — addon management, GitHub access, catalogue, self-update and supporting services
* `MainForm.cs` — main user interface
* `CreditsForm.cs` — addon credits
* `UpdateForm.cs` — application update dialog
* `Program.cs` — entry point (also dispatches the `--apply-update` updater mode)
* `addons.json` — bundled catalogue (fallback; the live catalogue is loaded from the repository)

See [Architecture](architecture.md) for details.

## Working with the addon catalogue

When adding or modifying an entry in `addons.json`:

1. Verify the upstream repository, branch and addon directory name.
2. Confirm the repository contains a valid `.toc` (at the root or in a same-named subfolder) and that `folder` equals the `.toc` name.
3. Check the upstream license and attribution.
4. Test installation when possible.

Most additions happen through the submission issue form and the `approved` label, which runs the same validation automatically. See [Addon Catalog](addon-catalog.md).

## Testing changes

```powershell
dotnet restore EbonholdAddonManager.slnx
dotnet build EbonholdAddonManager.slnx --configuration Release
```

When changing addon installation/update logic, test: new install, existing update, version detection, package validation, rollback after failure, preservation of local files, and uninstall.

## Versioning and releases

The application version is set with `<Version>` in the `.csproj`. A release is produced by pushing a `v*` tag: the release workflow stamps the build with the tag version, publishes a single-file self-contained `win-x64` zip plus a `.sha256`, creates the GitHub release, and scans it with VirusTotal. The app compares its version to the latest release to offer self-updates.

## GitHub Actions

* `build.yml` — restores and builds the solution on pushes and pull requests.
* `release.yml` — on a `v*` tag: publish, zip, checksum, create the release, then VirusTotal scan.
* `addon-approval.yml` — on the `approved` label: validate an addon submission and commit it to `addons.json`.

## Coding guidelines

* Keep changes focused; avoid unrelated refactoring.
* Prefer clear names, small methods, explicit error handling and the existing patterns.
* Code, comments and identifiers are written in English.
* Do not hard-code user-specific paths, add unnecessary dependencies, silently swallow exceptions, or commit generated files.
* Do not copy third-party addon source into this repository.

---

<a id="guide-de-développement"></a>

# Guide de développement

[English](#development-guide) | **Français**

## Prérequis

* Windows
* SDK .NET 8 (ou un SDK plus récent compatible)
* Git
* Visual Studio 2022 ou un autre environnement C#/.NET

Le projet cible `net8.0-windows` et utilise Windows Forms.

## Compiler et lancer

```powershell
git clone https://github.com/AxFeed-Wow/EbonholdAddonManager.git
cd EbonholdAddonManager
dotnet restore EbonholdAddonManager.slnx
dotnet build EbonholdAddonManager.slnx --configuration Release
dotnet run --project EbonholdAddonManager.csproj
```

## Organisation du projet

* `Models/` — modèles de données
* `Services/` — gestion des addons, accès GitHub, catalogue, auto-update et services de support
* `MainForm.cs` — interface principale
* `CreditsForm.cs` — crédits des addons
* `UpdateForm.cs` — dialogue de mise à jour de l'application
* `Program.cs` — point d'entrée (dispatche aussi le mode updater `--apply-update`)
* `addons.json` — catalogue embarqué (secours ; le catalogue en direct est chargé depuis le dépôt)

Voir [Architecture](architecture.md) pour les détails.

## Travailler avec le catalogue d'addons

En ajoutant ou modifiant une entrée de `addons.json` :

1. Vérifie le dépôt source, la branche et le nom du dossier de l'addon.
2. Confirme que le dépôt contient un `.toc` valide (à la racine ou dans un sous-dossier du même nom) et que `folder` est identique au nom du `.toc`.
3. Vérifie la licence et l'attribution en amont.
4. Teste l'installation quand c'est possible.

La plupart des ajouts passent par le formulaire de soumission et le label `approved`, qui exécute la même validation automatiquement. Voir [Catalogue d'addons](addon-catalog.md).

## Tester les changements

```powershell
dotnet restore EbonholdAddonManager.slnx
dotnet build EbonholdAddonManager.slnx --configuration Release
```

En modifiant la logique d'installation/mise à jour, teste : nouvelle installation, mise à jour existante, détection de version, validation du package, rollback après échec, préservation des fichiers locaux, et désinstallation.

## Versioning et releases

La version de l'application est définie avec `<Version>` dans le `.csproj`. Une release est produite en poussant un tag `v*` : le workflow de release stampe le build avec la version du tag, publie un zip `win-x64` self-contained mono-fichier plus un `.sha256`, crée la release GitHub, et la scanne avec VirusTotal. L'app compare sa version à la dernière release pour proposer les mises à jour automatiques.

## GitHub Actions

* `build.yml` — restaure et compile la solution sur les push et pull requests.
* `release.yml` — sur un tag `v*` : publish, zip, checksum, création de la release, puis scan VirusTotal.
* `addon-approval.yml` — sur le label `approved` : valide une soumission d'addon et la commit dans `addons.json`.

## Règles de code

* Garde les changements ciblés ; évite le refactoring sans rapport.
* Privilégie des noms clairs, de petites méthodes, une gestion d'erreurs explicite et les patterns existants.
* Le code, les commentaires et les identifiants sont écrits en anglais.
* Ne code pas en dur de chemins spécifiques à un utilisateur, n'ajoute pas de dépendances inutiles, n'avale pas silencieusement les exceptions, et ne commite pas de fichiers générés.
* Ne copie pas de code source d'addons tiers dans ce dépôt.
