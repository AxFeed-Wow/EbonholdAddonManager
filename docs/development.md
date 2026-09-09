\# Development Guide



\## Requirements



To build Ebonhold Addon Manager locally, you need:



\* Windows

\* .NET 8 SDK or a compatible newer .NET SDK

\* Git

\* Visual Studio 2022 or another C#/.NET development environment



The project targets:



```text

net8.0-windows

```



and uses Windows Forms.



\## Clone the repository



Clone the repository and enter the project directory:



```powershell

git clone https://github.com/AxFeed-Wow/EbonholdAddonManager.git

cd EbonholdAddonManager

```



\## Build



Restore dependencies and build the solution:



```powershell

dotnet restore EbonholdAddonManager.slnx

dotnet build EbonholdAddonManager.slnx

```



For a Release build:



```powershell

dotnet build EbonholdAddonManager.slnx --configuration Release

```



\## Run



The application can be started through Visual Studio or with:



```powershell

dotnet run --project EbonholdAddonManager.csproj

```



\## Project organization



Application logic is separated into:



\* `Models/` — data models

\* `Services/` — addon management and supporting services

\* `MainForm.cs` — main user interface

\* `CreditsForm.cs` — addon information and credits

\* `Program.cs` — application entry point

\* `addons.json` — addon catalog



See \[Architecture](architecture.md) for a more detailed description.



\## Working with the addon catalog



Addon definitions are stored in `addons.json`.



When adding or modifying an entry:



1\. Verify the upstream repository.

2\. Verify the branch.

3\. Verify the addon directory name.

4\. Check that the repository contains a valid `.toc` file.

5\. Check the upstream license and attribution requirements.

6\. Test installation or update behavior when possible.



See \[Addon Catalog](addon-catalog.md) for the complete catalog documentation.



\## Testing changes



Before submitting a pull request:



```powershell

dotnet restore EbonholdAddonManager.slnx

dotnet build EbonholdAddonManager.slnx --configuration Release

```



When changing addon installation or update logic, test both successful and failure scenarios where possible.



In particular, changes to the updater should be checked for:



\* New addon installation

\* Existing addon update

\* Version detection

\* Package validation

\* Installation validation

\* Rollback after failure

\* Preservation of local files

\* Permission-related failures



\## Generated files



Do not commit generated build output.



The following directories are ignored by Git:



```text

bin/

obj/

.vs/

```



User-specific Visual Studio files should also remain untracked.



\## Coding guidelines



Keep changes focused and avoid unrelated refactoring.



Prefer:



\* Clear and descriptive names

\* Small, focused methods

\* Explicit error handling

\* Existing project patterns

\* Minimal dependencies



Avoid:



\* Hard-coded user-specific paths

\* Unnecessary external dependencies

\* Silent exception handling

\* Committing generated files

\* Copying third-party addon source code into the repository



\## Pull requests



Before opening a pull request:



1\. Build the project successfully.

2\. Test the affected functionality.

3\. Review the changes with `git diff`.

4\. Update documentation when necessary.

5\. Explain the purpose of the change in the pull request description.



See \[CONTRIBUTING.md](../CONTRIBUTING.md) for contribution guidelines.



\## GitHub Actions



The repository contains a GitHub Actions workflow under:



```text

.github/workflows/build.yml

```



The workflow restores and builds the solution on pushes and pull requests.



A pull request should therefore keep the project in a buildable state.



\## Third-party code



Do not copy third-party addon source code into this repository unless its inclusion is explicitly permitted and appropriate.



The manager is designed to retrieve addons from their upstream repositories.



Third-party addon licenses remain separate from the MIT license of Ebonhold Addon Manager.
