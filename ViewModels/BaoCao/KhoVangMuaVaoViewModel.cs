using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyLoginApp.Helpers;
using MyLoginApp.Models.BaoCao;
using MySqlConnector;


namespace MyLoginApp.ViewModels.BaoCao
{
    public partial class KhoVangMuaVaoViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<KhoVangMuaVaoModel> danhSachKhoVang = new();

        [ObservableProperty]
        private ObservableCollection<KhoVangMuaVaoModel> danhSachHienThi = new();

        [ObservableProperty]
        private ObservableCollection<string> danhSachNhomVang = new();

        [ObservableProperty]
        private ObservableCollection<string> danhSachTenHangHoa = new();

        [ObservableProperty]
        private bool isFilterPopupVisible = false;

        [ObservableProperty]
        private string selectedNhomVang = string.Empty;

        [ObservableProperty]
        private string selectedTenHangHoa = string.Empty;

        [ObservableProperty]
        private string filterTenHangHoa = string.Empty;

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

        public KhoVangMuaVaoViewModel()
        {
            // Tải dữ liệu ban đầu
            Task.Run(async () =>
            {
                await LoadDanhSachKhoVangAsync();
                await LoadDanhSachNhomVangAsync();
            });
        }

        [RelayCommand]
        private async Task LoadDanhSachKhoVangAsync()
        {
            await LoadKhoVangWithPaginationAsync(CurrentPage, PageSize, TuKhoaTimKiem, SelectedNhomVang);
        }

