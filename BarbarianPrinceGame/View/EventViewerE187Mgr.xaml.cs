using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class EventViewerE187Mgr : UserControl
   {
      public delegate bool EndE187Callback();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int NO_ROLL_REQUIRED = 10;
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
      public enum E187Enum
      {
         CHECK_DESTRUCTION,
         SHOW_RESULTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private E187Enum myState = E187Enum.CHECK_DESTRUCTION;
      private EndE187Callback myCallback = null;
      private int myMaxRowCount = 0;
      private GridRow[] myGridRows = null;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //-----------------------------------------------------------------------------------------
      public EventViewerE187Mgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE187Mgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE187Mgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE187Mgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE187Mgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE187Mgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool CheckAntipoisonAmuletDestruction(EndE187Callback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckAntipoisonAmuletDestruction(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckAntipoisonAmuletDestruction(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E187Enum.CHECK_DESTRUCTION;
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myCallback = callback;
         //--------------------------------------------------
         int i = 0;
         bool isAmuletHeld = false;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "CheckAntipoisonAmuletDestruction(): mi=null");
               return false;
            }
            myGridRows[i] = new GridRow(mi);
            if (true == mi.IsSpecialItemHeld(SpecialEnum.AntiPoisonAmulet))
               isAmuletHeld = true;
            else
               myGridRows[i].myDieRoll = NO_ROLL_REQUIRED;
            ++i;
         }
         if (false == isAmuletHeld)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckAntipoisonAmuletDestruction(): nobody has the antipoison amulet & gi.IsHeld=" + myGameInstance.IsSpecialItemHeld(SpecialEnum.AntiPoisonAmulet));
            return false;
         }
         //--------------------------------------------------
         // Add the unassignable mapitems that never move or change to the Grid Rows
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckAntipoisonAmuletDestruction(): UpdateGrid() return false");
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
         if (E187Enum.END == myState)
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
         if (E187Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            if (false == myCallback())
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
            case E187Enum.CHECK_DESTRUCTION:
               myTextBlockInstructions.Inlines.Add(new Run("Possible destruction of amulet. Roll die."));
               break;
            case E187Enum.SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue"));
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
            case E187Enum.CHECK_DESTRUCTION:
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = " Roll=12 destroys " };
               myStackPanelAssignable.Children.Add(label);
               Image img5 = new Image { Tag = "AmuletAntiPoison", Source = MapItem.theMapImages.GetBitmapImage("AmuletAntiPoison"), Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(img5);
               break;
            case E187Enum.SHOW_RESULTS:
               BitmapImage bmi1 = new BitmapImage();
               bmi1.BeginInit();
               bmi1.UriSource = new Uri(MapImage.theImageDirectory + "CampFire2.gif", UriKind.Absolute);
               bmi1.EndInit();
               Image img1 = new Image { Tag = "Campfire", Source = bmi1, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img1, bmi1);
               myStackPanelAssignable.Children.Add(img1);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         myStackPanelAssignable.Children.Add(r0);
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
            if (NO_ROLL_REQUIRED == myGridRows[i].myDieRoll)
               continue;
            //--------------------------------
            if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
               bmi.EndInit();
               Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
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
               if (12 != myGridRows[i].myDieRoll)
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("AmuletAntiPoison"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img1);
                  Grid.SetRow(img1, rowNum);
                  Grid.SetColumn(img1, 2);
               }
               else
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Deny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img1);
                  Grid.SetRow(img1, rowNum);
                  Grid.SetColumn(img1, 2);
               }
            }
            //------------------------------------
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
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
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
            return;
         }
         IMapItem mi = myGridRows[i].myMapItem;
         myGridRows[i].myDieRoll = dieRoll;
         if (12 == myGridRows[i].myDieRoll)
         {
            if (false == mi.RemoveSpecialItem(SpecialEnum.AntiPoisonAmulet))
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): RemoveSpecialItem()= false");
         }
         mi.IsAntipoisonAmuletUsed = false;
         //------------------------------
         myState = E187Enum.SHOW_RESULTS;
         for(int j=0; j < myMaxRowCount; j++) 
         {
            IMapItem mapItem = myGridRows[j].myMapItem;
            if (true == mapItem.IsAntipoisonAmuletUsed)
            {
               myState = E187Enum.CHECK_DESTRUCTION;
               break;
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (E187Enum.SHOW_RESULTS == myState)
         {
            myState = E187Enum.END;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img0) // Check all images within the myStackPanelAssignable
                  {
                  }
               }
            }
            if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if (false == myIsRollInProgress)
                  {
                     myRollResultRowNum = Grid.GetRow(img1);  // select the row number of the opener
                     myIsRollInProgress = true;
                     RollEndCallback callback = ShowDieResults;
                     myDieRoller.RollMovingDice(myCanvas, callback);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         String content = (String)b.Content;
         if (null == myRulesMgr)
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
         else if (false == myRulesMgr.ShowRule(content))
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false for c=" + content);
      }
   }
}
