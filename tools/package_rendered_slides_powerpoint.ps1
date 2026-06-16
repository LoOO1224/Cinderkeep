$ErrorActionPreference = "Stop"

$out = "C:\Users\user\Desktop\Downloads\Cinderkeep_GDP_V1.pptx"
$preview = "C:\Users\user\AppData\Local\Temp\cinderkeep-gdp-v1\preview"
$slideWidth = 1280
$slideHeight = 720

if (Test-Path -LiteralPath $out) {
    Remove-Item -LiteralPath $out -Force
}

$pp = $null
$pres = $null
try {
    $pp = New-Object -ComObject PowerPoint.Application
    $pp.Visible = -1
    $pres = $pp.Presentations.Add()
    $pres.PageSetup.SlideWidth = $slideWidth
    $pres.PageSetup.SlideHeight = $slideHeight

    for ($i = 1; $i -le 16; $i++) {
        $img = Join-Path $preview ("slide-{0:D2}.png" -f $i)
        if (-not (Test-Path -LiteralPath $img)) {
            throw "Missing rendered slide image: $img"
        }
        $slide = $pres.Slides.Add($i, 12)
        $pic = $slide.Shapes.AddPicture($img, $false, $true, 0, 0, $slideWidth, $slideHeight)
        $pic.Name = "Cinderkeep_GDP_V1_Slide_{0:D2}_Rendered" -f $i
    }

    $pres.SaveAs($out, 24)
    $pres.Close()
}
finally {
    if ($pres) {
        try { [System.Runtime.InteropServices.Marshal]::ReleaseComObject($pres) | Out-Null } catch {}
    }
    if ($pp) {
        try { $pp.Quit() } catch {}
        try { [System.Runtime.InteropServices.Marshal]::ReleaseComObject($pp) | Out-Null } catch {}
    }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}

Write-Output "saved $out"
