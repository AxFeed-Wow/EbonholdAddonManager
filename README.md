\# Ebonhold Addon Manager



Community addon manager for World of Warcraft Project Ebonhold.



Ebonhold Addon Manager helps players discover, install, update and manage addons for Ebonhold from their respective GitHub repositories.



> \*\*Ebonhold Addon Manager is an independent community project. It is not affiliated with, endorsed by, or officially associated with Project Ebonhold or the authors of the addons listed in the application.\*\*



\## Features



\* Automatic detection of an Ebonhold installation

\* Manual Ebonhold folder selection

\* Addon catalogue powered by `addons.json`

\* Automatic addon installation

\* Addon updates

\* Update-all functionality

\* Local and remote version detection

\* GitHub repository metadata

\* Author and license information

\* Direct links to addon repositories

\* SHA-256 validation of packaged files

\* Automatic rollback when an update fails validation

\* Preservation of existing local files that are not part of an addon update

\* English and French interface

\* Administrator elevation only when required



\## How it works



Ebonhold Addon Manager uses the GitHub repositories defined in `addons.json` as the source for addon updates.



For each addon, the manager can determine:



\* Whether the addon is installed

\* The installed version

\* The available remote version

\* Whether an update is available

\* The repository hosting the addon

\* The declared author

\* The available license information



During an update, the manager downloads the repository, validates the addon package, prepares the new version and verifies the resulting files using SHA-256 hashes.



If the installation fails validation, the previous installation is restored whenever possible.



\### Local files



The manager does not automatically remove files that are not present in the downloaded addon package.



This is intentional: addon directories may contain user-created files, configuration data or other local content that should not be deleted simply because it is not present in the upstream repository.



\## Addon sources



The addons managed by this application are hosted and maintained by their respective authors.



The application does not claim ownership of any third-party addon.



Addon repositories, authors and license information are displayed in the application's Credits section and are sourced from the corresponding repositories when available.



If a repository does not clearly specify a license, the application displays \*\*"License not specified"\*\* rather than assuming that redistribution is permitted.



\## Installation



\### Requirements



\* Windows 10 or later

\* A working Project Ebonhold installation

\* Internet access for downloading addon information and updates



\### Using a release



Download the latest release from the GitHub Releases page and run the application.



The manager will attempt to automatically locate your Ebonhold installation.



If it cannot find it automatically, use \*\*Change folder\*\* to select the appropriate installation directory.



\### Building from source



Clone the repository:



```powershell

git clone https://github.com/AxFeed-Wow/EbonholdAddonManager.git

cd EbonholdAddonManager

```



Build the project:



```powershell

dotnet build

```



Run it with:



```powershell

dotnet run

```



\## Addon catalogue



The addon catalogue is stored in:



```text

addons.json

```



Each entry defines the information required to locate and manage an addon.



Example:



```json

{

&#x20; "id": "example-addon",

&#x20; "name": "Example Addon",

&#x20; "folder": "ExampleAddon",

&#x20; "repository": "Author/Repository",

&#x20; "branch": "main",

&#x20; "preferRelease": true

}

```



The repository should point to the addon author's GitHub repository.



The `folder` value must correspond to the addon directory installed under:



```text

Interface/AddOns/

```



\## Project structure



```text

EbonholdAddonManager/

├── Models/

│   ├── AddonDefinition.cs

│   ├── AddonInfo.cs

│   └── RepositoryMetadata.cs

│

├── Services/

│   ├── AddonManagerService.cs

│   ├── AddonUpdater.cs

│   ├── AdminService.cs

│   ├── CatalogService.cs

│   ├── GitHubService.cs

│   ├── InstallationDetector.cs

│   ├── LocalizationService.cs

│   ├── SettingsService.cs

│   └── TocReader.cs

│

├── addons.json

├── CreditsForm.cs

├── MainForm.cs

├── Program.cs

├── EbonholdAddonManager.csproj

└── EbonholdAddonManager.slnx

```



\## Development



The project is built with:



\* C#

\* .NET 8

\* Windows Forms

\* GitHub-hosted addon repositories



The project aims to remain lightweight and straightforward to maintain.



Contributions, bug reports and feature suggestions are welcome.



