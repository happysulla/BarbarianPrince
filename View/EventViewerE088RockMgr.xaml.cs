using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
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
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class EventViewerE088RockMgr : UserControl
   {
      public delegate bool EndRockCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public int myWound;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
            myWound = Utilities.NO_RESULT;
         }
      };
      public enum E088Enum
      {
         FALL_ROCK_CHECK,
         FALL_ROCK_WOUNDS,
         SHOW_RESULTS,
         MOUNTS_CHECK,
         SHOW_RESULTS_MOUNTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private bool myIsUnmountedMount = false;
      private E088Enum myState = E088Enum.FALL_ROCK_CHECK;
      private EndRockCallback myCallback = null;
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
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //-----------------------------------------------------------------------------------------
      public EventViewerE088RockMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE010Mgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE010Mgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE010Mgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE010Mgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE010Mgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool FallingRockCheck(EndRockCallback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "FallingRockCheck(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "FallingRockCheck(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myIsUnmountedMount = false;
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myCallback = callback;
         myState = E088Enum.FALL_ROCK_CHECK;
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "FallingRockCheck(): mi=null");
               return false;
            }
            myGridRows[i] = new GridRow(mi);
            if( ( 1 < mi.Mounts.Count ) || ((1 == mi.Mounts.Count) && (false == mi.IsRiding))) // if more than one mount or one mount but not riding
               myIsUnmountedMount = true;
            ++i;
         }        
         //--------------------------------------------------
         // Add the unassignable mapitems that never move or change to the Grid Rows
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "FallingRockCheck(): UpdateGrid() return false");
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
         if (E088Enum.END == myState)
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
         if (E088Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            //---------------------------------
            bool isAnyMemberLeaving = false;
            for (int i = 0; i < myMaxRowCount; ++i) // Remove all members that deserted
            {
               if (2 < myGridRows[i].myDieRoll)
               {
                  myGameInstance.RemoveAbandonerInParty(myGridRows[i].myMapItem, true); // EventViewerE088RockMgr
                  isAnyMemberLeaving = true;
               }
            }
            //---------------------------------
            if (true == isAnyMemberLeaving) // If any member leaves, all fickle members leave
            {
               IMapItems fickleMembers = new MapItems();
               foreach (IMapItem mi in myGameInstance.PartyMembers)
               {
                  if (true == mi.IsFickle)
                     fickleMembers.Add(mi);
               }
               foreach (IMapItem mi in fickleMembers)
                  myGameInstance.RemoveAbandonerInParty(mi);
            }
            //---------------------------------
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
            case E088Enum.FALL_ROCK_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for rock impact."));
               break;
            case E088Enum.FALL_ROCK_WOUNDS:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for character wounds."));
               break;
            case E088Enum.MOUNTS_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for mount deaths."));
               break;
            case E088Enum.SHOW_RESULTS:
            case E088Enum.SHOW_RESULTS_MOUNTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click landslide to continue."));
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
            case E088Enum.FALL_ROCK_CHECK:
               Label labelForRockResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               labelForRockResult.Content = "Wounds: Roll=5 adds one  -or-  Roll=6 add die + 1";
               myStackPanelAssignable.Children.Add(labelForRockResult);
               Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r0);
               break;
            case E088Enum.FALL_ROCK_WOUNDS:
               BitmapImage bmi0 = new BitmapImage();
               bmi0.BeginInit();
               bmi0.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
               bmi0.EndInit();
               Image img0 = new Image { Name = "WoundRoll", Source = bmi0, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img0, bmi0);
               myStackPanelAssignable.Children.Add(img0);
               break;
            case E088Enum.MOUNTS_CHECK:
               Label labelForSickResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               labelForSickResult.Content = "Roll=5 lose mount  -or-  Roll=6 lose mount & load";
               myStackPanelAssignable.Children.Add(labelForSickResult);
               Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r1);
               break;
            case E088Enum.SHOW_RESULTS:
            case E088Enum.SHOW_RESULTS_MOUNTS:
               Image img5 = new Image { Name = "LandSlide", Source = MapItem.theMapImages.GetBitmapImage("LandSlide1"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img5);
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
            //--------------------------------
            if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
            {
               if(E088Enum.FALL_ROCK_WOUNDS != myState)
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
            }
            else
            {
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRoll.ToString() };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 1);
               //-------------------------------
               if ((E088Enum.FALL_ROCK_CHECK == myState) || (E088Enum.SHOW_RESULTS == myState))
               {
                  Label labelWound = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myWound.ToString() };
                  myGrid.Children.Add(labelWound);
                  Grid.SetRow(labelWound, rowNum);
                  Grid.SetColumn(labelWound, 2);
               }
               else if ((E088Enum.MOUNTS_CHECK == myState) || (E088Enum.SHOW_RESULTS_MOUNTS == myState))
               {
                  if ( 4 < myGridRows[i].myDieRoll )
                  {
                     Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("MountDeny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myGrid.Children.Add(img5);
                     Grid.SetRow(img5, rowNum);
                     Grid.SetColumn(img5, 2);
                  }
                  else 
                  {
                     Image img6 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mount"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myGrid.Children.Add(img6);
                     Grid.SetRow(img6, rowNum);
                     Grid.SetColumn(img6, 2);
                  }
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
      private bool ResetGridForMounts()
      {
         myTextBlock2.Text = "Lives?";
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetGridForMounts(): mi=null");
               return false;
            }
            for(int k=0; k < mi.Mounts.Count; ++k)
            {
               if ((0 == k) && (true == mi.IsRiding)) // if riding, skip the first mount
                  continue;
               myGridRows[i] = new GridRow(mi.Mounts[k]);
               ++i;
            }
         }
         myMaxRowCount = i;
         return true;
      }
      public void ShowDieResults(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
            return;
         }
         //-----------------------------------------------------------------
         IMapItem mi = myGridRows[i].myMapItem;
         if ( E088Enum.FALL_ROCK_CHECK ==  myState )
         {
            myGridRows[i].myDieRoll = dieRoll;
            if (5 == dieRoll)
            {
               mi.SetWounds(1, 0);
               myGridRows[i].myWound = 1;
            }
            else if (6 == dieRoll)
            {
               myState = E088Enum.FALL_ROCK_WOUNDS;
               if (false == UpdateGrid())
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
               myIsRollInProgress = false;
               return;
            }
            else
            {
               myGridRows[i].myWound = 0;
            }
            myState = E088Enum.SHOW_RESULTS;
            for( int k=0; k<myMaxRowCount; ++k)
            {
               if( Utilities.NO_RESULT == myGridRows[k].myDieRoll )
               {
                  myState = E088Enum.FALL_ROCK_CHECK;
                  break;
               }
            }
         }
         else if (E088Enum.FALL_ROCK_WOUNDS == myState)
         {
            myGridRows[i].myWound = dieRoll+1;
            mi.SetWounds(myGridRows[i].myWound, 0);
            myState = E088Enum.SHOW_RESULTS;
            for (int k = 0; k < myMaxRowCount; ++k)
            {
               if (Utilities.NO_RESULT == myGridRows[k].myDieRoll)
               {
                  myState = E088Enum.FALL_ROCK_CHECK;
                  break;
               }
            }
         }
         else if (E088Enum.MOUNTS_CHECK == myState)
         {
            myGridRows[i].myDieRoll = dieRoll;
            if (5 == dieRoll)
            {
               bool isMountFound = false;
               foreach (IMapItem member in myGameInstance.PartyMembers)
               {
                  foreach (IMapItem mount in member.Mounts)
                  {
                     if (mount.Name == mi.Name)
                     {
                        member.Mounts.Remove(mount); // just remove the horse
                        isMountFound = true;
                        break;
                     }
                  }
                  if (true == isMountFound)
                     break;
               }
            }
            else if (6 == dieRoll)
            {
               bool isMountFound = false;
               foreach (IMapItem member in myGameInstance.PartyMembers)
               {
                  foreach (IMapItem mount in member.Mounts)
                  {
                     if (mount.Name == mi.Name)
                     {
                        if( false == member.RemoveMountWithLoad(mi) ) // some of the possessions is also removed
                           Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): RemoveMountWithLoad() return false");
                        isMountFound = true;
                        break;
                     }
                  }
                  if (true == isMountFound)
                     break;
               }
            }
            myState = E088Enum.SHOW_RESULTS_MOUNTS;
            for (int k = 0; k < myMaxRowCount; ++k)
            {
               if (Utilities.NO_RESULT == myGridRows[k].myDieRoll)
               {
                  myState = E088Enum.MOUNTS_CHECK;
                  break;
               }
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
                  if (ui1 is Image img0) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img0)
                     {
                        if ("LandSlide" == img0.Name)
                        {
                           if ((E088Enum.SHOW_RESULTS == myState) && (true == myIsUnmountedMount))
                           {
                              myState = E088Enum.MOUNTS_CHECK;
                              if( false == ResetGridForMounts())
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForMounts() return false");
                           }
                           else
                           {
                              myState = E088Enum.END;
                           }
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                        else if( "WoundRoll" == img0.Name )
                        {
                           if (false == myIsRollInProgress)
                           {
                              myIsRollInProgress = true;
                              myDieRoller.RollMovingDie(myCanvas, ShowDieResults);
                              img0.Visibility = Visibility.Hidden;
                           }
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
                     myRollResultRowNum = Grid.GetRow(img1);  // select the row number of the opener
                     myIsRollInProgress = true;
                     myDieRoller.RollMovingDie(myCanvas, ShowDieResults);
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
