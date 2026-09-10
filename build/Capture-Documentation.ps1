param(
    [string]$Application = "$PSScriptRoot/../artifacts/documentation-app/PermissionScope.exe",
    [string[]]$Locales = @('en-US','pt-BR','es','fr','de','ar','ja','zh-Hans'),
    [string[]]$Scenes = @('home','analyze','access','access-path','compare','simulation','technical','unknown'),
    [ValidateSet('Light','Dark')][string]$Theme = 'Light'
)
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath("$PSScriptRoot/..")
$Application = [IO.Path]::GetFullPath($Application)
if (-not (Test-Path -LiteralPath $Application)) { throw 'Publish the application to artifacts/documentation-app first.' }
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class DocumentationCapture {
 [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr value);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window, IntPtr dc, uint flags);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr window, out Rect rect);
 [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr window);
 public struct Rect { public int Left,Top,Right,Bottom; }
}
'@
[void][DocumentationCapture]::SetThreadDpiAwarenessContext([IntPtr](-4))
. "$PSScriptRoot/Get-DocumentationFingerprint.ps1"
$fingerprint = Get-DocumentationFingerprint -Root $repository
$output = Join-Path $repository 'docs/screenshots'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$manifestPath = Join-Path $output 'manifest.json'
$records = @()
if (Test-Path -LiteralPath $manifestPath) { $records = @((Get-Content $manifestPath -Raw | ConvertFrom-Json).captures) }
function Find-Control($window, [string]$id) {
    $condition = New-Object System.Windows.Automation.PropertyCondition ([System.Windows.Automation.AutomationElement]::AutomationIdProperty),$id
    for ($attempt=0; $attempt -lt 80; $attempt++) {
        $control = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,$condition)
        if ($control) { return $control }
        Start-Sleep -Milliseconds 100
    }
    throw "Missing demo control: $id"
}
foreach ($locale in $Locales) {
    if ($locale -notin @('en-US','pt-BR','es','fr','de','ar','ja','zh-Hans')) { throw 'Unsupported locale.' }
    foreach ($scene in $Scenes) {
        if ($scene -notin @('home','analyze','access','access-path','compare','simulation','technical','unknown')) { throw 'Unsupported scene.' }
        $data = Join-Path $repository "artifacts/documentation-data/$locale-$scene-$Theme"
        New-Item -ItemType Directory -Path $data -Force | Out-Null
        @{Language=$locale;Appearance=$Theme} | ConvertTo-Json | Set-Content (Join-Path $data 'settings.json') -Encoding UTF8
        $start = New-Object Diagnostics.ProcessStartInfo
        $start.FileName=$Application; $start.Arguments="--demo $scene"; $start.UseShellExecute=$false
        $start.EnvironmentVariables['PERMISSIONSCOPE_DATA_DIR']=$data
        $process = [Diagnostics.Process]::Start($start)
        try {
            for ($attempt=0; $attempt -lt 500; $attempt++) {
                $process.Refresh()
                if ($process.HasExited) { throw 'Demo application exited before capture.' }
                if ($process.MainWindowHandle -ne 0) { break }
                Start-Sleep -Milliseconds 100
            }
            if ($process.MainWindowHandle -eq 0) { throw 'Demo window did not open.' }
            Start-Sleep -Milliseconds 1000
            $handle = $process.MainWindowHandle
            $window = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
            if ($scene -in @('compare','simulation')) {
                $button = if ($scene -eq 'compare') {'RunComparison'} else {'RunSimulation'}
                (Find-Control $window $button).GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
                Start-Sleep -Milliseconds 800
            }
            $elements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
            $visibleText = ($elements | ForEach-Object { $_.Current.Name }) -join "`n"
            foreach ($private in @($env:USERPROFILE,$env:COMPUTERNAME,[Security.Principal.WindowsIdentity]::GetCurrent().User.Value)) {
                if ($private -and $visibleText.IndexOf($private,[StringComparison]::OrdinalIgnoreCase) -ge 0) { throw 'Private host data detected in demo accessibility tree.' }
            }
            $catalog = Get-Content (Join-Path $repository "src/PermissionScope.App/Locales/$locale.json") -Raw -Encoding UTF8 | ConvertFrom-Json
            if (-not $visibleText.Contains($catalog.DemoTitle)) { throw 'Localized synthetic-demo banner was not found.' }
            if ($scene -in @('simulation','technical','access-path','unknown')) {
                $pane = Find-Control $window 'AccessDetail'
                if ($scene -eq 'unknown') {
                    $expanders = $pane.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
                    foreach ($element in $expanders) {
                        if ($element.Current.Name -eq $catalog.SummaryUnknown) {
                            $pattern=$null
                            if ($element.TryGetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern,[ref]$pattern)) { $pattern.Expand(); break }
                        }
                    }
                }
                $scroll=$null
                if ($pane.TryGetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern,[ref]$scroll) -and $scroll.Current.VerticallyScrollable) {
                    $percent = if ($scene -eq 'simulation') { 58 } elseif ($scene -eq 'technical') { 45 } elseif ($scene -eq 'access-path') { 45 } else { 30 }
                    $scroll.SetScrollPercent(-1,$percent)
                }
                Start-Sleep -Milliseconds 300
            }
            $folder = Join-Path $output $locale
            New-Item -ItemType Directory -Path $folder -Force | Out-Null
            $relative = "$locale/$scene-$($Theme.ToLowerInvariant()).png"
            $path = Join-Path $output $relative
            $rect = New-Object DocumentationCapture+Rect
            if (-not [DocumentationCapture]::GetWindowRect($handle,[ref]$rect)) { throw 'Cannot obtain window bounds.' }
            $bitmap = New-Object Drawing.Bitmap ($rect.Right-$rect.Left),($rect.Bottom-$rect.Top)
            $graphics = [Drawing.Graphics]::FromImage($bitmap); $dc=$graphics.GetHdc()
            try { if (-not [DocumentationCapture]::PrintWindow($handle,$dc,2)) { throw 'Native window capture failed.' } }
            finally { $graphics.ReleaseHdc($dc); $graphics.Dispose() }
            try { $bitmap.Save($path,[Drawing.Imaging.ImageFormat]::Png) } finally { $bitmap.Dispose() }
            $records = @($records | Where-Object { $_.path -ne $relative })
            $records += [pscustomobject]@{path=$relative;locale=$locale;scene=$scene;theme=$Theme;fixture='permissionscope-demo-v1';version='1.0.0';sourceSha256=$fingerprint;sha256=(Get-FileHash $path -Algorithm SHA256).Hash;width=$rect.Right-$rect.Left;height=$rect.Bottom-$rect.Top;dpi=[DocumentationCapture]::GetDpiForWindow($handle);capturedAt=[DateTimeOffset]::UtcNow.ToString('O');privacyCheck='Synthetic source and own-process accessibility tree checked; no OCR certification'}
            @{schema=1;captures=@($records | Sort-Object path)} | ConvertTo-Json -Depth 5 | Set-Content $manifestPath -Encoding UTF8
            Write-Output "Captured $relative"
        }
        finally { if (-not $process.HasExited) { [void]$process.CloseMainWindow(); if (-not $process.WaitForExit(3000)) { $process.Kill() } }; $process.Dispose() }
    }
}
if ((Get-DocumentationFingerprint -Root $repository) -ne $fingerprint) { throw 'Source changed during capture; regenerate the images.' }
