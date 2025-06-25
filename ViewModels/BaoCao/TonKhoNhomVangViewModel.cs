using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyLoginApp.Helpers;
using MyLoginApp.Models.BaoCao;
using MySqlConnector;
using System.Timers;

namespace MyLoginApp.ViewModels
{
    public partial class TonKhoNhomVangViewModel : ObservableObject, IDisposable
    {
        // Timer để tự động làm mới dữ liệu
        private System.Timers.Timer autoRefreshTimer;
        private const int AUTO_REFRESH_INTERVAL = 120000; // 2 phút thay vì 30 giây

        [ObservableProperty]
        private bool isAutoRefreshEnabled = false;

        [ObservableProperty]
        private ObservableCollection<TonKhoNhomVangModel> danhSachTonKho = new();

        [ObservableProperty]
        private ObservableCollection<TonKhoNhomVangModel> danhSachHienThi = new();

        [ObservableProperty]
        private ObservableCollection<string> danhSachLoaiVang = new();

        [ObservableProperty]
        private bool isFilterPopupVisible = false;

        // Thuộc tính cho loại vàng được chọn với tính năng tự động reset
        private string _loaiVangDuocChon = string.Empty;
        public string LoaiVangDuocChon
        {
            get => _loaiVangDuocChon;
            set
            {
                SetProperty(ref _loaiVangDuocChon, value);
            }
        }

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages = 1;

        [ObservableProperty]
        private int pageSize = 10;

        [ObservableProperty]
        private bool canGoNext = false;

        [ObservableProperty]
        private bool canGoPrevious = false;

        [ObservableProperty]
        private string tuKhoaTimKiem = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string loadingMessage = string.Empty;

        [ObservableProperty]
        private string debugInfo = string.Empty;

        [ObservableProperty]
        private decimal tongCanTong;
        [ObservableProperty]
        private decimal tongTLHot;
        [ObservableProperty]
        private decimal tongTLVang;
        [ObservableProperty]
        private decimal tongCongGoc;
        [ObservableProperty]
        private decimal tongGiaCong;

        [ObservableProperty]
        private int tongSoPhieu;

        private ObservableCollection<TonKhoNhomVangModel> danhSachTonKhoFull = new();

        public TonKhoNhomVangViewModel()
        {
            // Khởi tạo timer tự động làm mới dữ liệu
            autoRefreshTimer = new System.Timers.Timer(AUTO_REFRESH_INTERVAL);
            autoRefreshTimer.Elapsed += AutoRefreshTimer_Elapsed;
            autoRefreshTimer.AutoReset = true;
            
            // Không tự động khởi động timer vì mặc định tắt auto refresh
            // if (IsAutoRefreshEnabled)
            // {
            //     autoRefreshTimer.Start();
            // }

            // Tải dữ liệu ban đầu
            Task.Run(async () =>
            {
                await LoadDanhSachTonKhoAsync();
                await LoadDanhSachLoaiVangAsync();
            });
        }

        private async void AutoRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    Console.WriteLine("Đang tự động làm mới dữ liệu...");
                    // Chỉ refresh dữ liệu tồn kho, không refresh danh sách loại vàng
                    // để tránh làm mất bộ lọc hiện tại
                    await LoadDanhSachTonKhoAsync();
                    Console.WriteLine("Đã làm mới dữ liệu thành công!");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tự động làm mới: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ToggleAutoRefresh()
        {
            IsAutoRefreshEnabled = !IsAutoRefreshEnabled;
            if (IsAutoRefreshEnabled)
            {
                autoRefreshTimer.Start();
                Console.WriteLine("Đã bật tự động làm mới");
            }
            else
            {
                autoRefreshTimer.Stop();
                Console.WriteLine("Đã tắt tự động làm mới");
            }
        }

        public void Dispose()
        {
            autoRefreshTimer?.Dispose();
        }

