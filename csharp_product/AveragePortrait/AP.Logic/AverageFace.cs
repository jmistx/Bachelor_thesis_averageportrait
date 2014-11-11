using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace AP.Logic
{
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
                face.FaceBitmap.Draw(new Rectangle((int)(eye.X - 10), (int)(eye.Y - 10), 20, 20), new Bgr(Color.Red), 2);
        }

        public void MakeAverage(IEnumerable<Face> faces, IList<Eye> standardEyes, bool drawEyes = false)
        {
            foreach (var face in faces)
            {
                var transformation = Transformation.Construct(leftEye: face.LeftEye, rightEye: face.RightEye, standardEyes: standardEyes);
                var rotationMatrix = transformation.AsMatrix<float>();
                
                if (drawEyes) DrawEyes(face);

                var faceBitmap = face.FaceBitmap.WarpAffine(rotationMatrix, face.FaceBitmap.Width, face.FaceBitmap.Height, INTER.CV_INTER_CUBIC, WARP.CV_WARP_DEFAULT, new Bgr(Color.Black));
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
    }
}