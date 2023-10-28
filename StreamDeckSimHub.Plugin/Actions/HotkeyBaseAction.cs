// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
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
    protected SimHubConnection SimHubConnection { get; }
    protected TSettings HotkeySettings { get; private set; }
    private KeyboardUtils.Hotkey? _hotkey;
    private KeyboardUtils.Hotkey? _longKeypressHotkey;
    private int _state;
    private readonly IPropertyChangedReceiver _propertyChangedReceiver;
    private PropertyChangedArgs? _lastPropertyChangedEvent;
    private bool _simHubTriggerActive;
    private readonly ShortAndLongPressHandler _salHandler;

    protected HotkeyBaseAction(SimHubConnection simHubConnection)
    {
        HotkeySettings = new TSettings();
        SimHubConnection = simHubConnection;
        _propertyChangedReceiver = new PropertyChangedDelegate(PropertyChanged);
        _salHandler = new ShortAndLongPressHandler(OnShortPress, OnLongPress, OnLongReleased);
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
        Logger.LogInformation("OnWillDisappear ({coords}): {settings}", args.Payload.Coordinates, HotkeySettings);
        await Unsubscribe();

        // Just to be sure that there are no dangling input triggers. Actually we should not reach this code.
        if (_simHubTriggerActive)
        {
            Logger.LogWarning("SimHub trigger still active. Sending \"released\" command");
            _simHubTriggerActive = false;
            await SimHubConnection.SendTriggerInputReleased(HotkeySettings.SimHubControl);
        }

        await base.OnWillDisappear(args);
    }

    /// <summary>
    /// Called when the value of a SimHub property has changed.
    /// </summary>
    private async Task PropertyChanged(PropertyChangedArgs args)
    {
        Logger.LogDebug("Property {PropertyName} changed to '{PropertyValue}'", args.PropertyName, args.PropertyValue);
        _lastPropertyChangedEvent = args;
        _state = ValueToState(args.PropertyType, args.PropertyValue);
        await SetStateAsync(_state);
    }

    /// <summary>
    /// Refires the last received PropertyChanged event, but only, if we already have received an event so far.
    /// </summary>
    protected async Task RefirePropertyChanged()
    {
        if (_lastPropertyChangedEvent != null)
        {
            await PropertyChanged(_lastPropertyChangedEvent);
        }
    }

    protected abstract int ValueToState(PropertyType propertyType, IComparable? propertyValue);

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, TSettings settings)
    {
        Logger.LogInformation("OnDidReceiveSettings ({coords}): {settings}", args.Payload.Coordinates, settings);

        await SetSettings(settings, false);
        await base.OnDidReceiveSettings(args, settings);
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        if (!HotkeySettings.HasLongKeypress)
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
        if (!HotkeySettings.HasLongKeypress)
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
        await Task.Delay(TimeSpan.FromMilliseconds(HotkeySettings.LongKeypressShortHoldTime));
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
        if (!string.IsNullOrWhiteSpace(HotkeySettings.SimHubControl))
        {
            _simHubTriggerActive = true;
            await SimHubConnection.SendTriggerInputPressed(HotkeySettings.SimHubControl);
        }
    }

    private async Task UpNormal()
    {
        // Hotkey
        KeyboardUtils.KeyUp(_hotkey);
        // SimHubControl
        if (!string.IsNullOrWhiteSpace(HotkeySettings.SimHubControl))
        {
            // Let's hope that nobody changed the settings since the "pressed" command...
            _simHubTriggerActive = false;
            await SimHubConnection.SendTriggerInputReleased(HotkeySettings.SimHubControl);
        }
        // Stream Deck always toggles the state for each keypress (at "key up", to be precise). So we have to set the
        // state again to the correct one, after Stream Deck has done its toggling stuff.
        await SetStateAsync(_state);
    }

    private async Task DownLong()
    {
        // Hotkey
        KeyboardUtils.KeyDown(_longKeypressHotkey);
        // SimHubControl
        if (!string.IsNullOrWhiteSpace(HotkeySettings.SimHubControl))
        {
            _simHubTriggerActive = true;
            await SimHubConnection.SendTriggerInputPressed(HotkeySettings.SimHubControl);
        }
    }

    private async Task UpLong()
    {
        // Hotkey
        KeyboardUtils.KeyUp(_longKeypressHotkey);
        // SimHubControl
        if (!string.IsNullOrWhiteSpace(HotkeySettings.SimHubControl))
        {
            // Let's hope that nobody changed the settings since the "pressed" command...
            _simHubTriggerActive = false;
            await SimHubConnection.SendTriggerInputReleased(HotkeySettings.SimHubControl);
        }
        // Stream Deck always toggles the state for each keypress (at "key up", to be precise). So we have to set the
        // state again to the correct one, after Stream Deck has done its toggling stuff.
        await SetStateAsync(_state);
    }

    /// <summary>
    /// Configures this action with the given settings. This method is also responsible to subscribe the "SimHubProperty (for state)"
    /// and to unsubscribe a previously used SimHub property from the SimHubConnection.
    /// </summary>
    protected virtual async Task SetSettings(TSettings settings, bool forceSubscribe)
    {
        // Unsubscribe previous SimHub property, if it was set and is different than the new one.
        if (!string.IsNullOrEmpty(HotkeySettings.SimHubProperty) && HotkeySettings.SimHubProperty != settings.SimHubProperty)
        {
            await SimHubConnection.Unsubscribe(HotkeySettings.SimHubProperty, _propertyChangedReceiver);
        }

        _hotkey = KeyboardUtils.CreateHotkey(settings.Ctrl, settings.Alt, settings.Shift, settings.Hotkey);
        _longKeypressHotkey = KeyboardUtils.CreateHotkey(settings.LongKeypressSettings.Ctrl, settings.LongKeypressSettings.Alt, settings.LongKeypressSettings.Shift, settings.LongKeypressSettings.Hotkey);

        // Subscribe SimHub property, if it is set and different than the previous one.
        if (!string.IsNullOrEmpty(settings.SimHubProperty) && (settings.SimHubProperty != HotkeySettings.SimHubProperty || forceSubscribe))
        {
            await SimHubConnection.Subscribe(settings.SimHubProperty, _propertyChangedReceiver);
        }

        HotkeySettings = settings;
        _salHandler.LongPressTimeSpan = TimeSpan.FromMilliseconds(HotkeySettings.LongKeypressTimeSpan);
    }

    /// <summary>
    /// This method has to unsubscribe all properties, which have been subscribed by this instance.
    /// </summary>
    protected virtual async Task Unsubscribe()
    {
        if (!string.IsNullOrEmpty(HotkeySettings.SimHubProperty))
        {
            await SimHubConnection.Unsubscribe(HotkeySettings.SimHubProperty, _propertyChangedReceiver);
        }
    }
}