using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Emgu.CV;

namespace AP.Logic
{
    public class Transformation
    {
        public static Transformation Construct(Eye leftEye = null, Eye rightEye = null, ICollection<Eye> standardEyes = null, double width = 0)
        {
            var eyes = new Collection<Eye>
            {
                leftEye,
                rightEye
            };

            var transformation = new Transformation();
            var sortedEyes = eyes.OrderBy(_ => _.X).ToList();
            var eyeVector = sortedEyes[0] - sortedEyes[1];
            if (eyeVector.X != 0)
            {
                transformation.Angle = Math.Atan(eyeVector.Y / eyeVector.X);
                transformation.Center = new PointF{X = sortedEyes[0].X, Y = sortedEyes[0].Y};
            }
            if (standardEyes != null)
            {
                var sortedStandardEyes = standardEyes.OrderBy(_ => _.X).ToList();
                var standardEyesVector = sortedStandardEyes[1] - sortedStandardEyes[0];
                width = Hypot(standardEyesVector.X, standardEyesVector.Y);
                transformation.Translation = (sortedStandardEyes[0] - sortedEyes[0]).ToPoint();
            }
            if (width > 0)
            {
                transformation.Scale = width/Hypot(eyeVector.X, eyeVector.Y);
            }
            
            return transformation;
        }

        private static double Hypot(double x, double y)
        {
            return Math.Sqrt(x*x + y*y);
        }

        public double Angle { get; set; }

        public double AngleInDegrees
        {
            get { return (180 * Angle)/Math.PI; }
        }

        public double Scale { get; set; }
        public PointF Center { get; set; }
        public PointF Translation { get; set; }


        public Matrix<T> AsMatrix<T>() where T : struct
        {
            var matrix = new Emgu.CV.RotationMatrix2D<T>(Center, AngleInDegrees, Scale);
            return matrix;
        }
    }
}