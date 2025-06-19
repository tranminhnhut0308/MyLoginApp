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
                            var model = new TonKhoLoaiVangModel
                            {
                                NHOM_TEN = reader["NHOM_TEN"] == DBNull.Value ? string.Empty : reader.GetString("NHOM_TEN"),
                                HANGHOAMA = reader["HANGHOAMA"] == DBNull.Value ? string.Empty : reader.GetString("HANGHOAMA"),
                                HANG_HOA_TEN = reader["HANG_HOA_TEN"] == DBNull.Value ? string.Empty : reader.GetString("HANG_HOA_TEN"),
                                LOAI_TEN = reader["LOAI_TEN"] == DBNull.Value ? string.Empty : reader.GetString("LOAI_TEN"),
                                CAN_TONG = reader["CAN_TONG"] == DBNull.Value ? 0 : reader.GetDouble("CAN_TONG"),
                                TL_HOT = reader["TL_HOT"] == DBNull.Value ? 0 : reader.GetDouble("TL_HOT"),
                                CONG_GOC = reader["CONG_GOC"] == DBNull.Value ? 0 : reader.GetDouble("CONG_GOC"),
                                GIA_CONG = reader["GIA_CONG"] == DBNull.Value ? 0 : reader.GetDouble("GIA_CONG"),
                                DON_GIA_BAN = reader["DON_GIA_BAN"] == DBNull.Value ? 0 : reader.GetDouble("DON_GIA_BAN"),
                            };
                            double truhot = model.CAN_TONG - model.TL_HOT;
                            double truhotquydoi = truhot / 100.0;
                            model.ThanhTien = (decimal)(model.DON_GIA_BAN * truhotquydoi) + (decimal)model.GIA_CONG;
                            DanhSachTonKho.Add(model);
                        }
                        // Sau khi load xong toàn bộ dữ liệu, thực hiện phân trang ban đầu
                        PhanTrangDanhSach();
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
    }

    public static class CollectionExtensions
    {
        public static ObservableCollection<T> ToObservableCollections<T>(this System.Collections.Generic.IEnumerable<T> source)
        {
            return source != null ? new ObservableCollection<T>(source) : new ObservableCollection<T>();
        }
    }
}