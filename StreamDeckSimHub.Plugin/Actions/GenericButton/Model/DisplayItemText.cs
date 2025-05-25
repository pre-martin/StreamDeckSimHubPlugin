// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class DisplayItemText : DisplayItem
{
    public const string UiName = "Text";

    public required string Text { get; set; }
    public required Font Font { get; set; }
    public required Color Color { get; set; }

    public static DisplayItemText Create()
    {
        return new DisplayItemText
        {
            Name = "",
            DisplayParameters = new DisplayParameters(),
            VisibilityConditions = [],
            Text = "",
            Font = SystemFonts.CreateFont("Arial", 12, FontStyle.Regular),
            Color = Color.White
        };
    }
}