// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Layouts;
using SharpDeck.PropertyInspectors;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions;

[StreamDeckAction("net.planetrenner.simhub.dial")]
public class DialAction : StreamDeckAction<DialActionSettings>
{
    private readonly SimHubConnection _simHubConnection;
    private DialActionSettings _settings = new();
    private KeyboardUtils.Hotkey? _hotkey;
    private KeyboardUtils.Hotkey? _hotkeyLeft;
    private KeyboardUtils.Hotkey? _hotkeyRight;
    private readonly IPropertyChangedReceiver _displayPropertyChangedReceiver;
    private readonly ShakeItStructureFetcher _shakeItStructureFetcher;

    public DialAction(SimHubConnection simHubConnection, ShakeItStructureFetcher shakeItStructureFetcher)
    {
        _simHubConnection = simHubConnection;
        _shakeItStructureFetcher = shakeItStructureFetcher;
        _displayPropertyChangedReceiver = new PropertyChangedDelegate(DisplayPropertyChanged);
    }

    /// <summary>
    /// Method to handle the event "fetchShakeItBassStructure" from the Property Inspector. Fetches the ShakeIt Bass structure
    /// from SimHub and sends the result through the event "shakeItBassStructure" back to the Property Inspector.
    /// </summary>
    [PropertyInspectorMethod("fetchShakeItBassStructure")]
    public async Task FetchShakeItBassStructure(FetchShakeItStructureArgs args)
    {
        var profiles = await _shakeItStructureFetcher.FetchBassStructure();
        await SendToPropertyInspectorAsync(new { message = "shakeItBassStructure", profiles, args.SourceId });
    }

    /// <summary>
    /// Method to handle the event "fetchShakeItMotorsStructure" from the Property Inspector. Fetches the ShakeIt Motors structure
    /// from SimHub and sends the result through the event "shakeItMotorsStructure" back to the Property Inspector.
    /// </summary>
    [PropertyInspectorMethod("fetchShakeItMotorsStructure")]
    public async Task FetchShakeItMotorsStructure(FetchShakeItStructureArgs args)
    {
        var profiles = await _shakeItStructureFetcher.FetchMotorsStructure();
        await SendToPropertyInspectorAsync(new { message = "shakeItMotorsStructure", profiles, args.SourceId });
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<DialActionSettings>();
        Logger.LogInformation("OnWillAppear ({coords}): {settings}", args.Payload.Coordinates, settings);
        await SetSettings(settings, true);

        await base.OnWillAppear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, DialActionSettings settings)
    {
        Logger.LogInformation("OnDidReceiveSettings ({coords}): {settings}", args.Payload.Coordinates, settings);

        await SetSettings(settings, false);
        await base.OnDidReceiveSettings(args, settings);
    }

    protected override async Task OnDialRotate(ActionEventArgs<DialRotatePayload> args)
    {
        Logger.LogInformation("OnDialRotate ({coords}): Ticks: {ticks}, Pressed {pressed}", args.Payload.Coordinates, args.Payload.Ticks,
            args.Payload.Pressed);
        if (args.Payload.Ticks < 0)
        {
            KeyboardUtils.KeyDown(_hotkeyLeft);
            if (!string.IsNullOrWhiteSpace(_settings.SimHubControlLeft))
            {
                await _simHubConnection.SendTriggerInputPressed(_settings.SimHubControlLeft);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            KeyboardUtils.KeyUp(_hotkeyLeft);
            if (!string.IsNullOrWhiteSpace(_settings.SimHubControlLeft))
            {
                await _simHubConnection.SendTriggerInputReleased(_settings.SimHubControlLeft);
            }
        }
        else if (args.Payload.Ticks > 0)
        {
            KeyboardUtils.KeyDown(_hotkeyRight);
            if (!string.IsNullOrWhiteSpace(_settings.SimHubControlRight))
            {
                await _simHubConnection.SendTriggerInputPressed(_settings.SimHubControlRight);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            KeyboardUtils.KeyUp(_hotkeyRight);
            if (!string.IsNullOrWhiteSpace(_settings.SimHubControlRight))
            {
                await _simHubConnection.SendTriggerInputReleased(_settings.SimHubControlRight);
            }
        }
    }

    protected override async Task OnDialPress(ActionEventArgs<DialPayload> args)
    {
        Logger.LogInformation("OnDialPress ({coords}): Pressed {pressed}", args.Payload.Coordinates, args.Payload.Pressed);

        if (args.Payload.Pressed)
        {
            KeyboardUtils.KeyDown(_hotkey);
            if (!string.IsNullOrWhiteSpace(_settings.SimHubControl))
            {
                await _simHubConnection.SendTriggerInputPressed(_settings.SimHubControl);
            }
        }
        else
        {
            KeyboardUtils.KeyUp(_hotkey);
            if (!string.IsNullOrWhiteSpace(_settings.SimHubControl))
            {
                await _simHubConnection.SendTriggerInputReleased(_settings.SimHubControl);
            }
        }
    }

    private async Task SetSettings(DialActionSettings settings, bool forceSubscribe)
    {
        // Unsubscribe previous SimHub property, if it was set and is different than the new one.
        if (!string.IsNullOrEmpty(_settings.DisplaySimHubProperty) && _settings.DisplaySimHubProperty != settings.DisplaySimHubProperty)
        {
            await _simHubConnection.Unsubscribe(_settings.DisplaySimHubProperty, _displayPropertyChangedReceiver);
        }

        _hotkey = KeyboardUtils.CreateHotkey(settings.Ctrl, settings.Alt, settings.Shift, settings.Hotkey);
        _hotkeyLeft = KeyboardUtils.CreateHotkey(settings.CtrlLeft, settings.AltLeft, settings.ShiftLeft, settings.HotkeyLeft);
        _hotkeyRight = KeyboardUtils.CreateHotkey(settings.CtrlRight, settings.AltRight, settings.ShiftRight, settings.HotkeyRight);

        // Subscribe SimHub property, if it is set and different than the previous one.
        if (!string.IsNullOrEmpty(settings.DisplaySimHubProperty) &&
            (settings.DisplaySimHubProperty != _settings.DisplaySimHubProperty || forceSubscribe))
        {
            await _simHubConnection.Subscribe(settings.DisplaySimHubProperty, _displayPropertyChangedReceiver);
        }

        _settings = settings;
    }

    private async Task DisplayPropertyChanged(PropertyChangedArgs args)
    {
        Logger.LogDebug("DisplayProperty {PropertyName} changed to '{PropertyValue}'", args.PropertyName, args.PropertyValue);
        await SetFeedbackAsync(new LayoutA1 { Value = args.PropertyValue == null ? string.Empty : args.PropertyValue.ToString() });
    }
}