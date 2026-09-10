param([int]$ProcessId, [string]$AnalysisPath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
$process = Get-Process -Id $ProcessId
$window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
function Find-Element([string]$property, [string]$value) {
    $id = if ($property -eq 'Id') { [System.Windows.Automation.AutomationElement]::AutomationIdProperty } else { [System.Windows.Automation.AutomationElement]::NameProperty }
    $condition = New-Object System.Windows.Automation.PropertyCondition $id,$value
    for ($attempt=0; $attempt -lt 40; $attempt++) {
        $element = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,$condition)
        if ($element) { return $element }
        Start-Sleep -Milliseconds 150
    }
    throw "Control not found: $property=$value"
}
function Invoke-Button($element) { $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }
Invoke-Button (Find-Element 'Name' 'Analisar')
$input = Find-Element 'Id' 'AnalysisPath'
$input.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($AnalysisPath)
Invoke-Button (Find-Element 'Id' 'RunAnalysis')
$save = Find-Element 'Name' 'Salvar snapshot'
'PASS Analyze: result view is reachable'
$accessTab = Find-Element 'Id' 'TabAccess'
$accessTab.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle()
Invoke-Button (Find-Element 'Name' ('Access Path ' + [char]0x2192))
'PASS Access Path: evidence view is reachable'
Invoke-Button $save
[void](Find-Element 'Name' 'Snapshot salvo')
'PASS Snapshot: save completed'
