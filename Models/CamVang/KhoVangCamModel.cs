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
        public decimal Can_Tong { get; set; }        // Cân tổng
        public decimal TL_Hot { get; set; }          // Trọng lượng hột
        public decimal Don_Gia { get; set; }        // Đơn giá
        public DateTime Ngay_Cam { get; set; }      // Ngày cầm
        public DateTime? Ngay_QuaHan { get; set; }  // Ngày quá hạn (nullable)
        public DateTime? Ngay_ThanhLy { get; set; } // Ngày thanh lý (nullable)

        // Thuộc tính để hiển thị Cân Tổng theo đơn vị Lượng
        public string CanTongFormatted => $"{(Can_Tong / 1000m).ToString("0.############################")} L";

        // Thuộc tính để hiển thị TL Hột theo đơn vị Lượng
        public string TLHotFormatted => $"{(TL_Hot / 1000m).ToString("0.############################")} L";

        // Thuộc tính để hiển thị Đơn Giá với đơn vị VNĐ
        public string DonGiaFormatted => $"{Don_Gia:N0} VNĐ";
    }
}
