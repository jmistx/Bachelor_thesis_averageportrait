using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace AP.Logic
{
    public class FaceProcessor
    {
        public CascadeClassifier Face { get; set; }
        public FaceProcessor()
        {
            const string faceCascade = "haarcascade_frontalface_alt2.xml";
            Face = new CascadeClassifier(faceCascade);
        }

        public IList<Rectangle> GetFaces(Image<Bgr, Byte> image)
        {
            var faces = new List<Rectangle>();

            using (var gray = image.Convert<Gray, Byte>())
            {
                gray._EqualizeHist();
                var facesDetected = Face.DetectMultiScale(
                            gray,
                            1.3,
                            5,
                            new Size(20, 20),
                            Size.Empty);
                faces.AddRange(facesDetected);
            }

            return faces;
        }

        public Image<Bgr, Byte> GetRectFromImage(Image<Bgr, Byte> image, Rectangle rectangle)
        {
            return image.GetSubRect(rectangle);
        }

        public Image<Bgr, Byte> ScaleImages(Image<Bgr, Byte> image, int w, int h)
        {
            var scaledImage = new Image<Bgr, Byte>(w, h, new Bgr(Color.Black));
            if (image.Width > w || image.Height > h)
            {
                throw new SyntaxErrorException("Assertion");
            }
            var offsetX = (w - image.Width)/2;
            var offsetY = (h - image.Height)/2;

            scaledImage.ROI = new Rectangle(offsetX, offsetY, image.Width, image.Height);
            image.CopyTo(scaledImage);
            scaledImage.ROI = Rectangle.Empty;

            return scaledImage;
        }


    }
}