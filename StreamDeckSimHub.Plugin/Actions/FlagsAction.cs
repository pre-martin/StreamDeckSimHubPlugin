// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Tracks the state of all flags.
/// </summary>
internal class FlagState
{
    internal bool Black;
    internal bool Blue;
    internal bool Checkered;
    internal bool Green;
    internal bool Orange;
    internal bool White;
    internal bool Yellow;
}

/// <summary>
/// Displays the flags on a Stream Deck key.
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.flags")]
public class FlagsAction : StreamDeckAction<FlagsSettings>
{
    private readonly SimHubConnection _simHubConnection;
    private readonly ImageManager _imageManager;
    private readonly IPropertyChangedReceiver _propertyChangedReceiver;
    private StreamDeckKeyInfo? _sdKeyInfo;
    private bool _gameRunning;
    private FlagState _flagState = new();
    private Image _noFlag;
    private Image _blackFlag;
    private Image _blueFlag;
    private Image _checkeredFlag;
    private Image _greenFlag;
    private Image _orangeFlag;
    private Image _whiteFlag;
    private Image _yellowFlag;

    public FlagsAction(SimHubConnection simHubConnection, ImageUtils imageUtils, ImageManager imageManager)
    {
        _simHubConnection = simHubConnection;
        _imageManager = imageManager;
        _propertyChangedReceiver = new PropertyChangedDelegate(PropertyChanged);

        _noFlag = imageUtils.GetEmptyImage();
        _blackFlag = imageUtils.GetEmptyImage();
        _blueFlag = imageUtils.GetEmptyImage();
        _checkeredFlag = imageUtils.GetEmptyImage();
        _greenFlag = imageUtils.GetEmptyImage();
        _orangeFlag = imageUtils.GetEmptyImage();
        _whiteFlag = imageUtils.GetEmptyImage();
        _yellowFlag = imageUtils.GetEmptyImage();
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<FlagsSettings>();
        Logger.LogInformation("OnWillAppear ({coords}): {settings}", args.Payload.Coordinates, settings);

        _sdKeyInfo = StreamDeckKeyInfoBuilder.Build(StreamDeck.Info, args.Device, args.Payload.Controller);
        PopulateImages(settings, _sdKeyInfo);

        await SetImageAsync(_checkeredFlag.ToBase64String(PngFormat.Instance));

        await _simHubConnection.Subscribe("DataCorePlugin.GameRunning", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameData.Flag_Black", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameData.Flag_Blue", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameData.Flag_Checkered", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameData.Flag_Green", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameData.Flag_Orange", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameData.Flag_White", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameData.Flag_Yellow", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameRawData.Graphics.globalYellow1", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameRawData.Graphics.globalYellow2", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("DataCorePlugin.GameRawData.Graphics.globalYellow3", _propertyChangedReceiver);

        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear ({coords}): {settings}", args.Payload.Coordinates, args.Payload.GetSettings<DialActionSettings>());

        await _simHubConnection.Unsubscribe("DataCorePlugin.GameRunning", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameData.Flag_Black", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameData.Flag_Blue", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameData.Flag_Checkered", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameData.Flag_Green", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameData.Flag_Orange", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameData.Flag_White", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameData.Flag_Yellow", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameRawData.Graphics.globalYellow1", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameRawData.Graphics.globalYellow2", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("DataCorePlugin.GameRawData.Graphics.globalYellow3", _propertyChangedReceiver);

        await SetImageAsync(_checkeredFlag.ToBase64String(PngFormat.Instance));

        await base.OnWillAppear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, FlagsSettings settings)
    {
        Logger.LogInformation("OnDidReceiveSettings ({coords}): {settings}", args.Payload.Coordinates, settings);

        PopulateImages(settings, _sdKeyInfo!);

        await SetImageAsync(_checkeredFlag.ToBase64String(PngFormat.Instance));

        await base.OnDidReceiveSettings(args, settings);
    }

    protected override async Task OnPropertyInspectorDidAppear(ActionEventArgs args)
    {
        Logger.LogInformation("OnPropertyInspectorDidAppear");

        var images = _imageManager.ListCustomImages();
        await SendToPropertyInspectorAsync(new { message = "customImages", images });
    }

    /// <summary>
    /// Called when the value of a SimHub property has changed.
    /// </summary>
    private async Task PropertyChanged(PropertyChangedArgs args)
    {
        if (args.PropertyName == "DataCorePlugin.GameRunning")
        {
            _gameRunning = Equals(args.PropertyValue, 1);
        }

        if (!_gameRunning)
        {
            // game not running, show checked flag as placeholder
            _flagState = new FlagState();
            await SetImageAsync(_checkeredFlag.ToBase64String(PngFormat.Instance));
            return;
        }

        switch (args.PropertyName)
        {
            case "DataCorePlugin.GameData.Flag_Black":
                _flagState.Black = Equals(args.PropertyValue, 1);
                break;
            case "DataCorePlugin.GameData.Flag_Blue":
                _flagState.Blue = Equals(args.PropertyValue, 1);
                break;
            case "DataCorePlugin.GameData.Flag_Checkered":
                _flagState.Checkered = Equals(args.PropertyValue, 1);
                break;
            case "DataCorePlugin.GameData.Flag_Green":
                _flagState.Green = Equals(args.PropertyValue, 1);
                break;
            case "DataCorePlugin.GameData.Flag_Orange":
                _flagState.Orange = Equals(args.PropertyValue, 1);
                break;
            case "DataCorePlugin.GameData.Flag_White":
                _flagState.White = Equals(args.PropertyValue, 1);
                break;
            case "DataCorePlugin.GameData.Flag_Yellow":
                _flagState.Yellow = Equals(args.PropertyValue, 1);
                break;
        }

        // "Green" must be after other flags, because several flags can be "on" in the same time. We want to have
        // "Green" after "Yellow".
        if (_flagState.Black)
        {
            await SetImageAsync(_blackFlag.ToBase64String(PngFormat.Instance));
        }
        else if (_flagState.Blue)
        {
            await SetImageAsync(_blueFlag.ToBase64String(PngFormat.Instance));
        }
        else if (_flagState.Checkered)
        {
            await SetImageAsync(_checkeredFlag.ToBase64String(PngFormat.Instance));
        }
        else if (_flagState.Orange)
        {
            await SetImageAsync(_orangeFlag.ToBase64String(PngFormat.Instance));
        }
        else if (_flagState.White)
        {
            await SetImageAsync(_whiteFlag.ToBase64String(PngFormat.Instance));
        }
        else if (_flagState.Yellow)
        {
            await SetImageAsync(_yellowFlag.ToBase64String(PngFormat.Instance));
        }
        else if (_flagState.Green)
        {
            await SetImageAsync(_greenFlag.ToBase64String(PngFormat.Instance));
        }
        else
        {
            await SetImageAsync(_noFlag.ToBase64String(PngFormat.Instance));
        }
    }

    private void PopulateImages(FlagsSettings settings, StreamDeckKeyInfo sdKeyInfo)
    {
        _noFlag = _imageManager.GetCustomImage(settings.NoFlag, sdKeyInfo);
        _blackFlag = _imageManager.GetCustomImage(settings.BlackFlag, sdKeyInfo);
        _blueFlag = _imageManager.GetCustomImage(settings.BlueFlag, sdKeyInfo);
        _checkeredFlag = _imageManager.GetCustomImage(settings.CheckeredFlag, sdKeyInfo);
        _greenFlag = _imageManager.GetCustomImage(settings.GreenFlag, sdKeyInfo);
        _orangeFlag = _imageManager.GetCustomImage(settings.OrangeFlag, sdKeyInfo);
        _whiteFlag = _imageManager.GetCustomImage(settings.WhiteFlag, sdKeyInfo);
        _yellowFlag = _imageManager.GetCustomImage(settings.YellowFlag, sdKeyInfo);
    }
}