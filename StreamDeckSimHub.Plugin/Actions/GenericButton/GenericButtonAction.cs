// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.ComponentModel;
using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.PropertyInspectors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using StreamDeckSimHub.Plugin.ActionEditor;
using StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton;

/// <summary>
/// Completely customizable button.
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.generic-button")]
public class GenericButtonAction : StreamDeckAction<SettingsDto>
{
    private readonly SettingsConverter _settingsConverter;
    private readonly ImageManager _imageManager;
    private readonly ActionEditorManager _actionEditorManager;
    private readonly SimHubConnection _simHubConnection;
    private readonly IPropertyChangedReceiver _statePropertyChangedReceiver;
    private readonly ButtonRenderer _buttonRenderer;

    private StreamDeckKeyInfo? _sdKeyInfo;
    private Coordinates? _coordinates;
    private Settings? _settings;
    private readonly HashSet<string> _subscribedProperties = new(StringComparer.OrdinalIgnoreCase);

    public GenericButtonAction(SettingsConverter settingsConverter,
        ImageManager imageManager,
        ActionEditorManager actionEditorManager,
        SimHubConnection simHubConnection)
    {
        _settingsConverter = settingsConverter;
        _imageManager = imageManager;
        _actionEditorManager = actionEditorManager;
        _simHubConnection = simHubConnection;
        _statePropertyChangedReceiver = new PropertyChangedDelegate(PropertyChanged);
        _buttonRenderer = new ButtonRenderer(GetProperty);
    }

    private void SubscribeToSettingsChanges()
    {
        if (_settings != null)
        {
            _settings.SettingsChanged += OnSettingsChanged;
        }
    }

