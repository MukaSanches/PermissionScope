# PermissionScope Access Test Corpus

Version 1 covers discretionary rights for an isolated synthetic SID. The harness passes each descriptor to Windows Authz after file generic-right mapping and checks the recorded hexadecimal mask. These expectations are verified by the harness; they are not a replacement for Windows access checks in the product.

Important counterexamples: a NULL DACL is not an empty DACL; a deny for one right does not deny every right; and noncanonical ordering can make an earlier allow relevant. Do not simplify these to a rule that every deny always wins.

The synthetic context does not establish an interactive or network logon, complete group membership, conditional claims, share access or successful file opening. Add cases with a clear context and reproducible Windows evidence. Bump the corpus schema when interpretation changes.

## Test Categories

### Core Permission Evaluation (26 tests)
- Null vs Empty DACL behavior
- Generic rights mapping (GR, GW, GX, GA)
- Allow/Deny ACE interactions and accumulation
- Multiple consecutive denies
- Allow after partial deny restoration
- Complex ACE ordering (Windows first-applicable-deny semantics)
- Zero-mask ACE handling
- Bitwise OR accumulation from same SID
- Owner implicit rights (READ_CONTROL, WRITE_DAC)
- Special principals (CO, CG, OW, AU, SY, WD, BA, NU, IU, AN, Proxy)
- Inheritance flags (CI, OI, IO, NP, ID, CICO, IOIO, CIFPIOISA)
- Protected (P) and auto-inherit (S) flags
- Descriptor parsing and hash determinism

### Owner Rights (OW) Specific (4 tests)
- Owner Rights SID overrides implicit owner permissions
- Owner Rights allow/deny auditable evidence
- Owner Rights ACE applicability to non-owners

### Inheritance Flag Combinations (5 tests)
- CI (Container Inherit) alone
- OI (Object Inherit) alone
- ID (Inherit Only) prevention
- NP (No Propagate) with CI
- Multiple inheritance flags combined

### Special Principal Tests (6 tests)
- Local Admins (BA)
- Network (NU)
- Interactive (IU)
- Service SID
- Anonymous Logon (AN)
- Proxy SID

### Standard Rights Combinations (4 tests)
- READ_CONTROL implicit grant to owner
- WRITE_DAC implicit grant to owner
- SYNCHRONIZE handling
- Access to descriptor owner without explicit ACE

### ACE Type Variations (4 tests)
- AccessAllowedCallback (XA) unsupported marking
- AccessDeniedCallback (XD) unsupported marking
- SystemAudit (S) parsing without access effect
- SystemAlarm (Z) parsing without access effect

### Malformed and Edge Case Descriptors (6 tests)
- Descriptor with only owner/group, no DACL
- Empty ACE list validation
- Single ACE parsing
- ACE index preservation
- Hash determinism
- Hash differentiation

### Evidence and Explanation (5 tests)
- ACE index tracking in evidence
- Deny relation evidence
- Membership path resolution
- Null DACL evidence with Everyone principal
- Owner rights evidence relation

### Risk Findings (8 tests)
- Null DACL CriticalExposure detection
- Noncanonical ACL Review finding
- Protected DACL Review finding
- Special ACE Review finding
- Broad Allow to World (CriticalExposure)
- Broad Allow to Authenticated Users (HighExposure)
- Unresolved SID Review finding
- Multiple findings on same descriptor

### Impact Simulation (9 tests)
- Remove first/last ACE operations
- Add allow ACE positioning
- Add deny ACE precedence
- Precedence maintenance
- Error handling: empty DACL removal
- Error handling: negative index
- Error handling: out-of-range index
- NULL DACL conversion requirement

### Locale Resolution (12 tests)
- Exact match behavior
- Language fallback
- Regional sibling fallback
- Regional formatting preservation
- Arabic RTL detection
- Persian RTL with English fallback
- Chinese script-specific handling (Traditional/Simplified)
- Script substitution prevention
- Japanese exact match
- Korean base language fallback
- Unavailable locale fallback
- Empty available list exception

### Capability Decisions (6 tests)
- FullControl capability state
- Read capability subsets
- Write capability subsets
- Partial capability state
- Unknown state propagation
- Zero mask Denied capabilities

### Share and NTFS Intersection (4 tests)
- Minimum intersection rule
- Null share mask (NTFS only)
- Remote context Unknown state
- Reparse point Unknown state

### Performance Benchmarks (3 tests)
- 500 randomized ACL differential testing
- Deep 10,000-group traversal efficiency
- Cycle detection in group graph

## Implementation Notes

### Test Structure
All tests follow the pattern: `Test("Name", () => Assert(condition))` or `await TestAsync("Name", async () => ...)`. Failures are collected and reported at the end with timing information.

### Fixtures
- `fixtureSid`: Synthetic SID "S-1-5-21-111111111-222222222-333333333-1001" used consistently across tests
- `Sddl(string aces)`: Helper function to build complete SDDL strings with standard owner/group
- `corpus/discretionary-v1.json`: JSON-based test cases for corpus-driven testing

### Assertion Style
- Explicit boolean conditions with optional message parameter
- Hexadecimal masks for permission bits (e.g., `0x60000` for READ_CONTROL | WRITE_DAC)
- LINQ expressions for collection assertions (`.Any()`, `.Single()`, `.All()`)
- Exception expectation via `Throws<T>()` helper

### Key Constants
- `Rights.Read`, `Rights.Write`, `Rights.FullControl`: Predefined permission masks
- `AccessState.Granted`, `AccessState.Denied`, `AccessState.Partial`, `AccessState.Unknown`
- `Severity.CriticalExposure`, `Severity.HighExposure`, `Severity.Review`

## Running Tests

Execute with `dotnet test` on Windows with .NET SDK. Use `--results <path>` argument to generate JSON report with failures and benchmarks.

## Adding New Tests

When adding tests:
1. Follow existing naming conventions (technical English, descriptive)
2. Include inline comments explaining complex scenarios
3. Group related tests with section comment delimiters
4. Update this README with new categories
5. Ensure tests are deterministic and isolated
6. Use `fixtureSid` for consistency unless testing SID mismatch explicitly
7. Document any Windows version dependencies
