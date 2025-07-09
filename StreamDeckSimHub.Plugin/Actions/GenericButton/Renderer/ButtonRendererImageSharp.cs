// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using NLog;
using SharpDeck.Events.Received;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Tools;
using StreamDeckSimHub.Plugin.PropertyLogic;
using Size = SixLabors.ImageSharp.Size;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Renderer;

public class ButtonRendererImageSharp(GetPropertyDelegate getProperty) : IButtonRenderer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly StreamDeckKeyInfo DefaultKeyInfo = StreamDeckKeyInfoBuilder.DefaultKeyInfo;

    private readonly FormatHelper _formatHelper = new();
    private readonly object _expressionLock = new();
    private Coordinates _coords = new() { Column = -1, Row = -1 };

    public void SetCoordinates(Coordinates coordinates)
    {
        _coords = coordinates;
    }

    public string Render(StreamDeckKeyInfo targetKeyInfo, Collection<DisplayItem> displayItems)
    {
        Logger.Debug($"({_coords}) Rendering...");
        var image = new Image<Rgba32>(targetKeyInfo.KeySize.Width, targetKeyInfo.KeySize.Height);

        // Iterate over all display items.
        foreach (var displayItem in displayItems)
        {
            if (!IsVisible(displayItem))
            {
                continue;
            }

            // Render the item.
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
                    Logger.Warn($"({_coords})   Unknown DisplayItem type: {displayItem.GetType().Name}");
                    break;
            }
            //image.SaveAsPng($@"\image_{_coords}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}_{displayItem.DisplayName}.png");
        }

        return image.ToBase64String(PngFormat.Instance);
    }

    /// <summary>
    /// Renders an image.
    /// </summary>
    private void RenderImage(Image<Rgba32> image, StreamDeckKeyInfo keyInfo, DisplayItemImage imageItem)
    {
        try
        {
            // Position + Size
            var position = imageItem.DisplayParameters.Position;
            var boundingSize = imageItem.DisplayParameters.Size ?? keyInfo.KeySize;

            var resizedImage = imageItem.Image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = boundingSize,
                Mode = ResizeMode.Max,
                Position = AnchorPositionMode.Center
            }));

            // Rotation
            var rotatedImage = imageItem.DisplayParameters.Rotation == 0
                ? resizedImage
                : resizedImage.Clone(ctx => ctx.Rotate(imageItem.DisplayParameters.Rotation));
            // Calculate new position, as the rotated image may be larger than before rotation.
            var newPosition = new Point(
                position.X - (rotatedImage.Width - boundingSize.Width) / 2,
                position.Y - (rotatedImage.Height - boundingSize.Height) / 2);

            image.Mutate(ctx => ctx.DrawImage(rotatedImage, newPosition, imageItem.DisplayParameters.Transparency));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"({_coords})   Error rendering image item \"{imageItem.DisplayName}\"");
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
            RenderString(image, keyInfo, textItem, textItem.Font, textItem.Color, textItem.Text);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"({_coords})   Error rendering text item \"{textItem.DisplayName}\"");
        }
    }

    /// <summary>
    /// Renders a value display item to the image.
    /// </summary>
    private void RenderValue(Image<Rgba32> image, StreamDeckKeyInfo keyInfo, DisplayItemValue valueItem)
    {
        try
        {
            var value = EvaluateExpression($"({_coords})   Value of \"{valueItem.DisplayName}\"", valueItem.NCalcPropertyHolder);
            var format = _formatHelper.CompleteFormatString(valueItem.DisplayFormat);
            var formattedValue = string.Format(CultureInfo.CurrentCulture, format, value);
            RenderString(image, keyInfo, valueItem, valueItem.Font, valueItem.Color, formattedValue);
        }
        catch (FormatException ex)
        {
            Logger.Warn(
                $"({_coords})   Error formatting value for item \"{valueItem.DisplayName}\" with format \"{valueItem.DisplayFormat}\": {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"({_coords})   Error rendering value item \"{valueItem.DisplayName}\"");
        }
    }

    private void RenderString(Image<Rgba32> image, StreamDeckKeyInfo keyInfo,
        DisplayItem displayItem, Font font, Color color, string s)
    {
        // Scale font to the device key size
        var scaledFont = ScaleFont(font, keyInfo.KeySize);

        // Color + Transparency
        var colorWithAlpha = color.WithAlpha(displayItem.DisplayParameters.Transparency);

        // Position + Size
        var position = displayItem.DisplayParameters.Position;
        var boundingSize = displayItem.DisplayParameters.Size ?? keyInfo.KeySize;
        var boundingRect = new RectangleF(position.X, position.Y, boundingSize.Width, boundingSize.Height);

        // Center point of the bounding rectangle
        var centerPoint = new PointF(boundingRect.X + boundingRect.Width / 2f,
            boundingRect.Y + boundingRect.Height / 2f);

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
        var rotationRadians = displayItem.DisplayParameters.Rotation * (float)Math.PI / 180f;

        // Move the center point to origin, apply rotation, then move back to the original center point.
        var transform = Matrix3x2.CreateTranslation(-centerPoint) *
                        Matrix3x2.CreateRotation(rotationRadians) *
                        Matrix3x2.CreateTranslation(centerPoint);

        image.Mutate(ctx =>
        {
            ctx.SetDrawingTransform(transform);
            ctx.DrawText(textOptions, s, colorWithAlpha);
            //ctx.Draw(Color.LightGray, 2f, boundingRect); // Debug: Draw the bounding rectangle
            //ctx.Fill(Color.Red, new EllipsePolygon(centerPoint, 3f)); // Debug: Draw center point
            ctx.SetDrawingTransform(Matrix3x2.Identity);
        });
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

        var value = EvaluateExpression($"({_coords})   Visibility of \"{item.DisplayName}\"", item.NCalcConditionHolder);
        return value is true or > 0 or > 0.0f or > 0.0d;
    }

    private object? EvaluateExpression(string loggingContext, NCalcHolder nCalcHolder)
    {
        if (nCalcHolder.NCalcExpression == null) return null;

        var expression = nCalcHolder.NCalcExpression;

        lock (_expressionLock)
        {
            // Are the parameters of the NCalc expression up to date?
            if (expression.DynamicParameters.Count != nCalcHolder.UsedProperties.Count)
            {
                expression.DynamicParameters.Clear();
                foreach (var usedProperty in nCalcHolder.UsedProperties)
                {
                    expression.DynamicParameters[usedProperty] = _ => getProperty.Invoke(usedProperty);
                }
            }
        }

        try
        {
            var result = expression.Evaluate();
            if (Logger.IsDebugEnabled)
            {
                var msg = $"{loggingContext}: ";
                msg += $"\"{expression.ExpressionString}\" => \"{result}\", parameters: ";
                msg = nCalcHolder.UsedProperties.Aggregate(msg,
                    (current, propName) => current + $"\"{propName}\"=\"{getProperty.Invoke(propName)}\", ");
                Logger.Debug(msg);
            }

            return result;
        }
        catch (Exception e)
        {
            Logger.Warn($"{loggingContext} Error evaluating expression: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Scale the font size based on the key resolution. So we can use the same font size across different Stream Deck models.
    /// Base size is a standard Stream Deck with 72 x 72 pixels.
    /// </summary>
    private Font ScaleFont(Font font, Size keySize)
    {
        if (keySize.Height == DefaultKeyInfo.KeySize.Height) return font;

        var scaleFactor = keySize.Height / DefaultKeyInfo.KeySize.Height;
        return new Font(font.Family, font.Size * scaleFactor, font.FontStyle());
    }
}