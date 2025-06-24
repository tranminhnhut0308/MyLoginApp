using MyLoginApp.ViewModels;

namespace MyLoginApp.Pages.BaoCao;

public partial class TonKhoNhomVangPage : ContentPage
{
    private TonKhoNhomVangViewModel _viewModel;

    public TonKhoNhomVangPage()
    {
        InitializeComponent();
        _viewModel = new TonKhoNhomVangViewModel();
        BindingContext = _viewModel;
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

    private async void OnTimKiemClicked(object sender, EventArgs e)
    {
        string tuKhoa = await DisplayPromptAsync(
            "Tìm kiếm", 
            "Nhập mã loại, tên loại, mã hàng hoặc tên hàng...",
            "OK",
            "Hủy",
            initialValue: _viewModel.TuKhoaTimKiem);

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            _viewModel.TuKhoaTimKiem = tuKhoa;
            _viewModel.ThucHienTimKiemCommand.Execute(null);
        }
    }

    private void OnLoaiVangSelectedIndexChanged(object sender, EventArgs e)
    {
        if (_viewModel != null && sender is Picker picker)
        {
            string selectedValue = picker.SelectedItem?.ToString() ?? string.Empty;
            _viewModel.ApplyFilterCommand.Execute(selectedValue);
        }
    }
}