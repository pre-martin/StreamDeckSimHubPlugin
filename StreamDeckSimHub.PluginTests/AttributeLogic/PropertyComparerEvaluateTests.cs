﻿// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.PluginTests.AttributeLogic;

public class PropertyComparerEvaluateTests
{
    private PropertyComparer _propertyComparer;
    private readonly IComparable? _propValueTrue = PropertyType.Boolean.Parse("True");
    private readonly IComparable? _propValueFalse = PropertyType.Boolean.Parse("False");
    private readonly IComparable? _propValueZero = PropertyType.Integer.Parse("0");
    private readonly IComparable? _propValueOne = PropertyType.Integer.Parse("1");
    private readonly IComparable? _propValueTwo = PropertyType.Integer.Parse("2");
    private readonly IComparable? _propValueThree = PropertyType.Integer.Parse("3");
    private readonly IComparable? _propValueLongZero = PropertyType.Long.Parse("0");
    private readonly IComparable? _propValueLongOne = PropertyType.Long.Parse("1");
    private readonly IComparable? _propValueLongTwo = PropertyType.Long.Parse("2");

    [SetUp]
    public void Init()
    {
        var logger = Mock.Of<ILogger<PropertyComparer>>();
        _propertyComparer = new PropertyComparer(logger);
    }

    [Test]
    public void BooleanPropWithBooleanValue()
    {
        var ceUpper = _propertyComparer.Parse("dcp.gd.IsLapValid>=TRUE");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueTrue, ceUpper), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueFalse, ceUpper), Is.False);

        var ceLower = _propertyComparer.Parse("dcp.gd.IsLapValid>=true");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueTrue, ceLower), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueFalse, ceLower), Is.False);

        var ceMixed = _propertyComparer.Parse("dcp.gd.IsLapValid!=FaLSe");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueTrue, ceMixed), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueFalse, ceMixed), Is.False);
    }

    [Test]
    public void BooleanPropWithIntegerValue()
    {
        var ce = _propertyComparer.Parse("dcp.gd.IsLapValid==1");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueTrue, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueFalse, ce), Is.False);

        var ce2 = _propertyComparer.Parse("dcp.gd.IsLapValid==2");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueTrue, ce2), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueFalse, ce2), Is.False);

        var ce3 = _propertyComparer.Parse("dcp.gd.IsLapValid==0");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueTrue, ce3), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Boolean, _propValueFalse, ce3), Is.True);
    }

    [Test]
    public void IntegerPropWithBooleanValue()
    {
        var ce = _propertyComparer.Parse("dcp.gd.EngineStarted==TruE");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueOne, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueTwo, ce), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueZero, ce), Is.False);

        var ce2 = _propertyComparer.Parse("dcp.gd.EngineStarted==false");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueOne, ce2), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueZero, ce2), Is.True);
    }

    [Test]
    public void IntegerPropWithIntegerValue()
    {
        var ce = _propertyComparer.Parse("dcp.gd.EngineStarted==1");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueOne, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueZero, ce), Is.False);

        var ce2 = _propertyComparer.Parse("dcp.gd.EngineStarted>1");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueOne, ce2), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueZero, ce2), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueTwo, ce2), Is.True);
    }

    [Test]
    public void LongPropWithBooleanValue()
    {
        var ce = _propertyComparer.Parse("some.property==true");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongOne, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongTwo, ce), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongZero, ce), Is.False);

        var ce2 = _propertyComparer.Parse("some.property==false");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongOne, ce2), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongZero, ce2), Is.True);
    }

    [Test]
    public void LongPropWithIntegerValue()
    {
        var ce = _propertyComparer.Parse("some.property==1");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongOne, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongZero, ce), Is.False);

        var ce2 = _propertyComparer.Parse("some.property>=2");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongOne, ce2), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongZero, ce2), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Long, _propValueLongTwo, ce2), Is.True);
    }

    [Test]
    public void DoublePropWithDoubleValue()
    {
        var ce = _propertyComparer.Parse("acc.graphics.fuelEstimatedLaps>=3.5");
        var propValue5Dot9 = PropertyType.Double.Parse("5.9");
        var propValue3Dot4 = PropertyType.Double.Parse("3.4");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Double, propValue5Dot9, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Double, propValue3Dot4, ce), Is.False);
    }

    [Test]
    public void IntegerPropBetweenIntegerValues()
    {
        var ce = _propertyComparer.Parse("acc.graphics.WiperLV~~1;2");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueZero, ce), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueOne, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueTwo, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueThree, ce), Is.False);
    }

    [Test]
    public void IntegerPropInvalidBetween()
    {
        var ce = _propertyComparer.Parse("acc.graphics.WiperLV~~1");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueZero, ce), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueOne, ce), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueTwo, ce), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Integer, _propValueThree, ce), Is.False);
    }

    [Test]
    public void ObjectProp()
    {
        var ce = _propertyComparer.Parse("DataCorePlugin.GameData.Gear>=3");
        var propValue2 = PropertyType.Object.Parse("2");
        var propValue3 = PropertyType.Object.Parse("3");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Object, propValue2, ce), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Object, propValue3, ce), Is.True);

        var propValueN = PropertyType.Object.Parse("N");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Object, propValueN, ce), Is.False);
    }

    [Test]
    public void ObjectPropBetween()
    {
        var ce = _propertyComparer.Parse("DataCorePlugin.GameData.Gear~~3;4");
        var propValue2 = PropertyType.Object.Parse("2");
        var propValue3 = PropertyType.Object.Parse("3");
        var propValue5 = PropertyType.Object.Parse("5");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Object, propValue2, ce), Is.False);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Object, propValue3, ce), Is.True);
        Assert.That(_propertyComparer.Evaluate(PropertyType.Object, propValue5, ce), Is.False);

        var propValueN = PropertyType.Object.Parse("N");
        Assert.That(_propertyComparer.Evaluate(PropertyType.Object, propValueN, ce), Is.False);
    }
}