using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class BannerDialog : System.Windows.Window
   {
      public bool CtorError { get; } = false;
      private bool myIsDragging = false;
      private System.Windows.Point myOffsetInBannerWindow;
      private System.Drawing.Point myPreviousScreenPoint;
      private int myInitialScreen;
      private double myM11Previous;
      private double myM22Previous;
      private string myKey = "";
      public string Key { get => myKey; }
      public TextBlock TextBoxDiplay { get => myTextBlockDisplay; }
      public BannerDialog(string key, StringReader sr)
      {
         InitializeComponent();
         BitmapImage img = MapItem.theMapImages.GetBitmapImage("Parchment");
         ImageBrush brush = new ImageBrush(img);
         this.Background = brush;   
         //-------------------------------
         Image imageAxes = new Image() { Source = MapItem.theMapImages.GetBitmapImage("CrossedAxes") };
         myButtonClose.Content = imageAxes;
         //-------------------------------
         try
         {
            XmlTextReader xr = new XmlTextReader(sr);
            myTextBlockDisplay = (TextBlock)XamlReader.Load(xr);
            myScrollViewerTextBlock.Content = myTextBlockDisplay;
            myTextBlockDisplay.MouseLeftButtonDown += Window_MouseLeftButtonDown;
            myKey = key;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "BannerDialog(): e=" + e.ToString() + "  for key=" + key);
            CtorError = true;
            return;
         }
      }
      private int ConvertMousePointToScreenIndex(System.Drawing.Point mousePoint)
      {
         System.Drawing.Rectangle ret;
         int numScreens = System.Windows.Forms.Screen.AllScreens.Length;
         for (int i = 0; i < numScreens; i++)
         {
            ret = Screen.AllScreens[i].Bounds;
            StringBuilder sb = new StringBuilder();
            sb.Append("s[");
            sb.Append(i.ToString());
            sb.Append("]={");
            sb.Append(ret.X.ToString());
            sb.Append(",");
            sb.Append(ret.Y.ToString());
            sb.Append(",");
            sb.Append((ret.X+ret.Width).ToString());
            sb.Append(",");
            sb.Append((ret.Y + ret.Height).ToString());
            sb.Append("}");
            if (ret.Contains(mousePoint))
            {
               sb.Append(" CONTAINS p=(");
               sb.Append(mousePoint.X.ToString());
               sb.Append(",");
               sb.Append(mousePoint.Y.ToString());
               sb.Append(")");
               Console.WriteLine(sb.ToString());
               return i;
            }
         }
         return 0;
      }
      public System.Drawing.Point TransformToPixels(Visual visual, System.Drawing.Point pt)
      {
         Matrix matrix;
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
      //-------------------------------------------------------------------------
      private void BannerLoaded(object sender, EventArgs e)
      {
         myScrollViewerTextBlock.Height = myDockPanel.ActualHeight - myButtonClose.Height - 50;
         myTextBlockDisplay.Height = myTextBlockDisplay.ActualHeight;
      }
      private void ButtonClose_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         myIsDragging = true;
         myOffsetInBannerWindow = e.GetPosition(this);
         System.Windows.Point newPoint1 = this.PointToScreen(e.GetPosition(this));
         myPreviousScreenPoint = new System.Drawing.Point((int)newPoint1.X, (int)newPoint1.Y);
         myInitialScreen = ConvertMousePointToScreenIndex(myPreviousScreenPoint);
         System.Windows.Point currentPt = this.PointToScreen(e.GetPosition(this));  // Find the current mouse position in screen coordinates.
         Matrix matrix;
         PresentationSource source = PresentationSource.FromVisual(this);
         if (source != null)
         {
            myM11Previous = source.CompositionTarget.TransformToDevice.M11;
            myM22Previous = source.CompositionTarget.TransformToDevice.M22;
         }
         else
         {
            using (var src = new HwndSource(new HwndSourceParameters()))
            {
               myM11Previous = src.CompositionTarget.TransformToDevice.M11;
               myM22Previous = src.CompositionTarget.TransformToDevice.M22;
            }
         }
         //---------------------
         StringBuilder sb = new StringBuilder();
         sb.Append(" offset=");
         sb.Append(myOffsetInBannerWindow.ToString());
         sb.Append(" pt=");
         sb.Append(myPreviousScreenPoint.ToString());
         sb.Append(" is=");
         sb.Append(myInitialScreen.ToString());
         Console.WriteLine(sb.ToString());
         //---------------------
         myIsDragging = true;
         e.Handled = true;
      }
      private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (false == myIsDragging)
         {
            base.OnMouseMove(e);
         }
         else
         {
            System.Windows.Point currentPt = this.PointToScreen(e.GetPosition(this));  // Find the current mouse position in screen coordinates.
            Matrix matrix;
            PresentationSource source = PresentationSource.FromVisual(this);
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
            //----------------------------------
            StringBuilder sb = new StringBuilder();
            sb.Append(" currentPt=");
            sb.Append(currentPt.ToString());
            sb.Append(" M11=");
            sb.Append(matrix.M11.ToString());
            sb.Append(" M22=");
            sb.Append(matrix.M22.ToString());
            sb.Append(" offset=");
            sb.Append(myOffsetInBannerWindow.X.ToString("00"));
            sb.Append(",");
            sb.Append(myOffsetInBannerWindow.Y.ToString("00"));
            //----------------------------------
            System.Drawing.Point pt = new System.Drawing.Point((int)currentPt.X, (int)currentPt.Y);
            int newScreen = ConvertMousePointToScreenIndex(pt);
            if (newScreen != myInitialScreen)
            {
               // Stop moving the window when mouse shows up on new screen
               // Allows user to move to new screen by grabbing on new screen
               myIsDragging = false;
               sb.Append(" !!!!!!!!!!!!!!!ns=");
               sb.Append(newScreen.ToString());
               myIsDragging = false;
            }
            else if (myM11Previous != matrix.M11)
            {
               myM11Previous = matrix.M11;   
               myM22Previous = matrix.M22;   
               currentPt.X = currentPt.X / myM11Previous;
               currentPt.Y = currentPt.Y / myM22Previous;
               sb.Append(" ***************myM11P=");
               sb.Append(myM11Previous.ToString("0.0"));
               myIsDragging = false;
               sb.Append(" newPoint=");
               sb.Append(currentPt.X.ToString("00"));
               sb.Append(",");
               sb.Append(currentPt.Y.ToString("00"));
               this.Left = currentPt.X - myOffsetInBannerWindow.X; // Move the window.
               this.Top = currentPt.Y - myOffsetInBannerWindow.Y;
               myIsDragging = false;
            }
            else
            {
               //currentPt.Offset(-myOffsetInBannerWindow.X, -myOffsetInBannerWindow.Y); // Compensate for the position the control was clicked.
               currentPt.X = currentPt.X / matrix.M11;
               currentPt.Y = currentPt.Y / matrix.M22;
               this.Left = currentPt.X - myOffsetInBannerWindow.X; // Move the window.
               this.Top = currentPt.Y - myOffsetInBannerWindow.Y;
               sb.Append(" newPoint=");
               sb.Append(currentPt.X.ToString("00"));
               sb.Append(",");
               sb.Append(currentPt.Y.ToString("00"));
            }
            Console.WriteLine(sb.ToString());
            e.Handled = true;
         }

      }
      private void Window_MouseUp(object sender, MouseButtonEventArgs e)
      {
         myIsDragging = false;
      }
   }
}
