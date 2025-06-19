using MyLoginApp.PageModels;

namespace MyLoginApp.Pages.Controls
{
    public partial class CategoryChart : ContentView
    {
        public CategoryChart()
        {
            InitializeComponent();
            BindingContext = new MainPageModel(); // Gán BindingContext
        }
    }
}
