// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub;

/// <summary>
/// Parses a property line which was received from SimHub Property Server.
/// </summary>
public class PropertyParser
{

    public (string name, PropertyType type, IComparable? value)? ParseLine(string line)
    {
        var lineItems = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (lineItems.Length != 4)
        {
            return null;
        }

        var name = lineItems[1];
        var typeAsString = lineItems[2];
        var valueAsString = lineItems[3];
        if (valueAsString == "(null)") valueAsString = null;

        // See https://github.com/pre-martin/SimHubPropertyServer/blob/main/Property/SimHubProperty.cs
        var type = typeAsString switch
        {
            "boolean" => PropertyType.Boolean,
            "integer" => PropertyType.Integer,
            "long" => PropertyType.Long,
            "double" => PropertyType.Double,
            _ => PropertyType.Double // Should not happen. But best guess should always be "double".
        };
        var value = type.ParseFromSimHub(valueAsString);

        return (name, type, value);
    }
}