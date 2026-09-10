# Install and analyze your first folder

Download an installer or portable ZIP from [GitHub Releases](https://github.com/MukaSanches/PermissionScope/releases). Use x64 on Intel/AMD PCs and ARM64 on Windows ARM. The portable directory must remain complete: the GUI, CLI worker, runtimes and resource files work together.

Compare the download's SHA-256 with the matching entry in the release's `SHA256SUMS.txt`:

```powershell
Get-FileHash .\PermissionScope-1.0.0-x64-Setup.exe -Algorithm SHA256
```

Current installers are unsigned. A matching hash verifies that your file matches the published artifact; it does not establish a code-signing identity. Do not disable Windows security or bypass an organizational policy to install it. Store availability is recorded separately in [release status](release-status.md).

The Setup installer installs for the current user. Open PermissionScope from the Start menu. For portable use, extract everything before opening `PermissionScope.exe`.

Choose **Explore the demo** first. The LAB environment is fictional, calculated in memory by Windows Authz, and requires no directory or account creation. Then choose **Analyze**, enter a real folder, and leave the identity empty to use your own Windows token. Select an object and read its plain-language result. Access Path shows evidence; Technical preserves the original descriptor.

Use **Save snapshot** to preserve an observation. A later scan and **Compare** reveal descriptor or evaluated-context changes. **Export report** creates an independent document. Review names and paths before sharing a real report.

Uninstall through Windows Installed apps. Saved observations may remain in `%LOCALAPPDATA%\PermissionScope`; retain them if you need the evidence. The portable edition requires no uninstall, but do not remove its directory while a scan is running.
