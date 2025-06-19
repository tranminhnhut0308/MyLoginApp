using System;

namespace MyLoginApp.Models
{
    public class PhieuDoiModel
    {
        public int PhieuDoiId { get; set; } // Mã phiếu đổi
        public string PhieuDoiMa { get; set; } // Mã phiếu đổi
        public int PhieuXuatId { get; set; } // ID phiếu xuất
        public int PhieuMuaVaoId { get; set; } // ID phiếu mua vào
        public decimal TriGiaBan { get; set; } // Giá trị bán
        public decimal TriGiaMua { get; set; } // Giá trị mua
        public decimal ThanhToan { get; set; } // Thanh toán
        public string PhieuXuatMa { get; set; } // Mã phiếu xuất
        public string PhieuMuaMa { get; set; } // Mã phiếu mua vào
    }
}
