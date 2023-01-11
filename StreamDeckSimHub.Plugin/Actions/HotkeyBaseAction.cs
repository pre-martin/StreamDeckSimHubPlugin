// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// This action sends a key stroke to the active window, it can send an input trigger and it can update its state from a SimHub property.
/// </summary>
/// <remarks>
/// Concrete implementations have to handle the conversion from SimHub property values into Stream Deck action states.
/// </remarks>
public abstract class HotkeyBaseAction : StreamDeckAction<HotkeySettings>, IPropertyChangedReceiver
{
    private readonly SimHubConnection _simHubConnection;

    private HotkeySettings _hotkeySettings;
    private Keyboard.VirtualKeyShort? _vks;
    private Keyboard.ScanCodeShort? _scs;
    private int _state;

    protected HotkeyBaseAction(SimHubConnection simHubConnection)
    {
        _hotkeySettings = new HotkeySettings();
        _simHubConnection = simHubConnection;
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<HotkeySettings>();
        Logger.LogInformation(
            "OnWillAppear: Modifiers: Ctrl: {Ctrl}, Alt: {Alt}, Shift: {Shift}, Hotkey: {Hotkey}, SimHubControl: {SimHubControl}, SimHubProperty: {SimHubProperty}",
            settings.Ctrl, settings.Alt, settings.Shift, settings.Hotkey, settings.SimHubControl,
            settings.SimHubProperty);
        await SetSettings(settings, true);
        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        await _simHubConnection.Unsubscribe(_hotkeySettings.SimHubProperty, this);

        await base.OnWillDisappear(args);
    }

    /// <summary>
    /// Called when the value of a SimHub property has changed.
    /// </summary>
    public async void PropertyChanged(PropertyChangedArgs args)
    {
        Logger.LogDebug("Property {PropertyName} changed to '{PropertyValue}'", args.PropertyName, args.PropertyValue);
        _state = ValueToState(args.PropertyType, args.PropertyValue);
        await SetStateAsync(_state);
    }

    protected abstract int ValueToState(PropertyType propertyType, IComparable? propertyValue);

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, HotkeySettings settings)
    {
        Logger.LogInformation(
            "OnDidReceiveSettings: Modifiers: Ctrl: {Ctrl}, Alt: {Alt}, Shift: {Shift}, Hotkey: {Hotkey}, SimHubControl: {SimHubControl}, SimHubProperty: {SimHubProperty}",
            settings.Ctrl, settings.Alt, settings.Shift, settings.Hotkey, settings.SimHubControl,
            settings.SimHubProperty);

        await SetSettings(settings, false);
        await base.OnDidReceiveSettings(args, settings);
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        // Hotkey
        if (_hotkeySettings.Ctrl) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (_hotkeySettings.Alt) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (_hotkeySettings.Shift) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
        if (_vks.HasValue && _scs.HasValue) Keyboard.KeyDown(_vks.Value, _scs.Value);
        // SimHubControl
        if (!string.IsNullOrWhiteSpace(_hotkeySettings.SimHubControl))
            await _simHubConnection.SendTriggerInput(_hotkeySettings.SimHubControl);

        await base.OnKeyDown(args);
    }

    protected override async Task OnKeyUp(ActionEventArgs<KeyPayload> args)
    {
        if (_vks.HasValue && _scs.HasValue) Keyboard.KeyUp(_vks.Value, _scs.Value);
        if (_hotkeySettings.Ctrl) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (_hotkeySettings.Alt) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (_hotkeySettings.Shift) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
        // Stream Deck always toggles the state for each keypress (at "key up", to be precise). So we have to set the
        // state again to the correct one, after Stream Deck has done its toggling stuff.
        await SetStateAsync(_state);

        await base.OnKeyUp(args);
    }

    protected virtual async Task SetSettings(HotkeySettings settings, bool forceSubscribe)
    {
        // Unsubscribe previous SimHub property, if it was set and is different than the new one.
        if (!string.IsNullOrEmpty(_hotkeySettings.SimHubProperty) && _hotkeySettings.SimHubProperty != settings.SimHubProperty)
        {
            await _simHubConnection.Unsubscribe(_hotkeySettings.SimHubProperty, this);
        }

        this._vks = null;
        this._scs = null;
        if (!string.IsNullOrEmpty(settings.Hotkey))
        {
            var virtualKeyShort = KeyboardUtils.FindVirtualKey(settings.Hotkey);
            if (virtualKeyShort == null)
            {
                Logger.LogError("Could not find VirtualKeyCode for hotkey '{Hotkey}'", settings.Hotkey);
                return;
            }

            var scanCodeShort =
                KeyboardUtils.MapVirtualKey((uint)virtualKeyShort, KeyboardUtils.MapType.MAPVK_VK_TO_VSC);
            if (scanCodeShort == 0)
            {
                Logger.LogError("Could not find ScanCode for hotkey '{Hotkey}'", settings.Hotkey);
                return;
            }

            this._vks = virtualKeyShort;
            this._scs = (Keyboard.ScanCodeShort)scanCodeShort;
        }

        // Subscribe SimHub property, if it is set and different than the previous one.
        if ((!string.IsNullOrEmpty(settings.SimHubProperty) && settings.SimHubProperty != _hotkeySettings.SimHubProperty) || forceSubscribe)
        {
            await _simHubConnection.Subscribe(settings.SimHubProperty, this);
        }

        this._hotkeySettings = settings;
    }
}