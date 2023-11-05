// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.PluginTests.Tools;

public class StateManagerTests
{
    private static readonly ILogger<PropertyComparer> Logger = Mock.Of<ILogger<PropertyComparer>>();
    private PropertyComparer _propertyComparer = new(Logger);

    [SetUp]
    public void Init()
    {
    }

    [Test]
    public async Task TestSubscriptions()
    {
        var simHubMock = new Mock<ISimHubConnection>();
        var stateChangedMock = new Mock<StateManager.StateChangedFunc>();

        var stateManager = new StateManager(_propertyComparer, simHubMock.Object, stateChangedMock.Object);
        // No state so far in StateManager, so we expect a subscription
        await stateManager.HandleExpression("dcp.gd.TCLevel>1", false);
        simHubMock.Verify(shc => shc.Subscribe("dcp.gd.TCLevel", It.IsAny<IPropertyChangedReceiver>()), Times.Exactly(1));

        // Change only on the condition, but not on the property, so nothing should happen
        simHubMock.Invocations.Clear();
        await stateManager.HandleExpression("dcp.gd.TCLevel<1", false);
        simHubMock.Verify(shc => shc.Subscribe(It.IsAny<string>(), It.IsAny<IPropertyChangedReceiver>()), Times.Never);
        simHubMock.Verify(shc => shc.Unsubscribe(It.IsAny<string>(), It.IsAny<IPropertyChangedReceiver>()), Times.Never);

        // New property, so sub/unsub must be called
        simHubMock.Invocations.Clear();
        await stateManager.HandleExpression("dcp.gd.TCCut<1", false);
        simHubMock.Verify(shc => shc.Unsubscribe("dcp.gd.TCLevel", It.IsAny<IPropertyChangedReceiver>()), Times.Exactly(1));
        simHubMock.Verify(shc => shc.Subscribe("dcp.gd.TCCut", It.IsAny<IPropertyChangedReceiver>()), Times.Exactly(1));

        // Finish -> unsubscribe
        simHubMock.Invocations.Clear();
        stateManager.Deactivate();
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

        var lastState = -1;
        var stateManager = new StateManager(_propertyComparer, simHubMock.Object, StateChangedFunc);

        // Setup
        await stateManager.HandleExpression("dcp.gd.ABSLevel>0", false);
        Debug.Assert(propChangedReceiver != null, nameof(propChangedReceiver) + " != null");

        // Inject new property value "1"
        await propChangedReceiver.PropertyChanged(new PropertyChangedArgs("dcp.gd.ABSLevel", PropertyType.Integer, 1));
        Assert.That(lastState, Is.EqualTo(1));

        // Inject new property value "0"
        await propChangedReceiver.PropertyChanged(new PropertyChangedArgs("dcp.gd.ABSLevel", PropertyType.Integer, 0));
        Assert.That(lastState, Is.EqualTo(0));

        // Change condition so that the state has to be recalculated
        await stateManager.HandleExpression("dcp.gd.ABSLevel==0", false);
        // Property value "0" from above should now have changed the state from "0" to "1".
        Assert.That(lastState, Is.EqualTo(1));
        return;

        Task StateChangedFunc(int state)
        {
            lastState = state;
            return Task.CompletedTask;
        }
    }
}
