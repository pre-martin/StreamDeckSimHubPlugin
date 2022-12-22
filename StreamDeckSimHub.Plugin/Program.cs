// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SharpDeck.Extensions.Hosting;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;

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
    serviceCollection.AddSingleton<PropertyComparer>();
}