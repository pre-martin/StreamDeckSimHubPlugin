// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.PluginTests.SimHub;

public class PropertyParserTests
{
    private PropertyParser _parser;

    [SetUp]
    public void Init()
    {
        _parser = new PropertyParser();
    }

    [Test]
    public void ParseDouble()
    {
        var result1 = _parser.ParseLine("Property dcp.gd.SpeedKmh double (null)");
        Assert.That(result1, Is.Not.Null);
        Assert.That(result1.Value.name, Is.EqualTo("dcp.gd.SpeedKmh"));
        Assert.That(result1.Value.type, Is.EqualTo(PropertyType.Double));
        Assert.That(result1.Value.value, Is.Null);

        var result2 = _parser.ParseLine("Property dcp.gd.SpeedKmh double 18.3334");
        Assert.That(result2, Is.Not.Null);
        Assert.That(result2.Value.name, Is.EqualTo("dcp.gd.SpeedKmh"));
        Assert.That(result2.Value.type, Is.EqualTo(PropertyType.Double));
        Assert.That(result2.Value.value, Is.InRange(18.33, 18.34));
    }

    [Test]
    public void ParseWhitespace()
    {
        var result1 = _parser.ParseLine("Property DataCorePlugin.ExternalScript.BlinkingGear object  ");
        Assert.That(result1, Is.Not.Null);
        Assert.That(result1.Value.name, Is.EqualTo("DataCorePlugin.ExternalScript.BlinkingGear"));
        Assert.That(result1.Value.type, Is.EqualTo(PropertyType.Double));
        Assert.That(result1.Value.value, Is.EqualTo(0));

        var result2 = _parser.ParseLine("Property DataCorePlugin.ExternalScript.BlinkingGear object   ");
        Assert.That(result2, Is.Not.Null);
        Assert.That(result2.Value.name, Is.EqualTo("DataCorePlugin.ExternalScript.BlinkingGear"));
        Assert.That(result2.Value.type, Is.EqualTo(PropertyType.Double));
        Assert.That(result2.Value.value, Is.EqualTo(0));

    }
}
