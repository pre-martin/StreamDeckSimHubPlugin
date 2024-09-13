// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SharpDeck.Extensions.Hosting;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

// Main entry. Started by Stream Deck with appropriate arguments.

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

host.Services.GetRequiredService<SimHubConnection>().Run();

host.Run();


void ConfigureServices(IServiceCollection serviceCollection)
{
    serviceCollection.AddSingleton<PropertyParser>();
    serviceCollection.AddSingleton<SimHubConnection>();
    serviceCollection.AddSingleton<ShakeItStructureFetcher>();
    serviceCollection.AddSingleton<PropertyComparer>();
    serviceCollection.AddSingleton<ImageUtils>();
    serviceCollection.AddSingleton<ImageManager>();
    serviceCollection.AddSingleton<IFileSystem>(new FileSystem());
}
