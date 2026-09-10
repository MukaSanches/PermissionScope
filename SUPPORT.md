# PermissionScope support

PermissionScope support is designed around reproducible evidence and privacy-safe diagnostics.

## Before opening an issue

1. Confirm you are using the current public release.
2. Read the [FAQ and troubleshooting guide](docs/faq.md).
3. Reproduce with the smallest safe resource and identity scope possible.
4. Prefer a synthetic example when the problem is about permission semantics.
5. Copy the application's safe diagnostic output when available.

## Include

- PermissionScope version.
- Windows edition and build.
- x64 or ARM64.
- Operation being performed.
- Expected result and actual result.
- Minimal reproducible steps.
- Minimal synthetic SDDL or fixture when relevant.
- Error code/message.

## Do not include publicly

- passwords, secrets or tokens;
- private keys;
- production snapshots containing sensitive identities;
- unredacted confidential paths;
- full company account inventories;
- exploit details for an unpatched security vulnerability.

Security-sensitive findings should follow [SECURITY.md](SECURITY.md).

## Learn first

New users can follow the [PermissionScope Academy](docs/courses/README.md), starting with the synthetic LAB environment before investigating real files.

## Trust and releases

Use only distribution channels explicitly listed by the project. GitHub release packages include SHA-256 checksum files. See the [Trust Center](docs/TRUST-CENTER.md) and [release status](docs/release-status.md).
