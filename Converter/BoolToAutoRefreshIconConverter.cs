using System.Globalization;

namespace MyLoginApp.Converter
{
    public class BoolToAutoRefreshIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEnabled)
            {
                return isEnabled ? "refresh_on.png" : "refresh_off.png";
            }
            return "refresh_off.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 