// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Layouts;
using SharpDeck.PropertyInspectors;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions;

[StreamDeckAction("net.planetrenner.simhub.dial")]
public class DialAction : StreamDeckAction<DialActionSettings>
{
    private readonly SimHubConnection _simHubConnection;
    private readonly ImageUtils _imageUtils;
    private readonly StateManager _stateManager;
    private readonly DisplayManager _displayManager;
    private DialActionSettings _settings = new();
    private KeyboardUtils.Hotkey? _hotkey;
    private KeyboardUtils.Hotkey? _hotkeyLeft;
    private KeyboardUtils.Hotkey? _hotkeyRight;
    private readonly ShakeItStructureFetcher _shakeItStructureFetcher;
    private readonly KeyQueue _keyQueue;

    public DialAction(SimHubConnection simHubConnection, PropertyComparer propertyComparer, ImageUtils imageUtils, ShakeItStructureFetcher shakeItStructureFetcher)
    {
        _simHubConnection = simHubConnection;
        _imageUtils = imageUtils;
        _keyQueue = new KeyQueue(simHubConnection);
        _shakeItStructureFetcher = shakeItStructureFetcher;
        _stateManager = new StateManager(propertyComparer, simHubConnection, StateChangedFunc);
        _displayManager = new DisplayManager(simHubConnection, DisplayChangedFunc);
    }

    private async Task StateChangedFunc(int _)
    {
        await SetFeedback(_displayManager.LastDisplayValue, _displayManager.DisplayFormat);
    }

    private async Task DisplayChangedFunc(IComparable? value, string format)
    {
        await SetFeedback(value, format);
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
        _keyQueue.Start();
        await SetSettings(settings, true);

        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear ({coords}): {settings}", args.Payload.Coordinates, args.Payload.GetSettings<DialActionSettings>());
        _keyQueue.Stop();
        _stateManager.Deactivate();
        _displayManager.Deactivate();

        await base.OnWillDisappear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, DialActionSettings settings)
    {
        Logger.LogInformation("OnDidReceiveSettings ({coords}): {settings}", args.Payload.Coordinates, settings);

        await SetSettings(settings, false);
        await base.OnDidReceiveSettings(args, settings);
    }

    protected override async Task OnTitleParametersDidChange(ActionEventArgs<TitlePayload> args)
    {
        // Display the title on the round icon in the Stream Deck application.
        var title = string.IsNullOrEmpty(args.Payload.Title) ? "Dial" : args.Payload.Title;
        await SetImageAsync(_imageUtils.GenerateDialImage(title));
        await base.OnTitleParametersDidChange(args);
    }

    protected override Task OnDialRotate(ActionEventArgs<DialRotatePayload> args)
    {
        Logger.LogInformation("OnDialRotate ({coords}): Ticks: {ticks}, Pressed {pressed}", args.Payload.Coordinates, args.Payload.Ticks, args.Payload.Pressed);
        // Rotate events can appear faster than they are processed (because we have a delay between "key down" and "key up".
        // Thus we have to place them into a queue, where they are processed by a different thread.
        switch (args.Payload.Ticks)
        {
            case < 0:
                _keyQueue.Enqueue(_hotkeyLeft, _settings.SimHubControlLeft, -args.Payload.Ticks);
                break;
            case > 0:
                _keyQueue.Enqueue(_hotkeyRight, _settings.SimHubControlRight, args.Payload.Ticks);
                break;
        }

        return Task.CompletedTask;
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
        await _stateManager.HandleExpression(settings.SimHubProperty, forceSubscribe);
        await _displayManager.HandleDisplayProperties(settings.DisplaySimHubProperty, settings.DisplayFormat, forceSubscribe);

        _hotkey = KeyboardUtils.CreateHotkey(settings.Ctrl, settings.Alt, settings.Shift, settings.Hotkey);
        _hotkeyLeft = KeyboardUtils.CreateHotkey(settings.CtrlLeft, settings.AltLeft, settings.ShiftLeft, settings.HotkeyLeft);
        _hotkeyRight = KeyboardUtils.CreateHotkey(settings.CtrlRight, settings.AltRight, settings.ShiftRight, settings.HotkeyRight);

        _settings = settings;
    }

    private async Task SetFeedback(IComparable? value, string format)
    {
        var displayValue = value ?? string.Empty;
        var color = _stateManager.State > 0 ? "#ffffff" : "#333333";
        try
        {
            await SetFeedbackAsync(new DialLayout { Value = new Text { Value = string.Format(format, displayValue), Color = color } });
        }
        catch (FormatException)
        {
            await SetFeedbackAsync(new DialLayout { Value = new Text { Value = displayValue.ToString(), Color = color } });
        }
    }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class DialLayout
{
    public Text Value { get; set; } = "";
}