# Verification

PermissionScope uses layered evidence. A green build is necessary, but it is not treated as proof of every runtime, enterprise or distribution claim.

## Core executable harness

Run:

```powershell
dotnet run --project tests/PermissionScope.Tests -c Release -- --results artifacts/test-results.json
```

The harness returns nonzero on failure. Coverage includes NULL/empty DACL, generic rights, deny ordering, noncanonical order, inherited and inherit-only ACEs, CREATOR OWNER, OWNER RIGHTS, conditional ACE detection, unresolved identities, deny-only and disabled token SIDs, graph cycles, multiple contributors, share intersection, Unknown semantics, snapshot integrity, SQL duplicate protection, exports, formula/HTML escaping, file remediation and rollback, stale-plan rejection and exact-path confirmation.

The harness evaluates seeded random DACLs against an independent ordered-bit reference. Integration tests create temporary NTFS trees, check child/grandchild inheritance, invoke `icacls` as a differential witness and verify that scanning does not alter the descriptor.

## Native UI verification

`build/Test-Ui.ps1` and `build/Test-UiRegression.ps1` retain focused regression journeys.

`build/Test-UiTrust.ps1` is the broader Production Trust gate. It launches the real native application with isolated synthetic data, traverses all eight published locales in Light/Dark themes and uses stable AutomationIds rather than translated control labels. It verifies critical navigation, Analyze inputs, Access Path tabs, Settings, Compare and Simulation. It also rejects obvious host identity/path leakage from the synthetic accessibility tree.

This is native UI Automation evidence, not Narrator/NVDA certification. See `docs/accessibility.md`.

## Controlled performance evidence

`build/Measure-Scan.ps1` performs a detailed synthetic local scan measurement and validation. `build/Measure-TrustBenchmarks.ps1` runs a small matrix and publishes only sanitized aggregates: object count, elapsed time, objects/second, observed peak working set, errors/Unknown count and termination behavior. Raw worker output is removed by default because it may contain local identity/path context.

These numbers are trend evidence for the tested environment, not an SLA or competitor benchmark.

## Distribution readiness

The Production Trust distribution job:

1. builds current-version x64 and ARM64 artifacts;
2. creates current-version unsigned MSIX candidates;
3. unpacks and validates MSIX identity, architecture, payload and hashes;
4. generates current-version WinGet/Chocolatey drafts from local installer SHA-256 values;
5. rejects stale versions/checksums and any unverified claim that Store/WinGet submission already happened.

`build/Sign-Artifacts.ps1` is an explicit external signing gate and requires a real certificate already installed in the Windows certificate store. The repository does not include private signing material.

## Continuous integration layers

The repository currently uses complementary workflows for:

- Windows x64/ARM64 build and packaging;
- core executable tests;
- documentation/native-capture integrity;
- browser/site regression and accessibility;
- CodeQL;
- release preflight/evidence;
- Production Trust native UI, benchmark and distribution readiness.

A release claim should use the evidence from the exact commit/tag being distributed.

## External validation boundaries

Ordinary hosted CI does not provide every environment needed to prove Windows permission behavior. In particular, no generic CI pass should be interpreted as certification of:

- representative physical Windows-on-ARM execution;
- every AD/trusted-domain topology;
- representative SMB/DFS/Central Access Policy behavior;
- trusted Authenticode publisher reputation;
- Microsoft Store certification;
- WinGet acceptance;
- complete Narrator/NVDA usability.

Use `docs/hardware-validation.md`, `docs/external-validation.md`, `docs/store-readiness.md` and `docs/capability-matrix.md` to move those claims forward with recorded evidence.
