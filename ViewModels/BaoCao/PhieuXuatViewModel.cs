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

        private int _tongSoPhieu;
        public int TongSoPhieu
        {
            get => _tongSoPhieu;
            set => SetProperty(ref _tongSoPhieu, value);
        }

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

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        private decimal _tongCanTongAll;
        public decimal TongCanTongAll { get => _tongCanTongAll; set => SetProperty(ref _tongCanTongAll, value); }
        private decimal _tongTLHotAll;
        public decimal TongTLHotAll { get => _tongTLHotAll; set => SetProperty(ref _tongTLHotAll, value); }
        private decimal _tongTruHotAll;
        public decimal TongTruHotAll { get => _tongTruHotAll; set => SetProperty(ref _tongTruHotAll, value); }
        private decimal _tongThanhTienAll;
        public decimal TongThanhTienAll { get => _tongThanhTienAll; set => SetProperty(ref _tongThanhTienAll, value); }
        private decimal _tongGiaGocAll;
        public decimal TongGiaGocAll { get => _tongGiaGocAll; set => SetProperty(ref _tongGiaGocAll, value); }
        private decimal _tongLaiLoAll;
        public decimal TongLaiLoAll { get => _tongLaiLoAll; set => SetProperty(ref _tongLaiLoAll, value); }

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
                IsRefreshing = true;
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

                // 1. Load toàn bộ danh sách phiếu xuất (không phân trang)
                var allPhieuXuat = new List<PhieuXuatModel>();
                string queryAll = @"
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
                    queryAll += " WHERE phx_phieu_xuat.PHIEU_XUAT_MA LIKE @Search";
                }
                using (var cmdAll = new MySqlCommand(queryAll, conn))
                {
                    if (!string.IsNullOrEmpty(searchKeyword))
                    {
                        cmdAll.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                    }
                    using (var readerAll = await cmdAll.ExecuteReaderAsync())
                    {
                        while (await readerAll.ReadAsync())
                        {
                            var canTong = readerAll.GetDecimal("CAN_TONG");
                            var tlHot = readerAll.GetDecimal("TL_HOT");
                            var congGoc = readerAll.GetDecimal("CONG_GOC");
                            var donGiaGoc = readerAll.GetDecimal("DON_GIA_GOC");
                            var thanhTien = readerAll.GetDecimal("THANH_TIEN");
                            decimal tinhTruHot = canTong - tlHot;
                            string bien1 = ((int)tinhTruHot).ToString();
                            if (bien1.Length >= 1 && bien1.Length <= 4)
                            {
                                tinhTruHot = tinhTruHot / 100;
                            }
                            decimal giaGoc = donGiaGoc * tinhTruHot + congGoc;
                            decimal laiLo = thanhTien - giaGoc;
                            allPhieuXuat.Add(new PhieuXuatModel
                            {
                                PhieuXuatId = readerAll.GetInt32("PHIEU_XUAT_ID"),
                                PhieuXuatMa = readerAll.GetString("PHIEU_XUAT_MA"),
                                HangHoaMa = readerAll.GetString("HANGHOAMA"),
                                HangHoaTen = readerAll.GetString("HANG_HOA_TEN"),
                                DonGia = readerAll.GetDecimal("DON_GIA"),
                                NgayXuat = readerAll.GetDateTime("NGAY_XUAT"),
                                ThanhTien = thanhTien,
                                DonGiaGoc = donGiaGoc,
                                CanTong = canTong,
                                TlHot = tlHot,
                                TruHot = tinhTruHot,
                                CongGoc = congGoc,
                                LoaiVang = readerAll.GetString("LOAIVANG"),
                                GiaGoc = giaGoc,
                                LaiLo = laiLo
                            });
                        }
                    }
                }

                // 2. Tính tổng trên allPhieuXuat
                TongCanTongAll = allPhieuXuat.Sum(x => x.CanTong);
                TongTLHotAll = allPhieuXuat.Sum(x => x.TlHot);
                TongTruHotAll = allPhieuXuat.Sum(x => x.TruHot);
                TongThanhTienAll = allPhieuXuat.Sum(x => x.ThanhTien);
                TongGiaGocAll = allPhieuXuat.Sum(x => x.GiaGoc);
                TongLaiLoAll = allPhieuXuat.Sum(x => x.LaiLo);

                // 3. Hiển thị trang hiện tại
                DanhSachPhieuXuat.Clear();
                foreach (var item in allPhieuXuat.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
                {
                    DanhSachPhieuXuat.Add(item);
                }

                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));

                TongSoPhieu = allPhieuXuat.Count;
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
                IsRefreshing = false;
            }
        }

        public decimal TongCanTong => DanhSachPhieuXuat.Sum(x => x.CanTong);
        public decimal TongTLHot => DanhSachPhieuXuat.Sum(x => x.TlHot);
        public decimal TongTruHot => DanhSachPhieuXuat.Sum(x => x.TruHot);
        public decimal TongThanhTien => DanhSachPhieuXuat.Sum(x => x.ThanhTien);
        public decimal TongGiaGoc => DanhSachPhieuXuat.Sum(x => x.GiaGoc);
        public decimal TongLaiLo => DanhSachPhieuXuat.Sum(x => x.LaiLo);
    }
}
