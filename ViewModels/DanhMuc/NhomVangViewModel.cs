using MyLoginApp.Models;
using MyLoginApp.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using MySqlConnector;
using System.Data;
using System.Linq;
using System.Diagnostics;

namespace MyLoginApp.ViewModels
{
    public class NhomVangViewModel : BaseViewModel
    {
        public ObservableCollection<NhomVangModel> ListNhomVang { get; set; } = new ObservableCollection<NhomVangModel>();

        private NhomVangModel? _selectedNhomVang;
        public NhomVangModel? SelectedNhomVang
        {
            get => _selectedNhomVang;
            set
            {
                if (_selectedNhomVang != value)
                {
                    if (_selectedNhomVang != null)
                        _selectedNhomVang.IsSelected = false;

                    _selectedNhomVang = value;

                    if (_selectedNhomVang != null)
                        _selectedNhomVang.IsSelected = true;

                    OnPropertyChanged(nameof(SelectedNhomVang));
                    (ShowEditFormCommand as Command)?.ChangeCanExecute();
                    (DeleteCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        private NhomVangModel _formNhomVang = new NhomVangModel();
        public NhomVangModel FormNhomVang
        {
            get => _formNhomVang;
            set => SetProperty(ref _formNhomVang, value);
        }

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

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchCommand.Execute(null);
                }
            }
        }

        private ObservableCollection<NhomVangModel> _filteredListNhomVang = new ObservableCollection<NhomVangModel>();
        public ObservableCollection<NhomVangModel> FilteredListNhomVang
        {
            get => _filteredListNhomVang;
            set => SetProperty(ref _filteredListNhomVang, value);
        }

        // Thêm các thuộc tính cho phân trang
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private int _totalPages = 1;
        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        private bool _canGoNext = true;
        public bool CanGoNext
        {
            get => _canGoNext;
            set => SetProperty(ref _canGoNext, value);
        }

        private bool _canGoPrevious = false;
        public bool CanGoPrevious
        {
            get => _canGoPrevious;
            set => SetProperty(ref _canGoPrevious, value);
        }

        public ICommand ShowAddFormCommand { get; }
        public ICommand ShowEditFormCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveAddCommand { get; }
        public ICommand SaveEditCommand { get; }
        public ICommand CancelAddEditCommand { get; }
        public ICommand SelectItemCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand RefreshCommand { get; }

        public NhomVangViewModel()
        {
            try
            {
                ShowAddFormCommand = new Command(OnShowAddForm);
                ShowEditFormCommand = new Command(OnShowEditForm, () => SelectedNhomVang != null);
                DeleteCommand = new Command(OnDelete, () => SelectedNhomVang != null);
                SaveAddCommand = new Command(OnSaveAdd);
                SaveEditCommand = new Command(OnSaveEdit);
                CancelAddEditCommand = new Command(OnCancelAddEdit);
                SelectItemCommand = new Command<NhomVangModel>(OnSelectItem);
                SearchCommand = new Command(OnSearch);
                NextPageCommand = new Command(OnNextPage, () => CanGoNext);
                PreviousPageCommand = new Command(OnPreviousPage, () => CanGoPrevious);
                RefreshCommand = new Command(OnRefresh);

                // Khởi tạo danh sách trước khi load dữ liệu
                ListNhomVang = new ObservableCollection<NhomVangModel>();
                FilteredListNhomVang = new ObservableCollection<NhomVangModel>();

                Task.Run(async () =>
                {
                    try
                    {
                        await LoadNhomVangAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi khi load dữ liệu nhóm vàng: {ex.Message}");
                        Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        await Shell.Current.DisplayAlert("Lỗi", "Không thể tải dữ liệu nhóm vàng. Vui lòng thử lại sau.", "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khởi tạo NhomVangViewModel: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void OnCancelAddEdit(object obj)
        {
            IsAdding = false;
            IsEditing = false;
            FormNhomVang = new NhomVangModel();
        }

        private void OnSelectItem(NhomVangModel item)
        {
            if (item == null) return;

            foreach (var nhomVang in ListNhomVang)
                nhomVang.IsSelected = false;

            item.IsSelected = true;
            SelectedNhomVang = item;
        }

        private void OnSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredListNhomVang = new ObservableCollection<NhomVangModel>(ListNhomVang);
                return;
            }

            var searchText = SearchText.ToLower();
            var filteredItems = ListNhomVang.Where(x =>
                x.TenNhom?.ToLower().Contains(searchText) == true ||
                x.KyHieu?.ToLower().Contains(searchText) == true ||
                x.MaLoai.ToString().Contains(searchText)
            ).ToList();

            FilteredListNhomVang = new ObservableCollection<NhomVangModel>(filteredItems);
        }

        private async void OnNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadNhomVangAsync();
            }
        }

        private async void OnPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadNhomVangAsync();
            }
        }

        private async void OnRefresh()
        {
            CurrentPage = 1;
            await LoadNhomVangAsync();
        }

        private async Task LoadNhomVangAsync()
        {
            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    Debug.WriteLine("Không thể kết nối đến cơ sở dữ liệu");
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        await Shell.Current.DisplayAlert("Lỗi", "⚠️ Chưa kết nối CSDL, không thể load dữ liệu!", "OK");
                    });
                    return;
                }

