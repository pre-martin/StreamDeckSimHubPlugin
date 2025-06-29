// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NCalc.Exceptions;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.PluginTests.PropertyLogic;

public class NCalcHandlerTests
{
    private readonly NCalcHandler _handler = new();

    [Test]
    public void ExtractProperties_SingleProperty_ReturnsPropertyName()
    {
        var result = _handler.ExtractProperties("[DataCorePlugin.Computed.Fuel_RemainingLaps] <= 2");
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result, Is.EquivalentTo(new[] { "DataCorePlugin.Computed.Fuel_RemainingLaps" }));
    }

    [Test]
    public void ExtractProperties_MultipleProperties_ReturnsAllPropertyNames()
    {
        var result = _handler.ExtractProperties("""
            [DataCorePlugin.Computed.Fuel_RemainingLaps] <= 2 and 
            [DataCorePlugin.GameData.RemainingLaps] > 1 and 
            ceiling(sin(3.1415/2)) == 0
            """);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Is.EquivalentTo(new[]
        {
            "DataCorePlugin.Computed.Fuel_RemainingLaps", "DataCorePlugin.GameData.RemainingLaps"
        }));
    }

    [Test]
    public void ExtractProperties_NoProperties_ReturnsEmptySet()
    {
        var result = _handler.ExtractProperties("1 + 2");
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ExtractProperties_InvalidExpression_ThrowsException()
    {
        Assert.Throws<NCalcParserException>(() => _handler.ExtractProperties("invalid expression"));
    }
}