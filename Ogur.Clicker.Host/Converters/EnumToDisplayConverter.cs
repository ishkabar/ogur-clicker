// Ogur.Clicker.Host/Converters/EnumToDisplayConverter.cs
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Ogur.Clicker.Host.Converters;

public class EnumToDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        var text = value.ToString();

        // Add spaces before capital letters
        return Regex.Replace(text!, "([a-z])([A-Z])", "$1 $2");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}