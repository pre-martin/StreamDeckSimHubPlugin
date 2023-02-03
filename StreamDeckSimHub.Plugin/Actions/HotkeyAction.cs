// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SharpDeck;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// This action sends a key stroke to the active window and it can update its state from a SimHub property.
/// This action supports two states: "0" and "1".
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.hotkey")]
public class HotkeyAction : HotkeyBaseAction<HotkeyActionSettings>
{
    private readonly PropertyComparer _propertyComparer;
    private ConditionExpression? _conditionExpression;
    private readonly IPropertyChangedReceiver _titlePropertyChangedReceiver;

    public HotkeyAction(SimHubConnection simHubConnection, PropertyComparer propertyComparer) : base(simHubConnection)
    {
        _propertyComparer = propertyComparer;
        _titlePropertyChangedReceiver =  new TitlePropertyChanged(SetTitleProperty);
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
            RefirePropertyChanged();
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
            SimHubPropertyTitle = ac.SimHubPropertyTitle
        };

        // Unsubscribe previous SimHub "Title" property, if it was set and is different than the new one.
        if (!string.IsNullOrEmpty(HotkeySettings.SimHubPropertyTitle) && HotkeySettings.SimHubPropertyTitle != ac.SimHubPropertyTitle)
        {
            await SimHubConnection.Unsubscribe(HotkeySettings.SimHubPropertyTitle, _titlePropertyChangedReceiver);
        }
        // Subscribe SimHub "Title" property, if it is set and different than the previous one.
        if (!string.IsNullOrEmpty(ac.SimHubPropertyTitle) && (ac.SimHubPropertyTitle != HotkeySettings.SimHubPropertyTitle ||
                                                              forceSubscribe))
        {
            await SimHubConnection.Subscribe(ac.SimHubPropertyTitle, _titlePropertyChangedReceiver);
        }

        await base.SetSettings(acNew, forceSubscribe);
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

    private async Task SetTitleProperty(IComparable? property)
    {
        await SetTitleAsync(property == null ? "" : property.ToString());
    }

    private class TitlePropertyChanged : IPropertyChangedReceiver
    {
        private readonly Func<IComparable?, Task> _action;

        public TitlePropertyChanged(Func<IComparable?, Task> action)
        {
            _action = action;
        }

        public async void PropertyChanged(PropertyChangedArgs args)
        {
            await _action(args.PropertyValue);
        }
    }
}