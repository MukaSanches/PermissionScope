## Outcome

Describe the change and the user-visible result in plain language.

## Risk and boundaries

- What Windows/security behavior can this change affect?
- What does this change intentionally not claim or solve?
- Is there a rollback or compatibility concern?

## Verification

- [ ] Build and executable integration harness passed.
- [ ] Calculation/security changes include a deterministic regression case and Windows-backed evidence where applicable.
- [ ] Snapshot/import/export changes fail closed on malformed or inconsistent data.
- [ ] UI changes were checked at compact/wide sizes, keyboard-only navigation and the relevant themes/locales.
- [ ] Website changes pass Chromium, Firefox, WebKit, accessibility and horizontal-overflow checks.
- [ ] No credentials, private snapshots, host identifiers or invented results are included.
- [ ] Generated READMEs/site pages were regenerated from their source instead of edited by hand.
- [ ] Documentation, release status and dependency/license notices match the change.
- [ ] Source changes that affect native documentation evidence have fresh screenshots; the manifest fingerprint was not manually advanced.
- [ ] Packaging/release changes preserve source fingerprints, checksums, SBOM/provenance and disposable-runner install testing.

## Release impact

Select one:

- [ ] No public release impact.
- [ ] Requires documentation/evidence regeneration.
- [ ] Requires new package/release artifacts.
- [ ] Changes a documented product boundary or security assumption.
