using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MyLoginApp.Helpers; // Import helper
using MySqlConnector;
using Microsoft.Maui.Storage; // Thêm dòng này để sử dụng SecureStorage

namespace MyLoginApp.Pages
{
    public partial class LoginPage : ContentPage
    {
        // ✅ Biến kiểm tra đã kết nối CSDL chưa
        private bool isConnectedToDatabase = false;

        public LoginPage()
        {
            InitializeComponent();

            // ✅ Tự động load lại thông tin kết nối đã lưu khi mở app
            LoadPreviousConnection();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadSavedCredentials();
        }

        private async Task LoadSavedCredentials()
        {
            try
            {
                var savedUsername = await SecureStorage.GetAsync("username");
                var savedPassword = await SecureStorage.GetAsync("password");

                if (!string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
                {
                    usernameEntry.Text = savedUsername;
                    passwordEntry.Text = savedPassword;
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu không thể truy cập SecureStorage
                Console.WriteLine($"Lỗi khi tải thông tin đăng nhập: {ex.Message}");
            }
        }

        // ✅ Load kết nối cũ và kiểm tra luôn
        private async void LoadPreviousConnection()
        {
            DatabaseHelper.LoadSavedConnectionString();

            if (DatabaseHelper.IsConnectionConfigured())
            {
                if (await DatabaseHelper.TestConnectionAsync())
                {
                    isConnectedToDatabase = true;

                    // ✅ Lấy tên database từ DatabaseHelper
                    string dbName = DatabaseHelper.DatabaseName;

                    await DisplayAlert("Thông báo", $"✅ Đã tự động kết nối với CSDL [{dbName}] thành công!", "OK");
                    connectFieldsLayout.IsVisible = false; // Ẩn khối nhập vì đã kết nối
                }
                else
                {
                    await DisplayAlert("Cảnh báo", "⚠️ Không thể kết nối CSDL từ thông tin đã lưu. Vui lòng kiểm tra lại!", "OK");
                }
            }
        }

        // 👉 Sự kiện nút "Kết Nối"
        private void OnShowConnectFieldsClicked(object sender, EventArgs e)
        {
            connectFieldsLayout.IsVisible = !connectFieldsLayout.IsVisible; // Ẩn/Hiện khối nhập kết nối
        }

        // 👉 Sự kiện nút "Xác nhận kết nối"
        private async void OnConnectClicked(object sender, EventArgs e)
        {
            string dbUser = dbUserEntry.Text?.Trim();
            string dbPassword = dbPasswordEntry.Text?.Trim();
            string dbName = dbNameEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(dbUser) || string.IsNullOrWhiteSpace(dbPassword) || string.IsNullOrWhiteSpace(dbName))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin kết nối!", "OK");
                return;
            }

            // ✅ Cập nhật chuỗi kết nối
            DatabaseHelper.SetConnectionString(dbUser, dbPassword, dbName);

            // ✅ Kiểm thử kết nối
            if (await DatabaseHelper.TestConnectionAsync())
            {
                isConnectedToDatabase = true; // ✅ Đánh dấu đã kết nối thành công
                await DisplayAlert("Thành công", $"✅ Kết nối CSDL [{dbName}] thành công!", "OK");
                connectFieldsLayout.IsVisible = false; // Ẩn khối nhập sau khi thành công
            }
            else
            {
                isConnectedToDatabase = false; // Kết nối thất bại
                await DisplayAlert("Lỗi", "❌ Kết nối thất bại, vui lòng kiểm tra lại thông tin!", "OK");
            }
        }

        // 👉 Sự kiện nút Login
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // ✅ Kiểm tra đã kết nối chưa
            if (!isConnectedToDatabase)
            {
                await DisplayAlert("Lỗi", "⚠️ Bạn chưa kết nối CSDL. Vui lòng nhấn nút 'Kết Nối' trước khi đăng nhập!", "OK");
                return;
            }

            try
            {
                // hoàn thiện sẽ mở đăng nhập tay
                string username = usernameEntry.Text?.Trim();
                string password = passwordEntry.Text?.Trim();

                


                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    await DisplayAlert("Lỗi", "Vui lòng nhập tên đăng nhập và mật khẩu!", "OK");
                    return;
                }

                // ✅ Kiểm tra đăng nhập
                if (await CheckLoginAsync(username, password))
                {
                    await DisplayAlert("Thông báo", $"🎉 Đăng nhập thành công: {username}", "OK");

                    // Lưu tên đăng nhập và mật khẩu
                    await SecureStorage.SetAsync("username", username);
                    await SecureStorage.SetAsync("password", password);

                    await Shell.Current.GoToAsync("//MainMenuPage");
                }
                else
                {
                    await DisplayAlert("Lỗi", "❌ Sai tài khoản hoặc mật khẩu!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi hệ thống", ex.Message, "OK");
            }
        }

        // 👉 Check login qua bcrypt, sử dụng DatabaseHelper
        private async Task<bool> CheckLoginAsync(string username, string password)
        {
            try
            {
                // ✅ Dùng kết nối chung
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();

                const string query = "SELECT MAT_KHAU FROM pq_user WHERE USER_TEN = @username LIMIT 1";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                var resultObj = await cmd.ExecuteScalarAsync();

                if (resultObj != null && BCrypt.Net.BCrypt.Verify(password, resultObj.ToString()))
                    return true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi kết nối", ex.Message, "OK");
            }

            return false;
        }
    }
}
