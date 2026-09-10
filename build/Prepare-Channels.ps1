param([string]$Repository='MukaSanches/PermissionScope', [switch]$VerifyPublished)
$ErrorActionPreference='Stop'
if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') { throw 'Invalid GitHub repository identifier.' }
$root=[System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$output=Join-Path $root 'artifacts/channels'
$winget=Join-Path $output 'winget/SamuelSanches.PermissionScope/1.0.0'
$chocolatey=Join-Path $output 'chocolatey'
New-Item -ItemType Directory -Path $winget,(Join-Path $chocolatey 'tools') -Force | Out-Null
$release="https://github.com/$Repository/releases/download/v1.0.0"
$hashes=@{}
foreach ($architecture in @('x64','arm64')) {
    $filename="PermissionScope-1.0.0-$architecture-Setup.exe"
    $hashes[$architecture]=(Get-FileHash -LiteralPath (Join-Path $root "artifacts/$filename") -Algorithm SHA256).Hash
    if ($VerifyPublished) {
        $download=Join-Path $output $filename
        Invoke-WebRequest -UseBasicParsing -Uri "$release/$filename" -OutFile $download
        if ((Get-FileHash -LiteralPath $download).Hash -ne $hashes[$architecture]) { throw "Published installer mismatch: $architecture" }
    }
}
$version=@'
PackageIdentifier: SamuelSanches.PermissionScope
PackageVersion: 1.0.0
DefaultLocale: en-US
ManifestType: version
ManifestVersion: 1.6.0
'@
$locale=@"
PackageIdentifier: SamuelSanches.PermissionScope
PackageVersion: 1.0.0
PackageLocale: en-US
Publisher: Samuel Sanches
PublisherUrl: https://github.com/$Repository
PackageName: PermissionScope
PackageUrl: https://github.com/$Repository
License: MIT
LicenseUrl: https://github.com/$Repository/blob/main/LICENSE
Copyright: Copyright 2026 Samuel Sanches
ShortDescription: Native Windows permission analysis with inspectable access evidence.
Moniker: permissionscope
Tags:
  - permissions
  - ntfs
  - windows
  - access-control
ReleaseNotesUrl: https://github.com/$Repository/releases/tag/v1.0.0
ManifestType: defaultLocale
ManifestVersion: 1.6.0
"@
$installer=@"
PackageIdentifier: SamuelSanches.PermissionScope
PackageVersion: 1.0.0
InstallerType: nullsoft
Scope: user
MinimumOSVersion: 10.0.17763.0
InstallModes:
  - interactive
  - silent
InstallerSwitches:
  Silent: /S
  SilentWithProgress: /S
  InstallLocation: /D=<INSTALLPATH>
AppsAndFeaturesEntries:
  - DisplayName: PermissionScope
    Publisher: Samuel Sanches
    DisplayVersion: 1.0.0
Installers:
  - Architecture: x64
    InstallerUrl: $release/PermissionScope-1.0.0-x64-Setup.exe
    InstallerSha256: $($hashes['x64'])
  - Architecture: arm64
    InstallerUrl: $release/PermissionScope-1.0.0-arm64-Setup.exe
    InstallerSha256: $($hashes['arm64'])
ManifestType: installer
ManifestVersion: 1.6.0
"@
$version | Set-Content -LiteralPath (Join-Path $winget 'SamuelSanches.PermissionScope.yaml') -Encoding UTF8
$locale | Set-Content -LiteralPath (Join-Path $winget 'SamuelSanches.PermissionScope.locale.en-US.yaml') -Encoding UTF8
$installer | Set-Content -LiteralPath (Join-Path $winget 'SamuelSanches.PermissionScope.installer.yaml') -Encoding UTF8
$nuspec=@"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd"><metadata>
<id>permissionscope</id><version>1.0.0</version><title>PermissionScope</title><authors>Samuel Sanches</authors><owners>Samuel Sanches</owners>
<projectUrl>https://github.com/$Repository</projectUrl><licenseUrl>https://github.com/$Repository/blob/main/LICENSE</licenseUrl><requireLicenseAcceptance>false</requireLicenseAcceptance>
<summary>Native Windows permission analysis with inspectable evidence.</summary>
<description>Free, local Windows permission analysis, snapshots, comparison and simulation. Uses Windows Authz for discretionary rights and explicit Unknown states for unverified context. This package installs the unsigned x64 application per user. ARM64 is available from the project release.</description>
<tags>windows permissions ntfs admin</tags><releaseNotes>https://github.com/$Repository/releases/tag/v1.0.0</releaseNotes>
</metadata><files><file src="tools\**" target="tools" /></files></package>
"@
$nuspec | Set-Content -LiteralPath (Join-Path $chocolatey 'permissionscope.nuspec') -Encoding UTF8
$installScript=@'
$ErrorActionPreference = 'Stop'
if (-not [Environment]::Is64BitOperatingSystem) { throw '64-bit Windows is required.' }
$arguments = @{
    packageName = $env:ChocolateyPackageName
    fileType = 'exe'
    url64bit = '__URL__'
    checksum64 = '__HASH__'
    checksumType64 = 'sha256'
    silentArgs = '/S'
    validExitCodes = @(0)
}
Install-ChocolateyPackage @arguments
'@
$installScript.Replace('__URL__',"$release/PermissionScope-1.0.0-x64-Setup.exe").Replace('__HASH__',$hashes['x64']) | Set-Content -LiteralPath (Join-Path $chocolatey 'tools/chocolateyinstall.ps1') -Encoding UTF8
$uninstallScript=@'
$ErrorActionPreference = 'Stop'
$entries = @(Get-UninstallRegistryKey -SoftwareName 'PermissionScope')
if ($entries.Count -eq 0) { return }
if ($entries.Count -ne 1 -or $entries[0].UninstallString -notmatch '^"([^"]+Uninstall\.exe)"$') { throw 'Ambiguous uninstall registration; uninstall through Windows Settings.' }
Uninstall-ChocolateyPackage -PackageName $env:ChocolateyPackageName -FileType exe -SilentArgs '/S' -File $Matches[1] -ValidExitCodes @(0)
'@
$uninstallScript | Set-Content -LiteralPath (Join-Path $chocolatey 'tools/chocolateyuninstall.ps1') -Encoding UTF8
@{ prepared=$true; publishedInstallersVerified=[bool]$VerifyPublished; wingetValidated=$false; wingetSubmitted=$false; chocolateyTested=$false; chocolateySubmitted=$false; storeSubmitted=$false } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'status.json') -Encoding UTF8
"Prepared channel drafts at $output. No submission was performed."
