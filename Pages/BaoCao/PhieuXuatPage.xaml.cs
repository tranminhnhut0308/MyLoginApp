using MyLoginApp.ViewModels.BaoCao;

namespace MyLoginApp.Pages.BaoCao;

public partial class PhieuXuatPage : ContentPage
{
    private PhieuXuatViewModel _viewModel;

    public PhieuXuatPage()
    {
        InitializeComponent();
        _viewModel = new PhieuXuatViewModel();
        BindingContext = _viewModel;

        // Debug thông tin ViewModel
        Console.WriteLine(_viewModel);
    }

    private async void OnTimKiemClicked(object sender, EventArgs e)
    {
        string keyword = await DisplayPromptAsync("Tìm kiếm", "Nhập mã phiếu hoặc tên hàng hóa:", "Tìm", "Hủy");
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            _viewModel.SearchKeyword = keyword;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Console.WriteLine("OnAppearing called");  // Debug log
        if (_viewModel != null)
        {
            await _viewModel.LoadPhieuXuatAsync();
        }
    }

}
