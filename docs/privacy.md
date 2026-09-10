# Privacy

PermissionScope does not operate an account system, telemetry endpoint, advertising integration, analytics backend or cloud storage service. There is no generative AI in the application.

Analysis reads paths, security descriptors, account identifiers, group membership and related Windows access metadata. It does not read file contents. When you request a network path or domain identity, Windows may contact that server or directory using your current Windows credentials.

Snapshots, preferences, diagnostics and rollback journals are stored under `%LOCALAPPDATA%\PermissionScope`. Reports are written only to a destination you choose. Diagnostic messages and reports may contain personal account names and confidential paths. Review them before sharing. Imported snapshots are untrusted historical observations, not proof of current access.

You can remove local observations by closing the app and removing the appropriate files from its data folder. Do not discard rollback records while an applied change may still need to be reversed. Uninstalling the app preserves these local records.

The project website contains no application analytics, advertising, tracking pixels or external fonts. Its hosting provider and GitHub receive normal web requests and may process connection metadata under their own policies. Downloading releases contacts GitHub. This is distinct from application telemetry.

Questions can be raised through the project's GitHub issues without sharing private data. For security reports, follow `SECURITY.md`.
