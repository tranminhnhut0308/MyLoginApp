using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoginApp.Models.BaoCao
{
    public class TonKhoNhomVangModel
    {
        public string LOAI_TEN { get; set; }
        public string LOAIMA { get; set; }
        public string HANGHOAMA { get; set; }
        public string HANG_HOA_TEN { get; set; }
        public decimal CAN_TONG { get; set; }
        public decimal TL_HOT { get; set; }
        public decimal TL_VANG { get; set; } // TL_VANG = CAN_TONG - TL_HOT
        public decimal CONG_GOC { get; set; }
        public decimal GIA_CONG { get; set; }
        public string CanTongL => (CAN_TONG / 1000m).ToString("0.#####") + " L";
        public string TLHotL => (TL_HOT / 1000m).ToString("0.#####") + " L";
        public string TLVangL => (TL_VANG / 1000m).ToString("0.#####") + " L";
    }
}
