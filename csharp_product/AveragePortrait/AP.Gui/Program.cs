using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AP.Logic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace AP.Gui
{
    public static class Program
    {
        public static Image<Bgr, Byte> Run()
        {
            var images = Directory.GetFiles("images", "*.jpg");
            var image = new Image<Bgr, byte>(images[0]);
            long detectionTime;
            var faces = new List<Rectangle>();
            var eyes = new List<Rectangle>();
            var faceProcessor = new FaceProcessor();
            image = faceProcessor.ScaleImages(image, 600, 600);
            DetectFace.Detect(image, "haarcascade_frontalface_alt2.xml", "haarcascade_eye.xml", faces, eyes,
                out detectionTime);
            foreach (Rectangle face in faces)
                image.Draw(face, new Bgr(Color.Red), 2);
            foreach (Rectangle eye in eyes)
                image.Draw(eye, new Bgr(Color.Blue), 2);
            return image;
        }
    }
}