using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace AP.Logic
{
    public interface IAverageFace
    {
        void MakeAverage(IEnumerable<Face> faces, IList<Eye> standardEyes, bool drawEyes = false);
        Bitmap ResultBitmap { get; set; }
    }

    public class PureAverageFace : IAverageFace
    {
        private readonly int _width;
        private readonly int _height;

        public PureAverageFace(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public Bitmap ResultBitmap { get; set; }
        public void MakeAverage(IEnumerable<Face> faces, IList<Eye> standardEyes, bool drawEyes = false)
        {
            var face = faces.First();
            var bitmap = new Bitmap(_width, _height);
            //transform
            using (var g = Graphics.FromImage(bitmap))
            {
                var matrix = new Matrix();
                
                float scale = (standardEyes[1].X - standardEyes[0].X) / (face.RightEye.X - face.LeftEye.X);
                matrix.Translate((standardEyes[0].X - face.LeftEye.X * scale), (standardEyes[0].Y - face.LeftEye.Y * scale));
                matrix.Scale(scale, scale);
                matrix.Translate(face.LeftEye.X, face.LeftEye.Y);
                double x = face.RightEye.X - face.LeftEye.X;
                double y = face.RightEye.Y - face.LeftEye.Y;
                float angle =  (float) ((float) (180.0F*Math.Atan(y/x))/Math.PI);
                matrix.Rotate(-angle);
                matrix.Translate(-face.LeftEye.X, -face.LeftEye.Y);
                g.Transform = matrix;
                g.DrawImage(face.OriginalBitmap, 0 ,0);
            }
            
            ResultBitmap = bitmap;
        }
    }

    public class AverageFace 
    {
        private readonly int _width;
        private readonly int _height;
        private int _count;
        public Image<Bgr, int> Result { get; private set; }

        public AverageFace(int width, int height)
        {
            _width = width;
            _height = height;
            Result = new Image<Bgr, int>(_width, _height);
            _count = 0;
        }

        private void DrawEyes(Face face)
        {
            foreach (var eye in face.Eyes)
                face.FaceBitmap_.Draw(new Rectangle((int)(eye.X - 10), (int)(eye.Y - 10), 20, 20), new Bgr(Color.Red), 2);
        }

        public void MakeAverage(IEnumerable<Face> faces, IList<Eye> standardEyes, bool drawEyes = false)
        {
            foreach (var face in faces)
            {
                var transformation = Transformation.Construct(leftEye: face.LeftEye, rightEye: face.RightEye, standardEyes: standardEyes);
                var rotationMatrix = transformation.AsMatrix<float>();
                
                if (drawEyes) DrawEyes(face);

                var faceBitmap = face.FaceBitmap_.WarpAffine(rotationMatrix, face.FaceBitmap.Width, face.FaceBitmap.Height, INTER.CV_INTER_CUBIC, WARP.CV_WARP_DEFAULT, new Bgr(Color.Black));
                var translationMatrix = new Matrix<float>(new float[,]
                {
                    {1, 0, transformation.Translation.X},
                    {0, 1, transformation.Translation.Y}
                });
                faceBitmap = faceBitmap.WarpAffine(translationMatrix, _width, _height, INTER.CV_INTER_CUBIC, WARP.CV_WARP_DEFAULT, new Bgr(Color.Black));
                Add(faceBitmap);
            }
        }

        public void Add(Image<Bgr, byte> image)
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

        public Bitmap ResultBitmap { get; set; }
    }
}