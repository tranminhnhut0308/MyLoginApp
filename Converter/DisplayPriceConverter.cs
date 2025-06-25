using System.Globalization;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

namespace MyLoginApp.Converter
{
    /// <summary>
    /// Converter hiển thị giá trị 0 thay vì để trống cho các trường giá trị null khi hiển thị trong danh sách
    /// </summary>
    public class DisplayPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Lấy format hiển thị từ parameter
            string format = parameter as string ?? "{0}";

            // Nếu giá trị là null, trả về chuỗi format với giá trị 0
            if (value == null)
            {
                return string.Format(culture, format, 0);
            }

            // Nếu có giá trị, áp dụng định dạng
            return string.Format(culture, format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Xử lý trường hợp null hoặc chuỗi trống
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return null; // Trả về null thay vì giá trị mặc định
                }

                string input = value.ToString().Trim();
                
                // Thêm kiểm tra an toàn trước khi xử lý
                if (input.Length == 0)
                {
                    return null;
                }
                
                // Sử dụng Regex để xử lý an toàn hơn
                string numericValue = Regex.Replace(input, "[^0-9.,]", "");
                
                if (string.IsNullOrWhiteSpace(numericValue))
                {
                    return null;
                }

                // Thêm xử lý đặc biệt cho trường hợp chỉ còn dấu phẩy hoặc dấu chấm
                if (numericValue == "." || numericValue == ",")
                {
                    return null;
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
                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                {
                    if (decimal.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                    {
                        return result;
                    }
                    // Nếu parse không thành công, trả về null thay vì 0 để tránh crash
                    return null;
                }
                else if (targetType == typeof(double) || targetType == typeof(double?))
                {
                    if (double.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                    {
                        return result;
                    }
                    return null;
                }
                else if (targetType == typeof(int) || targetType == typeof(int?))
                {
                    if (int.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
                    {
                        return result;
                    }
                    return null;
                }
                else if (targetType == typeof(long) || targetType == typeof(long?))
                {
                    if (long.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
                    {
                        return result;
                    }
                    return null;
                }
                
                // Kiểu dữ liệu không được hỗ trợ, trả về null
                Debug.WriteLine($"DisplayPriceConverter: Không hỗ trợ kiểu dữ liệu {targetType}");
                return null;
            }
            catch (Exception ex)
            {
                // Log lỗi cho mục đích debug
                Debug.WriteLine($"DisplayPriceConverter Error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Trả về null nếu có lỗi nghiêm trọng để bảo vệ ứng dụng
                return null;
            }
        }
    }
} 