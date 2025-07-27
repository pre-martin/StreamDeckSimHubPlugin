// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NCalc;
using NCalc.Exceptions;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.PluginTests.PropertyLogic;

public class NCalcHandlerTests
{
    private readonly NCalcHandler _handler = new();

    [Test]
    public void Parse_SingleProperty_ReturnsPropertyName()
    {
        var result = _handler.Parse("[DataCorePlugin.Computed.Fuel_RemainingLaps] <= 2", out var ncalcExpression);
        Assert.That(ncalcExpression, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result, Is.EquivalentTo(new[] { "DataCorePlugin.Computed.Fuel_RemainingLaps" }));
    }

    [Test]
    public void Parse_MultipleProperties_ReturnsAllPropertyNames()
    {
        var result = _handler.Parse("""
            [DataCorePlugin.Computed.Fuel_RemainingLaps] <= 2 and 
            [DataCorePlugin.GameData.RemainingLaps] > 1 and 
            ceiling(sin(3.1415/2)) == 0
            """,
            out var ncalcExpression);
        Assert.That(ncalcExpression, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Is.EquivalentTo(new[]
        {
            "DataCorePlugin.Computed.Fuel_RemainingLaps", "DataCorePlugin.GameData.RemainingLaps"
        }));
    }

    [Test]
    public void Parse_NoProperties_ReturnsEmptySet()
    {
        var result = _handler.Parse("1 + 2", out var ncalcExpression);
        Assert.That(ncalcExpression, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Parse_InvalidExpression_ThrowsException()
    {
        Expression? ncalcExpression = null;
        try
        {
            _handler.Parse("invalid expression", out ncalcExpression);
            Assert.Fail("Expected NCalcParserException was not thrown.");
        }
        catch (NCalcParserException)
        {
            // The "out" variable must not have been set in case of an exception
            Assert.That(ncalcExpression, Is.Null);
        }
    }

    [Test]
    public void Parse_EmptyExpression_DoesNotThrow()
    {
        var result = _handler.Parse(string.Empty, out var ncalcExpression);
        Assert.That(ncalcExpression, Is.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void UpdateNCalcHolder_InvalidExpression()
    {
        var ncalcHolder = new NCalcHolder { UsedProperties = ["testProperty"] };

        var errorMessage = _handler.UpdateNCalcHolder("invalid expression", [], ncalcHolder);

        // We want an error message, an updated expressions string, but no changes to NCalcExpression or UsedProperties.
        Assert.That(errorMessage, Is.Not.Null);
        Assert.That(ncalcHolder.ExpressionString, Is.EqualTo("invalid expression"));
        Assert.That(ncalcHolder.NCalcExpression, Is.Null);
        Assert.That(ncalcHolder.UsedProperties, Has.Count.EqualTo(1));
        Assert.That(ncalcHolder.UsedProperties, Is.EquivalentTo(new[] { "testProperty" }));
    }
}