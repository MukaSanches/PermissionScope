$ErrorActionPreference = 'Stop'
if ($env:GITHUB_ACTIONS -ne 'true' -or [string]::IsNullOrWhiteSpace($env:RUNNER_TEMP)) {
    throw 'Run this installation test only on a disposable GitHub Actions runner.'
}
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$temporaryRoot = [IO.Path]::GetFullPath($env:RUNNER_TEMP).TrimEnd('\') + '\'
$destination = [IO.Path]::GetFullPath((Join-Path $temporaryRoot 'PermissionScope-install-test'))
if (-not $destination.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid test destination.' }
$registry = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\PermissionScope'
if ((Test-Path -LiteralPath $registry) -or (Test-Path -LiteralPath $destination)) { throw 'An existing installation or test directory must not be overwritten.' }
$installer = Join-Path $root 'artifacts/PermissionScope-1.0.0-x64-Setup.exe'
$install = Start-Process -FilePath $installer -ArgumentList "/S /D=$destination" -Wait -PassThru -WindowStyle Hidden
if ($install.ExitCode -ne 0) { throw "Installer exited with $($install.ExitCode)." }
$metadata = Get-ItemProperty -LiteralPath $registry
if ($metadata.InstallLocation -ne $destination -or $metadata.DisplayVersion -ne '1.0.0') { throw 'Installation registration differs from the requested destination/version.' }
try {
    $env:PERMISSIONSCOPE_DATA_DIR = Join-Path $temporaryRoot 'PermissionScope-install-test-data'
    $fixture = Join-Path $temporaryRoot 'PermissionScope-install-test-fixture'
    New-Item -ItemType Directory -Path $fixture -Force | Out-Null
    $cli = Join-Path $destination 'permissionscope-cli.exe'
    $json = & $cli explain $fixture --json
    if ($LASTEXITCODE -notin @(0,3)) { throw 'Installed CLI could not scan the fixture.' }
    $result = ($json -join "`n") | ConvertFrom-Json
    if ($result.Resources.Count -ne 1 -or $result.Resources[0].Error) { throw 'Installed CLI did not return the expected resource.' }
    if (-not (Test-Path -LiteralPath (Join-Path $destination 'PermissionScope.exe'))) { throw 'Desktop executable is missing.' }
} finally {
    $uninstaller = Join-Path $destination 'Uninstall.exe'
    $uninstall = Start-Process -FilePath $uninstaller -ArgumentList "/S _?=$destination" -Wait -PassThru -WindowStyle Hidden
    if ($uninstall.ExitCode -ne 0) { throw "Uninstaller exited with $($uninstall.ExitCode)." }
}
if ((Test-Path -LiteralPath $registry) -or (Test-Path -LiteralPath (Join-Path $destination 'PermissionScope.exe')) -or (Test-Path -LiteralPath (Join-Path $destination 'permissionscope-cli.exe'))) {
    throw 'Uninstallation left registered application files.'
}
@{ passed=$true; architecture='x64'; installation='silent per-user'; cliScan=$true; uninstallation=$true; desktopWalkthrough=$false } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $root 'artifacts/installer-test.json') -Encoding UTF8
'Silent installation, installed CLI scan and uninstallation passed on the disposable runner.'
