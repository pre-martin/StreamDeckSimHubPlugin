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
    public void ParseTimeSpan()
    {
        var result1 = _parser.ParseLine("Property dcp.gd.SessionTimeLeft timespan (null)");
        Assert.That(result1, Is.Not.Null);
        Assert.That(result1.Value.name, Is.EqualTo("dcp.gd.SessionTimeLeft"));
        Assert.That(result1.Value.type, Is.EqualTo(PropertyType.TimeSpan));
        Assert.That(result1.Value.value, Is.Null);


        var result2 = _parser.ParseLine("Property dcp.gd.SessionTimeLeft timespan 00:04:10.8570000");
        Assert.That(result2, Is.Not.Null);
        Assert.That(result2.Value.name, Is.EqualTo("dcp.gd.SessionTimeLeft"));
        Assert.That(result2.Value.type, Is.EqualTo(PropertyType.TimeSpan));
        Assert.That(result2.Value.value, Is.EqualTo(TimeSpan.FromSeconds(4 * 60 + 10) + TimeSpan.FromMilliseconds(857)));
    }

    [Test]
    public void ParseString()
    {
        var result1 = _parser.ParseLine("Property dcp.gd.Gear string (null)");
        Assert.That(result1, Is.Not.Null);
        Assert.That(result1.Value.name, Is.EqualTo("dcp.gd.Gear"));
        Assert.That(result1.Value.type, Is.EqualTo(PropertyType.String));
        Assert.That(result1.Value.value, Is.Null);

        var result2 = _parser.ParseLine("Property dcp.gd.Gear string N");
        Assert.That(result2, Is.Not.Null);
        Assert.That(result2.Value.name, Is.EqualTo("dcp.gd.Gear"));
        Assert.That(result2.Value.type, Is.EqualTo(PropertyType.String));
        Assert.That(result2.Value.value, Is.EqualTo("N"));
    }

    [Test]
    public void ParseWhitespace()
    {
        var result1 = _parser.ParseLine("Property DataCorePlugin.ExternalScript.BlinkingGear object  ");
        Assert.That(result1, Is.Not.Null);
        Assert.That(result1.Value.name, Is.EqualTo("DataCorePlugin.ExternalScript.BlinkingGear"));
        Assert.That(result1.Value.type, Is.EqualTo(PropertyType.Object));
        Assert.That(result1.Value.value, Is.EqualTo(" "));

        var result2 = _parser.ParseLine("Property DataCorePlugin.ExternalScript.BlinkingGear object   ");
        Assert.That(result2, Is.Not.Null);
        Assert.That(result2.Value.name, Is.EqualTo("DataCorePlugin.ExternalScript.BlinkingGear"));
        Assert.That(result2.Value.type, Is.EqualTo(PropertyType.Object));
        Assert.That(result2.Value.value, Is.EqualTo("  "));

    }
}
