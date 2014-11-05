using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Logic;
using NUnit.Framework;

namespace AP.Test
{
    [TestFixture]
    public class EyesShould 
    {
        [Test]
        public void HaveSimpleOperations()
        {
            var left = new Eye {X = 100, Y = 50};
            var right= new Eye {X = 150, Y = 60};

            var eyeVector = right - left;
            Assert.AreEqual(150 - 100, eyeVector.X);
            Assert.AreEqual(60 - 50, eyeVector.Y);

            var eyeSum = left + right;
            Assert.AreEqual(100 + 150, eyeSum.X);
            Assert.AreEqual(50 + 60, eyeSum.Y);
        }
    }
}
