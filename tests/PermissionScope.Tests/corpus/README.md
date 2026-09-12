# PermissionScope Access Test Corpus

Version 1 covers discretionary rights for an isolated synthetic SID. The harness passes each descriptor to Windows Authz after file generic-right mapping and checks the recorded hexadecimal mask. These expectations are verified by the harness; they are not a replacement for Windows access checks in the product.

Important counterexamples: a NULL DACL is not an empty DACL; a deny for one right does not deny every right; and noncanonical ordering can make an earlier allow relevant. Do not simplify these to a rule that every deny always wins.

The synthetic context does not establish an interactive or network logon, complete group membership, conditional claims, share access or successful file opening. Add cases with a clear context and reproducible Windows evidence. Bump the corpus schema when interpretation changes.

## Test Categories

### Core Permission Evaluation
- Null vs Empty DACL behavior
- Generic rights mapping (GR, GW, GX, GA)
- Allow/Deny ACE interactions
- Inheritance flags (CI, OI, IO, NP, ID)
- Special principals (CO, CG, OW, AU, SY, WD)
- Owner implicit rights (READ_CONTROL, WRITE_DAC)

### Edge Cases
- Multiple consecutive denies
- Zero-mask ACEs
- Complex ACE ordering
- Callback and conditional ACEs
- Protected and auto-inherit flags

### Evidence and Explanation
- ACE index tracking
- Membership path resolution
- Null DACL evidence
- Owner rights evidence

### Risk Findings
- Null DACL detection
- Noncanonical ACL warnings
- Broad principal exposure
- Unresolved SID handling

### Impact Simulation
- ACE removal operations
- ACE addition operations
- Precedence maintenance
- Error handling for invalid operations

### Locale Resolution
- Exact match behavior
- Language fallback
- Regional sibling fallback
- Script-specific handling
- RTL language detection

### Capability Decisions
- FullControl capability state
- Read/Write capability subsets
- Partial capability state
- Unknown state propagation

### Performance Benchmarks
- Randomized ACL differential testing
- Deep group traversal (10,000 edges)
- Descriptor parsing throughput
