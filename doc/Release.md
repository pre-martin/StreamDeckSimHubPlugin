# Release Process

1. Create and push a tag like `v1.2` (see [manifest.json](net.planetrenner.simhub.sdPlugin/manifest.json) for the current version).
2. Build the native plugin (see [the native part of the plugin](PluginNative/README.md)).
3. Use the "DistributionTool" from Stream Deck to create the installable archive:
   ```
   DistributionTool.exe -b -i net.planetrenner.simhub.sdPlugin -o ..\Release
   ```
4. Create a release in GitHub from the tag and attach the archive.
5. Increment the version in "manifest.json".
