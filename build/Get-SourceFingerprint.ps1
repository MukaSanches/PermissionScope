function Get-SourceFingerprint {
    param([Parameter(Mandatory)][string]$Root)
    $files = @(
        foreach ($directory in @('src','tests','assets')) {
            Get-ChildItem -LiteralPath (Join-Path $Root $directory) -Recurse -File |
                Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
        }
        foreach ($name in @('Directory.Build.props','global.json','PermissionScope.sln')) {
            Get-Item -LiteralPath (Join-Path $Root $name)
        }
    ) | Sort-Object FullName
    $lines = foreach ($file in $files) {
        $relative = [IO.Path]::GetRelativePath($Root, $file.FullName).Replace('\','/')
        if ($file.Name -eq 'packages.lock.json') {
            # Restore adds architecture-specific targets; compare the shared dependency graph.
            $lockData = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
            $shared = foreach ($target in $lockData.dependencies.PSObject.Properties | Sort-Object Name) {
                if ($target.Name.Contains('/')) { continue }
                foreach ($package in $target.Value.PSObject.Properties | Sort-Object Name) {
                    "$($target.Name)/$($package.Name):$($package.Value | ConvertTo-Json -Depth 30 -Compress)"
                }
            }
            "$relative $($shared -join '|')"
        } elseif ($file.Extension -in @('.cs','.csproj','.json','.xaml','.props','.sln','.manifest','.md','.xml')) {
            $normalized = [IO.File]::ReadAllText($file.FullName).Replace("`r`n","`n")
            $digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($normalized)))
            "$relative $digest"
        } else {
            "$relative $((Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash)"
        }
    }
    $bytes = [Text.Encoding]::UTF8.GetBytes(($lines -join "`n"))
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
}
