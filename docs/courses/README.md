# PermissionScope Academy

A practical learning path for people who are new to Windows permissions and for administrators who need a disciplined investigation workflow.

The Academy does not replace the canonical technical documentation. It turns the existing documentation into a guided curriculum with explicit outcomes and safe exercises.

## Starter certification path

1. [Windows permission fundamentals](fundamentals.md) — identities, SIDs, ACEs, DACLs, inheritance, effective access and Unknown.
2. [Reading Access Path](access-path.md) — move from a summary to the recorded evidence and limitations.
3. [Safe troubleshooting](troubleshooting.md) — investigate, preserve evidence, change narrowly and keep rollback possible.

## Product workflow track

| Skill | Learning outcome | Canonical material |
| --- | --- | --- |
| Install and verify | Choose x64/ARM64, install or use portable ZIP and verify SHA-256 | [Release status](../release-status.md) · [Development](../development.md) |
| Explore safely | Learn the UI using only the fictional LAB environment | [Visual guides](../guides/en-US.md) |
| Analyze a folder | Run a focused local analysis without changing permissions | [FAQ](../faq.md) · [Access model](../access-model.md) |
| Choose an identity | Understand current-token evaluation and identity evidence | [Access model](../access-model.md) |
| Interpret results | Distinguish Granted, Partial, Denied and Unknown | [Glossary](../glossary.md) · [Fundamentals](fundamentals.md) |
| Verify Access Path | Trace memberships and contributing permission entries | [Access Path course](access-path.md) |
| Save snapshots | Preserve an observation for later review | [Visual guides](../guides/en-US.md) |
| Compare observations | Identify permission-state changes without assuming missing means deleted | [FAQ](../faq.md) |
| Simulate safely | Model removing a permission entry in memory before touching a file | [Security model](../security.md) |
| Export evidence | Produce HTML, CSV, JSON, XLSX or PDF reports and review sensitive data before sharing | [Privacy](../privacy.md) |
| Apply a supported change | Use explicit confirmation, durable journal, verification and rollback | [Security model](../security.md) · [Troubleshooting](troubleshooting.md) |

## CLI and automation track

Start with the synthetic demo, then move to real resources only after you understand the output model.

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe explain "C:\Finance"
./permissionscope-cli.exe scan "C:\Finance" --save
./permissionscope-cli.exe export <snapshot-id> --format html --output report.html
```

Study next:

- [Development and command-line workflow](../development.md)
- [Access model](../access-model.md)
- [Benchmarks and measurement boundaries](../benchmarks.md)

## Security and audit track

1. Read the [Trust Center](../TRUST-CENTER.md).
2. Read the [Security policy](../../SECURITY.md).
3. Understand the [security model](../security.md).
4. Learn what PermissionScope deliberately leaves Unknown.
5. Learn how release checksums, source fingerprints and SBOMs support verification.
6. Practice with synthetic data before investigating production resources.

## Contributor track

1. Read [CONTRIBUTING](../../CONTRIBUTING.md).
2. Build the solution using [development.md](../development.md).
3. Run resource and documentation validation.
4. Run the integration harness.
5. Add a regression fixture for any confirmed permission-calculation defect.
6. Keep generated documentation synchronized.
7. Review the [roadmap](../../ROADMAP.md) before proposing large architectural changes.

## Learning principles

- Evidence before confidence.
- Synthetic examples before production data.
- Read before change.
- Unknown is a valid safety result.
- Every important claim should be traceable to Windows evidence, source, tests or release artifacts.
- Privacy and rollback are part of the workflow, not afterthoughts.

## Official handbooks

The project also publishes corporate-style PDF handbooks for every supported documentation language under [`docs/handbooks`](../handbooks/README.md).
