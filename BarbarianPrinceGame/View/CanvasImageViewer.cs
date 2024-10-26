using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace BarbarianPrince
{
   internal class CanvasImageViewer : IView
   {
      public bool CtorError { get; } = false;
      private Canvas myCanvas = null;
      //-------------------------------------------------
      public CanvasImageViewer(Canvas c)
      {
         if (null == c)
         {
            Logger.Log(LogEnum.LE_ERROR, "CanvasImageViewer(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
      }
      //-------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         switch (action)
         {
            case GameAction.E228ShowTrueLove:
               ShowTrueLove(myCanvas);
               break;
            case GameAction.EndGameWin:
               ShowEndGameSuccess(myCanvas);
               break;
            case GameAction.EndGameLost:
               ShowEndGameFail(myCanvas);
               break;
            default:
               break;
         }
      }
      //-------------------------------------------------
      private IMapPoint GetCanvasCenter(Canvas c)
      {
         ScrollViewer sv = (ScrollViewer)c.Parent;
         double x = 0.0;
         if (c.ActualWidth < sv.ActualWidth / Utilities.ZoomCanvas)
            x = c.ActualWidth / 2 + sv.HorizontalOffset;
         else
            x = sv.ActualWidth / (2 * Utilities.ZoomCanvas) + sv.HorizontalOffset / Utilities.ZoomCanvas;
         double y = 0.0;
         if (c.ActualHeight < sv.ActualHeight / Utilities.ZoomCanvas)
            y = c.ActualHeight / 2 + sv.VerticalOffset;
         else
            y = sv.ActualHeight / (2 * Utilities.ZoomCanvas) + sv.VerticalOffset / Utilities.ZoomCanvas;
         IMapPoint mp = (IMapPoint)new MapPoint(x, y);
         return mp;
      }
      private void ShowTrueLove(Canvas c)
      {
         BitmapImage bmi2 = new BitmapImage();
         bmi2.BeginInit();
         bmi2.UriSource = new Uri(MapImage.theImageDirectory + "FallingHearts.gif", UriKind.Absolute);
         bmi2.EndInit();
         Image img = new Image { Source = bmi2, Height = c.ActualHeight, Width = c.ActualWidth, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi2);
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 99999);
      }
      private void ShowEndGameSuccess(Canvas c)
      {
         c.LayoutTransform = new ScaleTransform(1.0, 1.0);
         BitmapImage bmi1 = new BitmapImage();
         int randomNum = Utilities.RandomGenerator.Next(5);
         bmi1.BeginInit();
         if (0 == randomNum)
            bmi1.UriSource = new Uri(MapImage.theImageDirectory + "EndGameSuccess.gif", UriKind.Absolute);
         else
            bmi1.UriSource = new Uri(MapImage.theImageDirectory + "EndGameSuccess2.gif", UriKind.Absolute);
         bmi1.EndInit();
         Image img = new Image { Source = bmi1, Height = c.ActualHeight, Width = c.ActualWidth, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi1);
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 99999);
      }
      private void ShowEndGameFail(Canvas c)
      {
         c.LayoutTransform = new ScaleTransform(1.0, 1.0);
         BitmapImage bmi1 = new BitmapImage();
         bmi1.BeginInit();
         bmi1.UriSource = new Uri(MapImage.theImageDirectory + "EndGameFail.gif", UriKind.Absolute);
         bmi1.EndInit();
         Image img = new Image { Source = bmi1, Height = c.ActualHeight, Width = c.ActualWidth, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi1);
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 99999);
      }
   }
}
