// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.Concurrent;

namespace StreamDeckSimHub.Plugin.Tools;

/// <summary>
/// Arguments that can be passed to the handler methods OnShortPress, OnLongPress and OnLongReleased.
/// </summary>
public interface IHandlerArgs;

/// <summary>
/// Handles the detection of short and long (key/button) presses.
/// <p/>
/// If the time between KeyDown and KeyUp is shorter than "longPressTimeSpan", the callback "OnShortPress" will be called.
/// If the time is larger, the callback "OnLongPress" will be called.
/// </summary>
public class ShortAndLongPressHandler(
    TimeSpan longPressTimeSpan,
    Func<IHandlerArgs?, Task> onShortPress,
    Func<IHandlerArgs?, Task> onLongPress,
    Func<IHandlerArgs?, Task> onLongReleased)
{
    private ConcurrentStack<IHandlerArgs?> KeyPressStack { get; } = new();
    private IHandlerArgs? _handlerArgs;
    public TimeSpan LongPressTimeSpan { get; set; } = longPressTimeSpan;
    private Func<IHandlerArgs?, Task> OnShortPress { get; } = onShortPress;
    private Func<IHandlerArgs?, Task> OnLongPress { get; } = onLongPress;
    private Func<IHandlerArgs?, Task> OnLongReleased { get; } = onLongReleased;
    private CancellationTokenSource _cancellationTokenSource = new();

    public ShortAndLongPressHandler(
        Func<IHandlerArgs?, Task> onShortPress,
        Func<IHandlerArgs?, Task> onLongPress,
        Func<IHandlerArgs?, Task> onLongReleased) : this(TimeSpan.FromMilliseconds(500), onShortPress, onLongPress, onLongReleased)
    {
    }

    public Task KeyDown(IHandlerArgs? args = null)
    {
        KeyPressStack.Push(args);
        _handlerArgs = args;
        if (LongPressTimeSpan > TimeSpan.Zero)
        {
            Task.Run(async () =>
            {
                try
                {
                    var me = this;
                    _cancellationTokenSource = new CancellationTokenSource();
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

    private async Task TryHandlePress(Func<IHandlerArgs?, Task> handler)
    {
        // we reach this code either:
        // - when "KeyUp" was received before the end of the "Delay" (= short key press)
        // - when the "Delay" has expired (= start of long key press).
        // - when "KeyUp" was received after the Delay (= end of long key press)
        if (KeyPressStack.TryPop(out _))
        {
            await _cancellationTokenSource.CancelAsync();
            await handler(_handlerArgs);
        }
        else
        {
            // we cannot use "out var handlerArgs" from above, because if it is a long press, the stack is already empty.
            await OnLongReleased(_handlerArgs);
        }
    }
}
