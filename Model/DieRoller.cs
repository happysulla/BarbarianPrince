using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using WpfAnimatedGif;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public class DieRoller : IDieRoller
   {
      private const double ZOOM_DICE = 1.707;
      private RollEndCallback myCallbackEndRoll = null;
      private LoadEndCallback myCallbackEndLoad = null;
      private int myDieRollResults = 0;
      public bool CtorError { get; } = false;
      //-----------------------------------------------------------
      private Canvas myCanvas = null;
      public List<Button> theDice = null;
      private int myLoadedCount = 0;
      //-----------------------------------------------------------
      public DieRoller(Canvas c, LoadEndCallback callback = null)
      {
         myCallbackEndLoad = callback;
         myCanvas = c;
         if (null == theDice)
         {
            theDice = new List<Button>();
            if (false == ReadDiceXml(c))
            {
               Logger.Log(LogEnum.LE_ERROR, "DiceRoller() ReadDiceXml() return false");
               CtorError = true;
            }
         }
      }
      //-----------------------------------------------------------
      public void HideDie()
      {
         foreach (Button b in theDice)
         {
            b.Visibility = Visibility.Hidden;
            b.IsEnabled = false;
         }
      }
      public int RollStationaryDie(Canvas c, RollEndCallback cb)
      {
         myDieRollResults = 0;
         myCallbackEndRoll = cb;
         ScrollViewer sv = (ScrollViewer)c.Parent;
         foreach (Button b in theDice)
            HideDie();
         IMapPoint mp = GetCanvasCenter(sv, c);
         int randomNum = Utilities.RandomGenerator.Next(0, 6);
         int die1 = RollStationaryDie(mp, randomNum);
         if (0 == die1)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDice(): RollStationaryDie() returned 0");
            return 0;
         }
         myDieRollResults = die1;
         return myDieRollResults;
      }
      public int RollStationaryDice(Canvas c, RollEndCallback cb)
      {
         myDieRollResults = 0;
         myCallbackEndRoll = cb;
         ScrollViewer sv = (ScrollViewer)c.Parent;
         HideDie();
         IMapPoint mp = GetCanvasCenter(sv, c);
         IMapPoint mp1 = new MapPoint(mp.X, mp.Y - 0.65 * Utilities.theMapItemSize / Utilities.ZoomCanvas);
         int randomNum = Utilities.RandomGenerator.Next(0, 6);
         int die1 = RollStationaryDie(mp1, randomNum);
         if (0 == die1)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDice(): RollStationaryDie() returned 0");
            return 0;
         }
         //----------------------------------------------------------------------------------------
         IMapPoint mp2 = new MapPoint(mp.X, mp.Y + 0.65 * Utilities.theMapItemSize / Utilities.ZoomCanvas);
         randomNum = Utilities.RandomGenerator.Next(6, 12);
         int die2 = RollStationaryDie(mp2, randomNum);
         if (0 == die2)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDice(): RollStationaryDie() returned 0");
            return 0;
         }
         myDieRollResults = die1 + die2;
         return myDieRollResults;
      }
      public int RollMovingDie(Canvas c, RollEndCallback cb)
      {
         myDieRollResults = 0;
         myCallbackEndRoll = cb;
         ScrollViewer sv = (ScrollViewer)c.Parent;
         HideDie();
         IMapPoint mp = GetCanvasCenter(sv, c);
         int randomNum = Utilities.RandomGenerator.Next(0, 6);
         int die1 = RollMovingDie(sv, c, mp, randomNum);
         if (0 == die1)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDice(): RollStationaryDie() returned 0");
            return 0;
         }
         myDieRollResults = die1;
         return myDieRollResults;
      }
      public int RollMovingDice(Canvas c, RollEndCallback cb)
      {
         ScrollViewer sv = (ScrollViewer)c.Parent;
         HideDie();
         IMapPoint mp = GetCanvasCenter(sv, c);
         IMapPoint mp1 = new MapPoint(mp.X, mp.Y - 0.65 * Utilities.theMapItemSize / Utilities.ZoomCanvas);
         int randomNum = Utilities.RandomGenerator.Next(0, 6);
         int die1 = RollMovingDie(sv, c, mp1, randomNum);
         if (0 == die1)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDice(): 1-RollMovingDice() returned 0");
            return 0;
         }
         //--------------------------------------------------------
         myDieRollResults = 0;
         myCallbackEndRoll = cb;
         IMapPoint mp2 = new MapPoint(mp.X, mp.Y + 0.65 * Utilities.theMapItemSize / Utilities.ZoomCanvas);
         randomNum = Utilities.RandomGenerator.Next(6, 12);
         int die2 = RollMovingDie(sv, c, mp2, randomNum);
         if (0 == die2)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDice(): 2-RollMovingDice() returned 0");
            return 0;
         }
         myDieRollResults = die1 + die2;
         return myDieRollResults;
      }
      public int Roll3MovingDice(Canvas c, RollEndCallback cb)
      {
         ScrollViewer sv = (ScrollViewer)c.Parent;
         HideDie();
         IMapPoint mp = GetCanvasCenter(sv, c);
         IMapPoint mp1 = new MapPoint(mp.X, mp.Y - 0.65 * Utilities.theMapItemSize / Utilities.ZoomCanvas);
         int randomNum = Utilities.RandomGenerator.Next(0, 6);
         int die1 = RollMovingDie(sv, c, mp1, randomNum);
         if (0 == die1)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDice(): 1-Roll3MovingDice() returned 0");
            return 0;
         }
         //--------------------------------------------------------
         myDieRollResults = 0;
         myCallbackEndRoll = cb;
         IMapPoint mp2 = new MapPoint(mp.X, mp.Y + 0.65 * Utilities.theMapItemSize / Utilities.ZoomCanvas);
         randomNum = Utilities.RandomGenerator.Next(6, 12);
         int die2 = RollMovingDie(sv, c, mp2, randomNum);
         if (0 == die2)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDice(): 2-Roll3MovingDice() returned 0");
            return 0;
         }
         //--------------------------------------------------------
         myDieRollResults = 0;
         myCallbackEndRoll = cb;
         IMapPoint mp3 = new MapPoint(mp.X, mp.Y + 0.65 * Utilities.theMapItemSize / Utilities.ZoomCanvas);
         randomNum = Utilities.RandomGenerator.Next(12, 18);
         int die3 = RollMovingDie(sv, c, mp2, randomNum);
         if (0 == die3)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDice(): 3-Roll3MovingDice() returned 0");
            return 0;
         }
         myDieRollResults = die1 + die2 + die3;
         return myDieRollResults;
      }
      //-----------------------------------------------------------
      private bool ReadDiceXml(Canvas c)
      {
         IMapItems mapItems = new MapItems();
         XmlTextReader reader = null;
         try
         {
            // Load the reader with the data file and ignore all white space nodes.         
            reader = new XmlTextReader("../Config/DiceRolls.xml") { WhitespaceHandling = WhitespaceHandling.None };
            while (reader.Read())
            {
               if (reader.Name == "DiceRoll")
               {
                  if (reader.IsStartElement())
                  {
                     string imageName = reader.GetAttribute("value");
                     reader.Read();
                     string zoomStr = reader.GetAttribute("value");
                     Double zoom = Double.Parse(zoomStr);
                     reader.Read();
                     string topImageName = reader.GetAttribute("value");
                     //------------------------------------------------
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri("../../Images/" + Utilities.RemoveSpaces(topImageName) + ".gif", UriKind.Relative);
                     bmi.EndInit();
                     Image img = new Image { Source = bmi, Stretch = Stretch.Fill, Name = imageName };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     ImageBehavior.SetAutoStart(img, true);
                     ImageBehavior.SetRepeatBehavior(img, new RepeatBehavior(1));
                     ImageBehavior.AddAnimationLoadedHandler(img, ImageAnimationLoaded);
                     ImageBehavior.AddAnimationCompletedHandler(img, ImageAnimationCompleted);
                     //------------------------------------------------
                     Button b = new Button();
                     b.Name = Utilities.RemoveSpaces(imageName);
                     b.Width = zoom * Utilities.theMapItemSize;
                     b.Height = zoom * Utilities.theMapItemSize;
                     b.IsEnabled = true;
                     b.BorderThickness = new Thickness(0);
                     b.Background = new SolidColorBrush(Colors.Transparent);
                     b.Foreground = new SolidColorBrush(Colors.Transparent);
                     b.Visibility = Visibility.Hidden;
                     b.IsEnabled = true;
                     b.Content = img;
                     theDice.Add(b);
                     c.Children.Add(b);
                     b.Click += ClickButton;
                  } // end if
               } // end if
            } // end while
            return true;
         } // try
         catch (Exception e)
         {
            Console.WriteLine("Territory.CreateTerritories(): Exception:  e.Message={0} while reading reader.Name={1}", e.Message, reader.Name);
            return false;
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      private int RollStationaryDie(IMapPoint mp, int randomNum)
      {
         Button b = theDice[randomNum];
         if (null == theDice[randomNum])
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDie(): b=null");
            return 0;
         }
         double zoom = ZOOM_DICE / Utilities.ZoomCanvas;
         theDice[randomNum].Width = zoom * Utilities.theMapItemSize;
         theDice[randomNum].Height = zoom * Utilities.theMapItemSize;
         theDice[randomNum].IsEnabled = true;
         theDice[randomNum].Visibility = Visibility.Visible;
         Image img = (Image)theDice[randomNum].Content;
         var controller = ImageBehavior.GetAnimationController(img);
         if (null == controller)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDie(): controller=null img.Name=" + img.Name);
            return 0;
         }
         controller.GotoFrame(0);
         controller.Play();
         Canvas.SetLeft(theDice[randomNum], mp.X - zoom * Utilities.theMapItemOffset);
         Canvas.SetTop(theDice[randomNum], mp.Y - zoom * Utilities.theMapItemOffset);
         Canvas.SetZIndex(theDice[randomNum], 10000);
         return (randomNum % 6 + 1);
      }
      private int RollMovingDie(ScrollViewer sv, Canvas c, IMapPoint mp, int randomNum)
      {
         Button b = theDice[randomNum];
         if (null == b)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDie(): b=null");
            return 0;
         }
         double zoom = ZOOM_DICE / Utilities.ZoomCanvas;
         b.Width = zoom * Utilities.theMapItemSize;
         b.Height = zoom * Utilities.theMapItemSize;
         b.Visibility = Visibility.Visible;
         b.IsEnabled = true;
         Image img = (Image)b.Content;
         ImageAnimationController controller = ImageBehavior.GetAnimationController(img);
         if (null == controller)
         {
            Logger.Log(LogEnum.LE_ERROR, "RollStationaryDie(): controller=null img.Name=" + img.Name);
            return 0;
         }
         controller.GotoFrame(0);
         controller.Play();
         IMapPoint centerPoint = new MapPoint(mp.X - zoom * Utilities.theMapItemOffset, mp.Y - zoom * Utilities.theMapItemOffset);
         Canvas.SetLeft(b, centerPoint.X);
         Canvas.SetTop(b, centerPoint.Y);
         Canvas.SetZIndex(theDice[randomNum], 10000);
         Thread.Sleep(100);
         //----------------------------------------------------
         if (false == DiceAnimate(sv, c, b, centerPoint))
         {
            Logger.Log(LogEnum.LE_ERROR, "RollMovingDie(): MovePathAnimate() returned false");
            return 0;
         }
         return (randomNum % 6 + 1);
      }
      private bool DiceAnimate(ScrollViewer sv, Canvas c, Button b, IMapPoint startPoint)
      {
         // This algorithm determines the direction of the dice. They always start
         // In the negative X-directon. Based on the X-direction and Y-direction different
         // events can occur.  Each box edges are formed by either the Canvas or myScrollViewer
         // depending on which forms the smaller box. The value y= x * tan(ang).
         // X = min of Canvas.ActualWidth or myScrollViewer.ActualWidth
         // Y = min of Canvas.ActualHeight or myScrollViewer.ActualHeight
         // (x1,y1) = starting point
         //
         // Case #0: -X,-Y Direction: Die hits top wall before X left
         // Case #1: +X,-Y Direction: Die hits top wall before X right 
         //      (0,0)_____________________________
         //       |    \                    /     |     
         //     y1|     \                  /      | y1
         //       |      \                /       |
         //       |<----(x1,y1)       (x1,y1)---->|
         //       |   x1                    X-x1  |
         //       |                               |
         //       _______________________________(X,Y)
         // Case #2: -X,-Y Direction: Die hits X left before top 
         // Case #3: +X,-Y Direction: Die hits X right before top 
         //     (0,0)______________________________
         //       |                               |
         //       |\                             /|
         //       | \                           / |     
         //     y |  \                         /  | y
         //       |< --                       --->|
         //       |  x1                      X-x1 |
         //       |                               |
         //       _______________________________(X,Y)
         // Case #4: -X,+Y Direction: Die hits bottom  before X left
         // Case #5: +X,+Y Direction: Die hits bottom  before X right
         //     (0,0)_____________________________
         //       |                              |     
         //       |                              |
         //       |    x1                  X-x1  |
         //       |<------               ------->|
         //       |      /               \       |
         //  Y-y1 |     /                 \      | Y-y1
         //       |    /                   \     |
         //       |___/_____________________\___(X,Y)
         // Case #6: -X,+Y Direction:  Die hits X left before bottom 
         // Case #7: +X,+Y Direction:  Die hits X right before bottom 
         //      (0,0)____________________________
         //       | x1                      X-x1 |
         //       |<---                      --->|
         //       |  /                        \  |
         //     y | /                          \ | y
         //       |/                            \|
         //       |                              |
         //       |____________________________(X,Y)
         // For each case the distance of the die traveled is limited to a certain amount so that
         // the animination goes at a constant speed accross the path. The path distance is
         // Path Distance += sqrt(x^2 + y^2)
         try
         {
            const int ANIMATE_TIME_MS = 2200;
            const double PATH_DISTANCE = 1000;  // limits the animation path - distance & time correlate to speed of dice
            const double BUTTON_BOARDER = 15; // add 10 for the button border
            double dieSize = ZOOM_DICE * Utilities.theMapItemSize / Utilities.ZoomCanvas;
            PathFigure aPathFigure = new PathFigure() { StartPoint = new System.Windows.Point(startPoint.X, startPoint.Y) };
            double angle1InDegrees = Utilities.RandomGenerator.Next(20, 60);
            double angle1InRadians = angle1InDegrees * Math.PI / 180;
            double tangetOfAngle = Math.Tan(angle1InRadians);
            //--------------------------------------
            double borderButtonSize = BUTTON_BOARDER / Utilities.ZoomCanvas;
            double Xdelta = sv.HorizontalOffset / Utilities.ZoomCanvas;
            double Xleft = Xdelta - borderButtonSize; // add in thumb offset for horizontal scroll bar
            double Xright = (Xdelta + c.ActualWidth + BUTTON_BOARDER) - dieSize;
            if (Visibility.Visible == sv.ComputedHorizontalScrollBarVisibility)
               Xright = Xdelta + sv.ActualWidth / Utilities.ZoomCanvas + borderButtonSize - System.Windows.SystemParameters.VerticalScrollBarWidth - dieSize;
            //--------------------------------------
            double Ydelta = sv.VerticalOffset / Utilities.ZoomCanvas;
            double Ytop = Ydelta - borderButtonSize; // add in thumb offset for vertical scroll bar 
            double Ybottom = (Ydelta + c.ActualHeight + BUTTON_BOARDER) - dieSize;
            if (Visibility.Visible == sv.ComputedVerticalScrollBarVisibility)
               Ybottom = Ydelta + sv.ActualHeight / Utilities.ZoomCanvas + 2 * borderButtonSize - System.Windows.SystemParameters.HorizontalScrollBarHeight - dieSize;
            //--------------------------------------
            // 0=(-X,-Y)   1=(+X,-Y)  2=(-X,+Y)  3=(+X,+Y)
            //      (0,0)---------------------
            //       |           |           |
            //       |     0     |     1     |
            //       |           |           |
            //       |-----------|-----------|  
            //       |           |           |
            //       |     2     |     3     |
            //       |           |           |
            //       ----------------------(X,Y)
            int dieDirection = 0;
            if (0 < Utilities.RandomGenerator.Next(0, 2)) // Random set of either up or down
               dieDirection = 2;
            //--------------------------------------
            // Show the boundaries
            //CreateEllipse(Xleft, Ytop);
            //CreateEllipse(Xright, Ytop);
            //CreateEllipse(Xright, Ybottom);
            //CreateEllipse(Xleft, Ybottom);
            //CreateEllipse(startPoint.X, startPoint.Y);
            //--------------------------------------
            double x1 = startPoint.X;
            double y1 = startPoint.Y;
            double x2 = 0;
            double y2 = 0;
            double pathDistance = 0.0;       // limits the animation path
            string dieDirectionChange = "";  // logging variable 
            Logger.Log(LogEnum.LE_VIEW_DICE_MOVING, "DiceAnimate(): d=" + PATH_DISTANCE.ToString("####") + " (x1,y1)=" + startPoint.ToString() + " (Xd,Yd)=(" + Xdelta.ToString("###.") + "," + Ydelta.ToString("###.") + ") (Xl,Yt)=(" + Xleft.ToString("###.") + "," + Ytop.ToString("###.") + ") (Xr,Yb)=(" + Xright.ToString("###.") + "," + Ybottom.ToString("###.") + ")");
            for (int i = 0; i < 10; ++i)
            {
               switch (dieDirection)
               {
                  case 0: // -X,-Y
                     double yt0 = (x1 - Xdelta) * tangetOfAngle;
                     double yDiff0 = y1 - Ytop;
                     if (yDiff0 < yt0) // case #0: bounce off top
                     {
                        dieDirectionChange = "(-X,-Y)->(-X,+Y) c=0=top yt1=" + yt0.ToString("###") + " > ydiff=" + yDiff0.ToString("###");
                        dieDirection = 2; // 2=(-X,+Y) 
                        x2 = x1 - (yDiff0 / tangetOfAngle);
                        y2 = Ytop;
                     }
                     else // case #2 ==>  y < y1  bounce of X-left wall
                     {
                        dieDirectionChange = "(-X,-Y)->(+X,-Y) c=2=left";
                        dieDirection = 1; // 1=(+X,-Y)
                        x2 = Xleft;
                        y2 = y1 - yt0;
                     }
                     break;
                  case 1: // +X,-Y
                     double yt1 = (Xright - x1) * tangetOfAngle;
                     double yDiff1 = y1 - Ytop;
                     if (yDiff1 < yt1) // case #1: bounce off top
                     {
                        dieDirectionChange = "(+X,-Y)->(+X,+Y) c=1=top yt1=" + yt1.ToString("###") + " > ydiff=" + yDiff1.ToString("###");
                        dieDirection = 3; // 3=(+X,+Y)
                        x2 = x1 + (yDiff1 / tangetOfAngle);
                        y2 = Ytop;
                     }
                     else // case #3 ==>  yt1 < y1 bounce of X-right
                     {
                        dieDirectionChange = "(+X,-Y)->(-X,-Y) c=3=right";
                        dieDirection = 0; // 0=(-X,-Y)
                        x2 = Xright;
                        y2 = y1 - yt1;
                     }
                     break;
                  case 2: // -X,+Y
                     double yt2 = (x1 - Xleft) * tangetOfAngle;
                     double yDiff2 = Ybottom - y1;
                     if (yDiff2 < yt2) // case #4: bounce off bottom
                     {
                        dieDirectionChange = "(-X,+Y)->(-X,-Y) c=4=bottom" + yt2.ToString("###") + " > ydiff=" + yDiff2.ToString("###");
                        dieDirection = 0; // 0=(-X,-Y)
                        x2 = x1 - (yDiff2 / tangetOfAngle);
                        y2 = Ybottom;
                     }
                     else // case #6 ==> yt2 < (Y - y1) : bounce off X-left
                     {
                        dieDirectionChange = "(-X,+Y)->(+X,+Y) c=6=left";
                        dieDirection = 3; // 3=(+X,+Y)
                        x2 = Xleft;
                        y2 = y1 + yt2;
                     }
                     break;
                  case 3: // +X,+Y
                     double yt3 = (Xright - x1) * tangetOfAngle;
                     double yDiff3 = Ybottom - y1;
                     if (yDiff3 < yt3) // case #5: bounce off bottom
                     {
                        dieDirectionChange = "(+X,+Y)->(+X,-Y) c=5=bottom";
                        dieDirection = 1; // 1 = (+X,-Y)
                        x2 = x1 + (yDiff3 / tangetOfAngle);
                        y2 = Ybottom;
                     }
                     else // case #7 ==> yt3 < (Y - y1) : bounce off X-right
                     {
                        dieDirectionChange = "(+X,+Y)->(-X,+Y) c=7=right";
                        dieDirection = 2; // 2=(-X,+Y)
                        x2 = Xright;
                        y2 = y1 + yt3;
                     }
                     break;
                  default: Logger.Log(LogEnum.LE_ERROR, "DiceAnimate(): Reached default"); return false;
               }
               System.Windows.Point newPoint = new System.Windows.Point(x2, y2);
               LineSegment lineSegment = new LineSegment(newPoint, false);
               aPathFigure.Segments.Add(lineSegment);
               pathDistance += Math.Sqrt((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1));
               if (PATH_DISTANCE < pathDistance)
                  break;
               Logger.Log(LogEnum.LE_VIEW_DICE_MOVING, "DiceAnimate(): " + i.ToString() + ") (x1,y1)=(" + x1.ToString("####") + "," + y1.ToString("####") + ") (x2,y2)=(" + x2.ToString("####") + "," + y2.ToString("####") + ") ==>" + dieDirectionChange);
               x1 = x2;
               y1 = y2;
            }
            //-------------------------------------------------------------------
            PathGeometry aPathGeo = new PathGeometry();
            aPathGeo.Figures.Add(aPathFigure);
            aPathGeo.Freeze();
            DoubleAnimationUsingPath xAnimiation = new DoubleAnimationUsingPath
            {
               DecelerationRatio = 0.9,
               PathGeometry = aPathGeo,
               Duration = TimeSpan.FromMilliseconds(ANIMATE_TIME_MS),
               Source = PathAnimationSource.X
            };
            DoubleAnimationUsingPath yAnimiation = new DoubleAnimationUsingPath
            {
               DecelerationRatio = 0.9,
               PathGeometry = aPathGeo,
               Duration = TimeSpan.FromMilliseconds(ANIMATE_TIME_MS),
               Source = PathAnimationSource.Y
            };
            b.RenderTransform = new TranslateTransform();
            b.BeginAnimation(Canvas.LeftProperty, xAnimiation);
            b.BeginAnimation(Canvas.TopProperty, yAnimiation);
            return true;
         }
         catch (Exception e)
         {
            b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
            b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
            Console.WriteLine("DiceAnimate() - EXCEPTION THROWN e={0}", e.ToString());
            return false;
         }
      }
      private bool Reset(Canvas c, Image img)
      {
         ScrollViewer sv = (ScrollViewer)c.Parent;
         Button b = (Button)img.Parent;
         if (null == b)
         {
            Logger.Log(LogEnum.LE_ERROR, "Reset(): b=null");
            return false;
         }
         double positionX = Canvas.GetLeft(b);
         double positionY = Canvas.GetTop(b);
         Canvas.SetLeft(b, positionX);
         Canvas.SetTop(b, positionY);
         b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
         b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
         return true;
      }
      private IMapPoint GetCanvasCenter(ScrollViewer sv, Canvas c)
      {
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
      private void CreateEllipse(double x, double y)
      {
         // For debugging, show elipses of die boundaries
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
      //-----------------------------------------------------------
      private void ClickButton(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         b.Visibility = Visibility.Hidden;
         b.IsEnabled = false;
      }
      private void ImageAnimationLoaded(object sender, RoutedEventArgs e)
      {
         if (null != myCallbackEndLoad)
         {
            if (12 == ++myLoadedCount) // when 12 dice are loaded
               myCallbackEndLoad();
         }
      }
      private void ImageAnimationCompleted(object sender, RoutedEventArgs e)
      {
         Image img = (Image)sender;
         if (false == Reset(myCanvas, img))
         {
            Logger.Log(LogEnum.LE_ERROR, "ImageAnimationCompleted(): Reset() returned false");
            return;
         }
         if (null != myCallbackEndRoll)
         {
            myCallbackEndRoll(myDieRollResults);
            myCallbackEndRoll = null; // only do one callback per dice roll
         }

      }
   }
}
