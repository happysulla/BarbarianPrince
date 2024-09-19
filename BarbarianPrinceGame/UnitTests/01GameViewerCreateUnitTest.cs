using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BarbarianPrince
{
    public class GameViewerCreateUnitTest : IUnitTest
    {
        //--------------------------------------------------------------------
        private DockPanel myDockPanel = null;
        private ScrollViewer myScrollViewerCanvas = null;
        private Canvas myCanvas = null;
        //--------------------------------------------------------------------
        private int myIndexName = 0;
        private List<string> myHeaderNames = new List<string>();
        private List<string> myCommandNames = new List<string>();
        public bool CtorError { get; } = false;
        public string HeaderName { get { return myHeaderNames[myIndexName]; } }
        public string CommandName { get { return myCommandNames[myIndexName]; } }
        //--------------------------------------------------------------------
        public GameViewerCreateUnitTest(DockPanel dp)
        {
            //------------------------------------
            myIndexName = 0;
            myHeaderNames.Add("01-Frame Sizes");
            myHeaderNames.Add("01-Canvas CenterPoint");
            myHeaderNames.Add("01-Finish");
            //------------------------------------
            myCommandNames.Add("Show Dialog");
            myCommandNames.Add("Show Center");
            myCommandNames.Add("Finish");
            //------------------------------------
            myDockPanel = dp;
            foreach (UIElement ui0 in dp.Children)
            {
                if (ui0 is DockPanel dockPanelInside)
                {
                    foreach (UIElement ui1 in dockPanelInside.Children)
                    {
                        if (ui1 is ScrollViewer)
                        {
                            myScrollViewerCanvas = (ScrollViewer)ui1;
                            if (myScrollViewerCanvas.Content is Canvas)
                                myCanvas = (Canvas)myScrollViewerCanvas.Content;  // Find the Canvas in the visual tree
                        }
                    }
                }
            }
            if (null == myCanvas) // log error and return if canvas not found
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvas=null");
                CtorError = true;
                return;
            }
        }
        public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
        {
            if (CommandName == myCommandNames[0])
            {
                GameViewerCreateDialog dialog = new GameViewerCreateDialog(ref myDockPanel); // Get the name from user
                dialog.Show();
            }
            else if (CommandName == myCommandNames[1])
            {
                IMapPoint mp = GetCanvasCenter(myScrollViewerCanvas, myCanvas);
                CreateEllipse(mp.X, mp.Y); // Add new elipses
            }
            else if (CommandName == myCommandNames[2])
            {
                if (false == Cleanup(ref gi))
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): Cleanup() return falsed");
                    return false;
                }
            }
            return true;
        }
        public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
        {
            if (HeaderName == myHeaderNames[0])
            {
                ++myIndexName;
            }
            else if (HeaderName == myHeaderNames[1])
            {
                ++myIndexName;
            }
            else if (HeaderName == myHeaderNames[2])
            {
                if (false == Cleanup(ref gi))
                {
                    Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() return falsed");
                    return false;
                }
            }
            return true;
        }
        public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
        {
            //--------------------------------------------------
            // Remove any existing UI elements from the Canvas
            List<UIElement> results = new List<UIElement>();
            foreach (UIElement ui in myCanvas.Children)
            {
                if (ui is Ellipse)
                    results.Add(ui);
                if (ui is Button b)
                {
                    if (true == b.IsVisible)
                        results.Add(ui);
                }
            }
            foreach (UIElement ui1 in results)
                myCanvas.Children.Remove(ui1);
            //--------------------------------------------------
            ++gi.GameTurn;
            return true;
        }
        //--------------------------------------------------------------------
        private IMapPoint GetCanvasCenter(ScrollViewer scrollViewer, Canvas canvas)
        {
            double x = 0.0;
            if (canvas.ActualWidth < scrollViewer.ActualWidth / Utilities.ZoomCanvas)
                x = canvas.ActualWidth / 2 + scrollViewer.HorizontalOffset;
            else
                x = scrollViewer.ActualWidth / (2 * Utilities.ZoomCanvas) + scrollViewer.HorizontalOffset / Utilities.ZoomCanvas;
            double y = 0.0;
            if (canvas.ActualHeight < scrollViewer.ActualHeight / Utilities.ZoomCanvas)
                y = canvas.ActualHeight / 2 + scrollViewer.VerticalOffset;
            else
                y = scrollViewer.ActualHeight / (2 * Utilities.ZoomCanvas) + scrollViewer.VerticalOffset / Utilities.ZoomCanvas;
            IMapPoint mp = (IMapPoint)new MapPoint(x, y);
            return mp;
        }
        private void CreateEllipse(double x, double y)
        {
            List<UIElement> results = new List<UIElement>(); // Remove old ellipse
            foreach (UIElement ui in myCanvas.Children)
            {
                if (ui is Ellipse)
                    results.Add(ui);
            }
            foreach (UIElement ui1 in results)
                myCanvas.Children.Remove(ui1);
            //-------------------------------Add new ellipse
            SolidColorBrush brushBlack = new SolidColorBrush();
            Ellipse aEllipse = new Ellipse
            {
                Tag = Utilities.RemoveSpaces("CenterPoint"),
                Fill = Brushes.Black,
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Width = 30,
                Height = 30
            };
            Canvas.SetLeft(aEllipse, x);
            Canvas.SetTop(aEllipse, y);
            myCanvas.Children.Add(aEllipse);
        }
    }
}

