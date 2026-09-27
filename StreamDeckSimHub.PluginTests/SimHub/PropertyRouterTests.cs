// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.PluginTests.SimHub;

public class PropertyRouterTests
{
    private PropertyRouter _router;
    private Mock<ISimHubConnection> _simHubConnection;
    private BuiltInPropertyManager _builtInPropertyManager;
    private IPropertyChangedReceiver _receiver;

    [SetUp]
    public void Init()
    {
        _simHubConnection = new Mock<ISimHubConnection>();
        _builtInPropertyManager = new BuiltInPropertyManager();
        _router = new PropertyRouter(_simHubConnection.Object, _builtInPropertyManager);
        _receiver = Mock.Of<IPropertyChangedReceiver>();
    }

    [Test]
    public async Task TestSubscribeBuiltInRoutesToBuiltInManager()
    {
        await _router.Subscribe(BuiltInProperties.ConnectionConnected, _receiver);

        _simHubConnection.Verify(c => c.Subscribe(It.IsAny<string>(), It.IsAny<IPropertyChangedReceiver>()), Times.Never);
    }

    [Test]
    public async Task TestSubscribeSimHubRoutesToSimHubConnection()
    {
        await _router.Subscribe("DataCorePlugin.GameData.SpeedKmh", _receiver);

        _simHubConnection.Verify(c => c.Subscribe("DataCorePlugin.GameData.SpeedKmh", _receiver), Times.Once);
    }

    [Test]
    public async Task TestUnsubscribeBuiltInRoutesToBuiltInManager()
    {
        await _router.Subscribe(BuiltInProperties.ConnectionConnected, _receiver);
        await _router.Unsubscribe(BuiltInProperties.ConnectionConnected, _receiver);

        _simHubConnection.Verify(c => c.Unsubscribe(It.IsAny<string>(), It.IsAny<IPropertyChangedReceiver>()), Times.Never);
    }

    [Test]
    public async Task TestUnsubscribeSimHubRoutesToSimHubConnection()
    {
        await _router.Unsubscribe("DataCorePlugin.GameData.SpeedKmh", _receiver);

        _simHubConnection.Verify(c => c.Unsubscribe("DataCorePlugin.GameData.SpeedKmh", _receiver), Times.Once);
    }

    [Test]
    public async Task TestGetPropertyBuiltInRoutesToBuiltInManager()
    {
        await _builtInPropertyManager.SetProperty(BuiltInProperties.ConnectionConnected, true);

        var result = _router.GetProperty(BuiltInProperties.ConnectionConnected);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.PropertyValue, Is.EqualTo(true));
        _simHubConnection.Verify(c => c.GetProperty(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void TestGetPropertySimHubRoutesToSimHubConnection()
    {
        _router.GetProperty("DataCorePlugin.GameData.SpeedKmh");

        _simHubConnection.Verify(c => c.GetProperty("DataCorePlugin.GameData.SpeedKmh"), Times.Once);
    }

    [Test]
    public async Task TestPrefixIsCaseInsensitive()
    {
        await _router.Subscribe("streamdecksimhub.Connection.Connected", _receiver);

        _simHubConnection.Verify(c => c.Subscribe(It.IsAny<string>(), It.IsAny<IPropertyChangedReceiver>()), Times.Never);
    }
}
