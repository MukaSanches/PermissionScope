# Microsoft Store and trusted distribution readiness

PermissionScope can automate package preparation and internal validation, but several distribution milestones are controlled by external identities and services. This document distinguishes what CI can prove from what Microsoft or a trusted certificate authority must prove.

## What the repository can prove automatically

The Production Trust workflow can:

- derive the package version from `Directory.Build.props` rather than hard-coded release numbers;
- build x64 and ARM64 application/CLI artifacts from the current source;
- produce current-version NSIS installers;
- produce unsigned x64 and ARM64 MSIX candidates;
- unpack those MSIX files and verify package identity, architecture, payload and expected assets;
- generate WinGet and Chocolatey draft manifests using the current version and local installer SHA-256 values;
- fail when a draft contains a stale version or checksum;
- record explicitly that Store, WinGet and signing claims remain false until externally established.

## Trusted signing gate

`build/Sign-Artifacts.ps1` supports a certificate already installed in the Windows certificate store. It requires:

- an explicit certificate thumbprint;
- an HTTPS RFC3161 timestamp service;
- an accessible private key;
- explicit PowerShell confirmation because signing mutates the artifacts.

After signing, the script runs SignTool verification and writes `artifacts/production-trust/signing-evidence.json`.

The repository does **not** ship a private key, PFX password or signing certificate. Never add any of those to source control, issue attachments or workflow YAML.

## Microsoft Store gate

Before submission:

1. Reserve/use the final Partner Center product identity.
2. Obtain the exact Publisher and package Identity values assigned by Partner Center.
3. Build the MSIX candidates using those exact values.
4. Run the applicable Windows App Certification Kit / Partner Center preflight on the candidate.
5. Verify Store listing text, screenshots, age/category declarations, privacy statements and support links.
6. Submit through the authorized Partner Center account.
7. Record the submission/certification result only after Microsoft reports it.

A locally generated MSIX is not Store certification. A successful CI run must never be described as Microsoft approval.

## WinGet gate

`build/Prepare-Channels.ps1` creates a current-version manifest draft. Before opening a WinGet submission:

1. Release immutable public installers for the exact version/tag.
2. Run `build/Prepare-Channels.ps1 -VerifyPublished` from the exact release source state.
3. Confirm both public URLs download successfully and match the manifest hashes.
4. Validate with the current official WinGet tooling/process.
5. Submit to the official WinGet packages repository.
6. Treat availability as live only after the submission is accepted and searchable/installable through WinGet.

## Chocolatey gate

The repository also prepares a Chocolatey draft for compatibility testing. It is not part of the core trust claim. A package must be tested against public immutable release artifacts before any community-feed submission.

## Release evidence language

Use these phrases precisely:

- **MSIX candidate generated and structurally validated** — allowed after the Production Trust distribution job passes.
- **Authenticode signed** — allowed only after SignTool verification for the exact artifact.
- **Microsoft Store certified/published** — allowed only after Partner Center confirms certification/publication.
- **WinGet available** — allowed only after the official manifest is accepted and installable.
- **ARM64 package available** — allowed for cross-built package evidence.
- **ARM64 physically validated** — allowed only after a representative device report is recorded.
