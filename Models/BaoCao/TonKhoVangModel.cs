using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyLoginApp.Models.BaoCao
{
    public class TonKhoVangModel
    {
        public string NHOM_TEN { get; set; }
        public decimal CAN_TONG { get; set; }
        public decimal TL_HOT { get; set; }
        public decimal CONG_GOC { get; set; }
        public decimal GIA_CONG { get; set; }
        public decimal DON_GIA_BAN { get; set; }
        public int SL_TON { get; set; }
        public decimal TL_THUC => CAN_TONG - TL_HOT;
        public bool IsGroup { get; set; }
        public decimal ThanhTien
        {
            get
            {
                // Nếu có giá công (GIA_CONG > 0) thì là dòng tổng hợp
                if (GIA_CONG > 0)
                    return (DON_GIA_BAN * (TL_THUC / 100)) + GIA_CONG;
                // Nếu không có giá công thì là dòng chi tiết
                else
                    return (CAN_TONG / 100) * DON_GIA_BAN * SL_TON;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
