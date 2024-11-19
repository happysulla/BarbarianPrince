using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace BarbarianPrince
{
   public static class ScreenExtensions
   {
      //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280511(v=vs.85).aspx
      public enum DpiType
      {
         Effective = 0,
         Angular = 1,
         Raw = 2,
      }
      //-----------------------------------------------------------------------------------
      public static void GetDpi(this System.Windows.Forms.Screen screen, DpiType dpiType, out uint dpiX, out uint dpiY)
      {
         if (null == screen)
         {
            dpiX = 96;
            dpiY = 96;
            return;
         }
         var pnt = new System.Drawing.Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
         var mon = MonitorFromPoint(pnt, 2/*MONITOR_DEFAULTTONEAREST*/);
         GetDpiForMonitor(mon, dpiType, out dpiX, out dpiY);
      }
      //-----------------------------------------------------------------------------------
      //https://msdn.microsoft.com/en-us/library/windows/desktop/dd145062(v=vs.85).aspx
      [DllImport("User32.dll")]
      private static extern IntPtr MonitorFromPoint([In] System.Drawing.Point pt, [In] uint dwFlags);
      //-----------------------------------------------------------------------------------
      //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510(v=vs.85).aspx
      [DllImport("Shcore.dll")]
      private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);
      //-----------------------------------------------------------------------------------
      public static System.Drawing.Point TransformToPixels(Visual visual, System.Drawing.Point pt)
      {
         System.Windows.Media.Matrix matrix;
         var source = PresentationSource.FromVisual(visual);
         if (source != null)
         {
            matrix = source.CompositionTarget.TransformToDevice;
         }
         else
         {
            using (var src = new HwndSource(new HwndSourceParameters()))
            {
               matrix = src.CompositionTarget.TransformToDevice;
            }
         }
         int pixelX = (int)(matrix.M11 * pt.X);
         int pixelY = (int)(matrix.M22 * pt.Y);
         System.Drawing.Point newPt = new System.Drawing.Point(pixelX, pixelY);
         return newPt;
      }
      //-----------------------------------------------------------------------------------
      public static string GetMonitor(System.Windows.Window window)
      {
         string monitorS = Screen.FromHandle(new WindowInteropHelper(window).Handle).DeviceName;
         string monitorName = monitorS.Substring(4);
         return monitorName;
      }
      //-----------------------------------------------------------------------------------
      public static Screen GetScreenFromPoint(System.Drawing.Point mousePoint)
      {
         System.Drawing.Rectangle ret;
         int numScreens = System.Windows.Forms.Screen.AllScreens.Length;
         for (int i = 0; i < numScreens; i++)
         {
            ret = Screen.AllScreens[i].Bounds;
            if (ret.Contains(mousePoint))
               return Screen.AllScreens[i];
         }
         return null;
      }
      //-----------------------------------------------------------------------------------
      public static int GetScreenIndexFromPoint(System.Drawing.Point mousePoint)
      {
         System.Drawing.Rectangle ret;
         int numScreens = System.Windows.Forms.Screen.AllScreens.Length;
         for (int i = 0; i < numScreens; i++)
         {
            ret = Screen.AllScreens[i].Bounds;
            if (ret.Contains(mousePoint))
               return i;
         }
         return 0;
      }
      //-----------------------------------------------------------------------------------
      public static string PrintScreenBounds()
      {
         StringBuilder sb = new StringBuilder();
         int numScreens = System.Windows.Forms.Screen.AllScreens.Length;
         for (int i = 0; i < numScreens; i++)
         {
            System.Drawing.Rectangle ret = Screen.AllScreens[i].Bounds;
            sb.Append("s[");
            sb.Append(i.ToString());
            sb.Append("]={");
            sb.Append(ret.X.ToString());
            sb.Append(",");
            sb.Append((ret.X + ret.Width).ToString());
            sb.Append("}");
         }
         return sb.ToString();
      }
      //-----------------------------------------------------------------------------------
      public static System.Windows.Media.Matrix GetMatrixFromVisual(Visual visual)
      {
         PresentationSource source = PresentationSource.FromVisual(visual);
         if (source != null)
         {
            return source.CompositionTarget.TransformToDevice;
         }
         else
         {
            using (var src = new HwndSource(new HwndSourceParameters()))
            {
               return src.CompositionTarget.TransformFromDevice;
            }
         }
      }
      //-----------------------------------------------------------------------------------
   }
}
