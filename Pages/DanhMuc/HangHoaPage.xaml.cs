using MyLoginApp.ViewModels;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace MyLoginApp.Pages;

public partial class HangHoaPage : ContentPage
{
    private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();
    private bool _isFormatting = false;
    private decimal? _canTong;
    private decimal? _congGoc;
    public decimal? CanTong
    {
        get => _canTong;
        set
        {
            if (_canTong != value)
            {
                _canTong = value;
                OnPropertyChanged(nameof(CanTong));
            }
        }
    }

    public decimal? CongGoc
    {
        get => _congGoc;
        set
        {
            if (_congGoc != value)
            {
                _congGoc = value;
                OnPropertyChanged(nameof(CongGoc));
            }
        }
    }

    public HangHoaPage()
    {
        try
        {
            InitializeComponent();
            BindingContext = new HangHoaViewModel();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khởi tạo HangHoaPage: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(async () => {
                await Shell.Current.DisplayAlert("Lỗi", "Không thể khởi tạo trang Hàng hóa", "OK");
            });
        }
    }

    private async void OnTimKiemClicked(object sender, EventArgs e)
    {
        if (BindingContext is HangHoaViewModel viewModel)
        {
            string keyword = await DisplayPromptAsync("Tìm kiếm", "Nhập mã hoặc tên hàng hóa:", "Tìm", "Hủy", initialValue: viewModel.SearchKeyword);
            if (keyword != null) // User clicked "Tìm" or "Hủy"
            {
                viewModel.SearchKeyword = keyword; // Set keyword (even if empty) and trigger saearch
            }
        }
    }
    private void CongGocEntry_TextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        if (_isFormatting) return;
        if (sender is Entry entry)
        {
            // Loại bỏ mọi ký tự không phải số
            string raw = new string(entry.Text?.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(raw))
            {
                entry.Text = string.Empty;
                return;
            }
            if (decimal.TryParse(raw, out decimal value))
            {
                string formatted = value.ToString(); // Không format lại có dấu phẩy
                if (entry.Text != formatted)
                {
                    _isFormatting = true;
                    int cursorPos = formatted.Length;
                    entry.Text = formatted;
                    entry.CursorPosition = cursorPos;
                    _isFormatting = false;
                }
            }
        }
    }
    private async void OnSearchCompleted(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is HangHoaViewModel viewModel)
            {
                if (viewModel.SearchCommand.CanExecute(null))
                    await Task.Run(() => viewModel.SearchCommand.Execute(null));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi tìm kiếm: {ex.Message}");
        }
    }

    private void OnSearchTextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        try
        {
            // Huỷ tìm kiếm trước đó nếu có
            _searchCancellationTokenSource.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _searchCancellationTokenSource.Token;

            // Bắt đầu tìm kiếm sau 500ms nếu không có thao tác mới
            Task.Delay(500, cancellationToken).ContinueWith(task =>
            {
                if (!task.IsCanceled)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (sender is SearchBar searchBar && BindingContext is HangHoaViewModel viewModel)
                        {
                            string query = searchBar.Text?.Trim() ?? string.Empty;
                            viewModel.SearchKeyword = query;
                            if (viewModel.SearchCommand.CanExecute(null))
                                viewModel.SearchCommand.Execute(null);
                        }
                    });
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi xử lý tìm kiếm: {ex.Message}");
        }
    }

    private async void OnClearSearchClicked(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is HangHoaViewModel viewModel)
            {
                viewModel.SearchKeyword = string.Empty;
                if (viewModel.SearchCommand.CanExecute(null))
                    await Task.Run(() => viewModel.SearchCommand.Execute(null));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi xóa tìm kiếm: {ex.Message}");
        }
    }

    private void CongGocEntry_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            // Loại bỏ mọi ký tự không phải số
            string raw = new string(entry.Text?.Where(char.IsDigit).ToArray());
            decimal value;
            if (!decimal.TryParse(raw, out value))
            {
                value = 0;
            }
            entry.Text = value.ToString(); // Không format lại có dấu phẩy
        }
    }
}

public class ZeroToEmptyStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal dec && dec == 0)
            return string.Empty;
        return value?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(value as string))
            return 0m;
        if (decimal.TryParse(value as string, out decimal result))
            return result;
        return 0m;
    }
}
