// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using System.Windows.Media.Imaging;
using NLog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using Svg.Skia;

namespace StreamDeckSimHub.Plugin.Tools;

public class ImageUtils
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Returns a static image without content. Can be used to initialize images before they are actually loaded.
    /// </summary>
    /// <remarks>Do not modify the returned image, because this will affect all instances.</remarks>
    /// <returns>Always a high-res image. Stream Deck can scale it for us, because there will be no scaling losses.</returns>
    public static readonly Image EmptyImage = new Image<Rgba32>(144, 144);

    private readonly Image _errorSmall;
    private readonly Image _errorLarge;

    public ImageUtils()
    {
        _errorSmall = CreateErrorImage(72, 72);
        _errorLarge = CreateErrorImage(144, 144);
    }

    private Image<Rgba32> CreateErrorImage(int width, int height)
    {
        var image = new Image<Rgba32>(width, height);
        var thickness = (float)width / 10;
        image.Mutate(x => x
            .DrawLine(Color.Red, thickness, new PointF(0, 0), new Point(width, height))
            .DrawLine(Color.Red, thickness, new PointF(0, height), new Point(width, 0)));

        return image;
    }

    public Image GetErrorImage(StreamDeckKeyInfo keyInfo)
    {
        return keyInfo.IsHighRes ? _errorLarge : _errorSmall;
    }

    /// <summary>
    /// Loads an SVG file and coverts it into a JPEG file. Internally, we only handle <c>Image</c> instances, that's the
    /// reason why we convert vector to bitmap data.
    /// </summary>
    /// <param name="svgFileName">The path to the image</param>
    /// <param name="sdKeyInfo">The vector data is scaled for the given <c>StreamDeckKeyInfo</c></param>
    /// <returns>A bitmap image. If the SVG cannot be loaded, a static error image is returned.</returns>
    public virtual Image FromSvgFile(string svgFileName, StreamDeckKeyInfo sdKeyInfo)
    {
        try
        {
            using var svg = SKSvg.CreateFromFile(svgFileName);
            var keyWidth = sdKeyInfo.KeySize.Width;
            var keyHeight = sdKeyInfo.KeySize.Height;

            if (svg.Model is null)
            {
                Logger.Warn($"Could not determine model of SVG file '{svgFileName}'. The file seems to be invalid.");
                return GetErrorImage(sdKeyInfo);
            }

            if (svg.Picture is null)
            {
                Logger.Warn($"Could not determine picture of SVG file '{svgFileName}'. The file seems to be invalid.");
                return GetErrorImage(sdKeyInfo);
            }


            var scaleX = keyWidth / svg.Model.CullRect.Width;
            var scaleY = keyHeight / svg.Model.CullRect.Height;

            // Stream Deck always scales to a square. So no need to keep the aspect ratio here - just scale to square.
            //var scale = scaleX > scaleY ? scaleY : scaleX;

            var bitmap = svg.Picture.ToBitmap(SKColor.Empty, scaleX, scaleY, SKColorType.Rgba8888, SKAlphaType.Premul,
                SKColorSpace.CreateSrgb());
            if (bitmap is null)
            {
                // We have a valid SVG file, but no bitmap. This means that the SVG file is empty (empty "svg" element).
                return EmptyImage;
            }

            var data = bitmap.Encode(SKEncodedImageFormat.Png, 90);
            return Image.Load(data.ToArray());
        }
        catch (Exception e)
        {
            Logger.Warn(e, $"Could not read SVG file '{svgFileName}' and convert it into a bitmap");
            return sdKeyInfo.IsHighRes ? _errorLarge : _errorSmall;
        }
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

        return image.ToBase64String(PngFormat.Instance);
    }

    /// <summary>
    /// Converts a SixLabors <c>Image</c> instance into a <c>BitmapImage</c> for WPF usage.
    /// </summary>
    public BitmapImage FromImage(Image image)
    {
        using var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = memoryStream;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        return bitmapImage;
    }
}
