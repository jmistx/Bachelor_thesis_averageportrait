using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace AP.Logic
{
    public static class Program
    {
        public static Image<Bgr, int> MakeAveragePortrait(bool cropFace = true)
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
                
                var realEyes = new List<Eye>();
                
                foreach (var eye in eyes)
                {
                    realEyes.Add(new Eye
                    {
                        X = (float) (eye.X + eye.Width/2.0),
                        Y = (float) (eye.Y + eye.Height/2.0)
                    });
                }
                foreach (var eye in realEyes)
                    faceImage.Draw(new Rectangle((int) (eye.X - 10), (int) (eye.Y - 10), 20, 20), new Bgr(Color.Red), 2);

                var standardEyes = new List<Eye>
                {
                    new Eye {X = 100, Y = 200},
                    new Eye {X = 200, Y = 200}
                };
                var transform = Transformation.Construct(realEyes, standardEyes);
                var rotationMatrix = transform.AsMatrix<float>();
                faceImage = faceImage.WarpAffine(rotationMatrix, 600, 600, INTER.CV_INTER_CUBIC, WARP.CV_WARP_DEFAULT, new Bgr(Color.Black));
                var translationMatrix = new Matrix<float>(new float[,]
                {
                    {1, 0, transform.Translation.X},
                    {0, 1, transform.Translation.Y}
                });
                faceImage = faceImage.WarpAffine(translationMatrix, 600, 600, INTER.CV_INTER_CUBIC, WARP.CV_WARP_DEFAULT, new Bgr(Color.Black));
                faceImage = faceProcessor.IncreaseImageSize(faceImage, 600, 600);

                averageFace.Add(faceImage);
            }

            averageFace.MakeAverage();

            return averageFace.Result;
        }
    }
}