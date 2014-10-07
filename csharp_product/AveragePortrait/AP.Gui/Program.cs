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
        public static Image<Bgr, Int32> MakeAveragePortrait(bool cropFace = true)
        {
            var averageFace = new AverageFace(600, 600);
            var faceProcessor = new FaceProcessor();

            var images = Directory.GetFiles("images", "*.jpg");

            foreach (var imagePath in images)
            {
                var image = new Image<Bgr, byte>(imagePath);

                IList<Rectangle> faces = faceProcessor.GetFaces(image);
                var face = faces.Single();

                Image<Bgr, byte> faceImage = null;

                if (cropFace)
                {
                    faceImage = faceProcessor.GetRectFromImage(image, face);
                    face = Rectangle.Empty;
                }
                else
                {
                    faceImage = image;
                }

                var eyes = faceProcessor.GetEyes(faceImage, face);

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