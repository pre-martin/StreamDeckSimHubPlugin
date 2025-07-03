// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows.Forms;
using System.Windows.Input;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

namespace StreamDeckSimHub.Plugin.ActionEditor.Views;

public partial class DisplayItemValueView
{
    public DisplayItemValueView()
    {
        InitializeComponent();
    }

    private void FontTextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not DisplayItemValueViewModel vm) return;

        var fontDialog = new FontDialog
        {
            Font = vm.Font.ToWindowsFormsFont(),
            FontMustExist = true
        };

        if (fontDialog.ShowDialog() == DialogResult.OK)
        {
            var usedFont = fontDialog.Font;

            if (SystemFonts.TryGet(usedFont.Name, out var fontFamily))
            {
                // TODO Strikethrough and Underline
                var fontStyle = usedFont.ToFontStyle();
                var font = fontFamily.CreateFont(usedFont.Size, fontStyle);
                vm.Font = font;
            }
        }
    }

    private void ColorRectangle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not DisplayItemValueViewModel vm) return;

        var dlg = new ColorDialog
        {
            AllowFullOpen = true,
            Color = vm.ColorAsWpf.ToWindowsFormsColor()
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            vm.ImageSharpColor = Color.FromRgba(dlg.Color.R, dlg.Color.G, dlg.Color.B, dlg.Color.A);
        }
    }
}