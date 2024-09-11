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
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class EventViewerE126RaftInCurrentMgr : UserControl
   {
      public delegate bool EndRaftCurrentCheckCallback(bool isAnyLost, bool isPrinceLost);
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public IMapItem myMapItemOwner;
         public int myDieRoll;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myMapItemOwner = null;
            myDieRoll = Utilities.NO_RESULT;
         }
      };
      public enum E126Enum
      {
         RAFT_CURRENT_CHECK,
         SHOW_RESULTS,
         MOUNTS_CHECK,
         SHOW_RESULTS_MOUNTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private EndRaftCurrentCheckCallback myCallback = null;
      private E126Enum myState = E126Enum.RAFT_CURRENT_CHECK;
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
      private bool myIsAnybodyLost = false;
      private bool myIsPrinceLost = false;
      private bool myIsMountInParty = false;
      //---------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      public EventViewerE126RaftInCurrentMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE126RaftInCurrentMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE126RaftInCurrentMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE126RaftInCurrentMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE126RaftInCurrentMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE126RaftInCurrentMgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool RaftInCurrentCheck(EndRaftCurrentCheckCallback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "RaftInCurrentCheck(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "RaftInCurrentCheck(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRowMembers = new GridRow[Utilities.MAX_GRID_ROW];
         myGridRowMounts = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E126Enum.RAFT_CURRENT_CHECK;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myCallback = callback;
         myIsAnybodyLost = false;
         myIsPrinceLost = false;
         myIsMountInParty = false;
         myTextBlock1.Visibility = Visibility.Hidden;
         //--------------------------------------------------
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "RaftInCurrentCheck(): mi=null");
               return false;
            }
            if (true == mi.IsFlyingMount())  // if the mount has rider, remove rider
            {
               if (null != mi.Rider)
               {
                  mi.Rider.Mounts.Remove(mi);
                  mi.Rider = null;
               }
            }
         }
         //--------------------------------------------------
         int i = 0;
         int k = 0;
         foreach (IMapItem member in myGameInstance.PartyMembers)
         {
            if (true == member.IsFlyer()) // Skip Flyers 
               continue;
            myGridRowMembers[i] = new GridRow(member);
            ++i;
            foreach (IMapItem mount in member.Mounts)
            {
               if (true == mount.Name.Contains("Pegasus"))
                  continue;
               myGridRowMounts[k] = new GridRow(mount);
               myGridRowMounts[k].myMapItemOwner = member;
               ++k;
               myIsMountInParty = true;
            }
         }
         myMaxRowCountMount = k;
         myMaxRowCountMember = i;
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "RaftInCurrentCheck(): UpdateGrid() return false");
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
         if (E126Enum.END == myState)
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
         if (E126Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            //---------------------------------
            for (int k = 0; k < myMaxRowCountMount; k++) // remove dead mounts
            {
               IMapItem deadMount = myGridRowMounts[k].myMapItem;
               if (6 == myGridRowMounts[k].myDieRoll)
               {
                  IMapItem owner = myGridRowMounts[k].myMapItemOwner;
                  owner.Mounts.Remove(deadMount);
               }
            }
            for (int i = 0; i < myMaxRowCountMember; i++) // remove lost members
            {
               if (6 == myGridRowMembers[i].myDieRoll)
               {
                  IMapItem member = myGridRowMembers[i].myMapItem;
                  if (false == member.Name.Contains("Prince"))
                     member.IsKilled = true;
               }
            }
            if (true == myIsPrinceLost)
            {
               myGameInstance.Prince.OverlayImageName = "";
               myGameInstance.RemoveLeaderlessInParty();
               myGameInstance.Prince.ResetPartial(); // remove all possessions
            }
            else
            {
               myGameInstance.ProcessIncapacitedPartyMembers("Lost Overboard"); // possessions, food, coin, mounts are all transferred
            }
            //---------------------------------
            if (false == myCallback(myIsAnybodyLost, myIsPrinceLost))
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
            case E126Enum.RAFT_CURRENT_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for lost overboard."));
               break;
            case E126Enum.MOUNTS_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for lost mounts."));
               break;
            case E126Enum.SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case E126Enum.SHOW_RESULTS_MOUNTS:
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
            case E126Enum.RAFT_CURRENT_CHECK:
            case E126Enum.MOUNTS_CHECK:
               Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r0);
               break;
            case E126Enum.SHOW_RESULTS:
               Image img3 = null;
               if (true == myIsMountInParty)
               {
                  img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mount"), Name = "Continue", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myTextBlock0.Text = "Mount";
                  myTextBlock1.Visibility = Visibility.Visible;
               }
               else
               {
                  img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Name = "Continue", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               }
               myStackPanelAssignable.Children.Add(img3);
               break;
            case E126Enum.SHOW_RESULTS_MOUNTS:
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
         if ((E126Enum.RAFT_CURRENT_CHECK == myState) || (E126Enum.SHOW_RESULTS == myState))
         {
            for (int i = 0; i < myMaxRowCountMember; ++i)
            {
               int rowNum = i + STARTING_ASSIGNED_ROW;
               IMapItem mi = myGridRowMembers[i].myMapItem;
               //------------------------------------
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): mi=null for i=" + i.ToString());
                  return false;
               }
               //------------------------------------
               Button b = CreateButton(mi);
               myGrid.Children.Add(b);
               Grid.SetRow(b, rowNum);
               Grid.SetColumn(b, 0);
               //--------------------------------
               if ((true == myIsPrinceLost) && (false == mi.Name.Contains("Prince")))
               {
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, 2);
               }
               else if (Utilities.NO_RESULT == myGridRowMembers[i].myDieRoll)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 2);
               }
               else
               {
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowMembers[i].myDieRoll.ToString() };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, 2);
                  //-------------------------------
                  Label labelDeath = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                  if (6 == myGridRowMembers[i].myDieRoll)
                     labelDeath.Content = "lost";
                  else
                     labelDeath.Content = "safe";
                  myGrid.Children.Add(labelDeath);
                  Grid.SetRow(labelDeath, rowNum);
                  Grid.SetColumn(labelDeath, 3);
               }
            }
         }
         else
         {
            for (int i = 0; i < myMaxRowCountMount; ++i)
            {
               int rowNum = i + STARTING_ASSIGNED_ROW;
               IMapItem mi = myGridRowMounts[i].myMapItem;
               IMapItem owner = myGridRowMounts[i].myMapItemOwner;
               //------------------------------------
               Button b = CreateButton(mi);
               myGrid.Children.Add(b);
               Grid.SetRow(b, rowNum);
               Grid.SetColumn(b, 0);
               Button b1 = CreateButton(owner);
               myGrid.Children.Add(b1);
               Grid.SetRow(b1, rowNum);
               Grid.SetColumn(b1, 1);
               //--------------------------------
               if (Utilities.NO_RESULT == myGridRowMounts[i].myDieRoll)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 2);
               }
               else
               {
                  string dieRollLabel = myGridRowMounts[i].myDieRoll.ToString();
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowMounts[i].myDieRoll.ToString() };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, 2);
                  if (6 == myGridRowMounts[i].myDieRoll)
                  {
                     Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("MountDeny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myGrid.Children.Add(img5);
                     Grid.SetRow(img5, rowNum);
                     Grid.SetColumn(img5, 3);
                  }
                  else
                  {
                     Image img6 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mount"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myGrid.Children.Add(img6);
                     Grid.SetRow(img6, rowNum);
                     Grid.SetColumn(img6, 3);
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
      public void ShowDieResults(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
            return;
         }
         if (E126Enum.RAFT_CURRENT_CHECK == myState)
         {
            IMapItem mi = myGridRowMembers[i].myMapItem;
            myGridRowMembers[i].myDieRoll = dieRoll;
            if (6 == dieRoll)
            {
               mi.OverlayImageName = "OMIA";
               if (true == mi.Name.Contains("Prince"))
               {
                  myIsPrinceLost = true;
                  myIsMountInParty = false; // all mounts are lost
                  myIsAnybodyLost = false;  // no possession or wealth to distribute when callback is called since there is no possessions or wealth
                  myState = E126Enum.SHOW_RESULTS;
               }
               else
               {
                  myIsAnybodyLost = true;
                  myState = E126Enum.SHOW_RESULTS;
                  for (int j = 0; j < myMaxRowCountMember; ++j)
                  {
                     IMapItem mi1 = myGridRowMembers[j].myMapItem;
                     if (Utilities.NO_RESULT == myGridRowMembers[j].myDieRoll)
                        myState = E126Enum.RAFT_CURRENT_CHECK;
                  }
               }
            }
            else
            {
               myState = E126Enum.SHOW_RESULTS;
               for (int j = 0; j < myMaxRowCountMember; ++j)
               {
                  IMapItem mi1 = myGridRowMembers[j].myMapItem;
                  if (Utilities.NO_RESULT == myGridRowMembers[j].myDieRoll)
                     myState = E126Enum.RAFT_CURRENT_CHECK;
               }
            }
         }
         else if (E126Enum.MOUNTS_CHECK == myState)
         {
            IMapItem mount = myGridRowMounts[i].myMapItem;
            myGridRowMounts[i].myDieRoll = dieRoll;
            if (6 == dieRoll)
               myIsAnybodyLost = true;
            myState = E126Enum.SHOW_RESULTS_MOUNTS;
            for (int k = 0; k < myMaxRowCountMount; ++k)
            {
               if (Utilities.NO_RESULT == myGridRowMounts[k].myDieRoll)
               {
                  myState = E126Enum.MOUNTS_CHECK;
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
                           if ((E126Enum.SHOW_RESULTS == myState) && (true == myIsMountInParty))
                              myState = E126Enum.MOUNTS_CHECK;
                           else
                              myState = E126Enum.END;
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
                     myRollResultRowNum = Grid.GetRow(img1);  // select the row number of the opener
                     int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
                     if (i < 0)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): Invalid state i=" + i.ToString());
                     }
                     else
                     {
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