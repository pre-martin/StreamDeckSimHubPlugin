// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StreamDeckSimHub.Plugin.ActionEditor.Converters;

/// <summary>
/// Converts a boolean value to Visibility.Collapsed (true) or Visibility.Visible (false).
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility visibility && visibility != Visibility.Visible;
    }
}