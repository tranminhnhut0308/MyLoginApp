using System.ComponentModel;
using System.Globalization;

namespace MyLoginApp.Models
{
    public class LoaiVangModel : INotifyPropertyChanged
    {
        public int NhomHangID { get; set; }
        public string NhomHangMA { get; set; }
        public int NhomChaID { get; set; }
        public string TenLoaiVang { get; set; }
        
        private decimal? _donGiaVon;
        public decimal? DonGiaVon
        {
            get => _donGiaVon;
            set
            {
                if (_donGiaVon != value)
                {
                    _donGiaVon = value;
                    OnPropertyChanged(nameof(DonGiaVon));
                }
            }
        }
        
        private decimal? _donGiaMua;
        public decimal? DonGiaMua
        {
            get => _donGiaMua;
            set
            {
                if (_donGiaMua != value)
                {
                    _donGiaMua = value;
                    OnPropertyChanged(nameof(DonGiaMua));
                }
            }
        }
        
        private decimal? _donGiaBan;
        public decimal? DonGiaBan
        {
            get => _donGiaBan;
            set
            {
                if (_donGiaBan != value)
                {
                    _donGiaBan = value;
                    OnPropertyChanged(nameof(DonGiaBan));
                }
            }
        }
        
        private decimal? _donGiaCam;
        public decimal? DonGiaCam
        {
            get => _donGiaCam;
            set
            {
                if (_donGiaCam != value)
                {
                    _donGiaCam = value;
                    OnPropertyChanged(nameof(DonGiaCam));
                }
            }
        }
        
        public string MuaBan { get; set; }
        public int SuDung { get; set; }
        public string GhiChu { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Giữ class LoaiVang cũ để tương thích nếu cần
    public class LoaiVang : INotifyPropertyChanged
    {
        public string TenLoaiVang { get; set; }
        public double? DonGiaVon { get; set; }
        public double? DonGiaMua { get; set; }
        public double? DonGiaBan { get; set; }
        public double? DonGiaCam { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
