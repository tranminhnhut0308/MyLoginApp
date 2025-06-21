using MyLoginApp.ViewModels.BaoCao;

namespace MyLoginApp.Pages.BaoCao;

public partial class TonKhoLoaiVangPage : ContentPage
{
	public TonKhoLoaiVangPage()
	{
		InitializeComponent();
	}

	private async void OnTimKiemClicked(object sender, EventArgs e)
	{
		var viewModel = BindingContext as TonKhoLoaiVangViewModel;
		if (viewModel == null) return;

		string tuKhoa = await DisplayPromptAsync("Tìm kiếm", "Nhập từ khóa:", "OK", "Hủy", viewModel.TuKhoaTimKiem);
		if (tuKhoa != null) 
		{
			viewModel.TuKhoaTimKiem = tuKhoa;
			if (viewModel.ThucHienTimKiemCommand.CanExecute(null))
			{
				viewModel.ThucHienTimKiemCommand.Execute(null);
			}
		}
	}
}