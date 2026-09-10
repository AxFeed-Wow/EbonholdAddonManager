# Ebonhold Addon Manager

**English** | [Français](#français)

Community addon manager for World of Warcraft Project Ebonhold.

Ebonhold Addon Manager helps players discover, install, update, uninstall and manage addons for Ebonhold from their respective GitHub repositories.

> **Ebonhold Addon Manager is an independent community project. It is not affiliated with, endorsed by, or officially associated with Project Ebonhold or the authors of the addons listed in the application.**

> ⚠️ **Heads-up on addons**
>
> Some addons in the catalogue may rely on other addons that are **not listed here**, and some may **stop working** after a Project Ebonhold or upstream update.
>
> If an addon fails to install, update, or run — or if you think a dependency is missing — please **ping me on Discord** so the catalogue can be fixed.

## Features

* Automatic detection of an Ebonhold installation, with manual folder selection
* **Live addon catalogue** fetched from the project repository — new addons appear without updating the app
* **In-app "Propose an addon"** submission for community addons
* One-click **install, update, update-all and uninstall**
* Local and remote version detection
* SHA-256 validation of packaged files, with automatic rollback when an update fails validation
* Preservation of existing local files that are not part of an addon package
* **Automatic application self-update** (changelog prompt, SHA-256 verification, rollback on failure)
* English and French interface
* Repository, author and license information

## How it works

The addon catalogue is defined in `addons.json`. The application loads it **live from the project repository** (with a local cache and a bundled fallback for offline use), so an approved addon reaches everyone on the next launch — no new release required.

For each addon, the manager determines whether it is installed, the installed version, the available remote version, whether an update is available, and the repository, author and license.

During an update, the manager downloads the repository, validates the addon package, verifies the resulting files with SHA-256 hashes, and restores the previous installation if validation fails. Files that already exist locally but are not part of the downloaded package are preserved (they may contain user configuration or data).

### Proposing an addon

Anyone can submit their custom Ebonhold addon with the **Propose an addon** button (or the GitHub issue form). Submissions are reviewed before being added. Once approved, the addon is added to the catalogue automatically and appears in the app on the next launch.

### Application self-update

On launch, the manager checks the latest GitHub release. If a newer version is available, it shows the changelog and offers to update. If accepted, it downloads and verifies the new version, closes, replaces the files, and restarts — rolling back on failure.

## Installation

### Requirements

* Windows 10 or later
* A working Project Ebonhold installation
* Internet access for downloading addon information and updates

### Using a release

Download the latest release from the GitHub Releases page and run the application. The manager will attempt to locate your Ebonhold installation automatically; otherwise use **Change folder** to select it.

### Building from source

```powershell
git clone https://github.com/AxFeed-Wow/EbonholdAddonManager.git
cd EbonholdAddonManager
dotnet build
dotnet run
```

## Addon catalogue

The catalogue is stored in `addons.json`. Each entry defines the information required to locate and manage an addon:

```json
{
  "id": "example-addon",
  "name": "Example Addon",
  "folder": "ExampleAddon",
  "repository": "Author/Repository",
  "branch": "main",
  "preferRelease": true
}
```

`repository` points to the addon author's GitHub repository, and `folder` must match the addon directory installed under `Interface/AddOns/` (it must equal the `.toc` file name).

See [docs/addon-catalog.md](docs/addon-catalog.md) for the full catalogue documentation.

## Security

If you discover a security vulnerability, please do not disclose it publicly through a GitHub issue. See [SECURITY.md](SECURITY.md).

## Disclaimer

Ebonhold Addon Manager is an independent community-developed tool. It is not affiliated with Project Ebonhold, Blizzard Entertainment, or the authors of the third-party addons managed by the application. All third-party addon names, repositories, trademarks and associated rights remain with their respective owners.

## License

Released under the MIT License. See [LICENSE](LICENSE).

---

<a id="français"></a>

# Ebonhold Addon Manager (Français)

[English](#ebonhold-addon-manager) | **Français**

Gestionnaire d'addons communautaire pour World of Warcraft Project Ebonhold.

Ebonhold Addon Manager aide les joueurs à découvrir, installer, mettre à jour, désinstaller et gérer les addons d'Ebonhold depuis leurs dépôts GitHub respectifs.

> **Ebonhold Addon Manager est un projet communautaire indépendant. Il n'est ni affilié, ni approuvé, ni officiellement associé à Project Ebonhold ou aux auteurs des addons listés dans l'application.**

> ⚠️ **À savoir sur les addons**
>
> Certains addons du catalogue peuvent dépendre d'autres addons qui **ne sont pas listés ici**, et certains peuvent **cesser de fonctionner** après une mise à jour de Project Ebonhold ou d'un dépôt source.
>
> Si un addon ne s'installe pas, ne se met pas à jour, ne fonctionne pas — ou si tu penses qu'une dépendance manque — **ping-moi sur Discord** pour que le catalogue soit corrigé.

## Fonctionnalités

* Détection automatique d'une installation Ebonhold, avec sélection manuelle du dossier
* **Catalogue d'addons en direct** récupéré depuis le dépôt du projet — les nouveaux addons apparaissent sans mettre à jour l'application
* Bouton **« Proposer un addon »** intégré pour les addons communautaires
* **Installer, mettre à jour, tout mettre à jour et désinstaller** en un clic
* Détection des versions locale et distante
* Validation SHA-256 des fichiers du package, avec rollback automatique si une mise à jour échoue à la validation
* Préservation des fichiers locaux existants qui ne font pas partie du package d'un addon
* **Mise à jour automatique de l'application** (affichage du changelog, vérification SHA-256, rollback en cas d'échec)
* Interface en anglais et en français
* Informations de dépôt, d'auteur et de licence

## Fonctionnement

Le catalogue d'addons est défini dans `addons.json`. L'application le charge **en direct depuis le dépôt du projet** (avec un cache local et un fichier embarqué en secours hors ligne), donc un addon approuvé arrive chez tout le monde au prochain lancement — aucune nouvelle release nécessaire.

Pour chaque addon, le gestionnaire détermine s'il est installé, la version installée, la version distante disponible, si une mise à jour existe, ainsi que le dépôt, l'auteur et la licence.

Lors d'une mise à jour, le gestionnaire télécharge le dépôt, valide le package de l'addon, vérifie les fichiers résultants avec des empreintes SHA-256, et restaure l'installation précédente si la validation échoue. Les fichiers déjà présents localement mais absents du package téléchargé sont préservés (ils peuvent contenir de la configuration ou des données utilisateur).

### Proposer un addon

N'importe qui peut soumettre son addon Ebonhold custom via le bouton **Proposer un addon** (ou le formulaire d'issue GitHub). Les soumissions sont examinées avant d'être ajoutées. Une fois approuvé, l'addon est ajouté au catalogue automatiquement et apparaît dans l'app au lancement suivant.

