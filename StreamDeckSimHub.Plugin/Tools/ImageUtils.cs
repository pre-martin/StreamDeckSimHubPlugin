// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.RegularExpressions;
using NLog;

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
}
