// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace StreamDeckSimHub.Plugin.ActionEditor.Tools;

/// <summary>
/// We have quite some different color representations in the code. This class provides some converters.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Returns the hex representation of a SixLabors.ImageSharp.Color without the alpha channel.
    /// </summary>
    public static string ToHexWithoutAlpha(this Color color)
    {
        return color.ToHex()[..6];
    }

    /// <summary>
    /// As we use Windows Forms for the color picker, we need to convert WPF color to System.Drawing.Color (Windows Forms).
    /// </summary>
    public static System.Drawing.Color ToWindowsFormsColor(this System.Windows.Media.Color color)
    {
        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    /// <summary>
    /// System.Drawing.Color (Windows Forms) to WPF color.
    /// </summary>
    public static System.Windows.Media.Color ToWpfColor(this System.Drawing.Color color)
    {
        return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    /// <summary>
    /// ImageSharp Color to WPF color.
    /// </summary>
    public static System.Windows.Media.Color ToWpfColor(this Color color)
    {
        var c = color.ToPixel<Argb32>();
        return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
    }
}

public static class FontExtensions
{
    /// <summary>
    /// Returns the SixLabors.Fonts.FontStyle from the given ImageSharp Font.
    /// </summary>
    public static FontStyle FontStyle(this Font font)
    {
        if (font is { IsBold: true, IsItalic: true }) return SixLabors.Fonts.FontStyle.BoldItalic;
        if (font.IsBold) return SixLabors.Fonts.FontStyle.Bold;
        if (font.IsItalic) return SixLabors.Fonts.FontStyle.Italic;
        return SixLabors.Fonts.FontStyle.Regular;
    }

    /// <summary>
    /// Converts SixLabors.Fonts.Font to System.Drawing.Font (Windows Forms).
    /// </summary>
    public static System.Drawing.Font ToWindowsFormsFont(this Font font)
    {
        var fontStyle = ToWindowsFormsFontStyle(font);
        return new System.Drawing.Font(font.Family.Name, font.Size, fontStyle);
    }

    /// <summary>
    /// Converts the font style of a System.Drawing.Font (Windows Forms) to SixLabors.Fonts.FontStyle.
    /// </summary>
    public static FontStyle ToFontStyle(this System.Drawing.Font font)
    {
        if (font is { Bold: true, Italic: true }) return SixLabors.Fonts.FontStyle.BoldItalic;
        if (font.Bold) return SixLabors.Fonts.FontStyle.Bold;
        if (font.Italic) return SixLabors.Fonts.FontStyle.Italic;
        return SixLabors.Fonts.FontStyle.Regular;
    }

    private static System.Drawing.FontStyle ToWindowsFormsFontStyle(Font font)
    {
        if (font.IsBold) return System.Drawing.FontStyle.Bold;
        if (font.IsItalic) return System.Drawing.FontStyle.Italic;
        return System.Drawing.FontStyle.Regular;
    }
}