# Verifying a PermissionScope release

PermissionScope publishes several independent forms of release evidence. They answer different questions and should be used together when you need stronger assurance about a downloaded package.

Set the version you downloaded once so the examples do not depend on a hard-coded historical release:

```powershell
$version = "1.0.1" # replace with the release you downloaded
$installer = ".\PermissionScope-$version-x64-Setup.exe"
```

## 1. Verify SHA-256

Download the package and `SHA256SUMS.txt` from the **same GitHub Release**, then compare the package hash with the published value.

```powershell
Get-FileHash $installer -Algorithm SHA256
Get-Content .\SHA256SUMS.txt
```

A checksum detects byte changes. It does not by itself prove who built the file.

GitHub also exposes a SHA-256 digest for uploaded release assets in its release metadata. That is another integrity signal, not a replacement for provenance verification.

## 2. Verify GitHub build provenance

For packages built on the repository's `main` branch after provenance attestations were enabled, GitHub records a signed artifact attestation. With a current GitHub CLI:

```powershell
gh attestation verify $installer -R MukaSanches/PermissionScope
```

Use the downloaded ZIP path instead if you are verifying the portable package.

This verifies that the file digest is covered by a GitHub artifact attestation associated with this repository and its GitHub Actions build identity. It does **not** mean the software is vulnerability-free or that every Windows access outcome is guaranteed.

## 3. Verify the CycloneDX SBOM attestation

Eligible `main` builds also bind the generated CycloneDX dependency inventory to the installer and portable-package digests.

```powershell
gh attestation verify $installer `
  -R MukaSanches/PermissionScope `
  --predicate-type https://cyclonedx.org/bom
```

The SBOM is generated from the locked dependency graph during packaging. It is evidence about declared software components, not a vulnerability scan or a security certification.

## 4. Check release immutability when available

GitHub release immutability is a repository setting for **future** releases. When a PermissionScope release is marked `Immutable` on GitHub, you can verify that status with a current GitHub CLI:

```powershell
gh release verify "v$version" -R MukaSanches/PermissionScope
```

For an immutable release, you can also verify that a local file exactly matches the asset published with that release:

```powershell
gh release verify-asset "v$version" $installer -R MukaSanches/PermissionScope
```

Do not assume that every historical PermissionScope release is immutable. Check the release page or verification command for the specific tag you downloaded.

## What each signal proves

| Evidence | What it helps establish | What it does not prove |
|---|---|---|
| SHA-256 checksum | The downloaded bytes match a published digest | Build origin or safety |
| GitHub provenance attestation | The artifact digest is linked to this repository's GitHub Actions build | Absence of vulnerabilities |
| CycloneDX SBOM attestation | A signed dependency inventory is bound to the artifact digest | That every component is risk-free |
| Immutable release verification | The published release tag/assets are protected by GitHub's release immutability controls | That the software is vulnerability-free |
| Source + reproducible review | Inspectable implementation and build instructions | Automatic trust in a binary |

## Important release scope

Direct GitHub-distribution installers are currently unsigned unless the release notes explicitly state otherwise. Microsoft Store, WinGet, Authenticode/code signing and other channels have separate status and must not be inferred from the existence of GitHub attestations.

See also:

- [Trust Center](TRUST-CENTER.md)
- [Release status](release-status.md)
- [Security policy](../SECURITY.md)
- [Third-party notices](../THIRD-PARTY-NOTICES.md)
