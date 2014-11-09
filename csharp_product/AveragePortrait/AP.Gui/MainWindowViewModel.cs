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

        public IList<FaceViewModel> Faces { get; set; }

        public ICommand OpenPhotosCommand { get; set; }

        public ICommand PrepareAveragePortraitCommand { get; set; }

        public MainWindowViewModel()
        {
            OpenPhotosCommand = new RelayCommand(OpenPhotos);
            PrepareAveragePortraitCommand = new RelayCommand(PrepareAveragePortrait);
        }

        public void MakeAveragePortrait(string[] images, bool cropFace = false)
        {
            Faces = new List<FaceViewModel>();
            var averageFace = new AverageFace(600, 600);

            var faceProcessor = new FaceProcessor();
            var standardEyes = new List<Eye>
                {
                    new Eye {X = 200, Y = 200},
                    new Eye {X = 250, Y = 300}
                };
            var faces = images.Select(image => new Face(faceProcessor, image, standardEyes, cropFace)).ToList();
            foreach (var face in faces)
            {
                averageFace.Add(face.FaceBitmap);
            }
            averageFace.MakeAverage();
            AverageFaceResult = BitmapSourceConvert.ToBitmapSource(averageFace.Result);
            foreach (var face in faces)
            {
                Faces.Add(new FaceViewModel(face));
            }
            RaisePropertyChanged("AverageFaceResult");
            RaisePropertyChanged("Faces");
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
            Faces = new List<FaceViewModel>();
            
            var faceProcessor = new FaceProcessor();
            
            var standardEyes = new List<Eye>
                {
                    new Eye {X = 200, Y = 200},
                    new Eye {X = 250, Y = 300}
                };
            var faces = images.Select(image => new Face(faceProcessor, image, standardEyes, cropFace)).ToList();
            
            foreach (var face in faces)
            {
                Faces.Add(new FaceViewModel(face));
            }

            RaisePropertyChanged("Faces");
        }

        private void PrepareAveragePortrait()
        {
            var averageFace = new AverageFace(600, 600);
            foreach (var face in Faces)
            {
                averageFace.Add(face.Face.FaceBitmap);
            }
            averageFace.MakeAverage();
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
