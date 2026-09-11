param(
    [string]$Repository='MukaSanches/PermissionScope',
    [string]$PackageIdentifier='SamuelSanches.PermissionScope',
    [string]$Publisher='Samuel Sanches',
    [switch]$VerifyPublished
)
$ErrorActionPreference='Stop'
if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') { throw 'Invalid GitHub repository identifier.' }
if ($PackageIdentifier -notmatch '^[A-Za-z0-9.-]+$') { throw 'Invalid package identifier.' }
$root=[System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. (Join-Path $PSScriptRoot 'Get-ProjectVersion.ps1')
$version=Get-ProjectVersion -Root $root
$output=Join-Path $root 'artifacts/channels'
$winget=Join-Path $output "winget/$PackageIdentifier/$version"
$chocolatey=Join-Path $output 'chocolatey'
if (Test-Path -LiteralPath $output) { Remove-Item -LiteralPath $output -Recurse -Force }
New-Item -ItemType Directory -Path $winget,(Join-Path $chocolatey 'tools') -Force | Out-Null
$release="https://github.com/$Repository/releases/download/v$version"
$hashes=@{}
$publishedVerified=[ordered]@{}
foreach ($architecture in @('x64','arm64')) {
    $filename="PermissionScope-$version-$architecture-Setup.exe"
    $local=Join-Path $root "artifacts/$filename"
    if (-not (Test-Path -LiteralPath $local)) { throw "Missing local installer: $local. Run build/Package.ps1 for both architectures first." }
    $hashes[$architecture]=(Get-FileHash -LiteralPath $local -Algorithm SHA256).Hash
    $publishedVerified[$architecture]=$false
    if ($VerifyPublished) {
        $download=Join-Path $output ("published-" + $filename)
        Invoke-WebRequest -UseBasicParsing -Uri "$release/$filename" -OutFile $download
        $remoteHash=(Get-FileHash -LiteralPath $download -Algorithm SHA256).Hash
        if ($remoteHash -ne $hashes[$architecture]) { throw "Published installer mismatch: $architecture. Do not submit a manifest whose checksum does not match the public asset." }
        $publishedVerified[$architecture]=$true
        Remove-Item -LiteralPath $download -Force
    }
}
$versionManifest=@"
PackageIdentifier: $PackageIdentifier
PackageVersion: $version
DefaultLocale: en-US
ManifestType: version
ManifestVersion: 1.6.0
"@
$localeManifest=@"
PackageIdentifier: $PackageIdentifier
PackageVersion: $version
PackageLocale: en-US
Publisher: $Publisher
PublisherUrl: https://github.com/$Repository
PackageName: PermissionScope
PackageUrl: https://github.com/$Repository
License: Apache-2.0
LicenseUrl: https://github.com/$Repository/blob/main/LICENSE
Copyright: Copyright 2026 Samuel Sanches
ShortDescription: Native Windows permission analysis with inspectable access evidence.
Moniker: permissionscope
Tags:
  - permissions
  - ntfs
  - windows
  - access-control
  - local-first
ReleaseNotesUrl: https://github.com/$Repository/releases/tag/v$version
ManifestType: defaultLocale
ManifestVersion: 1.6.0
"@
$installerManifest=@"
PackageIdentifier: $PackageIdentifier
PackageVersion: $version
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
    Publisher: $Publisher
    DisplayVersion: $version
Installers:
  - Architecture: x64
    InstallerUrl: $release/PermissionScope-$version-x64-Setup.exe
    InstallerSha256: $($hashes['x64'])
  - Architecture: arm64
    InstallerUrl: $release/PermissionScope-$version-arm64-Setup.exe
    InstallerSha256: $($hashes['arm64'])
ManifestType: installer
ManifestVersion: 1.6.0
"@
$versionManifest | Set-Content -LiteralPath (Join-Path $winget "$PackageIdentifier.yaml") -Encoding UTF8
$localeManifest | Set-Content -LiteralPath (Join-Path $winget "$PackageIdentifier.locale.en-US.yaml") -Encoding UTF8
$installerManifest | Set-Content -LiteralPath (Join-Path $winget "$PackageIdentifier.installer.yaml") -Encoding UTF8

$nuspec=@"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd"><metadata>
<id>permissionscope</id><version>$version</version><title>PermissionScope</title><authors>Samuel Sanches</authors><owners>Samuel Sanches</owners>
<projectUrl>https://github.com/$Repository</projectUrl><license type="expression">Apache-2.0</license><requireLicenseAcceptance>false</requireLicenseAcceptance>
<summary>Native Windows permission analysis with inspectable evidence.</summary>
<description>Free, local Windows permission analysis, snapshots, comparison and simulation. Uses Windows Authz for discretionary rights and explicit Unknown states for unverified context. This package installs the x64 application per user. ARM64 is available from the project release.</description>
<tags>windows permissions ntfs admin local-first</tags><releaseNotes>https://github.com/$Repository/releases/tag/v$version</releaseNotes>
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
$installScript.Replace('__URL__',"$release/PermissionScope-$version-x64-Setup.exe").Replace('__HASH__',$hashes['x64']) | Set-Content -LiteralPath (Join-Path $chocolatey 'tools/chocolateyinstall.ps1') -Encoding UTF8
$uninstallScript=@'
$ErrorActionPreference = 'Stop'
$entries = @(Get-UninstallRegistryKey -SoftwareName 'PermissionScope')
if ($entries.Count -eq 0) { return }
if ($entries.Count -ne 1 -or $entries[0].UninstallString -notmatch '^"([^"]+Uninstall\.exe)"$') { throw 'Ambiguous uninstall registration; uninstall through Windows Settings.' }
Uninstall-ChocolateyPackage -PackageName $env:ChocolateyPackageName -FileType exe -SilentArgs '/S' -File $Matches[1] -ValidExitCodes @(0)
'@
$uninstallScript | Set-Content -LiteralPath (Join-Path $chocolatey 'tools/chocolateyuninstall.ps1') -Encoding UTF8

$status=[ordered]@{
    schema=1
    version=$version
    packageIdentifier=$PackageIdentifier
    prepared=$true
    localInstallerHashes=$hashes
    publishedInstallersVerified=[bool]$VerifyPublished
    publishedArchitectureVerification=$publishedVerified
    wingetValidated=$false
    wingetSubmitted=$false
    chocolateyTested=$false
    chocolateySubmitted=$false
    storeSubmitted=$false
    note=if($VerifyPublished){'Public GitHub release assets matched local installer hashes. Channel submission was not performed.'}else{'Drafts use hashes from local artifacts. Public release equivalence and channel submission are intentionally unverified.'}
    generatedAt=[DateTimeOffset]::UtcNow.ToString('O')
}
$status | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $output 'status.json') -Encoding UTF8
Write-Host "Prepared current-version channel drafts at $output. No submission was performed."
