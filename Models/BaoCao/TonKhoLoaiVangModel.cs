using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyLoginApp.Models.BaoCao
{
    public class TonKhoLoaiVangModel
    {
        public string NHOM_TEN { get; set; }
        public string HANGHOAMA { get; set; }
        public string HANG_HOA_TEN { get; set; }
        public string LOAI_TEN { get; set; }
        public double CAN_TONG { get; set; }
        public double TL_HOT { get; set; }
        public decimal CONG_GOC { get; set; }
        public decimal GIA_CONG { get; set; }
        public decimal DON_GIA_BAN { get; set; }

        public decimal ThanhTien => (((decimal)CAN_TONG - (decimal)TL_HOT) / 100m) * DON_GIA_BAN + GIA_CONG;

        // Formatted properties
        public string CanTongFormatted => $"{(CAN_TONG / 1000).ToString("0.############################")} L";
        public string TlHotFormatted => $"{(TL_HOT / 1000).ToString("0.############################")} L";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
