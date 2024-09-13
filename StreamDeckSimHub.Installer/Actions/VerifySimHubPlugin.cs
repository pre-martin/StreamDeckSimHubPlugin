using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using StreamDeckSimHub.Installer.Tools;

namespace StreamDeckSimHub.Installer.Actions;

public class VerifySimHubPlugin : AbstractInstallerAction
{
    public override string Name => "Verify SimHub installation and SimHub Property Server plugin";

    protected override Task<ActionResult> ExecuteInternal()
    {
        var installFolder = GetSimHubInstallFolder();
        if (!File.Exists(Path.Combine(installFolder, "SimHubWPF.exe")))
        {
            SetAndLogInfo("Could not find SimHub installation. SimHub is required for this plugin.");
            return Task.FromResult(ActionResult.Warning);
        }

        if (!File.Exists(Path.Combine(installFolder, "PropertyServer.dll")))
        {
            SetAndLogInfo($"Could not find SimHub Property Server plugin. Is is required for this plugin. Please install it from {Configuration.SimHubPluginUrl}");
            return Task.FromResult(ActionResult.Warning);
        }

        var pluginVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(installFolder, "PropertyServer.dll"));
        Version pluginVersion = new(pluginVersionInfo.ProductMajorPart, pluginVersionInfo.ProductMinorPart, pluginVersionInfo.ProductBuildPart);
        if (pluginVersion < Configuration.RequiredSimHubPluginVersion)
        {
            SetAndLogInfo($"SimHub Property Server plugin is too old. Found version {pluginVersion}, required version is {Configuration.RequiredSimHubPluginVersion}. Please update the SimHub Property Server plugin by visiting {Configuration.SimHubPluginUrl}");
            return Task.FromResult(ActionResult.Warning);
        }

        SetAndLogInfo("Found SimHub and the SimHub Property Server plugin.");
        return Task.FromResult(ActionResult.Success);
    }

    private string GetSimHubInstallFolder()
    {
        var installPath = (string?) Registry.GetValue(Configuration.SimHubRegistryFolder, Configuration.SimHubRegistryInstallFolder, null);
        if (!string.IsNullOrEmpty(installPath))
        {
            SetAndLogInfo($"Found SimHub directory in registry: {installPath}");
            return installPath;
        }

        SetAndLogInfo($"Could not find SimHub directory in registry. Using default.");
        return Configuration.StreamDeckDefaultInstallFolder;
    }

}
