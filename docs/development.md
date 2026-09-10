# Development and reproducible documentation

Use Windows, .NET 10 SDK and Windows SDK build tools. Native GUI capture requires an interactive Windows desktop. The complete integration harness is executable, not a `dotnet test` adapter.

```powershell
dotnet build PermissionScope.sln -c Release -p:Platform=x64
dotnet run --project tests/PermissionScope.Tests -c Release -- --results artifacts/test-results.json
npm ci --ignore-scripts
./build/Verify-Documentation.ps1
```

To regenerate public evidence after changing the UI, translations or fixture:

```powershell
dotnet publish src/PermissionScope.App/PermissionScope.App.csproj -c Release -p:Platform=x64 -r win-x64 --self-contained true -o artifacts/documentation-app
dotnet publish src/PermissionScope.Cli/PermissionScope.Cli.csproj -c Release -r win-x64 --self-contained true -o artifacts/documentation-cli
./build/Create-DemoEnvironment.ps1
./build/Capture-Documentation.ps1
./build/Capture-Documentation.ps1 -Scenes access -Theme Dark
node build/Build-Documentation.mjs
./build/Verify-Documentation.ps1
```

`DemoFixture.cs` is the public scenario source. `docs/content/locales.json` is the source for equivalent README, guide and site copy. UI strings stay in the application's locale catalogs. Do not manually edit generated pages. `docs/screenshots/manifest.json` records source fingerprints, image hashes, locale, theme, DPI and fixture version. Captures are native `PrintWindow` output from the application's own process; no pixel replacement or fake evidence. The pipeline checks its accessibility tree for host identifiers before capture. This is not an OCR or native-speaker certification.

| Change | Required evidence |
|---|---|
| Permission engine / fixture | Windows integration tests, regenerated sample reports, screenshot freshness check |
| UI layout or locale | Native captures, visual inspection, locale checks |
| README / site copy | Regenerate documents; links, locale parity and browser accessibility tests |
| Installer | Disposable Windows CI install, CLI launch and uninstall |
| Release package | Both architectures, source fingerprint, SHA-256, SBOM, release verification |

The synthetic fixture version is `permissionscope-demo-v1`. Names, SIDs, descriptors and observation dates are fictional; no host inventory is collected. Some report formats contain generation metadata and are not byte-for-byte reproducible. No claim of bit-identical Windows builds is made.

Package with `./build/Package.ps1 -Architecture x64` and `arm64`. Run `build/Verify-Release.ps1` after source archives and checksums are generated. Stale binaries are rejected. CI install testing is restricted to disposable GitHub runners and must not overwrite a user's existing installation.
