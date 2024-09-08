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
   /// <summary>
   /// Interaction logic for EventViewerE095TiredMountMgr.xaml
   /// </summary>
   public partial class EventViewerE095TiredMountMgr : UserControl
   {
      public delegate bool EndTiredMountCheckCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public IMapItem myMapItemOwner;
         public int myDieRoll;
         public GridRow(IMapItem mount, IMapItem owner)
         {
            myMapItem      = mount;
            myMapItemOwner = owner;
            myDieRoll = Utilities.NO_RESULT;
         }
      };
      public enum E090Enum
      {
         MOUNTS_CHECK,
         SHOW_RESULTS_MOUNTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private EndTiredMountCheckCallback myCallback = null;
      private E090Enum myState = E090Enum.MOUNTS_CHECK;
      private int myMaxRowCount= 0;
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
      public EventViewerE095TiredMountMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
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
      public bool TiredMountCheck(EndTiredMountCheckCallback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "TiredMountCheck(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "TiredMountCheck(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E090Enum.MOUNTS_CHECK;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myCallback = callback;
         myMaxRowCount = 0;
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "TiredMountCheck(): mi=null");
               return false;
            }
            foreach(IMapItem mount in mi.Mounts)
            {
               if (true == mount.IsFlyingMountCarrier())
                  continue;
               myGridRows[i] = new GridRow(mount, mi);
               ++i;
            }
         }
         myMaxRowCount = i;
         if (0 == myMaxRowCount) 
         {
            Logger.Log(LogEnum.LE_ERROR, "TiredMountCheck(): myMaxRowCount=0");
            return false;
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "TiredMountCheck(): UpdateGrid() return false");
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
            case E090Enum.MOUNTS_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for mount health."));
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
            case E090Enum.MOUNTS_CHECK:
               Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r0);
               break;
            case E090Enum.SHOW_RESULTS_MOUNTS:
               if (0 < myGameInstance.AtRiskMounts.Count )
               {
                  Image img31 = new Image { Source = MapItem.theMapImages.GetBitmapImage("MountTired"), Name = "Continue", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img31);
                  Label labelForMountResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                  labelForMountResult.Content = " = Mount(s) at risk if you travel next action";
                  myStackPanelAssignable.Children.Add(labelForMountResult);
               }
               else
               {
                  Image img31 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Name = "Continue", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img31);
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
            //------------------------------------
            IMapItem mount = myGridRows[i].myMapItem;
            Button b = CreateButton(mount);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //------------------------------------
            IMapItem owner = myGridRows[i].myMapItemOwner;
            Button b1 = CreateButton(owner);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 1);
            //--------------------------------
            if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(Utilities.theImageDirectoryPath + "dieRoll.gif", UriKind.Relative);
               bmi.EndInit();
               Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 2);
            }
            else
            {
               string dieRollLabel = myGridRows[i].myDieRoll.ToString();
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollLabel };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 2);
               //-------------------------------
               Image imgResult = null;
               switch (myGridRows[i].myDieRoll)
               {
                  case 1:
                  case 2:
                  case 3:
                  case 4:
                     imgResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mount"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     break;
                  case 5:
                     imgResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("MountTired"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     break;
                  case 6:
                     imgResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("MountDeny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): Reached default dieRoll=" + myGridRows[i].myDieRoll.ToString());
                     return false;
               }
               myGrid.Children.Add(imgResult);
               Grid.SetRow(imgResult, rowNum);
               Grid.SetColumn(imgResult, 3);
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
         if ( (i < 0) || (Utilities.MAX_GRID_ROW <= i) )
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
            return;
         }
         IMapItem mount = myGridRows[i].myMapItem;
         IMapItem owner = myGridRows[i].myMapItemOwner;
         myGridRows[i].myDieRoll = dieRoll;
         switch (dieRoll)
         {
            case 1:
            case 2:
            case 3:
            case 4:
               break;  // no effect
            case 5:
               myGameInstance.AtRiskMounts.Add(mount); // mount will die if travel as next action
               break;
            case 6:
               myGameInstance.IsMountsAtRisk = true;
               owner.Mounts.Remove(mount); // mount is dead
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): Reached default dieRoll=" + dieRoll.ToString());
               return;
         }
         //-----------------------------------------------------------------
         myState = E090Enum.SHOW_RESULTS_MOUNTS;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            IMapItem mi1 = myGridRows[j].myMapItem;
            if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
               myState = E090Enum.MOUNTS_CHECK;
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
                           myState = E090Enum.END;
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
