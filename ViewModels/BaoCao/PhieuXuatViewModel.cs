using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MyLoginApp.Helpers;
using MyLoginApp.Models;
using MyLoginApp.Models.BaoCao;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyLoginApp.ViewModels.BaoCao
{
    public partial class PhieuXuatViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<PhieuXuatModel> danhSachPhieuXuat = new();  // Danh sách các phiếu xuất

        [ObservableProperty]
        private bool isLoading;  // Trạng thái đang tải

        [ObservableProperty]
        private int pageSize = 5; // Mỗi trang sẽ hiển thị tối đa 5 phiếu

        [ObservableProperty]
        private int currentPage = 1; // Trang hiện tại

        [ObservableProperty]
        private int _totalPages = 1; // Tổng số trang

        // Các property để xác định khả năng chuyển trang
        public bool CanGoNext => CurrentPage < _totalPages;
        public bool CanGoPrevious => CurrentPage > 1;

        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                SetProperty(ref _searchKeyword, value);
                CurrentPage = 1;
                _ = LoadPhieuXuatAsync(_searchKeyword);
            }
        }

        [RelayCommand]
        private async Task GoNextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                await LoadPhieuXuatAsync(SearchKeyword);
            }
        }

        [RelayCommand]
        private async Task GoPreviousPage()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                await LoadPhieuXuatAsync(SearchKeyword);
            }
        }

        // Hàm tải danh sách phiếu xuất từ cơ sở dữ liệu
        public async Task LoadPhieuXuatAsync(string searchKeyword = "")
        {
            try
            {
                IsLoading = true;
                DanhSachPhieuXuat.Clear();

                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.DisplayAlert("Lỗi", "Không thể kết nối database", "OK");
                    }
                    return;
                }

                // Đếm tổng số bản ghi
                string countQuery = @"
                    SELECT COUNT(*) 
                    FROM phx_phieu_xuat
                    JOIN phx_chi_tiet_phieu_xuat ON phx_phieu_xuat.PHIEU_XUAT_ID = phx_chi_tiet_phieu_xuat.PHIEU_XUAT_ID
                    JOIN danh_muc_hang_hoa ON danh_muc_hang_hoa.HANGHOAID = phx_chi_tiet_phieu_xuat.HANGHOAID";

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    countQuery += " WHERE phx_phieu_xuat.PHIEU_XUAT_MA LIKE @Search";
                }

                using var countCmd = new MySqlCommand(countQuery, conn);
                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    countCmd.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                }

                var totalRecords = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                _totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

                // Log database query
                string query = @"
            SELECT 
                phx_chi_tiet_phieu_xuat.PHIEU_XUAT_ID, 
                phx_phieu_xuat.PHIEU_XUAT_MA, 
                danh_muc_hang_hoa.HANGHOAMA, 
                phx_chi_tiet_phieu_xuat.HANG_HOA_TEN, 
                phx_chi_tiet_phieu_xuat.DON_GIA, 
                phx_phieu_xuat.NGAY_XUAT, 
                phx_chi_tiet_phieu_xuat.THANH_TIEN, 
                danh_muc_hang_hoa.DON_GIA_GOC, 
                danh_muc_hang_hoa.CAN_TONG, 
                danh_muc_hang_hoa.TL_HOT, 
                (danh_muc_hang_hoa.CAN_TONG - danh_muc_hang_hoa.TL_HOT) AS 'TRU_HOT', 
                danh_muc_hang_hoa.CONG_GOC, 
                phx_chi_tiet_phieu_xuat.LOAIVANG 
            FROM 
                phx_phieu_xuat
            JOIN phx_chi_tiet_phieu_xuat ON phx_phieu_xuat.PHIEU_XUAT_ID = phx_chi_tiet_phieu_xuat.PHIEU_XUAT_ID
            JOIN danh_muc_hang_hoa ON danh_muc_hang_hoa.HANGHOAID = phx_chi_tiet_phieu_xuat.HANGHOAID";

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    query += " WHERE phx_phieu_xuat.PHIEU_XUAT_MA LIKE @Search";
                }

                query += " ORDER BY phx_phieu_xuat.PHIEU_XUAT_ID DESC LIMIT @PageSize OFFSET @Offset";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PageSize", PageSize);
                cmd.Parameters.AddWithValue("@Offset", (CurrentPage - 1) * PageSize);

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<PhieuXuatModel>();

                while (await reader.ReadAsync())
                {
                    list.Add(new PhieuXuatModel
                    {
                        PhieuXuatId = reader.GetInt32("PHIEU_XUAT_ID"),
                        PhieuXuatMa = reader.GetString("PHIEU_XUAT_MA"),
                        HangHoaMa = reader.GetString("HANGHOAMA"),
                        HangHoaTen = reader.GetString("HANG_HOA_TEN"),
                        DonGia = reader.GetDecimal("DON_GIA"),
                        NgayXuat = reader.GetDateTime("NGAY_XUAT"),
                        ThanhTien = reader.GetDecimal("THANH_TIEN"),
                        DonGiaGoc = reader.GetDecimal("DON_GIA_GOC"),
                        CanTong = reader.GetDecimal("CAN_TONG"),
                        TlHot = reader.GetDecimal("TL_HOT"),
                        TruHot = reader.GetDecimal("TRU_HOT"),
                        CongGoc = reader.GetDecimal("CONG_GOC"),
                        LoaiVang = reader.GetString("LOAIVANG")
                    });
                }

                if (list.Count == 0)
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.DisplayAlert("Thông báo", "Không có phiếu xuất nào", "OK");
                    }
                }

                foreach (var item in list)
                {
                    DanhSachPhieuXuat.Add(item);
                }

                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", $"Lỗi khi tải phiếu xuất: {ex.Message}", "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
