#requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CliPath,
    [ValidateRange(1, 100000)][int]$FileCount = 100,
    [switch]$AllowLarge,
    [switch]$MeasurePhases,
    [ValidateRange(1, 3600)][int]$TimeoutSeconds = 300,
    [ValidateRange(1, 60000)][int]$CancelAfterMilliseconds = 500
)
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'This harness requires Windows.' }
if ($FileCount -gt 10000 -and -not $AllowLarge) { throw 'More than 10000 files requires -AllowLarge.' }
$cli = (Resolve-Path -LiteralPath $CliPath).Path
if ([IO.Path]::GetExtension($cli) -ne '.exe') { throw 'CliPath must identify a prebuilt Windows CLI executable.' }
$runRoot = Join-Path ([IO.Path]::GetTempPath()) ('PermissionScope-benchmark-' + [guid]::NewGuid().ToString('N'))
$fixture = Join-Path $runRoot 'fixture'
[IO.Directory]::CreateDirectory($fixture) | Out-Null
$resultsPath = Join-Path $runRoot 'results.json'
$volume = [IO.DriveInfo]::new([IO.Path]::GetPathRoot($fixture))
$folderCount = [Math]::Min(100, $FileCount)
$report = [ordered]@{
    SchemaVersion = 1; StartedUtc = [DateTimeOffset]::UtcNow.ToString('o')
    Status = 'preparing'; RunDirectory = $runRoot; RetainedFixture = $fixture
    CliPath = $cli; CliSha256Before = (Get-FileHash -LiteralPath $cli -Algorithm SHA256).Hash
    PowerShell = $PSVersionTable.PSVersion.ToString(); OS = [Environment]::OSVersion.VersionString
    LogicalProcessors = [Environment]::ProcessorCount
    FixtureFileSystem = $volume.DriveFormat
    FileCount = $FileCount; DirectoryCountIncludingRoot = $folderCount + 1
    ExpectedObjects = $FileCount + $folderCount + 1
    TimeoutSeconds = $TimeoutSeconds; CancelAfterMilliseconds = $CancelAfterMilliseconds
    MeasurePhases = [bool]$MeasurePhases; ConsumerPhases = $null
    Notes = @('Synthetic empty files with inherited ACLs; no user data scanned.',
        'Elapsed includes process startup, identity resolution, scan, serialization and output drain.',
        'Peak working set is observed for the CLI child only at roughly 50ms intervals; not GUI memory.',
        'Optional consumer timings use PowerShell ReadLine/ConvertFrom-Json after child exit, not WinUI deserialization. Memory boundaries are observations, not peaks.',
        'Worker scan includes identity/progress; serialize covers final snapshot only; write covers worker encoding/output blocking, not consumer drain. Sidecar I/O is outside these phases.',
        'Termination measures Kill(entireProcessTree) to child exit, not cooperative or GUI cancellation.',
        'Executable hash identifies the apphost only, not the dependent assemblies or complete engine build.',
        'Raw outputs contain local paths and Windows token identities; publish only sanitized metrics.',
        'Fixture and outputs are retained; no automatic deletion. No comparative benchmark claims.')
}
function Save-Report {
    $report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $resultsPath -Encoding utf8
}
function Invoke-MeasuredWorker([string]$Name, [bool]$Terminate) {
    $stdoutPath = Join-Path $runRoot ($Name + '.stdout.jsonl')
    $stderrPath = Join-Path $runRoot ($Name + '.stderr.txt')
    $start = [Diagnostics.ProcessStartInfo]::new($cli)
    $start.UseShellExecute = $false; $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
    foreach ($argument in @('scan', $fixture, '--files', '--worker')) { $start.ArgumentList.Add($argument) }
    $phasePath = Join-Path $runRoot ($Name + '.phases.json')
    if ($MeasurePhases) { $start.ArgumentList.Add('--worker-metrics'); $start.ArgumentList.Add($phasePath) }
    $process = [Diagnostics.Process]::new(); $process.StartInfo = $start
    $stdoutFile = [IO.File]::Create($stdoutPath); $stderrFile = [IO.File]::Create($stderrPath)
    $clock = [Diagnostics.Stopwatch]::StartNew()
    $metrics = [ordered]@{ Status = 'starting'; ProcessId = $null; ExitCode = $null
        ElapsedMilliseconds = $null; ObservedPeakWorkingSetBytes = 0L; MemorySamples = 0
        ForceTerminationLatencyMilliseconds = $null; TerminationRequested = $false
        Stdout = $stdoutPath; Stderr = $stderrPath
        PhaseDiagnosticsStatus = if ($MeasurePhases) { 'missing' } else { 'not-requested' }
        WorkerPhases = $null }
    $started = $false
    try {
        $started = $process.Start()
        if (-not $started) { throw 'Could not start CLI.' }
        $metrics.ProcessId = $process.Id
        $outTask = $process.StandardOutput.BaseStream.CopyToAsync($stdoutFile)
        $errTask = $process.StandardError.BaseStream.CopyToAsync($stderrFile)
        while (-not $process.HasExited) {
            $process.Refresh()
            try {
                $metrics.ObservedPeakWorkingSetBytes = [Math]::Max($metrics.ObservedPeakWorkingSetBytes, $process.PeakWorkingSet64)
                $metrics.MemorySamples++
            } catch [InvalidOperationException] { }
            $timeout = $clock.Elapsed.TotalSeconds -ge $TimeoutSeconds
            if ($timeout -or ($Terminate -and $clock.ElapsedMilliseconds -ge $CancelAfterMilliseconds)) {
                if ($process.HasExited) { break }
                $metrics.TerminationRequested = $true
                $killClock = [Diagnostics.Stopwatch]::StartNew()
                try { $process.Kill($true) } catch [InvalidOperationException] {
                    if (-not $process.HasExited) { throw }
                    $metrics.TerminationRequested = $false
                }
                if (-not $process.WaitForExit(10000)) { throw 'Child did not exit within 10 seconds after termination request.' }
                if ($metrics.TerminationRequested) {
                    $metrics.ForceTerminationLatencyMilliseconds = $killClock.Elapsed.TotalMilliseconds
                    $metrics.Status = if ($timeout) { 'timeout' } else { 'force-terminated' }
                }
                break
            }
            Start-Sleep -Milliseconds 50
        }
        if (-not [Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($outTask, $errTask), 10000)) {
            throw 'Output drain exceeded 10 seconds.'
        }
        $metrics.ExitCode = $process.ExitCode
        if ($metrics.Status -eq 'starting') {
            $metrics.Status = if ($Terminate) { 'completed-before-cancel' } else { 'completed' }
        }
    } catch {
        $metrics.Status = 'failed'; $metrics.Error = $_.Exception.Message
    } finally {
        if ($started -and -not $process.HasExited) {
            try { $process.Kill($true); $null = $process.WaitForExit(10000) }
            catch { $metrics.CleanupError = $_.Exception.Message }
        }
        $clock.Stop(); $metrics.ElapsedMilliseconds = $clock.Elapsed.TotalMilliseconds
        $stdoutFile.Dispose(); $stderrFile.Dispose(); $process.Dispose()
        $metrics.StdoutBytes = (Get-Item -LiteralPath $stdoutPath).Length
        $metrics.StderrBytes = (Get-Item -LiteralPath $stderrPath).Length
    }
    if ($MeasurePhases -and (Test-Path -LiteralPath $phasePath -PathType Leaf)) {
        try {
            $phases = Get-Content -LiteralPath $phasePath -Raw | ConvertFrom-Json
            if ($metrics.Status -notin @('completed', 'completed-before-cancel') -or $metrics.ExitCode -ne 0) { throw 'Worker did not complete naturally.' }
            if ($phases.schemaVersion -ne 1 -or $phases.complete -isnot [bool] -or -not $phases.complete -or $phases.objects -ne $report.ExpectedObjects) { throw 'Invalid diagnostics envelope.' }
            foreach ($field in @('scanMilliseconds', 'serializeMilliseconds', 'writeMilliseconds')) {
                $value = $phases.$field
                if ($null -eq $value -or $value -is [string] -or $value -is [bool] -or -not [double]::IsFinite([double]$value) -or [double]$value -lt 0) { throw 'Invalid phase duration.' }
            }
            $metrics.WorkerPhases = [ordered]@{ ScanMilliseconds = $phases.scanMilliseconds; SerializeMilliseconds = $phases.serializeMilliseconds; WriteMilliseconds = $phases.writeMilliseconds }
            $metrics.PhaseDiagnosticsStatus = 'available'
        } catch { $metrics.PhaseDiagnosticsStatus = 'invalid'; $metrics.WorkerPhases = $null }
    }
    return $metrics
}
Save-Report
Write-Host "Retained benchmark directory: $runRoot"
try {
    $preparation = [Diagnostics.Stopwatch]::StartNew()
    for ($i = 0; $i -lt $folderCount; $i++) { [IO.Directory]::CreateDirectory((Join-Path $fixture ('d' + $i))) | Out-Null }
    for ($i = 0; $i -lt $FileCount; $i++) {
        $path = Join-Path $fixture ('d' + ($i % $folderCount)) ('f' + $i + '.txt')
        [IO.File]::WriteAllBytes($path, [byte[]]@())
    }
    $report.PreparationMilliseconds = $preparation.Elapsed.TotalMilliseconds
    $report.Status = 'running'; Save-Report
    $report.FullScan = Invoke-MeasuredWorker 'full-scan' $false
    Save-Report
    if ($report.FullScan.Status -eq 'completed' -and $report.FullScan.ExitCode -eq 0) {
        # Parsing happens after measurement; this monitor's allocations are not CLI measurements.
        $reader = [IO.File]::OpenText($report.FullScan.Stdout)
        if ($MeasurePhases) {
            $monitor = [Diagnostics.Process]::GetCurrentProcess(); $monitor.Refresh()
            $consumer = [ordered]@{ Status = 'incomplete'; ReadLineMilliseconds = 0.0; ParseMilliseconds = 0.0; ValidationMilliseconds = $null; WorkingSetBeforeBytes = $monitor.WorkingSet64; WorkingSetAfterBytes = $null }
            $report.ConsumerPhases = $consumer
            $phaseTimer = [Diagnostics.Stopwatch]::new()
        }
        try {
            $snapshot = $null
            while ($true) {
                if ($MeasurePhases) { $phaseTimer.Restart() }
                $line = $reader.ReadLine()
                if ($MeasurePhases) { $consumer.ReadLineMilliseconds += $phaseTimer.Elapsed.TotalMilliseconds }
                if ($null -eq $line) { break }
                if ($MeasurePhases) { $phaseTimer.Restart() }
                $message = $line | ConvertFrom-Json -Depth 100
                if ($MeasurePhases) { $consumer.ParseMilliseconds += $phaseTimer.Elapsed.TotalMilliseconds }
                if ($null -ne $message.snapshot) { $snapshot = $message.snapshot }
            }
            if ($MeasurePhases) { $phaseTimer.Restart() }
            if ($null -eq $snapshot) { throw 'Worker returned no snapshot.' }
            $report.Validation = [ordered]@{
                Objects = @($snapshot.resources).Count
                Errors = @($snapshot.resources | Where-Object { $null -ne $_.error }).Count
                Unknown = @($snapshot.resources | Where-Object { $null -eq $_.decision -or $_.decision.state -eq 'Unknown' }).Count
                Cancelled = $snapshot.cancelled
                RootMatches = [string]::Equals($snapshot.root, $fixture, [StringComparison]::OrdinalIgnoreCase)
                SchemaVersion = $snapshot.schemaVersion
            }
            $report.Validation.Passed = ($report.Validation.Objects -eq $report.ExpectedObjects -and $report.Validation.Errors -eq 0 -and -not $snapshot.cancelled -and $report.Validation.RootMatches -and $snapshot.schemaVersion -eq 1)
            if ($MeasurePhases) { $consumer.ValidationMilliseconds = $phaseTimer.Elapsed.TotalMilliseconds; $consumer.Status = 'complete' }
        } finally {
            if ($MeasurePhases) { $monitor.Refresh(); $consumer.WorkingSetAfterBytes = $monitor.WorkingSet64; $monitor.Dispose() }
            $reader.Dispose(); $snapshot = $null; $line = $null; $message = $null
        }
        Save-Report
        $report.TerminationProbe = Invoke-MeasuredWorker 'termination-probe' $true
        if ($report.TerminationProbe.Status -eq 'completed-before-cancel') {
            $reader = [IO.File]::OpenText($report.TerminationProbe.Stdout)
            try {
                $lastLine = $null
                while ($null -ne ($line = $reader.ReadLine())) { $lastLine = $line }
                $naturalSnapshot = ($lastLine | ConvertFrom-Json -Depth 100).snapshot
                $report.TerminationProbe.NaturalCompletionValid = (
                    $report.TerminationProbe.ExitCode -eq 0 -and $null -ne $naturalSnapshot -and
                    [string]::Equals($naturalSnapshot.root, $fixture, [StringComparison]::OrdinalIgnoreCase) -and $naturalSnapshot.schemaVersion -eq 1 -and
                    -not $naturalSnapshot.cancelled -and @($naturalSnapshot.resources).Count -eq $report.ExpectedObjects -and
                    @($naturalSnapshot.resources | Where-Object { $null -ne $_.error }).Count -eq 0)
            } finally { $reader.Dispose(); $naturalSnapshot = $null; $lastLine = $null; $line = $null }
        }
    }
    $report.CliSha256After = (Get-FileHash -LiteralPath $cli -Algorithm SHA256).Hash
    $report.ExecutableUnchanged = $report.CliSha256Before -eq $report.CliSha256After
    $terminationValid = $report.TerminationProbe.Status -eq 'force-terminated' -or ($report.TerminationProbe.Status -eq 'completed-before-cancel' -and $report.TerminationProbe.NaturalCompletionValid)
    $report.Status = if ($report.Validation -and $report.Validation.Passed -and $report.ExecutableUnchanged -and $terminationValid) { 'passed' } else { 'failed' }
} catch {
    $report.Status = 'failed'; $report.Error = $_.Exception.Message
} finally {
    $report.CompletedUtc = [DateTimeOffset]::UtcNow.ToString('o'); Save-Report
    Write-Host "Results: $resultsPath"
    Write-Host "Fixture and raw outputs retained at: $runRoot"
}
if ($report.Status -ne 'passed') { throw "Benchmark did not pass; inspect $resultsPath" }
