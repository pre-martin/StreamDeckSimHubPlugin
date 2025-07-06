// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Numerics;
using NLog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Renderer;

public class ButtonRendererImageSharp(GetPropertyDelegate getProperty) : IButtonRenderer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly StreamDeckKeyInfo DefaultKeyInfo = StreamDeckKeyInfoBuilder.DefaultKeyInfo;

    public string Render(StreamDeckKeyInfo targetKeyInfo, Collection<DisplayItem> displayItems)
    {
        var image = new Image<Rgba32>(targetKeyInfo.KeySize.Width, targetKeyInfo.KeySize.Height);

        // Iterate over all display items.
        foreach (var displayItem in displayItems)
        {
            if (!IsVisible(displayItem))
            {
                Logger.Debug($"Skipping rendering of \"{displayItem.DisplayName}\" due to visibility conditions not met.");
                continue; // Skip to next item
            }

            // Render the item.
            Logger.Debug($"Rendering \"{displayItem.DisplayName}\"...");

            switch (displayItem)
            {
                case DisplayItemImage imageItem:
                    RenderImage(image, targetKeyInfo, imageItem);
                    break;
                case DisplayItemText textItem:
                    RenderText(image, targetKeyInfo, textItem);
                    break;
                case DisplayItemValue valueItem:
                    RenderValue(image, targetKeyInfo, valueItem);
                    break;
                default:
                    Logger.Warn($"Unknown DisplayItem type: {displayItem.GetType().Name}");
                    break;
            }
            //image.SaveAsPng($@"D:\image_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}_{displayItem.DisplayName}.png");
        }

        return image.ToBase64String(PngFormat.Instance);
    }

    /// <summary>
    /// Renders an image display item to the image.
    /// </summary>
    private void RenderImage(Image<Rgba32> image, StreamDeckKeyInfo keyInfo, DisplayItemImage imageItem)
    {
        try
        {
            // Position + Size
            var position = imageItem.DisplayParameters.Position;
            var boundingSize = imageItem.DisplayParameters.Size ?? keyInfo.KeySize;
            var boundingRect = new RectangleF(position.X, position.Y, boundingSize.Width, boundingSize.Height);

            // Center point of the bounding rectangle
            var centerPoint = new PointF(boundingRect.X + boundingRect.Width / 2f, boundingRect.Y + boundingRect.Height / 2f);

            // Get source image directly (it's already an ImageSharp Image object)
            var sourceImage = imageItem.Image;

            // Calculate scaling factor based on ScaleType
            float scaleFactor;
            switch (imageItem.DisplayParameters.Scale)
            {
                case ScaleType.None:
                    scaleFactor = 1.0f;
                    break;
                case ScaleType.ToSize:
                    // Scale to fit the bounding size while maintaining aspect ratio
                    var widthRatio = boundingRect.Width / sourceImage.Width;
                    var heightRatio = boundingRect.Height / sourceImage.Height;
                    scaleFactor = Math.Min(widthRatio, heightRatio);
                    break;
                case ScaleType.ToDevice:
                    // Scale to fit the device key size
                    var deviceWidthRatio = (float)keyInfo.KeySize.Width / sourceImage.Width;
                    var deviceHeightRatio = (float)keyInfo.KeySize.Height / sourceImage.Height;
                    scaleFactor = Math.Min(deviceWidthRatio, deviceHeightRatio);
                    break;
                default:
                    scaleFactor = 1.0f;
                    break;
            }

            // Calculate dimensions based on scale factor
            var scaledWidth = sourceImage.Width * scaleFactor;
            var scaledHeight = sourceImage.Height * scaleFactor;

            // Calculate position to center the image within the bounding box
            var offsetX = (boundingRect.Width - scaledWidth) / 2f;
            var offsetY = (boundingRect.Height - scaledHeight) / 2f;

            // Create a Rectangle for DrawImage (converts from RectangleF to Rectangle)
            var imageRect = new Rectangle(
                (int)(boundingRect.X + offsetX),
                (int)(boundingRect.Y + offsetY),
                (int)scaledWidth,
                (int)scaledHeight
            );

            // Rotation
            Image rotatedImage;
            if (imageItem.DisplayParameters.Rotation == 0)
            {
                rotatedImage = sourceImage;
            }
            else
            {
                rotatedImage = sourceImage.CloneAs<Rgba32>();
                rotatedImage.Mutate(ctx => ctx.Rotate(imageItem.DisplayParameters.Rotation) );
            }

            // Apply transparency
            var opacity = imageItem.DisplayParameters.Transparency;

            image.Mutate(ctx =>
            {
                // Draw the image with transparency
                ctx.DrawImage(rotatedImage, imageRect, opacity);
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error rendering image item \"{imageItem.DisplayName}\"");
        }
    }

    /// <summary>
    /// Renders a text display item to the image.
    /// </summary>
    private void RenderText(Image<Rgba32> image, StreamDeckKeyInfo keyInfo, DisplayItemText textItem)
    {
        if (string.IsNullOrWhiteSpace(textItem.Text)) return;

        try
        {
            // Scale font to the device key size
            var scaledFont = ScaleFont(textItem.Font, keyInfo.KeySize);

            // Color + Transparency
            var color = textItem.Color.WithAlpha(textItem.DisplayParameters.Transparency);

            // Position + Size
            var position = textItem.DisplayParameters.Position;
            var boundingSize = textItem.DisplayParameters.Size ?? keyInfo.KeySize;
            var boundingRect = new RectangleF(position.X, position.Y, boundingSize.Width, boundingSize.Height);

            // Center point of the bounding rectangle
            var centerPoint = new PointF(boundingRect.X + boundingRect.Width / 2f, boundingRect.Y + boundingRect.Height / 2f);

            // Configure text options - set origin to the center point for proper rotation
            var textOptions = new RichTextOptions(scaledFont)
            {
                HorizontalAlignment = HorizontalAlignment.Center, // text box
                VerticalAlignment = VerticalAlignment.Center, // text box
                TextAlignment = TextAlignment.Center, // text alignment within the text box
                WrappingLength = boundingRect.Width,
                WordBreaking = WordBreaking.BreakAll,
                Origin = centerPoint
            };

            // Rotation
            var rotationRadians = textItem.DisplayParameters.Rotation * (float)Math.PI / 180f;

            // Move the center point to origin, apply rotation, then move back to the original center point.
            var transform = Matrix3x2.CreateTranslation(-centerPoint) *
                            Matrix3x2.CreateRotation(rotationRadians) *
                            Matrix3x2.CreateTranslation(centerPoint);

            image.Mutate(ctx =>
            {
                ctx.SetDrawingTransform(transform);
                ctx.DrawText(textOptions, textItem.Text, color);
                ctx.Draw(Color.LightGray, 2f, boundingRect); // Debug: Draw the bounding rectangle
                ctx.Fill(Color.Red, new EllipsePolygon(centerPoint, 3f)); // Debug: Draw center point
                ctx.SetDrawingTransform(Matrix3x2.Identity);
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error rendering text item \"{textItem.DisplayName}\"");
        }
    }

    /// <summary>
    /// Renders a value display item to the image.
    /// </summary>
    private void RenderValue(Image<Rgba32> image, StreamDeckKeyInfo keyInfo, DisplayItemValue valueItem)
    {
        // TODO: Implement value rendering
        // 1. Get the property value using getProperty delegate: var value = getProperty(valueItem.Property)
        // 2. Format the value using valueItem.DisplayFormat if provided
        // 3. Render the formatted value with valueItem.Font and valueItem.Color
        // 4. Apply positioning, transparency, rotation, and scaling from valueItem.DisplayParameters
    }

    /// <summary>
    /// Evaluates the conditions of the item. If the result is true or a positive number, the item is considered visible.
    /// </summary>
    private bool IsVisible(Item item)
    {
        if (item.ConditionsHolder.NCalcExpression == null)
        {
            return true; // No condition means always visible.
        }

        var expression = item.ConditionsHolder.NCalcExpression;

        // TODO We should set the parameters only once, not for each invocation.
        foreach (var usedProperty in item.ConditionsHolder.UsedProperties)
        {
            expression.DynamicParameters[usedProperty] = _ => getProperty.Invoke(usedProperty);
        }

        var result = expression.Evaluate();
        return result is true or > 0;
    }

    /// <summary>
    /// Scale the font size based on the key resolution. So we can use the same font size across different Stream Deck models.
    /// Base size is a standard Stream Deck with 72 x 72 pixels.
    /// </summary>
    private Font ScaleFont(Font font, Size keySize)
    {
        if (keySize.Height != DefaultKeyInfo.KeySize.Height)
        {
            var scaleFactor = keySize.Height / DefaultKeyInfo.KeySize.Height;
            return new Font(font.Family, font.Size * scaleFactor, font.FontStyle());
        }

        return font;
    }
}