                try
                {
                    // Đếm tổng số bản ghi
                    string countQuery = "SELECT COUNT(*) FROM loai_hang";
                    int totalRecords;
                    await using (var countCmd = new MySqlCommand(countQuery, conn))
                    {
                        totalRecords = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                    }

                    // Tính toán phân trang
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);
                    CanGoNext = CurrentPage < TotalPages;
                    CanGoPrevious = CurrentPage > 1;

                    // Query chính với phân trang
                    string query = @"
                        SELECT LOAIID, LOAIMA, LOAI_TEN, GHI_CHU, SU_DUNG 
                        FROM loai_hang 
                        ORDER BY LOAIID 
                        LIMIT @Offset, @PageSize";

                    await using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Offset", (CurrentPage - 1) * PageSize);
                        cmd.Parameters.AddWithValue("@PageSize", PageSize);
                        cmd.CommandTimeout = 30;

                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var tempList = new List<NhomVangModel>();

                            while (await reader.ReadAsync())
                            {
                                try
                                {
                                    var nhomVang = new NhomVangModel
                                    {
                                        LoaiID = reader.IsDBNull("LOAIID") ? 0 : Convert.ToInt32(reader["LOAIID"]),
                                        MaLoai = reader.IsDBNull("LOAIMA") ? 0 : Convert.ToInt32(reader["LOAIMA"]),
                                        TenNhom = reader.IsDBNull("LOAI_TEN") ? "" : reader.GetString("LOAI_TEN"),
                                        KyHieu = reader.IsDBNull("GHI_CHU") ? "" : reader.GetString("GHI_CHU"),
                                        SuDung = reader.IsDBNull("SU_DUNG") ? 0 : Convert.ToInt32(reader["SU_DUNG"]),
                                        IsSelected = false
                                    };
                                    tempList.Add(nhomVang);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Lỗi khi đọc dữ liệu nhóm vàng: {ex.Message}");
                                    continue;
                                }
                            }

