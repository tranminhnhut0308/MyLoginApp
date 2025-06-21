using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyLoginApp.Models;
using MyLoginApp.Helpers;
using MySqlConnector;
using Microsoft.Maui.Controls;

namespace MyLoginApp.ViewModels
{
    public partial class KhoVangCamViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<KhoVangCamModel> danhSachKhoVangCam = new();

        [ObservableProperty]
        private string searchKeyword;

        [ObservableProperty]
        private bool isLoading;

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
        public bool CanGoNext => DanhSachKhoVangCam.Count >= PageSize;

        public ICommand LoadDataCommand => new AsyncRelayCommand(() => LoadKhoVangCamAsync(SearchKeyword));
        public ICommand GoNextPageCommand => new RelayCommand(NextPage);
        public ICommand GoPreviousPageCommand => new RelayCommand(PreviousPage);

        private void NextPage()
        {
            CurrentPage++;
            _ = LoadKhoVangCamAsync(SearchKeyword);
        }

        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                _ = LoadKhoVangCamAsync(SearchKeyword);
            }
        }

        partial void OnSearchKeywordChanged(string value)
        {
            CurrentPage = 1;
            _ = LoadKhoVangCamAsync(value);
        }

        public async Task LoadKhoVangCamAsync(string search = "")
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                await MainThread.InvokeOnMainThreadAsync(() => DanhSachKhoVangCam.Clear());

                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể kết nối database", "OK");
                    return;
                }

                string query = @"
                    SELECT *
                    FROM cam_kho_vang_cam
                    WHERE 1 = 1";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " AND (Ten_KH LIKE @Search OR PHIEU_MA LIKE @Search OR TEN_HANG_HOA LIKE @Search OR LOAI_VANG LIKE @Search)";
                }

                query += " ORDER BY KHO_ID DESC LIMIT @PageSize OFFSET @Offset";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PageSize", PageSize);
                cmd.Parameters.AddWithValue("@Offset", (CurrentPage - 1) * PageSize);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var item = new KhoVangCamModel
                    {
                        Ten_KH = reader["Ten_KH"].ToString(),
                        Ma_Phieu = reader["PHIEU_MA"].ToString(),
                        Ten_Hang = reader["TEN_HANG_HOA"].ToString(),
                        Loai_Vang = reader["LOAI_VANG"].ToString(),
                        Can_Tong = Convert.ToDecimal(reader["CAN_TONG"]),
                        TL_Hot = Convert.ToDecimal(reader["TL_HOT"]),
                        Don_Gia = Convert.ToDecimal(reader["DON_GIA"]),
                        Ngay_Cam = Convert.ToDateTime(reader["NGAY_CAM"]),
                        Ngay_QuaHan = reader["NGAY_QUA_HAN"] == DBNull.Value ? null : Convert.ToDateTime(reader["NGAY_QUA_HAN"]),
                        Ngay_ThanhLy = reader["NGAY_THANH_LY"] == DBNull.Value ? null : Convert.ToDateTime(reader["NGAY_THANH_LY"])
                    };

                    await MainThread.InvokeOnMainThreadAsync(() => DanhSachKhoVangCam.Add(item));
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    OnPropertyChanged(nameof(CanGoNext));
                    OnPropertyChanged(nameof(CanGoPrevious));
                });

                if (DanhSachKhoVangCam.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Thông báo", "Không có kết quả nào.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
