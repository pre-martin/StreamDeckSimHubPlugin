// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using Microsoft.Win32;
using StreamDeckSimHub.Installer.Tools;

namespace StreamDeckSimHub.Installer.Actions;

/// <summary>
/// Starts the Stream Deck software.
/// </summary>
public class StartStreamDeckSoftware : AbstractInstallerAction
{
    public override string Name => "Starting Stream Deck software";

    protected override Task<ActionResult> ExecuteInternal()
    {
        var installDir = GetStreamDeckInstallationPath();
        ProcessTools.StartProcess(Path.Combine(installDir, "StreamDeck.exe"), installDir);

        SetAndLogInfo("Stream Deck software started");
        return Task.FromResult(ActionResult.Success);
    }

    private string GetStreamDeckInstallationPath()
    {
        var installPath = (string?) Registry.GetValue(Configuration.StreamDeckRegistryFolder, "InstallDir", null);
        if (!string.IsNullOrEmpty(installPath))
        {
            SetAndLogInfo($"Found Stream Deck directory in registry: {installPath}");
            return installPath;
        }

        SetAndLogInfo($"Could not find Stream Deck directory in registry. Using default.");
        return Path.Combine("C:", "Program Files", "Elgato", "StreamDeck");
    }
}
