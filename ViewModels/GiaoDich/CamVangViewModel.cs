using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyLoginApp.Helpers;
using MySqlConnector;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Collections.Generic;

namespace MyLoginApp.ViewModels
{
    // Class để chứa thông tin loại vàng và đơn giá cầm
    public class LoaiVangItem : ObservableObject
    {
        public int NhomId { get; set; }
        public string TenLoaiVang { get; set; }
        public double DonGiaCam { get; set; }

        public override string ToString() => TenLoaiVang;
    }

    public partial class CamVangViewModel : ObservableObject
    {
        [ObservableProperty] private string tenKhach, soDienThoai, tenHang, ghiChu;
        [ObservableProperty] private double? canTong, tlHot, donGia, thanhTien, tienCam, laiSuat;
        [ObservableProperty] private DateTime ngayCam = DateTime.Now;
        [ObservableProperty] private DateTime? ngayHetHan = null;

        // Thêm ObservableProperty cho SelectedLoaiVang
        [ObservableProperty] private LoaiVangItem selectedLoaiVang;

        // Giả sử có property lưu ID khách hàng
        [ObservableProperty] private string khachHangId;

        // Trừ hột được tính tự động từ cân tổng và TL hột
        public string TruHotDisplay
        {
            get
            {
                double truHot = (CanTong ?? 0) - (TlHot ?? 0);
                return truHot.ToString("N3") + " gram";
            }
        }

        public ObservableCollection<LoaiVangItem> LoaiVangList { get; } = new ObservableCollection<LoaiVangItem>();

