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
      private string myKey = "";
      public string Key { get => myKey; }
      private TextBlock myTextBlockDisplay = null;
      public TextBlock TextBoxDiplay { get => myTextBlockDisplay; }
      public static bool theIsCheckBoxChecked = false;
      private bool myIsReopen = false;
      public bool IsReopen { get => myIsReopen; }
      //------------------------------------
      private bool myIsDragging = false;
      private System.Windows.Point myOffsetInBannerWindow;
#if UT3
      private System.Drawing.Point myPreviousScreenPoint;
      private Screen myPreviousScreen;
      private int myPreviousScreenIndex;
      private string myPreviousMonitor;
      private double myPreviousScaleRatio;
      private double myPreviousScreenRatio;
      private System.Windows.Media.Matrix myPreviousMatrix;
#endif
      //-------------------------------------------------------------------------------------
      public BannerDialog(string key, StringReader sr)
      {
         InitializeComponent();
         myIsReopen = false; // Tell parent to reopen on font change
         BitmapImage img = MapItem.theMapImages.GetBitmapImage("Parchment");
         ImageBrush brush = new ImageBrush(img);
         this.Background = brush;
         //-------------------------------
         Image imageAxes = new Image() { Source = MapItem.theMapImages.GetBitmapImage("CrossedAxes") };
         myButtonClose.Content = imageAxes;
         //-------------------------------
         myCheckBoxFont.IsChecked = theIsCheckBoxChecked;
         //-------------------------------
         try
         {
            XmlTextReader xr = new XmlTextReader(sr);
            myTextBlockDisplay = (TextBlock)XamlReader.Load(xr); // TextBox created in RuleManager.ShowRule()
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
#if UT3
         System.Windows.Point newPoint1 = this.PointToScreen(e.GetPosition(this));
         myPreviousScreenPoint = new System.Drawing.Point((int)newPoint1.X, (int)newPoint1.Y);
         myPreviousScreen = ScreenExtensions.GetScreenFromPoint(myPreviousScreenPoint);
         myPreviousScreenIndex = ScreenExtensions.GetScreenIndexFromPoint(myPreviousScreenPoint);
         myPreviousMonitor = ScreenExtensions.GetMonitor(this);
         myPreviousMatrix = ScreenExtensions.GetMatrixFromVisual(this);
         uint dpiX = 0;
         uint dpiY = 0;
         ScreenExtensions.GetDpi(myPreviousScreen, ScreenExtensions.DpiType.Effective, out dpiX, out dpiY);
         myPreviousScaleRatio = 96.0 / dpiX;
         myPreviousScreenRatio = System.Windows.SystemParameters.PrimaryScreenWidth / ScreenExtensions.GetScreenResolutionWidthFromPoint(myPreviousScreenPoint);
         StringBuilder sb = new StringBuilder();
         sb.Append(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
         sb.Append(ScreenExtensions.PrintScreenBounds());
         sb.Append(" pt(");
         sb.Append(myPreviousScreenPoint.X.ToString());
         sb.Append(") screenIndex=(");
         sb.Append(myPreviousScreenIndex.ToString());
         sb.Append(") mon=(");
         sb.Append(myPreviousMonitor);
         sb.Append(") scaleRatio=(");
         sb.Append(myPreviousScaleRatio.ToString());
         sb.Append(") screenRatio=(");
         sb.Append(myPreviousScreenRatio.ToString());
         sb.Append(") this.left=(");
         sb.Append(this.Left.ToString());
         sb.Append(")");
         System.Diagnostics.Debug.WriteLine(sb.ToString());
#endif
         //---------------------
         myIsDragging = true;
         e.Handled = true;
      }
      private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (false == myIsDragging)
         {
            base.OnMouseMove(e);
            return;
         }
         System.Windows.Point newPoint1 = this.PointToScreen(e.GetPosition(this));
         System.Windows.Media.Matrix currentMatrix = ScreenExtensions.GetMatrixFromVisual(this);
#if UT3
         System.Drawing.Point currentScreenPt = new System.Drawing.Point((int)newPoint1.X, (int)newPoint1.Y);
         Screen currentScreen = ScreenExtensions.GetScreenFromPoint(currentScreenPt);
         int currentScreenIndex = ScreenExtensions.GetScreenIndexFromPoint(currentScreenPt);
         string currentMonitor = ScreenExtensions.GetMonitor(this);
         uint dpiX = 1;
         uint dpiY = 1;
         ScreenExtensions.GetDpi(currentScreen, ScreenExtensions.DpiType.Effective, out dpiX, out dpiY);
         double scaleRatio = 96.0 / dpiX;
         double screenRatio = System.Windows.SystemParameters.PrimaryScreenWidth / ScreenExtensions.GetScreenResolutionWidthFromPoint(currentScreenPt);
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
            sb.Append(") scaleratio(");
            sb.Append(myPreviousScaleRatio.ToString("0.00"));
            sb.Append("=>");
            sb.Append(scaleRatio.ToString("0.00"));
            sb.Append(") r1(");
            sb.Append(myPreviousScreenRatio.ToString("0.00"));
            sb.Append("=>");
            sb.Append(screenRatio.ToString("0.00"));
            sb.Append(") psw(");
            sb.Append(SystemParameters.PrimaryScreenWidth.ToString());
            sb.Append(") this.Left(");
            sb.Append(this.Left.ToString());
            sb.Append(") dpiX(");
            sb.Append(dpiX.ToString());
            sb.Append(")");
            System.Diagnostics.Debug.WriteLine(sb.ToString());
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
            sb.Append(") scaleRatio(");
            sb.Append(myPreviousScaleRatio.ToString("0.00"));
            sb.Append("=>");
            sb.Append(scaleRatio.ToString("0.00"));
            sb.Append(") r1(");
            sb.Append(myPreviousScreenRatio.ToString("0.00"));
            sb.Append("=>");
            sb.Append(screenRatio.ToString("0.00"));
            sb.Append(") psw(");
            sb.Append(SystemParameters.PrimaryScreenWidth.ToString());
            sb.Append(") this.Left(");
            sb.Append(this.Left.ToString());
            sb.Append(") dpiX(");
            sb.Append(dpiX.ToString());
            sb.Append(")");
            System.Diagnostics.Debug.WriteLine(sb.ToString());
         }
         else
         {
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
            sb.Append(") scaleRatio(");
            sb.Append(myPreviousScaleRatio.ToString("0.00"));
            sb.Append("=>");
            sb.Append(scaleRatio.ToString("0.00"));
            sb.Append(") r1(");
            sb.Append(myPreviousScreenRatio.ToString("0.00"));
            sb.Append("=>");
            sb.Append(screenRatio.ToString("0.00"));
            sb.Append(") psw(");
            sb.Append(SystemParameters.PrimaryScreenWidth.ToString());
            sb.Append(") this.Left(");
            sb.Append(this.Left.ToString());
            sb.Append(") dpiX(");
            sb.Append(dpiX.ToString());
            sb.Append(")");
            System.Diagnostics.Debug.WriteLine(sb.ToString());
            sb.Append(this.Left.ToString());
         }
         this.Left = (currentScreenPt.X - myOffsetInBannerWindow.X) / currentMatrix.M11;
         this.Top = (currentScreenPt.Y - myOffsetInBannerWindow.Y) / currentMatrix.M22;
         myPreviousScreenRatio = screenRatio;
         myPreviousScaleRatio = scaleRatio;
         myPreviousMonitor = currentMonitor;
         myPreviousScreenIndex = currentScreenIndex;
         myPreviousScreenPoint = currentScreenPt;
#else
         this.Left = (newPoint1.X - myOffsetInBannerWindow.X) / currentMatrix.M11;
         this.Top = (newPoint1.Y - myOffsetInBannerWindow.Y) / currentMatrix.M22;
#endif
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
      private void myCheckBoxFont_Unchecked(object sender, RoutedEventArgs e)
      {
         theIsCheckBoxChecked = false;
         myIsReopen = true;
         Close();
      }
      private void myCheckBoxFont_Click(object sender, RoutedEventArgs e)
      {
         theIsCheckBoxChecked = true;
         myIsReopen = true;
         Close();
      }
   }
}
