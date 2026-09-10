param(
    [switch]$Full,
    [switch]$SkipNpmCi
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$started = [Diagnostics.Stopwatch]::StartNew()
$steps = [Collections.Generic.List[object]]::new()

function Invoke-ReadinessStep {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][scriptblock]$Action
    )
    Write-Host "`n=== $Name ===" -ForegroundColor Cyan
    $clock = [Diagnostics.Stopwatch]::StartNew()
    try {
        $global:LASTEXITCODE = 0
        & $Action
        if ($LASTEXITCODE -ne 0) { throw "$Name failed with exit code $LASTEXITCODE." }
        $steps.Add([pscustomobject]@{ Step=$Name; Status='PASS'; Seconds=[Math]::Round($clock.Elapsed.TotalSeconds,2) })
    }
    catch {
        $steps.Add([pscustomobject]@{ Step=$Name; Status='FAIL'; Seconds=[Math]::Round($clock.Elapsed.TotalSeconds,2) })
        $steps | Format-Table -AutoSize
        throw
    }
}

Push-Location $root
try {
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

    if (-not $SkipNpmCi) {
        Invoke-ReadinessStep 'Install locked website dependencies' { npm ci --ignore-scripts --no-fund }
    }
    Invoke-ReadinessStep 'Validate resources and website syntax' { npm run check }
    Invoke-ReadinessStep 'Build Windows x64' { dotnet build PermissionScope.sln -c Release -p:Platform=x64 }
    Invoke-ReadinessStep 'Run integration harness' { dotnet run --project tests/PermissionScope.Tests -c Release -- --results artifacts/test-results.json }
    Invoke-ReadinessStep 'Run website regression and accessibility' { npm run test:site }
    Invoke-ReadinessStep 'Verify documentation and native-capture freshness' { & ./build/Verify-Documentation.ps1 -SkipBrowser }

    if ($Full) {
        $nsis = Get-Command makensis.exe -ErrorAction SilentlyContinue
        if (-not $nsis) {
            $candidate = Join-Path ${env:ProgramFiles(x86)} 'NSIS/makensis.exe'
            if (-not (Test-Path -LiteralPath $candidate)) {
                throw 'Full readiness requires NSIS 3.12. Install it before running -Full.'
            }
        }
        Invoke-ReadinessStep 'Package x64' { & ./build/Package.ps1 -Architecture x64 }
        Invoke-ReadinessStep 'Package ARM64' { & ./build/Package.ps1 -Architecture arm64 }
        Invoke-ReadinessStep 'Package source' { & ./build/Package-Source.ps1 }

        $releaseAssets = @(
            'PermissionScope-1.0.0-x64-Setup.exe',
            'PermissionScope-1.0.0-win-x64.zip',
            'PermissionScope-1.0.0-arm64-Setup.exe',
            'PermissionScope-1.0.0-win-arm64.zip',
            'PermissionScope-1.0.0-source.zip'
        )
        Invoke-ReadinessStep 'Create combined release checksums' {
            $lines = foreach ($name in $releaseAssets) {
                $path = Join-Path $root "artifacts/$name"
                if (-not (Test-Path -LiteralPath $path)) { throw "Missing release asset: $name" }
                $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
                "$hash  $name"
            }
            $lines | Set-Content -LiteralPath (Join-Path $root 'artifacts/SHA256SUMS.txt') -Encoding ASCII
        }
        Invoke-ReadinessStep 'Verify assembled release' { & ./build/Verify-Release.ps1 }
    }

    Write-Host "`nPermissionScope release-readiness summary" -ForegroundColor Green
    $steps | Format-Table -AutoSize
    Write-Host ("PASS in {0:N1}s. Full packaging: {1}" -f $started.Elapsed.TotalSeconds,$Full.IsPresent) -ForegroundColor Green
}
finally {
    Pop-Location
}
