// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.ComponentModel;
using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.PropertyInspectors;
using StreamDeckSimHub.Plugin.ActionEditor;
using StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Renderer;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;
using StreamDeckAction = StreamDeckSimHub.Plugin.Actions.Model.StreamDeckAction;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton;

/// <summary>
/// Completely customizable button.
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.generic-button")]
public class GenericButtonAction : StreamDeckAction<SettingsDto>, ICommandVisitor
{
    private readonly SettingsConverter _settingsConverter;
    private readonly ImageManager _imageManager;
    private readonly ActionEditorManager _actionEditorManager;
    private readonly SimHubConnection _simHubConnection;
    private readonly NCalcHandler _ncalcHandler;
    private readonly SimHubManager _simHubManager;
    private readonly KeyQueue _dialKeyQueue;
    private readonly IPropertyChangedReceiver _statePropertyChangedReceiver;
    private readonly IButtonRenderer _buttonRenderer;

    private StreamDeckKeyInfo? _sdKeyInfo;
    private Coordinates? _coordinates;
    private Settings? _settings;
    private readonly HashSet<string> _subscribedProperties = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource _settingsChangedDebounceCts = new();

    private List<CommandItem> _activeCommandItems = new();

    public GenericButtonAction(
        SettingsConverter settingsConverter,
        ImageManager imageManager,
        ActionEditorManager actionEditorManager,
        SimHubConnection simHubConnection,
        NCalcHandler ncalcHandler)
    {
        _settingsConverter = settingsConverter;
        _imageManager = imageManager;
        _actionEditorManager = actionEditorManager;
        _simHubConnection = simHubConnection;
        _ncalcHandler = ncalcHandler;
        _simHubManager = new SimHubManager(simHubConnection);
        _dialKeyQueue = new KeyQueue(simHubConnection);
        _statePropertyChangedReceiver = new PropertyChangedDelegate(PropertyChanged);
        _buttonRenderer = new ButtonRendererImageSharp(GetProperty);
    }

    private void SubscribeToSettingsChanges()
    {
        if (_settings != null)
        {
            _settings.SettingsChanged += OnSettingsChanged;
        }
    }

