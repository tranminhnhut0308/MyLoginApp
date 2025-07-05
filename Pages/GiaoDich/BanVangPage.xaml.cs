using System.Collections.ObjectModel;
using System.Linq;
using MySqlConnector;
using MyLoginApp.Helpers;

// Replace System.Drawing with alias to avoid color conflict
using DrawingColor = System.Drawing.Color;
using MyLoginApp.Views;
using MyLoginApp.Models;
using MyLoginApp.Models.DanhMuc;
using static Microsoft.Maui.ApplicationModel.Permissions;
using System.Reflection;
using Plugin.Maui.Audio;
using ZXing.Common;
using SkiaSharp;
using ZXing;
using ZXing.QrCode;
using ZXing.SkiaSharp;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Text;

namespace MyLoginApp.Pages
{
    // Lớp để lưu trữ thông tin các mặt hàng đã quét
    public class ScannedItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GoldType { get; set; }
        public decimal Weight { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        // Thêm thuộc tính để lưu trữ đối tượng HangHoaModel
        public HangHoaModel HangHoa { get; set; }
    }

    public partial class BanVangPage : ContentPage
    {
        private bool isCameraOn = false;
        private bool isProcessingBarcode = true;
        private IAudioPlayer _audioPlayer;
        private KhachHang khachHangDaChon;
        private string maVangQuetDuoc;
        private ObservableCollection<KhachHang> DanhSachKhachHang = new();
        private CancellationTokenSource _cancelTokenSource;
        private HangHoaModel hangHoaDaQuet;
        private decimal ThanhToan = 0;
        private decimal TongTien = 0;
        private string maCCCDDaQuet; // Thêm biến để lưu mã CCCD đã quét
        private KhachHang khachHangTuCCCD; // Thêm biến để lưu khách hàng từ CCCD

        // Danh sách lưu trữ các mặt hàng đã quét
        private List<ScannedItem> scannedItems = new List<ScannedItem>();

        public BanVangPage()
        {
            InitializeComponent();
            // InitializeAudioPlayerAsync();

        }
        private async void InitializeAudioPlayerAsync()
        {
            var audioService = AudioManager.Current;
            var stream = await FileSystem.OpenAppPackageFileAsync("beep.mp3");
            _audioPlayer = audioService.CreatePlayer(stream);
        }
        // Cải thiện lấy nét cho camera, tối ưu cho khoảng cách gần


        private async void OnThanhToanClicked(object sender, EventArgs e)
        {
            if (khachHangDaChon == null)
            {
                await DisplayAlert("Thiếu thông tin", "Vui lòng chọn khách hàng trước khi thanh toán.", "OK");
                return;
            }

            if (scannedItems.Count == 0)
            {
                await DisplayAlert("Thiếu thông tin", "Vui lòng quét mã vàng trước khi thanh toán.", "OK");
                return;
            }

            // Tạo nội dung bill tổng hợp cho tất cả mặt hàng đã quét
            StringBuilder billBuilder = new StringBuilder();
            billBuilder.AppendLine($"👤 Khách hàng: {khachHangDaChon.TenKH}");
            billBuilder.AppendLine($"📞 Điện thoại: {khachHangDaChon.SoDienThoai}\n");
            billBuilder.AppendLine("=== DANH SÁCH MẶT HÀNG ===");

            int stt = 1;
            foreach (var item in scannedItems)
            {
                billBuilder.AppendLine($"{stt}. {item.Name} - {item.GoldType}");
                billBuilder.AppendLine($"   Mã: {item.Id}");
                billBuilder.AppendLine($"   TL: {item.Weight}g - Đơn giá: {item.Price:N0}đ");
                billBuilder.AppendLine($"   Thành tiền: {item.Total:N0}đ");
                billBuilder.AppendLine("-------------------------");
                stt++;
            }

            billBuilder.AppendLine($"\n💵 TỔNG THANH TOÁN:  {ThanhToan:N0}đ");

            // Hiển thị hóa đơn trong alert và đợi người dùng nhấn OK
            bool result = await DisplayAlert("Hóa Đơn Thanh Toán", billBuilder.ToString(), "OK", "Hủy");

            // Chỉ thực hiện thanh toán nếu người dùng nhấn OK
            if (result)
            {
                // Tạo phiếu xuất mới cho tất cả các mặt hàng
                bool phieuXuatCreated = await TaoPhieuXuat(khachHangDaChon.MaKH, scannedItems);

                if (phieuXuatCreated)
                {
                    // Chuyển sang trang HoaDonPage để xem chi tiết hóa đơn
                    await Navigation.PushAsync(new HoaDonPage(khachHangDaChon, scannedItems, ThanhToan));

                    // Reset các biến liên quan sau thanh toán thành công
                    khachHangDaChon = null;
                    maVangQuetDuoc = null;
                    hangHoaDaQuet = null;
                    lblKhachHangDaChon.IsVisible = false;
                    lblResult.Text = "📌 Kết quả:";
                    lblQRDetails.Text = "";
                    frameQRDetails.IsVisible = false;
                    lblTongTien.Text = "🧮 Thành tiền: 0đ";
                    ThanhToan = 0;
                    TongTien = 0;

                    // Xóa danh sách quét
                    scannedItems.Clear();
                    stackScannedItems.Clear();
                    frameScannedItems.IsVisible = false;
                }
                else
                {
                    await DisplayAlert("Lỗi", "Không thể tạo phiếu xuất. Vui lòng kiểm tra lại.", "OK");
                }
            }
            // Nếu người dùng nhấn "Hủy", không làm gì cả
        }


