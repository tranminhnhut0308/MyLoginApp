using Microsoft.Maui.Controls;
using MyLoginApp.ViewModels;

namespace MyLoginApp.Pages;
public partial class PhieuDongLaiPage : ContentPage
{
    public PhieuDongLaiViewModel ViewModel { get; set; }

    public PhieuDongLaiPage()
    {
        InitializeComponent();
        ViewModel = new PhieuDongLaiViewModel();
        BindingContext = ViewModel;

        // Load dữ liệu ban đầu
        _ = ViewModel.LoadPhieuDongLaiAsync();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.SearchKeyword = e.NewTextValue;
    }
}
