using MyLoginApp.Pages.BaoCao;
using MyLoginApp.Pages.Controls;
namespace MyLoginApp;


public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Đăng ký các route để dùng Shell điều hướng
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(MainMenuPage), typeof(MainMenuPage));

        // Đăng ký các route Danh Mục
        Routing.RegisterRoute(nameof(LoaiVangPage), typeof(LoaiVangPage));
        Routing.RegisterRoute(nameof(NhomVangPage), typeof(NhomVangPage)); 
        Routing.RegisterRoute(nameof(HangHoaPage), typeof(HangHoaPage));
        Routing.RegisterRoute(nameof(KhoPage), typeof(KhoPage));
        Routing.RegisterRoute(nameof(NCCPage), typeof(NCCPage));
        Routing.RegisterRoute(nameof(KhachHangPage), typeof(KhachHangPage));
        Routing.RegisterRoute(nameof(DonViPage), typeof(DonViPage));

        // Đăng ký các route Giao dịch
        Routing.RegisterRoute(nameof(BanVangPage), typeof(BanVangPage));
        Routing.RegisterRoute(nameof(CamVangPage), typeof(CamVangPage));
        Routing.RegisterRoute(nameof(MuaVangPage), typeof(MuaVangPage));

        // Đăng ký các route Hệ thống
        Routing.RegisterRoute(nameof(NguoiDungPage), typeof(NguoiDungPage));
        Routing.RegisterRoute(nameof(LaiTinhKhachPage), typeof(LaiTinhKhachPage));

        // Đăng ký các route Cầm vàng
        Routing.RegisterRoute(nameof(PhieuDangCamPage), typeof(PhieuDangCamPage));
        Routing.RegisterRoute(nameof(PhieuDongLaiPage), typeof(PhieuDongLaiPage));
        Routing.RegisterRoute(nameof(PhieuQuaHanPage), typeof(PhieuQuaHanPage));
        Routing.RegisterRoute(nameof(KhoVangCamPage), typeof(KhoVangCamPage));

        // Đăng ký các route Báo Cáo
        Routing.RegisterRoute(nameof(PhieuXuatPage), typeof(PhieuXuatPage));
        Routing.RegisterRoute(nameof(TonKhoLoaiVangPage), typeof(TonKhoLoaiVangPage));
        Routing.RegisterRoute(nameof(TonKhoVangPage), typeof(TonKhoVangPage)); 
        Routing.RegisterRoute(nameof(TonKhoNhomVangPage), typeof(TonKhoNhomVangPage));
        Routing.RegisterRoute(nameof(PhieuDoiPage), typeof(PhieuDoiPage));
        
    }
}
