using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using AP.Logic;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.TeamFoundation.MVVM;
using System.Windows.Media.Imaging;
using Emgu.CV.WPF;

namespace AP.Gui
{
    public class MainWindowViewModel : ViewModelBase
    {
        public BitmapSource AverageFaceResult { get; set; }

        public IList<FaceViewModel> Faces { get; set; }

        public void MakeAveragePortrait()
        {
            Faces = new List<FaceViewModel>();
            CreateAveragePortrait();
            RaisePropertyChanged("AverageFaceResult");
            RaisePropertyChanged("Faces");
        }

        private void CreateAveragePortrait(bool cropFace = false)
        {
            var averageFace = new AverageFace(600, 600);
            var images = Directory.GetFiles("images", "*.jpg");
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
