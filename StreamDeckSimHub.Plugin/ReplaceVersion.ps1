# Copyright (C) 2022 Martin Renner
# LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

#
# Reads a given JSON file and replaces the value of the attribute "Version" with the value given as argument.
#
# Can be used to replace the version in manifest.json.
#


if ($Args.Count -lt 2) {
    throw 'Arguments are missing'
    Exit 1
}

$ManifestFile = $Args[0]
$Version = $Args[1]

$manifest = (Get-Content($ManifestFile) | ConvertFrom-Json)
$manifest.Version = $Version
$manifest | ConvertTo-Json -depth 100 | Out-File $ManifestFile
