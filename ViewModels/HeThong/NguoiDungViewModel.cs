using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MyLoginApp.Models;
using MySqlConnector;
using MyLoginApp.Helpers;

namespace MyLoginApp.ViewModels
{
    public partial class NguoiDungViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<NguoiDungModel> danhSachNguoiDung = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string searchKeyword = string.Empty;

        [ObservableProperty]
        private string ketQuaTimKiem = string.Empty;

        [ObservableProperty]
        private int pageSize = 20; // mặc định mỗi trang 20 user

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int tongSoNguoiDung = 0;

        public bool CanGoNext => DanhSachNguoiDung.Count >= PageSize;
        public bool CanGoPrevious => CurrentPage > 1;

        public async Task LoadNguoiDungAsync(string searchKeyword = "")
        {
            try
            {
                IsLoading = true;
                DanhSachNguoiDung.Clear();

                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể kết nối database", "OK");
                    return;
                }

                // Đếm tổng số người dùng không trùng lặp
                string countQuery = @"
            SELECT COUNT(DISTINCT USER_TEN) AS TongSo
            FROM pq_user
            WHERE su_dung = 1";

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    countQuery += " AND USER_TEN LIKE @Search";
                }

                using var countCmd = new MySqlCommand(countQuery, conn);
                
                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    countCmd.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                }

                var totalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                TongSoNguoiDung = totalCount;

                // Truy vấn để lấy danh sách người dùng không trùng lặp
                string query = @"
            SELECT u.* 
            FROM pq_user u
            JOIN (
                SELECT USER_TEN, MAX(USER_ID) AS max_id
                FROM pq_user
                WHERE su_dung = 1
                GROUP BY USER_TEN
            ) as grouped
            ON u.USER_TEN = grouped.USER_TEN AND u.USER_ID = grouped.max_id
            WHERE u.su_dung = 1";

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    query += " AND u.USER_TEN LIKE @Search";
                    KetQuaTimKiem = $"Kết quả tìm kiếm cho '{searchKeyword}': {totalCount} người dùng";
                }
                else
                {
                    KetQuaTimKiem = $"Tổng số: {totalCount} người dùng";
                }

                query += " ORDER BY u.USER_ID DESC LIMIT @PageSize OFFSET @Offset";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PageSize", PageSize);
                cmd.Parameters.AddWithValue("@Offset", (CurrentPage - 1) * PageSize);

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<NguoiDungModel>();

                while (await reader.ReadAsync())
                {
                    list.Add(new NguoiDungModel
                    {
                        UserId = reader["USER_MA"].ToString(),
                        UserName = reader["USER_TEN"].ToString(),
                        Password = reader["MAT_KHAU"].ToString(),
                        BiKhoa = reader.GetInt32("BIKHOA"),
                        LyDoKhoa = string.IsNullOrEmpty(reader["LY_DO_KHOA"].ToString()) ? "Không có" : reader["LY_DO_KHOA"].ToString(),
                        NgayTao = reader.IsDBNull(reader.GetOrdinal("NGAY_TAO")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("NGAY_TAO")),
                        NhomUserId = reader.GetInt32("USER_ID")
                    });
                }

                if (list.Count == 0 && !string.IsNullOrEmpty(searchKeyword))
                {
                    await Shell.Current.DisplayAlert("Thông báo", "Không tìm thấy người dùng phù hợp", "OK");
                }
                else if (list.Count == 0 && CurrentPage == 1)
                {
                    await Shell.Current.DisplayAlert("Thông báo", "Không có người dùng nào", "OK");
                }

                foreach (var item in list)
                {
                    DanhSachNguoiDung.Add(item);
                }

                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Lỗi khi tải người dùng: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
