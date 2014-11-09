using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ListView = System.Windows.Controls.ListView;
using MessageBox = System.Windows.MessageBox;

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
            var listView = sender as ListView;
            Debug.Assert(listView != null, "listView != null");
            if (listView.SelectedItems.Count <= 0) return;
            var face = listView.SelectedItems[0] as FaceViewModel;
            ViewModel.SelectFaceForEdit(face);
        }

        private void FaceEdit_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(sender as IInputElement);
            ViewModel.SetCurrentFaceLeftEye(position);
        }

        private void FaceEdit_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(sender as IInputElement);
            ViewModel.SetCurrentFaceRightEye(position);
        }
    }
}
