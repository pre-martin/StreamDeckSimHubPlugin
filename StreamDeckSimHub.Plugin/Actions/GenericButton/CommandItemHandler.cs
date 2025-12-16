// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using NLog;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton;

/// <summary>
/// Takes care of handling commands for key presses, dial rotations, and touch taps.
/// </summary>
public class CommandItemHandler(ISimHubConnection simHubConnection, IKeyboardUtils keyboardUtils) : ICommandItemVisitor
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly PressAndReleaseQueue _pressAndReleaseQueue = new(simHubConnection);
    private readonly SimHubManager _simHubManager = new(simHubConnection);
    private ShortAndLongPressHandler? _salHandler;

    private List<CommandItem> _activeCommandItems = [];

    public string Context { get; set; } = "no valid context";

    public void Start()
    {
        _pressAndReleaseQueue.Start();
    }

    public async Task Stop()
    {
        await _pressAndReleaseQueue.Stop();
        await _simHubManager.Deactivate();
    }

    #region Key, Dial and Touch actions

    public async Task KeyDown(Collection<CommandItem> commandItems, Func<CommandItem, bool> isActive)
    {
        await Down(commandItems, isActive, StreamDeckAction.KeyDown);
    }

    public async Task KeyUp()
    {
        await Up(StreamDeckAction.KeyUp);
    }

    public async Task DialDown(Collection<CommandItem> commandItems, Func<CommandItem, bool> isActive)
    {
        await Down(commandItems, isActive, StreamDeckAction.DialDown);
    }

    public async Task DialUp()
    {
        await Up(StreamDeckAction.DialUp);
    }

    private async Task Down(Collection<CommandItem> commandItems, Func<CommandItem, bool> isActive, StreamDeckAction action)
    {
        _salHandler = null;
        if (action != StreamDeckAction.KeyDown && action != StreamDeckAction.DialDown)
        {
            throw new ArgumentException("Invalid action for Down method", nameof(action));
        }

        // "long" is calculated by looking at all items, independent of their active condition.
        var hasLong = commandItems.Any(ci => ci is ICommandItemLong { LongEnabled: true });
        // We want to execute the following actions (including "Up") on all commands, that are currently active.
        _activeCommandItems = commandItems.Where(isActive).ToList();

        if (hasLong)
        {
            _logger.Debug($"Down: action={action}, active items={_activeCommandItems.Count}, long");

            _salHandler = new ShortAndLongPressHandler(OnShortPress, OnLongPress, OnLongReleased);
            await _salHandler.KeyDown(new SalHandlerArgs(action));
        }
        else
        {
            // Restrict further and remove all "long" command items.
            _activeCommandItems.RemoveAll(ci => ci is ICommandItemLong { LongEnabled: true });
            _logger.Debug($"Down: action={action}, active items={_activeCommandItems.Count}, noLong");

            var localItems = new List<CommandItem>(_activeCommandItems);
            foreach (var commandItem in localItems)
            {
                await commandItem.Accept(this, action);
            }
        }
    }

    private async Task Up(StreamDeckAction action)
    {
        if (action != StreamDeckAction.KeyUp && action != StreamDeckAction.DialUp)
        {
            throw new ArgumentException("Invalid action for Up method", nameof(action));
        }

        _logger.Debug($"Up: action={action}, active items={_activeCommandItems.Count}, long={_salHandler != null}");
        if (_salHandler != null)
        {
            await _salHandler.KeyUp();
            _salHandler = null;
        }
        else
        {
            var localItems = new List<CommandItem>(_activeCommandItems);
            _activeCommandItems.Clear();
            foreach (var commandItem in localItems)
            {
                await commandItem.Accept(this, action);
            }
        }
    }

    public async Task DialRotate(Collection<CommandItem> commandItems, Func<CommandItem, bool> isActive, int ticks)
    {
        var localItems = commandItems.Where(isActive).ToList();
        var args = new DialVisitorArgs(ticks);
        foreach (var commandItem in localItems)
        {
            await commandItem.Accept(this, ticks < 0 ? StreamDeckAction.DialLeft : StreamDeckAction.DialRight, args);
        }
    }

    public async Task TouchTap(Collection<CommandItem> commandItems, Func<CommandItem, bool> isActive)
    {
        var localItems = commandItems.Where(isActive).ToList();
        foreach (var commandItem in localItems)
        {
            await commandItem.Accept(this, StreamDeckAction.TouchTap);
        }
    }

    private async Task OnShortPress(IHandlerArgs? args)
    {
        if (args is not SalHandlerArgs salArgs) return;

        // Restrict further and remove all "long" command items.
        _activeCommandItems.RemoveAll(ci => ci is ICommandItemLong { LongEnabled: true });
        _logger.Debug($"Short press: action={salArgs.Action}, active items={_activeCommandItems.Count}");
        var localItems = new List<CommandItem>(_activeCommandItems);
        _activeCommandItems.Clear();
        foreach (var commandItem in localItems)
        {
            await commandItem.Accept(this, salArgs.Action);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(20));

        var action = salArgs.Action == StreamDeckAction.KeyDown ? StreamDeckAction.KeyUp : StreamDeckAction.DialUp;
        foreach (var commandItem in localItems)
        {
            await commandItem.Accept(this, action);
        }
    }

    private async Task OnLongPress(IHandlerArgs? args)
    {
        if (args is not SalHandlerArgs salArgs) return;

        // Restrict further and remove all "not long" command items.
        _activeCommandItems.RemoveAll(ci => ci is not ICommandItemLong l || l.LongEnabled == false);
        _logger.Debug($"Long press: action={salArgs.Action}, active items={_activeCommandItems.Count}");
        var localItems = new List<CommandItem>(_activeCommandItems);
        foreach (var commandItem in localItems)
        {
            await commandItem.Accept(this, salArgs.Action);
        }
    }

    private async Task OnLongReleased(IHandlerArgs? args)
    {
        if (args is not SalHandlerArgs salArgs) return;

        _logger.Debug($"Long release: action={salArgs.Action}, active items={_activeCommandItems.Count}");
        var localItems = new List<CommandItem>(_activeCommandItems);
        _activeCommandItems.Clear();
        var action = salArgs.Action == StreamDeckAction.KeyDown ? StreamDeckAction.KeyUp : StreamDeckAction.DialUp;
        foreach (var commandItem in localItems)
        {
            await commandItem.Accept(this, action);
        }
    }

    #endregion

    #region ICommandVisitor implementation

    public Task Visit(CommandItemKeypress commandItem, StreamDeckAction action, IVisitorArgs? args)
    {
        _logger.Debug($"Visit for Keypress: \"{commandItem.DisplayName}\", action: {action}");
        var ticks = args is DialVisitorArgs dialArgs ? dialArgs.Ticks : -1;

        switch (action)
        {
            case StreamDeckAction.KeyDown or StreamDeckAction.DialDown:
                keyboardUtils.KeyDown(commandItem.Hotkey);
                break;
            case StreamDeckAction.KeyUp or StreamDeckAction.DialUp:
                keyboardUtils.KeyUp(commandItem.Hotkey);
                break;
            case StreamDeckAction.DialLeft:
                _pressAndReleaseQueue.Enqueue(commandItem.Hotkey, null, null, ticks);
                break;
            case StreamDeckAction.DialRight:
                _pressAndReleaseQueue.Enqueue(commandItem.Hotkey, null, null, ticks);
                break;
            case StreamDeckAction.TouchTap:
                _pressAndReleaseQueue.Enqueue(commandItem.Hotkey, null, null, 1);
                break;
        }

        return Task.CompletedTask;
    }

    public async Task Visit(CommandItemSimHubControl commandItem, StreamDeckAction action, IVisitorArgs? args)
    {
        _logger.Debug($"Visit for SimHub control: \"{commandItem.DisplayName}\", action: {action}");
        var ticks = args is DialVisitorArgs dialArgs ? dialArgs.Ticks : -1;

        switch (action)
        {
            case StreamDeckAction.KeyDown or StreamDeckAction.DialDown:
                await _simHubManager.TriggerInputPressed(commandItem.Control);
                break;
            case StreamDeckAction.KeyUp or StreamDeckAction.DialUp:
                await _simHubManager.TriggerInputReleased(commandItem.Control);
                break;
            case StreamDeckAction.DialLeft:
                _pressAndReleaseQueue.Enqueue(null, commandItem.Control, null, -ticks);
                break;
            case StreamDeckAction.DialRight:
                _pressAndReleaseQueue.Enqueue(null, commandItem.Control, null, ticks);
                break;
            case StreamDeckAction.TouchTap:
                _pressAndReleaseQueue.Enqueue(null, commandItem.Control, null, 1);
                break;
        }
    }

    public async Task Visit(CommandItemSimHubRole commandItem, StreamDeckAction action, IVisitorArgs? args)
    {
        _logger.Debug($"Visit SimHub role for \"{commandItem.DisplayName}\", action: {action}");
        var ticks = args is DialVisitorArgs dialArgs ? dialArgs.Ticks : -1;

        switch (action)
        {
            case StreamDeckAction.KeyDown or StreamDeckAction.DialDown:
                await _simHubManager.RolePressed(Context, commandItem.Role);
                break;
            case StreamDeckAction.KeyUp or StreamDeckAction.DialUp:
                await _simHubManager.RoleReleased(Context, commandItem.Role);
                break;
            case StreamDeckAction.DialLeft:
                _pressAndReleaseQueue.Enqueue(null, null, (Context, commandItem.Role), -ticks);
                break;
            case StreamDeckAction.DialRight:
                _pressAndReleaseQueue.Enqueue(null, null, (Context, commandItem.Role), ticks);
                break;
            case StreamDeckAction.TouchTap:
                _pressAndReleaseQueue.Enqueue(null, null, (Context, commandItem.Role), 1);
                break;
        }
    }

    #endregion
}

internal record SalHandlerArgs(StreamDeckAction Action) : IHandlerArgs;

internal record DialVisitorArgs(int Ticks) : IVisitorArgs;
