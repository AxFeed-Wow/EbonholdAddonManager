# Ebonhold Addon Manager



Community addon manager for World of Warcraft Project Ebonhold.



Ebonhold Addon Manager helps players discover, install, update and manage addons for Ebonhold from their respective GitHub repositories.



> **Ebonhold Addon Manager is an independent community project. It is not affiliated with, endorsed by, or officially associated with Project Ebonhold or the authors of the addons listed in the application.**



## Features



* Automatic detection of an Ebonhold installation

* Manual Ebonhold folder selection

* Addon catalogue powered by `addons.json`

* Automatic addon installation

* Addon updates

* Update-all functionality

* Local and remote version detection

* GitHub repository metadata

* Author and license information

* Direct links to addon repositories

* SHA-256 validation of packaged files

* Automatic rollback when an update fails validation

* Preservation of existing local files that are not part of an addon update

* English and French interface

* Administrator elevation only when required



## How it works



Ebonhold Addon Manager uses the GitHub repositories defined in `addons.json` as the source for addon updates.



For each addon, the manager can determine:



* Whether the addon is installed

* The installed version

* The available remote version

* Whether an update is available

* The repository hosting the addon

* The declared author

* The available license information



During an update, the manager downloads the repository, validates the addon package, prepares the new version and verifies the resulting files using SHA-256 hashes.



If the installation fails validation, the previous installation is restored whenever possible.



### Local files



The manager does not automatically remove files that are not present in the downloaded addon package.



This is intentional: addon directories may contain user-created files, configuration data or other local content that should not be deleted simply because it is not present in the upstream repository.



## Addon sources



The addons managed by this application are hosted and maintained by their respective authors.



The application does not claim ownership of any third-party addon.



Addon repositories, authors and license information are displayed in the application's Credits section and are sourced from the corresponding repositories when available.



If a repository does not clearly specify a license, the application displays **"License not specified"** rather than assuming that redistribution is permitted.



## Installation



### Requirements



* Windows 10 or later

* A working Project Ebonhold installation

* Internet access for downloading addon information and updates



### Using a release



Download the latest release from the GitHub Releases page and run the application.



The manager will attempt to automatically locate your Ebonhold installation.



If it cannot find it automatically, use **Change folder** to select the appropriate installation directory.



### Building from source



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



## Addon catalogue



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



## Project structure



```text

EbonholdAddonManager/

Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Models/

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ AddonDefinition.cs

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ AddonInfo.cs

Ã¢â€â€š   Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ RepositoryMetadata.cs

Ã¢â€â€š

Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Services/

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ AddonManagerService.cs

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ AddonUpdater.cs

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ AdminService.cs

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ CatalogService.cs

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ GitHubService.cs

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ InstallationDetector.cs

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ LocalizationService.cs

Ã¢â€â€š   Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ SettingsService.cs

Ã¢â€â€š   Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ TocReader.cs

Ã¢â€â€š

Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ addons.json

Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ CreditsForm.cs

Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ MainForm.cs

Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ Program.cs

Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ EbonholdAddonManager.csproj

Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ EbonholdAddonManager.slnx

```



## Development



The project is built with:



* C#

* .NET 8

* Windows Forms

* GitHub-hosted addon repositories



The project aims to remain lightweight and straightforward to maintain.



Contributions, bug reports and feature suggestions are welcome.



See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.



## Security



If you discover a security vulnerability, please do not disclose it publicly through a GitHub issue.



See [SECURITY.md](SECURITY.md) for information about responsible disclosure.



## Disclaimer



Ebonhold Addon Manager is an independent community-developed tool.



It is not affiliated with Project Ebonhold, Blizzard Entertainment, or the authors of the third-party addons managed by the application.



All third-party addon names, repositories, trademarks and associated rights remain with their respective owners.



Users are responsible for ensuring that their use and redistribution of third-party addons complies with the applicable licenses.



## License



Ebonhold Addon Manager is released under the MIT License.



See [LICENSE](LICENSE) for the full license text.



---



# Ebonhold Addon Manager Ã¢â‚¬â€ FranÃƒÂ§ais



