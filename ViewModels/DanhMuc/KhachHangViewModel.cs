using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MyLoginApp.Helpers;
using MyLoginApp.Models;
using MyLoginApp.Models.DanhMuc;
using MySqlConnector;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyLoginApp.ViewModels
{
    public partial class KhachHangViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<KhachHang> danhSachKhachHang = new();

        [ObservableProperty]
        private KhachHang selectedKhachHang;

        [ObservableProperty]
        private KhachHang formKhachHang = new();

        [ObservableProperty]
        private bool isEditing;

        public KhachHangViewModel()
        {
            _ = LoadKhachHangAsync();
        }

        private async Task LoadKhachHangAsync()
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "SELECT KH_MA, KH_TEN, DIEN_THOAI FROM phx_khach_hang LIMIT 10";

                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                DanhSachKhachHang.Clear();
                int count = 0;

                while (await reader.ReadAsync())
                {
                    var khachHang = new KhachHang
                    {
                        MaKH = reader.GetString(reader.GetOrdinal("KH_MA")),
                        TenKH = reader.GetString(reader.GetOrdinal("KH_TEN")),
                        SoDienThoai = reader.GetString(reader.GetOrdinal("DIEN_THOAI")),
                    };

                    DanhSachKhachHang.Add(khachHang);
                    count++;
                }

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }


        [RelayCommand]
        private void ShowEditForm()
        {
            if (SelectedKhachHang == null) return;
            FormKhachHang = new KhachHang
            {
                MaKH = SelectedKhachHang.MaKH,
                TenKH = SelectedKhachHang.TenKH,
                SoDienThoai = SelectedKhachHang.SoDienThoai
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveEditAsync()
        {
            if (SelectedKhachHang == null || FormKhachHang == null) return;

            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "UPDATE phx_khach_hang SET KH_TEN = @TenKH, DIEN_THOAI = @SoDienThoai WHERE KH_MA = @MaKH"; // Sửa đúng tên cột

                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TenKH", FormKhachHang.TenKH);
                cmd.Parameters.AddWithValue("@SoDienThoai", FormKhachHang.SoDienThoai);
                cmd.Parameters.AddWithValue("@MaKH", FormKhachHang.MaKH);

                await cmd.ExecuteNonQueryAsync();
                IsEditing = false;
                await LoadKhachHangAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }
    }
}
