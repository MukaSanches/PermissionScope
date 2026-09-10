param([int]$ProcessId, [string]$Screenshot, [switch]$ListControls)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class ScopeWindowCapture {
    [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    public struct Rect { public int Left, Top, Right, Bottom; }
}
'@
$process = Get-Process -Id $ProcessId
[void][ScopeWindowCapture]::SetThreadDpiAwarenessContext([IntPtr](-4))
$handle = $process.MainWindowHandle
if ($handle -eq 0) { throw 'Application has no main window.' }
$window = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
if ($ListControls) {
    $elements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($element in $elements) {
        if ($element.Current.ControlType.ProgrammaticName -match 'Button|Edit|ComboBox|ListItem') {
            [pscustomobject]@{ Type=$element.Current.ControlType.ProgrammaticName; Name=$element.Current.Name; Id=$element.Current.AutomationId; Enabled=$element.Current.IsEnabled }
        }
    }
}
if ($Screenshot) {
    $rect = New-Object ScopeWindowCapture+Rect
    [void][ScopeWindowCapture]::GetWindowRect($handle, [ref]$rect)
    $bitmap = New-Object System.Drawing.Bitmap ($rect.Right - $rect.Left), ($rect.Bottom - $rect.Top)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $dc = $graphics.GetHdc()
    try { [void][ScopeWindowCapture]::PrintWindow($handle, $dc, 2) }
    finally { $graphics.ReleaseHdc($dc); $graphics.Dispose() }
    try { $bitmap.Save($Screenshot, [System.Drawing.Imaging.ImageFormat]::Png) }
    finally { $bitmap.Dispose() }
}
