using System.Globalization;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Converter
{
    public class NullableValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nếu giá trị là null, trả về chuỗi trống
            if (value == null || (value is double doubleValue && doubleValue == 0) || 
                (value is decimal decimalValue && decimalValue == 0))
            {
                return string.Empty;
            }
            
            // Nếu có định dạng, áp dụng định dạng
            if (parameter != null && value != null)
            {
                string format = parameter.ToString();
                return string.Format(culture, format, value);
            }
            
            // Trả về giá trị nguyên bản
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Nếu chuỗi trống hoặc null, trả về null hoặc giá trị mặc định
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null 
                        ? Activator.CreateInstance(targetType) 
                        : null;
                }

                string input = value.ToString().Trim();
                
                // Loại bỏ tất cả ký tự không phải số, dấu chấm hoặc dấu phẩy
                string numericValue = new string(input.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                
                if (string.IsNullOrWhiteSpace(numericValue))
                {
                    return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null 
                        ? Activator.CreateInstance(targetType) 
                        : null;
                }

                // Chuẩn hóa định dạng số
                // Thay thế dấu phẩy ngăn cách phần nghìn theo chuẩn văn hóa hiện tại
                if (CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator == ",")
                {
                    // Nếu dấu phẩy là dấu ngăn cách phần nghìn (VD: 1,000,000)
                    numericValue = numericValue.Replace(",", string.Empty);
                }
                else if (CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator == ".")
                {
                    // Nếu dấu chấm là dấu ngăn cách phần nghìn (VD: 1.000.000)
                    numericValue = numericValue.Replace(".", string.Empty).Replace(",", ".");
                }

                // Xử lý theo từng kiểu dữ liệu đích
                if (targetType == typeof(double) || targetType == typeof(double?))
                {
                    if (double.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                    {
                        return result;
                    }
                    // Trả về 0 nếu parse thất bại
                    return targetType == typeof(double) ? 0d : (double?)0d;
                }
                else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                {
                    if (decimal.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                    {
                        return result;
                    }
                    // Trả về 0 nếu parse thất bại
                    return targetType == typeof(decimal) ? 0m : (decimal?)0m;
                }
                else if (targetType == typeof(int) || targetType == typeof(int?))
                {
                    if (int.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
                    {
                        return result;
                    }
                    // Trả về 0 nếu parse thất bại
                    return targetType == typeof(int) ? 0 : (int?)0;
                }
                else if (targetType == typeof(long) || targetType == typeof(long?))
                {
                    if (long.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
                    {
                        return result;
                    }
                    // Trả về 0 nếu parse thất bại
                    return targetType == typeof(long) ? 0L : (long?)0L;
                }
                
                // Kiểu dữ liệu không được hỗ trợ, trả về giá trị gốc
                Debug.WriteLine($"NullableValueConverter: Không hỗ trợ kiểu dữ liệu {targetType}");
                return value;
            }
            catch (Exception ex)
            {
                // Log lỗi cho mục đích debug
                Debug.WriteLine($"NullableValueConverter Error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Trả về UnsetValue nếu có lỗi nghiêm trọng để bảo vệ ứng dụng
                return BindableProperty.UnsetValue;
            }
        }
    }
} 