        private async Task<bool> TaoPhieuXuat(string khachHangId, List<ScannedItem> danhSachSanPham)
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return false;

                await using var transaction = await conn.BeginTransactionAsync();

                // Kiểm tra số lượng tồn kho cho tất cả mặt hàng
                foreach (var item in danhSachSanPham)
                {
                    var checkCmd = new MySqlCommand("SELECT SL_TON FROM ton_kho WHERE HANGHOAID = @HangHoaId", conn, transaction);
                    checkCmd.Parameters.AddWithValue("@HangHoaId", item.Id);
                    var result = await checkCmd.ExecuteScalarAsync();
                    
                    if (result == null || result == DBNull.Value)
                    {
                        await DisplayAlert("Thông báo", $"Hàng hóa {item.Name} (mã: {item.Id}) không tồn tại trong kho.", "OK");
                        await transaction.RollbackAsync();
                        return false;
                    }
                    
                    int slTon = Convert.ToInt32(result);
                    if (slTon <= 0)
                    {
                        string message = slTon == 0 ? "đã được bán trước đó" : "không tồn tại trong kho";
                        await DisplayAlert("Thông báo", $"Hàng hóa {item.Name} (mã: {item.Id}) {message}.", "OK");
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                // Tạo phiếu xuất chung
                string phieuXuatMa = $"PX{DateTime.Now:yyyyMMddHHmmss}";
                DateTime ngayXuat = DateTime.Now;

                // Tính tổng các thông số cho phiếu xuất
                decimal tongCanTong = danhSachSanPham.Sum(item => item.Weight);
                decimal tongTlHot = danhSachSanPham.Sum(item => item.HangHoa?.TrongLuongHot ?? 0);
                int tongSl = danhSachSanPham.Count;
                decimal tongGiaCong = danhSachSanPham.Sum(item => item.HangHoa?.GiaCong ?? 0);

                var insertPhieuXuat = new MySqlCommand(@"
                                    INSERT INTO phx_phieu_xuat (
                                        PHIEU_XUAT_MA, NGAY_XUAT, KH_ID, TONG_TIEN, KHACH_DUA,
                                        CAN_TONG, TL_HOT, THOI_LAI, TONG_SL, GIA_CONG, TIEN_BOT, THANH_TOAN
                                    )
                                    VALUES (
                                        @PhieuXuatMa, @NgayXuat, @KhachHangId, @TongTien, @KhachDua,
                                        @CanTong, @TLHot, @ThoiLai, @TongSL, @GiaCong, @TienBot, @ThanhToan
                                    );
                                    SELECT LAST_INSERT_ID();", conn, transaction);

                // Thiết lập giá trị cho các tham số
                insertPhieuXuat.Parameters.AddWithValue("@PhieuXuatMa", phieuXuatMa);
                insertPhieuXuat.Parameters.AddWithValue("@NgayXuat", ngayXuat);
                insertPhieuXuat.Parameters.AddWithValue("@KhachHangId", khachHangId);
                insertPhieuXuat.Parameters.AddWithValue("@TongTien", ThanhToan);
                insertPhieuXuat.Parameters.AddWithValue("@KhachDua", ThanhToan);
                insertPhieuXuat.Parameters.AddWithValue("@CanTong", tongCanTong);
                insertPhieuXuat.Parameters.AddWithValue("@TLHot", tongTlHot);
                insertPhieuXuat.Parameters.AddWithValue("@ThoiLai", 0);
                insertPhieuXuat.Parameters.AddWithValue("@TongSL", tongSl);
                insertPhieuXuat.Parameters.AddWithValue("@GiaCong", tongGiaCong);
                insertPhieuXuat.Parameters.AddWithValue("@TienBot", 0);
                insertPhieuXuat.Parameters.AddWithValue("@ThanhToan", ThanhToan);

                // Thực thi lệnh và lấy ID phiếu xuất mới
                long phieuXuatId = Convert.ToInt64(await insertPhieuXuat.ExecuteScalarAsync());

                // Thêm chi tiết cho từng sản phẩm
                foreach (var item in danhSachSanPham)
                {
                    var hangHoa = item.HangHoa;

                    if (hangHoa == null)
                    {
                        // Nếu không có thông tin đầy đủ, lấy thông tin từ cơ sở dữ liệu
                        hangHoa = await DatabaseHelper.LayHangHoaTheoMaAsync(item.Id);
                        if (hangHoa == null)
                        {
                            await transaction.RollbackAsync();
                            await DisplayAlert("Lỗi", $"Không tìm thấy thông tin hàng hóa có mã {item.Id}", "OK");
                            return false;
                        }
                    }

                    var insertChiTiet = new MySqlCommand(@"
                        INSERT INTO phx_chi_tiet_phieu_xuat (
                            PHIEU_XUAT_ID, HANGHOAID, HANGHOAMA, HANG_HOA_TEN,
                            SO_LUONG, DON_GIA, THANH_TIEN, CAN_TONG,
                            TL_HOT, GIA_CONG, NHOMHANGID, LOAIVANG
                        ) VALUES (
                            @PhieuXuatId, @HangHoaId, @HangHoaMa, @HangHoaTen,
                            @SoLuong, @DonGia, @ThanhTien, @CanTong,
                            @TLHot, @GiaCong, @NhomHangId, @LoaiVang)", conn, transaction);

                    insertChiTiet.Parameters.AddWithValue("@PhieuXuatId", phieuXuatId);
                    insertChiTiet.Parameters.AddWithValue("@HangHoaId", item.Id);
                    insertChiTiet.Parameters.AddWithValue("@HangHoaMa", item.Id);
                    insertChiTiet.Parameters.AddWithValue("@HangHoaTen", item.Name ?? (object)DBNull.Value);
                    insertChiTiet.Parameters.AddWithValue("@SoLuong", 1);
                    insertChiTiet.Parameters.AddWithValue("@DonGia", item.Price);
                    insertChiTiet.Parameters.AddWithValue("@ThanhTien", item.Total);
                    insertChiTiet.Parameters.AddWithValue("@CanTong", hangHoa.CanTong);
                    insertChiTiet.Parameters.AddWithValue("@TLHot", hangHoa.TrongLuongHot);
                    insertChiTiet.Parameters.AddWithValue("@GiaCong", hangHoa.GiaCong);
                    insertChiTiet.Parameters.AddWithValue("@NhomHangId", hangHoa.Nhom);
                    insertChiTiet.Parameters.AddWithValue("@LoaiVang", item.GoldType ?? (object)DBNull.Value);

                    if (await insertChiTiet.ExecuteNonQueryAsync() <= 0)
                    {
                        await transaction.RollbackAsync();
                        await DisplayAlert("Lỗi", $"Thêm chi tiết phiếu xuất cho mặt hàng {item.Name} thất bại.", "OK");
                        return false;
                    }

                    // Cập nhật trạng thái tồn kho cho từng sản phẩm
                    bool tonKhoUpdated = await CapNhatTonKhoThanhKhong(item.Id);
                    if (!tonKhoUpdated)
                    {
                        await transaction.RollbackAsync();
                        await DisplayAlert("Lỗi", $"Cập nhật tồn kho cho mặt hàng {item.Name} thất bại.", "OK");
                        return false;
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                await DisplayAlert("Lỗi", "Không thể tạo phiếu xuất: " + ex.Message, "OK");
                return false;
            }
        }


        private async Task<bool> CapNhatTonKhoThanhKhong(string hangHoaID)
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return false;

                // Cập nhật SL_TON trong bảng ton_kho thành 0 dựa trên HANGHOAID
                string updateQuery = @"
                UPDATE ton_kho
                SET SL_TON = 0
                WHERE HANGHOAID = @HangHoaID";

                await using (var updateCmd = new MySqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@HangHoaID", hangHoaID);
                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi cập nhật tồn kho: {ex.Message}");
                await DisplayAlert("Lỗi", $"Lỗi cập nhật tồn kho: {ex.Message}", "OK");
                return false;
            }
        }
        private async void OnChonKhachHangClicked(object sender, EventArgs e)
        {
            frameCustomerSelectionArea.IsVisible = true; // Hiển thị khung chứa các tùy chọn
            frameNhapTenKhach.IsVisible = true; // Mặc định hiển thị nhập tên
            lblHoac.IsVisible = true; // Mặc định hiển thị nhãn "Hoặc"
            btnQuetCCCD.IsVisible = true; // Mặc định hiển thị nút "Quét CCCD"

            frameThemKhach.IsVisible = false;
            lblKhachHangDaChon.IsVisible = false;
            btnXacNhanKhach.IsVisible = false;
            frameQuetCCCD.IsVisible = false; // Ẩn khung quét CCCD ban đầu
            lblCCCDInfo.IsVisible = false; // Ẩn thông báo CCCD khi bắt đầu chọn khách hàng
            lblCCCDInfo.Text = string.Empty; // Xóa nội dung thông báo CCCD
            btnXacNhanCCCD.IsVisible = false; // Ẩn nút xác nhận CCCD

            entryTenKhach.Text = string.Empty;
            lblThongTinKH.Text = string.Empty;
            lblThongTinKH.IsVisible = false;

            await LoadKhachHangAsync();
        }

        private void OnTenKhachHangChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
        {
            string tenNhap = entryTenKhach.Text?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(tenNhap))
            {
                lblThongTinKH.IsVisible = false;
                btnXacNhanKhach.IsVisible = false;
                frameThemKhach.IsVisible = false;
                lblHoac.IsVisible = true; // Hiển thị lại nhãn "Hoặc" nếu trường tên trống
                btnQuetCCCD.IsVisible = true; // Hiển thị lại nút "Quét CCCD" nếu trường tên trống
                return;
            }

            // Ẩn nhãn "Hoặc" và nút "Quét CCCD" khi bắt đầu nhập tên
            lblHoac.IsVisible = false;
            btnQuetCCCD.IsVisible = false;

            var khach = DanhSachKhachHang.FirstOrDefault(k => k.TenKH.ToLower().Contains(tenNhap));

            if (khach != null)
            {
                lblThongTinKH.Text = $"✅ Đã tìm thấy: {khach.TenKH} - 📞 {khach.SoDienThoai}";
                lblThongTinKH.IsVisible = true;
                btnXacNhanKhach.IsVisible = true;
                frameThemKhach.IsVisible = false;
                khachHangDaChon = khach;
            }
            else
            {
                lblThongTinKH.Text = "❌ Không tìm thấy khách. Bạn có thể thêm mới.";
                lblThongTinKH.IsVisible = true;
                btnXacNhanKhach.IsVisible = false;
                frameThemKhach.IsVisible = true;
            }
        }

        private async void OnXacNhanKhachClicked(object sender, EventArgs e)
        {
            if (khachHangDaChon != null)
            {
                lblKhachHangDaChon.Text = $"✅ {khachHangDaChon.TenKH} - 📞 {khachHangDaChon.SoDienThoai}";
                lblKhachHangDaChon.IsVisible = true;
                lblCCCDInfo.IsVisible = false; // Ẩn thông báo CCCD
                lblCCCDInfo.Text = string.Empty; // Xóa nội dung thông báo CCCD
                frameNhapTenKhach.IsVisible = false;
                frameThemKhach.IsVisible = false;
                btnXacNhanKhach.IsVisible = false;
                frameCustomerSelectionArea.IsVisible = false; // Ẩn khung tùy chọn sau khi xác nhận

                // Tự động bật camera sau khi xác nhận khách hàng nếu chưa bật
                if (!isCameraOn && await Permissions.CheckStatusAsync<Permissions.Camera>() == PermissionStatus.Granted)
                {
                    isCameraOn = true;
                }
            }
            else
            {
                await DisplayAlert("Lỗi", "Không có khách nào được chọn!", "OK");
            }
        }

        private async void OnThemKhachHangClicked(object sender, EventArgs e)
        {
            string tenKhach = entryTenKhach.Text?.Trim();
            string soDienThoai = entrySoDienThoai.Text?.Trim();
            string diaChi = entryDiaChi.Text?.Trim();
            string cmnd = entryCCCD.Text?.Trim();

            if (string.IsNullOrEmpty(tenKhach) || string.IsNullOrEmpty(soDienThoai))
            {
                await DisplayAlert("Thiếu thông tin", "Vui lòng nhập tên và số điện thoại của khách hàng.", "OK");
                return;
            }

            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string insertQuery = "INSERT INTO phx_khach_hang (KH_TEN, DIEN_THOAI, DIA_CHI, CMND) VALUES (@TenKH, @SDT, @DiaChi, @CMND)";
                using var cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@TenKH", tenKhach);
                cmd.Parameters.AddWithValue("@SDT", soDienThoai);
                cmd.Parameters.AddWithValue("@DiaChi", diaChi);
                cmd.Parameters.AddWithValue("@CMND", cmnd);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    // Lấy KH_ID vừa được tạo
                    long newKhachHangId = cmd.LastInsertedId;

                    // Cập nhật KH_MA bằng KH_ID
                    string updateQuery = "UPDATE phx_khach_hang SET KH_MA = @KhachHangMa WHERE KH_ID = @KhachHangId";
                    using var updateCmd = new MySqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@KhachHangMa", newKhachHangId.ToString()); // Chuyển đổi sang string để gán cho KH_MA
                    updateCmd.Parameters.AddWithValue("@KhachHangId", newKhachHangId);
                    await updateCmd.ExecuteNonQueryAsync();

                    await DisplayAlert("Thành công", "Đã thêm khách hàng mới thành công!", "OK");
                    await LoadKhachHangAsync();
                    khachHangDaChon = DanhSachKhachHang.FirstOrDefault(k => k.TenKH == tenKhach && k.SoDienThoai == soDienThoai);
                    btnXacNhanKhach.IsVisible = true;
                    lblKhachHangDaChon.Text = $"✅ {khachHangDaChon.TenKH} - 📞 {khachHangDaChon.SoDienThoai}";
                    lblKhachHangDaChon.IsVisible = true;
                    lblCCCDInfo.IsVisible = false; // Ẩn thông báo CCCD
                    lblCCCDInfo.Text = string.Empty; // Xóa nội dung thông báo CCCD
                    frameNhapTenKhach.IsVisible = false;
                    frameThemKhach.IsVisible = false;
                    frameQuetCCCD.IsVisible = false;
                    btnXacNhanKhach.IsVisible = false; // Ẩn nút xác nhận cũ
                    btnXacNhanCCCD.IsVisible = false; // Ẩn nút xác nhận CCCD
                    frameCustomerSelectionArea.IsVisible = false; // Ẩn khung tùy chọn sau khi xác nhận

                    // Tự động bật camera sau khi xác nhận khách hàng nếu chưa bật
                    if (!isCameraOn && await Permissions.CheckStatusAsync<Permissions.Camera>() == PermissionStatus.Granted)
                    {
                        isCameraOn = true;
                    }
                    lblCCCDInfo.IsVisible = false; // Ẩn thông báo CCCD
                    lblCCCDInfo.Text = string.Empty; // Xóa nội dung thông báo CCCD
                }
                else
                {
                    await DisplayAlert("Lỗi", "Không thể thêm khách hàng mới.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
                lblKhachHangDaChon.IsVisible = true;
                frameNhapTenKhach.IsVisible = false;
                frameThemKhach.IsVisible = false;
                frameQuetCCCD.IsVisible = false;
                btnXacNhanKhach.IsVisible = false; // Ẩn nút xác nhận cũ
                btnXacNhanCCCD.IsVisible = false; // Ẩn nút xác nhận CCCD
                frameCustomerSelectionArea.IsVisible = false; // Ẩn khung tùy chọn sau khi xác nhận
                lblCCCDInfo.IsVisible = false; // Ẩn thông báo CCCD

                // Tự động bật camera sau khi xác nhận khách hàng nếu chưa bật
                if (!isCameraOn && await Permissions.CheckStatusAsync<Permissions.Camera>() == PermissionStatus.Granted)
                {
                    isCameraOn = true;
                }
                lblCCCDInfo.IsVisible = false; // Ẩn thông báo CCCD
                lblCCCDInfo.Text = string.Empty; // Xóa nội dung thông báo CCCD
            }
        }

        private async Task<bool> RequestCameraPermission()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Lỗi", "Ứng dụng cần quyền Camera để hoạt động!", "OK");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi yêu cầu quyền camera: {ex.Message}");
                await DisplayAlert("Lỗi", $"Lỗi khi yêu cầu quyền camera: {ex.Message}", "OK");
                return false;
            }
        }

