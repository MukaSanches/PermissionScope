# Security

Read-only is the starting mode. Analysis reads metadata, descriptors and identity information; it never reads file contents or modifies an ACL. There is no telemetry endpoint, update check, credential database or application account.

Real changes are limited to ordinary local files with one hard link. The workflow simulates the proposed descriptor, asks for the exact path, saves an observation and journal, pins the file with a handle that denies deletion/rename, checks the file identity and current descriptor hash, writes only the DACL, then rereads it. Windows enforces the caller's rights. There is no silent elevation.

The opened handle must resolve to a local volume GUID before any write. This also rejects mapped drives or parent links that reach a remote resource when Windows cannot establish a local volume. The behavior follows [GetFinalPathNameByHandleW](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getfinalpathnamebyhandlew); network shares do not have volume GUID paths.

Rollback requires the same file identity and verified post-change hash. If another actor changes the ACL, rollback refuses to overwrite it. Permission checks are optimistic: Windows does not provide an atomic compare-and-set DACL operation. Directory propagation, UNC remediation, conditional ACEs and ownership changes are excluded.

Snapshots contain potentially confidential paths, identity names, SIDs and ACLs. They are local, not encrypted by this application, and protected by the current user's Windows profile ACL. Export files use the permissions of the selected destination. Uninstall preserves snapshots and settings.

SQLite uses parameterized queries, transactions and a busy timeout. Reports are atomically replaced; HTML is escaped and CSV formula prefixes are neutralized. XLSX uses inline text cells, never formulas derived from data. Stored hashes are integrity checks, not signatures.

The local last-error log contains exception details and may include paths. Review it before sharing. Debug tools and test-only ACL changes are confined to dedicated temporary trees.
