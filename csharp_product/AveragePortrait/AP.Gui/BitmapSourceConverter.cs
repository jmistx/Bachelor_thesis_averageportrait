//----------------------------------------------------------------------------
//  Copyright (C) 2004-2013 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Emgu.CV.WPF
{
   public static class BitmapSourceConvert
   {
      /// <summary>
      /// Delete a GDI object
      /// </summary>
      /// <param name="o">The poniter to the GDI object to be deleted</param>
      /// <returns></returns>
      [DllImport("gdi32")]
      private static extern int DeleteObject(IntPtr o);

       public static BitmapSource ToBitmapSource(Bitmap source)
       {
           if (source == null)
           {
               return null;
           }

           IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

           BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
               ptr,
               IntPtr.Zero,
               Int32Rect.Empty,
               System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

           DeleteObject(ptr); //release the HBitmap
           return bs;
           
       }
   }
}