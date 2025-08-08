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
    public void Parse_InvalidStrFunction_ThrowsException()
    {
        Expression? ncalcExpression = null;
        try
        {
            _handler.Parse("str()", out ncalcExpression);
            Assert.Fail("Expected NCalcParserException was not thrown.");
        }
        catch (NCalcParserException ex)
        {
            // The "out" variable must not have been set in case of an exception
            Assert.That(ncalcExpression, Is.Null);
            Assert.That(ex.Message, Does.Contain("parsing the expression"));
            Assert.That(ex.InnerException, Is.Not.Null);
            Assert.That(ex.InnerException.Message, Does.Contain("one argument"));
        }
    }

    [Test]
    public void UpdateNCalcHolder_InvalidExpression()
    {
        var ncalcHolder = new NCalcHolder { UsedProperties = ["testProperty"] };

        var errorMessage = _handler.UpdateNCalcHolder("invalid expression", ncalcHolder);

        // We want an error message, an updated expressions string, but no changes to NCalcExpression or UsedProperties.
        Assert.That(errorMessage, Is.Not.Null);
        Assert.That(ncalcHolder.ExpressionString, Is.EqualTo("invalid expression"));
        Assert.That(ncalcHolder.NCalcExpression, Is.Null);
        Assert.That(ncalcHolder.UsedProperties, Has.Count.EqualTo(1));
        Assert.That(ncalcHolder.UsedProperties, Is.EquivalentTo(new[] { "testProperty" }));
    }

    [Test]
    public void Evaluate_StrFunction()
    {
        var ncalcHolder = new NCalcHolder();
        var errorMsg = _handler.UpdateNCalcHolder("str(123) + 'x'", ncalcHolder);
        Assert.That(errorMsg, Is.Null);

        var result = _handler.EvaluateExpression(ncalcHolder, _ => 1, "TestContext");
        Assert.That(result, Is.EqualTo("123x"));
    }

    [Test]
    public void CleanupShakeItDictionary_RemovesUnusedEntries()
    {
        var ncalcHolder = new NCalcHolder();
        ncalcHolder.ShakeItDictionary["sib.guid1"] = "ABS";
        _handler.UpdateNCalcHolder("[sib.guid1.IsMuted]", ncalcHolder);

        var result1 = _handler.CleanupShakeItDictionary(ncalcHolder);
        Assert.That(result1, Is.False);
        Assert.That(ncalcHolder.ShakeItDictionary.Count, Is.EqualTo(1));

        _handler.UpdateNCalcHolder("1 + 2", ncalcHolder);
        var result2 = _handler.CleanupShakeItDictionary(ncalcHolder);
        Assert.That(result2, Is.True);
        Assert.That(ncalcHolder.ShakeItDictionary.Count, Is.EqualTo(0));
    }
}