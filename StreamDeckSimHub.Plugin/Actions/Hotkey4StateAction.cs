// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Globalization;
using SharpDeck;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// This action sends a key stroke to the active window and it can update its state from a SimHub property.
/// This action supports four states: "0", "1", "2" and "3".
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.hotkey4")]
public class Hotkey4StateAction : HotkeyBaseAction
{
    public Hotkey4StateAction(SimHubConnection simHubConnection) : base(simHubConnection)
    {
    }

    protected override int ValueToState(string propertyType, string? propertyValue)
    {
        // see https://github.com/pre-martin/SimHubPropertyServer/blob/main/Property/SimHubProperty.cs, "TypeToString()"
        switch (propertyType)
        {
            case "boolean":
                return propertyValue == "True" ? 1 : 0;
            case "integer":
            case "long":
                return propertyValue != null ? int.Parse(propertyValue, CultureInfo.InvariantCulture) : 0;
            default:
                return 0;
        }
    }
}