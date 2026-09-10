$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$repository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$destination = Join-Path $repository 'artifacts/PermissionScope-1.0.0-source.zip'
New-Item -ItemType Directory -Path (Join-Path $repository 'artifacts') -Force | Out-Null
$stream = [System.IO.File]::Open($destination,[System.IO.FileMode]::Create)
$zip = New-Object System.IO.Compression.ZipArchive $stream,([System.IO.Compression.ZipArchiveMode]::Create)
Push-Location $repository
try {
    $files = & rg --files --hidden -g '!.toolchain/**' -g '!artifacts/**' -g '!**/bin/**' -g '!**/obj/**' -g '!.git/**'
    if ($LASTEXITCODE -ne 0) { throw 'Source enumeration failed.' }
    foreach ($file in $files) {
        [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,(Join-Path $repository $file),$file.Replace('\','/'),[System.IO.Compression.CompressionLevel]::Optimal)
    }
    "Packaged $($files.Count) source files."
} finally { $zip.Dispose(); $stream.Dispose(); Pop-Location }
