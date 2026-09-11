# Physical hardware validation protocol

This protocol turns hardware claims into repeatable evidence. It is intentionally separate from cross-compilation: a successful ARM64 build is not a physical ARM64 runtime test.

## Required record

Create one record per tested device with:

- validation date and PermissionScope commit/tag;
- Windows edition, version and OS build;
- CPU/SoC model and architecture;
- installed RAM;
- filesystem used for local test data;
- package type tested (installer, ZIP, MSIX/Store as applicable);
- SHA-256 of the exact tested artifact;
- tester name or public handle;
- pass/fail/blocked status for every step below;
- sanitized notes for any failure.

Do not publish device usernames, home-directory paths, machine names, domain names, SIDs or customer ACLs.

## Baseline journey

1. Verify artifact SHA-256 against the expected build/release evidence.
2. Install using the intended package path.
3. Launch PermissionScope without elevation unless the scenario explicitly requires elevation.
4. Open the synthetic demo and inspect Access, Access Path, Findings, Permissions, Technical and Simulation views.
5. Scan a dedicated local NTFS fixture containing explicit allow/deny entries, inherited entries, at least one nested directory and several files.
6. Confirm the scan completes without application crash and that Unknown results remain explicit rather than being converted into Granted/Denied.
7. Save and reopen a snapshot.
8. Compare two synthetic/test snapshots.
9. Export HTML, CSV, JSON, XLSX and PDF where the script support is appropriate for the chosen test text.
10. Exercise keyboard-only navigation through the primary workflow.
11. Switch Light/Dark and Windows High Contrast where available.
12. Close and reopen the application, then verify stored settings/snapshots behave as documented.
13. Uninstall and verify the application registration is removed.

## ARM64-specific acceptance

A physical Windows-on-ARM result can be recorded as **validated on the tested device** only when:

- the ARM64 build is running natively rather than through x64 emulation;
- install, launch, synthetic demo, local NTFS scan, snapshot, compare, export and uninstall all pass;
- no architecture-specific crash or missing native dependency is observed;
- artifact SHA and hardware/OS details are recorded;
- the report does not generalize one device into universal ARM64 certification.

## Performance sampling

After functional validation, run the sanitized benchmark matrix with the same build. Record at least 100 and 1,000 synthetic files; 10,000 is recommended for a controlled extended run. Avoid comparing devices unless power mode, background load and methodology are stated.

## Evidence template

```text
PermissionScope commit/tag:
Artifact filename:
Artifact SHA-256:
Date:
Tester:
Windows edition/version/build:
CPU/SoC:
Architecture:
RAM:
Filesystem:
Package type:

Install: PASS / FAIL / BLOCKED
Launch: PASS / FAIL / BLOCKED
Synthetic demo: PASS / FAIL / BLOCKED
Local NTFS scan: PASS / FAIL / BLOCKED
Access Path: PASS / FAIL / BLOCKED
Snapshot reopen: PASS / FAIL / BLOCKED
Compare: PASS / FAIL / BLOCKED
Exports: PASS / FAIL / BLOCKED
Keyboard journey: PASS / FAIL / BLOCKED
High Contrast: PASS / FAIL / BLOCKED
Uninstall: PASS / FAIL / BLOCKED

Notes (sanitized):
```

## Current claim boundary

Until a completed physical ARM64 record is published, PermissionScope should continue to say **ARM64 cross-built/package-validated in CI; physical ARM64 execution not yet certified**.
