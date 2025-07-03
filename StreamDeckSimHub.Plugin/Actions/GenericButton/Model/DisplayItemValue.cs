// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class DisplayItemValue : DisplayItem
{
    public const string UiName = "Value";

    [ObservableProperty] private string _property = string.Empty;
    [ObservableProperty] private string _displayFormat = string.Empty;
    [ObservableProperty] private Font _font = SystemFonts.CreateFont("Arial", 20, FontStyle.Regular);
    [ObservableProperty] private Color _color = Color.White;

    public static DisplayItemValue Create()
    {
        return new DisplayItemValue();
    }
}

