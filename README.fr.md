<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. Do not edit generated localized READMEs by hand. -->
<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/repository-banner.png" alt="PermissionScope — See who has access. Understand why." width="100%"></p>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/project-avatar.png" alt="PermissionScope mark" width="72" height="72"></p>

<p align="center">
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml"><img alt="Windows build" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE"><img alt="Apache-2.0" src="https://img.shields.io/badge/license-Apache--2.0-2764E7"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/releases"><img alt="Release" src="https://img.shields.io/github/v/release/MukaSanches/PermissionScope?display_name=tag&color=2764E7"></a>
</p>

<p align="center"><strong>Les autorisations Windows, enfin inspectables.</strong><br>Comprenez qui peut accéder à un dossier et vérifiez les règles qui expliquent la réponse.</p>
<table><tr><td align="center"><a href="https://mukasanches.github.io/PermissionScope/index.fr.html"><strong>Website</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/releases/latest"><strong>Téléchargements de la version</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md"><strong>Trust Center</strong></a></td></tr><tr><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/README.md">Academy</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md">Roadmap</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a></td></tr></table>

<p align="center"><sub>Langues</sub></p>
<table><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.md"><strong>English</strong></a><br><sub>en-US</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.pt-BR.md"><strong>Português</strong></a><br><sub>pt-BR</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.es.md"><strong>Español</strong></a><br><sub>es</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.fr.md"><strong>Français</strong></a><br><sub>fr</sub></td></tr><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.de.md"><strong>Deutsch</strong></a><br><sub>de</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ar.md"><strong>العربية</strong></a><br><sub>ar</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ja.md"><strong>日本語</strong></a><br><sub>ja</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.zh-Hans.md"><strong>简体中文</strong></a><br><sub>zh-Hans</sub></td></tr></table>

---

<table><tr><td width="33%"><strong>Local-first</strong><br><sub>Aucun compte PermissionScope, aucune télémétrie applicative ni service cloud obligatoire.</sub></td><td width="33%"><strong>Décision native Windows</strong><br><sub>L’accès effectif est évalué avec Windows Authz, pas avec une approximation artisanale.</sub></td><td width="33%"><strong>Inconnu reste Inconnu</strong><br><sub>Un contexte manquant n’est jamais converti silencieusement en Autorisé ou Refusé.</sub></td></tr></table>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/fr/access-light.png" alt="LAB\Alex obtient Modification via LAB\Finance. Captures natives d’un scénario synthétique évalué par Authz, sans données d’entreprise réelles ni résultats retouchés." width="94%"></p>

## Commencer en deux minutes

1. Téléchargez l’installateur complet ou le ZIP portable depuis Releases. Choisissez x64 pour Intel/AMD ou ARM64 pour un PC ARM.
2. Installez pour votre compte ou extrayez tout le ZIP et ouvrez PermissionScope.exe.
3. Explorez la démonstration avec LAB\Alex. Pour vos fichiers, choisissez Analyser et indiquez un dossier.
4. Laissez l’identité vide pour utiliser votre jeton Windows actuel. Ouvrez Access Path pour vérifier les preuves.

## Du résultat à la preuve

Windows Authz calcule le masque. Access Path présente les entrées contributrices et les appartenances enregistrées. Un indicateur d’héritage ne prouve pas l’ancêtre d’origine. Le contrôle total dans l’ACL ne garantit pas l’ouverture d’un fichier.

```text
Windows identity
      ↓
SID + recorded membership context
      ↓
ACL / discretionary permission entries
      ↓
Windows Authz evaluation
      ↓
Granted · Partial · Denied · Unknown
      ↓
Access Path → contributing evidence
```

## Lire le résultat

Autorisé permet l’action selon les règles discrétionnaires évaluées. Partiel permet certaines actions. Refusé n’accorde aucun accès. Inconnu signifie que le contexte ne suffit pas à confirmer la réponse ; il ne vaut jamais autorisation.

## Vérifier l’explication

Windows Authz calcule le masque. Access Path présente les entrées contributrices et les appartenances enregistrées. Un indicateur d’héritage ne prouve pas l’ancêtre d’origine. Le contrôle total dans l’ACL ne garantit pas l’ouverture d’un fichier.

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/fr/access-path-light.png" alt="Access Path" width="94%"></p>

## Surface du produit

<table><tr><td><strong>Analyser</strong><br><sub>Inspectez l’accès effectif pour un dossier et une identité.</sub></td><td><strong>Expliquer</strong><br><sub>Suivez Access Path et les preuves contributrices.</sub></td><td><strong>Instantané</strong><br><sub>Enregistrez des observations locales pour les revoir plus tard.</sub></td></tr><tr><td><strong>Comparer</strong><br><sub>Comparez des observations sans supposer que les ressources absentes ont été supprimées.</sub></td><td><strong>Simuler</strong><br><sub>Modélisez le retrait d’une règle en mémoire avant un changement réel.</sub></td><td><strong>Exporter</strong><br><sub>Sorties HTML, CSV, JSON, XLSX et PDF.</sub></td></tr></table>

