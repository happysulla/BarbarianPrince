using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class EventViewerE044HighAltarMgr : UserControl
   {
      public delegate bool EndE044Callback(bool isPrinceBlessed, bool isArtifactFound);
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
         }
      };
      public enum E044Enum
      {
         INVOCATION_CHECK,
         ENGULFED_IN_FLAMES,
         LIGHTENING_STRIKES,
         INVOKER_WOUND,
         VOICES_OF_GOD_RIDDLE,
         VOICES_OF_GOD_TREASURE,
         THUNDERBOLTS,
         SHOW_RESULTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      private bool myIsPrinceBlessed = false;
      private bool myIsClueFound = false;
      //---------------------------------------------
      private E044Enum myState = E044Enum.INVOCATION_CHECK;
      private EndE044Callback myCallback = null;
      private int myMaxRowCount = 0;
      private GridRow[] myGridRows = null;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResulltRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerE044HighAltarMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE044HighAltarMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE044HighAltarMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE044HighAltarMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE044HighAltarMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE044HighAltarMgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool CheckInvocation(EndE044Callback callback)
      {
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckInvocation(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckInvocation(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E044Enum.INVOCATION_CHECK;
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myIsPrinceBlessed = false;
         myIsClueFound = false;
         myIsRollInProgress = false;
         myRollResulltRowNum = 0;
         myCallback = callback;
         //--------------------------------------------------
         int i = 0;
         IMapItem prince = null;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "CheckInvocation(): mi=null");
               return false;
            }
            if ("Prince" == mi.Name)
               prince = mi;
            myGridRows[i] = new GridRow(mi);
            ++i;
         }
         if (null == prince)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckInvocation(): prince=null");
            return false;
         }
         if (false == myGameInstance.IsReligionInParty())
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckInvocation(): myGameInstance.IsReligionInParty()=false");
            return false;
         }
         //--------------------------------------------------
         // Add the unassignable mapitems that never move or change to the Grid Rows
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckInvocation(): UpdateGrid() return false");
            return false;
         }
         myScrollViewer.Content = myGrid;
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateEndState() returned false");
            return false;
         }
         if (E044Enum.END == myState)
            return true;
         if (false == UpdateUserInstructions())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
            return false;
         }
         if (false == UpdateAssignablePanel())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
            return false;
         }
         if (false == UpdateGridRows())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
            return false;
         }
         return true;
      }
      private bool UpdateEndState()
      {
         if (E044Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            if (false == myCallback(myIsPrinceBlessed, myIsClueFound))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateUserInstructions()
      {
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case E044Enum.INVOCATION_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Select one religious person to perform invocation or click campfire to skip."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               break;
            case E044Enum.ENGULFED_IN_FLAMES:
               myTextBlockInstructions.Inlines.Add(new Run("Invoker is engulfed in godly flames. Click campfire to continue."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               break;
            case E044Enum.LIGHTENING_STRIKES:
               myTextBlockInstructions.Inlines.Add(new Run("Lighting and earthquakes destroy the high altar resulting in flying stones and fire."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Roll die for each member to suffer that many wounds."));
               break;
            case E044Enum.INVOKER_WOUND:
               myTextBlockInstructions.Inlines.Add(new Run("Monk or priest struck with one wound. Click campfire to continue."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               break;
            case E044Enum.VOICES_OF_GOD_RIDDLE:
               myTextBlockInstructions.Inlines.Add(new Run("Voice of the gods sound forth and gives you an impossible riddle."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue. Stupid!"));
               break;
            case E044Enum.VOICES_OF_GOD_TREASURE:
               myTextBlockInstructions.Inlines.Add(new Run("Voices of the gods gives a prophecy that seems to promise a treasure."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Click treasure chest to determine location."));
               break;
            case E044Enum.THUNDERBOLTS:
               myTextBlockInstructions.Inlines.Add(new Run("One of the gods appears issuing thunderbolts and flames"));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("pledging support to your cause.You will regain your throne and"));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("win the game if you can return alive to the Northlands."));
               break;
            case E044Enum.SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue"));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E044Enum.INVOCATION_CHECK:
            case E044Enum.ENGULFED_IN_FLAMES:
            case E044Enum.INVOKER_WOUND:
            case E044Enum.VOICES_OF_GOD_RIDDLE:
            case E044Enum.THUNDERBOLTS:
            case E044Enum.SHOW_RESULTS:
               BitmapImage bmi6 = new BitmapImage();
               bmi6.BeginInit();
               bmi6.UriSource = new Uri(MapImage.theImageDirectory + "CampFire2.gif", UriKind.Absolute);
               bmi6.EndInit();
               Image img6 = new Image { Tag = "Campfire", Source = bmi6, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img6, bmi6);
               myStackPanelAssignable.Children.Add(img6);
               //-------------------------------------------
               Rectangle r9 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Width = Utilities.ZOOM * Utilities.theMapItemOffset,
                  Height = Utilities.ZOOM * Utilities.theMapItemOffset
               };
               myStackPanelAssignable.Children.Add(r9);
               break;
            case E044Enum.LIGHTENING_STRIKES:
               Rectangle r8 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myStackPanelAssignable.Children.Add(r8);
               break;
            case E044Enum.VOICES_OF_GOD_TREASURE:
               BitmapImage bmi5 = new BitmapImage();
               bmi5.BeginInit();
               bmi5.UriSource = new Uri(MapImage.theImageDirectory + "Chest.gif", UriKind.Absolute);
               bmi5.EndInit();
               Image img5 = new Image { Tag = "Chest", Source = bmi5, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img5, bmi5);
               myStackPanelAssignable.Children.Add(img5);
               //-------------------------------------------
               Rectangle r5 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Width = Utilities.ZOOM * Utilities.theMapItemOffset,
                  Height = Utilities.ZOOM * Utilities.theMapItemOffset
               };
               myStackPanelAssignable.Children.Add(r5);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRows()
      {
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //------------------------------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            //------------------------------------
            switch (myState)
            {
               case E044Enum.INVOCATION_CHECK:
                  Button b = CreateButton(mi);
                  myGrid.Children.Add(b);
                  Grid.SetRow(b, rowNum);
                  Grid.SetColumn(b, 0);
                  bool isReligious = ((mi.Name.Contains("Monk") || mi.Name.Contains("Priest")) && (false == mi.IsUnconscious));
                  if (true == isReligious)
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
                     bmi.EndInit();
                     Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset, Name = "DieRoll" };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 1);
                  }
                  break;
               case E044Enum.ENGULFED_IN_FLAMES:
               case E044Enum.INVOKER_WOUND:
                  Button b1 = CreateButton(mi);
                  myGrid.Children.Add(b1);
                  Grid.SetRow(b1, rowNum);
                  Grid.SetColumn(b1, 0);
                  if (Utilities.NO_RESULT < myGridRows[i].myDieRoll)
                  {
                     Label labelWound = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRoll.ToString() };
                     myGrid.Children.Add(labelWound);
                     Grid.SetRow(labelWound, rowNum);
                     Grid.SetColumn(labelWound, 1);
                  }
                  break;
               case E044Enum.LIGHTENING_STRIKES:
               case E044Enum.SHOW_RESULTS:
                  Button b2 = CreateButton(mi);
                  myGrid.Children.Add(b2);
                  Grid.SetRow(b2, rowNum);
                  Grid.SetColumn(b2, 0);
                  if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
                     bmi.EndInit();
                     Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset, Name = "DieRoll" };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 1);
                  }
                  else
                  {
                     Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRoll.ToString() };
                     myGrid.Children.Add(label);
                     Grid.SetRow(label, rowNum);
                     Grid.SetColumn(label, 1);
                  }
                  break;
               case E044Enum.VOICES_OF_GOD_RIDDLE:
               case E044Enum.VOICES_OF_GOD_TREASURE:
                  break;
               case E044Enum.THUNDERBOLTS:
                  if (STARTING_ASSIGNED_ROW == rowNum)
                  {
                     myTextBlock1.Visibility = Visibility.Hidden;
                     myTextBlock2.Visibility = Visibility.Hidden;
                     myColDef1.Width = new GridLength(2);
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri(MapImage.theImageDirectory + "LightningScene.gif", UriKind.Absolute);
                     bmi.EndInit();
                     Image img = new Image { Source = bmi, Width = 400, Height = 400, HorizontalAlignment = HorizontalAlignment.Left, Name = "GodBlessed" };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 1);
                  }
                  break;
               case E044Enum.END:
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default s=" + myState.ToString());
                  break;
            }
         }
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(1);
         b.BorderBrush = Brushes.Black;
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, mi, false, true); // This sets the image as the button's content
         return b;
      }
      public void ShowDieResults(int dieRoll)
      {
         int i = myRollResulltRowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         if (E044Enum.INVOCATION_CHECK == myState)
         {
            switch (dieRoll)
            {
               case 1:
                  myState = E044Enum.ENGULFED_IN_FLAMES;
                  mi.OverlayImageName = "EngulfedInFlames";
                  mi.IsKilled = true;
                  myGridRows[i].myDieRoll = dieRoll;
                  break;
               case 2: myState = E044Enum.LIGHTENING_STRIKES; break;
               case 3:
                  myState = E044Enum.INVOKER_WOUND;
                  mi.SetWounds(1, 0);
                  myGridRows[i].myDieRoll = dieRoll;
                  break;
               case 4: myState = E044Enum.VOICES_OF_GOD_RIDDLE; break;
               case 5:
                  myState = E044Enum.VOICES_OF_GOD_TREASURE;
                  myIsClueFound = true;
                  myGridRows[i].myDieRoll = dieRoll;
                  break;
               case 6:
                  myState = E044Enum.THUNDERBOLTS;
                  myIsPrinceBlessed = true;
                  myGridRows[i].myDieRoll = dieRoll;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default" + myState.ToString());
                  return;
            }
         }
         else if (E044Enum.LIGHTENING_STRIKES == myState)
         {
            myGridRows[i].myDieRoll = dieRoll;
            mi.SetWounds(dieRoll, 0);
            myState = E044Enum.SHOW_RESULTS;
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
                  myState = E044Enum.LIGHTENING_STRIKES;
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img)
                     {
                        string name = (string)img.Tag;
                        if ("Campfire" == name)
                           myState = E044Enum.END;
                        if ("Chest" == name)
                           myState = E044Enum.END;
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
               }
            }
            if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if ("GodBlessed" == img1.Name)
                  {
                     if (false == UpdateGrid())
                        Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                     return;
                  }
                  else if (false == myIsRollInProgress)
                  {
                     myRollResulltRowNum = Grid.GetRow(img1);
                     myIsRollInProgress = true;
                     RollEndCallback callback = ShowDieResults;
                     myDieRoller.RollMovingDie(myCanvas, callback);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
   }
}
