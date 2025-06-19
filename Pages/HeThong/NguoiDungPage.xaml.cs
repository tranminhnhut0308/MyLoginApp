using MyLoginApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Pages;

public partial class NguoiDungPage : ContentPage
{
    public NguoiDungPage()
    {
        InitializeComponent();
        Loaded += async (s, e) => await ((NguoiDungViewModel)BindingContext).LoadNguoiDungAsync();
    }
    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var vm = BindingContext as NguoiDungViewModel;
        if (vm != null)
        {
            vm.CurrentPage = 1; // Reset về trang đầu khi tìm kiếm mới
            await vm.LoadNguoiDungAsync(e.NewTextValue);
        }
    }


}
