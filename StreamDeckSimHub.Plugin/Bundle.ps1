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

    # Prepare for Stream Deck with Stream Deck CLI
    Copy-Item "net.planetrenner.simhub.sdPlugin\manifest-streamdeck.json" -Destination "net.planetrenner.simhub.sdPlugin\manifest.json"

    streamdeck bundle net.planetrenner.simhub.sdPlugin
    if ($? -eq $False) {
        Write-Host "`nBundling with Stream Deck CLI failed`n"
        Exit 1
    }

    # Prepare for Stream Dock
    Copy-Item "net.planetrenner.simhub.sdPlugin\manifest-streamdock.json" -Destination "net.planetrenner.simhub.sdPlugin\manifest.json"
    Compress-Archive -Path "net.planetrenner.simhub.sdPlugin\*" -DestinationPath "net.planetrenner.simhub-streamdock.zip" -Force

    Popd
}
catch {
    Write-Host "`nAn error occured while bundling plugin:"
    Write-Host $_
    Exit 1
}
