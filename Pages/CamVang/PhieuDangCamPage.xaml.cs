using System.Collections.ObjectModel;
using MySqlConnector;
using MyLoginApp.Helpers;
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
    public partial class PhieuDangCamPage : ContentPage
    {
        private bool isCameraOn = false;
        private bool isProcessingBarcode = true;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationTokenSource _searchTokenSource;
        private const int SEARCH_DELAY = 300; // Độ trễ 300ms

        public PhieuDangCamPage()
        {
            InitializeComponent();
            BindingContext = new PhieuDangCamViewModel();  // Gán BindingContext ở đây
        }

        private async Task<bool> RequestCameraPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied)
            {
                bool answer = await DisplayAlert("Quyền truy cập camera", 
                    "Ứng dụng cần quyền truy cập camera để quét mã. Bạn có muốn cấp quyền không?", 
                    "Có", "Không");
                if (answer)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    return status == PermissionStatus.Granted;
                }
                return false;
            }

            status = await Permissions.RequestAsync<Permissions.Camera>();
            return status == PermissionStatus.Granted;
        }

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

                // Resize ảnh nếu quá lớn để đảm bảo quét chính xác
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

                // Cấu hình BarcodeReader tối ưu
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
                            BarcodeFormat.CODABAR,
                            BarcodeFormat.EAN_13,
                            BarcodeFormat.EAN_8,
                            BarcodeFormat.UPC_A,
                            BarcodeFormat.UPC_E
                        }
                    }
                };

                var result = reader.Decode(bitmap);
                if (result == null || string.IsNullOrWhiteSpace(result.Text))
                {
                    await DisplayAlert("Thông báo", "Không tìm thấy mã. Vui lòng chụp mã rõ nét, chính diện và đủ sáng.", "OK");
                    return null;
                }

                return result.Text;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Có lỗi khi quét mã: {ex.Message}", "OK");
                return null;
            }
        }

        private async Task OnChupVaQuetQRClicked(object sender, EventArgs e)
        {
            try
            {
                // Hiển thị loading với thông báo đang chụp
                loadingGrid.IsVisible = true;
                lblLoadingText.Text = "Đang tìm mã phiếu........";

                var result = await ChupVaQuetQRAsync();
                if (string.IsNullOrEmpty(result))
                {
                    loadingGrid.IsVisible = false;
                    lblKhongTimThay.Text = "❌ Không tìm thấy mã phiếu";
                    lblKhongTimThay.IsVisible = true;
                    return;
                }

                // Cập nhật thông báo đang tìm kiếm
                lblLoadingText.Text = $"Đang load danh sách phiếu .....: {result}";

                // Tự động điền vào ô tìm kiếm và thực hiện tìm kiếm
                if (BindingContext is PhieuDangCamViewModel viewModel)
                {
                    // Cập nhật SearchKeyword để hiển thị trong ô tìm kiếm
                    viewModel.SearchKeyword = result;
                    // Thực hiện tìm kiếm
                    await viewModel.OnSearchTextChanged(result);
                    // Ẩn thông báo không tìm thấy
                    lblKhongTimThay.IsVisible = false;
                }

                // Ẩn loading sau khi tìm kiếm xong
                loadingGrid.IsVisible = false;
            }
            catch (Exception ex)
            {
                // Ẩn loading nếu có lỗi
                loadingGrid.IsVisible = false;
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        // Xử lý sự kiện khi nhập từ khoá tìm kiếm
        private async void OnSearchTextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue;

            // Hủy tìm kiếm trước đó nếu có
            _searchTokenSource?.Cancel();
            _searchTokenSource = new CancellationTokenSource();

            try
            {
                // Đợi một khoảng thời gian trước khi thực hiện tìm kiếm
                await Task.Delay(SEARCH_DELAY, _searchTokenSource.Token);

                if (BindingContext is PhieuDangCamViewModel viewModel)
                {
                    // Hiển thị loading
                    loadingGrid.IsVisible = true;
                    lblLoadingText.Text = $"Đang tìm kiếm phiếu với mã: {searchText}";

                    try
                    {
                        await viewModel.OnSearchTextChanged(searchText);
                        // Ẩn thông báo không tìm thấy khi người dùng nhập lại
                        lblKhongTimThay.IsVisible = false;
                    }
                    finally
                    {
                        // Ẩn loading sau khi tìm kiếm xong
                        loadingGrid.IsVisible = false;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Bỏ qua nếu tìm kiếm bị hủy
            }
            catch (Exception ex)
            {
                // Ẩn loading nếu có lỗi
                loadingGrid.IsVisible = false;
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        private async void OnTimKiemPhieuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Chọn cách tìm kiếm", "Huỷ", null, "Nhập mã phiếu", "Quét mã QR");
            if (action == "Nhập mã phiếu")
            {
                string result = await DisplayPromptAsync("Nhập mã phiếu", "Vui lòng nhập mã phiếu cần tìm:");
                if (!string.IsNullOrWhiteSpace(result))
                {
                    if (BindingContext is PhieuDangCamViewModel viewModel)
                    {
                        loadingGrid.IsVisible = true;
                        lblLoadingText.Text = $"Đang tìm kiếm phiếu với mã: {result}";
                        await viewModel.OnSearchTextChanged(result);
                        lblKhongTimThay.IsVisible = false;
                        loadingGrid.IsVisible = false;
                    }
                }
            }
            else if (action == "Quét mã QR")
            {
                await OnChupVaQuetQRClicked(sender, e);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _searchTokenSource?.Cancel();
            _searchTokenSource?.Dispose();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is PhieuDangCamViewModel viewModel)
            {
                loadingGrid.IsVisible = true;
                lblLoadingText.Text = "Đang tải danh sách phiếu...";
                await viewModel.OnSearchTextChanged("");
                lblKhongTimThay.IsVisible = false;
                loadingGrid.IsVisible = false;
            }
        }
    }
}
