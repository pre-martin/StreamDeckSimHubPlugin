// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class DisplayItemImage : DisplayItem
{
    public const string UiName = "Image";

    [ObservableProperty] private Image _image = ImageUtils.EmptyImage;
    public string RelativePath { get; set; } = string.Empty;

    public static DisplayItemImage Create()
    {
        return new DisplayItemImage();
    }
}