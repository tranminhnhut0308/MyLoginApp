using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Converter
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                if (d == Math.Truncate(d))
                    return ((int)d).ToString();
                return d.ToString("G", culture);
            }
            if (value is double?)
            {
                var dn = (double?)value;
                if (dn.HasValue)
                {
                    if (dn.Value == Math.Truncate(dn.Value))
                        return ((int)dn.Value).ToString();
                    return dn.Value.ToString("G", culture);
                }
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value?.ToString(), out double result))
                return result;
            return 0d;
        }
    }
} 