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

namespace BarbarianPrince
{
   public partial class EventViewerTransportMgr : System.Windows.Controls.UserControl
   {
      public delegate bool EndLoadCallback();
      private const int STARTING_ASSIGNED_ROW = 8;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public IMapItem[] myCarriers;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myCarriers = new IMapItem[5];
         }
      };
      public enum LoadEnum
      {
         LE_ASSIGN_MOUNTS,
         LE_ASSIGN_CARRIERS,
         LE_ASSIGN_CARRIERS_STROKE,
         LE_ASSIGN_FOOD_GOLD,
         LE_END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private LoadEnum myState = LoadEnum.LE_ASSIGN_MOUNTS;
      private EndLoadCallback myCallback = null;
      private IMapItem myMapItemDragged = null;
      private int myRowNum = 0;
      private IMapItems myUnassignedMounts = new MapItems();
      private IMapItems myUnconsciousMembers = new MapItems();
      private IMapItems myConsciousMembers = new MapItems();
      private int myPartyMountCount = 0; // number of mounts held by party
      private int myMaxFreeLoad = 0; // only allow carrying if max free load allows carrying somebody
      private bool myIsSomeCoinOrFood = false; // show food/coin if party has some in assigable panel
      private ITerritory myTerritory = null;
      private GridRow[] myGridRows = null;
      //---------------------------------------------
      private int myUnassignedCoin = 0;
      private int myUnassignedFood = 0;
      private bool myIsHighPass = false;
      private bool myIsSunStrokeInParty = false;  // e121 
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private readonly Dictionary<string, Cursor> myCursors = new Dictionary<string, Cursor>();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerTransportMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerTransportMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerTransportMgr(): sv=null");
            CtorError = true;
            return;
         }
         sv.Content = myGrid;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerTransportMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool TransportLoad(EndLoadCallback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "TransportLoad(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "TransportLoad(): partyMembers=null");
            return false;
         }
         //--------------------------------------------------
         myConsciousMembers.Clear();
         myUnconsciousMembers.Clear();
         myUnassignedMounts.Clear();
         //--------------------------------------------------
         myCallback = callback;
         myIsSomeCoinOrFood = false;
         myPartyMountCount = 0;
         myMaxFreeLoad = 0;
         myUnassignedCoin = 0;
         myUnassignedFood = 0;
         myMapItemDragged = null;
         myTerritory = myGameInstance.Prince.Territory;
         myIsSunStrokeInParty = false;  // e121 
         //--------------------------------------------------
         myIsHighPass = (7 < myGameInstance.DieResults["e086a"][0]);
         if (true == myIsHighPass) // death in the high pass
         {
            bool isMountsKilled = (8 < myGameInstance.DieResults["e086a"][0]);
            myGameInstance.DieResults["e086a"][0] = Utilities.NO_RESULT;
            foreach (IMapItem mi in myGameInstance.PartyMembers) 
            {
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "TransportLoad(): mi=null");
                  return false;
               }
               //------------------------------------
               if (mi.Food < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "1-TransportLoad(): mi=" + mi.Name + " food=" + mi.Food.ToString() + " < 0 ");
                  return false;
               }
               if (mi.Coin < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "2-TransportLoad(): mi=" + mi.Name + " coin=" + mi.Coin.ToString() + " < 0 ");
                  return false;
               }
               //------------------------------------
               if ( ( (true== isMountsKilled) && (0 < mi.Mounts.Count)) || (true == mi.IsKilled) || (true == mi.IsUnconscious)) // transfer belongings to unassigned
               {
                  myUnassignedFood += mi.Food;
                  myUnassignedCoin += mi.Coin;
                  mi.Food = 0;
                  mi.Coin = 0;
               }
            }
            myGameInstance.ProcessIncapacitedPartyMembers("High Pass");
            if (true == isMountsKilled)
            {
               foreach (IMapItem mi in myGameInstance.PartyMembers)
               {
                  mi.Rider = null;
                  mi.Mounts.Clear();
               }
            }
         }
         //--------------------------------------------------
         if( true == myGameInstance.IsAirborne)
         {
            foreach (IMapItem mi in myGameInstance.PartyMembers)
            {
               if( false == mi.RemoveNonFlyingMounts())
               {
                  Logger.Log(LogEnum.LE_ERROR, "TransportLoad(): RemoveNonFlyingMounts() returned false");
                  return false;
               }
               if (true == mi.IsFlyer())
               {
                  if( (true == mi.IsExhausted ) || (true == mi.IsSunStroke) )
                  {
                     mi.IsFlying = false;
                  }
                  else
                  {
                     if( 0 != mi.StarveDayNum ) // if starving only eagles and falcon fly
                     {
                        if ((true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Falcon")))
                           mi.IsFlying = true;
                        else
                           mi.IsFlying = false;
                     }
                     else
                     {
                        mi.IsFlying = true;
                     }
                  }
                  mi.IsRiding = true;
               }
            }
         }
         else
         {
            foreach (IMapItem mi in myGameInstance.PartyMembers)
            {
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "TransportLoad(): mi=null");
                  return false;
               }
               if (null != mi.Rider)
               {
                  mi.Rider.Mounts.Remove(mi);
                  mi.Rider = null;
               }
               if (false == mi.IsFlyer())
               {
                  mi.IsRiding = false; // dismount all party members
                  mi.IsFlying = false;
               }
               else
               {
                  mi.IsRiding = true;
                  mi.IsFlying = true;
                  if ((0 != mi.StarveDayNum) && (false == mi.Name.Contains("Eagle")) && (false == mi.Name.Contains("Falcon")))
                     mi.IsFlying = false;
                  if ( (true==mi.IsExhausted) || (true == mi.IsSunStroke) )
                     mi.IsFlying = false;
               }
            }
         }
         //--------------------------------------------------
         System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, Utilities.theMapItemOffset); // set the center of the MapItem as the hot point for the cursor
         foreach (IMapItem mi in myGameInstance.PartyMembers) 
         {
            if (mi.Food < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "3-TransportLoad(): mi=" + mi.Name + " food=" + mi.Food.ToString() + " < 0 ");
               return false;
            }
            if (mi.Coin < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "4-TransportLoad(): mi=" + mi.Name + " coin=" + mi.Coin.ToString() + " < 0 ");
               return false;
            }
            //------------------------------------
            if ((0 < mi.Food) || (0 < mi.Coin) || (0 < myUnassignedCoin) || (0 < myUnassignedFood) )
               myIsSomeCoinOrFood = true;
            //------------------------------------
            Button b = CreateButton(mi, false, false, true);
            myCursors[mi.Name] = Utilities.ConvertToCursor(b, hotPoint);
            //------------------------------------
            IMapItems killedMembers = new MapItems();
            myPartyMountCount += mi.Mounts.Count;
            if ( (true == mi.IsUnconscious)|| (true == mi.IsSunStroke) )
            {
               if (true == mi.IsSunStroke)
                  myIsSunStrokeInParty = true;
               myUnconsciousMembers.Add(mi);
               myUnassignedFood += mi.Food;
               myUnassignedCoin += mi.Coin;
               mi.Food = 0;
               mi.Coin = 0;
               foreach (IMapItem mount in mi.Mounts) // For unconscious, add mounts to the unassigned pool
               {
                  int maxMountLoad = Utilities.MaxMountLoad;
                  if (true == mount.IsExhausted)
                     maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted 
                  myMaxFreeLoad += (maxMountLoad >> mount.StarveDayNum);
                  myUnassignedMounts.Add(mount);
               }
               mi.Mounts.Clear();
               mi.CarriedMembers.Clear();
            }
            else
            {
               myConsciousMembers.Add(mi);
               if ( (true == mi.IsFlyingMountCarrier()) && (false == mi.IsExhausted) )// if this is griffon/harpy, add to AssignablePanel if can carry enough
               {
                  int loadCanCarry = Utilities.MaxMountLoad >> mi.StarveDayNum;
                  if (null == mi.Rider)
                  {
                     if (Utilities.PersonBurden <= loadCanCarry)
                     {
                        ++myPartyMountCount;
                        myUnassignedMounts.Add(mi);
                     }
                     myMaxFreeLoad += loadCanCarry;
                  }
                  else
                  {
                     if (loadCanCarry < Utilities.PersonBurden)
                     {
                        mi.Rider.Mounts.Remove(mi);
                        mi.Rider = null;
                        myMaxFreeLoad += loadCanCarry;
                     }
                     else
                     {
                        myMaxFreeLoad += (loadCanCarry - Utilities.PersonBurden);
                     }
                  }
               }
               else
               {
                  int maxLoad = Utilities.MaxLoad;
                  if (true == mi.IsExhausted)
                     maxLoad = Utilities.MaxLoad >> 1; // e120 - half the mount load if exhausted 
                  if ( (false == mi.Name.Contains("Eagle")) || (false == mi.Name.Contains("Falcon")) )
                     myMaxFreeLoad += (maxLoad >> mi.StarveDayNum);
                  foreach (IMapItem mount in mi.Mounts)
                  {
                     int maxMountLoad = Utilities.MaxMountLoad;
                     if (true == mount.IsExhausted)
                        maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted 
                     myMaxFreeLoad += (maxMountLoad >> mount.StarveDayNum);
                  }
               }
               GetLoadCanCarry(mi);// move food/coin to unassigned if load too big
               if (0 < mi.Mounts.Count) // set to riding/flying if possible
                  mi.SetMountState(mi.Mounts[0]); // calls GetLoadCanCarry()
            }
         }
         //----------------------------------------
         if (0 < myPartyMountCount)
         {
            IMapItem horse= new MapItem("Horse", 1.0, false, false, false, "MHorse", "MHorse", myTerritory, 0, 0, 0);
            Button h = CreateButton(horse, false, false, true);
            myCursors[horse.Name] = Utilities.ConvertToCursor(h, hotPoint);
            IMapItem pegasus= new MapItem("Pegasus", 1.0, false, false, false, "MPegasus", "MPegasus", myTerritory, 0, 0, 0);
            Button p = CreateButton(pegasus, false, false, true);
            myCursors[pegasus.Name] = Utilities.ConvertToCursor(p, hotPoint);
            IMapItem unicorn= new MapItem("Unicorn", 1.0, false, false, false, "MUnicorn", "MUnicorn", myTerritory, 0, 0, 0);
            Button u = CreateButton(unicorn, false, false, true);
            myCursors[unicorn.Name] = Utilities.ConvertToCursor(u, hotPoint);
         }
         //----------------------------------------
         if (0 < myPartyMountCount)
            myState = LoadEnum.LE_ASSIGN_MOUNTS;
         else if (0 < myUnconsciousMembers.Count)
            myState = LoadEnum.LE_ASSIGN_CARRIERS;
         else
            myState = LoadEnum.LE_ASSIGN_FOOD_GOLD;
         //--------------------------------------------------
         if (false == ResetGrid(myState)) // Recreate Grid based on state
         {
            Logger.Log(LogEnum.LE_ERROR, "TransportLoad(): UpdateGrid() return false");
            return false;
         }
         //--------------------------------------------------
         // Add the unassignable mapitems that never move or change to the Grid Rows
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "TransportLoad(): UpdateGrid() return false");
            return false;
         }
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private bool ResetGrid(LoadEnum state)
      {
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         int i = 0;
         if (LoadEnum.LE_ASSIGN_MOUNTS == state)
         {
            foreach (IMapItem mi in myConsciousMembers)
            {
               myGridRows[i++] = new GridRow(mi);
               mi.CarriedMembers.Clear();
            }
            foreach (IMapItem mi in myUnconsciousMembers)
               mi.CarriedMembers.Clear();
         }
         else if (LoadEnum.LE_ASSIGN_CARRIERS == state)
         {
            foreach (IMapItem mi in myUnconsciousMembers)
            {
               myGridRows[i++] = new GridRow(mi);
               mi.CarriedMembers.Clear();
            }
            foreach (IMapItem mi in myConsciousMembers)
               mi.CarriedMembers.Clear();
         }
         else if (LoadEnum.LE_ASSIGN_FOOD_GOLD == state)
         {
            foreach (IMapItem mi in myConsciousMembers)
               myGridRows[i++] = new GridRow(mi);
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ResetGrid(): reach default state=" + myState.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateEndState() return false");
            return false;
         }
         if (LoadEnum.LE_END == myState)
            return true;
         if (false == UpdateUserInstructions())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() return false");
            return false;
         }
         if (false == UpdateHeader())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() return false");
            return false;
         }
         if (false == UpdateAssignablePanel())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() return false");
            return false;
         }
         if (false == UpdateGridRows())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() return false");
            return false;
         }
         return true;
      }
      private bool UpdateEndState()
      {
         if (LoadEnum.LE_END == myState)
         {
            if (true == myGameInstance.IsAirborne)
            {
               IMapItems nonFlyingMembers = new MapItems();
               foreach (IMapItem mi in myGameInstance.PartyMembers)
               {
                  if (false == mi.IsFlying)
                     nonFlyingMembers.Add(mi);
               }
               foreach (IMapItem mi in nonFlyingMembers)
                  myGameInstance.RemoveAbandonedInParty(mi);
               if (0 < nonFlyingMembers.Count)
               {
                  IMapItems fickleMembers = new MapItems(); // fickle memebers leave if anybody is abandoned
                  foreach (IMapItem mi in myGameInstance.PartyMembers)
                  {
                     if (true == mi.IsFickle)
                        fickleMembers.Add(mi);
                  }
                  foreach (IMapItem fickle in fickleMembers)
                     myGameInstance.RemoveAbandonedInParty(fickle); // fickle take wealth with them
               }
            }
            //---------------------------------------------
            foreach (IMapItem mi in myGameInstance.PartyMembers) // e69 - Remve the overlay for all party members who are resting but unconscious
            {
               if(true == mi.IsUnconscious)
                  mi.OverlayImageName = "";
            }
            //---------------------------------------------
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): myCallback=null");
               return false;
            }
            myGameInstance.IsMountsAtRisk = false; // reset this to false so that next time this dialog is shown, show the right icons
            if (false == myCallback())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): myCallback() returned false");
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
            case LoadEnum.LE_ASSIGN_MOUNTS:
               if (0 == myUnassignedMounts.Count)
               {
                  if (0 < myUnconsciousMembers.Count)
                     myTextBlockInstructions.Inlines.Add(new Run("Click mount to move or click carrying icon to continue."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Click mount to move or click backpack to continue."));
               }
               else
               {
                  myTextBlockInstructions.Inlines.Add(new Run("Click mount to redistribute."));
               }
               break;
            case LoadEnum.LE_ASSIGN_CARRIERS:
               if ((true == myGameInstance.IsWoundedWarriorRest) || (true == myGameInstance.IsWoundedBlackKnightRest))
                  myTextBlockInstructions.Inlines.Add(new Run("Resting to heal! Click backpack to continue."));
               else if (myMaxFreeLoad < Utilities.PersonBurden)
                  myTextBlockInstructions.Inlines.Add(new Run("Unable to carry your friend. Click backpack to continue."));
               else if(( true == myIsSunStrokeInParty ) && (true == myGameInstance.Prince.IsSunStroke) )
                     myTextBlockInstructions.Inlines.Add(new Run("Must assigned carriers for sunstroke victims with Prince being first carried."));
               else if (true == myIsSunStrokeInParty)
                  myTextBlockInstructions.Inlines.Add(new Run("Must assigned carriers for sunstroke victims before continuing."));
               else if (0 < myUnassignedMounts.Count)
                  myTextBlockInstructions.Inlines.Add(new Run("Click carriers to assign them, adjust loads, click mount to adjust, or click backpack to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click carriers to assign them, click mount to adjust, or click backpack to continue."));
               break;
            case LoadEnum.LE_ASSIGN_FOOD_GOLD:
               if (0 < myUnassignedCoin)
               {
                  string endOfInstruction0 = "cache to store coin in hex: ";
                  if (true == myIsHighPass)
                     endOfInstruction0 = "snow shows to continue: ";
                  if (0 < myPartyMountCount)
                     myTextBlockInstructions.Inlines.Add(new Run("Change possessions, click mount to adjust, or select " + endOfInstruction0));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Change possessions or select " + endOfInstruction0));
                  Button buttonRule214 = new Button() { Content = "r214", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonRule214.Click += ButtonRule_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonRule214));
               }
               else
               {
                  if (false == myIsSomeCoinOrFood)
                  {
                     if (0 < myPartyMountCount)
                        myTextBlockInstructions.Inlines.Add(new Run("Click mount to adjust or click image to continue:"));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Click image to continue:"));
                  }
                  else
                  {
                     if (0 < myPartyMountCount)
                        myTextBlockInstructions.Inlines.Add(new Run("Change possessions, click mount to adjust, or click image to continue: "));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Change possessions or click image to continue: "));
                  }
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateHeader()
      {
         myStackPanelCheckMarks.Children.Clear();
         CheckBox cb = new CheckBox() { FontSize = 12, IsEnabled = false, IsChecked = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         string content = "";

         switch (myState)
         {
            case LoadEnum.LE_ASSIGN_MOUNTS:
               content = "Assign Mounts";
               break;
            case LoadEnum.LE_ASSIGN_CARRIERS:
               content = "Assign Carriers";
               break;
            case LoadEnum.LE_ASSIGN_FOOD_GOLD:
               if (false == myIsSomeCoinOrFood)
                  cb.IsChecked = false;
               content = "Assign Loads";
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): reached default");
               return false;
         }
         if (true == myGameInstance.IsAirborne)
            content += " and Prince must be assigned flying mount";
         cb.Content = content;
         myStackPanelCheckMarks.Children.Add(cb);
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case LoadEnum.LE_ASSIGN_MOUNTS:
               bool isAllFlyingCarriers = true;
               foreach (IMapItem mount in myUnassignedMounts)
               {
                  if (false == mount.IsFlyingMountCarrier())
                     isAllFlyingCarriers = false;
               }
               if ((0 == myUnassignedMounts.Count) || (true == isAllFlyingCarriers))
               {
                  if (0 < myUnconsciousMembers.Count)
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri(MapImage.theImageDirectory + "CarryingMan.gif", UriKind.Absolute);
                     bmi.EndInit();
                     Image img = new Image { Tag = "CarryingMan", Source = bmi, Width = Utilities.theMapItemSize + 15, Height = Utilities.theMapItemSize + 10 };
                     myStackPanelAssignable.Children.Add(img);
                  }
                  else
                  {
                     BitmapImage bmi0 = new BitmapImage();
                     bmi0.BeginInit();
                     bmi0.UriSource = new Uri(MapImage.theImageDirectory + "Backpack.gif", UriKind.Absolute);
                     bmi0.EndInit();
                     Image img0 = new Image { Tag = "Backpack", Source = bmi0, Width = Utilities.theMapItemSize + 10, Height = Utilities.theMapItemSize + 10 };
                     myStackPanelAssignable.Children.Add(img0);
                  }
               }
               else
               {
                  Rectangle rfiller = new Rectangle(){ Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize,Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(rfiller);
               }
               //----------------------------------------------------
               Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Stroke = mySolidColorBrushBlack, Fill = Brushes.Transparent,
                  StrokeThickness = 2.0, StrokeDashArray = myDashArray, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r0);
               //----------------------------------------------------
               foreach (IMapItem mi in myUnassignedMounts) // Add a button for each Mount in Assignable Stackpanel
               {
                  bool isRectangleBorderAdded = false; // If dragging a map item, show rectangle around that MapItem
                  if (null != myMapItemDragged && mi.Name == myMapItemDragged.Name)
                     isRectangleBorderAdded = true;
                  Button b = CreateButton(mi, true, isRectangleBorderAdded);
                  myStackPanelAssignable.Children.Add(b);
               }
               if (myUnassignedMounts.Count != myPartyMountCount) // Show rectangle if assignables not at max count
               {
                  Rectangle r1 = new Rectangle() { Visibility = Visibility.Visible, Stroke = mySolidColorBrushBlack, Fill = Brushes.Transparent,
                     StrokeThickness = 2.0, StrokeDashArray = myDashArray, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize};
                  myStackPanelAssignable.Children.Add(r1);
               }
               break;
            case LoadEnum.LE_ASSIGN_CARRIERS:
               bool isError = false;
               bool isBackpackShown = IsBackpackShown(ref isError);
               if( true == isError )
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): IsBackpackShown() returned false for s=" + myState.ToString());
                  return false;
               }
               if (true == isBackpackShown)
               {
                  BitmapImage bmi1 = new BitmapImage();
                  bmi1.BeginInit();
                  bmi1.UriSource = new Uri(MapImage.theImageDirectory + "Backpack.gif", UriKind.Absolute);
                  bmi1.EndInit();
                  Image img1 = new Image { Tag = "Backpack", Source = bmi1, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize + 10 };
                  myStackPanelAssignable.Children.Add(img1);
               }
               else
               {
                  Rectangle rfiller = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(rfiller);
               }
               Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r2);
               if (0 < myPartyMountCount)
               {
                  BitmapImage bmi2 = new BitmapImage();
                  bmi2.BeginInit();
                  bmi2.UriSource = new Uri(MapImage.theImageDirectory + "Mount.gif", UriKind.Absolute);
                  bmi2.EndInit();
                  Image img2 = new Image { Tag = "Mount", Source = bmi2, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
                  Rectangle r3 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
                  myStackPanelAssignable.Children.Add(r3);
               }
               if (Utilities.PersonBurden <= myMaxFreeLoad) // only allow assignment if max free load allows carrying somebody
               {
                  foreach (IMapItem mi in myConsciousMembers) // Add a button for each assignable that has not reached max
                  {
                     int loadCanCarry = GetLoadCanCarry(mi);
                     if ((false == IsInEachRow(mi)) && (0 < loadCanCarry))
                     {
                        bool isRectangleBorderAdded = false;
                        if (null != myMapItemDragged && mi.Name == myMapItemDragged.Name)
                           isRectangleBorderAdded = true;
                        Button b = CreateButton(mi, true, isRectangleBorderAdded);
                        myStackPanelAssignable.Children.Add(b);
                     }
                  }
                  bool isAnyCarrierAssigned = false;  // Only show Rect if no carrier is assigned to any row
                  for (int i = 0; i < myUnconsciousMembers.Count; ++i)
                  {
                     for (int j = 1; j < 5; ++j)
                     {
                        if (null != myGridRows[i].myCarriers[j])
                           isAnyCarrierAssigned = true;
                     }
                  }
                  if (true == isAnyCarrierAssigned)
                  {
                     Rectangle r4 = new Rectangle()
                     {
                        Visibility = Visibility.Visible,
                        Stroke = mySolidColorBrushBlack,
                        Fill = Brushes.Transparent,
                        StrokeThickness = 2.0,
                        StrokeDashArray = myDashArray,
                        Width = Utilities.ZOOM * Utilities.theMapItemSize,
                        Height = Utilities.ZOOM * Utilities.theMapItemSize
                     };
                     myStackPanelAssignable.Children.Add(r4);
                  }
               }
               break;
            case LoadEnum.LE_ASSIGN_FOOD_GOLD:
               if (true == myIsHighPass)
               {
                  Image img21 = new Image { Source = MapItem.theMapImages.GetBitmapImage("SnowShoes"), Tag = "Continue", Width = Utilities.theMapItemSize, Height = Height = Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img21);
               }
               else if (0 < myUnassignedCoin)
               {
                  Image img31 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Cache"), Tag = "Cache", Width = Utilities.theMapItemSize + 10, Height = Height = Utilities.theMapItemSize + 10 };
                  myStackPanelAssignable.Children.Add(img31);
               }
               else if ((true == myGameInstance.IsMountsAtRisk) || (("e078" == myGameInstance.EventActive) && (myGameInstance.Prince.MovementUsed < myGameInstance.Prince.Movement)) || ("e126" == myGameInstance.EventActive))
               {
                  Image img21 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Tag = "Continue", Width = Utilities.theMapItemSize, Height = Height = Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img21);
               }
               else if (myGameInstance.Prince.MovementUsed < myGameInstance.Prince.Movement)
               {
                  if (false == myGameInstance.IsAirborne) // If not airborne 
                  {
                     Image img21 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Tag = "Continue", Width = Utilities.theMapItemSize, Height = Height = Utilities.theMapItemSize };
                     myStackPanelAssignable.Children.Add(img21);
                  }
                  else if (true == myGameInstance.Prince.IsFlying)  // If airborne, prince must be flying
                  {
                     Image img21 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Tag = "Continue", Width = Utilities.theMapItemSize, Height = Height = Utilities.theMapItemSize };
                     myStackPanelAssignable.Children.Add(img21);
                  }
               }
               else
               {
                  BitmapImage bmi4 = new BitmapImage();
                  bmi4.BeginInit();
                  bmi4.UriSource = new Uri(MapImage.theImageDirectory + "Sun1.gif", UriKind.Absolute);
                  bmi4.EndInit();
                  Image img4 = new Image { Tag = "Continue", Source = bmi4, Width = Utilities.theMapItemSize + 10, Height = Utilities.theMapItemSize + 10 };
                  ImageBehavior.SetAnimatedSource(img4, bmi4);
                  ImageBehavior.SetAutoStart(img4, true);
                  ImageBehavior.SetRepeatBehavior(img4, RepeatBehavior.Forever);
                  myStackPanelAssignable.Children.Add(img4);
               }
               //--------------------------------------------
               Rectangle r5 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r5);
               if (0 < myPartyMountCount)
               {
                  BitmapImage bmi3 = new BitmapImage();
                  bmi3.BeginInit();
                  bmi3.UriSource = new Uri(MapImage.theImageDirectory + "Mount.gif", UriKind.Absolute);
                  bmi3.EndInit();
                  Image img3 = new Image { Tag = "Mount", Source = bmi3, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img3);
                  //--------------------------------------------
                  Rectangle r6 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
                  myStackPanelAssignable.Children.Add(r6);
               }
               //--------------------------------------------
               if (0 < myUnconsciousMembers.Count)
               {
                  BitmapImage bmi5 = new BitmapImage();
                  bmi5.BeginInit();
                  bmi5.UriSource = new Uri(MapImage.theImageDirectory + "CarryingMan.gif", UriKind.Absolute);
                  bmi5.EndInit();
                  Image img5 = new Image { Tag = "CarryingMan", Source = bmi5, Width = Utilities.theMapItemSize + 15, Height = Utilities.theMapItemSize + 15 };
                  myStackPanelAssignable.Children.Add(img5);
                  Rectangle r7 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
                  myStackPanelAssignable.Children.Add(r7);
               }
               //************************************************
               if (true == myIsSomeCoinOrFood) // only show Food and Coin if there was some in party at start of dialog
               {
                  //--------------------------------------------
                  BitmapImage bmi6 = new BitmapImage();
                  bmi6.BeginInit();
                  bmi6.UriSource = new Uri(MapImage.theImageDirectory + "Food.gif", UriKind.Absolute);
                  bmi6.EndInit();
                  Image img6 = new Image { Tag="Food", Source = bmi6, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img6);
                  //--------------------------------------------
                  string sContent3 = "= " + myUnassignedFood.ToString();
                  Label labelforFood = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContent3 };
                  myStackPanelAssignable.Children.Add(labelforFood);
                  //--------------------------------------------
                  Rectangle r8 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize  };
                  myStackPanelAssignable.Children.Add(r8);
                  //--------------------------------------------
                  BitmapImage bmi7 = new BitmapImage();
                  bmi7.BeginInit();
                  bmi7.UriSource = new Uri(MapImage.theImageDirectory + "Coin.gif", UriKind.Absolute);
                  bmi7.EndInit();
                  Image img7 = new Image { Tag = "Coin", Source = bmi7, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img7);
                  //--------------------------------------------
                  string sContent4 = "= " + myUnassignedCoin.ToString();
                  Label labelforCoin = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContent4 };
                  myStackPanelAssignable.Children.Add(labelforCoin);
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
         if (LoadEnum.LE_ASSIGN_MOUNTS == myState)
            UpdateGridRowMounts();
         else if (LoadEnum.LE_ASSIGN_CARRIERS == myState)
            UpdateGridRowCarried();
         else
            UpdateGridRowFoodCoin();
         return true;
      }
      private void UpdateGridRowMounts()
      {
         myTextBlock1.Text = "Mounts";
         myTextBlock2.Text = "Riding";
         myTextBlock3.Text = "Flying";
         myTextBlock4.Text = "Carried Loads";
         myTextBlock5.Text = "Free Loads";
         int maxRowCount = myConsciousMembers.Count;
         for (int i = 0; i < maxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem partyMember = myGridRows[i].myMapItem;
            IMapItem ownerMount = null;
            if(0 < partyMember.Mounts.Count)
               ownerMount = partyMember.Mounts[0]; 
            //------------------------------------------------
            Button b = CreateButton(partyMember, false);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //------------------------------------------------
            if (null != partyMember.Rider)
            {
               Button b1 = CreateButton(partyMember.Rider, false);
               myGrid.Children.Add(b1);
               Grid.SetRow(b1, rowNum);
               Grid.SetColumn(b1, 1);
            }
            else if (null != ownerMount) // If mounts assigned
            {
               Button b1 = CreateButton(ownerMount, true);
               myGrid.Children.Add(b1);
               Grid.SetRow(b1, rowNum);
               Grid.SetColumn(b1, 1);
            }
            else
            {
               if (0 < myPartyMountCount) // If mounts exists in party to be assigned
               {
                  if (false == partyMember.IsFlyer())
                  {
                     Rectangle r = new Rectangle()
                     {
                        Visibility = Visibility.Visible,
                        Stroke = mySolidColorBrushBlack,
                        Fill = Brushes.Transparent,
                        StrokeThickness = 2.0,
                        StrokeDashArray = myDashArray,
                        Width = Utilities.ZOOM * Utilities.theMapItemSize,
                        Height = Utilities.ZOOM * Utilities.theMapItemSize
                     };
                     myGrid.Children.Add(r);
                     Grid.SetRow(r, rowNum);
                     Grid.SetColumn(r, 1);
                  }
               }
               else
               {
                  Label labelforMount = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "0" };
                  myGrid.Children.Add(labelforMount);
                  Grid.SetRow(labelforMount, rowNum);
                  Grid.SetColumn(labelforMount, 1);
               }
            }
            //------------------------------------------------
            int loads = GetLoadCanCarry(partyMember);
            CheckBox cb = new CheckBox() { FontSize = 12, IsChecked = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (true == partyMember.IsFlyer())
            {
               cb.IsEnabled = false;
               cb.IsChecked = true;
            }
            else
            {
               if (null != ownerMount) 
               {
                  if ( (true == ownerMount.IsFlyingMountCarrier()) || (true == myGameInstance.IsAirborne) )
                  {
                     cb.IsEnabled = false;
                     cb.IsChecked = true;
                  }
                  else
                  {
                     bool IsRidingPossible = this.IsRidingPossible(partyMember);
                     if (false == partyMember.IsRiding)
                        cb.IsChecked = false;
                     cb.IsEnabled = IsRidingPossible;
                     if (true == IsRidingPossible)
                     {
                        cb.Checked += CheckBoxRiding_Checked;
                        cb.Unchecked += CheckBoxRiding_Unchecked;
                     }
                  }
               }
               else
               {
                  cb.IsEnabled = false;
                  cb.IsChecked = false;
               }
            }
            myGrid.Children.Add(cb);
            Grid.SetRow(cb, rowNum);
            Grid.SetColumn(cb, 2);
            //------------------------------------------------
            CheckBox cb2 = new CheckBox() { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (0 < partyMember.Mounts.Count)
            {
               IMapItem mount = partyMember.Mounts[0];
               if ((true == mount.IsFlyingMount()) && (true == partyMember.IsRiding) && (0 == mount.StarveDayNum) && (false == mount.IsExhausted)) // Cannot fly if mount has any starvation days
               {
                  if (true == partyMember.IsFlying)
                     cb2.IsChecked = true;
                  else
                     cb2.IsChecked = false;
                  if (true == myGameInstance.IsAirborne)
                  {
                     cb2.IsEnabled = false;
                  }
                  else
                  {
                     cb2.Checked += CheckBoxFlying_Checked;
                     cb2.Unchecked += CheckBoxFlying_Unchecked;
                  }
               }
               else
               {
                  cb2.IsEnabled = false;
                  cb2.IsChecked = false;
               }
            }
            else
            {
               cb2.IsEnabled = false;
               if ( true == partyMember.IsFlyingMountCarrier() )
               {
                  if ((null != partyMember.Rider) && (true == partyMember.Rider.IsFlying))
                     cb2.IsChecked = true;
                  else if ( (null == partyMember.Rider) && (0 == partyMember.StarveDayNum) && (false == partyMember.IsExhausted) && (false == partyMember.IsSunStroke))// if Griffon has any starvation days or exhausted, it cannot fly
                     cb2.IsChecked = true;
                  else
                     cb2.IsChecked = false;
               }
               else
               {
                  if ((true == partyMember.Name.Contains("Eagle")) || (true == partyMember.Name.Contains("Falcon")) )
                     cb2.IsChecked = true;
                  else
                     cb2.IsChecked = false;
               }
            }
            myGrid.Children.Add(cb2);
            Grid.SetRow(cb2, rowNum);
            Grid.SetColumn(cb2, 3);
            //------------------------------------------------
            Label labelforCarried = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = partyMember.CarriedMembers.Count.ToString() };
            myGrid.Children.Add(labelforCarried);
            Grid.SetRow(labelforCarried, rowNum);
            Grid.SetColumn(labelforCarried, 4);
            //------------------------------------------------
            Label labelforLoads = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = loads.ToString() };
            myGrid.Children.Add(labelforLoads);
            Grid.SetRow(labelforLoads, rowNum);
            Grid.SetColumn(labelforLoads, 5);
         }
      }
      private void UpdateGridRowCarried()
      {
         myTextBlock1.Text = "Carrier";
         myTextBlock2.Text = "Carrier";
         myTextBlock3.Text = "Carrier";
         myTextBlock4.Text = "Carrier";
         myTextBlock5.Text = "Remaining Loads";
         //------------------------------------------------
         int maxRowCount = myUnconsciousMembers.Count;
         for (int k = 0; k < maxRowCount; ++k)
         {
            int rowNum = k + STARTING_ASSIGNED_ROW;
            IMapItem unconscious = myGridRows[k].myMapItem;
            int remainingLoad = GetRemainingLoad(unconscious);
            if (0 < remainingLoad)
            {
               if ((true == myGameInstance.IsWoundedWarriorRest) || (true == myGameInstance.IsWoundedBlackKnightRest))
               {
                  unconscious.IsKilled = false;
                  unconscious.OverlayImageName = "ORest";
               }
               else
               {
                  unconscious.IsKilled = true;
                  if (true == unconscious.IsSunStroke)
                     unconscious.OverlayImageName = "Sun5";
                  else
                     unconscious.OverlayImageName = "OKIA";
               }
            }
            else
            {
               unconscious.IsKilled = false;
               if (true == unconscious.IsSunStroke)
                  unconscious.OverlayImageName = "Sun5";
               else
                  unconscious.OverlayImageName = "";
            }
            //------------------------------------------------
            Button b0 = CreateButton(unconscious, false);
            myGrid.Children.Add(b0);
            Grid.SetRow(b0, rowNum);
            Grid.SetColumn(b0, 0);
            //------------------------------------------------
            int rowCount = Math.Min(5, myConsciousMembers.Count + 1);
            for (int j = 1; j < rowCount; ++j)
            {
               IMapItem carrier = myGridRows[k].myCarriers[j];
               if (null == carrier)
               {
                  if (0 < remainingLoad) // only add rectangles if there is remaining load to put a new carrier
                  {
                     Rectangle r = new Rectangle()
                     {
                        Visibility = Visibility.Visible,
                        Stroke = mySolidColorBrushBlack,
                        Fill = Brushes.Transparent,
                        StrokeThickness = 2.0,
                        StrokeDashArray = myDashArray,
                        Width = Utilities.ZOOM * Utilities.theMapItemSize,
                        Height = Utilities.ZOOM * Utilities.theMapItemSize
                     };
                     myGrid.Children.Add(r);
                     Grid.SetRow(r, rowNum);
                     Grid.SetColumn(r, j);
                  }
               }
               else
               {
                  StackPanel stackpanel1 = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
                  Button bMinusLoad = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
                  Button bCarrier = CreateButton(carrier, true);
                  Button bPlusLoad = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
                  int carriedLoad = 0;
                  bool isKeyExist = carrier.CarriedMembers.ContainsKey(unconscious);
                  if (true == isKeyExist)
                     carriedLoad = carrier.CarriedMembers[unconscious];
                  int loadCanCarry = GetLoadCanCarry(carrier);
                  if ((0 < carriedLoad) && (remainingLoad < Utilities.PersonBurden))
                  {
                     bMinusLoad.Click += ButtonCarry_Click; // when clicked, add to remaining load and free load ( decreasing carried load )
                     bMinusLoad.IsEnabled = true;
                  }
                  if ((0 < remainingLoad) && (0 < loadCanCarry))
                  {
                     bPlusLoad.Click += ButtonCarry_Click; // when clicked, remove from remaining load and free load ( increasing carried load )
                     bPlusLoad.IsEnabled = true;
                  }
                  stackpanel1.Children.Add(bMinusLoad);
                  stackpanel1.Children.Add(bCarrier);
                  stackpanel1.Children.Add(bPlusLoad);
                  myGrid.Children.Add(stackpanel1);
                  Grid.SetRow(stackpanel1, rowNum);
                  Grid.SetColumn(stackpanel1, j);
               }
            }
            //------------------------------------------------
            Label labelforRemainingLoad = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = remainingLoad.ToString() };
            myGrid.Children.Add(labelforRemainingLoad);
            Grid.SetRow(labelforRemainingLoad, rowNum);
            Grid.SetColumn(labelforRemainingLoad, 5);
         }
      }
      private void UpdateGridRowFoodCoin()
      {
         myTextBlock1.Text = "Riding";
         myTextBlock2.Text = "Unc Load";
         myTextBlock3.Text = "Food Load";
         myTextBlock4.Text = "Coin Load";
         myTextBlock5.Text = "Free Loads";
         //------------------------------------------------
         int maxRowCount = myConsciousMembers.Count;
         for (int i = 0; i < maxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem partyMember = myGridRows[i].myMapItem;
            int loads = GetLoadCanCarry(partyMember);
            //------------------------------------------------
            Button b = CreateButton(partyMember, false);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //------------------------------------------------
            CheckBox cb0 = new CheckBox() { IsChecked = partyMember.IsRiding, IsEnabled = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            myGrid.Children.Add(cb0);
            Grid.SetRow(cb0, rowNum);
            Grid.SetColumn(cb0, 1);
            //------------------------------------------------
            int carriedLoad = 0;
            foreach (int value in partyMember.CarriedMembers.Values)
               carriedLoad += value;
            Label labelforCarried = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = carriedLoad.ToString() };
            myGrid.Children.Add(labelforCarried);
            Grid.SetRow(labelforCarried, rowNum);
            Grid.SetColumn(labelforCarried, 2);
            //------------------------------------------------
            StackPanel stackpanelFood = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
            Button bMinusFood = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
            if (0 < partyMember.Food)
            {
               bMinusFood.Click += ButtonFood_Click;
               bMinusFood.IsEnabled = true;
            }
            stackpanelFood.Children.Add(bMinusFood);
            Label labelforFood = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = partyMember.Food.ToString() };
            stackpanelFood.Children.Add(labelforFood);
            Button bPlusFood = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
            if ((0 < myUnassignedFood) && (0 < loads))
            {
               bPlusFood.Click += ButtonFood_Click;
               bPlusFood.IsEnabled = true;
            }
            stackpanelFood.Children.Add(bPlusFood);
            myGrid.Children.Add(stackpanelFood);
            Grid.SetRow(stackpanelFood, rowNum);
            Grid.SetColumn(stackpanelFood, 3);
            //------------------------------------------------
            StackPanel stackpanelCoin = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
            Button bMinusCoin = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
            if (0 < partyMember.Coin)
            {
               bMinusCoin.Click += ButtonCoin_Click;
               bMinusCoin.IsEnabled = true;
            }
            stackpanelCoin.Children.Add(bMinusCoin);
            Label labelforCoin = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = partyMember.Coin.ToString() };
            stackpanelCoin.Children.Add(labelforCoin);
            Button bPlusCoin = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
            if (0 < myUnassignedCoin)
            {
               int remainder = partyMember.Coin % 100;
               if (((0 < remainder) && (remainder < 100)) || (0 < loads))
               {
                  bPlusCoin.Click += ButtonCoin_Click;
                  bPlusCoin.IsEnabled = true;
               }
            }
            stackpanelCoin.Children.Add(bPlusCoin);
            myGrid.Children.Add(stackpanelCoin);
            Grid.SetRow(stackpanelCoin, rowNum);
            Grid.SetColumn(stackpanelCoin, 4);
            //------------------------------------------------
            Label labelforLoads = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = loads.ToString() };
            myGrid.Children.Add(labelforLoads);
            Grid.SetRow(labelforLoads, rowNum);
            Grid.SetColumn(labelforLoads, 5);
         }
      }
      //-----------------------------------------------------------------------------------------
      private Button CreateButton(IMapItem mi, bool isEnabled, bool isRectangleAdded = false, bool isCursor = false)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         if (true == isCursor)
         {
            b.Width = Utilities.theMapItemSize;
            b.Height = Utilities.theMapItemSize;
         }
         else
         {
            b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
            b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         }
         if (false == isRectangleAdded)
         {
            b.BorderThickness = new Thickness(0);
         }
         else
         {
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = Brushes.Black;
         }
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         if (true == isEnabled)
         {
            b.IsEnabled = true;
            b.Click += this.Button_Click;
         }
         MapItem.SetButtonContent(b, mi, !isCursor, true); // This sets the image as the button's content
         return b;
      }
      private int GetLoadCanCarry(IMapItem partyMember)
      {
         if ( (true == partyMember.Name.Contains("Eagle")) || (true == partyMember.Name.Contains("Falcon")) )
            return 0;
         int loadCanCarry = 0;
         int mountLoad = 0;
         int personLoad = 0;
         bool isRidingPossible = false;
         if ( true == partyMember.IsFlyingMountCarrier() )
         {
            int maxMountLoad = Utilities.MaxMountLoad;
            if (true == partyMember.IsExhausted)
            {
               maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the load if exhausted 
               partyMember.IsFlying = false;
            }
            if( (false == partyMember.Name.Contains("Eagle")) && (false == partyMember.Name.Contains("Falcon")) && (0 != partyMember.StarveDayNum ) )
               partyMember.IsFlying = false;
            loadCanCarry = (maxMountLoad >> partyMember.StarveDayNum);
            if (null != partyMember.Rider)
               loadCanCarry = 0;
         }
         else
         {
            Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): " + partyMember.Name + " >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> f=" + partyMember.Food.ToString() + " c=" + partyMember.Coin.ToString());
            foreach (IMapItem mount in partyMember.Mounts)
            {
               int maxMountLoad = Utilities.MaxMountLoad;
               if (true == mount.IsExhausted)
                  maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted 
               mountLoad = (maxMountLoad >> mount.StarveDayNum);
               if ( (Utilities.PersonBurden <= mountLoad) && (false == mount.IsExhausted) )
                  isRidingPossible = true;
               loadCanCarry += mountLoad;
            }
            Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): 1=> lc=" + loadCanCarry.ToString() + " ml=" + mountLoad.ToString() + " pl=" + personLoad.ToString() + " r?=" + isRidingPossible.ToString() + " after mount load adds");
            if ((false == isRidingPossible) && (false == myGameInstance.IsAirborne) )
               partyMember.IsRiding = false;
            //------------------------------------------
            int maxPersonLoad = Utilities.MaxLoad;
            if (true == partyMember.IsExhausted)
               maxPersonLoad = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
            personLoad = maxPersonLoad >> partyMember.StarveDayNum; // divide by half for each starve day
            if ( false == partyMember.IsUnconscious ) // only add person load if not riding
               loadCanCarry += personLoad;
            Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): 2=> lc=" + loadCanCarry.ToString() + " ml=" + mountLoad.ToString() + " pl=" + personLoad.ToString() + " r?=" + isRidingPossible.ToString() + " after person load add");
         }
         foreach (var item in partyMember.CarriedMembers)
         {
            IMapItem carriedMember = item.Key;
            if (0 == loadCanCarry)
            {
               partyMember.CarriedMembers[carriedMember] = 0;  // Reset all remaining carried member loads to zero
               continue;
            }
            if (partyMember.CarriedMembers[carriedMember] < loadCanCarry)
            {
               loadCanCarry -= partyMember.CarriedMembers[carriedMember];
            }
            else
            {
               partyMember.CarriedMembers[carriedMember] = loadCanCarry;
               myUnassignedFood += partyMember.Food;
               myUnassignedCoin += partyMember.Coin;
               partyMember.Food = 0;
               partyMember.Coin = 0;
               return 0;
            }
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): 3=> lc=" + loadCanCarry.ToString() + " ml=" + mountLoad.ToString() + " pl=" + personLoad.ToString() + " r?=" + isRidingPossible.ToString() + " after person carry removed");
         if (0 == loadCanCarry)
         {
            myUnassignedFood += partyMember.Food;
            myUnassignedCoin += partyMember.Coin;
            partyMember.Food = 0;
            partyMember.Coin = 0;
            return loadCanCarry;
         }
         //------------------------------------------
         if ( (true == partyMember.IsRiding) && (false == partyMember.IsFlyer()) )
            loadCanCarry -= Utilities.PersonBurden; // 20 for man riding and  what he can carry
         Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): 4=> lc=" + loadCanCarry.ToString() + " ml=" + mountLoad.ToString() + " pl=" + personLoad.ToString() + " r?=" + isRidingPossible.ToString() + " after riding burden");
         //------------------------------------------
         if (0 < partyMember.Coin)
         {
            int remainder = partyMember.Coin % 100;
            if( partyMember.Coin < remainder )
            {
               Logger.Log(LogEnum.LE_ERROR, "GetLoadCanCarry(): (coin=" + partyMember.Coin.ToString() + ") < (remainder=" + remainder.ToString() + ")");
               partyMember.Coin = 0;
               return loadCanCarry;
            }
            int hundreds = partyMember.Coin - remainder;
            int hundredsLoad = hundreds / 100;
            int coinLoad = hundredsLoad;
            if (0 < remainder)
               ++coinLoad;
            if (loadCanCarry == coinLoad)
            {
               myUnassignedFood += partyMember.Food;
               partyMember.Food = 0;
               return 0;
            }
            else if (loadCanCarry < coinLoad)
            {
               Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): 4a=> lc=" + loadCanCarry.ToString() + " ml=" + mountLoad.ToString() + " pl=" + personLoad.ToString() + " cl=" + coinLoad.ToString());
               myUnassignedCoin += remainder; // guaranteed to get rid of remainder
               partyMember.Coin -= remainder;
               if (1 < coinLoad)
               {
                  int diffCoin100Load = hundredsLoad - loadCanCarry;
                  int diffCoin100 = diffCoin100Load * 100;
                  myUnassignedCoin += diffCoin100;
                  partyMember.Coin -= loadCanCarry * 100;
                  Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): 4b=> lc=" + loadCanCarry.ToString() + " ml=" + mountLoad.ToString() + " pl=" + personLoad.ToString() + " cl!=" + diffCoin100.ToString() + " c=" + partyMember.Coin.ToString());
               }
               myUnassignedFood += partyMember.Food;
               partyMember.Food = 0;
               return 0;
            }
            else
            {
               loadCanCarry -= coinLoad;
            }
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): 5=> lc=" + loadCanCarry.ToString() + " ml=" + mountLoad.ToString() + " pl=" + personLoad.ToString() + " r?=" + isRidingPossible.ToString() + " f=" + partyMember.Food.ToString() + " c=" + partyMember.Coin.ToString() + " after food burden");
         //------------------------------------------
         if (0 < partyMember.Food)
         {
            if (loadCanCarry < partyMember.Food)
            {
               int diffFood = partyMember.Food - loadCanCarry;
               myUnassignedFood += diffFood;
               partyMember.Food = loadCanCarry;
               Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): 6=> lc=" + loadCanCarry.ToString() + " f=" + partyMember.Food.ToString() + " df=" + diffFood.ToString() + " after food burden");
               return 0;
            }
            else
            {
               loadCanCarry -= partyMember.Food;
            }
         }
         //------------------------------------------
         Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetLoadCanCarry(): " + partyMember.Name + " <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< f=" + partyMember.Food.ToString() + " c=" + partyMember.Coin.ToString() + " lc=" + loadCanCarry.ToString());
         return loadCanCarry;
      }
      private int GetRemainingLoad(IMapItem unconscious)
      {
         int remainingLoad = Utilities.PersonBurden;
         foreach (IMapItem member in myConsciousMembers)
         {
            foreach (IMapItem carried in member.CarriedMembers.Keys)
            {
               if (carried.Name == unconscious.Name)
               {
                  remainingLoad -= member.CarriedMembers[carried]; ;
                  break;
               }
            }
         }
         return remainingLoad;
      }  // get the remainiing load for carrying a person
      private void InsertMapItem(IMapItem mi, int i, int j)
      {
         if (myUnconsciousMembers.Count <= i)
         {
            Logger.Log(LogEnum.LE_ERROR, "InsertMapItem(): invalid param i=" + i.ToString() + " >= c=" + myUnconsciousMembers.Count.ToString());
            return;
         }
         if (true == IsInRow(mi, i))
            return;
         IMapItem carried = myGridRows[i].myMapItem;
         IMapItem old = myGridRows[i].myCarriers[j];
         if (null != old)
            old.CarriedMembers.Remove(carried);
         int remainingLoad = GetRemainingLoad(carried);
         int loadCanCarry = GetLoadCanCarry(mi);
         if (loadCanCarry < remainingLoad) // dismount
            mi.IsRiding = false;
         loadCanCarry = GetLoadCanCarry(mi);
         if (loadCanCarry < remainingLoad) // remove coin
         {
            myUnassignedCoin += mi.Coin;
            mi.Coin = 0;
         }
         loadCanCarry = GetLoadCanCarry(mi);
         if (loadCanCarry < remainingLoad) // remove food
         {
            myUnassignedFood += mi.Food;
            mi.Food = 0;
         }
         loadCanCarry = GetLoadCanCarry(mi);
         if (0 == loadCanCarry) // do not allow insert if there is no free load 
            return;
         if (remainingLoad < loadCanCarry)
            mi.CarriedMembers[carried] = remainingLoad;
         else
            mi.CarriedMembers[carried] = loadCanCarry;
         myGridRows[i].myCarriers[j] = mi;
      }
      private void RemoveMapItemFromGridRow(IMapItem mi)
      {
         if ((myRowNum < STARTING_ASSIGNED_ROW) || (Utilities.MAX_GRID_ROW < myRowNum))
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveMapItemFromGridRow(): invalid param myRowNum=" + myRowNum.ToString());
            return;
         }
         int i = myRowNum - STARTING_ASSIGNED_ROW;
         IMapItem carried = myGridRows[i].myMapItem;
         for (int j = 1; j < 5; ++j)
         {
            IMapItem carrier = myGridRows[i].myCarriers[j];
            if (null == carrier)
               continue;
            if (carrier.Name == mi.Name)
            {
               mi.CarriedMembers.Remove(carried);
               myGridRows[i].myCarriers[j] = null;
            }
         }
         myRowNum = 0;
      }
      private bool IsInEachRow(IMapItem mi)
      {
         for (int i = 0; i < myUnconsciousMembers.Count; ++i)
         {
            if (false == IsInRow(mi, i))
               return false;
         }
         return true;
      }
      private bool IsInRow(IMapItem mi, int i)
      {
         bool isInRow = false;
         for (int j = 1; j < 5; ++j)
         {
            if (null == myGridRows[i].myCarriers[j])
               continue;
            if (mi.Name == myGridRows[i].myCarriers[j].Name)
               isInRow = true;
         }
         return isInRow;
      }
      private bool IsRidingPossible(IMapItem partyMember)
      {
         if ( (true == partyMember.Name.Contains("Giant")) || (true == partyMember.IsFlyer()) ) // mounts cannot carry giants - Flyers do not ride
            return false;
         foreach (IMapItem mount in partyMember.Mounts)
         {
            if(true == mount.IsExhausted) //e120 - if exhausted, cannot be riding
               continue; 
            int mountLoad = (Utilities.MaxMountLoad >> mount.StarveDayNum);
            if (Utilities.PersonBurden <= mountLoad)
               return true;
         }
         return false;
      }
      private bool IsBackpackShown(ref bool isError)
      {
         isError = false;
         if (false == myIsSunStrokeInParty)
            return true;
         //------------------------------------------
         int maxFreeLoad = 0;
         int currentLoad = 0;
         foreach (IMapItem mi in myConsciousMembers)
         {
            maxFreeLoad += mi.GetMaxFreeLoad();
            currentLoad += GetLoadCanCarry(mi);
         }
         int maxSunstrokeAllowed = (int)Math.Floor(((decimal)maxFreeLoad) / ((decimal)Utilities.PersonBurden));
         int currentSunstrokeAllowed = (int)Math.Floor(((decimal)currentLoad) / ((decimal)Utilities.PersonBurden));
         //------------------------------------------
         int numSunStroke = 0;
         int numSunStrokeCarried = 0;
         foreach (IMapItem mi in myUnconsciousMembers)
         {
            if (true == mi.IsSunStroke)
            {
               ++numSunStroke;
               if (0 == GetRemainingLoad(mi))
                  ++numSunStrokeCarried;
            }
         }
         //------------------------------------------
         int maxSunstrokeNeededToBeCarried = Math.Min(numSunStroke, maxSunstrokeAllowed);
         if(currentSunstrokeAllowed < maxSunstrokeNeededToBeCarried ) // if current does not support what is needed, need to remove all coin/food
         {
            foreach(IMapItem mi in myConsciousMembers)
            {
               if (false == mi.IsFlyer())
               {
                  mi.IsRiding = false; // dismount 
                  mi.IsFlying = false;
               }
               myUnassignedFood += mi.Food;
               myUnassignedCoin += mi.Coin;
               mi.Food = 0;
               mi.Coin = 0;
            }
         }
         if ( numSunStrokeCarried == maxSunstrokeNeededToBeCarried)
         {
            if (true == myGameInstance.Prince.IsSunStroke)
            {
               if (0 == maxSunstrokeAllowed)
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsBackpackShown(): Prince has sun stroke but maxSunstrokeAllowed=" + maxSunstrokeAllowed.ToString());
                  isError = true;
                  return true;
               }
               else
               {
                  if (0 < GetRemainingLoad(myGameInstance.Prince))
                     return false;
               }
            }
            return true;
         }
         return false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (LoadEnum.LE_END == myState)
         {
            myState = LoadEnum.LE_END;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            //------------------------------------------------------------------
            if (ui is StackPanel sp) // If true, could be assignable panel or grid panel
            {
               foreach (UIElement ui1 in sp.Children)
               {
                  if (ui1 is Image img)
                  {
                     if (result.VisualHit == img) // First check all rectangles in the myStackPanelAssignable
                     {
                        string name = (string)img.Tag;
                        if ("Backpack" == name)
                        {
                           if (LoadEnum.LE_ASSIGN_CARRIERS == myState)
                           {
                              for (int i = 0; i < myUnconsciousMembers.Count; ++i)  // Anybody unconscious with remaining load should be dropped
                              {
                                 IMapItem unconscious = myGridRows[i].myMapItem;
                                 int remainingLoad = GetRemainingLoad(unconscious);
                                 if (0 < remainingLoad)
                                 {
                                    for (int j = 1; j < 5; ++j)
                                    {
                                       if (null != myGridRows[i].myCarriers[j])
                                       {
                                          IMapItem carrier = myGridRows[i].myCarriers[j];
                                          bool isKeyExist = carrier.CarriedMembers.ContainsKey(unconscious);
                                          if (true == isKeyExist)
                                             carrier.CarriedMembers.Remove(unconscious);
                                          myGridRows[i].myCarriers[j] = null;
                                       }
                                    }
                                 }
                              }
                           }
                           myState = LoadEnum.LE_ASSIGN_FOOD_GOLD;
                           ResetGrid(myState);
                        }
                        else if ("Mount" == name)
                        {
                           myState = LoadEnum.LE_ASSIGN_MOUNTS;
                           ResetGrid(myState);
                        }
                        else if ("CarryingMan" == name)
                        {
                           myState = LoadEnum.LE_ASSIGN_CARRIERS;
                           ResetGrid(myState);
                        }
                        else if ("Cache" == name)
                        {
                           myState = LoadEnum.LE_END;
                           myGameInstance.Caches.Add(myTerritory, myUnassignedCoin);
                           Logger.Log(LogEnum.LE_MANAGE_CACHE, "EventViewerTransportMgr.Grid_MouseDown(): adding myUnassignedCoin=" + myUnassignedCoin.ToString() + " for t=" + myTerritory.Name);

                        }
                        else if ("Continue" == name)
                        {
                           myState = LoadEnum.LE_END;
                        }
                        else if (("Food" == name) || ("Coin" == name) )
                        {
                           // do nothing
                        }
                        else
                        {
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): Clicking Image - Reached default myState=" + myState.ToString() + " img.Tag=" + name);
                        }
                        break;
                     }
                  }
                  else if (ui1 is Rectangle rect) // Dragging Button to Assignable Panel
                  {
                     if (result.VisualHit == rect) 
                     {
                        int rowNum = Grid.GetRow(sp);
                        int i = rowNum - STARTING_ASSIGNED_ROW;
                        int j = Grid.GetColumn(sp);
                        if (null != myMapItemDragged)
                        {
                           if (0 < i) // if true, dragging item to rect in grid row is error
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): 0 < i=" + i.ToString() + " for myState=" + myState.ToString());
                           }
                           else 
                           {
                              if (LoadEnum.LE_ASSIGN_MOUNTS == myState) // Dragging Button to Assignable Panel - add to unassigned mounts
                              {
                                 myUnassignedMounts.Add(myMapItemDragged);
                                 if (true == myMapItemDragged.IsFlyingMountCarrier()) 
                                 {
                                    IMapItem rider = myMapItemDragged.Rider;
                                    if (null != rider )
                                    {
                                       rider.IsFlying = false;
                                       rider.Mounts.Remove(myMapItemDragged.Name);
                                       myMapItemDragged.Rider = null;
                                       if (0 < rider.Mounts.Count)
                                          rider.SetMountState(rider.Mounts[0]);
                                    }
                                 }
                              }
                              else if (LoadEnum.LE_ASSIGN_CARRIERS == myState)
                              {
                                 RemoveMapItemFromGridRow(myMapItemDragged);
                              }
                              else
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): Assignable Rect Click - Dragging - Reached default myState=" + myState.ToString());
                              }
                           }
                        }
                        myGrid.Cursor = Cursors.Arrow;
                        myMapItemDragged = null;
                        break;
                     }
                  }
               } // end  foreach (UIElement ui1 in panel.Children)
            } // end if (ui is StackPanel panel)
              //------------------------------------------------------------------
            else if (ui is Rectangle rect) // If true, this is grid row - dropping to grid row
            {
               if (result.VisualHit == rect)
               {
                  int rowNum = Grid.GetRow(rect);
                  int i = rowNum - STARTING_ASSIGNED_ROW;
                  int j = Grid.GetColumn(rect);
                  if (null != myMapItemDragged)
                  {
                     if (LoadEnum.LE_ASSIGN_MOUNTS == myState)
                     {
                        IMapItem owner = myGridRows[i].myMapItem;
                        if (true == myMapItemDragged.IsFlyingMountCarrier())
                           myMapItemDragged.Rider = owner;
                        if( 0 < owner.Mounts.Count )
                        {
                           IMapItem mountBeingReplaced = owner.Mounts[0];
                           if (true == mountBeingReplaced.IsFlyingMountCarrier()) // return Griffon/Harpy back to unassigned pool
                           {
                              myUnassignedMounts.Add(mountBeingReplaced);
                              mountBeingReplaced.Rider.Mounts.Remove(mountBeingReplaced);
                              mountBeingReplaced.Rider = null;
                              mountBeingReplaced.IsRiding = true;
                              if( (true == mountBeingReplaced.IsExhausted) || (0 != mountBeingReplaced.StarveDayNum) || (true == mountBeingReplaced.IsSunStroke))
                                 mountBeingReplaced.IsFlying = false;
                              else
                                 mountBeingReplaced.IsFlying = true;
                           }
                        }
                        owner.Mounts.Insert(0, myMapItemDragged);
                        owner.SetMountState(myMapItemDragged);
                     }
                     else if (LoadEnum.LE_ASSIGN_CARRIERS == myState)
                     {
                        InsertMapItem(myMapItemDragged, i, j);
                        IMapItem carried = myGridRows[i].myMapItem;
                        carried.IsRiding = true;
                        carried.IsFlying = true;
                        foreach (IMapItem carrier in myGridRows[i].myCarriers )
                        {
                           if (null == carrier)
                              continue;
                           if (false == carrier.IsRiding)
                              carried.IsRiding = false;
                           if (false == carrier.IsFlying)
                              carried.IsFlying = false;
                        }
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): Rect Click - Dragging Grid Row - Reached default myState=" + myState.ToString());
                     }
                  }
                  myGrid.Cursor = Cursors.Arrow;
                  myMapItemDragged = null;
               }
            }
         } // end foreach
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         myRowNum = Grid.GetRow(b);
         int j = Grid.GetColumn(b);
         if (b.Parent is StackPanel sp)
         {
            myRowNum = Grid.GetRow(sp);
            j = Grid.GetColumn(sp);
         }
         int i = myRowNum - STARTING_ASSIGNED_ROW;
         if (null != myMapItemDragged) // If true, dragging Button and clicking another button
         {
            if (STARTING_ASSIGNED_ROW <= myRowNum)  // If true, dragging to Grid Row
            {
               if (myMapItemDragged.Name != b.Name)
               {
                  if (LoadEnum.LE_ASSIGN_MOUNTS == myState)
                  {
                     IMapItem owner = myGridRows[i].myMapItem;
                     if (true == myMapItemDragged.IsFlyingMountCarrier())
                     {
                        myMapItemDragged.Rider = owner; // replace the rider
                        owner.IsRiding = true;
                        if( true == myMapItemDragged.IsFlying )
                           owner.IsFlying = true;
                     }
                     if (0 < owner.Mounts.Count)
                     {
                        IMapItem mountBeingReplaced = owner.Mounts[0];
                        if (true == mountBeingReplaced.IsFlyingMountCarrier())
                        {
                           mountBeingReplaced.Rider.Mounts.Remove(mountBeingReplaced);
                           mountBeingReplaced.Rider = null;
                           myUnassignedMounts.Add(mountBeingReplaced);
                           mountBeingReplaced.IsRiding = true;
                           if ((true == mountBeingReplaced.IsExhausted) || (0 != mountBeingReplaced.StarveDayNum) || (true == mountBeingReplaced.IsSunStroke))
                              mountBeingReplaced.IsFlying = false;
                           else
                              mountBeingReplaced.IsFlying = true;
                        }
                     }
                     owner.Mounts.Insert(0, myMapItemDragged);
                     owner.SetMountState(myMapItemDragged);
                  }
                  else if (LoadEnum.LE_ASSIGN_CARRIERS == myState)
                  {
                     InsertMapItem(myMapItemDragged, i, j);
                     IMapItem carried = myGridRows[i].myMapItem;
                     carried.IsRiding = true;
                     carried.IsFlying = true;
                     foreach (IMapItem carrier in myGridRows[i].myCarriers)
                     {
                        if (null == carrier)
                           continue;
                        if (false == carrier.IsRiding)
                           carried.IsRiding = false;
                        if (false == carrier.IsFlying)
                           carried.IsFlying = false;
                     }
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Button_Click(): Grid Row Click - Dragging - Reached default myState=" + myState.ToString());
                  }
               }
            }
            else // Dragging to Assignable Panel
            {
               if (myMapItemDragged.Name != b.Name)
               {
                  if (LoadEnum.LE_ASSIGN_MOUNTS == myState)
                  {
                     myUnassignedMounts.Add(myMapItemDragged);
                     if (true == myMapItemDragged.IsFlyingMountCarrier())
                     {
                        IMapItem rider = myMapItemDragged.Rider;
                        if (null == rider)
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): rider=null for mi=" + myMapItemDragged.Name);
                        else
                           rider.Mounts.Remove(myMapItemDragged.Name);
                        myMapItemDragged.Rider = null;
                     }
                  }
               }
            }
            myMapItemDragged = null; // If dragging and clicked on same map item, end the dragging operation
            myGrid.Cursor = Cursors.Arrow;
         }
         else  // Selecting button to drag since myMapItemDragged=null
         {
            if (STARTING_ASSIGNED_ROW <= myRowNum) // if true, selecting from GridRow
            {
               if (LoadEnum.LE_ASSIGN_MOUNTS == myState)
               {
                  IMapItem owner = myGridRows[i].myMapItem;
                  owner.IsRiding = false;
                  owner.IsFlying = false;
                  myMapItemDragged = owner.Mounts.Remove(b.Name);
                  if (0 < owner.Mounts.Count)
                  {
                     IMapItem mountBeingRotated = myGridRows[i].myMapItem.Mounts[0];
                     if (true == mountBeingRotated.IsFlyingMountCarrier())
                        mountBeingRotated.Rider = myGridRows[i].myMapItem;
                     if (Utilities.PersonBurden <= owner.GetFreeLoad())
                     {
                        owner.IsRiding = true;
                        if ( (true == mountBeingRotated.Name.Contains("Pegasus")) && (false == mountBeingRotated.IsExhausted) && (0 == mountBeingRotated.StarveDayNum) )
                           owner.IsFlying = true;
                     }
                  }
               }
               else if (LoadEnum.LE_ASSIGN_CARRIERS == myState)
               {
                  myMapItemDragged = myGridRows[i].myCarriers[j];
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "Button_Click(): Grid Row Click - Not Dragging - Reached default myState=" + myState.ToString());
               }
               if (null == myMapItemDragged)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Button_Click(): Grid Row Click - Not Dragging - myMapItemDragged=null for b.Name=" + b.Name + " state=" + myState.ToString());
                  return;
               }
            }
            else // Selecting from Assignable Panel
            {
               if (LoadEnum.LE_ASSIGN_MOUNTS == myState)
                  myMapItemDragged = myUnassignedMounts.Remove(b.Name);
               else if (LoadEnum.LE_ASSIGN_CARRIERS == myState)
                  myMapItemDragged = myConsciousMembers.Find(b.Name);
               else
                  Logger.Log(LogEnum.LE_ERROR, "Button_Click(): Assignable panel Click - Not Dragging - Reached default myState=" + myState.ToString());
               if (null == myMapItemDragged)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Button_Click(): mi=null for b.Name=" + b.Name);
                  return;
               }
            }
            string name = myMapItemDragged.Name;
            if (true == name.Contains("Horse"))
               name = "Horse";
            else if (true == name.Contains("Pegasus"))
               name = "Pegasus";
            else if (true == name.Contains("Unicorn"))
               name = "Unicorn";
            myGrid.Cursor = myCursors[name]; // change cursor of button being dragged
         }
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): UpdateGrid() return false");
            return;
         }
      }
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
            return;
         }
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (false == myRulesMgr.ShowRule(key))
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false key" + key);
      }
      private void CheckBoxRiding_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxRiding_Checked(): rowNum=" + rowNum.ToString());
            return;
         }
         int i = rowNum - STARTING_ASSIGNED_ROW;
         myGridRows[i].myMapItem.IsRiding = true;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxRiding_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxRiding_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerTransportMgr.CheckBoxRiding_Unchecked(): rowNum=" + rowNum.ToString());
            return;
         }
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         if ( false == mi.IsFlyer() )
            mi.IsRiding = false;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxRiding_Unchecked(): UpdateGrid() return false");
      }
      private void CheckBoxFlying_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFlying_Checked(): rowNum=" + rowNum.ToString());
            return;
         }
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem partyMember = myGridRows[i].myMapItem;
         if (0 == partyMember.Mounts.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFlying_Checked(): partyMember.Mounts.Count =0");
            return;
         }
         IMapItem mount = partyMember.Mounts[0];
         if (true == mount.IsFlyingMountCarrier())
         {
            for (int j = 0; j < myConsciousMembers.Count; ++j)
            {
               if (mount.Name == myGridRows[j].myMapItem.Name) 
                  myGridRows[j].myMapItem.IsFlying = true;
            }
         }
         partyMember.IsFlying = true;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFlying_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxFlying_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFlying_Unchecked(): rowNum=" + rowNum.ToString());
            return;
         }
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem partyMember = myGridRows[i].myMapItem;
         if (0 == partyMember.Mounts.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFlying_Checked(): partyMember.Mounts.Count =0");
            return;
         }
         IMapItem mount = partyMember.Mounts[0];
         if (true == mount.IsFlyingMountCarrier())
         {
            for (int j = 0; j < myConsciousMembers.Count; ++j)
            {
               if (mount.Name == myGridRows[j].myMapItem.Name)
                  myGridRows[j].myMapItem.IsFlying = false;
            }
         }
         partyMember.IsFlying = false;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFlying_Unchecked(): UpdateGrid() return false");
      }
      private void ButtonCarry_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         StackPanel sp = (StackPanel)b.Parent;
         int rowNum = Grid.GetRow(sp);
         int j = Grid.GetColumn(sp);
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem carried = myGridRows[i].myMapItem;
         IMapItem carrier = myGridRows[i].myCarriers[j];
         bool isKeyExist = carrier.CarriedMembers.ContainsKey(carried);
         string content = (String)b.Content;
         if ("-" == content)
         {
            if (false == isKeyExist)
               Logger.Log(LogEnum.LE_ERROR, "ButtonCarry_Click(): Invalid State - trying to decrease non existed carried load for " + carrier.Name + " carrying " + carried.Name);
            else
               --carrier.CarriedMembers[carried];
         }
         else if ("+" == content)
         {
            if (false == isKeyExist)
               carrier.CarriedMembers[carried] = 1;
            else
               ++carrier.CarriedMembers[carried];
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonCarry_Click(): Reached default for " + content);
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonCarry_Click(): UpdateGrid() return false");
      }
      private void ButtonFood_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         StackPanel sp = (StackPanel)b.Parent;
         int rowNum = Grid.GetRow(sp);
         int i = rowNum - STARTING_ASSIGNED_ROW;
         string content = (String)b.Content;
         if ("-" == content)
         {
            ++myUnassignedFood;
            --myGridRows[i].myMapItem.Food;
         }
         else if ("+" == content)
         {
            --myUnassignedFood;
            ++myGridRows[i].myMapItem.Food;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonFood_Click(): Reached default for " + content);
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonFood_Click(): UpdateGrid() return false");
      }
      private void ButtonCoin_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         StackPanel sp = (StackPanel)b.Parent;
         int rowNum = Grid.GetRow(sp);
         int i = rowNum - STARTING_ASSIGNED_ROW;
         string content = (String)b.Content;
         if ("-" == content)
         {
            int coinToTransfer = 0;
            if (100 < myGridRows[i].myMapItem.Coin)
               coinToTransfer = 100;
            else if (0 < myGridRows[i].myMapItem.Coin)
               coinToTransfer = myGridRows[i].myMapItem.Coin;
            myGridRows[i].myMapItem.Coin -= coinToTransfer;
            myUnassignedCoin += coinToTransfer;
         }
         else if ("+" == content)
         {
            int coinToTransfer = 0;
            if (100 < myUnassignedCoin)
               coinToTransfer = 100;
            else if (0 < myUnassignedCoin)
               coinToTransfer = myUnassignedCoin;
            myGridRows[i].myMapItem.Coin += coinToTransfer;
            myUnassignedCoin -= coinToTransfer;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonCoin_Click(): Reached default for " + content);
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonCoin_Click(): UpdateGrid() return false");
      }
   }
}
