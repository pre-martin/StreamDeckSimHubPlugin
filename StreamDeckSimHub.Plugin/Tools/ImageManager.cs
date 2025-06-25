// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using NLog;
using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Tools;

public partial class ImageManager(IFileSystem fileSystem, ImageUtils imageUtils)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string[] SupportedExtensions = [".svg", ".png", ".jpg", ".jpeg", ".gif"];
    private readonly IDirectoryInfo _customImagesDirectory = fileSystem.DirectoryInfo.New(Path.Combine("images", "custom"));

    /// <summary>
    /// Convenience property to access the ImageUtils instance.
    /// </summary>
    public ImageUtils ImageUtils => imageUtils;

    /// <summary>
    /// Returns an array with all custom images. The images are returned with their relative path inside
    /// the "custom images" directory. Images with quality suffix in their name ("@2x") are excluded from the list.
    /// </summary>
    public string[] ListCustomImages()
    {
        var imageQualityRegex = ImageQualityRegex(); // "image@2x.png", "image@3x.png", ...
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
    /// Returns an array with all subdirectories inside the "custom images" directory.
    /// </summary>
    public string[] ListCustomImagesSubdirectories()
    {
        try
        {
            var fn = _customImagesDirectory.FullName;
            var directories = _customImagesDirectory.GetDirectories("*", new EnumerationOptions { RecurseSubdirectories = true })
                .ToList()
                .Select(directoryInfo => directoryInfo.FullName[(fn.Length + 1)..])
                .Select(fileName => fileName.Replace(@"\", "/"))
                .OrderBy(fileName => fileName)
                .ToList();
            directories.Insert(0, "/"); // Add root directory as first entry
            return directories.ToArray();
        }
        catch (Exception e)
        {
            Logger.Error($"Could not list custom images subdirectories: {e.Message}");
            return [];
        }
    }

    /// <summary>
    /// Returns an array with all custom images in the given subdirectory.
    /// </summary>
    public string[] ListCustomImages(string subdirectory)
    {
        var imageQualityRegex = ImageQualityRegex(); // "image@2x.png", "image@3x.png", ...
        var dir = Path.Combine(_customImagesDirectory.FullName, subdirectory == "/" ? string.Empty : subdirectory);
        var subDirInfo = fileSystem.DirectoryInfo.New(dir);
        if (!subDirInfo.Exists)
        {
            Logger.Warn($"Custom images subdirectory '{subdirectory}' does not exist.");
            return [];
        }

        try
        {
            var fn = _customImagesDirectory.FullName;
            return subDirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                .Where(fileInfo => SupportedExtensions.Contains(fileInfo.Extension.ToLowerInvariant()))
                .Select(fileInfo => fileInfo.FullName[(fn.Length + 1)..])
                .Select(fileName => imageQualityRegex.Replace(fileName, "."))
                .Select(fileName => fileName.Replace(@"\", "/"))
                .ToList()
                .Distinct()
                .OrderBy(fileName => fileName)
                .ToArray();
        }
        catch (Exception e)
        {
            Logger.Error($"Could not list custom images from subdirectory '{subdirectory}': {e.Message}");
            return [];
        }
    }

    /// <summary>
    /// Returns an image from the "custom image" folder.
    /// </summary>
    /// <param name="relativePath">The relative path inside the "custom images" folder</param>
    /// <param name="sdKeyInfo">Information about the specific Stream Deck key or dial</param>
    /// <returns>The image in encoded form, or a default error image, if it could not be loaded.</returns>
    public Image GetCustomImage(string relativePath, StreamDeckKeyInfo sdKeyInfo)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return ImageUtils.EmptyImage;
        }

        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        if (Path.GetExtension(relativePath).Equals(".svg", StringComparison.InvariantCultureIgnoreCase))
        {
            // No quality suffix for SVG files.
            var fn = Path.Combine(_customImagesDirectory.FullName, relativePath);
            return imageUtils.FromSvgFile(fn, sdKeyInfo);
        }

        var newName = FindResolutionForKeyInfo(_customImagesDirectory.FullName, relativePath, sdKeyInfo);
        var newFn = Path.Combine(_customImagesDirectory.FullName, newName);
        if (!fileSystem.File.Exists(newFn))
        {
            Logger.Warn($"Custom image '{newFn}' does not exist.");
            return imageUtils.GetErrorImage(sdKeyInfo);
        }

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

    // "image@2x.png", "image@3x.png", ...
    [GeneratedRegex(@"@\dx\.")]
    private static partial Regex ImageQualityRegex();
}