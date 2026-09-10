$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$assetDirectory = Join-Path $PSScriptRoot '../assets'
function Draw-Mark([int]$size) {
    $bitmap = New-Object System.Drawing.Bitmap $size,$size
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.ColorTranslator]::FromHtml('#17315C'))
    $pen = New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#83A9FC')), ([single]($size*3/64))
    $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
    $points = @(@(19,49),@(19,15),@(46,15),@(46,33),@(19,33))
    for ($i=0; $i -lt 4; $i++) { $g.DrawLine($pen,[single]($points[$i][0]*$size/64),[single]($points[$i][1]*$size/64),[single]($points[$i+1][0]*$size/64),[single]($points[$i+1][1]*$size/64)) }
    foreach ($point in $points) { $g.FillEllipse($brush,[single](($point[0]-4)*$size/64),[single](($point[1]-4)*$size/64),[single]($size*8/64),[single]($size*8/64)) }
    $pen.Dispose(); $brush.Dispose(); $g.Dispose()
    return $bitmap
}
$images = @()
foreach ($size in @(16,32,48,64,128,256)) {
    $bitmap = Draw-Mark $size
    $memory = New-Object System.IO.MemoryStream
    $bitmap.Save($memory,[System.Drawing.Imaging.ImageFormat]::Png)
    $images += ,@{Size=$size;Bytes=$memory.ToArray()}
    $memory.Dispose(); $bitmap.Dispose()
}
$file = [System.IO.File]::Create((Join-Path $assetDirectory 'PermissionScope.ico'))
$writer = New-Object System.IO.BinaryWriter $file
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$images.Count)
    $offset = 6+16*$images.Count
    foreach ($image in $images) {
        $dimension = if ($image.Size -eq 256) {0} else {$image.Size}
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension); $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$image.Bytes.Length); $writer.Write([uint32]$offset)
        $offset += $image.Bytes.Length
    }
    foreach ($image in $images) { $writer.Write([byte[]]$image.Bytes) }
} finally { $writer.Dispose(); $file.Dispose() }
foreach ($entry in @(@('Square44x44Logo.png',44),@('Square150x150Logo.png',150),@('StoreLogo.png',50))) {
    $bitmap = Draw-Mark $entry[1]
    try { $bitmap.Save((Join-Path $assetDirectory $entry[0]),[System.Drawing.Imaging.ImageFormat]::Png) } finally { $bitmap.Dispose() }
}
