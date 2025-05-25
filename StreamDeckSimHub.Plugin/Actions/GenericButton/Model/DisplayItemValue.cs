// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class DisplayItemValue : DisplayItem
{
    public const string UiName = "Value";

    public required string Property { get; set; }
    public required string DisplayFormat { get; set; }
    public required Font Font { get; set; }
    public required Color Color { get; set; }

    public static DisplayItemValue Create()
    {
        return new DisplayItemValue
        {
            Name = "",
            DisplayParameters = new DisplayParameters(),
            VisibilityConditions = [],
            Property = "",
            DisplayFormat = "",
            Font = SystemFonts.CreateFont("Arial", 12, FontStyle.Regular),
            Color = Color.White
        };
    }
}