        private async void OnToggleCameraClicked(object sender, EventArgs e)
        {
            {
                await DisplayAlert("Lỗi", "Camera chưa được khởi tạo!", "OK");
                return;
            }

            if (!isCameraOn && !await RequestCameraPermission()) return;

            isCameraOn = !isCameraOn;

        }
        private async Task LoadKhachHangAsync()
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = "SELECT KH_MA, KH_TEN, DIEN_THOAI FROM phx_khach_hang";
                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                DanhSachKhachHang.Clear();

                while (await reader.ReadAsync())
                {
                    DanhSachKhachHang.Add(new KhachHang
                    {
                        MaKH = reader["KH_MA"].ToString(),
                        TenKH = reader["KH_TEN"].ToString(),
                        SoDienThoai = reader["DIEN_THOAI"].ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RequestCameraPermission();

        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            if (isCameraOn)
            {
            }
        }

        // Thêm các phương thức hỗ trợ lấy nét tốt hơn cho camera

        private async Task<string> ChupVaQuetQRAsync()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo == null)
                    return null;

                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                var bitmap = SKBitmap.Decode(imageBytes);
                if (bitmap == null)
                {
                    await DisplayAlert("Lỗi", "Không thể đọc ảnh vừa chụp.", "OK");
                    return null;
                }

                // ✅ Resize ảnh nếu quá lớn để đảm bảo quét chính xác
                const int maxWidth = 1024;
                if (bitmap.Width > maxWidth)
                {
                    float scale = (float)maxWidth / bitmap.Width;
                    var resized = bitmap.Resize(
                        new SKImageInfo((int)(bitmap.Width * scale), (int)(bitmap.Height * scale)),
                        SKFilterQuality.High);
                    bitmap.Dispose();
                    bitmap = resized;
                }

                // ✅ Cấu hình BarcodeReader tối ưu
                var reader = new BarcodeReader<SKBitmap>(bmp => new SKBitmapLuminanceSource(bmp))
                {
                    AutoRotate = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        PureBarcode = false,
                        PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.CODABAR
                }
                    }
                };

                var result = reader.Decode(bitmap);
                if (result == null || string.IsNullOrWhiteSpace(result.Text))
                {
                    await DisplayAlert("Thông báo", "Không tìm thấy mã. Vui lòng chụp mã rõ nét, chính diện và đủ sáng.", "OK");
                    return null;
                }
                if (_audioPlayer != null)
                {
                    _audioPlayer.Play();
                }
                return result.Text;
            }
            catch (Exception ex)
            {
                await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi khi quét mã: {ex.Message}", "OK");
                return null;
            }
        }

        private async void OnChupVaQuetQRClicked(object sender, EventArgs e)
        {
            try
            {
                var qrResult = await ChupVaQuetQRAsync();
                if (!string.IsNullOrEmpty(qrResult))
                {
                    maVangQuetDuoc = qrResult; // 🔥 GÁN vào biến toàn cục để sau dùng
                    lblResult.Text = $"📌 Kết quả: {qrResult}";
                    lblQRDetails.Text = $"📦 Mã: {qrResult} - Đang kiểm tra thông tin...";
                    frameQRDetails.IsVisible = true;

                    try
                    {
                        // Tự động tìm thông tin hàng hóa:
                        var hangHoa = await DatabaseHelper.LayHangHoaTheoMaAsync(qrResult.Trim());

                        if (hangHoa == null)
                        {
                            lblQRDetails.Text = "❌ Không tìm thấy thông tin hàng hóa.";
                            return;
                        }

                        // Kiểm tra số lượng tồn kho
                        using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                        if (conn != null)
                        {
                            var checkCmd = new MySqlCommand("SELECT SL_TON FROM ton_kho WHERE HANGHOAID = @HangHoaId", conn);
                            checkCmd.Parameters.AddWithValue("@HangHoaId", qrResult.Trim());
                            var result = await checkCmd.ExecuteScalarAsync();
                            
                            if (result == null || result == DBNull.Value)
                            {
                                lblQRDetails.Text = "❌ Hàng hóa không tồn tại trong kho.";
                                return;
                            }
                            
                            int slTon = Convert.ToInt32(result);
                            if (slTon <= 0)
                            {
                                string message = slTon == 0 ? "đã được bán trước đó" : "không tồn tại trong kho";
                                lblQRDetails.Text = $"❌ Hàng hóa {message}.";
                                return;
                            }
                        }

                        // Lấy đơn giá bán từ nhóm hàng
                        var loaiVang = await DatabaseHelper.Lay_DonGiaBan_loaivang_TheoMa_hanghoaAsync(qrResult.Trim());

                        // Tính toán tổng tiền dựa trên thông tin vàng và đơn giá
                        decimal donGiaBan = loaiVang?.DonGiaBan ?? hangHoa.DonViGoc; // Fallback to DonViGoc if loaiVang is null
                        decimal truHot = hangHoa.CanTong - hangHoa.TrongLuongHot;

                        // Tính toán thành tiền theo công thức: (donGiaBan/100) * truHot + giá công
                        TongTien = (donGiaBan / 100) * truHot + hangHoa.GiaCong * hangHoa.SoLuong;

                        hangHoaDaQuet = hangHoa; // Lưu lại hàng hóa đã quét

                        lblQRDetails.Text =
                                $"📦 {"Tên hàng    :".PadRight(10)}   {hangHoa.TenHangHoa}\n" +
                                $"🏷️ {"Loại vàng   :".PadRight(10)}   {hangHoa.LoaiVang}\n" +
                                $"⚖️ {"Cân tổng    :".PadRight(10)}   {hangHoa.CanTong:N3}g\n" +
                                $"💎 {"Hột         :".PadRight(10)}   {hangHoa.TrongLuongHot:N3}g\n" +
                                $"💰 {"Trọng l.vàng:".PadRight(10)}   {truHot:N3}g\n" +
                                $"💰 {"Đơn giá bán :".PadRight(10)}   {donGiaBan:N0}đ\n" +
                                $"🛠️ {"Giá công    :".PadRight(10)}   {hangHoa.GiaCong:N0}đ\n" +
                                $"🧾 {"Thành Tiền   :".PadRight(10)}   {TongTien:N0}đ";

                        // Thêm vào tổng tiền thanh toán
                        ThanhToan += TongTien;

                        // 👉 Hiển thị thành tiền đã cộng dồn
                        lblTongTien.Text = $"🧮 Tổng Thanh Toán: {ThanhToan:N0}đ";

                        // Kiểm tra xem hàng hóa đã được quét trước đó chưa
                        if (scannedItems.Any(item => item.Id == hangHoa.HangHoaID))
                        {
                            lblQRDetails.Text = $"❌ Hàng hóa {hangHoa.TenHangHoa} đã được quét trước đó.";
                            return;
                        }

                        // Thêm vào danh sách đã quét
                        AddScannedItemToList(hangHoa, TongTien);
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Lỗi dữ liệu", $"Lỗi khi lấy thông tin hàng hóa: {ex.Message}", "OK");
                        lblQRDetails.Text = "❌ Lỗi khi lấy thông tin hàng hóa.";
                    }
                }
                else
                {
                    await DisplayAlert("QR Code", "Không tìm thấy mã QR trong ảnh.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Lỗi khi quét mã QR: {ex.Message}", "OK");
            }
        }

        private async void OnResetClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Xác nhận", "Bạn có chắc muốn làm mới giao dịch này?", "Có", "Không");

            if (confirm)
            {
                // Reset tất cả thông tin
                khachHangDaChon = null;
                maVangQuetDuoc = null;
                hangHoaDaQuet = null;
                ThanhToan = 0;
                TongTien = 0;

                // Xóa danh sách quét
                scannedItems.Clear();
                stackScannedItems.Clear();

                // Reset giao diện
                lblKhachHangDaChon.IsVisible = false;
                lblResult.Text = "📌 Kết quả:";
                lblQRDetails.Text = "";
                frameQRDetails.IsVisible = false;
                frameScannedItems.IsVisible = false;
                lblTongTien.Text = "🧮 Thành tiền: 0đ";
                frameNhapTenKhach.IsVisible = false;
                frameThemKhach.IsVisible = false;
                frameQuetCCCD.IsVisible = false; // Ẩn khung quét CCCD khi reset
                frameCustomerSelectionArea.IsVisible = false; // Ẩn khung tùy chọn khi reset
                lblCCCDInfo.IsVisible = false; // Ẩn thông báo CCCD khi reset
                lblCCCDInfo.Text = string.Empty; // Xóa nội dung thông báo CCCD khi reset

                await DisplayAlert("Thông báo", "Đã làm mới giao dịch!", "OK");
            }
        }

        // Phương thức thêm item vào danh sách và hiển thị
        private void AddScannedItemToList(HangHoaModel hangHoa, decimal tongTien)
        {
            var item = new ScannedItem
            {
                Id = hangHoa.HangHoaID,
                Name = hangHoa.TenHangHoa,
                GoldType = hangHoa.LoaiVang,
                Weight = hangHoa.CanTong,
                Price = hangHoa.DonViGoc,
                Total = tongTien,
                HangHoa = hangHoa  // Lưu trữ đối tượng HangHoaModel để sử dụng sau này
            };

            scannedItems.Add(item);

            // Hiển thị danh sách các mặt hàng đã quét
            frameScannedItems.IsVisible = true;

            // Tạo frame để chứa thông tin item với bố cục đẹp hơn
            var itemFrame = new Microsoft.Maui.Controls.Frame
            {
                BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#2A2A2A"),
                CornerRadius = 10,
                Margin = new Microsoft.Maui.Thickness(0, 0, 0, 5),
                Padding = new Microsoft.Maui.Thickness(10)
            };

            // Tạo layout chứa nội dung
            var layout = new Microsoft.Maui.Controls.VerticalStackLayout();

            // Dòng tên và loại vàng
            var nameLabel = new Microsoft.Maui.Controls.Label
            {
                Text = $"{item.Name} - {item.GoldType}",
                TextColor = Microsoft.Maui.Graphics.Colors.White,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold
            };

            // Dòng trọng lượng và giá
            var detailsLabel = new Microsoft.Maui.Controls.Label
            {
                Text = $"Trọng lượng: {item.Weight:N3}g - Giá: {item.Price:N0}đ",
                TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#CCCCCC"),
                FontSize = 12
            };

            // Dòng thành tiền
            var totalLabel = new Microsoft.Maui.Controls.Label
            {
                Text = $"Thành tiền: {item.Total:N0}đ",
                TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#00FF7F"),
                FontSize = 12,
                FontAttributes = FontAttributes.Bold
            };

            // Thêm các label vào layout
            layout.Add(nameLabel);
            layout.Add(detailsLabel);
            layout.Add(totalLabel);

            // Thêm layout vào frame
            itemFrame.Content = layout;

            // Thêm tap gesture để xóa item
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) => {
                bool confirm = await DisplayAlert("Xác nhận", $"Bạn có muốn xóa {item.Name} khỏi danh sách?", "Có", "Không");
                if (confirm)
                {
                    // Giảm tổng tiền
                    ThanhToan -= item.Total;
                    lblTongTien.Text = $"🧮 Tổng Thanh Toán: {ThanhToan:N0}đ";

                    // Xóa khỏi danh sách
                    scannedItems.Remove(item);
                    stackScannedItems.Remove(itemFrame);

                    // Ẩn khung nếu không còn item nào
                    if (scannedItems.Count == 0)
                    {
                        frameScannedItems.IsVisible = false;
                    }
                }
            };
            itemFrame.GestureRecognizers.Add(tapGestureRecognizer);

            // Thêm frame vào stack
            stackScannedItems.Add(itemFrame);
        }

        private async void OnQuetCCCDClicked(object sender, EventArgs e)
        {
            frameCustomerSelectionArea.IsVisible = true; // Hiển thị khung lựa chọn khách hàng chính
            frameNhapTenKhach.IsVisible = false; // Ẩn khung nhập tên khách hàng
            frameThemKhach.IsVisible = false;
            lblKhachHangDaChon.IsVisible = false;
            btnXacNhanKhach.IsVisible = false;
            frameQuetCCCD.IsVisible = true; // Hiển thị khung quét CCCD
            lblCCCDInfo.IsVisible = false;
            btnXacNhanCCCD.IsVisible = false;
            lblHoac.IsVisible = false; // Ẩn nhãn "Hoặc"
            btnQuetCCCD.IsVisible = false; // Ẩn nút "Quét CCCD" sau khi nhấn

            try
            {
                // Tái sử dụng logic chụp và quét QR từ phương thức hiện có
                var cccdResult = await ChupVaQuetQRAsync();
                if (!string.IsNullOrEmpty(cccdResult))
                {
                    var parts = cccdResult.Split('|'); // Khai báo biến parts ở đây
                    string cccdToSearch = cccdResult.Trim();

                    // Phân tích chuỗi CCCD nếu nó có định dạng phức tạp (ví dụ: có dấu '|'), lấy phần số CCCD
                    if (parts.Length > 0)
                    {
                        cccdToSearch = parts[0].Trim();
                    }

                    maCCCDDaQuet = cccdToSearch; // Lưu trữ số CCCD đã được trích xuất

                    lblCCCDInfo.Text = $"Đang tìm kiếm khách hàng với CCCD: {maCCCDDaQuet}...";
                    lblCCCDInfo.IsVisible = true;

                    // Tìm khách hàng trong DB theo CCCD
                    var khach = await DatabaseHelper.LayKhachHangTheoCCCDAsync(maCCCDDaQuet);

                    if (khach != null)
                    {
                        khachHangTuCCCD = khach;
                        lblCCCDInfo.Text = $"✅ {khach.TenKH} - 📞 {khach.SoDienThoai}";
                        btnXacNhanCCCD.IsVisible = true;
                    }
                    else
                    {
                        lblCCCDInfo.Text = "❌ Không tìm thấy khách hàng với này. Vui lòng thêm khách hàng mới.";
                        lblCCCDInfo.IsVisible = true; // Đảm bảo thông báo hiển thị
                        frameThemKhach.IsVisible = true; // Hiển thị form thêm mới
                        btnXacNhanCCCD.IsVisible = false; // Ẩn nút xác nhận khi không tìm thấy khách hàng

                        // Sử dụng biến parts đã khai báo ở trên
                        if (parts.Length >= 6) // Đảm bảo có đủ các phần tử cần thiết
                        {
                            entryCCCD.Text = parts[0].Trim(); // Số CCCD
                            entryTenKhach.Text = parts[2].Trim(); // Họ và tên
                            entryDiaChi.Text = parts[5].Trim(); // Địa chỉ
                            entrySoDienThoai.Text = string.Empty; // Để trống số điện thoại hoặc xóa nếu có
                        }
                        else
                        {
                            // Nếu không theo định dạng chi tiết, chỉ điền toàn bộ chuỗi vào trường CMND và thông báo
                            entryCCCD.Text = cccdResult.Trim();
                            entryTenKhach.Text = string.Empty; // Đảm bảo trường tên trống
                            entryDiaChi.Text = string.Empty; // Đảm bảo trường địa chỉ trống
                            entrySoDienThoai.Text = string.Empty; // Đảm bảo trường SĐT trống
                            await DisplayAlert("Cảnh báo", "Không thể phân tích đầy đủ thông tin từ mã CCCD. Vui lòng điền thủ công các trường còn thiếu.", "OK");
                        }

                        khachHangTuCCCD = null;
                    }
                }
                else
                {
                    lblCCCDInfo.Text = "Không quét được mã CCCD. Vui lòng thử lại.";
                    lblCCCDInfo.IsVisible = true; // Đảm bảo thông báo hiển thị
                    btnXacNhanCCCD.IsVisible = false; // Ẩn nút xác nhận khi quét không thành công
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Lỗi khi quét CCCD: {ex.Message}", "OK");
                lblCCCDInfo.Text = "Đã xảy ra lỗi khi quét CCCD.";
                lblCCCDInfo.IsVisible = true; // Đảm bảo thông báo hiển thị
                btnXacNhanCCCD.IsVisible = false; // Ẩn nút xác nhận khi có lỗi
            }
        }

        private async void OnXacNhanCCCDClicked(object sender, EventArgs e)
        {
            if (khachHangTuCCCD != null)
            {
                khachHangDaChon = khachHangTuCCCD;
                lblKhachHangDaChon.Text = $"✅ {khachHangDaChon.TenKH} - 📞 {khachHangDaChon.SoDienThoai}";
                lblKhachHangDaChon.IsVisible = true;
                lblCCCDInfo.IsVisible = false; // Ẩn thông báo CCCD
                lblCCCDInfo.Text = string.Empty; // Xóa nội dung thông báo CCCD
                frameNhapTenKhach.IsVisible = false;
                frameThemKhach.IsVisible = false;
                frameQuetCCCD.IsVisible = false;
                btnXacNhanKhach.IsVisible = false; // Ẩn nút xác nhận cũ
                btnXacNhanCCCD.IsVisible = false; // Ẩn nút xác nhận CCCD
                frameCustomerSelectionArea.IsVisible = false; // Ẩn khung tùy chọn sau khi xác nhận

                // Tự động bật camera sau khi xác nhận khách hàng nếu chưa bật
                if (!isCameraOn && await Permissions.CheckStatusAsync<Permissions.Camera>() == PermissionStatus.Granted)
                {
                    isCameraOn = true;
                }
                lblCCCDInfo.IsVisible = false; // Ẩn thông báo CCCD
                lblCCCDInfo.Text = string.Empty; // Xóa nội dung thông báo CCCD
            }
            else
            {
                await DisplayAlert("Lỗi", "Không có khách nào được chọn từ CCCD!", "OK");
            }
        }

    }
}