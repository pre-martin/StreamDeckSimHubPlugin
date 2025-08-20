// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO.Abstractions.TestingHelpers;
using SharpDeck.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.PluginTests.Tools;

public class ImageManagerTests
{
    [Test]
    public void TestListCustomImages()
    {
        var emptyFileData = new MockFileData(string.Empty);
        var baseDir = Path.Combine("images", "custom");

        var imageUtils = Mock.Of<ImageUtils>();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Combine(baseDir, "test.svg"), emptyFileData },
            { Path.Combine(baseDir, "subDir", "testSub.svg"), emptyFileData },
            { Path.Combine(baseDir, "subDir", "subSub", "testSub.svg"), emptyFileData },
            { Path.Combine(baseDir, "image1.png"), emptyFileData },
            { Path.Combine(baseDir, "image1@2x.png"), emptyFileData },
            { Path.Combine(baseDir, "img@2x.png"), emptyFileData },
        });

        var imageManager = new ImageManager(fileSystem, imageUtils);
        var customImages = imageManager.ListCustomImages();

        Assert.That(customImages.Count, Is.EqualTo(5));
        Assert.That(customImages[0], Is.EqualTo("image1.png"));
        Assert.That(customImages[1], Is.EqualTo("img.png"));
        Assert.That(customImages[2], Is.EqualTo("test.svg"));
        Assert.That(customImages[3], Is.EqualTo("subDir/testSub.svg"));
        Assert.That(customImages[4], Is.EqualTo("subDir/subSub/testSub.svg"));
    }

    [Test]
    public void TestListCustomImagesSubdirectories()
    {
        var emptyFileData = new MockFileData(string.Empty);
        var baseDir = Path.Combine("images", "custom");

        var imageUtils = Mock.Of<ImageUtils>();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Combine(baseDir, "test.svg"), emptyFileData },
            { Path.Combine(baseDir, "subDir", "testSub.svg"), emptyFileData },
            { Path.Combine(baseDir, "otherDir", "image1.png"), emptyFileData },
            { Path.Combine(baseDir, "subDir", "subSubDir2", "someFile.png"), emptyFileData },
            { Path.Combine(baseDir, "subDir", "subSubDir1", "icon.png"), emptyFileData },
            { Path.Combine(baseDir, "otherDir", "sub", "image1.png"), emptyFileData },
        });

        var imageManager = new ImageManager(fileSystem, imageUtils);
        var directories = imageManager.ListCustomImagesSubdirectories();

        Assert.That(directories.Count, Is.EqualTo(6));
        Assert.That(directories[0], Is.EqualTo("/"));
        Assert.That(directories[1], Is.EqualTo("otherDir"));
        Assert.That(directories[2], Is.EqualTo("otherDir/sub"));
        Assert.That(directories[3], Is.EqualTo("subDir"));
        Assert.That(directories[4], Is.EqualTo("subDir/subSubDir1"));
        Assert.That(directories[5], Is.EqualTo("subDir/subSubDir2"));
    }

    [Test]
    public void TestGetCustomImageSvg()
    {
        var imageUtils = new Mock<ImageUtils>();
        imageUtils
            .Setup(iu => iu.FromSvgFile(It.IsAny<string>(), It.IsAny<StreamDeckKeyInfo>(), It.IsAny<bool>()))
            .Returns((string _, StreamDeckKeyInfo sdki, bool _) => new Image<Rgba32>(sdki.KeySize.Width, sdki.KeySize.Height));
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @"images\custom\sub\test@2x.svg", new MockFileData(string.Empty) },
            { @"images\custom\sub\test.svg", new MockFileData(string.Empty) },
        });

        var imageManager = new ImageManager(fileSystem, imageUtils.Object);
        var sdXl = new StreamDeckKeyInfo(DeviceType.StreamDeckXL, false, new Size(144, 144), true);
        using var image = imageManager.GetCustomImage("sub/test.svg", sdXl);

        // ImageUtils must have been called - especially not with "@2x" even for the Hires XL, because SVGs have no suffix.
        var customImages = fileSystem.DirectoryInfo.New(Path.Combine("images", "custom")).FullName;
        imageUtils.Verify(iu => iu.FromSvgFile(Path.Combine(customImages, "sub", "test.svg"), sdXl, false));
        Assert.That(image, Is.Not.Null);
    }

    [Test]
    public void TestGetCustomImageBitmap()
    {
        var imageUtils = new Mock<ImageUtils>();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @"images\custom\sub\test@2x.png", new MockFileData(CreateTestImage(144, 144)) },
            { @"images\custom\sub\test.png", new MockFileData(CreateTestImage(72, 72)) },
        });

        var imageManager = new ImageManager(fileSystem, imageUtils.Object);

        // Test with SD
        var sd = new StreamDeckKeyInfo(DeviceType.StreamDeck, false, new Size(72, 72), false);
        using var image = imageManager.GetCustomImage("sub/test.png", sd);

        // Get lo-res image for SD
        Assert.That(image, Is.Not.Null);
        Assert.That(image.Width, Is.EqualTo(72));
        Assert.That(image.Height, Is.EqualTo(72));

        // Test with SD XL
        var sdXl = new StreamDeckKeyInfo(DeviceType.StreamDeckXL, false, new Size(144, 144), true);
        using var imageHires = imageManager.GetCustomImage("sub/test.png", sdXl);

        // Get hi-res image for SD XL
        Assert.That(imageHires, Is.Not.Null);
        Assert.That(imageHires.Width, Is.EqualTo(144));
        Assert.That(imageHires.Height, Is.EqualTo(144));
    }

    private byte[] CreateTestImage(int width, int height)
    {
        using var testImage = new Image<Rgba32>(width, height);
        using var testImageStream = new MemoryStream();
        testImage.SaveAsPng(testImageStream);
        return testImageStream.ToArray();
    }
}