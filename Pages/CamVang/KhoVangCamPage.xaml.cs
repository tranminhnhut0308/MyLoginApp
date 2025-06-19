using MyLoginApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Pages;

public partial class KhoVangCamPage : ContentPage
{
    private KhoVangCamViewModel _viewModel;

    public KhoVangCamPage()
    {
        InitializeComponent();
        _viewModel = new KhoVangCamViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            await _viewModel.LoadKhoVangCamAsync();
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SearchKeyword = e.NewTextValue;
            await _viewModel.LoadKhoVangCamAsync(_viewModel.SearchKeyword);
        }
    }
}
