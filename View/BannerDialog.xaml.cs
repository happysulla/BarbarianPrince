using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
    public partial class BannerDialog : System.Windows.Window
    {
        public bool CtorError { get; } = false;
        private bool myIsDragging = false;
        private System.Windows.Point myPreviousLocation;
        private string myKey = "";
        public string Key { get => myKey; }
        //private TextBlock myTextBlockDisplay = null;
        public TextBlock TextBoxDiplay { get => myTextBlockDisplay; }
        public BannerDialog(string key, StringReader sr)
        {
            InitializeComponent();
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
            myPreviousLocation = e.GetPosition(this);
            myIsDragging = true;
            e.Handled = true;
        }
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("w=");
            sb.Append(myBannerDialog.ActualHeight.ToString());
            sb.Append(" dp=");
            sb.Append(myDockPanel.ActualHeight.ToString());
            sb.Append(" sv=");
            sb.Append(myScrollViewerTextBlock.ActualHeight.ToString());
            sb.Append(" tb=");
            sb.Append(myTextBlockDisplay.ActualHeight.ToString());
            System.Windows.MessageBox.Show(sb.ToString());
            myBannerDialog.Height = myTextBlockDisplay.ActualHeight;
            myDockPanel.Height = myTextBlockDisplay.ActualHeight;
        }
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (true == myIsDragging)
            {
                System.Windows.Point newPoint = this.PointToScreen(e.GetPosition(this));  // Find the current mouse position in screen coordinates.
                newPoint.Offset(-myPreviousLocation.X, -myPreviousLocation.Y); // Compensate for the position the control was clicked.
                this.Left = newPoint.X; // Move the window.
                this.Top = newPoint.Y;
            }
            base.OnMouseMove(e);
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            myIsDragging = false;
        }
    }
}
