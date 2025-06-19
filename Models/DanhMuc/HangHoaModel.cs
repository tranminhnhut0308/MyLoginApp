namespace MyLoginApp.Models
{
    public class HangHoaModel
    {
        public String HangHoaID { get; set; }  // Mã hàng hóa
        public string HangHoaMa { get; set; }  // Mã hàng hóa từ HANGHOAMA
        public string TenHangHoa { get; set; }  // Tên hàng hóa
        public string LoaiVang { get; set; }  // Loại vàng
        public string Nhom { get; set; }  // Nhóm hàng hóa
        public decimal CanTong { get; set; }  // Cân tổng
        public decimal TrongLuongHot { get; set; }  // Trọng lượng hột
        
        // Mở rộng để có thể gán giá trị từ bên ngoài
        private decimal _truHot;
        public decimal TruHot 
        { 
            get => _truHot == 0 ? CanTong - TrongLuongHot : _truHot;  // Nếu chưa gán, tự động tính
            set => _truHot = value;
        }
        
        public decimal CongGoc { get; set; }  // Công gốc
        public decimal GiaCong { get; set; }  // Giá công
        public decimal DonViGoc { get; set; }  // Đơn giá gốc
        public decimal SoLuong { get; set; }
        
        // Thuộc tính dùng cho UI
        public bool IsSelected { get; set; }
    }
}