// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Actions.GenericButton;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.PluginTests.Actions.GenericButton;

public class CommandHandlerTests
{
    private readonly TimeSpan _timeSpanShorter = TimeSpan.FromMilliseconds(50);
    private readonly TimeSpan _timeSpanLonger = TimeSpan.FromMilliseconds(1000);
    private readonly Hotkey _keyS = KeyboardUtils.CreateHotkey(false, false, false, "S")!;
    private readonly Hotkey _keyL = KeyboardUtils.CreateHotkey(false, false, false, "L")!;
    private Mock<ISimHubConnection> _simHubConnectionMock;
    private Mock<IKeyboardUtils> _keyboardUtilsMock;
    private CommandHandler _commandHandler;

    [SetUp]
    public void Setup()
    {
        _simHubConnectionMock = new Mock<ISimHubConnection>();
        _keyboardUtilsMock = new Mock<IKeyboardUtils>();
        _commandHandler = new CommandHandler(_simHubConnectionMock.Object, _keyboardUtilsMock.Object);
    }

    [Test]
    public async Task TestShortKeypress()
    {
        var settings = CreateSettings();

        await _commandHandler.KeyDown(settings.CommandItems[StreamDeckAction.KeyDown], _ => true);
        await Task.Delay(_timeSpanShorter);
        await _commandHandler.KeyUp();

        _keyboardUtilsMock.Verify(x => x.KeyDown(_keyS), Times.Once);
        _simHubConnectionMock.Verify(x => x.SendTriggerInputPressed("S"), Times.Once);
        _simHubConnectionMock.Verify(x => x.SendTriggerInputReleased("S"), Times.Once);
        _keyboardUtilsMock.Verify(x => x.KeyDown(_keyL), Times.Never);
        _simHubConnectionMock.Verify(x => x.SendTriggerInputPressed("L"), Times.Never);
        _simHubConnectionMock.Verify(x => x.SendTriggerInputReleased("L"), Times.Never);
    }

    [Test]
    public async Task TestLongKeypress()
    {
        var settings = CreateSettings();

        await _commandHandler.KeyDown(settings.CommandItems[StreamDeckAction.KeyDown], _ => true);
        await Task.Delay(_timeSpanLonger);
        await _commandHandler.KeyUp();

        _keyboardUtilsMock.Verify(x => x.KeyDown(_keyS), Times.Never);
        _simHubConnectionMock.Verify(x => x.SendTriggerInputPressed("S"), Times.Never);
        _simHubConnectionMock.Verify(x => x.SendTriggerInputReleased("S"), Times.Never);
        _keyboardUtilsMock.Verify(x => x.KeyDown(_keyL), Times.Once);
        _simHubConnectionMock.Verify(x => x.SendTriggerInputPressed("L"), Times.Once);
        _simHubConnectionMock.Verify(x => x.SendTriggerInputReleased("L"), Times.Once);
    }

    private Settings CreateSettings()
    {
        var settings = new Settings { KeySize = StreamDeckKeyInfoBuilder.DefaultKeyInfo.KeySize };
        settings.CommandItems[StreamDeckAction.KeyDown].Add(new CommandItemKeypress { Key = "S" });
        settings.CommandItems[StreamDeckAction.KeyDown].Add(new CommandItemSimHubControl { Control = "S" });
        settings.CommandItems[StreamDeckAction.KeyDown].Add(new CommandItemKeypress { Key = "L", LongEnabled = true });
        settings.CommandItems[StreamDeckAction.KeyDown].Add(new CommandItemSimHubControl { Control = "L", LongEnabled = true });

        return settings;
    }
}