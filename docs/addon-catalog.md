\# Addon Catalog



\## Overview



Ebonhold Addon Manager uses `addons.json` as its addon catalog.



The catalog contains references to third-party addon repositories. It does not contain or redistribute the addon source code itself.



Each catalog entry describes where an addon can be found and how the manager should retrieve it.



\## File format



`addons.json` contains a JSON array of addon definitions.



Example:



```json

{

&#x20; "id": "example-addon",

&#x20; "name": "Example Addon",

&#x20; "folder": "ExampleAddon",

&#x20; "repository": "Author/ExampleAddon",

&#x20; "branch": "main",

&#x20; "preferRelease": true

}

```



\## Fields



\### `id`



Unique identifier used internally by the application.



It should be stable and should not be changed without a good reason.



Example:



```json

"id": "ebon-affix-alert"

```



\### `name`



Display name shown to the user.



Example:



```json

"name": "Ebon Affix Alert"

```



\### `folder`



Name of the addon directory inside the Ebonhold installation.



Example:



```json

"folder": "EbonAffixAlert"

```



This value should correspond to the directory created by the addon itself.



\### `repository`



GitHub repository in the `owner/repository` format.



Example:



```json

"repository": "Kebbie/EbonAffixAlert"

```



The repository should point to the upstream project maintained by the addon author or maintainer.



\### `branch`



Git branch used when retrieving the addon source.



Example:



```json

"branch": "main"

```



Use the branch actually maintained by the upstream repository.



\### `preferRelease`



Determines whether the manager should prefer a published GitHub release when one is available.



Example:



```json

"preferRelease": true

```



\## Adding an addon



Before adding an addon to the catalog, verify:



1\. The repository exists and is publicly accessible.

2\. The repository belongs to the appropriate addon author or maintainer.

3\. The correct branch is configured.

4\. The addon folder name is correct.

5\. The repository contains a valid WoW addon `.toc` file.

6\. The repository license and attribution information are respected.

7\. The addon is actually relevant to Project Ebonhold.



Example:



```json

{

&#x20; "id": "my-addon",

&#x20; "name": "My Addon",

&#x20; "folder": "MyAddon",

&#x20; "repository": "Author/MyAddon",

&#x20; "branch": "main",

&#x20; "preferRelease": true

}

```



\## Third-party ownership



The addons referenced by this catalog are third-party projects.



Their source code, names, trademarks, licenses and other intellectual property remain the property of their respective authors or rights holders.



Adding an addon to this catalog does not mean that the addon author endorses Ebonhold Addon Manager.



Ebonhold Addon Manager is an independent project and is not affiliated with Project Ebonhold unless explicitly stated otherwise by the relevant parties.



\## Licensing



The license of Ebonhold Addon Manager applies only to this manager's own source code.



It does not apply to third-party addons referenced by `addons.json`.



Users and contributors should consult the upstream repository for the applicable license and redistribution terms of each addon.



If an addon does not clearly specify a license, contributors should not assume that its source code can be redistributed.



\## Repository changes



Upstream repositories can change ownership, branch names, addon folder structures or distribution methods.



If an addon stops working:



\* Verify the upstream repository first.

\* Check whether the branch has changed.

\* Check whether the addon folder has changed.

\* Check whether the addon has moved to another repository.

\* Update `addons.json` only when the new information has been verified.



Do not silently replace an addon with an unrelated project using the same or a similar name.



\## Pull requests



Pull requests that modify `addons.json` should explain:



\* Which addon was added or changed

\* The upstream repository

\* Why the addon is relevant

\* Any relevant license or attribution information

\* Any special installation requirements



Avoid adding large numbers of unverified repositories at once.
