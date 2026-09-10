# Third-party notices

PermissionScope source: Apache-2.0, copyright 2026 Samuel Sanches.

| Component | Purpose | License |
|---|---|---|
| .NET runtime and libraries | Managed runtime, Windows APIs | MIT and component notices |
| Windows App SDK 1.8 / WinUI 3 | Native Windows interface | Microsoft Windows App SDK software license terms; redistribution permitted for binplaced application files |
| Microsoft.Windows.SDK.NET.Ref / CsWinRT | Windows projections | MIT |
| Microsoft.Data.Sqlite.Core 10.0.12 | Local snapshots | MIT |
| SQLitePCLRaw 3.0.5 | SQLite bindings / native SQLite | Apache-2.0; SQLite public domain |
| PDFsharp 6.2.4 | Text/vector PDF reports | MIT |
| System.DirectoryServices.Protocols 10.0.12 | LDAP | MIT |
| NSIS 3.12 | Installer compiler/stub | zlib/libpng; LZMA CPL-1.0 with linking exception |

The package includes available upstream license/notice files and NuGet package manifests under `ThirdParty`, and dependency inventory in `sbom.cdx.json`. Windows fonts are used from the operating system; no font files are redistributed with the app. PDF font subsets are embedded when supported by the font path.

The Windows SDK binary distribution terms are distinct from the MIT license of parts of its source. The selected native Windows components require no per-user subscription or application feature payment. Only WinUI and its required component dependencies are referenced; the App SDK's AI, ML and Widgets metapackage components are not used.

NSIS permits distributing generated installers independently of the application's license; its explicit LZMA exception does not subject linked installer code to the CPL. No payment-related SDK, cloud service or external model is included.
