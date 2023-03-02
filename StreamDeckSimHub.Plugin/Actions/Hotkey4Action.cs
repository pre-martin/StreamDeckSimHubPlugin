// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SharpDeck;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// This action sends a key stroke to the active window and it can update its state from a SimHub property.
/// This action supports four states: "0", "1", "2" and "3".
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.hotkey4")]
public class Hotkey4Action : HotkeyBaseAction<Hotkey4ActionSettings>
{
    public Hotkey4Action(SimHubConnection simHubConnection) : base(simHubConnection)
    {
    }

    protected override int ValueToState(PropertyType propertyType, IComparable? propertyValue)
    {
        switch (propertyType)
        {
            case PropertyType.Boolean:
                return propertyValue == null ? 0 : (bool)propertyValue ? 1 : 0;
            case PropertyType.Integer:
            case PropertyType.Long:
                return propertyValue == null ? 0 : (int)propertyValue;
            case PropertyType.Double:
                // "double" as 4-state? for the moment, simply return 0.
                return 0;
            case PropertyType.Object:
                // "object" as 4-state? for the moment, simply return 0.
                return 0;
            default:
                return 0;
        }
    }
}