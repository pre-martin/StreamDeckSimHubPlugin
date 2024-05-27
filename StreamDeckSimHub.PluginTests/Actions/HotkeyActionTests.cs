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

        // If "EngineStarted" is an "Integer" property, the old logic was:
        // - PropertyValue from SimHub > 0: True
        // - else: False
        var propValueOne = PropertyType.Integer.Parse("1");
        var propValueTwo = PropertyType.Integer.Parse("2");
        var propValueZero = PropertyType.Integer.Parse("0");
        var propValueMinusOne = PropertyType.Integer.Parse("-1");
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

        // If "EngineStarted" is a "Boolean" property, the old logic was:
        // - PropertyValue from SimHub = "True": > True
        // - else: False
        var propValueTrue = PropertyType.Boolean.Parse("True");
        var propValueFalse = PropertyType.Boolean.Parse("False");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, propValueTrue, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, propValueFalse, ce), Is.False);
    }
}