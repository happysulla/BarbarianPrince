using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BarbarianPrince
{
   public partial class GameViewerCreateDialog : System.Windows.Window
   {
      private bool myIsFirstShowing = true;
      private DockPanel TopPanel { get; set; } = null;
      Canvas myCanvas = null;
      ScrollViewer myScrollViewer = null;
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

         myCanvas = null;
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
                     myScrollViewer = (ScrollViewer)ui1;
                     if (myScrollViewer.Content is Canvas)
                     {
                        myCanvas = (Canvas)myScrollViewer.Content;
                        foreach (UIElement ui2 in myCanvas.Children)
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
         myTextBoxCanvasSizeX.Text = myCanvas.ActualWidth.ToString();
         myTextBoxCanvasSizeY.Text = myCanvas.ActualHeight.ToString();
         myTextBoxScrollViewerSizeX.Text = myScrollViewer.ActualWidth.ToString();
         myTextBoxScrollViewerSizeY.Text = myScrollViewer.ActualHeight.ToString();


         myTextBoxVScrollableHeight.Text = myScrollViewer.ScrollableHeight.ToString();
         myTextBoxVerticalOffset.Text = myScrollViewer.VerticalOffset.ToString();
         myTextBoxScrollableHeightPercent.Text = myScrollViewer.ScrollableHeight.ToString();
         int heightPercent = (int)(100.0 * (myScrollViewer.VerticalOffset / myScrollViewer.ScrollableHeight));
         myTextBoxScrollableHeightPercent.Text = heightPercent.ToString();
         double heightNormalized = myScrollViewer.ScrollableHeight / Utilities.ZoomCanvas;
         myTextBoxScrollableHeightNormalized.Text = heightNormalized.ToString();

         myTextBoxScrollableWidth.Text = myScrollViewer.ScrollableWidth.ToString();
         myTextBoxHorizontalOffset.Text = myScrollViewer.HorizontalOffset.ToString();
         myTextBoxScrollableWidthPercent.Text = myScrollViewer.ScrollableWidth.ToString();
         double widthPercent = (int)(100.0 * (myScrollViewer.HorizontalOffset / myScrollViewer.ScrollableWidth));
         myTextBoxScrollableWidthPercent.Text = widthPercent.ToString();
         double widthNormalized = myScrollViewer.ScrollableWidth / Utilities.ZoomCanvas;
         myTextBoxScrollableWidthNormalized.Text = widthNormalized.ToString();

         myTextBoxDockPanelControlsSizeX.Text = dockPanelControls.ActualWidth.ToString();
         myTextBoxDockPanelSizeX.Text = dockPanelInside.ActualWidth.ToString();
         myTextBoxDockPanelSizeY.Text = dockPanelInside.ActualHeight.ToString();
         myTextBoxTopPanelSizeX.Text = TopPanel.ActualWidth.ToString();
         myTextBoxTopPanelSizeY.Text = TopPanel.ActualHeight.ToString();

         myTextBoxScreenSizeX.Text = System.Windows.SystemParameters.PrimaryScreenWidth.ToString();
         myTextBoxScreenSizeY.Text = System.Windows.SystemParameters.PrimaryScreenHeight.ToString();
         myTextBoxVerticalThumbSizeX.Text = System.Windows.SystemParameters.VerticalScrollBarButtonHeight.ToString();
         myTextBoxVerticalThumbSizeY.Text = System.Windows.SystemParameters.VerticalScrollBarWidth.ToString();

         myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDown_Canvas;
         myCanvas.MouseRightButtonDown += this.MouseRIghtButtonDown_Canvas;
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
      private void MouseLeftButtonDown_Canvas(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition(myCanvas);
         double percentHeightB = (p.Y / myCanvas.ActualHeight);
         double percentHeight = percentHeightB;
         double percentToScroll = 0.0;
         if (percentHeight < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentHeight)
            percentToScroll = 1.0;
         else
            percentToScroll = percentHeight / 0.5 - 0.5;
         double amountToScroll = percentToScroll * myScrollViewer.ScrollableHeight;
         myScrollViewer.ScrollToVerticalOffset(amountToScroll);
         StringBuilder sb = new StringBuilder();
         sb.Append(" %B=");
         sb.Append(percentHeightB.ToString("#.##"));
         sb.Append(" % =");
         if( 0.0 == percentHeight)
            sb.Append("0.00");
         else
            sb.Append(percentHeight.ToString("#.##"));
         sb.Append(" %Scroll=");
         if (0.0 == percentToScroll)
            sb.Append("0.00");
         else
            sb.Append(percentToScroll.ToString("#.##"));
         sb.Append(" amountToScroll=");
         if (0.0 == amountToScroll)
            sb.Append("0.00");
         else
            sb.Append(amountToScroll.ToString("####.#"));
         sb.Append(" out of ");
         sb.Append(myScrollViewer.ScrollableHeight.ToString("####.#"));
         Console.WriteLine(sb.ToString());

         double percentWidthB = (p.X / myCanvas.ActualWidth);
         double percentWidth = percentWidthB;
         percentToScroll = 0.0;
         if (percentWidth < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentWidth)
            percentToScroll = 1.0;
         else
            percentToScroll = percentWidth / 0.5 - 0.5;
          amountToScroll = percentToScroll * myScrollViewer.ScrollableWidth;
         myScrollViewer.ScrollToHorizontalOffset(amountToScroll);
         e.Handled = true;
      }
      private void MouseRIghtButtonDown_Canvas(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition(myCanvas);
         double percentHeight = 100.0 * (p.Y / myCanvas.ActualHeight);
         double percentWidth = 100.0 * (p.X / myCanvas.ActualWidth);
         StringBuilder sb = new StringBuilder();
         sb.Append("X=");
         sb.Append(p.X.ToString("##.#"));
         sb.Append("\t%=");
         string spWidth = percentWidth.ToString("##");
         sb.Append(spWidth);
         sb.Append("\nY=");
         sb.Append(p.Y.ToString("##.#"));
         sb.Append("\t%=");
         string spHeight = percentHeight.ToString("##");
         sb.Append(spHeight);


         MessageBox.Show(sb.ToString());
         e.Handled = true;
      }
   }
}
