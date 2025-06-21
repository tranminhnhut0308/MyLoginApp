using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MyLoginApp.Helpers;
using MyLoginApp.Models;
using MySqlConnector;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Data;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace MyLoginApp.ViewModels
{
    public partial class HangHoaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<HangHoaModel> danhSachHangHoa = new();

        [ObservableProperty]
        private HangHoaModel selectedHangHoa;

        [ObservableProperty]
        private HangHoaModel formHangHoa = new();

        [ObservableProperty]
        private bool isAdding;

        [ObservableProperty]
        private bool isEditing;

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages = 1;

        [ObservableProperty]
        private int pageSize = 6;

        [ObservableProperty]
        private bool canGoNext = true;

        [ObservableProperty]
        private bool canGoPrevious;

        [ObservableProperty]
        private ObservableCollection<string> danhSachNhom = new();

        [ObservableProperty]
        private ObservableCollection<string> danhSachLoaiVang = new();

        [ObservableProperty]
        private string searchKeyword = string.Empty;

        [ObservableProperty]
        private bool isRefreshing;

        // Tự động kích hoạt tìm kiếm khi SearchKeyword thay đổi
        partial void OnSearchKeywordChanged(string value)
        {
            // Reset về trang đầu tiên và tải lại dữ liệu
            CurrentPage = 1;
            _ = LoadHangHoaAsync();
        }

        public HangHoaViewModel()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => {
                    _ = LoadHangHoaAsync();
                    _ = LoadNhomHangAsync();
                    _ = LoadLoaiVangAsync();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khởi tạo HangHoaViewModel: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MainThread.BeginInvokeOnMainThread(async () => {
                    await Shell.Current.DisplayAlert("Lỗi", "Lỗi khởi tạo dữ liệu hàng hóa", "OK");
                });
            }
        }

        private async Task LoadNhomHangAsync()
        {
            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) 
                {
                    Debug.WriteLine("Không thể kết nối CSDL khi tải danh sách nhóm hàng");
                    return;
                }

                try
                {
                    string query = "SELECT NHOM_TEN FROM nhom_hang";
                    using var cmd = new MySqlCommand(query, conn);
                    using var reader = await cmd.ExecuteReaderAsync();

                    DanhSachNhom.Clear();
                    while (await reader.ReadAsync())
                    {
                        string nhomHang = reader.GetString("NHOM_TEN");
                        DanhSachNhom.Add(nhomHang);
                    }
                }
                catch (MySqlException sqlEx)
                {
                    Debug.WriteLine($"Lỗi SQL khi tải danh sách nhóm hàng: {sqlEx.Message}");
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        await conn.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi tải danh sách nhóm hàng: {ex.Message}");
            }
        }

        private async Task LoadLoaiVangAsync()
        {
            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) 
                {
                    Debug.WriteLine("Không thể kết nối CSDL khi tải danh sách loại vàng");
                    return;
                }

                try
                {
                    string query = "SELECT LOAI_TEN FROM loai_hang";
                    using var cmd = new MySqlCommand(query, conn);
                    using var reader = await cmd.ExecuteReaderAsync();

                    DanhSachLoaiVang.Clear();
                    while (await reader.ReadAsync())
                    {
                        string loaiVang = reader.GetString("LOAI_TEN");
                        DanhSachLoaiVang.Add(loaiVang);
                    }
                }
                catch (MySqlException sqlEx)
                {
                    Debug.WriteLine($"Lỗi SQL khi tải danh sách loại vàng: {sqlEx.Message}");
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        await conn.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi tải danh sách loại vàng: {ex.Message}");
            }
        }

        private async Task LoadHangHoaAsync()
        {
            await LoadHangHoaWithPaginationAsync(CurrentPage, PageSize);
        }

        private async Task LoadHangHoaWithPaginationAsync(int page, int pageSize)
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    Debug.WriteLine("Không thể kết nối đến cơ sở dữ liệu");
                    MainThread.BeginInvokeOnMainThread(async () => {
                        await Shell.Current.DisplayAlert("Lỗi", "⚠️ Không thể kết nối đến cơ sở dữ liệu!", "OK");
                    });
                    return;
                }

                try
                {
                    // Tạo câu truy vấn với điều kiện tìm kiếm nếu có
                    string whereClause = "h.SU_DUNG = 1";
                    if (!string.IsNullOrWhiteSpace(SearchKeyword))
                    {
                        whereClause += $" AND (h.HANGHOAMA LIKE @SearchKeyword OR h.HANG_HOA_TEN LIKE @SearchKeyword)";
                    }

                    // Lấy tổng số bản ghi để tính số trang
                    string countQuery = $"SELECT COUNT(*) FROM danh_muc_hang_hoa h WHERE {whereClause}";
                    await using var countCmd = new MySqlCommand(countQuery, conn);
                    if (!string.IsNullOrWhiteSpace(SearchKeyword))
                    {
                        countCmd.Parameters.AddWithValue("@SearchKeyword", $"%{SearchKeyword}%");
                    }
                    var totalRecords = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                    if (TotalPages <= 0) TotalPages = 1; // Đảm bảo luôn có ít nhất 1 trang

                    // Cập nhật trạng thái phân trang
                    CanGoNext = CurrentPage < TotalPages;
                    CanGoPrevious = CurrentPage > 1;

                    Debug.WriteLine($"Current Page: {CurrentPage}, Total Pages: {TotalPages}, CanGoNext: {CanGoNext}, CanGoPrevious: {CanGoPrevious}");

                    // Offset cho pagination
                    int offset = (page - 1) * pageSize;

                    // Query với phân trang và sắp xếp HANGHOAMA giảm dần (cao nhất ở trên)
                    string query = @"
                    SELECT 
                        h.HANGHOAMA, 
                        h.HANG_HOA_TEN,
                        l.LOAI_TEN,
                        n.NHOM_TEN,
                        h.CAN_TONG,
                        h.TL_HOT, 
                        h.CONG_GOC, 
                        h.GIA_CONG,
                        h.DON_GIA_GOC 
                    FROM 
                        danh_muc_hang_hoa h
                        LEFT JOIN loai_hang l ON l.LOAIID = h.LOAIID 
                        LEFT JOIN nhom_hang n ON n.NHOMHANGID = h.NHOMHANGID
                    WHERE " + whereClause + @"
                    ORDER BY h.HANGHOAMA DESC
                    LIMIT @PageSize OFFSET @Offset";

                    await using var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    if (!string.IsNullOrWhiteSpace(SearchKeyword))
                    {
                        cmd.Parameters.AddWithValue("@SearchKeyword", $"%{SearchKeyword}%");
                    }
                    await using var reader = await cmd.ExecuteReaderAsync();

                    DanhSachHangHoa.Clear(); // Xóa trước khi thêm dữ liệu mới

                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            var hangHoa = new HangHoaModel
                            {
                                HangHoaID = reader.IsDBNull(reader.GetOrdinal("HANGHOAMA")) ? "" : reader.GetString("HANGHOAMA"),
                                TenHangHoa = reader.IsDBNull(reader.GetOrdinal("HANG_HOA_TEN")) ? "" : reader.GetString("HANG_HOA_TEN"),
                                Nhom = reader.IsDBNull(reader.GetOrdinal("NHOM_TEN")) ? "Chưa có nhóm" : reader.GetString("NHOM_TEN"),
                                LoaiVang = reader.IsDBNull(reader.GetOrdinal("LOAI_TEN")) ? "Không xác định" : reader.GetString("LOAI_TEN"),
                                CanTong = reader.IsDBNull(reader.GetOrdinal("CAN_TONG")) ? 0 : reader.GetDecimal("CAN_TONG"),
                                TrongLuongHot = reader.IsDBNull(reader.GetOrdinal("TL_HOT")) ? 0 : reader.GetDecimal("TL_HOT"),
                                CongGoc = reader.IsDBNull(reader.GetOrdinal("CONG_GOC")) ? 0 : reader.GetDecimal("CONG_GOC"),
                                GiaCong = reader.IsDBNull(reader.GetOrdinal("GIA_CONG")) ? 0 : reader.GetDecimal("GIA_CONG"),
                                DonViGoc = reader.IsDBNull(reader.GetOrdinal("DON_GIA_GOC")) ? 0 : reader.GetDecimal("DON_GIA_GOC")
                            };

                            DanhSachHangHoa.Add(hangHoa);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Lỗi xử lý bản ghi: {ex.Message}");
                        }
                    }

                    if (DanhSachHangHoa.Count == 0 && totalRecords > 0)
                    {
                        // Nếu không có dữ liệu trả về nhưng có bản ghi, có thể trang hiện tại vượt quá tổng số trang
                        CurrentPage = 1;
                        await LoadHangHoaWithPaginationAsync(CurrentPage, PageSize);
                    }
                }
                catch (MySqlException sqlEx)
                {
                    Debug.WriteLine($"Lỗi SQL: {sqlEx.Message}");
                    MainThread.BeginInvokeOnMainThread(async () => {
                        await Shell.Current.DisplayAlert("Lỗi dữ liệu", $"Không thể truy vấn dữ liệu hàng hóa: {sqlEx.Message}", "OK");
                    });
                }
                finally
                {
                    // Đảm bảo kết nối được đóng dù có lỗi
                    if (conn.State == ConnectionState.Open)
                    {
                        await conn.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi tải dữ liệu hàng hóa: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MainThread.BeginInvokeOnMainThread(async () => {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể tải dữ liệu hàng hóa. Vui lòng thử lại sau.", "OK");
                });
            }
        }

        [RelayCommand]
        private async Task GoToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadHangHoaWithPaginationAsync(CurrentPage, PageSize);
            }
        }

        [RelayCommand]
        private async Task GoToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadHangHoaWithPaginationAsync(CurrentPage, PageSize);
            }
        }

        [RelayCommand]
        private async Task ShowAddForm()
        {
            // Tạo mới đối tượng HangHoaModel
            FormHangHoa = new HangHoaModel();

            // Lấy mã hàng hóa cao nhất và tự động tăng lên
            FormHangHoa.HangHoaID = await GetNextHangHoaIDAsync();

            IsAdding = true;
            IsEditing = false;
        }

        private async Task<string> GetNextHangHoaIDAsync()
        {
            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return "0000001";

                // Tìm HANGHOAMA lớn nhất
                string query = "SELECT MAX(CAST(HANGHOAMA AS UNSIGNED)) AS MaxID FROM danh_muc_hang_hoa";
                using var cmd = new MySqlCommand(query, conn);
                var result = await cmd.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    return "0000001"; // Mã đầu tiên nếu không có bản ghi nào
                }

                // Tăng giá trị lên 1
                int currentMaxID = Convert.ToInt32(result);
                int nextID = currentMaxID + 1;

                // Format lại thành chuỗi với độ dài 8 số, thêm các số 0 ở đầu
                return nextID.ToString("D8");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy mã hàng hóa tiếp theo: {ex.Message}");
                return "0000001"; // Trả về giá trị mặc định nếu có lỗi
            }
        }

        [RelayCommand]
        private void ShowEditForm()
        {
            if (SelectedHangHoa == null) return;

            FormHangHoa = new HangHoaModel
            {
                HangHoaID = SelectedHangHoa.HangHoaID,
                TenHangHoa = SelectedHangHoa.TenHangHoa,
                LoaiVang = SelectedHangHoa.LoaiVang,
                Nhom = SelectedHangHoa.Nhom,
                CanTong = SelectedHangHoa.CanTong,
                TrongLuongHot = SelectedHangHoa.TrongLuongHot,
                CongGoc = SelectedHangHoa.CongGoc,
                GiaCong = SelectedHangHoa.GiaCong,
                DonViGoc = SelectedHangHoa.DonViGoc,
            };

            IsEditing = true;
            IsAdding = false;
        }

        [RelayCommand]
        private async Task SaveAdd()
        {
            if (string.IsNullOrWhiteSpace(FormHangHoa.TenHangHoa))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập tên hàng hóa!", "OK");
                return;
            }

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                // Lấy LOAIID và NHOMHANGID từ tên
                int loaiID = await GetLoaiIDFromNameAsync(FormHangHoa.LoaiVang, conn);
                int nhomID = await GetNhomIDFromNameAsync(FormHangHoa.Nhom, conn);

                string query = @"
                INSERT INTO danh_muc_hang_hoa (
                    HANGHOAMA, 
                    HANG_HOA_TEN, 
                    LOAIID, 
                    NHOMHANGID, 
                    CAN_TONG, 
                    TL_HOT, 
                    CONG_GOC, 
                    GIA_CONG, 
                    DON_GIA_GOC,
                    SU_DUNG
                ) VALUES (
                    @HANGHOAMA, 
                    @HANG_HOA_TEN, 
                    @LOAIID, 
                    @NHOMHANGID, 
                    @CAN_TONG, 
                    @TL_HOT, 
                    @CONG_GOC, 
                    @GIA_CONG, 
                    @DON_GIA_GOC,
                    1
                )";

                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HANGHOAMA", FormHangHoa.HangHoaID);
                cmd.Parameters.AddWithValue("@HANG_HOA_TEN", FormHangHoa.TenHangHoa);
                cmd.Parameters.AddWithValue("@LOAIID", loaiID);
                cmd.Parameters.AddWithValue("@NHOMHANGID", nhomID);
                cmd.Parameters.AddWithValue("@CAN_TONG", FormHangHoa.CanTong);
                cmd.Parameters.AddWithValue("@TL_HOT", FormHangHoa.TrongLuongHot);
                cmd.Parameters.AddWithValue("@CONG_GOC", FormHangHoa.CongGoc);
                cmd.Parameters.AddWithValue("@GIA_CONG", FormHangHoa.GiaCong);
                cmd.Parameters.AddWithValue("@DON_GIA_GOC", FormHangHoa.DonViGoc);

                await cmd.ExecuteNonQueryAsync();

                // Update SU_DUNG for LoaiVang if needed
                string updateLoaiVangQuery = "UPDATE loai_hang SET SU_DUNG = 1 WHERE LOAIID = @LOAIID AND SU_DUNG = 0";
                await using var updateLoaiVangCmd = new MySqlCommand(updateLoaiVangQuery, conn);
                updateLoaiVangCmd.Parameters.AddWithValue("@LOAIID", loaiID);
                await updateLoaiVangCmd.ExecuteNonQueryAsync();

                // Update SU_DUNG for NhomVang if needed
                string updateNhomVangQuery = "UPDATE nhom_hang SET SU_DUNG = 1 WHERE NHOMHANGID = @NHOMHANGID AND SU_DUNG = 0";
                await using var updateNhomVangCmd = new MySqlCommand(updateNhomVangQuery, conn);
                updateNhomVangCmd.Parameters.AddWithValue("@NHOMHANGID", nhomID);
                await updateNhomVangCmd.ExecuteNonQueryAsync();

                IsAdding = false;
                await Shell.Current.DisplayAlert("Thông báo", "✓ Thêm hàng hóa thành công!", "OK");
                await LoadHangHoaAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"⚠️ Lỗi khi thêm hàng hóa: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SaveEdit()
        {
            if (string.IsNullOrWhiteSpace(FormHangHoa.TenHangHoa))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập tên hàng hóa!", "OK");
                return;
            }

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                // Lấy LOAIID và NHOMHANGID từ tên
                int loaiID = await GetLoaiIDFromNameAsync(FormHangHoa.LoaiVang, conn);
                int nhomID = await GetNhomIDFromNameAsync(FormHangHoa.Nhom, conn);

                string query = @"
                UPDATE danh_muc_hang_hoa SET
                    HANG_HOA_TEN = @HANG_HOA_TEN,
                    LOAIID = @LOAIID,
                    NHOMHANGID = @NHOMHANGID,
                    CAN_TONG = @CAN_TONG,
                    TL_HOT = @TL_HOT,
                    CONG_GOC = @CONG_GOC,
                    GIA_CONG = @GIA_CONG,
                    DON_GIA_GOC = @DON_GIA_GOC
                WHERE HANGHOAMA = @HANGHOAMA";

                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HANGHOAMA", FormHangHoa.HangHoaID);
                cmd.Parameters.AddWithValue("@HANG_HOA_TEN", FormHangHoa.TenHangHoa);
                cmd.Parameters.AddWithValue("@LOAIID", loaiID);
                cmd.Parameters.AddWithValue("@NHOMHANGID", nhomID);
                cmd.Parameters.AddWithValue("@CAN_TONG", FormHangHoa.CanTong);
                cmd.Parameters.AddWithValue("@TL_HOT", FormHangHoa.TrongLuongHot);
                cmd.Parameters.AddWithValue("@CONG_GOC", FormHangHoa.CongGoc);
                cmd.Parameters.AddWithValue("@GIA_CONG", FormHangHoa.GiaCong);
                cmd.Parameters.AddWithValue("@DON_GIA_GOC", FormHangHoa.DonViGoc);

                await cmd.ExecuteNonQueryAsync();

                // Update SU_DUNG for LoaiVang if needed
                string updateLoaiVangQuery = "UPDATE loai_hang SET SU_DUNG = 1 WHERE LOAIID = @LOAIID AND SU_DUNG = 0";
                await using var updateLoaiVangCmd = new MySqlCommand(updateLoaiVangQuery, conn);
                updateLoaiVangCmd.Parameters.AddWithValue("@LOAIID", loaiID);
                await updateLoaiVangCmd.ExecuteNonQueryAsync();

                // Update SU_DUNG for NhomVang if needed
                string updateNhomVangQuery = "UPDATE nhom_hang SET SU_DUNG = 1 WHERE NHOMHANGID = @NHOMHANGID AND SU_DUNG = 0";
                await using var updateNhomVangCmd = new MySqlCommand(updateNhomVangQuery, conn);
                updateNhomVangCmd.Parameters.AddWithValue("@NHOMHANGID", nhomID);
                await updateNhomVangCmd.ExecuteNonQueryAsync();

                IsEditing = false;
                await Shell.Current.DisplayAlert("Thông báo", "✓ Cập nhật hàng hóa thành công!", "OK");
                await LoadHangHoaAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"⚠️ Lỗi khi cập nhật hàng hóa: {ex.Message}", "OK");
            }
        }

        private async Task<int> GetLoaiIDFromNameAsync(string loaiName, MySqlConnection conn)
        {
            try
            {
                string query = "SELECT LOAIID FROM loai_hang WHERE LOAI_TEN = @LoaiTen LIMIT 1";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LoaiTen", loaiName);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }

                // Nếu không tìm thấy, trả về ID mặc định
                return 1;
            }
            catch
            {
                return 1; // Trả về ID mặc định nếu có lỗi
            }
        }

        private async Task<int> GetNhomIDFromNameAsync(string nhomName, MySqlConnection conn)
        {
            try
            {
                string query = "SELECT NHOMHANGID FROM nhom_hang WHERE NHOM_TEN = @NhomTen LIMIT 1";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NhomTen", nhomName);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }

                // Nếu không tìm thấy, trả về ID mặc định
                return 1;
            }
            catch
            {
                return 1; // Trả về ID mặc định nếu có lỗi
            }
        }

        [RelayCommand]
        private void CancelForm()
        {
            IsAdding = false;
            IsEditing = false;
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (SelectedHangHoa == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", "Bạn có chắc muốn xóa hàng hóa này?", "Có", "Không");
            if (!confirm) return;

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                // Xóa logic thay vì xóa thật
                string query = "UPDATE danh_muc_hang_hoa SET SU_DUNG = 0 WHERE HANGHOAMA = @HANGHOAMA";
                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HANGHOAMA", SelectedHangHoa.HangHoaID);

                await cmd.ExecuteNonQueryAsync();
                await Shell.Current.DisplayAlert("Thông báo", "✓ Xóa hàng hóa thành công!", "OK");
                await LoadHangHoaAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"⚠️ Lỗi khi xóa hàng hóa: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private void SelectHangHoa(HangHoaModel hangHoa)
        {
            if (hangHoa != null)
            {
                // Bỏ chọn mục đã chọn trước đó
                foreach (var item in DanhSachHangHoa)
                {
                    item.IsSelected = false;
                }

                // Đánh dấu mục hiện tại là được chọn
                hangHoa.IsSelected = true;
                SelectedHangHoa = hangHoa;
                Debug.WriteLine($"Đã chọn hàng hóa: {hangHoa.TenHangHoa} (ID: {hangHoa.HangHoaID})");
            }
        }

        [RelayCommand]
        private async Task Search()
        {
            CurrentPage = 1;
            await LoadHangHoaAsync();
        }
    }
}
