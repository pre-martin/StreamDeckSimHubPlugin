// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class DisplayItemText : DisplayItem, IAcceptsModifierColor, IAcceptsModifierFlash
{
    public const string UiName = "Text";
    public const string UiIcon = "DiTextFieldsGray";

    [ObservableProperty] private string _text = string.Empty;
    [ObservableProperty] private Font _font = SystemFonts.CreateFont("Arial", 16, FontStyle.Regular);
    [ObservableProperty] private Color _color = Color.White;

    protected override string RawDisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrWhiteSpace(Text) ? Text : "Text";

    public static DisplayItemText Create()
    {
        return new DisplayItemText();
    }

    public override async Task Accept(IDisplayItemVisitor displayItemVisitor, IVisitorArgs? args = null)
    {
        await displayItemVisitor.Visit(this, args);
    }
}