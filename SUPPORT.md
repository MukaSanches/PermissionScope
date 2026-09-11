# PermissionScope support

PermissionScope support is organized around reproducible evidence, privacy-safe diagnostics and clear routing.

## Choose the right channel

| Need | Best route |
|---|---|
| Reproducible product defect | GitHub bug report form |
| Product improvement | GitHub feature request form |
| Usage question / discussion | GitHub Discussions |
| Security vulnerability | Private route in [`SECURITY.md`](SECURITY.md) |
| Release/package trust question | [`docs/release-status.md`](docs/release-status.md) and [`docs/TRUST-CENTER.md`](docs/TRUST-CENTER.md) |
| Contribution/development question | [`CONTRIBUTING.md`](CONTRIBUTING.md) |

## Before opening an issue

1. Confirm that you are using the current public release or identify the exact commit/build.
2. Read the [FAQ and troubleshooting guide](docs/faq.md).
3. Reproduce with the smallest safe resource and identity scope possible.
4. Prefer a synthetic example when the problem is about permission semantics.
5. Copy the application's privacy-safe diagnostic output when available.
6. Check whether the behavior is already documented as Unknown or outside the supported model.

## Include in a good report

- PermissionScope version or commit SHA;
- Windows edition and build;
- x64 or ARM64;
- operation being performed;
- expected result and actual result;
- minimal reproducible steps;
- minimal synthetic SDDL/fixture when relevant;
- exact error code/message when available;
- whether the issue reproduces in the synthetic demo.

## Do not include publicly

- passwords, secrets or tokens;
- private keys;
- production snapshots containing sensitive identities;
- unredacted confidential paths;
- complete company account inventories;
- exploit details for an unpatched security vulnerability;
- screenshots that accidentally expose unrelated personal or corporate data.

Security-sensitive findings must follow [`SECURITY.md`](SECURITY.md).

## Diagnostic standard

A useful report should allow another person to understand the failure without needing access to your organization.

Good evidence includes:

- a synthetic descriptor;
- a redacted error code;
- a minimal path created solely for reproduction;
- a Windows comparison result;
- the application's safe diagnostic output.

A full production snapshot is rarely necessary and should not be posted publicly.

## Support boundaries

Support does not imply a guaranteed response time or contractual SLA. The maintainer prioritizes security defects, incorrect permission conclusions, data-safety problems and reproducible regressions ahead of broad usage questions.

PermissionScope documents uncertainty rather than promising support for every Windows access-control context. Unsupported or incomplete contexts may legitimately remain Unknown.

## Learn first

New users can follow the [PermissionScope Academy](docs/courses/README.md), starting with the synthetic LAB environment before investigating real files.

## Trust and releases

Use only distribution channels explicitly listed by the project. GitHub release packages include SHA-256 checksum files. See the [Trust Center](docs/TRUST-CENTER.md) and [release status](docs/release-status.md) before relying on signing, Store, WinGet or architecture claims.
