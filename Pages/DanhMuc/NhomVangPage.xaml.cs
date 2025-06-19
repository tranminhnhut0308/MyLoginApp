using MyLoginApp.Models;
using MyLoginApp.ViewModels;
using System.Collections.ObjectModel;

namespace MyLoginApp.Pages;

public partial class NhomVangPage : ContentPage
{
    // Khai báo danh sách nhóm vàng

    public NhomVangPage()
    {
        InitializeComponent();
        BindingContext = new NhomVangViewModel();
    }
}