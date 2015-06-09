using System.Drawing;

namespace AP.Logic
{
    public class Eye
    {
        public float X { get; set; }
        public float Y { get; set; }

        public override string ToString()
        {
            return string.Format("{{X = {0}}} Y = {1}}}", X, Y);
        }
    }
}
