using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using WpfAnimatedGif;
using static BarbarianPrince.EventViewerE088RockMgr;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class EventViewerE085LedgeMgr : UserControl
   {
      public delegate bool EndLedgeFallCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int IS_FLYING = -10;
      private const int DISAPPEAR = +100;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public IMapItem myMount;
         public IMapItem myOwner;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
            myMount = null;
            myOwner = null;
         }
      };
      public enum E085Enum
      {
         FALL_CHECK,
         PRINCE_CHECK_WOUNDS,
         PRINCE_CHECK_LOST,
         PRINCE_LOST_SHOW,
         SHOW_RESULTS,
         MOUNTS_CHECK,
         SHOW_RESULTS_MOUNTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      private bool myIsUnmountedMount = false;
      private bool myIsPartyDisappear = false; // Prince slips and is assumed dead - party leaves him
      //---------------------------------------------
      private E085Enum myState = E085Enum.FALL_CHECK;
      private EndLedgeFallCallback myCallback = null;
      private int myMaxRowMemberCount = 0;
      private int myMaxRowMountCount = 0;
      private GridRow[] myGridRowsMembers = null;
      private GridRow[] myGridRowsMounts = null;
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
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerE085LedgeMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE085LedgeMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE085LedgeMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE085LedgeMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE085LedgeMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE085LedgeMgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool FallCheck(EndLedgeFallCallback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "FallCheck(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "FallCheck(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         IMapItem prince = myGameInstance.Prince;
         if (null == prince)
         {
            Logger.Log(LogEnum.LE_ERROR, "FallCheck(): prince=null");
            return false;
         }
         //--------------------------------------------------
         myGridRowsMembers = new GridRow[Utilities.MAX_GRID_ROW];
         myGridRowsMounts = new GridRow[Utilities.MAX_GRID_ROW];
         myMaxRowMemberCount = myGameInstance.PartyMembers.Count;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myCallback = callback;
         myIsUnmountedMount = false;
         myIsPartyDisappear = false;
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "FallCheck(): mi=null");
               return false;
            }
            int kStart = 0;
            myGridRowsMembers[i] = new GridRow(mi);
            if ((0 < mi.Mounts.Count) && ((true == prince.IsRiding) || (true==mi.IsRiding) )) // if prince is mounted, all members with a horse are considered mounted
            {
               myGridRowsMembers[i].myMount = mi.Mounts[0];
               kStart = 1; 
            }
            for (int k = kStart; k < mi.Mounts.Count; ++k)
            {
               if (false == mi.Mounts[k].IsFlying)
                  myIsUnmountedMount = true;
            }
            ++i;
         }
         //--------------------------------------------------
         for( int k=0; k<myMaxRowMemberCount; ++k ) // If this is a Flyer, or the mount is a griffon/harpy or pegasus, considered flying
         {
            IMapItem member = myGridRowsMembers[k].myMapItem;
            if (true == member.IsFlyer())
               myGridRowsMembers[k].myDieRoll = IS_FLYING;
            if( null != myGridRowsMembers[k].myMount )
            {
               IMapItem mount = myGridRowsMembers[k].myMount;
               if (true == mount.IsFlyingMount())
                  myGridRowsMembers[k].myDieRoll = IS_FLYING;
            }
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "FallCheck(): UpdateGrid() return false");
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
         if (E085Enum.END == myState)
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
         if ((E085Enum.FALL_CHECK == myState) || (E085Enum.PRINCE_CHECK_WOUNDS == myState) || (E085Enum.PRINCE_CHECK_LOST == myState)|| (E085Enum.PRINCE_LOST_SHOW == myState) || (E085Enum.SHOW_RESULTS == myState))
         {
            if (false == UpdateGridRowsMembers())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRowsMembers() returned false");
               return false;
            }
         }
         else
         {
            if (false == UpdateGridRowsMounts())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRowsMounts() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateEndState()
      {
         if (E085Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            //---------------------------------
            if (true == myIsPartyDisappear)
            {
               myGameInstance.RemoveLeaderlessInParty(); // keep possession and mount being ridden
               IMapItem prince = myGameInstance.Prince;
               IMapItems deadMounts = new MapItems();
               for (int k = 1; k < prince.Mounts.Count; ++k) // remove all unridden mounts - assume prince falls with mount he is riding
                  deadMounts.Add(prince.Mounts[k]);
               foreach (IMapItem mount in deadMounts)
                  prince.RemoveMountWithLoad(mount);
            }
            else
            {
               for (int i = 0; i < myMaxRowMemberCount; ++i) // Remove all members that deserted
               {
                  IMapItem mi = myGridRowsMembers[i].myMapItem;
                  if (null == mi)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): mi=null");
                     return false;
                  }
                  if ("Prince" == mi.Name)
                     continue;
                  if ( ((true == mi.IsRiding) && (6 == myGridRowsMembers[i].myDieRoll)) || (12 == myGridRowsMembers[i].myDieRoll) )
                  {
                     if (false == myGameInstance.RemoveVictimInParty(mi))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): RemoveVictimInParty() returned false");
                        return false;
                     }
                  }
               }
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
            case E085Enum.FALL_CHECK:
               if (1 == myGameInstance.PartyMembers.Count)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll to determine if Prince falls."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for each party member to see if they fall."));
               break;
            case E085Enum.PRINCE_CHECK_WOUNDS:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die and add one for wounds."));
               break;
            case E085Enum.PRINCE_CHECK_LOST:
               myTextBlockInstructions.Inlines.Add(new Run("Prince slipped. Roll die to see if party finds him."));
               break;
            case E085Enum.MOUNTS_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for unmounted mounts falling."));
               break;
            case E085Enum.SHOW_RESULTS:
               if( false == myIsUnmountedMount )
                  myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click mount to continue."));
               break;
            case E085Enum.SHOW_RESULTS_MOUNTS:
            case E085Enum.PRINCE_LOST_SHOW:
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
         switch (myState)
         {
            case E085Enum.FALL_CHECK:
            case E085Enum.MOUNTS_CHECK:
               Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r0);
               break;
            case E085Enum.PRINCE_CHECK_WOUNDS:
            case E085Enum.PRINCE_CHECK_LOST:
               BitmapImage bmi1 = new BitmapImage();
               bmi1.BeginInit();
               bmi1.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi1.EndInit();
               Image img1 = new Image { Name = "DieRoll", Source = bmi1, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img1, bmi1);
               myStackPanelAssignable.Children.Add(img1);
               break;
            case E085Enum.SHOW_RESULTS:
               if (false == myIsUnmountedMount)
               {
                  BitmapImage bmi2 = new BitmapImage();
                  bmi2.BeginInit();
                  bmi2.UriSource = new Uri("../../Images/Campfire2.gif", UriKind.Relative);
                  bmi2.EndInit();
                  Image img2 = new Image { Name = "Campfire", Source = bmi2, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img2, bmi2);
                  myStackPanelAssignable.Children.Add(img2);
               }
               else
               {
                  Image img5 = new Image { Name = "Mount", Source = MapItem.theMapImages.GetBitmapImage("Mount"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img5);
               }
               break;
            case E085Enum.PRINCE_LOST_SHOW:
            case E085Enum.SHOW_RESULTS_MOUNTS:
               BitmapImage bmi3 = new BitmapImage();
               bmi3.BeginInit();
               bmi3.UriSource = new Uri("../../Images/Campfire2.gif", UriKind.Relative);
               bmi3.EndInit();
               Image img3 = new Image { Name = "Campfire", Source = bmi3, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img3, bmi3);
               myStackPanelAssignable.Children.Add(img3);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRowsMembers()
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
         for (int i = 0; i < myMaxRowMemberCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRowsMembers[i].myMapItem;
            //------------------------------------
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            if(E085Enum.FALL_CHECK == myState)
            {
               if (null != myGridRowsMembers[i].myMount)
               {
                  Button b1 = CreateButton(myGridRowsMembers[i].myMount);
                  myGrid.Children.Add(b1);
                  Grid.SetRow(b1, rowNum);
                  Grid.SetColumn(b1, 1);
               }
            }
            //--------------------------------
            if (Utilities.NO_RESULT == myGridRowsMembers[i].myDieRoll)
            {
               if (E085Enum.FALL_CHECK == myState)
               {
                  if (null != myGridRowsMembers[i].myMount) // if on mount, die on roll of 6
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                     bmi.EndInit();
                     Image img = new Image { Name = "DieRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 2);
                  }
                  else // if on foot, die on roll of 12
                  {
                     Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "0" };
                     myGrid.Children.Add(label);
                     Grid.SetRow(label, rowNum);
                     Grid.SetColumn(label, 1);
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                     bmi.EndInit();
                     Image img = new Image { Name = "DiceRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 2);
                  }
               }
            }
            else
            {
               string dieRollLabel = myGridRowsMembers[i].myDieRoll.ToString();
               if (DISAPPEAR == myGridRowsMembers[i].myDieRoll)
                  dieRollLabel = "NA";
               if (IS_FLYING == myGridRowsMembers[i].myDieRoll)
                  dieRollLabel = "Fly";
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollLabel };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 2);
               //-------------------------------
               Label labelResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               myGrid.Children.Add(labelResult);
               Grid.SetRow(labelResult, rowNum);
               Grid.SetColumn(labelResult, 3);
               if( (E085Enum.FALL_CHECK == myState) || (E085Enum.SHOW_RESULTS == myState) || (E085Enum.PRINCE_LOST_SHOW == myState))
               {
                  if (IS_FLYING == myGridRowsMembers[i].myDieRoll)
                  {
                     labelResult.Content = "no";
                  }
                  else if (DISAPPEAR == myGridRowsMembers[i].myDieRoll)
                  {
                     labelResult.Content = "yes";
                  }
                  else if (((true == mi.IsRiding) && (6 == myGridRowsMembers[i].myDieRoll)) || (12 == myGridRowsMembers[i].myDieRoll))
                  {
                     if ("Prince" == mi.Name)
                        labelResult.Content = "NA";
                     else
                        labelResult.Content = "yes";
                  }
                  else
                  {
                     labelResult.Content = "no";
                  }
               }
            }
         }
         return true;
      }
      private bool UpdateGridRowsMounts()
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
         for (int i = 0; i < myMaxRowMountCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRowsMounts[i].myMapItem;
            //------------------------------------
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            if (E085Enum.MOUNTS_CHECK == myState)
            {
               if (null != myGridRowsMounts[i].myOwner)
               {
                  Button b1 = CreateButton(myGridRowsMounts[i].myOwner);
                  myGrid.Children.Add(b1);
                  Grid.SetRow(b1, rowNum);
                  Grid.SetColumn(b1, 1);
               }
            }
            //--------------------------------
            if (Utilities.NO_RESULT == myGridRowsMounts[i].myDieRoll)
            {
               if (E085Enum.MOUNTS_CHECK == myState)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                  bmi.EndInit();
                  Image img = new Image { Name = "DieRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 2);
               }
            }
            else
            {
               string dieRollLabel = myGridRowsMounts[i].myDieRoll.ToString();
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollLabel };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 2);
               //-------------------------------
               Label labelResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               myGrid.Children.Add(labelResult);
               Grid.SetRow(labelResult, rowNum);
               Grid.SetColumn(labelResult, 3);
               if (6 == myGridRowsMounts[i].myDieRoll)
                  labelResult.Content = "yes";
               else
                  labelResult.Content = "no";
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
         IMapItem prince = myGameInstance.Prince;
         myTextBlock0.Text = "Mount";
         myTextBlock1.Text = "Owner";
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetGridForMounts(): mi=null");
               return false;
            }
            int kStart = 0;
            if( (true == mi.IsRiding) || (true == prince.IsRiding) ) // skip the first mount if riding or prince is riding
               kStart = 1;
            for (int k = kStart; k < mi.Mounts.Count; ++k)
            {
               myGridRowsMounts[i] = new GridRow(mi.Mounts[k]); // all we want to check is unmounted mounts
               myGridRowsMounts[i].myOwner = mi;
               ++i;
            }
         }
         myMaxRowMountCount = i;
         return true;
      }
      public void ShowDieResults(int dieRoll)
      {
         //------------------------------------------------
         switch(myState)
         {
            case E085Enum.FALL_CHECK:
               int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
               if (i < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
                  return;
               }
               IMapItem mi = myGridRowsMembers[i].myMapItem;
               myGridRowsMembers[i].myDieRoll = dieRoll;
               if ("Prince" == mi.Name)
               {
                  if (((null != myGridRowsMembers[i].myMount) && (6 == myGridRowsMembers[i].myDieRoll)) || (12 == myGridRowsMembers[i].myDieRoll))
                  {

                     myState = E085Enum.PRINCE_CHECK_WOUNDS;
                     if (false == UpdateGrid())
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
                     myIsRollInProgress = false;
                     return;
                  }
               }
               myState = E085Enum.SHOW_RESULTS;
               for (int j = 0; j < myMaxRowMemberCount; ++j)
               {
                  IMapItem mi1 = myGridRowsMembers[j].myMapItem;
                  if (Utilities.NO_RESULT == myGridRowsMembers[j].myDieRoll)
                     myState = E085Enum.FALL_CHECK;
               }
               break;
            case E085Enum.PRINCE_CHECK_WOUNDS:
               int numWounds = dieRoll + 1;
               myGameInstance.Prince.SetWounds(numWounds, 0);
               myState = E085Enum.PRINCE_CHECK_LOST;
               break;
            case E085Enum.PRINCE_CHECK_LOST:
               if ( 4 < dieRoll )
               {
                  myIsPartyDisappear = true;
                  myState = E085Enum.PRINCE_LOST_SHOW;
                  myTextBlock3.Text = "Disappear?";
                  for (int k = 0; k < myMaxRowMemberCount; ++k)
                  {
                     IMapItem disappearingMember = myGridRowsMembers[k].myMapItem;
                     if ("Prince" != disappearingMember.Name)
                        myGridRowsMembers[k].myDieRoll = DISAPPEAR;
                  }
               }
               else
               {
                  myState = E085Enum.SHOW_RESULTS;
                  for (int j = 0; j < myMaxRowMemberCount; ++j)
                  {
                     IMapItem mi1 = myGridRowsMembers[j].myMapItem;
                     if (Utilities.NO_RESULT == myGridRowsMembers[j].myDieRoll)
                        myState = E085Enum.FALL_CHECK;
                  }
               }
               break;
            case E085Enum.MOUNTS_CHECK:
               int i1 = myRollResultRowNum - STARTING_ASSIGNED_ROW;
               if (i1 < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i1.ToString() + " myState=" + myState.ToString());
                  return;
               }
               IMapItem mount = myGridRowsMounts[i1].myMapItem;
               myGridRowsMounts[i1].myDieRoll = dieRoll;
               if (6 == dieRoll)
               {
                  if (null == myGridRowsMounts[i1].myOwner)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): rider=null for" + mount.Name);
                  }
                  else
                  {
                     if (false == myGridRowsMounts[i1].myOwner.RemoveMountWithLoad(mount)) // some of the possessions is also removed
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): RemoveMountWithLoad() return false");
                  }
               }
               myState = E085Enum.SHOW_RESULTS_MOUNTS;
               for (int j = 0; j < myMaxRowMountCount; ++j)
               {
                  IMapItem mi1 = myGridRowsMounts[j].myMapItem;
                  if (Utilities.NO_RESULT == myGridRowsMounts[j].myDieRoll)
                     myState = E085Enum.MOUNTS_CHECK;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default state=" + myState.ToString());
               return;
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
                        if ("Campfire" == img0.Name)
                        {
                           myState = E085Enum.END;
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                        else if ("Mount" == img0.Name)
                        {
                           myState = E085Enum.MOUNTS_CHECK;
                           if (false == ResetGridForMounts())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForMounts() return false");
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                        else if ("DieRoll" == img0.Name)
                        {
                           if (false == myIsRollInProgress)
                           {
                              myIsRollInProgress = true;
                              RollEndCallback callback = ShowDieResults;
                              myDieRoller.RollMovingDie(myCanvas, callback);
                              img0.Visibility = Visibility.Hidden;
                           }
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
                     myRollResultRowNum = Grid.GetRow(img1);  // select the row number of the opener
                     myIsRollInProgress = true;
                     RollEndCallback callback = ShowDieResults;
                     if ("DieRoll" == img1.Name)
                        myDieRoller.RollMovingDie(myCanvas, callback);
                     else
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