    private async void OnSettingsChanged(object? sender, EventArgs args)
    {
        try
        {
            // Special handling for DisplayItemImage.RelativePath: When the property changes, we update the Image in this central location.
            if (sender is DisplayItemImage diImage && args is PropertyChangedEventArgs
                {
                    PropertyName: nameof(DisplayItemImage.RelativePath)
                } && _sdKeyInfo != null)
            {
                diImage.Image = _imageManager.GetCustomImage(diImage.RelativePath, _sdKeyInfo);
            }

            if (_settings != null)
            {
                Logger.LogDebug("Settings changed: sender={sender}, args={args}", sender, args);
                var settingsDto = _settingsConverter.SettingsToDto(_settings);
                await SetSettingsAsync(settingsDto);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while saving settings");
        }

        // Update subscription as the used properties may have changed.
        await SubscribeProperties();
        await Render();
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillAppear ({coords})", args.Payload.Coordinates);
        _coordinates = args.Payload.Coordinates;

        _sdKeyInfo = StreamDeckKeyInfoBuilder.Build(StreamDeck.Info, args.Device, args.Payload.Controller);
        _settings = await ConvertSettings(args.Payload.GetSettings<SettingsDto>(), _sdKeyInfo);
        SubscribeToSettingsChanges();
        await SubscribeProperties();
        await Render();

        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear ({coords})", args.Payload.Coordinates);
        _actionEditorManager.RemoveGenericButtonEditor(Context);
        await UnsubscribeProperties();

        await base.OnWillDisappear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
    {
        // Should not get called, because we have no PropertyInspector.

        Logger.LogInformation("OnDidReceiveSettings ({coords})", args.Payload.Coordinates);
        _coordinates = args.Payload.Coordinates;

        _settings = await ConvertSettings(args.Payload.GetSettings<SettingsDto>(), _sdKeyInfo!);
        SubscribeToSettingsChanges();
        await SubscribeProperties();
        await Render();

        await base.OnDidReceiveSettings(args);
    }

    [PropertyInspectorMethod("openEditor")]
    public void OpenEditor()
    {
        if (_settings != null)
        {
            Logger.LogInformation("Opening editor ({coords}) for {uuid}", _coordinates, Context);
            _actionEditorManager.ShowGenericButtonEditor(Context, _settings);
        }
    }

    private async Task<Settings> ConvertSettings(SettingsDto dto, StreamDeckKeyInfo sdKeyInfo)
    {
        var settings = _settingsConverter.SettingsToModel(dto, sdKeyInfo);

        if (settings.KeySize == Settings.NewActionKeySize)
        {
            // This is a completely new action, so we need to set the key size.
            settings.KeySize = sdKeyInfo.KeySize;
            // and persist these modified settings.
            var settingsDto = _settingsConverter.SettingsToDto(settings);
            await SetSettingsAsync(settingsDto);
        }
        else if (settings.KeySize != sdKeyInfo.KeySize)
        {
            // GenericButton is used on a different StreamDeck key. Scale it.
            // TODO Scale, update KeyInfo and save config with SetSettings()
            Logger.LogWarning("Key size changed from {old} to {new}", settings.KeySize, sdKeyInfo.KeySize);
        }

        return settings;
    }

    /// <summary>
    /// Determines which properties are used in the current settings and compares them to the previously subscribed properties.
    /// No longer used properties are unsubscribed, and new properties are subscribed.
    /// </summary>
    private async Task SubscribeProperties()
    {
        var newProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var displayItem in _settings?.DisplayItems ?? [])
        {
            // Conditions in DisplayItems can contain properties.
            foreach (var propName in displayItem.ConditionsHolder.UsedProperties)
            {
                newProperties.Add(propName);
            }

            // DisplayItemValue.Property contains a property.
            if (displayItem is DisplayItemValue displayItemValue)
            {
                if (!string.IsNullOrEmpty(displayItemValue.Property))
                {
                    newProperties.Add(displayItemValue.Property);
                }
            }
        }

        if (_settings?.CommandItems != null)
        {
            foreach (var commandItemList in _settings.CommandItems.Values)
            {
                foreach (var commandItem in commandItemList)
                {
                    // Conditions in CommandItems can contain properties
                    foreach (var propName in commandItem.ConditionsHolder.UsedProperties)
                    {
                        newProperties.Add(propName);
                    }
                }
            }
        }

        var danglingProps = _subscribedProperties.Except(newProperties).ToList();
        var newToSubProps = newProperties.Except(_subscribedProperties).ToList();

        foreach (var prop in danglingProps)
        {
            await _simHubConnection.Unsubscribe(prop, _statePropertyChangedReceiver);
        }

        foreach (var prop in newToSubProps)
        {
            await _simHubConnection.Subscribe(prop, _statePropertyChangedReceiver);
        }


        _subscribedProperties.Clear();
        _subscribedProperties.UnionWith(newProperties);

        Logger.LogDebug("({coords}) danglingProps : {danglingProps}", _coordinates, danglingProps);
        Logger.LogDebug("({coords}) newToSubProps : {newToSubProps}", _coordinates, newToSubProps);
        Logger.LogDebug("({coords}) now subscribed: {subscribedProps}", _coordinates, _subscribedProperties);
    }

    private async Task UnsubscribeProperties()
    {
        foreach (var prop in _subscribedProperties)
        {
            await _simHubConnection.Unsubscribe(prop, _statePropertyChangedReceiver);
        }

        _subscribedProperties.Clear();
    }

    private async Task PropertyChanged(PropertyChangedArgs arg)
    {
        // We do not know if the property belongs to a DisplayItem or a CommandItem. For a CommandItem we would not
        // need to update the display. This could be optimized.
        await Render();
    }

    private async Task Render()
    {
        if (_settings == null) return;

        var image = _buttonRenderer.Render(_settings.KeyInfo, _settings.DisplayItems);
        await SetImageAsync(image.ToBase64String(PngFormat.Instance));
    }

    private IComparable? GetProperty(string propertyName)
    {
        var propertyChangedArgs = _simHubConnection.GetProperty(propertyName);
        return propertyChangedArgs?.PropertyValue;
    }
}