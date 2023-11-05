// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Tools;

/// <summary>
/// Manages the "Display" value of a Stream Deck key based on a SimHub property and a format string.
/// </summary>
public class DisplayManager
{
    private readonly ISimHubConnection _simHubConnection;
    private readonly DisplayChangedFunc _displayChangedFunc;
    private readonly FormatHelper _formatHelper = new();
    private readonly PropertyChangedDelegate _displayPropertyChangedReceiver;
    private PropertyChangedArgs? _lastDisplayPropertyChangedEvent;
    private string _displayProperty = string.Empty;
    public string DisplayFormat { get; private set; } = "${0}";
    public IComparable? LastDisplayValue { get; private set; }

    public delegate Task DisplayChangedFunc(IComparable? value, string format);

    public DisplayManager(ISimHubConnection simHubConnection, DisplayChangedFunc displayChangedFunc)
    {
        _simHubConnection = simHubConnection;
        _displayChangedFunc = displayChangedFunc;
        _displayPropertyChangedReceiver = new PropertyChangedDelegate(DisplayPropertyChanged);
    }

    public async Task HandleDisplayProperties(string displayProperty, string displayFormat, bool forceSubscribe)
    {
        // Unsubscribe previous SimHub "Display" property, if it was set and is different than the new one.
        if (!string.IsNullOrEmpty(_displayProperty) && displayProperty != _displayProperty)
        {
            await _simHubConnection.Unsubscribe(_displayProperty, _displayPropertyChangedReceiver);
            // In case of the new "Display" property being invalid or empty, we remove the old title value.
            await DisplayPropertyChanged(null);
        }

        // Subscribe SimHub "Display" property, if it is set and different than the previous one.
        if (!string.IsNullOrEmpty(displayProperty) &&
            (displayProperty != _displayProperty || forceSubscribe))
        {
            await _simHubConnection.Subscribe(displayProperty, _displayPropertyChangedReceiver);
        }

        var displayPropertyChanged = displayProperty != _displayProperty;
        _displayProperty = displayProperty;

        // Redisplay the title if the format for the title has changed.
        var newDisplayFormat = _formatHelper.CompleteFormatString(displayFormat);
        var displayFormatChanged = newDisplayFormat != DisplayFormat;
        DisplayFormat = newDisplayFormat;

        if (displayPropertyChanged || displayFormatChanged)
        {
            await RefireDisplayPropertyChanged();
        }
    }

    public async void Deactivate()
    {
        if (!string.IsNullOrEmpty(_displayProperty))
        {
            await _simHubConnection.Unsubscribe(_displayProperty, _displayPropertyChangedReceiver);
        }
    }

    private async Task DisplayPropertyChanged(PropertyChangedArgs? args)
    {
        _lastDisplayPropertyChangedEvent = args;
        LastDisplayValue = args?.PropertyValue;
        await _displayChangedFunc(args?.PropertyValue, DisplayFormat);
    }

    private async Task RefireDisplayPropertyChanged()
    {
        if (_lastDisplayPropertyChangedEvent != null)
        {
            await DisplayPropertyChanged(_lastDisplayPropertyChangedEvent);
        }
    }

}