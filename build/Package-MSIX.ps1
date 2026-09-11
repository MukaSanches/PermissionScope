param(
    [ValidateSet('x64','arm64')][string]$Architecture='x64',
    [string]$Publisher='CN=Samuel Sanches',
    [string]$IdentityName='SamuelSanches.PermissionScope',
    [string]$PublisherDisplayName='Samuel Sanches'
)
$ErrorActionPreference = 'Stop'
$repository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. (Join-Path $PSScriptRoot 'Get-ProjectVersion.ps1')
$version = Get-ProjectVersion -Root $repository
$baseVersion = ($version -split '[-+]')[0]
$parts = @($baseVersion.Split('.'))
if ($parts.Count -lt 3 -or $parts[0..2] | Where-Object { $_ -notmatch '^\d+$' }) { throw "MSIX requires a numeric major.minor.patch project version; got '$version'." }
$msixVersion = '{0}.{1}.{2}.0' -f [int]$parts[0],[int]$parts[1],[int]$parts[2]
if ([string]::IsNullOrWhiteSpace($Publisher) -or [string]::IsNullOrWhiteSpace($IdentityName)) { throw 'Publisher and IdentityName are required.' }

$source = Join-Path $repository "artifacts/PermissionScope-$version-win-$Architecture"
$staging = Join-Path $repository "artifacts/msix-$Architecture"
$output = Join-Path $repository "artifacts/PermissionScope-$version-$Architecture-unsigned.msix"
$metadataPath = "$output.metadata.json"
$artifactRoot = [System.IO.Path]::GetFullPath((Join-Path $repository 'artifacts')) + [System.IO.Path]::DirectorySeparatorChar
if (-not [System.IO.Path]::GetFullPath($staging).StartsWith($artifactRoot,[StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid MSIX staging directory.' }
if (-not (Test-Path -LiteralPath $source)) { throw "Packaged application not found: $source. Run build/Package.ps1 first." }
$buildInfoPath = Join-Path $source 'build-info.json'
if (-not (Test-Path -LiteralPath $buildInfoPath)) { throw 'Package build-info.json is missing.' }
$buildInfo = Get-Content -LiteralPath $buildInfoPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($buildInfo.version -ne $version -or $buildInfo.architecture -ne $Architecture) { throw 'Packaged application version/architecture does not match the requested MSIX.' }

if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging -Force | Out-Null
Copy-Item -Path (Join-Path $source '*') -Destination $staging -Recurse -Force
$assets = Join-Path $staging 'Assets'
New-Item -ItemType Directory -Path $assets -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $repository 'assets') -Filter '*.png' | Copy-Item -Destination $assets -Force

$publisherEscaped = [System.Security.SecurityElement]::Escape($Publisher)
$identityEscaped = [System.Security.SecurityElement]::Escape($IdentityName)
$displayEscaped = [System.Security.SecurityElement]::Escape($PublisherDisplayName)
$manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities" IgnorableNamespaces="uap rescap">
  <Identity Name="$identityEscaped" Publisher="$publisherEscaped" Version="$msixVersion" ProcessorArchitecture="$Architecture" />
  <Properties><DisplayName>PermissionScope</DisplayName><PublisherDisplayName>$displayEscaped</PublisherDisplayName><Logo>Assets\StoreLogo.png</Logo></Properties>
  <Dependencies><TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.26100.0" /></Dependencies>
  <Resources><Resource Language="en-US"/><Resource Language="pt-BR"/><Resource Language="es"/><Resource Language="fr"/><Resource Language="de"/><Resource Language="ar"/><Resource Language="ja"/><Resource Language="zh-Hans"/></Resources>
  <Applications><Application Id="PermissionScope" Executable="PermissionScope.exe" EntryPoint="Windows.FullTrustApplication"><uap:VisualElements DisplayName="PermissionScope" Description="Windows access analysis and permission explainability" BackgroundColor="#17315C" Square150x150Logo="Assets\Square150x150Logo.png" Square44x44Logo="Assets\Square44x44Logo.png" /></Application></Applications>
  <Capabilities><rescap:Capability Name="runFullTrust" /></Capabilities>
</Package>
"@
[System.IO.File]::WriteAllText((Join-Path $staging 'AppxManifest.xml'),$manifest,(New-Object System.Text.UTF8Encoding $false))
$makeappx = Get-ChildItem 'C:\Program Files (x86)\Windows Kits\10\bin' -Filter makeappx.exe -Recurse | Where-Object { $_.FullName -match '\\x64\\makeappx.exe$' } | Sort-Object FullName -Descending | Select-Object -First 1
if (-not $makeappx) { throw 'Windows SDK MakeAppx was not found.' }
if (Test-Path -LiteralPath $output) { Remove-Item -LiteralPath $output -Force }
& $makeappx.FullName pack /d $staging /p $output /o
if ($LASTEXITCODE -ne 0) { throw 'MSIX packaging failed.' }
@{
    schema = 1
    projectVersion = $version
    msixVersion = $msixVersion
    architecture = $Architecture
    identityName = $IdentityName
    publisher = $Publisher
    publisherDisplayName = $PublisherDisplayName
    sourceSha256 = $buildInfo.sourceSha256
    packageSha256 = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
    signed = $false
    storeCertified = $false
    generatedAt = [DateTimeOffset]::UtcNow.ToString('O')
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $metadataPath -Encoding UTF8
Write-Host "Created unsigned MSIX: $output"
Write-Host 'This package is structurally prepared only; signing and Microsoft Store certification are separate external gates.'
