using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Security.Policy;

namespace AP.Logic
{
    public interface IAverageFace
    {
        void MakeAverage(IList<Face> faces, IList<Eye> standardEyes, bool drawEyes = false);
        Bitmap ResultBitmap { get; set; }
    }

    public class PureAverageFace : IAverageFace
    {
        protected readonly int _width;
        protected readonly int _height;

        public PureAverageFace(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public Bitmap ResultBitmap { get; set; }

        virtual protected int[,] CreateAverageFaceBuffer(int width, int height ) {
            return new int[_width*3, _height];
        }

        public void MakeAverage(IList<Face> faces, IList<Eye> standardEyes, bool drawEyes = false)
        {
            var averageFace = CreateAverageFaceBuffer(_width, _height);
            var bitmap = new Bitmap(_width, _height);
            foreach (var face in faces)
            {
                var matrix = GetMatrixTransformToStandardEyes(standardEyes, face);
                face.OriginalBitmap.SetResolution(96F, 96F);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Transform = matrix;
                    g.DrawImage(face.OriginalBitmap, 0, 0);
                    AddBitmapToAverageFace(bitmap, averageFace);  
                    g.Clear(Color.Black);
                }
                
            }
            ResultBitmap = MakeAverageFaceBitmap(averageFace, faces.Count);
        }

