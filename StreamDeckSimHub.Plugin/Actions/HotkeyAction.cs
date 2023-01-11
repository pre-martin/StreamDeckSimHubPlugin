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
public class HotkeyAction : HotkeyBaseAction
{
    private readonly PropertyComparer _propertyComparer;
    private ConditionExpression? _conditionExpression;

    public HotkeyAction(SimHubConnection simHubConnection, PropertyComparer propertyComparer) : base(simHubConnection)
    {
        _propertyComparer = propertyComparer;
    }

    protected override async Task SetSettings(HotkeySettings ac, bool forceSubscribe)
    {
        _conditionExpression = _propertyComparer.Parse(ac.SimHubProperty);
        // The field "ac.SimHubProperty" may contain an expression, which is not understood by the base class. So we
        // construct a new instance without expression.
        var acNew = new HotkeySettings()
        {
            Hotkey = ac.Hotkey,
            SimHubProperty = _conditionExpression.Property,
            SimHubControl = ac.SimHubControl,
            Ctrl = ac.Ctrl,
            Alt = ac.Alt,
            Shift = ac.Shift
        };
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
}