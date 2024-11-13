// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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
    internal bool YellowSec1;
    internal bool YellowSec2;
    internal bool YellowSec3;
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
    private readonly Flags _flags = new();
    private int _tickCounter;
    private Image? _currentImage;
    private FlagData? _currentFlagData;

    public FlagsAction(SimHubConnection simHubConnection, ImageManager imageManager)
    {
        _simHubConnection = simHubConnection;
        _imageManager = imageManager;
        _propertyChangedReceiver = new PropertyChangedDelegate(PropertyChanged);
    }

    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<FlagsSettings>();
        Logger.LogInformation("OnWillAppear ({coords}): {settings}", args.Payload.Coordinates, settings);

        _sdKeyInfo = StreamDeckKeyInfoBuilder.Build(StreamDeck.Info, args.Device, args.Payload.Controller);
        ConvertSettings(settings, _sdKeyInfo);
        await SetActiveImage(_flags.CheckeredFlag.Image);

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

        PeriodicBackgroundService.Tick += OnTick;
    }

    private async Task OnTick()
    {
        // Flash (toggle between image and blank image) if flashing is enabled for the current flag.
        if (_currentFlagData != null && _currentFlagData.Flash && _currentImage != null)
        {
            if (_tickCounter < _currentFlagData.FlashOn)
            {
                Logger.LogTrace("OnTick ON ({ticks})...", _tickCounter);
                await SetImageAsync(_currentImage.ToBase64String(PngFormat.Instance));
            }
            else
            {
                Logger.LogTrace("OnTick OFF ({ticks})...", _tickCounter);
                await SetImageAsync(ImageUtils.EmptyImage.ToBase64String(PngFormat.Instance));
            }

            _tickCounter++;
            if (_tickCounter >= _currentFlagData.FlashOn + _currentFlagData.FlashOff)
            {
                Logger.LogTrace("Resetting tickCounter");
                _tickCounter = 0;
            }
        }
    }

    protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
    {
        Logger.LogInformation("OnWillDisappear ({coords}): {settings}", args.Payload.Coordinates, args.Payload.GetSettings<DialActionSettings>());

        PeriodicBackgroundService.Tick -= OnTick;

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

        await SetActiveImage(_flags.CheckeredFlag.Image);

        await base.OnWillDisappear(args);
    }

    protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, FlagsSettings settings)
    {
        Logger.LogInformation("OnDidReceiveSettings ({coords}): {settings}", args.Payload.Coordinates, settings);

        ConvertSettings(settings, _sdKeyInfo!);
        await SetActiveImage(_flags.CheckeredFlag.Image);

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
            await SetActiveImage(_flags.CheckeredFlag.Image);
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
            case "DataCorePlugin.GameRawData.Graphics.globalYellow1":
                _flagState.YellowSec1 = Equals(args.PropertyValue, 1);
                break;
            case "DataCorePlugin.GameRawData.Graphics.globalYellow2":
                _flagState.YellowSec2 = Equals(args.PropertyValue, 1);
                break;
            case "DataCorePlugin.GameRawData.Graphics.globalYellow3":
                _flagState.YellowSec3 = Equals(args.PropertyValue, 1);
                break;
        }

        // "Green" must be after other flags, because several flags can be "on" in the same time. We want to have
        // "Green" after "Yellow".
        if (_flagState.Black)
        {
            await SetActiveImage(_flags.BlackFlag);
        }
        else if (_flagState.Blue)
        {
            await SetActiveImage(_flags.BlueFlag);
        }
        else if (_flagState.Checkered)
        {
            await SetActiveImage(_flags.CheckeredFlag);
        }
        else if (_flagState.Orange)
        {
            await SetActiveImage(_flags.OrangeFlag);
        }
        else if (_flagState.White)
        {
            await SetActiveImage(_flags.WhiteFlag);
        }
        else if (_flagState.Yellow)
        {
            await SetActiveImage(_flags.YellowFlag);
        }
        else if (_flagState.YellowSec1 || _flagState.YellowSec2 || _flagState.YellowSec3)
        {
            // We have to combine them on a new image with the same size
            var image = new Image<Rgba32>(_flags.SectorFlag.Image.Width, _flags.SectorFlag.Image.Height);
            if (_flagState.YellowSec1)
            {
                image.Mutate(x => x.DrawImage(_flags.SectorFlag.Image, 1f));
            }

            if (_flagState.YellowSec2)
            {
                image.Mutate(x => x.DrawImage(_flags.SectorFlag.Image2, 1f));
            }

            if (_flagState.YellowSec3)
            {
                image.Mutate(x => x.DrawImage(_flags.SectorFlag.Image3, 1f));
            }

            await SetActiveImage(image, _flags.SectorFlag);
        }
        else if (_flagState.Green)
        {
            await SetActiveImage(_flags.GreenFlag);
        }
        else
        {
            await SetActiveImage(_flags.NoFlag);
        }
    }

    /// <summary>
    /// Displays the given image. Flashing is not supported.
    /// </summary>
    private async Task SetActiveImage(Image image)
    {
        await SetImageAsync(image.ToBase64String(PngFormat.Instance));
        _tickCounter = 0;
        _currentImage = image;
        _currentFlagData = null;
    }

    /// <summary>
    /// Displays the image from <c>FlagData</c>. <c>FlagData</c> is also used for flashing.
    /// </summary>
    private async Task SetActiveImage(FlagData fd)
    {
        await SetActiveImage(fd.Image);
        _currentFlagData = fd;
    }

    /// <summary>
    /// Displays the image (not from <c>FlagData</c>!). <c>FlagData</c> is used for flashing.
    /// </summary>
    private async Task SetActiveImage(Image image, FlagData fd)
    {
        await SetActiveImage(image);
        _currentFlagData = fd;
    }

    /// <summary>
    /// Convert the flat property list from <c>FlagsSettings</c> (received from the Property Inspector) into the
    /// structure <c>Flags</c>. Flash properties are validated during this step.
    /// </summary>
    private void ConvertSettings(FlagsSettings fs, StreamDeckKeyInfo sdKeyInfo)
    {
        SetImageData(_flags.NoFlag, fs.NoFlag, sdKeyInfo);
        SetImageData(_flags.BlackFlag, fs.BlackFlag, sdKeyInfo);
        SetImageData(_flags.BlueFlag, fs.BlueFlag, sdKeyInfo);
        SetImageData(_flags.CheckeredFlag, fs.CheckeredFlag, sdKeyInfo);
        SetImageData(_flags.GreenFlag, fs.GreenFlag, sdKeyInfo);
        SetImageData(_flags.OrangeFlag, fs.OrangeFlag, sdKeyInfo);
        SetImageData(_flags.WhiteFlag, fs.WhiteFlag, sdKeyInfo);
        SetImageData(_flags.YellowFlag, fs.YellowFlag, sdKeyInfo);
        SetImageData(_flags.SectorFlag, fs.YellowSec1, sdKeyInfo);
        SetSectorImageData(_flags.SectorFlag, fs.YellowSec2, fs.YellowSec3, sdKeyInfo);

        SetFlashData(_flags.NoFlag, fs.NoFlagFlash, fs.NoFlagFlashOn, fs.NoFlagFlashOff);
        SetFlashData(_flags.BlackFlag, fs.BlackFlagFlash, fs.BlackFlagFlashOn, fs.BlackFlagFlashOff);
        SetFlashData(_flags.BlueFlag, fs.BlueFlagFlash, fs.BlueFlagFlashOn, fs.BlueFlagFlashOff);
        SetFlashData(_flags.CheckeredFlag, fs.CheckeredFlagFlash, fs.CheckeredFlagFlashOn, fs.CheckeredFlagFlashOff);
        SetFlashData(_flags.GreenFlag, fs.GreenFlagFlash, fs.GreenFlagFlashOn, fs.GreenFlagFlashOff);
        SetFlashData(_flags.OrangeFlag, fs.OrangeFlagFlash, fs.OrangeFlagFlashOn, fs.OrangeFlagFlashOff);
        SetFlashData(_flags.WhiteFlag, fs.WhiteFlagFlash, fs.WhiteFlagFlashOn, fs.WhiteFlagFlashOff);
        SetFlashData(_flags.YellowFlag, fs.YellowFlagFlash, fs.YellowFlagFlashOn, fs.YellowFlagFlashOff);
        SetFlashData(_flags.SectorFlag, fs.YellowSecFlash, fs.YellowSecFlashOn, fs.YellowSecFlashOff);
    }

    /// <summary>
    /// Sets the image (filename and data) of a <c>FlagData</c> structure - only if the filename has changed.
    /// </summary>
    private void SetImageData(FlagData fd, string fileName, StreamDeckKeyInfo sdKeyInfo)
    {
        if (fd.FileName != fileName)
        {
            fd.FileName = fileName;
            fd.Image = string.IsNullOrEmpty(fileName)
                ? ImageUtils.EmptyImage
                : _imageManager.GetCustomImage(fileName, sdKeyInfo);
        }
    }

    private void SetSectorImageData(SectorFlagData fd, string fileName2, string fileName3, StreamDeckKeyInfo sdKeyInfo)
    {
        if (fd.FileName2 != fileName2)
        {
            fd.FileName2 = fileName2;
            fd.Image2 = string.IsNullOrEmpty(fileName2)
                ? ImageUtils.EmptyImage
                : _imageManager.GetCustomImage(fileName2, sdKeyInfo);
        }

        if (fd.FileName3 != fileName3)
        {
            fd.FileName3 = fileName3;
            fd.Image3 = string.IsNullOrEmpty(fileName2)
                ? ImageUtils.EmptyImage
                : _imageManager.GetCustomImage(fileName3, sdKeyInfo);
        }
    }

    private void SetFlashData(FlagData fd, bool flash, int? flashOn, int? flashOff)
    {
        const int min = 1;
        const int max = 50;

        fd.Flash = flash;
        fd.FlashOn = flashOn ?? 0;
        fd.FlashOff = flashOff ?? 0;

        if (flash)
        {
            if (fd.FlashOn is < min or > max)
            {
                Logger.LogWarning("Value {flashOn} for 'Flash On' is not in the allowed range {min}..{max}. Using 5.", flashOn, min, max);
                fd.FlashOn = 5;
            }

            if (fd.FlashOff is < min or > max)
            {
                Logger.LogWarning("Value {flashOff} for 'Flash Off' is not in the allowed range {min}..{max}. Using 5.", flashOff, min, max);
                fd.FlashOff = 5;
            }
        }
    }
}
