using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoginApp.Models
{
    public class PhieuDangCamModel
    {
        public string MaPhieu { get; set; }
        public string TenKhachHang { get; set; }
        public DateTime NgayCam { get; set; }
        public DateTime NgayQuaHan { get; set; }
        public decimal CanTong { get; set; }
        public decimal TLThuc { get; set; }
        public decimal DinhGia { get; set; }
        public decimal TienKhachNhan { get; set; }
        public decimal TienNhanThem { get; set; }
        public decimal TienCamMoi { get; set; }
        public decimal LaiSuat { get; set; }

        public decimal CanTongInLuong => CanTong / 1000m;
        public decimal TLThucInLuong => TLThuc / 1000m;
    }


}
