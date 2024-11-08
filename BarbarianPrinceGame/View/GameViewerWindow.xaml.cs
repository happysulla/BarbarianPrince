using BarbarianPrince.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Button = System.Windows.Controls.Button;
using MenuItem = System.Windows.Controls.MenuItem;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class GameViewerWindow : Window, IView
   {
      private const int MAX_DAILY_ACTIONS = 16;
      private const Double MARQUEE_SCROLL_ANMINATION_TIME = 30;
      public bool CtorError { get; } = false;
      //---------------------------------------------------------------------
      [Serializable]
      [StructLayout(LayoutKind.Sequential)]
      public struct POINT  // used in WindowPlacement structure
      {
         public int X;
         public int Y;
         public POINT(int x, int y)
         {
            X = x;
            Y = y;
         }
      }
      //-------------------------------------------
      [Serializable]
      [StructLayout(LayoutKind.Sequential)]
      public struct RECT // used in WindowPlacement structure
      {
         public int Left;
         public int Top;
         public int Right;
         public int Bottom;
         public RECT(int left, int top, int right, int bottom)
         {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
         }
      }
      //-------------------------------------------
      [Serializable]
      [StructLayout(LayoutKind.Sequential)]
      public struct WindowPlacement // used to save window position between sessions
      {
         public int length;
         public int flags;
         public int showCmd;
         public POINT minPosition;
         public POINT maxPosition;
         public RECT normalPosition;
         public bool IsZero()
         {
            if (0 != length)
               return false;
            if (0 != flags)
               return false;
            if (0 != minPosition.X)
               return false;
            if (0 != minPosition.Y)
               return false;
            if (0 != maxPosition.X)
               return false;
            if (0 != maxPosition.Y)
               return false;
            return true;
         }
      }
      //-------------------------------------------
      private static Double theEllipseDiameter = 10.0;
      private static Double theEllipseOffset = theEllipseDiameter / 2.0;
      private static Double X1 = 1.0;
      private static Double X2 = 1.0;
      private static Double X3 = 0.5;
      private static Double X4 = 1.5;
      private static Double X5 = 0.5;
      private static Double Y1 = 0.8;
      private static Double Y2 = 1.6;
      //---------------------------------------------------------------------
      private readonly IGameEngine myGameEngine = null;
      private IGameInstance myGameInstance = null;
      //---------------------------------------------------------------------
      private IDieRoller myDieRoller = null;
      private EventViewer myEventViewer = null;
      private MainMenuViewer myMainMenuViewer = null;
      private bool myExtendTimeGameOptionPrevious = false;
      private PartyDisplayDialog myPartyDisplayDialog = null;
      private EllipseDisplayDialog myEllipseDisplayDialog = null;
      private System.Windows.Input.Cursor myTargetCursor = null;
      private Dictionary<string, Polyline> myRivers = new Dictionary<string, Polyline>();
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private double myPreviousScrollHeight = 0.0;
      private double myPreviousScrollWidth = 0.0;
      //---------------------------------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushClear = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushGray = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushGreen = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushStart = new SolidColorBrush() { Color = Colors.Gold };
      private readonly SolidColorBrush mySolidColorBrushRed = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushPurple = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushRosyBrown = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushOrange = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushRest = new SolidColorBrush { Color = Colors.Yellow };
      private readonly SolidColorBrush mySolidColorBrushSkyBlue = new SolidColorBrush { Color = Colors.LightBlue };
      private readonly SolidColorBrush mySolidColorBrushWaterBlue = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      private readonly SolidColorBrush mySolidColorBrushWaterDark = new SolidColorBrush { Color = Colors.SteelBlue };
      private readonly SolidColorBrush mySolidColorBrushFollow = new SolidColorBrush { Color = Colors.HotPink };
      private readonly SolidColorBrush mySolidColorBrushPath = new SolidColorBrush { Color = Colors.White };
      //---------------------------------------------------------------------
      private readonly List<Button> myButtonMapItems = new List<Button>();
      private readonly SplashDialog mySplashScreen = null;
      private Button[] myButtonTimeTrackDays = new Button[7];
      private Button[] myButtonTimeTrackWeeks = new Button[15];
      private Button[] myButtonFoodSupply1s = new Button[10];
      private Button[] myButtonFoodSupply10s = new Button[10];
      private Button[] myButtonFoodSupply100s = new Button[5];
      private Button[] myButtonEndurances = new Button[12];
      private readonly List<Button> myButtonDailyAcions = new List<Button>();
      private readonly string[] myButtonDailyContents = new string[MAX_DAILY_ACTIONS] { "Travel", "Rest", "News", "Hire", "Audience", "Offering", "Search Ruins", "Search Cache", "Search Clue", "Arch Travel", "Follow", "Rafting", "Air Travel", "Steal Gems", "Rescue", "Attack" };
      //---------------------------------------------------------------------
      private ContextMenu myContextMenuButton = new ContextMenu();
      private readonly ContextMenu myContextMenuCanvas = new ContextMenu();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private int myBrushIndex = 0;
      private readonly List<Brush> myBrushes = new List<Brush>();
      private readonly List<Rectangle> myRectangles = new List<Rectangle>();
      private readonly List<Polygon> myPolygons = new List<Polygon>();
      private Rectangle myRectangleMoving = null;               // Not used - Rectangle that is moving with button
      private Rectangle myRectangleSelected = new Rectangle(); // Player has manually selected this button
      private ITerritory myTerritorySelected = null;
      private bool myIsTravelThroughGateActive = false;  // e045
      private Storyboard myStoryboard = new Storyboard();    // Show Statistics Marquee at end of game 
      private TextBlock myTextBoxMarquee; // Displayed at end to show Statistics of games
      private Double mySpeedRatioMarquee = 1.0;
      //-----------------------CONSTRUCTOR--------------------
      public GameViewerWindow(IGameEngine ge, IGameInstance gi)
      {
         NameScope.SetNameScope(this, new NameScope());
         myTextBoxMarquee = new TextBlock() { Foreground = Brushes.Red, FontFamily = myFontFam, FontSize = 24 };
         myTextBoxMarquee.MouseLeftButtonDown += MouseLeftButtonDownMarquee;
         myTextBoxMarquee.MouseLeftButtonUp += MouseLeftButtonUpMarquee;
         myTextBoxMarquee.MouseRightButtonDown += MouseRightButtonDownMarquee;
         this.RegisterName("tbMarquee", myTextBoxMarquee);
         //-----------------------------------------------------------------
         mySplashScreen = new SplashDialog(); // show splash screen waiting for finish initializing
         mySplashScreen.Show();
         InitializeComponent();
         //-----------------------------------------------------------------
         Image imageMap = new Image() { Name = "Map", Width = 810, Height = 985, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("Map") };
         myCanvas.Children.Add(imageMap);
         Canvas.SetLeft(imageMap, 0);
         Canvas.SetTop(imageMap, 0);
         //-----------------------------------------------------------------
         myGameEngine = ge;
         myGameInstance = gi;
         gi.GamePhase = GamePhase.GameSetup;
         myMainMenuViewer = new MainMenuViewer(myMainMenu, ge, gi);
         if (false == AddHotKeys(myMainMenuViewer))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): AddHotKeys() returned false");
            CtorError = true;
            return;
         }
         Options options = Deserialize(Settings.Default.GameOptions);
         myMainMenuViewer.NewGameOptions = options;
         gi.Options = options; // use the new game options for setting up the first game
         //-------------------------------------------
         if (false == String.IsNullOrEmpty(Settings.Default.GameDirectoryName))
            GameLoadMgr.theGamesDirectory = Settings.Default.GameDirectoryName; // remember the game directory name
         //-------------------------------------------
         Utilities.ZoomCanvas = Settings.Default.ZoomCanvas;
         myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
         StatusBarViewer sbv = new StatusBarViewer(myStatusBar, ge, gi, myCanvas);
         //-------------------------------------------
         if (false == String.IsNullOrEmpty(Settings.Default.GameTypeOriginal))
            myGameEngine.Statistics[0] = Utilities.Deserialize<GameStat>(Settings.Default.GameTypeOriginal);
         if (false == String.IsNullOrEmpty(Settings.Default.GameTypeRandParty))
            myGameEngine.Statistics[1] = Utilities.Deserialize<GameStat>(Settings.Default.GameTypeRandParty);
         if (false == String.IsNullOrEmpty(Settings.Default.GameTypeRandHex))
            myGameEngine.Statistics[2] = Utilities.Deserialize<GameStat>(Settings.Default.GameTypeRandHex);
         if (false == String.IsNullOrEmpty(Settings.Default.GameTypeRand))
            myGameEngine.Statistics[3] = Utilities.Deserialize<GameStat>(Settings.Default.GameTypeRand);
         if (false == String.IsNullOrEmpty(Settings.Default.GameTypeFun))
            myGameEngine.Statistics[4] = Utilities.Deserialize<GameStat>(Settings.Default.GameTypeFun);
         if (false == String.IsNullOrEmpty(Settings.Default.GameTypeCustom))
            myGameEngine.Statistics[5] = Utilities.Deserialize<GameStat>(Settings.Default.GameTypeCustom);
         if (false == String.IsNullOrEmpty(Settings.Default.GameTypeTotal))
            myGameEngine.Statistics[6] = Utilities.Deserialize<GameStat>(Settings.Default.GameTypeTotal);
         //-------------------------------------------
         if (false == String.IsNullOrEmpty(Settings.Default.theGameFeat))
         {
            GameEngine.theFeatsInGame = Utilities.Deserialize<GameFeat>(Settings.Default.theGameFeat);
            GameEngine.theFeatsInGameStarting = GameEngine.theFeatsInGame; // need to know difference between starting feats and feats that happen in this game
         }
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
         myDieRoller = new DieRoller(myCanvas, CloseSplashScreen); // Close the splash screen when die resources are loaded
         if (true == myDieRoller.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myDieRoller.CtorError=true");
            CtorError = true;
            return;
         }
         //-----------------------------------------------------------------
         myEventViewer = new EventViewer(myGameEngine, myGameInstance, myCanvas, myScrollViewerTextBlock, myStackPanelEndurance, Territory.theTerritories, myDieRoller);
         CanvasImageViewer civ = new CanvasImageViewer(myCanvas);
         CreateRiversFromXml();
         //-----------------------------------------------------------------
         CreateButtonTimeTrack(gi);
         CreateButtonFoodSupply();
         CreateButtonEndurance();
         CreateButtonDailyAction();
         //-----------------------------------------------------------------------
         // Implement the Model View Controller (MVC) pattern by registering views with
         // the game engine such that when the model data is changed, the views are updated.
         ge.RegisterForUpdates(civ);
         ge.RegisterForUpdates(myMainMenuViewer);
         ge.RegisterForUpdates(myEventViewer);
         ge.RegisterForUpdates(sbv);
         ge.RegisterForUpdates(this); // needs to be last so that canvas updates after all actions taken
         Logger.Log(LogEnum.LE_GAME_INIT, "GameViewerWindow(): \nzoomCanvas=" + Settings.Default.ZoomCanvas.ToString() + "\nwp=" + Settings.Default.WindowPlacement + "\noptions=" + Settings.Default.GameOptions);
