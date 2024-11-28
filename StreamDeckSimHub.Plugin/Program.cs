// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SharpDeck.Extensions.Hosting;
using StreamDeckSimHub.Plugin.ActionEditor;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin;

/// <summary>
/// Before the inclusion of WPF, this was the entry point into the plugin. Now it is just a static helper class
/// to initialize the Hosting environment.
/// </summary>
public abstract class Program
{
    public static IHost CreateHost()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging((context, loggingBuilder) =>
            {
                loggingBuilder
                    .ClearProviders()
                    .AddNLog();
            })
            .UseStreamDeck()
            .ConfigureServices(ConfigureServices)
            .Build();
        return host;
    }

    static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
    {
        serviceCollection.Configure<ConnectionSettings>(context.Configuration.GetSection("SimHubConnection"));
        serviceCollection.AddSingleton<PropertyParser>();
        serviceCollection.AddSingleton<SimHubConnection>();
        serviceCollection.AddSingleton<ShakeItStructureFetcher>();
        serviceCollection.AddSingleton<PropertyComparer>();
        serviceCollection.AddSingleton<ImageUtils>();
        serviceCollection.AddSingleton<ImageManager>();
        serviceCollection.AddSingleton<IFileSystem>(new FileSystem());
        serviceCollection.AddHostedService<PeriodicBackgroundService>();
        serviceCollection.AddSingleton<ActionEditorManager>();
    }
}