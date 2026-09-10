# Plain-language and technical glossary

| Term | In everyday language | Technical meaning |
|---|---|---|
| Identity | The account whose access you are checking | A Windows security principal identified by a SID |
| SID | Windows' identifier for an account or group | Security identifier; names are display metadata, not identity keys |
| Token | The security context Windows uses for a session | User SID, group SIDs and attributes, privileges and other logon context |
| ACE | One permission rule | An access-control entry with principal, rights, flags and type |
| ACL / DACL | The ordered list of access rules | Discretionary access-control list; order and NULL versus empty matter |
| Inherited | A rule Windows marks as received from above | The inherited ACE flag, not proof of a specific originating ancestor |
| Owner | The account recorded as owning the object | Owner rights can affect READ_CONTROL/WRITE_DAC; OWNER RIGHTS ACEs can restrict them |
| Mask | A compact set of allowed operations | Bitwise access rights, with generic rights mapped to file-specific rights |
| Authz | The Windows component that calculates access | AuthzAccessCheck using a supplied security descriptor and client context |
| Share | A folder exposed by a server | Share ACL and NTFS ACL are separate layers; remote token equivalence still needs verification |
| Unknown | Not enough verified context to answer | A first-class result, never converted into allowed access |
| Snapshot | A saved observation | Versioned local evidence with descriptor integrity hashes |
| Simulation | A calculation with a proposed rule change | An in-memory descriptor change; no mutation or descendant propagation |

For translators: preserve `Unknown` as uncertainty, not denial. Distinguish an ACL principal from a person and an observed inherited flag from a proven ancestor. Do not translate literal SIDs, paths, masks, CLI flags or API names.
