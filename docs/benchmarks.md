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
