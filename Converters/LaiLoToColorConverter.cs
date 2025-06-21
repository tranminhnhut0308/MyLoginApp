using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace MyLoginApp.Converters
{
    public class LaiLoToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal laiLo)
            {
                if (laiLo > 0)
                {
                    return Colors.Green;
                }
                else if (laiLo < 0)
                {
                    return Colors.Red;
                }
            }
            return Colors.Black; // Mặc định là màu đen
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 