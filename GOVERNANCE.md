# PermissionScope governance

PermissionScope is maintained as an evidence-first, local-first Windows project. Governance exists to keep technical decisions understandable, reviewable and reversible as the project grows.

## Project charter

The project exists to make Windows permission analysis easier to understand without pretending the operating system is simpler than it is.

The following are non-negotiable project principles:

1. **Correctness before confidence.** Incomplete context must remain Unknown rather than being converted into a reassuring answer.
2. **Safety before convenience.** Analysis and simulation stay distinct from state-changing operations.
3. **Evidence before claims.** Security, compatibility and performance claims require reproducible evidence or an explicit limitation.
4. **Local-first by default.** Core permission analysis must not depend on a PermissionScope account, backend or behavioral telemetry.
5. **Accessible by design.** Critical flows should remain keyboard-usable, understandable in plain language and respectful of contrast/motion preferences.
6. **Free core product.** The official PermissionScope project follows [`FREE-FOREVER.md`](FREE-FOREVER.md).
7. **One source of truth where practical.** Generated and localized material should derive from maintained canonical sources.
8. **No security theater.** The project does not use fake certification, invented assurance or unexplained badges as a substitute for evidence.

## Roles

### Maintainer

The maintainer is responsible for repository direction, merge decisions, release authorization, security boundaries, project policy and stewardship of official distribution channels.

Current maintainer: **Samuel Sanches (`@MukaSanches`)**.

### Contributor

A contributor proposes or implements changes. Contribution does not imply merge authority. Contributors are expected to follow [`CONTRIBUTING.md`](CONTRIBUTING.md), the security policy and the community conduct policy.

### Reporter / reviewer

Reporters and reviewers improve the project by supplying reproducible evidence, testing assumptions and identifying failure modes. Review quality is valued independently of code contribution volume.

## Decision classes

### Class 1 — Low risk

Examples: documentation corrections, synthetic examples, editorial cleanup, repository organization and accessibility improvements that do not change permission decisions.

Expected evidence: scope review and relevant formatting/content checks.

### Class 2 — Medium risk

Examples: UI workflow changes, exports, scanning behavior, performance work, packaging and non-decision data handling.

Expected evidence: regression coverage appropriate to the affected surface, compatibility review and rollback awareness.

### Class 3 — High risk

Examples: effective-access calculation, identity handling, ACL interpretation, filesystem mutation, rollback, trust boundaries, package capabilities and security-sensitive persistence.

Expected evidence: deterministic regression cases, explicit failure-mode review, Windows-backed comparison where applicable and a durable decision record for architectural changes.

## Decision process

For meaningful changes, the project follows this sequence:

```text
Problem statement
      ↓
Evidence / constraints
      ↓
Risk classification
      ↓
Alternatives considered
      ↓
Implementation
      ↓
Verification
      ↓
Review and merge decision
      ↓
Release decision when applicable
```

A large solution should not be approved merely because it is ambitious. The preferred change is the smallest design that solves the actual problem while preserving project boundaries.

## Durable decision records

Architectural or security-boundary decisions that future maintainers may reasonably question should be recorded under [`docs/decisions/`](docs/decisions/). Decision records explain context, choice, consequences and reversal conditions; they are not marketing material.

A decision record is strongly recommended when a change:

- modifies a trust boundary;
- creates or removes a state-changing capability;
- changes how Unknown is determined;
- introduces a long-lived dependency or platform constraint;
- changes release integrity/provenance policy;
- intentionally accepts a non-obvious technical tradeoff.

## Merge authority

The maintainer has final merge authority. A green CI run is necessary evidence, not automatic approval. A change may still be declined because of unclear scope, weak maintainability, security risk, unsupported product direction or insufficient evidence.

No contributor is entitled to merge based on effort already spent.

## Release authority and gates

A public release should not be treated as ready until the relevant gates are satisfied. Depending on the change, these include:

- current x64 and ARM64 build results;
- integration/regression evidence;
- packaging/install verification;
- current security and privacy documentation;
- checksums and dependency/SBOM evidence;
- release notes with known limitations;
- source-to-package traceability;
- no knowingly stale generated documentation;
- explicit acknowledgement of any untested platform claim.

Release automation supports the decision; it does not replace maintainer responsibility.

## Security priority

Credible security reports can supersede roadmap work when they affect permission decisions, local data safety, package integrity, trust boundaries or state-changing workflows. See [`SECURITY.md`](SECURITY.md).

Security fixes should add regression evidence when the defect can be deterministically reproduced.

## Roadmap and compatibility

[`ROADMAP.md`](ROADMAP.md) communicates direction, not contractual delivery dates. Security, correctness, Windows platform changes and maintainability can reorder priorities.

Breaking behavior should be avoided when a safe migration exists. When a deliberate break is necessary, the release notes should explain what changed, who is affected and what the migration path is.

## Deprecation

A feature may be deprecated when it is unsafe, misleading, structurally unmaintainable, replaced by a clearly better path or incompatible with a project principle. Deprecation should be documented before removal when practical.

## Governance changes

Changes to this governance document are themselves reviewable project decisions. Material changes should explain the motivation and expected effect on contributors or users.

## Conflict handling

Technical disagreements should return to evidence, project principles and failure modes. The maintainer may close a discussion when additional repetition is unlikely to change the decision.

Conduct disputes follow [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md). Security disclosure disputes follow [`SECURITY.md`](SECURITY.md).

## Transparency

Where privacy and security allow, project decisions should remain visible through issues, pull requests, commit history, release notes and decision records. Private security material is the main intentional exception.
