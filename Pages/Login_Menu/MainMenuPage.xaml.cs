using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        public ObservableCollection<MenuItemModel> MenuItems { get; set; }
        public ICommand MenuTappedCommand { get; set; }

        public MainMenuPage()
        {
            InitializeComponent();
        }

        // Biến điều khiển hiển thị SubMenu
        bool isDanhMucVisible = false;
        bool isHeThongVisible = false;


        // Khi bấm Danh Mục
        private async void OnDanhMucTapped(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Danh Mục","", null,
                "💰 Loại Vàng",
                "🏷️ Nhóm Vàng",
                "📦 Hàng Hóa",
                "🏬 Kho",
                "🏭 Nhà Cung Cấp",
                "👥 Khách Hàng",
                "⚖️ Đơn Vị");

            switch (action)
            {
                case "💰 Loại Vàng":
                    await Shell.Current.GoToAsync("LoaiVangPage");
                    break;
                case "🏷️ Nhóm Vàng":
                    await Shell.Current.GoToAsync("NhomVangPage");
                    break;
                case "📦 Hàng Hóa":
                    await Shell.Current.GoToAsync("HangHoaPage");
                    break;
                case "🏬 Kho":
                    await Shell.Current.GoToAsync("KhoPage");
                    break;
                case "🏭 Nhà Cung Cấp":
                    await Shell.Current.GoToAsync("NCCPage");
                    break;
                case "👥 Khách Hàng":
                    await Shell.Current.GoToAsync("KhachHangPage");
                    break;
                case "⚖️ Đơn Vị":
                    await Shell.Current.GoToAsync("DonViPage");
                    break;
            }
        }

        // Khi bấm Hệ Thống
        private async void OnHeThongTapped(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Hệ Thống", "", null,
               //Nhóm Người Dùng",
                "Người Dùng");
            //"Lãi Tính Khách",
            //"Thông Số Hệ Thống");

            switch (action)
            {
                case "Nhóm Người Dùng":
                    await Shell.Current.GoToAsync("NhomNguoiDungPage");
                    break;
                case "Người Dùng":
                    await Shell.Current.GoToAsync("NguoiDungPage");
                    break;
                case "Lãi Tính Khách":
                    await Shell.Current.GoToAsync("LaiTinhKhachPage");
                    break;
                case "Thông Số Hệ Thống":
                    await Shell.Current.GoToAsync("ThongSoPage");
                    break;
            }
        }

        // Khi bấm Giao Dịch
        private async void OnGiaoDichTapped(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Giao Dịch", "", null,
                "Bán Vàng",
                "Cầm Vàng");
            //Đổi Vàng",
            //Chuộc và Đóng Lãi");

            switch (action)
            {
                case "Bán Vàng":
                    await Shell.Current.GoToAsync("BanVangPage");
                    break;
                case "Cầm Vàng":
                    await Shell.Current.GoToAsync("CamVangPage");
                    break;
                case "Đổi Vàng":
                    await Shell.Current.GoToAsync("DoiVangPage");
                    break;
                case "Chuộc Vàng và Đóng Lãi":
                    await Shell.Current.GoToAsync("Chuoc&DongLaiPage");
                    break;
            }
        }

        // Khi bấm Cầm Vàng
        private async void OnCamVangTapped(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Cầm Vàng", "", null,
                "Phiếu Đang Cầm",
              //"Đã Thanh Toán",
                "Phiếu Đóng Lãi",
                "Quá Hạn Cần Thanh Toán",
                "Kho Vàng Cầm");
            //Phiếu Đã Thanh Toán - Ngày Chuộc");

            switch (action)
            {
                case "Phiếu Đang Cầm":
                    await Shell.Current.GoToAsync("PhieuDangCamPage");
                    break;
                case "Đã Thanh Toán":
                    await Shell.Current.GoToAsync("DaThanhToanPage");
                    break;
                case "Phiếu Đóng Lãi":
                    await Shell.Current.GoToAsync("PhieuDongLaiPage");
                    break;
                case "Quá Hạn Cần Thanh Toán":
                    await Shell.Current.GoToAsync("PhieuQuaHanPage");
                    break;
                case "Kho Vàng Cầm":
                    await Shell.Current.GoToAsync("KhoVangCamPage");
                    break;
                case "Phiếu Đã Thanh Toán - Ngày Chuộc":
                    await Shell.Current.GoToAsync("PhieuDaThanhToanNgayChuocPage");
                    break;
            }
        }

        // Khi bấm Báo Cáo
        private async void OnBaoCaoTapped(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Báo Cáo", "", null,
                "Phiếu Xuất",
                "Tồn Kho Loại Vàng",
                "Tồn Kho Vàng",
                "Tồn Kho Nhóm Vàng",
               //Kho Vàng Mua Vào",
               //Phiếu Mua Vào",
                "Phiếu Đổi");
            //In Phiếu Xuất");

            switch (action)
            {
                case "Phiếu Xuất":
                    await Shell.Current.GoToAsync("PhieuXuatPage");
                    break;
                case "Tồn Kho Loại Vàng":
                    await Shell.Current.GoToAsync("TonKhoLoaiVangPage");
                    break;
                case "Tồn Kho Vàng":
                    await Shell.Current.GoToAsync("TonKhoVangPage");
                    break;
                case "Tồn Kho Nhóm Vàng":
                    await Shell.Current.GoToAsync("TonKhoNhomVangPage");
                    break;
                /*case "Kho Vàng Mua Vào":
                    await Shell.Current.GoToAsync("KhoVangMuaVaoPage");
                    break;
                case "Phiếu Mua Vào":
                    await Shell.Current.GoToAsync("PhieuMuaVaoPage");
                    break;*/
                case "Phiếu Đổi":
                    await Shell.Current.GoToAsync("PhieuDoiPage");
                    break;
                /*case "In Phiếu Xuất":
                    await Shell.Current.GoToAsync("InPhieuXuatPage");
                    break;*/
            }
        }

        // Xử lý sự kiện khi bấm nút Bán Vàng
        private async void OnBanVangButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("BanVangPage");
        }

        // Xử lý sự kiện khi bấm nút Cầm Vàng
        private async void OnCamVangButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("CamVangPage");
        }

        /// <summary>
        /// Xử lý submenu khi bấm vào các phần tử con
        /// </summary>
        private async void OnSubMenuTapped(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            string pageName = btn.CommandParameter as string;

            if (!string.IsNullOrEmpty(pageName))
            {
                await Shell.Current.GoToAsync($"//{pageName}");
            }
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Đăng Xuất", "Bạn có chắc muốn đăng xuất?", "Đồng ý", "Hủy");
            if (confirm)
            {
                await DisplayAlert("Thông báo", "Đăng xuất thành công!", "OK");
                // TODO: Chuyển về màn hình đăng nhập
                 await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        public class MenuItemModel
        {
            public string Title { get; set; }
            public string Icon { get; set; }
            public string TargetPage { get; set; }
        }
    }
}
