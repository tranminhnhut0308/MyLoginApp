using System.ComponentModel;

namespace MyLoginApp.Models
{
    public class HangHoaModel : INotifyPropertyChanged
    {
        public String HangHoaID { get; set; }  // Mã hàng hóa
        public string HangHoaMa { get; set; }  // Mã hàng hóa
        public string TenHangHoa { get; set; }  // Tên hàng hóa
        public string LoaiVang { get; set; }  // Loại vàng
        public string Nhom { get; set; }  // Nhóm hàng hóa
        private decimal _canTong;
        public decimal CanTong
        {
            get => _canTong;
            set
            {
                if (_canTong != value)
                {
                    _canTong = value;
                    OnPropertyChanged(nameof(CanTong));
                    OnPropertyChanged(nameof(TruHot));
                }
            }
        }
        private decimal _trongLuongHot;
        public decimal TrongLuongHot
        {
            get => _trongLuongHot;
            set
            {
                if (_trongLuongHot != value)
                {
                    _trongLuongHot = value;
                    OnPropertyChanged(nameof(TrongLuongHot));
                    OnPropertyChanged(nameof(TruHot));
                }
            }
        }
        public decimal TruHot => CanTong - TrongLuongHot;
        public decimal CongGoc { get; set; }  // Công gốc
        public decimal GiaCong { get; set; }  // Giá công
        public decimal DonViGoc { get; set; }  // Đơn giá gốc
        public decimal SoLuong { get; set; }
        
        // Thuộc tính dùng cho UI
        public bool IsSelected { get; set; }

        public int LoaiId { get; set; }         // ID loại vàng
        public int DvtId { get; set; }          // Đơn vị tính
        public int NhomHangId { get; set; }     // ID nhóm hàng
        public int NccId { get; set; }          // ID nhà cung cấp
        public int GiamGiaId { get; set; }      // ID giảm giá
        public decimal GiaBan { get; set; }     // Giá bán
        public decimal Vat { get; set; }        // VAT
        public decimal Thue { get; set; }       // Thuế
        public int SuDung { get; set; }         // Trạng thái sử dụng
        public int SlIn { get; set; }           // Số lượng in
        public string GhiChu { get; set; }      // Ghi chú
        public string TaoMa { get; set; }       // Tạo mã
        public decimal GiaBanSi { get; set; }   // Giá bán sỉ
        public decimal TuoiBan { get; set; }    // Tuổi bán
        public decimal TuoiMua { get; set; }    // Tuổi mua
        public string XuatXu { get; set; }      // Xuất xứ
        public string KyHieu { get; set; }      // Ký hiệu
        public DateTime Ngay { get; set; }      // Ngày nhập/cập nhật
        public int BoLuong { get; set; }        // Bộ lượng

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}