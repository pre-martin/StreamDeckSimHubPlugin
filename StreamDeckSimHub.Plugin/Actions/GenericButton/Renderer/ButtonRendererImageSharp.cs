// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using NLog;
using SharpDeck.Events.Received;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.Tools;
using Size = SixLabors.ImageSharp.Size;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Renderer;

public class ButtonRendererImageSharp : IButtonRenderer
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly StreamDeckKeyInfo _defaultKeyInfo = StreamDeckKeyInfoBuilder.DefaultKeyInfo;
    private readonly NCalcHandler _ncalcHandler = new();
    private readonly FormatHelper _formatHelper = new();
    private readonly ConditionEvaluator _conditionEvaluator;
    private Coordinates _coords = new() { Column = -1, Row = -1 };

    public ButtonRendererImageSharp(GetPropertyDelegate getProperty)
    {
        _conditionEvaluator = new ConditionEvaluator(
            _ncalcHandler,
            getProperty,
            () => _coords.ToString());
    }

    public void SetCoordinates(Coordinates coordinates)
    {
        _coords = coordinates;
    }

    public Image<Rgba32> Render(StreamDeckKeyInfo targetKeyInfo, Collection<DisplayItem> displayItems, BlinkOverride? blinkOverride = null)
    {
        _logger.Debug($"({_coords}) Rendering...");
        var image = new Image<Rgba32>(targetKeyInfo.KeySize.Width, targetKeyInfo.KeySize.Height);

        // Iterate over all display items.
        foreach (var displayItem in displayItems)
        {
            if (!_conditionEvaluator.IsItemActive(displayItem)) continue;

            // Check if any active ModifierBlink is in the "Off" (= hide) phase.
            // When blinkOverride is enabled, it owns the phase state (all blink in sync). Otherwise, each ModifierBlink uses its own individual phase state.
            var shouldHideForBlink = false;
            foreach (var modifier in displayItem.Modifiers)
            {
                if (modifier is ModifierBlink modifierBlink && _conditionEvaluator.IsModifierActive(modifier))
                {
                    if (blinkOverride is { Enabled: true } ? blinkOverride.IsOffPhase() : modifierBlink.IsOffPhase())
                    {
                        shouldHideForBlink = true;
                        break;
                    }
                }
            }

            if (shouldHideForBlink) continue;

            // Render the item.
            switch (displayItem)
            {
                case DisplayItemBox boxItem:
                    RenderBox(image, targetKeyInfo, boxItem);
                    break;
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
                    _logger.Warn($"({_coords})   Unknown DisplayItem type: {displayItem.GetType().Name}");
                    break;
            }
            //image.SaveAsPng($@"\image_{_coords}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}_{displayItem.DisplayName}.png");
        }

        return image;
    }

    /// <summary>
    /// Renders a box.
    /// </summary>
    private void RenderBox(Image<Rgba32> image, StreamDeckKeyInfo keyInfo, DisplayItemBox boxItem)
    {
        try
        {
            // Color + Transparency + Color Modifiers
            var color = boxItem.Color.WithAlpha(boxItem.DisplayParameters.Transparency);
            foreach (var modifier in boxItem.Modifiers)
            {
                if (modifier is ModifierColor modifierColor && _conditionEvaluator.IsModifierActive(modifier))
                {
                    color = modifierColor.Color.WithAlpha(boxItem.DisplayParameters.Transparency);
                }
            }

            // Position + Size + Corner Radius
            var position = boxItem.DisplayParameters.Position;
            var boundingSize = boxItem.DisplayParameters.Size ?? keyInfo.KeySize;
            var rect = new RectangleF(position.X, position.Y, boundingSize.Width, boundingSize.Height);
            var maxCornerRadius = MathF.Min(rect.Width, rect.Height) / 2f;
            var cornerRadius = Math.Clamp(boxItem.CornerRadius, 0f, maxCornerRadius);
            var path = cornerRadius <= 0f
                ? new RectangularPolygon(rect)
                : CreateRoundedRectanglePath(rect, cornerRadius);

            // Rotation
            if (boxItem.DisplayParameters.Rotation == 0)
            {
                // No rotation
                image.Mutate(ctx => ctx.Fill(color, path));
            }
            else
            {
                // With rotation, create a rotated shape using matrix transformation
                var centerPoint = new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                var rotationRadians = boxItem.DisplayParameters.Rotation * (float)Math.PI / 180f;

                // Create transformation: translate to origin, rotate, translate back to center
                var transform = Matrix3x2.CreateTranslation(-centerPoint) *
                                Matrix3x2.CreateRotation(rotationRadians) *
                                Matrix3x2.CreateTranslation(centerPoint);

                image.Mutate(ctx =>
                {
                    ctx.SetDrawingTransform(transform);
                    ctx.Fill(color, path);
                    ctx.SetDrawingTransform(Matrix3x2.Identity);
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"({_coords})   Error rendering box item \"{boxItem.DisplayName}\"");
        }
    }

    private static IPath CreateRoundedRectanglePath(RectangleF rect, float cornerRadius)
    {
        var left = rect.Left;
        var top = rect.Top;
        var right = rect.Right;
        var bottom = rect.Bottom;
        var radius = Math.Clamp(cornerRadius, 0f, MathF.Min(rect.Width, rect.Height) / 2f);

        if (radius <= 0f)
        {
            return new RectangularPolygon(rect);
        }

        var pathBuilder = new PathBuilder();
        pathBuilder.StartFigure();
        pathBuilder.MoveTo(new PointF(left + radius, top));
        pathBuilder.LineTo(right - radius, top);
        pathBuilder.ArcTo(radius, radius, 0, false, true, new PointF(right, top + radius));
        pathBuilder.LineTo(right, bottom - radius);
        pathBuilder.ArcTo(radius, radius, 0, false, true, new PointF(right - radius, bottom));
        pathBuilder.LineTo(left + radius, bottom);
        pathBuilder.ArcTo(radius, radius, 0, false, true, new PointF(left, bottom - radius));
        pathBuilder.LineTo(left, top + radius);
        pathBuilder.ArcTo(radius, radius, 0, false, true, new PointF(left + radius, top));
        pathBuilder.CloseFigure();

        return pathBuilder.Build();
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
                Position = AnchorPositionMode.Center,
                Sampler = KnownResamplers.Lanczos3
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
            _logger.Error(ex, $"({_coords})   Error rendering image item \"{imageItem.DisplayName}\"");
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
            _logger.Error(ex, $"({_coords})   Error rendering text item \"{textItem.DisplayName}\"");
        }
    }

    /// <summary>
    /// Renders a value display item to the image.
    /// </summary>
    private void RenderValue(Image<Rgba32> image, StreamDeckKeyInfo keyInfo, DisplayItemValue valueItem)
    {
        try
        {
            var value = _conditionEvaluator.Evaluate(valueItem.NCalcPropertyHolder, $"Value of \"{valueItem.DisplayName}\"");
            var format = _formatHelper.CompleteFormatString(valueItem.DisplayFormat);
            var formattedValue = string.Format(CultureInfo.CurrentCulture, format, value);
            RenderString(image, keyInfo, valueItem, valueItem.Font, valueItem.Color, formattedValue);
        }
        catch (FormatException ex)
        {
            _logger.Warn(
                $"({_coords})   Error formatting value for item \"{valueItem.DisplayName}\" with format \"{valueItem.DisplayFormat}\": {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"({_coords})   Error rendering value item \"{valueItem.DisplayName}\"");
        }
    }

    private void RenderString(Image<Rgba32> image, StreamDeckKeyInfo keyInfo,
        DisplayItem displayItem, Font font, Color color, string s)
    {
        // Scale font to the device key size
        var scaledFont = ScaleFont(font, keyInfo.KeySize);

        // Color + Transparency + Color Modifiers
        var colorWithAlpha = color.WithAlpha(displayItem.DisplayParameters.Transparency);
        foreach (var modifier in displayItem.Modifiers)
        {
            if (modifier is ModifierColor modifierColor && _conditionEvaluator.IsModifierActive(modifier))
            {
                colorWithAlpha = modifierColor.Color.WithAlpha(displayItem.DisplayParameters.Transparency);
            }
        }

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
    /// Scale the font size based on the key resolution. So we can use the same font size across different Stream Deck models.
    /// Base size is a standard Stream Deck with 72 x 72 pixels.
    /// </summary>
    private Font ScaleFont(Font font, Size keySize)
    {
        if (keySize.Height == _defaultKeyInfo.KeySize.Height) return font;

        var scaleFactor = (float)keySize.Height / _defaultKeyInfo.KeySize.Height;
        return new Font(font.Family, font.Size * scaleFactor, font.FontStyle());
    }
}