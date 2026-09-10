param([Parameter(Mandatory=$true)][int]$ProcessId, [Parameter(Mandatory=$true)][string]$AnalysisPath, [string]$DataDirectory=(Join-Path $env:LOCALAPPDATA 'PermissionScope'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class ScopeLayoutTest {
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int w, int height, uint flags);
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr c);
}
'@
$operationLog = Join-Path $DataDirectory 'last-operation-error.log'
$started = [DateTime]::UtcNow
function Wait-Control([string]$id) {
    for ($attempt=0; $attempt -lt 60; $attempt++) {
        $process = Get-Process -Id $ProcessId
        $window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
        $element = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,(New-Object System.Windows.Automation.PropertyCondition ([System.Windows.Automation.AutomationElement]::AutomationIdProperty),$id))
        if ($element) { return $element }
        Start-Sleep -Milliseconds 100
    }
    throw "Control not found: $id"
}
for ($iteration=0; $iteration -lt 4; $iteration++) {
    (Wait-Control 'NavHome').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Milliseconds 250
    & (Join-Path $PSScriptRoot 'Test-Ui.ps1') -ProcessId $ProcessId -AnalysisPath $AnalysisPath
    $handle = (Get-Process -Id $ProcessId).MainWindowHandle
    [void][ScopeLayoutTest]::SetThreadDpiAwarenessContext([IntPtr](-4))
    $scale = [ScopeLayoutTest]::GetDpiForWindow($handle) / 96.0
    $width = if ($iteration % 2 -eq 0) { 720 } else { 1180 }
    if (-not [ScopeLayoutTest]::SetWindowPos($handle,[IntPtr]::Zero,0,0,[int]($width*$scale),[int](760*$scale),6)) { throw 'Resize failed.' }
    Start-Sleep -Milliseconds 400
    [void](Wait-Control 'NavSettings')
    (Wait-Control 'NavSettings').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    [void](Wait-Control 'AppearanceChoice')
    if ((Test-Path -LiteralPath $operationLog) -and (Get-Item -LiteralPath $operationLog).LastWriteTimeUtc -gt $started) { throw (Get-Content -LiteralPath $operationLog -Raw -Encoding UTF8) }
    "PASS page reuse and responsive navigation: cycle $($iteration+1)"
}
