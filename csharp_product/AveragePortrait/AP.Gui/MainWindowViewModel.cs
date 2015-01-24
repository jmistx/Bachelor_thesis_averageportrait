using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AP.Logic;
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
        public int Width {get { return 600; }}
        public int Height {get { return 600; }}
        
        
        public BitmapSource AverageFaceResult { get; set; }

        public FaceViewModel CurrentFaceViewModel { get; set; }

        public List<Face> Faces { get; set; }

        public IList<FaceViewModel> FacesViewModel { get; set; }

        public ICommand OpenPhotosCommand { get; set; }

        public ICommand PrepareAveragePortraitCommand { get; set; }

        public ICommand SaveResultCommand { get; set; }

        private Eye _leftStandardEye = new Eye { X = 250, Y = 250 };
        private Eye _rightStandardEye = new Eye { X = 350, Y = 250 };

        public float StandardEyeSize { get { return 40; }}
        public float LeftStandardEyeX { get { return _leftStandardEye.X - StandardEyeSize / 2; } }
        public float LeftStandardEyeY { get { return _leftStandardEye.Y - StandardEyeSize / 2; } }
        public float RightStandardEyeX { get { return _rightStandardEye.X - StandardEyeSize / 2; } }
        public float RightStandardEyeY { get { return _rightStandardEye.Y - StandardEyeSize / 2; } }

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

            IFaceProcessor faceProcessor = new FaceProcessor();
            Faces = images.Select(image => new Face(faceProcessor, image, cropFace)).ToList();
            
            foreach (var face in Faces)
            {
                FacesViewModel.Add(new FaceViewModel(face));
            }

            RaisePropertyChanged("FacesViewModel");
        }

        private void PrepareAveragePortrait()
        {
            var averageFace = new PureAverageFace(Width, Height);
            var standardEyes = new List<Eye>
                {
                    _leftStandardEye,
                    _rightStandardEye
                };
            averageFace.MakeAverage(Faces, standardEyes);
            AverageFaceResult = BitmapSourceConvert.ToBitmapSource(averageFace.ResultBitmap);
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

        public void SetLeftStandardEye(Point position)
        {
            _leftStandardEye.X = (float) position.X;
            _leftStandardEye.Y = (float) position.Y;
            _rightStandardEye.Y = (float) position.Y;
            RaisePropertyChanged("LeftStandardEyeX");
            RaisePropertyChanged("LeftStandardEyeY");
            RaisePropertyChanged("RightStandardEyeX");
            RaisePropertyChanged("RightStandardEyeY");
        }

        public void SetRightStandardEye(Point position)
        {
            _rightStandardEye.X = (float)position.X;
            _rightStandardEye.Y = (float)position.Y;
            _leftStandardEye.Y = (float)position.Y;
            RaisePropertyChanged("LeftStandardEyeX");
            RaisePropertyChanged("LeftStandardEyeY");
            RaisePropertyChanged("RightStandardEyeX");
            RaisePropertyChanged("RightStandardEyeY");
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
            Picture = BitmapSourceConvert.ToBitmapSource(face.OriginalBitmap);
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
