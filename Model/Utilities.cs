using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace BarbarianPrince
{
   public class Utilities
   {
      public const int NO_RESULT = -100;
      public const int FOREVER = 100000;
      public const int STACK = 1;
      public const double ZOOM = 1.25;
      public const int MAX_GRID_ROW = 40;
      public const int MAX_SLAVE_GIRLS = 7;
      public static SolidColorBrush theBrushBlood = new SolidColorBrush();
      public static SolidColorBrush theBrushRegion = new SolidColorBrush();
      public static SolidColorBrush theBrushRegionClear = new SolidColorBrush();
      public static SolidColorBrush theBrushControlButton = new SolidColorBrush();
      public static SolidColorBrush theBrushScrollViewerActive = new SolidColorBrush();
      public static SolidColorBrush theBrushScrollViewerInActive = new SolidColorBrush();
      public static int MapItemNum { set; get; } = 0;
      public static int GroupNum { set; get; } = 1;
      public static int PorterNum { set; get; } = 0; // used to identify porters in e210 that are paired togetther
      public static int MaxLoad { get; } = 10;
      public static int MaxMountLoad { get; } = 30;
      public static int PersonBurden { get; } = 20; // how much a person measures in loads
      public static int MaxDays = 70; // 10 weeks
      public static Double ZoomCanvas { get; set; } = 1.0;
      public static Double theMapItemOffset = 20;
      public static Double theMapItemSize = 40;  // size of a MapItem black
      public static int theStackSize = 1000;
      public static string[] theNorthOfTragothHexes = new string[21] { "0101", "0201", "0301", "0302", "0401", "0501", "0502", "0601", "0701", "0801", "0901", "1001", "1101", "1201", "1301", "1501", "1601", "1701", "1801", "1901", "2001" };
      private static readonly Random theRandom = new Random(); // default seed is System time
      static public Random RandomGenerator
      {
         get
         {
            //int outIndex = theRandom.Next(300);
            //int divisor = 3;
            //for ( int j=0; j < outIndex; ++j )
            //{
            //   switch(divisor)
            //   {
            //      case 3:  divisor = 11; break;
            //      case 5:  divisor = 19; break;
            //      case 7:  divisor = 5;  break;
            //      case 11: divisor = 13; break;
            //      case 13: divisor = 7;  break;
            //      case 17: divisor = 3;  break;
            //      case 19: divisor = 17; break;
            //   }
            //   int seed = theRandom.Next(265535);
            //   while (seed < divisor)
            //      seed *= 3 ;
            //   int index = seed / divisor;
            //   int inIndex = theRandom.Next(2000);
            //   for (int i = 0; i < inIndex; i++)
            //      seed = theRandom.Next(seed);
            //}
            //Random aRandom = new Random(theRandom.Next(265535));
            return theRandom;
         }
      }
      public static string RemoveSpaces(string aLine)
      {
         string[] aStringArray1 = aLine.Split(new char[] { '"' });
         int length = aStringArray1.Length;
         if (0 == length % 2)
            throw new Exception("Syntax Error: Invalid number of quotes");
         for (int i = 0; i < aStringArray1.Length; i += 2)
         {
            string aSubString = "";
            string[] aStringArray2 = aStringArray1[i].Split(new char[] { ' ' });
            foreach (string aString in aStringArray2)
               aSubString += aString;
            aStringArray1[i] = aSubString;
         }
         StringBuilder sb = new StringBuilder();
         foreach (string aString in aStringArray1)
            sb.Append(aString);
         aLine = sb.ToString();
         return aLine;
      }
      public static bool IsSubstring(string parentString, string substring) // Find if S2 is a substring of S1
      {
         int lenParentString = parentString.Length;
         int lenSubstring = substring.Length;
         for (int i = 0; i <= lenParentString - lenSubstring; i++) // A loop to slide pat[] one by one
         {
            int j;
            for (j = 0; j < lenSubstring; j++)  // For current index i, check for pattern match 
               if (parentString[i + j] != substring[j])
                  break;
            if (j == lenSubstring)
               return true;
         }
         return false;
      }
      public static Cursor ConvertToCursor(UIElement control, Point hotSpot)
      {
         //--------------------------------------------
         // convert FrameworkElement to PNG stream
         var pngStream = new MemoryStream();
         control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
         Rect rect = new Rect(0, 0, control.DesiredSize.Width, control.DesiredSize.Height);
         RenderTargetBitmap rtb = new RenderTargetBitmap((int)control.DesiredSize.Width, (int)control.DesiredSize.Height, 96, 96, PixelFormats.Pbgra32);
         control.Arrange(rect);
         rtb.Render(control);
         //--------------------------------------------
         PngBitmapEncoder png = new PngBitmapEncoder();
         png.Frames.Add(BitmapFrame.Create(rtb));
         png.Save(pngStream);
         //--------------------------------------------
         // write cursor header info
         var cursorStream = new MemoryStream();
         cursorStream.Write(new byte[2] { 0x00, 0x00 }, 0, 2);                               // ICONDIR: Reserved. Must always be 0.
         cursorStream.Write(new byte[2] { 0x02, 0x00 }, 0, 2);                               // ICONDIR: Specifies image type: 1 for icon (.ICO) image, 2 for cursor (.CUR) image. Other values are invalid
         cursorStream.Write(new byte[2] { 0x01, 0x00 }, 0, 2);                               // ICONDIR: Specifies number of images in the file.
         cursorStream.Write(new byte[1] { (byte)control.DesiredSize.Width }, 0, 1);          // ICONDIRENTRY: Specifies image width in pixels. Can be any number between 0 and 255. Value 0 means image width is 256 pixels.
         cursorStream.Write(new byte[1] { (byte)control.DesiredSize.Height }, 0, 1);         // ICONDIRENTRY: Specifies image height in pixels. Can be any number between 0 and 255. Value 0 means image height is 256 pixels.
         cursorStream.Write(new byte[1] { 0x00 }, 0, 1);                                     // ICONDIRENTRY: Specifies number of colors in the color palette. Should be 0 if the image does not use a color palette.
         cursorStream.Write(new byte[1] { 0x00 }, 0, 1);                                     // ICONDIRENTRY: Reserved. Should be 0.
         cursorStream.Write(new byte[2] { (byte)hotSpot.X, 0x00 }, 0, 2);                    // ICONDIRENTRY: Specifies the horizontal coordinates of the hotspot in number of pixels from the left.
         cursorStream.Write(new byte[2] { (byte)hotSpot.Y, 0x00 }, 0, 2);                    // ICONDIRENTRY: Specifies the vertical coordinates of the hotspot in number of pixels from the top.
         cursorStream.Write(new byte[4] {                                                    // ICONDIRENTRY: Specifies the size of the image's data in bytes
                                          (byte)((pngStream.Length & 0x000000FF)),
                                          (byte)((pngStream.Length & 0x0000FF00) >> 0x08),
                                          (byte)((pngStream.Length & 0x00FF0000) >> 0x10),
                                          (byte)((pngStream.Length & 0xFF000000) >> 0x18)
                                       }, 0, 4);
         cursorStream.Write(new byte[4] {                                                    // ICONDIRENTRY: Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file
                                          (byte)0x16,
                                          (byte)0x00,
                                          (byte)0x00,
                                          (byte)0x00,
                                       }, 0, 4);

         // copy PNG stream to cursor stream
         pngStream.Seek(0, SeekOrigin.Begin);
         pngStream.CopyTo(cursorStream);

         // return cursor stream
         cursorStream.Seek(0, SeekOrigin.Begin);
         return new Cursor(cursorStream);
      }
   }
}