        // Tải danh sách loại vàng từ cơ sở dữ liệu
        private async Task LoadLoaiVangAsync()
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn != null)
                {
                    using var cmd = new MySqlCommand("SELECT NHOMHANGID, NHOM_TEN, DON_GIA_CAM FROM nhom_hang WHERE SU_DUNG = 1", conn);
                    using var reader = await cmd.ExecuteReaderAsync();
                    LoaiVangList.Clear();
                    while (await reader.ReadAsync())
                    {
                        LoaiVangList.Add(new LoaiVangItem
                        {
                            NhomId = Convert.ToInt32(reader["NHOMHANGID"]),
                            TenLoaiVang = reader["NHOM_TEN"].ToString(),
                            DonGiaCam = Convert.ToDouble(reader["DON_GIA_CAM"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không tải được danh sách loại vàng: {ex.Message}", "OK");
            }
        }

        // Xử lý khi SelectedLoaiVang thay đổi
        partial void OnSelectedLoaiVangChanged(LoaiVangItem value)
        {
            if (value != null)
            {
                DonGia = value.DonGiaCam / 100;
            }
            else
            {
                DonGia = 0;
            }
            TinhThanhTien();
        }

        // Các phương thức tính toán tự động
        partial void OnDonGiaChanged(double? value) => TinhThanhTien();
        partial void OnCanTongChanged(double? value)
        {
            TinhTruHot();
            TinhThanhTien();
        }
        partial void OnTlHotChanged(double? value)
        {
            TinhTruHot();
            TinhThanhTien();
        }

        // Tính trừ hột = cân tổng - TL hột
        private void TinhTruHot()
        {
            OnPropertyChanged(nameof(TruHotDisplay));
        }
        // Tính thành tiền = trừ hột * đơn giá
        private void TinhThanhTien() => ThanhTien = ((CanTong ?? 0) - (TlHot ?? 0)) * (DonGia ?? 0);

        // Xử lý thanh toán
        [RelayCommand]
        public async Task<string> OnThanhToanClickedAsync()
        {
            string maPhieuVuaTao = null;
            try
            {
                var missingFields = new List<string>();

                // Kiểm tra thông tin khách hàng
                if (string.IsNullOrWhiteSpace(TenKhach))
                {
                    missingFields.Add("Tên khách hàng");
                }

                // Kiểm tra thông tin vàng
                if (SelectedLoaiVang == null)
                {
                    missingFields.Add("Loại vàng");
                }

                if (string.IsNullOrWhiteSpace(TenHang))
                {
                    missingFields.Add("Tên hàng hóa");
                }

                // Kiểm tra thông tin cân nặng
                if ((CanTong ?? 0) <= 0)
                {
                    missingFields.Add("Cân tổng (phải lớn hơn 0)");
                }

                if ((TlHot ?? 0) < 0)
                {
                    missingFields.Add("TL hột (không được âm)");
                }

                // Kiểm tra thông tin tiền
                if ((TienCam ?? 0) <= 0)
                {
                    missingFields.Add("Tiền cầm (phải lớn hơn 0)");
                }

                if ((LaiSuat ?? 0) < 0)
                {
                    missingFields.Add("Lãi suất (không được âm)");
                }

                // Kiểm tra ngày hết hạn
                if (NgayHetHan == null)
                {
                    missingFields.Add("Ngày hết hạn");
                }
                else if (NgayHetHan <= NgayCam)
                {
                    missingFields.Add("Ngày hết hạn (phải sau ngày cầm)");
                }

                // Kiểm tra thành tiền
                if ((ThanhTien ?? 0) <= 0)
                {
                    missingFields.Add("Thành tiền (phải lớn hơn 0)");
                }

                // Nếu có trường nào chưa nhập hoặc không hợp lệ
                if (missingFields.Count > 0)
                {
                    var message = "Vui lòng kiểm tra lại các thông tin sau:\n\n" + 
                                string.Join("\n", missingFields.Select(f => $"• {f}"));
                    await Shell.Current.DisplayAlert("Thiếu thông tin", message, "OK");
                    return null;
                }

                // Nếu tất cả thông tin đã hợp lệ, tiếp tục lưu vào database
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn != null)
                {
                    using var cmd = new MySqlCommand(@"
                        INSERT INTO cam_chi_tiet_phieu_cam_vang (
                            TEN_HANG_HOA, CAN_TONG, TL_HOT, DON_GIA, THANH_TIEN, SO_LUONG, GHI_CHU, SU_DUNG, LOAI_VANG, NHOM_ID
                        ) VALUES (
                            @tenHangHoa, @canTong, @tlHot, @donGia, @thanhTien, @soLuong, @ghiChu, @suDung, @loaiVang, @nhomId
                        )", conn);

                    cmd.Parameters.AddWithValue("@tenHangHoa", TenHang ?? "");
                    cmd.Parameters.AddWithValue("@canTong", CanTong ?? 0);
                    cmd.Parameters.AddWithValue("@tlHot", TlHot ?? 0);
                    cmd.Parameters.AddWithValue("@donGia", DonGia ?? 0);
                    cmd.Parameters.AddWithValue("@thanhTien", ThanhTien ?? 0);
                    cmd.Parameters.AddWithValue("@soLuong", 1); // mặc định 1
                    cmd.Parameters.AddWithValue("@ghiChu", string.IsNullOrWhiteSpace(GhiChu) ? DBNull.Value : GhiChu);
                    cmd.Parameters.AddWithValue("@suDung", 1); // mặc định 1
                    cmd.Parameters.AddWithValue("@loaiVang", SelectedLoaiVang != null ? SelectedLoaiVang.TenLoaiVang : "");
                    cmd.Parameters.AddWithValue("@nhomId", DonGia);

                    await cmd.ExecuteNonQueryAsync();

                    // Lấy CHI_TIET_ID vừa tạo
                    var getIdCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn);
                    var chiTietIdObj = await getIdCmd.ExecuteScalarAsync();
                    long chiTietId = Convert.ToInt64(chiTietIdObj);
                    long phieuCamVangId = chiTietId + 10;

                    // Cập nhật lại PHIEU_CAM_VANG_ID
                    using (var updateCmd = new MySqlCommand(
                        "UPDATE cam_chi_tiet_phieu_cam_vang SET PHIEU_CAM_VANG_ID = @phieuCamVangId WHERE CHI_TIET_ID = @chiTietId", conn))
                    {
                        updateCmd.Parameters.AddWithValue("@phieuCamVangId", phieuCamVangId);
                        updateCmd.Parameters.AddWithValue("@chiTietId", chiTietId);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    // Lấy PHIEU_CAM_VANG_ID lớn nhất hiện tại
                    long newPhieuCamVangId = 1;
                    using (var getMaxCmd = new MySqlCommand("SELECT IFNULL(MAX(PHIEU_CAM_VANG_ID), 0) + 1 FROM cam_phieu_cam_vang", conn))
                    {
                        var obj = await getMaxCmd.ExecuteScalarAsync();
                        newPhieuCamVangId = Convert.ToInt64(obj);
                    }

                    // Lấy PHIEU_MA mới (rút gọn: PC.ddMMyy01)
                    string todayShort = DateTime.Now.ToString("ddMMyy");
                    string prefix = $"PC.{todayShort}";
                    string phieuMaMoi = $"{prefix}01";
                    using (var getPhieuMaCmd = new MySqlCommand("SELECT PHIEU_MA FROM cam_phieu_cam_vang WHERE PHIEU_MA LIKE @prefix ORDER BY PHIEU_MA DESC LIMIT 1", conn))
                    {
                        getPhieuMaCmd.Parameters.AddWithValue("@prefix", prefix + "%");
                        var obj = await getPhieuMaCmd.ExecuteScalarAsync();
                        if (obj != null)
                        {
                            string lastPhieuMa = obj.ToString();
                            int stt = int.Parse(lastPhieuMa.Substring(prefix.Length)) + 1;
                            phieuMaMoi = $"{prefix}{stt:D2}";
                        }
                    }

                    // Insert với PHIEU_CAM_VANG_ID mới
                    using (var cmdPhieu = new MySqlCommand(@"
                        INSERT INTO cam_phieu_cam_vang
                        (PHIEU_CAM_VANG_ID, SO_PHIEU, PHIEU_MA, CAN_TONG, TL_HOT, TONG_GIA_TRI, LAI_XUAT, TIEN_KHACH_NHAN, TU_NGAY, DEN_NGAY, SO_NGAY_CAM, KH_ID, TIEN_LAI_NGAY, TIEN_CHU, GHI_CHU)
                        VALUES
                        (@phieuCamVangId, @soPhieu, @phieuMa, @canTong, @tlHot, @tongGiaTri, @laiXuat, @tienKhachNhan, NOW(), @denNgay, @soNgayCam, @khId, @tienLaiNgay, @tienChu, @ghiChu)
                    ", conn))
                    {
                        cmdPhieu.Parameters.AddWithValue("@phieuCamVangId", newPhieuCamVangId);
                        cmdPhieu.Parameters.AddWithValue("@soPhieu", "123"); // Luôn là 123
                        cmdPhieu.Parameters.AddWithValue("@phieuMa", phieuMaMoi);
                        cmdPhieu.Parameters.AddWithValue("@canTong", CanTong ?? 0);
                        cmdPhieu.Parameters.AddWithValue("@tlHot", TlHot ?? 0);
                        cmdPhieu.Parameters.AddWithValue("@tongGiaTri", ThanhTien ?? 0);
                        cmdPhieu.Parameters.AddWithValue("@laiXuat", LaiSuat ?? 0);
                        cmdPhieu.Parameters.AddWithValue("@tienKhachNhan", TienCam ?? 0);
                        cmdPhieu.Parameters.AddWithValue("@denNgay", NgayHetHan ?? DateTime.Now);
                        cmdPhieu.Parameters.AddWithValue("@soNgayCam", (NgayHetHan != null ? (NgayHetHan.Value - NgayCam).Days : 0));
                        cmdPhieu.Parameters.AddWithValue("@khId", KhachHangId ?? "");
                        cmdPhieu.Parameters.AddWithValue("@tienLaiNgay", 0);
                        cmdPhieu.Parameters.AddWithValue("@tienChu", "");
                        cmdPhieu.Parameters.AddWithValue("@ghiChu", string.IsNullOrWhiteSpace(GhiChu) ? DBNull.Value : GhiChu);
                        await cmdPhieu.ExecuteNonQueryAsync();
                    }

                    maPhieuVuaTao = phieuMaMoi;

                    await Shell.Current.DisplayAlert("Thành công", $"Tạo phiếu cầm thành công!\nMã phiếu: {maPhieuVuaTao}", "OK");
                    
                    // Reset form
                    TenKhach = "";
                    SoDienThoai = "";
                    TenHang = "";
                    CanTong = null;
                    TlHot = null;
                    DonGia = null;
                    ThanhTien = null;
                    TienCam = null;
                    LaiSuat = null;
                    NgayCam = DateTime.Now;
                    NgayHetHan = null;
                    GhiChu = "";
                    // Không set SelectedLoaiVang = null để tránh lỗi binding
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể lưu thông tin: {ex.Message}", "OK");
            }
            return maPhieuVuaTao;
        }

        // Khởi tạo và tải dữ liệu ban đầu
        public CamVangViewModel() => LoadLoaiVangAsync();
    }
}
