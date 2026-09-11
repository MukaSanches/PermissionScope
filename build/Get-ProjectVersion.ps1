function Get-ProjectVersion {
    param([string]$Root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..')))
    $propsPath = Join-Path $Root 'Directory.Build.props'
    if (-not (Test-Path -LiteralPath $propsPath)) { throw 'Directory.Build.props was not found.' }
    [xml]$props = Get-Content -LiteralPath $propsPath -Raw
    $versionNode = $props.Project.PropertyGroup | ForEach-Object { $_.SelectSingleNode('Version') } | Where-Object { $_ } | Select-Object -First 1
    if (-not $versionNode) { throw 'Project version is missing from Directory.Build.props.' }
    $version = $versionNode.InnerText.Trim()
    if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "Release version must be numeric SemVer (major.minor.patch): $version" }
    return $version
}
