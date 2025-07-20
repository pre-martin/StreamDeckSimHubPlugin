// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin;

/// <summary>
/// Entry point into the application.
/// </summary>
public partial class App
{
    private readonly IHost _host;

    public App()
    {
        _host = Program.CreateHost();
        NLog.LogManager.GetCurrentClassLogger().Info("Starting StreamDeckSimHub plugin {version}", ThisAssembly.AssemblyFileVersion);
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            ((SimHubConnection)_host.Services.GetRequiredService<ISimHubConnection>()).Run();
            WeakReferenceMessenger.Default.RegisterAll(this);
            await _host.StartAsync();
        }
        catch (Exception ex)
        {
            NLog.LogManager.GetCurrentClassLogger().Error(ex, "Error during application startup");
        }
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }
    }
}