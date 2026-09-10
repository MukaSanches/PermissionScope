# PermissionScope 1.0.0 release checklist

This checklist is the release contract for the first public PermissionScope version. A version number alone does not make a build release-ready: the evidence below must agree with the exact commit that is tagged and published.

## 1. Source freeze

- [ ] The intended release commit is on `main`.
- [ ] `CHANGELOG.md`, `docs/release-status.md` and `docs/releases/v1.0.0.md` describe the same product boundaries.
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

- [ ] `Site compatibility` is green for Chromium, Firefox and WebKit.
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

- [ ] x64 installer and portable ZIP are produced from the current source fingerprint.
- [ ] ARM64 installer and portable ZIP are produced from the same source revision.
- [ ] x64 installer installs, launches its CLI smoke test and uninstalls on a disposable Windows environment.
- [ ] Source archive is produced.
- [ ] `SHA256SUMS.txt` covers the five public release assets.
- [ ] Package directories contain the license, notice, third-party notices, SBOM and build metadata.

ARM64 is cross-built until physical ARM hardware validation is explicitly recorded; do not describe it as hardware-certified before then.

## 6. Publication

The release workflow is intentionally manual. A dry run can be started from a branch with `publish=false`.

To publish:

1. Confirm every required check above is green on the intended commit.
2. Create the immutable tag `v1.0.0` on that exact commit.
3. Open **Actions → Release PermissionScope 1.0.0**.
4. Run the workflow from tag `v1.0.0` with `publish=true`.
5. The workflow must refuse publication from any other ref and must refuse to overwrite an existing `v1.0.0` release.

After publication:

- [ ] Download one x64 asset from the public release and verify it against `SHA256SUMS.txt`.
- [ ] Confirm the website download links resolve to the published assets.
- [ ] Confirm GitHub shows the expected release notes and provenance attestations.
- [ ] Confirm the release badge in the repository resolves to `v1.0.0`.

## 7. Store and WinGet are separate milestones

GitHub release readiness does not imply Microsoft Store or WinGet approval. Store identity, signing, certification and WinGet submission remain separate distribution gates and must be reported independently in `docs/release-status.md`.
