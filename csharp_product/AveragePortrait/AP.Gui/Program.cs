using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms.VisualStyles;
using AP.Logic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace AP.Gui
{
    public class AverageFace
    {
        private int _count;
        public Image<Bgr, Int32> Result { get; private set; }
        public AverageFace(int width, int height)
        {
            Result = new Image<Bgr, Int32>(width, height);
            _count = 0;
        }

        public void Add(Image<Bgr, Byte> image)
        {
            _count++;

            var output = Result.Data;
            var input = image.Data;

            for (var i = 0; i < Result.Height; i++)
            {
                for (var j = 0; j < Result.Width; j++)
                {
                    output[i, j, 0] += input[i, j, 0];
                    output[i, j, 1] += input[i, j, 1];
                    output[i, j, 2] += input[i, j, 2];
                }
            } 
        }

        public void MakeAverage()
        {
            if (_count == 0) return;

            var resultData = Result.Data;
            var height = Result.Height;
            var width = Result.Width;

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    resultData[i, j, 0] /= _count;
                    resultData[i, j, 1] /= _count;
                    resultData[i, j, 2] /= _count;
                }
            }

            _count = 1;
        }
    }
    public static class Program
    {
        public static Image<Bgr, Int32> Run()
        {
            var averageFace = new AverageFace(600, 700);
            var faceProcessor = new FaceProcessor();

            var images = Directory.GetFiles("images", "*.jpg");

            foreach (var imagePath in images)
            {
                var image = new Image<Bgr, byte>(imagePath);

                var faces = new List<Rectangle>();
                var eyes = new List<Rectangle>();
                

                image = faceProcessor.IncreaseImageSize(image, 600, 700);
                DetectFace.Detect(image, "haarcascade_frontalface_alt2.xml", "haarcascade_eye.xml", faces, eyes);
                foreach (Rectangle face in faces)
                    image.Draw(face, new Bgr(Color.Red), 2);
                foreach (Rectangle eye in eyes)
                    image.Draw(eye, new Bgr(Color.Blue), 2);

                averageFace.Add(image);
            }

            averageFace.MakeAverage();

            return averageFace.Result;
        }
    }
}