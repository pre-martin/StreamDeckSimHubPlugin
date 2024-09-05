// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Tracks the state of all flags.
/// </summary>
internal class FlagState
{
    internal bool black;
    internal bool blue;
    internal bool checkered;
    internal bool green;
    internal bool orange;
    internal bool white;
    internal bool yellow;
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
    private DeviceInfo? _deviceInfo;
    private bool _gameRunning;
    private FlagState _flagState = new();
    private readonly string _noFlag;
    private readonly string _blackFlag;
    private readonly string _blueFlag;
    private readonly string _checkeredFlag;
    private readonly string _greenFlag;
    private readonly string _orangeFlag;
    private readonly string _whiteFlag;
    private readonly string _yellowFlag;

    public FlagsAction(SimHubConnection simHubConnection, ImageUtils imageUtils, ImageManager imageManager)
    {
        _simHubConnection = simHubConnection;
        _imageManager = imageManager;
        _propertyChangedReceiver = new PropertyChangedDelegate(PropertyChanged);

        _noFlag = imageUtils.EncodeSvg("<svg viewBox=\"0 0 70 70\" />");
        _blackFlag = imageUtils.LoadSvg("images/icons/flag-black.svg");
        _blueFlag = imageUtils.LoadSvg("images/icons/flag-blue.svg");
        _checkeredFlag = imageUtils.LoadSvg("images/icons/flag-checkered.svg");
        _greenFlag = imageUtils.LoadSvg("images/icons/flag-green.svg");
        _orangeFlag = imageUtils.LoadSvg("images/icons/flag-orange.svg");
        _whiteFlag = imageUtils.LoadSvg("images/icons/flag-white.svg");
        _yellowFlag = imageUtils.LoadSvg("images/icons/flag-yellow.svg");
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<FlagsSettings>();
        Logger.LogInformation("OnWillAppear ({coords}): {settings}", args.Payload.Coordinates, settings);

        _deviceInfo = StreamDeck.Info.Devices.FirstOrDefault(deviceInfo => deviceInfo.Id == args.Device);

        var checkeredFlag = _imageManager.GetCustomImageEncoded(settings.CheckeredFlag, _deviceInfo);
        await SetImageAsync(checkeredFlag);

        await _simHubConnection.Subscribe("dcp.GameRunning", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("dcp.gd.Flag_Black", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("dcp.gd.Flag_Blue", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("dcp.gd.Flag_Checkered", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("dcp.gd.Flag_Green", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("dcp.gd.Flag_Orange", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("dcp.gd.Flag_White", _propertyChangedReceiver);
        await _simHubConnection.Subscribe("dcp.gd.Flag_Yellow", _propertyChangedReceiver);

        await base.OnWillAppear(args);
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear ({coords}): {settings}", args.Payload.Coordinates, args.Payload.GetSettings<DialActionSettings>());

        await _simHubConnection.Unsubscribe("dcp.GameRunning", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("dcp.gd.Flag_Black", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("dcp.gd.Flag_Blue", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("dcp.gd.Flag_Checkered", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("dcp.gd.Flag_Green", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("dcp.gd.Flag_Orange", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("dcp.gd.Flag_White", _propertyChangedReceiver);
        await _simHubConnection.Unsubscribe("dcp.gd.Flag_Yellow", _propertyChangedReceiver);

        await SetImageAsync(_checkeredFlag);

        await base.OnWillAppear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, FlagsSettings settings)
    {
        Logger.LogInformation("OnDidReceiveSettings ({coords}): {settings}", args.Payload.Coordinates, settings);
        // TODO load images from settings

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
        if (args.PropertyName == "dcp.GameRunning")
        {
            _gameRunning = Equals(args.PropertyValue, true);
        }

        if (!_gameRunning)
        {
            // game not running, show checked flag as placeholder
            _flagState = new();
            await SetImageAsync(_checkeredFlag);
            return;
        }


        switch (args.PropertyName)
        {
            case "dcp.gd.Flag_Black":
                _flagState.black = Equals(args.PropertyValue, 1);
                break;
            case "dcp.gd.Flag_Blue":
                _flagState.blue = Equals(args.PropertyValue, 1);
                break;
            case "dcp.gd.Flag_Checkered":
                _flagState.checkered = Equals(args.PropertyValue, 1);
                break;
            case "dcp.gd.Flag_Green":
                _flagState.green = Equals(args.PropertyValue, 1);
                break;
            case "dcp.gd.Flag_Orange":
                _flagState.orange = Equals(args.PropertyValue, 1);
                break;
            case "dcp.gd.Flag_White":
                _flagState.white = Equals(args.PropertyValue, 1);
                break;
            case "dcp.gd.Flag_Yellow":
                _flagState.yellow = Equals(args.PropertyValue, 1);
                break;
        }

        if (_flagState.black)
        {
            await SetImageAsync(_blackFlag);
        }
        else if (_flagState.blue)
        {
            await SetImageAsync(_blueFlag);
        }
        else if (_flagState.checkered)
        {
            await SetImageAsync(_checkeredFlag);
        }
        else if (_flagState.green)
        {
            await SetImageAsync(_greenFlag);
        }
        else if (_flagState.orange)
        {
            await SetImageAsync(_orangeFlag);
        }
        else if (_flagState.white)
        {
            await SetImageAsync(_whiteFlag);
        }
        else if (_flagState.yellow)
        {
            await SetImageAsync(_yellowFlag);
        }
        else
        {
            await SetImageAsync(_noFlag);
        }
    }
}