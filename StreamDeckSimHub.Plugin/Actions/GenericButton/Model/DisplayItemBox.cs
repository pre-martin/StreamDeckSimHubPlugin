// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class DisplayItemBox : DisplayItem, IAcceptsModifierBlink, IAcceptsModifierColor
{
    public const string UiName = "Box";
    public const string UiIcon = "DiEmptyBoxGray";

    [ObservableProperty] private Color _color = Color.White;

    protected override string RawDisplayName => !string.IsNullOrEmpty(Name) ? Name : UiName;

    public static DisplayItemBox Create()
    {
        return new DisplayItemBox();
    }

    public override async Task Accept(IDisplayItemVisitor displayItemVisitor, IVisitorArgs? args = null)
    {
        await displayItemVisitor.Visit(this, args);
    }
}