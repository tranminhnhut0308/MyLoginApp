using MyLoginApp.ViewModels.BaoCao;

namespace MyLoginApp.Pages.BaoCao;

public partial class TonKhoVangPage : ContentPage
{
    private readonly TonKhoVangViewModel viewModel;
    public TonKhoVangPage()
	{
		InitializeComponent();
        viewModel = new TonKhoVangViewModel();
        BindingContext = viewModel;

        // Load dữ liệu ban đầu
        _ = viewModel.LoadDanhSachTonKhoVang();
    }
}