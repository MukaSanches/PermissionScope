param([string]$Cli="$PSScriptRoot/../artifacts/documentation-cli/permissionscope-cli.exe")
$ErrorActionPreference='Stop'
$output=[IO.Path]::GetFullPath("$PSScriptRoot/../samples/reports")
& $Cli demo --output $output
if ($LASTEXITCODE -ne 0) { throw 'Synthetic report generation failed.' }
'Created synthetic reports only. No Windows accounts, groups or resource ACLs were created.'
