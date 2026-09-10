# Release scope

Implemented: native WinUI GUI, shared CLI engine, Authz discretionary decisions, local file ACLs, UNC/mapped-share descriptors, conservative remote projections, token and local-group evidence, LDAP group traversal, primary-group/disabled/SID-history metadata when available, explicit Unknown states, findings, snapshots, comparison, in-memory ACE removal, guarded local-file changes and rollback, HTML/CSV/JSON/XLSX/PDF exports, daily interactive-user Task Scheduler integration, English and Brazilian Portuguese UI.

Current boundaries:

- No complete expansion of every ACL group into every person. Results identify ACL principals and evaluate one selected context per scan.
- Remote logon equivalence, trusted-domain coverage, DFS target discovery/divergence and central access policy are not implemented as certified effective-access results. UNC projections remain Unknown.
- Inherited ACEs retain Windows flags; the exact ancestor and depth are not established. Group membership range continuation and trust referrals are not complete.
- Simulation removes one ACE for the selected context. GUI addition/editing, group changes and descendant propagation are not included. Actual remediation is restricted to ordinary local files.
- Scans materialize their results. There is no resumable million-object persistent stream. A worker terminated by cancellation does not save unfinished observations.
- PDF direct export supports Latin, Greek and Cyrillic text. Complex scripts, CJK and emoji require the HTML report and the browser's Print to PDF. XLSX/JSON/HTML preserve Unicode.
- Comparison shows descriptor and evaluated-context changes where comparable; it does not calculate the impact on every member of a changed group.
- Scheduling is daily and uses the signed-in user's session. Weekly/custom schedules and retention can be configured in Windows Task Scheduler; no scheduler-management UI is included.
- English and Brazilian Portuguese catalogs ship alongside six explicitly labeled translation previews. Regional/script fallback and RTL culture resolution are tested. Technical evidence/report translation, exhaustive high-contrast testing, screen-reader certification, full worldwide translation and all requested identity filters remain incomplete. See translation-quality.md for coverage and review status.
- Direct-distribution artifacts are unsigned. MSIX requires a real publisher identity and signing certificate or Store ingestion. No Store/WinGet submission has been performed.

Unknown results preserve the available evidence and explain which context is missing. Validate domain and server behavior in a representative environment before using the application as an enterprise audit authority.
