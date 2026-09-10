# Contributing to Ebonhold Addon Manager

**English** | [Français](#contribuer-à-ebonhold-addon-manager)

Thank you for your interest in contributing. Contributions, bug reports, documentation improvements and feature suggestions are welcome.

## Submitting an addon

The easiest way to get an addon into the catalogue is the **Propose an addon** button in the app, or the [addon submission issue form](../../issues/new?template=addon_submission.yml). Provide the addon name, its GitHub repository (`owner/repository`), the addon folder name (it must match the `.toc` file name) and the branch.

Submissions are reviewed before being added. Once approved, the addon is added to `addons.json` automatically and appears in the app on the next launch — no new release is required.

## Before contributing

Please check the existing issues and pull requests first. For significant changes, opening an issue to discuss the approach is recommended.

## Reporting bugs

Please provide: a clear description, steps to reproduce, expected vs. actual behaviour, your Windows version, the Ebonhold version when relevant, and any error messages or screenshots. Do not include personal information or credentials.

## Feature requests

Explain what problem the feature solves, how you expect it to work, and why it is useful. The project aims to remain lightweight and focused on addon management.

## Pull requests

1. Keep changes focused on the proposed feature or fix; avoid unrelated changes.
2. Make sure the project builds (`dotnet build`).
3. Test the affected functionality when possible.
4. Update the documentation when necessary.
5. Keep third-party addon source code out of this repository.

PR descriptions should explain what changed and why.

## Code style

* Prefer clear, descriptive names.
* Keep methods focused on a single responsibility.
* Avoid unnecessary dependencies.
* Handle errors explicitly where appropriate.
* Avoid hard-coded user-specific paths.
* Do not commit generated build files.
* Code, comments and identifiers are written in English.

## Third-party addons

The manager references third-party addons hosted in their own repositories. Do not copy third-party addon source code into this repository unless there is a clear legal basis. When modifying `addons.json`, make sure the repository information is accurate and points to the correct upstream repository.

## Security issues

Do not report security vulnerabilities through public GitHub issues. See [SECURITY.md](SECURITY.md).

## License

By contributing, you agree that your contributions may be distributed under the project's [MIT License](LICENSE).

---

<a id="contribuer-à-ebonhold-addon-manager"></a>

# Contribuer à Ebonhold Addon Manager

[English](#contributing-to-ebonhold-addon-manager) | **Français**

Merci de ton intérêt pour contribuer. Les contributions, rapports de bugs, améliorations de la documentation et suggestions de fonctionnalités sont les bienvenus.

## Proposer un addon

Le plus simple pour ajouter un addon au catalogue est le bouton **Proposer un addon** dans l'app, ou le [formulaire de soumission d'addon](../../issues/new?template=addon_submission.yml). Indique le nom de l'addon, son dépôt GitHub (`propriétaire/dépôt`), le nom du dossier de l'addon (il doit correspondre au nom du fichier `.toc`) et la branche.

Les soumissions sont examinées avant d'être ajoutées. Une fois approuvé, l'addon est ajouté à `addons.json` automatiquement et apparaît dans l'app au lancement suivant — aucune nouvelle release nécessaire.

## Avant de contribuer

Vérifie d'abord les issues et pull requests existantes. Pour des changements importants, ouvrir une issue pour discuter de l'approche est recommandé.

## Signaler un bug

Fournis : une description claire, les étapes pour reproduire, le comportement attendu vs. constaté, ta version de Windows, la version d'Ebonhold si pertinent, et tout message d'erreur ou capture d'écran. N'inclus pas d'informations personnelles ni d'identifiants.

## Demandes de fonctionnalités

Explique quel problème la fonctionnalité résout, comment tu l'imagines, et pourquoi elle est utile. Le projet vise à rester léger et centré sur la gestion d'addons.

## Pull requests

1. Garde les changements centrés sur la fonctionnalité ou le correctif ; évite les modifications sans rapport.
2. Assure-toi que le projet compile (`dotnet build`).
3. Teste la fonctionnalité concernée quand c'est possible.
4. Mets à jour la documentation si nécessaire.
5. Ne mets pas le code source d'addons tiers dans ce dépôt.

Les descriptions de PR doivent expliquer ce qui change et pourquoi.

## Style de code

* Privilégie des noms clairs et descriptifs.
* Garde les méthodes centrées sur une seule responsabilité.
* Évite les dépendances inutiles.
* Gère les erreurs explicitement le cas échéant.
* Évite les chemins codés en dur spécifiques à un utilisateur.
* Ne commite pas les fichiers de build générés.
* Le code, les commentaires et les identifiants sont écrits en anglais.

## Addons tiers

Le gestionnaire référence des addons tiers hébergés dans leurs propres dépôts. Ne copie pas le code source d'addons tiers dans ce dépôt sans base légale claire. En modifiant `addons.json`, assure-toi que les informations de dépôt sont exactes et pointent vers le bon dépôt source.

## Problèmes de sécurité

Ne signale pas de failles de sécurité via des issues GitHub publiques. Voir [SECURITY.md](SECURITY.md).

## Licence

En contribuant, tu acceptes que tes contributions puissent être distribuées sous la [licence MIT](LICENSE) du projet.
