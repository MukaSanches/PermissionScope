param([switch]$SkipBrowser)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath("$PSScriptRoot/..")
Push-Location $root
try {
    & node build/Build-Documentation.mjs
    if ($LASTEXITCODE -ne 0) { throw 'Documentation generation failed.' }
    & node build/Enhance-Pages.mjs
    if ($LASTEXITCODE -ne 0) { throw 'Progressive Pages enhancement failed.' }

    # Documentation verification must only judge artifacts owned by the documentation
    # generators. Earlier build/restore steps can legitimately rewrite project lock files
    # for a single runtime identifier; those unrelated changes must not masquerade as stale
    # READMEs or Pages output.
    $generatedPathspecs=@(
        '.github/README.md',
        ':(glob)README*.md',
        ':(glob)docs/guides/*.md',
        ':(glob)site/index*.html',
        ':(glob)site/screenshots/*/access-light.png',
        ':(glob)site/reports/permissionscope-demo.*'
    )
    & git diff --exit-code -- @generatedPathspecs
    if ($LASTEXITCODE -ne 0) { throw 'Generated documentation or enhanced Pages are stale.' }

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
