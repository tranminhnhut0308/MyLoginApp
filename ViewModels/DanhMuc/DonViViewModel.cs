using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySqlConnector;
using MyLoginApp.Helpers;
using MyLoginApp.Models;

namespace MyLoginApp.ViewModels
{
    public partial class DonViViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<DonVi> danhSachDonVi = new();

        [ObservableProperty]
        private DonVi selectedDonVi;

        [ObservableProperty]
        private DonVi formDonVi = new();

        [ObservableProperty]
        private bool isAdding;

        [ObservableProperty]
        private bool isEditing;

        public DonViViewModel()
        {
            _ = LoadDonViAsync();
        }

        private async Task LoadDonViAsync()
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "SELECT DON_VI_MA, DON_VI_TEN, DIA_CHI_HD, DIEN_THOAI FROM ns_don_vi";

                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                DanhSachDonVi.Clear();
                while (await reader.ReadAsync())
                {
                    var donVi = new DonVi
                    {
                        MaDV = reader.GetString(reader.GetOrdinal("DON_VI_MA")),
                        TenDV = reader.GetString(reader.GetOrdinal("DON_VI_TEN")),
                        DiaChi = reader.GetString(reader.GetOrdinal("DIA_CHI_HD")),
                        DienThoai = reader.GetString(reader.GetOrdinal("DIEN_THOAI")),
                    };

                    DanhSachDonVi.Add(donVi);
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
            FormDonVi = new DonVi();
            IsAdding = true;
        }

        [RelayCommand]
        private void ShowEditForm()
        {
            if (SelectedDonVi == null) return;
            FormDonVi = new DonVi
            {
                MaDV = SelectedDonVi.MaDV,
                TenDV = SelectedDonVi.TenDV,
                DiaChi = SelectedDonVi.DiaChi,
                DienThoai = SelectedDonVi.DienThoai,
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

                string query = "INSERT INTO phx_don_vi (DV_MA, DV_TEN, DIA_CHI, DIEN_THOAI) VALUES (@MaDV, @TenDV, @DiaChi, @DienThoai)";
                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaDV", FormDonVi.MaDV);
                cmd.Parameters.AddWithValue("@TenDV", FormDonVi.TenDV);
                cmd.Parameters.AddWithValue("@DiaChi", FormDonVi.DiaChi);
                cmd.Parameters.AddWithValue("@DienThoai", FormDonVi.DienThoai);

                await cmd.ExecuteNonQueryAsync();
                IsAdding = false;
                await LoadDonViAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task SaveEditAsync()
        {
            if (SelectedDonVi == null) return;

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "UPDATE phx_don_vi SET DV_TEN = @TenDV, DIA_CHI = @DiaChi, DIEN_THOAI = @DienThoai WHERE DV_MA = @MaDV";
                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TenDV", FormDonVi.TenDV);
                cmd.Parameters.AddWithValue("@DiaChi", FormDonVi.DiaChi);
                cmd.Parameters.AddWithValue("@DienThoai", FormDonVi.DienThoai);
                cmd.Parameters.AddWithValue("@MaDV", FormDonVi.MaDV);

                await cmd.ExecuteNonQueryAsync();
                IsEditing = false;
                await LoadDonViAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (SelectedDonVi == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", "Bạn có chắc muốn xóa đơn vị này?", "Có", "Không");
            if (!confirm) return;

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "DELETE FROM phx_don_vi WHERE DV_MA = @MaDV";
                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaDV", SelectedDonVi.MaDV);

                await cmd.ExecuteNonQueryAsync();
                await LoadDonViAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }
    }
}
