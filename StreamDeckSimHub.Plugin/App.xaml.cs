// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using StreamDeckSimHub.Plugin.ActionEditor;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;
using StreamDeckSimHub.Plugin.Tools.AutoUpdate;

namespace StreamDeckSimHub.Plugin;

/// <summary>
/// Entry point into the application.
/// </summary>
public partial class App
{
    private readonly IHost _host;

    public App()
    {
        var localDevMode = Environment.GetCommandLineArgs().Length == 2 && Environment.GetCommandLineArgs()[1] == "dev";
        _host = Program.CreateHost(localDevMode);
        LogManager.GetCurrentClassLogger().Info("Starting StreamDeckSimHub plugin {version}", ThisAssembly.AssemblyFileVersion);
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            var simHubConnection = _host.Services.GetRequiredService<ISimHubConnection>();
            if (simHubConnection is SimHubConnection shc)
            {
                shc.Run();
            }
            await _host.StartAsync();
        }
        catch (Exception ex)
        {
            LogManager.GetCurrentClassLogger().Error(ex, "Error during application startup");
        }


        var localDevMode = Environment.GetCommandLineArgs().Length == 2 && Environment.GetCommandLineArgs()[1] == "dev";

        if (localDevMode)
        {
            UpdateStatus.LatestVersion = new Version("99.99.1");
        }
        else
        {
            // Run version check in the background. We use it only in the GenericButtonEditor, which is not yet opened.
            _ = GetLatestVersionAsync();
        }

        // Developer mode: Open a Generic Button Editor directly for testing
        if (localDevMode)
        {
            var settings = new Settings
            {
                KeySize = StreamDeckKeyInfoBuilder.DefaultKeyInfo.KeySize
            };
            settings.SettingsChanged += (sender, e) =>
            {
                Console.WriteLine($"sender: {sender} / {e.PropertyName}");
            };

            var actionEditorManager = _host.Services.GetService<ActionEditorManager>();
            var window = actionEditorManager!.ShowGenericButtonEditor("someUuid", settings);
            window.Closed += (_, _) =>
            {
                actionEditorManager.RemoveGenericButtonEditor("someUuid");
                Current.Shutdown();
            };
        }
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }
    }

    private async Task GetLatestVersionAsync()
    {
        try
        {
            var updater = _host.Services.GetRequiredService<AutoUpdater>();
            var versionInfo = await updater.GetLatestVersion();
            UpdateStatus.LatestVersion = new Version(versionInfo.TagName);
            UpdateStatus.LatestVersionException = null;
        }
        catch (Exception ex)
        {
            LogManager.GetCurrentClassLogger().Error($"Error checking for new version: {ex.Message}");
            UpdateStatus.LatestVersion = null;
            UpdateStatus.LatestVersionException = ex;
        }
    }
}