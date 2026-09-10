# Architecture

`Core` contains immutable observations, descriptors, rights mapping, evidence construction, graph traversal, comparison, simulation, deterministic findings, SQLite storage and reports. `Windows` owns the native API boundary, Authz context lifetime, LDAP, SMB, scan worker protocol and narrowly scoped remediation. `App` owns WinUI composition and localization. `Cli` invokes the same scanner and calculation classes.

The Release GUI starts a separate CLI scan process and reads JSON messages over redirected pipes. The process isolates blocking native network calls; cancellation terminates only that worker. The GUI remains available. A forcibly cancelled scan does not persist its unfinished observations. Debug can run the engine directly when a CLI has not been built.

Native memory and Authz contexts use SafeHandle wrappers. Authz handles have session scope. SID names and share descriptors are cached per scan. LDAP graph searches use signed/sealed Windows authentication, a request timeout and cycle protection.

Traversal skips reparse targets, records per-object failures and materializes the finished observation. It is not an unlimited-memory streaming database scanner. The result list renders one 150-object page at a time. SQLite lists are paginated; snapshot payloads are transactional JSON with a checksum. The schema is versioned and newer schemas are rejected.
