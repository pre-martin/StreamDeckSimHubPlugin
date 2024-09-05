// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.RegularExpressions;
using NLog;
using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Tools;

public class ImageManager(ImageUtils imageUtils)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly DirectoryInfo CustomImagesDirectory = new(@"images\custom");
    private static readonly string[] SupportedExtensions = [".svg", ".png", ".jpg", ".jpeg", ".gif"];

    /// <summary>
    /// Returns an array with all custom images. The images are returned with their relative path inside
    /// the "custom images" directory. Images with quality suffix in their name ("@2x") are excluded from the list.
    /// </summary>
    public string[] ListCustomImages()
    {
        var imageQualityRegex = new Regex(@"@{\d}x\."); // "image@2x.png", "image@3x.png", ...
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
            return [];
        }
    }

    /// <summary>
    /// Returns an image from the  "custom image" folder.
    /// </summary>
    /// <param name="relativePath">The relative path inside the "custom images" folder</param>
    /// <param name="sdKeyInfo">Information about the specific Stream Deck key or dial</param>
    /// <returns>The image in encoded form, or a default error image, if it could not be loaded.</returns>
    public Image GetCustomImage(string relativePath, StreamDeckKeyInfo sdKeyInfo)
    {
        relativePath = relativePath.Replace('/', '\\');
        if (Path.GetExtension(relativePath).Equals(".svg", StringComparison.InvariantCultureIgnoreCase))
        {
            // No quality suffix for SVG files.
            var fn = Path.Combine(CustomImagesDirectory.FullName, relativePath);
            return imageUtils.FromSvgFile(fn, sdKeyInfo);
        }

        var newName = FindResolutionForKeyInfo(relativePath, sdKeyInfo);
        var newFn = Path.Combine(CustomImagesDirectory.FullName, newName);
        try
        {
            return Image.Load(newFn);
        }
        catch (Exception e)
        {
            Logger.Warn($"Could not load custom image '{newFn}': {e.Message}");
            return imageUtils.GetErrorImage(sdKeyInfo);
        }
    }

    private string FindResolutionForKeyInfo(string relativePath, StreamDeckKeyInfo sdKeyInfo)
    {
        if (sdKeyInfo.IsHighRes)
        {
            var extension = Path.GetExtension(relativePath);
            var hqFile = relativePath[..relativePath.LastIndexOf('.')] + "@2x" + extension;
            if (File.Exists(hqFile)) return hqFile;
        }

        return relativePath;
    }
}