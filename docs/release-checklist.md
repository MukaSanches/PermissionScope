# PermissionScope release checklist

This checklist is the release contract for PermissionScope. A version number alone does not make a build release-ready: the evidence below must agree with the exact commit that is tagged and published. The current release line is 1.0.1; the existing 1.0.0 tag/release is historical and must not be moved or overwritten.

## 1. Source freeze

- [ ] The intended release commit is on `main`.
- [ ] `Directory.Build.props`, `CHANGELOG.md`, `CITATION.cff`, `docs/release-status.md` and the matching `docs/releases/v<version>.md` describe the same version and boundaries.
- [ ] No generated README, localized site page or handbook was edited by hand.
- [ ] Dependency lock files are current and `npm ci` / NuGet restore report no unresolved audit failure.

## 2. Windows correctness

Run on Windows:

```powershell
./build/Test-ReleaseReadiness.ps1
```

Required result:

- [ ] x64 solution build succeeds with zero errors.
- [ ] The executable integration harness finishes with zero failed cases.
- [ ] Real NTFS, Authz, ACL ordering, Unknown-state, snapshot, export and remediation regression cases pass.
- [ ] Website regression and accessibility checks pass with zero Axe violations.
- [ ] Documentation parity, links, locale resources and screenshot provenance pass.

If the screenshot check says that the source is stale, do not replace only the manifest fingerprint. Regenerate the native captures from the current source on an interactive Windows desktop:

```powershell
dotnet publish src/PermissionScope.App/PermissionScope.App.csproj -c Release -p:Platform=x64 -r win-x64 --self-contained true -o artifacts/documentation-app
dotnet publish src/PermissionScope.Cli/PermissionScope.Cli.csproj -c Release -r win-x64 --self-contained true -o artifacts/documentation-cli
./build/Create-DemoEnvironment.ps1
./build/Capture-Documentation.ps1
./build/Capture-Documentation.ps1 -Scenes access -Theme Dark
node build/Build-Documentation.mjs
./build/Verify-Documentation.ps1
```

- [ ] All regenerated images were visually inspected for clipping, incorrect locale, stale values or host/private data.

## 3. Cross-platform website evidence

- [ ] Site compatibility is green for Chromium, Firefox and WebKit when the workflow is applicable to the release commit.
- [ ] 320 px mobile and short-landscape layouts have no horizontal overflow.
- [ ] Keyboard skip link works.
- [ ] Forced-colors checks pass.
- [ ] No external request is required for the static website to function.

## 4. Security and trust

- [ ] CodeQL is green on the exact release commit.
- [ ] Imported snapshots fail closed when integrity checks fail.
- [ ] Remote, conditional, incomplete or unverified contexts remain `Unknown` instead of being promoted to a confident access result.
- [ ] Security-sensitive reports use the private disclosure path in `SECURITY.md` rather than public issues.
- [ ] Direct installers are still described as unsigned until a real signing/Store publisher identity exists.

## 5. Packaging

For the full local verification path:

```powershell
./build/Test-ReleaseReadiness.ps1 -Full
```

Required result:

- [ ] x64 installer and portable ZIP are produced from the current source fingerprint and project version.
- [ ] ARM64 installer and portable ZIP are produced from the same source revision/version.
- [ ] x64 installer installs, runs its CLI smoke test and uninstalls on a disposable Windows environment.
- [ ] Source archive is produced for the same version.
- [ ] `SHA256SUMS.txt` covers the five public release assets.
- [ ] Package directories contain the license, notice, third-party notices, CycloneDX SBOM and build metadata.
- [ ] Provenance and SBOM attestations succeed for the release packages.

ARM64 is cross-built until physical ARM hardware validation is explicitly recorded; do not describe it as hardware-certified before then.

## 6. Publication

For 1.0.1, `.github/workflows/release.yml` is the guarded publisher. Its pull-request run is a dry run and must not create a tag or release. After the release preparation is merged to `main`, the workflow:

1. rebuilds/tests the exact merge commit;
2. packages x64, ARM64 and source assets;
3. assembles combined SHA-256 checksums;
4. waits for the normal `Windows build` and `CodeQL` push workflows for the same SHA to succeed;
5. confirms `main` has not advanced;
6. refuses to move an existing `v1.0.1` tag or overwrite an existing public release;
7. creates `v1.0.1` on that exact commit and publishes the release assets/notes.

After publication:

- [ ] Tag `v1.0.1` resolves to the exact validated commit.
- [ ] GitHub Release `PermissionScope 1.0.1` is public, not draft/prerelease.
- [ ] Five versioned release assets plus `SHA256SUMS.txt` are present.
- [ ] Download one x64 asset from the public release and verify it against `SHA256SUMS.txt`.
- [ ] Confirm the website release-download link resolves to the published release.
- [ ] Confirm GitHub shows the expected release notes and package attestations.

## 7. Store and WinGet are separate milestones

GitHub release readiness does not imply Microsoft Store or WinGet approval. Store identity, signing, certification and WinGet submission remain separate distribution gates and must be reported independently in `docs/release-status.md`.
