using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace AP.Logic
{
    public interface IFaceProcessor
    {
        IList<Rectangle> GetEyes(Image<Bgr, byte> image, Rectangle face);
        IList<Rectangle> GetFaces(Image<Bgr, Byte> image);
        Image<Bgr, Byte> GetRectFromImage(Image<Bgr, Byte> image, Rectangle rectangle);
    }

    public class FaceProcessor : IFaceProcessor
    {
        public IList<Rectangle> GetEyes(Image<Bgr, byte> image, Rectangle face)
        {
            return new List<Rectangle>();
        }

        public IList<Rectangle> GetFaces(Image<Bgr, byte> image)
        {
            return new List<Rectangle>();
        }

        public Image<Bgr, byte> GetRectFromImage(Image<Bgr, byte> image, Rectangle rectangle)
        {
            return image;
        }
    }
}