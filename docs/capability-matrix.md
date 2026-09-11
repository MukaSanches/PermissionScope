# Capability and assurance matrix

PermissionScope separates **implemented behavior**, **tested evidence**, **partial/Unknown behavior** and **external certification**. A feature is not described as certified merely because it builds or has an automated test.

Legend:

- **Supported** — implemented with automated evidence in this repository.
- **Partial** — implemented for a bounded set of cases; limitations are material.
- **Unknown by design** — PermissionScope deliberately refuses to manufacture a confident answer when required Windows context is unavailable.
- **External gate** — requires a real account, certificate, service, network/domain topology, assistive technology review or physical hardware outside ordinary CI.

## Permission analysis

| Capability | Status | Evidence / boundary |
|---|---|---|
| Local NTFS discretionary ACL inspection | Supported | Native descriptor parsing plus Windows Authz evaluation and integration tests |
| Effective discretionary access for selected local identity context | Supported | Windows Authz-backed decision model; `Unknown` is retained when context is insufficient |
| Allow/deny ordering, generic rights and inherited/inherit-only ACE behavior | Supported | Executable regression harness and differential NTFS fixtures |
| Access Path / contributing evidence | Supported | Contributor evidence, token/group relationship where available, ACE index, masks, flags and source are exposed |
| Share + NTFS intersection | Partial | Supported where share security context is available; remote equivalence is not inferred when it cannot be verified |
| UNC remote effective access | Unknown by design | Remote-logon equivalence, remote token behavior and server-side policy can be outside local evidence |
| DFS target discovery/divergence | Partial / not certified | No claim of complete namespace/target authority |
| Central Access Policy / conditional enterprise policy | Partial / Unknown by design | Conditional/special rules are detected conservatively; complete enterprise policy evaluation is not claimed |
| Domain/trust traversal | Partial | Available directory evidence is used, but trust referrals and every large-membership continuation path are not certified complete |
| Exact inherited ACE ancestor/depth | Partial | Windows inheritance flags are retained; exact ancestor/depth is not always established |
| Snapshot comparison | Supported | Compares recorded descriptors and evaluated context where comparable |
| In-memory ACE-removal simulation | Supported, bounded | Simulates removal of one ACE for the selected context; it is not a full policy-change planner |
| Local-file remediation + rollback | Supported, guarded | Restricted to ordinary local files and protected by stale-plan/path confirmation checks |
| Arbitrary directory/group/policy modification | Not a goal | PermissionScope is analysis-first and does not present itself as a general directory administration suite |

## Application and reports

| Capability | Status | Evidence / boundary |
|---|---|---|
| Native WinUI application | Supported | Windows x64 build, native controls and automated journeys |
| CLI engine | Supported | Shared analysis engine plus executable integration harness |
| HTML / CSV / JSON / XLSX exports | Supported | Regression coverage and escaping/integrity tests |
| Direct PDF export | Partial | Latin, Greek and Cyrillic are supported directly; complex scripts/CJK/emoji should use HTML + browser Print to PDF |
| Local-first operation | Supported | No required PermissionScope account or required cloud backend for core analysis |
| Application telemetry | Not required | Core operation does not require application telemetry |
| Eight documentation/site locales | Supported as publication surface | Locale parity is automated |
| Native app language quality | Mixed assurance | English and Brazilian Portuguese are primary; additional catalogs require native-speaker review before claiming equivalent linguistic quality |

## Platform and distribution

| Capability | Status | Evidence / boundary |
|---|---|---|
| x64 build/package/install/uninstall | Supported | CI packages and smoke-tests installer installation and removal on disposable Windows runners |
| ARM64 build/package | Supported as cross-build | ARM64 binaries/packages are produced in CI |
| ARM64 physical execution | External gate | Requires representative Windows-on-ARM hardware and recorded validation |
| Unsigned NSIS distribution | Supported | GitHub release artifacts and SHA-256 checksums |
| Unsigned MSIX candidate generation | Supported as readiness artifact | Current-version package identity/payload is structurally validated; this is not Store certification |
| Trusted Authenticode signing | External gate | `build/Sign-Artifacts.ps1` is ready for a real code-signing identity; no certificate is bundled or claimed |
| Microsoft Store publication | External gate | Requires Partner Center publisher identity, signing/ingestion and certification |
| WinGet publication | External gate | Current-version manifests can be generated and validated internally; acceptance occurs in Microsoft's repository/process |

## Quality assurance

| Assurance | Status | Evidence / boundary |
|---|---|---|
| Core executable tests | Supported | Deterministic regression/integration harness |
| CodeQL | Supported | GitHub workflow runs on repository changes |
| Native UI control reachability | Supported | Production Trust UI Automation uses stable AutomationIds across 8 locales and light/dark themes |
| Synthetic performance trend | Supported | Sanitized benchmark matrix records object count, elapsed time and observed peak working set without publishing local identity/path data |
| Website browser/accessibility checks | Supported | Chromium/Firefox/WebKit plus automated accessibility checks |
| Narrator/NVDA certification | External gate | Manual assistive-technology review is required; automated UI Automation is not certification |
| Independent security audit | External gate | Community/external review is encouraged; no third-party audit is claimed unless a report is published |

## Rule for future claims

A status may move from Partial/External gate to Supported only when the repository contains reproducible evidence or links to the external evidence required for that claim. Marketing text, a successful compilation or a single unrecorded manual test is not enough.