        [RelayCommand]
        private async Task LoadDanhSachTonKhoAsync()
        {
            try
            {
                // Ghi log để debug
                Console.WriteLine("LoadDanhSachTonKhoAsync được gọi");
                await LoadTonKhoWithPaginationAsync(CurrentPage, PageSize, TuKhoaTimKiem, LoaiVangDuocChon);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LoadDanhSachTonKhoAsync: {ex.Message}");
                DebugInfo = $"Lỗi: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ShowFilterPopup()
        {
            IsFilterPopupVisible = true;
        }

        [RelayCommand]
        private async Task ApplyFilter(string loaiVang)
        {
            try
            {
                Console.WriteLine($"ApplyFilter được gọi với loaiVang: '{loaiVang}'");
                
                // Nếu chọn "Tất cả" thì đặt LoaiVangDuocChon về rỗng để xóa bộ lọc
                LoaiVangDuocChon = loaiVang == "Tất cả" ? string.Empty : loaiVang;
                
                Console.WriteLine($"LoaiVangDuocChon sau khi set: '{LoaiVangDuocChon}'");

                CurrentPage = 1; // Reset về trang đầu tiên khi lọc
                await LoadDanhSachTonKhoAsync();
                
                // Đóng popup filter
                IsFilterPopupVisible = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ApplyFilter: {ex.Message}");
                DebugInfo = $"Lỗi lọc: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CancelFilter()
        {
            IsFilterPopupVisible = false;
        }

        [RelayCommand]
        private async Task GoNextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadDanhSachTonKhoAsync();
            }
        }

        [RelayCommand]
        private async Task GoPreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadDanhSachTonKhoAsync();
            }
        }

        [RelayCommand]
        private async Task ThucHienTimKiem()
        {
            try
            {
                Console.WriteLine("ThucHienTimKiem được gọi");
                CurrentPage = 1; // Reset về trang đầu tiên khi tìm kiếm
                await LoadDanhSachTonKhoAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ThucHienTimKiem: {ex.Message}");
                DebugInfo = $"Lỗi tìm kiếm: {ex.Message}";
            }
        }

        private async Task LoadDanhSachLoaiVangAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Đang tải danh sách loại vàng...";

                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "⚠️ Không thể kết nối đến cơ sở dữ liệu!", "OK");
                    return;
                }

                string query = @"
                    SELECT DISTINCT loai_hang.LOAI_TEN
                    FROM loai_hang
                    JOIN danh_muc_hang_hoa ON loai_hang.LOAIID = danh_muc_hang_hoa.LOAIID
                    JOIN ton_kho ON danh_muc_hang_hoa.HANGHOAID = ton_kho.HANGHOAID
                    WHERE ton_kho.sl_ton = 1 AND danh_muc_hang_hoa.SU_DUNG = 1
                    ORDER BY loai_hang.LOAI_TEN";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                DanhSachLoaiVang.Clear();
                DanhSachLoaiVang.Add("Tất cả"); // Thêm tùy chọn Tất cả

                while (await reader.ReadAsync())
                {
                    string loaiTen = reader.GetString("LOAI_TEN");
                    DanhSachLoaiVang.Add(loaiTen);
                }

                Console.WriteLine($"Đã tải {DanhSachLoaiVang.Count - 1} loại vàng.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LoadDanhSachLoaiVangAsync: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTonKhoWithPaginationAsync(int page, int pageSize, string searchText = "", string loaiVang = "")
        {
            try
            {
                Console.WriteLine($"LoadTonKhoWithPaginationAsync - page: {page}, pageSize: {pageSize}, searchText: '{searchText}', loaiVang: '{loaiVang}'");
                
                IsLoading = true;
                LoadingMessage = "Đang tải dữ liệu tồn kho...";

                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "⚠️ Không thể kết nối đến cơ sở dữ liệu!", "OK");
                    return;
                }

                // Câu truy vấn cho toàn bộ dữ liệu
                string queryFull = @"
                    SELECT 
                        loai_hang.LOAI_TEN,
                        loai_hang.LOAIMA,
                        danh_muc_hang_hoa.HANGHOAMA,
                        danh_muc_hang_hoa.HANG_HOA_TEN,
                        danh_muc_hang_hoa.CAN_TONG,
                        danh_muc_hang_hoa.TL_HOT,
                        danh_muc_hang_hoa.CONG_GOC,
                        danh_muc_hang_hoa.GIA_CONG
                    FROM 
                        danh_muc_hang_hoa
                        JOIN ton_kho ON danh_muc_hang_hoa.HANGHOAID = ton_kho.HANGHOAID
                        JOIN loai_hang ON danh_muc_hang_hoa.LOAIID = loai_hang.LOAIID
                    WHERE 
                        ton_kho.SL_TON = 1 
                        AND danh_muc_hang_hoa.SU_DUNG = 1";

                // Thêm điều kiện tìm kiếm nếu có
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    queryFull += @" AND (
                        danh_muc_hang_hoa.HANG_HOA_TEN LIKE @SearchText OR 
                        danh_muc_hang_hoa.HANGHOAMA LIKE @SearchText OR
                        loai_hang.LOAI_TEN LIKE @SearchText
                    )";
                }

                // Thêm điều kiện lọc theo loại vàng nếu có
                if (!string.IsNullOrWhiteSpace(loaiVang) && loaiVang != "Tất cả")
                {
                    queryFull += " AND loai_hang.LOAI_TEN = @LoaiVang";
                    Console.WriteLine($"Thêm bộ lọc loại vàng: '{loaiVang}'");
                }
                else
                {
                    Console.WriteLine("Không áp dụng bộ lọc loại vàng");
                }

                queryFull += " ORDER BY loai_hang.LOAI_TEN, danh_muc_hang_hoa.HANG_HOA_TEN";

                using var cmdFull = new MySqlCommand(queryFull, conn);
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    cmdFull.Parameters.AddWithValue("@SearchText", $"%{searchText}%");
                }
                if (!string.IsNullOrWhiteSpace(loaiVang) && loaiVang != "Tất cả")
                {
                    cmdFull.Parameters.AddWithValue("@LoaiVang", loaiVang);
                }
                using var readerFull = await cmdFull.ExecuteReaderAsync();

                danhSachTonKhoFull.Clear();
                decimal sumCanTong = 0;
                decimal sumTLHot = 0;
                decimal sumTLVang = 0;
                decimal sumCongGoc = 0;
                decimal sumGiaCong = 0;
                while (await readerFull.ReadAsync())
                {
                    var tonKho = new TonKhoNhomVangModel
                    {
                        LOAI_TEN = readerFull.GetString("LOAI_TEN"),
                        LOAIMA = readerFull.GetString("LOAIMA"),
                        HANGHOAMA = readerFull.GetString("HANGHOAMA"),
                        HANG_HOA_TEN = readerFull.GetString("HANG_HOA_TEN"),
                        CAN_TONG = readerFull.GetDecimal("CAN_TONG"),
                        TL_HOT = readerFull.GetDecimal("TL_HOT"),
                        TL_VANG = readerFull.GetDecimal("CAN_TONG") - readerFull.GetDecimal("TL_HOT"),
                        CONG_GOC = readerFull.GetDecimal("CONG_GOC"),
                        GIA_CONG = readerFull.GetDecimal("GIA_CONG")
                    };
                    danhSachTonKhoFull.Add(tonKho);
                    sumCanTong += tonKho.CAN_TONG;
                    sumTLHot += tonKho.TL_HOT;
                    sumTLVang += tonKho.TL_VANG;
                    sumCongGoc += tonKho.CONG_GOC;
                    sumGiaCong += tonKho.GIA_CONG;
                }
                TongCanTong = sumCanTong;
                TongTLHot = sumTLHot;
                TongTLVang = sumTLVang;
                TongCongGoc = sumCongGoc;
                TongGiaCong = sumGiaCong;
                TongSoPhieu = danhSachTonKhoFull.Count;

                // Phân trang dữ liệu để hiển thị
                DanhSachTonKho.Clear();
                int offset = (page - 1) * pageSize;
                var pageData = danhSachTonKhoFull.Skip(offset).Take(pageSize).ToList();
                foreach (var item in pageData)
                {
                    DanhSachTonKho.Add(item);
                }
                DanhSachHienThi = new ObservableCollection<TonKhoNhomVangModel>(DanhSachTonKho);

                // Tính lại tổng số trang
                TotalPages = (int)Math.Ceiling((double)danhSachTonKhoFull.Count / pageSize);
                if (TotalPages <= 0) TotalPages = 1;
                CanGoNext = CurrentPage < TotalPages;
                CanGoPrevious = CurrentPage > 1;

                DebugInfo = $"Tìm thấy {DanhSachTonKho.Count} kết quả. Trang {CurrentPage}/{TotalPages}.";
                Console.WriteLine($"LoadTonKhoWithPaginationAsync hoàn thành - Tổng: {danhSachTonKhoFull.Count}, Hiển thị: {DanhSachTonKho.Count}, Trang: {CurrentPage}/{TotalPages}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LoadTonKhoWithPaginationAsync: {ex.Message}");
                DebugInfo = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
