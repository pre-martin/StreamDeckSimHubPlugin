// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SharpDeck.Events.Received;
using StreamDeckSimHub.Plugin.Actions;

namespace StreamDeckSimHub.PluginTests.Actions;

public class ShortAndLongPressHandlerTests
{
    private readonly TimeSpan _timeSpan = TimeSpan.FromMilliseconds(100);
    private readonly TimeSpan _timeSpanShorter = TimeSpan.FromMilliseconds(50);
    private readonly TimeSpan _timeSpanLonger = TimeSpan.FromMilliseconds(150);

    [Test]
    public async Task TestShortPress()
    {
        var cb = new CallbackHolder();
        var handler = new ShortAndLongPressHandler(_timeSpan, cb.OnShortPress, cb.OnLongPress, cb.OnLongReleased);
        await handler.KeyDown(new ActionEventArgs<KeyPayload>());
        await Task.Delay(_timeSpanShorter);
        await handler.KeyUp();

        cb.AssertAndReset(true, false);
    }

    [Test]
    public async Task TestLongPress()
    {
        var cb = new CallbackHolder();
        var handler = new ShortAndLongPressHandler(_timeSpan, cb.OnShortPress, cb.OnLongPress, cb.OnLongReleased);
        await handler.KeyDown(new ActionEventArgs<KeyPayload>());
        await Task.Delay(_timeSpanLonger);
        await handler.KeyUp();

        cb.AssertAndReset(false, true);
    }

    [Test]
    public async Task TestShortShort()
    {
        // Short+Short must not trigger Short+Long! See https://github.com/GeekyEggo/SharpDeck/issues/18 for details.

        var cb = new CallbackHolder();
        var handler = new ShortAndLongPressHandler(_timeSpan, cb.OnShortPress, cb.OnLongPress, cb.OnLongReleased);

        await handler.KeyDown(new ActionEventArgs<KeyPayload>());
        await Task.Delay(_timeSpanShorter);
        await handler.KeyUp();

        cb.AssertAndReset(true, false);

        await handler.KeyDown(new ActionEventArgs<KeyPayload>());
        await Task.Delay(_timeSpanShorter);
        await handler.KeyUp();

        cb.AssertAndReset(true, false);
    }

    [Test]
    public async Task TestLongLong()
    {
        // Long+Long to test that the delay timer works several times

        var cb = new CallbackHolder();
        var handler = new ShortAndLongPressHandler(_timeSpan, cb.OnShortPress, cb.OnLongPress, cb.OnLongReleased);

        await handler.KeyDown(new ActionEventArgs<KeyPayload>());
        await Task.Delay(_timeSpanLonger);
        await handler.KeyUp();

        cb.AssertAndReset(false, true);

        await handler.KeyDown(new ActionEventArgs<KeyPayload>());
        await Task.Delay(_timeSpanLonger);
        await handler.KeyUp();

        cb.AssertAndReset(false, true);
    }

    private class CallbackHolder
    {
        private bool _shortWasCalled;
        private bool _longWasCalled;

        internal void AssertAndReset(bool expectedShort, bool expectedLong)
        {
            Assert.Multiple(() =>
            {
                Assert.That(_shortWasCalled, Is.EqualTo(expectedShort), "Short {0} be called", expectedShort ? "must" : "must not");
                Assert.That(_longWasCalled, Is.EqualTo(expectedLong), "Long {0} be called", expectedLong ? "must" : "must not");
            });
            _shortWasCalled = false;
            _longWasCalled = false;
        }

        internal Task OnShortPress(ActionEventArgs<KeyPayload> args)
        {
            _shortWasCalled = true;
            return Task.CompletedTask;
        }

        internal Task OnLongPress(ActionEventArgs<KeyPayload> args)
        {
            _longWasCalled = true;
            return Task.CompletedTask;
        }

        internal Task OnLongReleased()
        {
            return Task.CompletedTask;
        }
    }
}