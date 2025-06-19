using MyLoginApp.Models;
using MyLoginApp.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MySqlConnector;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MyLoginApp.ViewModels
{
    public class LoaiVangViewModel : BaseViewModel
    {
        // ===== Danh sách loại vàng =====
        public ObservableCollection<LoaiVangModel> ListLoaiVang { get; set; } = new();

        // ===== Thuộc tính quản lý form =====
        private bool _isAdding;
        public bool IsAdding
        {
            get => _isAdding;
            set { if (SetProperty(ref _isAdding, value)) OnPropertyChanged(nameof(IsShowingList)); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { if (SetProperty(ref _isEditing, value)) OnPropertyChanged(nameof(IsShowingList)); }
        }

        public bool IsShowingList => !IsAdding && !IsEditing;

        // ===== Model form nhập liệu =====
        private LoaiVangModel _formLoaiVang = new();
        public LoaiVangModel FormLoaiVang
        {
            get => _formLoaiVang;
            set => SetProperty(ref _formLoaiVang, value);
        }

        // ===== Các property tùy chỉnh cho form =====
        public string DonGiaVonText
        {
            get => FormLoaiVang.DonGiaVon.HasValue ? FormLoaiVang.DonGiaVon.Value.ToString("N0") : string.Empty;
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        FormLoaiVang.DonGiaVon = null;
                    }
                    else
                    {
                        string numericValue = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                        if (decimal.TryParse(numericValue, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal result))
                        {
                            FormLoaiVang.DonGiaVon = result;
                        }
                    }
                    OnPropertyChanged(nameof(DonGiaVonText));
                }
                catch (Exception)
                {
                    // Nếu có lỗi thì giữ giá trị cũ
                }
            }
        }

        public string DonGiaMuaText
        {
            get => FormLoaiVang.DonGiaMua.HasValue ? FormLoaiVang.DonGiaMua.Value.ToString("N0") : string.Empty;
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        FormLoaiVang.DonGiaMua = null;
                    }
                    else
                    {
                        FormLoaiVang.DonGiaMua = ParseDecimalValue(value);
                    }
                    OnPropertyChanged(nameof(DonGiaMuaText));
                }
                catch (Exception)
                {
                    // Xử lý ngoại lệ
                }
            }
        }

        public string DonGiaBanText
        {
            get => FormLoaiVang.DonGiaBan.HasValue ? FormLoaiVang.DonGiaBan.Value.ToString("N0") : string.Empty;
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        FormLoaiVang.DonGiaBan = null;
                    }
                    else
                    {
                        FormLoaiVang.DonGiaBan = ParseDecimalValue(value);
                    }
                    OnPropertyChanged(nameof(DonGiaBanText));
                }
                catch (Exception)
                {
                    // Xử lý ngoại lệ
                }
            }
        }

        public string DonGiaCamText
        {
            get => FormLoaiVang.DonGiaCam.HasValue ? FormLoaiVang.DonGiaCam.Value.ToString("N0") : string.Empty;
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        FormLoaiVang.DonGiaCam = null;
                    }
                    else
                    {
                        FormLoaiVang.DonGiaCam = ParseDecimalValue(value);
                    }
                    OnPropertyChanged(nameof(DonGiaCamText));
                }
                catch (Exception)
                {
                    // Xử lý ngoại lệ
                }
            }
        }

        // ===== Model item được chọn =====
        private LoaiVangModel _selectedLoaiVang;
        public LoaiVangModel SelectedLoaiVang
        {
            get => _selectedLoaiVang;
            set
            {
                if (_selectedLoaiVang != value)
                {
                    // Bỏ chọn mục cũ
                    if (_selectedLoaiVang != null)
                        _selectedLoaiVang.IsSelected = false;

                    _selectedLoaiVang = value;

                    // Đánh dấu mục mới là được chọn
                    if (_selectedLoaiVang != null)
                        _selectedLoaiVang.IsSelected = true;

                    OnPropertyChanged(nameof(SelectedLoaiVang));
                }
            }
        }

        // ===== Các command =====
        public ICommand LoadCommand { get; }
        public ICommand ShowAddFormCommand { get; }
        public ICommand SaveAddCommand { get; }
        public ICommand CancelAddEditCommand { get; }
        public ICommand ShowEditFormCommand { get; }
        public ICommand SaveEditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SelectItemCommand { get; }

        // ===== Constructor =====
        public LoaiVangViewModel()
        {
            LoadCommand = new Command(async () => await LoadLoaiVangAsync());
            ShowAddFormCommand = new Command(ShowAddForm);
            SaveAddCommand = new Command(async () => await SaveAddLoaiVangAsync());
            CancelAddEditCommand = new Command(CancelAddEdit);
            ShowEditFormCommand = new Command(ShowEditForm);
            SaveEditCommand = new Command(async () => await SaveEditLoaiVangAsync());
            DeleteCommand = new Command(async () => await DeleteLoaiVangAsync());
            SelectItemCommand = new Command<LoaiVangModel>(OnSelectItem);

            Task.Run(() => LoadLoaiVangAsync());
        }

        // ===== Helper method để parse giá trị số từ chuỗi =====
        private decimal? ParseDecimalValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            try
            {
                // Loại bỏ tất cả ký tự không phải số, chấm hoặc phẩy
                string numericValue = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                
                if (string.IsNullOrWhiteSpace(numericValue))
                    return null;

                // Chuẩn hóa định dạng số
                if (decimal.TryParse(numericValue, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal result))
                {
                    return result;
                }
            }
            catch (Exception)
            {
                // Xử lý ngoại lệ nếu có
            }

            return null;
        }

        // ===== Load dữ liệu từ database =====
        private async Task LoadLoaiVangAsync()
        {
            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = @"SELECT 
                    NHOMHANGID, 
                    NHOMHANGMA, 
                    NHOMCHAID, 
                    NHOM_TEN, 
                    DON_GIA_BAN, 
                    DON_GIA_MUA, 
                    MUA_BAN, 
                    DON_GIA_VON, 
                    DON_GIA_CAM, 
                    SU_DUNG,
                    GHI_CHU
                FROM nhom_hang";

                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                ListLoaiVang.Clear();
                while (await reader.ReadAsync())
                {
                    try
                    {
                        var loaiVang = new LoaiVangModel();

                        // Đọc và xử lý từng trường một cách an toàn
                        if (!reader.IsDBNull(reader.GetOrdinal("NHOMHANGID")))
                            loaiVang.NhomHangID = reader.GetInt32("NHOMHANGID");

                        loaiVang.NhomHangMA = reader.IsDBNull(reader.GetOrdinal("NHOMHANGMA")) ? string.Empty : reader.GetString("NHOMHANGMA");

                        if (!reader.IsDBNull(reader.GetOrdinal("NHOMCHAID")))
                            loaiVang.NhomChaID = reader.GetInt32("NHOMCHAID");

                        loaiVang.TenLoaiVang = reader.IsDBNull(reader.GetOrdinal("NHOM_TEN")) ? string.Empty : reader.GetString("NHOM_TEN");

                        loaiVang.DonGiaVon = reader.IsDBNull(reader.GetOrdinal("DON_GIA_VON")) 
                            ? null 
                            : reader.GetDecimal("DON_GIA_VON");

                        loaiVang.DonGiaMua = reader.IsDBNull(reader.GetOrdinal("DON_GIA_MUA"))
                            ? null
                            : reader.GetDecimal("DON_GIA_MUA");

                        loaiVang.DonGiaBan = reader.IsDBNull(reader.GetOrdinal("DON_GIA_BAN"))
                            ? null
                            : reader.GetDecimal("DON_GIA_BAN");

                        loaiVang.DonGiaCam = reader.IsDBNull(reader.GetOrdinal("DON_GIA_CAM"))
                            ? null
                            : reader.GetDecimal("DON_GIA_CAM");

                        loaiVang.MuaBan = reader.IsDBNull(reader.GetOrdinal("MUA_BAN")) ? null : reader.GetString("MUA_BAN");

                        if (!reader.IsDBNull(reader.GetOrdinal("SU_DUNG")))
                            loaiVang.SuDung = reader.GetInt32("SU_DUNG");

                        loaiVang.GhiChu = reader.IsDBNull(reader.GetOrdinal("GHI_CHU")) ? null : reader.GetString("GHI_CHU");

                        ListLoaiVang.Add(loaiVang);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi đọc hàng dữ liệu: {ex.Message}");
                        // Tiếp tục với hàng tiếp theo thay vì dừng toàn bộ
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
                });
            }
        }

        // ===== Xử lý chọn thẻ (card) =====
        private void OnSelectItem(LoaiVangModel item)
        {
            if (item == null) return;

            // Bỏ chọn tất cả các mục khác
            foreach (var loaiVang in ListLoaiVang)
                loaiVang.IsSelected = false;

            // Đánh dấu mục được chọn
            item.IsSelected = true;
            SelectedLoaiVang = item;

            // Hiển thị thông báo nhỏ về mục đã chọn (không quá gây phiền)
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Đã vô hiệu hóa thông báo theo yêu cầu
                /*
                // Chỉ hiển thị thông báo nếu đây là lần đầu tiên chọn mục
                if (!_hasSelectedItemBefore)
                {
                    await Shell.Current.DisplayAlert("Đã chọn", $"Đã chọn mục: {item.TenLoaiVang}\nBạn có thể sửa hoặc xóa mục này.", "OK");
                    _hasSelectedItemBefore = true;
                }
                */
            });
        }

        // Biến đánh dấu người dùng đã từng chọn mục chưa
        private bool _hasSelectedItemBefore = false;

        // ===== Hiện form Thêm mới =====
        private void ShowAddForm()
        {
            // Tạo model mới với các giá trị đơn giá là null (trống)
            FormLoaiVang = new LoaiVangModel
            {
                DonGiaVon = null,
                DonGiaMua = null,
                DonGiaBan = null,
                DonGiaCam = null
            };
            IsAdding = true;
        }

        // ===== Hiện form Sửa =====
        private async void ShowEditForm()
        {
            if (SelectedLoaiVang == null)
            {
                await Shell.Current.DisplayAlert("Thông báo", "Vui lòng chọn loại vàng để sửa!", "OK");
                return;
            }

            FormLoaiVang = new LoaiVangModel
            {
                NhomHangID = SelectedLoaiVang.NhomHangID,
                NhomHangMA = SelectedLoaiVang.NhomHangMA,
                NhomChaID = SelectedLoaiVang.NhomChaID,
                TenLoaiVang = SelectedLoaiVang.TenLoaiVang,
                DonGiaVon = SelectedLoaiVang.DonGiaVon,
                DonGiaMua = SelectedLoaiVang.DonGiaMua,
                DonGiaBan = SelectedLoaiVang.DonGiaBan,
                DonGiaCam = SelectedLoaiVang.DonGiaCam,
                MuaBan = SelectedLoaiVang.MuaBan,
                SuDung = SelectedLoaiVang.SuDung,
                GhiChu = SelectedLoaiVang.GhiChu
            };
            IsEditing = true;
        }

        // ===== Hủy thêm/sửa =====
        private void CancelAddEdit()
        {
            IsAdding = false;
            IsEditing = false;
            FormLoaiVang = new LoaiVangModel();
        }

        // ===== Thêm mới loại vàng =====
        private async Task SaveAddLoaiVangAsync()
        {
            if (string.IsNullOrWhiteSpace(FormLoaiVang.TenLoaiVang))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập tên loại vàng", "OK");
                return;
            }

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                // Tạo mã NHOMHANGMA bằng cách tìm mã nhỏ nhất chưa được sử dụng
                string findNextIdQuery = @"
                    SELECT t.NHOMHANGMA+1 AS NextID
                    FROM nhom_hang t
                    LEFT JOIN nhom_hang t2 ON t.NHOMHANGMA+1 = t2.NHOMHANGMA
                    WHERE t2.NHOMHANGMA IS NULL
                    ORDER BY t.NHOMHANGMA ASC
                    LIMIT 1;";

                // Nếu không có khoảng trống, lấy MAX + 1
                string getMaxMaQuery = "SELECT IFNULL(MAX(CAST(NHOMHANGMA AS UNSIGNED)), 0) FROM nhom_hang";

                // Thử tìm ID kế tiếp chưa sử dụng
                await using var findNextCmd = new MySqlCommand(findNextIdQuery, conn);
                var nextIdResult = await findNextCmd.ExecuteScalarAsync();

                int nextMa = 1;
                if (nextIdResult != null && nextIdResult != DBNull.Value)
                {
                    int.TryParse(nextIdResult.ToString(), out nextMa);
                }
                else
                {
                    // Nếu không tìm thấy khoảng trống, dùng MAX + 1
                    await using var getMaxCmd = new MySqlCommand(getMaxMaQuery, conn);
                    var maxMaResult = await getMaxCmd.ExecuteScalarAsync();

                    if (maxMaResult != null && maxMaResult != DBNull.Value)
                    {
                        int.TryParse(maxMaResult.ToString(), out int currentMax);
                        nextMa = currentMax + 1;
                    }
                }

                // Chuẩn bị câu lệnh insert
                string insertQuery = @"
                    INSERT INTO nhom_hang (
                        NHOMHANGMA, 
                        NHOM_TEN, 
                        DON_GIA_BAN, 
                        DON_GIA_MUA, 
                        DON_GIA_VON, 
                        DON_GIA_CAM, 
                        SU_DUNG,
                        GHI_CHU
                    ) VALUES (
                        @NHOMHANGMA, 
                        @NHOM_TEN, 
                        @DON_GIA_BAN, 
                        @DON_GIA_MUA, 
                        @DON_GIA_VON, 
                        @DON_GIA_CAM, 
                        @SU_DUNG,
                        @GHI_CHU
                    )";

                await using var insertCmd = new MySqlCommand(insertQuery, conn);

                // Thiết lập tham số cho câu lệnh
                insertCmd.Parameters.AddWithValue("@NHOMHANGMA", nextMa.ToString());
                insertCmd.Parameters.AddWithValue("@NHOM_TEN", FormLoaiVang.TenLoaiVang);
                
                // Xử lý các giá trị null - gửi DBNull.Value nếu giá trị là null
                insertCmd.Parameters.AddWithValue("@DON_GIA_VON", FormLoaiVang.DonGiaVon != null ? (object)FormLoaiVang.DonGiaVon : DBNull.Value);
                insertCmd.Parameters.AddWithValue("@DON_GIA_MUA", FormLoaiVang.DonGiaMua != null ? (object)FormLoaiVang.DonGiaMua : DBNull.Value);
                insertCmd.Parameters.AddWithValue("@DON_GIA_BAN", FormLoaiVang.DonGiaBan != null ? (object)FormLoaiVang.DonGiaBan : DBNull.Value);
                insertCmd.Parameters.AddWithValue("@DON_GIA_CAM", FormLoaiVang.DonGiaCam != null ? (object)FormLoaiVang.DonGiaCam : DBNull.Value);

                // SU_DUNG mặc định là 0
                insertCmd.Parameters.AddWithValue("@SU_DUNG", 0);
                // GHI_CHU không thể null, gán chuỗi rỗng
                insertCmd.Parameters.AddWithValue("@GHI_CHU", "");

                await insertCmd.ExecuteNonQueryAsync();

                await Shell.Current.DisplayAlert("Thành công", "Đã thêm loại vàng mới", "OK");

                // Đóng form và cập nhật lại danh sách
                IsAdding = false;
                await LoadLoaiVangAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể thêm loại vàng: {ex.Message}", "OK");
            }
        }

        // ===== Sửa loại vàng =====
        private async Task SaveEditLoaiVangAsync()
        {
            if (string.IsNullOrWhiteSpace(FormLoaiVang.TenLoaiVang))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập tên loại vàng", "OK");
                return;
            }

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                // Kiểm tra ID có tồn tại không
                string checkQuery = "SELECT COUNT(*) FROM nhom_hang WHERE NHOMHANGID = @NHOMHANGID";
                await using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@NHOMHANGID", FormLoaiVang.NhomHangID);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count == 0)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy loại vàng để cập nhật", "OK");
                    IsEditing = false;
                    return;
                }

                string updateQuery = @"
                    UPDATE nhom_hang SET 
                        NHOM_TEN = @NHOM_TEN,
                        DON_GIA_BAN = @DON_GIA_BAN,
                        DON_GIA_MUA = @DON_GIA_MUA,
                        DON_GIA_VON = @DON_GIA_VON,
                        DON_GIA_CAM = @DON_GIA_CAM
                    WHERE NHOMHANGID = @NHOMHANGID";

                await using var updateCmd = new MySqlCommand(updateQuery, conn);

                updateCmd.Parameters.AddWithValue("@NHOMHANGID", FormLoaiVang.NhomHangID);
                updateCmd.Parameters.AddWithValue("@NHOM_TEN", FormLoaiVang.TenLoaiVang);
                
                // Xử lý các giá trị null khi cập nhật
                updateCmd.Parameters.AddWithValue("@DON_GIA_VON", FormLoaiVang.DonGiaVon != null ? (object)FormLoaiVang.DonGiaVon : DBNull.Value);
                updateCmd.Parameters.AddWithValue("@DON_GIA_MUA", FormLoaiVang.DonGiaMua != null ? (object)FormLoaiVang.DonGiaMua : DBNull.Value);
                updateCmd.Parameters.AddWithValue("@DON_GIA_BAN", FormLoaiVang.DonGiaBan != null ? (object)FormLoaiVang.DonGiaBan : DBNull.Value);
                updateCmd.Parameters.AddWithValue("@DON_GIA_CAM", FormLoaiVang.DonGiaCam != null ? (object)FormLoaiVang.DonGiaCam : DBNull.Value);

                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    await Shell.Current.DisplayAlert("Thành công", "Đã cập nhật loại vàng!", "OK");

                    // Đóng form và cập nhật lại danh sách
                    IsEditing = false;
                    await LoadLoaiVangAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể cập nhật. Vui lòng thử lại.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể cập nhật loại vàng: {ex.Message}", "OK");
            }
        }

        // ===== Xóa loại vàng =====
        private async Task DeleteLoaiVangAsync()
        {
            if (SelectedLoaiVang == null)
            {
                await Shell.Current.DisplayAlert("Thông báo", "Vui lòng chọn loại vàng để xóa", "OK");
                return;
            }

            try
            {
                // Kiểm tra kết nối đến cơ sở dữ liệu
                if (!await DatabaseHelper.TestConnectionAsync())
                {
                    await Shell.Current.DisplayAlert("Lỗi kết nối", "Không thể kết nối đến cơ sở dữ liệu. Vui lòng kiểm tra kết nối và thử lại.", "OK");
                    return;
                }

                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi kết nối", "Không thể mở kết nối đến cơ sở dữ liệu.", "OK");
                    return;
                }

                // Kiểm tra ID tồn tại và lấy giá trị SU_DUNG hiện tại
                string checkQuery = "SELECT SU_DUNG FROM nhom_hang WHERE NHOMHANGID = @NHOMHANGID";
                await using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@NHOMHANGID", SelectedLoaiVang.NhomHangID);
                var result = await checkCmd.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy loại vàng để xóa", "OK");
                    return;
                }

                // Kiểm tra giá trị SU_DUNG
                int suDungValue = 0;
                if (int.TryParse(result.ToString(), out suDungValue) && suDungValue == 1)
                {
                    await Shell.Current.DisplayAlert("❌ Không thể xóa",
                        "Loại vàng này đang được sử dụng trong hệ thống.\n\n" +
                        "Loại vàng này có thể đang được sử dụng cho các sản phẩm hoặc giao dịch hiện có. Bạn chỉ có thể xóa các loại vàng không được sử dụng.",
                        "Đã hiểu");
                    return;
                }

                bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Bạn có chắc muốn xóa loại vàng '{SelectedLoaiVang.TenLoaiVang}'?", "Có", "Không");
                if (!confirm) return;

                // Thực hiện xóa với timeout ngắn hơn
                string query = "DELETE FROM nhom_hang WHERE NHOMHANGID = @NHOMHANGID";
                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NHOMHANGID", SelectedLoaiVang.NhomHangID);
                cmd.CommandTimeout = 15; // Giảm timeout xuống 15 giây

                try
                {
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        await Shell.Current.DisplayAlert("✅ Thành công", "Đã xóa loại vàng!", "OK");
                        SelectedLoaiVang = null; // Bỏ chọn item đã xóa
                        await LoadLoaiVangAsync();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("⚠️ Lỗi", "Không thể xóa loại vàng. Vui lòng thử lại.", "OK");
                    }
                }
                catch (MySqlException sqlEx) when (sqlEx.Message.Contains("foreign key constraint") || sqlEx.Number == 1451)
                {
                    // Bắt lỗi ràng buộc khóa ngoại
                    await Shell.Current.DisplayAlert("❌ Không thể xóa",
                        "Loại vàng này đang được sử dụng trong các bảng khác như kho, hóa đơn hoặc giao dịch.\n\n" +
                        "Bạn không thể xóa dữ liệu đang được sử dụng trong hệ thống.",
                        "Đã hiểu");
                }
            }
            catch (MySqlException sqlEx)
            {
                // Xử lý cụ thể lỗi MySQL
                string errorMessage = sqlEx.Number switch
                {
                    0 => "Không thể kết nối đến máy chủ MySQL. Vui lòng kiểm tra kết nối mạng.",
                    1042 => "Không thể kết nối đến máy chủ. Vui lòng kiểm tra địa chỉ máy chủ.",
                    1045 => "Tên đăng nhập hoặc mật khẩu không đúng.",
                    1451 => "Loại vàng này đang được sử dụng trong các bảng khác và không thể xóa.",
                    _ => $"Lỗi MySQL: {sqlEx.Message}"
                };

                await Shell.Current.DisplayAlert("⚠️ Lỗi cơ sở dữ liệu", errorMessage, "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("⚠️ Lỗi", $"Không thể xóa loại vàng: {ex.Message}", "OK");
            }
        }
    }
}