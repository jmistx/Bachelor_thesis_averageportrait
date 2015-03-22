using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace AP.Logic
{
    public interface IAverageFace
    {
        void MakeAverage(IList<Face> faces, IList<Eye> standardEyes, bool drawEyes = false);
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
        public void MakeAverage(IList<Face> faces, IList<Eye> standardEyes, bool drawEyes = false)
        {
            var averageFace = new int[_width * 3, _height];
            //var face = faces.First();
            var bitmap = new Bitmap(_width, _height);
            foreach (var face in faces)
            {
                var matrix = GetMatrixTransformToStandardEyes(standardEyes, face);
                face.OriginalBitmap.SetResolution(96F, 96F);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Transform = matrix;
                    g.DrawImage(face.OriginalBitmap, 0, 0);
                    AddBitmapToAverageFace(bitmap, averageFace);  
                    g.Clear(Color.Black);
                }
                
            }
            ResultBitmap = MakeAverageFaceBitmap(averageFace, faces.Count);
        }

        private unsafe Bitmap MakeAverageFaceBitmap(int[,] averageFace, int facesCount)
        {
            var averageFaceBitmap = new Bitmap(_width, _height);

            BitmapData abmData =
                averageFaceBitmap.LockBits(new Rectangle(0, 0, averageFaceBitmap.Width, averageFaceBitmap.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride1 = abmData.Stride;
            System.IntPtr Scan01 = abmData.Scan0;
            unsafe
            {
                byte* p = (byte*) (void*) Scan01;
                int nOffset = stride1 - averageFaceBitmap.Width*3;
                int nWidth = averageFaceBitmap.Width*3;
                for (int y = 0; y < averageFaceBitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        p[0] = (byte) (averageFace[x, y]/facesCount);
                        ++p;
                    }
                    p += nOffset;
                }
            }
            averageFaceBitmap.UnlockBits(abmData);
            return averageFaceBitmap;
        }

        private static unsafe void AddBitmapToAverageFace(Bitmap bitmap, int[,] averageFace)
        {
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*) (void*) Scan0;
                int nOffset = stride - bitmap.Width*3;
                int nWidth = bitmap.Width*3;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        averageFace[x, y] += p[0];
                        ++p;
                    }
                    p += nOffset;
                }
            }
            bitmap.UnlockBits(bmData);
        }

        private static Matrix GetMatrixTransformToStandardEyes(IList<Eye> standardEyes, Face face)
        {
            var matrix = new Matrix();

            float scale = (standardEyes[1].X - standardEyes[0].X)/(face.RightEye.X - face.LeftEye.X);
            matrix.Translate((standardEyes[0].X - face.LeftEye.X*scale), (standardEyes[0].Y - face.LeftEye.Y*scale));
            matrix.Scale(scale, scale);
            matrix.Translate(face.LeftEye.X, face.LeftEye.Y);
            double x = face.RightEye.X - face.LeftEye.X;
            double y = face.RightEye.Y - face.LeftEye.Y;
            float angle = (float) ((float) (180.0F*Math.Atan(y/x))/Math.PI);
            matrix.Rotate(-angle);
            matrix.Translate(-face.LeftEye.X, -face.LeftEye.Y);
            return matrix;
        }
    }
}