# Verifying a PermissionScope release

PermissionScope publishes several independent forms of release evidence. They answer different questions and should be used together when you need stronger assurance about a downloaded package.

## 1. Verify SHA-256

Download the package and the matching `SHA256SUMS-*.txt` file from the same GitHub Release, then compare the package hash with the published value.

On PowerShell:

```powershell
Get-FileHash .\PermissionScope-1.0.0-x64-Setup.exe -Algorithm SHA256
```

A checksum detects byte changes. It does not by itself prove who built the file.

## 2. Verify GitHub build provenance

For packages built on the repository's `main` branch after provenance attestations were enabled, GitHub records a signed artifact attestation. With a current GitHub CLI:

```powershell
gh attestation verify .\PermissionScope-1.0.0-x64-Setup.exe -R MukaSanches/PermissionScope
```

Use the downloaded ZIP path instead if you are verifying the portable package.

This verifies that the file digest is covered by a GitHub artifact attestation associated with this repository and its GitHub Actions build identity. It does **not** mean the software is vulnerability-free or that every Windows access outcome is guaranteed.

## 3. Verify the CycloneDX SBOM attestation

New `main` builds also bind the generated CycloneDX dependency inventory to the same installer and portable-package digests.

```powershell
gh attestation verify .\PermissionScope-1.0.0-x64-Setup.exe `
  -R MukaSanches/PermissionScope `
  --predicate-type https://cyclonedx.org/bom
```

The SBOM is generated from the locked dependency graph during packaging. It is evidence about declared software components, not a vulnerability scan or a security certification.

## What each signal proves

| Evidence | What it helps establish | What it does not prove |
|---|---|---|
| SHA-256 checksum | The downloaded bytes match the published digest | Build origin or safety |
| GitHub provenance attestation | The artifact digest is linked to this repository's GitHub Actions build | Absence of vulnerabilities |
| CycloneDX SBOM attestation | A signed dependency inventory is bound to the artifact digest | That every component is risk-free |
| Source + reproducible review | Inspectable implementation and build instructions | Automatic trust in a binary |

## Important release scope

Direct GitHub-distribution installers are currently unsigned unless the release notes explicitly state otherwise. Microsoft Store, WinGet, code signing and other channels have separate status and must not be inferred from the existence of GitHub attestations.

See also:

- [Trust Center](TRUST-CENTER.md)
- [Release status](release-status.md)
- [Security policy](../SECURITY.md)
- [Third-party notices](../THIRD-PARTY-NOTICES.md)