## Conserver les preuves

Enregistrez des instantanés locaux, comparez les observations, simulez le retrait d’une règle en mémoire et exportez en HTML, CSV, JSON, XLSX ou PDF. Une ressource absente d’une observation ultérieure n’est pas présumée supprimée.

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe explain "C:\Finance"
./permissionscope-cli.exe scan "C:\Finance" --save
./permissionscope-cli.exe export <snapshot-id> --format html --output report.html
```

## Périmètre et confidentialité

Les connexions distantes, contextes S4U, règles conditionnelles et cibles de liens non vérifiées restent Inconnus. Intégrité, chiffrement, verrous et privilèges sont hors périmètre. Sans télémétrie, compte ni service cloud. Les exports réels peuvent contenir des chemins et noms sensibles.

> Analyse et simulation sont en lecture seule. Appliquer est un processus distinct, confirmé explicitement, limité aux fichiers locaux ordinaires, avec instantané, journal durable, vérification et restauration. Dossiers, liens et fichiers à liens physiques multiples sont exclus.

## Télécharger et vérifier

Windows 10 1809 ou ultérieur. Les paquets complets incluent les runtimes. Installateurs non signés : vérifiez SHA-256. ARM64 est compilé en CI, sans certification sur matériel ARM. Consultez l’état pour Store et WinGet.

[Téléchargements de la version](https://github.com/MukaSanches/PermissionScope/releases/latest) · [SHA-256](https://github.com/MukaSanches/PermissionScope/releases/latest) · [État de la version](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)

## Conçu pour être vérifié

| Conçu pour être vérifié | |
|---|---|
| Source | Dépôt public et historique des commits |
| Releases | Artefacts versionnés et sommes SHA-256 |
| Chaîne logicielle | SBOM CycloneDX et automatisations épinglées lorsque documentées |
| Sécurité | Modèle de sécurité, divulgation responsable et CodeQL |
| Plateforme | Chemins de build x64 et ARM64 |
| Documentation | Huit manuels localisés, guides visuels et cours Academy |
| Confidentialité | Conception local-first et consignes explicites sur la sensibilité des exports |

[Ouvrir le Trust Center](https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md)

## Apprenez le modèle, pas seulement les boutons

- [Bases des autorisations](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/fundamentals.md)
- [Lire Access Path](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/access-path.md)
- [Diagnostic sûr](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/troubleshooting.md)
- [Handbooks PDF · 8 languages](https://github.com/MukaSanches/PermissionScope/tree/main/docs/handbooks)

## Choisir la suite

- [Guide visuel](https://github.com/MukaSanches/PermissionScope/blob/main/docs/guides/fr.md)
- [Modèle technique](https://github.com/MukaSanches/PermissionScope/blob/main/docs/access-model.md)
- [Questions et dépannage](https://github.com/MukaSanches/PermissionScope/blob/main/docs/faq.md)
- [Glossaire simple](https://github.com/MukaSanches/PermissionScope/blob/main/docs/glossary.md)
- [Confidentialité](https://github.com/MukaSanches/PermissionScope/blob/main/docs/privacy.md)
- [État de la version](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)
- [Governance](https://github.com/MukaSanches/PermissionScope/blob/main/GOVERNANCE.md)
- [Support](https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md)
- [Citation](https://github.com/MukaSanches/PermissionScope/blob/main/CITATION.cff)

## Limites d’ingénierie

PermissionScope ne prétend pas qu’une ACL discrétionnaire explique tous les résultats possibles d’ouverture de fichier. Intégrité, chiffrement, verrous, certains contextes distants/S4U, règles conditionnelles, privilèges administratifs et cibles reparse non vérifiées peuvent être hors contexte. Ces limites sont documentées au lieu d’être masquées.

## Compiler et tester

Utilisez Windows et le SDK .NET 10. Les tests sont un exécutable d’intégration, pas dotnet test. Consultez le guide de développement pour les paquets et les vérifications documentaires.

[Development](https://github.com/MukaSanches/PermissionScope/blob/main/docs/development.md) · [Benchmarks](https://github.com/MukaSanches/PermissionScope/blob/main/docs/benchmarks.md) · [Roadmap](https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md)

## Aide et contribution

Indiquez version, Windows, opération et code d’erreur. L’onglet Technique copie un diagnostic sans chemins, comptes ni SID. Traductions préliminaires ; les preuves techniques peuvent rester en anglais. Aucune relecture native ni certification d’assistance n’est revendiquée.

---

<p align="center"><sub>Créé par Samuel Sanches · ssanches011@gmail.com · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE">Apache-2.0</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md">Support</a></sub></p>
