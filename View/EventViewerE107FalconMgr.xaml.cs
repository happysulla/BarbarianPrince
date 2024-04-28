using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;
using static BarbarianPrince.EventViewerE031Mgr;

namespace BarbarianPrince
{
   public partial class EventViewerE107FalconMgr : UserControl
   {
      public delegate bool EndFalconCheck();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int FALCON_NOT_FED = 10;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public bool myIsFed;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myIsFed = false;
            myDieRoll = Utilities.NO_RESULT;
         }
      };
      public enum E107Enum
      {
         FALCON_CHECK,
         SHOW_RESULTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private E107Enum myState = E107Enum.FALCON_CHECK;
      private EndFalconCheck myCallback = null;
      private int myMaxRowCount = 0;
      private GridRow[] myGridRows = null;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myDieRollRowNum = 0;
      private bool myIsRollInProgress = false;
      private int myFoodOriginal = 0;
      private int myFoodCurrent = 0;
      //---------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");

      public EventViewerE107FalconMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool FalconLeaveCheck(EndFalconCheck callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "FalconLeaveCheck(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "FalconLeaveCheck(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E107Enum.FALCON_CHECK;
         myIsRollInProgress = false;
         myDieRollRowNum = 0;
         myCallback = callback;
         //--------------------------------------------------
         myFoodCurrent = myGameInstance.GetFoods();
         myFoodOriginal = myFoodCurrent;
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "FalconLeaveCheck(): mi=null");
               return false;
            }
            if( true == mi.Name.Contains("Falcon"))
            {
               myGridRows[i] = new GridRow(mi);
               if( 0 < myFoodCurrent )
               {
                  --myFoodCurrent;
                  myGridRows[i].myIsFed = true;
               }
               else
               {
                  myGridRows[i].myDieRoll = FALCON_NOT_FED;
                  myGridRows[i].myIsFed = false;
               }
               ++i;
            }
         }
         myMaxRowCount = i;
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "FalconLeaveCheck(): UpdateGrid() return false");
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
         if (E107Enum.END == myState)
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
         if (E107Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            for( int i=0; i<myMaxRowCount; ++i ) // If any falcon exists that is fed and stays, there is a falcon in the party tommorrow
            {
               IMapItem falcon = myGridRows[i].myMapItem;
               if ( (6 == myGridRows[i].myDieRoll) || (FALCON_NOT_FED == myGridRows[i].myDieRoll) )
                  myGameInstance.RemoveAbandonerInParty(falcon);
               else
                  myGameInstance.IsFalconFed = true;
            }
            int diff = myFoodOriginal - myFoodCurrent; // any food needs to be removed from party
            if (0 < diff)
               myGameInstance.ReduceFoods(diff);
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
         //---------------------------------
         bool isAnyDieRollNeeded = false;
         for (int i = 0; i < myMaxRowCount; i++)
         {
            if ((true == myGridRows[i].myIsFed) && (Utilities.NO_RESULT == myGridRows[i].myDieRoll))
               isAnyDieRollNeeded = true;
         }
         //---------------------------------
         switch (myState)
         {
            case E107Enum.FALCON_CHECK:
               if( true == isAnyDieRollNeeded )
               {
                  if (1 < myMaxRowCount)
                     myTextBlockInstructions.Inlines.Add(new Run("Roll for falcons leaving."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Roll for falcon leaving."));
               }
               else
               {
                  myTextBlockInstructions.Inlines.Add(new Run(" Click campfire to continue or check box to feed"));
               }
               break;
            case E107Enum.SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue."));
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
         Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         BitmapImage bmi7 = new BitmapImage();
         bmi7.BeginInit();
         bmi7.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
         bmi7.EndInit();
         Image img7 = new Image { Name = "Campfire", Source = bmi7, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         ImageBehavior.SetAnimatedSource(img7, bmi7);
         //---------------------------------
         bool isAnyDieRollNeeded = false;
         for(int i=0; i<myMaxRowCount; i++)
         {
            if ( (true == myGridRows[i].myIsFed) && (Utilities.NO_RESULT == myGridRows[i].myDieRoll) )
               isAnyDieRollNeeded = true;
         }
         //---------------------------------
         switch (myState)
         {
            case E107Enum.FALCON_CHECK:
               if( true == isAnyDieRollNeeded )
                  myStackPanelAssignable.Children.Add(r0); // do not show campfire if need to make a roll
               else
                  myStackPanelAssignable.Children.Add(img7);
               break;
            case E107Enum.SHOW_RESULTS:
               myStackPanelAssignable.Children.Add(img7);
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
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //------------------------------------
            CheckBox cb = new CheckBox() { FontSize = 12, IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            cb.IsChecked = myGridRows[i].myIsFed;
            myGrid.Children.Add(cb);
            Grid.SetRow(cb, rowNum);
            Grid.SetColumn(cb, 1);
            //--------------------------------
            bool isDieRolled = (Utilities.NO_RESULT < myGridRows[i].myDieRoll) && (myGridRows[i].myDieRoll < FALCON_NOT_FED);
            if (true == myGridRows[i].myIsFed)
            {
               if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
               {
                  cb.IsEnabled = true; // can choose to not feed falcon
                  cb.Unchecked += CheckBoxFeed_Unchecked;
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 2);
               }
               else
               {
                  string dieResult = myGridRows[i].myDieRoll.ToString();
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieResult };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, 2);
                  //-------------------------------
                  string falconLeaveResult = "no";
                  if (6 == myGridRows[i].myDieRoll)
                     falconLeaveResult = "yes";
                  Label labelResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = falconLeaveResult };
                  myGrid.Children.Add(labelResult);
                  Grid.SetRow(labelResult, rowNum);
                  Grid.SetColumn(labelResult, 3);
               }
            }
            else
            {
               if (FALCON_NOT_FED == myGridRows[i].myDieRoll) 
               {
                  if (0 < myFoodCurrent) // if have food, can feed the falcon
                  {
                     cb.IsEnabled = true;
                     cb.Checked += CheckBoxFeed_Checked;
                  }
               }
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 2);
               //-------------------------------
               Label labelResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "yes" };
               myGrid.Children.Add(labelResult);
               Grid.SetRow(labelResult, rowNum);
               Grid.SetColumn(labelResult, 3);
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
         int i = myDieRollRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
            return;
         }
         myGridRows[i].myDieRoll = dieRoll;
         myState = E107Enum.SHOW_RESULTS;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            IMapItem mi1 = myGridRows[j].myMapItem;
            if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
               myState = E107Enum.FALCON_CHECK;
         }
         //-----------------------------------------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (E107Enum.SHOW_RESULTS == myState)
         {
            myState = E107Enum.END;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img0) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img0)
                     {
                        if ("Campfire" == img0.Name)
                        {
                           myState = E107Enum.END;
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                     }
                  }
               }
            }
            if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if (false == myIsRollInProgress)
                  {
                     myDieRollRowNum = Grid.GetRow(img1);  // select the row number of the opener
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
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         String content = (String)b.Content;
         if (null == myRulesMgr)
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
         else if (false == myRulesMgr.ShowRule(content))
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false for c=" + content);
      }
      private void CheckBoxFeed_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFeed_Checked(): STARTING_ASSIGNED_ROW > rowNum=" + rowNum.ToString());
            return;
         }
         cb.IsChecked = true;
         int i = rowNum - STARTING_ASSIGNED_ROW;
         myGridRows[i].myIsFed = true;
         myFoodCurrent -= 1;
         //------------------------------------
         myGridRows[i].myDieRoll = Utilities.NO_RESULT;
         myState = E107Enum.SHOW_RESULTS;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            IMapItem mi1 = myGridRows[j].myMapItem;
            if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
               myState = E107Enum.FALCON_CHECK;
         }
         //------------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFeed_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxFeed_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFeed_Unchecked(): STARTING_ASSIGNED_ROW > rowNum=" + rowNum.ToString());
            return;
         }
         cb.IsChecked = false;
         int i = rowNum - STARTING_ASSIGNED_ROW;
         myGridRows[i].myIsFed = false;
         myFoodCurrent += 1;
         //------------------------------------
         myGridRows[i].myDieRoll = FALCON_NOT_FED;
         myState = E107Enum.SHOW_RESULTS;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            IMapItem mi1 = myGridRows[j].myMapItem;
            if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
               myState = E107Enum.FALCON_CHECK;
         }
         //------------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxDouble_Unchecked(): UpdateGrid() return false");
      }
   }
}
