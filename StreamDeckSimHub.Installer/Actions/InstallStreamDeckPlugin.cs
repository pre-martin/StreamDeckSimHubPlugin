// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using StreamDeckSimHub.Installer.Tools;

namespace StreamDeckSimHub.Installer.Actions
{
    public class InstallStreamDeckPlugin : AbstractInstallerAction
    {
        public override string Name => "Installing Stream Deck SimHub Plugin";

        protected override Task<ActionResult> ExecuteInternal()
        {
            if (Directory.Exists(Path.Combine(Configuration.StreamDeckPluginDir, Configuration.PluginDirName)))
            {
                var pluginDir = Path.Combine(Configuration.StreamDeckPluginDir, Configuration.PluginDirName);
                SetAndLogInfo($"Deleting existing Stream Deck SimHub Plugin from {pluginDir}");
                var result = DeleteExistingInstallation(pluginDir);
                if (!result)
                {
                    return Task.FromResult(ActionResult.Error);
                }
            }

            SetAndLogInfo($"Installing Stream Deck SimHub Plugin to {Configuration.StreamDeckPluginDir}");
            if (!ExtractPlugin(Configuration.StreamDeckPluginDir))
            {
                return Task.FromResult(ActionResult.Error);
            }

            SetAndLogInfo("Successfully installed Stream Deck SimHub Plugin");
            return Task.FromResult(ActionResult.Success);
        }

        private bool DeleteExistingInstallation(string pluginDir)
        {
            try
            {
                // Delete all files in the base directory
                var baseDirInfo = new DirectoryInfo(pluginDir);
                foreach (var fileInfo in baseDirInfo.EnumerateFiles())
                {
                    fileInfo.Delete();
                }

                // Delete all directories recursive in the base directory - except "images"
                foreach (var dirInfo in baseDirInfo.EnumerateDirectories().Where(dirInfo => dirInfo.Name != "images"))
                {
                    dirInfo.Delete(true);
                }

                // Delete all directories recursive in the "images" directory - except "custom"
                var imagesDirInfo = new DirectoryInfo(Path.Combine(pluginDir, "images"));
                foreach (var dirInfo in imagesDirInfo.EnumerateDirectories().Where(dirInfo => dirInfo.Name != "custom"))
                {
                    dirInfo.Delete(true);
                }

                return true;
            }
            catch (Exception e)
            {
                SetAndLogError(e, $"Could not delete existing installation: {e.Message}");
                return false;
            }
        }

        private bool ExtractPlugin(string streamDeckPluginDir)
        {
            var myAssembly = typeof(App).Assembly;
            var resourcePaths = myAssembly.GetManifestResourceNames().Where(res => res.EndsWith(Configuration.PluginZipName)).ToList();
            if (resourcePaths.Count == 1)
            {
                using (var pluginStream = myAssembly.GetManifestResourceStream(resourcePaths[0]))
                {
                    if (pluginStream == null)
                    {
                        SetAndLogError("Could not find embedded Stream Deck SimHub Plugin (stream is null)");
                        return false;
                    }

                    var archive = new ZipArchive(pluginStream, ZipArchiveMode.Read);
                    foreach (var entry in archive.Entries)
                    {
                        var fullName = Path.Combine(streamDeckPluginDir, entry.FullName);
                        var directory = Path.GetDirectoryName(fullName);
                        if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
                        if (entry.Name != "") entry.ExtractToFile(fullName, true);
                    }
                }

                return true;
            }

            SetAndLogError($"Could not find embedded Stream Deck SimHub Plugin ({resourcePaths.Count} streams)");
            return false;
        }
    }
}