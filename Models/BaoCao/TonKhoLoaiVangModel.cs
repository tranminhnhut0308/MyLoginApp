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
        public double CONG_GOC { get; set; }
        public double GIA_CONG { get; set; }
        public double DON_GIA_BAN { get; set; }

        public decimal ThanhTien { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
