#requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string]$CliPath)
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'Windows required.' }
$cli = (Resolve-Path -LiteralPath $CliPath).Path
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('PermissionScope-metrics-tests-' + [guid]::NewGuid().ToString('N'))
$fixture = Join-Path $testRoot 'fixture'
[IO.Directory]::CreateDirectory($fixture) | Out-Null
for ($i = 0; $i -lt 100; $i++) { [IO.File]::WriteAllText((Join-Path $fixture "$i.txt"), '') }
function Assert-True([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function Invoke-Cli([string[]]$Arguments, [switch]$Interrupt) {
    $start = [Diagnostics.ProcessStartInfo]::new($cli)
    $start.UseShellExecute = $false; $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
    foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::new(); $process.StartInfo = $start
    $started = $false
    try {
        $started = $process.Start()
        Assert-True $started 'CLI failed to start.'
        $output = $process.StandardOutput.ReadToEndAsync(); $errors = $process.StandardError.ReadToEndAsync()
        if ($Interrupt) { $process.Kill($true) }
        Assert-True ($process.WaitForExit(30000)) 'CLI test timed out.'
        Assert-True ([Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($output, $errors), 10000)) 'Output drain timed out.'
        return @{ ExitCode = $process.ExitCode; Stdout = $output.Result; Stderr = $errors.Result }
    } finally {
        if ($started -and -not $process.HasExited) { $process.Kill($true); $null = $process.WaitForExit(10000) }
        $process.Dispose()
    }
}
function Read-StableSnapshot($Run) {
    Assert-True ($Run.ExitCode -eq 0) 'Worker failed.'
    Assert-True ([string]::IsNullOrEmpty($Run.Stderr)) 'Unexpected stderr.'
    $snapshot = $null
    foreach ($line in ($Run.Stdout -split '\r?\n' | Where-Object { $_ })) {
        $message = $line | ConvertFrom-Json
        Assert-True (@($message.PSObject.Properties.Name | Where-Object { $_ -notin @('progress', 'snapshot') }).Count -eq 0) 'Protocol changed.'
        if ($null -ne $message.snapshot) { $snapshot = $message.snapshot }
    }
    Assert-True ($null -ne $snapshot -and $snapshot.resources.Count -eq 101 -and -not $snapshot.cancelled) 'Invalid snapshot.'
    foreach ($field in @('id', 'createdAt', 'completedAt')) { $snapshot.PSObject.Properties.Remove($field) }
    foreach ($resource in $snapshot.resources) { $resource.PSObject.Properties.Remove('observedAt') }
    return ($snapshot | ConvertTo-Json -Depth 100 -Compress)
}
$baseArgs = @('scan', $fixture, '--files', '--worker')
$without = Read-StableSnapshot (Invoke-Cli $baseArgs)
Assert-True (@(Get-ChildItem -LiteralPath $testRoot -Filter '*.json').Count -eq 0) 'Unrequested sidecar created.'
$metricsPath = Join-Path $testRoot 'metrics.json'
$with = Read-StableSnapshot (Invoke-Cli ($baseArgs + @('--worker-metrics', $metricsPath)))
Assert-True ($with -eq $without) 'Metrics changed snapshot evidence.'
$metrics = Get-Content -LiteralPath $metricsPath -Raw | ConvertFrom-Json
Assert-True ($metrics.schemaVersion -eq 1 -and $metrics.complete -eq $true -and $metrics.objects -eq 101) 'Invalid sidecar.'
foreach ($field in @('scanMilliseconds', 'serializeMilliseconds', 'writeMilliseconds')) {
    Assert-True ($null -ne $metrics.$field -and [double]::IsFinite([double]$metrics.$field) -and $metrics.$field -ge 0) 'Invalid phase.'
}
Assert-True (@($metrics.PSObject.Properties.Name | Where-Object { $_ -notin @('schemaVersion','complete','objects','scanMilliseconds','serializeMilliseconds','writeMilliseconds') }).Count -eq 0) 'Unexpected diagnostic data.'
$blockedPath = Join-Path $testRoot 'missing-parent\metrics.json'
$failedWrite = Read-StableSnapshot (Invoke-Cli ($baseArgs + @('--worker-metrics', $blockedPath)))
Assert-True ($failedWrite -eq $without -and -not (Test-Path -LiteralPath $blockedPath)) 'Sidecar failure changed scan.'
$existing = Join-Path $testRoot 'existing.json'; [IO.File]::WriteAllText($existing, 'sentinel')
$preserved = Read-StableSnapshot (Invoke-Cli ($baseArgs + @('--worker-metrics', $existing)))
Assert-True ($preserved -eq $without -and [IO.File]::ReadAllText($existing) -eq 'sentinel') 'Existing file was replaced.'
$invalid = Invoke-Cli @('scan', $fixture, '--worker-metrics', (Join-Path $testRoot 'invalid.json'))
Assert-True ($invalid.ExitCode -eq 2) 'Metrics accepted without worker.'
$missing = Invoke-Cli ($baseArgs + @('--worker-metrics'))
Assert-True ($missing.ExitCode -eq 2) 'Missing metrics path accepted.'
$interruptedPath = Join-Path $testRoot 'interrupted.json'
$interrupted = Invoke-Cli ($baseArgs + @('--worker-metrics', $interruptedPath)) -Interrupt
Assert-True ($interrupted.ExitCode -ne 0 -and -not (Test-Path -LiteralPath $interruptedPath)) 'Interrupted worker produced completed metrics.'
Write-Host 'PASS: protocol, equivalent evidence, numeric phases, no unrequested output, failed-write isolation, existing-file preservation, argument validation, early interruption.'
Write-Host "Retained synthetic fixture: $testRoot"
