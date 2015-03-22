using System.Collections.Generic;
using System.Drawing;

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
        public Face(string image)
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
        public List<Eye> Eyes { get; set; }
    }
}