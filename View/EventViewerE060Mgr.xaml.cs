using System;
using System.Collections.Generic;
using System.Linq;
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
using WpfAnimatedGif;
using Point = System.Windows.Point;
using static BarbarianPrince.EventViewerStarvationMgr;
using static BarbarianPrince.EventViewerLodgingMgr;

namespace BarbarianPrince
{
   public partial class EventViewerE060Mgr : UserControl
   {
      public delegate bool EndE060PaymentCallback();
      private const int STARTING_ASSIGNED_ROW = 8;
      //---------------------------------------------
      public struct MountRow
      {
         public string myName;
         public bool myIsReleased;
         public MountRow(string name, bool isReleased)
         {
            myName = name;
            myIsReleased = isReleased;
         }
      };
      public struct GridRow
      {
         public IMapItem myMapItem;
         public bool myIsReleased;
         public List<MountRow> myMountRows;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myIsReleased = false;
            myMountRows = null;
         }
      };
      public enum E060Enum
      {
         RELEASE_SELECTION,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      private int myCoinOriginal = 0;
      private int myCoinCurrent = 0;
      private int myTotalCost = 0;  // total costs to release all party members
      //---------------------------------------------
      private E060Enum myState = E060Enum.RELEASE_SELECTION;
      private EndE060PaymentCallback myCallback = null;
      private int myMaxRowCount = 0;
      private int myNumMountsCount = 0;
      private bool myIsMoreThanOneMountToMapItem = false;
      private GridRow[] myGridRows = null;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private bool myIsHeaderCheckBoxChecked = false;
      //---------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerE060Mgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerPlagueDustMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerPlagueDustMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerPlagueDustMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerPlagueDustMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool PaymentCheck(EndE060PaymentCallback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "PaymentCheck(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 2) // this dialog should only show if there is more than the prince
         {
            Logger.Log(LogEnum.LE_ERROR, "PaymentCheck(): myGameInstance.PartyMembers.Count < 2");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E060Enum.RELEASE_SELECTION;
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myNumMountsCount = 0;
         myIsMoreThanOneMountToMapItem = false;
         myCallback = callback;
         myCoinOriginal = 0;
         myCoinCurrent = 0;
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "PaymentCheck(): mi=null");
               return false;
            }
            myGridRows[i] = new GridRow(mi);
            myCoinOriginal += mi.Coin;
            myNumMountsCount += mi.Mounts.Count;
            if (1 < mi.Mounts.Count)
               myIsMoreThanOneMountToMapItem = true;
            if ("Prince" == mi.Name)
               myGridRows[i].myIsReleased = true;
            //-----------------------------------
            if (false == mi.IsFlyer())
            {
               mi.IsRiding = false; // dismount all party members
               mi.IsFlying = false;
            }
            if ( null != mi.Rider )
            {
               mi.Rider.Mounts.Remove(mi);
               mi.Rider = null;
            }
            //-----------------------------------
            myGridRows[i].myMountRows = new List<MountRow>();
            foreach (IMapItem mount in mi.Mounts)
            {
               MountRow mr = new MountRow(mount.Name, false);
               myGridRows[i].myMountRows.Add(mr);
            }
            ++i;
         }
         //--------------------------------------------------
         myIsHeaderCheckBoxChecked = false;
         myCoinCurrent = myCoinOriginal - 2;  // minus two for Prince
         myTotalCost = myMaxRowCount*2 - 2;   // minus two for cost of Prince
         myTotalCost += myNumMountsCount;
         if (myTotalCost <= myCoinCurrent)
         {
            myCoinCurrent = myCoinOriginal - myTotalCost;
            for (int k = 0; k < myMaxRowCount; ++k)
            {
               myGridRows[k].myIsReleased = true;
               myGridRows[k].myMountRows.Clear();
               foreach (IMapItem mount in myGridRows[k].myMapItem.Mounts)
               {
                  MountRow mr = new MountRow(mount.Name, true);
                  myGridRows[k].myMountRows.Add(mr);
               }
            }
            myIsHeaderCheckBoxChecked = true;
         }
         //--------------------------------------------------
         // Add the unassignable mapitems that never move or change to the Grid Rows
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenChest(): UpdateGrid() return false");
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
         if (E060Enum.END == myState)
            return true;
         if (false == UpdateHeader())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
            return false;
         }
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
         if (E060Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            //---------------------------------
            for (int i = 0; i < myMaxRowCount; ++i) // Remove all members that are abandoned
            {
               IMapItem mi = myGridRows[i].myMapItem;  
               foreach (MountRow mr1 in myGridRows[i].myMountRows) // remove all mounts abandoned
               {
                  if (false == mr1.myIsReleased)
                     mi.Mounts.Remove(mr1.myName);
               }
               if (false == myGridRows[i].myIsReleased)
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
            case E060Enum.RELEASE_SELECTION:
               myTextBlockInstructions.Inlines.Add(new Run("Select which party members to pay dues. Click campfire when done."));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default = " + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateHeader()
      {
         myStackPanelCheckMarks.Children.Clear();
         CheckBox cb = new CheckBox() { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         if (true == myIsHeaderCheckBoxChecked)
         {
            cb.IsChecked = true;
            cb.Unchecked += CheckBoxHeader_Unchecked;
         }
         else
         {
            cb.IsChecked = false;
            cb.Checked += CheckBoxHeader_Checked;
            if (myCoinCurrent < myTotalCost)
               cb.IsEnabled = false;
         }
         cb.Content = "Pay Release Costs " + myTotalCost.ToString();
         myStackPanelCheckMarks.Children.Add(cb);
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E060Enum.RELEASE_SELECTION:
               BitmapImage bmi11 = new BitmapImage();
               bmi11.BeginInit();
               bmi11.UriSource = new Uri("../../Images/Campfire2.gif", UriKind.Relative);
               bmi11.EndInit();
               Image img11 = new Image { Name = "Campfire", Source = bmi11, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img11, bmi11);
               myStackPanelAssignable.Children.Add(img11);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         //--------------------------------------------
         Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
         myStackPanelAssignable.Children.Add(r2);
         //--------------------------------------------
         Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Coin"), Name = "Coin", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         myStackPanelAssignable.Children.Add(img1);
         string sContentCoin = "= " + myCoinCurrent.ToString();
         Label labelforCoin = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContentCoin };
         myStackPanelAssignable.Children.Add(labelforCoin);
         return true;
      }
      private bool UpdateGridRows()
      {
         if (true == myIsMoreThanOneMountToMapItem)
            myTextBlock3.Text = "Click to Rotate";
         else
            myTextBlock3.Text = "Mounts";
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
            CheckBox cb = new CheckBox() { IsEnabled = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if ((false == myIsHeaderCheckBoxChecked) && (true == mi.IsFickle)) // fickle members will adbandon if anybody else is adbandoned
            {
               cb.IsChecked = false;
               Label labelCost = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(labelCost);
               Grid.SetRow(labelCost, rowNum);
               Grid.SetColumn(labelCost, 2);
            }
            else if (true == myGridRows[i].myIsReleased)
            {
               cb.IsChecked = true;
               cb.Unchecked += CheckBoxRelease_Unchecked;
               if ("Prince" != mi.Name)
                  cb.IsEnabled = true;
               Label labelCost = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "2" };
               myGrid.Children.Add(labelCost);
               Grid.SetRow(labelCost, rowNum);
               Grid.SetColumn(labelCost, 2);
            }
            else
            {
               cb.IsChecked = false;
               cb.Checked += CheckBoxRelease_Checked;
               if (1 < myCoinCurrent)
                  cb.IsEnabled = true;
            }
            myGrid.Children.Add(cb);
            Grid.SetRow(cb, rowNum);
            Grid.SetColumn(cb, 1);
            //--------------------------------
            // Column #3 - Show the mounts
            IMapItem mount = null;
            if (0 < mi.Mounts.Count)
               mount = mi.Mounts[0];
            if (null == mount) // If no mounts assigned
            {
               Label labelforMounts = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "0" };
               myGrid.Children.Add(labelforMounts);
               Grid.SetRow(labelforMounts, rowNum);
               Grid.SetColumn(labelforMounts, 3);
            }
            else
            {
               Button b1 = CreateButton(mount);
               b1.Click += ButtonMount_Click;
               myGrid.Children.Add(b1);
               Grid.SetRow(b1, rowNum);
               Grid.SetColumn(b1, 3);
            }
            //--------------------------------
            // Column #4 - Check box for Mounts
            if (null != mount)
            {
               CheckBox cb2 = new CheckBox() { IsEnabled = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               myGrid.Children.Add(cb2);
               Grid.SetRow(cb2, rowNum);
               Grid.SetColumn(cb2, 4);
               foreach (MountRow mr in myGridRows[i].myMountRows)
               {
                  if (mr.myName == mount.Name)
                  {
                     if (true == mr.myIsReleased)
                     {
                        cb2.IsEnabled = true;
                        cb2.IsChecked = true;
                        cb2.Unchecked += CheckBoxMount_Unchecked;
                        Label labelCost = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "1" };
                        myGrid.Children.Add(labelCost);
                        Grid.SetRow(labelCost, rowNum);
                        Grid.SetColumn(labelCost, 5);
                     }
                     else
                     {
                        cb2.IsChecked = false;
                        cb2.Checked += CheckBoxMount_Checked;
                        if (0 < myCoinCurrent)
                           cb2.IsEnabled = true;
                     }
                     break;
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
                           myState = E060Enum.END;
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                     }
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
      private void CheckBoxHeader_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         myIsHeaderCheckBoxChecked = true;
         myCoinCurrent = myCoinOriginal - myTotalCost;
         for (int k = 0; k < myMaxRowCount; ++k)
         {
            myGridRows[k].myIsReleased = true;
            myGridRows[k].myMountRows.Clear();
            foreach (IMapItem mount in myGridRows[k].myMapItem.Mounts)
            {
               MountRow mr = new MountRow(mount.Name, true);
               myGridRows[k].myMountRows.Add(mr);
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxHeader_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         myIsHeaderCheckBoxChecked = false;
         for (int k = 0; k < myMaxRowCount; ++k)
         {
            if ("Prince" != myGridRows[k].myMapItem.Name)
               myGridRows[k].myIsReleased = false;
            myGridRows[k].myMountRows.Clear();
            foreach (IMapItem mount in myGridRows[k].myMapItem.Mounts)
            {
               MountRow mr = new MountRow(mount.Name, false);
               myGridRows[k].myMountRows.Add(mr);
            }
         }
         myCoinCurrent = myCoinOriginal - 2;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Unchecked(): UpdateGrid() return false");
      }
      private void CheckBoxRelease_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxRelease_Checked(): rowNum=" + rowNum.ToString());
            return;
         }
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         myGridRows[i].myIsReleased = true;
         myCoinCurrent -= 2;
         myIsHeaderCheckBoxChecked = true;
         for (int k = 0; k < myMaxRowCount; ++k)
         {
            if (false == myGridRows[k].myIsReleased)
               myIsHeaderCheckBoxChecked = false;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxRelease_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxRelease_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxRelease_Unchecked(): rowNum=" + rowNum.ToString());
            return;
         }
         myIsHeaderCheckBoxChecked = false;
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         myGridRows[i].myIsReleased = false;
         myCoinCurrent += 2;

         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxRelease_Unchecked(): UpdateGrid() return false");
      }
      private void ButtonMount_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         int rowNum = Grid.GetRow(b);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonMount_Click(): invalid param rowNum=" + rowNum);
            return;
         }
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         mi.Mounts.Rotate(1);
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Click(): UpdateGrid() return false");
      }
      private void CheckBoxMount_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMount_Checked(): rowNum=" + rowNum.ToString());
            return;
         }
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         if (0 == mi.Mounts.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMount_Checked(): mounts count=0 for mi=" + mi.Name);
            return;
         }
         IMapItem mount = mi.Mounts[0];
         --myCoinCurrent;
         int k = 0;
         for (k = 0; k < myGridRows[i].myMountRows.Count; ++k)
         {
            if (myGridRows[i].myMountRows[k].myName == mount.Name)
               break;
         }
         MountRow mr = new MountRow(mount.Name, true);
         myGridRows[i].myMountRows[k] = mr;
         //---------------------------------------------
         myIsHeaderCheckBoxChecked = true;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            if (false == myGridRows[j].myIsReleased)
               myIsHeaderCheckBoxChecked = false;
            foreach (MountRow mr1 in myGridRows[j].myMountRows)
            {
               if( false == mr1.myIsReleased )
                  myIsHeaderCheckBoxChecked = false;
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMount_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxMount_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMount_Unchecked(): rowNum=" + rowNum.ToString());
            return;
         }
         myIsHeaderCheckBoxChecked = false;
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         if (0 == mi.Mounts.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMount_Unchecked(): mounts count=0 for mi=" + mi.Name);
            return;
         }
         IMapItem mount = mi.Mounts[0];
         ++myCoinCurrent;
         int k = 0;
         for (k = 0; k < myGridRows[i].myMountRows.Count; ++k)
         {
            if (myGridRows[i].myMountRows[k].myName == mount.Name)
               break;
         }
         MountRow mr = new MountRow(mount.Name, false);
         myGridRows[i].myMountRows[k] = mr;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMount_Unchecked(): UpdateGrid() return false");
      }
   }
}
