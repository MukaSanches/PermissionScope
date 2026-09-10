# Distribution

The portable folder includes the .NET runtime, Windows App SDK, GUI, worker/CLI and local dependencies. Keep it intact. The NSIS installer installs per user and requires no administrative prompt. Its uninstaller removes a generated list of installed files, not an arbitrary recursive directory, and preserves local user data.

`build/Package.ps1` publishes x64 or ARM64, gathers dependency metadata/notices, compiles Setup when NSIS is available, creates the portable ZIP and writes SHA-256 checksums. `build/Package-MSIX.ps1` creates an unsigned MSIX through Windows SDK MakeAppx. No certificate is generated or impersonated.

For Store submission, reserve the app in Partner Center, use its assigned Identity Name/Publisher values in the manifest, run Windows App Certification Kit on a supported machine and submit through the authorized account. The default `CN=Samuel Sanches` is a manifest publisher label, not proof of an existing certificate or Store reservation.

For direct MSIX distribution, supply a real code-signing certificate with a matching subject and use `signtool sign /fd SHA256` with that certificate. Timestamp using the certificate issuer's supported service. Sign the Setup executable separately for publisher verification. Unsigned MSIX cannot be installed through normal trusted sideloading without a valid signing workflow.

For WinGet, publish immutable HTTPS release assets first, then create manifests using package identifier `SamuelSanches.PermissionScope`, version `1.0.0`, installer type `nullsoft`, scope `user`, architecture-specific Setup URL and the generated SHA-256. Validate with `winget validate` and test installation/uninstallation before submitting. No nonexistent download URL or publishing credentials are included.
