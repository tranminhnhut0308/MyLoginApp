using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using MyLoginApp.Models;
using MySqlConnector;
using MyLoginApp.Models.DanhMuc;

namespace MyLoginApp.Helpers
{
    public static class DatabaseHelper
    {
        private static string _connectionString;

        // ✅ Thuộc tính lấy tên Database đang kết nối
        public static string DatabaseName { get; private set; }

        // ✅ Set lại chuỗi kết nối và lưu vào Preferences
        public static void SetConnectionString(string user, string password, string database)
        {
            _connectionString = $"server=baokhoagold.ddns.net;user={user};password={password};database={database};port=3306;Connection Timeout=15;";

            // ✅ Lưu thông tin vào bộ nhớ Preferences
            Preferences.Set("db_user", user);
            Preferences.Set("db_password", password);
            Preferences.Set("db_database", database);

            DatabaseName = database; // Gán lại tên database
        }

        // ✅ Load thông tin đã lưu từ Preferences
        public static void LoadSavedConnectionString()
        {
            var user = Preferences.Get("db_user", string.Empty);
            var password = Preferences.Get("db_password", string.Empty);
            var database = Preferences.Get("db_database", string.Empty);

            if (!string.IsNullOrWhiteSpace(user) &&
                !string.IsNullOrWhiteSpace(password) &&
                !string.IsNullOrWhiteSpace(database))
            {
                _connectionString = $"server=baokhoagold.ddns.net;user={user};password={password};database={database};port=3306;Connection Timeout=15;";
                DatabaseName = database; // Gán lại tên database khi load
            }
        }

        // ✅ Lấy connection mở sẵn
        public static async Task<MySqlConnection> GetOpenConnectionAsync()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                Console.WriteLine("Chuỗi kết nối trống hoặc null");
                return null;
            }