        [RelayCommand]
        private async Task LoadDanhSachNhomVangAsync()
        {
            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "SELECT DISTINCT NHOM_TEN FROM nhom_hang WHERE SU_DUNG = 1 ORDER BY NHOM_TEN";

                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                var nhomVangList = new ObservableCollection<string>();
                nhomVangList.Add("Tất cả"); // Thêm lựa chọn "Tất cả"

                while (await reader.ReadAsync())
                {
                    nhomVangList.Add(reader.GetString("NHOM_TEN"));
                }

                DanhSachNhomVang = nhomVangList;
                SelectedNhomVang = "Tất cả"; // Mặc định chọn "Tất cả"
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LoadDanhSachNhomVangAsync: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ThucHienTimKiem()
        {
            CurrentPage = 1; // Reset về trang 1 khi tìm kiếm
            Task.Run(async () => await LoadKhoVangWithPaginationAsync(CurrentPage, PageSize, TuKhoaTimKiem, SelectedNhomVang));
        }

        [RelayCommand]
        private void ShowFilterPopup()
        {
            IsFilterPopupVisible = true;
        }

        [RelayCommand]
        private void HideFilterPopup()
        {
            IsFilterPopupVisible = false;
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            IsFilterPopupVisible = false;
            CurrentPage = 1; // Reset về trang 1 khi áp dụng bộ lọc

            Task.Run(async () => await LoadKhoVangWithPaginationAsync(CurrentPage, PageSize, TuKhoaTimKiem, SelectedNhomVang));
        }

        [RelayCommand]
        private void GoNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                Task.Run(async () => await LoadKhoVangWithPaginationAsync(CurrentPage, PageSize, TuKhoaTimKiem, SelectedNhomVang));
            }
        }

        [RelayCommand]
        private void GoPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                Task.Run(async () => await LoadKhoVangWithPaginationAsync(CurrentPage, PageSize, TuKhoaTimKiem, SelectedNhomVang));
            }
        }

        // Theo dõi thay đổi của SelectedNhomVang
        partial void OnSelectedNhomVangChanged(string value)
        {
            // Khi thay đổi nhóm vàng, load lại danh sách tên hàng hóa
            Task.Run(async () => await LoadDanhSachTenHangHoaAsync(value));
        }

        [RelayCommand]
        private async Task LoadDanhSachTenHangHoaAsync(string nhomVang)
        {
            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = @"
                    SELECT DISTINCT TEN_HANG_HOA 
                    FROM phm_kho_vang_mua_vao kvmv
                    JOIN nhom_hang nh ON nh.NHOMHANGID = kvmv.NHOMHANGID
                    WHERE kvmv.DA_XUAT = 0";

                // Thêm điều kiện lọc theo nhóm vàng nếu có
                if (!string.IsNullOrWhiteSpace(nhomVang) && nhomVang != "Tất cả")
                {
                    query += " AND nh.NHOM_TEN = @nhomVang";
                }

                query += " ORDER BY TEN_HANG_HOA";

                await using var cmd = new MySqlCommand(query, conn);

                if (!string.IsNullOrWhiteSpace(nhomVang) && nhomVang != "Tất cả")
                {
                    cmd.Parameters.AddWithValue("@nhomVang", nhomVang);
                }

                await using var reader = await cmd.ExecuteReaderAsync();

                var tenHangList = new ObservableCollection<string>();
                tenHangList.Add("Tất cả"); // Thêm lựa chọn "Tất cả"

                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0))
                    {
                        string tenHang = reader.GetString(0);
                        if (!string.IsNullOrWhiteSpace(tenHang))
                        {
                            tenHangList.Add(tenHang);
                        }
                    }
                }

                // Cập nhật UI trên main thread
                Device.BeginInvokeOnMainThread(() =>
                {
                    DanhSachTenHangHoa = tenHangList;
                    SelectedTenHangHoa = "Tất cả"; // Mặc định chọn "Tất cả"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LoadDanhSachTenHangHoaAsync: {ex.Message}");
            }
        }

        private async Task LoadKhoVangWithPaginationAsync(int page, int pageSize, string searchTerm = "", string nhomVang = "")
        {
            if (IsLoading) return; // Tránh gọi đồng thời nhiều lần

            IsLoading = true;
            LoadingMessage = "Đang tải dữ liệu kho vàng mua vào...";

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    IsLoading = false;
                    return;
                }

                // Xây dựng câu truy vấn với pagination và điều kiện search
                string countQuery = @"
                SELECT COUNT(*) AS total_rows
                FROM 
                    phm_kho_vang_mua_vao kvmv
                JOIN 
                    nhom_hang nh ON nh.NHOMHANGID = kvmv.NHOMHANGID
                WHERE 
                    kvmv.DA_XUAT = 0";

                string dataQuery = @"
                SELECT 
                    kvmv.TEN_HANG_HOA,
                    nh.NHOM_TEN AS NHOM_TEN,
                    kvmv.CAN_TONG,
                    kvmv.TL_LOC,
                    kvmv.TL_HOT,
                    kvmv.TL_THUC,
                    kvmv.SO_LUONG,
                    kvmv.DON_GIA,
                    kvmv.THANH_TIEN
                FROM 
                    phm_kho_vang_mua_vao kvmv
                JOIN 
                    nhom_hang nh ON nh.NHOMHANGID = kvmv.NHOMHANGID
                WHERE 
                    kvmv.DA_XUAT = 0";

                // Thêm điều kiện tìm kiếm nếu có
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    string searchCondition = $" AND (kvmv.TEN_HANG_HOA LIKE @searchTerm OR nh.NHOM_TEN LIKE @searchTerm)";
                    countQuery += searchCondition;
                    dataQuery += searchCondition;
                }

                // Thêm điều kiện lọc theo nhóm vàng nếu có
                if (!string.IsNullOrWhiteSpace(nhomVang) && nhomVang != "Tất cả")
                {
                    string nhomVangCondition = " AND nh.NHOM_TEN = @nhomVang";
                    countQuery += nhomVangCondition;
                    dataQuery += nhomVangCondition;
                }

                // Thêm điều kiện lọc theo tên hàng nếu có
                if (!string.IsNullOrWhiteSpace(SelectedTenHangHoa) && SelectedTenHangHoa != "Tất cả")
                {
                    string tenHangCondition = " AND kvmv.TEN_HANG_HOA = @tenHang";
                    countQuery += tenHangCondition;
                    dataQuery += tenHangCondition;
                }

                // Thêm ORDER BY và LIMIT cho query chính
                dataQuery += " ORDER BY kvmv.TEN_HANG_HOA";
                dataQuery += " LIMIT @offset, @limit";

                // Tính toán tổng số bản ghi và tổng số trang
                int totalRows = 0;
                await using (var countCmd = new MySqlCommand(countQuery, conn))
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        countCmd.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");
                    }

                    if (!string.IsNullOrWhiteSpace(nhomVang) && nhomVang != "Tất cả")
                    {
                        countCmd.Parameters.AddWithValue("@nhomVang", nhomVang);
                    }

                    if (!string.IsNullOrWhiteSpace(SelectedTenHangHoa) && SelectedTenHangHoa != "Tất cả")
                    {
                        countCmd.Parameters.AddWithValue("@tenHang", SelectedTenHangHoa);
                    }

                    var result = await countCmd.ExecuteScalarAsync();
                    totalRows = Convert.ToInt32(result);
                }

                TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
                if (CurrentPage > TotalPages && TotalPages > 0)
                {
                    CurrentPage = TotalPages;
                }

                CanGoNext = CurrentPage < TotalPages;
                CanGoPrevious = CurrentPage > 1;

                // Truy vấn dữ liệu với phân trang
                await using var dataCmd = new MySqlCommand(dataQuery, conn);

                // Thêm các tham số cho truy vấn
                int offset = (page - 1) * pageSize;
                dataCmd.Parameters.AddWithValue("@offset", offset);
                dataCmd.Parameters.AddWithValue("@limit", pageSize);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    dataCmd.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");
                }

                if (!string.IsNullOrWhiteSpace(nhomVang) && nhomVang != "Tất cả")
                {
                    dataCmd.Parameters.AddWithValue("@nhomVang", nhomVang);
                }

                if (!string.IsNullOrWhiteSpace(SelectedTenHangHoa) && SelectedTenHangHoa != "Tất cả")
                {
                    dataCmd.Parameters.AddWithValue("@tenHang", SelectedTenHangHoa);
                }

                // Clear danh sách hiện tại
                DanhSachKhoVang.Clear();

                try
                {
                    // Đọc dữ liệu
                    await using var reader = await dataCmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            var khoVang = new KhoVangMuaVaoModel
                            {
                                TEN_HANG_HOA = reader["TEN_HANG_HOA"] is DBNull ? string.Empty : reader.GetString("TEN_HANG_HOA"),
                                NHOM_TEN = reader["NHOM_TEN"] is DBNull ? string.Empty : reader.GetString("NHOM_TEN"),
                                CAN_TONG = reader["CAN_TONG"] is DBNull ? 0 : reader.GetDecimal("CAN_TONG"),
                                TL_LOC = reader["TL_LOC"] is DBNull ? 0 : reader.GetDecimal("TL_LOC"),
                                TL_HOT = reader["TL_HOT"] is DBNull ? 0 : reader.GetDecimal("TL_HOT"),
                                TL_THUC = reader["TL_THUC"] is DBNull ? 0 : reader.GetDecimal("TL_THUC"),
                                SO_LUONG = reader["SO_LUONG"] is DBNull ? 0 : reader.GetInt32("SO_LUONG"),
                                DON_GIA = reader["DON_GIA"] is DBNull ? 0 : reader.GetDecimal("DON_GIA"),
                                THANH_TIEN = reader["THANH_TIEN"] is DBNull ? 0 : reader.GetDecimal("THANH_TIEN")
                            };

                            DanhSachKhoVang.Add(khoVang);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi khi đọc dữ liệu: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thực thi truy vấn: {ex.Message}");
                }

                // Cập nhật danh sách hiển thị
                Device.BeginInvokeOnMainThread(() =>
                {
                    DanhSachHienThi = new ObservableCollection<KhoVangMuaVaoModel>(DanhSachKhoVang);

                    // Cập nhật thông tin debug
                    string filterInfo = "";
                    if (!string.IsNullOrEmpty(nhomVang) && nhomVang != "Tất cả")
                    {
                        filterInfo += $" | Nhóm: {nhomVang}";
                    }
                    if (!string.IsNullOrEmpty(SelectedTenHangHoa) && SelectedTenHangHoa != "Tất cả")
                    {
                        filterInfo += $" | Tên hàng: {SelectedTenHangHoa}";
                    }

                    DebugInfo = $"Tìm thấy {totalRows} kết quả{filterInfo}. Trang {CurrentPage}/{TotalPages}.";

                    Console.WriteLine($"Đã tải {DanhSachKhoVang.Count} bản ghi.");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LoadKhoVangWithPaginationAsync: {ex.Message}");
                DebugInfo = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = string.Empty;
            }
        }
    }
}
