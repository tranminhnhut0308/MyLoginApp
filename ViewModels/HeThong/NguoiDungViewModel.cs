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
        private ObservableCollection<NguoiDungModel> danhSachNguoiDung = new();  // Make sure this is public

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private int pageSize = 20; // mặc định mỗi trang 20 user

        [ObservableProperty]
        private int currentPage = 1;

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

                // Sử dụng SELECT * để lấy tất cả cột
                string query = @"
            SELECT * 
            FROM pq_user
            WHERE su_dung = 1";  // Chỉ lấy người dùng có su_dung = 1

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    query += " AND USER_TEN LIKE @Search";
                }

                query += " ORDER BY user_id DESC LIMIT @PageSize OFFSET @Offset";

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

                if (list.Count == 0)
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
