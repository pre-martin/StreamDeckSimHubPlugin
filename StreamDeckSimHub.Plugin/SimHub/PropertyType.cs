// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

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
                var result = double.TryParse(propertyValue, out var doubleResult);
                return result ? doubleResult : 0.0d;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
        }
    }

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
                var result = bool.TryParse(propertyValue, out var boolResult);
                if (result)
                {
                    return boolResult;
                }
                result = int.TryParse(propertyValue, out var intResult);
                return result && (intResult > 0);
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
                var result = double.TryParse(propertyValue, out var doubleResult);
                return result ? doubleResult : 0.0d;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
        }
    }
}