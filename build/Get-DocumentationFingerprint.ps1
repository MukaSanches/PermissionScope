function Get-DocumentationFingerprint {
    param([Parameter(Mandatory)][string]$Root)
    # Track rendered behavior and dependencies, without invalidating images for test-only or license changes.
    # Native screenshots exercise the WinUI app and its Core/Windows dependencies.
    # CLI-only protocol or benchmark changes do not alter the captured interface.
    $renderedRoots = @(
        (Join-Path $Root 'src/PermissionScope.App'),
        (Join-Path $Root 'src/PermissionScope.Core'),
        (Join-Path $Root 'src/PermissionScope.Windows'),
        (Join-Path $Root 'assets')
    )
    $files = Get-ChildItem -LiteralPath $renderedRoots -Recurse -File |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Name -ne 'packages.lock.json' } | Sort-Object FullName
    $lines = foreach ($file in $files) {
        $relative=[IO.Path]::GetRelativePath($Root,$file.FullName).Replace('\','/')
        if ($file.Extension -in @('.cs','.csproj','.json','.xaml','.manifest')) {
            $bytes=[Text.Encoding]::UTF8.GetBytes([IO.File]::ReadAllText($file.FullName).Replace("`r`n","`n"))
            "$relative $([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)))"
        } else { "$relative $((Get-FileHash $file.FullName -Algorithm SHA256).Hash)" }
    }
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($lines -join "`n")))
}
