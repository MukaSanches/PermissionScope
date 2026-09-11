# PermissionScope roadmap

PermissionScope follows evidence-first development: correctness, safety and explainability take priority over feature count.

## Now — harden 1.0.x

### Automated trust gates

- Keep x64 and ARM64 packaging reproducible from the project version/source fingerprint.
- Keep native UI Automation stable across published locales and Light/Dark themes without depending on translated button text.
- Keep a sanitized synthetic benchmark matrix in CI for regression evidence.
- Keep unsigned MSIX candidates and WinGet/channel drafts structurally validated without overstating external certification.
- Keep supply-chain verification, CodeQL, checksums, SBOM/provenance and immutable release evidence green.

### External evidence still required

- Complete physical-device validation, especially representative Windows on ARM hardware.
- Run and record Narrator/NVDA, high-scaling and complete High Contrast journeys.
- Review non-English/non-Brazilian-Portuguese native application catalogs with native speakers before claiming equivalent linguistic quality.
- Use a real trusted code-signing identity and preserve signing evidence.
- Complete Microsoft Partner Center identity, applicable Windows App Certification/Store checks and certification.
- Submit WinGet only from immutable public installers whose hashes match the final manifest.
- Invite reproducible external permission-model cases and capture validated disagreements as permanent regression tests.

## Next — make permission evidence easier to understand

- Improve Access Path as the canonical explanation surface.
- Add more synthetic teaching fixtures for inheritance, groups, deny rules and Unknown states.
- Expand comparison and investigation workflows without weakening safety boundaries.
- Improve large-tree navigation and filtering.
- Extend Academy material and troubleshooting playbooks.
- Expand evidence for remote/domain scenarios without converting unavailable context into guessed decisions.

## Later — scale and ecosystem

- Validate very large scans on controlled hardware with published methodology.
- Expand representative SMB, trusted-domain and DFS validation environments.
- Explore additional export interoperability requested by administrators and auditors.
- Improve enterprise-friendly deployment documentation without introducing a required cloud backend.
- Publish to additional trusted Windows distribution channels only when release guarantees can be maintained.
- Grow a contributor/tester community around privacy-safe synthetic validation cases.

## Evidence map

- [`docs/capability-matrix.md`](docs/capability-matrix.md) — what is Supported, Partial, Unknown by design or an external gate.
- [`docs/testing.md`](docs/testing.md) — automated verification layers.
- [`docs/benchmarks.md`](docs/benchmarks.md) — performance methodology and publication rules.
- [`docs/accessibility.md`](docs/accessibility.md) — automated accessibility evidence vs manual certification.
- [`docs/hardware-validation.md`](docs/hardware-validation.md) — physical-device validation protocol.
- [`docs/store-readiness.md`](docs/store-readiness.md) — signing, Store and WinGet gates.
- [`docs/external-validation.md`](docs/external-validation.md) — community/expert validation process.

## Non-goals

PermissionScope will not trade correctness for confident-looking guesses. It will not require a cloud account for its core local permission analysis, and the main application is intended to remain free. It will not claim certification, hardware validation or channel acceptance before the corresponding evidence exists.

Roadmap items are directional rather than promises. Security fixes and correctness regressions may reorder priorities.
