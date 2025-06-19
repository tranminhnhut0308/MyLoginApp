using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoginApp.ViewModels
{
    public partial class LoginPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private String _userName;

        [ObservableProperty]
        private String _password;

    }
}
