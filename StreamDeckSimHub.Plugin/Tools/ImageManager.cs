// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using NLog;
using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Tools;

public class ImageManager(IFileSystem fileSystem, ImageUtils imageUtils)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string[] SupportedExtensions = [".svg", ".png", ".jpg", ".jpeg", ".gif"];
    private readonly IDirectoryInfo _customImagesDirectory = fileSystem.DirectoryInfo.New(Path.Combine("images", "custom"));

    /// <summary>
    /// Returns an array with all custom images. The images are returned with their relative path inside
    /// the "custom images" directory. Images with quality suffix in their name ("@2x") are excluded from the list.
    /// </summary>
    public string[] ListCustomImages()
    {
        var imageQualityRegex = new Regex(@"@\dx\."); // "image@2x.png", "image@3x.png", ...
        try
        {
            var fn = _customImagesDirectory.FullName;
            return _customImagesDirectory.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(fileInfo => SupportedExtensions.Contains(fileInfo.Extension.ToLowerInvariant()))
                .Select(fileInfo => fileInfo.FullName[(fn.Length + 1)..])
                .Select(fileName => imageQualityRegex.Replace(fileName, "."))
                .Select(fileName => fileName.Replace(@"\", "/"))
                .ToList()
                .Distinct()
                .OrderBy(fileName => fileName.Count(c => c == '/') + fileName)
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
        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        if (Path.GetExtension(relativePath).Equals(".svg", StringComparison.InvariantCultureIgnoreCase))
        {
            // No quality suffix for SVG files.
            var fn = Path.Combine(_customImagesDirectory.FullName, relativePath);
            return imageUtils.FromSvgFile(fn, sdKeyInfo);
        }

        var newName = FindResolutionForKeyInfo(_customImagesDirectory.FullName, relativePath, sdKeyInfo);
        var newFn = Path.Combine(_customImagesDirectory.FullName, newName);
        try
        {
            using var stream = fileSystem.File.OpenRead(newFn);
            return Image.Load(stream);
        }
        catch (Exception e)
        {
            Logger.Warn($"Could not load custom image '{newFn}': {e.Message}");
            return imageUtils.GetErrorImage(sdKeyInfo);
        }
    }

    private string FindResolutionForKeyInfo(string baseDir, string relativePath, StreamDeckKeyInfo sdKeyInfo)
    {
        if (sdKeyInfo.IsHighRes)
        {
            var extension = Path.GetExtension(relativePath);
            var hqFile = relativePath[..relativePath.LastIndexOf('.')] + "@2x" + extension;
            if (fileSystem.File.Exists(Path.Combine(baseDir, hqFile))) return hqFile;
        }

        return relativePath;
    }
}