#if UT1
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
         //-------------------------------------------------------
         if (GameAction.TravelAirRedistribute == action) // if redistributing loads to to air travel, do not update canvas
            return;
         //-------------------------------------------------------
         if ((null != myTargetCursor) && (GameAction.UpdateStatusBar == action)) // increase/decrease size of cursor when zoom in or out
         {
            myTargetCursor.Dispose();
            double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
            System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Target"), Width = sizeCursor, Height = sizeCursor };
            myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
            this.myCanvas.Cursor = myTargetCursor;
         }
         //-------------------------------------------------------
         else if ((GameAction.UpdateLoadingGame == action) || (GameAction.UpdateNewGame == action) )
         {
            myGameInstance = gi;
            myButtonMapItems.Clear();
            foreach (UIElement ui in myCanvas.Children) // remove all buttons on map
            {
               if (ui is Button b)
               {
                  if (true == b.Name.Contains("Prince"))
                  {
                     myCanvas.Children.Remove(ui);
                     break;
                  }
               }
            }
            myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
            this.Title = UpdateTitle(gi.Options);
         }
         //-------------------------------------------------------
         Option optionExtendTime = gi.Options.Find("ExtendEndTime");
         if (null == optionExtendTime)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): gi.Options.Find(ExtendEndTime)");
            optionExtendTime = new Option("ExtendEndTime", false);
         }
         if (myExtendTimeGameOptionPrevious != optionExtendTime.IsEnabled) // if there is change in this option, recreate the buttons
         {
            CreateButtonTimeTrack(gi);
            myExtendTimeGameOptionPrevious = optionExtendTime.IsEnabled;
         }
         //-------------------------------------------------------
         UpdateStackPanelDailyActions(gi);
         UpdateTimeTrack(gi);
         UpdateFoodSupply(gi);
         UpdatePrinceEnduranceStatus(gi);
         UpdateScrollbarThumbnails(gi.Prince.Territory);
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowAboutDialog:
            case GameAction.E228ShowTrueLove:
            case GameAction.EndGameWin:
            case GameAction.EndGameLost:
               break;
            case GameAction.RemoveSplashScreen:
               this.Title = UpdateTitle(gi.Options);
               if (false == UpdateCanvas(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvas() returned error ");
               mySplashScreen.Close();
               UpdateScrollbarThumbnails(gi.Prince.Territory);
               break;
            case GameAction.UpdateGameOptions:
               this.Title = UpdateTitle(gi.Options);
               Option option = gi.Options.Find("ExtendEndTime");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): gi.Options.Find(ExtendEndTime)");
               else
                  CreateButtonTimeTrack(gi);
               break;
            case GameAction.ShowAllRivers:
               UpdateCanvasRiver("Dienstal Branch", false);
               UpdateCanvasRiver("Largos River", false);
               UpdateCanvasRiver("Nesser River", false);
               UpdateCanvasRiver("Greater Nesser River", false);
               UpdateCanvasRiver("Lesser Nesser River", false);
               UpdateCanvasRiver("Trogoth River", false);
               break;
            case GameAction.EndGameFinal:
               myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
               if (false == UpdateCanvasPath(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas(): UpdateCanvasPath() returned false");
               if (false == UpdateCanvas(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvas() returned error ");
               break;
            case GameAction.ShowPartyPath:
               if (true == myMainMenuViewer.IsPathShown)
               {
                  if (false == UpdateCanvasPath(gi))
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas(): UpdateCanvasPath() returned false");
               }
               else
               {
                  List<UIElement> elements = new List<UIElement>();
                  foreach (UIElement ui in myCanvas.Children)
                  {
                     if (ui is Polyline polyline) // remove all polylines 
                        elements.Add(ui);
                     if (ui is Ellipse ellipse) // remove all polylines 
                        elements.Add(ui);
                     if (ui is Image img)
                     {
                        if ("Map" == img.Name)
                           continue;
                        elements.Add(ui);
                     }
                  }
                  foreach (UIElement ui1 in elements)
                     myCanvas.Children.Remove(ui1);
               }
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
            case GameAction.EndGameShowStats:
               if (false == UpdateCanvasShowStats(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvas() returned error ");
               break;
            default:
               if (false == UpdateCanvas(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvas() returned error ");
               break;
         }
      }
      //-----------------------SUPPORTING FUNCTIONS--------------------
      private void CloseSplashScreen() // callback function that removes splash screen when dice are loaded
      {
         GameAction outAction = GameAction.RemoveSplashScreen;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void CreateButtonTimeTrack(IGameInstance gi)
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
         //--------------------------------------
         myStackPanelTimeTrackWeek.Children.Clear();
         Option option = gi.Options.Find("ExtendEndTime");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): gi.Options.Find(ExtendEndTime)");
            option = new Option("ExtendEndTime", false);
         }
         int numWeeks = 10;
         if (true == option.IsEnabled)
            numWeeks = 15;
         for (int i = 0; i < numWeeks; ++i)
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
         UpdateTimeTrack(gi);
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
         ITerritory territory = Territory.theTerritories.Find(territoryName);
         if (null == territory)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItem(): TerritoryExtensions.Find() returned null");
            return false;
         }
         System.Windows.Controls.Button b = new Button { ContextMenu = myContextMenuButton, Name = Utilities.RemoveSpaces(mi.Name), Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         Canvas.SetLeft(b, territory.CenterPoint.X - mi.Zoom * Utilities.theMapItemOffset + (counterCount * Utilities.STACK));
         Canvas.SetTop(b, territory.CenterPoint.Y - mi.Zoom * Utilities.theMapItemOffset + (counterCount * Utilities.STACK));
         MapItem.SetButtonContent(b, mi, false, false, false, false); // This sets the image as the button's content
         myButtonMapItems.Add(b);
         myCanvas.Children.Add(b);
         Canvas.SetZIndex(b, counterCount);
         b.Click += ClickButtonMapItem;
         b.MouseEnter += MouseEnterMapItem;
         b.MouseLeave += MouseLeaveMapItem;
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
            Console.WriteLine("CreateRiversFromXml(): Cannot Read from Rivers.xml file:\ne.Message={0} while reading reader.Name={1}", e.Message, reader.Name);
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      //---------------------------------------
      private Options Deserialize(String s_xml)
      {
         Options options = new Options();
         if (false == String.IsNullOrEmpty(s_xml))
         {
            try // XML serializer does not work for Interfaces
            {
               StringReader stringreader = new StringReader(s_xml);
               XmlReader xmlReader = XmlReader.Create(stringreader);
               XmlSerializer serializer = new XmlSerializer(typeof(Options)); // Sustem.IO.FileNotFoundException thrown but normal behavior - handled in XmlSerializer constructor
               options = (Options)serializer.Deserialize(xmlReader);
            }
            catch (DirectoryNotFoundException dirException)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + "\ndirException=" + dirException.ToString());
            }
            catch (FileNotFoundException fileException)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + "\nfileException=" + fileException.ToString());
            }
            catch (IOException ioException)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + "\nioException=" + ioException.ToString());
            }
            catch (Exception ex)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + "\nex=" + ex.ToString());
            }
         }
         if (0 == options.Count)
            options.SetDefaults();
         return options;
      }
      //---------------------------------------
      private string UpdateTitle(Options options)
      {
         string name = "CustomGame";
         Option option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return "Barbarian Prince - Custom Game";
         name = "MaxFunGame";
         option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return "Barbarian Prince - Fun Game";
         name = "RandomGame";
         option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return "Barbarian Prince - All Random Options Game";
         name = "RandomHexGame";
         option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return "Barbarian Prince - Random Hex Game";
         name = "RandomPartyGame";
         option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return "Barbarian Prince - Random Party Game";
         return "Barbarian Prince - Orginal Game";
      }
      private void UpdateTimeTrack(IGameInstance gi)
      {
         Option option = gi.Options.Find("ExtendEndTime");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): gi.Options.Find(ExtendEndTime)");
            option = new Option("ExtendEndTime", false);
         }
         int numWeeks = 10;
         if (true == option.IsEnabled)
            numWeeks = 15;
         //---------------------------------------------------
         for (int i = 0; i < 7; ++i)
         {
            myButtonTimeTrackDays[i].ClearValue(Control.BackgroundProperty);
            myButtonTimeTrackDays[i].FontWeight = FontWeights.Normal;
            myButtonTimeTrackDays[i].IsEnabled = false;
         }
         for (int j = 0; j < numWeeks; ++j)
         {
            myButtonTimeTrackWeeks[j].ClearValue(Control.BackgroundProperty);
            myButtonTimeTrackWeeks[j].FontWeight = FontWeights.Normal;
            myButtonTimeTrackWeeks[j].IsEnabled = false;
         }
         if (gi.Days < 0)
            return;
         //---------------------------------------------------
         int week = gi.Days / 7; // round down to nearest integer
         --numWeeks;
         if (numWeeks < week)
            week = numWeeks;
         myButtonTimeTrackWeeks[week].Background = Utilities.theBrushControlButton;
         myButtonTimeTrackWeeks[week].FontWeight = FontWeights.Bold;
         myButtonTimeTrackWeeks[week].IsEnabled = true;
         //---------------------------------------------------
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
      private void UpdatePrinceEnduranceStatus(IGameInstance gi)
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
            Logger.Log(LogEnum.LE_ERROR, "UpdatePrinceEnduranceStatus(): healthRemaining=" + healthRemaining.ToString());
            healthRemaining = 0;
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
            if ((true == gi.IsInMapItems("WarriorBoy")) && ("1212" == gi.Prince.Territory.Name))
            {
               myStackPanelDailyActions.Children.Add(myButtonDailyAcions[15]);    // Sneak Attack in Huldra Castle
               myStackPanelDailyActions.Visibility = Visibility.Visible;
            }
            if ((true == gi.IsSecretBaronHuldra) && ("1611" == gi.Prince.Territory.Name))
            {
               myStackPanelDailyActions.Children.Add(myButtonDailyAcions[14]);    // Rescue True Heir to Huldra Castle
               myStackPanelDailyActions.Visibility = Visibility.Visible;
            }
            else if ((true == gi.IsSpecialItemHeld(SpecialEnum.Foulbane)) && ("0323" == gi.Prince.Territory.Name))
            {
               myStackPanelDailyActions.Children.Add(myButtonDailyAcions[13]);    // Steal Count Drogat Gems
               myStackPanelDailyActions.Visibility = Visibility.Visible;
            }
            if (false == gi.IsHeavyRainDismount) // if choose to dismount due to heavy rains, do not fly
            {
               bool isButtonAdded = false;
               foreach (IMapItem partyMember in gi.PartyMembers) // if at least one party member is a flying mount, Prince can fly
               {
                  if (true == partyMember.IsFlyingMountCarrier())  // mount cannot fly if starving or exhausted
                  {
                     if ((0 == partyMember.StarveDayNum) && (false == partyMember.IsExhausted) && (false == partyMember.IsSunStroke))
                     {
                        myStackPanelDailyActions.Children.Add(myButtonDailyAcions[12]);    // air travel
                        myStackPanelDailyActions.Visibility = Visibility.Visible;
                        isButtonAdded = true;
                        break;
                     }
                  }
                  else
                  {
                     foreach (IMapItem mount in partyMember.Mounts)
                     {
                        if ((true == mount.IsFlyingMount()) && (0 == mount.StarveDayNum) && (false == mount.IsExhausted) && (false == mount.IsSunStroke))
                        {
                           myStackPanelDailyActions.Children.Add(myButtonDailyAcions[12]);    // air travel
                           myStackPanelDailyActions.Visibility = Visibility.Visible;
                           isButtonAdded = true;
                           break;
                        }
                        if (true == isButtonAdded)
                           break;
                     }
                  }
                  if (true == isButtonAdded)
                     break;
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
                  if ((null != gi.Arches.Find(t.Name)) && (true == gi.IsArchTravelKnown)) // arch travel needs to be known to use it
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[9]);
                  if ((null != gi.SecretClues.Find(t.Name)) || (null != gi.WizardAdviceLocations.Find(t.Name)) || (null != gi.PixieAdviceLocations.Find(t.Name)) )
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[8]);
                  if (null != gi.Caches.Find(t.Name))
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[7]);
                  if (true == isInRuins)
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[6]);
                  if ((true == isInTemple) && (1 < myGameInstance.GetCoins()))
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[5]);   // offering
                  if (((true == isInTownOrCastle) || (true == isInTemple)) && (false == gi.ForbiddenAudiences.Contains(gi))) // audience
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[4]);
                  if ((true == isInTownOrCastle) && (false == myGameInstance.ForbiddenHires.Contains(gi.Prince.Territory))) // hire
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[3]);
                  if (((true == isInTownOrCastle) || (true == isInTemple)) && (false == gi.HiddenTowns.Contains(t))) // news - hidden towns do not due news
                     myStackPanelDailyActions.Children.Add(myButtonDailyAcions[2]);
               }
               //-------------------------------------------------------------------------
               if ((false == gi.IsExhausted) || (true == t.IsOasis) || ("Desert" != t.Type)) // e120 - cannot rest if exhausted in desert without oasis
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
            if ((false == gi.IsTrainHorse) && (false == gi.IsFloodContinue) && (false == gi.IsWoundedWarriorRest) && (false == gi.IsWoundedBlackKnightRest)) // cannot travel if training horse, in flood, or waiting for warrior to rest
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
            Console.WriteLine("UpdateCanvasRiver(): unknown river=" + river + " EXCEPTION THROWN e={0}", e.ToString());
         }
      }
      private bool UpdateCanvasPath(IGameInstance gi)
      {
         try
         {
            Dictionary<string, int> hexCounts = new Dictionary<string, int>();
            foreach (EnteredHex hex in gi.EnteredHexes) // get the number of ellipses to be shown in hex
               hexCounts[hex.HexName] = hex.Position;
            PointCollection aPointCollection = new PointCollection();
            foreach (EnteredHex hex in gi.EnteredHexes)
            {
               int countOfHexesInHex = hexCounts[hex.HexName];
               System.Windows.Point p = UpdateCanvasPathCreateEllipse(hex, countOfHexesInHex);
               if ((p.X < 0.0) && (p.Y < 0.0))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasPath(): UpdateCanvasPathCreateEllipse() returned error");
                  return false;
               }
               aPointCollection.Add(p);
            }
            Polyline aPolyline = new Polyline();
            aPolyline.Stroke = mySolidColorBrushPath;
            aPolyline.StrokeThickness = 2;
            aPolyline.Points = aPointCollection;
            aPolyline.StrokeDashArray = myDashArray;
            Canvas.SetZIndex(aPolyline, 1);
            myCanvas.Children.Add(aPolyline);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasPath(): EXCEPTION THROWN e=" + e.ToString());
            return false;
         }
         return true;
      }
      private Point UpdateCanvasPathCreateEllipse(EnteredHex enteredHex, int ellipsesInHex)
      {
         System.Windows.Point p = new System.Windows.Point(0.0, 0.0);
         ITerritory t = Territory.theTerritories.Find(enteredHex.HexName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasPathCreateEllipse(): t=null for name=" + enteredHex.HexName);
            return p;
         }
         //-----------------------------------------
         Ellipse aEllipse = new Ellipse
         {
            Tag = enteredHex.Identifer,
            Fill = mySolidColorBrushClear,
            StrokeThickness = 2,
            Stroke = mySolidColorBrushBlack,
            Width = theEllipseDiameter,
            Height = theEllipseDiameter
         };
         aEllipse.MouseEnter += this.MouseEnterEllipse;
         aEllipse.MouseLeave += this.MouseLeaveEllipse;
         SolidColorBrush brush = mySolidColorBrushBlack;
         switch (enteredHex.ColorAction)
         {
            case ColorActionEnum.CAE_START:
               brush = mySolidColorBrushStart;
               break;
            case ColorActionEnum.CAE_REST:
               brush = mySolidColorBrushRest;
               break;
            case ColorActionEnum.CAE_LOST:
               brush = mySolidColorBrushRosyBrown;
               break;
            case ColorActionEnum.CAE_JAIL:
               brush = mySolidColorBrushBlack;
               break;
            case ColorActionEnum.CAE_TRAVEL:
               brush = mySolidColorBrushGreen;
               break;
            case ColorActionEnum.CAE_TRAVEL_AIR:
               brush = mySolidColorBrushSkyBlue;
               break;
            case ColorActionEnum.CAE_TRAVEL_RAFT:
               brush = mySolidColorBrushWaterBlue;
               break;
            case ColorActionEnum.CAE_TRAVEL_DOWNRIVER:
               brush = mySolidColorBrushWaterDark;
               break;
            case ColorActionEnum.CAE_ESCAPE:
               brush = mySolidColorBrushRed;
               break;
            case ColorActionEnum.CAE_FOLLOW:
               brush = mySolidColorBrushFollow;
               break;
            case ColorActionEnum.CAE_SEARCH:
               brush = mySolidColorBrushOrange;
               break;
            case ColorActionEnum.CAE_SEARCH_RUINS:
               brush = mySolidColorBrushGray;
               break;
            case ColorActionEnum.CAE_SEEK_NEWS:
            case ColorActionEnum.CAE_HIRE:
            case ColorActionEnum.CAE_AUDIENCE:
            case ColorActionEnum.CAE_OFFERING:
               brush = mySolidColorBrushPurple;
               break;
            default:
               break;
         }
         aEllipse.Stroke = brush;
         aEllipse.Fill = brush;
         //-----------------------------------------
         double xOffset = 0.0;
         double yOffset = 0.0;
         UpdateCanvasPathSetEllipseLocation(enteredHex, ellipsesInHex, out xOffset, out yOffset);
         //-----------------------------------------
         p.X = t.CenterPoint.X + xOffset;
         p.Y = t.CenterPoint.Y + yOffset;
         Canvas.SetLeft(aEllipse, p.X - theEllipseOffset);
         Canvas.SetTop(aEllipse, p.Y - theEllipseOffset);
         Canvas.SetZIndex(aEllipse, 1000);
         myCanvas.Children.Add(aEllipse);
         return p;
      }
      private void UpdateCanvasPathSetEllipseLocation(EnteredHex enteredHex, int ellipsesInHex, out double xOffset, out double yOffset)
      {
         xOffset = 0.0;
         yOffset = 0.0;
         switch (ellipsesInHex)
         {
            case 0:
               break;
            case 1:
               if (0 == enteredHex.Position)
                  xOffset = -X1 * theEllipseDiameter;
               else
                  xOffset = +X1 * theEllipseDiameter;
               break;
            case 2:
               if (0 == enteredHex.Position)
                  xOffset = -X2 * theEllipseDiameter;
               else if (1 == enteredHex.Position)
                  xOffset = 0.0;
               else
                  xOffset = +X2 * theEllipseDiameter;
               break;
            case 3:
               if (0 == enteredHex.Position)
                  xOffset = -X4 * theEllipseDiameter;
               else if (1 == enteredHex.Position)
                  xOffset = -X3 * theEllipseDiameter;
               else if (2 == enteredHex.Position)
                  xOffset = +X3 * theEllipseDiameter;
               else
                  xOffset = +X4 * theEllipseDiameter;
               break;
            case 4:
               if (0 == enteredHex.Position)
                  yOffset = -Y1 * theEllipseDiameter;
               else if (1 == enteredHex.Position)
                  xOffset = -X4 * theEllipseDiameter;
               else if (2 == enteredHex.Position)
                  xOffset = -X3 * theEllipseDiameter;
               else if (3 == enteredHex.Position)
                  xOffset = +X3 * theEllipseDiameter;
               else
                  xOffset = +X4 * theEllipseDiameter;
               break;
            case 5:
               if (0 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = -X1 * theEllipseDiameter;
               }
               else if (1 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = +X1 * theEllipseDiameter;
               }
               else if (2 == enteredHex.Position)
               {
                  xOffset = -X4 * theEllipseDiameter;
               }
               else if (3 == enteredHex.Position)
               {
                  xOffset = -X3 * theEllipseDiameter;
               }
               else if (4 == enteredHex.Position)
               {
                  xOffset = +X3 * theEllipseDiameter;
               }
               else
               {
                  xOffset = +X4 * theEllipseDiameter;
               }
               break;
            case 6:
               if (0 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (1 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
               }
               else if (2 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else if (3 == enteredHex.Position)
               {
                  xOffset = -X4 * theEllipseDiameter;
               }
               else if (4 == enteredHex.Position)
               {
                  xOffset = -X3 * theEllipseDiameter;
               }
               else if (5 == enteredHex.Position)
               {
                  xOffset = +X3 * theEllipseDiameter;
               }
               else
               {
                  xOffset = +X4 * theEllipseDiameter;
               }
               break;
            case 7:
               if (0 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (1 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
               }
               else if (2 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else if (3 == enteredHex.Position)
               {
                  xOffset = -X4 * theEllipseDiameter;
               }
               else if (4 == enteredHex.Position)
               {
                  xOffset = -X3 * theEllipseDiameter;
               }
               else if (5 == enteredHex.Position)
               {
                  xOffset = +X3 * theEllipseDiameter;
               }
               else if (6 == enteredHex.Position)
               {
                  xOffset = +X4 * theEllipseDiameter;
               }
               else
               {
                  yOffset = +Y1 * theEllipseDiameter;
               }
               break;
            case 8:
               if (0 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (1 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
               }
               else if (2 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else if (3 == enteredHex.Position)
               {
                  xOffset = -X4 * theEllipseDiameter;
               }
               else if (4 == enteredHex.Position)
               {
                  xOffset = -X3 * theEllipseDiameter;
               }
               else if (5 == enteredHex.Position)
               {
                  xOffset = +X3 * theEllipseDiameter;
               }
               else if (6 == enteredHex.Position)
               {
                  xOffset = +X4 * theEllipseDiameter;
               }
               else if (7 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = -X1 * theEllipseDiameter;
               }
               else
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = +X1 * theEllipseDiameter;
               }
               break;
            case 9:
               if (0 == enteredHex.Position)
               {
                  yOffset = +Y1 * -theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (1 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
               }
               else if (2 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else if (3 == enteredHex.Position)
               {
                  xOffset = -X4 * theEllipseDiameter;
               }
               else if (4 == enteredHex.Position)
               {
                  xOffset = -X3 * theEllipseDiameter;
               }
               else if (5 == enteredHex.Position)
               {
                  xOffset = +X3 * theEllipseDiameter;
               }
               else if (6 == enteredHex.Position)
               {
                  xOffset = +X4 * theEllipseDiameter;
               }
               else if (7 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (8 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
               }
               else
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               break;
            case 10:
               if (0 == enteredHex.Position)
               {
                  yOffset = -Y2 * theEllipseDiameter;
                  xOffset = -X5 * theEllipseDiameter;
               }
               else if (1 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (2 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
               }
               else if (3 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else if (4 == enteredHex.Position)
               {
                  xOffset = -X4 * theEllipseDiameter;
               }
               else if (5 == enteredHex.Position)
               {
                  xOffset = -X3 * theEllipseDiameter;
               }
               else if (6 == enteredHex.Position)
               {
                  xOffset = +X3 * theEllipseDiameter;
               }
               else if (7 == enteredHex.Position)
               {
                  xOffset = +X4 * theEllipseDiameter;
               }
               else if (8 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (9 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
               }
               else
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               break;
            case 11:
               if (0 == enteredHex.Position)
               {
                  yOffset = -Y2 * theEllipseDiameter;
                  xOffset = -X5 * theEllipseDiameter;
               }
               else if (1 == enteredHex.Position)
               {
                  yOffset = -Y2 * theEllipseDiameter;
                  xOffset = +X5 * theEllipseDiameter;
               }
               else if (2 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (3 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
               }
               else if (4 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else if (5 == enteredHex.Position)
               {
                  xOffset = -X4 * theEllipseDiameter;
               }
               else if (6 == enteredHex.Position)
               {
                  xOffset = -X3 * theEllipseDiameter;
               }
               else if (7 == enteredHex.Position)
               {
                  xOffset = +X3 * theEllipseDiameter;
               }
               else if (8 == enteredHex.Position)
               {
                  xOffset = +X4 * theEllipseDiameter;
               }
               else if (9 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (10 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
               }
               else
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               break;
            case 12:
               if (0 == enteredHex.Position)
               {
                  yOffset = -Y2 * theEllipseDiameter;
                  xOffset = -X5 * theEllipseDiameter;
               }
               else if (1 == enteredHex.Position)
               {
                  yOffset = -Y2 * theEllipseDiameter;
                  xOffset = +X5 * theEllipseDiameter;
               }
               else if (2 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (3 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
               }
               else if (4 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else if (5 == enteredHex.Position)
               {
                  xOffset = -X4 * theEllipseDiameter;
               }
               else if (6 == enteredHex.Position)
               {
                  xOffset = -X3 * theEllipseDiameter;
               }
               else if (7 == enteredHex.Position)
               {
                  xOffset = +X3 * theEllipseDiameter;
               }
               else if (8 == enteredHex.Position)
               {
                  xOffset = +X4 * theEllipseDiameter;
               }
               else if (9 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (10 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
               }
               else if (11 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else
               {
                  yOffset = +Y2 * theEllipseDiameter;
                  xOffset = -X5 * theEllipseDiameter;
               }
               break;
            default:
               if (0 == enteredHex.Position)
               {
                  yOffset = -Y2 * theEllipseDiameter;
                  xOffset = -X5 * theEllipseDiameter;
               }
               else if (1 == enteredHex.Position)
               {
                  yOffset = -Y2 * theEllipseDiameter;
                  xOffset = +X5 * theEllipseDiameter;
               }
               else if (2 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (3 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
               }
               else if (4 == enteredHex.Position)
               {
                  yOffset = -Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else if (5 == enteredHex.Position)
               {
                  xOffset = -X4 * theEllipseDiameter;
               }
               else if (6 == enteredHex.Position)
               {
                  xOffset = -X3 * theEllipseDiameter;
               }
               else if (7 == enteredHex.Position)
               {
                  xOffset = +X3 * theEllipseDiameter;
               }
               else if (8 == enteredHex.Position)
               {
                  xOffset = +X4 * theEllipseDiameter;
               }
               else if (9 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = -X2 * theEllipseDiameter;
               }
               else if (10 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
               }
               else if (11 == enteredHex.Position)
               {
                  yOffset = +Y1 * theEllipseDiameter;
                  xOffset = +X2 * theEllipseDiameter;
               }
               else if (12 == enteredHex.Position)
               {
                  yOffset = +Y2 * theEllipseDiameter;
                  xOffset = -X5 * theEllipseDiameter;
               }
               else // if 13 or greater, last one shows up here
               {
                  yOffset = +Y2 * theEllipseDiameter;
                  xOffset = +X5 * theEllipseDiameter;
               }
               break;
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
            if (ui is Ellipse ellipse)
            {
               if("CenterPoint" != ellipse.Name) // CenterPoint is a unit test ellipse
                  elements.Add(ui);
            }
            if (ui is Image img)
            {
               if ("Map" == img.Name)
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
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
         foreach (ITerritory t in gi.PixieAdviceLocations)
         {
            double size = 1.6 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Pixie"), Width = size, Height = size };
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
            double size = 0.9 * Utilities.theMapItemOffset;
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DwarfMines"), Width = 2.0 * size, Height = size };
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
            Canvas.SetTop(img1, t.CenterPoint.Y - size / 3);
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
         foreach (ITerritory t in gi.OrcTowers)
         {
            Image img1 = new Image { Tag = "KeepOrc", Source = MapItem.theMapImages.GetBitmapImage("TowerOrc"), Width = 0.6 * Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
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
            ITerritory t = Territory.theTerritories.Find("1611");
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
            ITerritory t = Territory.theTerritories.Find("2018");
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
            if (true == gi.AbandonedTemples.Contains(fa.ForbiddenTerritory)) // abandoned temples disappear
            {
               // do nothing
            }
            else if ((AudienceConstraintEnum.RELIGION == fa.Constraint) && (true == gi.IsReligionInParty())) // If religious constraint, but religion in party, ignore it
            {
               // do nothing
            }
            else
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
         if (true == myMainMenuViewer.IsPathShown)
         {
            if (false == UpdateCanvasPath(gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas(): UpdateCanvasPath() returned false");
               return false;
            }
         }
         //-------------------------------------------------------
         if (null != gi.TargetHex)
         {
            UpdateCanvasHexToShowPolygon(gi.TargetHex);
            gi.TargetHex = null;
         }
         //-------------------------------------------------------
         try
         {
            switch (action)
            {
               case GameAction.EndGameClose:
                  GameAction outActionClose = GameAction.EndGameExit;
                  myGameEngine.PerformAction(ref gi, ref outActionClose);
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
                     if (("e213a" == myGameInstance.EventActive) || ("e401" == myGameInstance.EventActive) || (GamePhase.Rest == gi.SunriseChoice) || (GamePhase.SearchCache == gi.SunriseChoice) || (GamePhase.SearchTreasure == gi.SunriseChoice) || (GamePhase.SearchRuins == gi.SunriseChoice))
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
                  myIsTravelThroughGateActive = false;
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
                        points.Add(new System.Windows.Point(mp1.X, mp1.Y));
                     Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Tag = t.ToString() };
                     myCanvas.Children.Add(aPolygon);
                  }
                  break;
               case GameAction.E130JailedOnTravels:
                  if (false == MoveToNewHexWhenJailed(gi))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas():  MoveToNewHexWhenJailed() returned false");
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
               case GameAction.CampfireStarvationCheck: 
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
            if (0 < gi.MapItemMoves.Count)
            {
               IMapItemMove mim2 = gi.MapItemMoves[0];
               IMapItem mi = mim2.MapItem;
               Logger.Log(LogEnum.LE_VIEW_MIM, "UpdateCanvasMovement():<<<<<<<< oT=" + mim2.OldTerritory.ToString() + "-->" + mim2.NewTerritory.ToString() + " ae=" + myGameInstance.EventActive + " c=" + myGameInstance.SunriseChoice.ToString() + " m=" + myGameInstance.Prince.MovementUsed + "/" + myGameInstance.Prince.Movement);
               if (false == MovePathAnimate(mim2))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): MovePathAnimate() returned false t=" + mim2.OldTerritory.ToString());
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "UpdateCanvasMovement(): MovePathAnimate() returned false gi.MapItemMoves.Clear()");
                  gi.MapItemMoves.Clear();
                  return false;
               }
               mi.Territory = mim2.NewTerritory;
               mi.TerritoryStarting = mim2.NewTerritory;
            }
         }
         catch (Exception e)
         {
            Console.WriteLine("UpdateCanvasMovement() - EXCEPTION THROWN e={0}", e.ToString());
            return false;
         }
         return true;
      }
      private void UpdateCanvasHexToShowPolygon(ITerritory t)
      {
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasHexToShowPolygon(): t=null");
            return;
         }
         UpdateScrollbarThumbnails(t);
         PointCollection points = new PointCollection();
         foreach (IMapPoint mp1 in t.Points)
            points.Add(new System.Windows.Point(mp1.X, mp1.Y));
         Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Tag = t.ToString() };
         myPolygons.Add(aPolygon);
         myCanvas.Children.Add(aPolygon);
      }
      private void UpdateScrollbarThumbnails(ITerritory t)
      {
         double percentHeight = (t.CenterPoint.Y / myCanvas.ActualHeight);
         double percentToScroll = 0.0;
         if (percentHeight < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentHeight)
            percentToScroll = 1.0;
         else
            percentToScroll = percentHeight / 0.5 - 0.5;
         double scrollHeight = myScollViewerInside.ScrollableHeight;
         if (0.0 == scrollHeight)
            scrollHeight = myPreviousScrollHeight;
         else
            myPreviousScrollHeight = myScollViewerInside.ScrollableHeight;
         double amountToScroll = percentToScroll * scrollHeight;
         myScollViewerInside.ScrollToVerticalOffset(amountToScroll);
         //--------------------------------------------------------------------
         double percentWidth = (t.CenterPoint.X / myCanvas.ActualWidth);
         if (percentWidth < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentWidth)
            percentToScroll = 1.0;
         else
            percentToScroll = percentWidth / 0.5 - 0.5;
         double scrollWidth = myScollViewerInside.ScrollableWidth;
         if (0.0 == scrollWidth)
            scrollWidth = myPreviousScrollWidth;
         else
            myPreviousScrollWidth = myScollViewerInside.ScrollableWidth;
         amountToScroll = percentToScroll * scrollWidth;
         myScollViewerInside.ScrollToHorizontalOffset(amountToScroll);
      }
      private bool UpdateMapItemRectangle(IGameInstance gi)
      {
         if (0 == myGameInstance.Prince.MovementUsed) // if this is the first movement, do not show rectangle
            return true;
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
            ITerritory selectedTerritory = gi.Prince.Territory;
            for (int i = gi.EnteredHexes.Count - 1; -1 < i; --i)  // get previous territory
            {
               string hexName = gi.EnteredHexes[i].HexName;
               if (gi.Prince.Territory.Name != hexName)
               {
                  ITerritory t = Territory.theTerritories.Find(hexName);
                  if (null == t)
                     Logger.Log(LogEnum.LE_ERROR, "GetPreviousHex(): theTerritories.Find() returned null for n=" + hexName);
                  else
                     selectedTerritory = t;
                  break;
               }
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in selectedTerritory.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Tag = selectedTerritory.ToString() };
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
            ITerritory t = Territory.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasHexTravelToShowPolygons(): 1 t=null for " + s);
               return false;
            }
            if (true == gi.ForbiddenHexes.Contains(t))
               continue;
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
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
         foreach (ITerritory t in Territory.theTerritories)
         {
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
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
         foreach (ITerritory t in Territory.theTerritories)
         {
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegionClear, Points = points, Tag = t.ToString() };
            aPolygon.MouseDown += MouseDownPolygonLetterChoice;
            myPolygons.Add(aPolygon);
            myCanvas.Children.Add(aPolygon);
         }
         return true;
      }
      private bool UpdateCanvasShowStats(IGameInstance gi)
      {
         myButtonMapItems.Clear();
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Image img)
            {
               if ("Map" == img.Name)
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
            if (ui is Button b)
            {
               if( false == b.Name.Contains("Die"))
                  elements.Add(ui);
            }
         }
         foreach (UIElement ui1 in elements)
            myCanvas.Children.Remove(ui1);
         //-------------------------------
         myDieRoller.HideDie();
         gi.Statistic.myEndDaysCount++;  // add one to get rid of zero index
         //-------------------------------
         int index = UpdateCanvasShowStatGetIndex(gi.Options);
         string gametype = UpdateCanvasShowStateGetName(index);
         bool isMultipleGameTypesPlayed = UpdateCanvasShowStatsAdds(index, gi.Statistic);
         Settings.Default.GameTypeOriginal = Utilities.Serialize<GameStat>(myGameEngine.Statistics[0]);
         Settings.Default.GameTypeRandParty = Utilities.Serialize<GameStat>(myGameEngine.Statistics[1]);
         Settings.Default.GameTypeRandHex = Utilities.Serialize<GameStat>(myGameEngine.Statistics[2]);
         Settings.Default.GameTypeRand = Utilities.Serialize<GameStat>(myGameEngine.Statistics[3]);
         Settings.Default.GameTypeFun = Utilities.Serialize<GameStat>(myGameEngine.Statistics[4]);
         Settings.Default.GameTypeCustom = Utilities.Serialize<GameStat>(myGameEngine.Statistics[5]);
         Settings.Default.GameTypeTotal = Utilities.Serialize<GameStat>(myGameEngine.Statistics[6]);
         Settings.Default.theGameFeat = Utilities.Serialize<GameFeat>(GameEngine.theFeatsInGame);
         //-------------------------------------------
         Settings.Default.Save();
         //-------------------------------
         myTextBoxMarquee.Inlines.Clear();
         myTextBoxMarquee.Inlines.Add(new Run("Current Game Statistics:") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, TextDecorations = TextDecorations.Underline });
         UpdateCanvasShowStatsText(myTextBoxMarquee, gi.Statistic);
         //-------------------------------
         if ( 1 < myGameEngine.Statistics[index].myNumGames)
         {
            myTextBoxMarquee.Inlines.Add(new LineBreak());
            myTextBoxMarquee.Inlines.Add(new LineBreak());
            string title1 = "All '" + gametype + "' Statistics:";
            myTextBoxMarquee.Inlines.Add(new Run(title1) { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, TextDecorations = TextDecorations.Underline });
            UpdateCanvasShowStatsText(myTextBoxMarquee, myGameEngine.Statistics[index]);
         }
         //-------------------------------
         if( true == isMultipleGameTypesPlayed )
         {
            myTextBoxMarquee.Inlines.Add(new LineBreak());
            myTextBoxMarquee.Inlines.Add(new LineBreak());
            string title2 = "All Games Statistics:";
            myTextBoxMarquee.Inlines.Add(new Run(title2) { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, TextDecorations = TextDecorations.Underline });
            UpdateCanvasShowStatsText(myTextBoxMarquee, myGameEngine.Statistics[6]);
         }
         //-------------------------------
         myCanvas.ClipToBounds = true;
         myCanvas.Children.Add(myTextBoxMarquee);
         myTextBoxMarquee.UpdateLayout();
         //-------------------------------
         DoubleAnimation doubleAnimation = new DoubleAnimation();
         doubleAnimation.From = -myTextBoxMarquee.ActualHeight;
         doubleAnimation.To = myCanvas.ActualHeight;
         doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
         doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(MARQUEE_SCROLL_ANMINATION_TIME));
         Storyboard.SetTargetName(doubleAnimation, "tbMarquee");
         Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Canvas.BottomProperty));
         myStoryboard.Children.Add(doubleAnimation);
         myStoryboard.Begin(this, true);
         return true;
      }
      private int UpdateCanvasShowStatGetIndex(Options options)
      {
         string name = "CustomGame";
         Option option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 5;
         name = "MaxFunGame";
         option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 4;
         name = "RandomGame";
         option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 3;
         name = "RandomHexGame";
         option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 2;
         name = "RandomPartyGame";
         option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 1;
         return 0;
      }
      private string UpdateCanvasShowStateGetName(int index)
      {
         string gameType = "";
         switch (index)
         {
            case 0:
               gameType = "Original Game";
               break;
            case 1:
               gameType = "Random Party Game";
               break;
            case 2:
               gameType = "Random Hex Game";
               break;
            case 3:
               gameType = "All Random Game";
               break;
            case 4:
               gameType = "Maximum Fun Game";
               break;
            case 5:
               gameType = "Custom Game";
               break;
            default:
               gameType = "Total";
               break;
         }
         return gameType;
      }
      private bool UpdateCanvasShowStatsAdds(int index, GameStat stat)
      {
         myGameEngine.Statistics[index].myNumGames++;
         myGameEngine.Statistics[index].myNumWins += stat.myNumWins;
         myGameEngine.Statistics[index].myEndDaysCount += stat.myEndDaysCount;
         myGameEngine.Statistics[index].myEndCoinCount += stat.myEndCoinCount;
         myGameEngine.Statistics[index].myEndFoodCount += stat.myEndFoodCount;
         myGameEngine.Statistics[index].myEndPartyCount += stat.myEndPartyCount;
         myGameEngine.Statistics[index].myDaysLost += stat.myDaysLost;
         myGameEngine.Statistics[index].myNumEncounters += stat.myNumEncounters;
         myGameEngine.Statistics[index].myNumOfRestDays += stat.myNumOfRestDays;
         myGameEngine.Statistics[index].myNumOfAudienceAttempt += stat.myNumOfAudienceAttempt;
         myGameEngine.Statistics[index].myNumOfAudience += stat.myNumOfAudience;
         myGameEngine.Statistics[index].myNumOfOffering += stat.myNumOfOffering;
         myGameEngine.Statistics[index].myDaysInJailorDungeon += stat.myDaysInJailorDungeon;
         myGameEngine.Statistics[index].myNumRiverCrossingSuccess += stat.myNumRiverCrossingSuccess;
         myGameEngine.Statistics[index].myNumRiverCrossingFailure += stat.myNumRiverCrossingFailure;
         myGameEngine.Statistics[index].myNumDaysOnRaft += stat.myNumDaysOnRaft;
         myGameEngine.Statistics[index].myNumDaysAirborne += stat.myNumDaysAirborne;
         myGameEngine.Statistics[index].myNumDaysArchTravel += stat.myNumDaysArchTravel;
         myGameEngine.Statistics[index].myNumOfPartyKilled += stat.myNumOfPartyKilled;
         myGameEngine.Statistics[index].myNumOfPartyHeal += stat.myNumOfPartyHeal;
         myGameEngine.Statistics[index].myNumOfPartyKill += stat.myNumOfPartyKill;
         myGameEngine.Statistics[index].myNumOfPartyKillEndurance += stat.myNumOfPartyKillEndurance;
         myGameEngine.Statistics[index].myNumOfPartyKillCombat += stat.myNumOfPartyKillCombat;
         myGameEngine.Statistics[index].myNumOfPrinceKill += stat.myNumOfPrinceKill;
         myGameEngine.Statistics[index].myNumOfPrinceHeal += stat.myNumOfPrinceHeal;
         myGameEngine.Statistics[index].myNumOfPrinceStarveDays += stat.myNumOfPrinceStarveDays;
         myGameEngine.Statistics[index].myNumOfPrinceUncounscious += stat.myNumOfPrinceUncounscious;
         myGameEngine.Statistics[index].myNumOfPrinceResurrection += stat.myNumOfPrinceResurrection;
         myGameEngine.Statistics[index].myNumOfPrinceAxeDeath += stat.myNumOfPrinceAxeDeath;
         if (myGameEngine.Statistics[index].myMaxPartySize < stat.myMaxPartySize)
            myGameEngine.Statistics[index].myMaxPartySize = stat.myMaxPartySize;
         if (myGameEngine.Statistics[index].myMaxPartyEndurance < stat.myMaxPartyEndurance)
            myGameEngine.Statistics[index].myMaxPartyEndurance = stat.myMaxPartyEndurance;
         if (myGameEngine.Statistics[index].myMaxPartyCombat < stat.myMaxPartyCombat)
            myGameEngine.Statistics[index].myMaxPartyCombat = stat.myMaxPartyCombat;
         //-----------------------------------------
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumGames++;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumWins += stat.myNumWins;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myEndDaysCount += stat.myEndDaysCount;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myEndCoinCount += stat.myEndCoinCount;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myEndFoodCount += stat.myEndFoodCount;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myEndPartyCount += stat.myEndPartyCount;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myDaysLost += stat.myDaysLost;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumEncounters += stat.myNumEncounters;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfRestDays += stat.myNumOfRestDays;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfAudienceAttempt += stat.myNumOfAudienceAttempt;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfAudience += stat.myNumOfAudience;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfOffering += stat.myNumOfOffering;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myDaysInJailorDungeon += stat.myDaysInJailorDungeon;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumRiverCrossingSuccess += stat.myNumRiverCrossingSuccess;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumRiverCrossingFailure += stat.myNumRiverCrossingFailure;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumDaysOnRaft += stat.myNumDaysOnRaft;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumDaysAirborne += stat.myNumDaysAirborne;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumDaysArchTravel += stat.myNumDaysArchTravel;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPartyKilled += stat.myNumOfPartyKilled;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPartyHeal += stat.myNumOfPartyHeal;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPartyKill += stat.myNumOfPartyKill;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPartyKillEndurance += stat.myNumOfPartyKillEndurance;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPartyKillCombat += stat.myNumOfPartyKillCombat;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPrinceKill += stat.myNumOfPrinceKill;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPrinceHeal += stat.myNumOfPrinceHeal;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPrinceStarveDays += stat.myNumOfPrinceStarveDays;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPrinceUncounscious += stat.myNumOfPrinceUncounscious;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPrinceResurrection += stat.myNumOfPrinceResurrection;
         myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myNumOfPrinceAxeDeath += stat.myNumOfPrinceAxeDeath;
         if (myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myMaxPartySize < stat.myMaxPartySize)
            myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myMaxPartySize = stat.myMaxPartySize;
         if (myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myMaxPartyEndurance < stat.myMaxPartyEndurance)
            myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myMaxPartyEndurance = stat.myMaxPartyEndurance;
         if (myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myMaxPartyCombat < stat.myMaxPartyCombat)
            myGameEngine.Statistics[GameEngine.MAX_GAME_TYPE].myMaxPartyCombat = stat.myMaxPartyCombat;
         //-----------------------------------------
         bool isMultipleGameTypesPlayed = false;
         for (int i = 0; i < GameEngine.MAX_GAME_TYPE; ++i)
         {
            if (index == i) // do not look at current game type
               continue;
            if (0 < myGameEngine.Statistics[i].myNumGames)
               isMultipleGameTypesPlayed = true;
         }
         return isMultipleGameTypesPlayed;
      }
      private void UpdateCanvasShowStatsText(TextBlock tb, GameStat stat)
      {
         if( 1 < stat.myNumGames )
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Games = " + stat.myNumGames.ToString()) { FontWeight = FontWeights.Bold });
            int winRatio = (int)Math.Round(100.0 * ((double)stat.myNumWins / (double)stat.myNumGames));
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("% Wins = " + winRatio.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (1 < stat.myNumGames)
         {
            tb.Inlines.Add(new LineBreak());
            int average = stat.myEndDaysCount / stat.myNumGames;
            tb.Inlines.Add(new Run("Average Days = " + average.ToString()) { FontWeight = FontWeights.Bold });
         }
         else
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Days = " + stat.myEndDaysCount.ToString()) { FontWeight = FontWeights.Bold });
         }
         if( 1 < stat.myNumGames )
         {
            tb.Inlines.Add(new LineBreak());
            int average = stat.myEndCoinCount / stat.myNumGames;
            tb.Inlines.Add(new Run("Average Coins = " + average.ToString()) { FontWeight = FontWeights.Bold });
         }
         else
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Coins = " + stat.myEndCoinCount.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (1 < stat.myNumGames)
         {
            tb.Inlines.Add(new LineBreak());
            int average = stat.myEndFoodCount / stat.myNumGames;
            tb.Inlines.Add(new Run("Average Food = " + average.ToString()) { FontWeight = FontWeights.Bold });
         }
         else
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Food = " + stat.myEndFoodCount.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (1 < stat.myNumGames)
         {
            tb.Inlines.Add(new LineBreak());
            int average = stat.myEndPartyCount / stat.myNumGames;
            tb.Inlines.Add(new Run("Average Party Count = " + average.ToString()) { FontWeight = FontWeights.Bold });
         }
         else
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Party Count = " + stat.myEndPartyCount.ToString()) { FontWeight = FontWeights.Bold });
         }
         //-------------------------------------
         if (0 < stat.myMaxPartySize)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Maximum Party Size = " + stat.myMaxPartySize.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myMaxPartyEndurance)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Maximum Party Endurance = " + stat.myMaxPartyEndurance.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myMaxPartyCombat)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Maximum Party Combat = " + stat.myMaxPartyCombat.ToString()) { FontWeight = FontWeights.Bold });
         }
         //-------------------------------------
         if (0 < stat.myDaysLost)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Lost = " + stat.myDaysLost.ToString()) { FontWeight = FontWeights.Bold });
            int percent = (int)Math.Round(100.0 * ((double)stat.myDaysLost / (double)stat.myEndDaysCount));
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("% Lost = " + percent.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumEncounters)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Encounters = " + stat.myNumEncounters.ToString()) { FontWeight = FontWeights.Bold });
            int percent = (int)Math.Round(100.0 * ((double)stat.myNumEncounters / (double)stat.myEndDaysCount));
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("% Encounters = " + percent.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumOfRestDays)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Resting = " + stat.myNumOfRestDays.ToString()) { FontWeight = FontWeights.Bold });
            int percent = (int)Math.Round(100.0 * ((double)stat.myNumOfRestDays / (double)stat.myEndDaysCount));
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("% Resting = " + percent.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumOfAudienceAttempt)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Audiences Attempts = " + stat.myNumOfAudienceAttempt.ToString()) { FontWeight = FontWeights.Bold });
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Audiences = " + stat.myNumOfAudience.ToString()) { FontWeight = FontWeights.Bold });
            int percent = (int)Math.Round(100.0 * ((double)stat.myNumOfAudience / (double)stat.myNumOfAudienceAttempt));
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("% Audience = " + percent.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumOfOffering)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Offerings = " + stat.myNumOfOffering.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myDaysInJailorDungeon)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Jail = " + stat.myDaysInJailorDungeon.ToString()) { FontWeight = FontWeights.Bold });
            int percent = (int)Math.Round(100.0 * ((double)stat.myDaysInJailorDungeon / (double)stat.myEndDaysCount));
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("% Jail = " + percent.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumRiverCrossingSuccess)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Crossings = " + stat.myNumRiverCrossingSuccess.ToString()) { FontWeight = FontWeights.Bold });
            int total = stat.myNumRiverCrossingFailure + stat.myNumRiverCrossingSuccess;
            int percent = (int)Math.Round(100.0 * ((double)stat.myNumRiverCrossingSuccess / (double)total));
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("% Crossing = " + percent.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumDaysOnRaft)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Raft Days = " + stat.myNumDaysOnRaft.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumDaysAirborne)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Airbourne Days = " + stat.myNumDaysAirborne.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumDaysArchTravel)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Arch Days = " + stat.myNumDaysArchTravel.ToString()) { FontWeight = FontWeights.Bold });
         }
         //-------------------------------------
         if (0 < stat.myNumOfPartyKill)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Monsters Killed = " + stat.myNumOfPartyKilled.ToString()) { FontWeight = FontWeights.Bold });
            int average = stat.myEndDaysCount / stat.myNumOfPartyKill;
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Days/Monster Kill= " + average.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumOfPartyKillEndurance)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Killed Endurance = " + stat.myNumOfPartyKillEndurance.ToString()) { FontWeight = FontWeights.Bold });
            if (1 < stat.myNumGames)
            {
               int average = stat.myNumOfPartyKillEndurance / stat.myNumGames;
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Average Killed Endurance = " + average.ToString()) { FontWeight = FontWeights.Bold });
            }
         }
         if (0 < stat.myNumOfPartyKillCombat)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Killed Combat = " + stat.myNumOfPartyKillCombat.ToString()) { FontWeight = FontWeights.Bold });
            if (1 < stat.myNumGames)
            {
               int average = stat.myNumOfPartyKillCombat / stat.myNumGames;
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Average Killed Combat = " + average.ToString()) { FontWeight = FontWeights.Bold });
            }
         }
         //-------------------------------------
         if (0 < stat.myNumOfPartyKilled)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Party Killed = " + stat.myNumOfPartyKill.ToString()) { FontWeight = FontWeights.Bold });
            int average = stat.myEndDaysCount / stat.myNumOfPartyKilled;
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Days/Party Killed = " + average.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumOfPartyHeal)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Healed Wounds = " + stat.myNumOfPartyHeal.ToString()) { FontWeight = FontWeights.Bold });
         }
         //-------------------------------------
         if (0 < stat.myNumOfPrinceHeal)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Prince Endurance Healed = " + stat.myNumOfPrinceHeal.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumOfPrinceKill)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Prince Kills = " + stat.myNumOfPrinceKill.ToString()) { FontWeight = FontWeights.Bold });
            int average = stat.myEndDaysCount / stat.myNumOfPrinceKill;
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Days/Prince Kills = " + average.ToString()) { FontWeight = FontWeights.Bold });
            if (1 < stat.myNumGames)
            {
               int average1 = stat.myNumOfPrinceKill / stat.myNumGames;
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Average Prince Kills = " + average1.ToString()) { FontWeight = FontWeights.Bold });
            }
         }
         if (0 < stat.myNumOfPrinceStarveDays)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Prince Starvation = " + stat.myNumOfPrinceStarveDays.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumOfPrinceUncounscious)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Prince Unconscious = " + stat.myNumOfPrinceUncounscious.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumOfPrinceResurrection)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Prince Resurrections = " + stat.myNumOfPrinceResurrection.ToString()) { FontWeight = FontWeights.Bold });
         }
         if (0 < stat.myNumOfPrinceAxeDeath)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Prince Executions = " + stat.myNumOfPrinceAxeDeath.ToString()) { FontWeight = FontWeights.Bold });
         }
      }
      private bool MoveToNewHexWhenJailed(IGameInstance gi)
      {
         ITerritory oldT = gi.Prince.Territory;
         IStack oldStack = gi.Stacks.Find(oldT);
         if (null == oldStack)
         {
            oldStack = myGameInstance.Stacks.Find(gi.Prince.Territory);
            if (null == oldStack)
               Logger.Log(LogEnum.LE_ERROR, "MoveToNewHexWhenJailed(): oldStack=null for t=" + oldT.ToString() + " bc Prince not found in any stacks");
            else
               Logger.Log(LogEnum.LE_ERROR, "MoveToNewHexWhenJailed(): oldStack=null for t=" + oldT.ToString() + " bc Prince in stack=" + oldStack.Territory.Name);
            if (null == oldStack)
            {
               oldStack = new Stack(gi.Prince.Territory) as IStack;
               myGameInstance.Stacks.Add(oldStack);
            }
            oldStack.MapItems.Add(gi.Prince);
         }
         if (null == gi.NewHex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveToNewHexWhenJailed(): gi.NewHex=null");
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
            mi.Territory = gi.NewHex; // MoveToNewHexWhenJailed()
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
            PathFigure aPathFigure = new PathFigure() { StartPoint = new System.Windows.Point(xStart, yStart) };
            int lastItemIndex = mim.BestPath.Territories.Count - 1;
            for (int i = 0; i < lastItemIndex; i++) // add intermediate movement points - not really used in Barbarian Prince as only move one hex at a time
            {
               ITerritory t = mim.BestPath.Territories[i];
               double x = t.CenterPoint.X - Utilities.theMapItemOffset;
               double y = t.CenterPoint.Y - Utilities.theMapItemOffset;
               System.Windows.Point newPoint = new System.Windows.Point(x, y);
               LineSegment lineSegment = new LineSegment(newPoint, false);
               aPathFigure.Segments.Add(lineSegment);
            }
            // Add the last line segment
            double xEnd = mim.NewTerritory.CenterPoint.X - Utilities.theMapItemOffset;
            double yEnd = mim.NewTerritory.CenterPoint.Y - Utilities.theMapItemOffset;
            if ((Math.Abs(xEnd - xStart) < 2) && (Math.Abs(yEnd - yStart) < 2)) // if already at final location, skip animation or get runtime exception
               return true;
            System.Windows.Point newPoint2 = new System.Windows.Point(xEnd, yEnd);
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
         System.Windows.Point newPoint = new System.Windows.Point(xPostion, yPostion);
         aPointCollection.Add(newPoint);
         foreach (ITerritory t in mim.BestPath.Territories)
         {
            xPostion = t.CenterPoint.X + offset;
            yPostion = t.CenterPoint.Y + offset;
            newPoint = new System.Windows.Point(xPostion, yPostion);
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
         Logger.Log(LogEnum.LE_USER_ACTION, "ClickButtonDailyAction(): >>>>>>>>>>>>>>>>>>>>>>>>>" + s1 + " for ae=" + myGameInstance.EventActive + " c=" + myGameInstance.SunriseChoice.ToString() + " m=" + myGameInstance.Prince.MovementUsed + "/" + myGameInstance.Prince.Movement);
         if (s1 == myButtonDailyContents[0])
         {
            if (true == myGameInstance.IsHeavyRainContinue)
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
         else if (s1 == myButtonDailyContents[13])
         {
            outAction = GameAction.E146StealGems;
         }
         else if (s1 == myButtonDailyContents[14])
         {
            outAction = GameAction.E144RescueHeir;
         }
         else if (s1 == myButtonDailyContents[15])
         {
            outAction = GameAction.E144SneakAttack;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickButtonDailyAction() reached default b.Content=" + s1);
         }
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void ClickButtonMapItem(object sender, RoutedEventArgs e)
      {
         if ((GamePhase.Travel == myGameInstance.GamePhase) && (0 < myGameInstance.Prince.MovementUsed))
         {
            Logger.Log(LogEnum.LE_USER_ACTION, "ClickButtonMapItem(): >>>>>>>>>>>>>>>>>>>>>>>>>ae=" + myGameInstance.EventActive + " c=" + myGameInstance.SunriseChoice.ToString() + " m=" + myGameInstance.Prince.MovementUsed + "/" + myGameInstance.Prince.Movement);
            GameAction outAction = GameAction.TravelEndMovement;
            myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         }
      }
      private void MouseEnterMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
         Button b = (Button)sender;
         if (1 < myGameInstance.PartyMembers.Count)
         {
            myPartyDisplayDialog = new PartyDisplayDialog(myGameInstance, myCanvas, b);
            Logger.Log(LogEnum.LE_VIEW_DIALOG_PARTY, "MouseEnterMapItem(): Showing due to 1 > partyCount=" + myGameInstance.PartyMembers.Count.ToString());
            myPartyDisplayDialog.Show();
         }
      }
      private void MouseLeaveMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (null != myPartyDisplayDialog)
            myPartyDisplayDialog.Close();
         myPartyDisplayDialog = null;
      }
      private void MouseDownPolygonTravel(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         Polygon clickedPolygon = (Polygon)sender;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonTravel(): clickedPolygon=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         myTerritorySelected = Territory.theTerritories.Find(Utilities.RemoveSpaces(clickedPolygon.Tag.ToString()));
         if (null == myTerritorySelected)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonTravel(): selectedTerritory=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         if (false == CreateMapItemMove(Territory.theTerritories, myGameInstance, myTerritorySelected))
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonTravel(): CreateMapItemMove() returned false");
            return;
         }
         GameAction outAction = GameAction.TravelLostCheck;
         if (null != myGameInstance.AirSpiritLocations) // e110c - air spirit moves party
         {
            myGameInstance.GamePhase = GamePhase.Encounter;
            outAction = GameAction.E110AirSpiritTravelEnd;
            myGameInstance.NewHex = myTerritorySelected;   // MouseDownPolygonTravel() - when air spirit moves party
         }
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         Logger.Log(LogEnum.LE_USER_ACTION, "MouseDownPolygonTravel(): >>>>>>>>>>>>>>>>>>>>>>>>>ae=" + myGameInstance.EventActive + " c=" + myGameInstance.SunriseChoice.ToString() + " m=" + myGameInstance.Prince.MovementUsed + "/" + myGameInstance.Prince.Movement + " mim=" + myGameInstance.MapItemMoves[0].ToString());
      }
      private void MouseDownPolygonArchOfTravel(object sender, MouseButtonEventArgs e) // e045
      {
         if (false == myIsTravelThroughGateActive)
            return;
         //-------------------------------------
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
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
            oldStack = new Stack(myGameInstance.Prince.Territory) as IStack;
            myGameInstance.Stacks.Add(oldStack);
         }
         ITerritory newT = Territory.theTerritories.Find(Utilities.RemoveSpaces(clickedPolygon.Tag.ToString()));
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
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         Polygon clickedPolygon = (Polygon)sender;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonLetterChoice(): clickedPolygon=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         ITerritory letterLocation = Territory.theTerritories.Find(Utilities.RemoveSpaces(clickedPolygon.Tag.ToString()));
         if (null == letterLocation)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonLetterChoice(): newT=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         //-------------------------------------
         if ((true == myGameInstance.IsInCastle(letterLocation)) || (true == myGameInstance.IsInTemple(letterLocation)))
         {
            myGameInstance.TargetHex = letterLocation;
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
      private void MouseEnterEllipse(object sender, MouseEventArgs e)
      {
         Ellipse mousedEllipse = (Ellipse)sender;
         if (null == mousedEllipse)
            return;
         foreach (EnteredHex hex in myGameInstance.EnteredHexes)
         {
            string name = (string)mousedEllipse.Tag;
            if (hex.Identifer == name)
            {
               myEllipseDisplayDialog = new EllipseDisplayDialog(hex, myEventViewer.myRulesMgr);
               myEllipseDisplayDialog.Show();
               break;
            }
         }
         //e.Handled = true;  
      }
      private void MouseLeaveEllipse(object sender, MouseEventArgs e)
      {
         Ellipse mousedEllipse = (Ellipse)sender;
         if (null == mousedEllipse)
            return;
         if (null != myEllipseDisplayDialog)
            myEllipseDisplayDialog.Close();
         myEllipseDisplayDialog = null;
         e.Handled = true;
      }
      private void MouseLeftButtonDownMarquee(object sender, MouseEventArgs e)
      {
         myStoryboard.Pause(this);
      }
      private void MouseLeftButtonUpMarquee(object send, MouseEventArgs e)
      {
         myStoryboard.Resume(this);
      }
      private void MouseRightButtonDownMarquee(object send, MouseEventArgs e)
      {
         if ((0.5 < mySpeedRatioMarquee) && (mySpeedRatioMarquee < 2.0))
            mySpeedRatioMarquee = 2.0;
         else if (1.5 < mySpeedRatioMarquee)
            mySpeedRatioMarquee = 0.5;
         else
            mySpeedRatioMarquee = 1.0;
         myStoryboard.SetSpeedRatio(this, mySpeedRatioMarquee);
      }
      //-------------GameViewerWindow---------------------------------
      private void ContentRenderedGameViewerWindow(object sender, EventArgs e)
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
      private void SizeChangedGameViewerWindow(object sender, SizeChangedEventArgs e)
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
      private void ClosedGameViewerWindow(object sender, EventArgs e)
      {
         Application app = Application.Current;
         app.Shutdown();
      }
      protected override void OnSourceInitialized(EventArgs e)
      {
         base.OnSourceInitialized(e);
         try
         {
            // Load window placement details for previous application session from application settings
            // Note - if window was closed on a monitor that is now disconnected from the computer,
            //        SetWindowPlacement places the window onto a visible monitor.
            if (false == String.IsNullOrEmpty(Settings.Default.WindowPlacement))
            {
               WindowPlacement wp = Utilities.Deserialize<WindowPlacement>(Settings.Default.WindowPlacement);
               wp.length = Marshal.SizeOf(typeof(WindowPlacement));
               wp.flags = 0;
               wp.showCmd = (wp.showCmd == SwShowminimized ? SwShownormal : wp.showCmd);
               var hwnd = new WindowInteropHelper(this).Handle;
               if (false == SetWindowPlacement(hwnd, ref wp))
                  Logger.Log(LogEnum.LE_ERROR, "SetWindowPlacement() returned false");
            }
            if (0.0 != Settings.Default.ScrollViewerHeight)
               myScollViewerInside.Height = Settings.Default.ScrollViewerHeight;
            if (0.0 != Settings.Default.ScrollViewerWidth)
               myScollViewerInside.Width = Settings.Default.ScrollViewerWidth;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "OnSourceInitialized() e=" + ex.ToString());
         }
         return;
      }
      protected override void OnClosing(CancelEventArgs e) //  // WARNING - Not fired when Application.SessionEnding is fired
      {
         base.OnClosing(e);
         WindowPlacement wp; // Persist window placement details to application settings
         var hwnd = new WindowInteropHelper(this).Handle;
         if (false == GetWindowPlacement(hwnd, out wp))
            Logger.Log(LogEnum.LE_ERROR, "OnClosing(): GetWindowPlacement() returned false");
         string sWinPlace = Utilities.Serialize<WindowPlacement>(wp);
         Settings.Default.WindowPlacement = sWinPlace;
         //-------------------------------------------
         Settings.Default.ZoomCanvas = Utilities.ZoomCanvas;
         //-------------------------------------------
         Settings.Default.ScrollViewerHeight = myScollViewerInside.Height;
         Settings.Default.ScrollViewerWidth = myScollViewerInside.Width;
         //-------------------------------------------
         Settings.Default.GameDirectoryName = Settings.Default.GameDirectoryName;
         //-------------------------------------------
         string sOptions = Utilities.Serialize<Options>(myGameInstance.Options);
         Settings.Default.GameOptions = sOptions;
         //-------------------------------------------
         Settings.Default.GameTypeOriginal = Utilities.Serialize<GameStat>(myGameEngine.Statistics[0]);
         Settings.Default.GameTypeRandParty = Utilities.Serialize<GameStat>(myGameEngine.Statistics[1]);
         Settings.Default.GameTypeRandHex = Utilities.Serialize<GameStat>(myGameEngine.Statistics[2]);
         Settings.Default.GameTypeRand = Utilities.Serialize<GameStat>(myGameEngine.Statistics[3]);
         Settings.Default.GameTypeFun = Utilities.Serialize<GameStat>(myGameEngine.Statistics[4]);
         Settings.Default.GameTypeCustom = Utilities.Serialize<GameStat>(myGameEngine.Statistics[5]);
         Settings.Default.GameTypeTotal = Utilities.Serialize<GameStat>(myGameEngine.Statistics[6]);
         Settings.Default.theGameFeat = Utilities.Serialize<GameFeat>(GameEngine.theFeatsInGame);
         //-------------------------------------------
         Settings.Default.Save();
         //-------------------------------------------
         if (false == GameLoadMgr.SaveGameToFile(myGameInstance))
            Logger.Log(LogEnum.LE_ERROR, "OnClosing(): SaveGameToFile() returned false");
      }
      //-------------CONTROLLER HELPER FUNCTIONS---------------------------------
      private bool CreateMapItemMove(ITerritories territories, IGameInstance gi, ITerritory newTerritory)
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
         gi.MapItemMoves.Insert(0, mim); // insert at front of line
         return true;
      }
      private bool AddHotKeys(MainMenuViewer mmv)
      {
         try
         {
            RoutedCommand command = new RoutedCommand();
            KeyGesture keyGesture = new KeyGesture(Key.N, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemNew_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.O, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemFileOpen_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.C, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemClose_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.S, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemSaveAs_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.U, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBinding undoCmdBinding = new CommandBinding(command, mmv.MenuItemEditRecover_Click, mmv.MenuItemEditRecover_ClickCanExecute);
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemEditUndo_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.R, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBinding recoverCmdBinding = new CommandBinding(command, mmv.MenuItemEditRecover_Click, mmv.MenuItemEditRecover_ClickCanExecute);
            CommandBindings.Add(recoverCmdBinding);
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemFileOptions_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.P, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemViewPath_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemViewRivers_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.I, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemViewInventory_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F1, ModifierKeys.None);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpRules_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F2, ModifierKeys.None);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpEvents_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F3, ModifierKeys.None);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpIcons_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F4, ModifierKeys.None);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpCharacters_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.A, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpAbout_Click));
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddHotKeys(): ex=" + ex.ToString());
            return false;
         }
         return true;
      }
      //-----------------------------------------------------------------------
      #region Win32 API declarations to set and get window placement
      [DllImport("user32.dll")]
      private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);
      [DllImport("user32.dll")]
      private static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);
      private const int SwShownormal = 1;
      private const int SwShowminimized = 2;
      #endregion
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
