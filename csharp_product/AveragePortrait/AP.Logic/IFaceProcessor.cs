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
}