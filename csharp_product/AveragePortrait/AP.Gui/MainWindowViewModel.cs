using AP.Logic;
using Microsoft.TeamFoundation.MVVM;
using System.Windows.Media.Imaging;
using Emgu.CV.WPF;

namespace AP.Gui
{
    public class MainWindowViewModel : ViewModelBase
    {
        public BitmapSource AverageFaceResult { get; set; }

        public void MakeAveragePortrait()
        {
            var image = Program.MakeAveragePortrait();
            AverageFaceResult = BitmapSourceConvert.ToBitmapSource(image);
            RaisePropertyChanged("AverageFaceResult");
        }
    }
}
