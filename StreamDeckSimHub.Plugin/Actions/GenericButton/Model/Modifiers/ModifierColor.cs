// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;

public partial class ModifierColor : Modifier
{
    public const string UiName = "Color";

    public static ModifierColor Create()
    {
        return new ModifierColor();
    }

    [ObservableProperty] private Color _color = Color.White;

    public override string DisplayName => UiName;
}