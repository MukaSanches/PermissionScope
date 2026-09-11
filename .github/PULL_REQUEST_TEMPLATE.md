## Outcome

Describe the problem and the resulting change in plain language. A reviewer should understand the intended outcome before reading the diff.

## Change class

Select one (see `GOVERNANCE.md`):

- [ ] Class 1 — low risk / documentation / repository maintenance
- [ ] Class 2 — medium risk / UI / export / performance / packaging
- [ ] Class 3 — high risk / permission decisions / identity / mutation / trust boundary

## Scope

**Changed:**

- 

**Intentionally unchanged:**

- 

## Risk and boundaries

- What Windows/security behavior can this change affect?
- What failure mode matters most?
- What does this change intentionally not claim or solve?
- Is there a rollback, migration or compatibility concern?

## Evidence

Describe the strongest evidence used to verify the result. Prefer reproducible commands, deterministic fixtures, Windows-backed comparisons and CI output over statements such as “works for me”.

```text
Paste concise verification evidence here when useful.
```

## Verification checklist

Check only what is applicable; explain intentionally skipped checks below.

- [ ] Build and executable integration harness passed.
- [ ] Calculation/security changes include a deterministic regression case and Windows-backed evidence where applicable.
- [ ] Snapshot/import/export changes fail closed on malformed or inconsistent data.
- [ ] UI changes were checked at relevant compact/wide sizes, keyboard-only navigation and relevant themes/locales.
- [ ] No credentials, production snapshots, host identifiers, confidential paths or invented results are included.
- [ ] Generated documentation was changed through its canonical source rather than edited in isolation.
- [ ] Documentation, release status and dependency/license notices match the change.
- [ ] Native-documentation evidence has fresh screenshots when source changes invalidate prior captures.
- [ ] Packaging/release changes preserve source fingerprints, checksums, SBOM/provenance and disposable-runner install testing where applicable.
- [ ] High-risk architectural changes include or update a decision record under `docs/decisions/`.

## Release impact

Select the strongest applicable statement:

- [ ] No public release impact.
- [ ] Requires documentation/evidence regeneration.
- [ ] Requires new package/release artifacts.
- [ ] Changes a documented product boundary or security assumption.

## Known limitations / untested assumptions

State what has **not** been verified. Use `None` only when that is genuinely accurate.

## Reviewer focus

Point reviewers to the files, assumptions or failure modes that deserve the most scrutiny.
