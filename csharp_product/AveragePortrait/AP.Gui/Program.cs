using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using AP.Logic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace AP.Gui
{
    public static class Program
    {
        public static Image<Bgr, Int32> Run()
        {
            var averageFace = new AverageFace(600, 600);
            var faceProcessor = new FaceProcessor();

            var images = Directory.GetFiles("images", "*.jpg");

            foreach (var imagePath in images)
            {
                var image = new Image<Bgr, byte>(imagePath);

                IList<Rectangle> faces;// = new List<Rectangle>();
                //var eyes = new List<Rectangle>();

                faces = faceProcessor.GetFaces(image);
                var face = faces.Single();
                var faceImage = faceProcessor.GetRectFromImage(image, face);

                var eyes = faceProcessor.GetEyes(faceImage);

                //DetectFace.Detect(image, "haarcascade_frontalface_alt2.xml", "haarcascade_eye.xml", faces, eyes);
                foreach (Rectangle eye in eyes)
                    faceImage.Draw(eye, new Bgr(Color.Red), 2);

                faceImage = faceProcessor.IncreaseImageSize(faceImage, 600, 600);

                averageFace.Add(faceImage);
            }

            averageFace.MakeAverage();

            return averageFace.Result;
        }
    }
}