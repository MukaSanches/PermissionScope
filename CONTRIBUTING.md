# Contributing to PermissionScope

PermissionScope welcomes focused contributions that improve correctness, safety, clarity, accessibility or maintainability. Start with a reproducible problem; avoid broad rewrites whose value cannot be verified independently.

## Before you start

Use the repository's issue forms for bugs and feature proposals. Security-sensitive findings must follow [`SECURITY.md`](SECURITY.md) instead of a public issue.

For substantial architectural or trust-boundary changes, read [`GOVERNANCE.md`](GOVERNANCE.md) and the decision-record guidance in [`docs/decisions/`](docs/decisions/).

Never commit real enterprise permission inventories, credentials, private keys, tokens, confidential paths or unredacted production snapshots.

## Development environment

Use Windows, the SDK pinned by `global.json`, and the Windows SDK/build tools required by the solution.

Core verification:

```powershell
dotnet build PermissionScope.sln -c Release -p:Platform=x64
dotnet run --project tests/PermissionScope.Tests -c Release -- --results artifacts/test-results.json
```

The test project is an executable integration harness, not a `dotnet test` project. Tests modify only their own temporary fixtures. UI regression tests require an interactive Windows desktop and should run under a dedicated test account.

## Contribution workflow

1. Reproduce or define the problem with the smallest safe example.
2. Classify the change using the risk classes in [`GOVERNANCE.md`](GOVERNANCE.md).
3. Create a focused branch.
4. Make the smallest coherent change that solves the problem.
5. Add or update deterministic verification.
6. Update only the documentation that the change actually affects.
7. Open a pull request using the repository template and state limitations explicitly.

Suggested branch prefixes:

- `fix/` — defect correction;
- `feat/` — product capability;
- `docs/` — documentation-only work;
- `test/` — test/fixture work;
- `chore/` — repository/build maintenance;
- `security/` — security work only when public disclosure is appropriate.

## Engineering rules

- Keep native permission logic outside the UI layer.
- Preserve **Unknown** whenever the available Windows context cannot establish an answer.
- Do not change expected permission masks merely to make a failing regression test pass.
- Prefer synthetic descriptors/fixtures over private real-world data.
- Keep dependencies minimal and disclose their licenses.
- Do not introduce telemetry, mandatory accounts, paywalls or a required cloud backend into the core product.
- Treat exports, snapshots and diagnostics as potentially sensitive data.
- Keep source, package and release claims aligned with what CI and physical testing actually verify.

## UI and localization

- Keep user-facing strings in the localization system rather than hard-coding them in view logic.
- Preserve placeholders, Unicode and bidirectional isolation for paths, SIDs and identity text.
- For visual changes, test relevant compact/wide layouts, keyboard navigation, light/dark/high-contrast behavior and affected locales.
- Do not claim native-speaker review or assistive-technology certification unless it actually occurred.

## Documentation

Generated localized READMEs and generated site material must be changed through their canonical source, not edited in isolation. A contribution should not create documentation drift merely to improve presentation in one locale.

Factual claims should be written conservatively. Prefer “tested on”, “supported by this workflow” or “not yet certified” over vague assurance language.

## Pull request quality bar

A reviewer should be able to identify, without reconstructing the whole history:

- the problem being solved;
- the user-visible or engineering outcome;
- the risk class;
- the evidence used to verify the change;
- known limitations or untested assumptions;
- whether release artifacts or documentation need regeneration.

Keep pull requests narrow enough that those questions have clear answers.

## Commit quality

Use descriptive commit messages that explain the decision or outcome rather than narrating editor actions. Examples:

```text
Preserve Unknown for incomplete remote token context
Add regression fixture for inherited deny precedence
Clarify unsigned installer verification guidance
```

Avoid generic messages such as `update`, `fix stuff` or `changes`.

## Review and acceptance

Green automation is necessary evidence, not an automatic right to merge. Contributions can be declined because of security risk, unclear scope, unsupported product direction, weak maintainability or insufficient verification.

For high-risk changes, expect focused review of failure modes and a durable decision record when the architecture or trust model changes.

## License and conduct

Contributions are accepted under the project's Apache-2.0 license. Participation is also subject to [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).

Translation contributions should state which locales were actually reviewed. Machine-generated or unreviewed translations must not be represented as human-certified.
