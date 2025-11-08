// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using StreamDeckSimHub.Installer.Tools;

namespace StreamDeckSimHub.Installer.Actions
{
    /// <summary>
    /// Starts the Stream Deck software.
    /// </summary>
    public class StartStreamDeckSoftware : AbstractInstallerAction
    {
        public override string Name => "Starting Stream Deck software";

        protected override Task<ActionResult> ExecuteInternal()
        {
            var installFolder = GetStreamDeckInstallFolder();
            var streamDeckExePath = Path.Combine(installFolder, "StreamDeck.exe");
            if (!File.Exists(streamDeckExePath))
            {
                SetAndLogError($"File not found: {streamDeckExePath}. Cannot start Stream Deck software.");
                return Task.FromResult(ActionResult.Warning);
            }
            ProcessTools.StartProcess(streamDeckExePath, installFolder);

            SetAndLogInfo("Stream Deck software started");
            return Task.FromResult(ActionResult.Success);
        }

        private string GetStreamDeckInstallFolder()
        {
            var installPath = (string)Registry.GetValue(Configuration.StreamDeckRegistryFolder, Configuration.StreamDeckRegistryInstallFolder, null);
            if (!string.IsNullOrEmpty(installPath))
            {
                SetAndLogInfo($"Found Stream Deck directory in registry: {installPath}");
                return installPath;
            }

            SetAndLogInfo($"Could not find Stream Deck directory in registry. Using default: {Configuration.SimHubDefaultInstallFolder}");
            return Configuration.StreamDeckDefaultInstallFolder;
        }
    }
}