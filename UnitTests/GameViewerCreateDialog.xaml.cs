using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BarbarianPrince
{
    public partial class GameViewerCreateDialog : System.Windows.Window
    {
        private bool myIsFirstShowing = true;
        private DockPanel TopPanel { get; set; } = null;
        public GameViewerCreateDialog(ref DockPanel topPanel)
        {
            if (null == topPanel)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog() dockPanel=null");
                return;
            }
            TopPanel = topPanel;
            DockPanel dockPanelInside = null;
            DockPanel dockPanelControls = null;
            ScrollViewer scrollViewer = null;
            Canvas canvas = null;
            Image image = null;
            foreach (UIElement ui0 in TopPanel.Children) // top panel holds myMainMenu, myDockePanelInside, and myStatusBar
            {
                if (ui0 is DockPanel) // myDockPanelInside holds myScrollViewerInside (which holds canvas) and myDockPanelControls
                {
                    dockPanelInside = (DockPanel)ui0;
                    foreach (UIElement ui1 in dockPanelInside.Children)
                    {
                        if (ui1 is ScrollViewer)
                        {
                            scrollViewer = (ScrollViewer)ui1;
                            if (scrollViewer.Content is Canvas)
                            {
                                canvas = (Canvas)scrollViewer.Content;
                                foreach (UIElement ui2 in canvas.Children)
                                {
                                    if (ui2 is Image)
                                    {
                                        image = (Image)ui2;
                                        break;
                                    }
                                }
                            }
                        }
                        if (ui1 is DockPanel)
                            dockPanelControls = (DockPanel)ui1;
                    }
                    break;
                }
            }
            if (null == image)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog() image=null");
                return;
            }
            if (null == dockPanelControls)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog() dockPanelControls=null");
                return;
            }
            InitializeComponent();
            myTextBoxScaleTransform.Text = Utilities.ZoomCanvas.ToString();
            myTextBoxImageSizeX.Text = image.ActualWidth.ToString();
            myTextBoxImageSizeY.Text = image.ActualHeight.ToString();
            myTextBoxCanvasSizeX.Text = canvas.ActualWidth.ToString();
            myTextBoxCanvasSizeY.Text = canvas.ActualHeight.ToString();
            myTextBoxScrollViewerSizeX.Text = scrollViewer.ActualWidth.ToString();
            myTextBoxScrollViewerSizeY.Text = scrollViewer.ActualHeight.ToString();
            myTextBoxVerticalOffset.Text = scrollViewer.VerticalOffset.ToString();
            myTextBoxHorizontalOffset.Text = scrollViewer.HorizontalOffset.ToString();
            myTextBoxDockPanelControlsSizeX.Text = dockPanelControls.ActualWidth.ToString();
            myTextBoxDockPanelSizeX.Text = dockPanelInside.ActualWidth.ToString();
            myTextBoxDockPanelSizeY.Text = dockPanelInside.ActualHeight.ToString();
            myTextBoxTopPanelSizeX.Text = TopPanel.ActualWidth.ToString();
            myTextBoxTopPanelSizeY.Text = TopPanel.ActualHeight.ToString();

            myTextBoxScreenSizeX.Text = System.Windows.SystemParameters.PrimaryScreenWidth.ToString();
            myTextBoxScreenSizeY.Text = System.Windows.SystemParameters.PrimaryScreenHeight.ToString();
            myTextBoxVerticalThumbSizeX.Text = System.Windows.SystemParameters.VerticalScrollBarButtonHeight.ToString();
            myTextBoxVerticalThumbSizeY.Text = System.Windows.SystemParameters.VerticalScrollBarWidth.ToString();
        }
        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            DockPanel dockPanelInside = null;
            DockPanel dockPanelControls = null;
            ScrollViewer scrollViewer = null;
            Canvas canvas = null;
            Image image = null;
            foreach (UIElement ui0 in TopPanel.Children)
            {
                if (ui0 is DockPanel)
                {
                    dockPanelInside = (DockPanel)ui0;
                    foreach (UIElement ui1 in dockPanelInside.Children)
                    {
                        if (ui1 is ScrollViewer)
                        {
                            scrollViewer = (ScrollViewer)ui1;
                            if (scrollViewer.Content is Canvas)
                            {
                                canvas = (Canvas)scrollViewer.Content;
                                foreach (UIElement ui2 in canvas.Children)
                                {
                                    if (ui2 is Image)
                                    {
                                        image = (Image)ui2;
                                        break;
                                    }
                                }
                            }
                        }
                        if (ui1 is DockPanel)
                            dockPanelControls = (DockPanel)ui1;
                    }
                    break;
                }
            }
            if (null == image)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog() image=null");
                return;
            }
            if (null == dockPanelControls)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog() dockPanelControls=null");
                return;
            }
            dockPanelInside.Height = Double.Parse(myTextBoxDockPanelSizeY.Text);
            dockPanelInside.Width = Double.Parse(myTextBoxDockPanelSizeX.Text);
            dockPanelControls.Width = Double.Parse(myTextBoxDockPanelControlsSizeX.Text);
            scrollViewer.Height = Double.Parse(myTextBoxScrollViewerSizeY.Text);
            scrollViewer.Width = Double.Parse(myTextBoxScrollViewerSizeX.Text);
            canvas.Height = Double.Parse(myTextBoxCanvasSizeY.Text);
            canvas.Width = Double.Parse(myTextBoxCanvasSizeX.Text);
            image.Height = Double.Parse(myTextBoxImageSizeY.Text);
            image.Width = Double.Parse(myTextBoxImageSizeX.Text);
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void TextBoxScaleTransform_TextChanged(object sender, TextChangedEventArgs e)
        {
            Canvas canvas = null;
            foreach (UIElement ui0 in TopPanel.Children)
            {
                if (ui0 is DockPanel dockPanelInside)
                {
                    dockPanelInside = (DockPanel)ui0;
                    foreach (UIElement ui1 in dockPanelInside.Children)
                    {
                        if (ui1 is ScrollViewer scrollViewer)
                        {
                            if (scrollViewer.Content is Canvas)
                            {
                                canvas = (Canvas)scrollViewer.Content;
                                break;
                            }
                        }
                    }
                }
            }
            if (null == canvas)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog() canvas=null");
                return;
            }
            if (false == myIsFirstShowing) // do not zoom when window is first shown
            {
                Utilities.ZoomCanvas = Double.Parse(myTextBoxScaleTransform.Text);
                canvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
            }
            else
            {
                myIsFirstShowing = false;
            }

        }
    }
}
