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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace BarbarianPrince
{
   public partial class EventViewerE079HeavyRainMgr : UserControl
   {
      public delegate bool EndCatchColdCheck(bool isAnyLost);
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int COLD_ALREADY = 10;
      private const int NO_MOUNT = 11;
      private const int NOT_RIDING = 12;
      private const int GRIFFON_MOUNT = 13; // Griffon mounts do not die
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public int myDieRollMount;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
            myDieRollMount = Utilities.NO_RESULT;
         }
      };
      public enum E079Enum
      {
         COLD_CHECK,
         SHOW_RESULTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private E079Enum myState = E079Enum.COLD_CHECK;
      private EndCatchColdCheck myCallback = null;
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
      private int myRollResultColNum = -1;
      private bool myIsRollInProgress = false;
      private bool myIsAnyLost = false;
      //---------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //-----------------------------------------------------------------------------------------
      public EventViewerE079HeavyRainMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
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
      public bool ColdCheck(EndCatchColdCheck callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "CurseCheck(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "CurseCheck(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myRollResultColNum = -1;
         myCallback = callback;
         myIsAnyLost = false;
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "CurseCheck(): mi=null");
               return false;
            }
            myGridRows[i] = new GridRow(mi);
            if (true == mi.IsCatchCold)
               myGridRows[i].myDieRoll = COLD_ALREADY;
            if (false == mi.IsRiding)
               myGridRows[i].myDieRollMount = NOT_RIDING;
            if (0 == mi.Mounts.Count)
            {
               myGridRows[i].myDieRollMount = NO_MOUNT;
            }
            else
            {
               IMapItem mount = mi.Mounts[0];
               if( true == mount.Name.Contains("Griffon") ) 
                  myGridRows[i].myDieRollMount = GRIFFON_MOUNT;
            }

            ++i;
         }
         //--------------------------------------------------
         myState = E079Enum.SHOW_RESULTS;
         for( int k=0; k< myMaxRowCount; ++k)
         {
            if( Utilities.NO_RESULT == myGridRows[k].myDieRoll)
               myState = E079Enum.COLD_CHECK;
            if (Utilities.NO_RESULT == myGridRows[k].myDieRollMount)
               myState = E079Enum.COLD_CHECK;
         }
         //--------------------------------------------------
         // Add the unassignable mapitems that never move or change to the Grid Rows
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "CurseCheck(): UpdateGrid() return false");
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
         if (E079Enum.END == myState)
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
         if (E079Enum.END == myState)
         {
            myGameInstance.IsHeavyRainContinue = false;        // e079
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            myGameInstance.RemoveKilledInParty("Caught your death of cold"); 
            if (false == myCallback(myIsAnyLost))
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
            case E079Enum.COLD_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Click die to roll for catching cold. Colds add one wound."));
               break;
            case E079Enum.SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
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
            case E079Enum.COLD_CHECK:
               Image img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("HeavyRainSick"), Width = 0.75*Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img3);
               Label labelForSickResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               labelForSickResult.Content = " if roll > 4 ";
               myStackPanelAssignable.Children.Add(labelForSickResult);
               if( true == myGameInstance.IsHeavyRainContinue )
               {
                  Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = 3* Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r0);
                  Image img4 = new Image { Source = MapItem.theMapImages.GetBitmapImage("MountDeny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img4);
                  Label labelForDeadHorse = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                  labelForDeadHorse.Content = " if roll > 4 ";
                  myStackPanelAssignable.Children.Add(labelForDeadHorse);
               }
               break;
            case E079Enum.SHOW_RESULTS:
               if( true == myGameInstance.IsHeavyRainNextDay)
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img1);
               }
               else
               {
                  BitmapImage bmi1 = new BitmapImage();
                  bmi1.BeginInit();
                  bmi1.UriSource = new Uri("../../Images/Campfire2.gif", UriKind.Relative);
                  bmi1.EndInit();
                  Image img1 = new Image { Name = "Campfire", Source = bmi1, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img1, bmi1);
                  myStackPanelAssignable.Children.Add(img1);
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRows()
      {
         if( false == myGameInstance.IsHeavyRainContinue )
         {
            myTextBlock3.Visibility = Visibility.Hidden;
            myTextBlock4.Visibility = Visibility.Hidden;
            myTextBlock5.Visibility = Visibility.Hidden;
         }
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
            //--------------------------------
            if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi.EndInit();
               Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 1);
            }
            else
            {
               string dieRollLabel = myGridRows[i].myDieRoll.ToString();
               if (COLD_ALREADY == myGridRows[i].myDieRoll)
                  dieRollLabel = "NA";
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollLabel };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 1);
               //-------------------------------
               Label labelResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               myGrid.Children.Add(labelResult);
               Grid.SetRow(labelResult, rowNum);
               Grid.SetColumn(labelResult, 2);
               if ((4 < myGridRows[i].myDieRoll) || (true == mi.IsCatchCold) ) // can already have a cold at beginning
                  labelResult.Content = "yes";
               else
                  labelResult.Content = "no";
            }
            //----------------------------------------------------
            if (true == myGameInstance.IsHeavyRainContinue) // if HeavyRains continue, the mounts may die
            {
               if (0 < mi.Mounts.Count)
               {
                  Button b1 = CreateButton(mi.Mounts[0]);
                  myGrid.Children.Add(b1);
                  Grid.SetRow(b1, rowNum);
                  Grid.SetColumn(b1, 3);
               }
               //-----------------------------------
               if (Utilities.NO_RESULT == myGridRows[i].myDieRollMount)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 4);
               }
               else if( (NOT_RIDING == myGridRows[i].myDieRollMount) || (NO_MOUNT == myGridRows[i].myDieRollMount) || (GRIFFON_MOUNT == myGridRows[i].myDieRollMount) )
               {
                  Label labelResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content="NA" };
                  myGrid.Children.Add(labelResult);
                  Grid.SetRow(labelResult, rowNum);
                  Grid.SetColumn(labelResult, 5);
               }
               else
               {
                  string dieRollLabel = myGridRows[i].myDieRollMount.ToString();
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollLabel };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, 4);
                  //-------------------------------
                  Label labelResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                  myGrid.Children.Add(labelResult);
                  Grid.SetRow(labelResult, rowNum);
                  Grid.SetColumn(labelResult, 5);
                  if (4 < myGridRows[i].myDieRollMount)
                     labelResult.Content = "yes";
                  else
                     labelResult.Content = "no";
               }
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
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
            return;
         }
         IMapItem mi = myGridRows[i].myMapItem;
         //-----------------------------------------------------------------
         if (1 == myRollResultColNum) // person may catch cold
         {
            myGridRows[i].myDieRoll = dieRoll;
            if (4 < dieRoll)
            {
               if( false == mi.IsCatchCold) // only cause would if catch cold. If already have cold, do not add another wound
                  mi.SetWounds(1, 0);
               if (true == mi.IsKilled)
                  myIsAnyLost = true;
               mi.IsCatchCold = true;
            }
         }
         else if (4 == myRollResultColNum) // mount may die
         {
            myGridRows[i].myDieRollMount = dieRoll;
            if (4 < dieRoll)
            {
               mi.RemoveMountedMount();
               myIsAnyLost = true;
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myRollResultColNum=" + myRollResultColNum.ToString() + " myState=" + myState.ToString());
            return;
         }
         //-----------------------------------------------------------------
         myState = E079Enum.SHOW_RESULTS;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            IMapItem mi1 = myGridRows[j].myMapItem;
            if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
            {
               myState = E079Enum.COLD_CHECK;
            }
            else if (true == myGameInstance.IsHeavyRainContinue)
            {
               if (Utilities.NO_RESULT == myGridRows[j].myDieRollMount)
                  myState = E079Enum.COLD_CHECK;
            }
         }
         //-----------------------------------------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if( E079Enum.SHOW_RESULTS == myState )
         {
            myState = E079Enum.END;
            if ( false == UpdateEndState())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateEndState() returned false");
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
                           myState = E079Enum.END;
                           if (false == UpdateEndState())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateEndState() returned false");
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
                     myRollResultRowNum = Grid.GetRow(img1);
                     myRollResultColNum = Grid.GetColumn(img1);
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
   }
}
