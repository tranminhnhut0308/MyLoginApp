using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyLoginApp.Models.BaoCao
{
    public partial class PhieuXuatModel : ObservableObject
    {
        [ObservableProperty]
        private int phieuXuatId;

        [ObservableProperty]
        private string phieuXuatMa;

        private string _hangHoaMa;
        public string HangHoaMa
        {
            get => _hangHoaMa;
            set
            {
                if (_hangHoaMa != value)
                {
                    _hangHoaMa = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _hangHoaTen;
        public string HangHoaTen
        {
            get => _hangHoaTen;
            set
            {
                if (_hangHoaTen != value)
                {
                    _hangHoaTen = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _donGia;
        public decimal DonGia
        {
            get => _donGia;
            set
            {
                if (_donGia != value)
                {
                    _donGia = value;
                    OnPropertyChanged();
                    CalculateGiaGocAndLaiLo();
                }
            }
        }

        private DateTime _ngayXuat;
        public DateTime NgayXuat
        {
            get => _ngayXuat;
            set
            {
                if (_ngayXuat != value)
                {
                    _ngayXuat = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _thanhTien;
        public decimal ThanhTien
        {
            get => _thanhTien;
            set
            {
                if (_thanhTien != value)
                {
                    _thanhTien = value;
                    OnPropertyChanged();
                    CalculateGiaGocAndLaiLo();
                }
            }
        }

        private decimal _donGiaGoc;
        public decimal DonGiaGoc
        {
            get => _donGiaGoc;
            set
            {
                if (_donGiaGoc != value)
                {
                    _donGiaGoc = value;
                    OnPropertyChanged();
                    CalculateGiaGocAndLaiLo();
                }
            }
        }

        private decimal _canTong;
        public decimal CanTong
        {
            get => _canTong;
            set
            {
                if (_canTong != value)
                {
                    _canTong = value;
                    OnPropertyChanged();
                    CalculateGiaGocAndLaiLo();
                }
            }
        }

        private decimal _tlHot;
        public decimal TlHot
        {
            get => _tlHot;
            set
            {
                if (_tlHot != value)
                {
                    _tlHot = value;
                    OnPropertyChanged();
                    CalculateGiaGocAndLaiLo();
                }
            }
        }

        public decimal TruHot
        {
            get => CanTong - TlHot;
            set { OnPropertyChanged(); CalculateGiaGocAndLaiLo(); }
        }

        private decimal _congGoc;
        public decimal CongGoc
        {
            get => _congGoc;
            set
            {
                if (_congGoc != value)
                {
                    _congGoc = value;
                    OnPropertyChanged();
                    CalculateGiaGocAndLaiLo();
                }
            }
        }

        private string _loaiVang;
        public string LoaiVang
        {
            get => _loaiVang;
            set
            {
                if (_loaiVang != value)
                {
                    _loaiVang = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _laiLo;
        public decimal LaiLo
        {
            get => _laiLo;
            set
            {
                if (_laiLo != value)
                {
                    _laiLo = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _giaGoc;
        public decimal GiaGoc
        {
            get => _giaGoc;
            set
            {
                if (_giaGoc != value)
                {
                    _giaGoc = value;
                    OnPropertyChanged();
                    CalculateLaiLo();
                }
            }
        }

        public string CanTongFormatted => $"{(CanTong / 1000m):0.####} L";
        public string TlHotFormatted => $"{(TlHot / 1000m):0.####} L";
        public string TruHotFormatted => $"{(TruHot / 1000m):0.####} L";

        // Phương thức tính toán Giá gốc và Lãi/Lỗ
        private void CalculateGiaGocAndLaiLo()
        {
            try
            {
                // Tính Giá gốc theo công thức: DonGiaGoc * TruHot + CongGoc
                GiaGoc = (DonGiaGoc * TruHot) + CongGoc;

                // Sau khi tính xong Giá gốc, phương thức CalculateLaiLo sẽ được tự động gọi
                // qua setter của GiaGoc
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tính giá gốc: {ex.Message}");
                GiaGoc = 0;
            }
        }

        // Phương thức tính Lãi/Lỗ
        private void CalculateLaiLo()
        {
            try
            {
                // Tính Lãi/Lỗ theo công thức: Thành tiền - Giá gốc
                LaiLo = ThanhTien - GiaGoc;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tính lãi lỗ: {ex.Message}");
                LaiLo = 0;
            }
        }

        // Sự kiện PropertyChanged để thông báo binding khi có thuộc tính thay đổi
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
