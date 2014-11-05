using System;
using NUnit.Framework;

namespace AP.Test
{
    public class Expect
    {
        public static void FloatEquals(double expected, double actual)
        {
            const double tolerance = 0.0000001;
            if (Math.Abs(expected - actual) > tolerance)
            {
                throw new AssertionException(string.Format("expected: {0} actual: {1}", expected, actual));
            }
        }
    }
}