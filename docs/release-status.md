# Release scope

PermissionScope 1.0.0 is treated as a release candidate until the exact publication commit passes the project release contract. The version number is not evidence by itself.

## Release-readiness contract

Before a public GitHub release is published, the exact commit must satisfy all applicable automated and manual gates:

- x64 build succeeds and the executable integration harness has zero failures;
- website regression/accessibility checks pass, including zero Axe violations;
- Chromium, Firefox and WebKit compatibility checks are green at desktop, tablet, mobile and short-landscape sizes;
- localized documentation and resources remain in parity;
- the 72 native application screenshots match the current documentation source fingerprint and have been regenerated rather than re-hashed after source changes;
- x64 and ARM64 packages are built from the current source fingerprint;
- the x64 installer passes install/CLI/uninstall smoke testing on a disposable Windows runner;
- public release assets have SHA-256 checksums and provenance attestations;
- CodeQL is green on the release commit;
- publication is performed from the immutable `v1.0.0` tag through the guarded release workflow.

Use `build/Test-ReleaseReadiness.ps1` for the local preflight and `docs/release-checklist.md` for the complete go/no-go procedure. A stale native-capture fingerprint is a release blocker, not a warning to bypass.

## Current 1.0.0 release-candidate checkpoint — 2026-09-10

The consolidated candidate was validated on commit `7d03f5da7bd2193d597d2213fa68af14e2060134` before this status-only documentation update.

Validated evidence:

- x64 Release build succeeded with 0 warnings and 0 errors;
- executable integration harness completed with 88 passed and 0 failed;
- website regression/accessibility passed at 1440, 768 and 375 pixel widths across all eight site locales;
- Axe reported 0 violations; keyboard skip link, forced-colors behavior and external-request blocking passed;
- universal Chromium, Firefox and WebKit compatibility passed;
- CodeQL completed successfully;
- ARM64 Release build, packaging and artifact upload completed successfully;
- resource/site validation passed for eight resource catalogs and eight localized Pages;
- documentation structural validation passed for 55 documents, 72 image hashes, eight locale mappings and the synthetic SID allowlist.

The remaining release blocker is deliberate: the native screenshot provenance gate reports the existing capture set as stale after application-source changes. The first reported stale capture is `ar/access-dark.png`. The 72 native captures must be regenerated from the current application on an interactive Windows desktop and visually reviewed. The capture manifest fingerprint must not be advanced manually to bypass this check.

Until that evidence is refreshed and the x64 workflow proceeds through package/install/CLI/uninstall validation, this state is a release candidate and must not be tagged or published as the final `v1.0.0` release.

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
- English and Brazilian Portuguese catalogs ship alongside six explicitly labeled translation previews. Regional/script fallback and RTL culture resolution are tested. Technical evidence/report translation, exhaustive high-contrast testing, screen-reader certification, full worldwide translation and all requested identity filters remain incomplete. See translation-quality.md for coverage and review status.
- Direct-distribution artifacts are unsigned. MSIX requires a real publisher identity and signing certificate or Store ingestion. No Store/WinGet submission has been performed.
- ARM64 is cross-built in CI. Physical ARM hardware execution is not certified until a representative ARM device has been tested and recorded.

## Distribution claims

A green GitHub release pipeline establishes only the repository's direct-distribution evidence for that tagged commit. It does not establish Microsoft Store certification, Windows publisher reputation, SmartScreen reputation or WinGet acceptance.

The website and README must continue to state that direct installers are unsigned until signing is actually established. Store and WinGet availability must be reported only after those channels are live.

Unknown results preserve the available evidence and explain which context is missing. Validate domain and server behavior in a representative environment before using the application as an enterprise audit authority.
