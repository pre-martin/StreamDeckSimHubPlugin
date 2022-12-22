// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.PluginTests.AttributeLogic;

public class PropertyComparerEvaluateTests
{
    private PropertyComparer _propertyComparer;
    private readonly IComparable? _propValueTrue = PropertyType.Boolean.ParseFromSimHub("True");
    private readonly IComparable? _propValueFalse = PropertyType.Boolean.ParseFromSimHub("False");

    [SetUp]
    public void Init()
    {
        var logger = Mock.Of<ILogger<PropertyComparer>>();
        _propertyComparer = new PropertyComparer(logger);
    }

    [Test]
    public void TestBooleanWithBoolean()
    {
        var ce = _propertyComparer.Parse("dcp.gd.EngineStarted>=True");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueTrue, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueFalse, ce), Is.False);
    }

    [Test]
    public void TestBooleanWithInteger()
    {
        var ce = _propertyComparer.Parse("dcp.gd.EngineStarted==1");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueTrue, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueFalse, ce), Is.False);

        var ce2 = _propertyComparer.Parse("dcp.gd.EngineStarted==0");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueTrue, ce2), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueFalse, ce2), Is.True);
    }
}