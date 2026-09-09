# Architecture



## Overview



Ebonhold Addon Manager is a Windows desktop application built with C# and .NET 8 using Windows Forms.



The application is organized into a small set of models, services and UI forms. The goal is to keep addon management logic separate from the user interface while keeping the project lightweight and easy to maintain.



## Project structure



```text

EbonholdAddonManager/

â”œâ”€â”€ Models/

â”‚   â”œâ”€â”€ AddonDefinition.cs

â”‚   â”œâ”€â”€ AddonInfo.cs

â”‚   â””â”€â”€ RepositoryMetadata.cs

â”œâ”€â”€ Services/

â”‚   â”œâ”€â”€ AddonManagerService.cs

â”‚   â”œâ”€â”€ AddonUpdater.cs

â”‚   â”œâ”€â”€ AdminService.cs

â”‚   â”œâ”€â”€ CatalogService.cs

â”‚   â”œâ”€â”€ GitHubService.cs

â”‚   â”œâ”€â”€ InstallationDetector.cs

â”‚   â”œâ”€â”€ LocalizationService.cs

â”‚   â”œâ”€â”€ SettingsService.cs

â”‚   â””â”€â”€ TocReader.cs

â”œâ”€â”€ .github/

â”œâ”€â”€ docs/

â”œâ”€â”€ addons.json

â”œâ”€â”€ CreditsForm.cs

â”œâ”€â”€ MainForm.cs

â””â”€â”€ Program.cs

```



## Main components



### `MainForm`



The main Windows Forms interface.



Responsibilities include:



* Displaying the addon list

* Showing local and remote versions

* Starting addon installation and updates

* Refreshing addon information

* Displaying progress and status messages

* Managing language selection

* Opening addon repository information



The form delegates addon management operations to the service layer.



### `AddonManagerService`



Coordinates addon-related operations.



Responsibilities include:



* Loading addon definitions

* Scanning installed addons

* Comparing local and remote versions

* Determining whether an addon can be installed or updated

* Calling the updater when an installation or update is required



### `AddonUpdater`



Handles the actual addon installation and update process.



The update process is designed to be conservative:



1\. Download the selected repository archive.

2\. Extract the archive into a temporary directory.

3\. Locate and validate the addon package.

4\. Prepare the new installation.

5\. Validate package files using SHA-256 hashes.

6\. Create a backup of the current installation.

7\. Replace the addon files.

8\. Validate the resulting installation.

9\. Roll back when an operation fails.



Files that already exist locally but are not part of the downloaded addon package are preserved rather than automatically deleted.



### `GitHubService`



Provides access to addon repository information hosted on GitHub.



It is responsible for:



* Retrieving repository metadata

* Reading addon repository files

* Downloading repository archives

* Retrieving release information when configured

* Providing author, license and repository information



The service uses the repository definitions provided by `addons.json`.



### `InstallationDetector`



Attempts to locate the Ebonhold installation automatically.



If automatic detection does not find a valid installation, the application can allow the user to select the installation directory manually.



### `CatalogService`



Loads the addon catalog from `addons.json`.



The catalog defines information such as:



* Addon identifier

* Display name

* Local addon folder

* GitHub repository

* Branch

* Release preference



Keeping this information outside the application code makes it possible to update the addon catalog without modifying the management logic.



### `TocReader`



Reads World of Warcraft addon `.toc` files.



The service is used to retrieve addon version information from the local installation and downloaded addon packages.



### `AdminService`



Handles elevation to administrator privileges when an operation requires permissions that the current process does not have.



The application does not run permanently with administrator privileges.



### `SettingsService`



Stores user preferences used by the application, such as the selected language and installation path.



### `LocalizationService`



Provides the localized strings used by the user interface.



The application currently supports:



* English

* French



### `CreditsForm`



Displays addon information such as:



* Author

* License

* Repository

* Direct GitHub link



It also makes the distinction between the manager itself and third-party addons explicit.



## Data flow



A typical addon update follows this flow:



```text

MainForm

&#x20;  â”‚

&#x20;  â–¼

AddonManagerService

&#x20;  â”‚

&#x20;  â”œâ”€â”€â–º CatalogService

&#x20;  â”‚

&#x20;  â”œâ”€â”€â–º InstallationDetector

&#x20;  â”‚

&#x20;  â”œâ”€â”€â–º TocReader

&#x20;  â”‚

&#x20;  â””â”€â”€â–º GitHubService

&#x20;            â”‚

&#x20;            â–¼

&#x20;       Remote addon data

&#x20;            â”‚

&#x20;            â–¼

&#x20;     AddonManagerService

&#x20;            â”‚

&#x20;            â–¼

&#x20;       AddonUpdater

&#x20;            â”‚

&#x20;            â”œâ”€â”€â–º Download

&#x20;            â”œâ”€â”€â–º Validate

&#x20;            â”œâ”€â”€â–º Backup

&#x20;            â”œâ”€â”€â–º Install

&#x20;            â””â”€â”€â–º Rollback on failure

```



## Configuration



Addon definitions are stored in `addons.json`.



The application does not embed third-party addon source code in the repository. The catalog only contains the information required to locate and manage the upstream addon repositories.



## Design principles



The project follows a few simple principles:



* Keep the UI separate from addon management logic.

* Prefer explicit validation over assumptions.

* Preserve existing local files whenever possible.

* Avoid unnecessary API requests.

* Do not require administrator privileges unless necessary.

* Keep third-party addon code outside this repository.

* Keep the application independent from Project Ebonhold and addon authors.

* Prefer simple and maintainable implementations over unnecessary complexity.
