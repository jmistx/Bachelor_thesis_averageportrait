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
        Bitmap FaceBitmap { get; set; }
        List<Eye> Eyes { get; set; }
    }

    public class Face : IFace
    {
        public Bitmap PrepareThumbnail(Bitmap original)
        {
            double aspect = original.Height / (double)OriginalBitmap.Width;
            return new Bitmap(OriginalBitmap, 200, (int)(aspect * 200));
        }
        public Face(IFaceProcessor faceProcessor, string image, bool cropFace)
        {
            OriginalBitmap = new Bitmap(image);

            Thumbnail = PrepareThumbnail(OriginalBitmap);

            LeftEye = new Eye();
            RightEye = new Eye();
        }

        public Eye LeftEye { get; set; }
        public Eye RightEye { get; set; }
        public Bitmap OriginalBitmap { get; set; }
        public Bitmap Thumbnail { get; set; }
        public Bitmap FaceBitmap { get; set; }
        public Image<Bgr, byte> FaceBitmap_ { get; set; }
        public Transformation Transformation { get; set; }
        public List<Eye> Eyes { get; set; }
    }
}