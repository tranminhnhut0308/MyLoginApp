using CommunityToolkit.Mvvm.ComponentModel;
using MyLoginApp.Helpers;
using MyLoginApp.Models;
using MySqlConnector;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyLoginApp.ViewModels.BaoCao
{
    public partial class PhieuDoiViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<PhieuDoiModel> danhSachPhieuDoi = new();  // Danh sách các phiếu đổi

        [ObservableProperty]
        private bool isLoading;  // Trạng thái đang tải

        [ObservableProperty]
        private int pageSize = 20; // Mỗi trang sẽ hiển thị tối đa 20 phiếu

        [ObservableProperty]
        private int currentPage = 1; // Trang hiện tại

        // Các property để xác định khả năng chuyển trang
        public bool CanGoNext => DanhSachPhieuDoi.Count >= PageSize;
        public bool CanGoPrevious => CurrentPage > 1;
        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                SetProperty(ref _searchKeyword, value);
                CurrentPage = 1;
                _ = LoadPhieuDoiAsync(_searchKeyword);
            }
        }
        // Hàm tải danh sách phiếu đổi từ cơ sở dữ liệu
        public async Task LoadPhieuDoiAsync(string searchKeyword = "")
        {
            try
            {
                IsLoading = true;
                DanhSachPhieuDoi.Clear();

                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể kết nối database", "OK");
                    return;
                }

                // Câu lệnh SQL để truy vấn dữ liệu
                string query = @"
                    SELECT 
                        phd_phieu_doi.PHIEU_DOI_ID, 
                        phd_phieu_doi.PHIEU_DOI_MA, 
                        phd_phieu_doi.PHIEU_XUAT_ID,
                        phd_phieu_doi.PHIEU_MUA_VAO_ID,
                        phd_phieu_doi.TRI_GIA_BAN,
                        phd_phieu_doi.TRI_GIA_MUA,
                        phd_phieu_doi.THANH_TOAN,
                        phx_phieu_xuat.PHIEU_XUAT_MA,
                        phm_phieu_mua_vao.PHIEU_MA
                    FROM phd_phieu_doi
                    JOIN phx_phieu_xuat ON phd_phieu_doi.PHIEU_XUAT_ID = phx_phieu_xuat.PHIEU_XUAT_ID
                    JOIN phm_phieu_mua_vao ON phd_phieu_doi.PHIEU_MUA_VAO_ID = phm_phieu_mua_vao.PHIEU_MUA_VAO_ID
                    JOIN phx_khach_hang ON phd_phieu_doi.KH_ID = phx_khach_hang.KH_ID";

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    query += " AND phd_phieu_doi.PHIEU_DOI_MA LIKE @Search";
                }

                query += " ORDER BY phd_phieu_doi.PHIEU_DOI_ID DESC LIMIT @PageSize OFFSET @Offset";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PageSize", PageSize);
                cmd.Parameters.AddWithValue("@Offset", (CurrentPage - 1) * PageSize);

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<PhieuDoiModel>();

                while (await reader.ReadAsync())
                {
                    list.Add(new PhieuDoiModel
                    {
                        PhieuDoiId = reader.GetInt32("PHIEU_DOI_ID"),
                        PhieuDoiMa = reader.GetString("PHIEU_DOI_MA"),
                        PhieuXuatId = reader.GetInt32("PHIEU_XUAT_ID"),
                        PhieuMuaVaoId = reader.GetInt32("PHIEU_MUA_VAO_ID"),
                        TriGiaBan = reader.GetDecimal("TRI_GIA_BAN"),
                        TriGiaMua = reader.GetDecimal("TRI_GIA_MUA"),
                        ThanhToan = reader.GetDecimal("THANH_TOAN"),
                        PhieuXuatMa = reader.GetString("PHIEU_XUAT_MA"),
                        PhieuMuaMa = reader.GetString("PHIEU_MA")
                    });
                }

                if (list.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Thông báo", "Không có phiếu đổi nào", "OK");
                }

                foreach (var item in list)
                {
                    DanhSachPhieuDoi.Add(item);
                }

                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Lỗi khi tải phiếu đổi: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
