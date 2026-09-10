param([ValidateSet('x64','arm64')][string]$Architecture='x64', [string]$Publisher='CN=Samuel Sanches')
$ErrorActionPreference = 'Stop'
$repository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$source = Join-Path $repository "artifacts/PermissionScope-1.0.0-win-$Architecture"
$staging = Join-Path $repository "artifacts/msix-$Architecture"
$artifactRoot = [System.IO.Path]::GetFullPath((Join-Path $repository 'artifacts')) + [System.IO.Path]::DirectorySeparatorChar
if (-not [System.IO.Path]::GetFullPath($staging).StartsWith($artifactRoot,[StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid MSIX staging directory.' }
if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging -Force | Out-Null
Copy-Item -Path (Join-Path $source '*') -Destination $staging -Recurse -Force
$assets = Join-Path $staging 'Assets'
New-Item -ItemType Directory -Path $assets -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $repository 'assets') -Filter '*.png' | Copy-Item -Destination $assets
$publisherEscaped = [System.Security.SecurityElement]::Escape($Publisher)
$manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities" IgnorableNamespaces="uap rescap">
  <Identity Name="SamuelSanches.PermissionScope" Publisher="$publisherEscaped" Version="1.0.0.0" ProcessorArchitecture="$Architecture" />
  <Properties><DisplayName>PermissionScope</DisplayName><PublisherDisplayName>Samuel Sanches</PublisherDisplayName><Logo>Assets\StoreLogo.png</Logo></Properties>
  <Dependencies><TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.26100.0" /></Dependencies>
  <Resources><Resource Language="en-US"/><Resource Language="pt-BR"/><Resource Language="es"/><Resource Language="fr"/><Resource Language="de"/><Resource Language="ar"/><Resource Language="ja"/><Resource Language="zh-Hans"/></Resources>
  <Applications><Application Id="PermissionScope" Executable="PermissionScope.exe" EntryPoint="Windows.FullTrustApplication"><uap:VisualElements DisplayName="PermissionScope" Description="Windows access analysis and permission explainability" BackgroundColor="#17315C" Square150x150Logo="Assets\Square150x150Logo.png" Square44x44Logo="Assets\Square44x44Logo.png" /></Application></Applications>
  <Capabilities><rescap:Capability Name="runFullTrust" /></Capabilities>
</Package>
"@
[System.IO.File]::WriteAllText((Join-Path $staging 'AppxManifest.xml'),$manifest,(New-Object System.Text.UTF8Encoding $false))
$makeappx = Get-ChildItem 'C:\Program Files (x86)\Windows Kits\10\bin' -Filter makeappx.exe -Recurse | Where-Object { $_.FullName -match '\\x64\\makeappx.exe$' } | Sort-Object FullName -Descending | Select-Object -First 1
if (-not $makeappx) { throw 'Windows SDK MakeAppx was not found.' }
& $makeappx.FullName pack /d $staging /p (Join-Path $repository "artifacts/PermissionScope-1.0.0-$Architecture-unsigned.msix") /o
if ($LASTEXITCODE -ne 0) { throw 'MSIX packaging failed.' }
