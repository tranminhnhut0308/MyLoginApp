using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyLoginApp.Helpers;
using MyLoginApp.Models;
using MySqlConnector;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace MyLoginApp.ViewModels
{
    public partial class PhieuQuaHanViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<PhieuQuaHanModel> danhSachPhieuQuaHan = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int pageSize = 10;

        [ObservableProperty]
        private string searchKeyword = string.Empty;

        partial void OnSearchKeywordChanged(string value)
        {
            CurrentPage = 1;
            _ = LoadPhieuQuaHanAsync();
        }

        public bool CanGoNext => DanhSachPhieuQuaHan.Count >= PageSize;
        public bool CanGoPrevious => CurrentPage > 1;

        public IAsyncRelayCommand LoadPhieuQuaHanCommand { get; }
        public IRelayCommand NextPageCommand { get; }
        public IRelayCommand PreviousPageCommand { get; }

        private CancellationTokenSource _searchCancellationTokenSource;

        public PhieuQuaHanViewModel()
        {
            LoadPhieuQuaHanCommand = new AsyncRelayCommand(LoadPhieuQuaHanAsync);
            NextPageCommand = new RelayCommand(NextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage);
        }

        private void NextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                LoadPhieuQuaHanCommand.Execute(null);
            }
        }

        private void PreviousPage()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                LoadPhieuQuaHanCommand.Execute(null);
            }
        }

        public async Task LoadPhieuQuaHanAsync()
        {
            try
            {
                IsLoading = true;
                DanhSachPhieuQuaHan.Clear();

                _searchCancellationTokenSource?.Cancel();
                _searchCancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _searchCancellationTokenSource.Token;

                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể kết nối database", "OK");
                    return;
                }

                string query = @"
                    SELECT 
                        cam_phieu_cam_vang.PHIEU_CAM_VANG_ID,
                        cam_phieu_cam_vang.PHIEU_MA,
                        phx_khach_hang.KH_TEN,
                        cam_phieu_cam_vang.TU_NGAY,
                        cam_phieu_cam_vang.DEN_NGAY,
                        DATEDIFF(DATE(cam_phieu_cam_vang.DEN_NGAY), CURDATE()) AS SONGAY,
                        cam_phieu_cam_vang.CAN_TONG,
                        cam_phieu_cam_vang.TONG_GIA_TRI,
                        cam_phieu_cam_vang.TIEN_KHACH_NHAN,
                        cam_phieu_cam_vang.LAI_XUAT
                    FROM 
                        cam_phieu_cam_vang
                    INNER JOIN 
                        cam_chi_tiet_phieu_cam_vang ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_chi_tiet_phieu_cam_vang.PHIEU_CAM_VANG_ID
                    INNER JOIN 
                        phx_khach_hang ON phx_khach_hang.KH_ID = cam_phieu_cam_vang.KH_ID
                    WHERE 
                        cam_phieu_cam_vang.DA_THANH_TOAN IS NULL 
                        AND cam_phieu_cam_vang.THANH_LY IS NULL
                        AND DATE(cam_phieu_cam_vang.DEN_NGAY) <= CURDATE()";

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    query += " AND phx_khach_hang.KH_TEN LIKE @Search";
                }

                query += " ORDER BY cam_phieu_cam_vang.PHIEU_CAM_VANG_ID DESC LIMIT @PageSize OFFSET @Offset";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PageSize", PageSize);
                cmd.Parameters.AddWithValue("@Offset", (CurrentPage - 1) * PageSize);

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{SearchKeyword}%");
                }

                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                var list = new List<PhieuQuaHanModel>();

                while (await reader.ReadAsync())
                {
                    list.Add(new PhieuQuaHanModel
                    {
                        Ma_Phieu = reader["PHIEU_MA"].ToString(),
                        Ten_KH = reader["KH_TEN"].ToString(),
                        TU_NGAY = reader.IsDBNull(reader.GetOrdinal("TU_NGAY")) ? null : reader.GetDateTime("TU_NGAY"),
                        DEN_NGAY = reader.IsDBNull(reader.GetOrdinal("DEN_NGAY")) ? null : reader.GetDateTime("DEN_NGAY"),
                        SONGAY = reader.GetInt32("SONGAY"),
                        TL_Thuc = reader.GetDouble("CAN_TONG"),
                        Dinh_Gia = reader.GetDouble("TONG_GIA_TRI"),
                        TIEN_KHACH_NHAN = reader.GetDouble("TIEN_KHACH_NHAN"),
                        LAI_XUAT = reader.GetDouble("LAI_XUAT")
                    });
                }

                if (list.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Thông báo", "Không có dữ liệu phù hợp", "OK");
                }

                foreach (var item in list)
                {
                    DanhSachPhieuQuaHan.Add(item);
                }

                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Tác vụ tìm kiếm trước đó đã bị hủy.");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Lỗi khi tải dữ liệu: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
