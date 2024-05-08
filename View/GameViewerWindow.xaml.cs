using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Button = System.Windows.Controls.Button;
using MenuItem = System.Windows.Controls.MenuItem;

namespace BarbarianPrince
{
   public partial class GameViewerWindow : Window, IView
   {
      private const int MAX_DAILY_ACTIONS = 13;
      public bool CtorError { get; } = false;
      //---------------------------------------------------------------------
      private readonly IGameEngine myGameEngine = null;
      private IGameInstance myGameInstance = null;
      //---------------------------------------------------------------------
      private IDieRoller myDieRoller = null;
      private EventViewer myEventViewer = null;
      private Cursor myTargetCursor = null;
      private Dictionary<string, Polyline> myRivers = new Dictionary<string, Polyline>();
      //---------------------------------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushClear = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushGray = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushGreen = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushRed = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushPurple = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushRosyBrown = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushOrange = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushDeepSkyBlue = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      //---------------------------------------------------------------------
      private readonly List<Button> myButtonMapItems = new List<Button>();
      private readonly SplashDialog mySplashScreen = null;
      private Button[] myButtonTimeTrackDays = new Button[7];
      private Button[] myButtonTimeTrackWeeks = new Button[10];
      private Button[] myButtonFoodSupply1s = new Button[10];
      private Button[] myButtonFoodSupply10s = new Button[10];
      private Button[] myButtonFoodSupply100s = new Button[5];
      private Button[] myButtonEndurances = new Button[12];
      private readonly List<Button> myButtonDailyAcions = new List<Button>();
      private readonly string[] myButtonDailyContents = new string[MAX_DAILY_ACTIONS] { "Travel", "Rest", "News", "Hire", "Audience", "Offering", "Search Ruins", "Search Cache", "Search Clue", "Arch Travel", "Follow", "Rafting", "Air Travel" };
      //---------------------------------------------------------------------
      private ContextMenu myContextMenuButton = new ContextMenu();
      private readonly ContextMenu myContextMenuCanvas = new ContextMenu();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private int myBrushIndex = 0;
      private readonly List<Brush> myBrushes = new List<Brush>();
      private readonly List<Rectangle> myRectangles = new List<Rectangle>();
      private readonly List<Polygon> myPolygons = new List<Polygon>();
      private Rectangle myRectangleMoving = null;               // Rentable that is moving with button
      private Rectangle myRectangleSelected = new Rectangle(); // Player has manually selected this button
      private ITerritory myTerritorySelected = null;
      private bool myIsTravelThroughGateActive = false;  // e045
      private bool myIsContentRendered = false;  // Is the initial content rendered - do not resize frames unless already rendered - used during startup
      public GameViewerWindow(IGameEngine ge, IGameInstance gi)
      {
         //-----------------------------------------------------------------
         mySplashScreen = new SplashDialog(); // show splash screen waiting for finish initializing
         mySplashScreen.Show();
         InitializeComponent();
         //-----------------------------------------------------------------
         myGameEngine = ge;
         myGameInstance = gi;
         gi.GamePhase = GamePhase.GameSetup;
         MainMenuViewer mmv = new MainMenuViewer(myMainMenu, ge, gi);
         StatusBarViewer sbv = new StatusBarViewer(myStatusBar, ge, gi, myCanvas);
         //-----------------------------------------------------------------
         Utilities.theBrushBlood.Color = Color.FromArgb(0xFF, 0xA4, 0x07, 0x07);
         Utilities.theBrushRegion.Color = Color.FromArgb(0x7F, 0x11, 0x09, 0xBB); // nearly transparent but slightly colored
         Utilities.theBrushRegionClear.Color = Color.FromArgb(0, 0, 0x01, 0x0); // nearly transparent but slightly colored
         Utilities.theBrushControlButton.Color = Color.FromArgb(0xFF, 0x43, 0x33, 0xFF); // menu blue
         Utilities.theBrushScrollViewerActive.Color = Color.FromArgb(0xFF, 0xB9, 0xEA, 0x9E); // light green 
         Utilities.theBrushScrollViewerInActive.Color = Color.FromArgb(0x17, 0x00, 0x00, 0x00); // gray
         //-----------------------------------------------------------------                                                                          
         mySolidColorBrushClear.Color = Color.FromArgb(0, 0, 1, 0); // Create standard color brushes
         mySolidColorBrushBlack.Color = Colors.Black;
         mySolidColorBrushGray.Color = Colors.Ivory;
         mySolidColorBrushGreen.Color = Colors.Green;
         mySolidColorBrushRed.Color = Colors.Red;
         mySolidColorBrushOrange.Color = Colors.Orange;
         mySolidColorBrushPurple.Color = Colors.Purple;
         mySolidColorBrushRosyBrown.Color = Colors.RosyBrown;
         //---------------------------------------------------------------------
         // Create a container of brushes for painting paths.
         // The first brush is the alien color.
         // The second brush is the townspeople color.
         myBrushes.Add(Brushes.Green);
         myBrushes.Add(Brushes.Blue);
         myBrushes.Add(Brushes.Purple);
         myBrushes.Add(Brushes.Yellow);
         myBrushes.Add(Brushes.Red);
         myBrushes.Add(Brushes.Orange);
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
         //-----------------------------------------------------------------            
         myRectangleSelected.Stroke = Brushes.Red; // Create a Bounding Rectangle to indicate when a MapItem is selected to be moved by mouse pointer
         myRectangleSelected.StrokeThickness = 3.0;
         myRectangleSelected.Width = Utilities.theMapItemSize + 2;
         myRectangleSelected.Height = Utilities.theMapItemSize + 2;
         myRectangleSelected.Visibility = Visibility.Hidden;
         myCanvas.Children.Add(myRectangleSelected);
         Canvas.SetZIndex(myRectangleSelected, 1000);
         //-----------------------------------------------------------------
         LoadEndCallback callback = RemoveSplashScreen;
         myDieRoller = new DieRoller(myCanvas, callback);
         if (true == myDieRoller.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myDieRoller.CtorError=true");
            CtorError = true;
            return;
         }
         //-----------------------------------------------------------------
         myEventViewer = new EventViewer(myGameEngine, myGameInstance, myCanvas, myScrollViewerTextBlock, myStackPanelEndurance, gi.Territories, myDieRoller);
         CanvasImageViewer civ = new CanvasImageViewer(myCanvas);
         CreateRiversFromXml();
         //-----------------------------------------------------------------
         CreateButtonTimeTrack();
         CreateButtonFoodSupply();
         CreateButtonEndurance();
         CreateButtonDailyAction();
         //-----------------------------------------------------------------------
         // Implement the Model View Controller (MVC) pattern by registering views with
         // the game engine such that when the model data is changed, the views are updated.
         ge.RegisterForUpdates(civ);
         ge.RegisterForUpdates(mmv);
         ge.RegisterForUpdates(myEventViewer);
         ge.RegisterForUpdates(sbv);
         ge.RegisterForUpdates(this); // needs to be last so that canvas updates after all actions taken
#if UT2
            if (false == ge.CreateUnitTests(gi, myDockPanelTop, myEventViewer, myDieRoller))
            {
               Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): CreateUnitTests() returned false");
               CtorError = true;
               return;
            }
            gi.GamePhase = GamePhase.UnitTest;
#endif
      }
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if (GameAction.RemoveSplashScreen == action)
            mySplashScreen.Close();
         //-------------------------------------------------------
         if ((null != myTargetCursor) && (GameAction.UpdateStatusBar == action)) // increase/decrease size of cursor when zoom in or out
         {
            myTargetCursor.Dispose();
            double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
            Point hotPoint = new Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Target"), Width = sizeCursor, Height = sizeCursor };
            myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
            this.myCanvas.Cursor = myTargetCursor;
         }
         UpdateStackPanelDailyActions(gi);
         UpdateTimeTrack(gi);
         UpdateFoodSupply(gi);
         UpdatePrinceEndurance(gi);
         switch( action )
         {
            case GameAction.E228ShowTrueLove:
               break;
            case GameAction.ShowDienstalBranch:
               UpdateCanvasRiver("Dienstal Branch");
               if (false == UpdateCanvasHexTravelToShowPolygons(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasHexTravelToShowPolygons() returned error ");
               break;
            case GameAction.ShowLargosRiver:
               UpdateCanvasRiver("Largos River");
               if (false == UpdateCanvasHexTravelToShowPolygons(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasHexTravelToShowPolygons() returned error ");
               break;
            case GameAction.ShowNesserRiver:
               UpdateCanvasRiver("Nesser River");
               UpdateCanvasRiver("Greater Nesser River", false);
               UpdateCanvasRiver("Lesser Nesser River", false);
               if (false == UpdateCanvasHexTravelToShowPolygons(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasHexTravelToShowPolygons() returned error ");
               break;
            case GameAction.ShowTrogothRiver:
               UpdateCanvasRiver("Trogoth River");
               if (false == UpdateCanvasHexTravelToShowPolygons(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasHexTravelToShowPolygons() returned error ");
               break;
            default:
               if( false == UpdateCanvas(gi, action) )
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvas() returned error ");
               break;
         }
      }
      //-----------------------SUPPORTING FUNCTIONS--------------------
      private void RemoveSplashScreen() // callback function that removes splash screen when dice are loaded
      {
         GameAction outAction = GameAction.RemoveSplashScreen;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void CreateButtonTimeTrack()
      {
         myStackPanelTimeTrackDay.Children.Clear();
         Thickness thickness = new Thickness(0, 0, 180, 0);
         Label heading = new Label() { Margin = thickness, Content = "TIME TRACK" };
         myStackPanelTimeTrackDay.Children.Add(heading);
         for (int i = 0; i < 7; ++i)
         {
            int k = i + 1;
            string content = "Day#" + k.ToString();
            myButtonTimeTrackDays[i] = new Button { Height = Utilities.theMapItemSize, Width = Utilities.theMapItemSize, FontSize = 10, IsEnabled = false, Content = content };
            myStackPanelTimeTrackDay.Children.Add(myButtonTimeTrackDays[i]);
         }
         myStackPanelTimeTrackWeek.Children.Clear();
         for (int i = 0; i < 10; ++i)
         {
            int k = i + 1;
            string content = "Week#" + k.ToString();
            myButtonTimeTrackWeeks[i] = new Button { Height = Utilities.theMapItemSize, Width = Utilities.theMapItemSize, FontSize = 8, IsEnabled = false, Content = content };
            myStackPanelTimeTrackWeek.Children.Add(myButtonTimeTrackWeeks[i]);
         }
         // Set the defaults 
         myButtonTimeTrackDays[0].Background = Utilities.theBrushControlButton;
         myButtonTimeTrackWeeks[0].Background = Utilities.theBrushControlButton;
         myButtonTimeTrackDays[0].FontWeight = FontWeights.Bold;
         myButtonTimeTrackWeeks[0].FontWeight = FontWeights.Bold;
         myButtonTimeTrackDays[0].IsEnabled = true;
         myButtonTimeTrackWeeks[0].IsEnabled = true;
      }
      private void CreateButtonFoodSupply()
      {
         myStackPanelFoodSupply100s.Children.Clear();
         Thickness thickness = new Thickness(0, 0, 260, 0);
         Label heading = new Label() { Margin = thickness, Content = "FOOD SUPPLY" };
         myStackPanelFoodSupply100s.Children.Add(heading);
         for (int i = 0; i < 5; ++i)
         {
            int value = i * 100;
            myButtonFoodSupply100s[i] = new Button { Height = Utilities.theMapItemSize, Width = Utilities.theMapItemSize, IsEnabled = false, Content = value.ToString() };
            myStackPanelFoodSupply100s.Children.Add(myButtonFoodSupply100s[i]);
         }
         myStackPanelFoodSupply10s.Children.Clear();
         for (int i = 0; i < 10; ++i)
         {
            int value = i * 10;
            myButtonFoodSupply10s[i] = new Button { Height = Utilities.theMapItemSize, Width = Utilities.theMapItemSize, IsEnabled = false, Content = value.ToString() };
            myStackPanelFoodSupply10s.Children.Add(myButtonFoodSupply10s[i]);
         }
         myStackPanelFoodSupply1s.Children.Clear();
         for (int i = 0; i < 10; ++i)
         {
            myButtonFoodSupply1s[i] = new Button { Height = Utilities.theMapItemSize, Width = Utilities.theMapItemSize, IsEnabled = false, Content = i.ToString() };
            myStackPanelFoodSupply1s.Children.Add(myButtonFoodSupply1s[i]);
         }
         // Set the defaults 
         myButtonFoodSupply1s[0].Background = Utilities.theBrushControlButton;
         myButtonFoodSupply10s[0].Background = Utilities.theBrushControlButton;
         myButtonFoodSupply100s[0].Background = Utilities.theBrushControlButton;
         myButtonFoodSupply1s[0].FontWeight = FontWeights.Bold;
         myButtonFoodSupply10s[0].FontWeight = FontWeights.Bold;
         myButtonFoodSupply100s[0].FontWeight = FontWeights.Bold;
         myButtonFoodSupply1s[0].IsEnabled = true;
         myButtonFoodSupply10s[0].IsEnabled = true;
         myButtonFoodSupply100s[0].IsEnabled = true;
      }
      private void CreateButtonEndurance()
      {
         myStackPanelEndurance.Children.Clear();
         Label labelEndurance = new Label() { FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Content = "Prince ENDURANCE" };
         myStackPanelEndurance.Children.Add(labelEndurance);
         for (int i = 11; -1 < i; --i)
         {
            myButtonEndurances[i] = new Button { Height = Utilities.theMapItemSize, Width = Utilities.theMapItemSize, IsEnabled = false, Content = i.ToString() };
            myStackPanelEndurance.Children.Add(myButtonEndurances[i]);
         }
         myButtonEndurances[0].Content = "dead";
         myButtonEndurances[1].Content = "unc.";
         myButtonEndurances[9].Background = Utilities.theBrushControlButton;
         myButtonEndurances[9].FontWeight = FontWeights.Bold;
         myButtonEndurances[9].IsEnabled = true;
         myButtonEndurances[10].Visibility = Visibility.Hidden; // Can grow to 10 levels based on event in game
         myButtonEndurances[11].Visibility = Visibility.Hidden; // Can grow to 10 levels based on event in game
      }
      private bool CreateButtonMapItem(IMapItem mi, int counterCount)
      {
         string territoryName = Utilities.RemoveSpaces(mi.Territory.ToString());
         ITerritory territory = TerritoryExtensions.Find(myGameInstance.Territories, territoryName);
         if (null == territory)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItem(): TerritoryExtensions.Find() returned null");
            return false;
         }
         System.Windows.Controls.Button b = new Button { ContextMenu = myContextMenuButton, Name = Utilities.RemoveSpaces(mi.Name), Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         Canvas.SetLeft(b, territory.CenterPoint.X - mi.Zoom * Utilities.theMapItemOffset + (counterCount * Utilities.STACK));
         Canvas.SetTop(b, territory.CenterPoint.Y - mi.Zoom * Utilities.theMapItemOffset + (counterCount * Utilities.STACK));
         MapItem.SetButtonContent(b, mi, false, false); // This sets the image as the button's content
         myButtonMapItems.Add(b);
         myCanvas.Children.Add(b);
         Canvas.SetZIndex(b, counterCount);
         b.Click += ClickButtonMapItem;
         return true;
      }
      private bool CreateButtonDailyAction()
      {
         Button b = null;
         for (int i = 0; i < MAX_DAILY_ACTIONS; ++i)
         {
            b = new Button { Height = 42, Width = 77, Content = myButtonDailyContents[i] };
            b.Click += ClickButtonDailyAction;
            myButtonDailyAcions.Add(b);
         }
         return true;
      }
      private void CreateRiversFromXml()
      {
         XmlTextReader reader = null;
         PointCollection points = null;
         string name = null;
         try
         {
            reader = new XmlTextReader("../../Config/Rivers.xml") { WhitespaceHandling = WhitespaceHandling.None }; // Load the reader with the data file and ignore all white space nodes.    
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
                           points.Add(new Point(X1, Y1));
                        }
                        else
                        {
                           break;
                        }
                     }  // end while
                  } // end if
                  Polyline polyline = new Polyline { Points = points, Stroke = mySolidColorBrushDeepSkyBlue, StrokeThickness = 2, Visibility = Visibility.Visible };
                  myRivers[name] = polyline;
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
      //---------------------------------------
      private void UpdateTimeTrack(IGameInstance gi)
      {
         for (int i = 0; i < 7; ++i)
         {
            myButtonTimeTrackDays[i].ClearValue(Control.BackgroundProperty);
            myButtonTimeTrackDays[i].FontWeight = FontWeights.Normal;
            myButtonTimeTrackDays[i].IsEnabled = false;
         }
         for (int j = 0; j < 10; ++j)
         {
            myButtonTimeTrackWeeks[j].ClearValue(Control.BackgroundProperty);
            myButtonTimeTrackWeeks[j].FontWeight = FontWeights.Normal;
            myButtonTimeTrackWeeks[j].IsEnabled = false;
         }
         if (gi.Days < 0)
            return;
         int week = gi.Days / 7; // round down to nearest integer
         if (9 < week)
            week = 9;
         myButtonTimeTrackWeeks[week].Background = Utilities.theBrushControlButton;
         myButtonTimeTrackWeeks[week].FontWeight = FontWeights.Bold;
         myButtonTimeTrackWeeks[week].IsEnabled = true;

         int day = gi.Days - week * 7;
         if (6 < day)
            day = 6;
         myButtonTimeTrackDays[day].Background = Utilities.theBrushControlButton;
         myButtonTimeTrackDays[day].FontWeight = FontWeights.Bold;
         myButtonTimeTrackDays[day].IsEnabled = true;
      }
      private void UpdateFoodSupply(IGameInstance gi)
      {
         int foodSum = 0;
         foreach (IMapItem mi in gi.PartyMembers)
            foodSum += mi.Food;
         for (int i = 0; i < 10; ++i)
         {
            myButtonFoodSupply1s[i].ClearValue(Control.BackgroundProperty);
            myButtonFoodSupply1s[i].FontWeight = FontWeights.Normal;
            myButtonFoodSupply1s[i].IsEnabled = false;
            myButtonFoodSupply10s[i].ClearValue(Control.BackgroundProperty);
            myButtonFoodSupply10s[i].FontWeight = FontWeights.Normal;
            myButtonFoodSupply10s[i].IsEnabled = false;
         }
         for (int j = 0; j < 5; ++j)
         {
            myButtonFoodSupply100s[j].ClearValue(Control.BackgroundProperty);
            myButtonFoodSupply100s[j].FontWeight = FontWeights.Normal;
            myButtonFoodSupply100s[j].IsEnabled = false;
         }
         int remainder100s = foodSum % 100;
         int hundreds = foodSum - remainder100s;
         int k = hundreds / 100;
         if (k < 0 || 4 < k)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateFoodSupply() 4 < k=" + k.ToString());
            return;
         }
         myButtonFoodSupply100s[k].Background = Utilities.theBrushControlButton;
         myButtonFoodSupply100s[k].FontWeight = FontWeights.Bold;
         myButtonFoodSupply100s[k].IsEnabled = true;
         foodSum -= hundreds;
         int remainder10s = remainder100s % 10;
         int tens = foodSum - remainder10s;
         k = tens / 10;
         if (k < 0 || 9 < k)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateFoodSupply() 1 -> 9 < k=" + k.ToString());
            return;
         }
         myButtonFoodSupply10s[k].Background = Utilities.theBrushControlButton;
         myButtonFoodSupply10s[k].FontWeight = FontWeights.Bold;
         myButtonFoodSupply10s[k].IsEnabled = true;
         foodSum -= tens;
         k = foodSum;
         if (k < 0 || 9 < k)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateFoodSupply() 2 -> 9 < k=" + k.ToString());
            return;
         }
         myButtonFoodSupply1s[k].Background = Utilities.theBrushControlButton;
         myButtonFoodSupply1s[k].FontWeight = FontWeights.Bold;
         myButtonFoodSupply1s[k].IsEnabled = true;
      }
      private void UpdatePrinceEndurance(IGameInstance gi)
      {
         for (int i = 0; i < 12; ++i)
         {
            myButtonEndurances[i].ClearValue(Control.BackgroundProperty);
            myButtonEndurances[i].FontWeight = FontWeights.Normal;
            myButtonEndurances[i].IsEnabled = false;
            myButtonEndurances[i].Visibility = Visibility.Visible;
         }
         if (8 == gi.Prince.Endurance) // Sash can increase endurance as well as special event
         {
            myButtonEndurances[9].Visibility = Visibility.Hidden;
            myButtonEndurances[10].Visibility = Visibility.Hidden;
            myButtonEndurances[11].Visibility = Visibility.Hidden;
         }
         else if (9 == gi.Prince.Endurance) // Sash can increase endurance as well as special event
         {
            myButtonEndurances[10].Visibility = Visibility.Hidden;
            myButtonEndurances[11].Visibility = Visibility.Hidden;
         }
         else if (10 == gi.Prince.Endurance) // Sash can increase endurance as well as special event
         {
            myButtonEndurances[11].Visibility = Visibility.Hidden;
         }
         int healthRemaining = gi.Prince.Endurance - gi.Prince.Wound - gi.Prince.Poison;
         if ((healthRemaining < 0) || (11 < healthRemaining))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdatePrinceEndurance(): healthRemaining=" + healthRemaining.ToString());
            return;
         }
         myButtonEndurances[healthRemaining].Background = Utilities.theBrushControlButton;
         myButtonEndurances[healthRemaining].FontWeight = FontWeights.Bold;
         myButtonEndurances[healthRemaining].IsEnabled = true;
      }
      private void UpdateStackPanelDailyActions(IGameInstance gi)
      {
         if (GamePhase.SunriseChoice == gi.GamePhase)
         {
            for (int i = 0; i < MAX_DAILY_ACTIONS; ++i)
            {
               myButtonDailyAcions[i].ClearValue(Control.BackgroundProperty);
               myButtonDailyAcions[i].IsEnabled = true;
            }
            myStackPanelDailyActions.Children.Clear();
            Label labelDailyActions = new Label() { FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "DAILY ACTIONS" };
            myStackPanelDailyActions.Children.Add(labelDailyActions);
            //------------------------------------------------------------------
            if ((0 <  gi.Prince.Mounts.Count) && (false == gi.IsHeavyRainDismount) ) // if choose to dismount due to heavy rains, do not fly
            {
               IMapItem mount = gi.Prince.Mounts[0];
               if( (0 == mount.StarveDayNum ) && ( false == mount.IsExhausted ) ) // mount cannot fly if starving or exhausted
               {
                  if ( true == mount.IsFlyingMount() )
                  {
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[12]);    // air travel
                     myStackPanelDailyActions.Visibility = Visibility.Visible;
                  }
               }
            }
            if (RaftEnum.RE_RAFT_SHOWN == gi.RaftState) 
            {
               myStackPanelDailyActions.Children.Add(myButtonDailyAcions[11]);    // travel by raft
               myStackPanelDailyActions.Visibility = Visibility.Visible;
            }
            if (false == gi.IsMustLeaveHex)
            {
               ITerritory t = gi.Prince.Territory;
               bool isInTownOrCastle = myGameInstance.IsInTownOrCastle(t);
               bool isInTemple = myGameInstance.IsInTemple(t);
               bool isInRuins = myGameInstance.IsInRuins(t);
               if ((false == gi.IsTrainHorse) && (false == gi.IsWoundedWarriorRest) && (false == gi.IsWoundedBlackKnightRest))
               {
                  if ((0 < gi.NumMembersBeingFollowed) && (false == gi.IsFloodContinue))
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[10]);
                  if (null != gi.Arches.Find(t.Name))
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[9]);
                  if (null != gi.SecretClues.Find(t.Name))
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[8]);
                  if (null != gi.WizardAdviceLocations.Find(t.Name))
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[8]);
                  if (null != gi.Caches.Find(t.Name))
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[7]);
                  if (true == isInRuins)
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[6]);
                  if ((true == isInTemple) && (1 < myGameInstance.GetCoins()))
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[5]);   // offering
                  if (((true == isInTownOrCastle) || (true == isInTemple)) && (false == gi.ForbiddenAudiences.Contains(gi.Prince.Territory))) // audience
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[4]);
                  if ((true == isInTownOrCastle) && (false == myGameInstance.ForbiddenHires.Contains(gi.Prince.Territory))) // hire
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[3]);
                  if ( ((true == isInTownOrCastle) || (true == isInTemple)) && (false == gi.HiddenTowns.Contains(t)) ) // news - hidden towns do not due news
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[2]);
               }
               //-------------------------------------------------------------------------
               if ((false == gi.IsExhausted) ||  (true == t.IsOasis) || ("Desert" != t.Type)) // e120 - cannot rest if exhausted in desert without oasis
               {
                  if ((true == isInTownOrCastle) || (true == isInTemple))          // heal 
                     myButtonDailyAcions[1].Content = "Rest & Lodge";
                  else if (true == gi.IsTrainHorse)
                     myButtonDailyAcions[1].Content = "Rest & Train";
                  else
                     myButtonDailyAcions[1].Content = myButtonDailyContents[1];
                  myStackPanelDailyActions.Children.Add(myButtonDailyAcions[1]);
               }
            }
            if ((false == gi.IsTrainHorse) && ( false == gi.IsFloodContinue ) && (false == gi.IsWoundedWarriorRest)&& (false == gi.IsWoundedBlackKnightRest)) // cannot travel if training horse, in flood, or waiting for warrior to rest
            {
               myStackPanelDailyActions.Children.Add(myButtonDailyAcions[0]);    // travel
               myStackPanelDailyActions.Visibility = Visibility.Visible;
            }
            Logger.Log(LogEnum.LE_VIEW_UPDATE_DAILY_ACTIONS, "UpdateStackPanelDailyActions(): Update Daily Stack Panel Actions");
         }
      }
      private void UpdateCanvasRiver(string river, bool isClearExistingRiver = true)
      {
         //------------------------------------
         if (true == isClearExistingRiver)
         {
            List<UIElement> elements = new List<UIElement>();
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Polygon polygon)
                  elements.Add(ui);
               if (ui is Polyline polyline)
                  elements.Add(ui);
            }
            foreach (UIElement ui1 in elements)
               myCanvas.Children.Remove(ui1);
         }
         //------------------------------------
         try
         {
            Polyline polyline = myRivers[river];
            Canvas.SetZIndex(polyline, 0);
            myCanvas.Children.Add(polyline); // add polyline to canvas
            const double SIZE = 6.0;
            int i = 0;
            double X1 = 0.0;
            double Y1 = 0.0;
            foreach (Point p in polyline.Points)
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
                  Point one = new Point(Xcenter - SIZE, Ycenter - SIZE);
                  Point two = new Point(Xcenter + SIZE, Ycenter);
                  Point three = new Point(Xcenter - SIZE, Ycenter + SIZE);
                  points.Add(one);
                  points.Add(two);
                  points.Add(three);
                  //---------------------------------------
                  Polygon triangle = new Polygon() { Name = "River", Points = points, Stroke = mySolidColorBrushDeepSkyBlue, Fill = mySolidColorBrushDeepSkyBlue, Visibility = Visibility.Visible };
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
            Console.WriteLine("UpdateCanvasRiver(): unknown river=" + river + " EXCEPTION THROWN e={0}", e.ToString());
         }
      }
      //---------------------------------------
      private bool UpdateCanvas(IGameInstance gi, GameAction action, bool isOnlyLastLegRemoved = false)
      {
         //-------------------------------------------------------
         // Clean the Canvas of all marks
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Polyline polyline)
               elements.Add(ui);
            if (ui is Image img)
            {
               if ("myMap" == img.Name)
                  continue;
               elements.Add(ui);
            }
         }
         foreach (UIElement ui1 in elements)
            myCanvas.Children.Remove(ui1);
         //-------------------------------------------------------
         foreach (ITerritory t in gi.HiddenTemples)
         {
            double size = 1.5 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TempleIcon"), Width = size, Height = 1.5 * size };
            Canvas.SetLeft(img1, t.CenterPoint.X - Utilities.theMapItemOffset / 1.1);
            Canvas.SetTop(img1, t.CenterPoint.Y - Utilities.theMapItemOffset / 1.3);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.DwarfAdviceLocations)
         { 
            double size = 1.3 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DwarfAdvice"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 2);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 2);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.WizardAdviceLocations)
         {
            double size = 1.3 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("WizardAdvice"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 2);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 2);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.SecretClues)
         {
            double size = Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Secrets"), Width = 1.2 * size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 1.1);
            Canvas.SetTop(img1, t.CenterPoint.Y + size / 4);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.EagleLairs)
         {
            double size = Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("EagleNest"), Width = size, Height = 1.25 * size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 1.3);
            Canvas.SetTop(img1, t.CenterPoint.Y - 0.75 * size);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.DwarvenMines)
         {
            double size = 0.9*Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DwarfMines"), Width = 2.0*size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 2);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.ElfTowns)
         {
            double size = 1.5 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElvenTown"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 2);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 2);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.HalflingTowns)
         {
            double size = 1.5 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("HalflingTown"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 3);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 3 );
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.ElfCastles)
         {
            double size = 1.5 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElvenCastle"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 2);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 2);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.HiddenRuins)
         {
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Ruins"), Width = Utilities.theMapItemOffset, Height = 1.5 * Utilities.theMapItemOffset };
            Canvas.SetLeft(img1, t.CenterPoint.X - Utilities.theMapItemOffset / 1.1);
            Canvas.SetTop(img1, t.CenterPoint.Y - Utilities.theMapItemOffset / 1.3);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.HiddenTowns)
         {
            double size = 1.5 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("HiddenTown"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 2);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 2);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.Arches)
         {
            Image img1 = new Image { Tag = "Arch", Source = MapItem.theMapImages.GetBitmapImage("Arch"), Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
            Canvas.SetLeft(img1, t.CenterPoint.X + 0.2 * Utilities.theMapItemOffset);
            Canvas.SetTop(img1, t.CenterPoint.Y - 0.25 * Utilities.theMapItemOffset);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.GoblinKeeps)
         {
            Image img1 = new Image { Tag = "Keep", Source = MapItem.theMapImages.GetBitmapImage("TowerGoblin"), Width = 0.75 * Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
            Canvas.SetLeft(img1, t.CenterPoint.X - 0.375 * Utilities.theMapItemOffset);
            Canvas.SetTop(img1, t.CenterPoint.Y);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.WizardTowers)  // e068
         {
            double size = 1.5 * Utilities.theMapItemOffset;
            Image img1 = new Image { Tag = "WizardTower", Source = MapItem.theMapImages.GetBitmapImage("TowerWizard"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - 0.8 * size);
            Canvas.SetTop(img1, t.CenterPoint.Y - 0.5 * size);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         if (true == gi.IsSecretBaronHuldra) // e144
         {
            ITerritory t = gi.Territories.Find("1611");
            double size = Utilities.ZOOM * Utilities.theMapItemSize;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("WarriorsHillTribe"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 1.7);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 2);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         if (true == gi.IsSecretCountDrogat) // e144
         {
            double size = Utilities.theMapItemSize;
            ITerritory t = gi.Territories.Find("2018");
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("FoulBane"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 1.7);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 2.0);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.ForbiddenHexes)
         {
            double size = 0.8 * Utilities.theMapItemSize;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Deny"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 2);
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 2);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.AbandonedTemples)
         {
            double size = 0.85 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DenyTemple"), Width = size, Height = size };
            Canvas.SetLeft(img1, t.CenterPoint.X - size / 2);
            Canvas.SetTop(img1, t.CenterPoint.Y - 1.3 * size);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.ForbiddenHires)
         {
            if (false == gi.AbandonedTemples.Contains(t))
            {
               double size = 0.85 * Utilities.theMapItemOffset;
               Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DenyHire"), Width = size, Height = size };
               Canvas.SetLeft(img1, t.CenterPoint.X - size / 2);
               Canvas.SetTop(img1, t.CenterPoint.Y + size / 4);
               myCanvas.Children.Add(img1);
            }
         }
         //-------------------------------------------------------
         foreach (IForbiddenAudience fa in gi.ForbiddenAudiences)
         {
            if (false == gi.AbandonedTemples.Contains(fa.ForbiddenTerritory))
            {
               double size = 0.85 * Utilities.theMapItemOffset;
               Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DenyAudience"), Width = size, Height = size };
               Canvas.SetLeft(img1, fa.ForbiddenTerritory.CenterPoint.X - size / 2);
               Canvas.SetTop(img1, fa.ForbiddenTerritory.CenterPoint.Y - 1.3 * size);
               myCanvas.Children.Add(img1);
            }
         }
         //-------------------------------------------------------
         foreach (ITerritory t in gi.LetterOfRecommendations)
         {
            double size = 0.85 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Letter"), Width = size, Height = 0.75 * size };
            Canvas.SetLeft(img1, t.CenterPoint.X);
            Canvas.SetTop(img1, t.CenterPoint.Y - 0.75 * size);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         foreach (ICache c in gi.Caches)
         {
            ITerritory t = c.TargetTerritory;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Cache"), Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
            Canvas.SetLeft(img1, t.CenterPoint.X - Utilities.theMapItemOffset / 2);
            Canvas.SetTop(img1, t.CenterPoint.Y);
            myCanvas.Children.Add(img1);
         }
         //-------------------------------------------------------
         if (GamePhase.UnitTest == gi.GamePhase)
            return true;
         //-------------------------------------------------------
         // Add newly added party members to stack
         foreach (IMapItem partyMember in myGameInstance.PartyMembers)
         {
            if ("offboard" == partyMember.TerritoryStarting.Name)
               continue;
            if ("Prince" != partyMember.Name) // only show prince
               continue;
            IStack stack = myGameInstance.Stacks.Find(partyMember);
            if (null == stack)
            {
               stack = myGameInstance.Stacks.Find(partyMember.Territory);
               if (null == stack)
               {
                  stack = new Stack(partyMember.Territory) as IStack;
                  myGameInstance.Stacks.Add(stack);
               }
               stack.MapItems.Add(partyMember);
            }
         }
         //-------------------------------------------------------
         Button b = myButtonMapItems.Find(Utilities.RemoveSpaces(gi.Prince.Name));
         if (null != b)
         {
            b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
            b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
            gi.Prince.SetLocation(0);
            Canvas.SetLeft(b, gi.Prince.Location.X);
            Canvas.SetTop(b, gi.Prince.Location.Y);
            Canvas.SetZIndex(b, 0);
         }
         else
         {
            if (false == CreateButtonMapItem(gi.Prince, 0))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas(): CreateButton() returned false");
               return false;
            }
         }
         //-------------------------------------------------------
         if (null != gi.ActiveHex)
         {
            UpdateCanvasHexToShowPolygon(gi.ActiveHex);
            gi.ActiveHex = null;
         }
         //-------------------------------------------------------
         try
         {
            switch (action)
            {
               case GameAction.EndGameLost:
               case GameAction.EndGameWin:
                  StringBuilder sbEnd = new StringBuilder();
                  sbEnd.Append("Game ends on Day#");
                  ++myGameInstance.Days;
                  sbEnd.Append(myGameInstance.Days.ToString());
                  sbEnd.Append(" due to '");
                  sbEnd.Append(myGameInstance.EndGameReason);
                  sbEnd.Append("' in ");
                  sbEnd.Append(myGameInstance.Prince.Territory.Name);
                  MessageBox.Show(sbEnd.ToString());
                  GameAction outAction = GameAction.EndGameExit;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
                  break;
               case GameAction.TravelLostCheck:
                  if (0 == gi.MapItemMoves.Count)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas(): invalid gi.MapItemMoves.Count=0 state gi.SunriseChoice=" + myGameInstance.SunriseChoice.ToString() + " a=" + action.ToString());
                     return false;
                  }
                  if (RiverCrossEnum.TC_CROSS_YES == gi.MapItemMoves[0].RiverCross) // if river is crossed, the lost check is done in the new hex
                  {
                     if (false == UpdateCanvasMovement(gi, action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  UpdateCanvasMovement() returned false when gi.SunriseChoice=" + myGameInstance.SunriseChoice.ToString() + " a=" + action.ToString());
                        return false;
                     }
                     gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_YES_SHOWN;
                  }
                  if (null != myTerritorySelected)
                  {
                     if( ("e213a" == myGameInstance.EventActive ) || ("e401" == myGameInstance.EventActive) )
                        UpdateCanvasHexToShowPolygon(myGameInstance.NewHex); // e126 - if raft moved downriver, show the polygon in the nex hex
                     else
                        UpdateCanvasHexToShowPolygon(myTerritorySelected);
                  }
                  break;
               case GameAction.TravelEndMovement:
               case GameAction.TravelShowLostEncounter:
                  foreach (Polygon p in myPolygons)
                     p.MouseDown -= MouseDownPolygonTravel;
                  myPolygons.Clear();
                  myTerritorySelected = null;
                  myRectangleSelected.Visibility = Visibility.Hidden;
                  break;
               case GameAction.E045ArchOfTravel:
                  UpdateTimeTrack(gi);
                  myIsTravelThroughGateActive = true;
                  if (false == UpdateCanvasHexArchTravel(gi))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  UpdateCanvasHexArchTravel() returned false");
                     return false;
                  }
                  break;
               case GameAction.E045ArchOfTravelEnd:
                  foreach (Polygon p in myPolygons)
                     p.MouseDown -= MouseDownPolygonArchOfTravel;
                  myPolygons.Clear();
                  break;
               case GameAction.E156MayorTerritorySelection:
                  if (false == UpdateCanvasHexLetterChoice(gi))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  UpdateCanvasHexLetterChoice() returned false");
                     return false;
                  }
                  break;
               case GameAction.E156MayorTerritorySelectionEnd:
                  foreach (Polygon p in myPolygons)
                     p.MouseDown -= MouseDownPolygonLetterChoice;
                  myPolygons.Clear();
                  break;
               case GameAction.E209ShowSecretRites:
                  foreach (ITerritory t in gi.SecretRites)
                  {
                     PointCollection points = new PointCollection();
                     foreach (IMapPoint mp1 in t.Points)
                        points.Add(new Point(mp1.X, mp1.Y));
                     Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Tag = t.ToString() };
                     myCanvas.Children.Add(aPolygon);
                  }
                  break;
               case GameAction.E130JailedOnTravels:
                  if (false == MoveToNewHex(gi))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  MoveToNewHex() returned false");
                     return false;
                  }
                  break;
               case GameAction.TravelShowMovementEncounter:
                  if (0 < gi.MapItemMoves.Count)
                  {
                     if (false == UpdateCanvasMovement(gi, action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  UpdateCanvasMovement() returned false");
                        return false;
                     }
                  }
                  myRectangleSelected.Visibility = Visibility.Hidden;
                  break;
               case GameAction.E035IdiotStartDay:
               case GameAction.E035IdiotContinue:
               case GameAction.E054EscapeKeep:
               case GameAction.E106OvercastLost:
               case GameAction.E129EscapeGuards:
               case GameAction.E126RaftInCurrentRedistribute:
               case GameAction.E126RaftInCurrentEnd:
               case GameAction.E203EscapeFromPrison:
               case GameAction.E203EscapeFromDungeon:
               case GameAction.E203NightEnslaved:
               case GameAction.E203EscapeEnslaved:
               case GameAction.CampfirePlagueDust:
               case GameAction.EncounterLootStart:
               case GameAction.EncounterFollow:
               case GameAction.EncounterSurrender:
               case GameAction.Hunt:
               case GameAction.CampfireLoadTransport:
               case GameAction.CampfireTrueLoveCheck:
               case GameAction.CampfireLodgingCheck:
               //case GameAction.CampfireStarvationCheck: // causing runtime error b/c button was changing while it was moving
               case GameAction.TravelShowLost:
                  if (0 < gi.MapItemMoves.Count)
                  {
                     if (false == UpdateCanvasMovement(gi, action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  UpdateCanvasMovement() returned false");
                        return false;
                     }
                  }
                  myRectangleSelected.Visibility = Visibility.Hidden;
                  break;
               //-------------------------------------------
               case GameAction.E079HeavyRainsContinueTravel:
               case GameAction.E110AirSpiritConfusedEnd:
               case GameAction.E110AirSpiritTravelEnd:
               case GameAction.Travel:
                  if (0 < gi.MapItemMoves.Count)
                  {
                     if (false == UpdateCanvasMovement(gi, action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  UpdateCanvasMovement() returned false");
                        return false;
                     }
                  }
                  else
                  {
                     if (false == UpdateMapItemRectangle(gi))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  UpdateMapItemRectangle() returned false");
                        return false;
                     }
                  }
                  if (gi.Prince.MovementUsed < gi.Prince.Movement)
                  {
                     if (false == UpdateCanvasHexTravelToShowPolygons(gi))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  UpdateCanvasHexTravelToShowPolygons() returned false");
                        return false;
                     }
                  }
                  else
                  {
                     myRectangleSelected.Visibility = Visibility.Hidden;
                  }
                  break;
               case GameAction.E110AirSpiritTravel:
                  if (false == UpdateCanvasHexTravelToShowPolygons(gi))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  UpdateCanvasHexTravelToShowPolygons() returned false");
                     return false;
                  }
                  break;
               default:
                  break;
            }
         }
         catch (Exception e)
         {
            Console.WriteLine("UpdateCanvas() - EXCEPTION THROWN a=" + action.ToString() + "\ne={0}", e.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateCanvasMovement(IGameInstance gi, GameAction action)
      {
         try
         {
            Logger.Log(LogEnum.LE_VIEW_MIM, "UpdateCanvasMovement(): ------------------------------------------");
            if( 0 < gi.MapItemMoves.Count)
            {
               IMapItemMove mim2 = gi.MapItemMoves[0];
               IMapItem mi = mim2.MapItem;
               if (false == MovePathAnimate(mim2))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): MovePathAnimate() returned false t=" + mim2.OldTerritory.ToString());
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "UpdateCanvasMovement(): MovePathAnimate() returned false gi.MapItemMoves.Clear()");
                  gi.MapItemMoves.Clear();
                  return false;
               }
               mi.Territory = mim2.NewTerritory;
               mi.TerritoryStarting = mim2.NewTerritory;
               Logger.Log(LogEnum.LE_VIEW_MIM, "UpdateCanvasMovement(): mim=" + mi.Name + " moveto t=" + mi.Territory.Name);
            }
         }
         catch (Exception e)
         {
            Console.WriteLine("UpdateCanvasMovement() - EXCEPTION THROWN e={0}", e.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateMapItemRectangle(IGameInstance gi)
      {
         if (null == myRectangleSelected)
         {
            Console.WriteLine("UpdateMapItemRectangle(): myRectangleSelection=null");
            return false;
         }
         //------------------------------------------------------
         Button b = myButtonMapItems.Find(gi.Prince.Name); // Put a rectangle around the prince when selected to move
         if (null == b)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateMapItemRectangle(): unable to find button for prince");
            return false;
         }
         double x = Canvas.GetLeft(b) - 1;
         double y = Canvas.GetTop(b) - 1;
         Canvas.SetZIndex(b, 999);
         myRectangleSelected.BeginAnimation(Canvas.LeftProperty, null);
         myRectangleSelected.BeginAnimation(Canvas.TopProperty, null);
         Canvas.SetLeft(myRectangleSelected, x);
         Canvas.SetTop(myRectangleSelected, y);
         myRectangleSelected.Visibility = Visibility.Visible;
         return true;
      }
      private void UpdateCanvasHexToShowPolygon(ITerritory t)
      {
         PointCollection points = new PointCollection();
         foreach (IMapPoint mp1 in t.Points)
            points.Add(new Point(mp1.X, mp1.Y));
         Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Tag = t.ToString() };
         myPolygons.Add(aPolygon);
         myCanvas.Children.Add(aPolygon);
      }
      private bool UpdateCanvasHexTravelToShowPolygons(IGameInstance gi)
      {
         //------------------------------------------------------
         foreach (Polygon p in myPolygons)
            p.MouseDown -= MouseDownPolygonTravel;
         myPolygons.Clear();
         //------------------------------------------------------
         myTerritorySelected = null;
         if (true == gi.IsImpassable) // e089 - must return to hex entered
         {
            int tCount = gi.EnteredTerritories.Count;
            if (tCount < 2)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasHexTravelToShowPolygons(): Invalid state with tCount=" + tCount.ToString());
               return false;
            }
            ITerritory t = gi.EnteredTerritories[tCount - 2];
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Tag = t.ToString() };
            aPolygon.MouseDown += MouseDownPolygonTravel;
            myPolygons.Add(aPolygon);
            myCanvas.Children.Add(aPolygon);
            return true;
         }
         List<String> sTerritories = null;
         if (RaftEnum.RE_RAFT_CHOSEN == myGameInstance.RaftState)
            sTerritories = gi.Prince.Territory.Rafts;
         else if ("e110c" == myGameInstance.EventActive)
            sTerritories = myGameInstance.AirSpiritLocations;
         else
            sTerritories = gi.Prince.Territory.Adjacents;
         foreach (string s in sTerritories)
         {
            ITerritory t = gi.Territories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasHexTravelToShowPolygons(): 1 t=null for " + s);
               return false;
            }
            if (true == gi.ForbiddenHexes.Contains(t))
               continue;
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Tag = t.ToString() };
            aPolygon.MouseDown += MouseDownPolygonTravel;
            myPolygons.Add(aPolygon);
            myCanvas.Children.Add(aPolygon);
         }
         return true;
      }
      private bool UpdateCanvasHexArchTravel(IGameInstance gi)
      {
         myPolygons.Clear();
         foreach (ITerritory t in gi.Territories)
         {
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegionClear, Points = points, Tag = t.ToString() };
            aPolygon.MouseDown += MouseDownPolygonArchOfTravel;
            myPolygons.Add(aPolygon);
            myCanvas.Children.Add(aPolygon);
         }
         return true;
      }
      private bool UpdateCanvasHexLetterChoice(IGameInstance gi)
      {
         myPolygons.Clear();
         foreach (ITerritory t in gi.Territories)
         {
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegionClear, Points = points, Tag = t.ToString() };
            aPolygon.MouseDown += MouseDownPolygonLetterChoice;
            myPolygons.Add(aPolygon);
            myCanvas.Children.Add(aPolygon);
         }
         return true;
      }
      private bool MoveToNewHex(IGameInstance gi)
      {
         ITerritory oldT = gi.Prince.Territory;
         IStack oldStack = gi.Stacks.Find(oldT);
         if (null == oldStack)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonArchOfTravel(): oldStack=null for t=" + oldT.ToString());
            return false;
         }
         if (null == gi.NewHex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonArchOfTravel(): gi.NewHex=null" );
            return false;
         }
         IStack newStack = myGameInstance.Stacks.Find(gi.NewHex);
         if (null == newStack)
         {
            newStack = new Stack(gi.NewHex);
            myGameInstance.Stacks.Add(newStack);
         }
         //-------------------------------------
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            mi.Territory = gi.NewHex;
            newStack.MapItems.Add(mi);
            oldStack.MapItems.Remove(mi);
         }
         if (0 == oldStack.MapItems.Count)
            myGameInstance.Stacks.Remove(oldStack);
         return true;
      }
      private bool MovePathAnimate(IMapItemMove mim)
      {
         if ("Prince" != mim.MapItem.Name) // only prince is moved on map
            return true;
         const int ANIMATE_TIME_SEC = 2;
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for n=" + mim.MapItem.Name);
            return false;
         }
         Button b = myButtonMapItems.Find(Utilities.RemoveSpaces(mim.MapItem.Name));
         if (null == b)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for n=" + mim.MapItem.Name);
            return false;
         }
         try
         {
            Canvas.SetZIndex(b, 100); // Move the button to the top of the Canvas
            double xStart = mim.MapItem.Location.X;
            double yStart = mim.MapItem.Location.Y;
            PathFigure aPathFigure = new PathFigure() { StartPoint = new Point(xStart, yStart) };
            int lastItemIndex = mim.BestPath.Territories.Count - 1;
            for (int i = 0; i < lastItemIndex; i++) // add intermediate movement points - not really used in Barbarian Prince as only move one hex at a time
            {
               ITerritory t = mim.BestPath.Territories[i];
               double x = t.CenterPoint.X - Utilities.theMapItemOffset;
               double y = t.CenterPoint.Y - Utilities.theMapItemOffset;
               Point newPoint = new Point(x , y);
               LineSegment lineSegment = new LineSegment(newPoint, false);
               aPathFigure.Segments.Add(lineSegment);
            }
            // Add the last line segment
            double xEnd = mim.NewTerritory.CenterPoint.X - Utilities.theMapItemOffset;
            double yEnd = mim.NewTerritory.CenterPoint.Y - Utilities.theMapItemOffset;
            if ( (Math.Abs(xEnd - xStart) < 2) && (Math.Abs(yEnd - yStart) < 2) ) // if already at final location, skip animation or get runtime exception
               return true;
            Point newPoint2 = new Point(xEnd, yEnd);
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
            myRectangleSelected.RenderTransform = new TranslateTransform();
            myRectangleSelected.BeginAnimation(Canvas.LeftProperty, xAnimiation);
            myRectangleSelected.BeginAnimation(Canvas.TopProperty, yAnimiation);
            return true;
         }
         catch (Exception e)
         {
            b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
            b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
            Console.WriteLine("MovePathAnimate() - EXCEPTION THROWN e={0}", e.ToString());
            return false;
         }
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
         Point newPoint = new Point(xPostion, yPostion);
         aPointCollection.Add(newPoint);
         foreach (ITerritory t in mim.BestPath.Territories)
         {
            xPostion = t.CenterPoint.X + offset;
            yPostion = t.CenterPoint.Y + offset;
            newPoint = new Point(xPostion, yPostion);
            aPointCollection.Add(newPoint);
         }
         //-----------------------------------------
         Polyline aPolyline = new Polyline();
         aPolyline.Stroke = myBrushes[myBrushIndex];
         aPolyline.StrokeThickness = 3;
         aPolyline.StrokeEndLineCap = PenLineCap.Triangle;
         aPolyline.Points = aPointCollection;
         aPolyline.StrokeDashArray = myDashArray;
         myCanvas.Children.Add(aPolyline);
         //-----------------------------------------
         myRectangleMoving = myRectangles[myBrushIndex];
         if (myRectangles.Count <= ++myBrushIndex)
            myBrushIndex = 0;
         Canvas.SetLeft(myRectangleMoving, mim.MapItem.Location.X);
         Canvas.SetTop(myRectangleMoving, mim.MapItem.Location.Y);
         Canvas.SetZIndex(myRectangleMoving, 1000);
         myRectangleMoving.Visibility = Visibility.Visible;
         return true;
      }
      //-------------CONTROLLER FUNCTIONS---------------------------------
      private void ClickButtonDailyAction(object sender, RoutedEventArgs e)
      {
         if (GamePhase.SunriseChoice != myGameInstance.GamePhase)
            return;
         Button b = (Button)sender;
         string s1 = (string)b.Content;
         foreach (UIElement ui in myStackPanelDailyActions.Children)
         {
            if (ui is Button b1)
            {
               b1.IsEnabled = false; // disable all buttons until active again
               if (b1.Content is string s2)
               {
                  if (s1 == s2)
                  {
                     b.Background = Utilities.theBrushControlButton; // indicate which button was selected
                     b1.IsEnabled = true; // disable all buttons until active again
                  }
               }
            }
         }
         GameAction outAction = GameAction.Error;
         myGameInstance.IsPartyRested = false; // party is not rested unless rest action is taken
         if (s1 == myButtonDailyContents[0])
         {
            if( true == myGameInstance.IsHeavyRainContinue )
               outAction = GameAction.E079HeavyRainsStartDayCheck;
            else 
               outAction = GameAction.Travel;
         }
         else if ((s1 == myButtonDailyContents[1]) || (s1 == "Rest & Lodge") || (s1 == "Rest & Train"))
         {
            myGameInstance.IsPartyRested = true;
            outAction = GameAction.RestEncounterCheck;
         }
         else if (s1 == myButtonDailyContents[2])
         {
            outAction = GameAction.SeekNews;
         }
         else if (s1 == myButtonDailyContents[3])
         {
            outAction = GameAction.SeekHire;
         }
         else if (s1 == myButtonDailyContents[4])
         {
            outAction = GameAction.SeekAudience;
         }
         else if (s1 == myButtonDailyContents[5])
         {
            outAction = GameAction.SeekOffering;
         }
         else if (s1 == myButtonDailyContents[6])
         {
            outAction = GameAction.SearchRuins;
         }
         else if (s1 == myButtonDailyContents[7])
         {
            outAction = GameAction.SearchCacheCheck;
         }
         else if (s1 == myButtonDailyContents[8])
         {
            outAction = GameAction.SearchTreasure;
         }
         else if (s1 == myButtonDailyContents[9])
         {
            outAction = GameAction.ArchTravel;
         }
         else if (s1 == myButtonDailyContents[10])
         {
            outAction = GameAction.EncounterFollow;
         }
         else if (s1 == myButtonDailyContents[11])
         {
            myGameInstance.RaftState = RaftEnum.RE_RAFT_CHOSEN;
            outAction = GameAction.Travel;
         }
         else if (s1 == myButtonDailyContents[12])
         {
            if (true == myGameInstance.IsHeavyRainContinue) 
               outAction = GameAction.E079HeavyRainsStartDayCheckInAir;
            else
               outAction = GameAction.TravelAir;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickButtonDailyAction() reached default b.Content=" + s1);
         }
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void ClickButtonMapItem(object sender, RoutedEventArgs e)
      {
         if (GamePhase.Travel != myGameInstance.GamePhase)
            return;
         GameAction outAction = GameAction.TravelEndMovement;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonTravel(object sender, MouseButtonEventArgs e)
      {
         Point canvasPoint = e.GetPosition(myCanvas);
         Polygon clickedPolygon = (Polygon)sender;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonTravel(): clickedPolygon=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         myTerritorySelected = TerritoryExtensions.Find(myGameInstance.Territories, Utilities.RemoveSpaces(clickedPolygon.Tag.ToString()));
         if (null == myTerritorySelected)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonTravel(): selectedTerritory=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         if (false == CreateMapItemMove(myGameInstance.Territories, myGameInstance, myTerritorySelected))
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonTravel(): CreateMapItemMove() returned false");
            return;
         }
         GameAction outAction = GameAction.TravelLostCheck;
         if ( null != myGameInstance.AirSpiritLocations ) // e110c - air spirit moves party
         {
            myGameInstance.GamePhase = GamePhase.Encounter;
            outAction = GameAction.E110AirSpiritTravelEnd;
            myGameInstance.NewHex = myTerritorySelected;
         }
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonArchOfTravel(object sender, MouseButtonEventArgs e) // e045
      {
         if (false == myIsTravelThroughGateActive)
            return;
         myIsTravelThroughGateActive = false;  // only allow one time per mouse click
         //-------------------------------------
         Point canvasPoint = e.GetPosition(myCanvas);
         Polygon clickedPolygon = (Polygon)sender;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonArchOfTravel(): clickedPolygon=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         //-------------------------------------
         ITerritory oldT = myGameInstance.Prince.Territory;
         IStack oldStack = myGameInstance.Stacks.Find(oldT);
         if (null == oldStack)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonArchOfTravel(): oldStack=null for t=" + oldT.ToString());
            return;
         }
         ITerritory newT = TerritoryExtensions.Find(myGameInstance.Territories, Utilities.RemoveSpaces(clickedPolygon.Tag.ToString()));
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonArchOfTravel(): newT=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         IStack newStack = myGameInstance.Stacks.Find(newT);
         if (null == newStack)
         {
            newStack = new Stack(newT);
            myGameInstance.Stacks.Add(newStack);
         }
         //-------------------------------------
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            mi.Territory = newT;
            newStack.MapItems.Add(mi);
            oldStack.MapItems.Remove(mi);
         }
         if (0 == oldStack.MapItems.Count)
            myGameInstance.Stacks.Remove(oldStack);
         //-------------------------------------
         GameAction outAction = GameAction.E045ArchOfTravelEnd;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonLetterChoice(object sender, MouseButtonEventArgs e) // e045
      {
         //-------------------------------------
         Point canvasPoint = e.GetPosition(myCanvas);
         Polygon clickedPolygon = (Polygon)sender;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonLetterChoice(): clickedPolygon=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         ITerritory letterLocation = TerritoryExtensions.Find(myGameInstance.Territories, Utilities.RemoveSpaces(clickedPolygon.Tag.ToString()));
         if (null == letterLocation)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonLetterChoice(): newT=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         //-------------------------------------
         if ((true == myGameInstance.IsInCastle(letterLocation)) || (true == myGameInstance.IsInTemple(letterLocation)))
         {
            myGameInstance.ActiveHex = letterLocation;
            myGameInstance.LetterOfRecommendations.Add(letterLocation);
            if (false == myGameInstance.ForbiddenAudiences.UpdateLetterLocation(letterLocation))
            {
               Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonLetterChoice(): UpdateLetterLocation() returned false for " + clickedPolygon.Tag.ToString());
               return;
            }
            GameAction outAction = GameAction.E156MayorTerritorySelectionEnd;
            myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         }
      }
      private void MouseLeftButtonDownCanvas(object sender, MouseButtonEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         //Point p = e.GetPosition(myCanvas);  // not used but useful info
         //--------------------------------------------------
         // Get the selected territory
         ITerritory selectedTerritory = null;
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Polygon aPolygon)
            {
               if (true == aPolygon.IsMouseOver)
               {
                  foreach (ITerritory t in Territory.theTerritories)
                  {
                     if (aPolygon.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
               }
            } // end if (ui is Polygon)
            if (null != selectedTerritory)
               break;
         }  // end foreach (UIElement ui in myCanvas.Children)
         if (null == selectedTerritory)  // If no territory is selected, return
            return;
         //---------------------------------------------------------------------
         switch (gi.GamePhase)
         {
            default:
               //MapItemCommonAction(selectedTerritory);
               break;
         }
      }
      private void MouseRightButtonDownCanvas(object sender, MouseButtonEventArgs e)
      {
         //Point p = e.GetPosition(myCanvas);  // not used but useful info
         // Get the selected territory
         ITerritory selectedTerritory = null;
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Polygon aPolygon)
            {
               if (true == aPolygon.IsMouseOver)
               {
                  foreach (ITerritory t in Territory.theTerritories)
                  {
                     if (aPolygon.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
               }

            } // end if (ui is Polygon
            if (null != selectedTerritory)
               break;
         }  // end foreach (UIElement ui in myCanvas.Children)
         if (null == selectedTerritory)  // If no territory is selected, return
            return;
         this.RotateStack(selectedTerritory);
      }
      //-------------ContextMenu---------------------------------
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
      private void ContextMenuButtonClickReturnToStart(object sender, RoutedEventArgs e)
      {
         if (sender is MenuItem mi)
         {
         }
      }
      private void ContextMenuButtonClickRotate(object sender, RoutedEventArgs e)
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
                     Logger.Log(LogEnum.LE_ERROR, "ContextMenuButtonClickRotate(): stack=null for name=" + b.Name);
                     return;
                  }
                  stack.Rotate();
                  UpdateCanvas(myGameInstance, GameAction.DieRollActionNone);
               }
            }
         }
      }
      private void ContextMenuButtonFlip(object sender, RoutedEventArgs e)
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
      private void ContextMenuButtonUnflip(object sender, RoutedEventArgs e)
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
      //-------------GameViewerWindow---------------------------------
      private void ContentRenderedGameViewerWindow(object sender, EventArgs e)
      {
         myIsContentRendered = true; // initial content rendered - start handling window resize actions
      }
      private void SizeChangedGameViewerWindow(object sender, SizeChangedEventArgs e)
      {
         if (true == myIsContentRendered) // only resize if the content is rendered for the first time
         {
            double mapPanelHeight = myDockPanelTop.ActualHeight - myMainMenu.ActualHeight - myStatusBar.ActualHeight;
            if (0 < mapPanelHeight) // Need to resize to take up panel content not taken by menu and status bar
            {
               myDockPanelInside.Height = mapPanelHeight;
               myScollViewerInside.Height = mapPanelHeight;
            }
            double mapPanelWidth = myDockPanelTop.ActualWidth - myDockPanelControls.ActualWidth - System.Windows.SystemParameters.VerticalScrollBarWidth;
            if (0 < mapPanelWidth) // need to resize so that scrollbar takes up panel not allocated to Control's DockPanel, i.e. where app controls are shown
               myScollViewerInside.Width = mapPanelWidth;
         }
      }
      private void ClosedGameViewerWindow(object sender, EventArgs e)
      {
         Application app = Application.Current;
         app.Shutdown();
      }
      //-------------CONTROLLER HELPER FUNCTIONS---------------------------------
      private void RotateStack(ITerritory selectedTerritory)
      {
      }
      private bool CreateMapItemMove(List<ITerritory> territories, IGameInstance gi, ITerritory newTerritory)
      {
         if (null == newTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): newTerritory=null");
            return false;
         }
         gi.Prince.TerritoryStarting = gi.Prince.Territory;
         MapItemMove mim = new MapItemMove(territories, gi.Prince, newTerritory);
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): Invalid State mim.NewTerritory=" + newTerritory.Name);
            return false;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): Invalid State bestpath.Count=" + mim.BestPath.Territories.Count.ToString() + "  mim.NewTerritory=" + mim.NewTerritory.Name);
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "CreateMapItemMove(): oT=" + gi.Prince.Territory.Name + " nT=" + mim.NewTerritory.Name);
         gi.MapItemMoves.Add(mim);
         return true;
      }
      private void CreateButtonContextMenu()
      {
         // Setup Context Menu for Buttons
         myContextMenuButton.Loaded += this.ContextMenuLoaded;
         MenuItem mi1 = new MenuItem() { Header = "_Return to Starting point", InputGestureText = "Ctrl+S" };
         mi1.Click += this.ContextMenuButtonClickReturnToStart;
         myContextMenuButton.Items.Add(mi1);
         MenuItem mi2 = new MenuItem { Header = "_Rotate Stack", InputGestureText = "Ctrl+R" };
         mi2.Click += this.ContextMenuButtonClickRotate;
         myContextMenuButton.Items.Add(mi2);
         MenuItem mi3 = new MenuItem() { Header = "_Flip", InputGestureText = "Ctrl+F" };
         mi3.Click += this.ContextMenuButtonFlip;
         myContextMenuButton.Items.Add(mi3);
         MenuItem mi4 = new MenuItem() { Header = "_Unflip", InputGestureText = "Ctrl+U" };
         mi4.Click += this.ContextMenuButtonUnflip;
         myContextMenuButton.Items.Add(mi4);
      }
   }
   public static class MyGameViewerWindowExtensions
   {
      public static Button Find(this IList<Button> list, string name)
      {
         IEnumerable<Button> results = from button in list
                                       where button.Name == name
                                       select button;
         if (0 < results.Count())
            return results.First();
         else
            return null;
      }
   }
}
