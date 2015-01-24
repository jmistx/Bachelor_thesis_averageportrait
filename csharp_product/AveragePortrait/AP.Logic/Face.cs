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
    public class Face
    {
        public Face(IFaceProcessor faceProcessor, string imagePath, bool cropFace)
        {
            Eyes = new List<Eye>();
            OriginalBitmap = new Image<Bgr, byte>(imagePath);
            Thumbnail = OriginalBitmap.Resize(200, 200, INTER.CV_INTER_LINEAR, preserveScale: true);

            var faces = faceProcessor.GetFaces(OriginalBitmap);
            var face = faces.FirstOrDefault();
            if (cropFace)
            {
                FaceBitmap = faceProcessor.GetRectFromImage(OriginalBitmap, face);
                face = Rectangle.Empty;
            }
            else
            {
                FaceBitmap = OriginalBitmap;
            }
            var eyes = faceProcessor.GetEyes(FaceBitmap, face).OrderBy(_ => _.X).ToList();

            foreach (var eye in eyes)
            {
                Eyes.Add(new Eye
                {
                    X = (float)(eye.X + eye.Width / 2.0),
                    Y = (float)(eye.Y + eye.Height / 2.0)
                });
            }

            LeftEye = Eyes.ElementAtOrDefault(0) ?? new Eye();
            RightEye = Eyes.ElementAtOrDefault(1) ?? new Eye();
        }
        public Eye LeftEye { get; set; }
        public Eye RightEye { get; set; } 

        public Image<Bgr, byte> OriginalBitmap { get; set; }
        public Image<Bgr, byte> Thumbnail { get; set; }
        public Image<Bgr, byte> FaceBitmap { get; set; }
        public Transformation Transformation { get; set; }
        public List<Eye> Eyes { get; set; }
    }
}