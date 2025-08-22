// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

/// <summary>
/// Interface for view models that support color selection
/// </summary>
public interface IColorSelectable
{
    /// <summary>
    /// Gets or sets the ImageSharp color
    /// </summary>
    Color ImageSharpColor { get; set; }

    /// <summary>
    /// Gets the color as a hex string
    /// </summary>
    string ColorHex { get; }

    /// <summary>
    /// Gets the color as a WPF color
    /// </summary>
    System.Windows.Media.Color ColorAsWpf { get; }
}