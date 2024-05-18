using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
    public partial class TableDialog : Window
    {
        public bool CtorError { get; } = false;
        private bool myIsDragging = false;
        private System.Windows.Point myPreviousLocation;
        private string myKey = "";
        public string Key { get => myKey; }
        private FlowDocument myFlowDocumentContent = null;
        public FlowDocument FlowDocumentContent { get => myFlowDocumentContent; }
        public TableDialog(string key, StringReader sr)
        {
            InitializeComponent();
            try
            {
                XmlTextReader xr = new XmlTextReader(sr);
                myFlowDocumentContent = (FlowDocument)XamlReader.Load(xr);
                myFlowDocumentScrollViewer.Document = myFlowDocumentContent;
                myKey = key;
            }
            catch (Exception e)
            {
                Logger.Log(LogEnum.LE_ERROR, " e=" + e.ToString() + " sr.content=\n" + sr.ToString());
                CtorError = true;
                return;
            }
        }
        //-------------------------------------------------------------------------
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void FlowDocument_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            myPreviousLocation = e.GetPosition(this);
            myIsDragging = true;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            myPreviousLocation = e.GetPosition(this);
            myIsDragging = true;
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
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
