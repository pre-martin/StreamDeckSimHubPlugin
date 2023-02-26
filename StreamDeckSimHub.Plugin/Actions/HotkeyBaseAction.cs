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
public abstract class HotkeyBaseAction<TSettings> : StreamDeckAction<TSettings>, IPropertyChangedReceiver
    where TSettings : HotkeyBaseActionSettings, new()
{
    protected SimHubConnection SimHubConnection { get; }
    protected TSettings HotkeySettings { get; private set; }
    private Keyboard.VirtualKeyShort? _vks;
    private Keyboard.ScanCodeShort? _scs;
    private int _state;
    private PropertyChangedArgs? _lastPropertyChangedEvent;
    private bool _simHubTriggerActive;

    protected HotkeyBaseAction(SimHubConnection simHubConnection)
    {
        HotkeySettings = new TSettings();
        SimHubConnection = simHubConnection;
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<TSettings>();
        Logger.LogInformation("OnWillAppear: {settings}", settings);
        await SetSettings(settings, true);
        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear: {settings}", HotkeySettings);
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
    public async Task PropertyChanged(PropertyChangedArgs args)
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
        Logger.LogInformation("OnDidReceiveSettings: {settings}", settings);

        await SetSettings(settings, false);
        await base.OnDidReceiveSettings(args, settings);
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        // Hotkey
        if (HotkeySettings.Ctrl) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (HotkeySettings.Alt) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (HotkeySettings.Shift) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
        if (_vks.HasValue && _scs.HasValue) Keyboard.KeyDown(_vks.Value, _scs.Value);
        // SimHubControl
        if (!string.IsNullOrWhiteSpace(HotkeySettings.SimHubControl))
        {
            _simHubTriggerActive = true;
            await SimHubConnection.SendTriggerInputPressed(HotkeySettings.SimHubControl);
        }

        await base.OnKeyDown(args);
    }

    protected override async Task OnKeyUp(ActionEventArgs<KeyPayload> args)
    {
        // Hotkey
        if (_vks.HasValue && _scs.HasValue) Keyboard.KeyUp(_vks.Value, _scs.Value);
        if (HotkeySettings.Ctrl) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (HotkeySettings.Alt) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (HotkeySettings.Shift) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
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

        await base.OnKeyUp(args);
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
            await SimHubConnection.Unsubscribe(HotkeySettings.SimHubProperty, this);
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
        if (!string.IsNullOrEmpty(settings.SimHubProperty) && (settings.SimHubProperty != HotkeySettings.SimHubProperty || forceSubscribe))
        {
            await SimHubConnection.Subscribe(settings.SimHubProperty, this);
        }

        this.HotkeySettings = settings;
    }

    /// <summary>
    /// This method has to unsubscribe all properties, which have been subscribed by this instance.
    /// </summary>
    protected virtual async Task Unsubscribe()
    {
        if (!string.IsNullOrEmpty(HotkeySettings.SimHubProperty))
        {
            await SimHubConnection.Unsubscribe(HotkeySettings.SimHubProperty, this);
        }
    }
}