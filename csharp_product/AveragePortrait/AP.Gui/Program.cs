//----------------------------------------------------------------------------
//  Copyright (C) 2004-2013 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.GPU;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace FaceDetection
{
    public static class Program
    {
        public static Image<Bgr, Byte> Run()
        {
            var image =
                new Image<Bgr, byte>(
                    @"D:\Projects\Face Recognition\averageportrait\csharp_product\AveragePortrait\AP.Gui\lena.jpg");
                //Read the files as an 8-bit Bgr image  
            long detectionTime;
            var faces = new List<Rectangle>();
            var eyes = new List<Rectangle>();
            DetectFace.Detect(image, "haarcascade_frontalface_default.xml", "haarcascade_eye.xml", faces, eyes,
                out detectionTime);
            foreach (Rectangle face in faces)
                image.Draw(face, new Bgr(Color.Red), 2);
            foreach (Rectangle eye in eyes)
                image.Draw(eye, new Bgr(Color.Blue), 2);
            return image;
        }
    }
}