# Changelog

## Unreleased

- Harden imported snapshot validation before data reaches UI, comparison or simulation paths, including integrity checks for both NTFS and share security descriptors and stricter nested metadata validation.
- Reduce default scan I/O by enumerating directories directly when file inclusion is disabled, improve enumeration-error progress reporting and reject the Windows `GLOBALROOT` device namespace.
- Repair website script isolation, mobile/WebKit overflow, accessibility diagnostics, contrast behavior and navigation-transition regressions across the eight localized static pages.
- Strengthen release engineering with a guarded manual/tagged 1.0.0 workflow, explicit preflight, provenance attestations, combined SHA-256 release checksums, source packaging and refusal to overwrite an existing public 1.0.0 release.
- Add an executable release-readiness command, a formal go/no-go checklist, first-release notes and safer GitHub issue routing for security/support questions.
- License PermissionScope's own source and documentation under Apache-2.0, preserving Samuel Sanches's attribution and third-party licenses. Include NOTICE and license metadata in newly built packages. Previously distributed artifacts are unchanged.

## 1.0.0

Initial public release candidate. Publication and certification status are tracked separately from the version number.

- Native WinUI application and shared CLI/worker using Windows Authz.
- Access evidence, explicit Unknown states, snapshots, comparison and permission simulation.
- Guarded local-file DACL changes with confirmation, durable journal, verification and rollback.
- HTML, PDF, XLSX, CSV and JSON reports; daily signed-in-user scheduling.
- Localized UI resources, light/dark appearance, compact navigation and scrollable narrow results.
- Corrected detached-control ownership errors during page reuse and compact navigation.
- Modern desktop file pickers, local operation diagnostics, long-path support and junction-cycle protection.
- Owner Rights ACEs now appear in the access evidence for the descriptor owner, with Windows-backed regression cases.
- Remediation verifies the opened file's local volume, including paths reached through a drive mapping or parent link.
- Packaging records a source fingerprint; release verification rejects stale binaries and invalid checksums. Initial GitHub releases remain drafts pending final validation.
- Self-contained x64/ARM64 packages; unsigned x64 MSIX preparation.
- Added an isolated synthetic LAB demonstration evaluated by Windows Authz, with five sample export formats and before/after snapshots.
- Added plain-language access summaries, actionable Unknown explanations and diagnostics without account names, paths or SIDs.
- Fixed missing Unknown guidance for unreadable resources and stale filters when entering the demo. CLI simulation now preserves Unknown states and limitations.
- Added eight equivalent READMEs, localized visual guides and static website pages, backed by 72 native light/dark captures and source/hash checks.
- Pinned workflow actions to upstream commit SHAs and added documentation, image integrity and locale checks to CI.

See `docs/release-status.md` for what is and is not established by this release.
