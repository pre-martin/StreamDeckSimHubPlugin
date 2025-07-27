// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Globalization;
using System.Windows.Data;

namespace StreamDeckSimHub.Plugin.ActionEditor.Converters;

public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}