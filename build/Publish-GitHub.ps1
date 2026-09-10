$ErrorActionPreference = 'Stop'
$repository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$gh = Join-Path $repository '.toolchain/gh/bin/gh.exe'
if (-not (Test-Path -LiteralPath $gh)) { $gh = 'gh' }
function Invoke-GitHub {
    & $gh @args
    if ($LASTEXITCODE -ne 0) { throw 'GitHub operation failed. No credentials are included in this diagnostic.' }
}
Push-Location $repository
try {
    & (Join-Path $PSScriptRoot 'Verify-Release.ps1')
    Invoke-GitHub auth status
    $account = Invoke-GitHub api user --jq .login
    if ($account -ne 'MukaSanches') { throw 'Expected the MukaSanches account. Stop and verify the intended publisher.' }
    if (git status --porcelain) { throw 'Review and commit all source changes before publishing.' }
    $repo = "$account/PermissionScope"
    & $gh repo view $repo --json name *> $null
    if ($LASTEXITCODE -ne 0) { Invoke-GitHub repo create $repo --public --description 'See who has access. Understand why. Free, local Windows permission analysis.' }
    $expectedRemote = "https://github.com/$repo.git"
    $remote = git remote get-url origin 2>$null
    if ($LASTEXITCODE -ne 0) { git remote add origin $expectedRemote }
    elseif ($remote -ne $expectedRemote) { throw 'Existing origin does not match the intended official repository.' }
    git push -u origin main
    if ($LASTEXITCODE -ne 0) { throw 'Source push failed.' }
    Invoke-GitHub repo edit $repo --enable-issues --enable-discussions --delete-branch-on-merge --add-topic windows --add-topic winui3 --add-topic ntfs --add-topic permissions --homepage "https://$account.github.io/PermissionScope/"
    Invoke-GitHub api --method PUT "repos/$repo/vulnerability-alerts"
    Invoke-GitHub api --method PUT "repos/$repo/private-vulnerability-reporting"
    $version = 'v1.0.0'
    & $gh release view $version --repo $repo *> $null
    if ($LASTEXITCODE -eq 0) { throw 'Release already exists. Existing release artifacts are immutable; do not overwrite them.' }
    $releaseManifest = Get-Content artifacts/release-manifest.json -Raw | ConvertFrom-Json
    $names = @($releaseManifest.files.name) + @('SHA256SUMS.txt','release-manifest.json')
    $assets = foreach ($name in $names) {
        if ($name -notmatch '^[A-Za-z0-9._-]+$') { throw 'Invalid release manifest filename.' }
        Get-Item -LiteralPath (Join-Path $repository "artifacts/$name")
    }
    if ($assets.Count -lt 6) { throw 'Expected release artifacts are missing.' }
    if (-not (git tag --list $version)) { git tag -a $version -m 'PermissionScope 1.0.0' }
    git push origin $version
    if ($LASTEXITCODE -ne 0) { throw 'Tag push failed.' }
    Invoke-GitHub release create $version --repo $repo --verify-tag --draft --title 'PermissionScope 1.0.0' --notes-file CHANGELOG.md @($assets.FullName)
    & $gh api "repos/$repo/pages" *> $null
    if ($LASTEXITCODE -ne 0) { Invoke-GitHub api --method POST "repos/$repo/pages" -f build_type=workflow }
    Invoke-GitHub workflow run pages.yml --repo $repo
    Invoke-GitHub release view $version --repo $repo --json url,tagName
    Invoke-GitHub run list --repo $repo --limit 6 --json name,status,conclusion,url
} finally { Pop-Location }
