using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoginApp.Models
{
    public class PhieuQuaHanModel
    {
        public string Ma_Phieu { get; set; }
        public string Ten_KH { get; set; }
        public DateTime? TU_NGAY { get; set; } 
        public DateTime? DEN_NGAY { get; set; }
        public int SONGAY { get; set; }
        public double TL_Thuc { get; set; }
        public double Dinh_Gia { get; set; }
        public double TIEN_KHACH_NHAN { get; set; }
        public double LAI_XUAT { get; set; }

    }
}
