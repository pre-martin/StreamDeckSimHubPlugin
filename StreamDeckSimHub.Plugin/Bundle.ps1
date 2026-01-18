# Copyright (C) 2024 Martin Renner
# LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)


if ($Args.Count -lt 1) {
    throw 'Arguments are missing'
}

$PublishDir = $Args[0]

Write-Host "`nBundling plugin with streamdeck cli"

try {
    Remove-Item "..\build\*" -Recurse

    Copy-Item "$PublishDir" -Destination "..\build" -Recurse
    Copy-Item -Path ..\Icons -Destination ..\build\publish\images\custom\@core -Recurse -Force
    Pushd ..\build
    Rename-Item -Path "publish" -NewName "net.planetrenner.simhub.sdPlugin" -ErrorAction Stop

    streamdeck bundle net.planetrenner.simhub.sdPlugin
    #streamdeck bundle --ignore-validation net.planetrenner.simhub.sdPlugin
    if ($? -eq $False) {
        Write-Host "`nBundling with streamdeck cli failed`n"
        Exit 1
    }

    Popd
}
catch {
    Write-Host "`nAn error occured while bundling plugin:"
    Write-Host $_
    Exit 1
}
