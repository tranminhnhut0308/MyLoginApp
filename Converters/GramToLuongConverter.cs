using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Converters
{
    public class GramToLuongConverter : IValueConverter
    {
        private const decimal GRAM_PER_LUONG = 37.5m;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Xử lý trường hợp CanTong và TLThuc là kiểu decimal từ model
                if (value == null) 
                    return "0.00 Lượng";
                
                decimal gramValue;
                
                // Xử lý chuyên biệt cho decimal - kiểu dữ liệu chính của CanTong và TLThuc
                if (value is decimal decimalValue)
                {
                    gramValue = decimalValue;
                }
                else if (value is double doubleValue)
                {
                    gramValue = (decimal)doubleValue;
                }
                else if (value is int intValue)
                {
                    gramValue = intValue;
                }
                else if (value is string stringValue && decimal.TryParse(stringValue, out decimal parsedDecimal))
                {
                    gramValue = parsedDecimal;
                }
                else
                {
                    return "0.00 Lượng"; // Trả về giá trị mặc định nếu không convert được
                }
                
                // Tính giá trị lượng từ gram
                decimal luongValue = gramValue / GRAM_PER_LUONG;
                
                // Định dạng số lượng với 2 số thập phân
                return $"{luongValue:0.00} Lượng";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chuyển đổi gram sang lượng: {ex.Message}");
                return "0.00 Lượng"; // Giá trị mặc định nếu có lỗi
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string luongString)
            {
                // Xóa ký tự "L" và parse giá trị
                luongString = luongString.Replace("Lượng", "").Trim();
                
                if (decimal.TryParse(luongString, out decimal luongValue))
                {
                    // Đã sửa để nhân giữa 2 decimal
                    return luongValue * GRAM_PER_LUONG;
                }
            }
            
            return 0m; // Trả về decimal 0
        }
    }
} 