﻿// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SharpDeck;
using SharpDeck.PropertyInspectors;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Arguments sent from the Property Inspector for the event "fetchShakeItBassStructure" and "fetchShakeItMotorsStructure".
/// </summary>
public class FetchShakeItStructureArgs
{
    public string SourceId { get; set; } = string.Empty;
}

/// <summary>
/// Extends <c>HotkeyBaseAction</c> with expressions for the state of the Hotkey, ShakeIt features and a title that can be
/// bound to a SimHub property.
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.hotkey")]
public class HotkeyAction : HotkeyBaseAction<HotkeyActionSettings>
{
    private readonly PropertyComparer _propertyComparer;
    private readonly ShakeItStructureFetcher _shakeItStructureFetcher;
    private ConditionExpression? _conditionExpression;
    private readonly IPropertyChangedReceiver _titlePropertyChangedReceiver;
    private string _titleFormat = "${0}";
    private PropertyChangedArgs? _lastTitlePropertyChangedEvent;
    private readonly FormatHelper _formatHelper = new();

    public HotkeyAction(
        SimHubConnection simHubConnection, PropertyComparer propertyComparer, ShakeItStructureFetcher shakeItStructureFetcher
    ) : base(simHubConnection)
    {
        _propertyComparer = propertyComparer;
        _shakeItStructureFetcher = shakeItStructureFetcher;
        _titlePropertyChangedReceiver = new PropertyChangedDelegate(TitlePropertyChanged);
    }

    /// <summary>
    /// Method to handle the event "fetchShakeItBassStructure" from the Property Inspector. Fetches the ShakeIt Bass structure
    /// from SimHub and sends the result through the event "shakeItBassStructure" back to the Property Inspector.
    /// </summary>
    [PropertyInspectorMethod("fetchShakeItBassStructure")]
    public async Task FetchShakeItBassStructure(FetchShakeItStructureArgs args)
    {
        var profiles = await _shakeItStructureFetcher.FetchBassStructure();
        await SendToPropertyInspectorAsync(new { message = "shakeItBassStructure", profiles, args.SourceId });
    }

    /// <summary>
    /// Method to handle the event "fetchShakeItMotorsStructure" from the Property Inspector. Fetches the ShakeIt Motors structure
    /// from SimHub and sends the result through the event "shakeItMotorsStructure" back to the Property Inspector.
    /// </summary>
    [PropertyInspectorMethod("fetchShakeItMotorsStructure")]
    public async Task FetchShakeItMotorsStructure(FetchShakeItStructureArgs args)
    {
        var profiles = await _shakeItStructureFetcher.FetchMotorsStructure();
        await SendToPropertyInspectorAsync(new { message = "shakeItMotorsStructure", profiles, args.SourceId });
    }

    protected override async Task SetSettings(HotkeyActionSettings ac, bool forceSubscribe)
    {
        var newCondExpr = _propertyComparer.Parse(ac.SimHubProperty);
        // If we have an existing condition and receive a new one, where the PropertyName was not changed (because this change will
        // trigger a unsubscribe+subscribe), but the Operator or the CompareValue has changed, we have to recalculate the
        // state of the action.
        var recalc = _conditionExpression != null &&
                     newCondExpr.Property == _conditionExpression.Property &&
                     (newCondExpr.Operator != _conditionExpression.Operator ||
                      newCondExpr.CompareValue != _conditionExpression.CompareValue);
        _conditionExpression = newCondExpr;
        if (recalc)
        {
            await RefirePropertyChanged();
        }

        var newTitleFormat = _formatHelper.CompleteFormatString(ac.TitleFormat);
        // Redisplay the title if the format for the title has changed.
        var recalcTitle = newTitleFormat != _titleFormat;
        _titleFormat = newTitleFormat;
        if (recalcTitle)
        {
            await RefireTitlePropertyChanged();
        }

        // The field "ac.SimHubProperty" may contain an expression, which is not understood by the base class. So we
        // construct a new instance without expression.
        var acNew = new HotkeyActionSettings()
        {
            Hotkey = ac.Hotkey,
            SimHubControl = ac.SimHubControl,
            SimHubProperty = _conditionExpression.Property,
            Ctrl = ac.Ctrl,
            Alt = ac.Alt,
            Shift = ac.Shift,
            TitleSimHubProperty = ac.TitleSimHubProperty
        };

        // Unsubscribe previous SimHub "Title" property, if it was set and is different than the new one.
        if (!string.IsNullOrEmpty(HotkeySettings.TitleSimHubProperty) && HotkeySettings.TitleSimHubProperty != ac.TitleSimHubProperty)
        {
            await SimHubConnection.Unsubscribe(HotkeySettings.TitleSimHubProperty, _titlePropertyChangedReceiver);
            // In case of the new "Title" property being invalid or empty, we remove the old title value.
            _lastTitlePropertyChangedEvent = null;
            await SetTitleProperty(null);
        }

        // Subscribe SimHub "Title" property, if it is set and different than the previous one.
        if (!string.IsNullOrEmpty(ac.TitleSimHubProperty) && (ac.TitleSimHubProperty != HotkeySettings.TitleSimHubProperty ||
                                                              forceSubscribe))
        {
            await SimHubConnection.Subscribe(ac.TitleSimHubProperty, _titlePropertyChangedReceiver);
        }

        await base.SetSettings(acNew, forceSubscribe);
    }

    protected override async Task Unsubscribe()
    {
        if (!string.IsNullOrEmpty(HotkeySettings.TitleSimHubProperty))
        {
            await SimHubConnection.Unsubscribe(HotkeySettings.TitleSimHubProperty, _titlePropertyChangedReceiver);
        }

        await base.Unsubscribe();
    }

    protected override int ValueToState(PropertyType propertyType, IComparable? propertyValue)
    {
        if (_conditionExpression == null)
        {
            return 0;
        }

        var isActive = _propertyComparer.Evaluate(propertyType, propertyValue, _conditionExpression);
        return isActive ? 1 : 0;
    }

    private async Task TitlePropertyChanged(PropertyChangedArgs args)
    {
        _lastTitlePropertyChangedEvent = args;
        await SetTitleProperty(args.PropertyValue);
    }

    /// <summary>
    /// Refire the last "TitlePropertyChanged" event that was received from SimHub.
    /// </summary>
    private async Task RefireTitlePropertyChanged()
    {
        if (_lastTitlePropertyChangedEvent != null)
        {
            await TitlePropertyChanged(_lastTitlePropertyChangedEvent);
        }
    }

    private async Task SetTitleProperty(IComparable? property)
    {
        if (property == null)
        {
            await SetTitleAsync(string.Empty);
        }
        else
        {
            try
            {
                await SetTitleAsync(string.Format(_titleFormat, property));
            }
            catch (FormatException)
            {
                await SetTitleAsync(property.ToString());
            }
        }
    }
}