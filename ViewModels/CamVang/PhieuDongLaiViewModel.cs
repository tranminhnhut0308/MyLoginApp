using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySqlConnector;
using MyLoginApp.Models;
using MyLoginApp.Helpers;

public class PhieuDongLaiViewModel : ObservableObject
{
    public ObservableCollection<PhieuDongLaiModel> DanhSachPhieuDongLai { get; set; } = new ObservableCollection<PhieuDongLaiModel>();

    private string _searchKeyword;
    public string SearchKeyword
    {
        get => _searchKeyword;
        set
        {
            SetProperty(ref _searchKeyword, value);
            CurrentPage = 1;
            _ = LoadPhieuDongLaiAsync(_searchKeyword);
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private int _currentPage = 1;
    private const int PageSize = 10;

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            SetProperty(ref _currentPage, value);
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
        }
    }

    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => DanhSachPhieuDongLai.Count >= PageSize;

    public ICommand LoadDataCommand => new AsyncRelayCommand(() => LoadPhieuDongLaiAsync(SearchKeyword));
    public ICommand GoNextPageCommand => new RelayCommand(NextPage);
    public ICommand GoPreviousPageCommand => new RelayCommand(PreviousPage);

    private void NextPage()
    {
        CurrentPage++;
        _ = LoadPhieuDongLaiAsync(SearchKeyword);
    }

    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            _ = LoadPhieuDongLaiAsync(SearchKeyword);
        }
    }

    public async Task LoadPhieuDongLaiAsync(string SearchKeyword = "")
    {
        try
        {
            IsLoading = true;
            DanhSachPhieuDongLai.Clear();

            using var conn = await DatabaseHelper.GetOpenConnectionAsync();
            if (conn == null)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Không thể kết nối database", "OK");
                return;
            }

            string query = @"
            SELECT 
                cam_phieu_cam_vang.PHIEU_CAM_VANG_ID,
                cam_phieu_cam_vang.PHIEU_MA AS PHIEU_CAM_MA,
                cam_nhan_tien_them.PHIEU_MA AS PHIEU_DL_MA,
                DATE_FORMAT(cam_phieu_cam_vang.TU_NGAY, '%d/%m/%Y') AS TU_NGAY,
                DATE_FORMAT(cam_nhan_tien_them.NGAY, '%d/%m/%Y') AS NGAY,
                phx_khach_hang.KH_TEN,
                cam_nhan_tien_them.KHACH_DUA
            FROM cam_nhan_tien_them
            INNER JOIN cam_phieu_cam_vang ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_nhan_tien_them.PHIEU_CAM_ID
            INNER JOIN phx_khach_hang ON phx_khach_hang.KH_ID = cam_phieu_cam_vang.KH_ID
            WHERE 1 = 1";

            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                query += " AND (phx_khach_hang.KH_TEN LIKE @Search OR cam_phieu_cam_vang.PHIEU_MA LIKE @Search OR cam_nhan_tien_them.PHIEU_MA LIKE @Search)";
            }

            query += " ORDER BY cam_nhan_tien_them.NGAY DESC LIMIT @PageSize OFFSET @Offset";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PageSize", PageSize);
            cmd.Parameters.AddWithValue("@Offset", (CurrentPage - 1) * PageSize);

            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                cmd.Parameters.AddWithValue("@Search", $"%{SearchKeyword}%");
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                DanhSachPhieuDongLai.Add(new PhieuDongLaiModel
                {
                    MaPhieuCam = reader["PHIEU_CAM_MA"].ToString(),
                    MaPhieuDongLai = reader["PHIEU_DL_MA"].ToString(),
                    TenKhachHang = reader["KH_TEN"].ToString(),

                    // Dữ liệu ngày sẽ được giữ nguyên dưới dạng string
                    NgayCam = reader["TU_NGAY"].ToString(),
                    NgayDongLai = reader["NGAY"].ToString(),

                    KhachDongTien = Convert.ToDecimal(reader["KHACH_DUA"])
                });
            }

            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
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