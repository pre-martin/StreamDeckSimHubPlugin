// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.Concurrent;
using NLog;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Tools;

public class KeyQueueEntry
{
    internal KeyboardUtils.Hotkey? Hotkey { get; init; }
    internal string? SimHubControl { get; init; }
}

/// <summary>
/// Manages a queue of keystrokes and SimHubControls. Entries can be placed in the queue, where they are processed by a thread.
/// </summary>
public class KeyQueue
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SimHubConnection _simHubConnection;
    private readonly ConcurrentQueue<KeyQueueEntry> _hotkeyQueue = new();
    private Thread? _workerThread;
    private readonly AutoResetEvent _waitHandle = new(false);

    public KeyQueue(SimHubConnection simHubConnection)
    {
        _simHubConnection = simHubConnection;
    }

    public void Start()
    {
        _workerThread = new Thread(WatchQueue)
        {
            IsBackground = true
        };
        _workerThread.Start();
    }

    public void Stop()
    {
        _workerThread?.Interrupt();
    }

    internal void Enqueue(KeyboardUtils.Hotkey? hotkey, string? simHubControl, int count)
    {
        Logger.Debug("Adding entries to queue");
        for (var i = 0; i < count; i++)
        {
            _hotkeyQueue.Enqueue(new KeyQueueEntry { Hotkey = hotkey, SimHubControl = simHubControl});
        }
        _waitHandle.Set();
    }

    private void WatchQueue()
    {
        try
        {
            while (true)
            {
                if (_hotkeyQueue.TryDequeue(out var queueEntry))
                {
                    Press(queueEntry.Hotkey, queueEntry.SimHubControl).Wait();
                }
                else
                {
                    _waitHandle.WaitOne();
                }
            }
        }
        catch (ThreadInterruptedException)
        {
            // We are expecting this exception, so no handling.
            Logger.Debug("Exiting thread");
        }
    }

    private async Task Press(KeyboardUtils.Hotkey? hotkey, string? simHubControl)
    {
        KeyboardUtils.KeyDown(hotkey);
        if (!string.IsNullOrWhiteSpace(simHubControl))
        {
            await _simHubConnection.SendTriggerInputPressed(simHubControl);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(20));

        KeyboardUtils.KeyUp(hotkey);
        if (!string.IsNullOrWhiteSpace(simHubControl))
        {
            await _simHubConnection.SendTriggerInputReleased(simHubControl);
        }
    }

}