Gestionnaire communautaire d'addons pour World of Warcraft Project Ebonhold.



Ebonhold Addon Manager permet de dÃƒÂ©couvrir, installer, mettre ÃƒÂ  jour et gÃƒÂ©rer les addons Ebonhold depuis leurs dÃƒÂ©pÃƒÂ´ts GitHub respectifs.



> **Ebonhold Addon Manager est un projet communautaire indÃƒÂ©pendant. Il n'est pas affiliÃƒÂ© ÃƒÂ  Project Ebonhold et n'est ni approuvÃƒÂ© ni officiellement associÃƒÂ© aux auteurs des addons rÃƒÂ©fÃƒÂ©rencÃƒÂ©s dans l'application.**



## FonctionnalitÃƒÂ©s



* DÃƒÂ©tection automatique de l'installation Ebonhold

* SÃƒÂ©lection manuelle du dossier Ebonhold

* Catalogue d'addons basÃƒÂ© sur `addons.json`

* Installation automatique des addons

* Mise ÃƒÂ  jour des addons

* Mise ÃƒÂ  jour de tous les addons

* DÃƒÂ©tection des versions locales et distantes

* RÃƒÂ©cupÃƒÂ©ration des mÃƒÂ©tadonnÃƒÂ©es GitHub

* Affichage des auteurs et licences

* Liens directs vers les dÃƒÂ©pÃƒÂ´ts des addons

* VÃƒÂ©rification SHA-256 des fichiers tÃƒÂ©lÃƒÂ©chargÃƒÂ©s

* Retour automatique ÃƒÂ  l'ancienne version en cas d'ÃƒÂ©chec de validation

* Conservation des fichiers locaux qui ne font pas partie d'une mise ÃƒÂ  jour

* Interface franÃƒÂ§aise et anglaise

* Ãƒâ€°lÃƒÂ©vation administrateur uniquement lorsque cela est nÃƒÂ©cessaire



## Fonctionnement



Ebonhold Addon Manager utilise les dÃƒÂ©pÃƒÂ´ts GitHub dÃƒÂ©finis dans `addons.json` comme sources pour les mises ÃƒÂ  jour des addons.



Pour chaque addon, l'application peut dÃƒÂ©terminer :



* S'il est installÃƒÂ©

* Sa version installÃƒÂ©e

* La version distante disponible

* Si une mise ÃƒÂ  jour est disponible

* Le dÃƒÂ©pÃƒÂ´t GitHub correspondant

* L'auteur dÃƒÂ©clarÃƒÂ©

* Les informations de licence disponibles



Lors d'une mise ÃƒÂ  jour, l'application tÃƒÂ©lÃƒÂ©charge le dÃƒÂ©pÃƒÂ´t, valide le contenu de l'addon, prÃƒÂ©pare la nouvelle version puis vÃƒÂ©rifie les fichiers rÃƒÂ©sultants ÃƒÂ  l'aide de leurs empreintes SHA-256.



Si l'installation ÃƒÂ©choue lors de la validation, l'ancienne version est restaurÃƒÂ©e lorsque cela est possible.



### Fichiers locaux



L'application ne supprime pas automatiquement les fichiers qui ne sont pas prÃƒÂ©sents dans le package tÃƒÂ©lÃƒÂ©chargÃƒÂ©.



Ce comportement est volontaire : les dossiers d'addons peuvent contenir des fichiers crÃƒÂ©ÃƒÂ©s par l'utilisateur, des donnÃƒÂ©es de configuration ou d'autres contenus locaux qui ne doivent pas ÃƒÂªtre supprimÃƒÂ©s simplement parce qu'ils ne sont pas prÃƒÂ©sents dans le dÃƒÂ©pÃƒÂ´t source.



## Sources des addons



Les addons gÃƒÂ©rÃƒÂ©s par cette application sont hÃƒÂ©bergÃƒÂ©s et maintenus par leurs auteurs respectifs.



L'application ne revendique aucun droit de propriÃƒÂ©tÃƒÂ© sur les addons tiers.



