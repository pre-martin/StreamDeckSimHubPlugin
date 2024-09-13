# Copyright (C) 2024 Martin Renner
# LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)


if ($Args.Count -lt 1) {
    throw 'Arguments are missing'
}

$PublishDir = $Args[0]

Remove-Item "..\build\*" -Recurse

Copy-Item "$PublishDir" -Destination "..\build" -Recurse
Pushd ..\build
Rename-Item -Path "publish" -NewName "net.planetrenner.simhub.sdPlugin"

..\..\DistributionTool.exe -b -i net.planetrenner.simhub.sdPlugin -o .

Popd
