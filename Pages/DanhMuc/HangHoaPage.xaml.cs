using MyLoginApp.ViewModels;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace MyLoginApp.Pages;

public partial class HangHoaPage : ContentPage
{
    private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

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
                viewModel.SearchKeyword = keyword; // Set keyword (even if empty) and trigger search
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

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
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
}
