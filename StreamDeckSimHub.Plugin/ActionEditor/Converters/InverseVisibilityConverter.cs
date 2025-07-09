// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StreamDeckSimHub.Plugin.ActionEditor.Converters;

/// <summary>
/// Converts Visibility to its inverse (Visibility.Visible to Visibility.Collapsed and vice versa).
/// </summary>
public class InverseVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility v)
        {
            return v == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility v)
        {
            return v == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Visible;
    }
}