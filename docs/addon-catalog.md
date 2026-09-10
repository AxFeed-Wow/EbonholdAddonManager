# Addon Catalog

**English** | [Français](#catalogue-daddons)

## Overview

Ebonhold Addon Manager uses `addons.json` as its addon catalogue. It references third-party addon repositories; it does not contain or redistribute the addon source code.

The application loads the catalogue **live from the project repository** (with a local cache and the bundled file as an offline fallback), so an approved addon reaches everyone on the next launch without a new release.

## File format

`addons.json` is a JSON array of addon definitions:

```json
{
  "id": "ebon-affix-alert",
  "name": "Ebon Affix Alert",
  "folder": "EbonAffixAlert",
  "repository": "Kebbie/EbonAffixAlert",
  "branch": "main",
  "preferRelease": true
}
```

### Fields

* **`id`** — stable internal identifier (kebab-case). Do not change it without reason.
* **`name`** — display name shown to the user.
* **`folder`** — the addon directory name under `Interface/AddOns/`. **It must equal the `.toc` file name** (without the extension). Version detection and installation rely on this.
* **`repository`** — GitHub repository in `owner/repository` form (the upstream project).
* **`branch`** — the branch to retrieve (`main`, `master`, `develop`, …).
* **`preferRelease`** — reserved; kept for compatibility.

## How an addon gets added

The easiest path is the **Propose an addon** button (or the submission issue form). A maintainer reviews the request and applies the `approved` label; a GitHub Action then validates it and commits the entry automatically. Validation checks that:

1. The repository exists and is public (not archived).
2. `<folder>.toc` resolves at the repository root or in a same-named subfolder.
3. No existing entry already uses the same `folder` or `repository`.

You can also edit `addons.json` directly in a pull request. Before adding an addon, also confirm the license and attribution, and that the addon is relevant to Project Ebonhold.

## Third-party ownership

The referenced addons are third-party projects. Their source code, names, trademarks and licenses remain the property of their respective authors. Adding an addon does not imply endorsement, and if a repository does not clearly specify a license, do not assume redistribution is permitted.

## When an addon stops working

Upstream repositories can change ownership, branch, folder structure or move entirely. If an addon breaks: verify the upstream repository, check whether the branch or folder changed, and update `addons.json` only once the new information is verified. Do not silently replace an addon with an unrelated project using a similar name.

---

<a id="catalogue-daddons"></a>

# Catalogue d'addons

[English](#addon-catalog) | **Français**

## Vue d'ensemble

Ebonhold Addon Manager utilise `addons.json` comme catalogue d'addons. Il référence des dépôts d'addons tiers ; il ne contient ni ne redistribue le code source des addons.

L'application charge le catalogue **en direct depuis le dépôt du projet** (avec un cache local et le fichier embarqué en secours hors ligne), donc un addon approuvé arrive chez tout le monde au lancement suivant, sans nouvelle release.

## Format du fichier

`addons.json` est un tableau JSON de définitions d'addons :

```json
{
  "id": "ebon-affix-alert",
  "name": "Ebon Affix Alert",
  "folder": "EbonAffixAlert",
  "repository": "Kebbie/EbonAffixAlert",
  "branch": "main",
  "preferRelease": true
}
```

### Champs

* **`id`** — identifiant interne stable (kebab-case). Ne le change pas sans raison.
* **`name`** — nom affiché à l'utilisateur.
* **`folder`** — le nom du dossier de l'addon sous `Interface/AddOns/`. **Il doit être identique au nom du fichier `.toc`** (sans l'extension). La détection de version et l'installation en dépendent.
* **`repository`** — dépôt GitHub au format `propriétaire/dépôt` (le projet source).
* **`branch`** — la branche à récupérer (`main`, `master`, `develop`, …).
* **`preferRelease`** — réservé ; conservé pour compatibilité.

## Comment un addon est ajouté

Le plus simple est le bouton **Proposer un addon** (ou le formulaire de soumission). Un mainteneur examine la demande et applique le label `approved` ; une GitHub Action la valide alors et commit l'entrée automatiquement. La validation vérifie que :

1. Le dépôt existe et est public (non archivé).
2. `<folder>.toc` se résout à la racine du dépôt ou dans un sous-dossier du même nom.
3. Aucune entrée existante n'utilise déjà le même `folder` ou `repository`.

Tu peux aussi éditer `addons.json` directement dans une pull request. Avant d'ajouter un addon, confirme aussi la licence et l'attribution, et que l'addon est pertinent pour Project Ebonhold.

## Propriété des tiers

Les addons référencés sont des projets tiers. Leur code source, noms, marques et licences restent la propriété de leurs auteurs respectifs. Ajouter un addon n'implique aucune approbation, et si un dépôt ne spécifie pas clairement de licence, ne suppose pas que la redistribution est permise.

## Quand un addon cesse de fonctionner

Les dépôts sources peuvent changer de propriétaire, de branche, de structure de dossier ou déménager entièrement. Si un addon casse : vérifie le dépôt source, regarde si la branche ou le dossier a changé, et ne mets à jour `addons.json` qu'une fois la nouvelle information vérifiée. Ne remplace pas silencieusement un addon par un projet sans rapport portant un nom similaire.
