using MyLoginApp.ViewModels.BaoCao;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Pages.BaoCao;

public partial class PhieuDoiPage : ContentPage
{
    private readonly PhieuDoiViewModel viewModel;

    public PhieuDoiPage()
    {
        InitializeComponent();
        viewModel = new PhieuDoiViewModel();
        BindingContext = viewModel;

        // Load dữ liệu ban đầu
        _ = viewModel.LoadPhieuDoiAsync();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        viewModel.SearchKeyword = e.NewTextValue;
    }
}

