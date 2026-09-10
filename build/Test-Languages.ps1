param([int]$ProcessId, [string]$DataDirectory)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes
function Find-Control([string]$id) {
    for ($attempt=0; $attempt -lt 50; $attempt++) {
        $window=[System.Windows.Automation.AutomationElement]::FromHandle((Get-Process -Id $ProcessId).MainWindowHandle)
        $control=$window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,(New-Object System.Windows.Automation.PropertyCondition ([System.Windows.Automation.AutomationElement]::AutomationIdProperty),$id))
        if ($control) { return $control }
        Start-Sleep -Milliseconds 100
    }
    throw "Missing control: $id"
}
$started=[DateTime]::UtcNow
foreach ($locale in @('ar','ja','de','fr','es','zh-Hans','en-US')) {
    (Find-Control 'NavSettings').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    $combo=Find-Control 'LanguageChoice'
    $combo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    Start-Sleep -Milliseconds 250
    $option=Find-Control ("Language_" + $locale)
    $option.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    (Find-Control 'ApplyPreferences').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Milliseconds 350
    $preferences=Get-Content -LiteralPath (Join-Path $DataDirectory 'settings.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($preferences.Language -ne $locale) { throw "Locale preference did not persist: $locale" }
    (Find-Control 'NavHome').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Milliseconds 250
    & (Join-Path $PSScriptRoot 'Inspect-Window.ps1') -ProcessId $ProcessId -Screenshot (Join-Path $PSScriptRoot "../artifacts/ui-$locale.png")
    $log=Join-Path $DataDirectory 'last-operation-error.log'
    if ((Test-Path -LiteralPath $log) -and (Get-Item -LiteralPath $log).LastWriteTimeUtc -gt $started) { throw (Get-Content -LiteralPath $log -Raw -Encoding UTF8) }
    "PASS language selection, persistence and page navigation: $locale"
}
