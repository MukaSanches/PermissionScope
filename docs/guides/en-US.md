# PermissionScope · Visual walkthrough

[Choose your next step](../../README.md)

LAB\Alex receives Modify through LAB\Finance. These are native application captures using a synthetic Authz fixture, not a real company or altered results.

## home

Choose Explore the demo to learn safely with LAB\Alex. For your files, choose Analyze and enter a folder.

![Choose Explore the demo to learn safely with LAB\Alex. For your files, choose Analyze and enter a folder.](../screenshots/en-US/home-light.png)

## analyze

Leave the identity blank to use your current Windows token. Open Access Path to inspect the evidence.

![Leave the identity blank to use your current Windows token. Open Access Path to inspect the evidence.](../screenshots/en-US/analyze-light.png)

## access

Granted allows the named action under the evaluated discretionary rules. Partial means only some actions are allowed. Denied means those rules grant no access. Unknown means the available context cannot confirm the answer; it is never treated as granted.

![Granted allows the named action under the evaluated discretionary rules. Partial means only some actions are allowed. Denied means those rules grant no access. Unknown means the available context cannot confirm the answer; it is never treated as granted.](../screenshots/en-US/access-light.png)

## access-path

Windows Authz calculates the mask. Access Path shows contributing entries and recorded membership evidence. Inherited flags do not prove the originating ancestor. Full control in the discretionary ACL does not guarantee a successful file open.

![Windows Authz calculates the mask. Access Path shows contributing entries and recorded membership evidence. Inherited flags do not prove the originating ancestor. Full control in the discretionary ACL does not guarantee a successful file open.](../screenshots/en-US/access-path-light.png)

## compare

Save local snapshots, compare observations, simulate removing a permission entry in memory, and export HTML, CSV, JSON, XLSX or PDF. Missing resources in a later observation are not assumed deleted.

![Save local snapshots, compare observations, simulate removing a permission entry in memory, and export HTML, CSV, JSON, XLSX or PDF. Missing resources in a later observation are not assumed deleted.](../screenshots/en-US/compare-light.png)

## simulation

Analysis and simulation are read only. Applying a change is a separate, explicitly confirmed workflow for ordinary local files, with a saved observation, durable journal, verification and rollback. Directories, links and multi-link files are excluded.

![Analysis and simulation are read only. Applying a change is a separate, explicitly confirmed workflow for ordinary local files, with a saved observation, durable journal, verification and rollback. Directories, links and multi-link files are excluded.](../screenshots/en-US/simulation-light.png)

## technical

Include the version, Windows version, operation and error code. The Technical tab can copy a diagnostic without paths, account names or SIDs. Language packs are previews; technical evidence can remain in English. Native speaker review and assistive-technology certification are not claimed.

![Include the version, Windows version, operation and error code. The Technical tab can copy a diagnostic without paths, account names or SIDs. Language packs are previews; technical evidence can remain in English. Native speaker review and assistive-technology certification are not claimed.](../screenshots/en-US/technical-light.png)

## unknown

Remote logons, S4U contexts, conditional rules and unverified reparse targets remain Unknown. Integrity policy, encryption, locks and administrative privileges are outside this decision. No application telemetry, accounts or cloud service. Paths and account names in real exports can be sensitive.

![Remote logons, S4U contexts, conditional rules and unverified reparse targets remain Unknown. Integrity policy, encryption, locks and administrative privileges are outside this decision. No application telemetry, accounts or cloud service. Paths and account names in real exports can be sensitive.](../screenshots/en-US/unknown-light.png)

[Development](../development.md) · [FAQ and troubleshooting](../faq.md) · [Release status](../release-status.md)
