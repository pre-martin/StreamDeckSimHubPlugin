// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class DisplayItemImage : DisplayItem
{
    public const string UiName = "Image";

    public required Image Image { get; set; }
    public required string RelativePath { get; set; } = string.Empty;

    public static DisplayItemImage Create()
    {
        return new DisplayItemImage
        {
            Name = "",
            DisplayParameters = new DisplayParameters(),
            VisibilityConditions = [],
            Image = ImageUtils.EmptyImage,
            RelativePath = string.Empty,
        };
    }
}