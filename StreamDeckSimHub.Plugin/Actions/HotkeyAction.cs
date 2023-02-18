// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SharpDeck;
using SharpDeck.PropertyInspectors;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Arguments sent from the Property Inspector for the event "fetchShakeItBassStructure".
/// </summary>
public class FetchShakeItBassStructureArgs
{
    public string SourceId { get; set; } = string.Empty;
}

/// <summary>
/// This action sends a key stroke to the active window and it can update its state from a SimHub property.
/// This action supports two states: "0" and "1".
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

    public HotkeyAction(
        SimHubConnection simHubConnection, PropertyComparer propertyComparer, ShakeItStructureFetcher shakeItStructureFetcher
    ) : base(simHubConnection)
    {
        _propertyComparer = propertyComparer;
        _shakeItStructureFetcher = shakeItStructureFetcher;
        _titlePropertyChangedReceiver = new TitlePropertyChangedReceiver(TitlePropertyChanged);
    }

    /// <summary>
    /// Method to handle the event "lookupSimHubProperties" from the Property Inspector. Fetches the ShakeIt Bass structure
    /// from SimHub and sends the result through the event "shakeItBassStructure" back to the Property Inspector.
    /// </summary>
    [PropertyInspectorMethod("fetchShakeItBassStructure")]
    public async Task FetchShakeItBassStructure(FetchShakeItBassStructureArgs args)
    {
        var profiles = await _shakeItStructureFetcher.FetchStructure();
        await SendToPropertyInspectorAsync(new { message = "shakeItBassStructure", profiles, args.SourceId });
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

        string newTitleFormat;
        if (string.IsNullOrEmpty(ac.TitleFormat)) newTitleFormat = "{0}";
        else newTitleFormat = ac.TitleFormat.IndexOf(':') == 0 ? $"{{0{ac.TitleFormat}}}" : $"{{0,{ac.TitleFormat}}}";
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

    private class TitlePropertyChangedReceiver : IPropertyChangedReceiver
    {
        private readonly Func<PropertyChangedArgs, Task> _action;

        public TitlePropertyChangedReceiver(Func<PropertyChangedArgs, Task> action)
        {
            _action = action;
        }

        public async Task PropertyChanged(PropertyChangedArgs args)
        {
            await _action(args);
        }
    }
}