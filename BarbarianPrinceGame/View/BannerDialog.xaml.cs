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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class BannerDialog : System.Windows.Window
   {
      public bool CtorError { get; } = false;
      private bool myIsDragging = false;
      private System.Windows.Point myOffsetInBannerWindow;
      private System.Drawing.Point myPreviousScreenPoint;
      private Screen myPreviousScreen;
      private int myPreviousScreenIndex;
      private string myPreviousMonitor;
      private double myPreviousRatio;
      private System.Drawing.Rectangle[] myScreenBounds = new System.Drawing.Rectangle[4];
      private System.Windows.Media.Matrix myPreviousMatrix;
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
         //----------------------------------
         int numScreens = System.Windows.Forms.Screen.AllScreens.Length;
         for (int i = 0; i < numScreens; i++)
            myScreenBounds[i] = Screen.AllScreens[i].Bounds;
         //-------------------------------
         try
         {
            XmlTextReader xr = new XmlTextReader(sr);
            myTextBlockDisplay = (TextBlock)XamlReader.Load(xr);
            myScrollViewerTextBlock.Content = myTextBlockDisplay;
            myTextBlockDisplay.MouseLeftButtonDown += Window_MouseLeftButtonDown;
            myTextBlockDisplay.MouseLeave += TextBlockDisplay_MouseLeave;
            myKey = key;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "BannerDialog(): e=" + e.ToString() + "  for key=" + key);
            CtorError = true;
            return;
         }
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
         myPreviousScreen = ScreenExtensions.GetScreenFromPoint(myPreviousScreenPoint);
         myPreviousScreenIndex = ScreenExtensions.GetScreenIndexFromPoint(myPreviousScreenPoint);
         myPreviousMonitor = ScreenExtensions.GetMonitor(this);
         myPreviousMatrix = ScreenExtensions.GetMatrixFromVisual(this);
         uint dpiX = 0;
         uint dpiY = 0;
         ScreenExtensions.GetDpi(myPreviousScreen, ScreenExtensions.DpiType.Effective, out dpiX, out dpiY);
         myPreviousRatio = 96.0 / dpiX;
         var dpiInfo = VisualTreeHelper.GetDpi(this);
         StringBuilder sb = new StringBuilder();
         sb.Append(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
         sb.Append(ScreenExtensions.PrintScreenBounds());
         sb.Append(" pt(");
         sb.Append(myPreviousScreenPoint.X.ToString());
         sb.Append(") screenIndex=(");
         sb.Append(myPreviousScreenIndex.ToString());
         sb.Append(") mon=(");
         sb.Append(myPreviousMonitor);
         sb.Append(") ratio=(");
         sb.Append(myPreviousRatio.ToString());
         sb.Append(") this.left=(");
         sb.Append(this.Left.ToString());
         sb.Append(")");
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
            System.Windows.Point newPoint1 = this.PointToScreen(e.GetPosition(this));
            System.Drawing.Point currentScreenPt = new System.Drawing.Point((int)newPoint1.X, (int)newPoint1.Y);
            Screen currentScreen = ScreenExtensions.GetScreenFromPoint(currentScreenPt);
            int currentScreenIndex = ScreenExtensions.GetScreenIndexFromPoint(currentScreenPt);
            string currentMonitor = ScreenExtensions.GetMonitor(this);
            System.Windows.Media.Matrix currentMatrix = ScreenExtensions.GetMatrixFromVisual(this);
            uint dpiX = 1;
            uint dpiY = 1;
            ScreenExtensions.GetDpi(currentScreen, ScreenExtensions.DpiType.Effective, out dpiX, out dpiY);
            double ratio = 96.0 / dpiX;
            //----------------------
            StringBuilder sb = new StringBuilder();
            if (myPreviousScreenIndex != currentScreenIndex)
            {
               sb.Append("sssssssssssssssssssssssss\n");
               sb.Append(ScreenExtensions.PrintScreenBounds());
               sb.Append(" pt(");
               sb.Append(myPreviousScreenPoint.X.ToString());
               sb.Append("=>");
               sb.Append(currentScreenPt.X.ToString());
               sb.Append(") scn(");
               sb.Append(myPreviousScreenIndex.ToString());
               sb.Append("=>");
               sb.Append(currentScreenIndex.ToString());
               sb.Append(") mon(");
               sb.Append(myPreviousMonitor);
               sb.Append("=>");
               sb.Append(currentMonitor);
               sb.Append(") ratio(");
               sb.Append(myPreviousRatio.ToString());
               sb.Append("=>");
               sb.Append(ratio.ToString());
               sb.Append(") this.Left(");
               sb.Append(this.Left.ToString());
               sb.Append(") dpiX(");
               sb.Append(dpiX.ToString());
               sb.Append(")");
               Console.WriteLine(sb.ToString());
            }
            else if (myPreviousMonitor != currentMonitor)
            {
               sb.Append("mmmmmmmmmmmmmmmmmmmmmmmmm\n");
               sb.Append(ScreenExtensions.PrintScreenBounds());
               sb.Append(" pt(");
               sb.Append(myPreviousScreenPoint.X.ToString());
               sb.Append("=>");
               sb.Append(currentScreenPt.X.ToString());
               sb.Append(") scn(");
               sb.Append(myPreviousScreenIndex.ToString());
               sb.Append("=>");
               sb.Append(currentScreenIndex.ToString());
               sb.Append(") mon(");
               sb.Append(myPreviousMonitor);
               sb.Append("=>");
               sb.Append(currentMonitor);
               sb.Append(") ratio(");
               sb.Append(myPreviousRatio.ToString());
               sb.Append("=>");
               sb.Append(ratio.ToString());
               sb.Append(") this.Left(");
               sb.Append(this.Left.ToString());
               sb.Append(") dpiX(");
               sb.Append(dpiX.ToString());
               sb.Append(")");
               Console.WriteLine(sb.ToString());
            }
            else
            {
               currentScreenPt.X = (int)(currentScreenPt.X);
               currentScreenPt.Y = (int)(currentScreenPt.Y);
               this.Left = currentScreenPt.X - myOffsetInBannerWindow.X * ratio;
               this.Top = currentScreenPt.Y - myOffsetInBannerWindow.Y * ratio;
               sb.Append(" pt(");
               sb.Append(myPreviousScreenPoint.X.ToString());
               sb.Append("=>");
               sb.Append(currentScreenPt.X.ToString());
               sb.Append(") scn(");
               sb.Append(myPreviousScreenIndex.ToString());
               sb.Append("=>");
               sb.Append(currentScreenIndex.ToString());
               sb.Append(") mon(");
               sb.Append(myPreviousMonitor);
               sb.Append("=>");
               sb.Append(currentMonitor);
               sb.Append(") ratio(");
               sb.Append(myPreviousRatio.ToString());
               sb.Append("=>");
               sb.Append(ratio.ToString());
               sb.Append(") this.Left(");
               sb.Append(this.Left.ToString());
               sb.Append(") dpiX(");
               sb.Append(dpiX.ToString());
               sb.Append(")");
               Console.WriteLine(sb.ToString());
               sb.Append(this.Left.ToString());
            }

            myPreviousRatio = ratio;
            myPreviousMonitor = currentMonitor;
            myPreviousScreenIndex = currentScreenIndex;
            myPreviousScreenPoint = currentScreenPt;
         }
         e.Handled = true;
      }
      private void Window_MouseUp(object sender, MouseButtonEventArgs e)
      {
         myIsDragging = false;
      }
      private void TextBlockDisplay_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
      {
         myIsDragging = false;
      }
   }
}
