using Microsoft.Maui.Controls;
using MyLoginApp.ViewModels;

namespace MyLoginApp.Pages
{
    public partial class PhieuQuaHanPage : ContentPage
    {
        private PhieuQuaHanViewModel viewModel;

        public PhieuQuaHanPage()
        {
            InitializeComponent();
            viewModel = new PhieuQuaHanViewModel();
            BindingContext = viewModel;

            // Gọi load dữ liệu khi trang được tạo
            Loaded += async (s, e) => await viewModel.LoadPhieuQuaHanAsync();
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.SearchKeyword = e.NewTextValue;
                await viewModel.LoadPhieuQuaHanAsync();  // Make sure this matches the method in ViewModel
            }
        }

    }
}
