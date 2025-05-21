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
        var imageUtils = Mock.Of<ImageUtils>();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Combine("images", "custom", "test.svg"), new MockFileData(string.Empty) },
            { Path.Combine("images", "custom", "subDir", "testSub.svg"), new MockFileData(string.Empty) },
            { Path.Combine("images", "custom", "image1.png"), new MockFileData(string.Empty) },
            { Path.Combine("images", "custom", "image1@2x.png"), new MockFileData(string.Empty) },
            { Path.Combine("images", "custom", "img@2x.png"), new MockFileData(string.Empty) },
        });

        var imageManager = new ImageManager(fileSystem, imageUtils);
        var customImages = imageManager.ListCustomImages();

        Assert.That(customImages.Count, Is.EqualTo(4));
        Assert.That(customImages[0], Is.EqualTo("image1.png"));
        Assert.That(customImages[1], Is.EqualTo("img.png"));
        Assert.That(customImages[2], Is.EqualTo("test.svg"));
        Assert.That(customImages[3], Is.EqualTo("subDir/testSub.svg"));
    }

    [Test]
    public void TestGetCustomImageSvg()
    {
        var imageUtils = new Mock<ImageUtils>();
        imageUtils
            .Setup(iu => iu.FromSvgFile(It.IsAny<string>(), It.IsAny<StreamDeckKeyInfo>()))
            .Returns((string _, StreamDeckKeyInfo sdki) => new Image<Rgba32>(sdki.KeySize.Width, sdki.KeySize.Height));
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
        imageUtils.Verify(iu => iu.FromSvgFile(Path.Combine(customImages, "sub", "test.svg"), sdXl));
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