using MyLoginApp.ViewModels;

namespace MyLoginApp.Pages.BaoCao;

public partial class TonKhoNhomVangPage : ContentPage
{
    private TonKhoNhomVangViewModel _viewModel;

    public TonKhoNhomVangPage()
    {
        InitializeComponent();
        _viewModel = BindingContext as TonKhoNhomVangViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Gọi lệnh tải dữ liệu khi trang hiển thị
        if (_viewModel != null)
        {
            _viewModel.LoadDanhSachTonKhoCommand.Execute(null);
        }
    }
}