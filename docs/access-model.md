# Access model

## Decision

The result is **discretionary file access for a selected security context**, not a guarantee that opening a file succeeds. The engine reads owner, primary group and DACL using `GetNamedSecurityInfo` or `GetSecurityInfo`. Generic file rights are expanded before `AuthzAccessCheck(MAXIMUM_ALLOWED)`. Authz evaluates stored ACE ordering; the application does not reorder input.

The current user is evaluated with `AuthzInitializeContextFromToken`, including enabled and deny-only group attributes. Other identities require an S4U context. These are projections and remain Unknown because actual logon characteristics may differ. Isolated-SID contexts are used only by differential tests.

`Granted` at the aggregate level means all supported file rights; `Partial` means a non-empty proper subset; `Denied` means no supported rights. Each capability has its own decision. `Unknown` prevents capabilities from being reported as granted or denied. The raw projected masks remain available for inspection.

A NULL DACL grants discretionary access. An empty DACL grants nothing to a non-owner without applicable special rights. Authz handles implicit owner rights and OWNER RIGHTS entries. Administrative privilege override is not reported as ordinary ACL access.

Integrity labels, central access policy, claims, encrypted content, parent traversal, deletion allowed by a parent's DELETE_CHILD right, sharing violations, privilege-based opens and actual server authentication are outside the decision. A Delete denial describes this object's DACL; it does not rule out deletion through the parent. Conditional/object-specific ACEs are detected and labeled Unknown. The engine does not inspect the SACL or certify Dynamic Access Control equivalence.

## SMB and identity evidence

`NetShareGetInfo(502)` reads share permissions and the backing local path. Mapped drives are resolved through `WNetGetUniversalName`. File and share masks are computed and intersected as projections; the remote context remains Unknown. Server-local groups, DFS target divergence and cross-trust logons are not certified.

`NetLocalGroupGetMembers` provides real local-group edges. LDAP `memberOf` provides direct group edges; `primaryGroupID`, account-disabled state and SID history are recorded where available. Parent groups are traversed with cycle detection. Ranged memberships, referrals and unreachable domains generate warnings. SID history is recorded as history, never fabricated as group membership. A transitive token SID is identified as token inclusion, not as a direct member-of edge.

## Access Path

Each ACE evidence item records the index, SID, original mask, flags, resource and relevant contributed bits. Allow and deny paths can contribute independently. Windows' inherited flag is shown, but the originating ancestor is not claimed without proof. Owner and NULL-DACL evidence are separate. Access Path is an explanation of the checked context, not a complete enumeration of every person.

## Stored observations

Snapshot checksums detect accidental corruption, not malicious authorship. Imported JSON is untrusted historical evidence; it is not a signed attestation. Remediation rereads the live descriptor and never authorizes a write solely from imported decision text.

References: [Authz context creation](https://learn.microsoft.com/windows/win32/api/authz/nf-authz-authzinitializecontextfromsid), [remote access evaluation](https://learn.microsoft.com/troubleshoot/windows-server/windows-security/access-checks-windows-apis-return-incorrect-results), [file security rights](https://learn.microsoft.com/windows/win32/fileio/file-security-and-access-rights).
