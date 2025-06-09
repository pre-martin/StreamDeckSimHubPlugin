// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class DisplayItemText : DisplayItem
{
    public const string UiName = "Text";

    [ObservableProperty] private string _text = string.Empty;
    [ObservableProperty] private Font _font = SystemFonts.CreateFont("Arial", 12, FontStyle.Regular);
    [ObservableProperty] private Color _color = Color.White;

    public static DisplayItemText Create()
    {
        return new DisplayItemText();
    }
}