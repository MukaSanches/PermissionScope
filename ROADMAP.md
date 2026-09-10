# PermissionScope roadmap

PermissionScope follows evidence-first development: correctness, safety and explainability take priority over feature count.

## Now — harden 1.0.x

- Keep x64 and ARM64 packaging reproducible.
- Expand physical-device validation, especially Windows on ARM.
- Add native UI automation for critical end-to-end flows.
- Expand controlled scan benchmarks and GUI responsiveness measurements.
- Continue accessibility validation and native-speaker review of language packs.
- Prepare Microsoft Store packaging, identity, WACK validation and certification notes.
- Strengthen supply-chain verification and provenance.

## Next — make permission evidence easier to understand

- Improve Access Path as the canonical explanation surface.
- Add more synthetic teaching fixtures for inheritance, groups, deny rules and Unknown states.
- Expand comparison and investigation workflows without weakening safety boundaries.
- Improve large-tree navigation and filtering.
- Extend Academy material and troubleshooting playbooks.

## Later — scale and ecosystem

- Validate very large scans on controlled hardware.
- Explore additional export interoperability requested by administrators and auditors.
- Improve enterprise-friendly deployment documentation without introducing a required cloud backend.
- Publish to additional trusted Windows distribution channels only when release guarantees can be maintained.

## Non-goals

PermissionScope will not trade correctness for confident-looking guesses. It will not require a cloud account for its core local permission analysis, and the main application is intended to remain free.

Roadmap items are directional rather than promises. Security fixes and correctness regressions may reorder priorities.
