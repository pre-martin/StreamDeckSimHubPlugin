// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class DisplayItemImage : DisplayItem
{
    public const string UiName = "Image";

    // Image is being updated centrally by GenericButtonAction from the value of RelativePath.
    public Image Image { get; set; } = ImageUtils.EmptyImage;
    [ObservableProperty] private string _relativePath = string.Empty;

    protected override string RawDisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrWhiteSpace(RelativePath) ? Path.GetFileNameWithoutExtension(RelativePath) : "Image";

    public static DisplayItemImage Create()
    {
        return new DisplayItemImage();
    }

    public override async Task Accept(IDisplayItemVisitor displayItemVisitor, IVisitorArgs? args = null)
    {
        await displayItemVisitor.Visit(this, args);
    }
}