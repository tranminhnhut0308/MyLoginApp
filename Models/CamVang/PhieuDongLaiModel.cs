using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoginApp.Models
{
    public class PhieuDongLaiModel
    {
        public int PhieuCamId { get; set; }
        public string MaPhieuCam { get; set; }
        public string MaPhieuDongLai { get; set; }
        public string NgayCam { get; set; }
        public string NgayDongLai { get; set; }
        public string TenKhachHang { get; set; }
        public decimal KhachDongTien { get; set; }

    }
}
