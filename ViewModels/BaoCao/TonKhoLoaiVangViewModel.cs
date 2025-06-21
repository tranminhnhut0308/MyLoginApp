using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyLoginApp.Helpers;
using MyLoginApp.Models.BaoCao;
using MySqlConnector;

namespace MyLoginApp.ViewModels.BaoCao
{
    public class TonKhoLoaiVangViewModel : INotifyPropertyChanged
    {
        private int _pageSize = 10; // Số lượng item trên mỗi trang
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    OnPropertyChanged();
                    PhanTrangDanhSach(); // Thực hiện phân trang lại khi thay đổi kích thước trang
                }
            }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    PhanTrangDanhSach(); // Thực hiện phân trang lại khi thay đổi trang
                }
            }
        }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            set
            {
                if (_totalItems != value)
                {
                    _totalItems = value;
                    OnPropertyChanged();
                    TotalPages = (int)Math.Ceiling((double)_totalItems / PageSize);
                    OnPropertyChanged(nameof(TotalPages));
                }
            }
        }

        public int TotalPages { get; private set; }

        private ObservableCollection<TonKhoLoaiVangModel> _danhSachTonKho;
        public ObservableCollection<TonKhoLoaiVangModel> DanhSachTonKho
        {
            get => _danhSachTonKho;
            set
            {
                if (_danhSachTonKho != value)
                {
                    _danhSachTonKho = value;
                    OnPropertyChanged();
                    // Khi dữ liệu gốc thay đổi, cần thực hiện phân trang lại
                    PhanTrangDanhSach();
                }
            }
        }

        private ObservableCollection<TonKhoLoaiVangModel> _danhSachHienThi;
        public ObservableCollection<TonKhoLoaiVangModel> DanhSachHienThi
        {
            get => _danhSachHienThi ?? DanhSachTonKho;
            set
            {
                _danhSachHienThi = value;
                OnPropertyChanged();
                PhanTrangDanhSach(); // Thực hiện phân trang lại khi thay đổi kết quả tìm kiếm
            }
        }

        private ObservableCollection<TonKhoLoaiVangModel> _danhSachHienThiDaPhanTrang;
        public ObservableCollection<TonKhoLoaiVangModel> DanhSachHienThiDaPhanTrang
        {
            get => _danhSachHienThiDaPhanTrang;
            set
            {
                _danhSachHienThiDaPhanTrang = value;
                OnPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _loadingMessage;
        public string LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                if (_loadingMessage != value)
                {
                    _loadingMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _tuKhoaTimKiem;
        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set
            {
                if (_tuKhoaTimKiem != value)
                {
                    _tuKhoaTimKiem = value;
                    OnPropertyChanged();
                    ThucHienTimKiem(); // Gọi tìm kiếm mỗi khi từ khóa thay đổi
                }
            }
        }

        public Command LoadDanhSachTonKhoCommand { get; }
        public Command ThucHienTimKiemCommand { get; }
        public Command GoToPreviousPageCommand { get; }
        public Command GoToNextPageCommand { get; }

        // Summary Properties
        private int _tongSoLoaiVang;
        public int TongSoLoaiVang
        {
            get => _tongSoLoaiVang;
            set { _tongSoLoaiVang = value; OnPropertyChanged(); }
        }

        private double _tongCanTong;
        public double TongCanTong
        {
            get => _tongCanTong;
            set 
            { 
                _tongCanTong = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(TongCanTongFormatted));
            }
        }

        private double _tongTLHot;
        public double TongTLHot
        {
            get => _tongTLHot;
            set 
            { 
                _tongTLHot = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(TongTLHotFormatted));
            }
        }

        private decimal _tongThanhTien;
        public decimal TongThanhTien
        {
            get => _tongThanhTien;
            set { _tongThanhTien = value; OnPropertyChanged(); }
        }

        private decimal _tongCongGoc;
        public decimal TongCongGoc
        {
            get => _tongCongGoc;
            set { _tongCongGoc = value; OnPropertyChanged(); }
        }

        private decimal _tongGiaCong;
        public decimal TongGiaCong
        {
            get => _tongGiaCong;
            set { _tongGiaCong = value; OnPropertyChanged(); }
        }

        // Formatted Totals
        public string TongCanTongFormatted => $"{(TongCanTong / 1000).ToString("0.############################")} L";
        public string TongTLHotFormatted => $"{(TongTLHot / 1000).ToString("0.############################")} L";

        public TonKhoLoaiVangViewModel()
        {
            DanhSachTonKho = new ObservableCollection<TonKhoLoaiVangModel>();
            DanhSachHienThi = DanhSachTonKho; // Để giữ toàn bộ dữ liệu sau tìm kiếm
            DanhSachHienThiDaPhanTrang = new ObservableCollection<TonKhoLoaiVangModel>();
            LoadDanhSachTonKhoCommand = new Command(async () => await LoadDanhSachTonKho());
            ThucHienTimKiemCommand = new Command(ThucHienTimKiem);
            GoToPreviousPageCommand = new Command(() => { if (CurrentPage > 1) CurrentPage--; });
            GoToNextPageCommand = new Command(() => { if (CurrentPage < TotalPages) CurrentPage++; });

            // Tải dữ liệu ban đầu
            Task.Run(async () => await LoadDanhSachTonKho());
        }

        private async Task LoadDanhSachTonKho()
        {
            IsBusy = true;
            LoadingMessage = "Đang tải dữ liệu tồn kho...";

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;
                string query = @"
                    SELECT
                        nhom_hang.NHOM_TEN,
                        danh_muc_hang_hoa.HANGHOAMA,
                        danh_muc_hang_hoa.HANG_HOA_TEN,
                        loai_hang.LOAI_TEN,
                        danh_muc_hang_hoa.CAN_TONG,
                        danh_muc_hang_hoa.TL_HOT,
                        danh_muc_hang_hoa.CONG_GOC,
                        danh_muc_hang_hoa.GIA_CONG,
                        nhom_hang.DON_GIA_BAN
                    FROM
                        danh_muc_hang_hoa,
                        ton_kho,
                        nhom_hang,
                        loai_hang
                    WHERE
                        danh_muc_hang_hoa.HANGHOAID = ton_kho.HANGHOAID
                        AND danh_muc_hang_hoa.NHOMHANGID = nhom_hang.NHOMHANGID
                        AND danh_muc_hang_hoa.LOAIID = loai_hang.LOAIID
                        AND ton_kho.sl_ton = 1
                        AND danh_muc_hang_hoa.SU_DUNG = 1
                        AND nhom_hang.SU_DUNG = 1;";
                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        DanhSachTonKho.Clear();
                        while (await reader.ReadAsync())
                        {
                            var item = new TonKhoLoaiVangModel
                            {
                                NHOM_TEN = reader["NHOM_TEN"].ToString(),
                                HANGHOAMA = reader["HANGHOAMA"].ToString(),
                                HANG_HOA_TEN = reader["HANG_HOA_TEN"].ToString(),
                                LOAI_TEN = reader["LOAI_TEN"].ToString(),
                                CAN_TONG = Convert.ToDouble(reader["CAN_TONG"]),
                                TL_HOT = Convert.ToDouble(reader["TL_HOT"]),
                                CONG_GOC = Convert.ToDecimal(reader["CONG_GOC"]),
                                GIA_CONG = Convert.ToDecimal(reader["GIA_CONG"]),
                                DON_GIA_BAN = Convert.ToDecimal(reader["DON_GIA_BAN"])
                            };
                            DanhSachTonKho.Add(item);
                        }
                        // Sau khi load xong toàn bộ dữ liệu, thực hiện phân trang ban đầu
                        PhanTrangDanhSach();

                        // Update summary
                        CalculateSummary();
                    }
                    else
                    {
                        LoadingMessage = "Không có dữ liệu hàng hóa phù hợp.";
                        DanhSachTonKho.Clear();
                        PhanTrangDanhSach(); // Để cập nhật TotalPages về 0
                    }
                }
                else
                {
                    LoadingMessage = "Lỗi khi đọc dữ liệu từ database.";
                    DanhSachTonKho.Clear();
                    PhanTrangDanhSach(); // Để cập nhật TotalPages về 0
                }
            }
            catch (Exception ex)
            {
                LoadingMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
                Console.WriteLine($"Lỗi LoadDanhSachTonKho: {ex}");
                DanhSachTonKho.Clear();
                PhanTrangDanhSach(); // Để cập nhật TotalPages về 0
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CalculateSummary()
        {
            var sourceList = string.IsNullOrEmpty(TuKhoaTimKiem) ? DanhSachTonKho : DanhSachHienThi;
            TongCanTong = sourceList.Sum(x => x.CAN_TONG);
            TongTLHot = sourceList.Sum(x => x.TL_HOT);
            TongThanhTien = sourceList.Sum(x => x.ThanhTien);
            TongSoLoaiVang = sourceList.Select(x => x.NHOM_TEN).Distinct().Count();
            TongCongGoc = sourceList.Sum(x => x.CONG_GOC);
            TongGiaCong = sourceList.Sum(x => x.GIA_CONG);
        }

        private void ThucHienTimKiem()
        {
            if (string.IsNullOrWhiteSpace(TuKhoaTimKiem))
            {
                DanhSachHienThi = DanhSachTonKho;
            }
            else
            {
                var ketQuaTimKiem = DanhSachTonKho.Where(item =>
                    item.HANG_HOA_TEN.ToLower().Contains(TuKhoaTimKiem.ToLower()) ||
                    item.NHOM_TEN.ToLower().Contains(TuKhoaTimKiem.ToLower()) ||
                    item.LOAI_TEN.ToLower().Contains(TuKhoaTimKiem.ToLower()) ||
                    item.HANGHOAMA.ToLower().Contains(TuKhoaTimKiem.ToLower())
                ).ToObservableCollections();
                DanhSachHienThi = ketQuaTimKiem;
            }
            // Sau khi tìm kiếm, thực hiện phân trang trên kết quả
            CurrentPage = 1; // Reset về trang đầu tiên sau khi tìm kiếm
            PhanTrangDanhSach();

            // Update summary after search
            CalculateSummary();
        }

        private void PhanTrangDanhSach()
        {
            TotalItems = DanhSachHienThi.Count;
            DanhSachHienThiDaPhanTrang = new ObservableCollection<TonKhoLoaiVangModel>(
                DanhSachHienThi.Skip((CurrentPage - 1) * PageSize).Take(PageSize)
            );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;
    }

    public static class CollectionExtensions
    {
        public static ObservableCollection<T> ToObservableCollections<T>(this System.Collections.Generic.IEnumerable<T> source)
        {
            return source != null ? new ObservableCollection<T>(source) : new ObservableCollection<T>();
        }
    }
}