# Questions and troubleshooting

## Does this change my permissions?

Analysis, export and simulation do not. Apply is a separate workflow for ordinary local files, with an exact-path confirmation, saved state, durable journal, Windows verification and rollback. It refuses directories, reparse points and files with multiple hard links. Demo mode cannot apply changes.

## Does Granted mean I can open a file?

It means the evaluated discretionary rules allow the named capability. Integrity policy, encryption, locks, parent traversal, privileges and other restrictions can still affect a real open operation. Read the scope next to the result.

## Why is a network folder Unknown?

Reading its share and NTFS descriptors is not the same as reproducing the server's logon token. Server-local groups, claims, DFS target policies and the authentication context can differ. The local projections remain inspectable; they are not promoted to a verified remote result.

## Does an unresolved SID mean the account was deleted?

No. Name resolution can fail because of connectivity, directory visibility, trust boundaries or unavailable services. The SID is preserved as evidence.

## Why does the ACL list fewer entries than the number of employees?

An ACL contains principals such as groups. PermissionScope evaluates one selected context. It does not claim to enumerate every person transitively represented by every group.

## Can a deny always override an allow?

Order, masks, inheritance and the applicable token matter. PermissionScope asks Windows Authz and preserves entry order. It does not reduce Windows access checks to a blanket deny-wins rule.

## A scan fails or a folder cannot be read

Confirm that the path exists and the current account is authorized to read its descriptor. For UNC paths, confirm connectivity to the server. Do not assume that running as administrator proves another user's access. Inspect the error code, try a small authorized local folder, and include the safe diagnostic from Technical in an issue.

## The application fails to start from a ZIP

Extract the full directory to a local location. Do not run only the EXE copied out of the archive. Confirm Windows 10 1809 or newer and the correct CPU architecture. Record the version and Windows error before reinstalling.

## Why does PDF export reject Arabic, Japanese or Chinese paths?

The direct PDF text layout supports Latin, Greek and Cyrillic, not complex-script shaping. Export HTML and print to PDF in a browser for those scripts. HTML, JSON and XLSX preserve Unicode. This limitation is intentional to avoid silently broken glyphs.

## Can I submit a real report with a bug?

Prefer the synthetic demo or a minimal fabricated SDDL case. Real reports contain resource paths, names, SIDs and security descriptors. The safe diagnostic deliberately excludes these fields; copying full evidence is a different action.

## Is this certified for my enterprise or screen reader?

No certification is claimed. Automated checks, tested Windows cases and known boundaries are documented. Validate your directory topology, server behavior and assistive technology in a representative environment.
