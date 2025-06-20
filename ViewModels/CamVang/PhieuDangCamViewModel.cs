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
    private decimal tongGiaTri;
    private decimal tongTienThem;
    private decimal tongTLHotAll;
    private decimal tongThanhTienAll;
    private decimal tongGiaGocAll;
    private decimal tongLaiLoAll;
    private decimal _tongTruHotAll;

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
        set => SetProperty(ref tongTLHot, value);
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
    public decimal TongGiaTri
    {
        get => tongGiaTri;
        set => SetProperty(ref tongGiaTri, value);
    }
    public decimal TongTienThem
    {
        get => tongTienThem;
        set => SetProperty(ref tongTienThem, value);
    }
    public decimal TongTLHotAll
    {
        get => tongTLHotAll;
        set => SetProperty(ref tongTLHotAll, value);
    }
    public decimal TongThanhTienAll
    {
        get => tongThanhTienAll;
        set => SetProperty(ref tongThanhTienAll, value);
    }
    public decimal TongGiaGocAll
    {
        get => tongGiaGocAll;
        set => SetProperty(ref tongGiaGocAll, value);
    }
    public decimal TongLaiLoAll
    {
        get => tongLaiLoAll;
        set => SetProperty(ref tongLaiLoAll, value);
    }
    public decimal TongTruHotAll
    {
        get => _tongTruHotAll;
        set => SetProperty(ref _tongTruHotAll, value);
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
                SELECT COUNT(*) FROM (
                    SELECT cam_phieu_cam_vang.PHIEU_CAM_VANG_ID
                    FROM cam_phieu_cam_vang
                    LEFT JOIN cam_chi_tiet_phieu_cam_vang ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_chi_tiet_phieu_cam_vang.PHIEU_CAM_VANG_ID
                    LEFT JOIN phx_khach_hang ON cam_phieu_cam_vang.KH_ID = phx_khach_hang.KH_ID
                    WHERE cam_phieu_cam_vang.DA_THANH_TOAN IS NULL 
                        AND cam_phieu_cam_vang.THANH_LY IS NULL 
                        AND cam_chi_tiet_phieu_cam_vang.SU_DUNG = 1";
            if (!string.IsNullOrEmpty(searchText))
            {
                countQuery += " AND (cam_phieu_cam_vang.PHIEU_MA LIKE @SearchText OR phx_khach_hang.KH_TEN LIKE @SearchText)";
            }
            countQuery += " GROUP BY cam_phieu_cam_vang.PHIEU_CAM_VANG_ID ) AS sub";

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
                    SUM(T.CAN_TONG) AS TONG_CAN,
                    SUM(T.TL_HOT) AS TONG_TL_HOT,
                    SUM(T.CAN_TONG - T.TL_HOT) AS TONG_TL_THUC,
                    SUM(T.TONG_GIA_TRI) AS TONG_GIA_TRI,
                    SUM(T.TIEN_KHACH_NHAN) AS TONG_TIEN_NHAN,
                    SUM(T.TIEN_THEM) AS TONG_TIEN_THEM,
                    SUM(T.TIEN_CAM_MOI) AS TONG_TIEN_CAM_MOI
                FROM (
                    SELECT 
                        cam_phieu_cam_vang.PHIEU_CAM_VANG_ID,
                        cam_phieu_cam_vang.CAN_TONG,
                        cam_phieu_cam_vang.TL_HOT,
                        cam_phieu_cam_vang.TONG_GIA_TRI,
                        cam_phieu_cam_vang.TIEN_KHACH_NHAN,
                        IF(cam_nhan_tien_them.TIEN_THEM IS NULL, 0, cam_nhan_tien_them.TIEN_THEM) AS TIEN_THEM,
                        cam_phieu_cam_vang.TIEN_KHACH_NHAN + IF(cam_nhan_tien_them.TIEN_THEM IS NULL, 0, cam_nhan_tien_them.TIEN_THEM) AS TIEN_CAM_MOI
                    FROM cam_phieu_cam_vang
                    LEFT JOIN cam_chi_tiet_phieu_cam_vang ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_chi_tiet_phieu_cam_vang.PHIEU_CAM_VANG_ID
                    LEFT JOIN cam_nhan_tien_them ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_nhan_tien_them.PHIEU_CAM_ID
                    WHERE cam_phieu_cam_vang.DA_THANH_TOAN IS NULL 
                      AND cam_phieu_cam_vang.THANH_LY IS NULL
                      AND cam_chi_tiet_phieu_cam_vang.SU_DUNG = 1
                    GROUP BY cam_phieu_cam_vang.PHIEU_CAM_VANG_ID
                ) T
            ";
            if (!string.IsNullOrEmpty(searchText))
            {
                // Nếu có tìm kiếm, thêm điều kiện vào subquery
                int idx = sumQuery.IndexOf("GROUP BY");
                sumQuery = sumQuery.Insert(idx, " AND (cam_phieu_cam_vang.PHIEU_MA LIKE @SearchText OR cam_phieu_cam_vang.KH_ID IN (SELECT KH_ID FROM phx_khach_hang WHERE KH_TEN LIKE @SearchText)) ");
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
                        TongTLThuc = sumReader["TONG_TL_THUC"] == DBNull.Value ? 0 : Convert.ToDecimal(sumReader["TONG_TL_THUC"]);
                        TongGiaTri = sumReader["TONG_GIA_TRI"] == DBNull.Value ? 0 : Convert.ToDecimal(sumReader["TONG_GIA_TRI"]);
                        TongTienNhan = sumReader["TONG_TIEN_NHAN"] == DBNull.Value ? 0 : Convert.ToDecimal(sumReader["TONG_TIEN_NHAN"]);
                        TongTienThem = sumReader["TONG_TIEN_THEM"] == DBNull.Value ? 0 : Convert.ToDecimal(sumReader["TONG_TIEN_THEM"]);
                        TongTienCamMoi = sumReader["TONG_TIEN_CAM_MOI"] == DBNull.Value ? 0 : Convert.ToDecimal(sumReader["TONG_TIEN_CAM_MOI"]);
                    }
                }
            }

            string query = @"
                SELECT 
                    cam_phieu_cam_vang.PHIEU_CAM_VANG_ID,
                    cam_phieu_cam_vang.PHIEU_MA,
                    phx_khach_hang.KH_TEN,
                    DATE_FORMAT(cam_phieu_cam_vang.TU_NGAY, '%d/%m/%Y') as TU_NGAY,
                    DATE_FORMAT(cam_phieu_cam_vang.DEN_NGAY, '%d/%m/%Y') as DEN_NGAY,
                    cam_phieu_cam_vang.CAN_TONG,
                    cam_phieu_cam_vang.TL_HOT,
                    cam_phieu_cam_vang.CAN_TONG - cam_phieu_cam_vang.TL_HOT as TL_THUC,
                    cam_phieu_cam_vang.TONG_GIA_TRI,
                    cam_phieu_cam_vang.TIEN_KHACH_NHAN,
                    cam_phieu_cam_vang.LAI_XUAT,
                    IF(cam_nhan_tien_them.TIEN_THEM is null, 0, cam_nhan_tien_them.TIEN_THEM) as 'TIEN_THEM',
                    cam_phieu_cam_vang.TIEN_KHACH_NHAN + IF(cam_nhan_tien_them.TIEN_THEM is null, 0, cam_nhan_tien_them.TIEN_THEM) as 'TIEN_MOI'
                FROM cam_phieu_cam_vang
                LEFT JOIN cam_chi_tiet_phieu_cam_vang ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_chi_tiet_phieu_cam_vang.PHIEU_CAM_VANG_ID
                LEFT JOIN phx_khach_hang ON cam_phieu_cam_vang.KH_ID = phx_khach_hang.KH_ID
                LEFT JOIN cam_nhan_tien_them ON cam_phieu_cam_vang.PHIEU_CAM_VANG_ID = cam_nhan_tien_them.PHIEU_CAM_ID
                WHERE 
                    cam_phieu_cam_vang.DA_THANH_TOAN is null
                    AND cam_phieu_cam_vang.THANH_LY is null
                    AND cam_chi_tiet_phieu_cam_vang.SU_DUNG = 1";

            if (!string.IsNullOrEmpty(searchText))
            {
                query += " AND (cam_phieu_cam_vang.PHIEU_MA LIKE @SearchText OR phx_khach_hang.KH_TEN LIKE @SearchText)";
            }

            query += " GROUP BY cam_phieu_cam_vang.PHIEU_CAM_VANG_ID ";
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
                    NgayCam = DateTime.TryParseExact(reader["TU_NGAY"].ToString(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var ngayCam) ? ngayCam : DateTime.MinValue,
                    NgayQuaHan = DateTime.TryParseExact(reader["DEN_NGAY"].ToString(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var ngayQH) ? ngayQH : DateTime.MinValue,
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

            // Đảm bảo reader đã đóng trước khi mở truy vấn mới
            if (!reader.IsClosed) reader.Close();

            string sumQueryAll = @"
                SELECT 
                    SUM(danh_muc_hang_hoa.CAN_TONG) AS TongCanTong,
                    SUM(danh_muc_hang_hoa.TL_HOT) AS TongTLHot,
                    SUM(danh_muc_hang_hoa.CAN_TONG - danh_muc_hang_hoa.TL_HOT) AS TongTruHot,
                    SUM(phx_chi_tiet_phieu_xuat.THANH_TIEN) AS TongThanhTien,
                    SUM(danh_muc_hang_hoa.DON_GIA_GOC * (danh_muc_hang_hoa.CAN_TONG - danh_muc_hang_hoa.TL_HOT) + danh_muc_hang_hoa.CONG_GOC) AS TongGiaGoc,
                    SUM(phx_chi_tiet_phieu_xuat.THANH_TIEN - (danh_muc_hang_hoa.DON_GIA_GOC * (danh_muc_hang_hoa.CAN_TONG - danh_muc_hang_hoa.TL_HOT) + danh_muc_hang_hoa.CONG_GOC)) AS TongLaiLo
                FROM phx_phieu_xuat
                JOIN phx_chi_tiet_phieu_xuat ON phx_phieu_xuat.PHIEU_XUAT_ID = phx_chi_tiet_phieu_xuat.PHIEU_XUAT_ID
                JOIN danh_muc_hang_hoa ON danh_muc_hang_hoa.HANGHOAID = phx_chi_tiet_phieu_xuat.HANGHOAID";
            if (!string.IsNullOrEmpty(searchText))
            {
                sumQueryAll += " WHERE phx_phieu_xuat.PHIEU_XUAT_MA LIKE @Search";
            }
            using (var sumCmdALL = new MySqlCommand(sumQueryAll, conn))
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    sumCmdALL.Parameters.AddWithValue("@Search", $"%{searchText}%");
                }
                using (var sumReaderALL = await sumCmdALL.ExecuteReaderAsync())
                {
                    if (await sumReaderALL.ReadAsync())
                    {
                        int idxTongCanTong = sumReaderALL.GetOrdinal("TongCanTong");
                        int idxTongTLHot = sumReaderALL.GetOrdinal("TongTLHot");
                        int idxTongTruHot = sumReaderALL.GetOrdinal("TongTruHot");
                        int idxTongThanhTien = sumReaderALL.GetOrdinal("TongThanhTien");
                        int idxTongGiaGoc = sumReaderALL.GetOrdinal("TongGiaGoc");
                        int idxTongLaiLo = sumReaderALL.GetOrdinal("TongLaiLo");

                        TongCanTong = sumReaderALL.IsDBNull(idxTongCanTong) ? 0 : sumReaderALL.GetDecimal(idxTongCanTong);
                        TongTLHotAll = sumReaderALL.IsDBNull(idxTongTLHot) ? 0 : sumReaderALL.GetDecimal(idxTongTLHot);
                        TongTruHotAll = sumReaderALL.IsDBNull(idxTongTruHot) ? 0 : sumReaderALL.GetDecimal(idxTongTruHot);
                        TongThanhTienAll = sumReaderALL.IsDBNull(idxTongThanhTien) ? 0 : sumReaderALL.GetDecimal(idxTongThanhTien);
                        TongGiaGocAll = sumReaderALL.IsDBNull(idxTongGiaGoc) ? 0 : sumReaderALL.GetDecimal(idxTongGiaGoc);
                        TongLaiLoAll = sumReaderALL.IsDBNull(idxTongLaiLo) ? 0 : sumReaderALL.GetDecimal(idxTongLaiLo);
                    }
                }
            }
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
