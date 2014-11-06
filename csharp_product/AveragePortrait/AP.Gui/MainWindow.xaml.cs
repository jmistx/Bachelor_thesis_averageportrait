using System.Windows;

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
    }
}
