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
pwsh -File build/Measure-Scan.ps1 -CliPath ./artifacts/documentation-cli/permissionscope-cli.exe

# Run larger fixtures separately, after builds and packaging have finished.
pwsh -File build/Measure-Scan.ps1 -CliPath ./artifacts/documentation-cli/permissionscope-cli.exe -FileCount 10000 -TimeoutSeconds 300
pwsh -File build/Measure-Scan.ps1 -CliPath ./artifacts/documentation-cli/permissionscope-cli.exe -FileCount 100000 -AllowLarge -TimeoutSeconds 900
```

The executable path is an example: select the actual prebuilt CLI being evaluated. Results record its path and hash; verify that the accompanying DLLs come from the same intended build. Avoid concurrent builds and background workloads for measurements intended for comparison.

Each invocation creates a uniquely named directory under TEMP, with a synthetic fixture, raw worker output and `results.json`. It retains them for inspection and does not change ACLs, save application snapshots or delete directories. Review local paths and token/account information in raw output before sharing anything; synthetic files do not make the worker's identity evidence anonymous.

The fixture contains up to 100 subdirectories plus the root. Consequently, 10,000 files means 10,101 scan objects; 100,000 files means 100,101 objects. Completion must be checked against that count and the returned snapshot, not just exit code zero.

Measurements cover the CLI child: elapsed time including startup, identity resolution, scan, serialization and output drain; observed peak working set; output size; and exit/validation status. Snapshot parsing occurs after the measured run. The monitor writes output to files instead of buffering the final JSON line while the worker runs. It does not measure GUI memory, GUI deserialization or end-user responsiveness. Use a local NTFS TEMP volume for the planned NTFS scenarios and record the environment.

A second run requests forced worker termination after `-CancelAfterMilliseconds` (default 500). This uses the same process-tree termination mechanism as the wrapper but does not exercise the GUI button or cooperative cancellation. A run that finishes before the request has no measured termination latency. Timeouts and invalid/incomplete output must be reported as such, never as successful scan measurements.

No 10k/100k measurements are established by adding this harness. Repeat on controlled hardware and report individual runs before making performance claims; no competitor benchmark is included.
