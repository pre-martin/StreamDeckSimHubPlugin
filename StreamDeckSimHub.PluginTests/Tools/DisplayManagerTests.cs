// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Diagnostics;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.PluginTests.Tools;

public class DisplayManagerTests
{
    [Test]
    public async Task TestSubscriptions()
    {
        var simHubMock = new Mock<ISimHubConnection>();
        var displayChangedMock = new Mock<DisplayManager.DisplayChangedFunc>();

        var displayManager = new DisplayManager(simHubMock.Object, displayChangedMock.Object);
        // No state so far in DisplayManager, so we expect a subscription
        await displayManager.HandleDisplayProperties("dcp.gd.TCLevel", "", false);
        simHubMock.Verify(shc => shc.Subscribe("dcp.gd.TCLevel", It.IsAny<IPropertyChangedReceiver>()), Times.Exactly(1));

        // Change only on the format, but not on the property, so nothing should happen
        simHubMock.Invocations.Clear();
        await displayManager.HandleDisplayProperties("dcp.gd.TCLevel", ":D", false);
        simHubMock.Verify(shc => shc.Subscribe(It.IsAny<string>(), It.IsAny<IPropertyChangedReceiver>()), Times.Never);
        simHubMock.Verify(shc => shc.Unsubscribe(It.IsAny<string>(), It.IsAny<IPropertyChangedReceiver>()), Times.Never);

        // New property, so sub/unsub must be called
        simHubMock.Invocations.Clear();
        await displayManager.HandleDisplayProperties("dcp.gd.TCCut", ":D", false);
        simHubMock.Verify(shc => shc.Unsubscribe("dcp.gd.TCLevel", It.IsAny<IPropertyChangedReceiver>()), Times.Exactly(1));
        simHubMock.Verify(shc => shc.Subscribe("dcp.gd.TCCut", It.IsAny<IPropertyChangedReceiver>()), Times.Exactly(1));

        // Finish -> unsubscribe
        simHubMock.Invocations.Clear();
        displayManager.Deactivate();
        simHubMock.Verify(shc => shc.Unsubscribe("dcp.gd.TCCut", It.IsAny<IPropertyChangedReceiver>()), Times.Exactly(1));
    }

    [Test]
    public async Task TestEvents()
    {
        var simHubMock = new Mock<ISimHubConnection>();
        // Intercept IPropertyChangedReceiver so that we can trigger new values from "SimHubConnection"
        IPropertyChangedReceiver? propChangedReceiver = null;
        simHubMock.Setup(shc => shc.Subscribe(It.IsAny<string>(), It.IsAny<IPropertyChangedReceiver>()))
            .Callback((string _, IPropertyChangedReceiver r) => propChangedReceiver = r);

        IComparable? lastValue = null;
        var lastFormat = "";
        var displayManager = new DisplayManager(simHubMock.Object, DisplayChangedFunc);

        // Setup
        await displayManager.HandleDisplayProperties("dcp.gd.ABSLevel", "", false);
        Debug.Assert(propChangedReceiver != null, nameof(propChangedReceiver) + " != null");

        // Inject new property value "1"
        await propChangedReceiver.PropertyChanged(new PropertyChangedArgs("dcp.gd.ABSLevel", PropertyType.Integer, 1));
        Assert.That(lastValue, Is.EqualTo(1));
        Assert.That(lastFormat, Is.EqualTo("{0}"));

        // Inject new property value "8"
        await propChangedReceiver.PropertyChanged(new PropertyChangedArgs("dcp.gd.ABSLevel", PropertyType.Integer, 8));
        Assert.That(lastValue, Is.EqualTo(8));
        Assert.That(lastFormat, Is.EqualTo("{0}"));

        // Change format so that the display value has to be updated
        await displayManager.HandleDisplayProperties("dcp.gd.ABSLevel", ":D2", false);
        Assert.That(lastValue, Is.EqualTo(8));
        Assert.That(lastFormat, Is.EqualTo("{0:D2}"));

        return;

        Task DisplayChangedFunc(IComparable? value, string format)
        {
            lastValue = value;
            lastFormat = format;
            return Task.CompletedTask;
        }
    }

    [Test]
    public async Task TestWithoutSimHub()
    {
        var simHubMock = new Mock<ISimHubConnection>();

        IComparable? lastValue = null;
        var lastFormat = "";
        var displayManager = new DisplayManager(simHubMock.Object, DisplayChangedFunc);

        // Setup
        await displayManager.HandleDisplayProperties("acc.graphics.WiperLV", "Wiper {:D}", false);

        // Subscribe must have been called on the SimHubConnection
        simHubMock.Verify(shc => shc.Subscribe("acc.graphics.WiperLV", It.IsAny<IPropertyChangedReceiver>()), Times.Exactly(1));
        // And the DisplayManager must have fired a fake property value
        Assert.That(lastFormat, Is.EqualTo("Wiper {0:D}"));
        Assert.That(lastValue, Is.Null);


        return;

        Task DisplayChangedFunc(IComparable? value, string format)
        {
            lastValue = value;
            lastFormat = format;
            return Task.CompletedTask;
        }
    }
}