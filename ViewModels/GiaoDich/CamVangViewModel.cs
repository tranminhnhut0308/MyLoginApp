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
            OnPropertyChanged(nameof(TruHot));
            TinhThanhTien();
        }
        partial void OnTlHotChanged(double? value)
        {
            OnPropertyChanged(nameof(TruHot));
            TinhThanhTien();
        }

        // Tính thành tiền = trừ hột * đơn giá (dùng giá trị nhập tay của TruHot)
        private void TinhThanhTien() => ThanhTien = TruHot * (DonGia ?? 0);

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

                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn != null)
                {
                    // 1. Trước khi insert, lấy PHIEU_CAM_VANG_ID lớn nhất hiện có
                    long newPhieuCamVangId = 1;
                    using (var getMaxIdCmd = new MySqlCommand("SELECT MAX(PHIEU_CAM_VANG_ID) FROM cam_phieu_cam_vang", conn))
                    {
                        var obj = await getMaxIdCmd.ExecuteScalarAsync();
                        if (obj != DBNull.Value && obj != null)
                        {
                            newPhieuCamVangId = Convert.ToInt64(obj) + 1;
                        }
                    }

                    // 2. Insert vào bảng cam_phieu_cam_vang trước
                    string phieuMaMoi = ""; // Khai báo biến trước
                    // Sinh mã phiếu mới
                    string todayShort = DateTime.Now.ToString("ddMMyy");
                    string prefix = $"PC.{todayShort}";
                    phieuMaMoi = $"{prefix}01";
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
                    using (var cmdPhieu = new MySqlCommand(@"
                        INSERT INTO cam_phieu_cam_vang
                        (PHIEU_CAM_VANG_ID, SO_PHIEU, PHIEU_MA, CAN_TONG, TL_HOT, TONG_GIA_TRI, LAI_XUAT, TIEN_KHACH_NHAN, TU_NGAY, DEN_NGAY, SO_NGAY_CAM, KH_ID, TIEN_LAI_NGAY, TIEN_CHU, GHI_CHU)
                        VALUES
                        (@phieuCamVangId, @soPhieu, @phieuMa, @canTong, @tlHot, @tongGiaTri, @laiXuat, @tienKhachNhan, NOW(), @denNgay, @soNgayCam, @khId, @tienLaiNgay, @tienChu, @ghiChu)
                    ", conn))
                    {
                        cmdPhieu.Parameters.AddWithValue("@phieuCamVangId", newPhieuCamVangId);
                        cmdPhieu.Parameters.AddWithValue("@soPhieu", "123");
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

                    // 3. Insert vào bảng cam_chi_tiet_phieu_cam_vang với PHIEU_CAM_VANG_ID vừa tạo
                    using (var cmd = new MySqlCommand(@"
                        INSERT INTO cam_chi_tiet_phieu_cam_vang (
                            PHIEU_CAM_VANG_ID, TEN_HANG_HOA, CAN_TONG, TL_HOT, DON_GIA, THANH_TIEN, SO_LUONG, GHI_CHU, SU_DUNG, LOAI_VANG, NHOM_ID
                        ) VALUES (
                            @phieuCamVangId, @tenHangHoa, @canTong, @tlHot, @donGia, @thanhTien, @soLuong, @ghiChu, @suDung, @loaiVang, @nhomId
                        )", conn))
                    {
                        cmd.Parameters.AddWithValue("@phieuCamVangId", newPhieuCamVangId);
                        cmd.Parameters.AddWithValue("@tenHangHoa", TenHang ?? "");
                        cmd.Parameters.AddWithValue("@canTong", CanTong ?? 0);
                        cmd.Parameters.AddWithValue("@tlHot", TlHot ?? 0);
                        cmd.Parameters.AddWithValue("@donGia", DonGia ?? 0);
                        cmd.Parameters.AddWithValue("@thanhTien", ThanhTien ?? 0);
                        cmd.Parameters.AddWithValue("@soLuong", 1);
                        cmd.Parameters.AddWithValue("@ghiChu", string.IsNullOrWhiteSpace(GhiChu) ? DBNull.Value : GhiChu);
                        cmd.Parameters.AddWithValue("@suDung", 1);
                        cmd.Parameters.AddWithValue("@loaiVang", SelectedLoaiVang != null ? SelectedLoaiVang.TenLoaiVang : "");
                        cmd.Parameters.AddWithValue("@nhomId", DonGia);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // 4. Insert vào bảng cam_kho_vang_cam
                    using (var cmdKho = new MySqlCommand(@"
                        INSERT INTO cam_kho_vang_cam (
                            MA_HANG_HOA, TEN_HANG_HOA, LOAI_VANG, CAN_TONG, TL_HOT, DON_GIA, NGAY_CAM, NGAY_QUA_HAN, TEN_KH, PHIEU_MA, SU_DUNG
                        ) VALUES (
                            @maHangHoa, @tenHangHoa, @loaiVang, @canTong, @tlHot, @donGia, @ngayCam, @ngayQuaHan, @tenKh, @phieuMa, @suDung
                        )", conn))
                    {
                        cmdKho.Parameters.AddWithValue("@maHangHoa", DBNull.Value); // hoặc mã hàng hóa thực tế nếu có
                        cmdKho.Parameters.AddWithValue("@tenHangHoa", TenHang ?? "");
                        cmdKho.Parameters.AddWithValue("@loaiVang", SelectedLoaiVang != null ? SelectedLoaiVang.TenLoaiVang : "");
                        cmdKho.Parameters.AddWithValue("@canTong", CanTong ?? 0);
                        cmdKho.Parameters.AddWithValue("@tlHot", TlHot ?? 0);
                        cmdKho.Parameters.AddWithValue("@donGia", DonGia ?? 0);
                        cmdKho.Parameters.AddWithValue("@ngayCam", NgayCam);
                        cmdKho.Parameters.AddWithValue("@ngayQuaHan", NgayHetHan ?? (object)DBNull.Value);
                        cmdKho.Parameters.AddWithValue("@tenKh", TenKhach ?? "");
                        cmdKho.Parameters.AddWithValue("@phieuMa", phieuMaMoi);
                        cmdKho.Parameters.AddWithValue("@suDung", 1);
                        await cmdKho.ExecuteNonQueryAsync();
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

        // Thêm property TruHot tự động tính
        public double TruHot => (CanTong ?? 0) - (TlHot ?? 0);
    }
}
