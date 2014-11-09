using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using AP.Logic;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.TeamFoundation.MVVM;
using System.Windows.Media.Imaging;
using Emgu.CV.WPF;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Point = System.Windows.Point;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace AP.Gui
{
    public class MainWindowViewModel : ViewModelBase
    {
        public BitmapSource AverageFaceResult { get; set; }

        public FaceViewModel CurrentFaceViewModel { get; set; }

        public List<Face> Faces { get; set; }

        public IList<FaceViewModel> FacesViewModel { get; set; }

        public ICommand OpenPhotosCommand { get; set; }

        public ICommand PrepareAveragePortraitCommand { get; set; }

        public ICommand SaveResultCommand { get; set; }

        public MainWindowViewModel()
        {
            OpenPhotosCommand = new RelayCommand(OpenPhotos);
            PrepareAveragePortraitCommand = new RelayCommand(PrepareAveragePortrait);
            SaveResultCommand = new RelayCommand(SaveResult);
        }

        private void SaveResult()
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".png";
            saveDialog.Filter = "PNG Images|*.png";
            if (saveDialog.ShowDialog() == false)
            {
                return;
            }
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(AverageFaceResult));
            var stream = saveDialog.OpenFile();
            pngEncoder.Save(stream);
            stream.Close();
        }

        private void OpenPhotos()
        {
            var openFileDialog = new OpenFileDialog {Multiselect = true};
            openFileDialog.ShowDialog();
            var images = openFileDialog.FileNames;

            LoadFaces(images);
        }

        private void LoadFaces(IEnumerable<string> images, bool cropFace = false)
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
            var averageFace = new AverageFace(600, 600);
            var standardEyes = new List<Eye>
                {
                    new Eye {X = 200, Y = 200},
                    new Eye {X = 250, Y = 300}
                };
            averageFace.MakeAverage(Faces, standardEyes);
            AverageFaceResult = BitmapSourceConvert.ToBitmapSource(averageFace.Result);
            RaisePropertyChanged("AverageFaceResult");
        }

        public void SelectFaceForEdit(FaceViewModel face)
        {
            CurrentFaceViewModel = face;
            RaisePropertyChanged("CurrentFaceViewModel");
        }

        public void SetCurrentFaceLeftEye(Point position)
        {
            CurrentFaceViewModel.SetLeftEye(position);
        }

        public void SetCurrentFaceRightEye(Point position)
        {
            CurrentFaceViewModel.SetRightEye(position);
        }
    }

    public class FaceViewModel : ViewModelBase
    {
        public Face Face { get; set; }
        public BitmapSource Tumbnail { get; set; }
        public BitmapSource Picture { get; set; }

        public String LeftEye { get { return Face.LeftEye.ToString(); } }

        public double EyeSize { get { return 40; }}

        public double LeftEyePositionX { get { return Face.LeftEye.X - EyeSize/2; } }
        public double LeftEyePositionY { get { return Face.LeftEye.Y - EyeSize / 2; } }

        public double RightEyePositionX { get { return Face.RightEye.X - EyeSize / 2; } }
        public double RightEyePositionY { get { return Face.RightEye.Y - EyeSize / 2; } }

        public String RightEye { get { return Face.RightEye.ToString(); } }

        public void SetLeftEye(Point position)
        {
            var eye = new Eye
            {
                X = (float)position.X,
                Y = (float)position.Y
            };
            Face.LeftEye = eye;
            RaisePropertyChanged("LeftEye");
            RaisePropertyChanged("LeftEyePositionX");
            RaisePropertyChanged("LeftEyePositionY");
        }

        public FaceViewModel(Face face)
        {
            Face = face;
            Tumbnail = BitmapSourceConvert.ToBitmapSource(face.Thumbnail);
            Picture = BitmapSourceConvert.ToBitmapSource(face.FaceBitmap);
        }

        public void SetRightEye(Point position)
        {
            var eye = new Eye
            {
                X = (float)position.X,
                Y = (float)position.Y
            };
            Face.RightEye = eye;
            RaisePropertyChanged("RightEye");
            RaisePropertyChanged("RightEyePositionX");
            RaisePropertyChanged("RightEyePositionY");
        }
    }
}
