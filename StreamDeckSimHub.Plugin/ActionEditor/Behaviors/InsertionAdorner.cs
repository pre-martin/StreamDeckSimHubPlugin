// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace StreamDeckSimHub.Plugin.ActionEditor.Behaviors;

/// <summary>
/// Adorner that provides a visual indication of where an item will be inserted
/// </summary>
public class InsertionAdorner : Adorner
{
    private readonly double _position;

    /// <summary>
    /// Creates a new insertion adorner
    /// </summary>
    /// <param name="adornedElement">The element being adorned (typically the ListBox)</param>
    /// <param name="position">The position of the insertion line</param>
    public InsertionAdorner(UIElement adornedElement, double position) : base(adornedElement)
    {
        _position = position;
        IsHitTestVisible = false; // Pass through mouse events
    }

    /// <summary>
    /// Renders the adorner
    /// </summary>
    protected override void OnRender(DrawingContext drawingContext)
    {
        // Create a pen for the insertion line
        var pen = new Pen(Brushes.Blue, 2);
        pen.DashStyle = DashStyles.Dash;

        // Horizontal line for a vertical list
        drawingContext.DrawLine(pen,
            new Point(0, _position),
            new Point(AdornedElement.RenderSize.Width, _position));
    }
}