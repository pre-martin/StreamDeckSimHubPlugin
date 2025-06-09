// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.PropertyInspectors;
using StreamDeckSimHub.Plugin.ActionEditor;
using StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton;

/// <summary>
/// Completely customizable button.
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.generic-button")]
public class GenericButtonAction(
    PropertyComparer propertyComparer,
    ImageManager imageManager,
    ActionEditorManager actionEditorManager) : StreamDeckAction<SettingsDto>
{
    private readonly SettingsConverter _settingsConverter = new(propertyComparer, imageManager);
    private StreamDeckKeyInfo? _sdKeyInfo;
    private Coordinates? _coordinates;
    private Settings? _settings;

    private void SubscribeToSettingsChanges()
    {
        if (_settings != null)
        {
            _settings.SettingsChanged += Settings_OnSettingsChanged;
        }
    }

    private void Settings_OnSettingsChanged(object? sender, EventArgs e)
    {
        // Placeholder: Insert code to execute when Settings or any child changes
        Logger.LogInformation("Settings changed");
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillAppear ({coords})", args.Payload.Coordinates);
        _coordinates = args.Payload.Coordinates;

        _sdKeyInfo = StreamDeckKeyInfoBuilder.Build(StreamDeck.Info, args.Device, args.Payload.Controller);
        _settings = ConvertSettings(args.Payload.GetSettings<SettingsDto>(), _sdKeyInfo);
        SubscribeToSettingsChanges();

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

        _settings = ConvertSettings(args.Payload.GetSettings<SettingsDto>(), _sdKeyInfo!);
        SubscribeToSettingsChanges();

        await base.OnDidReceiveSettings(args);
    }

    [PropertyInspectorMethod("openEditor")]
    public void OpenEditor()
    {
        if (_settings != null)
        {
            Logger.LogInformation("Opening editor ({coords})", _coordinates);
            actionEditorManager.ShowGenericButtonEditor(Context, _settings);
        }
    }

    private Settings ConvertSettings(SettingsDto dto, StreamDeckKeyInfo sdKeyInfo)
    {
        var settings = _settingsConverter.SettingsToModel(dto, sdKeyInfo);

        if (settings.KeySize != sdKeyInfo.KeySize)
        {
            // GenericButton is used on a different StreamDeck key. Scale it.
            // TODO Scale, update KeyInfo and save config with SetSettings()
            Logger.LogWarning("Key size changed from {old} to {new}", settings.KeySize, sdKeyInfo.KeySize);
        }
        return settings;
    }
}
