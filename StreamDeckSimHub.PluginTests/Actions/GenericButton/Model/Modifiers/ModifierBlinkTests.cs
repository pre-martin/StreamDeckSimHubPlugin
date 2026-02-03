// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Renderer;
using static StreamDeckSimHub.PluginTests.Actions.GenericButton.RendererTestHelper;
using static StreamDeckSimHub.Plugin.Tools.StreamDeckKeyInfoBuilder;

namespace StreamDeckSimHub.PluginTests.Actions.GenericButton.Model.Modifiers;

public class ModifierBlinkTests
{
    private Settings _settings;

    [SetUp]
    public void Setup()
    {
        _settings = new Settings { KeySize = DefaultKeyInfo.KeySize };
    }

    [Test]
    public void BlinkFullPhase()
    {
        var displayItem = new DisplayItemText { Text = "X" };
        var modifierBlink = new ModifierBlink { DurationOn = 2, DurationOff = 3 };
        displayItem.Modifiers.Add(modifierBlink);
        _settings.DisplayItems.Add(displayItem);

        var renderer = new ButtonRendererImageSharp(EmptyPropertyProvider);

        var activeStateChanged = modifierBlink.DetermineTransition(true);
        Assert.That(activeStateChanged, Is.True, "Expected modifier active state to transition");

        // Tick1: On phase
        modifierBlink.Tick();
        var image1 = renderer.Render(DefaultKeyInfo, _settings.DisplayItems);
        Assert.That(ImageHasNonBlackPixels(image1), Is.True, "Tick 1 should be visible");

        // Tick2: On phase
        modifierBlink.Tick();
        var image2 = renderer.Render(DefaultKeyInfo, _settings.DisplayItems);
        Assert.That(ImageHasNonBlackPixels(image2), Is.True, "Tick 2 should be visible");

        // Tick 3: Off phase
        modifierBlink.Tick();
        var image3 = renderer.Render(DefaultKeyInfo, _settings.DisplayItems);
        Assert.That(ImageHasNonBlackPixels(image3), Is.False, "Tick 3 should be invisible");

        // Tick 4: Off phase
        modifierBlink.Tick();
        var image4 = renderer.Render(DefaultKeyInfo, _settings.DisplayItems);
        Assert.That(ImageHasNonBlackPixels(image4), Is.False, "Tick 4 should be invisible");

        // Tick 5: Off phase
        modifierBlink.Tick();
        var image5 = renderer.Render(DefaultKeyInfo, _settings.DisplayItems);
        Assert.That(ImageHasNonBlackPixels(image5), Is.False, "Tick 5 should be invisible");

        // Tick 1: (wrap) On phase
        modifierBlink.Tick();
        var image6 = renderer.Render(DefaultKeyInfo, _settings.DisplayItems);
        Assert.That(ImageHasNonBlackPixels(image6), Is.True, "Tick 6 should be visible");

        // Tick 2: On phase
        modifierBlink.Tick();
        var image7 = renderer.Render(DefaultKeyInfo, _settings.DisplayItems);
        Assert.That(ImageHasNonBlackPixels(image7), Is.True, "Tick 7 should be visible");

        // Tick 3: Off phase
        modifierBlink.Tick();
        var image8 = renderer.Render(DefaultKeyInfo, _settings.DisplayItems);
        Assert.That(ImageHasNonBlackPixels(image8), Is.False, "Tick 8 should be invisible");
    }

    private string? EmptyPropertyProvider(string propName) => null;
}