param([switch]$SkipBuild)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath("$PSScriptRoot/..")
Push-Location $root
try {
    & "$PSScriptRoot/Verify-Documentation.ps1"
    foreach($architecture in @('x64','arm64')) { & "$PSScriptRoot/Package.ps1" -Architecture $architecture -SkipBuild:$SkipBuild }
    if (git status --porcelain) { throw 'Commit the reviewed source before creating its release archive.' }
    git archive --format=zip --output=artifacts/PermissionScope-1.0.0-source.zip HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Source archive failed.' }
    $names=@('PermissionScope-1.0.0-x64-Setup.exe','PermissionScope-1.0.0-arm64-Setup.exe','PermissionScope-1.0.0-win-x64.zip','PermissionScope-1.0.0-win-arm64.zip','PermissionScope-1.0.0-source.zip')
    $entries=foreach($name in $names) { $file=Get-Item -LiteralPath "artifacts/$name"; [ordered]@{name=$name;bytes=$file.Length;sha256=(Get-FileHash $file.FullName -Algorithm SHA256).Hash} }
    . "$PSScriptRoot/Get-SourceFingerprint.ps1"
    [ordered]@{schema=1;version='1.0.0';commit=(git rev-parse HEAD);sourceSha256=(Get-SourceFingerprint -Root $root);files=@($entries);signature='Unsigned';provenance='Built with repository packaging scripts; no third-party attestation claimed'} | ConvertTo-Json -Depth 5 | Set-Content artifacts/release-manifest.json -Encoding UTF8
    $checksums=@($entries | ForEach-Object { "$($_.sha256)  $($_.name)" })
    $checksums+="$( (Get-FileHash artifacts/release-manifest.json -Algorithm SHA256).Hash )  release-manifest.json"
    $checksums | Set-Content artifacts/SHA256SUMS.txt -Encoding ASCII
    & "$PSScriptRoot/Verify-Release.ps1"
} finally { Pop-Location }
