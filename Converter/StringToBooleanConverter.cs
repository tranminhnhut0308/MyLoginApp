using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Converters
{
    public class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Returns true if the string is not null and not empty
            return value is string stringValue && !string.IsNullOrWhiteSpace(stringValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 