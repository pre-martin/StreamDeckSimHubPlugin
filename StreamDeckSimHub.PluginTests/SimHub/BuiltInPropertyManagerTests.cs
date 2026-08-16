// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.PluginTests.SimHub;

public class BuiltInPropertyManagerTests
{
    private BuiltInPropertyManager _manager;

    [SetUp]
    public void Init()
    {
        _manager = new BuiltInPropertyManager();
    }

    [Test]
    public async Task TestSubscribeReceivesCurrentValueImmediately()
    {
        await _manager.SetProperty(BuiltInProperties.ConnectionConnected, true);

        var received = new List<PropertyChangedArgs>();
        var receiver = new PropertyChangedDelegate(args => { received.Add(args); return Task.CompletedTask; });

        await _manager.Subscribe(BuiltInProperties.ConnectionConnected, receiver);

        Assert.That(received, Has.Count.EqualTo(1));
        Assert.That(received[0].PropertyValue, Is.EqualTo(true));
    }

    [Test]
    public async Task TestSubscribeReceivesNothingWhenNoValueSet()
    {
        var received = new List<PropertyChangedArgs>();
        var receiver = new PropertyChangedDelegate(args => { received.Add(args); return Task.CompletedTask; });

        await _manager.Subscribe(BuiltInProperties.ConnectionConnected, receiver);

        Assert.That(received, Is.Empty);
    }

    [Test]
    public async Task TestSetPropertyNotifiesSubscriber()
    {
        var received = new List<PropertyChangedArgs>();
        var receiver = new PropertyChangedDelegate(args => { received.Add(args); return Task.CompletedTask; });
        await _manager.Subscribe(BuiltInProperties.ConnectionConnected, receiver);

        await _manager.SetProperty(BuiltInProperties.ConnectionConnected, false);
        await _manager.SetProperty(BuiltInProperties.ConnectionConnected, true);

        Assert.That(received, Has.Count.EqualTo(2));
        Assert.That(received[0].PropertyValue, Is.EqualTo(false));
        Assert.That(received[1].PropertyValue, Is.EqualTo(true));
    }

    [Test]
    public async Task TestSetPropertySameValueDoesNotNotify()
    {
        await _manager.SetProperty(BuiltInProperties.ConnectionConnected, true);

        var received = new List<PropertyChangedArgs>();
        var receiver = new PropertyChangedDelegate(args => { received.Add(args); return Task.CompletedTask; });
        await _manager.Subscribe(BuiltInProperties.ConnectionConnected, receiver);
        received.Clear(); // ignore the immediate delivery on subscribe

        await _manager.SetProperty(BuiltInProperties.ConnectionConnected, true);

        Assert.That(received, Is.Empty);
    }

    [Test]
    public async Task TestUnsubscribeStopsNotifications()
    {
        var received = new List<PropertyChangedArgs>();
        var receiver = new PropertyChangedDelegate(args => { received.Add(args); return Task.CompletedTask; });
        await _manager.Subscribe(BuiltInProperties.ConnectionConnected, receiver);

        await _manager.Unsubscribe(BuiltInProperties.ConnectionConnected, receiver);
        await _manager.SetProperty(BuiltInProperties.ConnectionConnected, true);

        Assert.That(received, Is.Empty);
    }

    [Test]
    public async Task TestGetPropertyReturnsCurrentValue()
    {
        Assert.That(_manager.GetProperty(BuiltInProperties.ConnectionConnected), Is.Null);

        await _manager.SetProperty(BuiltInProperties.ConnectionConnected, true);

        var result = _manager.GetProperty(BuiltInProperties.ConnectionConnected);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.PropertyValue, Is.EqualTo(true));
    }
}
