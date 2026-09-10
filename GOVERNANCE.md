# PermissionScope governance

PermissionScope is maintained as an evidence-first open-source Windows project. Project decisions prioritize correctness, safety, privacy, explainability and reproducibility over feature count or release speed.

## Decision principles

1. **Correctness before confidence.** If the available Windows context cannot establish an answer, the product should preserve uncertainty rather than invent certainty.
2. **Safety before convenience.** Analysis and simulation remain separate from state-changing workflows.
3. **Evidence before claims.** Performance, security and compatibility claims should be backed by reproducible tests or clearly labeled as limitations.
4. **Local-first by default.** Core permission analysis should not require an account, backend or telemetry service.
5. **Accessible by design.** Critical workflows should remain keyboard-usable, understandable in plain language and respectful of user motion/contrast preferences.
6. **One source of truth where possible.** Generated documentation and localized material should derive from maintained canonical sources.

## Maintainer responsibility

The project maintainer has final responsibility for releases, security boundaries and project direction. Community contributions are evaluated on technical merit, compatibility with the security model and long-term maintenance cost.

## Change classes

### Low risk

Documentation corrections, synthetic examples, accessibility improvements and tooling that do not change permission decisions.

### Medium risk

UI workflow changes, export changes, scanning behavior and performance work.

### High risk

Anything that changes effective-access calculation, identity handling, filesystem mutation, rollback, trust boundaries or package capabilities.

High-risk changes should receive focused regression coverage and explicit review of failure modes before release.

## Release gates

A public release should aim to have:

- reproducible source-to-package traceability;
- current x64 and ARM64 build results;
- integration/regression tests;
- installer/package verification;
- current security and privacy documentation;
- checksums and dependency inventory;
- release notes with known limitations;
- no knowingly stale generated documentation.

## Security reports

Security reports take priority over roadmap work when they can affect permission decisions, local data safety, package integrity or state-changing workflows. See [SECURITY.md](SECURITY.md).

## Roadmap

The public [ROADMAP.md](ROADMAP.md) describes direction rather than contractual promises. Security, correctness and platform changes can reorder priorities.
