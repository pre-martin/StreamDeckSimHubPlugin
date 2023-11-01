// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.RegularExpressions;
using NLog;
using SkiaSharp;

namespace StreamDeckSimHub.Plugin.Tools;

public class ImageUtils
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Regex _lineBreakRegex = new("[\r\n]+");

    /// <summary>
    /// Loads a given SVG file from the file system and removes all line breaks. This is important so that
    /// Stream Deck can handle the SVG image.
    /// </summary>
    public string LoadSvg(string svgFile)
    {
        try
        {
            var svgAsText = File.ReadAllText(svgFile);
            return EncodeSvg(_lineBreakRegex.Replace(svgAsText, ""));
        }
        catch (Exception e)
        {
            Logger.Warn($"Could not find file '{svgFile}' : {e.Message}");
            return EncodeSvg("<svg viewBox=\"0 0 70 70\"><rect x=\"20\" y=\"20\" width=\"30\" height=\"30\" stroke=\"red\" fill=\"red\" /></svg>");
        }
    }

    /// <summary>
    /// Prepends the mime type to a given SVG image. Stream Deck expects it like that.
    /// </summary>
    public string EncodeSvg(string svg)
    {
        return "data:image/svg+xml;charset=utf8," + svg;
    }

    /// <summary>
    /// Prepends the mime type to a given PNG image. Stream Deck expects it like that.
    /// </summary>
    public string EncodePng(byte[] png)
    {
        return "data:image/png;base64," + Convert.ToBase64String(png);
    }

    /// <summary>
    /// Generates an encoded PNG image which displays the title. Can be used on the round icons in the Stream Deck application.
    /// </summary>
    public string GenerateDialImage(string title)
    {
        var imageInfo = new SKImageInfo(72, 72);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;

        using var paint = new SKPaint();
        paint.Color = SKColors.White;
        paint.TextSize = 20f;
        paint.IsAntialias = true;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        paint.TextAlign = SKTextAlign.Center;

        canvas.DrawText(title, imageInfo.Width / 2f, 42f, paint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);

        return EncodePng(data.ToArray());
    }
}
