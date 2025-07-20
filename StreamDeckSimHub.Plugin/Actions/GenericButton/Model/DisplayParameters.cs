// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class DisplayParameters : ObservableObject
{
    [ObservableProperty] private float _transparency = 1f;
    [ObservableProperty] private Point _position = new(0, 0);
    [ObservableProperty] private Size? _size;
    [ObservableProperty] private ScaleType _scale = ScaleType.None;
    [ObservableProperty] private int _rotation;

    partial void OnTransparencyChanged(float value)
    {
        Transparency = Math.Clamp(value, 0f, 1f);
    }
}
