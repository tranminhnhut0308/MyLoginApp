using MyLoginApp.ViewModels;
using MyLoginApp.ViewModels.BaoCao;

namespace MyLoginApp.Pages.BaoCao;

public partial class KhoVangMuaVaoPage : ContentPage
{
	private KhoVangMuaVaoViewModel _viewModel;

	public KhoVangMuaVaoPage()
	{
		InitializeComponent();
		_viewModel = BindingContext as KhoVangMuaVaoViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		
		// Gọi lệnh tải dữ liệu khi trang hiển thị
		if (_viewModel != null)
		{
			_viewModel.LoadDanhSachKhoVangCommand.Execute(null);
		}
	}
}