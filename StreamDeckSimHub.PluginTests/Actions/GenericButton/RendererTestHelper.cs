// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace StreamDeckSimHub.PluginTests.Actions.GenericButton;

public abstract class RendererTestHelper
{
    public static bool ImageHasNonBlackPixels(Image<Rgba32> image)
    {
        var nonBlackFound = false;
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                foreach (var pixel in row)
                {
                    // Check if pixel is not black (R, G, or B > threshold)
                    if (pixel.R > 10 || pixel.G > 10 || pixel.B > 10)
                    {
                        nonBlackFound = true;
                        return;
                    }
                }
            }
        });

        return nonBlackFound;
    }

    public static int CountNonBlackPixels(Image<Rgba32> image)
    {
        var count = 0;
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                foreach (var pixel in row)
                {
                    // Check if pixel is not black (R, G, or B > threshold)
                    if (pixel.R > 10 || pixel.G > 10 || pixel.B > 10)
                    {
                        count++;
                    }
                }
            }
        });

        return count;
    }
}
