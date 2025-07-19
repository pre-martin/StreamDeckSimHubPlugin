// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.PluginTests.Tools;

public class ShortAndLongPressHandlerTests
{
    private readonly TimeSpan _timeSpan = TimeSpan.FromMilliseconds(100);
    private readonly TimeSpan _timeSpanShorter = TimeSpan.FromMilliseconds(50);
    private readonly TimeSpan _timeSpanLonger = TimeSpan.FromMilliseconds(150);

    [Test]
    public async Task TestShortPress()
    {
        var handlerArgs = new HandlerArgs();
        var cb = new CallbackHolder();
        var handler = new ShortAndLongPressHandler(_timeSpan, cb.OnShortPress, cb.OnLongPress, cb.OnLongReleased);
        await handler.KeyDown(handlerArgs);
        await Task.Delay(_timeSpanShorter);
        await handler.KeyUp();

        cb.AssertAndReset(true, false, handlerArgs);
    }

    [Test]
    public async Task TestLongPress()
    {
        var handlerArgs = new HandlerArgs();
        var cb = new CallbackHolder();
        var handler = new ShortAndLongPressHandler(_timeSpan, cb.OnShortPress, cb.OnLongPress, cb.OnLongReleased);
        await handler.KeyDown(handlerArgs);
        await Task.Delay(_timeSpanLonger);
        await handler.KeyUp();

        cb.AssertAndReset(false, true, handlerArgs);
    }

    [Test]
    public async Task TestShortShort()
    {
        // Short+Short must not trigger Short+Long! See https://github.com/GeekyEggo/SharpDeck/issues/18 for details.

        var firstHandlerArgs = new HandlerArgs();
        var cb = new CallbackHolder();
        var handler = new ShortAndLongPressHandler(_timeSpan, cb.OnShortPress, cb.OnLongPress, cb.OnLongReleased);

        await handler.KeyDown(firstHandlerArgs);
        await Task.Delay(_timeSpanShorter);
        await handler.KeyUp();

        cb.AssertAndReset(true, false, firstHandlerArgs);

        var secondHandlerArgs = new HandlerArgs();
        await handler.KeyDown(secondHandlerArgs);
        await Task.Delay(_timeSpanShorter);
        await handler.KeyUp();

        cb.AssertAndReset(true, false, secondHandlerArgs);
    }

    [Test]
    public async Task TestLongLong()
    {
        // Long+Long to test that the delay timer works several times

        var firstHandlerArgs = new HandlerArgs();
        var cb = new CallbackHolder();
        var handler = new ShortAndLongPressHandler(_timeSpan, cb.OnShortPress, cb.OnLongPress, cb.OnLongReleased);

        await handler.KeyDown(firstHandlerArgs);
        await Task.Delay(_timeSpanLonger);
        await handler.KeyUp();

        cb.AssertAndReset(false, true, firstHandlerArgs);

        var secondHandlerArgs = new HandlerArgs();
        await handler.KeyDown(secondHandlerArgs);
        await Task.Delay(_timeSpanLonger);
        await handler.KeyUp();

        cb.AssertAndReset(false, true, secondHandlerArgs);
    }

    private class CallbackHolder
    {
        private bool _shortWasCalled;
        private bool _longWasCalled;
        private HandlerArgs? _receivedArgs;

        internal void AssertAndReset(bool expectedShort, bool expectedLong, IHandlerArgs? expectedHandlerArgs)
        {
            Assert.Multiple(() =>
            {
                Assert.That(_shortWasCalled, Is.EqualTo(expectedShort), $"Short {(expectedShort ? "must" : "must not")} be called");
                Assert.That(_longWasCalled, Is.EqualTo(expectedLong), $"Long {(expectedLong ? "must" : "must not")} be called");
                Assert.That(_receivedArgs, Is.EqualTo(expectedHandlerArgs), "Received args must match the original args");
            });
            _shortWasCalled = false;
            _longWasCalled = false;
        }

        internal Task OnShortPress(IHandlerArgs? args)
        {
            _receivedArgs = args as HandlerArgs;
            _shortWasCalled = true;
            return Task.CompletedTask;
        }

        internal Task OnLongPress(IHandlerArgs? args)
        {
            _receivedArgs = args as HandlerArgs;
            _longWasCalled = true;
            return Task.CompletedTask;
        }

        internal Task OnLongReleased(IHandlerArgs? args)
        {
            _receivedArgs = args as HandlerArgs;
            return Task.CompletedTask;
        }
    }

    private class HandlerArgs : IHandlerArgs;
}