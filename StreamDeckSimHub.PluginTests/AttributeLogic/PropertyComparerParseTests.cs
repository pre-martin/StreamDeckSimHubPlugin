// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.PluginTests.AttributeLogic;

public class PropertyComparerParseTests
{
    private PropertyComparer _propertyComparer;

    [SetUp]
    public void Init()
    {
        var logger = Mock.Of<ILogger<PropertyComparer>>();
        _propertyComparer = new PropertyComparer(logger);
    }

    [Test]
    public void TestParseEquals()
    {
        var ce = _propertyComparer.Parse("property.name==5");
        Assert.That(ce.Property, Is.EqualTo("property.name"));
        Assert.That(ce.Operator, Is.EqualTo(ConditionOperator.Eq));
        Assert.That(ce.CompareValue, Is.EqualTo("5"));

        var ce2 = _propertyComparer.Parse("  property.name ==  5 ");
        Assert.That(ce2.Property, Is.EqualTo("property.name"));
        Assert.That(ce2.Operator, Is.EqualTo(ConditionOperator.Eq));
        Assert.That(ce2.CompareValue, Is.EqualTo("5"));
    }

    [Test]
    public void TestParseGreaterEquals()
    {
        var ce = _propertyComparer.Parse("property.name>=5");
        Assert.That(ce.Property, Is.EqualTo("property.name"));
        Assert.That(ce.Operator, Is.EqualTo(ConditionOperator.Ge));
        Assert.That(ce.CompareValue, Is.EqualTo("5"));

        var ce2 = _propertyComparer.Parse("  property.name >=  5 ");
        Assert.That(ce2.Property, Is.EqualTo("property.name"));
        Assert.That(ce2.Operator, Is.EqualTo(ConditionOperator.Ge));
        Assert.That(ce2.CompareValue, Is.EqualTo("5"));
    }

    [Test]
    public void TestParseNotEquals()
    {
        var ce = _propertyComparer.Parse("property.name!=1");
        Assert.That(ce.Property, Is.EqualTo("property.name"));
        Assert.That(ce.Operator, Is.EqualTo(ConditionOperator.Ne));
        Assert.That(ce.CompareValue, Is.EqualTo("1"));

        var ce2 = _propertyComparer.Parse("  property.name !=  0 ");
        Assert.That(ce2.Property, Is.EqualTo("property.name"));
        Assert.That(ce2.Operator, Is.EqualTo(ConditionOperator.Ne));
        Assert.That(ce2.CompareValue, Is.EqualTo("0"));
    }

    [Test]
    public void TestParseBetween()
    {
        var ce = _propertyComparer.Parse("some.property~~1;5");
        Assert.That(ce.Property, Is.EqualTo("some.property"));
        Assert.That(ce.Operator, Is.EqualTo(ConditionOperator.Between));
        Assert.That(ce.CompareValue, Is.EqualTo("1;5"));
    }
}