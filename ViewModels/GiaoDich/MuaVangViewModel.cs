using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyLoginApp.Helpers;
using MySqlConnector;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MyLoginApp.ViewModels
{
    public partial class MuaVangViewModel : ObservableObject
    {
        // Các thuộc tính để bind với UI
        [ObservableProperty] private string tenKhach, tenHang, selectedLoaiVang, ghiChu;
        [ObservableProperty] private double canTong, tlHot, truHot, donGia, thanhTien, tienMua;
        [ObservableProperty] private DateTime ngayMua = DateTime.Now;

        // Danh sách loại vàng
        public ObservableCollection<string> LoaiVangList { get; } = new ObservableCollection<string>();

        // Tải danh sách loại vàng từ DB
        private async Task LoadLoaiVangAsync()
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn != null)
                {
                    using var cmd = new MySqlCommand("SELECT NHOM_TEN FROM nhom_hang WHERE SU_DUNG = 1", conn);
                    using var reader = await cmd.ExecuteReaderAsync();
                    LoaiVangList.Clear();
                    while (await reader.ReadAsync()) LoaiVangList.Add(reader["NHOM_TEN"].ToString());
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không tải được loại vàng: {ex.Message}", "OK");
            }
        }

        // Tính thành tiền
      

        private void TinhThanhTien()
        {
            
        }

        // Command để xử lý sự kiện thanh toán
        [RelayCommand]
        public async Task OnMuaVangClickedAsync()
        {
            if (string.IsNullOrEmpty(TenKhach))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập tên khách hàng!", "OK");
                return;
            }

            if (CanTong <= 0 || DonGia <= 0)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin về cân nặng và đơn giá!", "OK");
                return;
            }

            await Shell.Current.DisplayAlert("Mua Vàng", $"Khách: {TenKhach}\nThành tiền: {ThanhTien:N0}đ", "OK");

            // Có thể thêm logic lưu thông tin vào cơ sở dữ liệu ở đây
        }

        // Constructor để gọi LoadLoaiVangAsync
        public MuaVangViewModel()
        {
            // Tải loại vàng khi ViewModel được khởi tạo
            LoadLoaiVangAsync();
        }
    }
}
