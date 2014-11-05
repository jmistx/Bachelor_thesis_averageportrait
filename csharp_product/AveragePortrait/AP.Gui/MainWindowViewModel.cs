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

        public IList<BitmapSource> Thumbnails { get; set; }

        public void MakeAveragePortrait()
        {
            Thumbnails = new List<BitmapSource>();
            CreateAveragePortrait();
            RaisePropertyChanged("AverageFaceResult");
            RaisePropertyChanged("Thumbnails");
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
                Thumbnails.Add(BitmapSourceConvert.ToBitmapSource(face.Thumbnail));
            }
        }
    }
}
