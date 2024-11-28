// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.PropertyInspectors;
using StreamDeckSimHub.Plugin.ActionEditor;
using StreamDeckSimHub.Plugin.Actions.GenericButtonModel;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Completely customizable button.
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.generic-button")]
public class GenericButtonAction(ActionEditorManager actionEditorManager) : StreamDeckAction<Model>
{
    private Coordinates? _coordinates;

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillAppear ({coords})", args.Payload.Coordinates);
        _coordinates = args.Payload.Coordinates;

        var settings = args.Payload.GetSettings<Model>();
        var sdKeyInfo = StreamDeckKeyInfoBuilder.Build(StreamDeck.Info, args.Device, args.Payload.Controller);
        if (settings.KeyInfo.KeySize != sdKeyInfo.KeySize)
        {
            // GenericButton is used on a different StreamDeck key. Scale it.
            // TODO Scale, update KeyInfo and save config with SetSettings()
        }

        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear ({coords})", args.Payload.Coordinates);
        actionEditorManager.RemoveGenericButtonEditor(Context);

        await base.OnWillDisappear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
    {
        Logger.LogInformation("OnDidReceiveSettings ({coords})", args.Payload.Coordinates);
        await base.OnDidReceiveSettings(args);
    }

    [PropertyInspectorMethod("openEditor")]
    public void OpenEditor()
    {
        Logger.LogInformation("Opening editor ({coords})", _coordinates);
        actionEditorManager.ShowGenericButtonEditor(Context);
    }
}
