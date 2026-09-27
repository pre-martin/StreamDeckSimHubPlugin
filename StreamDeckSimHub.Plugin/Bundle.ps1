# Copyright (C) 2026 Martin Renner
# LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)


if ($Args.Count -lt 1) {
    throw 'Arguments are missing'
}

$PublishDir = $Args[0]
$BuildDir = "..\build"
$BundleSourceDir = "$BuildDir\publish"

Write-Host "`nBundling plugin"

try {
    if (Test-Path -LiteralPath $BuildDir) {
        Write-Host "  - Removing old build files"
        $buildEntries = Get-ChildItem -Path "$BuildDir\*" -ErrorAction SilentlyContinue
        if ($null -ne $buildEntries) {
            Remove-Item "$BuildDir\*" -Recurse -Force
        }
    }

    if (-not (Test-Path -LiteralPath $BuildDir)) {
        New-Item -ItemType Directory -Path $BuildDir | Out-Null
    }

    New-Item -ItemType Directory -Path $BundleSourceDir -Force | Out-Null
    Write-Host "  - Copying publish files to build directory"
    Copy-Item -Path "$PublishDir*" -Destination $BundleSourceDir -Recurse -Force
    Write-Host "  - Copying @core icons to build directory"
    Copy-Item -Path ..\Icons -Destination "$BundleSourceDir\images\custom\@core" -Recurse -Force

    $pdbFiles = Get-ChildItem -Path "$BundleSourceDir\*.pdb" -File -Recurse -ErrorAction SilentlyContinue
    if ($null -ne $pdbFiles) {
        Write-Host "  - Removing .pdb files from bundle"
        Remove-Item -Path $pdbFiles.FullName -Force
    }

    Pushd ..\build
    Rename-Item -Path "publish" -NewName "net.planetrenner.simhub.sdPlugin" -ErrorAction Stop

    # Prepare for Stream Deck with Stream Deck CLI
    Write-Host "  - Bundling plugin for Stream Deck"
    Copy-Item "net.planetrenner.simhub.sdPlugin\manifest-streamdeck.json" -Destination "net.planetrenner.simhub.sdPlugin\manifest.json"

    streamdeck bundle net.planetrenner.simhub.sdPlugin
    if ($? -eq $False) {
        Write-Host "`nBundling with Stream Deck CLI failed`n"
        Exit 1
    }

    # Prepare for Stream Dock
    Write-Host "  - Bundling plugin for Stream Dock"
    Copy-Item "net.planetrenner.simhub.sdPlugin\manifest-streamdock.json" -Destination "net.planetrenner.simhub.sdPlugin\manifest.json"
    Compress-Archive -Path "net.planetrenner.simhub.sdPlugin\*" -DestinationPath "net.planetrenner.simhub-streamdock.zip" -Force

    Popd
}
catch {
    Write-Host "`nAn error occured while bundling plugin:"
    Write-Host $_
    Exit 1
}
