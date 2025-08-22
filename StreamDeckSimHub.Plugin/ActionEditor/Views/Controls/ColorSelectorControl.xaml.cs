// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows.Forms;
using System.Windows.Input;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;
using Color = SixLabors.ImageSharp.Color;

namespace StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;

public partial class ColorSelectorControl
{
    public ColorSelectorControl()
    {
        InitializeComponent();
    }

    private void ColorRectangle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not IColorSelectable colorViewModel) return;

        var dlg = new ColorDialog
        {
            AllowFullOpen = true,
            Color = colorViewModel.ColorAsWpf.ToWindowsFormsColor()
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            colorViewModel.ImageSharpColor = Color.FromRgba(dlg.Color.R, dlg.Color.G, dlg.Color.B, dlg.Color.A);
        }
    }
}
