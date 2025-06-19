using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyLoginApp.Helpers;
using MyLoginApp.Models.BaoCao;
using MySqlConnector;

namespace MyLoginApp.ViewModels
{
    public partial class TonKhoNhomVangViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<TonKhoNhomVangModel> danhSachTonKho = new();

        [ObservableProperty]
        private ObservableCollection<TonKhoNhomVangModel> danhSachHienThi = new();

        [ObservableProperty]
        private ObservableCollection<string> danhSachLoaiVang = new();

        [ObservableProperty]
        private bool isFilterPopupVisible = false;

        [ObservableProperty]
        private string selectedLoaiVang = string.Empty;

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

        public TonKhoNhomVangViewModel()
        {
            // Tải dữ liệu ban đầu
            Task.Run(async () =>
            {
                await LoadDanhSachTonKhoAsync();
                await LoadDanhSachLoaiVangAsync();
            });
        }

        [RelayCommand]
        private async Task LoadDanhSachTonKhoAsync()
        {
            try
            {
                // Ghi log để debug
                Console.WriteLine("LoadDanhSachTonKhoAsync được gọi");
                await LoadTonKhoWithPaginationAsync(CurrentPage, PageSize, TuKhoaTimKiem, SelectedLoaiVang);
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
        private async Task ApplyFilterAsync()
        {
            try
            {
                // Nếu chọn "Tất cả" thì đặt SelectedLoaiVang về rỗng để xóa bộ lọc
                if (SelectedLoaiVang == "Tất cả")
                {
                    SelectedLoaiVang = string.Empty;
                }

                CurrentPage = 1; // Reset về trang đầu tiên khi lọc
                await LoadDanhSachTonKhoAsync();
                IsFilterPopupVisible = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ApplyFilterAsync: {ex.Message}");
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
                IsLoading = true;
                LoadingMessage = "Đang tải dữ liệu tồn kho...";

                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "⚠️ Không thể kết nối đến cơ sở dữ liệu!", "OK");
                    return;
                }

                // Tính toán offset cho phân trang
                int offset = (page - 1) * pageSize;

                // Log thông tin debug
                Console.WriteLine($"Tìm kiếm: '{searchText}', Loại vàng: '{loaiVang}'");

                // Tạo câu truy vấn SQL với điều kiện tìm kiếm
                string whereClause = "ton_kho.sl_ton = 1 and danh_muc_hang_hoa.SU_DUNG = 1";

                // Điều kiện tìm kiếm
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    whereClause += @" AND (
                        loai_hang.LOAI_TEN LIKE @SearchText 
                        OR loai_hang.LOAIMA LIKE @SearchText
                        OR danh_muc_hang_hoa.HANGHOAMA LIKE @SearchText 
                        OR danh_muc_hang_hoa.HANG_HOA_TEN LIKE @SearchText
                    )";
                }

                // Điều kiện lọc theo loại vàng
                if (!string.IsNullOrWhiteSpace(loaiVang) && loaiVang != "Tất cả")
                {
                    whereClause += " AND loai_hang.LOAI_TEN = @LoaiVang";
                }

                // Đếm tổng số bản ghi để tính số trang
                string countQuery = @"
                    SELECT COUNT(*) 
                    FROM danh_muc_hang_hoa, ton_kho, nhom_hang, loai_hang 
                    WHERE danh_muc_hang_hoa.HANGHOAID = ton_kho.HANGHOAID 
                    and danh_muc_hang_hoa.NHOMHANGID = nhom_hang.NHOMHANGID 
                    and loai_hang.LOAIID = danh_muc_hang_hoa.LOAIID 
                    and " + whereClause;

                using var countCmd = new MySqlCommand(countQuery, conn);

                // Thêm tham số tìm kiếm nếu có
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    countCmd.Parameters.AddWithValue("@SearchText", $"%{searchText}%");
                }

                // Thêm tham số lọc theo loại vàng nếu có
                if (!string.IsNullOrWhiteSpace(loaiVang) && loaiVang != "Tất cả")
                {
                    countCmd.Parameters.AddWithValue("@LoaiVang", loaiVang);
                }

                var totalRecords = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                Console.WriteLine($"Tổng số bản ghi tìm thấy: {totalRecords}");

                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                if (TotalPages <= 0) TotalPages = 1; // Đảm bảo có ít nhất 1 trang

                // Cập nhật trạng thái điều hướng
                CanGoNext = CurrentPage < TotalPages;
                CanGoPrevious = CurrentPage > 1;

                // Truy vấn chính lấy dữ liệu với phân trang
                string query = @"
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
                        danh_muc_hang_hoa, ton_kho, nhom_hang, loai_hang 
                    WHERE 
                        danh_muc_hang_hoa.HANGHOAID = ton_kho.HANGHOAID 
                        and danh_muc_hang_hoa.NHOMHANGID = nhom_hang.NHOMHANGID 
                        and loai_hang.LOAIID = danh_muc_hang_hoa.LOAIID 
                        and " + whereClause + @"
                    ORDER BY 
                        CAST(danh_muc_hang_hoa.HANGHOAMA AS UNSIGNED) ASC
                    LIMIT @PageSize OFFSET @Offset";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@Offset", offset);

                // Thêm tham số tìm kiếm nếu có
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    cmd.Parameters.AddWithValue("@SearchText", $"%{searchText}%");
                }

                // Thêm tham số lọc theo loại vàng nếu có
                if (!string.IsNullOrWhiteSpace(loaiVang) && loaiVang != "Tất cả")
                {
                    cmd.Parameters.AddWithValue("@LoaiVang", loaiVang);
                }

                using var reader = await cmd.ExecuteReaderAsync();

                DanhSachTonKho.Clear();

                while (await reader.ReadAsync())
                {
                    try
                    {
                        var tonKho = new TonKhoNhomVangModel
                        {
                            LOAI_TEN = reader.GetString("LOAI_TEN"),
                            LOAIMA = reader.GetString("LOAIMA"),
                            HANGHOAMA = reader.GetString("HANGHOAMA"),
                            HANG_HOA_TEN = reader.GetString("HANG_HOA_TEN"),
                            CAN_TONG = reader.GetDecimal("CAN_TONG"),
                            TL_HOT = reader.GetDecimal("TL_HOT"),
                            TL_VANG = reader.GetDecimal("CAN_TONG") - reader.GetDecimal("TL_HOT"),
                            CONG_GOC = reader.GetDecimal("CONG_GOC"),
                            GIA_CONG = reader.GetDecimal("GIA_CONG")
                        };

                        DanhSachTonKho.Add(tonKho);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi đọc dữ liệu: {ex.Message}");
                    }
                }

                // Cập nhật danh sách hiển thị
                DanhSachHienThi = new ObservableCollection<TonKhoNhomVangModel>(DanhSachTonKho);

                // Cập nhật thông tin debug
                string filterInfo = !string.IsNullOrEmpty(loaiVang) && loaiVang != "Tất cả" ? $" (Loại: {loaiVang})" : "";
                DebugInfo = $"Tìm thấy {DanhSachTonKho.Count} kết quả{filterInfo}. Trang {CurrentPage}/{TotalPages}.";

                Console.WriteLine($"Đã tải {DanhSachTonKho.Count} bản ghi.");
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