            try
            {
                var conn = new MySqlConnection(_connectionString);
                // ConnectionTimeout là thuộc tính chỉ đọc, đã đặt trong chuỗi kết nối
                await conn.OpenAsync();
                return conn;
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"Lỗi MySQL khi mở kết nối: {sqlEx.Message}, Mã lỗi: {sqlEx.Number}");
                throw; // Ném lại ngoại lệ để xử lý ở lớp cao hơn
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi không xác định khi mở kết nối: {ex.Message}");
                throw; // Ném lại ngoại lệ để xử lý ở lớp cao hơn
            }
        }

        // ✅ Test kết nối
        public static async Task<bool> TestConnectionAsync()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                return false;
            }

            try
            {
                // Tạo chuỗi kết nối với timeout ngắn hơn cho kiểm tra
                string testConnectionString = _connectionString;
                if (!testConnectionString.Contains("Connection Timeout"))
                {
                    testConnectionString += ";Connection Timeout=5;";
                }

                using var conn = new MySqlConnection(testConnectionString);
                // ConnectionTimeout là thuộc tính chỉ đọc, đã đặt trong chuỗi kết nối
                await conn.OpenAsync();

                // Kiểm tra kết nối bằng cách thực hiện một truy vấn đơn giản
                using var cmd = new MySqlCommand("SELECT 1", conn);
                cmd.CommandTimeout = 5;
                await cmd.ExecuteScalarAsync();

                await conn.CloseAsync();
                return true;
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"Lỗi MySQL khi kiểm tra kết nối: {sqlEx.Message}, Mã lỗi: {sqlEx.Number}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi không xác định khi kiểm tra kết nối: {ex.Message}");
                return false;
            }
        }

        // ✅ Kiểm tra đã có kết nối chưa
        public static bool IsConnectionConfigured()
        {
            return !string.IsNullOrWhiteSpace(_connectionString);
        }

        public static async Task<HangHoaModel> LayHangHoaTheoMaAsync(string maHangHoa)
        {
            if (string.IsNullOrEmpty(maHangHoa))
                return null;

            try
            {
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new MySqlCommand(@"
            SELECT 
                dm.HANGHOAMA, 
                dm.HANG_HOA_TEN,
                nh.NHOM_TEN,
                lh.LOAI_TEN,
                dm.CAN_TONG,
                dm.TL_HOT, 
                dm.CONG_GOC, 
                dm.GIA_CONG,
                dm.DON_GIA_GOC
            FROM 
                danh_muc_hang_hoa dm 
                JOIN loai_hang lh ON lh.LOAIID = dm.LOAIID 
                JOIN nhom_hang nh ON nh.NHOMHANGID = dm.NHOMHANGID 
            WHERE 
                dm.SU_DUNG = 1 AND dm.HANGHOAMA = @ma
 
            LIMIT 20", conn);

                cmd.Parameters.AddWithValue("@ma", maHangHoa);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {

                    return new HangHoaModel
                    {
                        HangHoaID = reader["HANGHOAMA"].ToString(),
                        TenHangHoa = reader["HANG_HOA_TEN"].ToString(),
                        Nhom = reader["NHOM_TEN"].ToString(),
                        LoaiVang = reader["LOAI_TEN"].ToString(),
                        CanTong = reader.GetDecimal("CAN_TONG"),
                        TrongLuongHot = reader.GetDecimal("TL_HOT"),
                        CongGoc = reader.GetDecimal("CONG_GOC"),
                        GiaCong = reader.GetDecimal("GIA_CONG"),
                        DonViGoc = reader.GetDecimal("DON_GIA_GOC"),
                        SoLuong = 1,// nếu mặc định 1, hoặc thêm vào SELECT nếu có

                    };


                }
                ;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy hàng hóa: {ex.Message}");
            }
            return null;
        }

        public static async Task<LoaiVangHelper> Lay_DonGiaBan_loaivang_TheoMa_hanghoaAsync(string mahanghoa)
        {
            if (string.IsNullOrEmpty(mahanghoa))
                return null;

            try
            {
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new MySqlCommand(@"
                SELECT 
                    nh.DON_GIA_BAN,
                    nh.NHOMHANGID,
                    lh.LOAI_TEN
                FROM 
                    danh_muc_hang_hoa dm 
                    JOIN loai_hang lh ON lh.LOAIID = dm.LOAIID 
                    JOIN nhom_hang nh ON nh.NHOMHANGID = dm.NHOMHANGID 
                WHERE 
                    dm.SU_DUNG = 1 AND dm.HANGHOAMA = @ma
 
                LIMIT 20", conn);

                cmd.Parameters.AddWithValue("@ma", mahanghoa);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new LoaiVangHelper
                    {
                        DonGiaBan = reader.IsDBNull(reader.GetOrdinal("DON_GIA_BAN")) ? null : reader.GetDecimal("DON_GIA_BAN"),
                        NhomHangID = reader.GetInt32("NHOMHANGID"),
                        LoaiTen = reader.GetString("LOAI_TEN")
                    };
                }
                else
                {
                    Console.WriteLine($"Không tìm thấy dữ liệu với mã nhóm vàng: {mahanghoa}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy đơn giá bán của loại vàng: {ex.Message}");
            }
            return null;
        }

        public static async Task<KhachHang> LayKhachHangTheoCCCDAsync(string cccd)
        {
            if (string.IsNullOrEmpty(cccd))
                return null;

            try
            {
                using var conn = await GetOpenConnectionAsync();
                using var cmd = new MySqlCommand("SELECT KH_MA, KH_TEN, DIEN_THOAI, CMND FROM phx_khach_hang WHERE CMND = @cccd LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@cccd", cccd);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new KhachHang
                    {
                        MaKH = reader["KH_MA"].ToString(),
                        TenKH = reader["KH_TEN"].ToString(),
                        SoDienThoai = reader["DIEN_THOAI"].ToString(),
                        CMND = reader["CMND"].ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy khách hàng theo CMND: {ex.Message}");
            }
            return null;
        }
    }

    // Simple LoaiVangModel class for database operations
    public class LoaiVangHelper
    {
        public decimal? DonGiaBan { get; set; }
        public int NhomHangID { get; set; }
        public string LoaiTen { get; set; }
    }
}
