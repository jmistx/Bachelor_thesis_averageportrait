using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace AP.Logic
{
    public interface IFace
    {
        Eye LeftEye { get; set; }
        Eye RightEye { get; set; }
        Bitmap OriginalBitmap { get; set; }
        Bitmap Thumbnail { get; set; }
        Image<Bgr, byte> FaceBitmap { get; set; }
        Transformation Transformation { get; set; }
        List<Eye> Eyes { get; set; }
    }

    public class Face : IFace
    {
        public Face(IFaceProcessor faceProcessor, string image, bool cropFace)
        {
            LeftEye = new Eye();
            RightEye = new Eye();
        }

        public Eye LeftEye { get; set; }
        public Eye RightEye { get; set; }
        public Bitmap OriginalBitmap { get; set; }
        public Bitmap Thumbnail { get; set; }
        public Image<Bgr, byte> FaceBitmap { get; set; }
        public Transformation Transformation { get; set; }
        public List<Eye> Eyes { get; set; }
    }
}