using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public class TerritoryRegionUnitTest : IUnitTest
   {
      public static Double theEllipseOffset = 8;
      //--------------------------------------------------------
      private DockPanel myDockPanel = null;
      private Canvas myCanvas = null;
      private IGameInstance myGameInstance = null;

      ITerritory myAnchorTerritory = null;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      private List<IMapPoint> myPoints = new List<IMapPoint>();
      //--------------------------------------------------------
      SolidColorBrush mySolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // nearly transparent but slightly colored
      SolidColorBrush mySolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
      SolidColorBrush mySolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black fill
      SolidColorBrush mySolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };                       // red fill
      //--------------------------------------------------------
      public bool CtorError { get; } = false;
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //--------------------------------------------------------
      public TerritoryRegionUnitTest(DockPanel dp, IGameInstance gi)
      {
         myIndexName = 0;
         myHeaderNames.Add("03-Delete Regions");
         myHeaderNames.Add("03-Add Regions");
         myHeaderNames.Add("03-Finish");
         //------------------------------------
         myCommandNames.Add("01-Regions");
         myCommandNames.Add("02-Regions");
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
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (CommandName == myCommandNames[0])
         {
            //--------------------------------------------
            // Remove all Ellipse and Polygons
            List<UIElement> results = new List<UIElement>();
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
                  results.Add(ui);
               if (ui is Polygon)
                  results.Add(ui);
            }
            foreach (UIElement ui1 in results)
               myCanvas.Children.Remove(ui1);
            foreach (ITerritory t in gi.Territories)
               t.Points.Clear();
         }
         else if (CommandName == myCommandNames[1])
         {
            //--------------------------------------------
            // Remove all Ellipse and Polygons
            List<UIElement> results = new List<UIElement>();
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
                  results.Add(ui);
               if (ui is Polygon)
                  results.Add(ui);
            }
            foreach (UIElement ui1 in results)
               myCanvas.Children.Remove(ui1);
            CreateEllipses(gi.Territories);
            CreatePolygons(gi.Territories);
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
      public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
      {
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
            if (ui is Polygon p)
               p.Fill = Utilities.theBrushRegionClear;
         }
         foreach (UIElement ui1 in results)
            myCanvas.Children.Remove(ui1);
         // Delete Existing Territories.xml file and create a new one based on myGameEngine.Territories container
         try
         {
            System.IO.File.Delete("../Config/Territories.xml");  // delete old file
            XmlDocument aXmlDocument = CreateXml(myGameInstance.Territories); // create a new XML document based on Territories
            using (FileStream writer = new FileStream("../Config/Territories.xml", FileMode.OpenOrCreate, FileAccess.Write))
            {
               XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
               using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
               {
                  aXmlDocument.Save(xmlWriter);
               }
            }
            using (FileStream writer = new FileStream("../Config/TerritoriesBackup.xml", FileMode.OpenOrCreate, FileAccess.Write))
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
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------
      public bool CreatePoint(IMapPoint mp)
      {
         if (null != myAnchorTerritory) // Add points to the anchor territy that define the region
         {
            // Do an intersection with any other points that
            // are part of any other region.  If a point is found
            // that is very close, assume that is the correct
            // point to add instead of the mouse click.
            double minDistance = 10;
            IMapPoint selectedMp = mp;
            foreach (String s in myAnchorTerritory.Adjacents)
            {
               Territory adjacentTerritory = null;
               foreach (Territory t in myGameInstance.Territories)
               {
                  if (s == Utilities.RemoveSpaces(t.ToString()))
                  {
                     adjacentTerritory = t;
                     break;
                  }
               }
               if (null == adjacentTerritory) // Check for error
               {
                  MessageBox.Show("Unable to find " + s);
                  return false;
               }
               foreach (IMapPoint mp1 in adjacentTerritory.Points)
               {
                  double distance = getRange(mp, mp1);
                  // Find the minimum distance between this point and any adjacent territory point.
                  // Use that point if it is below a set amount.
                  if (distance < minDistance)
                  {
                     minDistance = distance;
                     selectedMp.X = mp1.X;
                     selectedMp.Y = mp1.Y;
                     Console.WriteLine("\t\t==> {0} from {1} with d={2}", selectedMp.ToString(), adjacentTerritory.Name, distance);
                  }
               }
            }  // end foreach()
            Console.WriteLine("\t\t++>{0} to {1}", selectedMp.ToString(), myAnchorTerritory.Name); // An adjacent point was not found.  Add the mouse click point as a new point.
            myPoints.Add(selectedMp);
         }
         return true;
      }
      public void CreateEllipses(List<ITerritory> territories)
      {
         foreach (Territory t in territories)
         {
            Ellipse aEllipse = new Ellipse { Tag = Utilities.RemoveSpaces(t.ToString()) };
            aEllipse.Fill = Brushes.AliceBlue;
            aEllipse.StrokeThickness = 1;
            aEllipse.Stroke = Brushes.Red;
            aEllipse.Width = 15;
            aEllipse.Height = 15;
            System.Windows.Point p = new System.Windows.Point(t.CenterPoint.X, t.CenterPoint.Y);
            p.X -= theEllipseOffset;
            p.Y -= theEllipseOffset;
            Canvas.SetLeft(aEllipse, p.X);
            Canvas.SetTop(aEllipse, p.Y);
            myCanvas.Children.Add(aEllipse);
            myEllipses.Add(aEllipse);
            aEllipse.MouseDown += this.MouseDownEllipse;
         }
      }
      public void CreatePolygons(List<ITerritory> territories)
      {
         foreach (Territory t in territories)
         {
            if (1 < t.Points.Count)
            {
               PointCollection points = new PointCollection();
               foreach (IMapPoint mp1 in t.Points)
                  points.Add(new System.Windows.Point(mp1.X, mp1.Y));
               Polygon aPolygon = new Polygon { Fill= Utilities.theBrushRegion, Points = points, Tag = t.ToString(), Visibility= Visibility.Visible };
               aPolygon.MouseDown += this.MouseDownPolygon;
               Canvas.SetZIndex(aPolygon, 0);
               myCanvas.Children.Add(aPolygon);
            }
         }
      }
      public XmlDocument CreateXml(List<ITerritory> territories)
      {
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<Territories></Territories>");
         foreach (Territory t in territories)
         {
            XmlElement territoryElem = aXmlDocument.CreateElement("Territory");  // name of territory
            territoryElem.SetAttribute("value", t.Name);
            aXmlDocument.DocumentElement.AppendChild(territoryElem);
            //----------------------------------------------------
            XmlElement typeElem = aXmlDocument.CreateElement("type"); // type of territory, open, farmland, forest, hills, mountains, desert, swamp
            typeElem.SetAttribute("value", t.Type.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(typeElem);
            //----------------------------------------------------
            XmlElement coinElem = aXmlDocument.CreateElement("sector"); // sector of territory
            coinElem.SetAttribute("value", t.Coin.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(coinElem);
            //----------------------------------------------------
            XmlElement pointElem = aXmlDocument.CreateElement("point"); // center point for this territory
            pointElem.SetAttribute("X", t.CenterPoint.X.ToString());
            pointElem.SetAttribute("Y", t.CenterPoint.Y.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(pointElem);
            //----------------------------------------------------
            XmlElement townElem = aXmlDocument.CreateElement("isTown");
            townElem.SetAttribute("value", t.IsTown.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(townElem);
            XmlElement castleElem = aXmlDocument.CreateElement("isCastle");
            castleElem.SetAttribute("value", t.IsCastle.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(castleElem);
            XmlElement ruinsElem = aXmlDocument.CreateElement("isRuin");
            ruinsElem.SetAttribute("value", t.IsRuin.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(ruinsElem);
            XmlElement templeElem = aXmlDocument.CreateElement("isTemple");
            templeElem.SetAttribute("value", t.IsTemple.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(templeElem);
            XmlElement oasisElem = aXmlDocument.CreateElement("isOasis");
            oasisElem.SetAttribute("value", t.IsOasis.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(oasisElem);
            foreach (string s in t.Roads) // List of adjacent territories
            {
               XmlElement roadElem = aXmlDocument.CreateElement("road");
               roadElem.SetAttribute("value", s);
               aXmlDocument.DocumentElement.LastChild.AppendChild(roadElem);
            }
            foreach (string s in t.Rivers) // List of adjacent territories
            {
               XmlElement riverElem = aXmlDocument.CreateElement("river");
               riverElem.SetAttribute("value", s);
               aXmlDocument.DocumentElement.LastChild.AppendChild(riverElem);
            }
            foreach (string s in t.Adjacents) // List of adjacent territories
            {
               XmlElement adjacentElem = aXmlDocument.CreateElement("adjacent");
               adjacentElem.SetAttribute("value", s);
               aXmlDocument.DocumentElement.LastChild.AppendChild(adjacentElem);
            }
            foreach (string s in t.Rafts) // List of territories that can be seen from this territory
            {
               XmlElement adjacentElem = aXmlDocument.CreateElement("raft");
               adjacentElem.SetAttribute("value", s);
               aXmlDocument.DocumentElement.LastChild.AppendChild(adjacentElem);
            }
            foreach (IMapPoint p in t.Points) // Points that make up the polygon of this territory
            {
               System.Windows.Point point = new System.Windows.Point(p.X, p.Y);
               XmlElement regionPointElem = aXmlDocument.CreateElement("regionPoint");
               regionPointElem.SetAttribute("X", p.X.ToString());
               regionPointElem.SetAttribute("Y", p.Y.ToString());
               aXmlDocument.DocumentElement.LastChild.AppendChild(regionPointElem);
            }
         }
         return aXmlDocument;
      }
      //--------------------------------------------------------
      void MouseDownEllipse(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
         Console.WriteLine("MouseDownEllipse.MouseDown(): {0}", mp.ToString());
         ITerritory matchingTerritory = null; // Find the corresponding Territory
         Ellipse mousedEllipse = (Ellipse)sender;
         foreach (ITerritory t in myGameInstance.Territories)
         {
            if (mousedEllipse.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
            {
               matchingTerritory = t;
               break;
            }
         }
         if (null == matchingTerritory) // Check for error
         {
            MessageBox.Show("Unable to find " + mousedEllipse.Tag.ToString());
            return;
         }
         if (null == myAnchorTerritory)
         {
            MessageBox.Show("Anchoring " + mousedEllipse.Tag.ToString());
            myAnchorTerritory = matchingTerritory; // If there is no anchor territory. Set it.
            mousedEllipse.Fill = mySolidColorBrush3;
            myCanvas.MouseDown += MouseDownCanvas;
            return;
         }
         if (matchingTerritory.ToString() == myAnchorTerritory.ToString())
         {
            // If this is the matching territory is the anchor territory, the user
            // is requesting that they are done adding points for
            // defining the Region.  The Region is used set as part of the Territory. 
            MessageBox.Show("Saving " + mousedEllipse.Tag.ToString());
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in myPoints)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = mySolidColorBrush3, Points = points, Tag = matchingTerritory.ToString() };
            aPolygon.MouseDown += this.MouseDownPolygon;
            aPolygon.Fill = mySolidColorBrush2;
            myCanvas.Children.Add(aPolygon);
            mousedEllipse.Fill = mySolidColorBrush2;
            myAnchorTerritory.Points = new List<IMapPoint>(myPoints);
            myPoints.Clear();
            myAnchorTerritory = null;
            myCanvas.MouseDown -= MouseDownCanvas;
         }
         e.Handled = true;
      }
      void MouseDownCanvas(object sender, MouseButtonEventArgs e)
      {
         // This function adds points to the myPoints collection when an anchor territory is active.
         // The points to add are either new ones or ones that exist from adjacent territories.
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
         if (false == CreatePoint(mp))
            Logger.Log(LogEnum.LE_ERROR, "MouseDownCanvas->CreatePoint()");
         e.Handled = true;
      }
      void MouseDownPolygon(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
         Console.WriteLine("TerritoryRegionUnitTest.MouseDownPolygon(): {0}", mp.ToString());
         if (null == myAnchorTerritory)
         {
            // This function removes an existing polygon when it is clicked if no achor territory exists
            Polygon aPolygon = (Polygon)sender;
            ITerritory matchingTerritory = null;
            foreach (ITerritory t in myGameInstance.Territories)
            {
               if (aPolygon.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
               {
                  matchingTerritory = t;
                  break;
               }
            }
            if (null == matchingTerritory) // Check for error
            {
               MessageBox.Show("Unable to find " + aPolygon.Tag.ToString());
            }
            else if ((null == myAnchorTerritory) || matchingTerritory.ToString() == myAnchorTerritory.ToString())
            {
               matchingTerritory.Points.Clear();
               myCanvas.Children.Remove(aPolygon);
            }
         }
         else
         {
            if (false == CreatePoint(mp))
               Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygon->CreatePoint()");
         }
         e.Handled = true;
      }
      double getRange(IMapPoint p1, IMapPoint p2)
      {
         double d1 = Math.Abs(p1.X - p2.X);
         double d2 = Math.Abs(p1.Y - p2.Y);
         double distance = Math.Sqrt(Math.Pow(d1, 2.0) + Math.Pow(d2, 2.0));
         return distance;
      }
   }
}


