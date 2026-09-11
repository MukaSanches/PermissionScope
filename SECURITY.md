# PermissionScope security policy

PermissionScope treats incorrect permission decisions, unsafe state changes and package-integrity failures as security-relevant engineering concerns.

## Supported line

| Version line | Status |
|---|---|
| `1.0.x` | Maintained |
| Older / unreleased builds | Best-effort triage only |

Security fixes are distributed through the project's official GitHub releases. PermissionScope has no background updater, mandatory account or application telemetry service.

## Report privately

Use **Security → Report a vulnerability** on the official repository when private vulnerability reporting is available.

If the private reporting option is unavailable, open a public issue asking the maintainer to provide a private reporting channel **without including exploit details, secrets, private environment data or a proof of concept**.

Do not publish an unpatched vulnerability in an issue, discussion, pull request, screenshot, export or diagnostic attachment.

## What to include

A useful private report contains, where applicable:

- PermissionScope version or commit SHA;
- Windows edition/build and architecture;
- affected operation or trust boundary;
- minimal reproducible steps;
- minimal synthetic SDDL/fixture rather than production data;
- expected result and actual result;
- impact assessment;
- whether exploitation requires local access, elevated rights, crafted files or a specific identity context;
- suggested mitigation, if known.

Incorrect permission decisions are security-relevant. Confirmed deterministic calculation defects should receive a regression fixture before the fix ships.

## What not to send

Do not send passwords, access tokens, private keys, production account inventories, raw enterprise snapshots or unrelated confidential files. If a path or identity is needed to reproduce a problem, prefer a synthetic equivalent.

## Security boundaries

- Analysis and simulation are read-only.
- Applying a change is a separate, explicitly confirmed local-file workflow.
- **Unknown is not Granted or Denied.** Missing decision context remains explicit uncertainty.
- Server-token equivalence, conditional access and unsupported identity contexts are not invented.
- Snapshot hashes detect accidental alteration; they are not digital signatures or proof of origin.
- Diagnostics and exports can contain sensitive paths or identity information and should be reviewed before sharing.
- Core analysis does not require a PermissionScope backend or AI service.
- Direct-distribution binaries may be unsigned; verify SHA-256 against the official release manifest and obtain artifacts only from official project channels.

See [`docs/security.md`](docs/security.md), [`docs/release-status.md`](docs/release-status.md) and the [`Trust Center`](docs/TRUST-CENTER.md) for the documented security model and current distribution boundaries.

## Triage principles

Security reports are evaluated by impact, reproducibility and affected trust boundary rather than by sensational severity labels.

Priority is highest when a defect could:

1. produce an incorrect effective-access conclusion;
2. cause an unintended filesystem permission change;
3. weaken rollback or journal guarantees;
4. expose sensitive snapshot/diagnostic data unexpectedly;
5. compromise official package/release integrity;
6. bypass an explicit safety confirmation.

The project does not promise a fixed response or remediation SLA. Reports are handled as quickly as maintainer capacity and verification requirements allow.

## Coordinated disclosure

When a report is valid, public disclosure should wait until users have a practical mitigation or fixed release whenever that is feasible. Credit can be included in release notes when the reporter wants attribution and disclosure is safe.

The maintainer may request additional time when the fix requires Windows-backed regression evidence, packaging changes or coordinated release work.

## Out of scope

The following are not automatically vulnerabilities by themselves:

- an installer warning caused solely by the current unsigned direct-distribution status;
- behavior already documented as Unknown because required Windows context is unavailable;
- unsupported remote/S4U/conditional contexts that are clearly reported as unsupported;
- denial of access caused by mechanisms outside the documented discretionary ACL decision model, such as encryption or external locks.

Out-of-scope reports can still become product improvements if they expose confusing or misleading behavior.

## Security automation

CodeQL and build/test workflows provide continuous evidence, not certification. A green workflow does not prove the absence of vulnerabilities.
