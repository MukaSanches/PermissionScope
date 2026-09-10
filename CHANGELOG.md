# Changelog

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

See `docs/release-status.md` for what is and is not established by this release.
