using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            if (ret.Contains(mousePoint))
               return i;
         }
         return 0;
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
         //---------------------
         StringBuilder sb = new StringBuilder();
         sb.Append(" pt=");
         sb.Append(myPreviousScreenPoint.ToString());
         sb.Append("\nis=");
         sb.Append(myInitialScreen.ToString());
         Console.WriteLine(sb.ToString());
         //---------------------
         myIsDragging = true;
         e.Handled = true;
      }
      private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (true == myIsDragging)
         {
            System.Windows.Point newPoint = this.PointToScreen(e.GetPosition(this));  // Find the current mouse position in screen coordinates.
            System.Drawing.Point pt = new System.Drawing.Point((int)newPoint.X, (int)newPoint.Y);
            int newScreen = ConvertMousePointToScreenIndex(pt);
            if (newScreen != myInitialScreen)
            {
               // Stop moving the window when mouse shows up on new screen
               // Allows user to move to new screen by grabbing on new screen
            }
            else
            {
               newPoint.Offset(-myOffsetInBannerWindow.X, -myOffsetInBannerWindow.Y); // Compensate for the position the control was clicked.
               this.Left = newPoint.X; // Move the window.
               this.Top = newPoint.Y;
            }
         }
         base.OnMouseMove(e);
      }
      private void Window_MouseUp(object sender, MouseButtonEventArgs e)
      {
         myIsDragging = false;
      }
   }
}
