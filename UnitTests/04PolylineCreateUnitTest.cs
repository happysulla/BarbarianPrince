using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public class PolylineCreateUnitTest : IUnitTest
   {
      public static Double theEllipseOffset = 8;
      //--------------------------------------------------------
      private DockPanel myDockPanel = null;
      private Canvas myCanvas = null;
      private IGameInstance myGameInstance = null;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      private List<IMapPoint> myPoints = new List<IMapPoint>();
      private Dictionary<string, Polyline> myRivers = new Dictionary<string, Polyline>();
      //--------------------------------------------------------
      private SolidColorBrush mySolidColorBrush3 = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      //--------------------------------------------------------
      public bool CtorError { get; } = false;
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //--------------------------------------------------------
      public PolylineCreateUnitTest(DockPanel dp, IGameInstance gi)
      {
         myIndexName = 0;
         myHeaderNames.Add("04-Start Polyline Test");
         myHeaderNames.Add("04-Dienstal Branch");
         myHeaderNames.Add("04-Largos River");
         myHeaderNames.Add("04-Nesser River");
         myHeaderNames.Add("04-Greater Nesser River");
         myHeaderNames.Add("04-Lesser Nesser River");
         myHeaderNames.Add("04-Trogoth River");
         myHeaderNames.Add("04-Finish");
         //------------------------------------
         myCommandNames.Add("00-Start");
         myCommandNames.Add("01-Dienstal Branch");
         myCommandNames.Add("02-Largos River");
         myCommandNames.Add("03-Nesser River");
         myCommandNames.Add("04-Greater Nesser River");
         myCommandNames.Add("05-Lesser Nesser River");
         myCommandNames.Add("06-Trogoth River");
         myCommandNames.Add("Cleanup");
         //------------------------------------
         myDockPanel = dp;
         foreach (UIElement ui0 in myDockPanel.Children)
         {
            if (ui0 is DockPanel dockPanelInside)
            {
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer sv)
                  {
                     if (sv.Content is Canvas canvas)
                        myCanvas = canvas;  // Find the Canvas in the visual tree
                  }
               }
            }
         }
         if (null == myCanvas) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest() myCanvas=null");
            CtorError = true;
            return;
         }
         //------------------------------------
         this.myGameInstance = gi;

         myCanvas.MouseDown += MouseDownCanvas;
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (CommandName == myCommandNames[0])
         {
            CreateEllipses();
            ReadRiversXml();
            CreateTriangles();
            if( false == NextTest(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command() NextTest() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[1])
         {
         }
         else if (CommandName == myCommandNames[2])
         {
         }
         else if (CommandName == myCommandNames[3])
         {
         }
         else if (CommandName == myCommandNames[4])
         {
         }
         else if (CommandName == myCommandNames[5])
         {
         }
         else if (CommandName == myCommandNames[6])
         {
         }
         else
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Cleanup() returned false");
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
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[4])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[5])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[6])
         {
            ++myIndexName;
         }
         else
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi) 
      {
         try
         {
            System.IO.File.Delete("../Config/Rivers.xml");  // delete old file
            XmlDocument aXmlDocument = CreateXml(); // create a new XML document based on Territories
            using (FileStream writer = new FileStream("../Config/Rivers.xml", FileMode.OpenOrCreate, FileAccess.Write))
            {
               XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
               using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
               {
                  aXmlDocument.Save(xmlWriter);
               }
            }
         }
         catch (Exception e)
         {
            Console.WriteLine("TerritoryUnitTest.Command() exeption={0}", e.Message);
            return false;
         }
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
            if (ui is Polyline)
               results.Add(ui);
            if (ui is Polygon p)
               p.Fill = Utilities.theBrushRegionClear;
         }
         foreach (UIElement ui1 in results)
            myCanvas.Children.Remove(ui1);
         myCanvas.MouseDown -= MouseDownCanvas;
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------
      private void ReadRiversXml()
      {
         XmlTextReader reader = null;
         PointCollection points = null;
         string name = null;
         try
         {
            reader = new XmlTextReader("../Config/Rivers.xml") { WhitespaceHandling = WhitespaceHandling.None }; // Load the reader with the data file and ignore all white space nodes.    
            while (reader.Read())
            {
               if (reader.Name == "River")
               {
                  points = new PointCollection();
                  if (reader.IsStartElement())
                  {
                     name = reader.GetAttribute("value");
                     while (reader.Read())
                     {
                        if ((reader.Name == "point" && (reader.IsStartElement())))
                        {
                           string value = reader.GetAttribute("X");
                           Double X1 = Double.Parse(value);
                           value = reader.GetAttribute("Y");
                           Double Y1 = Double.Parse(value);
                           points.Add(new System.Windows.Point(X1, Y1));
                        }
                        else
                        {
                           break;
                        }
                     }  // end while
                  } // end if
                  Polyline polyline = new Polyline { Points = points, Stroke = mySolidColorBrush3, StrokeThickness = 2, Visibility = Visibility.Visible };
                  myRivers[name] = polyline;
                  Canvas.SetZIndex(polyline, 0);
                  myCanvas.Children.Add(polyline);
               } // end if
            } // end while
         } // try
         catch (Exception e)
         {
            Console.WriteLine("ReadTerritoriesXml(): Exception:  e.Message={0} while reading reader.Name={1}", e.Message, reader.Name);
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      private bool CreatePoint(IMapPoint mp)
      {
         double minDistance = 10;
         IMapPoint selectedMp = mp;
         foreach (Territory t in Territory.theTerritories)
         {
            foreach (IMapPoint mp1 in t.Points)
            {
               double d1 = Math.Abs(mp.X - mp1.X);
               double d2 = Math.Abs(mp.Y - mp1.Y);
               double distance = Math.Sqrt(Math.Pow(d1, 2.0) + Math.Pow(d2, 2.0));
               if (distance < minDistance)
               {
                  minDistance = distance;
                  selectedMp.X = mp1.X;
                  selectedMp.Y = mp1.Y;
                  Console.WriteLine("\t\t==> {0} from {1} with d={2}", selectedMp.ToString(), t.Name, distance);
                  myPoints.Add(selectedMp);
                  return true;
               }
            }
         }  // end foreach()
         Console.WriteLine("\t\t++>{0}", selectedMp.ToString()); // An territory point was not found.  Add the mouse click point as a new point.
         myPoints.Add(selectedMp);
         return true;
      }
      private void CreateTriangles()
      {
         const double SIZE = 6.0;
         foreach (KeyValuePair<string, Polyline> kvp in myRivers)
         {
            int i = 0;
            double X1 = 0.0;
            double Y1 = 0.0;
            foreach (System.Windows.Point p in kvp.Value.Points)
            {
               if (0 == i)
               {
                  X1 = p.X;
                  Y1 = p.Y;
               }
               else
               {
                  double Xcenter = X1 + (p.X - X1) / 2.0;
                  double Ycenter = Y1 + (p.Y - Y1) / 2.0;
                  PointCollection points = new PointCollection();
                  System.Windows.Point one = new System.Windows.Point(Xcenter - SIZE, Ycenter - SIZE);
                  System.Windows.Point two = new System.Windows.Point(Xcenter + SIZE, Ycenter);
                  System.Windows.Point three = new System.Windows.Point(Xcenter - SIZE, Ycenter + SIZE);
                  points.Add(one);
                  points.Add(two);
                  points.Add(three);
                  Polygon triangle = new Polygon() { Name = "River", Points = points, Stroke = mySolidColorBrush3, Fill = mySolidColorBrush3, Visibility = Visibility.Visible };
                  double rotateAngle = GetRotateAngle(X1, Y1, p.X, p.Y);
                  triangle.RenderTransform = new RotateTransform(rotateAngle, Xcenter, Ycenter);
                  myCanvas.Children.Add(triangle);
                  X1 = p.X;
                  Y1 = p.Y;
               }
               ++i;
            }
         }
      }
      private double GetRotateAngle(double X1, double Y1, double X2, double Y2)
      {
         double rotateAngle = 0.0;
         if (Math.Abs(X2 - X1) < 10.0)
            X2 = X1;
         if (Math.Abs(Y2 - Y1) < 10.0)
            Y2 = Y1;
         if (Math.Abs(Y2 - Y1) < 10.0)
         {
            if (X2 < X1)
               rotateAngle = 180.0;
         }
         else if( X1 < X2 )
         {
            if (Y1 < Y2)
               rotateAngle = 60.0;
            else
               rotateAngle = -60.0;
         }
         else 
         {
            if (Y1 < Y2)
               rotateAngle = 120;
            else
               rotateAngle = -120.0;
         }
         return rotateAngle;
      }
      private void CreateEllipses()
      {
         Ellipse aEllipseStart = new Ellipse();
         aEllipseStart.Name = "Start";
         aEllipseStart.Fill = Brushes.AliceBlue;
         aEllipseStart.StrokeThickness = 1;
         aEllipseStart.Stroke = Brushes.Red;
         aEllipseStart.Width = 50;
         aEllipseStart.Height = 50;
         System.Windows.Point pStart = new System.Windows.Point(50, 50);
         pStart.X -= theEllipseOffset;
         pStart.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipseStart, pStart.X);
         Canvas.SetTop(aEllipseStart, pStart.Y);
         myCanvas.Children.Add(aEllipseStart);
         myEllipses.Add(aEllipseStart);
         aEllipseStart.MouseDown += this.MouseDownEllipse;
         //-----------------------------------------------------------------------
         Ellipse aEllipseEnd = new Ellipse ();
         aEllipseEnd.Name = "End";
         aEllipseEnd.Fill = Brushes.Orchid;
         aEllipseEnd.StrokeThickness = 1;
         aEllipseEnd.Stroke = Brushes.Red;
         aEllipseEnd.Width = 50;
         aEllipseEnd.Height = 50;
         System.Windows.Point pEnd = new System.Windows.Point(50, 150);
         pEnd.X -= theEllipseOffset;
         pEnd.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipseEnd, pEnd.X);
         Canvas.SetTop(aEllipseEnd, pEnd.Y);
         myCanvas.Children.Add(aEllipseEnd);
         myEllipses.Add(aEllipseEnd);
         aEllipseEnd.MouseDown += this.MouseDownEllipse;
      }
      private XmlDocument CreateXml()
      {
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<Rivers></Rivers>");
         foreach (KeyValuePair<string, Polyline> kvp in myRivers)
         {
            XmlElement nameElem = aXmlDocument.CreateElement("River");  // name of river
            nameElem.SetAttribute("value", kvp.Key);
            aXmlDocument.DocumentElement.AppendChild(nameElem);
            foreach(System.Windows.Point p in kvp.Value.Points)
            {
               XmlElement pointElem = aXmlDocument.CreateElement("point");
               pointElem.SetAttribute("X", p.X.ToString());
               pointElem.SetAttribute("Y", p.Y.ToString());
               aXmlDocument.DocumentElement.LastChild.AppendChild(pointElem);
            }
         }
         return aXmlDocument;
      }
      //----------------------------------------------------------
      void MouseDownEllipse(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
         Ellipse mousedEllipse = (Ellipse)sender;
         //----------------------------------------
         string name = null;
         switch (myIndexName-1)
         {
            case 0: name = "Dienstal Branch"; break;
            case 1: name = "Largos River"; break;
            case 2: name = "Nesser River"; break;
            case 3: name = "Greater Nesser River"; break;
            case 4: name = "Lesser Nesser River"; break;
            case 5: name = "Trogoth River"; break;
            default: Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): reached default with invalid ellipse index=" + myIndexName.ToString()); return;
         }
         //----------------------------------------
         if ("Start" == mousedEllipse.Name)
         {
            if( true == myRivers.ContainsKey(name))
            {
               Polyline polyline = myRivers[name];
               myCanvas.Children.Remove(polyline);
               myRivers.Remove(name);
            }
            myPoints.Clear();
         }
         else if ("End" == mousedEllipse.Name)
         {
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in myPoints)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polyline polyline = new Polyline { Points = points, Stroke = mySolidColorBrush3, StrokeThickness = 4, Visibility = Visibility.Visible };
            myRivers[name] = polyline;
            Canvas.SetZIndex(polyline, 0);
            myCanvas.Children.Add(polyline);
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): reached default with invalid ellipse name=" + mousedEllipse.Name);
            return;
         }
         e.Handled = true;
      }
      void MouseDownCanvas(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
         if (false == CreatePoint(mp))
            Logger.Log(LogEnum.LE_ERROR, "MouseDownCanvas->CreatePoint()");
         e.Handled = true;
      }
   }
}