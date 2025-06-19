using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySqlConnector;
using MyLoginApp.Helpers;
using MyLoginApp.Models;
using Microsoft.Maui;

namespace MyLoginApp.ViewModels
{
    public partial class NhaCungCapViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<NCC> danhSachNhaCungCap = new();

        [ObservableProperty]
        private NCC selectedNhaCungCap;

        [ObservableProperty]
        private NCC formNhaCungCap = new();

        [ObservableProperty]
        private bool isAdding;

        [ObservableProperty]
        private bool isEditing;

        public NhaCungCapViewModel()
        {
            _ = LoadNhaCungCapAsync();
        }

        private async Task LoadNhaCungCapAsync()
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "SELECT NCCMA, NCC_TEN, GHI_CHU FROM phn_nha_cung_cap";

                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                DanhSachNhaCungCap.Clear(); // Xóa danh sách trước khi thêm mới
                while (await reader.ReadAsync())
                {
                    var nhaCungCap = new NCC
                    {
                        MaNCC = reader.GetString(reader.GetOrdinal("NCCMA")),
                        TenNCC = reader.GetString(reader.GetOrdinal("NCC_TEN")),
                        KyHieu = reader.GetString(reader.GetOrdinal("GHI_CHU")),
                    };

                    DanhSachNhaCungCap.Add(nhaCungCap);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private void ShowAddForm()
        {
            FormNhaCungCap = new NCC();
            IsAdding = true;
        }

        [RelayCommand]
        private void ShowEditForm()
        {
            if (SelectedNhaCungCap == null) return;
            FormNhaCungCap = new NCC
            {
                MaNCC = SelectedNhaCungCap.MaNCC,
                TenNCC = SelectedNhaCungCap.TenNCC,
                KyHieu = SelectedNhaCungCap.KyHieu,
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveAddAsync()
        {
            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "INSERT INTO NhaCungCap (MaNCC, TenNCC, KyHieu) VALUES (@MaNCC, @TenNCC, @KyHieu)";
                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaNCC", FormNhaCungCap.MaNCC);
                cmd.Parameters.AddWithValue("@TenNCC", FormNhaCungCap.TenNCC);
                cmd.Parameters.AddWithValue("@KyHieu", FormNhaCungCap.KyHieu);

                await cmd.ExecuteNonQueryAsync();
                IsAdding = false;
                await LoadNhaCungCapAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task SaveEditAsync()
        {
            if (SelectedNhaCungCap == null) return;

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "UPDATE NhaCungCap SET TenNCC = @TenNCC, KyHieu = @KyHieu WHERE MaNCC = @MaNCC";
                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TenNCC", FormNhaCungCap.TenNCC);
                cmd.Parameters.AddWithValue("@KyHieu", FormNhaCungCap.KyHieu);
                cmd.Parameters.AddWithValue("@MaNCC", FormNhaCungCap.MaNCC);

                await cmd.ExecuteNonQueryAsync();
                IsEditing = false;
                await LoadNhaCungCapAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (SelectedNhaCungCap == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", "Bạn có chắc muốn xóa nhà cung cấp này?", "Có", "Không");
            if (!confirm) return;

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "DELETE FROM NhaCungCap WHERE MaNCC = @MaNCC";
                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaNCC", SelectedNhaCungCap.MaNCC);

                await cmd.ExecuteNonQueryAsync();
                await LoadNhaCungCapAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }
    }
}