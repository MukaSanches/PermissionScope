param([switch]$SkipBrowser)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath("$PSScriptRoot/..")
Push-Location $root
try {
    & node build/Build-Documentation.mjs --check
    if ($LASTEXITCODE -ne 0) { throw 'Generated documentation is stale.' }
    & node build/validate-resources.mjs
    if ($LASTEXITCODE -ne 0) { throw 'Locale validation failed.' }
    & node build/validate-documentation.mjs
    if ($LASTEXITCODE -ne 0) { throw 'Documentation validation failed.' }
    . "$PSScriptRoot/Get-DocumentationFingerprint.ps1"
    $fingerprint=Get-DocumentationFingerprint -Root $root
    $manifest=Get-Content docs/screenshots/manifest.json -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($capture in $manifest.captures) {
        if ($capture.sourceSha256 -ne $fingerprint) { throw "Screenshot source is stale: $($capture.path)" }
    }
    if (-not $SkipBrowser) { & npm run test:site; if ($LASTEXITCODE -ne 0) { throw 'Website regression failed.' } }
    "PASS documentation: $($manifest.captures.Count) current-source captures, locale parity, links, image hashes and synthetic report privacy."
} finally { Pop-Location }
