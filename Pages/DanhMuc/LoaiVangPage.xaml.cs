using MyLoginApp.ViewModels;
using System.Diagnostics;

namespace MyLoginApp.Pages
{
    public partial class LoaiVangPage : ContentPage
    {
        public LoaiVangPage()
        {
            try
            {
                InitializeComponent();
                BindingContext = new LoaiVangViewModel(); // Đúng chuẩn MVVM
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khởi tạo LoaiVangPage: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () => {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể khởi tạo trang Loại Vàng", "OK");
                });
            }
        }
    }
}
