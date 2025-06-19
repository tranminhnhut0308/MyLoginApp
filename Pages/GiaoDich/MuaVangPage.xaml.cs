using Microsoft.Maui.Controls;
using MyLoginApp.ViewModels;

namespace MyLoginApp.Pages
{
    public partial class MuaVangPage : ContentPage
    {
        public MuaVangPage()
        {
            InitializeComponent();
            BindingContext = new MuaVangViewModel(); // Gán ViewModel cho BindingContext
        }

        // Xử lý sự kiện khi nhấn nút Mua Vàng
        private async void OnMuaVangClicked(object sender, EventArgs e)
        {
            await ((MuaVangViewModel)BindingContext).OnMuaVangClickedAsync();
        }
    }
}
