# Security policy

PermissionScope 1.0.x is the currently maintained line. Security fixes are distributed through the project's GitHub releases. There is no background updater or telemetry service.

## Report privately

Use **Security → Report a vulnerability** on the official repository when private vulnerability reporting is enabled. If that option is unavailable, open an issue asking for a private reporting channel, without disclosing exploit details or private environment data. Do not post credentials, production snapshots, private paths, tokens or account inventories in public issues.

Include the version, Windows build, minimal synthetic SDDL, expected result, actual result and reproducible steps. Incorrect permission decisions are security-relevant. Every confirmed calculation defect should receive a regression fixture before its fix ships.

## Trust boundaries

- Analysis and simulation are read-only. Applying a change is a separate, explicitly confirmed local-file operation.
- Unknown is not denied or granted. Server token equivalence and conditional access are not inferred.
- Snapshots contain security-sensitive metadata. Their hashes detect accidental alteration; they are not signatures or proof of origin.
- Local diagnostics may contain paths and account names. Review them before sharing.
- Only the requested Windows resources and directories are contacted during analysis. No PermissionScope account, application backend or AI service exists.
- The current direct-distribution binaries are unsigned. Verify SHA-256 against the release manifest and obtain artifacts from the official repository.

See [the security model](docs/security.md) and [release boundaries](docs/release-status.md).
