using System.Windows;
using System.Windows.Controls;

namespace AP.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
            set { DataContext = value; }
            
        }
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
        }

        private void ThumbnailsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = e.AddedItems;
            var face = items[0] as FaceViewModel;
            ViewModel.SelectFaceForEdit(face);
        }
    }
}
