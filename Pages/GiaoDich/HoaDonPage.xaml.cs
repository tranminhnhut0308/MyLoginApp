using Microsoft.Maui.Controls;
using MyLoginApp.Models;
using MyLoginApp.Pages;
using MyLoginApp.Models.DanhMuc;
using System.Text;

namespace MyLoginApp.Views;

public partial class HoaDonPage : ContentPage
{
    public HoaDonPage(KhachHang khach, List<ScannedItem> danhSachHangHoa, decimal tongTien)
    {
        InitializeComponent();

        StringBuilder bill = new StringBuilder();

        // Thông tin khách hàng
        bill.AppendLine($"👤 Khách hàng: {khach.TenKH}");
        bill.AppendLine($"📞 Điện thoại: {khach.SoDienThoai}");
        bill.AppendLine();
        
        // Danh sách các mặt hàng
        bill.AppendLine("=== DANH SÁCH MẶT HÀNG ===");
        int stt = 1;
        foreach (var item in danhSachHangHoa)
        {
            bill.AppendLine($"{stt}. {item.Name} - {item.GoldType}");
            bill.AppendLine($"   Mã: {item.Id}");
            bill.AppendLine($"   Trọng lượng: {item.Weight:N3}g");
            bill.AppendLine($"   Đơn giá: {item.Price:N0}đ");
            bill.AppendLine($"   Thành tiền: {item.Total:N0}đ");
            bill.AppendLine("-------------------------");
            stt++;
        }
        
        // Tổng tiền
        bill.AppendLine($"\n💵 TỔNG THANH TOÁN: {tongTien:N0}đ");

        lblNoiDungHoaDon.Text = bill.ToString();
    }

    // Giữ lại constructor cũ để đảm bảo khả năng tương thích ngược
    public HoaDonPage(KhachHang khach, HangHoaModel hang, decimal tongTien)
    {
        InitializeComponent();

        string bill = $"👤 Khách hàng: {khach.TenKH}\n" +
                      $"📞 Điện thoại: {khach.SoDienThoai}\n" +
                      
                      $"📦 Mã vàng: {hang.HangHoaID}\n" +
                      $"🔖 Tên hàng: {hang.TenHangHoa}\n" +
                      $"🔸 Loại vàng: {hang.LoaiVang} | Nhóm: {hang.Nhom}\n" +
                      $"⚖️ Cân tổng: {hang.CanTong}g (Hột: {hang.TrongLuongHot}g | Trừ: {hang.TruHot}g)\n" +
                      $"💰 Giá công: {hang.GiaCong:N0}đ\n" +
                      $"🏷️ Đơn giá: {hang.DonViGoc:N0}đ\n" +
                      $"🧮 Tổng tiền: {tongTien:N0}đ";

        lblNoiDungHoaDon.Text = bill;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        // Kiểm tra nếu trang hiện tại là HoaDonPage và cần quay về trang BanVangPage
        var pages = Navigation.NavigationStack.ToList();
        var previousPage = pages.FirstOrDefault(p => p is BanVangPage);

        if (previousPage != null)
        {
            await Navigation.PopToRootAsync(); // Quay về trang BanVangPage nếu có trong Navigation Stack
        }
        else
        {
            await Navigation.PopAsync(); // Quay về trang trước đó
        }
    }
}
