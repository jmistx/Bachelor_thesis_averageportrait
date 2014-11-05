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
        public Face(FaceProcessor faceProcessor, string imagePath, List<Eye> standardEyes, bool cropFace)
        {
            Eyes = new List<Eye>();
            OriginalBitmap = new Image<Bgr, byte>(imagePath);
            Thumbnail = OriginalBitmap.Resize(200, 200, INTER.CV_INTER_LINEAR, preserveScale: true);

            var faces = faceProcessor.GetFaces(OriginalBitmap);
            var face = faces.Single();
            if (cropFace)
            {
                FaceBitmap = faceProcessor.GetRectFromImage(OriginalBitmap, face);
                face = Rectangle.Empty;
            }
            else
            {
                FaceBitmap = OriginalBitmap;
            }
            var eyes = faceProcessor.GetEyes(FaceBitmap, face);

            foreach (var eye in eyes)
            {
                Eyes.Add(new Eye
                {
                    X = (float)(eye.X + eye.Width / 2.0),
                    Y = (float)(eye.Y + eye.Height / 2.0)
                });
            }

            foreach (var eye in Eyes)
                FaceBitmap.Draw(new Rectangle((int)(eye.X - 10), (int)(eye.Y - 10), 20, 20), new Bgr(Color.Red), 2);

            Transformation = Transformation.Construct(Eyes, standardEyes);
            var rotationMatrix = Transformation.AsMatrix<float>();
            FaceBitmap = FaceBitmap.WarpAffine(rotationMatrix, 600, 600, INTER.CV_INTER_CUBIC, WARP.CV_WARP_DEFAULT, new Bgr(Color.Black));
            var translationMatrix = new Matrix<float>(new float[,]
                {
                    {1, 0, Transformation.Translation.X},
                    {0, 1, Transformation.Translation.Y}
                });
            FaceBitmap = FaceBitmap.WarpAffine(translationMatrix, 600, 600, INTER.CV_INTER_CUBIC, WARP.CV_WARP_DEFAULT, new Bgr(Color.Black));
            FaceBitmap = faceProcessor.IncreaseImageSize(FaceBitmap, 600, 600);

        }

        public Image<Bgr, byte> OriginalBitmap { get; set; }
        public Image<Bgr, byte> Thumbnail { get; set; }
        public Image<Bgr, byte> FaceBitmap { get; set; }
        public Transformation Transformation { get; set; }
        public List<Eye> Eyes { get; set; }
    }
}