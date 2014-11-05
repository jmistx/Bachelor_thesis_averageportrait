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
            var eyes = new List<Eye>
            {
                new Eye{X = 0, Y = 0},
                new Eye{X = 5, Y = 5}
            };
            var transformation = Transformation.Construct(eyes);
            Expect.FloatEquals(Math.PI/4.0, transformation.Angle);
        }

        [Test]
        public void CanAlignEyesToNegativeAngle()
        {
            var eyes = new List<Eye>
            {
                new Eye{X = 0, Y = 0},
                new Eye{X = 3, Y = -3}
            };
            var transformation = Transformation.Construct(eyes);
            Expect.FloatEquals(-Math.PI / 4.0, transformation.Angle);
        }

        [Test]
        public void CanScaleEyes()
        {
            var eyes = new List<Eye>
            {
                new Eye{X = 0, Y = 0},
                new Eye{X = 0, Y = 4}
            };
            var transformation = Transformation.Construct(eyes, width: 8);
            Expect.FloatEquals(2, transformation.Scale);
        }

        [Test]
        public void CanDoBothRotateAndScale()
        {
            var eyes = new List<Eye>()
            {
                new Eye{X = 2, Y = 2},
                new Eye{X = (float) (2 + 4 * Math.Cos(Math.PI/6)), Y = (float) (2 + 4*Math.Sin(Math.PI/6))}
            };

            var transformation = Transformation.Construct(eyes, width: 10);

            Expect.FloatEquals(Math.PI/6, transformation.Angle);
            Expect.FloatEquals(2.5, transformation.Scale);
            Assert.AreEqual(new PointF(2, 2), transformation.Center);
        }

        [Test]
        public void CanTranslate()
        {
            var eyes = new List<Eye>
            {
                new Eye{X = 0, Y = 0},
                new Eye{X = 0, Y = 4}
            };

            var standardEyes = new List<Eye>
            {
                new Eye{X = 3, Y = 5},
                new Eye{X = 3, Y = 7}
            };
            var transformation = Transformation.Construct(eyes, standardEyes: standardEyes);
            Expect.FloatEquals(0.5, transformation.Scale);
            Assert.AreEqual(new PointF(3, 5), transformation.Translation);
        }

        [Test]
        public void BeInvariantOfEyeOrder()
        {
            var eyes = new List<Eye>
            {
                new Eye{X = 0, Y = 0},
                new Eye{X = 3, Y = 4}
            };

            var standardEyes = new List<Eye>
            {
                new Eye{X = 3, Y = 5},
                new Eye{X = 3, Y = 7}
            };
            var transformation = Transformation.Construct(eyes, standardEyes: standardEyes);

            eyes = new List<Eye>
            {
                new Eye{X = 3, Y = 4},
                new Eye{X = 0, Y = 0}
            };
            var sameTransformation = Transformation.Construct(eyes, standardEyes: standardEyes);

            Expect.FloatEquals(transformation.Scale, sameTransformation.Scale);
            Assert.AreEqual(transformation.Translation, sameTransformation.Translation);
        }

        [Test]
        public void CanBeCastedToMatrix()
        {
            var eyes = new List<Eye>
            {
                new Eye{X = 3, Y = 4},
                new Eye{X = (float) (3 + 6*Math.Cos(Math.PI/6)), Y = (float) (4 + 6 * Math.Sin(Math.PI/6))}
            };

            var standardEyes = new List<Eye>
            {
                new Eye{X = 3, Y = 2},
                new Eye{X = 9, Y = 2}
            };

            var transformation = Transformation.Construct(eyes, standardEyes: standardEyes);
            transformation.AsMatrix<float>();
            //Assert???!!!
        }
    }
}