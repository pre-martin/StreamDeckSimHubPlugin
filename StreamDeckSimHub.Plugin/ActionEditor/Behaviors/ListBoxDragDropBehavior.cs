// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

namespace StreamDeckSimHub.Plugin.ActionEditor.Behaviors;

/// <summary>
/// Adds drag-and-drop functionality to a ListBox, allowing items to be reordered.
/// </summary>
public class ListBoxDragDropBehavior : Behavior<ListBox>
{
    /// Dependency property to determine which item types can be dragged
    public static readonly DependencyProperty DraggableTypeProperty = DependencyProperty.Register(
        nameof(DraggableType), typeof(Type), typeof(ListBoxDragDropBehavior), new PropertyMetadata(null));

    /// The type of items that can be dragged (null means all items are draggable)
    public Type? DraggableType
    {
        get => (Type)GetValue(DraggableTypeProperty);
        set => SetValue(DraggableTypeProperty, value);
    }

    private ListBoxItem? _draggedItem;
    private Point _startPoint;
    private bool _isDragging;
    private InsertionAdorner? _insertionAdorner;
    private AdornerLayer? _adornerLayer;
    private int _draggedItemIndex; // Track the index of the dragged item

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        AssociatedObject.Drop += OnDrop;
        AssociatedObject.DragOver += OnDragOver;
        AssociatedObject.AllowDrop = true;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        // Clean up adorners when detaching
        CleanupAdorners();

        AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
        AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
        AssociatedObject.Drop -= OnDrop;
        AssociatedObject.DragOver -= OnDragOver;
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(null);
        _draggedItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

