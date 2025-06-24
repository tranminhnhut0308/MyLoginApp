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

    private async void OnTimKiemClicked(object sender, EventArgs e)
    {
        string tuKhoa = await DisplayPromptAsync("Tìm kiếm", "Nhập từ khóa nhóm vàng:", "OK", "Hủy", viewModel.TuKhoaTimKiem);
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            viewModel.TuKhoaTimKiem = tuKhoa;
            if (viewModel.ThucHienTimKiemCommand.CanExecute(null))
            {
                viewModel.ThucHienTimKiemCommand.Execute(null);
            }
        }
    }
}