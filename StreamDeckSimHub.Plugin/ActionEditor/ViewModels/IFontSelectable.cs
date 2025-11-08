// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.Fonts;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

/// <summary>
/// Interface for view models that support font selection
/// </summary>
public interface IFontSelectable
{
    /// <summary>
    /// Gets or sets the font
    /// </summary>
    Font Font { get; set; }

    /// <summary>
    /// Gets a string representation of the font
    /// </summary>
    string FontAsString { get; }
}
