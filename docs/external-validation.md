# External validation program

PermissionScope benefits more from reproducible counterexamples than from praise. Administrators, Windows security practitioners, directory-service specialists and researchers are invited to challenge the decision model with privacy-safe fixtures.

## What makes a useful report

Prefer a minimal synthetic reproduction containing:

- Windows version/build and architecture;
- PermissionScope version/commit and artifact SHA-256;
- local NTFS, share/UNC, domain or other relevant topology;
- exact synthetic ACL/security descriptor or commands needed to create the fixture;
- selected identity context;
- expected Windows behavior and how that expectation was independently observed;
- PermissionScope result, including whether it returned Granted, Partial, Denied or Unknown;
- whether the disagreement affects the decision, evidence path, finding, comparison, export or UI;
- steps that a maintainer can run again.

## Privacy and security

Do not submit production ACL inventories, employee names, real customer paths, domain secrets, access tokens, private keys or confidential snapshots. Replace identifying values with synthetic accounts and paths while preserving the behavior.

Potential vulnerabilities should follow `SECURITY.md` privately rather than being posted as public validation cases.

## Evidence levels

A report can progress through these levels:

1. **Reported** — enough information to understand the claim.
2. **Reproduced** — a maintainer can reproduce it on a controlled fixture.
3. **Regression captured** — an executable test now prevents recurrence.
4. **Resolved** — implementation/documentation changed and the regression test passes.
5. **Independently confirmed** — another environment/person reproduced the fixed behavior.

The project should prefer moving cases upward through these levels instead of closing disagreements based only on discussion.

## High-value scenarios

Especially useful external validation includes:

- nested and deny-only group membership;
- SID history and primary-group behavior;
- noncanonical and inherited ACLs;
- SMB share + NTFS interactions;
- remote/domain identities where local and remote token contexts differ;
- trusted-domain referrals and large group membership;
- DFS namespaces/targets;
- conditional/special ACEs and Central Access Policy environments;
- reparse points;
- Windows-on-ARM execution;
- Narrator/NVDA and high-scaling accessibility journeys.

For areas that PermissionScope intentionally reports as Unknown, a useful test checks whether the explanation correctly identifies the missing context rather than demanding a guessed result.

## Publishing outcomes

When external evidence materially changes a trust boundary, update the capability matrix, regression tests and release notes together. Do not turn a single successful environment into a universal compatibility or certification claim.
