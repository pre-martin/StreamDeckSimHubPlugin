// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// This action sends an input trigger to SimHub.
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.input")]
public class InputAction : StreamDeckAction<InputSettings>
{
    private readonly SimHubConnection _simHubConnection;
    private InputSettings _inputSettings;

    public InputAction(SimHubConnection simHubConnection)
    {
        _simHubConnection = simHubConnection;
        _inputSettings = new InputSettings();
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<InputSettings>();
        Logger.LogInformation(
            "OnWillAppear: SimHubControl: {SimHubControl}", settings.SimHubControl);
        SetSettings(settings);
        await base.OnWillAppear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, InputSettings settings)
    {
        Logger.LogInformation(
            "OnDidReceiveSettings: SimHubControl: {SimHubControl}", settings.SimHubControl);

        SetSettings(settings);
        await base.OnDidReceiveSettings(args, settings);
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        if (!string.IsNullOrWhiteSpace(_inputSettings.SimHubControl))
            await _simHubConnection.SendTriggerInput(_inputSettings.SimHubControl);

        await base.OnKeyDown(args);
    }

    private void SetSettings(InputSettings settings)
    {
        this._inputSettings = settings;
    }
}