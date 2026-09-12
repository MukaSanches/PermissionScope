#requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ReplayPath,
    [Parameter(Mandatory)][string]$InputPath,
    [Parameter(Mandatory)][ValidateRange(1, 1000000)][int]$ExpectedObjects,
    [ValidateRange(1, 20)][int]$Runs = 3,
    [ValidateRange(1, 600)][int]$TimeoutSeconds = 120,
    [ValidateRange(1, 1073741824)][long]$MaxInputBytes = 134217728
)
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'This harness requires Windows.' }
$replay = (Resolve-Path -LiteralPath $ReplayPath).Path
$capturePath = (Resolve-Path -LiteralPath $InputPath).Path
$captureInfo = Get-Item -LiteralPath $capturePath
if ($captureInfo.Length -gt $MaxInputBytes) { throw 'Input exceeds MaxInputBytes.' }
$inputHash = (Get-FileHash -LiteralPath $capturePath -Algorithm SHA256).Hash
$reports = @()
foreach ($run in 1..$Runs) {
    $row = [ordered]@{ Run=$run; Status='starting'; Failure=$null; InputBytes=$captureInfo.Length; InputSha256=$inputHash;
        Lines=$null; ProgressMessages=$null; Objects=$null; Errors=$null; Unknown=$null; ReadMilliseconds=$null;
        DeserializeMilliseconds=$null; ValidationMilliseconds=$null; TotalMilliseconds=$null; AllocatedBytes=$null;
        WorkingSetBeforeBytes=$null; WorkingSetAfterDeserializeBytes=$null; WorkingSetAfterBytes=$null;
        ObservedPeakWorkingSetBytes=$null; MemorySamples=0 }
    $start = [Diagnostics.ProcessStartInfo]::new($replay)
    $start.UseShellExecute = $false; $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
    foreach ($argument in @('--input',$capturePath,'--expected-sha256',$inputHash,'--expected-objects',"$ExpectedObjects",'--max-bytes',"$MaxInputBytes")) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::new(); $process.StartInfo = $start
    $clock = [Diagnostics.Stopwatch]::StartNew(); $peak = 0L; $samples = 0; $started = $false
    try {
        $started = $process.Start()
        if (-not $started) { throw 'Replay process did not start.' }
        $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
        while (-not $process.HasExited) {
            if ($clock.Elapsed.TotalSeconds -ge $TimeoutSeconds) { $row.Failure='timeout'; $process.Kill($true); throw 'Replay timed out.' }
            $process.Refresh()
            try { $peak = [Math]::Max($peak, $process.PeakWorkingSet64); $samples++ } catch [InvalidOperationException] { }
            Start-Sleep -Milliseconds 20
        }
        if (-not [Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($stdout,$stderr),10000)) { $row.Failure='output-timeout'; throw 'Replay output drain timed out.' }
        if ($process.ExitCode -ne 0 -or -not [string]::IsNullOrEmpty($stderr.Result)) { $row.Failure='replay-rejected'; throw "Replay $run failed without publishing its raw error stream." }
        try { $result = $stdout.Result | ConvertFrom-Json } catch { $row.Failure='invalid-report'; throw }
        if ($result.status -ne 'passed' -or $result.objects -ne $ExpectedObjects -or $result.inputSha256 -ne $inputHash) { $row.Failure='validation'; throw "Replay $run validation failed." }
        foreach($name in @('Status','InputBytes','InputSha256','Lines','ProgressMessages','Objects','Errors','Unknown','ReadMilliseconds','DeserializeMilliseconds','ValidationMilliseconds','TotalMilliseconds','AllocatedBytes','WorkingSetBeforeBytes','WorkingSetAfterDeserializeBytes','WorkingSetAfterBytes')) { $row[$name]=$result.$name }
        $row.ObservedPeakWorkingSetBytes=$peak; $row.MemorySamples=$samples
    } catch {
        $row.Status='failed'
        if($null -eq $row.Failure){$row.Failure='unexpected'}
    } finally {
        if ($started -and -not $process.HasExited) { $process.Kill($true); $null=$process.WaitForExit(10000) }
        $process.Dispose()
        $reports += [pscustomobject]$row
    }
}
$reports | ConvertTo-Json -Depth 5
if (@($reports | Where-Object Status -ne 'passed').Count -gt 0) { throw 'One or more replays failed; sanitized results were emitted.' }
if (@($reports.InputSha256 | Select-Object -Unique).Count -ne 1) { throw 'Replay input hashes differ.' }
