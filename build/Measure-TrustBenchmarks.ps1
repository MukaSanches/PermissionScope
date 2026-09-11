#requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CliPath,
    [int[]]$FileCounts = @(100,1000),
    [switch]$Extended,
    [switch]$RetainRaw,
    [ValidateRange(30,1800)][int]$TimeoutSeconds = 300
)
$ErrorActionPreference='Stop'
if(-not $IsWindows){ throw 'Production Trust benchmarks require Windows.' }
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$CliPath=[IO.Path]::GetFullPath($CliPath)
if(-not (Test-Path -LiteralPath $CliPath)){ throw "CLI not found: $CliPath" }
if($Extended -and 10000 -notin $FileCounts){ $FileCounts=@($FileCounts)+10000 }
$FileCounts=@($FileCounts | Sort-Object -Unique)
foreach($count in $FileCounts){ if($count -lt 1 -or $count -gt 100000){ throw "Unsupported FileCount: $count" } }
$outputDir=Join-Path $root 'artifacts/production-trust'
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$runs=@()
$cliHash=(Get-FileHash -LiteralPath $CliPath -Algorithm SHA256).Hash

foreach($count in $FileCounts){
    Write-Host "Running synthetic scan benchmark: $count files"
    $arguments=@('-File', (Join-Path $PSScriptRoot 'Measure-Scan.ps1'), '-CliPath', $CliPath, '-FileCount', $count, '-TimeoutSeconds', $TimeoutSeconds)
    if($count -gt 10000){ $arguments += '-AllowLarge' }
    $lines=@(& pwsh @arguments 2>&1 | ForEach-Object { $_.ToString() })
    $resultLine=$lines | Where-Object { $_ -match '^Results:\s+(.+)$' } | Select-Object -Last 1
    if(-not $resultLine){ throw "Benchmark harness did not report results for $count files.`n$($lines -join "`n")" }
    $resultPath=([regex]::Match($resultLine,'^Results:\s+(.+)$')).Groups[1].Value.Trim()
    if(-not (Test-Path -LiteralPath $resultPath)){ throw "Benchmark result file not found: $resultPath" }
    $raw=Get-Content -LiteralPath $resultPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if($raw.Status -ne 'passed' -or -not $raw.Validation.Passed){ throw "Benchmark validation failed for $count files. Raw result: $resultPath" }
    $elapsed=[double]$raw.FullScan.ElapsedMilliseconds
    $objects=[int]$raw.Validation.Objects
    $runs += [pscustomobject]@{
        fileCount=[int]$raw.FileCount
        directoryCount=[int]$raw.DirectoryCountIncludingRoot
        objects=$objects
        elapsedMilliseconds=[math]::Round($elapsed,2)
        objectsPerSecond=if($elapsed -gt 0){[math]::Round($objects/($elapsed/1000),2)}else{0}
        observedPeakWorkingSetBytes=[long]$raw.FullScan.ObservedPeakWorkingSetBytes
        stdoutBytes=[long]$raw.FullScan.StdoutBytes
        validationPassed=[bool]$raw.Validation.Passed
        errors=[int]$raw.Validation.Errors
        unknown=[int]$raw.Validation.Unknown
        terminationProbe=[string]$raw.TerminationProbe.Status
        terminationLatencyMilliseconds=if($null -ne $raw.TerminationProbe.ForceTerminationLatencyMilliseconds){[math]::Round([double]$raw.TerminationProbe.ForceTerminationLatencyMilliseconds,2)}else{$null}
    }
    if(-not $RetainRaw -and $raw.RunDirectory -and (Test-Path -LiteralPath $raw.RunDirectory)){
        Remove-Item -LiteralPath $raw.RunDirectory -Recurse -Force
    }
    Write-Host "PASS benchmark: $objects objects in $([math]::Round($elapsed,2)) ms"
}

$report=[ordered]@{
    schema=1
    benchmarkKind='synthetic-local-NTFS-worker'
    cliSha256=$cliHash
    os=[Environment]::OSVersion.VersionString
    logicalProcessors=[Environment]::ProcessorCount
    runs=$runs
    comparativeClaim=$false
    slaClaim=$false
    rawOutputsRetained=[bool]$RetainRaw
    privacy='Published aggregate contains no scanned paths, account names, token SIDs or raw worker output.'
    methodology='Each size uses build/Measure-Scan.ps1 on synthetic empty files with inherited ACLs and validates object count, errors, schema and termination behavior.'
    completedUtc=[DateTimeOffset]::UtcNow.ToString('O')
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $outputDir 'benchmarks.json') -Encoding UTF8
Write-Host "PASS Production Trust benchmark matrix: $($runs.Count) validated sizes."
