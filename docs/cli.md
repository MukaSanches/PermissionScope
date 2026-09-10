# CLI: from a folder to a report

The packaged `permissionscope-cli.exe` uses the same Windows engine as the GUI. Commands do not modify ACLs. JSON output is suitable for automation; it does not remove the scope limitations.

```powershell
./permissionscope-cli.exe --help
./permissionscope-cli.exe explain "C:\Finance" --json
./permissionscope-cli.exe scan "C:\Finance" --files --save --output before.json
./permissionscope-cli.exe export before.json --format html --output report.html
./permissionscope-cli.exe export before.json --format xlsx --output report.xlsx
./permissionscope-cli.exe compare before.json after.json --json
./permissionscope-cli.exe findings before.json
./permissionscope-cli.exe simulate "C:\Finance" --remove-ace 0
```

`--user DOMAIN\name` or `--user SID` selects another identity. Omit it for the current token. Other identities use S4U and may remain Unknown. `--shallow` disables recursive scanning; `--files` includes files. Use Ctrl+C to request cancellation. The GUI worker's termination behavior is described in [release scope](release-status.md).

Export formats: HTML for interactive evidence and browser printing; CSV for rows; JSON for full snapshots; XLSX for six structured worksheets; PDF for supported scripts. CSV cells protect against spreadsheet formula injection; HTML escapes evidence. Never treat exports as anonymized.

Exit codes: `0` success, `1` operational failure, `2` invalid arguments, `3` incomplete/unknown scan, explanation or simulation, `130` cancellation. Simulation returns full before/after decisions, including state and limitations; projections alone are not verification of a remote logon.

For shareable examples that contain no machine inventory:

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe compare ./demo-reports/permissionscope-demo.json ./demo-reports/permissionscope-demo-after.json --json
```

The demo exports all five formats plus an after-snapshot. LAB\Alex receives Modify through LAB\Finance; the after-snapshot changes that rule to Read. The remote resource intentionally remains Unknown. This evaluates synthetic descriptors through real Windows Authz without creating accounts, files at the depicted paths or ACLs.
