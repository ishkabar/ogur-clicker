// Ogur.Clicker.Host/Converters/BoolToColorConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Ogur.Clicker.Host.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value
            ? new SolidColorBrush(Color.FromRgb(0, 122, 204))
            : new SolidColorBrush(Color.FromRgb(60, 60, 60));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}