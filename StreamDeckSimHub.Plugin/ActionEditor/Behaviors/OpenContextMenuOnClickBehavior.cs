// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StreamDeckSimHub.Plugin.ActionEditor.Behaviors;

/// <summary>
/// Modifies a Button to open its ContextMenu on left click instead of right click.
/// </summary>
public static class OpenContextMenuOnClickBehavior
{
    public static readonly DependencyProperty ApplyProperty =
        DependencyProperty.RegisterAttached(
            "Apply",
            typeof(bool),
            typeof(OpenContextMenuOnClickBehavior),
            new PropertyMetadata(false, OnApplyChanged));

    public static void SetApply(DependencyObject element, bool value) => element.SetValue(ApplyProperty, value);

    public static bool GetApply(DependencyObject element) => (bool)element.GetValue(ApplyProperty);

    private static void OnApplyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not true) return;
        if (d is not Button button) return;

        ContextMenuService.SetIsEnabled(button, false);
        button.Click += Button_Click;
        button.PreviewMouseRightButtonDown += SuppressRightClick;
    }

    private static void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }
    }

    private static void SuppressRightClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}