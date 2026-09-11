<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. Do not edit generated localized READMEs by hand. -->
<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/repository-banner.png" alt="PermissionScope — See who has access. Understand why." width="100%"></p>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/project-avatar.png" alt="PermissionScope mark" width="72" height="72"></p>

<p align="center">
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml"><img alt="Windows build" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE"><img alt="Apache-2.0" src="https://img.shields.io/badge/license-Apache--2.0-2764E7"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/releases"><img alt="Release" src="https://img.shields.io/github/v/release/MukaSanches/PermissionScope?display_name=tag&color=2764E7"></a>
</p>

<p align="center"><strong>Windows permissions, made inspectable.</strong><br>Understand who can access a folder — and inspect the rules behind the answer.</p>
<table><tr><td align="center"><a href="https://mukasanches.github.io/PermissionScope/index.html"><strong>Website</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/releases/latest"><strong>Release downloads</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md"><strong>Trust Center</strong></a></td></tr><tr><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/README.md">Academy</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md">Roadmap</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a></td></tr></table>

<p align="center"><sub>Languages</sub></p>
<table><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.md"><strong>English</strong></a><br><sub>en-US</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.pt-BR.md"><strong>Português</strong></a><br><sub>pt-BR</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.es.md"><strong>Español</strong></a><br><sub>es</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.fr.md"><strong>Français</strong></a><br><sub>fr</sub></td></tr><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.de.md"><strong>Deutsch</strong></a><br><sub>de</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ar.md"><strong>العربية</strong></a><br><sub>ar</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ja.md"><strong>日本語</strong></a><br><sub>ja</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.zh-Hans.md"><strong>简体中文</strong></a><br><sub>zh-Hans</sub></td></tr></table>

---

<table><tr><td width="33%"><strong>Local-first</strong><br><sub>No PermissionScope account, application telemetry or required cloud service.</sub></td><td width="33%"><strong>Windows-native decision</strong><br><sub>Effective access is evaluated with Windows Authz instead of a hand-written approximation.</sub></td><td width="33%"><strong>Unknown stays Unknown</strong><br><sub>Missing context is never silently converted into Granted or Denied.</sub></td></tr></table>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/en-US/access-light.png" alt="LAB\Alex receives Modify through LAB\Finance. These are native application captures using a synthetic Authz fixture, not a real company or altered results." width="94%"></p>

## Start in two minutes

1. Download the complete installer or portable ZIP from Releases. Choose x64 for Intel/AMD or ARM64 for an ARM PC.
2. Install for your account, or extract the entire ZIP and open PermissionScope.exe.
3. Choose Explore the demo to learn safely with LAB\Alex. For your files, choose Analyze and enter a folder.
4. Leave the identity blank to use your current Windows token. Open Access Path to inspect the evidence.

## From result to evidence

Windows Authz calculates the mask. Access Path shows contributing entries and recorded membership evidence. Inherited flags do not prove the originating ancestor. Full control in the discretionary ACL does not guarantee a successful file open.

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

## Read the result

Granted allows the named action under the evaluated discretionary rules. Partial means only some actions are allowed. Denied means those rules grant no access. Unknown means the available context cannot confirm the answer; it is never treated as granted.

## Verify the explanation

Windows Authz calculates the mask. Access Path shows contributing entries and recorded membership evidence. Inherited flags do not prove the originating ancestor. Full control in the discretionary ACL does not guarantee a successful file open.

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/en-US/access-path-light.png" alt="Access Path" width="94%"></p>

## Product surface

<table><tr><td><strong>Analyze</strong><br><sub>Inspect effective access for a folder and identity.</sub></td><td><strong>Explain</strong><br><sub>Follow Access Path and contributing evidence.</sub></td><td><strong>Snapshot</strong><br><sub>Save local observations for later review.</sub></td></tr><tr><td><strong>Compare</strong><br><sub>Compare observations without assuming missing resources were deleted.</sub></td><td><strong>Simulate</strong><br><sub>Model removal of a rule in memory before considering a real change.</sub></td><td><strong>Export</strong><br><sub>HTML, CSV, JSON, XLSX and PDF outputs.</sub></td></tr></table>

