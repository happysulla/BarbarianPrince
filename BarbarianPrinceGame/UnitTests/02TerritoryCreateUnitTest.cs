using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public class TerritoryCreateUnitTest : IUnitTest
   {
      private DockPanel myDockPanel = null;
      private IGameInstance myGameInstance = null;
      Canvas myCanvas = null;
      public bool isDragging = false;
      UIElement myItem = null;
      private System.Windows.Point myPreviousLocation;
      Territory myAnchorTerritory = null;
      private double myXColumn = 0;
      private Dictionary<string, Polyline> myRivers = new Dictionary<string, Polyline>();
      private List<Ellipse> myEllipses = new List<Ellipse>();
      public static Double theEllipseDiameter = 30;
      public static Double theEllipseOffset = theEllipseDiameter / 2.0;
      private int myIndexRaft = 0;
      private int myIndexDownRiver = 0;
      private readonly SolidColorBrush mySolidColorBrushWaterBlue = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      //-----------------------------------------
      private int myIndexName = 0;
      public bool CtorError { get; } = false;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public TerritoryCreateUnitTest(DockPanel dp, IGameInstance gi)
      {
         myIndexName = 0;
         myHeaderNames.Add("02-Delete Territories");
         myHeaderNames.Add("02-New Territories");
         myHeaderNames.Add("02-Set CenterPoints");
         myHeaderNames.Add("02-Verify Territories");
         myHeaderNames.Add("02-Set Roads");
         myHeaderNames.Add("02-Set Rivers");
         myHeaderNames.Add("02-Set Adjacents");
         myHeaderNames.Add("02-Set Rafts");
         myHeaderNames.Add("02-Set DownRiver");
         myHeaderNames.Add("02-Final");
         //------------------------------------
         myCommandNames.Add("00-Delete File");
         myCommandNames.Add("01-Click Center of Hex");
         myCommandNames.Add("02-Click Elispse to Move");
         myCommandNames.Add("03-Click Ellispe to Verify");
         myCommandNames.Add("04-Verify Roads");
         myCommandNames.Add("05-Verify Rivers");
         myCommandNames.Add("06-Verify Adjacents");
         myCommandNames.Add("07-Verify RaftHexes");
         myCommandNames.Add("08-Verify DownRiverHex");
         myCommandNames.Add("09-Cleanup");
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
         myGameInstance = gi;
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (CommandName == myCommandNames[0]) // Delete
         {
            string filename = ConfigFileReader.theConfigDirectory + "Territories.xml";
            System.IO.File.Delete(filename);  // delete old file
            if (false == NextTest(ref gi)) // automatically move next test
            {
               System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.Command(): NextTest() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[1]) //  Create territories
         {

         }
         else if (CommandName == myCommandNames[2]) // set centerpoints
         {

         }
         else if (CommandName == myCommandNames[3])
         {
            myXColumn = 0.0; // When set to zero, it indicates that use existing value instead of value from previous entry 
            // Want the same X value as specified in the last dialog. This lines up dots.
         }
         else if (CommandName == myCommandNames[4]) // Show Roads
         {
            if (false == ShowRoads(Territory.theTerritories))
            {
               System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.Command(): ShowRoads() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[5]) // Show Rivers
         {
            if (false == ShowRivers(Territory.theTerritories))
            {
               System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.Command(): ShowRivers() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[6])  // Show Adjacents
         {
            if (false == ShowAdjacents(Territory.theTerritories))
            {
               System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.Command(): ShowAdjacents() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[7]) // Show Raft Territories
         {
            if (false == ShowRaftTerritories(Territory.theTerritories))
            {
               System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.Command(): ShowRaftTerritories() returned false");
               return false;
            }
            myIndexRaft++;
            if (Territory.theTerritories.Count <= myIndexRaft)
               myIndexRaft = 0;
         }
         else if (CommandName == myCommandNames[8]) // SHow downriver territory
         {
            if (false == ShowDownRiverTerritory(Territory.theTerritories))
            {
               System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.Command(): ShowDownRiverTerritory() returned false");
               return false;
            }
            myIndexDownRiver++;
            if (Territory.theTerritories.Count <= myIndexDownRiver)
               myIndexDownRiver = 0;
         }
         else if (CommandName == myCommandNames[9]) // Finish - delete existing file and add new one
         {
            if (false == Cleanup(ref gi))
            {
               System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.Command(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
      {
         if (HeaderName == myHeaderNames[0])
         {
            CreateEllipses(Territory.theTerritories);
            myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDownCreateTerritory;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[1])
         {
            CreateEllipses(Territory.theTerritories);
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
            myCanvas.MouseLeftButtonDown += this.MouseDownSetCenterPoint;
            myCanvas.MouseMove += MouseMove;
            myCanvas.MouseUp += MouseUp;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            CreateEllipses(Territory.theTerritories);
            myCanvas.MouseMove -= MouseMove;
            myCanvas.MouseUp -= MouseUp;
            myCanvas.MouseLeftButtonDown -= this.MouseDownSetCenterPoint;
            myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDownVerifyTerritory;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            CreateEllipses(Territory.theTerritories);
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownVerifyTerritory;
            myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDownSetRoads;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[4])
         {
            CreateEllipses(Territory.theTerritories);
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownSetRoads;
            myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDownSetRivers;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[5])
         {
            CreateEllipses(Territory.theTerritories);
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownSetRivers;
            myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDownSetAdjacents;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[6])
         {
            CreateEllipses(Territory.theTerritories);
            if (0 == myIndexRaft)
            {
               myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
               myCanvas.MouseLeftButtonDown += this.MouseLeftDownSetRaftTerritories;
            }
            myAnchorTerritory = null;
            ++myIndexName;
            SolidColorBrush aSolidColorBrushClear = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
            SolidColorBrush aSolidColorBrushBlue = new SolidColorBrush { Color = Colors.Blue };
            foreach (Territory t in Territory.theTerritories) // Clear out all previous results
            {
               string tName = Utilities.RemoveSpaces(t.ToString());
               foreach (UIElement ui in myCanvas.Children)
               {
                  if (ui is Ellipse)
                  {
                     Ellipse ellipse = (Ellipse)ui;
                     if (tName == ellipse.Tag.ToString())
                     {
                        if (0 < t.Rafts.Count)
                           ellipse.Fill = aSolidColorBrushBlue;
                        else
                           ellipse.Fill = aSolidColorBrushClear;
                        break;
                     }
                  }
               }
            }
         }
         else if (HeaderName == myHeaderNames[7])
         {
            CreateEllipses(Territory.theTerritories);
            SolidColorBrush aSolidColorBrushClear = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
            SolidColorBrush aSolidColorBrushBlue = new SolidColorBrush { Color = Colors.Blue };
            foreach (Territory t in Territory.theTerritories) // Clear out all previous results
            {
               string tName = Utilities.RemoveSpaces(t.ToString());
               foreach (UIElement ui in myCanvas.Children)
               {
                  if (ui is Ellipse)
                  {
                     Ellipse ellipse = (Ellipse)ui;
                     if (tName == ellipse.Tag.ToString())
                     {
                        if ("" != t.DownRiver)
                           ellipse.Fill = aSolidColorBrushBlue;
                        else
                           ellipse.Fill = aSolidColorBrushClear;
                        break;
                     }
                  }
               }
            }
            if ( 0 == myIndexDownRiver)
            {
               myCanvas.MouseLeftButtonDown -= this.MouseLeftDownSetRaftTerritories;
               myCanvas.MouseLeftButtonDown += this.MouseLeftDownSetDownRiverTerritory;
               CreateRiversFromXml();
               UpdateCanvasRiver("Dienstal Branch");  // Show all the rivers
               UpdateCanvasRiver("Largos River");
               UpdateCanvasRiver("Nesser River");
               UpdateCanvasRiver("Greater Nesser River");
               UpdateCanvasRiver("Lesser Nesser River");
               UpdateCanvasRiver("Trogoth River");
            }
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[8])
         {
            CreateEllipses(Territory.theTerritories);
            myCanvas.MouseLeftButtonDown -= this.MouseLeftDownSetDownRiverTerritory;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else
         {
            if (false == Cleanup(ref gi))
            {
               System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.Command(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
      {
         //--------------------------------------------------
         if (HeaderName == myHeaderNames[1])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseDownSetCenterPoint;
            myCanvas.MouseMove -= MouseMove;
            myCanvas.MouseUp -= MouseUp;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownVerifyTerritory;
         }
         else if (HeaderName == myHeaderNames[4])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownSetRoads;
         }
         else if (HeaderName == myHeaderNames[5])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownSetRivers;
         }
         else if (HeaderName == myHeaderNames[6])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
         }
         else if (HeaderName == myHeaderNames[7])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftDownSetRaftTerritories;
         }
         else if (HeaderName == myHeaderNames[7])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftDownSetDownRiverTerritory;
         }
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvas.Children.Remove(ui1);
         //--------------------------------------------------
         // Delete Existing Territories.xml file and create a new one based on myGameEngine.Territories container
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + "Territories.xml";
            System.IO.File.Delete(filename);  // delete old file
            XmlDocument aXmlDocument = CreateXml(Territory.theTerritories); // create a new XML document based on Territories
            using (FileStream writer = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
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
            System.Diagnostics.Debug.WriteLine("Cleanup(): exeption={0}", e.Message);
            return false;
         }
         //--------------------------------------------------
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------------------
      public void CreateEllipse(ITerritory territory)
      {
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush{ Color = Color.FromArgb(100, 255, 255, 0) };
         Ellipse aEllipse = new Ellipse
         {
            Tag = Utilities.RemoveSpaces(territory.ToString()),
            Fill = aSolidColorBrush1,
            StrokeThickness = 1,
            Stroke = Brushes.Red,
            Width = theEllipseDiameter,
            Height = theEllipseDiameter
         };
         System.Windows.Point p = new System.Windows.Point(territory.CenterPoint.X, territory.CenterPoint.Y);
         p.X -= theEllipseOffset;
         p.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipse, p.X);
         Canvas.SetTop(aEllipse, p.Y);
         myCanvas.Children.Add(aEllipse);
         myEllipses.Add(aEllipse);
      }
      public void CreateEllipses(ITerritories territories)
      {
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvas.Children.Remove(ui1);
         //--------------------------------------------------
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // nearly transparent but slightly colored
         foreach (Territory t in territories)
         {
            Ellipse aEllipse = new Ellipse { Tag = Utilities.RemoveSpaces(t.ToString()), Stroke = Brushes.Red, Width = theEllipseDiameter, Height = theEllipseDiameter, StrokeThickness = 1 };
            aEllipse.Fill = aSolidColorBrush0;
            aEllipse.Visibility = Visibility.Visible;
            System.Windows.Point p = new System.Windows.Point(t.CenterPoint.X, t.CenterPoint.Y);
            p.X -= theEllipseOffset;
            p.Y -= theEllipseOffset;
            Canvas.SetLeft(aEllipse, p.X);
            Canvas.SetTop(aEllipse, p.Y);
            Canvas.SetZIndex(aEllipse, 99999);
            myCanvas.Children.Add(aEllipse);
            myEllipses.Add(aEllipse);
         }
      }
      public XmlDocument CreateXml(ITerritories territories)
      {
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<Territories></Territories>");
         foreach (Territory t in territories)
         {
            XmlElement terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
            terrElem.SetAttribute("value", t.Name);
            aXmlDocument.DocumentElement.AppendChild(terrElem);
            XmlElement typeElem = aXmlDocument.CreateElement("type"); // type of territory, open, farmland, forest, hills, mountains, desert, swamp
            typeElem.SetAttribute("value", t.Type.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(typeElem);
            XmlElement pointElem = aXmlDocument.CreateElement("point"); // center point for this territory
            pointElem.SetAttribute("X", t.CenterPoint.X.ToString());
            pointElem.SetAttribute("Y", t.CenterPoint.Y.ToString());
            aXmlDocument.DocumentElement.LastChild.AppendChild(pointElem);
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
            XmlElement downRiverElem = aXmlDocument.CreateElement("DownRiver");
            downRiverElem.SetAttribute("value", t.DownRiver);
            aXmlDocument.DocumentElement.LastChild.AppendChild(downRiverElem);
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
            foreach (string s in t.Rafts) // List of territories that can be raft to from this territory
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
      public bool ShowRoads(ITerritories territories)
      {
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // completely clear
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         SolidColorBrush aSolidColorBrush4 = new SolidColorBrush { Color = Colors.Yellow };
         foreach (Territory anchorTerritory in Territory.theTerritories)
         {
            string anchorName = Utilities.RemoveSpaces(anchorTerritory.ToString());
            Ellipse anchorEllipse = null; // Find the corresponding ellipse for this anchor territory
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (anchorName == ellipse.Tag.ToString())
                  {
                     anchorEllipse = ellipse;
                     break;
                  }
               }
            }
            if (null == anchorEllipse)
            {
               Logger.Log(LogEnum.LE_ERROR, anchorTerritory.ToString());
               return false;
            }
            if (0 < anchorTerritory.Roads.Count)
               anchorEllipse.Fill = aSolidColorBrush4;
            // At this point, the anchorEllipse and the anchorTerritory are found.
            foreach (string s in anchorTerritory.Roads)
            {
               ITerritory roadTerritory = null;
               foreach (ITerritory t in territories) // Find the Road Territory corresponding to this name
               {
                  if (t.ToString() == s)
                  {
                     roadTerritory = t;
                     break;
                  }
               }
               if (null == roadTerritory)
               {
                  MessageBox.Show("Not Found s=" + s);
                  return false;
               }
               string roadName = Utilities.RemoveSpaces(roadTerritory.ToString());
               Ellipse roadEllipse = null; // Find the corresponding ellipse for this road territory
               foreach (UIElement ui in myCanvas.Children)
               {
                  if (ui is Ellipse)
                  {
                     Ellipse ellipse = (Ellipse)ui;
                     if (roadName == ellipse.Tag.ToString())
                     {
                        roadEllipse = ellipse;
                        break;
                     }
                  }
               }
               if (null == roadEllipse)
               {
                  Logger.Log(LogEnum.LE_ERROR, roadName);
                  MessageBox.Show(anchorTerritory.ToString());
                  return false;
               }
               // Search the Adajance Road Territory Roads List to make sure the 
               // anchor territory is in that list. It shoudl be bi directional.
               bool isReturnFound = false;
               foreach (String s1 in roadTerritory.Roads)
               {
                  string returnName = Utilities.RemoveSpaces(s1);
                  if (returnName == anchorName)
                  {
                     isReturnFound = true; // Yes the adjacent road has a entry to return the road back to the anchor territory
                     break;
                  }
               }
               // Anchor Property not found in the adjacent road property territory.  This is an error condition.
               if (false == isReturnFound)
               {
                  anchorEllipse.Fill = aSolidColorBrush0; // change color of two ellipses to signify error
                  roadEllipse.Fill = aSolidColorBrush2;
                  StringBuilder sb = new StringBuilder("anchor="); sb.Append(anchorName); sb.Append(" NOT in list for road="); sb.Append(roadName);
                  MessageBox.Show(sb.ToString());
                  return false;
               }
            }
         }
         return true;
      }
      public bool ShowRivers(ITerritories territories)
      {
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // completely clear
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         SolidColorBrush aSolidColorBrush4 = new SolidColorBrush { Color = Colors.Yellow };
         foreach (Territory anchorTerritory in Territory.theTerritories)
         {
            string anchorName = Utilities.RemoveSpaces(anchorTerritory.ToString());
            Ellipse anchorEllipse = null; // Find the corresponding ellipse for this anchor territory
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (anchorName == ellipse.Tag.ToString())
                  {
                     anchorEllipse = ellipse;
                     break;
                  }
               }
            }
            if (null == anchorEllipse)
            {
               Logger.Log(LogEnum.LE_ERROR, anchorTerritory.ToString());
               return false;
            }
            if (0 < anchorTerritory.Rivers.Count)
               anchorEllipse.Fill = aSolidColorBrush4;
            // At this point, the anchorEllipse and the anchorTerritory are found.
            foreach (string s in anchorTerritory.Rivers)
            {
               ITerritory riverTerritory = null;
               foreach (ITerritory t in territories) // Find the River Territory corresponding to this name
               {
                  if (t.ToString() == s)
                  {
                     riverTerritory = t;
                     break;
                  }
               }
               if (null == riverTerritory)
               {
                  MessageBox.Show("Not Found s=" + s);
                  return false;
               }
               string adjacentName = Utilities.RemoveSpaces(riverTerritory.ToString());
               Ellipse riverEllipse = null; // Find the corresponding ellipse for this River territory
               foreach (UIElement ui in myCanvas.Children)
               {
                  if (ui is Ellipse)
                  {
                     Ellipse ellipse = (Ellipse)ui;
                     if (adjacentName == ellipse.Tag.ToString())
                     {
                        riverEllipse = ellipse;
                        break;
                     }
                  }
               }
               if (null == riverEllipse)
               {
                  Logger.Log(LogEnum.LE_ERROR, adjacentName);
                  MessageBox.Show(anchorTerritory.ToString());
                  return false;
               }
               // Search the Adajance River Territory Rivers List to make sure the 
               // anchor territory is in that list. It should be bi-directional.
               bool isReturnFound = false;
               foreach (String s1 in riverTerritory.Rivers)
               {
                  string returnName = Utilities.RemoveSpaces(s1);
                  if (returnName == anchorName)
                  {
                     isReturnFound = true; // Yes the adjacent River has a entry to return the River back to the anchor territory
                     break;
                  }
               }
               // Anchor Property not found in the adjacent River property territory.  This is an error condition.
               if (false == isReturnFound)
               {
                  anchorEllipse.Fill = aSolidColorBrush3; // change color of two ellipses to signify error
                  riverEllipse.Fill = aSolidColorBrush2;
                  StringBuilder sb = new StringBuilder("This red territory ="); sb.Append(anchorName); sb.Append(" NOT in river list of adjacent black territory="); sb.Append(adjacentName);
                  MessageBox.Show(sb.ToString());
                  return false;
               }
            }
         }
         return true;
      }
      public bool ShowAdjacents(ITerritories territories)
      {
         myAnchorTerritory = null;
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // completely clear
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         SolidColorBrush aSolidColorBrush4 = new SolidColorBrush { Color = Colors.Yellow };
         foreach (Territory anchorTerritory in territories)
         {
            string anchorName = Utilities.RemoveSpaces(anchorTerritory.ToString());
            Ellipse anchorEllipse = null; // Find the corresponding ellipse for this anchor territory
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (anchorName == ellipse.Tag.ToString())
                  {
                     anchorEllipse = ellipse;
                     break;
                  }
               }
            }
            if (null == anchorEllipse)
            {
               Logger.Log(LogEnum.LE_ERROR, anchorTerritory.ToString());
               return false;
            }
            if (0 < anchorTerritory.Adjacents.Count)
               anchorEllipse.Fill = aSolidColorBrush4;
            // At this point, the anchorEllipse and the anchorTerritory are found.
            foreach (string s in anchorTerritory.Adjacents)
            {
               ITerritory adjacentTerritory = null;
               foreach (ITerritory t in territories) // Find the River Territory corresponding to this name
               {
                  if (t.ToString() == s)
                  {
                     adjacentTerritory = t;
                     break;
                  }
               }
               if (null == adjacentTerritory)
               {
                  MessageBox.Show("Not Found s=" + s);
                  return false;
               }
               string adjacentName = Utilities.RemoveSpaces(adjacentTerritory.ToString());
               Ellipse adjacentEllipse = null; // Find the corresponding ellipse for this territory
               foreach (UIElement ui in myCanvas.Children)
               {
                  if (ui is Ellipse)
                  {
                     Ellipse ellipse = (Ellipse)ui;
                     if (adjacentName == ellipse.Tag.ToString())
                     {
                        adjacentEllipse = ellipse;
                        break;
                     }
                  }
               }
               if (null == adjacentEllipse)
               {
                  Logger.Log(LogEnum.LE_ERROR, adjacentName);
                  MessageBox.Show(anchorTerritory.ToString());
                  return false;
               }
               // Search the Adjacent Territory  List to make sure the 
               // anchor territory is in that list. It should be bi directional.
               bool isReturnFound = false;
               foreach (String s1 in adjacentTerritory.Adjacents)
               {
                  string returnName = Utilities.RemoveSpaces(s1);
                  if (returnName == anchorName)
                  {
                     isReturnFound = true; // Yes the adjacent River has a entry to return the River back to the anchor territory
                     break;
                  }
               }
               // Anchor Property not found in the adjacent property territory.  This is an error condition.
               if (false == isReturnFound)
               {
                  anchorEllipse.Fill = aSolidColorBrush3; // change color of two ellipses to signify error
                  adjacentEllipse.Fill = aSolidColorBrush2;
                  StringBuilder sb = new StringBuilder("anchor="); sb.Append(anchorName); sb.Append(" NOT in list for adjacent="); sb.Append(adjacentName);
                  MessageBox.Show(sb.ToString());
                  return false;
               }
            }
         }
         return true;
      }
      public bool ShowRaftTerritories(ITerritories territories)
      {
         SolidColorBrush aSolidColorBrushClear = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
         SolidColorBrush aSolidColorBrushBlack = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black
         SolidColorBrush aSolidColorBrushRed = new SolidColorBrush { Color = Colors.Red };
         SolidColorBrush aSolidColorBrushYellow = new SolidColorBrush { Color = Colors.Yellow };
         SolidColorBrush aSolidColorBrushBlue = new SolidColorBrush { Color = Colors.Blue };
         SolidColorBrush aSolidColorBrushPurple = new SolidColorBrush { Color = Colors.Purple };
         //------------------------------------------------
         int k = 0;
         foreach (Territory t in territories) // Clear out all previous results
         {
            string tName = Utilities.RemoveSpaces(t.ToString());
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (tName == ellipse.Tag.ToString())
                  {
                     if (0 < t.Rafts.Count)
                     {
                        if( k < myIndexRaft)
                           ellipse.Fill = aSolidColorBrushPurple;
                        else
                           ellipse.Fill = aSolidColorBrushBlue;
                     }
                     break;
                  }
               }
            }
            ++k;
         }
         //------------------------------------------------
         if ( territories.Count <= myIndexRaft )
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRaftTerritories(): myIndexRaft=" + myIndexRaft.ToString() + " > Count=" + territories.Count.ToString());
            return false;
         }
         bool isFirstIndex = false; // need to check if no rafts exists in any
         if (0 == myIndexRaft)
            isFirstIndex = true;
         ITerritory anchorTerritory = null;
         for (int i = myIndexRaft; i<territories.Count; ++i ) // Find an anchor territory with raft territories
         {
            anchorTerritory = territories[myIndexRaft];
            if (0 < anchorTerritory.Rafts.Count)
               break;
            myIndexRaft++;
         }
         if (myIndexRaft == territories.Count) // reached end
         {
            if( true == isFirstIndex )
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowRaftTerritories(): no rafts found");
               return false;
            }
            return true;
         }
         if( null == anchorTerritory ) 
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRaftTerritories(): anchorTerritory=null");
            return false;
         }
         //------------------------------------------------
         string anchorName = Utilities.RemoveSpaces(anchorTerritory.ToString()); // Find the corresponding ellipse for this anchor territory
         Ellipse anchorEllipse = null; 
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (anchorName == ellipse.Tag.ToString())
               {
                  anchorEllipse = ellipse;
                  break;
               }
            }
         }
         if (null == anchorEllipse)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRaftTerritories(): Unable to find ellipse for anchorTerritory=" + anchorTerritory.ToString());
            return false;
         }
         //------------------------------------------------
         anchorEllipse.Fill = aSolidColorBrushBlack;
         foreach (string s in anchorTerritory.Rafts)  // For each Raft territory, highlight it special color
         {
            ITerritory raftTerritory = null;
            foreach (ITerritory t in territories) 
            {
               if (t.ToString() == s)
               {
                  raftTerritory = t;
                  break;
               }
            }
            if (null == raftTerritory)
            {
               MessageBox.Show("Not Found s=" + s);
               return false;
            }
            string raftName = Utilities.RemoveSpaces(raftTerritory.ToString());
            Ellipse raftEllipse = null; 
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (raftName == ellipse.Tag.ToString())
                  {
                     raftEllipse = ellipse;
                     break;
                  }
               }
            }
            if (null == raftEllipse)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowRaftTerritories(): Unable to find ellipse for anchorTerritory=" + anchorTerritory.ToString() + " raftT=" + raftName);
               MessageBox.Show(anchorTerritory.ToString());
               return false;
            }
            if(0 == myIndexRaft%2) // alternate colors
               raftEllipse.Fill = aSolidColorBrushRed; 
            else
               raftEllipse.Fill = aSolidColorBrushYellow; 
         }
         return true;
      }
      public bool ShowDownRiverTerritory(ITerritories territories)
      {
         SolidColorBrush aSolidColorBrushClear = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
         SolidColorBrush aSolidColorBrushBlack = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black
         SolidColorBrush aSolidColorBrushRed = new SolidColorBrush { Color = Colors.Red };
         SolidColorBrush aSolidColorBrushYellow = new SolidColorBrush { Color = Colors.Yellow };
         SolidColorBrush aSolidColorBrushBlue = new SolidColorBrush { Color = Colors.Blue };
         SolidColorBrush aSolidColorBrushPurple = new SolidColorBrush { Color = Colors.Purple };
         //------------------------------------------------
         int k = 0;
         foreach (Territory t in territories) // Clear out all previous results
         {
            string tName = Utilities.RemoveSpaces(t.ToString());
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (tName == ellipse.Tag.ToString())
                  {
                     if ("" != t.DownRiver)
                     {
                        if (k < myIndexDownRiver)
                           ellipse.Fill = aSolidColorBrushPurple;
                        else
                           ellipse.Fill = aSolidColorBrushBlue;
                     }
                     break;
                  }
               }
            }
            ++k;
         }
         //------------------------------------------------
         if (territories.Count <= myIndexDownRiver)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDownRiverTerritory(): myIndexDownRiver=" + myIndexDownRiver.ToString() + " > Count=" + territories.Count.ToString());
            return false;
         }
         bool isFirstIndex = false; // need to check if no rafts exists in any
         if (0 == myIndexDownRiver)
            isFirstIndex = true;
         ITerritory anchorTerritory = null;
         for (int i = myIndexDownRiver; i < territories.Count; ++i) // Find an anchor territory with DownRiver hex
         {
            anchorTerritory = territories[myIndexDownRiver];
            if ("" !=  anchorTerritory.DownRiver)
               break;
            myIndexDownRiver++;
         }
         if (myIndexDownRiver == territories.Count) // reached end
         {
            if (true == isFirstIndex)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDownRiverTerritory(): no downrivers found");
               return false;
            }
            return true;
         }
         if (null == anchorTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDownRiverTerritory(): anchorTerritory=null");
            return false;
         }
         //------------------------------------------------
         string anchorName = Utilities.RemoveSpaces(anchorTerritory.ToString()); // Find the corresponding ellipse for this anchor territory
         Ellipse anchorEllipse = null;
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (anchorName == ellipse.Tag.ToString())
               {
                  anchorEllipse = ellipse;
                  break;
               }
            }
         }
         if (null == anchorEllipse)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDownRiverTerritory(): Unable to find ellipse for anchorTerritory=" + anchorTerritory.ToString());
            return false;
         }
         //------------------------------------------------
         anchorEllipse.Fill = aSolidColorBrushBlack;
         string downRiverName = anchorTerritory.DownRiver;
         Ellipse downRiverEllipse = null;
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (downRiverName == ellipse.Tag.ToString())
               {
                  downRiverEllipse = ellipse;
                  break;
               }
            }
         }
         if (null == downRiverEllipse)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDownRiverTerritory(): Unable to find ellipse for anchorTerritory=" + anchorTerritory.ToString() + " downRiverName=" + downRiverName);
            MessageBox.Show(anchorTerritory.ToString());
            return false;
         }
         if (0 == myIndexDownRiver % 2) // alternate colors
            downRiverEllipse.Fill = aSolidColorBrushRed;
         else
            downRiverEllipse.Fill = aSolidColorBrushYellow;
         return true;
      }
      private void CreateRiversFromXml()
      {
         XmlTextReader reader = null;
         PointCollection points = null;
         string name = null;
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + "Rivers.xml";
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None }; // Load the reader with the data file and ignore all white space nodes.    
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
                  Polyline polyline = new Polyline { Points = points, Stroke = mySolidColorBrushWaterBlue, StrokeThickness = 2, Visibility = Visibility.Visible };
                  myRivers[name] = polyline;
               } // end if
            } // end while
         } // try
         catch (Exception e)
         {
            System.Diagnostics.Debug.WriteLine("ReadTerritoriesXml(): Exception:  e.Message={0} while reading reader.Name={1}", e.Message, reader.Name);
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      private void UpdateCanvasRiver(string river)
      {
         try
         {
            Polyline polyline = myRivers[river];
            Canvas.SetZIndex(polyline, 1000);
            myCanvas.Children.Add(polyline); // add polyline to canvas
            const double SIZE = 6.0;
            int i = 0;
            double X1 = 0.0;
            double Y1 = 0.0;
            foreach (System.Windows.Point p in polyline.Points)
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
                  //---------------------------------------
                  Polygon triangle = new Polygon() { Name = "River", Points = points, Stroke = mySolidColorBrushWaterBlue, Fill = mySolidColorBrushWaterBlue, Visibility = Visibility.Visible };
                  double rotateAngle = 0.0;
                  if (Math.Abs(p.Y - Y1) < 10.0)
                  {
                     if (p.X < X1)
                        rotateAngle = 180.0;
                  }
                  else if (X1 < p.X)
                  {
                     if (Y1 < p.Y)
                        rotateAngle = 60.0;
                     else
                        rotateAngle = -60.0;
                  }
                  else
                  {
                     if (Y1 < p.Y)
                        rotateAngle = 120;
                     else
                        rotateAngle = -120.0;
                  }
                  triangle.RenderTransform = new RotateTransform(rotateAngle, Xcenter, Ycenter);
                  //---------------------------------------
                  myCanvas.Children.Add(triangle);
                  X1 = p.X;
                  Y1 = p.Y;
               }
               ++i;
            }
         }
         catch (Exception e)
         {
            System.Diagnostics.Debug.WriteLine("UpdateCanvasRiver(): unknown river=" + river + " EXCEPTION THROWN e={0}", e.ToString());
         }
      }
      //--------------------------------------------------------------------
      // Add the callback for clicking on the Canvas
      void MouseLeftButtonDownCreateTerritory(object sender, MouseButtonEventArgs e)
      {
         SolidColorBrush aSolidColorBrush = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         System.Windows.Point p = e.GetPosition(myCanvas);
         TerritoryCreateDialog dialog = new TerritoryCreateDialog(); // Get the name from user
         dialog.myTextBoxName.Focus();
         if (true == dialog.ShowDialog())
         {
            Territory territory = new Territory(dialog.myTextBoxName.Text) { CenterPoint = new MapPoint(p.X, p.Y) };
            territory.Type = dialog.RadioOutputText;
            territory.IsTown = dialog.IsTown;
            territory.IsCastle = dialog.IsCastle;
            territory.IsRuin = dialog.IsRuin;
            territory.IsTemple = dialog.IsTemple;
            territory.IsOasis = dialog.IsOasis;
            CreateEllipse(territory);
            Territory.theTerritories.Add(territory);
         }
      }
      void MouseDownSetCenterPoint(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition(myCanvas);
         System.Diagnostics.Debug.WriteLine("TerritoryUnitTest.MouseDown(): {0}", p.ToString());
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  if (false == isDragging)
                  {
                     MessageBox.Show(ellipse.Tag.ToString());
                     myPreviousLocation = p;
                     this.isDragging = true;
                     this.myItem = ui;
                  }
               }
            }
         }
      }
      void MouseMove(object sender, MouseEventArgs e)
      {
         if (true == isDragging)
         {
            if (null != myItem)
            {
               System.Windows.Point newPoint = e.GetPosition(myCanvas);
               Canvas.SetTop(myItem, newPoint.Y - theEllipseOffset);
               Canvas.SetLeft(myItem, newPoint.X - theEllipseOffset);
            }
         }
      }
      void MouseUp(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point newPoint = e.GetPosition(myCanvas);
         this.isDragging = false;
         if (this.myItem != null)
         {
            if (this.myItem is Ellipse)
            {
               Ellipse ellipse = (Ellipse)myItem;
               string tag = ellipse.Tag.ToString();
               foreach (Territory t in Territory.theTerritories)
               {
                  string name = Utilities.RemoveSpaces(t.ToString());
                  if (tag == name)
                  {
                     t.CenterPoint.X = newPoint.X;
                     t.CenterPoint.Y = newPoint.Y;
                     this.myItem = null;
                     break;
                  }
               }
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.MouseUp() this.myItem != null");
         }
      }
      void MouseLeftButtonDownVerifyTerritory(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition(myCanvas);
         System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.MouseLeftButtonDownVerifyTerritory(): {0}", p.ToString());
         ITerritory selectedTerritory = null;
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  string tag = ellipse.Tag.ToString();
                  foreach (Territory t in Territory.theTerritories)
                  {
                     string name = Utilities.RemoveSpaces(t.ToString());
                     if (tag == name)
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (null != selectedTerritory)
                     break;
               }
            }
         }
         if (null == selectedTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.MouseUp() this.myItem != null");
            return;
         }
         TerritoryVerifyDialog dialog = new TerritoryVerifyDialog(selectedTerritory, myXColumn);
         dialog.myButtonOk.Focus();
         if (true == dialog.ShowDialog())
         {
            selectedTerritory.Type = dialog.RadioOutputText;
            selectedTerritory.IsTown = dialog.IsTown;
            selectedTerritory.IsCastle = dialog.IsCastle;
            selectedTerritory.IsRuin = dialog.IsRuin;
            selectedTerritory.IsTemple = dialog.IsTemple;
            selectedTerritory.IsOasis = dialog.IsOasis;
            selectedTerritory.CenterPoint.X = Double.Parse(dialog.CenterPointX);
            myXColumn = selectedTerritory.CenterPoint.X; // Want the same X value as specified in the last dialog. This lines up dots.
            selectedTerritory.CenterPoint.Y = Double.Parse(dialog.CenterPointY);
         }
      }
      void MouseLeftButtonDownSetRoads(object sender, MouseButtonEventArgs e)
      {
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // nearly transparent but slightly colored
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black fill
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };                       // red fill
         System.Windows.Point p = e.GetPosition(myCanvas);
         foreach (UIElement ui in myCanvas.Children) // find the selected Ellipse that was clicked
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  Territory selectedTerritory = null;  // Find the corresponding Territory that user selected
                  foreach (Territory t in Territory.theTerritories) // Find the corresponding territory that correponds to clicked Ellipse
                  {
                     if (selectedEllipse.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (selectedTerritory == null) // Check for error
                  {
                     MessageBox.Show("Unable to find " + selectedEllipse.Tag.ToString());
                     return;
                  }
                  if (null == myAnchorTerritory)  // If there is no anchor territory. Set it to selected territory and return
                  {
                     myAnchorTerritory = selectedTerritory;
                     myAnchorTerritory.Roads.Clear();
                     selectedEllipse.Fill = aSolidColorBrush3;
                     return;
                  }
                  // Is this point is reached, the anchor territory was previously selected and is not null.
                  // Determine if the selected ellipse is the same as the anchor terrirtory or a different ellipse.
                  if (selectedTerritory.ToString() != myAnchorTerritory.ToString())
                  {
                     selectedEllipse.Fill = aSolidColorBrush2; // If the matching territory is not the anchor territory, change its color to black
                                                               // Find if the territory is already in the list. Only add it if it is not already added.
                     IEnumerable<string> results = from s in myAnchorTerritory.Roads where s == selectedTerritory.ToString() select s;
                     if (0 == results.Count())
                     {
                        System.Diagnostics.Debug.WriteLine("Adding {0} ", selectedTerritory.ToString());
                        myAnchorTerritory.Roads.Add(selectedTerritory.ToString()); // add to the anchor territory's roads list
                     }
                  }
                  else
                  {
                     // If this is the matching territory is the anchor territory, the user is requesting that they are done adding 
                     // to the roads list for the anchor territtory. Clear the data so another anchor territory can be selected.
                     StringBuilder sb = new StringBuilder("Saving"); sb.Append(selectedEllipse.Tag.ToString()); sb.Append(" "); sb.Append(myAnchorTerritory.ToString());
                     sb.Append(" "); sb.Append(selectedTerritory.ToString()); sb.Append(" ");
                     System.Diagnostics.Debug.WriteLine("Saving {0} ", selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;
                     foreach (UIElement ui1 in myCanvas.Children)
                     {
                        if (ui1 is Ellipse) // find teh anchor ellipse
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           foreach (Territory t in Territory.theTerritories)
                           {
                              if (ellipse1.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                              {
                                 if (0 == t.Roads.Count)
                                    ellipse1.Fill = aSolidColorBrush0; // return back to red
                                 else
                                    ellipse1.Fill = aSolidColorBrush1; // return back to clear 
                                 break;
                              }
                           }
                        }
                     }
                  } // else (selectedTerritory.ToString() != myAnchorTerritory.ToString())
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvas.Children)
      }
      void MouseLeftButtonDownSetRivers(object sender, MouseButtonEventArgs e)
      {
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // nearly transparent but slightly colored
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black fill
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };                       // red fill
         System.Windows.Point p = e.GetPosition(myCanvas);
         foreach (UIElement ui in myCanvas.Children) // find the selected Ellipse that was clicked
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  Territory selectedTerritory = null;  // Find the corresponding Territory that user selected
                  foreach (Territory t in Territory.theTerritories) // Find the corresponding territory that correponds to clicked Ellipse
                  {
                     if (selectedEllipse.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (selectedTerritory == null) // Check for error
                  {
                     MessageBox.Show("Unable to find " + selectedEllipse.Tag.ToString());
                     return;
                  }
                  if (null == myAnchorTerritory)  // If there is no anchor territory. Set it to selected territory and return
                  {
                     myAnchorTerritory = selectedTerritory;
                     myAnchorTerritory.Rivers.Clear();
                     selectedEllipse.Fill = aSolidColorBrush3;
                     return;
                  }
                  // Is this point is reached, the anchor territory was previously selected and is not null.
                  // Determine if the selected ellipse is the same as the anchor terrirtory or a different ellipse.
                  if (selectedTerritory.ToString() != myAnchorTerritory.ToString())
                  {
                     selectedEllipse.Fill = aSolidColorBrush2; // If the matching territory is not the anchor territory, change its color to black
                                                               // Find if the territory is already in the list. Only add it if it is not already added.
                     IEnumerable<string> results = from s in myAnchorTerritory.Rivers where s == selectedTerritory.ToString() select s;
                     if (0 == results.Count())
                     {
                        System.Diagnostics.Debug.WriteLine("Adding {0} ", selectedTerritory.ToString());
                        myAnchorTerritory.Rivers.Add(selectedTerritory.ToString()); // add to the anchor territory's Rivers list
                     }
                  }
                  else
                  {
                     // If this is the matching territory is the anchor territory, the user is requesting that they are done adding 
                     // to the Rivers list for the anchor territtory. Clear the data so another anchor territory can be selected.
                     StringBuilder sb = new StringBuilder("Saving"); sb.Append(selectedEllipse.Tag.ToString()); sb.Append(" "); sb.Append(myAnchorTerritory.ToString());
                     sb.Append(" "); sb.Append(selectedTerritory.ToString()); sb.Append(" ");
                     System.Diagnostics.Debug.WriteLine("Saving {0} ", selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;
                     foreach (UIElement ui1 in myCanvas.Children)
                     {
                        if (ui1 is Ellipse) // find teh anchor ellipse
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           foreach (Territory t in Territory.theTerritories)
                           {
                              if (ellipse1.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                              {
                                 if (0 == t.Rivers.Count)
                                    ellipse1.Fill = aSolidColorBrush0; // return back to red
                                 else
                                    ellipse1.Fill = aSolidColorBrush1; // return back to clear 
                                 break;
                              }
                           }
                        }
                     }
                  } // else (selectedTerritory.ToString() != myAnchorTerritory.ToString())
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvas.Children)
      }
      void MouseLeftButtonDownSetAdjacents(object sender, MouseButtonEventArgs e)
      {
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         System.Windows.Point p = e.GetPosition(myCanvas);
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  Territory selectedTerritory = null;  // Find the corresponding Territory that user selected
                  foreach (Territory t in Territory.theTerritories)
                  {
                     if (selectedEllipse.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (selectedTerritory == null) // Check for error
                  {
                     MessageBox.Show("Unable to find " + selectedEllipse.Tag.ToString());
                     return;
                  }
                  if (null == myAnchorTerritory)  // If there is no anchor territory. Set it.
                  {
                     StringBuilder sb = new StringBuilder("Anchoring: ");
                     sb.Append(selectedEllipse.Tag.ToString());
                     sb.Append(" ");
                     sb.Append(selectedTerritory.ToString());
                     sb.Append(" ");
                     System.Diagnostics.Debug.WriteLine("Anchoring {0} ", selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = selectedTerritory;
                     myAnchorTerritory.Adjacents.Clear();
                     selectedEllipse.Fill = aSolidColorBrush3;
                     return;
                  }
                  if (selectedTerritory.ToString() != myAnchorTerritory.ToString())
                  {
                     // If the matching territory is not the anchor territory, change its color.
                     selectedEllipse.Fill = aSolidColorBrush2;
                     // Find if the territory is already in the list. Only add it if it is not already added.
                     IEnumerable<string> results = from s in myAnchorTerritory.Adjacents where s == selectedTerritory.ToString() select s;
                     if (0 == results.Count())
                     {
                        System.Diagnostics.Debug.WriteLine("Adding {0} ", selectedTerritory.ToString());
                        myAnchorTerritory.Adjacents.Add(selectedTerritory.ToString());
                     }
                  }
                  else
                  {
                     // If this is the matching territory is the anchor territory, the user is requesting that it they are done adding 
                     // to the adjacents ellipse. Clear the data so another one can be selected.
                     StringBuilder sb = new StringBuilder("Saving"); sb.Append(selectedEllipse.Tag.ToString()); sb.Append(" "); sb.Append(myAnchorTerritory.ToString());
                     sb.Append(" "); sb.Append(selectedTerritory.ToString()); sb.Append(" ");
                     System.Diagnostics.Debug.WriteLine("Saving {0} ", selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;

                     foreach (UIElement ui1 in myCanvas.Children)
                     {
                        if (ui1 is Ellipse)
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           foreach (Territory t in Territory.theTerritories)
                           {
                              if (ellipse1.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                              {
                                 if (0 == t.Adjacents.Count)
                                    ellipse1.Fill = aSolidColorBrush0;
                                 else
                                    ellipse1.Fill = aSolidColorBrush1;
                                 break;
                              }
                           }
                        }
                     }
                  } // else (selectedTerritory.ToString() != myAnchorTerritory.ToString())
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvas.Children)
      }
      void MouseLeftDownSetRaftTerritories(object sender, MouseButtonEventArgs e)
      {
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         System.Windows.Point p = e.GetPosition(myCanvas);
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  Territory selectedTerritory = null;  // Find the corresponding Territory that user selected
                  foreach (Territory t in Territory.theTerritories)
                  {
                     if (selectedEllipse.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (selectedTerritory == null) // Check for error
                  {
                     MessageBox.Show("Unable to find " + selectedEllipse.Tag.ToString());
                     return;
                  }
                  if (null == myAnchorTerritory)  // If there is no anchor territory. Set it.
                  {
                     StringBuilder sb = new StringBuilder("Anchoring: ");
                     sb.Append(selectedEllipse.Tag.ToString());
                     sb.Append(" ");
                     sb.Append(selectedTerritory.ToString());
                     sb.Append(" ");
                     System.Diagnostics.Debug.WriteLine("Anchoring {0} ", selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = selectedTerritory;
                     myAnchorTerritory.Rafts.Clear();
                     selectedEllipse.Fill = aSolidColorBrush3;
                     return;
                  }
                  if (selectedTerritory.ToString() != myAnchorTerritory.ToString())
                  {
                     // If the matching territory is not the anchor territory,
                     // change its color.
                     selectedEllipse.Fill = aSolidColorBrush2;
                     // Find if the territory is already in the list.
                     // Only add it if it is not already added.
                     IEnumerable<string> results = from s in myAnchorTerritory.Rafts where s == selectedTerritory.ToString() select s;
                     if (0 == results.Count())
                     {
                        System.Diagnostics.Debug.WriteLine("Adding {0} ", selectedTerritory.ToString());
                        myAnchorTerritory.Rafts.Add(selectedTerritory.ToString());
                     }
                  }
                  else
                  {
                     // If this is the matching territory is the anchor territory, the user
                     // is requesting that it they are done adding 
                     // to the adjacents ellipse.
                     // Clear the data so another one can be selected.
                     StringBuilder sb = new StringBuilder("Saving");
                     sb.Append(selectedEllipse.Tag.ToString());
                     sb.Append(" ");
                     sb.Append(myAnchorTerritory.ToString());
                     sb.Append(" ");
                     sb.Append(selectedTerritory.ToString());
                     sb.Append(" ");
                     System.Diagnostics.Debug.WriteLine("Saving {0} ", selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;
                     foreach (UIElement ui1 in myCanvas.Children)
                     {
                        if (ui1 is Ellipse)
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           foreach (Territory t in Territory.theTerritories)
                           {
                              if (ellipse1.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                              {
                                 if (0 == t.Rafts.Count)
                                    ellipse1.Fill = aSolidColorBrush0;
                                 else
                                    ellipse1.Fill = aSolidColorBrush1;
                                 break;
                              }
                           }

                        }
                     }
                  } // else (selectedTerritory.ToString() != myAnchorTerritory.ToString())
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvas.Children)
      }
      void MouseLeftDownSetDownRiverTerritory(object sender, MouseButtonEventArgs e)
      {
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         System.Windows.Point p = e.GetPosition(myCanvas);
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  Territory selectedTerritory = null;  // Find the corresponding Territory that user selected
                  foreach (Territory t in Territory.theTerritories)
                  {
                     if (selectedEllipse.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (selectedTerritory == null) // Check for error
                  {
                     MessageBox.Show("Unable to find " + selectedEllipse.Tag.ToString());
                     return;
                  }
                  if (null == myAnchorTerritory)  // If there is no anchor territory. Set it.
                  {
                     myAnchorTerritory = selectedTerritory;
                     myAnchorTerritory.DownRiver = "";
                     selectedEllipse.Fill = aSolidColorBrush3;
                     return;
                  }
                  else
                  {
                     selectedEllipse.Fill = aSolidColorBrush2; // If the matching territory is not the anchor territory, change its color.
                     myAnchorTerritory.DownRiver = selectedTerritory.ToString(); // set the value
                     myAnchorTerritory = null;
                     foreach (UIElement ui1 in myCanvas.Children)
                     {
                        if (ui1 is Ellipse)
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           foreach (Territory t in Territory.theTerritories)
                           {
                              if (ellipse1.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                              {
                                 if ("" == t.DownRiver)
                                    ellipse1.Fill = aSolidColorBrush0;
                                 else
                                    ellipse1.Fill = aSolidColorBrush1;
                                 break;
                              }
                           }
                        }
                     }
                  }
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvas.Children)
      }
   }
}

