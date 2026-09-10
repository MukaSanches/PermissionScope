# PermissionScope Trust Center

PermissionScope is built around a simple rule: trust should be inspectable.

## Security posture

- Local-first Windows application; no PermissionScope account or application backend.
- No application telemetry service.
- Permission analysis and simulation are read-only.
- Applying a supported change is a separate, explicitly confirmed workflow.
- Unknown is never silently converted into Granted or Denied.
- Security-relevant calculation defects should receive regression coverage before release.

## Software supply chain

- GitHub Actions are pinned to immutable commit SHAs where used by the project.
- Dependency lock files are committed.
- CodeQL is part of the repository security workflow.
- Release packages include SHA-256 checksums.
- Packaging generates a CycloneDX SBOM from the locked dependency graph.
- Main-branch Windows packages receive GitHub artifact provenance attestations.
- Main-branch Windows packages also receive CycloneDX SBOM attestations bound to the installer and portable-package digests.
- Source fingerprinting is used during packaging to detect source changes while a distribution is being produced.

Artifact attestations establish verifiable build origin and bind release metadata to package digests; they are not a claim that a package is vulnerability-free or a security certification. See [release verification](release-verification.md) for exact commands and limitations.

## Distribution

Official public binaries are published only through the repository's GitHub Releases until additional channels are explicitly listed in `docs/release-status.md`.

Current direct-distribution installers are unsigned. Verify release checksums and, for builds that include them, GitHub artifact attestations before relying on a package. Microsoft Store packaging and signing are tracked separately from direct GitHub distribution.

## Privacy

PermissionScope does not need a cloud service to perform its core permission analysis. Real exports and local diagnostics can still contain sensitive Windows paths, account names, SIDs or permission evidence. Review material before sharing it.

## Architecture boundaries

The application evaluates Windows discretionary permission evidence. Integrity policy, encryption, file locks, administrative privileges and other Windows controls can still influence whether an operation succeeds.

Remote logon equivalence, conditional rules and unverified reparse targets are not guessed. They remain explicitly limited or Unknown.

## Verification resources

- [Release verification](release-verification.md)
- [Security policy](../SECURITY.md)
- [Security model](security.md)
- [Access model](access-model.md)
- [Benchmarks](benchmarks.md)
- [Release status](release-status.md)
- [Third-party notices](../THIRD-PARTY-NOTICES.md)
- [Official handbooks](handbooks/README.md)

## Reporting

Use the repository Security workflow for vulnerability reports whenever private vulnerability reporting is available. Never publish credentials, tokens, private snapshots or production account inventories in a public issue.
