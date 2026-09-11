#requires -Version 7.0
[CmdletBinding()]
param(
    [string]$Application = "$PSScriptRoot/../artifacts/production-trust-app/PermissionScope.exe",
    [string[]]$Locales = @('en-US','pt-BR','es','fr','de','ar','ja','zh-Hans'),
    [ValidateSet('Light','Dark')][string[]]$Themes = @('Light','Dark'),
    [ValidateRange(5,120)][int]$TimeoutSeconds = 45
)
$ErrorActionPreference='Stop'
if (-not $IsWindows) { throw 'Native UI trust automation requires Windows.' }
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$Application=[IO.Path]::GetFullPath($Application)
if (-not (Test-Path -LiteralPath $Application)) { throw "Application not found: $Application" }
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes

$allowedLocales=@('en-US','pt-BR','es','fr','de','ar','ja','zh-Hans')
foreach($locale in $Locales){ if($locale -notin $allowedLocales){ throw "Unsupported locale: $locale" } }
$outputDir=Join-Path $root 'artifacts/production-trust'
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$records=@()
$appHash=(Get-FileHash -LiteralPath $Application -Algorithm SHA256).Hash

function Wait-MainWindow([Diagnostics.Process]$process){
    $deadline=[DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    while([DateTime]::UtcNow -lt $deadline){
        $process.Refresh()
        if($process.HasExited){ throw "Application exited unexpectedly with code $($process.ExitCode)." }
        if($process.MainWindowHandle -ne 0){ return [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle) }
        Start-Sleep -Milliseconds 100
    }
    throw 'Application main window did not become available.'
}
function Find-Id($window,[string]$id,[int]$seconds=10){
    $condition=New-Object System.Windows.Automation.PropertyCondition ([System.Windows.Automation.AutomationElement]::AutomationIdProperty),$id
    $deadline=[DateTime]::UtcNow.AddSeconds($seconds)
    while([DateTime]::UtcNow -lt $deadline){
        $element=$window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,$condition)
        if($element){ return $element }
        Start-Sleep -Milliseconds 100
    }
    throw "AutomationId not found: $id"
}
function Invoke-Control($element){
    $pattern=$null
    if($element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern,[ref]$pattern)){ $pattern.Invoke(); return }
    if($element.TryGetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern,[ref]$pattern)){ $pattern.Toggle(); return }
    throw "Control does not expose Invoke or Toggle: $($element.Current.AutomationId)"
}
function Get-VisibleText($window){
    $elements=$window.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
    return (($elements | ForEach-Object { $_.Current.Name } | Where-Object { $_ }) -join "`n")
}
function Start-Demo([string]$locale,[string]$theme,[string]$scene){
    $data=Join-Path $root "artifacts/production-trust-data/$locale-$theme-$scene"
    if(Test-Path -LiteralPath $data){ Remove-Item -LiteralPath $data -Recurse -Force }
    New-Item -ItemType Directory -Path $data -Force | Out-Null
    @{Language=$locale;Appearance=$theme} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $data 'settings.json') -Encoding UTF8
    $start=[Diagnostics.ProcessStartInfo]::new()
    $start.FileName=$Application
    $start.ArgumentList.Add('--demo'); $start.ArgumentList.Add($scene)
    $start.UseShellExecute=$false
    $start.Environment['PERMISSIONSCOPE_DATA_DIR']=$data
    return [Diagnostics.Process]::Start($start)
}
function Stop-Demo([Diagnostics.Process]$process){
    if(-not $process.HasExited){
        [void]$process.CloseMainWindow()
        if(-not $process.WaitForExit(3000)){ $process.Kill($true); [void]$process.WaitForExit(5000) }
    }
    $process.Dispose()
}

foreach($locale in $Locales){
    $catalog=Get-Content -LiteralPath (Join-Path $root "src/PermissionScope.App/Locales/$locale.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach($theme in $Themes){
        $clock=[Diagnostics.Stopwatch]::StartNew()
        $process=Start-Demo $locale $theme 'access'
        try{
            $window=Wait-MainWindow $process
            Start-Sleep -Milliseconds 350
            foreach($id in @('NavHome','NavAnalyze','NavScans','NavCompare','NavSettings','AccessDetail','TabAccess','TabWhy','TabFindings','TabPermissions','TabTechnical','TabSimulate')){ [void](Find-Id $window $id) }
            $visible=Get-VisibleText $window
            if(-not $visible.Contains([string]$catalog.DemoTitle)){ throw "Localized demo title missing for $locale/$theme." }
            foreach($private in @($env:USERPROFILE,$env:COMPUTERNAME,[Security.Principal.WindowsIdentity]::GetCurrent().User.Value)){
                if($private -and $visible.IndexOf($private,[StringComparison]::OrdinalIgnoreCase) -ge 0){ throw "Private host value leaked into synthetic demo accessibility tree for $locale/$theme." }
            }
            Invoke-Control (Find-Id $window 'TabWhy')
            Start-Sleep -Milliseconds 200
            $visible=Get-VisibleText $window
            if($catalog.PSObject.Properties.Name -contains 'Contributors' -and -not $visible.Contains([string]$catalog.Contributors)){ throw "Access Path contributors surface was not reachable for $locale/$theme." }
            Invoke-Control (Find-Id $window 'NavSettings')
            [void](Find-Id $window 'AppearanceChoice')
            Invoke-Control (Find-Id $window 'NavAnalyze')
            foreach($id in @('AnalysisPath','AnalysisIdentity','RunAnalysis')){ [void](Find-Id $window $id) }
            $clock.Stop()
            $records += [pscustomobject]@{locale=$locale;theme=$theme;status='passed';elapsedMilliseconds=[math]::Round($clock.Elapsed.TotalMilliseconds,2)}
            Write-Host "PASS native UI trust: $locale / $theme"
        }
        finally{ Stop-Demo $process }
    }
}

# Exercise the two result-producing demo workflows once with stable AutomationIds.
foreach($scene in @('compare','simulation')){
    $process=Start-Demo 'en-US' 'Light' $scene
    try{
        $window=Wait-MainWindow $process
        if($scene -eq 'compare'){
            Invoke-Control (Find-Id $window 'RunComparison')
            [void](Find-Id $window 'ComparisonResult')
        } else {
            Invoke-Control (Find-Id $window 'RunSimulation')
            [void](Find-Id $window 'SimulationResult')
        }
        Write-Host "PASS native UI workflow: $scene"
    }
    finally{ Stop-Demo $process }
}

$report=[ordered]@{
    schema=1
    applicationSha256=$appHash
    localeCount=$Locales.Count
    themeCount=$Themes.Count
    journeys=$records
    comparisonWorkflow='passed'
    simulationWorkflow='passed'
    privateHostDataCheck='passed'
    screenReaderCertification=$false
    note='UI Automation verifies stable control reachability, localized synthetic surfaces and key workflows. It does not claim Narrator/NVDA certification.'
    completedUtc=[DateTimeOffset]::UtcNow.ToString('O')
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $outputDir 'ui-trust.json') -Encoding UTF8
Write-Host "PASS Production Trust UI automation: $($records.Count) locale/theme journeys plus compare and simulation."
