// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.Concurrent;
using NLog;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Tools;

public class KeyQueueEntry
{
    internal Hotkey? Hotkey { get; init; }
    internal string? SimHubControl { get; init; }
    internal (string owner, string? role)? SimHubRole { get; init; }
}

/// <summary>
/// Manages a queue of keystrokes and SimHubControls. Entries can be placed in the queue, where they are processed by a thread.
/// The thread "presses" and "releases" each entry in order.
/// <p/>
/// This class is especially useful for Dial actions, where Down/Up or Press/Release events have to be sent for each Dial step.
/// </summary>
public class PressAndReleaseQueue(ISimHubConnection simHubConnection)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SimHubManager _simHubManager = new(simHubConnection);
    private readonly KeyboardUtils _keyboardUtils = new();
    private readonly ConcurrentQueue<KeyQueueEntry> _hotkeyQueue = new();
    private Thread? _workerThread;
    private readonly AutoResetEvent _waitHandle = new(false);

    public void Start()
    {
        _workerThread = new Thread(WatchQueue)
        {
            IsBackground = true
        };
        _workerThread.Start();
    }

    public async Task Stop()
    {
        _workerThread?.Interrupt();
        await _simHubManager.Deactivate();
    }

    internal void Enqueue(Hotkey? hotkey, string? simHubControl, (string owner, string? role)? simHubRole,
        int count)
    {
        Logger.Debug($"Adding {Math.Abs(count)} entries to queue");
        for (var i = 0; i < Math.Abs(count); i++)
        {
            _hotkeyQueue.Enqueue(new KeyQueueEntry { Hotkey = hotkey, SimHubControl = simHubControl, SimHubRole = simHubRole });
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
                    PressAndRelease(queueEntry.Hotkey, queueEntry.SimHubControl, queueEntry.SimHubRole).Wait();
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

    private async Task PressAndRelease(
        Hotkey? hotkey, string? simHubControl, (string owner, string? role)? simHubRole)
    {
        _keyboardUtils.KeyDown(hotkey);
        await _simHubManager.TriggerInputPressed(simHubControl);
        if (simHubRole != null) await _simHubManager.RolePressed(simHubRole.Value.owner, simHubRole.Value.role);

        await Task.Delay(TimeSpan.FromMilliseconds(20));

        _keyboardUtils.KeyUp(hotkey);
        await _simHubManager.TriggerInputReleased(simHubControl);
        if (simHubRole != null) await _simHubManager.RoleReleased(simHubRole.Value.owner, simHubRole.Value.role);
    }
}