// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Globalization;

namespace StreamDeckSimHub.Plugin.SimHub;

/// <summary>
/// Property types that are exposed by the SimHub plugin.
/// </summary>
public enum PropertyType
{
    Boolean,
    Integer,
    Long,
    Double,
    TimeSpan,
    DateTime,
    String,
    Object
}

/// <summary>
/// Extensions for PropertyType.
/// </summary>
public static class PropertyTypeEx
{
    /// Converts a string value into a typed value, which is returned as <c>IComparable</c>. This method works for data
    /// received from SimHub, but also for user supplied values, like compare values.
    public static IComparable? Parse(this PropertyType propertyType, string? propertyValue)
    {
        if (propertyValue == null)
        {
            return null;
        }

        switch (propertyType)
        {
            case PropertyType.Boolean:
            {
                // Try to parse string from "true" or "false".
                var result = bool.TryParse(propertyValue, out var boolResult);
                if (result) return boolResult;
                // If not possible, try to parse as integer value and return "true" if the integer value is > 0.
                result = int.TryParse(propertyValue, out var intResult);
                return result && intResult > 0;
            }
            case PropertyType.Integer:
            {
                // Try to parse as integer value.
                var result = int.TryParse(propertyValue, out var intResult);
                if (result) return intResult;
                // If not possible, try to parse as boolean and return "1" for "true" and "0" for "false".
                result = bool.TryParse(propertyValue, out var boolResult);
                return result && boolResult ? 1 : 0;
            }
            case PropertyType.Long:
            {
                // Try to parse as long value.
                var result = long.TryParse(propertyValue, out var longResult);
                if (result) return longResult;
                // If not possible, try to parse as boolean and return "1" for "true" and "0" for "false".
                result = bool.TryParse(propertyValue, out var boolResult);
                return result && boolResult ? 1L : 0L;
            }
            case PropertyType.Double:
            {
                var result = double.TryParse(propertyValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleResult);
                return result ? doubleResult : 0.0d;
            }
            case PropertyType.TimeSpan:
            {
                var result = TimeSpan.TryParse(propertyValue, CultureInfo.InvariantCulture, out var timeSpanResult);
                return result ? timeSpanResult : null;
            }
            case PropertyType.DateTime:
            {
                var result = DateTime.TryParse(propertyValue, null, DateTimeStyles.RoundtripKind, out var dateTimeResult);
                return result ? dateTimeResult : null;
            }
            case PropertyType.String:
            {
                if (propertyValue.StartsWith("{base64}"))
                {
                    // Decode base64 string.
                    var base64String = propertyValue[8..];
                    var bytes = Convert.FromBase64String(base64String);
                    return System.Text.Encoding.UTF8.GetString(bytes);
                }
                return propertyValue;
            }
            case PropertyType.Object:
            {
                // Try to parse as double.
                var result = double.TryParse(propertyValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleResult);
                if (result) return doubleResult;
                // If not possible, return as string
                return propertyValue;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, "PropertyType parser not implemented for type");
        }
    }
}