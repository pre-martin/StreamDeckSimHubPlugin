// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;

namespace StreamDeckSimHub.Plugin.Actions.JsonSettings;

/// <summary>
/// JSON-serializable representation of a Point.
/// </summary>
public class PointDto
{
    /// <summary>
    /// The X coordinate.
    /// </summary>
    [JsonPropertyName("x")]
    public int X { get; set; } = 0;

    /// <summary>
    /// The Y coordinate.
    /// </summary>
    [JsonPropertyName("y")]
    public int Y { get; set; } = 0;
}

/// <summary>
/// JSON-serializable representation of a Size.
/// </summary>
public class SizeDto
{
    /// <summary>
    /// The width dimension.
    /// </summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>
    /// The height dimension.
    /// </summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }
}