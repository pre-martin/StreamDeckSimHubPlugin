// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.PluginTests.PropertyLogic;

public class FormatHelperTests
{
    private FormatHelper _fh = new FormatHelper();

    [Test]
    public void TestEmptyFormatString()
    {
        var result1 = _fh.CompleteFormatString("");
        Assert.That(result1, Is.EqualTo("{0}"));
    }

    [Test]
    public void TestSimpleFormatString()
    {
        var result1 = _fh.CompleteFormatString(":f0");
        Assert.That(result1, Is.EqualTo("{0:f0}"));

        var result2 = _fh.CompleteFormatString("-4:#.###");
        Assert.That(result2, Is.EqualTo("{0,-4:#.###}"));
    }

    [Test]
    public void TestFullFormatString()
    {
        var result1 = _fh.CompleteFormatString("{:f0}");
        Assert.That(result1, Is.EqualTo("{0:f0}"));

        var result2 = _fh.CompleteFormatString("{-4:#.###}");
        Assert.That(result2, Is.EqualTo("{0,-4:#.###}"));

        var result3 = _fh.CompleteFormatString("Some :f0 text before {:f0}");
        Assert.That(result3, Is.EqualTo("Some :f0 text before {0:f0}"));

        var result4 = _fh.CompleteFormatString("{:#0.0 %} now with suffix");
        Assert.That(result4, Is.EqualTo("{0:#0.0 %} now with suffix"));
    }

    [Test]
    public void TestMultilineFormatString()
    {
        var result1 = _fh.CompleteFormatString("Bias:\n{-3:f1}");
        Assert.That(result1, Is.EqualTo("Bias:\n{0,-3:f1}"));
    }
}