# Upstream engineering and reliability

The desktop file dialogs use the Microsoft Windows App SDK 1.8 picker API, hosted by the app's WindowId. This replaces the older Windows.Storage picker activation path and supports elevated desktop applications. The package is restored through NuGet; no unreviewed repository scripts are executed.

References consulted:

- https://github.com/microsoft/WindowsAppSDK/discussions/5810
- https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.storage.pickers?view=windows-app-sdk-1.8

Local diagnostics reproduced the generic COM error (0x800F1000) when a results control was attached to a new panel while still owned by a detached page. Input owners are now tracked explicitly instead of relying on FrameworkElement.Parent after page removal. A second ownership error in compact navigation was fixed by constructing only the chosen icon layout. The UI regression script cycles through analysis, evidence, saving, settings and compact/wide resizing four times.

Recoverable operation failures record timestamp, page, operation and exception in `%LOCALAPPDATA%\PermissionScope\last-operation-error.log`. This file stays local, may contain paths or account names and should be reviewed before sharing. The interface provides readable guidance and a separate technical-details view.

The integration harness covers actual Windows Authz decisions, the public synthetic Access Test Corpus, regional/script fallback, Unicode and long paths, junction cycles, snapshot integrity, export escaping, and guarded apply/rollback. The current machine-readable results record the executed check count. Passing these checks does not certify all Windows/domain configurations or guarantee an absence of defects.
