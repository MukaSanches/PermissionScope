<p align="center">
  <img src="../docs/brand/project-avatar.png" alt="PermissionScope mark" width="84" height="84">
</p>

<h1 align="center">PermissionScope · Repository Operations</h1>

<p align="center"><strong>Evidence-first maintenance for a local-first Windows security project.</strong></p>

<p align="center">
  <a href="../GOVERNANCE.md">Governance</a> ·
  <a href="../CONTRIBUTING.md">Contributing</a> ·
  <a href="../SECURITY.md">Security</a> ·
  <a href="../SUPPORT.md">Support</a> ·
  <a href="../docs/brand/README.md">Brand</a> ·
  <a href="../docs/decisions/README.md">Decision records</a>
</p>

---

This directory is the operating layer for the PermissionScope repository. It defines how work enters the project, how changes are reviewed, how security-sensitive reports are handled and how repository-facing presentation stays consistent.

## Operating model

| Area | Source of truth | Standard |
|---|---|---|
| Project direction | [`GOVERNANCE.md`](../GOVERNANCE.md) | Correctness, safety, evidence and maintainability before feature count |
| Contributions | [`CONTRIBUTING.md`](../CONTRIBUTING.md) | Reproducible problems, focused changes, explicit verification |
| Security | [`SECURITY.md`](../SECURITY.md) | Private disclosure for vulnerabilities; Unknown must remain Unknown |
| Community conduct | [`CODE_OF_CONDUCT.md`](../CODE_OF_CONDUCT.md) | Technical rigor without hostility or exclusion |
| Support | [`SUPPORT.md`](../SUPPORT.md) | Privacy-safe diagnostics and minimal reproducible examples |
| Brand | [`docs/brand/README.md`](../docs/brand/README.md) | Precise, calm, verifiable; no fake certification or cyber-theater |
| Decisions | [`docs/decisions/`](../docs/decisions/) | Durable records for high-impact architectural and security decisions |
| Releases | [`docs/release-status.md`](../docs/release-status.md) | Claims must match shipped evidence and current limitations |

## Change flow

```text
Problem / proposal
       ↓
Issue or focused discussion
       ↓
Risk classification
       ↓
Implementation + evidence
       ↓
Pull request review
       ↓
Automated verification
       ↓
Merge
       ↓
Release only when release gates are satisfied
```

### Risk classes

| Class | Typical examples | Expected review |
|---|---|---|
| Low | Documentation, synthetic examples, editorial cleanup | Scope and accuracy review |
| Medium | UI workflow, export behavior, performance, packaging | Regression coverage plus compatibility review |
| High | Permission decisions, identity handling, filesystem mutation, rollback, trust boundaries | Deterministic regression evidence, failure-mode review and durable decision record when architectural |

The full classification and authority model lives in [`GOVERNANCE.md`](../GOVERNANCE.md).

## Repository map

| Path | Purpose |
|---|---|
| `src/` | Product source |
| `tests/` | Executable integration and regression harness |
| `docs/` | Technical, trust, release, learning and brand documentation |
| `distribution/` | Packaging/distribution definitions |
| `build/` | Build, verification and documentation tooling |
| `.github/` | Repository governance, contribution forms and automation |
| `samples/` | Synthetic examples only |

Production secrets, real enterprise permission inventories and unredacted confidential snapshots do not belong in this repository.

## Triage policy

Issues should describe a real, reproducible problem before implementation preference. Reports that may expose an unpatched vulnerability must follow [`SECURITY.md`](../SECURITY.md) instead of a public issue.

Pull requests should be small enough to review as a coherent decision. A reviewer should be able to answer four questions without reverse-engineering the intent:

1. What changed?
2. Why is it correct?
3. What evidence verifies it?
4. What can still fail or remain unknown?

## Repository presentation standard

PermissionScope should look like a serious engineering project, not a marketing microsite disguised as a repository.

- Use the project banner and mark consistently.
- Prefer short, factual badges tied to verifiable automation.
- Do not use fake trust seals, invented certifications, vanity counters or unverifiable superlatives.
- Screenshots must be genuine product captures or clearly labeled synthetic fixtures.
- Security, privacy, compatibility and performance claims must point to evidence or state their limitation.
- Keep headings, tables and callouts useful at GitHub desktop and mobile widths.
- Use Permission Blue (`#2764E7`) and Deep Navy (`#17315C`) as the primary repository identity colors.

See [`docs/brand/README.md`](../docs/brand/README.md) for the full visual and editorial system.

## Maintainer release checklist

Before treating a change as release-ready, verify that the relevant build/test gates are green, release notes and known limitations are current, package checksums/SBOM evidence match the artifacts, security/privacy documentation still reflects reality, generated documentation is not stale and no claim exceeds what was actually tested.

## Authority and ownership

`CODEOWNERS` routes repository areas to the current maintainer for review visibility. It is not a substitute for branch protection, independent security review or future multi-maintainer governance.

PermissionScope is currently maintained by **Samuel Sanches (`@MukaSanches`)**. Governance is intentionally documented now so additional maintainers can be added later without inventing process after the project scales.
