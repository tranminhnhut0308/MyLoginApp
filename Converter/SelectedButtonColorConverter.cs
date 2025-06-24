using System.Globalization;

namespace MyLoginApp.Converter
{
    public class SelectedButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string selectedValue = value?.ToString() ?? string.Empty;
            string buttonValue = parameter?.ToString() ?? string.Empty;

            // Nếu nút hiện tại là nút được chọn hoặc
            // nếu nút hiện tại là "Tất cả" và không có giá trị nào được chọn
            if (selectedValue == buttonValue || 
                (buttonValue == "Tất cả" && string.IsNullOrEmpty(selectedValue)))
            {
                return Color.FromArgb("#1976D2"); // Màu xanh đậm khi được chọn
            }

            return Color.FromArgb("#007AFF"); // Màu xanh nhạt khi không được chọn
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 