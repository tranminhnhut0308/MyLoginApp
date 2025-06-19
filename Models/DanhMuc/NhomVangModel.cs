using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyLoginApp.Models
{
    public class NhomVangModel : INotifyPropertyChanged
    {
        // Khóa chính trong database (LOAIID)
        private int _loaiID;
        public int LoaiID
        {
            get => _loaiID;
            set
            {
                if (_loaiID != value)
                {
                    _loaiID = value;
                    OnPropertyChanged();
                }
            }
        }

        // Mã loại (LOAIMA)
        private int _maLoai;
        public int MaLoai
        {
            get => _maLoai;
            set
            {
                if (_maLoai != value)
                {
                    _maLoai = value;
                    OnPropertyChanged();
                }
            }
        }

        // Tên nhóm (LOAI_TEN)
        private string _tenNhom;
        public string TenNhom
        {
            get => _tenNhom;
            set
            {
                if (_tenNhom != value)
                {
                    _tenNhom = value;
                    OnPropertyChanged();
                }
            }
        }

        // Ký hiệu (GHI_CHU)
        private string _kyHieu;
        public string KyHieu
        {
            get => _kyHieu;
            set
            {
                if (_kyHieu != value)
                {
                    _kyHieu = value;
                    OnPropertyChanged();
                }
            }
        }

        // Trạng thái sử dụng (SU_DUNG)
        private int _suDung;
        public int SuDung
        {
            get => _suDung;
            set
            {
                if (_suDung != value)
                {
                    _suDung = value;
                    OnPropertyChanged();
                }
            }
        }

        // Trạng thái được chọn (UI only)
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}