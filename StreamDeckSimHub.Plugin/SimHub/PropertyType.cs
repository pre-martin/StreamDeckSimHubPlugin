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
    Double
}

/// <summary>
/// Extensions for PropertyType.
/// </summary>
public static class PropertyTypeEx
{
    /// <summary>
    /// Converts a string value into a typed value, which is returned as <c>IComparable</c>. The method assumes that the
    /// string value was received from SimHub Property Plugin.
    /// </summary>
    public static IComparable? ParseFromSimHub(this PropertyType propertyType, string? propertyValue)
    {
        if (propertyValue == null)
        {
            return null;
        }

        switch (propertyType)
        {
            case PropertyType.Boolean:
            {
                var result = bool.TryParse(propertyValue, out var boolResult);
                return result ? boolResult : false;
            }
            case PropertyType.Integer:
            {
                var result = int.TryParse(propertyValue, out var intResult);
                return result ? intResult : 0;
            }
            case PropertyType.Long:
            {
                var result = long.TryParse(propertyValue, out var longResult);
                return result ? longResult : 0L;
            }
            case PropertyType.Double:
            {
                var result = double.TryParse(propertyValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleResult);
                return result ? doubleResult : 0.0d;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
        }
    }

    /// Converts a string value into a typed value, which is returned as <c>IComparable</c>. The method is more liberal
    /// than <c>ParseFromSimHub</c> and accepts a wider range of property values.
    /// <remarks>
    /// This method should be used to parse user input.
    /// </remarks>
    public static IComparable? ParseLiberally(this PropertyType propertyType, string? propertyValue)
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
            default:
                throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
        }
    }
}