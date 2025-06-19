using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySqlConnector;
using MyLoginApp.Models;
using MyLoginApp.Helpers;

public class PhieuDangCamViewModel : ObservableObject
{
    private int currentPage = 1;
    private int totalPages = 1;
    private bool canGoNext = false;
    private bool canGoPrevious = false;
    private string searchKeyword = string.Empty;
    private int tongSoPhieu;
    private decimal tongCanTong;
    private decimal tongCanTongLuong;
    private decimal tongSoLuongVang;
    private decimal tongTLHot;
    private decimal tongTLThuc;
    private decimal tongTienNhan;
    private decimal tongTienCamMoi;

    public ObservableCollection<PhieuDangCamModel> DanhSachPhieuDangCam { get; set; } = new ObservableCollection<PhieuDangCamModel>();
    public int CurrentPage { get => currentPage; set => SetProperty(ref currentPage, value); }
    public int TotalPages { get => totalPages; set => SetProperty(ref totalPages, value); }
    public bool CanGoNext { get => canGoNext; set => SetProperty(ref canGoNext, value); }
    public bool CanGoPrevious { get => canGoPrevious; set => SetProperty(ref canGoPrevious, value); }
    public string SearchKeyword { get => searchKeyword; set => SetProperty(ref searchKeyword, value); }
    public int TongSoPhieu
    {
        get => tongSoPhieu;
        set => SetProperty(ref tongSoPhieu, value);
    }
    public decimal TongCanTong
    {
        get => tongCanTong;
        set
        {
            if (SetProperty(ref tongCanTong, value))
            {
                // Cập nhật TongTLThuc khi TongCanTong thay đổi
                TongTLThuc = value - TongTLHot;
            }
        }
    }
    public decimal TongCanTongLuong
    {
        get => tongCanTongLuong;
        set => SetProperty(ref tongCanTongLuong, value);
    }
    public decimal TongSoLuongVang
    {
        get => tongSoLuongVang;
        set => SetProperty(ref tongSoLuongVang, value);
    }
    public decimal TongTLHot
    {
        get => tongTLHot;
        set
        {
            if (SetProperty(ref tongTLHot, value))
            {
                // Cập nhật TongTLThuc khi TongTLHot thay đổi
                TongTLThuc = TongCanTong - value;
            }
        }
    }
    public decimal TongTLThuc
    {
        get => tongTLThuc;
        set => SetProperty(ref tongTLThuc, value);
    }
    public decimal TongTienNhan
    {
        get => tongTienNhan;
        set => SetProperty(ref tongTienNhan, value);
    }
    public decimal TongTienCamMoi
    {
        get => tongTienCamMoi;
        set => SetProperty(ref tongTienCamMoi, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    public int PageSize { get; set; } = 10;

    public ICommand GoNextPageCommand => new AsyncRelayCommand(GoNextPageAsync);
    public ICommand GoPreviousPageCommand => new AsyncRelayCommand(GoPreviousPageAsync);

    public async Task LoadPhieuDangCamWithPaginationAsync(int page, int pageSize, string searchText = "")
    {
        try
        {
            IsLoading = true;

            using var conn = await DatabaseHelper.GetOpenConnectionAsync();
            if (conn == null)
            {
                await Shell.Current.DisplayAlert("Lỗi", "⚠️ Không thể kết nối đến cơ sở dữ liệu!", "OK");
                return;
            }

            int offset = (page - 1) * pageSize;

            string countQuery = @"
                SELECT COUNT(DISTINCT cam_phieu_cam_vang.PHIEU_CAM_VANG_ID)
                FROM cam_phieu_cam_vang
                LEFT JOIN cam_chi_tiet_phieu_cam_vang ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_chi_tiet_phieu_cam_vang.PHIEU_CAM_VANG_ID
                LEFT JOIN phx_khach_hang ON cam_phieu_cam_vang.KH_ID = phx_khach_hang.KH_ID
                WHERE cam_phieu_cam_vang.DA_THANH_TOAN IS NULL 
                    AND cam_phieu_cam_vang.THANH_LY IS NULL 
                    AND cam_chi_tiet_phieu_cam_vang.SU_DUNG = 0";

            if (!string.IsNullOrEmpty(searchText))
            {
                countQuery += " AND (cam_phieu_cam_vang.PHIEU_MA LIKE @SearchText OR phx_khach_hang.KH_TEN LIKE @SearchText)";
            }

            using var countCmd = new MySqlCommand(countQuery, conn);
            if (!string.IsNullOrEmpty(searchText))
            {
                countCmd.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");
            }

            var totalRecords = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            CanGoNext = currentPage < TotalPages;
            CanGoPrevious = currentPage > 1;
            TongSoPhieu = totalRecords;

            // Lấy tổng cân tổng của toàn bộ phiếu (không chỉ trang hiện tại)
            string sumQuery = @"
                SELECT 
                    SUM(t.CAN_TONG) as TONG_CAN,
                    SUM(t.TL_HOT) as TONG_TL_HOT,
                    SUM(t.TIEN_KHACH_NHAN) as TONG_TIEN_NHAN,
                    SUM(t.TIEN_KHACH_NHAN + IFNULL(t.TIEN_THEM, 0)) as TONG_TIEN_CAM_MOI
                FROM (
                    SELECT DISTINCT 
                        cam_phieu_cam_vang.PHIEU_CAM_VANG_ID,
                        cam_phieu_cam_vang.CAN_TONG,
                        cam_phieu_cam_vang.TL_HOT,
                        cam_phieu_cam_vang.TIEN_KHACH_NHAN,
                        cam_nhan_tien_them.TIEN_THEM
                    FROM cam_phieu_cam_vang
                    LEFT JOIN cam_chi_tiet_phieu_cam_vang ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_chi_tiet_phieu_cam_vang.PHIEU_CAM_VANG_ID
                    LEFT JOIN phx_khach_hang ON cam_phieu_cam_vang.KH_ID = phx_khach_hang.KH_ID
                    LEFT JOIN cam_nhan_tien_them ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_nhan_tien_them.PHIEU_CAM_ID
                    WHERE cam_phieu_cam_vang.DA_THANH_TOAN IS NULL
                      AND cam_phieu_cam_vang.THANH_LY IS NULL
                      AND cam_chi_tiet_phieu_cam_vang.SU_DUNG = 0
                ) as t";
            if (!string.IsNullOrEmpty(searchText))
            {
                sumQuery += " AND (cam_phieu_cam_vang.PHIEU_MA LIKE @SearchText OR phx_khach_hang.KH_TEN LIKE @SearchText)";
            }
            using (var sumCmd = new MySqlCommand(sumQuery, conn))
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    sumCmd.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");
                }
                using (var sumReader = await sumCmd.ExecuteReaderAsync())
                {
                    if (await sumReader.ReadAsync())
                    {
                        TongCanTong = sumReader["TONG_CAN"] == DBNull.Value ? 0 : Convert.ToDecimal(sumReader["TONG_CAN"]);
                        TongTLHot = sumReader["TONG_TL_HOT"] == DBNull.Value ? 0 : Convert.ToDecimal(sumReader["TONG_TL_HOT"]);
                        TongTienNhan = sumReader["TONG_TIEN_NHAN"] == DBNull.Value ? 0 : Convert.ToDecimal(sumReader["TONG_TIEN_NHAN"]);
                        TongTienCamMoi = sumReader["TONG_TIEN_CAM_MOI"] == DBNull.Value ? 0 : Convert.ToDecimal(sumReader["TONG_TIEN_CAM_MOI"]);
                    }
                }
            }

            string query = @"
                SELECT 
                    cam_phieu_cam_vang.PHIEU_CAM_VANG_ID,
                    cam_phieu_cam_vang.PHIEU_MA,
                    phx_khach_hang.KH_TEN,
                    cam_phieu_cam_vang.TU_NGAY,
                    cam_phieu_cam_vang.DEN_NGAY,
                    cam_phieu_cam_vang.CAN_TONG,
                    cam_phieu_cam_vang.TL_HOT,
                    cam_phieu_cam_vang.CAN_TONG - cam_phieu_cam_vang.TL_HOT AS TL_THUC,
                    cam_phieu_cam_vang.TONG_GIA_TRI,
                    cam_phieu_cam_vang.TIEN_KHACH_NHAN,
                    cam_phieu_cam_vang.LAI_XUAT,
                    IF(cam_nhan_tien_them.TIEN_THEM IS NULL, 0, cam_nhan_tien_them.TIEN_THEM) AS TIEN_THEM,
                    cam_phieu_cam_vang.TIEN_KHACH_NHAN + IF(cam_nhan_tien_them.TIEN_THEM IS NULL, 0, cam_nhan_tien_them.TIEN_THEM) AS TIEN_MOI
                FROM cam_phieu_cam_vang
                LEFT JOIN cam_chi_tiet_phieu_cam_vang ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_chi_tiet_phieu_cam_vang.PHIEU_CAM_VANG_ID
                LEFT JOIN phx_khach_hang ON cam_phieu_cam_vang.KH_ID = phx_khach_hang.KH_ID
                LEFT JOIN cam_nhan_tien_them ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_nhan_tien_them.PHIEU_CAM_ID
                WHERE cam_phieu_cam_vang.DA_THANH_TOAN IS NULL 
                    AND cam_phieu_cam_vang.THANH_LY IS NULL 
                    AND cam_chi_tiet_phieu_cam_vang.SU_DUNG = 0";

            if (!string.IsNullOrEmpty(searchText))
            {
                query += " AND (cam_phieu_cam_vang.PHIEU_MA LIKE @SearchText OR phx_khach_hang.KH_TEN LIKE @SearchText)";
            }

            query += " ORDER BY cam_phieu_cam_vang.PHIEU_CAM_VANG_ID DESC LIMIT @PageSize OFFSET @Offset";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", offset);
            if (!string.IsNullOrEmpty(searchText))
            {
                cmd.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");
            }

            using var reader = await cmd.ExecuteReaderAsync();
            DanhSachPhieuDangCam.Clear();
            decimal sumSoLuongVang = 0;
            while (await reader.ReadAsync())
            {
                if (reader["KH_TEN"] == DBNull.Value)
                {
                    Console.WriteLine($"[DEBUG] Phiếu {reader["PHIEU_MA"]} không có KH_TEN (KH_ID null hoặc sai)");
                }
                var model = new PhieuDangCamModel
                {
                    MaPhieu = reader["PHIEU_MA"].ToString(),
                    TenKhachHang = reader["KH_TEN"].ToString(),
                    NgayCam = Convert.ToDateTime(reader["TU_NGAY"]),
                    NgayQuaHan = Convert.ToDateTime(reader["DEN_NGAY"]),
                    CanTong = Convert.ToDecimal(reader["CAN_TONG"]),
                    TLThuc = Convert.ToDecimal(reader["TL_THUC"]),
                    DinhGia = Convert.ToDecimal(reader["TONG_GIA_TRI"]),
                    TienKhachNhan = Convert.ToDecimal(reader["TIEN_KHACH_NHAN"]),
                    TienNhanThem = Convert.ToDecimal(reader["TIEN_THEM"]),
                    TienCamMoi = Convert.ToDecimal(reader["TIEN_MOI"]),
                    LaiSuat = Convert.ToDecimal(reader["LAI_XUAT"])
                };
                DanhSachPhieuDangCam.Add(model);
                sumSoLuongVang += model.DinhGia;
            }
            TongSoLuongVang = sumSoLuongVang;
            TongCanTongLuong = sumSoLuongVang / 37.5m;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"⚠️ Lỗi khi tải dữ liệu phiếu đang cầm: {ex.Message}", "OK");
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task GoNextPageAsync() => await ChangePageAsync(currentPage + 1);
    public async Task GoPreviousPageAsync() => await ChangePageAsync(currentPage - 1);

    private async Task ChangePageAsync(int newPage)
    {
        if (newPage > 0 && newPage <= TotalPages)
        {
            CurrentPage = newPage;
            await LoadPhieuDangCamWithPaginationAsync(CurrentPage, PageSize, SearchKeyword);
        }
    }

    public async Task OnSearchTextChanged(string searchText)
    {
        SearchKeyword = searchText;
        await LoadPhieuDangCamWithPaginationAsync(1, PageSize, searchText);
    }
}
