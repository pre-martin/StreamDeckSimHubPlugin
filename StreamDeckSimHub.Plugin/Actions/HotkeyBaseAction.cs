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
/// Base functionality to send a key stroke to the active window, send an input trigger, and to update the state from a SimHub property.
/// </summary>
/// <remarks>
/// Concrete implementations have to handle the conversion from SimHub property values into Stream Deck action states.
/// </remarks>
public abstract class HotkeyBaseAction<TSettings> : StreamDeckAction<TSettings> where TSettings : HotkeyBaseActionSettings, new()
{
    private readonly SimHubConnection _simHubConnection;
    private TSettings _hotkeySettings;
    private KeyboardUtils.Hotkey? _hotkey;
    private KeyboardUtils.Hotkey? _longKeypressHotkey;
    private readonly StateManager _stateManager;
    private bool _simHubTriggerActive;
    private readonly ShortAndLongPressHandler _salHandler;

    protected HotkeyBaseAction(SimHubConnection simHubConnection, PropertyComparer propertyComparer, bool useCondition)
    {
        _hotkeySettings = new TSettings();
        _simHubConnection = simHubConnection;
        _stateManager = new StateManager(propertyComparer, _simHubConnection, StateChangedFunc, useCondition);
        _salHandler = new ShortAndLongPressHandler(OnShortPress, OnLongPress, OnLongReleased);
    }

    private async Task StateChangedFunc(int state)
    {
        await SetStateAsync(state);
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<TSettings>();
        Logger.LogInformation("OnWillAppear ({coords}): {settings}", args.Payload.Coordinates, settings);
        await SetSettings(settings, true);
        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear ({coords}): {settings}", args.Payload.Coordinates, _hotkeySettings);
        await Unsubscribe();

        // Just to be sure that there are no dangling input triggers. Actually we should not reach this code.
        if (_simHubTriggerActive)
        {
            Logger.LogWarning("SimHub trigger still active. Sending \"released\" command");
            _simHubTriggerActive = false;
            await _simHubConnection.SendTriggerInputReleased(_hotkeySettings.SimHubControl);
        }

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
            await _salHandler.KeyDown(args);
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

    private async Task OnShortPress(ActionEventArgs<KeyPayload> args)
    {
        await DownNormal();
        await Task.Delay(TimeSpan.FromMilliseconds(_hotkeySettings.LongKeypressShortHoldTime));
        await UpNormal();
    }

    private async Task OnLongPress(ActionEventArgs<KeyPayload> args)
    {
        await DownLong();
    }

    private async Task OnLongReleased()
    {
        await UpLong();
    }

    private async Task DownNormal()
    {
        // Hotkey
        KeyboardUtils.KeyDown(_hotkey);
        // SimHubControl
        if (!string.IsNullOrWhiteSpace(_hotkeySettings.SimHubControl))
        {
            _simHubTriggerActive = true;
            await _simHubConnection.SendTriggerInputPressed(_hotkeySettings.SimHubControl);
        }
    }

    private async Task UpNormal()
    {
        // Hotkey
        KeyboardUtils.KeyUp(_hotkey);
        // SimHubControl
        if (!string.IsNullOrWhiteSpace(_hotkeySettings.SimHubControl))
        {
            // Let's hope that nobody changed the settings since the "pressed" command...
            _simHubTriggerActive = false;
            await _simHubConnection.SendTriggerInputReleased(_hotkeySettings.SimHubControl);
        }
        // Stream Deck always toggles the state for each keypress (at "key up", to be precise). So we have to set the
        // state again to the correct one, after Stream Deck has done its toggling stuff.
        await SetStateAsync(_stateManager.State);
    }

    private async Task DownLong()
    {
        // Hotkey
        KeyboardUtils.KeyDown(_longKeypressHotkey);
        // SimHubControl
        if (!string.IsNullOrWhiteSpace(_hotkeySettings.SimHubControl))
        {
            _simHubTriggerActive = true;
            await _simHubConnection.SendTriggerInputPressed(_hotkeySettings.SimHubControl);
        }
    }

    private async Task UpLong()
    {
        // Hotkey
        KeyboardUtils.KeyUp(_longKeypressHotkey);
        // SimHubControl
        if (!string.IsNullOrWhiteSpace(_hotkeySettings.SimHubControl))
        {
            // Let's hope that nobody changed the settings since the "pressed" command...
            _simHubTriggerActive = false;
            await _simHubConnection.SendTriggerInputReleased(_hotkeySettings.SimHubControl);
        }
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
    protected virtual Task Unsubscribe()
    {
        _stateManager.Deactivate();
        return Task.CompletedTask;
    }
}