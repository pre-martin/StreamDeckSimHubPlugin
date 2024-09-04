// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.RegularExpressions;
using NLog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
    /// Converts the given Image into the PNG format and prepends the mime type. Stream Deck expects it like that.
    /// </summary>
    private string EncodePng(Image image)
    {
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
    }

    /// <summary>
    /// Generates an encoded PNG image which displays the title. Can be used on the round icons in the Stream Deck application.
    /// </summary>
    public string GenerateDialImage(string title)
    {
        using var image = new Image<Rgba32>(72, 72);
        var font = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);
        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(36, 36), HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        image.Mutate(x => x.DrawText(textOptions, title, Color.White));

        return EncodePng(image);
    }
}
