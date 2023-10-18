// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.Concurrent;
using SharpDeck.Events.Received;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Handles the detection of short and long (key/button) presses.
/// <p/>
/// If the time between KeyDown and KeyUp is shorter than "longPressTimeSpan", the callback "OnShortPress" will be called.
/// If the time is larger, the callback "OnLongPress" will be called.
/// </summary>
public class ShortAndLongPressHandler
{
    private ConcurrentStack<ActionEventArgs<KeyPayload>> KeyPressStack { get; } = new();
    private TimeSpan LongPressTimeSpan { get; }
    private Func<ActionEventArgs<KeyPayload>, Task> OnShortPress { get; }
    private Func<ActionEventArgs<KeyPayload>, Task> OnLongPress { get; }
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ShortAndLongPressHandler(
        Func<ActionEventArgs<KeyPayload>, Task> onShortPress,
        Func<ActionEventArgs<KeyPayload>, Task> onLongPress) : this(TimeSpan.FromMilliseconds(500), onShortPress, onLongPress)
    {
    }

    public ShortAndLongPressHandler(
        TimeSpan longPressTimeSpan,
        Func<ActionEventArgs<KeyPayload>, Task> onShortPress,
        Func<ActionEventArgs<KeyPayload>, Task> onLongPress)
    {
        LongPressTimeSpan = longPressTimeSpan;
        OnShortPress = onShortPress;
        OnLongPress = onLongPress;
    }

    public Task KeyDown(ActionEventArgs<KeyPayload> args)
    {
        KeyPressStack.Push(args);
        if (LongPressTimeSpan > TimeSpan.Zero)
        {
            Task.Run(async () =>
            {
                try
                {
                    var me = this;
                    await Task.Delay(LongPressTimeSpan, _cancellationTokenSource.Token);
                    await me.TryHandlePress(OnLongPress);
                }
                catch (TaskCanceledException)
                {
                    // That is what we expect if a short press was faster
                }
            });
        }

        return Task.CompletedTask;
    }

    public async Task KeyUp()
    {
        await TryHandlePress(OnShortPress);
    }

    private async Task TryHandlePress(Func<ActionEventArgs<KeyPayload>, Task> handler)
    {
        if (KeyPressStack.TryPop(out var result))
        {
            _cancellationTokenSource.Cancel();
            await handler(result);
        }
    }
}