## Keep the evidence

Save local snapshots, compare observations, simulate removing a permission entry in memory, and export HTML, CSV, JSON, XLSX or PDF. Missing resources in a later observation are not assumed deleted.

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe explain "C:\Finance"
./permissionscope-cli.exe scan "C:\Finance" --save
./permissionscope-cli.exe export <snapshot-id> --format html --output report.html
```

## Scope and privacy

Remote logons, S4U contexts, conditional rules and unverified reparse targets remain Unknown. Integrity policy, encryption, locks and administrative privileges are outside this decision. No application telemetry, accounts or cloud service. Paths and account names in real exports can be sensitive.

> Analysis and simulation are read only. Applying a change is a separate, explicitly confirmed workflow for ordinary local files, with a saved observation, durable journal, verification and rollback. Directories, links and multi-link files are excluded.

## Download and verify

Windows 10 1809 or later. Complete packages include their runtimes. Installers are currently unsigned; verify SHA-256 against the release checksums. ARM64 is cross-built in CI; physical ARM execution is not certified. Store and WinGet availability must be checked in release status.

[Release downloads](https://github.com/MukaSanches/PermissionScope/releases/latest) · [SHA-256 checksums](https://github.com/MukaSanches/PermissionScope/releases/latest/download/SHA256SUMS.txt) · [Release status](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)

## Built for verification

| Built for verification | |
|---|---|
| Source | Public repository and commit history |
| Releases | Versioned artifacts and SHA-256 checksums |
| Supply chain | CycloneDX SBOM and pinned automation where documented |
| Security | Security model, disclosure guidance and CodeQL workflow |
| Platform | x64 and ARM64 build paths |
| Documentation | Eight localized handbooks, visual guides and Academy courses |
| Privacy | Local-first design and explicit export sensitivity guidance |

[Open the Trust Center](https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md)

## Learn the model, not just the buttons

- [Permission fundamentals](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/fundamentals.md)
- [Reading Access Path](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/access-path.md)
- [Safe troubleshooting](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/troubleshooting.md)
- [Handbooks PDF · 8 languages](https://github.com/MukaSanches/PermissionScope/tree/main/docs/handbooks)

## Choose your next step

- [Visual walkthrough](https://github.com/MukaSanches/PermissionScope/blob/main/docs/guides/en-US.md)
- [Technical model](https://github.com/MukaSanches/PermissionScope/blob/main/docs/access-model.md)
- [FAQ and troubleshooting](https://github.com/MukaSanches/PermissionScope/blob/main/docs/faq.md)
- [Plain-language glossary](https://github.com/MukaSanches/PermissionScope/blob/main/docs/glossary.md)
- [Privacy](https://github.com/MukaSanches/PermissionScope/blob/main/docs/privacy.md)
- [Release status](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)
- [Governance](https://github.com/MukaSanches/PermissionScope/blob/main/GOVERNANCE.md)
- [Support](https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md)
- [Citation](https://github.com/MukaSanches/PermissionScope/blob/main/CITATION.cff)

## Engineering boundaries

PermissionScope does not pretend discretionary ACL evaluation explains every possible file-open outcome. Integrity policy, encryption, locks, some remote/S4U contexts, conditional rules, administrative privilege effects and unverified reparse targets can be outside the available decision context. Those limits are documented instead of hidden.

## Build and test

Use Windows and .NET 10 SDK. Tests are an executable integration harness, not dotnet test. See the development guide for packaging and documentation verification.

[Development](https://github.com/MukaSanches/PermissionScope/blob/main/docs/development.md) · [Benchmarks](https://github.com/MukaSanches/PermissionScope/blob/main/docs/benchmarks.md) · [Roadmap](https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md)

## Help and contribution

Include the version, Windows version, operation and error code. The Technical tab can copy a diagnostic without paths, account names or SIDs. Language packs are previews; technical evidence can remain in English. Native speaker review and assistive-technology certification are not claimed.

---

<p align="center"><sub>Created by Samuel Sanches · ssanches011@gmail.com · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE">Apache-2.0</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md">Support</a></sub></p>
