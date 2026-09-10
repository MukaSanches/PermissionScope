$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. (Join-Path $PSScriptRoot 'Get-SourceFingerprint.ps1')
$fingerprint = Get-SourceFingerprint -Root $root
foreach ($architecture in @('x64','arm64')) {
    $directory = Join-Path $root "artifacts/PermissionScope-1.0.0-win-$architecture"
    $info = Get-Content -LiteralPath (Join-Path $directory 'build-info.json') -Raw | ConvertFrom-Json
    if ($info.sourceSha256 -ne $fingerprint -or $info.architecture -ne $architecture) { throw "Stale package: $architecture" }
    foreach ($file in @('PermissionScope.exe','permissionscope-cli.exe','sbom.cdx.json','THIRD-PARTY-NOTICES.md')) {
        if (-not (Test-Path -LiteralPath (Join-Path $directory $file))) { throw "Missing $architecture package file: $file" }
    }
}
$tests = Get-Content -LiteralPath (Join-Path $root 'artifacts/test-results.json') -Raw | ConvertFrom-Json
if ($tests.Failed -ne 0 -or $tests.Passed -lt 1) { throw 'Integration tests have not passed.' }
$latestSource = Get-ChildItem -LiteralPath (Join-Path $root 'src'),(Join-Path $root 'tests') -Recurse -File |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Name -ne 'packages.lock.json' } | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if ([DateTimeOffset]::Parse($tests.Timestamp).UtcDateTime -lt $latestSource.LastWriteTimeUtc) { throw 'Test results predate the source changes.' }
$site = Get-Content -LiteralPath (Join-Path $root 'artifacts/site-tests.json') -Raw | ConvertFrom-Json
if (-not $site.passed -or $site.axeViolations -ne 0) { throw 'Website checks have not passed.' }
$expected = @('PermissionScope-1.0.0-x64-Setup.exe','PermissionScope-1.0.0-arm64-Setup.exe','PermissionScope-1.0.0-win-x64.zip','PermissionScope-1.0.0-win-arm64.zip','PermissionScope-1.0.0-source.zip')
$checked = @{}
foreach ($line in Get-Content -LiteralPath (Join-Path $root 'artifacts/SHA256SUMS.txt')) {
    if ($line -notmatch '^([A-Fa-f0-9]{64})  ([A-Za-z0-9._-]+)$') { throw 'Malformed release checksum entry.' }
    $hash = $Matches[1]; $name = $Matches[2]
    if ($checked.ContainsKey($name)) { throw "Duplicate checksum: $name" }
    if ((Get-FileHash -LiteralPath (Join-Path $root "artifacts/$name") -Algorithm SHA256).Hash -ne $hash) { throw "Release checksum mismatch: $name" }
    $checked[$name] = $true
}
foreach ($name in $expected) { if (-not $checked.ContainsKey($name)) { throw "Missing release checksum: $name" } }
"Release preparation verified: $($tests.Passed) tests, current x64/ARM64 source fingerprints, package files and checksums. Remote CI, desktop accessibility and platform certification are separate checks."
