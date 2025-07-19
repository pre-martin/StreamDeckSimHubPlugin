// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Base functionality to send a keystroke to the active window, send an input trigger, and to update the state from a SimHub property.
/// </summary>
/// <remarks>
/// Concrete implementations have to handle the conversion from SimHub property values into Stream Deck action states.
/// </remarks>
public abstract class HotkeyBaseAction<TSettings> : StreamDeckAction<TSettings> where TSettings : HotkeyBaseActionSettings, new()
{
    private TSettings _hotkeySettings;
    private Hotkey? _hotkey;
    private Hotkey? _longKeypressHotkey;
    private readonly KeyboardUtils _keyboardUtils = new();
    private readonly StateManager _stateManager;
    private readonly SimHubManager _simHubManager;
    private readonly ShortAndLongPressHandler _salHandler;
    protected Coordinates? Coordinates;

    protected HotkeyBaseAction(SimHubConnection simHubConnection, PropertyComparer propertyComparer, bool useCondition)
    {
        _hotkeySettings = new TSettings();
        _stateManager = new StateManager(propertyComparer, simHubConnection, StateChangedFunc, useCondition);
        _simHubManager = new SimHubManager(simHubConnection);
        _salHandler = new ShortAndLongPressHandler(OnShortPress, OnLongPress, OnLongReleased);
    }

    private async Task StateChangedFunc(int state)
    {
        await SetStateAsync(state);
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<TSettings>();
        Coordinates = args.Payload.Coordinates;
        Logger.LogInformation("OnWillAppear ({coords}): {settings}", args.Payload.Coordinates, settings);

        await SetSettings(settings, true);
        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear ({coords}): {settings}", args.Payload.Coordinates, _hotkeySettings);
        await Unsubscribe();

        await base.OnWillDisappear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, TSettings settings)
    {
        Logger.LogInformation("OnDidReceiveSettings ({coords}): {settings}", args.Payload.Coordinates, settings);

        await SetSettings(settings, false);
        await base.OnDidReceiveSettings(args, settings);
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        if (!_hotkeySettings.HasLongKeypress)
        {
            await DownNormal();
        }
        else
        {
            await _salHandler.KeyDown();
        }
    }

    protected override async Task OnKeyUp(ActionEventArgs<KeyPayload> args)
    {
        if (!_hotkeySettings.HasLongKeypress)
        {
            await UpNormal();
        }
        else
        {
            await _salHandler.KeyUp();
        }
    }

    private async Task OnShortPress(IHandlerArgs? args)
    {
        await DownNormal();
        await Task.Delay(TimeSpan.FromMilliseconds(_hotkeySettings.LongKeypressShortHoldTime));
        await UpNormal();
    }

    private async Task OnLongPress(IHandlerArgs? args)
    {
        await DownLong();
    }

    private async Task OnLongReleased(IHandlerArgs? args)
    {
        await UpLong();
    }

    private async Task DownNormal()
    {
        // Hotkey
        _keyboardUtils.KeyDown(_hotkey);
        // SimHubControl
        await _simHubManager.TriggerInputPressed(_hotkeySettings.SimHubControl);
        // SimHubRole
        await _simHubManager.RolePressed(Context, _hotkeySettings.SimHubRole);
    }

    private async Task UpNormal()
    {
        // Hotkey
        _keyboardUtils.KeyUp(_hotkey);
        // SimHubControl
        await _simHubManager.TriggerInputReleased(_hotkeySettings.SimHubControl);
        // SimHubRole
        await _simHubManager.RoleReleased(Context, _hotkeySettings.SimHubRole);
        // Stream Deck always toggles the state for each keypress (at "key up", to be precise). So we have to set the
        // state again to the correct one, after Stream Deck has done its toggling stuff.
        await SetStateAsync(_stateManager.State);
    }

    private async Task DownLong()
    {
        // Hotkey
        _keyboardUtils.KeyDown(_longKeypressHotkey);
        // SimHubControl
        await _simHubManager.TriggerInputPressed(_hotkeySettings.SimHubControl);
        // SimHubRole
        await _simHubManager.RolePressed(Context, _hotkeySettings.SimHubRole);
    }

    private async Task UpLong()
    {
        // Hotkey
        _keyboardUtils.KeyUp(_longKeypressHotkey);
        // SimHubControl
        await _simHubManager.TriggerInputReleased(_hotkeySettings.SimHubControl);
        // SimHubRole
        await _simHubManager.RoleReleased(Context, _hotkeySettings.SimHubRole);
        // Stream Deck always toggles the state for each keypress (at "key up", to be precise). So we have to set the
        // state again to the correct one, after Stream Deck has done its toggling stuff.
        await SetStateAsync(_stateManager.State);
    }

    /// <summary>
    /// Configures this action with the given settings. This method is also responsible to subscribe the "SimHubProperty (for state)"
    /// and to unsubscribe a previously used SimHub property from the SimHubConnection.
    /// </summary>
    protected virtual async Task SetSettings(TSettings settings, bool forceSubscribe)
    {
        await _stateManager.HandleExpression(settings.SimHubProperty, forceSubscribe);

        _hotkey = KeyboardUtils.CreateHotkey(settings.Ctrl, settings.Alt, settings.Shift, settings.Hotkey);
        _longKeypressHotkey = KeyboardUtils.CreateHotkey(settings.LongKeypressSettings.Ctrl, settings.LongKeypressSettings.Alt, settings.LongKeypressSettings.Shift, settings.LongKeypressSettings.Hotkey);

        _hotkeySettings = settings;
        _salHandler.LongPressTimeSpan = TimeSpan.FromMilliseconds(_hotkeySettings.LongKeypressTimeSpan);
    }

    /// <summary>
    /// This method has to unsubscribe all properties, which have been subscribed by this instance.
    /// </summary>
    protected virtual async Task Unsubscribe()
    {
        _stateManager.Deactivate();
        await _simHubManager.Deactivate();
    }
}