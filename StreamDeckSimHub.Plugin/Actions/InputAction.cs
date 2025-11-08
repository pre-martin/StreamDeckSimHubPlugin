// Copyright (C) 2025 Martin Renner
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
public class InputAction(ISimHubConnection simHubConnection) : StreamDeckAction<InputSettings>
{
    private InputSettings _inputSettings = new();
    private bool _simHubTriggerActive;

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<InputSettings>();
        Logger.LogInformation(
            "OnWillAppear ({coords}): SimHubControl: {SimHubControl}", args.Payload.Coordinates, settings.SimHubControl);
        SetSettings(settings);
        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear ({coords})", args.Payload.Coordinates);
        // Just to be sure that there are no dangling input triggers. Actually we should not reach this code.
        if (_simHubTriggerActive)
        {
            Logger.LogWarning("SimHub trigger still active. Sending \"released\" command");
            _simHubTriggerActive = false;
            await simHubConnection.SendTriggerInputReleased(_inputSettings.SimHubControl);
        }

        await base.OnWillDisappear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, InputSettings settings)
    {
        Logger.LogInformation(
            "OnDidReceiveSettings ({coords}): SimHubControl: {SimHubControl}", args.Payload.Coordinates, settings.SimHubControl);

        SetSettings(settings);
        await base.OnDidReceiveSettings(args, settings);
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        if (!string.IsNullOrWhiteSpace(_inputSettings.SimHubControl))
        {
            _simHubTriggerActive = true;
            await simHubConnection.SendTriggerInputPressed(_inputSettings.SimHubControl);
        }

        await base.OnKeyDown(args);
    }

    protected override async Task OnKeyUp(ActionEventArgs<KeyPayload> args)
    {
        if (!string.IsNullOrWhiteSpace(_inputSettings.SimHubControl))
        {
            _simHubTriggerActive = false;
            await simHubConnection.SendTriggerInputReleased(_inputSettings.SimHubControl);
        }

        await base.OnKeyUp(args);
    }

    private void SetSettings(InputSettings settings)
    {
        this._inputSettings = settings;
    }
}