        protected virtual unsafe Bitmap MakeAverageFaceBitmap(int[,] averageFace, int facesCount)
        {
            var averageFaceBitmap = new Bitmap(_width, _height);

            BitmapData abmData =
                averageFaceBitmap.LockBits(new Rectangle(0, 0, averageFaceBitmap.Width, averageFaceBitmap.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride1 = abmData.Stride;
            System.IntPtr Scan01 = abmData.Scan0;
            unsafe
            {
                byte* p = (byte*) (void*) Scan01;
                int nOffset = stride1 - averageFaceBitmap.Width*3;
                int nWidth = averageFaceBitmap.Width*3;
                for (int y = 0; y < averageFaceBitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        p[0] = (byte) (averageFace[x, y]/facesCount);
                        ++p;
                    }
                    p += nOffset;
                }
            }
            averageFaceBitmap.UnlockBits(abmData);
            return averageFaceBitmap;
        }

        protected virtual unsafe void AddBitmapToAverageFace(Bitmap bitmap, int[,] averageFace)
        {
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*) (void*) Scan0;
                int nOffset = stride - bitmap.Width*3;
                int nWidth = bitmap.Width*3;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        averageFace[x, y] += p[0];
                        ++p;
                    }
                    p += nOffset;
                }
            }
            bitmap.UnlockBits(bmData);
        }

        private static Matrix GetMatrixTransformToStandardEyes(IList<Eye> standardEyes, Face face)
        {
            var matrix = new Matrix();

            float scale = (float) ((standardEyes[1].X - standardEyes[0].X)/Math.Sqrt(Math.Pow(face.RightEye.X - face.LeftEye.X, 2) + Math.Pow(face.RightEye.Y - face.LeftEye.Y, 2)));
            matrix.Translate((standardEyes[0].X - face.LeftEye.X*scale), (standardEyes[0].Y - face.LeftEye.Y*scale));
            matrix.Scale(scale, scale);
            matrix.Translate(face.LeftEye.X, face.LeftEye.Y);
            double x = face.RightEye.X - face.LeftEye.X;
            double y = face.RightEye.Y - face.LeftEye.Y;
            float angle = (float) ((float) (180.0F*Math.Atan(y/x))/Math.PI);
            matrix.Rotate(-angle);
            matrix.Translate(-face.LeftEye.X, -face.LeftEye.Y);
            return matrix;
        }
    }

    public class HsvAverageFace : PureAverageFace {
        private double precision = 10000;

        public HsvAverageFace(int width, int height) : base(width, height) {
        }

        protected override unsafe void AddBitmapToAverageFace(Bitmap bitmap, int[,] averageFace) {
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - bitmap.Width * 3;
                int nWidth = bitmap.Width * 3;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x += 3)
                    {
                        var b = p[0];
                        var g = p[1];
                        var r = p[2];
                        double h, s, v;
                        ColorToHSV(Color.FromArgb(r, g, b), out h, out s, out v);
                        averageFace[x + 0, y] += (int)(h * precision);
                        averageFace[x + 1, y] += (int)(s * precision);
                        averageFace[x + 2, y] += (int)(v * precision);
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            bitmap.UnlockBits(bmData);
        }

        protected override unsafe Bitmap MakeAverageFaceBitmap(int[,] averageFace, int facesCount) {
            var averageFaceBitmap = new Bitmap(_width, _height);

            BitmapData abmData =
                averageFaceBitmap.LockBits(new Rectangle(0, 0, averageFaceBitmap.Width, averageFaceBitmap.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride1 = abmData.Stride;
            System.IntPtr Scan01 = abmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan01;
                int nOffset = stride1 - averageFaceBitmap.Width * 3;
                int nWidth = averageFaceBitmap.Width * 3;
                for (int y = 0; y < averageFaceBitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x+=3)
                    {
                        double h = (averageFace[x + 0, y] / (double)precision) / facesCount;
                        double s = (averageFace[x + 1, y] / (double)precision) / facesCount;
                        double v = (averageFace[x + 2, y] / (double)precision) / facesCount;
                        Color color = ColorFromHSV(h, s, v);
                        p[0] = color.B;
                        p[1] = color.G;
                        p[2] = color.R;
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            averageFaceBitmap.UnlockBits(abmData);
            return averageFaceBitmap;
        }

        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }

    public class HslAverageFace : PureAverageFace
    {
        private double precision = 1000;

        public HslAverageFace(int width, int height)
            : base(width, height)
        {
        }

        protected override unsafe void AddBitmapToAverageFace(Bitmap bitmap, int[,] averageFace)
        {
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - bitmap.Width * 3;
                int nWidth = bitmap.Width * 3;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x += 3)
                    {
                        var b = p[0];
                        var g = p[1];
                        var r = p[2];
                        double h, s, l;
                        RGBtoHSL(r, g, b, out h, out s, out l);
                        averageFace[x + 0, y] += (int)(h * precision);
                        averageFace[x + 1, y] += (int)(s * precision);
                        averageFace[x + 2, y] += (int)(l * precision);
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            bitmap.UnlockBits(bmData);
        }

        protected override unsafe Bitmap MakeAverageFaceBitmap(int[,] averageFace, int facesCount)
        {
            var averageFaceBitmap = new Bitmap(_width, _height);

            BitmapData abmData =
                averageFaceBitmap.LockBits(new Rectangle(0, 0, averageFaceBitmap.Width, averageFaceBitmap.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride1 = abmData.Stride;
            System.IntPtr Scan01 = abmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan01;
                int nOffset = stride1 - averageFaceBitmap.Width * 3;
                int nWidth = averageFaceBitmap.Width * 3;
                for (int y = 0; y < averageFaceBitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x += 3)
                    {
                        double h = (averageFace[x + 0, y] / (double)precision) / facesCount;
                        double s = (averageFace[x + 1, y] / (double)precision) / facesCount;
                        double l = (averageFace[x + 2, y] / (double)precision) / facesCount;
                        Color color = HSLtoRGB(h, s, l);
                        p[0] = color.B;
                        p[1] = color.G;
                        p[2] = color.R;
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            averageFaceBitmap.UnlockBits(abmData);
            return averageFaceBitmap;
        }

        public static void RGBtoHSL(int red, int green, int blue, out double h, out double s, out double l ) {
            s = 0;
            h = 0;
            l = 0;

            // normalize red, green, blue values
            double r = (double)red / 255.0;
            double g = (double)green / 255.0;
            double b = (double)blue / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            // hue
            if (max == min)
            {
                h = 0; // undefined
            }
            else if (max == r && g >= b)
            {
                h = 60.0 * (g - b) / (max - min);
            }
            else if (max == r && g < b)
            {
                h = 60.0 * (g - b) / (max - min) + 360.0;
            }
            else if (max == g)
            {
                h = 60.0 * (b - r) / (max - min) + 120.0;
            }
            else if (max == b)
            {
                h = 60.0 * (r - g) / (max - min) + 240.0;
            }

            // luminance
            l = (max + min) / 2.0;

            // saturation
            if (l == 0 || max == min)
            {
                s = 0;
            }
            else if (0 < l && l <= 0.5)
            {
                s = (max - min) / (max + min);
            }
            else if (l > 0.5)
            {
                s = (max - min) / (2 - (max + min)); 
            }
        }

        public static Color HSLtoRGB(double h, double s, double l)
        {
            if (s == 0)
            {
                // achromatic color (gray scale)
                return Color.FromArgb(
                    Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                        l * 255.0))),
                    Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                        l * 255.0))),
                    Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                        l * 255.0)))
                    );
            }
            else
            {
                double q = (l < 0.5) ? (l * (1.0 + s)) : (l + s - (l * s));
                double p = (2.0 * l) - q;

                double Hk = h / 360.0;
                double[] T = new double[3];
                T[0] = Hk + (1.0 / 3.0);    // Tr
                T[1] = Hk;                // Tb
                T[2] = Hk - (1.0 / 3.0);    // Tg

                for (int i = 0; i < 3; i++)
                {
                    if (T[i] < 0) T[i] += 1.0;
                    if (T[i] > 1) T[i] -= 1.0;

                    if ((T[i] * 6) < 1)
                    {
                        T[i] = p + ((q - p) * 6.0 * T[i]);
                    }
                    else if ((T[i] * 2.0) < 1) //(1.0/6.0)<=T[i] && T[i]<0.5
                    {
                        T[i] = q;
                    }
                    else if ((T[i] * 3.0) < 2) // 0.5<=T[i] && T[i]<(2.0/3.0)
                    {
                        T[i] = p + (q - p) * ((2.0 / 3.0) - T[i]) * 6.0;
                    }
                    else T[i] = p;
                }

                return Color.FromArgb(
                    Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                        T[0] * 255.0))),
                    Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                        T[1] * 255.0))),
                    Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                        T[2] * 255.0)))
                    );
            }
        }
    }

    public class CmykAverageFace : PureAverageFace
    {
        private double precision = 1000;

        public CmykAverageFace(int width, int height)
            : base(width, height)
        {
        }

        protected override int[,] CreateAverageFaceBuffer(int width, int height) {
            return new int[width*4, height];
        }

        protected override unsafe void AddBitmapToAverageFace(Bitmap bitmap, int[,] averageFace)
        {
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - bitmap.Width * 3;
                int nWidth = bitmap.Width * 4;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x += 4)
                    {
                        var b = p[0];
                        var g = p[1];
                        var r = p[2];
                        double _c, _m, _y, _k;
                        RGBtoCMYK(r, g, b, out _c, out _m, out _y, out _k);
                        averageFace[x + 0, y] += (int)(_c * precision);
                        averageFace[x + 1, y] += (int)(_m * precision);
                        averageFace[x + 2, y] += (int)(_y * precision);
                        averageFace[x + 3, y] += (int)(_k * precision);
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            bitmap.UnlockBits(bmData);
        }

        protected override unsafe Bitmap MakeAverageFaceBitmap(int[,] averageFace, int facesCount)
        {
            var averageFaceBitmap = new Bitmap(_width, _height);

            BitmapData abmData =
                averageFaceBitmap.LockBits(new Rectangle(0, 0, averageFaceBitmap.Width, averageFaceBitmap.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride1 = abmData.Stride;
            System.IntPtr Scan01 = abmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan01;
                int nOffset = stride1 - averageFaceBitmap.Width * 3;
                int nWidth = averageFaceBitmap.Width * 4;
                for (int y = 0; y < averageFaceBitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x += 4)
                    {
                        double _c = (averageFace[x + 0, y] / (double)precision) / facesCount;
                        double _m = (averageFace[x + 1, y] / (double)precision) / facesCount;
                        double _y = (averageFace[x + 2, y] / (double)precision) / facesCount;
                        double _k = (averageFace[x + 3, y] / (double)precision) / facesCount;
                        Color color = CMYKtoRGB(_c, _m, _y, _k);
                        p[0] = color.B;
                        p[1] = color.G;
                        p[2] = color.R;
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            averageFaceBitmap.UnlockBits(abmData);
            return averageFaceBitmap;
        }

        public static Color CMYKtoRGB(double c, double m, double y, double k)
        {
            int red = Convert.ToInt32((1 - c) * (1 - k) * 255.0);
            int green = Convert.ToInt32((1 - m) * (1 - k) * 255.0);
            int blue = Convert.ToInt32((1 - y) * (1 - k) * 255.0);

            return Color.FromArgb(red, green, blue);
        }

        public static void RGBtoCMYK(int red, int green, int blue, out double o_c, out double o_m, out double o_y, out double o_k)
        {
            // normalizes red, green, blue values
            double c = (double)(255 - red) / 255;
            double m = (double)(255 - green) / 255;
            double y = (double)(255 - blue) / 255;

            double k = (double)Math.Min(c, Math.Min(m, y));

            if (Math.Abs(k - 1.0) < 0.000000001) {
                o_c = 0;
                o_m = 0;
                o_y = 0;
                o_k = k;
            }
            else {
                o_c = (c - k)/(1 - k);
                o_m = (m - k)/(1 - k);
                o_y = (y - k) / (1 - k);
                o_k = k;
            }
        }


    }

    public class XyzAverageFace : PureAverageFace
    {
        private double precision = 1000;

        public XyzAverageFace(int width, int height)
            : base(width, height)
        {
        }

        protected override unsafe void AddBitmapToAverageFace(Bitmap bitmap, int[,] averageFace)
        {
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - bitmap.Width * 3;
                int nWidth = bitmap.Width * 3;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x += 3)
                    {
                        var b = p[0];
                        var g = p[1];
                        var r = p[2];
                        double _x, _y, _z;
                        RGBtoXYZ(r, g, b, out _x, out _y, out _z);
                        averageFace[x + 0, y] += (int)(_x * precision);
                        averageFace[x + 1, y] += (int)(_y * precision);
                        averageFace[x + 2, y] += (int)(_z * precision);
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            bitmap.UnlockBits(bmData);
        }

        protected override unsafe Bitmap MakeAverageFaceBitmap(int[,] averageFace, int facesCount)
        {
            var averageFaceBitmap = new Bitmap(_width, _height);

            BitmapData abmData =
                averageFaceBitmap.LockBits(new Rectangle(0, 0, averageFaceBitmap.Width, averageFaceBitmap.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride1 = abmData.Stride;
            System.IntPtr Scan01 = abmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan01;
                int nOffset = stride1 - averageFaceBitmap.Width * 3;
                int nWidth = averageFaceBitmap.Width * 3;
                for (int y = 0; y < averageFaceBitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x += 3)
                    {
                        double _x = (averageFace[x + 0, y] / (double)precision) / facesCount;
                        double _y = (averageFace[x + 1, y] / (double)precision) / facesCount;
                        double _z = (averageFace[x + 2, y] / (double)precision) / facesCount;
                        Color color = XYZtoRGB(_x, _y, _z);
                        p[0] = color.B;
                        p[1] = color.G;
                        p[2] = color.R;
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            averageFaceBitmap.UnlockBits(abmData);
            return averageFaceBitmap;
        }

        public static void RGBtoXYZ(int red, int green, int blue, out double x, out double y, out double z)
        {
            // normalize red, green, blue values
            double rLinear = (double)red / 255.0;
            double gLinear = (double)green / 255.0;
            double bLinear = (double)blue / 255.0;

            // convert to a sRGB form
            double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (
                1 + 0.055), 2.2) : (rLinear / 12.92);
            double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (
                1 + 0.055), 2.2) : (gLinear / 12.92);
            double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (
                1 + 0.055), 2.2) : (bLinear / 12.92);

            // converts
            x = (r*0.4124 + g*0.3576 + b*0.1805);
            y = (r*0.2126 + g*0.7152 + b*0.0722);
            z = (r*0.0193 + g*0.1192 + b*0.9505);
        }

        public static Color XYZtoRGB(double x, double y, double z)
        {
            double Clinear0 = x * 3.2410 - y * 1.5374 - z * 0.4986; // red
            double Clinear1 = -x * 0.9692 + y * 1.8760 - z * 0.0416; // green
            double Clinear2 = x * 0.0556 - y * 0.2040 + z * 1.0570; // blue

                Clinear0 = (Clinear0 <= 0.0031308) ? 12.92 * Clinear0 : (
                    1 + 0.055) * Math.Pow(Clinear0, (1.0 / 2.4)) - 0.055;
                Clinear1 = (Clinear1 <= 0.0031308) ? 12.92 * Clinear1 : (
                        1 + 0.055) * Math.Pow(Clinear1, (1.0 / 2.4)) - 0.055;
                Clinear2 = (Clinear2 <= 0.0031308) ? 12.92 * Clinear2 : (
                            1 + 0.055) * Math.Pow(Clinear2, (1.0 / 2.4)) - 0.055;
                if (Clinear0 < 0) Clinear0 = 0;
                if (Clinear1 < 0) Clinear1 = 0;
                if (Clinear2 < 0) Clinear2 = 0;

            int r = Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                Clinear0 * 255.0)));
            int g = Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                Clinear1 * 255.0)));
            int b = Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear2 * 255.0)));
            return Color.FromArgb(r, g, b);
        }
    }

    public class LabAverageFace : PureAverageFace
    {
        private double precision = 1000;

        public LabAverageFace(int width, int height)
            : base(width, height)
        {
        }

        protected override unsafe void AddBitmapToAverageFace(Bitmap bitmap, int[,] averageFace)
        {
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - bitmap.Width * 3;
                int nWidth = bitmap.Width * 3;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x += 3)
                    {
                        var b = p[0];
                        var g = p[1];
                        var r = p[2];
                        double _x, _y, _z;
                        RGBtoXYZ(r, g, b, out _x, out _y, out _z);
                        double _l, _a, _b;
                        XYZtoLab(_x, _y,_z, out _l, out _a, out _b);
                        averageFace[x + 0, y] += (int)(_l * precision);
                        averageFace[x + 1, y] += (int)(_a * precision);
                        averageFace[x + 2, y] += (int)(_b * precision);
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            bitmap.UnlockBits(bmData);
        }

        protected override unsafe Bitmap MakeAverageFaceBitmap(int[,] averageFace, int facesCount)
        {
            var averageFaceBitmap = new Bitmap(_width, _height);

            BitmapData abmData =
                averageFaceBitmap.LockBits(new Rectangle(0, 0, averageFaceBitmap.Width, averageFaceBitmap.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride1 = abmData.Stride;
            System.IntPtr Scan01 = abmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan01;
                int nOffset = stride1 - averageFaceBitmap.Width * 3;
                int nWidth = averageFaceBitmap.Width * 3;
                for (int y = 0; y < averageFaceBitmap.Height; y++)
                {
                    for (int x = 0; x < nWidth; x += 3)
                    {
                        double _l = (averageFace[x + 0, y] / (double)precision) / facesCount;
                        double _a = (averageFace[x + 1, y] / (double)precision) / facesCount;
                        double _b = (averageFace[x + 2, y] / (double)precision) / facesCount;
                        double _x, _y, _z;
                        LabtoXYZ(_l, _a, _b, out _x, out _y, out _z);
                        Color color = XYZtoRGB(_x, _y, _z);
                        p[0] = color.B;
                        p[1] = color.G;
                        p[2] = color.R;
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            averageFaceBitmap.UnlockBits(abmData);
            return averageFaceBitmap;
        }

        private static class D65 {
            public const double X = 0.9505;
            public const double Y = 1.0;
            public const double Z = 1.0890;
        }

        private static double Fxyz(double t)
        {
            return ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));
        }

        public static void XYZtoLab(double x, double y, double z, out double L, out double A, out double B)
        {
            L = 116.0 * Fxyz(y / D65.Y) - 16;
            A = 500.0 * (Fxyz(x / D65.X) - Fxyz(y / D65.Y));
            B = 200.0 * (Fxyz(y / D65.Y) - Fxyz(z / D65.Z));
        }

        public static void LabtoXYZ(double l, double a, double b, out double x, out double y, out double z)
        {
            double delta = 6.0 / 29.0;

            double fy = (l + 16) / 116.0;
            double fx = fy + (a / 500.0);
            double fz = fy - (b / 200.0);

            x = (fx > delta)
                ? D65.X*(fx*fx*fx)
                : (fx - 16.0/116.0)*3*(
                    delta*delta)*D65.X;
            y = (fy > delta)
                ? D65.Y*(fy*fy*fy)
                : (fy - 16.0/116.0)*3*(
                    delta*delta)*D65.Y;
            z = (fz > delta)
                ? D65.Z*(fz*fz*fz)
                : (fz - 16.0/116.0)*3*(
                    delta*delta)*D65.Z;
        }

        public static void RGBtoXYZ(int red, int green, int blue, out double x, out double y, out double z)
        {
            // normalize red, green, blue values
            double rLinear = (double)red / 255.0;
            double gLinear = (double)green / 255.0;
            double bLinear = (double)blue / 255.0;

            // convert to a sRGB form
            double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (
                1 + 0.055), 2.2) : (rLinear / 12.92);
            double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (
                1 + 0.055), 2.2) : (gLinear / 12.92);
            double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (
                1 + 0.055), 2.2) : (bLinear / 12.92);

            // converts
            x = (r * 0.4124 + g * 0.3576 + b * 0.1805);
            y = (r * 0.2126 + g * 0.7152 + b * 0.0722);
            z = (r * 0.0193 + g * 0.1192 + b * 0.9505);
        }

        public static Color XYZtoRGB(double x, double y, double z)
        {
            double Clinear0 = x * 3.2410 - y * 1.5374 - z * 0.4986; // red
            double Clinear1 = -x * 0.9692 + y * 1.8760 - z * 0.0416; // green
            double Clinear2 = x * 0.0556 - y * 0.2040 + z * 1.0570; // blue

            Clinear0 = (Clinear0 <= 0.0031308) ? 12.92 * Clinear0 : (
                1 + 0.055) * Math.Pow(Clinear0, (1.0 / 2.4)) - 0.055;
            Clinear1 = (Clinear1 <= 0.0031308) ? 12.92 * Clinear1 : (
                    1 + 0.055) * Math.Pow(Clinear1, (1.0 / 2.4)) - 0.055;
            Clinear2 = (Clinear2 <= 0.0031308) ? 12.92 * Clinear2 : (
                        1 + 0.055) * Math.Pow(Clinear2, (1.0 / 2.4)) - 0.055;
            if (Clinear0 < 0) Clinear0 = 0;
            if (Clinear1 < 0) Clinear1 = 0;
            if (Clinear2 < 0) Clinear2 = 0;
            if (Clinear0 > 1) Clinear0 = 1;
            if (Clinear1 > 1) Clinear1 = 1;
            if (Clinear2 > 1) Clinear2 = 1;

            int r = Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                Clinear0 * 255.0)));
            int g = Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                Clinear1 * 255.0)));
            int b = Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear2 * 255.0)));
            return Color.FromArgb(r, g, b);
        }
    }
}