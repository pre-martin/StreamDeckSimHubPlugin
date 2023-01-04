// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.PluginTests.Actions;

public class HotkeyActionTests
{
    private PropertyComparer _propertyComparer;

    [SetUp]
    public void Init()
    {
        var logger = Mock.Of<ILogger<PropertyComparer>>();
        _propertyComparer = new PropertyComparer(logger);
    }

    [Test]
    public void TestOldComparisonInteger()
    {
        var ce = _propertyComparer.Parse("dcp.gd.EngineStarted");
        Assert.That(ce.Operator, Is.EqualTo(ConditionOperator.Gt));
        Assert.That(ce.CompareValue, Is.EqualTo("0"));

        // If "EngineStarted" would be an "Integer" property, the old logic was:
        // - PropertyValue from SimHub > 0: True
        // - else: False
        // We simulate also that we have received the values from SimHub. This ensures the old logic is still valid.
        var propValueOne = PropertyType.Integer.ParseFromSimHub("1");
        var propValueTwo = PropertyType.Integer.ParseFromSimHub("2");
        var propValueZero = PropertyType.Integer.ParseFromSimHub("0");
        var propValueMinusOne = PropertyType.Integer.ParseFromSimHub("-1");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, propValueOne, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, propValueTwo, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, propValueZero, ce), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, propValueMinusOne, ce), Is.False);
    }

    [Test]
    public void TestOldComparisonBoolean()
    {
        var ce = _propertyComparer.Parse("dcp.gd.EngineStarted");
        Assert.That(ce.Operator, Is.EqualTo(ConditionOperator.Gt));
        Assert.That(ce.CompareValue, Is.EqualTo("0"));

        // If "EngineStarted" would be a "Boolean" property, the old logic was:
        // - PropertyValue from SimHub = "True": > True
        // - else: False
        // We simulate also that we have received the values from SimHub. This ensures the old logic is still valid.
        var propValueTrue = PropertyType.Boolean.ParseFromSimHub("True");
        var propValueFalse = PropertyType.Boolean.ParseFromSimHub("False");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, propValueTrue, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, propValueFalse, ce), Is.False);
    }
}