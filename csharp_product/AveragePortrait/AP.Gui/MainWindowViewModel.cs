using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using AP.Logic;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.TeamFoundation.MVVM;
using System.Windows.Media.Imaging;
using Emgu.CV.WPF;
using Microsoft.Win32;

namespace AP.Gui
{
    public class MainWindowViewModel : ViewModelBase
    {
        public BitmapSource AverageFaceResult { get; set; }

        public List<Face> Faces { get; set; }

        public IList<FaceViewModel> FacesViewModel { get; set; }

        public ICommand OpenPhotosCommand { get; set; }

        public ICommand PrepareAveragePortraitCommand { get; set; }

        public MainWindowViewModel()
        {
            OpenPhotosCommand = new RelayCommand(OpenPhotos);
            PrepareAveragePortraitCommand = new RelayCommand(PrepareAveragePortrait);
        }

        private void OpenPhotos()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.ShowDialog();
            var images = openFileDialog.FileNames;

            LoadFaces(images);
        }

        private void LoadFaces(string[] images, bool cropFace = false)
        {
            FacesViewModel = new List<FaceViewModel>();

            var faceProcessor = new FaceProcessor();
            Faces = images.Select(image => new Face(faceProcessor, image, cropFace)).ToList();
            
            foreach (var face in Faces)
            {
                FacesViewModel.Add(new FaceViewModel(face));
            }

            RaisePropertyChanged("FacesViewModel");
        }

        private void PrepareAveragePortrait()
        {
            var averageFace = new AverageFace(900, 600);
            var standardEyes = new List<Eye>
                {
                    new Eye {X = 200, Y = 200},
                    new Eye {X = 250, Y = 300}
                };
            averageFace.MakeAverage(FacesViewModel.Select(_ => _.Face).ToList(), standardEyes);
            AverageFaceResult = BitmapSourceConvert.ToBitmapSource(averageFace.Result);
            RaisePropertyChanged("AverageFaceResult");
        }
    }

    public class FaceViewModel
    {
        public Face Face { get; set; }
        public BitmapSource Tumbnail { get; set; }

        public FaceViewModel(Face face)
        {
            Face = face;
            Tumbnail = BitmapSourceConvert.ToBitmapSource(face.Thumbnail);
        }
    }
}
