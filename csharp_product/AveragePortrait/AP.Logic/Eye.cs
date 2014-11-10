using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Features2D;

namespace AP.Logic
{
    public class Eye
    {
        public float X { get; set; }
        public float Y { get; set; }

        public static Eye operator +(Eye left, Eye right)
        {
            return new Eye  
            {
                X = left.X + right.X,
                Y = left.Y + right.Y
            };
        }

        public static Eye operator -(Eye left, Eye right)
        {
            return new Eye
            {
                X = left.X - right.X,
                Y = left.Y - right.Y
            };
        }

        public PointF ToPoint()
        {
            return new PointF(X, Y);
        }

        public override string ToString()
        {
            return string.Format("{{X = {0}}} Y = {1}}}", X, Y);
        }
    }
}
