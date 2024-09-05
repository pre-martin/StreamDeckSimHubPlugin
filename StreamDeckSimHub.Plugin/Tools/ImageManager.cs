// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.RegularExpressions;
using NLog;
using SharpDeck.Enums;
using SharpDeck.Events.Received;
using SixLabors.ImageSharp;
using SkiaSharp;
using Svg.Skia;
using Size = SharpDeck.Events.Received.Size;

namespace StreamDeckSimHub.Plugin.Tools;

public class ImageManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly IdentifiableDeviceInfo DefaultDevice = new()
    {
        Id = "dummy", Name = "Stream Deck", Size = new Size() { Columns = 5, Rows = 3 }, Type = DeviceType.StreamDeck,
    };
    private static readonly DirectoryInfo CustomImagesDirectory = new(@"images\custom");
    private static readonly string[] SupportedExtensions = { ".svg", ".png", ".jpg", ".jpeg", ".gif" };
    private readonly ImageUtils _imageUtils;

    // w/o suffix: 72x72   Stream Deck, Stream Deck Mini
    // suffix @2 : 144x144 Stream Deck XL, Stream Deck Plus
    // suffix @+ : 200x100 Stream Deck Plus Encoder

    public ImageManager(ImageUtils imageUtils)
    {
        _imageUtils = imageUtils;
    }

    /// <summary>
    /// Returns an array with all custom images. The images are returned with their relative path inside
    /// the "custom images" directory. Images with quality suffix in their name ("@2") are excluded from the list.
    /// </summary>
    public string[] ListCustomImages()
    {
        var imageQualityRegex = new Regex(@"@{\d}\."); // "image@2.png", "image@3.png", ...
        try
        {
            var fn = CustomImagesDirectory.FullName;
            return CustomImagesDirectory.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(fileInfo => SupportedExtensions.Contains(fileInfo.Extension.ToLower()))
                .Where(fileInfo => !imageQualityRegex.IsMatch(fileInfo.Name))
                .Select(fileInfo => fileInfo.FullName[(fn.Length + 1)..])
                .Select(fileName => fileName.Replace(@"\", "/"))
                .ToArray();
        }
        catch (Exception e)
        {
            Logger.Error($"Could not load custom images: {e.Message}");
            return Array.Empty<string>();
        }
    }

    public string GetCustomImageEncoded(string relativePath, DeviceInfo? deviceInfo)
    {
        if (relativePath.ToLowerInvariant().EndsWith(".gif"))
        {
            var fn = CustomImagesDirectory.FullName + '/' + relativePath;
            var bytes = File.ReadAllBytes(fn);
            return "data:image/gif;base64," + Convert.ToBase64String(bytes);
        }

        var image = GetCustomImage(relativePath, deviceInfo);
        var stream = new MemoryStream();
        //image.Save(stream, new GifEncoder());
        image.SaveAsGif(stream);
        File.WriteAllBytes(CustomImagesDirectory.FullName + @"\new.gif", stream.ToArray());
        return _imageUtils.EncodeGif(stream.ToArray());
    }

    private Image GetCustomImage(string relativePath, DeviceInfo? deviceInfo)
    {
        if (Path.GetExtension(relativePath).ToLowerInvariant() == ".svg")
        {
            var fn = CustomImagesDirectory.FullName + '/' + relativePath;
            if (!File.Exists(fn)) return null;
            var image = Image.Load(fn);
            return image;
        }

        if (Path.GetExtension(relativePath).ToLowerInvariant() == ".gif")
        {
            var fn = DetermineImageName(CustomImagesDirectory.FullName + '/' + relativePath, deviceInfo);
            if (!File.Exists(fn)) return null;
            var image = Image.Load(fn);
            return image;
        }

        return null;
    }

    public SKBitmap GetCustomImageSkia(string relativePath, DeviceInfo? deviceInfo)
    {
        // TODO Cache!!!

        if (Path.GetExtension(relativePath).ToLowerInvariant() == ".svg")
        {
            // SVG does not require any high-res suffix handling.
            var fn = CustomImagesDirectory.FullName + '/' + relativePath;
            if (!File.Exists(fn)) return CreateErrorImage();

            using var svg = SKSvg.CreateFromFile(fn);
            var bitmap = svg.Picture?.ToBitmap(SKColor.Empty, 1f, 1f, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            return bitmap ?? CreateErrorImage();

        } else if (Path.GetExtension(relativePath).ToLowerInvariant() == ".gif")
        {
            var fn = DetermineImageName(CustomImagesDirectory.FullName + '/' + relativePath, deviceInfo);
            if (!File.Exists(fn)) return CreateErrorImage();

            using var codec = SKCodec.Create(fn);
            if (codec == null) return CreateErrorImage();
            var frameCount = codec.FrameCount;
            if (frameCount == 1)
            {
                // GIF without animation.
                return SKBitmap.Decode(codec);
            }
        }
        var fileName = DetermineImageName(relativePath + '/' + relativePath, deviceInfo);
        if (fileName == null)
        {
            fileName = "images/icons/undefined.svg";
            Logger.Warn($"Could not find custom image: {relativePath}. Using default image {fileName}");
        }

        try
        {
            var fileNameLower = fileName.ToLowerInvariant();
            if (fileNameLower.EndsWith(".svg"))
            {
                return null;
            }
            var bytes = File.ReadAllBytes(fileName);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Could not load custom image: {fileName}");
        }

        return null;
    }

    private SKBitmap CreateErrorImage()
    {
        using var errorSvg = SKSvg.CreateFromSvg(ImageUtils.ErrorSvg);
        return errorSvg.Picture!.ToBitmap(SKColor.Empty, 1f, 1f, SKColorType.Alpha8, SKAlphaType.Premul, SKColorSpace.CreateSrgb())!;
    }

    private string DetermineImageName(string imagePath, DeviceInfo? deviceInfo)
    {
        if (deviceInfo?.Type is DeviceType.StreamDeckXL or DeviceType.StreamDeckPlus)
        {
            var extension = Path.GetExtension(imagePath);
            var basePath = imagePath[..^extension.Length];
            if (File.Exists(basePath + "@2" + extension))
            {
                return basePath + "@2" + extension;
            }
        }

        return imagePath;
    }
}