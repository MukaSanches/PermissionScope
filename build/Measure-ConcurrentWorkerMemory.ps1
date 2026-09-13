#requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ReplayPath,
    [Parameter(Mandatory)][string]$CliPath,
    [ValidateRange(1, 100000)][int]$FileCount = 100,
    [ValidateRange(1, 10)][int]$Runs = 3,
    [ValidateRange(5, 600)][int]$TimeoutSeconds = 120,
    [ValidateRange(5, 1000)][int]$SampleIntervalMilliseconds = 20,
    [ValidateRange(1, 1073741824)][long]$MaxOutputBytes = 134217728,
    [switch]$AllowLarge
)
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'This harness requires Windows.' }
if ($FileCount -gt 10000 -and -not $AllowLarge) { throw 'More than 10000 files requires -AllowLarge.' }
$replay = (Resolve-Path -LiteralPath $ReplayPath).Path
$cli = (Resolve-Path -LiteralPath $CliPath).Path
if ([IO.Path]::GetExtension($replay) -ne '.exe' -or [IO.Path]::GetExtension($cli) -ne '.exe') { throw 'ReplayPath and CliPath must identify prebuilt Windows executables.' }
$root = Join-Path ([IO.Path]::GetTempPath()) ('PermissionScope-concurrent-' + [guid]::NewGuid().ToString('N'))
$fixture = Join-Path $root 'fixture'
[IO.Directory]::CreateDirectory($fixture) | Out-Null
$folderCount = [Math]::Min(100, $FileCount)
$expectedObjects = $FileCount + $folderCount + 1
$rows = @()
$startedUtc = [DateTimeOffset]::UtcNow.ToString('o')
try {
    for ($i = 0; $i -lt $folderCount; $i++) { [IO.Directory]::CreateDirectory((Join-Path $fixture ('d' + $i))) | Out-Null }
    for ($i = 0; $i -lt $FileCount; $i++) { [IO.File]::WriteAllBytes((Join-Path $fixture ('d' + ($i % $folderCount)) ('f' + $i + '.txt')), [byte[]]@()) }
    foreach ($run in 1..$Runs) {
        $controlPath = Join-Path $root ("worker-$run.json")
        $row = [ordered]@{
            Run=$run; Status='starting'; Failure=$null; FileCount=$FileCount; ExpectedObjects=$expectedObjects
            ReplaySha256=(Get-FileHash -LiteralPath $replay -Algorithm SHA256).Hash
            CliSha256=(Get-FileHash -LiteralPath $cli -Algorithm SHA256).Hash
            SampleIntervalMilliseconds=$SampleIntervalMilliseconds; MemorySamples=0; PairedSamples=0; ConsumerOnlySamples=0; MissingSamples=0
            MinimumObservedIntervalMilliseconds=$null; MaximumObservedIntervalMilliseconds=$null; AverageObservedIntervalMilliseconds=$null
            MaximumObservedPipelineWorkingSetBytes=0L; MaximumObservedPipelinePrivateBytes=0L
            MaximumObservedReplayWorkingSetBytes=0L; MaximumObservedReplayPrivateBytes=0L
            MaximumObservedWorkerWorkingSetBytes=0L; MaximumObservedWorkerPrivateBytes=0L
            Objects=$null; Errors=$null; Unknown=$null; InputBytes=$null; TotalMilliseconds=$null
        }
        $start = [Diagnostics.ProcessStartInfo]::new($replay)
        $start.UseShellExecute=$false; $start.CreateNoWindow=$true; $start.RedirectStandardOutput=$true; $start.RedirectStandardError=$true
        foreach ($argument in @('--cli',$cli,'--scan-root',$fixture,'--worker-pid-file',$controlPath,'--expected-objects',"$expectedObjects",'--expected-root',$fixture,'--max-bytes',"$MaxOutputBytes",'--timeout-seconds',"$TimeoutSeconds")) { $start.ArgumentList.Add($argument) }
        $process=[Diagnostics.Process]::new(); $process.StartInfo=$start; $worker=$null; $started=$false
        $clock=[Diagnostics.Stopwatch]::StartNew(); $lastSample=$null; $intervalTotal=0.0
        try {
            $started=$process.Start()
            if (-not $started) { throw 'Replay process did not start.' }
            $stdout=$process.StandardOutput.ReadToEndAsync(); $stderr=$process.StandardError.ReadToEndAsync()
            while (-not $process.HasExited) {
                if ($clock.Elapsed.TotalSeconds -ge ($TimeoutSeconds + 10)) { $row.Failure='timeout'; $process.Kill($true); throw 'Concurrent replay timed out.' }
                if ($null -eq $worker -and (Test-Path -LiteralPath $controlPath -PathType Leaf)) {
                    try {
                        $control=Get-Content -LiteralPath $controlPath -Raw | ConvertFrom-Json
                        if ($control.schemaVersion -ne 1 -or $control.processId -lt 1) { throw 'Invalid worker control file.' }
                        $candidate=[Diagnostics.Process]::GetProcessById([int]$control.processId); $candidate.Refresh()
                        if ($candidate.StartTime.ToUniversalTime().Ticks -ne [long]$control.startTimeUtcTicks) { $candidate.Dispose(); throw 'Worker identity changed.' }
                        $worker=$candidate
                    } catch [ArgumentException] { $row.MissingSamples++ }
                }
                try {
                    $process.Refresh()
                    $replayWorking=$process.WorkingSet64; $replayPrivate=$process.PrivateMemorySize64
                    $row.MaximumObservedReplayWorkingSetBytes=[Math]::Max($row.MaximumObservedReplayWorkingSetBytes,$replayWorking)
                    $row.MaximumObservedReplayPrivateBytes=[Math]::Max($row.MaximumObservedReplayPrivateBytes,$replayPrivate)
                    $validSample=$false
                    if ($null -ne $worker) {
                        $worker.Refresh()
                        if (-not $worker.HasExited) {
                            $workerWorking=$worker.WorkingSet64; $workerPrivate=$worker.PrivateMemorySize64
                            $row.MaximumObservedPipelineWorkingSetBytes=[Math]::Max($row.MaximumObservedPipelineWorkingSetBytes,$replayWorking+$workerWorking)
                            $row.MaximumObservedPipelinePrivateBytes=[Math]::Max($row.MaximumObservedPipelinePrivateBytes,$replayPrivate+$workerPrivate)
                            $row.MaximumObservedWorkerWorkingSetBytes=[Math]::Max($row.MaximumObservedWorkerWorkingSetBytes,$workerWorking)
                            $row.MaximumObservedWorkerPrivateBytes=[Math]::Max($row.MaximumObservedWorkerPrivateBytes,$workerPrivate)
                            $row.PairedSamples++; $validSample=$true
                        } else {
                            $row.MaximumObservedPipelineWorkingSetBytes=[Math]::Max($row.MaximumObservedPipelineWorkingSetBytes,$replayWorking)
                            $row.MaximumObservedPipelinePrivateBytes=[Math]::Max($row.MaximumObservedPipelinePrivateBytes,$replayPrivate)
                            $row.ConsumerOnlySamples++; $validSample=$true
                        }
                    } else { $row.MissingSamples++ }
                    if ($validSample) {
                        $now=$clock.Elapsed.TotalMilliseconds
                        if ($null -ne $lastSample) {
                            $interval=$now-$lastSample; $intervalTotal+=$interval
                            if ($null -eq $row.MinimumObservedIntervalMilliseconds -or $interval -lt $row.MinimumObservedIntervalMilliseconds) { $row.MinimumObservedIntervalMilliseconds=$interval }
                            if ($null -eq $row.MaximumObservedIntervalMilliseconds -or $interval -gt $row.MaximumObservedIntervalMilliseconds) { $row.MaximumObservedIntervalMilliseconds=$interval }
                        }
                        $lastSample=$now; $row.MemorySamples++
                    }
                } catch [InvalidOperationException] { $row.MissingSamples++ }
                Start-Sleep -Milliseconds $SampleIntervalMilliseconds
            }
            if (-not [Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($stdout,$stderr),10000)) { $row.Failure='output-timeout'; throw 'Output drain timed out.' }
            if ($process.ExitCode -ne 0 -or -not [string]::IsNullOrEmpty($stderr.Result)) { $row.Failure='replay-rejected'; throw 'Concurrent replay failed without publishing its raw error stream.' }
            try { $result=$stdout.Result | ConvertFrom-Json } catch { $row.Failure='invalid-report'; throw }
            if ($result.schemaVersion -ne 2 -or $result.status -ne 'passed' -or $result.mode -ne 'concurrent-worker' -or
                $result.workerExitCode -ne 0 -or $result.objects -ne $expectedObjects -or $result.errors -ne 0 -or $result.unknown -ne 0 -or $row.PairedSamples -lt 1 -or $row.MemorySamples -lt 1) {
                $row.Failure='validation'; throw 'Concurrent replay validation failed.'
            }
            $row.Objects=$result.objects; $row.Errors=$result.errors; $row.Unknown=$result.unknown; $row.InputBytes=$result.inputBytes; $row.TotalMilliseconds=$result.totalMilliseconds
            if ($row.MemorySamples -gt 1) { $row.AverageObservedIntervalMilliseconds=$intervalTotal/($row.MemorySamples-1) }
            $row.Status='passed'
        } catch {
            $row.Status='failed'; if ($null -eq $row.Failure) { $row.Failure='unexpected' }
        } finally {
            if ($started -and -not $process.HasExited) { $process.Kill($true); $null=$process.WaitForExit(10000) }
            if ($null -ne $worker) { $worker.Dispose() }
            $process.Dispose(); $rows += [pscustomobject]$row
        }
    }
    $report=[ordered]@{
        SchemaVersion=1; Status=if (@($rows|Where-Object Status -ne 'passed').Count -eq 0) {'passed'} else {'failed'}
        StartedUtc=$startedUtc; CompletedUtc=[DateTimeOffset]::UtcNow.ToString('o'); Runs=$rows
        Notes=@(
            'Synthetic empty files with inherited ACLs; no user data was scanned.',
            'Pipeline maxima are per-sample sums, never sums of independent peaks; a confirmed exited worker contributes zero.',
            'Working-set sums can count shared pages twice and are not exclusive physical RAM.',
            'Private-memory sums are committed private bytes and are not resident physical RAM.',
            'The PowerShell supervisor, kernel memory, WinUI binding and rendering are excluded.',
            'Sampling follows the consumer through validation; paired, post-worker and unavailable samples are counted separately.',
            'Three runs describe repeatability only and do not establish statistical significance.'
        )
    }
    $report | ConvertTo-Json -Depth 8
    if ($report.Status -ne 'passed') { throw 'One or more concurrent measurements failed; sanitized results were emitted.' }
} finally {
    $resolvedRoot=[IO.Path]::GetFullPath($root); $resolvedTemp=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if ($resolvedRoot.StartsWith($resolvedTemp,[StringComparison]::OrdinalIgnoreCase) -and [IO.Path]::GetFileName($resolvedRoot).StartsWith('PermissionScope-concurrent-',[StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
