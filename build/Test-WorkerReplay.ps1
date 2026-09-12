#requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ReplayPath,
    [Parameter(Mandatory)][string]$CliPath
)
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'Windows required.' }
$replay = (Resolve-Path -LiteralPath $ReplayPath).Path
$cli = (Resolve-Path -LiteralPath $CliPath).Path
$root = Join-Path ([IO.Path]::GetTempPath()) ('PermissionScope-replay-tests-' + [guid]::NewGuid().ToString('N'))
$fixture = Join-Path $root 'dados-测试-δοκιμή'
[IO.Directory]::CreateDirectory($fixture) | Out-Null
[IO.File]::WriteAllText((Join-Path $fixture 'arquivo-ç.txt'), '')
$valid = Join-Path $root 'valid.jsonl'
function Assert-True([bool]$Condition,[string]$Message) { if(-not $Condition){throw $Message} }
function Invoke-Captured([string]$Executable,[string[]]$Arguments,[string]$OutputPath) {
    $start=[Diagnostics.ProcessStartInfo]::new($Executable); $start.UseShellExecute=$false; $start.CreateNoWindow=$true
    $start.RedirectStandardOutput=$true; $start.RedirectStandardError=$true
    foreach($argument in $Arguments){$start.ArgumentList.Add($argument)}
    $process=[Diagnostics.Process]::new(); $process.StartInfo=$start
    $started=$false
    try {
        $started=$process.Start(); Assert-True $started 'Process failed to start.'
        $stdoutTask=$process.StandardOutput.ReadToEndAsync(); $stderrTask=$process.StandardError.ReadToEndAsync()
        Assert-True ($process.WaitForExit(30000)) 'Process timed out.'
        Assert-True ([Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($stdoutTask,$stderrTask),10000)) 'Output drain timed out.'
        [IO.File]::WriteAllText($OutputPath,$stdoutTask.Result,[Text.UTF8Encoding]::new($false))
        return [pscustomobject]@{ExitCode=$process.ExitCode;Error=$stderrTask.Result}
    } finally {
        if($started -and -not $process.HasExited){$process.Kill($true);$null=$process.WaitForExit(10000)}
        $process.Dispose()
    }
}
function Invoke-Replay([string]$CapturePath,[string]$Hash,[long]$MaxBytes=1048576,[string]$ExpectedRoot=$fixture) {
    $out=Join-Path $root ([guid]::NewGuid().ToString('N')+'.out')
    $run=Invoke-Captured -Executable $replay -Arguments @('--input',$CapturePath,'--expected-sha256',$Hash,'--expected-objects','2','--expected-root',$ExpectedRoot,'--max-bytes',"$MaxBytes") -OutputPath $out
    $text=[IO.File]::ReadAllText($out); [pscustomobject]@{ExitCode=$run.ExitCode;Error=$run.Error;Output=$text}
}
try {
    $scan=Invoke-Captured -Executable $cli -Arguments @('scan',$fixture,'--files','--worker') -OutputPath $valid
    Assert-True ($scan.ExitCode -eq 0 -and [string]::IsNullOrEmpty($scan.Error)) 'Synthetic worker capture failed.'
    $hash=(Get-FileHash -LiteralPath $valid -Algorithm SHA256).Hash
    $ok=Invoke-Replay $valid $hash
    Assert-True ($ok.ExitCode -eq 0 -and [string]::IsNullOrEmpty($ok.Error)) 'Valid replay failed.'
    $report=$ok.Output|ConvertFrom-Json
    Assert-True ($report.status -eq 'passed' -and $report.objects -eq 2 -and $report.snapshots -eq 1) 'Valid replay result is incomplete.'
    $wrongRoot=Invoke-Replay $valid $hash 1048576 ($fixture+'-different')
    Assert-True ($wrongRoot.ExitCode -eq 1 -and $wrongRoot.Error.TrimEnd() -eq 'Worker replay failed without exposing input content.') 'Decoded Unicode root was not verified.'

    $content=[IO.File]::ReadAllText($valid)
    $malformed=Join-Path $root 'malformed.jsonl'; [IO.File]::WriteAllText($malformed,'{',[Text.UTF8Encoding]::new($false))
    $trimmed=$content.TrimEnd(); $truncated=Join-Path $root 'truncated.jsonl'; [IO.File]::WriteAllText($truncated,$trimmed.Substring(0,$trimmed.Length-1),[Text.UTF8Encoding]::new($false))
    $duplicate=Join-Path $root 'duplicate.jsonl'; [IO.File]::WriteAllText($duplicate,$content+$content,[Text.UTF8Encoding]::new($false))
    $emptyMessage=Join-Path $root 'empty-message.jsonl'; [IO.File]::WriteAllText($emptyMessage,"{}`n",[Text.UTF8Encoding]::new($false))
    foreach($invalid in @($malformed,$truncated,$duplicate,$emptyMessage)) {
        $invalidHash=(Get-FileHash -LiteralPath $invalid -Algorithm SHA256).Hash
        $failed=Invoke-Replay $invalid $invalidHash
        Assert-True ($failed.ExitCode -eq 1 -and $failed.Error.TrimEnd() -eq 'Worker replay failed without exposing input content.') 'Invalid replay did not fail safely.'
        Assert-True (-not $failed.Error.Contains($invalid,[StringComparison]::OrdinalIgnoreCase)) 'Failure exposed an input path.'
    }
    $wrongHash=Invoke-Replay $valid ('0'*64)
    Assert-True ($wrongHash.ExitCode -eq 2 -and [string]::IsNullOrEmpty($wrongHash.Error)) 'Hash mismatch was not rejected.'
    $tooLarge=Invoke-Replay $valid $hash 1
    Assert-True ($tooLarge.ExitCode -eq 2 -and [string]::IsNullOrEmpty($tooLarge.Error)) 'Input limit was not enforced.'
    $failureOutput=Join-Path $root 'failure-report.json'; $failureError=Join-Path $root 'failure-report.err'
    & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Measure-WorkerReplay.ps1') -ReplayPath $replay -InputPath $valid -ExpectedObjects 999 -Runs 2 -MaxInputBytes 1048576 1>$failureOutput 2>$failureError
    $failureExit=$LASTEXITCODE; $failedRows=Get-Content $failureOutput -Raw|ConvertFrom-Json
    Assert-True ($failureExit -ne 0 -and @($failedRows).Count -eq 2 -and @($failedRows|Where-Object {$_.Status -ne 'failed'}).Count -eq 0) 'Supervisor did not preserve sanitized failed rows.'
    'PASS: typed replay, Unicode root, malformed/truncated/duplicate/missing snapshots, hash and size limits, sanitized failures and retained failure rows.'
} finally {
    $resolvedRoot=[IO.Path]::GetFullPath($root); $resolvedTemp=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if($resolvedRoot.StartsWith($resolvedTemp,[StringComparison]::OrdinalIgnoreCase) -and [IO.Path]::GetFileName($resolvedRoot).StartsWith('PermissionScope-replay-tests-',[StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedRoot -Recurse -Force
    }
}
