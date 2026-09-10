# Verification

Run the executable test harness through `dotnet run --project tests/PermissionScope.Tests -c Release`. It returns nonzero on a failure and can write machine-readable results with `--results <file.json>`.

Coverage includes NULL/empty DACL, generic rights, deny ordering, noncanonical order, inherited and inherit-only ACEs, CREATOR OWNER, OWNER RIGHTS, conditional ACE detection, unresolved identities, deny-only and disabled token SIDs, graph cycles, multiple contributors, share intersection, Unknown semantics, snapshot integrity, SQL duplicate protection, exports, formula/HTML escaping, file remediation and rollback, stale-plan rejection and exact-path confirmation.

The harness evaluates 500 seeded random DACLs against an independent ordered-bit reference. Integration tests create temporary NTFS trees, check child/grandchild inheritance, invoke `icacls` as a differential witness and verify that scanning does not alter the descriptor. Synthetic benchmarks cover 100,000 descriptors/objects and 10,000 graph edges.

`build/Test-Ui.ps1` uses Windows UI Automation to reach analysis, Access Path and snapshot save. `build/Inspect-Window.ps1` captures only the application window with DPI-correct bounds. UI review also checks theme and narrow layouts.

No AD domain controller, representative SMB server, DFS namespace or ARM64 device is available in this environment. Those integration claims must remain unverified. A cross-compiled binary does not constitute an ARM64 runtime test.