    private async void OnSettingsChanged(object? sender, PropertyChangedEventArgs args)
    {
        try
        {
            // Special handling for DisplayItemImage.RelativePath: When the property changes, we update the Image in this central location.
            if (sender is DisplayItemImage displayItemImage &&
                args.PropertyName == nameof(DisplayItemImage.RelativePath) &&
                _sdKeyInfo != null)
            {
                displayItemImage.Image = _imageManager.GetCustomImage(displayItemImage.RelativePath, _sdKeyInfo);
            }

            // Debounce the more expensive calls.
            await _settingsChangedDebounceCts.CancelAsync();
            _settingsChangedDebounceCts = new CancellationTokenSource();
            var token = _settingsChangedDebounceCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(800), token);
                    if (!token.IsCancellationRequested)
                    {
                        if (_settings != null)
                        {
                            Logger.LogDebug("({coords}) Settings changed: sender={sender}, property={prop}",
                                _coordinates, sender, args.PropertyName);
                            var settingsDto = _settingsConverter.SettingsToDto(_settings);
                            await SetSettingsAsync(settingsDto, token);
                        }

                        await SubscribeProperties();

                        // Element added/removed from CommandItems: No rendering needed.
                        if (args.PropertyName == nameof(Settings.CommandItems)) return;
                        // CommandItem modified: No rendering needed.
                        if (sender is CommandItem) return;

                        await Render();
                    }
                }
                catch (TaskCanceledException)
                {
                }
            }, token);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "({coords}) Error while saving settings", _coordinates);
        }
    }

    #region Stream Deck event handlers

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("({coords}) OnWillAppear", args.Payload.Coordinates);
        _coordinates = args.Payload.Coordinates;
        _buttonRenderer.SetCoordinates(_coordinates);

        _sdKeyInfo = StreamDeckKeyInfoBuilder.Build(StreamDeck.Info, args.Device, args.Payload.Controller);
        _settings = await ConvertSettings(args.Payload.GetSettings<SettingsDto>(), _sdKeyInfo);
        SubscribeToSettingsChanges();
        _dialKeyQueue.Start();
        await SubscribeProperties();
        await Render();

        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("({coords}) OnWillDisappear", args.Payload.Coordinates);
        _actionEditorManager.RemoveGenericButtonEditor(Context);
        _dialKeyQueue.Stop();
        await UnsubscribeProperties();
        await _simHubManager.Deactivate();

        await base.OnWillDisappear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
    {
        // Should not get called, because we have no PropertyInspector. Implementation is the same as in OnWillAppear, but
        // we can omit the code for the StreamDeckKeyInfo.

        Logger.LogInformation("({coords}) OnDidReceiveSettings", args.Payload.Coordinates);
        _coordinates = args.Payload.Coordinates;

        _settings = await ConvertSettings(args.Payload.GetSettings<SettingsDto>(), _sdKeyInfo!);
        SubscribeToSettingsChanges();
        await SubscribeProperties();
        await Render();

        await base.OnDidReceiveSettings(args);
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        if (_settings == null) return;
        Logger.LogInformation("({coords}) OnKeyDown", args.Payload.Coordinates);

        // We want to execute the "Up" action exactly on those keys, that were also pressed down.
        _activeCommandItems = _settings.CommandItems[StreamDeckAction.KeyDown].Where(IsActive).ToList();
        foreach (var commandItem in _activeCommandItems)
        {
            await commandItem.Accept(this, StreamDeckAction.KeyDown, args);
        }
    }

    protected override async Task OnKeyUp(ActionEventArgs<KeyPayload> args)
    {
        Logger.LogInformation("({coords}) OnKeyUp", args.Payload.Coordinates);

        var localCommandItems = new List<CommandItem>(_activeCommandItems);
        _activeCommandItems.Clear();
        foreach (var commandItem in localCommandItems)
        {
            await commandItem.Accept(this, StreamDeckAction.KeyUp, args);
        }
    }

    protected override async Task OnDialRotate(ActionEventArgs<DialRotatePayload> args)
    {
        if (_settings == null) return;
        Logger.LogInformation("({coords}) OnDialRotate (Ticks {ticks})", args.Payload.Coordinates, args.Payload.Ticks);

        var activeCommandItems = args.Payload.Ticks < 0
            ? _settings.CommandItems[StreamDeckAction.DialLeft].Where(IsActive).ToList()
            : _settings.CommandItems[StreamDeckAction.DialRight].Where(IsActive).ToList();

        foreach (var commandItem in activeCommandItems)
        {
            await commandItem.Accept(this, args.Payload.Ticks < 0 ? StreamDeckAction.DialLeft : StreamDeckAction.DialRight, args);
        }
    }

    protected override async Task OnDialDown(ActionEventArgs<DialPayload> args)
    {
        if (_settings == null) return;
        Logger.LogInformation("({coords}) OnDialDown", args.Payload.Coordinates);

        // We want to execute the "Up" action exactly on those keys, that were also pressed down.
        _activeCommandItems = _settings.CommandItems[StreamDeckAction.DialDown].Where(IsActive).ToList();
        foreach (var commandItem in _activeCommandItems)
        {
            await commandItem.Accept(this, StreamDeckAction.DialDown, args);
        }
    }

    protected override async Task OnDialUp(ActionEventArgs<DialPayload> args)
    {
        Logger.LogInformation("({coords}) OnDialUp", args.Payload.Coordinates);

        var localCommandItems = new List<CommandItem>(_activeCommandItems);
        _activeCommandItems.Clear();
        foreach (var commandItem in localCommandItems)
        {
            await commandItem.Accept(this, StreamDeckAction.KeyUp, args);
        }
    }

    protected override async Task OnTouchTap(ActionEventArgs<TouchTapPayload> args)
    {
        if (_settings == null) return;
        Logger.LogInformation("({coords}) OnTouchTap", args.Payload.Coordinates);

        var activeCommandItems = _settings.CommandItems[StreamDeckAction.TouchTap].Where(IsActive).ToList();
        foreach (var commandItem in activeCommandItems)
        {
            await commandItem.Accept(this, StreamDeckAction.TouchTap, args);
        }
    }

    /// <summary>
    /// Called from the Property Inspector to open the editor for this action.
    /// </summary>
    [PropertyInspectorMethod("openEditor")]
    public void OpenEditor()
    {
        if (_settings != null)
        {
            Logger.LogInformation("Opening editor ({coords}) for {uuid}", _coordinates, Context);
            _actionEditorManager.ShowGenericButtonEditor(Context, _settings);
        }
    }

    #endregion

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
            Logger.LogWarning("({coords}) Key size changed from {old} to {new}", _coordinates, settings.KeySize,
                sdKeyInfo.KeySize);
        }

        return settings;
    }

    /// <summary>
    /// Determines which properties are used in the current settings and compares them to the previously subscribed properties.
    /// No longer used properties are unsubscribed, and new properties are subscribed.
    /// </summary>
    private async Task SubscribeProperties()
    {
        Logger.LogDebug("({coords}) SubscribeProperties", _coordinates);
        var newProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var displayItem in _settings?.DisplayItems ?? [])
        {
            // Conditions in DisplayItems can contain properties.
            foreach (var propName in displayItem.NCalcConditionHolder.UsedProperties)
            {
                Logger.LogDebug("({coords})   Found property \"{propName}\" in \"{name}\"", _coordinates, propName,
                    displayItem.DisplayName);
                newProperties.Add(propName);
            }

            // DisplayItemValue.PropertyHolder contains properties.
            if (displayItem is DisplayItemValue displayItemValue)
            {
                foreach (var propName in displayItemValue.NCalcPropertyHolder.UsedProperties)
                {
                    Logger.LogDebug("({coords})   Found property \"{propName}\" in \"{name}\"", _coordinates, propName,
                        displayItem.DisplayName);
                    newProperties.Add(propName);
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
                    foreach (var propName in commandItem.NCalcConditionHolder.UsedProperties)
                    {
                        Logger.LogDebug("({coords})   Found property \"{propName}\" in \"{name}\"", _coordinates, propName,
                            commandItem.DisplayName);
                        newProperties.Add(propName);
                    }
                }
            }
        }

        var danglingProps = _subscribedProperties.Except(newProperties).ToList();
        var newToSubProps = newProperties.Except(_subscribedProperties).ToList();

        _subscribedProperties.Clear();
        _subscribedProperties.UnionWith(newProperties);

        Logger.LogDebug("({coords})   danglingProps : {danglingProps}", _coordinates, danglingProps);
        Logger.LogDebug("({coords})   newToSubProps : {newToSubProps}", _coordinates, newToSubProps);
        Logger.LogDebug("({coords})   now subscribed: {subscribedProps}", _coordinates, _subscribedProperties);

        foreach (var prop in danglingProps)
        {
            await _simHubConnection.Unsubscribe(prop, _statePropertyChangedReceiver);
        }

        foreach (var prop in newToSubProps)
        {
            await _simHubConnection.Subscribe(prop, _statePropertyChangedReceiver);
        }
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
        if (_settings == null || _sdKeyInfo == null) return;

        var image = _buttonRenderer.Render(_sdKeyInfo, _settings.DisplayItems);
        await SetImageAsync(image);
    }

    private IComparable? GetProperty(string propertyName)
    {
        var propertyChangedArgs = _simHubConnection.GetProperty(propertyName);
        return propertyChangedArgs?.PropertyValue;
    }

    private bool IsActive(Item item)
    {
        return _ncalcHandler.IsConditionActive(item.NCalcConditionHolder, GetProperty,
            $"({_coordinates})   IsActive of \"{item.DisplayName}\"");
    }

    #region ICommandVisitor implementation

    public async Task Visit<TPayload>(CommandItemKeypress command, StreamDeckAction action, ActionEventArgs<TPayload> args)
    {
        var ticks = args is ActionEventArgs<DialRotatePayload> dialArgs ? dialArgs.Payload.Ticks : -1;

        switch (action)
        {
            case StreamDeckAction.KeyDown or StreamDeckAction.DialDown:
                if (!command.LongEnabled) KeyboardUtils.KeyDown(command.Hotkey);
                break;
            case StreamDeckAction.KeyUp or StreamDeckAction.DialUp:
                if (!command.LongEnabled) KeyboardUtils.KeyUp(command.Hotkey);
                break;
            case StreamDeckAction.DialLeft:
                _dialKeyQueue.Enqueue(command.Hotkey, null, null, -ticks);
                break;
            case StreamDeckAction.DialRight:
                _dialKeyQueue.Enqueue(command.Hotkey, null, null, ticks);
                break;
            case StreamDeckAction.TouchTap:
                _dialKeyQueue.Enqueue(command.Hotkey, null, null, 1);
                break;
        }
    }

    public async Task Visit<TPayload>(CommandItemSimHubControl command, StreamDeckAction action, ActionEventArgs<TPayload> args)
    {
        var ticks = args is ActionEventArgs<DialRotatePayload> dialArgs ? dialArgs.Payload.Ticks : -1;

        switch (action)
        {
            case StreamDeckAction.KeyDown or StreamDeckAction.DialDown:
                await _simHubManager.TriggerInputPressed(command.Control);
                break;
            case StreamDeckAction.KeyUp or StreamDeckAction.DialUp:
                await _simHubManager.TriggerInputReleased(command.Control);
                break;
            case StreamDeckAction.DialLeft:
                _dialKeyQueue.Enqueue(null, command.Control, null, -ticks);
                break;
            case StreamDeckAction.DialRight:
                _dialKeyQueue.Enqueue(null, command.Control, null, ticks);
                break;
            case StreamDeckAction.TouchTap:
                _dialKeyQueue.Enqueue(null, command.Control, null, 1);
                break;
        }
    }

    public async Task Visit<TPayload>(CommandItemSimHubRole command, StreamDeckAction action, ActionEventArgs<TPayload> args)
    {
        var ticks = args is ActionEventArgs<DialRotatePayload> dialArgs ? dialArgs.Payload.Ticks : -1;

        switch (action)
        {
            case StreamDeckAction.KeyDown or StreamDeckAction.DialDown:
                await _simHubManager.RolePressed(Context, command.Role);
                break;
            case StreamDeckAction.KeyUp or StreamDeckAction.DialUp:
                await _simHubManager.RoleReleased(Context, command.Role);
                break;
            case StreamDeckAction.DialLeft:
                _dialKeyQueue.Enqueue(null, null, (Context, command.Role), -ticks);
                break;
            case StreamDeckAction.DialRight:
                _dialKeyQueue.Enqueue(null, null, (Context, command.Role), ticks);
                break;
            case StreamDeckAction.TouchTap:
                _dialKeyQueue.Enqueue(null, null, (Context, command.Role), 1);
                break;
        }
    }

    #endregion
}