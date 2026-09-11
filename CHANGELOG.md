# Changelog

## Unreleased

## 1.0.1

- Harden imported snapshot validation before data reaches UI, comparison or simulation paths, including integrity checks for both NTFS and share security descriptors and stricter nested metadata validation.
- Reduce default scan I/O by enumerating directories directly when file inclusion is disabled, improve enumeration-error progress reporting and reject the Windows `GLOBALROOT` device namespace.
- Repair website script isolation, mobile/WebKit overflow, accessibility diagnostics, contrast behavior, PWA install metadata/offline caching and navigation-transition regressions across the eight localized static pages.
- Regenerate the 72 native application screenshots from the current WinUI source and keep screenshot/source provenance enforced by CI.
- Strengthen release engineering with x64/ARM64 packaging, x64 install/CLI/uninstall smoke testing, provenance and CycloneDX SBOM attestations, combined SHA-256 checksums and source packaging.
- Fix CycloneDX attestation metadata by emitting the required serial number plus schema/timestamp metadata.
- Make package, installer, source archive and release-verification versioning derive from the single project version instead of hard-coded `1.0.0` paths.
- Add an executable release-readiness command, formal go/no-go checks and safer GitHub issue routing for security/support questions.
- License PermissionScope's own source and documentation under Apache-2.0, preserving Samuel Sanches's attribution and third-party licenses. Include NOTICE and license metadata in newly built packages.

## 1.0.0

Initial public release.

- Native WinUI application and shared CLI/worker using Windows Authz.
- Access evidence, explicit Unknown states, snapshots, comparison and permission simulation.
- Guarded local-file DACL changes with confirmation, durable journal, verification and rollback.
- HTML, PDF, XLSX, CSV and JSON reports; daily signed-in-user scheduling.
- Localized UI resources, light/dark appearance, compact navigation and scrollable narrow results.
- Corrected detached-control ownership errors during page reuse and compact navigation.
- Modern desktop file pickers, local operation diagnostics, long-path support and junction-cycle protection.
- Owner Rights ACEs now appear in the access evidence for the descriptor owner, with Windows-backed regression cases.
- Remediation verifies the opened file's local volume, including paths reached through a drive mapping or parent link.
- Packaging records a source fingerprint; release verification rejects stale binaries and invalid checksums.
- Self-contained x64/ARM64 packages; unsigned direct-distribution installers.
- Added an isolated synthetic LAB demonstration evaluated by Windows Authz, with five sample export formats and before/after snapshots.
- Added plain-language access summaries, actionable Unknown explanations and diagnostics without account names, paths or SIDs.
- Fixed missing Unknown guidance for unreadable resources and stale filters when entering the demo. CLI simulation now preserves Unknown states and limitations.
- Added eight equivalent READMEs, localized visual guides and static website pages, backed by 72 native light/dark captures and source/hash checks.
- Pinned workflow actions to upstream commit SHAs and added documentation, image integrity and locale checks to CI.

See `docs/release-status.md` for what is and is not established by each release.
