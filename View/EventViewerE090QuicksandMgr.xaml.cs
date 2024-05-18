using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
using Point = System.Windows.Point;
using static BarbarianPrince.EventViewerE088RockMgr;
using static BarbarianPrince.EventViewerE121SunStrokeMgr;

namespace BarbarianPrince
{
   public partial class EventViewerE090QuicksandMgr : UserControl
   {
      public delegate bool EndQuicksandCheckCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int RIDER_JUMPING = 10;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public bool myIsDead;
         public bool myIsMountDead;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
            myIsDead = false;
            myIsMountDead = false;
         }
      };
      public enum E090Enum
      {
         QUICKSAND_CHECK,
         SHOW_RESULTS,
         MOUNTS_CHECK,
         SHOW_RESULTS_MOUNTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private EndQuicksandCheckCallback myCallback = null;
      private E090Enum myState = E090Enum.QUICKSAND_CHECK;
      private int myMaxRowCountMember = 0;
      private GridRow[] myGridRowMembers = null;
      private int myMaxRowCountMount = 0;
      private GridRow[] myGridRowMounts = null;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      private bool myIsAnybodySafe = false;
      private bool myIsUnmountedMount = false;
      //---------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      public EventViewerE090QuicksandMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE090QuicksandMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE090QuicksandMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE090QuicksandMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE090QuicksandMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE090QuicksandMgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool QuicksandCheck(EndQuicksandCheckCallback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "QuicksandCheck(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "QuicksandCheck(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRowMembers = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E090Enum.QUICKSAND_CHECK;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myCallback = callback;
         myIsAnybodySafe = false;
         myIsUnmountedMount = false;
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "QuicksandCheck(): mi=null");
               return false;
            }
            if ((1 < mi.Mounts.Count) || ((1 == mi.Mounts.Count) && (false == mi.IsRiding))) // if more than one mount or one mount but not riding
               myIsUnmountedMount = true;
            if ((true == mi.IsFlyingMountCarrier()) || (null != mi.Rider)) // if the griffon/harpy has rider, do not show. Griffon/Harpy is tied with rider
               continue;
            if (true == mi.Name.Contains("Falcon")) // skip falcons
               continue;
            if ( true == mi.Name.Contains("Eagle") )
            {
               myIsAnybodySafe = true;
               continue;
            }
            myGridRowMembers[i] = new GridRow(mi);
            ++i;
         }
         myMaxRowCountMember = i;
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "QuicksandCheck(): UpdateGrid() return false");
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
         if (E090Enum.END == myState)
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
         if (E090Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            //---------------------------------
            for (int k = 0; k < myMaxRowCountMember; k++) // Remove  dead members & their ridden mounts
            {
               IMapItem member = myGridRowMembers[k].myMapItem;
               if( true == myGridRowMembers[k].myIsMountDead ) // If mount is dead then rider is OK
               {
                  if( 0 == member.Mounts.Count )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): member.Mounts.Count=0");
                     return false;
                  }
                  IMapItem deadMount = member.Mounts[0];
                  if ( false == member.RemoveMountWithLoad(deadMount)) // removes the rider from mount
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): RemoveMountWithLoad() returned false");
                     return false;
                  }
                  if( true == deadMount.IsFlyingMountCarrier()) // remove the mount
                  {
                     deadMount.IsKilled = true;
                     deadMount.SetWounds(deadMount.Endurance, 0);
                  }
               }
               else if( true == myGridRowMembers[k].myIsDead ) // remove both rider and mount
               {
                  member.IsKilled = true;
                  member.SetWounds(member.Endurance, 0);
                  if (false == member.RemoveVictimMountAndLoad())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): RemoveVictimMountAndLoad() returned false");
                     return false;
                  }
               }
            }
            for (int k = 0; k < myMaxRowCountMount; k++) // remove dead mounts that are unridden
            {
               IMapItem deadMount = myGridRowMounts[k].myMapItem;
               if (true == myGridRowMounts[k].myIsMountDead)
               {
                  bool isMountFound = false;
                  foreach (IMapItem member in myGameInstance.PartyMembers)
                  {
                     foreach (IMapItem mount in member.Mounts)
                     {
                        if (mount.Name == deadMount.Name)
                        {
                           if (false == member.RemoveMountWithLoad(deadMount)) // some of the possessions is also removed
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): RemoveMountWithLoad() return false deadMount=" + deadMount.Name);
                              return false;
                           }
                           isMountFound = true;
                           break;
                        }
                     }
                     if (true == isMountFound)
                        break;
                  }
               }
            }
            //---------------------------------
            myGameInstance.RemoveKilledInParty("Quicksand");
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
            case E090Enum.QUICKSAND_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for quicksand effects or if riding, jump off."));
               break;
            case E090Enum.MOUNTS_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for mount deaths."));
               break;
            case E090Enum.SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case E090Enum.SHOW_RESULTS_MOUNTS:
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
            case E090Enum.QUICKSAND_CHECK:
            case E090Enum.MOUNTS_CHECK:
               Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r0);
               break;
            case E090Enum.SHOW_RESULTS:
               Image img3 = null;
               if (true == myIsUnmountedMount)
                  img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mount"), Name = "Continue", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               else
                  img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Name = "Continue", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img3);
               break;
            case E090Enum.SHOW_RESULTS_MOUNTS:
               Image img31 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Name = "Continue", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img31);
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
         if ((E090Enum.QUICKSAND_CHECK == myState) || (E090Enum.SHOW_RESULTS == myState))
         {
            for (int i = 0; i < myMaxRowCountMember; ++i)
            {
               int rowNum = i + STARTING_ASSIGNED_ROW;
               IMapItem mi = myGridRowMembers[i].myMapItem;
               //------------------------------------
               Button b = CreateButton(mi);
               myGrid.Children.Add(b);
               Grid.SetRow(b, rowNum);
               Grid.SetColumn(b, 0);
               //--------------------------------
               if (Utilities.NO_RESULT == myGridRowMembers[i].myDieRoll)
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
                  if( (true == mi.IsRiding) && (false == mi.IsFlyer()) ) // if riding and not a flyer, show the jump icon - Member is jumping off their mount
                  {
                     Image imgJump = new Image { Name = "Jump", Source = MapItem.theMapImages.GetBitmapImage("QuicksandJump"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myGrid.Children.Add(imgJump);
                     Grid.SetRow(imgJump, rowNum);
                     Grid.SetColumn(imgJump, 2);
                  }
               }
               else
               {
                  string dieRollLabel = myGridRowMembers[i].myDieRoll.ToString();
                  if (RIDER_JUMPING == myGridRowMembers[i].myDieRoll)
                     dieRollLabel = "NA";
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollLabel };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, 1);
                  //-------------------------------
                  Image imgResult = null;
                  if (true == myGridRowMembers[i].myIsDead)
                     imgResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("QuicksandDeath"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  else if (true == myGridRowMembers[i].myIsMountDead)
                     imgResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("MountDeny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  else
                     imgResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("QuicksandSave"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(imgResult);
                  Grid.SetRow(imgResult, rowNum);
                  Grid.SetColumn(imgResult, 2);
               }
            }
         }
         else
         {
            for (int i = 0; i < myMaxRowCountMount; ++i)
            {
               int rowNum = i + STARTING_ASSIGNED_ROW;
               IMapItem mi = myGridRowMounts[i].myMapItem;
               //------------------------------------
               Button b = CreateButton(mi);
               myGrid.Children.Add(b);
               Grid.SetRow(b, rowNum);
               Grid.SetColumn(b, 0);
               //--------------------------------
               if (Utilities.NO_RESULT == myGridRowMounts[i].myDieRoll)
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
                  string dieRollLabel = myGridRowMounts[i].myDieRoll.ToString();
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollLabel };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, 1);
                  if (true == myGridRowMounts[i].myIsMountDead)
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
         myGridRowMounts = new GridRow[Utilities.MAX_GRID_ROW];
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetGridForMounts(): mi=null");
               return false;
            }
            for (int k = 0; k < mi.Mounts.Count; ++k)
            {
               if ((0 == k) && (true == mi.IsRiding)) // if riding, skip the first mount
                  continue;
               myGridRowMounts[i] = new GridRow(mi.Mounts[k]);
               ++i;
            }
         }
         myMaxRowCountMount = i;
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
         if (E090Enum.QUICKSAND_CHECK == myState)
         {
            IMapItem partyMember = myGridRowMembers[i].myMapItem;
            myGridRowMembers[i].myDieRoll = dieRoll;
            switch (dieRoll)
            {
               case 1:
               case 2:
               case 3:
                  myIsAnybodySafe = true;
                  break;
               case 4:
                  if (false == myIsAnybodySafe)
                     myGridRowMembers[i].myIsDead = true;
                  break;
               case 5:
                  myIsAnybodySafe = true;
                  if ( (true == partyMember.IsRiding) && (false == partyMember.IsFlyer()) )
                     myGridRowMembers[i].myIsMountDead = true;
                  break;
               case 6:
                  myGridRowMembers[i].myIsDead = true;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): Reached default dieRoll=" + dieRoll.ToString());
                  return;
            }
            //-----------------------------------------------------------------
            myState = E090Enum.SHOW_RESULTS;
            for (int j = 0; j < myMaxRowCountMember; ++j)
            {
               IMapItem mi1 = myGridRowMembers[j].myMapItem;
               if (Utilities.NO_RESULT == myGridRowMembers[j].myDieRoll)
                  myState = E090Enum.QUICKSAND_CHECK;
            }
         }
         else if (E090Enum.MOUNTS_CHECK == myState)
         {
            myGridRowMounts[i].myDieRoll = dieRoll;
            IMapItem deadMount = myGridRowMounts[i].myMapItem;
            if (4 < dieRoll)
               myGridRowMounts[i].myIsMountDead = true;
            myState = E090Enum.SHOW_RESULTS_MOUNTS;
            for (int k = 0; k < myMaxRowCountMount; ++k)
            {
               if (Utilities.NO_RESULT == myGridRowMounts[k].myDieRoll)
               {
                  myState = E090Enum.MOUNTS_CHECK;
                  break;
               }
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
                        if ("Continue" == img0.Name)
                        {
                           if ((E090Enum.SHOW_RESULTS == myState) && (true == myIsUnmountedMount))
                           {
                              myState = E090Enum.MOUNTS_CHECK;
                              if (false == ResetGridForMounts())
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForMounts() return false");
                           }
                           else
                           {
                              myState = E090Enum.END;
                           }
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
                  myRollResultRowNum = Grid.GetRow(img1);  // select the row number of the opener
                  int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
                  if( i < 0 )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): Invalid state i=" + i.ToString());
                  }
                  else
                  {
                     if ("Jump" == img1.Name)
                     {
                        myIsAnybodySafe = true;
                        myGridRowMembers[i].myDieRoll = RIDER_JUMPING;
                        myGridRowMembers[i].myIsMountDead = true;
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                     }
                     else if (false == myIsRollInProgress)
                     {
                        myIsRollInProgress = true;
                        RollEndCallback callback = ShowDieResults;
                        myDieRoller.RollMovingDie(myCanvas, callback);
                        img1.Visibility = Visibility.Hidden;
                     }
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