                            // Cập nhật UI trên main thread
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                ListNhomVang.Clear();
                                foreach (var item in tempList)
                                {
                                    ListNhomVang.Add(item);
                                }
                                FilteredListNhomVang = new ObservableCollection<NhomVangModel>(ListNhomVang);
                            });
                        }
                    }
                }
                catch (MySqlException sqlEx)
                {
                    Debug.WriteLine($"Lỗi SQL: {sqlEx.Message}");
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        await Shell.Current.DisplayAlert("Lỗi dữ liệu", $"Không thể truy vấn dữ liệu: {sqlEx.Message}", "OK");
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
                Debug.WriteLine($"Lỗi khi load dữ liệu nhóm vàng: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể tải dữ liệu nhóm vàng. Vui lòng thử lại sau.", "OK");
                });
            }
        }

        private void OnShowAddForm()
        {
            IsAdding = true;
            IsEditing = false;
            FormNhomVang = new NhomVangModel
            {
                SuDung = 0 // Mặc định SU_DUNG = 0 khi thêm mới
            };
        }

        private void OnShowEditForm()
        {
            if (SelectedNhomVang != null)
            {
                IsEditing = true;
                IsAdding = false;

                // Clone the selected item properties to FormNhomVang
                FormNhomVang = new NhomVangModel
                {
                    LoaiID = SelectedNhomVang.LoaiID,
                    MaLoai = SelectedNhomVang.MaLoai,
                    TenNhom = SelectedNhomVang.TenNhom,
                    KyHieu = SelectedNhomVang.KyHieu,
                    SuDung = SelectedNhomVang.SuDung
                };
            }
        }

        private async void OnSaveAdd()
        {
            if (string.IsNullOrWhiteSpace(FormNhomVang.TenNhom))
            {
                await Shell.Current.DisplayAlert("Lỗi", "⚠️ Tên nhóm vàng không được để trống!", "OK");
                return;
            }

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "⚠️ Chưa kết nối CSDL, không thể thêm dữ liệu!", "OK");
                    return;
                }

                await using (conn)
                {
                    // Step 1: Get the maximum LOAIID
                    string getMaxIdQuery = "SELECT MAX(LOAIID) FROM loai_hang";
                    int nextLoaiID;

                    await using (var cmd = new MySqlCommand(getMaxIdQuery, conn))
                    {
                        object result = await cmd.ExecuteScalarAsync();
                        nextLoaiID = result != null && result != DBNull.Value ? Convert.ToInt32(result) + 1 : 1;
                    }

                    // Step 2: Find a unique LOAIMA
                    int nextLoaiMa = 1;
                    bool isUnique = false;

                    // First get the current max
                    string getMaxMaQuery = "SELECT MAX(LOAIMA) FROM loai_hang";
                    await using (var cmd = new MySqlCommand(getMaxMaQuery, conn))
                    {
                        object result = await cmd.ExecuteScalarAsync();
                        nextLoaiMa = result != null && result != DBNull.Value ? Convert.ToInt32(result) + 1 : 1;
                    }

                    // Then verify it's unique, if not, keep incrementing until we find a unique value
                    while (!isUnique)
                    {
                        string checkExistsQuery = "SELECT COUNT(*) FROM loai_hang WHERE LOAIMA = @LoaiMa";
                        await using (var cmd = new MySqlCommand(checkExistsQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@LoaiMa", nextLoaiMa);

                            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            if (count == 0)
                            {
                                isUnique = true;
                            }
                            else
                            {
                                nextLoaiMa++;
                            }
                        }
                    }

                    // Step 3: Insert the new record
                    string insertQuery = @"
                        INSERT INTO loai_hang (LOAIID, LOAIMA, LOAI_TEN, GHI_CHU, SU_DUNG)
                        VALUES (@LoaiID, @MaLoai, @TenNhom, @KyHieu, @SuDung)";

                    await using (var insertCmd = new MySqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@LoaiID", nextLoaiID);
                        insertCmd.Parameters.AddWithValue("@MaLoai", nextLoaiMa);
                        insertCmd.Parameters.AddWithValue("@TenNhom", FormNhomVang.TenNhom);
                        insertCmd.Parameters.AddWithValue("@KyHieu", string.IsNullOrEmpty(FormNhomVang.KyHieu) ? "" : FormNhomVang.KyHieu);
                        insertCmd.Parameters.AddWithValue("@SuDung", 0); // Mặc định là 0

                        await insertCmd.ExecuteNonQueryAsync();
                    }
                }

                await Shell.Current.DisplayAlert("Thành công", "✅ Đã thêm nhóm vàng mới!", "OK");
                IsAdding = false;
                FormNhomVang = new NhomVangModel();
                await LoadNhomVangAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi thêm nhóm vàng: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", $"❌ Không thể thêm nhóm vàng.\n{ex.Message}", "OK");
            }
        }

        private async void OnSaveEdit()
        {
            if (string.IsNullOrWhiteSpace(FormNhomVang.TenNhom))
            {
                await Shell.Current.DisplayAlert("Lỗi", "⚠️ Tên nhóm vàng không được để trống!", "OK");
                return;
            }

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "⚠️ Chưa kết nối CSDL, không thể cập nhật dữ liệu!", "OK");
                    return;
                }

                string query = @"
                    UPDATE loai_hang
                    SET LOAI_TEN = @TenNhom, GHI_CHU = @KyHieu, SU_DUNG = @SuDung
                    WHERE LOAIID = @LoaiID";

                await using (conn)
                await using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenNhom", FormNhomVang.TenNhom);
                    cmd.Parameters.AddWithValue("@KyHieu", string.IsNullOrEmpty(FormNhomVang.KyHieu) ? "" : FormNhomVang.KyHieu);
                    cmd.Parameters.AddWithValue("@SuDung", FormNhomVang.SuDung);
                    cmd.Parameters.AddWithValue("@LoaiID", FormNhomVang.LoaiID);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                        await Shell.Current.DisplayAlert("Thành công", "✅ Đã cập nhật nhóm vàng!", "OK");
                    else
                        await Shell.Current.DisplayAlert("Thông báo", "⚠️ Không tìm thấy nhóm vàng để cập nhật!", "OK");
                }

                IsEditing = false;
                FormNhomVang = new NhomVangModel();
                await LoadNhomVangAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi cập nhật nhóm vàng: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", $"❌ Không thể cập nhật nhóm vàng.\n{ex.Message}", "OK");
            }
        }

        private async void OnDelete()
        {
            if (SelectedNhomVang == null)
            {
                await Shell.Current.DisplayAlert("Thông báo", "⚠️ Vui lòng chọn nhóm vàng để xóa!", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Bạn có chắc muốn xóa nhóm vàng \"{SelectedNhomVang.TenNhom}\" không?", "Có", "Không");
            if (!confirm) return;

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "⚠️ Chưa kết nối CSDL, không thể xóa dữ liệu!", "OK");
                    return;
                }

                string query = "DELETE FROM loai_hang WHERE LOAIID = @LoaiID";

                await using (conn)
                await using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LoaiID", SelectedNhomVang.LoaiID);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        await Shell.Current.DisplayAlert("Thành công", "✅ Đã xóa nhóm vàng!", "OK");
                        SelectedNhomVang = null;
                        await LoadNhomVangAsync();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Lỗi", "❌ Không tìm thấy nhóm vàng để xóa!", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi xóa nhóm vàng: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", $"❌ Không thể xóa nhóm vàng.\n{ex.Message}", "OK");
            }
        }

    }
}