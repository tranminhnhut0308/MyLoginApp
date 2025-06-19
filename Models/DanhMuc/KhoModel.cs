namespace MyLoginApp.Models
{
    public class Kho
    {
        public string KhoMa { get; set; } // ID của kho
        public string TenKho { get; set; } // Tên của kho

        // Có thể thêm các thuộc tính khác nếu cần như địa chỉ, mô tả, trạng thái sử dụng, v.v.
        // public string DiaChi { get; set; } // Địa chỉ của kho
        // public bool SuDung { get; set; } // Trạng thái sử dụng
    }
}