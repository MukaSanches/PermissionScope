param([ValidateSet('x64','arm64')][string]$Architecture='x64', [switch]$SkipBuild)
$ErrorActionPreference = 'Stop'
$repository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$dotnet = Join-Path $repository '.toolchain/dotnet/dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) { $dotnet = 'dotnet' }
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$distribution = Join-Path $repository "artifacts/PermissionScope-1.0.0-win-$Architecture"
Push-Location $repository
try {
    . (Join-Path $PSScriptRoot 'Get-SourceFingerprint.ps1')
    $sourceFingerprint = Get-SourceFingerprint -Root $repository
    if (-not $SkipBuild) {
        $resolvedDistribution = [System.IO.Path]::GetFullPath($distribution)
        $artifactRoot = [System.IO.Path]::GetFullPath((Join-Path $repository 'artifacts')) + [System.IO.Path]::DirectorySeparatorChar
        if (-not $resolvedDistribution.StartsWith($artifactRoot,[System.StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid build output directory.' }
        if (Test-Path -LiteralPath $resolvedDistribution) { Remove-Item -LiteralPath $resolvedDistribution -Recurse -Force }
        & $dotnet publish src/PermissionScope.App/PermissionScope.App.csproj -c Release "-p:Platform=$Architecture" -r "win-$Architecture" --self-contained true -o $distribution --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { throw 'Application publish failed.' }
        & $dotnet publish src/PermissionScope.Cli/PermissionScope.Cli.csproj -c Release -r "win-$Architecture" --self-contained true -o $distribution --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { throw 'CLI publish failed.' }
        if ((Get-SourceFingerprint -Root $repository) -ne $sourceFingerprint) { throw 'Source changed during packaging. Rebuild before distribution.' }
        @{ sourceSha256=$sourceFingerprint; architecture=$Architecture; version='1.0.0'; builtAt=[DateTimeOffset]::UtcNow.ToString('O') } |
            ConvertTo-Json | Set-Content -LiteralPath (Join-Path $distribution 'build-info.json') -Encoding UTF8
    }
    $buildInfo = Get-Content -LiteralPath (Join-Path $distribution 'build-info.json') -Raw | ConvertFrom-Json
    if ($buildInfo.sourceSha256 -ne $sourceFingerprint) { throw 'Existing binaries do not match the current source. Run packaging without SkipBuild.' }
    foreach ($name in @('LICENSE','NOTICE','FREE-FOREVER.md','README.md','LEIA-ME.pt-BR.md','THIRD-PARTY-NOTICES.md','SECURITY.md','CHANGELOG.md')) { Copy-Item -LiteralPath (Join-Path $repository $name) -Destination $distribution }
    Get-ChildItem -LiteralPath $repository -Filter 'README.*.md' | Copy-Item -Destination $distribution
    Copy-Item -LiteralPath (Join-Path $repository 'docs') -Destination $distribution -Recurse -Force
    $legal = Join-Path $distribution 'ThirdParty'
    New-Item -ItemType Directory -Path $legal -Force | Out-Null
    $packages = Get-Content -LiteralPath 'src/PermissionScope.App/packages.lock.json' -Raw | ConvertFrom-Json
    $seen = @{}
    $components = @()
    foreach ($target in $packages.dependencies.PSObject.Properties) {
        foreach ($package in $target.Value.PSObject.Properties) {
            if ($package.Value.type -eq 'Project' -or $seen.ContainsKey($package.Name)) { continue }
            $seen[$package.Name] = $true
            $version = $package.Value.resolved
            $components += @{type='library';name=$package.Name;version=$version;purl="pkg:nuget/$($package.Name)@$version"}
            $packagePath = Join-Path $env:USERPROFILE ".nuget/packages/$($package.Name.ToLowerInvariant())/$version"
            if (Test-Path -LiteralPath $packagePath) {
                $packageLegal = Join-Path $legal "$($package.Name)-$version"
                New-Item -ItemType Directory -Path $packageLegal -Force | Out-Null
                Get-ChildItem -LiteralPath $packagePath -File | Where-Object { $_.Name -match 'license|notice|copying|\.nuspec$' } | Copy-Item -Destination $packageLegal
            }
        }
    }
    $sbom = @{bomFormat='CycloneDX';specVersion='1.5';version=1;metadata=@{component=@{type='application';name='PermissionScope';version='1.0.0';licenses=@(@{license=@{id='Apache-2.0'}})}};components=$components}
    $sbom | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $distribution 'sbom.cdx.json') -Encoding UTF8
    $files = Get-ChildItem -LiteralPath $distribution -File -Recurse
    $uninstall = @()
    foreach ($file in $files) { $relative = $file.FullName.Substring($distribution.Length+1); $uninstall += 'Delete "$INSTDIR\' + $relative + '"' }
    foreach ($directory in (Get-ChildItem -LiteralPath $distribution -Directory -Recurse | Sort-Object { $_.FullName.Length } -Descending)) {
        $relative = $directory.FullName.Substring($distribution.Length+1); $uninstall += 'RMDir "$INSTDIR\' + $relative + '"'
    }
    $uninstall | Set-Content -LiteralPath "artifacts/uninstall-$Architecture.nsh" -Encoding UTF8
    $compiler = Join-Path $repository '.toolchain/nsis-3.12/makensis.exe'
    if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path ${env:ProgramFiles(x86)} 'NSIS/makensis.exe' }
    if (Test-Path -LiteralPath $compiler) {
        & $compiler /V2 "/DARCH=$Architecture" build/installer.nsi
        if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
    }
    else { throw 'NSIS was not found. Install NSIS 3.12 before packaging a release.' }
    $archive = Join-Path $repository "artifacts/PermissionScope-1.0.0-win-$Architecture.zip"
    Compress-Archive -LiteralPath $distribution -DestinationPath $archive -Force
    $packageFiles = @($archive, (Join-Path $repository "artifacts/PermissionScope-1.0.0-$Architecture-Setup.exe")) | Where-Object { Test-Path -LiteralPath $_ }
    Get-FileHash -LiteralPath $packageFiles -Algorithm SHA256 -ErrorAction Stop | ForEach-Object { "$($_.Hash)  $([System.IO.Path]::GetFileName($_.Path))" } | Set-Content -LiteralPath "artifacts/SHA256SUMS-$Architecture.txt" -Encoding ASCII
} finally { Pop-Location }