See \[CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.



\## Security



If you discover a security vulnerability, please do not disclose it publicly through a GitHub issue.



See \[SECURITY.md](SECURITY.md) for information about responsible disclosure.



\## Disclaimer



Ebonhold Addon Manager is an independent community-developed tool.



It is not affiliated with Project Ebonhold, Blizzard Entertainment, or the authors of the third-party addons managed by the application.



All third-party addon names, repositories, trademarks and associated rights remain with their respective owners.



Users are responsible for ensuring that their use and redistribution of third-party addons complies with the applicable licenses.



\## License



Ebonhold Addon Manager is released under the MIT License.



See \[LICENSE](LICENSE) for the full license text.



\---



\# Ebonhold Addon Manager — Français



Gestionnaire communautaire d'addons pour World of Warcraft Project Ebonhold.



Ebonhold Addon Manager permet de découvrir, installer, mettre à jour et gérer les addons Ebonhold depuis leurs dépôts GitHub respectifs.



> \*\*Ebonhold Addon Manager est un projet communautaire indépendant. Il n'est pas affilié à Project Ebonhold et n'est ni approuvé ni officiellement associé aux auteurs des addons référencés dans l'application.\*\*



\## Fonctionnalités



\* Détection automatique de l'installation Ebonhold

\* Sélection manuelle du dossier Ebonhold

\* Catalogue d'addons basé sur `addons.json`

\* Installation automatique des addons

\* Mise à jour des addons

\* Mise à jour de tous les addons

\* Détection des versions locales et distantes

\* Récupération des métadonnées GitHub

\* Affichage des auteurs et licences

\* Liens directs vers les dépôts des addons

\* Vérification SHA-256 des fichiers téléchargés

\* Retour automatique à l'ancienne version en cas d'échec de validation

\* Conservation des fichiers locaux qui ne font pas partie d'une mise à jour

\* Interface française et anglaise

\* Élévation administrateur uniquement lorsque cela est nécessaire



\## Fonctionnement



Ebonhold Addon Manager utilise les dépôts GitHub définis dans `addons.json` comme sources pour les mises à jour des addons.



Pour chaque addon, l'application peut déterminer :



\* S'il est installé

\* Sa version installée

\* La version distante disponible

\* Si une mise à jour est disponible

\* Le dépôt GitHub correspondant

\* L'auteur déclaré

\* Les informations de licence disponibles



Lors d'une mise à jour, l'application télécharge le dépôt, valide le contenu de l'addon, prépare la nouvelle version puis vérifie les fichiers résultants à l'aide de leurs empreintes SHA-256.



Si l'installation échoue lors de la validation, l'ancienne version est restaurée lorsque cela est possible.



\### Fichiers locaux



L'application ne supprime pas automatiquement les fichiers qui ne sont pas présents dans le package téléchargé.



Ce comportement est volontaire : les dossiers d'addons peuvent contenir des fichiers créés par l'utilisateur, des données de configuration ou d'autres contenus locaux qui ne doivent pas être supprimés simplement parce qu'ils ne sont pas présents dans le dépôt source.



\## Sources des addons



Les addons gérés par cette application sont hébergés et maintenus par leurs auteurs respectifs.



L'application ne revendique aucun droit de propriété sur les addons tiers.



Les dépôts, auteurs et informations de licence sont affichés dans la section Crédits et sont récupérés depuis les dépôts correspondants lorsque ces informations sont disponibles.



Lorsqu'un dépôt ne précise pas clairement sa licence, l'application affiche \*\*« Licence non spécifiée »\*\* au lieu de supposer que la redistribution est autorisée.



\## Installation



\### Prérequis



\* Windows 10 ou version ultérieure

\* Une installation fonctionnelle de Project Ebonhold

\* Une connexion Internet pour récupérer les informations et les mises à jour des addons



\### Utilisation d'une release



Téléchargez la dernière version depuis la page GitHub Releases et lancez l'application.



Le gestionnaire tentera automatiquement de détecter votre installation Ebonhold.



Si celle-ci n'est pas détectée automatiquement, utilisez \*\*Changer de dossier\*\* pour sélectionner le dossier d'installation approprié.



\### Compilation depuis les sources



Clonez le dépôt :



```powershell

git clone https://github.com/AxFeed-Wow/EbonholdAddonManager.git

cd EbonholdAddonManager

```



Compilez le projet :



```powershell

dotnet build

```



Lancez l'application :



```powershell

dotnet run

```



\## Catalogue des addons



Le catalogue des addons se trouve dans :



```text

addons.json

```



Chaque entrée définit les informations nécessaires pour localiser et gérer un addon.



Exemple :



```json

{

&#x20; "id": "example-addon",

&#x20; "name": "Example Addon",

&#x20; "folder": "ExampleAddon",

&#x20; "repository": "Author/Repository",

&#x20; "branch": "main",

&#x20; "preferRelease": true

}

```



Le dépôt doit correspondre au dépôt GitHub de l'auteur de l'addon.



La valeur `folder` doit correspondre au dossier de l'addon installé dans :



```text

Interface/AddOns/

```



\## Développement



Le projet utilise :



\* C#

\* .NET 8

\* Windows Forms

\* Des dépôts d'addons hébergés sur GitHub



L'objectif est de conserver un projet léger, clair et facile à maintenir.



Les contributions, rapports de bugs et suggestions de fonctionnalités sont les bienvenus.



Consultez \[CONTRIBUTING.md](CONTRIBUTING.md) pour les règles de contribution.



\## Sécurité



Si vous découvrez une vulnérabilité de sécurité, merci de ne pas la publier directement dans une issue GitHub.



Consultez \[SECURITY.md](SECURITY.md) pour connaître la procédure de signalement.



\## Avertissement



Ebonhold Addon Manager est un outil communautaire développé indépendamment.



Il n'est pas affilié à Project Ebonhold, Blizzard Entertainment ou aux auteurs des addons tiers gérés par l'application.



Les noms, dépôts, marques et droits associés aux addons tiers restent la propriété de leurs détenteurs respectifs.



Les utilisateurs sont responsables du respect des licences applicables lors de l'utilisation ou de la redistribution des addons tiers.



\## Licence



Ebonhold Addon Manager est distribué sous licence MIT.



Consultez \[LICENSE](LICENSE) pour le texte complet de la licence.
