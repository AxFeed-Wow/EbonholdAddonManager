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
  "id": "example-addon",
  "name": "Example Addon",
  "folder": "ExampleAddon",
  "repository": "Author/Repository",
  "branch": "main",
  "preferRelease": true
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
