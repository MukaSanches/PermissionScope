#requires -Version 7.0
[CmdletBinding()]
param(
    [string]$Publisher='CN=Samuel Sanches',
    [string]$IdentityName='SamuelSanches.PermissionScope'
)
$ErrorActionPreference='Stop'
if(-not $IsWindows){ throw 'Distribution readiness validation requires Windows.' }
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. (Join-Path $PSScriptRoot 'Get-ProjectVersion.ps1')
$version=Get-ProjectVersion -Root $root
$baseVersion=($version -split '[-+]')[0]
$parts=@($baseVersion.Split('.'))
if($parts.Count -lt 3){ throw "Unexpected project version: $version" }
$msixVersion='{0}.{1}.{2}.0' -f [int]$parts[0],[int]$parts[1],[int]$parts[2]
$outputDir=Join-Path $root 'artifacts/production-trust'
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$makeappx=Get-ChildItem 'C:\Program Files (x86)\Windows Kits\10\bin' -Filter makeappx.exe -Recurse | Where-Object { $_.FullName -match '\\x64\\makeappx.exe$' } | Sort-Object FullName -Descending | Select-Object -First 1
if(-not $makeappx){ throw 'Windows SDK MakeAppx was not found.' }
$architectures=@()
foreach($architecture in @('x64','arm64')){
    $installer=Join-Path $root "artifacts/PermissionScope-$version-$architecture-Setup.exe"
    $msix=Join-Path $root "artifacts/PermissionScope-$version-$architecture-unsigned.msix"
    $metadataPath="$msix.metadata.json"
    foreach($file in @($installer,$msix,$metadataPath)){ if(-not (Test-Path -LiteralPath $file)){ throw "Required distribution artifact missing: $file" } }
    $metadata=Get-Content -LiteralPath $metadataPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if($metadata.projectVersion -ne $version -or $metadata.msixVersion -ne $msixVersion -or $metadata.architecture -ne $architecture){ throw "MSIX metadata mismatch for $architecture." }
    if($metadata.signed -ne $false -or $metadata.storeCertified -ne $false){ throw 'Unsigned readiness metadata must not claim signing or Store certification.' }
    if($metadata.identityName -ne $IdentityName -or $metadata.publisher -ne $Publisher){ throw "MSIX identity mismatch for $architecture." }
    if((Get-FileHash -LiteralPath $msix -Algorithm SHA256).Hash -ne $metadata.packageSha256){ throw "MSIX checksum mismatch for $architecture." }

    $unpack=Join-Path ([IO.Path]::GetTempPath()) ('PermissionScope-msix-check-' + [guid]::NewGuid().ToString('N'))
    try{
        & $makeappx.FullName unpack /p $msix /d $unpack /o | Out-Host
        if($LASTEXITCODE -ne 0){ throw "Could not unpack $architecture MSIX." }
        $manifestPath=Join-Path $unpack 'AppxManifest.xml'
        [xml]$manifest=Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8
        $ns=New-Object Xml.XmlNamespaceManager($manifest.NameTable); $ns.AddNamespace('f','http://schemas.microsoft.com/appx/manifest/foundation/windows10')
        $identity=$manifest.SelectSingleNode('/f:Package/f:Identity',$ns)
        if(-not $identity){ throw "MSIX Identity node missing for $architecture." }
        if($identity.Name -ne $IdentityName -or $identity.Publisher -ne $Publisher -or $identity.Version -ne $msixVersion -or $identity.ProcessorArchitecture -ne $architecture){ throw "MSIX manifest identity mismatch for $architecture." }
        foreach($relative in @('PermissionScope.exe','Assets\StoreLogo.png','Assets\Square150x150Logo.png','Assets\Square44x44Logo.png')){
            if(-not (Test-Path -LiteralPath (Join-Path $unpack $relative))){ throw "MSIX payload missing $relative for $architecture." }
        }
    }
    finally{ if(Test-Path -LiteralPath $unpack){ Remove-Item -LiteralPath $unpack -Recurse -Force } }
    $architectures += [pscustomobject]@{
        architecture=$architecture
        installerSha256=(Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash
        msixSha256=(Get-FileHash -LiteralPath $msix -Algorithm SHA256).Hash
        manifestIdentity='passed'
        payload='passed'
        signed=$false
        physicalHardwareCertified=if($architecture -eq 'arm64'){$false}else{$null}
    }
}

$statusPath=Join-Path $root 'artifacts/channels/status.json'
if(-not (Test-Path -LiteralPath $statusPath)){ throw 'Channel status.json is missing. Run build/Prepare-Channels.ps1 first.' }
$status=Get-Content -LiteralPath $statusPath -Raw -Encoding UTF8 | ConvertFrom-Json
if($status.version -ne $version -or -not $status.prepared){ throw 'Channel draft status does not match current project version.' }
foreach($claim in @('wingetSubmitted','chocolateySubmitted','storeSubmitted')){ if($status.$claim -ne $false){ throw "Readiness validation cannot accept an unverified external claim: $claim" } }
$wingetRoot=Join-Path $root "artifacts/channels/winget/$IdentityName/$version"
foreach($name in @("$IdentityName.yaml","$IdentityName.locale.en-US.yaml","$IdentityName.installer.yaml")){
    $path=Join-Path $wingetRoot $name
    if(-not (Test-Path -LiteralPath $path)){ throw "WinGet draft missing: $name" }
    $text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
    if($text -notmatch "PackageVersion:\s+$([regex]::Escape($version))"){ throw "WinGet draft has stale version: $name" }
}
$installerManifest=Get-Content -LiteralPath (Join-Path $wingetRoot "$IdentityName.installer.yaml") -Raw -Encoding UTF8
foreach($architecture in @('x64','arm64')){
    $hash=(Get-FileHash -LiteralPath (Join-Path $root "artifacts/PermissionScope-$version-$architecture-Setup.exe") -Algorithm SHA256).Hash
    if($installerManifest -notmatch [regex]::Escape($hash)){ throw "WinGet draft checksum is stale for $architecture." }
}

$report=[ordered]@{
    schema=1
    projectVersion=$version
    msixVersion=$msixVersion
    architectures=$architectures
    channelDrafts='passed'
    trustedSigning='external-gate'
    microsoftStoreCertification='external-gate'
    wingetAcceptance='external-gate'
    arm64PhysicalExecution='external-gate'
    readiness='passed'
    note='Passed means artifacts/manifests are internally consistent. It does not mean Microsoft Store, WinGet, trusted signing or physical ARM64 validation has occurred.'
    completedUtc=[DateTimeOffset]::UtcNow.ToString('O')
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $outputDir 'distribution-readiness.json') -Encoding UTF8
Write-Host 'PASS distribution readiness: current-version installers, unsigned MSIX packages and channel drafts are internally consistent.'
