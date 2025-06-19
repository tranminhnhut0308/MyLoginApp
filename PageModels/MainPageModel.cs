using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MyLoginApp.PageModels
{
    public class MainPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor
        public MainPageModel()
        {
            // Sample Data
            TodoCategoryData = new ObservableCollection<CategoryModel>()
            {
                new CategoryModel { Title = "Danh Mục 1", Count = 10 },
                new CategoryModel { Title = "Danh Mục 2", Count = 5 },
                new CategoryModel { Title = "Danh Mục 3", Count = 8 }
            };
        }

        private ObservableCollection<CategoryModel> _todoCategoryData;
        public ObservableCollection<CategoryModel> TodoCategoryData
        {
            get => _todoCategoryData;
            set
            {
                if (_todoCategoryData != value)
                {
                    _todoCategoryData = value;
                    OnPropertyChanged(nameof(TodoCategoryData));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Model
    public class CategoryModel
    {
        public string Title { get; set; }
        public int Count { get; set; }
    }
}
