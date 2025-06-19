using MyLoginApp.Helpers;
using Application = Microsoft.Maui.Controls.Application;

namespace MyLoginApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent(); // Load UI từ App.xaml

        DatabaseHelper.LoadSavedConnectionString(); // Thêm dòng này để tải chuỗi kết nối đã lưu

        MainPage = new AppShell(); // Nếu dùng Shell
    }
}
