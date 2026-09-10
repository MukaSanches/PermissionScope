# Contributing

Start with a reproducible problem, not a broad rewrite. For permission calculations, include a small synthetic descriptor and a Windows comparison. Never commit real enterprise snapshots, credentials or personal account inventories.

## Development

Use Windows, the SDK in `global.json`, and Windows SDK build tools. Run:

```powershell
dotnet build PermissionScope.sln -c Release -p:Platform=x64
dotnet run --project tests/PermissionScope.Tests -c Release -- --results artifacts/test-results.json
```

The test project is an executable integration harness, not a `dotnet test` project. Tests modify only their own temporary fixtures. UI regression tests require an interactive Windows desktop and write real test snapshots; use a dedicated test account.

## Pull requests

- Describe the user-visible outcome and the evidence used to verify it.
- Keep native permission logic outside the UI. Preserve Unknown when context is incomplete.
- Add regression fixtures for calculation bugs; do not adjust expected masks merely to make tests pass.
- Keep UI strings in locale catalogs. Preserve placeholders, Unicode and bidirectional isolation for paths/SIDs.
- Keep dependencies minimal and disclose their licenses. No telemetry, accounts, paywalls or cloud requirement.
- Include before/after screenshots for visual changes and test compact, wide, light, dark and high-contrast layouts.

Contributions are accepted under the project's MIT license. Translation review must state which locales were actually reviewed; generated translations are not represented as human-certified.
