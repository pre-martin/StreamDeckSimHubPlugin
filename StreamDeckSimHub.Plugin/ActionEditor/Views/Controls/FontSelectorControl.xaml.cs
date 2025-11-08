// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows.Forms;
using System.Windows.Input;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;
using SystemFonts = SixLabors.Fonts.SystemFonts;

namespace StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;

public partial class FontSelectorControl
{
    public FontSelectorControl()
    {
        InitializeComponent();
    }

    private void FontLabel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not IFontSelectable fontViewModel) return;

        var fontDialog = new FontDialog
        {
            Font = fontViewModel.Font.ToWindowsFormsFont(),
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
                fontViewModel.Font = font;
            }
        }
    }
}
