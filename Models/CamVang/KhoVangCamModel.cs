using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoginApp.Models
{
    public class KhoVangCamModel
    {
        public string Ten_KH { get; set; }          // Tên khách hàng
        public string Ma_Phieu { get; set; }        // Mã phiếu
        public string Ten_Hang { get; set; }        // Tên hàng hóa
        public string Loai_Vang { get; set; }       // Loại vàng
        public double Can_Tong { get; set; }        // Cân tổng
        public double TL_Hot { get; set; }          // Trọng lượng hột
        public decimal Don_Gia { get; set; }        // Đơn giá
        public DateTime Ngay_Cam { get; set; }      // Ngày cầm
        public DateTime? Ngay_QuaHan { get; set; }  // Ngày quá hạn (nullable)
        public DateTime? Ngay_ThanhLy { get; set; } // Ngày thanh lý (nullable)

    }
}
