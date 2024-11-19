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
      private static bool theIsScreenChange = false;
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
         //System.Windows.Point newPoint1 = this.PointToScreen(e.GetPosition(this));
         //myPreviousScreenPoint = new System.Drawing.Point((int)newPoint1.X, (int)newPoint1.Y);
         //myPreviousScreen = ScreenExtensions.GetScreenFromPoint(myPreviousScreenPoint);
         //myPreviousScreenIndex = ScreenExtensions.GetScreenIndexFromPoint(myPreviousScreenPoint);
         //myPreviousMonitor = ScreenExtensions.GetMonitor(this);
         //myPreviousMatrix = ScreenExtensions.GetMatrixFromVisual(this);
         //uint dpiX = 0;
         //uint dpiY = 0;
         //ScreenExtensions.GetDpi(myPreviousScreen, ScreenExtensions.DpiType.Effective, out dpiX, out dpiY);
         //myPreviousRatio = 96.0 / dpiX;
         ////---------------------
         //double leftPt = this.Left;
         //double leftPtR = this.Left * myPreviousRatio;
         //StringBuilder sb = new StringBuilder();
         //sb.Append(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
         //sb.Append(ScreenExtensions.PrintScreenBounds());
         //sb.Append(" pt(");
         //sb.Append(myPreviousScreenPoint.X.ToString());
         //sb.Append(") screenIndex=(");
         //sb.Append(myPreviousScreenIndex.ToString());
         //sb.Append(") mon=(");
         //sb.Append(myPreviousMonitor);
         //sb.Append(") ratio=(");
         //sb.Append(myPreviousRatio.ToString());
         //sb.Append(") this.left=(");
         //sb.Append(leftPt.ToString());
         //sb.Append(") this.left=(");
         //sb.Append(leftPtR.ToString());
         //sb.Append(")");
         //Console.WriteLine(sb.ToString());
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
            //StringBuilder sb = new StringBuilder();
            System.Windows.Point newPoint1 = this.PointToScreen(e.GetPosition(this));
            System.Drawing.Point currentScreenPt = new System.Drawing.Point((int)newPoint1.X, (int)newPoint1.Y);
            Screen currentScreen = ScreenExtensions.GetScreenFromPoint(currentScreenPt);
            //int currentScreenIndex = ScreenExtensions.GetScreenIndexFromPoint(currentScreenPt);
            //string currentMonitor = ScreenExtensions.GetMonitor(this); 
            //System.Windows.Media.Matrix currentMatrix = ScreenExtensions.GetMatrixFromVisual(this);
            uint dpiX = 1;
            uint dpiY = 1;
            ScreenExtensions.GetDpi(currentScreen, ScreenExtensions.DpiType.Effective, out dpiX, out dpiY);
            double ratio = 96.0 / dpiX;
            currentScreenPt.X = (int)(currentScreenPt.X * ratio);
            currentScreenPt.Y = (int)(currentScreenPt.Y * ratio);
            this.Left = currentScreenPt.X - myOffsetInBannerWindow.X * ratio; // Move the window.
            this.Top = currentScreenPt.Y - myOffsetInBannerWindow.Y * ratio;
            ////----------------------
            //if (myPreviousScreenIndex != currentScreenIndex)
            //{
            //   myIsDragging = false;
            //   currentScreenPt.X = (int)(currentScreenPt.X * ratio);
            //   currentScreenPt.Y = (int)(currentScreenPt.Y * ratio);
            //   switch (currentScreenIndex)
            //   {
            //      case 0:
            //         this.Left = -1900;
            //         this.Top = myScreenBounds[0].Top + 1;
            //         break;
            //      case 1:
            //         this.Left = myScreenBounds[1].Left + 0.75 * myScreenBounds[1].Width;
            //         this.Top = myScreenBounds[1].Top + 1;
            //         break;
            //      case 2:
            //         this.Left = myScreenBounds[2].Left + 1;
            //         this.Top = myScreenBounds[2].Top + 1;
            //         break;
            //      default:
            //         this.Left = currentScreenPt.X - myOffsetInBannerWindow.X; // Move the window.
            //         this.Top = currentScreenPt.Y - myOffsetInBannerWindow.Y;
            //         break;
            //   }
            //   sb.Append("sssssssssssssssssssssssss\n");
            //   sb.Append(ScreenExtensions.PrintScreenBounds());
            //   sb.Append(" pt(");
            //   sb.Append(myPreviousScreenPoint.X.ToString());
            //   sb.Append("=>");
            //   sb.Append(currentScreenPt.X.ToString());
            //   sb.Append(") scn(");
            //   sb.Append(myPreviousScreenIndex.ToString());
            //   sb.Append("=>");
            //   sb.Append(currentScreenIndex.ToString());
            //   sb.Append(") mon(");
            //   sb.Append(myPreviousMonitor);
            //   sb.Append("=>");
            //   sb.Append(currentMonitor);
            //   sb.Append(") ratio(");
            //   sb.Append(myPreviousRatio.ToString());
            //   sb.Append("=>");
            //   sb.Append(ratio.ToString());
            //   sb.Append(") this.Left(");
            //   sb.Append(this.Left.ToString());
            //   sb.Append(") dpiX(");
            //   sb.Append(dpiX.ToString());
            //   sb.Append(")");
            //   Console.WriteLine(sb.ToString());
            //   theIsScreenChange = true;
            //}
            //else if (myPreviousMonitor != currentMonitor)
            //{
            //   myIsDragging = false;
            //   currentScreenPt.X = (int)(currentScreenPt.X * ratio);
            //   currentScreenPt.Y = (int)(currentScreenPt.Y * ratio);
            //   switch(currentMonitor)
            //   {
            //      case "DISPLAY1":
            //         this.Left = -1900;
            //         this.Top = myScreenBounds[0].Top + 1;
            //         break;
            //      case "DISPLAY2":
            //         this.Left = myScreenBounds[1].Left + 0.5 * myScreenBounds[1].Width;
            //         this.Top = myScreenBounds[1].Top + 1;
            //         break;
            //      case "DISPLAY3":
            //         this.Left = myScreenBounds[2].Left + 1;
            //         this.Top = myScreenBounds[2].Top + 1;

            //         break;
            //      default:
            //         this.Left = currentScreenPt.X - myOffsetInBannerWindow.X; // Move the window.
            //         this.Top = currentScreenPt.Y - myOffsetInBannerWindow.Y;
            //         break;
            //   }
            //   sb.Append("mmmmmmmmmmmmmmmmmmmmmmmmm\n");
            //   sb.Append(ScreenExtensions.PrintScreenBounds());
            //   sb.Append(" pt(");
            //   sb.Append(myPreviousScreenPoint.X.ToString());
            //   sb.Append("=>");
            //   sb.Append(currentScreenPt.X.ToString());
            //   sb.Append(") scn(");
            //   sb.Append(myPreviousScreenIndex.ToString());
            //   sb.Append("=>");
            //   sb.Append(currentScreenIndex.ToString());
            //   sb.Append(") mon(");
            //   sb.Append(myPreviousMonitor);
            //   sb.Append("=>");
            //   sb.Append(currentMonitor);
            //   sb.Append(") ratio(");
            //   sb.Append(myPreviousRatio.ToString());
            //   sb.Append("=>");
            //   sb.Append(ratio.ToString());
            //   sb.Append(") this.Left(");
            //   sb.Append(this.Left.ToString());
            //   sb.Append(") dpiX(");
            //   sb.Append(dpiX.ToString());
            //   sb.Append(")");
            //   Console.WriteLine(sb.ToString());
            //   theIsScreenChange = true;
            //}
            //else
            //{
            //   currentScreenPt.X = (int)(currentScreenPt.X * ratio);
            //   currentScreenPt.Y = (int)(currentScreenPt.Y * ratio);
            //   if ("DISPLAY1" == currentMonitor)
            //   {
            //      this.Left = -1700; 
            //      this.Top = 0;
            //   }
            //   else
            //   {
            //      this.Left = currentScreenPt.X - 100; // Move the window.
            //      this.Top = 0 ;
            //   }
            //   if ( true == theIsScreenChange)
            //   {
            //      sb.Append(" pt(");
            //      sb.Append(myPreviousScreenPoint.X.ToString());
            //      sb.Append("=>");
            //      sb.Append(currentScreenPt.X.ToString());
            //      sb.Append(") scn(");
            //      sb.Append(myPreviousScreenIndex.ToString());
            //      sb.Append("=>");
            //      sb.Append(currentScreenIndex.ToString());
            //      sb.Append(") mon(");
            //      sb.Append(myPreviousMonitor);
            //      sb.Append("=>");
            //      sb.Append(currentMonitor);
            //      sb.Append(") ratio(");
            //      sb.Append(myPreviousRatio.ToString());
            //      sb.Append("=>");
            //      sb.Append(ratio.ToString());
            //      sb.Append(") this.Left(");
            //      sb.Append(this.Left.ToString());
            //      sb.Append(") dpiX(");
            //      sb.Append(dpiX.ToString());
            //      sb.Append(")");
            //      Console.WriteLine(sb.ToString());
            //      sb.Append(this.Left.ToString());
            //      theIsScreenChange = false;
            //   }
            //}
            //myPreviousRatio = ratio;
            //myPreviousMonitor = currentMonitor;
            //myPreviousScreenIndex = currentScreenIndex;
            //myPreviousScreenPoint = currentScreenPt;
            e.Handled = true;
         }

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