Les dÃƒÂ©pÃƒÂ´ts, auteurs et informations de licence sont affichÃƒÂ©s dans la section CrÃƒÂ©dits et sont rÃƒÂ©cupÃƒÂ©rÃƒÂ©s depuis les dÃƒÂ©pÃƒÂ´ts correspondants lorsque ces informations sont disponibles.



Lorsqu'un dÃƒÂ©pÃƒÂ´t ne prÃƒÂ©cise pas clairement sa licence, l'application affiche **Ã‚Â« Licence non spÃƒÂ©cifiÃƒÂ©e Ã‚Â»** au lieu de supposer que la redistribution est autorisÃƒÂ©e.



## Installation



### PrÃƒÂ©requis



* Windows 10 ou version ultÃƒÂ©rieure

* Une installation fonctionnelle de Project Ebonhold

* Une connexion Internet pour rÃƒÂ©cupÃƒÂ©rer les informations et les mises ÃƒÂ  jour des addons



### Utilisation d'une release



TÃƒÂ©lÃƒÂ©chargez la derniÃƒÂ¨re version depuis la page GitHub Releases et lancez l'application.



Le gestionnaire tentera automatiquement de dÃƒÂ©tecter votre installation Ebonhold.



Si celle-ci n'est pas dÃƒÂ©tectÃƒÂ©e automatiquement, utilisez **Changer de dossier** pour sÃƒÂ©lectionner le dossier d'installation appropriÃƒÂ©.



### Compilation depuis les sources



Clonez le dÃƒÂ©pÃƒÂ´t :



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



## Catalogue des addons



Le catalogue des addons se trouve dans :



```text

addons.json

```



Chaque entrÃƒÂ©e dÃƒÂ©finit les informations nÃƒÂ©cessaires pour localiser et gÃƒÂ©rer un addon.



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



Le dÃƒÂ©pÃƒÂ´t doit correspondre au dÃƒÂ©pÃƒÂ´t GitHub de l'auteur de l'addon.



La valeur `folder` doit correspondre au dossier de l'addon installÃƒÂ© dans :



```text

Interface/AddOns/

```



## DÃƒÂ©veloppement



Le projet utilise :



* C#

* .NET 8

* Windows Forms

* Des dÃƒÂ©pÃƒÂ´ts d'addons hÃƒÂ©bergÃƒÂ©s sur GitHub



L'objectif est de conserver un projet lÃƒÂ©ger, clair et facile ÃƒÂ  maintenir.



Les contributions, rapports de bugs et suggestions de fonctionnalitÃƒÂ©s sont les bienvenus.



Consultez [CONTRIBUTING.md](CONTRIBUTING.md) pour les rÃƒÂ¨gles de contribution.



## SÃƒÂ©curitÃƒÂ©



Si vous dÃƒÂ©couvrez une vulnÃƒÂ©rabilitÃƒÂ© de sÃƒÂ©curitÃƒÂ©, merci de ne pas la publier directement dans une issue GitHub.



Consultez [SECURITY.md](SECURITY.md) pour connaÃƒÂ®tre la procÃƒÂ©dure de signalement.



## Avertissement



Ebonhold Addon Manager est un outil communautaire dÃƒÂ©veloppÃƒÂ© indÃƒÂ©pendamment.



Il n'est pas affiliÃƒÂ© ÃƒÂ  Project Ebonhold, Blizzard Entertainment ou aux auteurs des addons tiers gÃƒÂ©rÃƒÂ©s par l'application.



Les noms, dÃƒÂ©pÃƒÂ´ts, marques et droits associÃƒÂ©s aux addons tiers restent la propriÃƒÂ©tÃƒÂ© de leurs dÃƒÂ©tenteurs respectifs.



Les utilisateurs sont responsables du respect des licences applicables lors de l'utilisation ou de la redistribution des addons tiers.



## Licence



Ebonhold Addon Manager est distribuÃƒÂ© sous licence MIT.



Consultez [LICENSE](LICENSE) pour le texte complet de la licence.
