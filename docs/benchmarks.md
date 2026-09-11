# Benchmarks

Measured on this Windows environment, .NET 10, Release, 9 September 2026. These are development measurements, not a comparative benchmark or SLA.

| Operation | Measured time |
|---|---:|
| Local scan, 3 directories | 128.62 ms |
| SQLite snapshot write/read, 3 objects | 221.99 ms |
| Compare 100,000 unchanged objects | 47.08 ms |
| Resolve path through 10,000 graph edges | 16.49 ms |
| Parse 100,000 descriptors | 1,215.55 ms |

Fixtures are synthetic except for the local scan and database operation. Numbers include the named operation, not project build time. See `artifacts/test-results.json` for subsequent runs. Cold startup and wide-area-network behavior need separate measurements on target hardware.

## Reproducible CLI/worker scan measurements

Run `build/Measure-Scan.ps1` with PowerShell 7 on Windows and an existing Release CLI apphost. The default is a small instrument check with 100 empty files. It does not build the project or measure the WinUI process.

```powershell
pwsh -File build/Measure-Scan.ps1 -CliPath ./artifacts/production-trust-cli/permissionscope-cli.exe

# Larger controlled fixtures
pwsh -File build/Measure-Scan.ps1 -CliPath ./artifacts/production-trust-cli/permissionscope-cli.exe -FileCount 10000 -TimeoutSeconds 300
pwsh -File build/Measure-Scan.ps1 -CliPath ./artifacts/production-trust-cli/permissionscope-cli.exe -FileCount 100000 -AllowLarge -TimeoutSeconds 900
```

The executable path is an example: select the actual prebuilt CLI being evaluated. Results record its path and hash; verify that accompanying DLLs come from the same intended build. Avoid concurrent builds and background workloads for measurements intended for comparison.

Each invocation creates a uniquely named directory under TEMP, with a synthetic fixture, raw worker output and `results.json`. The low-level harness retains them for inspection and does not change ACLs, save application snapshots or delete directories. Review local paths and token/account information in raw output before sharing anything; synthetic files do not make the worker's identity evidence anonymous.

The fixture contains up to 100 subdirectories plus the root. Consequently, 10,000 files means 10,101 scan objects; 100,000 files means 100,101 objects. Completion must be checked against that count and the returned snapshot, not just exit code zero.

Measurements cover the CLI child: elapsed time including startup, identity resolution, scan, serialization and output drain; observed peak working set; output size; and exit/validation status. Snapshot parsing occurs after the measured run. The monitor writes output to files instead of buffering the final JSON line while the worker runs. It does not measure GUI memory, GUI deserialization or end-user responsiveness.

A second run requests forced worker termination after `-CancelAfterMilliseconds` (default 500). This uses the same process-tree termination mechanism as the wrapper but does not exercise the GUI button or cooperative cancellation. A run that finishes before the request has no measured termination latency. Timeouts and invalid/incomplete output must be reported as such, never as successful scan measurements.

### Optional phase diagnostics

Pass `-MeasurePhases` to `build/Measure-Scan.ps1` when investigating where worker time is spent. The harness asks the CLI for an opt-in `--worker-metrics <file>` sidecar and records consumer timings after the process measurement:

```powershell
pwsh -File build/Measure-Scan.ps1 -CliPath ./artifacts/production-trust-cli/permissionscope-cli.exe -FileCount 100 -MeasurePhases
```

Worker metrics distinguish scanning (including identity resolution and progress), the single final snapshot serialization, and writing that serialized message to standard output. The write phase includes encoding and any blocking at the worker boundary; it does not represent the monitor's entire output drain.

Consumer metrics distinguish reading worker lines, PowerShell JSON parsing and snapshot validation. Working-set values before and after parsing are boundary observations for the monitor process, not peak memory and not WinUI measurements. The PowerShell parser is useful for attribution in this harness but does not represent the app's JSON deserializer or UI responsiveness.

The sidecar is optional, non-overwriting and best effort. An unavailable, invalid or interrupted sidecar is reported as missing/invalid diagnostics with null phase values; it does not turn a completed scan into a failure or a missing measurement into zero. Without `-MeasurePhases`, the worker protocol and benchmark behavior remain unchanged.

## Production Trust benchmark matrix

`build/Measure-TrustBenchmarks.ps1` wraps the detailed harness for repeatable CI trend evidence. By default it measures 100- and 1,000-file fixtures; `-Extended` adds 10,000 files. It fails if the underlying scan validation fails.

The published `artifacts/production-trust/benchmarks.json` intentionally contains only sanitized aggregate fields such as object count, elapsed milliseconds, objects/second, observed peak working set, error/Unknown counts and termination status. Raw worker output and fixture directories are deleted by default after the aggregate is extracted. Use `-RetainRaw` only for local investigation and review those files before sharing them.

A scheduled or manually extended CI run is useful for detecting large regressions, but the project deliberately does not encode a marketing SLA from one hosted runner. Hardware, power mode, filesystem, OS build, background load and cold/warm state matter.

## Rules for performance claims

- Never compare PermissionScope with another product unless the fixture, hardware and methodology are equivalent and published.
- Never convert one hosted-runner result into a universal performance claim.
- Keep raw paths/account identities out of public benchmark artifacts.
- Report failures/timeouts rather than dropping them from a result set.
- Prefer medians and multiple controlled runs for public hardware comparisons.
- Physical ARM64 results require the protocol in `hardware-validation.md`.

No 10k/100k production-performance claim is established merely by adding the harness; those measurements must be run and recorded on controlled hardware.

## Local 10,000-file baseline — 11 September 2026

Three consecutive runs used the published PermissionScope 1.0.1 x64 portable package on Windows 10.0.19045, PowerShell 7.6.5, NTFS and four logical processors. The package ZIP matched release SHA-256 `963d0d0795a3f1a274c81a284361b141d576ffb026061687d406aee59f9f3694`; the CLI apphost hash remained `AF573680D678E1DD6F3FE0FDEC5EB13C05BFF8DABFE155E4D2277FDE8C68EB69` during the runs.

Each synthetic fixture contained 10,000 empty files, 100 subdirectories and the root. All three runs returned exactly 10,101 resources with zero scan errors, zero Unknown decisions, matching roots, schema version 1 and a non-cancelled snapshot.

| Run | Elapsed | Observed CLI peak working set | Worker output | Forced process termination |
| --- | ---: | ---: | ---: | ---: |
| 1 | 5,688.16 ms | 298,483,712 bytes | 46,396,537 bytes | 115.68 ms |
| 2 | 4,055.48 ms | 296,398,848 bytes | 46,396,537 bytes | 18.86 ms |
| 3 | 3,860.76 ms | 296,382,464 bytes | 46,396,471 bytes | 16.04 ms |
| Median | 4,055.48 ms | 296,398,848 bytes | 46,396,537 bytes | 18.86 ms |

This is a local baseline for this package and fixture. It does not measure the WinUI process, JSON parsing in the parent, cooperative cancellation, remote/domain behavior or competing tools. Forced termination was requested after 500 ms and may include startup or identity resolution rather than active enumeration. The slower first run does not establish a cache or warm-up cause.

The approximately 46.4 MB worker response and approximately 296 MB observed child peak at 10,101 objects justify measuring scan, serialization and consumer parsing separately before attempting 100,000 files on this host. They do not establish linear growth, an out-of-memory failure or a single root cause. The 100,000-file run was deferred pending a resource budget and monitoring plan.
