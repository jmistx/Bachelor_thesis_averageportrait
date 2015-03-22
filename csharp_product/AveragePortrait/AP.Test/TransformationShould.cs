using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using AP.Logic;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AP.Test
{
    [TestFixture]
    public class TransformationShould
    {
        [Test]
        public void CanAlignEyes()
        {
            var transformation = Transformation.Construct(leftEye: new Eye { X = 0, Y = 0 }, rightEye: new Eye { X = 5, Y = 5 });
            Expect.FloatEquals(Math.PI/4.0, transformation.Angle);
        }

        [Test]
        public void CanAlignEyesToNegativeAngle()
        {
            var transformation = Transformation.Construct(leftEye: new Eye { X = 0, Y = 0 }, rightEye: new Eye { X = 3, Y = -3 });
            Expect.FloatEquals(-Math.PI / 4.0, transformation.Angle);
        }

        [Test]
        public void CanScaleEyes()
        {
            var transformation = Transformation.Construct(leftEye: new Eye { X = 0, Y = 0 }, rightEye: new Eye { X = 0, Y = 4 }, width: 8);
            Expect.FloatEquals(2, transformation.Scale);
        }

        [Test]
        public void CanDoBothRotateAndScale()
        {
            var leftEye = new Eye {X = 2, Y = 2};
            var rightEye = new Eye {X = (float) (2 + 4*Math.Cos(Math.PI/6)), Y = (float) (2 + 4*Math.Sin(Math.PI/6))};

            var transformation = Transformation.Construct(leftEye: leftEye, rightEye: rightEye, width: 10);

            Expect.FloatEquals(Math.PI/6, transformation.Angle);
            Expect.FloatEquals(2.5, transformation.Scale);
            Assert.AreEqual(new PointF(2, 2), transformation.Center);
        }

        [Test]
        public void CanTranslate()
        {
            var leftEye = new Eye{X = 0, Y = 0};
            var rightEye = new Eye{X = 0, Y = 4};

            var standardEyes = new List<Eye>
            {
                new Eye{X = 3, Y = 5},
                new Eye{X = 3, Y = 7}
            };
            var transformation = Transformation.Construct(leftEye: leftEye, rightEye: rightEye, standardEyes: standardEyes);
            Expect.FloatEquals(0.5, transformation.Scale);
            Assert.AreEqual(new PointF(3, 5), transformation.Translation);
        }

        [Test]
        public void BeInvariantOfEyeOrder()
        {
            var leftEye = new Eye{X = 0, Y = 0};
            var rightEye = new Eye{X = 3, Y = 4};

            var standardEyes = new List<Eye>
            {
                new Eye{X = 3, Y = 5},
                new Eye{X = 3, Y = 7}
            };
            var transformation = Transformation.Construct(leftEye: leftEye, rightEye: rightEye, standardEyes: standardEyes);
            var sameTransformation = Transformation.Construct(leftEye: rightEye, rightEye: leftEye, standardEyes: standardEyes);

            Expect.FloatEquals(transformation.Scale, sameTransformation.Scale);
            Assert.AreEqual(transformation.Translation, sameTransformation.Translation);
        }
    }
}