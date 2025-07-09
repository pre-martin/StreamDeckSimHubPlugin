// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using NLog;
using SharpDeck.Events.Received;
using SixLabors.ImageSharp.PixelFormats;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Tools;
using Size = SixLabors.ImageSharp.Size;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Renderer;

public class ButtonRendererGdi(GetPropertyDelegate getProperty) : IButtonRenderer
{
    private Coordinates _coords = new() { Column = -1, Row = -1 };
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly StreamDeckKeyInfo DefaultKeyInfo = StreamDeckKeyInfoBuilder.DefaultKeyInfo;

    public void SetCoordinates(Coordinates coordinates)
    {
        _coords = coordinates;
    }

    public string Render(StreamDeckKeyInfo targetKeyInfo, Collection<DisplayItem> displayItems)
    {
        using var renderBitmap = new Bitmap(targetKeyInfo.KeySize.Width, targetKeyInfo.KeySize.Height);
        using var renderGraphics = Graphics.FromImage(renderBitmap);

        // Set graphics quality settings
        renderGraphics.SmoothingMode = SmoothingMode.AntiAlias;
        renderGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        renderGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // Iterate over all display items.
        foreach (var displayItem in displayItems)
        {
            if (!IsVisible(displayItem))
            {
                Logger.Debug($"({_coords}) Skipping rendering of \"{displayItem.DisplayName}\" due to visibility conditions not met.");
                continue; // Skip to next item
            }

            // Render the item.
            Logger.Debug($"({_coords}) Rendering \"{displayItem.DisplayName}\"...");

            switch (displayItem)
            {
                case DisplayItemImage imageItem:
                    RenderImage(renderGraphics, targetKeyInfo, imageItem);
                    break;
                case DisplayItemText textItem:
                    RenderText(renderGraphics, targetKeyInfo, textItem);
                    break;
                case DisplayItemValue valueItem:
                    RenderValue(renderGraphics, targetKeyInfo, valueItem);
                    break;
                default:
                    Logger.Warn($"({_coords}) Unknown DisplayItem type: {displayItem.GetType().Name}");
                    break;
            }
        }

        // Convert bitmap to base64 string
        using var memoryStream = new MemoryStream();
        renderBitmap.Save(memoryStream, ImageFormat.Png);
        var imageBytes = memoryStream.ToArray();
        return Convert.ToBase64String(imageBytes);
    }

    /// <summary>
    /// Renders an image display item to the image.
    /// </summary>
    private void RenderImage(Graphics renderGraphics, StreamDeckKeyInfo keyInfo, DisplayItemImage imageItem)
    {
        // TODO: Implement image rendering
        // Use imageItem.Image
        // Apply positioning, transparency, rotation, and scaling from imageItem.DisplayParameters
    }

    /// <summary>
    /// Renders a text display item to the image.
    /// </summary>
    private void RenderText(Graphics renderGraphics, StreamDeckKeyInfo keyInfo, DisplayItemText textItem)
    {
        if (string.IsNullOrWhiteSpace(textItem.Text)) return;

        try
        {
            // Scale font to the device key size.
            var scaledFont = ScaleFont(textItem.Font.ToWindowsFormsFont(), keyInfo.KeySize);

            // Color + Transparency
            var c = textItem.Color.ToPixel<Argb32>();
            var color = Color.FromArgb((int)(textItem.DisplayParameters.Transparency * 255f), c.R, c.G, c.B);

            // Position + Size
            var position = textItem.DisplayParameters.Position;
            var boundingSize = textItem.DisplayParameters.Size ?? keyInfo.KeySize;
            var boundingRect = new RectangleF(position.X, position.Y, boundingSize.Width, boundingSize.Height);

            // Center point of the bounding rectangle
            var centerPoint = new PointF(boundingRect.X + boundingRect.Width / 2f, boundingRect.Y + boundingRect.Height / 2f);

            // Create a StringFormat for text alignment
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.Word,
                FormatFlags = StringFormatFlags.NoWrap
            };

            // Rotation
            var rotationDegrees = textItem.DisplayParameters.Rotation;

            // Save the current state of the graphics object
            var state = renderGraphics.Save();

            try
            {
                // Set up the transform for rotation
                renderGraphics.TranslateTransform(centerPoint.X, centerPoint.Y);
                renderGraphics.RotateTransform(rotationDegrees);
                renderGraphics.TranslateTransform(-centerPoint.X, -centerPoint.Y);

                // Draw the text
                renderGraphics.DrawString(textItem.Text, scaledFont, new SolidBrush(color), boundingRect, stringFormat);

                // Debug: Draw the bounding rectangle
                renderGraphics.DrawRectangle(new Pen(Color.LightGray, 2f),
                    boundingRect.X, boundingRect.Y, boundingRect.Width, boundingRect.Height);

                // Debug: Draw center point
                renderGraphics.FillEllipse(new SolidBrush(Color.Red),
                    centerPoint.X - 3f, centerPoint.Y - 3f, 6f, 6f);
            }
            finally
            {
                // Restore the graphics state
                renderGraphics.Restore(state);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"({_coords}) Error rendering text item \"{textItem.DisplayName}\"");
        }
    }

    /// <summary>
    /// Renders a value display item to the image.
    /// </summary>
    private void RenderValue(Graphics renderGraphics, StreamDeckKeyInfo keyInfo, DisplayItemValue valueItem)
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
        if (item.NCalcConditionHolder.NCalcExpression == null)
        {
            return true; // No condition means always visible.
        }

        var expression = item.NCalcConditionHolder.NCalcExpression;

        // TODO We should set the parameters only once, not for each invocation.
        foreach (var usedProperty in item.NCalcConditionHolder.UsedProperties)
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
            return new Font(font.Name, font.Size * scaleFactor, font.Style);
        }

        return font;
    }
}