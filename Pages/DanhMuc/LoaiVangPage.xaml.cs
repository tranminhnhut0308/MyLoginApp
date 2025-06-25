using MyLoginApp.ViewModels;
using System.Diagnostics;

namespace MyLoginApp.Pages
{
    public partial class LoaiVangPage : ContentPage
    {
        private LoaiVangViewModel _viewModel;

        public LoaiVangPage()
        {
            try
            {
                InitializeComponent();
                _viewModel = new LoaiVangViewModel();
                BindingContext = _viewModel; // Đúng chuẩn MVVM
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khởi tạo LoaiVangPage: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () => {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể khởi tạo trang Loại Vàng", "OK");
                });
            }
        }

        // Xử lý sự kiện TextChanged cho các trường nhập liệu
        private void OnDonGiaVonTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_viewModel != null)
                {
                    _viewModel.HandleDonGiaVonTextChanged(e.NewTextValue);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi xử lý sự kiện OnDonGiaVonTextChanged: {ex.Message}");
            }
        }

        private void OnDonGiaMuaTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_viewModel != null)
                {
                    _viewModel.HandleDonGiaMuaTextChanged(e.NewTextValue);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi xử lý sự kiện OnDonGiaMuaTextChanged: {ex.Message}");
            }
        }

        private void OnDonGiaBanTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_viewModel != null)
                {
                    _viewModel.HandleDonGiaBanTextChanged(e.NewTextValue);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi xử lý sự kiện OnDonGiaBanTextChanged: {ex.Message}");
            }
        }

        private void OnDonGiaCamTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_viewModel != null)
                {
                    _viewModel.HandleDonGiaCamTextChanged(e.NewTextValue);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi xử lý sự kiện OnDonGiaCamTextChanged: {ex.Message}");
            }
        }
    }
}
