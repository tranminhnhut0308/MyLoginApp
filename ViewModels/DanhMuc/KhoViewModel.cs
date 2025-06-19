using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MyLoginApp.Helpers;
using MyLoginApp.Models;
using MySqlConnector;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyLoginApp.ViewModels
{
    public partial class KhoViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Kho> danhSachKho = new();

        [ObservableProperty]
        private Kho selectedKho;

        [ObservableProperty]
        private Kho formKho = new();

        [ObservableProperty]
        private bool isEditing;

        public KhoViewModel()
        {
            _ = LoadKhoAsync();
        }

        private async Task LoadKhoAsync()
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "SELECT KHOMA, KHO_TEN FROM gd_kho ";

                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                DanhSachKho.Clear(); // Xóa trước khi thêm dữ liệu mới
                while (await reader.ReadAsync())
                {
                    var kho = new Kho
                    {
                        KhoMa = reader.GetString(reader.GetOrdinal("KHOMA")),
                        TenKho = reader.GetString(reader.GetOrdinal("KHO_TEN")),
                    };

                    DanhSachKho.Add(kho);
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
            if (SelectedKho == null) return;
            FormKho = new Kho
            {
                KhoMa = SelectedKho.KhoMa,
                TenKho = SelectedKho.TenKho,
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveEditAsync()
        {
            if (SelectedKho == null || FormKho == null) return;

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "UPDATE Kho SET TenKho = @TenKho WHERE KhoID = @KhoID";
                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TenKho", FormKho.TenKho);
                cmd.Parameters.AddWithValue("@KhoID", FormKho.KhoMa);

                await cmd.ExecuteNonQueryAsync();
                IsEditing = false;
                await LoadKhoAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }
    }
}