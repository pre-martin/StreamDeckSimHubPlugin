// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.PluginTests.SimHub;

/// <summary>
/// Tests to ensure that all known property types are really parsed correctly.
/// </summary>
public class PropertyTypeTests
{
    [Test]
    public void TestBoolean()
    {
        var r1 = PropertyType.Boolean.Parse("True");
        Assert.That(r1, Is.True);
    }

    [Test]
    public void TestInteger()
    {
        var r1 = PropertyType.Integer.Parse("10");
        Assert.That(r1, Is.EqualTo(10));
    }

    [Test]
    public void TestLong()
    {
        var r1 = PropertyType.Long.Parse("10");
        Assert.That(r1, Is.EqualTo(10));
    }

    [Test]
    public void TestDouble()
    {
        var r1 = PropertyType.Double.Parse("10.1");
        Assert.That(r1, Is.EqualTo(10.1));
    }

    [Test]
    public void TestTimespan()
    {
        var r1 = PropertyType.TimeSpan.Parse("1:05:03");
        Assert.That(r1, Is.EqualTo(TimeSpan.FromSeconds(1 * 3600 + 5 * 60 + 3)));
    }

    [Test]
    public void TestString()
    {
        var r1 = PropertyType.String.Parse("Hello");
        Assert.That(r1, Is.EqualTo("Hello"));
    }

    [Test]
    public void TestObject()
    {
        var r1 = PropertyType.Object.Parse("Hello");
        Assert.That(r1, Is.EqualTo("Hello"));
    }
}