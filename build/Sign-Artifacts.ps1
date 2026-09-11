#requires -Version 7.0
[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param(
    [Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{40,64}$')][string]$CertificateThumbprint,
    [Parameter(Mandatory)][ValidatePattern('^https://')][string]$TimestampUrl,
    [string[]]$Paths
)
$ErrorActionPreference='Stop'
if(-not $IsWindows){ throw 'Authenticode signing requires Windows.' }
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. (Join-Path $PSScriptRoot 'Get-ProjectVersion.ps1')
$version=Get-ProjectVersion -Root $root
if(-not $Paths -or $Paths.Count -eq 0){
    $Paths=@(
        "artifacts/PermissionScope-$version-x64-Setup.exe",
        "artifacts/PermissionScope-$version-arm64-Setup.exe",
        "artifacts/PermissionScope-$version-x64-unsigned.msix",
        "artifacts/PermissionScope-$version-arm64-unsigned.msix"
    )
}
$signtool=Get-ChildItem 'C:\Program Files (x86)\Windows Kits\10\bin' -Filter signtool.exe -Recurse | Where-Object { $_.FullName -match '\\x64\\signtool.exe$' } | Sort-Object FullName -Descending | Select-Object -First 1
if(-not $signtool){ throw 'Windows SDK SignTool was not found.' }
$thumb=$CertificateThumbprint.Replace(' ','').ToUpperInvariant()
$certificate=Get-ChildItem Cert:\CurrentUser\My,Cert:\LocalMachine\My -ErrorAction SilentlyContinue | Where-Object { $_.Thumbprint -eq $thumb } | Select-Object -First 1
if(-not $certificate){ throw "Signing certificate $thumb was not found in CurrentUser/My or LocalMachine/My." }
if(-not $certificate.HasPrivateKey){ throw 'Selected signing certificate has no accessible private key.' }
if($certificate.NotAfter -le [DateTime]::UtcNow){ throw 'Selected signing certificate is expired.' }

$results=@()
foreach($relative in $Paths){
    $path=if([IO.Path]::IsPathRooted($relative)){$relative}else{Join-Path $root $relative}
    $path=[IO.Path]::GetFullPath($path)
    if(-not $path.StartsWith([IO.Path]::GetFullPath((Join-Path $root 'artifacts')),[StringComparison]::OrdinalIgnoreCase)){ throw "Refusing to sign outside artifacts/: $path" }
    if(-not (Test-Path -LiteralPath $path)){ throw "Signing target not found: $path" }
    if(-not $PSCmdlet.ShouldProcess($path,"Authenticode sign with certificate $thumb and RFC3161 timestamp $TimestampUrl")){ continue }
    & $signtool.FullName sign /sha1 $thumb /fd SHA256 /tr $TimestampUrl /td SHA256 /a $path | Out-Host
    if($LASTEXITCODE -ne 0){ throw "SignTool sign failed: $path" }
    & $signtool.FullName verify /pa /all /v $path | Out-Host
    if($LASTEXITCODE -ne 0){ throw "SignTool verification failed: $path" }
    $results += [pscustomobject]@{file=[IO.Path]::GetFileName($path);sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash;certificateThumbprint=$thumb;verified=$true}
}
if($results.Count -eq 0){ Write-Host 'No artifacts were signed.'; return }
$output=Join-Path $root 'artifacts/production-trust'
New-Item -ItemType Directory -Path $output -Force | Out-Null
@{
    schema=1
    version=$version
    certificateSubject=$certificate.Subject
    certificateThumbprint=$thumb
    certificateNotAfter=$certificate.NotAfter.ToUniversalTime().ToString('O')
    timestampUrl=$TimestampUrl
    artifacts=$results
    verified=$true
    generatedAt=[DateTimeOffset]::UtcNow.ToString('O')
} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $output 'signing-evidence.json') -Encoding UTF8
Write-Host 'PASS trusted-signing gate: every requested artifact was signed and verified with SignTool.'
