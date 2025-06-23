using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StreamDeckSimHub.Plugin.ActionEditor.Converters
{
    /// <summary>
    /// Converts a boolean value to Visibility.Collapsed (true) or Visibility.Visible (false).
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v != Visibility.Visible;
            return false;
        }
    }
}