        // If we found a ListBoxItem and it contains a draggable item
        if (_draggedItem != null)
        {
            var item = _draggedItem.DataContext;

            // Check if the item is of the draggable type (if specified)
            if (DraggableType != null && !DraggableType.IsInstanceOfType(item))
            {
                _draggedItem = null;
                return;
            }

            _isDragging = true;
            // Store the index of the dragged item
            _draggedItemIndex = AssociatedObject.Items.IndexOf(item);
        }
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && _draggedItem != null)
        {
            var currentPosition = e.GetPosition(null);
            var diff = _startPoint - currentPosition;

            // Start drag operation if the mouse has moved far enough
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Initialize adorner layer if needed
                _adornerLayer ??= AdornerLayer.GetAdornerLayer(AssociatedObject);

                // Update insertion marker
                UpdateInsertionAdorner(e.GetPosition(AssociatedObject));

                // Start the drag operation
                DragDrop.DoDragDrop(AssociatedObject, _draggedItem.DataContext, DragDropEffects.Move);

                // Clean up adorners
                CleanupAdorners();

                _isDragging = false;
            }
        }
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        _draggedItem = null;
        CleanupAdorners();
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (_draggedItem == null)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var data = e.Data.GetData(_draggedItem.DataContext.GetType());
        if (data == null)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        // For CommandItems, check if the target is within the same StreamDeckAction group
        if (data is CommandItemViewModel draggedCommandItem)
        {
            // Find the item under the drop position
            var dropPosition = e.GetPosition(AssociatedObject);
            var targetElement = GetItemAtPosition(dropPosition);

            if (targetElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            var targetItem = targetElement.DataContext;

            // If target is a StreamDeckActionViewModel, don't allow drop
            if (targetItem is StreamDeckActionViewModel)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // If target is a CommandItemViewModel, check if it's in the same StreamDeckAction group
            if (targetItem is CommandItemViewModel targetCommandItem)
            {
                if (draggedCommandItem.ParentAction != targetCommandItem.ParentAction)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }
            }
        }

        // Update insertion marker
        UpdateInsertionAdorner(e.GetPosition(AssociatedObject));

        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        // Clean up adorners
        CleanupAdorners();

        if (_draggedItem == null)
            return;

        var data = e.Data.GetData(_draggedItem.DataContext.GetType());
        if (data == null)
            return;

        // Find the item under the drop position
        var dropPosition = e.GetPosition(AssociatedObject);
        var targetElement = GetItemAtPosition(dropPosition);

        if (targetElement == null)
            return;

        var targetItem = targetElement.DataContext;

        // Handle different item types
        if (data is DisplayItemViewModel displayItem)
        {
            HandleDisplayItemDrop(displayItem, targetItem);
        }
        else if (data is CommandItemViewModel draggedCommandItem)
        {
            // For CommandItems, ensure we're dropping within the same StreamDeckAction group
            if (targetItem is StreamDeckActionViewModel)
                return;

            if (targetItem is CommandItemViewModel targetCommandItem)
            {
                if (draggedCommandItem.ParentAction != targetCommandItem.ParentAction)
                    return;

                HandleCommandItemDrop(draggedCommandItem, targetCommandItem);
            }
        }
    }

    private void HandleDisplayItemDrop(DisplayItemViewModel draggedItem, object targetItem)
    {
        if (targetItem is not DisplayItemViewModel)
            return;

        // Get the source and target indices
        var sourceIndex = AssociatedObject.Items.IndexOf(draggedItem);
        var targetIndex = AssociatedObject.Items.IndexOf(targetItem);

        // Don't do anything if dropping onto itself
        if (sourceIndex == targetIndex)
            return;

        // Get the collection and reorder items
        if (AssociatedObject.ItemsSource is ObservableCollection<DisplayItemViewModel> itemsSource)
        {
            // Move the item in the collection
            itemsSource.Move(sourceIndex, targetIndex);

            // Update the underlying model (Settings.DisplayItems)
            if (AssociatedObject.DataContext is SettingsViewModel settingsViewModel)
            {
                settingsViewModel.UpdateDisplayItemsOrder();
            }
        }
    }

    private void HandleCommandItemDrop(CommandItemViewModel draggedItem, CommandItemViewModel targetItem)
    {
        // Get the source and target indices within the FlatCommandItems collection
        var sourceIndex = AssociatedObject.Items.IndexOf(draggedItem);
        var targetIndex = AssociatedObject.Items.IndexOf(targetItem);

        // Don't do anything if dropping onto itself
        if (sourceIndex == targetIndex)
            return;

        // Get the collection and reorder items
        if (AssociatedObject.ItemsSource is ObservableCollection<IFlatCommandItemsViewModel> itemsSource)
        {
            // Move the item in the collection
            itemsSource.Move(sourceIndex, targetIndex);

            // Update the underlying model (Settings.CommandItems)
            if (AssociatedObject.DataContext is SettingsViewModel settingsViewModel)
            {
                settingsViewModel.UpdateCommandItemsOrder(draggedItem.ParentAction);
            }
        }
    }

    private ListBoxItem? GetItemAtPosition(Point position)
    {
        HitTestResult? result = VisualTreeHelper.HitTest(AssociatedObject, position);
        return result == null ? null : FindAncestor<ListBoxItem>(result.VisualHit);
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null && current is not T)
        {
            current = VisualTreeHelper.GetParent(current);
        }

        return current as T;
    }

    /// <summary>
    /// Updates the insertion adorner based on the current mouse position
    /// </summary>
    private void UpdateInsertionAdorner(Point position)
    {
        // Remove existing insertion adorner
        if (_insertionAdorner != null && _adornerLayer != null)
        {
            _adornerLayer.Remove(_insertionAdorner);
            _insertionAdorner = null;
        }

        // Find the item below the mouse pointer
        var targetItem = GetItemAtPosition(position);

        // Now we have to find the correct Y position for the insertion line. WPF works like this:
        // - If the mouse pointer touches the lower bound of the item above, the dragged item will be inserted before that item.
        // - If the mouse pointer touches the upper bound of the item below, the dragged item will be inserted after that item.
        if (targetItem != null && _adornerLayer != null && _draggedItem != null)
        {
            // Get the bounds of the target item (they are constant in our use case, because all Items have the same size)
            var targetBounds = VisualTreeHelper.GetDescendantBounds(targetItem);
            // Determine the top Y position of the target item relative to the ListBox. So it will be absolute in the ListBox.
            var targetTopAbsolute = targetItem.TranslatePoint(new Point(0, 0), AssociatedObject).Y;

            // Get the target item's index
            var targetItemIndex = AssociatedObject.Items.IndexOf(targetItem.DataContext);

            // Determine where WPF will insert the dragged item, which depends on the drag direction
            bool insertAfter;
            if (targetItemIndex > _draggedItemIndex) insertAfter = true;
            else if (targetItemIndex < _draggedItemIndex) insertAfter = false;
            // Dragging onto self - use the vertical middle point to show the adorner either above or below.
            else insertAfter = position.Y > targetTopAbsolute + targetBounds.Height / 2;

            var lineY = targetTopAbsolute + (insertAfter ? targetBounds.Height : 0);

            // Create a new insertion adorner
            _insertionAdorner = new InsertionAdorner(AssociatedObject, lineY);
            _adornerLayer.Add(_insertionAdorner);
        }
    }

    /// <summary>
    /// Cleans up all adorners
    /// </summary>
    private void CleanupAdorners()
    {
        if (_adornerLayer != null)
        {
            if (_insertionAdorner != null)
            {
                _adornerLayer.Remove(_insertionAdorner);
                _insertionAdorner = null;
            }
        }
    }
}