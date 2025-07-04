using MyLoginApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Pages;

public partial class NguoiDungPage : ContentPage
{
    private NguoiDungViewModel _viewModel => BindingContext as NguoiDungViewModel;

    public NguoiDungPage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            await _viewModel.LoadNguoiDungAsync();
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.CurrentPage = 1; // Reset về trang đầu khi tìm kiếm mới
            _viewModel.SearchKeyword = e.NewTextValue; // Cập nhật SearchKeyword
            await _viewModel.LoadNguoiDungAsync(e.NewTextValue);
        }
    }

    private async void OnPreviousPageClicked(object sender, EventArgs e)
    {
        if (_viewModel != null && _viewModel.CurrentPage > 1)
        {
            _viewModel.CurrentPage--;
            await _viewModel.LoadNguoiDungAsync(_viewModel.SearchKeyword);
        }
    }

    private async void OnNextPageClicked(object sender, EventArgs e)
    {
        if (_viewModel != null && _viewModel.CanGoNext)
        {
            _viewModel.CurrentPage++;
            await _viewModel.LoadNguoiDungAsync(_viewModel.SearchKeyword);
        }
    }
}
