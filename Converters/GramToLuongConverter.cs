using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Converters
{
    public class GramToLuongConverter : IValueConverter
    {
        private const decimal PHAN_PER_LUONG = 10m;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Xử lý trường hợp CanTong và TLThuc là kiểu decimal từ model
                if (value == null) 
                    return "0.00 Lượng";
                
                decimal phanValue;
                
                // Xử lý chuyên biệt cho các kiểu dữ liệu
                if (value is decimal decimalValue)
                {
                    phanValue = decimalValue;
                }
                else if (value is double doubleValue)
                {
                    phanValue = (decimal)doubleValue;
                }
                else if (value is int intValue)
                {
                    phanValue = intValue;
                }
                else if (value is string stringValue && decimal.TryParse(stringValue, out decimal parsedDecimal))
                {
                    phanValue = parsedDecimal;
                }
                else
                {
                    return "0.00 Lượng"; // Trả về giá trị mặc định nếu không convert được
                }
                
                // Tính giá trị lượng từ phân (1 lượng = 10 phân)
                decimal luongValue = phanValue / PHAN_PER_LUONG;
                
                // Định dạng số lượng với 2 số thập phân
                return $"{luongValue:0.00} Lượng";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chuyển đổi phân sang lượng: {ex.Message}");
                return "0.00 Lượng"; // Giá trị mặc định nếu có lỗi
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string luongString)
            {
                // Xóa ký tự "Lượng" và parse giá trị
                luongString = luongString.Replace("Lượng", "").Trim();
                
                if (decimal.TryParse(luongString, out decimal luongValue))
                {
                    // Chuyển từ lượng sang phân (nhân với 10)
                    return luongValue * PHAN_PER_LUONG;
                }
            }
            
            return 0m; // Trả về decimal 0
        }
    }
} 