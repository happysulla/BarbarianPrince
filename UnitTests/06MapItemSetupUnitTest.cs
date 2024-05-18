using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
    public class MapItemSetupUnitTest : IUnitTest
    {
        private IGameEngine myGameEngine = null;
        private IGameInstance myGameInstance = null;
        //-----------------------------------------------------------
        private DockPanel myDockPanel = null;
        private Canvas myCanvas = null;
        private readonly List<Button> myButtons = new List<Button>();
        private Button myButtonSelected = null;
        private List<Ellipse> myEllipses = new List<Ellipse>();
        public static Double theEllipseOffset = 8;
        private ContextMenu myContextMenuButton = new ContextMenu();
        private Rectangle myRectangleSelected = new Rectangle(); // User manually selected this button. Highlight it with Rectangle
        //-----------------------------------------------------------
        private int myIndexName = 0;
        public bool CtorError { get; } = false;
        private List<string> myHeaderNames = new List<string>();
        private List<string> myCommandNames = new List<string>();
        public string HeaderName { get { return myHeaderNames[myIndexName]; } }
        public string CommandName { get { return myCommandNames[myIndexName]; } }
        //-----------------------------------------------------------
        public MapItemSetupUnitTest(DockPanel dp, IGameInstance gi, IGameEngine ge)
        {
            myIndexName = 0;
            myHeaderNames.Add("06-Delete MapItems");           // 0
            myHeaderNames.Add("06-Add MapItems");          // 1
            myHeaderNames.Add("06-Set Map Items Location");    // 2
            myHeaderNames.Add("06-Movement Local");         // 3
            myHeaderNames.Add("06-Finish");
            //------------------------------------------
            myCommandNames.Add("Delete File");
            myCommandNames.Add(" ");
            myCommandNames.Add(" ");
            myCommandNames.Add("Move Items Local");
            myCommandNames.Add(" ");
            //------------------------------------------
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
            if (null == myCanvas)
            {
                Logger.Log(LogEnum.LE_ERROR, "MapItemSetupUnitTest(): myCanvas=null");
                CtorError = true;
                return;
            }
            //------------------------------------------
            // Setup Context Menu for Buttons
            myContextMenuButton.Loaded += this.ContextMenuLoaded;
            MenuItem mi1 = new MenuItem() { Header = "_Return to Starting point", InputGestureText = "Ctrl+S" };
            mi1.Click += this.ContextMenuClickReturnToStart;
            myContextMenuButton.Items.Add(mi1);
            MenuItem mi2 = new MenuItem { Header = "_Rotate Stack", InputGestureText = "Ctrl+R" };
            mi2.Click += this.ContextMenuClickRotate;
            myContextMenuButton.Items.Add(mi2);
            MenuItem mi3 = new MenuItem() { Header = "_Flip", InputGestureText = "Ctrl+F" };
            mi3.Click += this.ContextMenuFlip;
            myContextMenuButton.Items.Add(mi3);
            MenuItem mi4 = new MenuItem() { Header = "_Unflip", InputGestureText = "Ctrl+U" };
            mi4.Click += this.ContextMenuUnflip;
            myContextMenuButton.Items.Add(mi4);
            //------------------------------------------
            // Create a Bounding Rectangle to indicate when a MapItem is selected. Initially not visible. It becomes visible when a MapItem is selected
            myRectangleSelected.Stroke = Brushes.Red;
            myRectangleSelected.StrokeThickness = 3.0;
            myRectangleSelected.Width = Utilities.theMapItemSize;
            myRectangleSelected.Height = Utilities.theMapItemSize;
            myRectangleSelected.Visibility = Visibility.Hidden;
            myCanvas.Children.Add(myRectangleSelected);
            Canvas.SetZIndex(myRectangleSelected, 1000);
            //------------------------------------------
            ITerritory starting = gi.Territories.Find("0507");
            IStack newStack = gi.Stacks.Find(starting);
            if (null == newStack)
            {
                newStack = new Stack(starting) as IStack;
                gi.Stacks.Add(newStack);
            }
            foreach (IMapItem mi in gi.PartyMembers)
            {
                IStack oldStack = gi.Stacks.Find(mi.Name);
                if (null != oldStack)
                    oldStack.MapItems.Remove(mi.Name);
                mi.TerritoryStarting = starting;
                mi.Territory = starting;
                mi.IsHidden = false;
                newStack.MapItems.Add(mi);
            }
            this.myGameEngine = ge;
            this.myGameInstance = gi;
        }
        public bool Command(ref IGameInstance gi)
        {
            if (CommandName == myCommandNames[0])
            {
                //System.IO.File.Delete("../Config/MapItems.xml");  // delete old file
                if (false == NextTest(ref gi)) // automatically move next test
                {
                    Console.WriteLine("MapItems.Command(): NextTest() Returned false");
                    return false;
                }
            }
            else if (CommandName == myCommandNames[3])
            {
                UpdateCanvas();
            }
            return true;
        }
        public bool NextTest(ref IGameInstance gi)
        {
            if (HeaderName == myHeaderNames[0])
            {
                CreateEllipses(gi.Territories);
                if (false == CreateButtons(gi.MapItems))
                {
                    Logger.Log(LogEnum.LE_ERROR, "NextTest(): CreateButtons returned false");
                    return false;
                }
                ++myIndexName;
            }
            else if (HeaderName == myHeaderNames[1])  // changing from "Add new map item"
            {
                foreach (UIElement ui in myCanvas.Children)
                {
                    if (ui is Ellipse e)
                        e.MouseDown -= this.MouseDownEllipseCreate;
                    if (ui is Button b)
                    {
                        b.Click -= this.ClickedButtonCreate;
                        b.Click += this.ClickedSelectMapItem;
                    }
                }
                myCanvas.MouseMove += MouseMoveMapItem;
                myCanvas.MouseDown += MouseDownCanvas;
                ++myIndexName;
            }
            else if (HeaderName == myHeaderNames[2])   // changing from "Set new map item location"
            {
                myCanvas.MouseMove -= MouseMoveMapItem;
                myCanvas.MouseDown -= MouseDownCanvas;
                List<UIElement> results = new List<UIElement>();
                foreach (UIElement ui in myCanvas.Children)
                {
                    if (ui is Ellipse e)
                        results.Add(ui);
                }
                foreach (UIElement ui1 in results)  // remove all ellipses
                    myCanvas.Children.Remove(ui1);
                CreatePolygons(gi.Territories);
                foreach (UIElement ui in myCanvas.Children)
                {
                    if (ui is Button b)
                    {
                        b.Click -= this.ClickedSelectMapItem;
                        b.Click += this.ClickedSelectMapItem2;
                    }
                }
                UpdateCanvas();
                ++myIndexName;
            }
            else if (HeaderName == myHeaderNames[3])  // changing from "Perform map item movement"
            {
                UpdateCanvas();
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
            //-----------------------------------------------------------------------
            // Clean up event handlers
            if (HeaderName == myHeaderNames[1])  // add new map item
            {
                foreach (UIElement ui in myCanvas.Children)
                {
                    if (ui is Ellipse e)
                        e.MouseLeftButtonDown -= this.MouseDownEllipseCreate;
                    if (ui is Button b)
                        b.MouseLeftButtonDown -= this.ClickedButtonCreate;
                }
                myCanvas.MouseMove -= MouseMoveMapItem;
                myCanvas.MouseDown -= MouseDownCanvas;
            }
            else if (HeaderName == myHeaderNames[2])  // set map item locations
            {
                foreach (UIElement ui in myCanvas.Children)
                {
                    if (ui is Button b)
                        b.MouseLeftButtonDown -= this.ClickedButtonCreate;
                }
            }
            else if (HeaderName == myHeaderNames[3])  // perform map item movement
            {
                foreach (UIElement ui in myCanvas.Children)
                {
                    if (ui is Button b)
                        b.MouseLeftButtonDown -= this.ClickedSelectMapItem2;
                }
            }
            //-----------------------------------------------------------------------
            List<UIElement> results = new List<UIElement>(); // Remove all elements from Canvas
            foreach (UIElement ui in myCanvas.Children)
            {
                if (ui is Ellipse)
                    results.Add(ui);
                if (ui is Rectangle)
                    results.Add(ui);
                if (ui is Polyline)
                    results.Add(ui);
            }
            foreach (UIElement ui1 in results)
                myCanvas.Children.Remove(ui1);
            //-----------------------------------------------------------------------
            // Delete Existing MapItems.xml file and create a new one
            try
            {
                if (null != gi.MapItems) // do not delete if nothing added
                {
                    System.IO.File.Delete("../Config/MapItems.xml");  // delete old file
                    XmlDocument aXmlDocument = CreateMapItemsXml(myGameInstance.MapItems); // create a new XML document based on Territories
                    if (null == aXmlDocument)
                    {
                        Logger.Log(LogEnum.LE_ERROR, "MapItemSetupUnitTest.Cleanup(): CreateMapItemsXml() returned null");
                        return false;
                    }
                    using (FileStream writer = new FileStream("../Config/MapItems.xml", FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
                        using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
                            aXmlDocument.Save(xmlWriter);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("MapItemSetupUnitTest.Cleanup() exeption={0}", e.Message);
                return false;
            }
            ++gi.GameTurn;
            return true;
        }
        //-----------------------------------------------------------
        public XmlDocument CreateMapItemsXml(IMapItems mapItems)
        {
            try
            {
                XmlDocument aXmlDocument = new XmlDocument();
                if (null == mapItems)
                    return aXmlDocument;
                aXmlDocument.LoadXml("<MapItems></MapItems>");
                foreach (IMapItem mi in mapItems)
                {
                    XmlElement mapItemElem = aXmlDocument.CreateElement("MapItem");
                    mapItemElem.SetAttribute("value", mi.Name);
                    aXmlDocument.DocumentElement.AppendChild(mapItemElem);
                    //---------------------------------------------------
                    XmlElement zoomElem = aXmlDocument.CreateElement("Zoom");
                    zoomElem.SetAttribute("value", mi.Zoom.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(zoomElem);
                    //---------------------------------------------------
                    XmlElement isHiddenElem = aXmlDocument.CreateElement("IsHidden");
                    isHiddenElem.SetAttribute("value", mi.IsHidden.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(isHiddenElem);
                    //---------------------------------------------------
                    XmlElement isAnimatedElem = aXmlDocument.CreateElement("IsAnimated");
                    isAnimatedElem.SetAttribute("value", mi.IsAnimated.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(isAnimatedElem);
                    //---------------------------------------------------
                    XmlElement isGuideElem = aXmlDocument.CreateElement("IsGuide");
                    isGuideElem.SetAttribute("value", mi.IsGuide.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(isGuideElem);
                    //---------------------------------------------------
                    XmlElement enduranceElem = aXmlDocument.CreateElement("Endurance");
                    enduranceElem.SetAttribute("value", mi.Endurance.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(enduranceElem);
                    //---------------------------------------------------
                    XmlElement movementElem = aXmlDocument.CreateElement("Movement");
                    movementElem.SetAttribute("value", mi.Movement.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(movementElem);
                    //---------------------------------------------------
                    XmlElement combatElem = aXmlDocument.CreateElement("Combat");
                    combatElem.SetAttribute("value", mi.Combat.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(combatElem);
                    //---------------------------------------------------
                    XmlElement coinElem = aXmlDocument.CreateElement("Coin");
                    coinElem.SetAttribute("value", mi.Coin.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(coinElem);
                    //---------------------------------------------------
                    XmlElement topImageNameElem = aXmlDocument.CreateElement("TopImageName");
                    topImageNameElem.SetAttribute("value", mi.TopImageName);
                    aXmlDocument.DocumentElement.LastChild.AppendChild(topImageNameElem);
                    //---------------------------------------------------
                    XmlElement bottomImageNameElem = aXmlDocument.CreateElement("BottomImageName");
                    bottomImageNameElem.SetAttribute("value", mi.BottomImageName);
                    aXmlDocument.DocumentElement.LastChild.AppendChild(bottomImageNameElem);
                    //---------------------------------------------------
                    XmlElement overlayImageNameElem = aXmlDocument.CreateElement("OverlayImageName");
                    overlayImageNameElem.SetAttribute("value", mi.OverlayImageName);
                    aXmlDocument.DocumentElement.LastChild.AppendChild(overlayImageNameElem);
                    //---------------------------------------------------
                    XmlElement locationElem = aXmlDocument.CreateElement("Location");
                    locationElem.SetAttribute("X", mi.Location.X.ToString());
                    locationElem.SetAttribute("Y", mi.Location.Y.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(locationElem);
                    //---------------------------------------------------
                    XmlElement enduranceUsedElem = aXmlDocument.CreateElement("Wound");
                    enduranceUsedElem.SetAttribute("value", mi.MovementUsed.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(enduranceUsedElem);
                    //---------------------------------------------------
                    XmlElement movementUsedElem = aXmlDocument.CreateElement("MovementUsed");
                    movementUsedElem.SetAttribute("value", mi.MovementUsed.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(movementUsedElem);
                    //---------------------------------------------------
                    XmlElement territoryElem = aXmlDocument.CreateElement("TerritoryName");
                    territoryElem.SetAttribute("value", mi.Territory.Name);
                    aXmlDocument.DocumentElement.LastChild.AppendChild(territoryElem);
                    //---------------------------------------------------
                    XmlElement territoryStartingElem = aXmlDocument.CreateElement("TerritoryStartingName");
                    territoryStartingElem.SetAttribute("value", mi.TerritoryStarting.Name);
                    aXmlDocument.DocumentElement.LastChild.AppendChild(territoryStartingElem);
                    //---------------------------------------------------
                    XmlElement isKilledElem = aXmlDocument.CreateElement("IsKilled");
                    isKilledElem.SetAttribute("value", mi.IsKilled.ToString());
                    aXmlDocument.DocumentElement.LastChild.AppendChild(isKilledElem);
                }
                return aXmlDocument;
            }
            catch (Exception e)
            {
                Console.WriteLine("TerritoryUnitTest.Command() exeption={0}", e.Message);
                return null;
            }
        }
        public void CreateEllipses(List<ITerritory> territories)
        {
            SolidColorBrush aSolidColorBrushRed = new SolidColorBrush { Color = Colors.Red };
            foreach (Territory t in territories)
            {
                Ellipse aEllipse = new Ellipse { Tag = Utilities.RemoveSpaces(t.ToString()) };
                aEllipse.Fill = aSolidColorBrushRed;
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
                aEllipse.MouseDown += this.MouseDownEllipseCreate;
            }
        }
        public bool CreateButtons(IMapItems mapItems)
        {
            //--------------------------------------------
            foreach (IMapItem mi in mapItems) // add the MapItems to stacks
            {
                if (null != myGameInstance.PartyMembers.Find(mi.Name))  // do not add to stack if already a PartyMember
                    continue;
                ITerritory t = mi.Territory;
                if (null == t)
                {
                    Logger.Log(LogEnum.LE_ERROR, "CreateButtons(): t=null");
                    return false;
                }
                IStack stack = myGameInstance.Stacks.Find(t);
                if (null == stack)
                {
                    stack = new Stack(t) as IStack;
                    myGameInstance.Stacks.Add(stack);
                }
                stack.MapItems.Add(mi);
            }
            //--------------------------------------------
            foreach (IStack stack in myGameInstance.Stacks) // Set the buttons on the canvas
            {
                int counterCount = 0;
                foreach (IMapItem mi in stack.MapItems)
                {
                    if (false == CreateButton(mi, counterCount))
                    {
                        Logger.Log(LogEnum.LE_ERROR, "CreateButtons(): CreateButton() returned false");
                        return false;
                    }
                    ++counterCount;
                }
            }
            return true;
        }
        public bool CreateButton(IMapItem mi, int counterCount)
        {
            string territoryName = Utilities.RemoveSpaces(mi.Territory.ToString());
            ITerritory territory = TerritoryExtensions.Find(myGameInstance.Territories, territoryName);
            if (null == territory)
            {
                Logger.Log(LogEnum.LE_ERROR, "CreateMapItem(): TerritoryExtensions.Find() returned null");
                return false;
            }
            System.Windows.Controls.Button b = new Button { };
            b.ContextMenu = myContextMenuButton;
            Canvas.SetLeft(b, territory.CenterPoint.X - mi.Zoom * Utilities.theMapItemOffset + (counterCount * 3));
            Canvas.SetTop(b, territory.CenterPoint.Y - mi.Zoom * Utilities.theMapItemOffset + (counterCount * 3));
            b.Name = Utilities.RemoveSpaces(mi.Name);
            b.Width = mi.Zoom * Utilities.theMapItemSize;
            b.Height = mi.Zoom * Utilities.theMapItemSize;
            b.IsEnabled = true;
            b.BorderThickness = new Thickness(0);
            b.Background = new SolidColorBrush(Colors.Transparent);
            b.Foreground = new SolidColorBrush(Colors.Transparent);
            b.Click += this.ClickedButtonCreate;
            MapItem.SetButtonContent(b, mi, false, true); // This sets the image as the button's content
            myButtons.Add(b);
            myCanvas.Children.Add(b);
            Canvas.SetZIndex(b, counterCount);
            return true;
        }
        public void CreatePolygons(List<ITerritory> territories)
        {
            SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(10, 100, 100, 0) }; // nearly transparent but slightly colored
            foreach (Territory t in territories)
            {
                PointCollection points = new PointCollection();
                foreach (IMapPoint mp1 in t.Points)
                    points.Add(new System.Windows.Point(mp1.X, mp1.Y));
                Polygon aPolygon = new Polygon { Fill = aSolidColorBrush0, Points = points, Tag = t.ToString() };
                Canvas.SetZIndex(aPolygon, 0);
                aPolygon.MouseDown += this.MouseDownPolygon;
                myCanvas.Children.Add(aPolygon);
            }
        }
        private bool CreateMapItemMove(List<ITerritory> territories, IStacks stacks, ITerritory selectedTerritory, IMapItem selectedMapItem)
        {
            if (null == selectedTerritory)
            {
                Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): selectedTerritory=null");
                return false;
            }
            if (null == selectedMapItem)
            {
                Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): selectedMapItem=null");
                return false;
            }
            IStack oldStack = stacks.Find(selectedMapItem.Territory);
            if (null == oldStack)
            {
                Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): oldStack=null");
                return false;
            }
            int movementLeftToUse = selectedMapItem.Movement - selectedMapItem.MovementUsed;
            // MapItem already selected to move.  Moving it to a known space
            if (selectedTerritory.Name == selectedMapItem.Territory.Name)
            {
                oldStack.Rotate(); // rotate the stack
            }
            else if (selectedMapItem.Movement <= selectedMapItem.MovementUsed) // already used up movement
            {
                oldStack.Rotate(); // rotate the stack
            }
            else if (movementLeftToUse < 1)
            {
                MessageBox.Show("No movement left");
                return true;
            }
            else
            {
                selectedMapItem.TerritoryStarting = selectedMapItem.Territory;
                MapItemMove mim = new MapItemMove(territories, selectedMapItem, selectedTerritory);
                if ((0 == mim.BestPath.Territories.Count) || (null == mim.NewTerritory))
                {
                    MessageBox.Show("Unable to take this path due to overstacking restrictions. Choose another endpoint.");
                    return true;
                }
                myGameInstance.MapItemMoves.Add(mim);
            }
            return true;
        }
        private bool CreateMapItemMove(List<ITerritory> territories, IGameInstance gi, ITerritory newTerritory)
        {
            if (null == newTerritory)
            {
                Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): newTerritory=null");
                return false;
            }

            IStack oldStack = gi.Stacks.Find(gi.Prince.Territory);
            if (null == oldStack)
            {
                Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): oldStack=null");
                return false;
            }
            int movementLeftToUse = gi.Prince.Movement - gi.Prince.MovementUsed;
            // MapItem already selected to move.  Moving it to a known space
            if (newTerritory.Name == gi.Prince.Territory.Name)
            {
                oldStack.Rotate(); // rotate the stack
            }
            else if (gi.Prince.Movement <= gi.Prince.MovementUsed) // already used up movement
            {
                oldStack.Rotate(); // rotate the stack
            }
            else if (movementLeftToUse < 1)
            {
                MessageBox.Show("No movement left");
                return true;
            }
            else
            {
                if (0 == gi.PartyMembers.Count)
                {
                    Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): gi.PartyMembers.Count=0");
                    return false;
                }
                while ("Prince" != gi.PartyMembers[0].Name) // get to top
                    gi.PartyMembers.Rotate(1);
                gi.PartyMembers.Reverse(); // move to bottom
                foreach (IMapItem mi in gi.PartyMembers)
                {
                    mi.TerritoryStarting = gi.Prince.Territory;
                    MapItemMove mim = new MapItemMove(territories, mi, newTerritory);
                    if ((0 == mim.BestPath.Territories.Count) || (null == mim.NewTerritory))
                    {
                        MessageBox.Show("Unable to take this path due to overstacking restrictions. Choose another endpoint.");
                        return true;
                    }
                    gi.MapItemMoves.Add(mim);
                }

            }
            return true;
        }
        private bool MovePathDisplay(IMapItemMove mim, int mapItemCount)
        {
            if (null == mim.NewTerritory)
            {
                Logger.Log(LogEnum.LE_ERROR, "MovePathDisplay(): mim.NewTerritory=null");
                return false;
            }
            //-----------------------------------------
            PointCollection aPointCollection = new PointCollection();
            double offset = 0.0;
            if (0 < mapItemCount)
            {
                if (0 == mapItemCount % 2)
                    offset = mapItemCount - 1;
                else
                    offset = -mapItemCount;
            }
            offset *= 3.0;
            double xPostion = mim.OldTerritory.CenterPoint.X + offset;
            double yPostion = mim.OldTerritory.CenterPoint.Y + offset;
            System.Windows.Point newPoint = new System.Windows.Point(xPostion, yPostion);
            aPointCollection.Add(newPoint);
            foreach (ITerritory t in mim.BestPath.Territories)
            {
                xPostion = t.CenterPoint.X + offset;
                yPostion = t.CenterPoint.Y + offset;
                newPoint = new System.Windows.Point(xPostion, yPostion);
                aPointCollection.Add(newPoint);
            }
            return true;
        }
        private bool MovePathAnimate(IMapItemMove mim, int newStackCount)
        {
            const int ANIMATE_TIME_SEC = 3;
            if (null == mim.NewTerritory)
            {
                Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for n=" + mim.MapItem.Name);
                return false;
            }
            Button b = myButtons.Find(Utilities.RemoveSpaces(mim.MapItem.Name));
            if (null == b)
            {
                Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for n=" + mim.MapItem.Name);
                return false;
            }
            try
            {
                Canvas.SetZIndex(b, 100 + 3 * newStackCount); // Move the button to the top of the Canvas
                PathFigure aPathFigure = new PathFigure() { StartPoint = new System.Windows.Point(mim.MapItem.Location.X, mim.MapItem.Location.Y) };
                int lastItemIndex = mim.BestPath.Territories.Count - 1;
                for (int i = 0; i < lastItemIndex; i++)
                {
                    ITerritory t = mim.BestPath.Territories[i];
               System.Windows.Point newPoint = new System.Windows.Point(t.CenterPoint.X - Utilities.theMapItemOffset, t.CenterPoint.Y - Utilities.theMapItemOffset);
                    LineSegment lineSegment = new LineSegment(newPoint, false);
                    aPathFigure.Segments.Add(lineSegment);
                }
            // Add the last line segment
            System.Windows.Point newPoint2 = new System.Windows.Point(mim.NewTerritory.CenterPoint.X + (3 * newStackCount) - Utilities.theMapItemOffset, mim.NewTerritory.CenterPoint.Y + (3 * newStackCount) - Utilities.theMapItemOffset);
                LineSegment lineSegment2 = new LineSegment(newPoint2, false);
                aPathFigure.Segments.Add(lineSegment2);
                // Animiate the map item along the line segment
                PathGeometry aPathGeo = new PathGeometry();
                aPathGeo.Figures.Add(aPathFigure);
                aPathGeo.Freeze();
                DoubleAnimationUsingPath xAnimiation = new DoubleAnimationUsingPath();
                xAnimiation.PathGeometry = aPathGeo;
                xAnimiation.Duration = TimeSpan.FromSeconds(ANIMATE_TIME_SEC);
                xAnimiation.Source = PathAnimationSource.X;
                DoubleAnimationUsingPath yAnimiation = new DoubleAnimationUsingPath();
                yAnimiation.PathGeometry = aPathGeo;
                yAnimiation.Duration = TimeSpan.FromSeconds(ANIMATE_TIME_SEC);
                yAnimiation.Source = PathAnimationSource.Y;
                b.RenderTransform = new TranslateTransform();
                b.BeginAnimation(Canvas.LeftProperty, xAnimiation);
                b.BeginAnimation(Canvas.TopProperty, yAnimiation);
                if (null == myRectangleSelected)
                {
                    Console.WriteLine("MovePathAnimate() myRectangleSelection=null");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
                b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
                myRectangleSelected.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
                myRectangleSelected.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
                Console.WriteLine("MovePathAnimate() - EXCEPTION THROWN e={0}", e.ToString());
                return false;
            }
        }
        private void UpdateCanvas()
        {
            foreach (IStack stack in myGameInstance.Stacks) // Set the buttons on the canvas
            {
                int counterCount = 0;
                foreach (IMapItem mi in stack.MapItems)
                {
                    Button b = myButtons.Find(Utilities.RemoveSpaces(mi.Name));
                    if (null != b)
                    {
                        b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
                        b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
                        mi.SetLocation(counterCount);
                        ++counterCount;
                        Canvas.SetLeft(b, mi.Location.X);
                        Canvas.SetTop(b, mi.Location.Y);
                        Canvas.SetZIndex(b, counterCount);
                    }
                }
            }
            if (0 < myGameInstance.MapItemMoves.Count)
            {
                //int count = 0;
                foreach (IMapItemMove mim in myGameInstance.MapItemMoves)
                {
                    IStack oldStack = myGameInstance.Stacks.Find(mim.OldTerritory);
                    if (null == oldStack)
                    {
                        Logger.Log(LogEnum.LE_ERROR, "Command(): oldStack=null for t=" + mim.OldTerritory.ToString());
                        return;
                    }
                    IStack newStack = myGameInstance.Stacks.Find(mim.NewTerritory);
                    if (null == newStack)
                    {
                        newStack = new Stack(mim.NewTerritory);
                        myGameInstance.Stacks.Add(newStack);
                    }
                    //if (false == MovePathDisplay(mim, count++))
                    //{
                    //   Logger.Log(LogEnum.LE_ERROR, "Command(): MovePathDisplay() returned false t=" + mim.OldTerritory.ToString());
                    //   return;
                    //}
                    if (false == MovePathAnimate(mim, newStack.MapItems.Count))
                    {
                        Logger.Log(LogEnum.LE_ERROR, "Command(): MovePathAnimate() returned false t=" + mim.OldTerritory.ToString());
                        return;
                    }
                    mim.MapItem.Territory = mim.NewTerritory;
                    mim.MapItem.TerritoryStarting = mim.NewTerritory;
                    newStack.MapItems.Add(mim.MapItem);
                    oldStack.MapItems.Remove(mim.MapItem);
                    if (0 == oldStack.MapItems.Count)
                        myGameInstance.Stacks.Remove(oldStack);
                }
                myGameInstance.MapItemMoves.Clear();
                return;
            }
            List<UIElement> lines = new List<UIElement>();
            foreach (UIElement ui in myCanvas.Children)
            {
                if (ui is Polyline)
                    lines.Add(ui);
            }
            foreach (UIElement line in lines)
                myCanvas.Children.Remove(line);
        }
        //-----------------------------------------------------------
        private void ContextMenuLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu cm)
            {
                for (int i = 0; i < cm.Items.Count; ++i) // Gray out all menu items as default
                {
                    if (cm.Items[i] is MenuItem menuItem)
                        menuItem.IsEnabled = true;
                }
                if (cm.PlacementTarget is Button b)
                {
                    IMapItem mi = myGameInstance.MapItems.Find(b.Name);
                    if (1 < cm.Items.Count) // Gray out the "Rotate Stack" menu item
                    {
                        if (cm.Items[1] is MenuItem menuItem)
                        {
                            IStack stack = myGameInstance.Stacks.Find(mi.Territory);
                            if (stack.MapItems.Count < 2)
                                menuItem.IsEnabled = false;
                        }
                    }
                }
            }
        }
        private void ContextMenuClickReturnToStart(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                if (mi.Parent is ContextMenu)
                {
                    ContextMenu cm = (ContextMenu)mi.Parent;
                    if (cm.PlacementTarget is Button b)
                    {
                    }
                }
            }
        }
        private void ContextMenuClickRotate(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.Parent is ContextMenu cm)
                {
                    if (cm.PlacementTarget is Button b)
                    {
                        IMapItem mi = myGameInstance.MapItems.Find(b.Name);
                        ITerritory t = mi.Territory;
                        IStack stack = myGameInstance.Stacks.Find(t);
                        if (null == stack)
                        {
                            Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickRotate(): stack=null for name=" + b.Name);
                            return;
                        }
                        stack.Rotate();
                        UpdateCanvas();
                    }
                }
            }
        }
        private void ContextMenuFlip(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.Parent is ContextMenu cm)
                {
                    if (cm.PlacementTarget is Button b)
                    {
                        IMapItem mi = myGameInstance.MapItems.Find(b.Name);
                        mi.Flip();
                        MapItem.SetButtonContent(b, mi, false, true);

                    }
                }
            }
        }
        private void ContextMenuUnflip(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.Parent is ContextMenu cm)
                {
                    if (cm.PlacementTarget is Button b)
                    {
                        IMapItem mi = myGameInstance.MapItems.Find(b.Name);
                        mi.Unflip();
                        MapItem.SetButtonContent(b, mi, false, true);

                    }
                }
            }
        }
        private void MouseDownEllipseCreate(object sender, MouseButtonEventArgs e)
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
            string matchingTerritoryName = Utilities.RemoveSpaces(matchingTerritory.ToString());
            MapItemCreateDialog dialog = new MapItemCreateDialog();
            dialog.myButtonOk.Focus();
            if (true == dialog.ShowDialog())
            {
                // Create the Image from the passed in data
                MapItem mapItem = new MapItem(dialog.MapItemName, dialog.Zoom, dialog.IsHidden, dialog.IsAnimated, false, dialog.TopImageName, dialog.BottomImageName, matchingTerritory, Int32.Parse(dialog.Endurance), Int32.Parse(dialog.Combat), 0);
                if (null == mapItem)
                {
                    Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): unable to new mapItem");
                    return;
                }
                myGameInstance.MapItems.Add(mapItem);
                //--------------------------------------------------
                ITerritory territory = TerritoryExtensions.Find(myGameInstance.Territories, matchingTerritoryName);
                IStack stack = myGameInstance.Stacks.Find(matchingTerritory);
                if (null == stack)
                {
                    stack = new Stack(matchingTerritory) as IStack;
                    myGameInstance.Stacks.Add(stack);
                }
                stack.MapItems.Add(mapItem);
                //--------------------------------------------------
                if (false == CreateButton(mapItem, stack.MapItems.Count - 1))
                {
                    Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): unable to create mapItem");
                    return;
                }
            }
            e.Handled = true;
        }
        private void ClickedButtonCreate(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            IMapItem mousedMapItem = myGameInstance.MapItems.Find(clickedButton.Name);
            if (null == mousedMapItem)
            {
                Logger.Log(LogEnum.LE_ERROR, "ClickedButtonCreate(): mapItem=null for name=" + clickedButton.Name);
                return;
            }
            ITerritory t = mousedMapItem.Territory;
            string territoryName = Utilities.RemoveSpaces(t.ToString());
            MapItemCreateDialog dialog = new MapItemCreateDialog();
            dialog.myTextBoxTopImageName.Focus();
            if (true == dialog.ShowDialog())
            {
                MapItem mapItem = new MapItem(dialog.MapItemName, dialog.Zoom, dialog.IsHidden, dialog.IsAnimated, false, dialog.TopImageName, dialog.BottomImageName, t, Int32.Parse(dialog.Endurance), Int32.Parse(dialog.Combat), 0);
                if (null == mapItem)
                {
                    Logger.Log(LogEnum.LE_ERROR, "clickedButton(): unable to new mapItem");
                    return;
                }
                myGameInstance.MapItems.Add(mapItem);
                //--------------------------------------------------
                IStack stack = myGameInstance.Stacks.Find(t);
                if (null == stack)
                {
                    stack = new Stack(t) as IStack;
                    myGameInstance.Stacks.Add(stack);
                }
                stack.MapItems.Add(mapItem);
                //--------------------------------------------------
                if (false == CreateButton(mapItem, stack.MapItems.Count - 1))
                {
                    Logger.Log(LogEnum.LE_ERROR, "clickedButton(): unable to create mapItem");
                    return;
                }
            }
            e.Handled = true;
        }
        private void ClickedSelectMapItem(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            double x = Canvas.GetLeft(clickedButton);
            double y = Canvas.GetTop(clickedButton);
            if (null == myButtonSelected)
            {
                myButtonSelected = clickedButton;
                Canvas.SetZIndex(myButtonSelected, 999);
                myRectangleSelected.BeginAnimation(Canvas.LeftProperty, null);
                myRectangleSelected.BeginAnimation(Canvas.TopProperty, null);
                Canvas.SetLeft(myRectangleSelected, x);
                Canvas.SetTop(myRectangleSelected, y);
                myRectangleSelected.Visibility = Visibility.Visible;             // highlight the moving button with a rectangle
            }
            else
            {
                IMapItem movingMapItem = myGameInstance.MapItems.Find(Utilities.RemoveSpaces(myButtonSelected.Name)); // Find the corresponding MapItem
                if (null == movingMapItem)
                {
                    myButtonSelected = null; // stop dragging
                    myRectangleSelected.Visibility = Visibility.Hidden;
                    MessageBox.Show("ClickedSelectMapItem(): Unable to find " + Utilities.RemoveSpaces(myButtonSelected.Name));
                    return;
                }
                IMapItem clickedMapItem = myGameInstance.MapItems.Find(Utilities.RemoveSpaces(clickedButton.Name)); // Find the corresponding MapItem
                if (null == clickedMapItem)
                {
                    myButtonSelected = null; // stop dragging
                    myRectangleSelected.Visibility = Visibility.Hidden;
                    MessageBox.Show("ClickedSelectMapItem(): Unable to find " + Utilities.RemoveSpaces(myButtonSelected.Name));
                    return;
                }
                ITerritory matchingTerritory = clickedMapItem.Territory;
                if (null == matchingTerritory) // Check for error
                {
                    Logger.Log(LogEnum.LE_ERROR, "ClickedSelectMapItem(): Unable to find matching territory for mi=" + clickedMapItem.Name);
                    matchingTerritory = movingMapItem.TerritoryStarting;
                }
                // Remove the Selected Map Item from its old stack 
                IStack oldStack = myGameInstance.Stacks.Find(movingMapItem.Territory);
                if (null == oldStack)
                {
                    myButtonSelected = null; // stop dragging
                    myRectangleSelected.Visibility = Visibility.Hidden;
                    MessageBox.Show("ClickedSelectMapItem(): Unable to find oldStack=" + movingMapItem.Territory.ToString());
                    return;
                }
                oldStack.MapItems.Remove(movingMapItem);
                // Add to new Stack
                movingMapItem.Territory = matchingTerritory;
                IStack newStack = myGameInstance.Stacks.Find(matchingTerritory);
                if (null == newStack)
                {
                    newStack = new Stack(matchingTerritory) as IStack;
                    myGameInstance.Stacks.Add(newStack);
                }
                newStack.MapItems.Add(movingMapItem);
                int offset = newStack.MapItems.Count - 1;
                Canvas.SetLeft(myButtonSelected, matchingTerritory.CenterPoint.X - Utilities.theMapItemOffset + (offset * 3));
                Canvas.SetTop(myButtonSelected, matchingTerritory.CenterPoint.Y - Utilities.theMapItemOffset + (offset * 3));
                Canvas.SetZIndex(myButtonSelected, offset);
                myButtonSelected = null; // stop dragging
                myRectangleSelected.Visibility = Visibility.Hidden;
                e.Handled = true;
            }
        }
        private void MouseMoveMapItem(object sender, MouseEventArgs e)
        {
            if (null != myButtonSelected)
            {
                System.Windows.Point newPoint = e.GetPosition(myCanvas);
                Canvas.SetTop(myButtonSelected, newPoint.Y + 5);
                Canvas.SetLeft(myButtonSelected, newPoint.X + 5);
                Canvas.SetTop(myRectangleSelected, newPoint.Y + 5);
                Canvas.SetLeft(myRectangleSelected, newPoint.X + 5);
            }
        }
        private void MouseDownCanvas(object sender, MouseButtonEventArgs e)
        {
            if (null == myButtonSelected) // do nothing if nothing is being dragged
                return;
            IMapItem movingMapItem = myGameInstance.MapItems.Find(myButtonSelected.Name); // Find the corresponding MapItem
            if (null == movingMapItem)
            {
                MessageBox.Show("MouseDownCanvas(): Unable to find " + Utilities.RemoveSpaces(myButtonSelected.Name));
                myButtonSelected = null; // stop dragging
                myRectangleSelected.Visibility = Visibility.Hidden;
                return;
            }
            System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
            HitTestResult result = VisualTreeHelper.HitTest(myCanvas, canvasPoint);
            if (null == result)
                return;
            ITerritory matchingTerritory = null; // Find the corresponding Territory
            if (result.VisualHit is Ellipse mousedEllipse)
            {
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
                    Logger.Log(LogEnum.LE_ERROR, "MouseDownCanvas(): Unable to find matching territory for e=" + mousedEllipse.Tag.ToString());
                    matchingTerritory = movingMapItem.TerritoryStarting;
                }
                // Remove the Selected Map Item from its old stack 
                IStack oldStack = myGameInstance.Stacks.Find(movingMapItem.Territory);
                if (null == oldStack)
                {
                    myButtonSelected = null; // stop dragging
                    myRectangleSelected.Visibility = Visibility.Hidden;
                    MessageBox.Show("MouseDownCanvas(): Unable to find oldStack=" + movingMapItem.Territory.ToString());
                    return;
                }
                oldStack.MapItems.Remove(movingMapItem);
                // Add to new Stack
                movingMapItem.Territory = matchingTerritory;
                IStack newStack = myGameInstance.Stacks.Find(matchingTerritory);
                if (null == newStack)
                {
                    newStack = new Stack(matchingTerritory) as IStack;
                    myGameInstance.Stacks.Add(newStack);
                }
                newStack.MapItems.Add(movingMapItem);
                int offset = newStack.MapItems.Count - 1;
                Canvas.SetLeft(myButtonSelected, matchingTerritory.CenterPoint.X - Utilities.theMapItemOffset + (offset * 3));
                Canvas.SetTop(myButtonSelected, matchingTerritory.CenterPoint.Y - Utilities.theMapItemOffset + (offset * 3));
                Canvas.SetZIndex(myButtonSelected, offset);
                myButtonSelected = null; // stop dragging
                myRectangleSelected.Visibility = Visibility.Hidden;
                e.Handled = true;
            }
        }
        private void ClickedSelectMapItem2(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            double x = Canvas.GetLeft(clickedButton);
            double y = Canvas.GetTop(clickedButton);
            System.Windows.Point p = new System.Windows.Point(x, y);
            if (null == myButtonSelected)
            {
                myButtonSelected = clickedButton;
                Canvas.SetZIndex(myButtonSelected, 999);
                myRectangleSelected.BeginAnimation(Canvas.LeftProperty, null);
                myRectangleSelected.BeginAnimation(Canvas.TopProperty, null);
                Canvas.SetLeft(myRectangleSelected, x);
                Canvas.SetTop(myRectangleSelected, y);
                myRectangleSelected.Visibility = Visibility.Visible;             // highlight the moving button with a rectangle
            }
            else
            {
                // Set the destination to the clicked button's territory
                IMapItem selectedMapItem = myGameInstance.MapItems.Find(myButtonSelected.Name); // Find the corresponding MapItem
                if (null == selectedMapItem)
                {
                    Logger.Log(LogEnum.LE_ERROR, "ClickedSelectMapItem2(): selectedMapItem=null");
                    return;
                }
                IMapItem targetedMapItem = myGameInstance.MapItems.Find(clickedButton.Name); // Find the corresponding MapItem
                if (null == targetedMapItem)
                {
                    Logger.Log(LogEnum.LE_ERROR, "ClickedSelectMapItem2(): targetedMapItem=null");
                    return;
                }
                if (false == CreateMapItemMove(myGameInstance.Territories, myGameInstance, targetedMapItem.Territory))
                {
                    Logger.Log(LogEnum.LE_ERROR, "ClickedSelectMapItem2(): CreateMapItemMove() returned false");
                    return;
                }
                IStack stack = myGameInstance.Stacks.Find(myButtonSelected.Name); // after marking movement, rotate the stack
                if (null == stack)
                    Logger.Log(LogEnum.LE_ERROR, "ClickedSelectMapItem2(): stack=null for n=" + myButtonSelected.Name);
                else
                    stack.Rotate();
                myRectangleSelected.Visibility = Visibility.Hidden;
                myButtonSelected = null;
            }
        }
        private void MouseDownPolygon(object sender, MouseButtonEventArgs e)
        {
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
            IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
            Console.WriteLine("MouseDownPolygon(): {0}", mp.ToString());
            Polygon clickedPolygon = (Polygon)sender;
            if (null == clickedPolygon)
            {
                Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygon(): clickedPolygon=null for " + clickedPolygon.Tag.ToString());
                return;
            }
            ITerritory selectedTerritory = TerritoryExtensions.Find(myGameInstance.Territories, Utilities.RemoveSpaces(clickedPolygon.Tag.ToString()));
            if (null == selectedTerritory)
            {
                Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygon(): selectedTerritory=null for " + clickedPolygon.Tag.ToString());
                return;
            }
            if (null != myButtonSelected)
            {
                IMapItem selectedMapItem = myGameInstance.MapItems.Find(myButtonSelected.Name); // Find the corresponding MapItem
                if (null == selectedMapItem)
                {
                    Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygon(): selectedMapItem=null");
                    return;
                }
                if (false == CreateMapItemMove(myGameInstance.Territories, myGameInstance, selectedTerritory))
                {
                    Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygon(): CreateMapItemMove() returned false");
                    return;
                }
                IStack stack = myGameInstance.Stacks.Find(myButtonSelected.Name); // after marking movement, rotate the stack
                if (null == stack)
                    Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygon(): stack=null for n=" + myButtonSelected.Name);
                else
                    stack.Rotate();
                myRectangleSelected.Visibility = Visibility.Hidden;
                myButtonSelected = null;
            }
        }
    }
}
