using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Converters
{
    public class LyToLuongConverter : IValueConverter
    {
        private const decimal LY_PER_LUONG = 1000m;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) 
                    return "0.00 L";
                
                decimal lyValue;
                
                // Xử lý các kiểu dữ liệu khác nhau
                if (value is decimal decimalValue)
                {
                    lyValue = decimalValue;
                }
                else if (value is double doubleValue)
                {
                    lyValue = (decimal)doubleValue;
                }
                else if (value is int intValue)
                {
                    lyValue = intValue;
                }
                else if (value is string stringValue && decimal.TryParse(stringValue, out decimal parsedDecimal))
                {
                    lyValue = parsedDecimal;
                }
                else
                {
                    return "0.00 L"; // Trả về giá trị mặc định nếu không convert được
                }
                
                // Tính giá trị lượng từ ly (1 lượng = 1000 ly)
                decimal luongValue = lyValue / LY_PER_LUONG;
                
                // Đầu tiên định dạng với 3 số thập phân
                string rawValue = luongValue.ToString("0.000", CultureInfo.InvariantCulture);
                
                // Kiểm tra và loại bỏ các số 0 không cần thiết ở cuối
                if (rawValue.EndsWith("00"))
                {
                    // Nếu kết thúc bằng 2 số 0, chỉ giữ 1 số thập phân (vd: 0.500 -> 0.5)
                    rawValue = luongValue.ToString("0.0", CultureInfo.InvariantCulture);
                }
                else if (rawValue.EndsWith("0"))
                {
                    // Nếu kết thúc bằng 1 số 0, chỉ giữ 2 số thập phân (vd: 0.050 -> 0.05)
                    rawValue = luongValue.ToString("0.00", CultureInfo.InvariantCulture);
                }
                
                return $"{rawValue} L";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chuyển đổi ly sang lượng: {ex.Message}");
                return "0.00 Lượng"; // Giá trị mặc định nếu có lỗi
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string luongString)
            {
                // Xóa ký tự "Lượng" và parse giá trị
                luongString = luongString.Replace("L", "").Trim();
                
                if (decimal.TryParse(luongString, out decimal luongValue))
                {
                    // Chuyển từ lượng sang ly (nhân với 1000)
                    return luongValue * LY_PER_LUONG;
                }
            }
            
            return 0m; // Trả về decimal 0
        }
    }
} 