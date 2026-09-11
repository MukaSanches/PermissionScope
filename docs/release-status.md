# Release scope

PermissionScope 1.0.0 is already a public historical release and remains immutable. PermissionScope 1.0.1 is the current release line for the security, reliability, accessibility, packaging and website work completed after that publication. A version number is not evidence by itself: the exact tagged commit must satisfy the release contract below.

## Release-readiness contract

Before a new public GitHub release is published, the exact commit must satisfy all applicable automated and manual gates:

- x64 build succeeds and the executable integration harness has zero failures;
- website regression/accessibility checks pass, including zero Axe violations;
- Chromium, Firefox and WebKit compatibility checks are green at desktop, tablet, mobile and short-landscape sizes where applicable;
- localized documentation and resources remain in parity;
- the 72 native application screenshots match the current documentation source fingerprint and are regenerated rather than re-hashed after application-source changes;
- x64 and ARM64 packages are built from the current source fingerprint and project version;
- the x64 installer passes install/CLI/uninstall smoke testing on a disposable Windows runner;
- public release assets have SHA-256 checksums and provenance/SBOM attestations;
- CodeQL is green on the exact release commit;
- publication creates an immutable version tag on the exact validated commit and refuses to overwrite an existing public release or move an existing tag.

Use `build/Test-ReleaseReadiness.ps1` for the local preflight and `docs/release-checklist.md` for the complete go/no-go procedure. A stale native-capture fingerprint is a release blocker, not a warning to bypass.

## Current 1.0.1 checkpoint — 2026-09-10

The 1.0.1 line incorporates the consolidated security/release-readiness work merged through PR #11 plus subsequent PWA identity/offline fixes and release-tooling corrections.

Established automated evidence on the consolidated code line includes:

- x64 Release build and executable integration harness passing;
- website regression/accessibility coverage across localized pages and responsive widths, with zero Axe violations in the recorded run;
- Chromium, Firefox and WebKit compatibility coverage;
- CodeQL coverage on the release line;
- ARM64 build and package paths;
- resource/site validation for eight resource catalogs and eight localized Pages;
- documentation structural/integrity validation for the 72 native captures and locale mappings;
- x64 install, installed-CLI scan and uninstall smoke testing on a disposable Windows runner;
- package provenance plus CycloneDX SBOM generation/attestation support.

The stale-capture blocker from the pre-merge candidate was resolved legitimately on commit `57e64c0993f273ec85c96b7cc892e0bd4f5d2693`. A Windows runner published the current x64 WinUI application and CLI, created the synthetic demo environment, generated the complete native screenshot set from the running application, rebuilt the generated documentation and passed `Verify-Documentation.ps1` before committing the refreshed captures. Native screenshot provenance remains enforced by ordinary CI; future application-source changes must regenerate the captures through the documented Windows procedure.

The release tooling now derives package/installer/source-archive versioning from `Directory.Build.props`, and the CycloneDX document includes the metadata required by GitHub's SBOM attestation action. The publication workflow for 1.0.1 must still finish all exact-commit gates before creating `v1.0.1`.

The existing `v1.0.0` tag and public release are intentionally not moved, rewritten or overwritten.

No Store, WinGet, signing or physical ARM64 certification claim is implied by a GitHub release.

## Implemented product surface

Implemented: native WinUI GUI, shared CLI engine, Authz discretionary decisions, local file ACLs, UNC/mapped-share descriptors, conservative remote projections, token and local-group evidence, LDAP group traversal, primary-group/disabled/SID-history metadata when available, explicit Unknown states, findings, snapshots, comparison, in-memory ACE removal, guarded local-file changes and rollback, HTML/CSV/JSON/XLSX/PDF exports, daily interactive-user Task Scheduler integration, English and Brazilian Portuguese UI.

## Current boundaries

- No complete expansion of every ACL group into every person. Results identify ACL principals and evaluate one selected context per scan.
- Remote logon equivalence, trusted-domain coverage, DFS target discovery/divergence and central access policy are not implemented as certified effective-access results. UNC projections remain Unknown.
- Inherited ACEs retain Windows flags; the exact ancestor and depth are not established. Group membership range continuation and trust referrals are not complete.
- Simulation removes one ACE for the selected context. GUI addition/editing, group changes and descendant propagation are not included. Actual remediation is restricted to ordinary local files.
- Scans materialize their results. There is no resumable million-object persistent stream. A worker terminated by cancellation does not save unfinished observations.
- PDF direct export supports Latin, Greek and Cyrillic text. Complex scripts, CJK and emoji require the HTML report and the browser's Print to PDF. XLSX/JSON/HTML preserve Unicode.
- Comparison shows descriptor and evaluated-context changes where comparable; it does not calculate the impact on every member of a changed group.
- Scheduling is daily and uses the signed-in user's session. Weekly/custom schedules and retention can be configured in Windows Task Scheduler; no scheduler-management UI is included.
- English and Brazilian Portuguese catalogs ship alongside six explicitly labeled translation previews. Regional/script fallback and RTL culture resolution are tested. Technical evidence/report translation, exhaustive high-contrast testing, screen-reader certification, full worldwide translation and all requested identity filters remain incomplete. See `translation-quality.md` for coverage and review status.
- Direct-distribution artifacts are unsigned. MSIX requires a real publisher identity and signing certificate or Store ingestion. No Store/WinGet approval is implied.
- ARM64 is cross-built in CI. Physical ARM hardware execution is not certified until a representative ARM device has been tested and recorded.

## Distribution claims

A green GitHub release pipeline establishes only the repository's direct-distribution evidence for that tagged commit. It does not establish Microsoft Store certification, Windows publisher reputation, SmartScreen reputation or WinGet acceptance.

The website and README must continue to state that direct installers are unsigned until signing is actually established. Store and WinGet availability must be reported only after those channels are live.

Unknown results preserve the available evidence and explain which context is missing. Validate domain and server behavior in a representative environment before using the application as an enterprise audit authority.