### Mise à jour automatique de l'application

Au lancement, le gestionnaire vérifie la dernière release GitHub. Si une version plus récente existe, il affiche le changelog et propose la mise à jour. Si tu acceptes, il télécharge et vérifie la nouvelle version, ferme, remplace les fichiers et redémarre — avec rollback en cas d'échec.

## Installation

### Prérequis

* Windows 10 ou plus récent
* Une installation Project Ebonhold fonctionnelle
* Un accès Internet pour télécharger les informations et les mises à jour

### Depuis une release

Télécharge la dernière release depuis la page GitHub Releases et lance l'application. Le gestionnaire tentera de localiser ton installation Ebonhold automatiquement ; sinon utilise **Changer de dossier** pour la sélectionner.

### Compilation depuis les sources

```powershell
git clone https://github.com/AxFeed-Wow/EbonholdAddonManager.git
cd EbonholdAddonManager
dotnet build
dotnet run
```

## Catalogue d'addons

Le catalogue est stocké dans `addons.json`. Chaque entrée définit les informations nécessaires pour localiser et gérer un addon :

```json
{
  "id": "example-addon",
  "name": "Example Addon",
  "folder": "ExampleAddon",
  "repository": "Author/Repository",
  "branch": "main",
  "preferRelease": true
}
```

`repository` pointe vers le dépôt GitHub de l'auteur de l'addon, et `folder` doit correspondre au dossier de l'addon installé sous `Interface/AddOns/` (il doit être identique au nom du fichier `.toc`).

Voir [docs/addon-catalog.md](docs/addon-catalog.md) pour la documentation complète du catalogue.

## Sécurité

Si tu découvres une faille de sécurité, ne la divulgue pas publiquement via une issue GitHub. Voir [SECURITY.md](SECURITY.md).

## Avertissement

Ebonhold Addon Manager est un outil communautaire indépendant. Il n'est pas affilié à Project Ebonhold, Blizzard Entertainment, ni aux auteurs des addons tiers gérés par l'application. Tous les noms, dépôts, marques et droits associés des addons tiers restent la propriété de leurs détenteurs respectifs.

## Licence

Publié sous licence MIT. Voir [LICENSE](LICENSE).
