// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Hosting;
using NLog;

namespace StreamDeckSimHub.Plugin.Tools;

/// <summary>
/// This background service triggers a 'Tick' event every 100 milliseconds.
/// </summary>
public class PeriodicBackgroundService : BackgroundService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public delegate Task AsyncEventHandler();

    public static event AsyncEventHandler? Tick;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            // Fire & forget
            _ = Task.Factory.StartNew(async state =>
            {
                try
                {
                    if (Tick is not null) await Tick();
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Exception while calling 'tick' event handler");
                }
            }, null, stoppingToken);
        }
    }
}