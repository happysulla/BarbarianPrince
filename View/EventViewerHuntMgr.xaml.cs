using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace BarbarianPrince
{
   public partial class EventViewerHuntMgr : System.Windows.Controls.UserControl
   {
      public delegate bool EndHuntCallback(bool isMobPursuit, bool isConstabularyPursuit);
      private const int STARTING_ASSIGNED_ROW = 8;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myAssignable;
         public int myModifier;
         public int myFood;
         public int myAssignmentCount;
         public int myResult;
      };
      public enum HuntEnum
      {
         LE_FICKLE_LEAVE,
         LE_HUNT,
         LE_HUNTER_HURT,
         LE_MOB_CHASE,
         LE_SHOW_MOB,
         LE_SHOW_WOUNDS,
         LE_PURCHASE,
         LE_SHOW_DESERTERS,
         LE_SHOW_RESULTS,
         LE_END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private HuntEnum myState = HuntEnum.LE_HUNT;
      private bool myIsFarmland = false;
      private int myFoodOriginal = 0;
      private int myFoodCurrent = 0;
      private int myNumMounts = 0;
      private int myCoinNeededForParty = 0;
      private int myCoinSpentForParty = 0;
      private int myCoinNeededForMounts = 0;
      private int myCoinSpentForMounts = 0;
      private int myFoodAdded = 0;
      private bool myIsMobPursuit = false;
      private bool myIsConstabularyPursuit = false;
      private bool myIsHeaderCheckBoxChecked = false;
      private bool myIsTownCastleTemple = false;
      private int myHunterWound = 0;
      private int myCoinOriginal = 0;
      private int myCoinCurrent = 0;
      private ITerritory myCurrentTerritory = null;
      private IMapItem myMapItemDragged = null;
      IMapItems myMapItems = new MapItems();
      private EndHuntCallback myCallback = null;
      //---------------------------------------------
      private int myMaxRowCount = 0;
      private GridRow[] myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResulltRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------
      private readonly Dictionary<string, Cursor> myCursors = new Dictionary<string, Cursor>();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerHuntMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerHuntMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerHuntMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerHuntMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerHuntMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerHuntMgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
         myGridHunt.MouseDown += Grid_MouseDown;
         myStackPanelCheckMarks.MouseDown += Header_MouseDown;
      }
      public bool PerformHunt(EndHuntCallback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformHunt(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformHunt(): gi.PartyMembers.Count < 1");
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_HUNT, "PerformHunt(): 1-myGameInstance.PartyMembers.Count=" + myGameInstance.PartyMembers.Count.ToString());
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = HuntEnum.LE_HUNT;
         myIsFarmland = false;
         myFoodOriginal = 0;
         myNumMounts = 0;
         myCoinNeededForParty = 0;
         myCoinSpentForParty = 0;
         myCoinSpentForMounts = 0;
         myFoodAdded = 0;
         myHunterWound = 0;
         myCoinOriginal = 0;
         myCoinCurrent = 0;
         myMapItemDragged = null;
         myIsTownCastleTemple = false;
         myIsRollInProgress = false;
         myRollResulltRowNum = 0;
         myCallback = callback;
         myTextBlockHeader.Text = "e215b Hunting";
         if (true == myIsFarmland) // only automatically check box if not farmland and suffer populated hunting
            myIsHeaderCheckBoxChecked = false;
         else
            myIsHeaderCheckBoxChecked = true;
         //--------------------------------------------------
         myCurrentTerritory = myGameInstance.Prince.Territory;
         if (0 < myGameInstance.MapItemMoves.Count)
            myCurrentTerritory = myGameInstance.MapItemMoves[0].NewTerritory;
         if (false == myGameInstance.IsEagleHunt) // if eagle hunt, do not land in hex but land on high crag - hunt is in air so terrain does not matter
         {
            if ( true == myGameInstance.IsInStructure(myCurrentTerritory) )
            {
               myIsTownCastleTemple = true;
               if ((true == myGameInstance.IsPartyContinuouslyLodged) && ("1923" == myCurrentTerritory.Name)) // e160d - always fed and lodged at this castle in hex 1923
               {
                  myGameInstance.IsPartyFed = true;
                  myGameInstance.IsMountsFed = true; 
                  myGameInstance.IsPartyLodged = true;
                  myGameInstance.IsMountsStabled = true;
               }
            }
            else if ("Farmland" == myCurrentTerritory.Type)
            {
               myIsFarmland = true;
               myGameInstance.IsMountsFed = true;
            }
            else if (("Countryside" == myCurrentTerritory.Type) || ("Hills" == myCurrentTerritory.Type) || ("Forest" == myCurrentTerritory.Type) )
            {
               myGameInstance.IsMountsFed = true;
            }
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_HUNT, "PerformHunt(): myIsTownCastleTemple=" + myIsTownCastleTemple.ToString() + " t=" + myCurrentTerritory.Name + " t.Type=" + myCurrentTerritory.Type.ToString());
         //--------------------------------------------------
         myMapItems.Clear();
         IMapItems leavingMembers = new MapItems();                 // some PartyMembers adbandon party when they enter town, castle, or temple
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if ((true == mi.IsTownCastleTempleLeave) && (true == myIsTownCastleTemple))
               leavingMembers.Add(mi);
            else
               myMapItems.Add(mi);
         }
         foreach (IMapItem mi in leavingMembers)
            myGameInstance.RemoveAbandonerInParty(mi);
         myMaxRowCount = myMapItems.Count;
         Logger.Log(LogEnum.LE_VIEW_SHOW_HUNT, "PerformHunt(): 2-myMapItems.Count=" + myMapItems.Count.ToString());
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myMapItems)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformHunt(): mi=null");
               return false;
            }
            myGridRows[i] = new GridRow();
            myFoodOriginal += mi.Food;
            myCoinOriginal += mi.Coin;
            myNumMounts += mi.Mounts.Count;
            if( true == mi.Name.Contains("Giant") )
               myCoinNeededForParty += 2;
            else if (true == mi.Name.Contains("Eagle")) // eagles do not require food
               myCoinNeededForParty += 0;
            else
               myCoinNeededForParty += 1;
            ++i;
         }
         //--------------------------------------------------
         myCoinCurrent = myCoinOriginal;
         myFoodCurrent = myFoodOriginal;
         //--------------------------------------------------
         IMapItem mount = new MapItem("HorseTemp", 1.0, false, false, false, "MHorse", "", myCurrentTerritory, 0, 0, 0);
         if (false == myIsTownCastleTemple)
         {
            myCoinNeededForParty = 0;
         }
         else
         {
            if (true == myGameInstance.CheapLodgings.Contains(myCurrentTerritory)) // if this is cheaper food, cost is divided by two
               myCoinNeededForParty = (int)Math.Ceiling(((double)myMapItems.Count) / 2.0);
            if (true == myGameInstance.IsPartyFed) // fed by lord of town or castle
               myCoinNeededForParty = 0;
            //-----------------------------------------------
            myGridRows[0].myAssignable = myGameInstance.Prince;
            Logger.Log(LogEnum.LE_VIEW_SHOW_HUNT, "PerformHunt(): 2-myMapItems.Count=" + myMapItems.Count.ToString());
            if ((myCoinNeededForParty <= myCoinOriginal) && (0 != myCoinNeededForParty) )// can purchase food for entire group if have enough coins
            {
               myState = HuntEnum.LE_PURCHASE;
               myGameInstance.IsPartyFed = true; // if not fed by lord, then food paid for by coin
               myTextBlockHeader.Text = "e215b Purchase Meals";
               myCoinSpentForParty = myCoinNeededForParty;
               myCoinCurrent -= myCoinSpentForParty;
               if (0 < myNumMounts)
               {
                  myGridRows[1].myAssignable = mount;
                  if (true == myGameInstance.IsMountsFed) // fed by lord of town or castle
                     myCoinNeededForMounts = 0;
                  else if (true == myGameInstance.CheapLodgings.Contains(myCurrentTerritory)) // if this is cheaper food, cost is divided by two
                     myCoinNeededForMounts = (int)Math.Ceiling(((double)myNumMounts) / 2.0);
                  else
                     myCoinNeededForMounts = myNumMounts; 
                  if (myCoinNeededForMounts <= myCoinCurrent)
                  {
                     myCoinSpentForMounts = myCoinNeededForMounts;
                     myCoinCurrent -= myCoinSpentForMounts;
                     myGameInstance.IsMountsFed = true;
                  }
               }
            }
            else
            {
               Logger.Log(LogEnum.LE_VIEW_SHOW_HUNT, "PerformHunt(): 2a-myMapItems.Count=" + myMapItems.Count.ToString() + " > myCoinOriginal=" + myCoinOriginal.ToString());
               if (null == myCallback)
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformHunt(): myCallback=null");
                  return false;
               }
               if (false == myCallback(myIsMobPursuit, myIsConstabularyPursuit))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformHunt(): myCallback() returned false");
                  return false;
               }
               return true;
            }
         }
         //--------------------------------------------------
         // If only Prince is in party
         Logger.Log(LogEnum.LE_VIEW_SHOW_HUNT, "PerformHunt(): 3-myMaxRowCount=" + myMaxRowCount.ToString());
         if (1 == myMaxRowCount)
         {
            Logger.Log(LogEnum.LE_VIEW_SHOW_HUNT, "PerformHunt():4-myMaxRowCount.Count=" + myMaxRowCount.ToString());
            myGridRows[0].myAssignable = myGameInstance.Prince;
            myGridRows[0].myAssignmentCount = GetAssignedCount();
            string miName = Utilities.RemoveSpaces(myGameInstance.Prince.Name);
            myMapItems.Remove(myGameInstance.Prince);
            if ( (6 < myGameInstance.Prince.Food) || (true == myIsFarmland) )// if prince is by himself - force user to select hunt checkbox
               myIsHeaderCheckBoxChecked = false;
            else
               myIsHeaderCheckBoxChecked = true;
         }
         //--------------------------------------------------
         Point hotPoint = new Point(Utilities.theMapItemOffset, Utilities.theMapItemOffset); // set the center of the MapItem as the hot point for the cursor
         foreach (IMapItem mi in myMapItems) // create the cursors for the party member buttons
         {
            Button b = CreateButton(mi, false, true, false);
            myCursors[mi.Name] = Utilities.ConvertToCursor(b, hotPoint);
         }
         if (0 < myNumMounts)
         {
            Button b = CreateButton(mount, false, true, false);
            myCursors[mount.Name] = Utilities.ConvertToCursor(b, hotPoint);
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformHunt(): UpdateGrid() return false");
            return false;
         }
         myScrollViewer.Content = myGridHunt;
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
         if (HuntEnum.LE_END == myState)
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
         if (HuntEnum.LE_END == myState)
         {
            myGameInstance.RemoveKilledInParty("Hunter's Death");
            int diffFood = myFoodCurrent - myFoodOriginal;  // allocate new food to party members
            myGameInstance.AddFoods(diffFood, true);
            int diffCoin = myCoinOriginal - myCoinCurrent;  // decrease coin from party members evenly
            myGameInstance.ReduceCoins(diffCoin);
            myGameInstance.Prince.OverlayImageName = "";
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            if (false == myCallback(myIsMobPursuit, myIsConstabularyPursuit))
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
            case HuntEnum.LE_HUNT:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (1 < myMaxRowCount)
                  {
                     if (false == myGameInstance.IsPartyRested)
                        myTextBlockInstructions.Inlines.Add(new Run("Unclick hunting or drag primary hunter. Roll die when ready."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Unclick hunting, drag primary hunter, or add helpers. Roll die when ready."));
                  }
                  else
                  {
                     myTextBlockInstructions.Inlines.Add(new Run("Unclick hunting or roll die when ready.")); // just prince in party
                  }
               }
               else
               {
                  myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue without hunting."));
               }
               break;
            case HuntEnum.LE_PURCHASE:
               if (true == myGameInstance.IsPartyFed)
               {
                  if (true == myGameInstance.IsPartyRested) // cannot purchase additional food stores if not rested for the day
                  {
                     int coinSpent = myCoinSpentForParty + myCoinSpentForMounts;
                     if (coinSpent < myCoinOriginal)
                        myTextBlockInstructions.Inlines.Add(new Run("Party fed. Purchase additional food store? If not, click inn to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Party fed. Unfeed or click inn to continue."));
                  }
                  else
                  {
                     myTextBlockInstructions.Inlines.Add(new Run("Party fed. Unfeed or click inn to continue."));
                  }
               }
               else
               {
                  myTextBlockInstructions.Inlines.Add(new Run("Party not fed. Purchase meals or click inn to continue."));
               }
               break;
            case HuntEnum.LE_HUNTER_HURT:
               myTextBlockInstructions.Inlines.Add(new Run("Hunter hurt. Roll die for wounds."));
               break;
            case HuntEnum.LE_MOB_CHASE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to determine mob or constabulary pursuit."));
               break;
            case HuntEnum.LE_SHOW_MOB:
               if (true == myIsMobPursuit)
                  myTextBlockInstructions.Inlines.Add(new Run("Peasant Mob Pursuit. Click anywhere to encounter."));
               else if (true == myIsConstabularyPursuit)
                  myTextBlockInstructions.Inlines.Add(new Run("Constabulary Pursuit. Click anywhere to encounter."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case HuntEnum.LE_SHOW_WOUNDS:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case HuntEnum.LE_SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue."));
               break;
            case HuntEnum.LE_END:
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateHeader()
      {
         CheckBox cb = new CheckBox() { FontSize = 12, IsEnabled = true, IsChecked = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         if (true == myIsHeaderCheckBoxChecked)
            cb.IsChecked = true;
         else
            cb.IsChecked = false;
         switch (myState)
         {
            case HuntEnum.LE_HUNT:
               myStackPanelCheckMarks.Children.Clear();
               if (true == myIsFarmland)
                  cb.Content = "Populated Region & Hunting";
               else
                  cb.Content = "Hunting";
               cb.Checked += CheckBoxHeader_Checked;
               cb.Unchecked += CheckBoxHeader_Unchecked;
               break;
            case HuntEnum.LE_PURCHASE:
               myStackPanelCheckMarks.Children.Clear();
               cb.Content = "Purchased Meals";
               cb.Checked += CheckBoxHeader_Checked;
               cb.Unchecked += CheckBoxHeader_Unchecked;
               break;
            case HuntEnum.LE_HUNTER_HURT:
               myStackPanelCheckMarks.Children.Clear();
               cb.Content = "Hunter Hurt";
               cb.IsEnabled = false;
               break;
            case HuntEnum.LE_MOB_CHASE:
            case HuntEnum.LE_SHOW_MOB:
            case HuntEnum.LE_SHOW_WOUNDS:
            case HuntEnum.LE_SHOW_DESERTERS:
            case HuntEnum.LE_SHOW_RESULTS:
            case HuntEnum.LE_END:
               cb.IsEnabled = false;
               return true;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): reached default myState=" + myState.ToString());
               return false;
         }
         myStackPanelCheckMarks.Children.Add(cb);
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case HuntEnum.LE_HUNT:
               //--------------------------------------------
               if (false == myIsHeaderCheckBoxChecked)
               {
                  BitmapImage bmi0 = new BitmapImage();
                  bmi0.BeginInit();
                  bmi0.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
                  bmi0.EndInit();
                  Image img0 = new Image { Tag = "Campfire", Source = bmi0, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img0, bmi0);
                  myStackPanelAssignable.Children.Add(img0);
               }
               else
               {
                  Rectangle r11 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r11);
               }
               Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden,Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r1);
               //-----------------------------------------------
               Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Food"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img1);
               //-----------------------------------------------
               int foodInSupply = myFoodOriginal + myFoodAdded;
               string sContent3 = "= " + foodInSupply.ToString();
               Label labelforFood3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = sContent3 };
               myStackPanelAssignable.Children.Add(labelforFood3);
               //-----------------------------------------------
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (1 < myMaxRowCount)
                  {
                     Rectangle r0 = new Rectangle()
                     {
                        Visibility = Visibility.Hidden,
                        Stroke = mySolidColorBrushBlack,
                        Fill = Brushes.Transparent,
                        StrokeThickness = 2.0,
                        StrokeDashArray = myDashArray,
                        Width = Utilities.ZOOM * Utilities.theMapItemOffset,
                        Height = Utilities.ZOOM * Utilities.theMapItemOffset
                     };
                     myStackPanelAssignable.Children.Add(r0);
                     foreach (IMapItem mi in myMapItems) // Add a button for each assignable that has not reached max
                     {
                        if (true == mi.IsUnconscious) // unconscious people cannot hunt
                           continue;
                        bool isRectangleBorderAdded = false;
                        if (null != myMapItemDragged && mi.Name == myMapItemDragged.Name) // If dragging a map item, show rectangle around that MapItem
                           isRectangleBorderAdded = true;
                        Button b = CreateButton(mi, isRectangleBorderAdded, false, false);
                        myStackPanelAssignable.Children.Add(b);
                     }
                     Rectangle r2 = new Rectangle()
                     {
                        Visibility = Visibility.Visible,
                        Stroke = mySolidColorBrushBlack,
                        Fill = Brushes.Transparent,
                        StrokeThickness = 2.0,
                        StrokeDashArray = myDashArray,
                        Width = Utilities.ZOOM * Utilities.theMapItemSize,
                        Height = Utilities.ZOOM * Utilities.theMapItemSize
                     };
                     if (myMapItems.Count < myMaxRowCount) // Add rectangle if at least one MapItem is assigned
                        myStackPanelAssignable.Children.Add(r2);
                  }
                  if (true == myGameInstance.IsEagleHunt) // if Eagle hunt, then they add one to hunt
                  {
                     Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = " +1 for " };
                     myStackPanelAssignable.Children.Add(label1);
                     Image imgEagle = new Image { Source = MapItem.theMapImages.GetBitmapImage("Eagle"), Width = 2 * Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myStackPanelAssignable.Children.Add(imgEagle);
                  }
               }
               break;
            case HuntEnum.LE_HUNTER_HURT:
               Label labelforWounds = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Wound: " };
               myStackPanelAssignable.Children.Add(labelforWounds);
               BitmapImage bmi2 = new BitmapImage();
               bmi2.BeginInit();
               bmi2.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi2.EndInit();
               Image img2 = new Image { Tag = "DieRoll", Source = bmi2, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img2, bmi2);
               myStackPanelAssignable.Children.Add(img2);
               Rectangle r3 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myStackPanelAssignable.Children.Add(r3);
               break;
            case HuntEnum.LE_SHOW_WOUNDS:
               string wounds = "Wound: " + myHunterWound.ToString();
               Label labelforWounds1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Wound: " + myHunterWound };
               myStackPanelAssignable.Children.Add(labelforWounds1);
               Rectangle r4 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myStackPanelAssignable.Children.Add(r4);
               break;
            case HuntEnum.LE_MOB_CHASE:
               Label labelForMob = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Result: " };
               myStackPanelAssignable.Children.Add(labelForMob);
               BitmapImage bmi3 = new BitmapImage();
               bmi3.BeginInit();
               bmi3.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi3.EndInit();
               Image img3 = new Image { Tag = "DieRoll", Source = bmi3, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img3, bmi3);
               myStackPanelAssignable.Children.Add(img3);
               Rectangle r5 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myStackPanelAssignable.Children.Add(r5);
               break;
            case HuntEnum.LE_SHOW_MOB:
               StringBuilder sb = new StringBuilder();
               sb.Append("Result: ");
               if (true == myIsMobPursuit)
                  sb.Append("5");
               else if (true == myIsConstabularyPursuit)
                  sb.Append("6");
               else
                  sb.Append("Escaped");
               Label labelforMob = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sb.ToString() };
               myStackPanelAssignable.Children.Add(labelforMob);
               Rectangle r6 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myStackPanelAssignable.Children.Add(r6);
               break;
            case HuntEnum.LE_SHOW_RESULTS:
               BitmapImage bmi4 = new BitmapImage();
               bmi4.BeginInit();
               bmi4.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
               bmi4.EndInit();
               Image img4 = new Image { Tag = "Campfire", Source = bmi4, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img4, bmi4);
               myStackPanelAssignable.Children.Add(img4);
               //-----------------------------------------------
               Rectangle r7 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemOffset,
                  Height = Utilities.ZOOM * Utilities.theMapItemOffset
               };
               myStackPanelAssignable.Children.Add(r7);
               //-----------------------------------------------
               Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Food"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img5);
               //-----------------------------------------------
               string sContent5 = "= " + myFoodCurrent.ToString();
               Label labelforFood5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContent5 };
               myStackPanelAssignable.Children.Add(labelforFood5);
               break;
            case HuntEnum.LE_PURCHASE:
               Image img6 = new Image { Tag = "Lodge", Source = MapItem.theMapImages.GetBitmapImage("Lodging3"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img6);
               //-----------------------------------------------
               Rectangle r8 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemOffset,
                  Height = Utilities.ZOOM * Utilities.theMapItemOffset
               };
               myStackPanelAssignable.Children.Add(r8);
               //-----------------------------------------------
               Image img7 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Food"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img7);
               //-----------------------------------------------
               string sContent6 = "= " + myFoodCurrent.ToString();
               Label labelforFood6 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = sContent6 };
               myStackPanelAssignable.Children.Add(labelforFood6);
               //-----------------------------------------------
               Rectangle r7a = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemOffset,
                  Height = Utilities.ZOOM * Utilities.theMapItemOffset
               };
               myStackPanelAssignable.Children.Add(r7a);
               //--------------------------------------------
               Image img8 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Coin"), Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img8);
               //--------------------------------------------
               string sContent8 = "= " + myCoinCurrent.ToString();
               Label labelforCoin = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContent8 };
               myStackPanelAssignable.Children.Add(labelforCoin);
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
         foreach (UIElement ui in myGridHunt.Children)
         {
            int rowNum = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGridHunt.Children.Remove(ui1);
         //------------------------------------------------------------
         if (HuntEnum.LE_PURCHASE == myState)
         {
            myGridHunt.Visibility = Visibility.Visible;
            UpdateGridRowPurchases();
            return true;
         }
         //------------------------------------------------------------
         myTextBlock0.Text = "Hunters";
         myTextBlock1.Text = "Primary";
         myTextBlock2.Text = "Helpers";
         myTextBlock3.Text = "Guides";
         myTextBlock4.Text = "Modifier";
         myTextBlock5.Text = "Roll";
         myTextBlock6.Text = "Result";
         myTextBlock3.Visibility = Visibility.Visible;
         myTextBlock4.Visibility = Visibility.Visible;
         myTextBlock5.Visibility = Visibility.Visible;
         myTextBlock5.Visibility = Visibility.Visible;
         myTextBlock6.Visibility = Visibility.Visible;
         //------------------------------------------------------------
         // Add buttons based on what is in myGridRows. 
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem hunter = myGridRows[i].myAssignable;
            if (null == hunter)// Add either Rectangle or Button for Assignable column
            {
               if (HuntEnum.LE_SHOW_RESULTS != myState) // do not add rectangles if showing final results
               {
                  myGridRows[i].myAssignmentCount = 0;
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
                  myGridHunt.Children.Add(r);
                  Grid.SetRow(r, rowNum);
                  Grid.SetColumn(r, 0);
               }
            }
            else
            {
               Logger.Log(LogEnum.LE_VIEW_SHOW_HUNT, "UpdateGridRows(): CreateButton() hunter=" + hunter.Name);
               Button b = CreateButton(hunter, false, false, true);
               myGridHunt.Children.Add(b);
               Grid.SetRow(b, rowNum);
               Grid.SetColumn(b, 0);
               if (1 == myGridRows[i].myAssignmentCount)
               {
                  if (0 < myGridRows[i].myResult)
                  {
                     Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
                     myGridHunt.Children.Add(labelforResult);
                     Grid.SetRow(labelforResult, rowNum);
                     Grid.SetColumn(labelforResult, 5);
                  }
                  else
                  {
                     CheckBox cb = new CheckBox() { FontSize = 12, IsEnabled = false, IsChecked = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                     myGridHunt.Children.Add(cb);
                     Grid.SetRow(cb, rowNum);
                     Grid.SetColumn(cb, 1);
                     if (true == myIsHeaderCheckBoxChecked)
                     {
                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                        bmi.EndInit();
                        Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                        ImageBehavior.SetAnimatedSource(img, bmi);
                        myGridHunt.Children.Add(img);
                        Grid.SetRow(img, rowNum);
                        Grid.SetColumn(img, 5);
                     }
                  }
               }
               else
               {
                  CheckBox cb = new CheckBox() { FontSize = 12, IsEnabled = false, IsChecked = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                  myGridHunt.Children.Add(cb);
                  Grid.SetRow(cb, rowNum);
                  Grid.SetColumn(cb, 2);
                  if ((true == hunter.IsGuide) && (true == hunter.GuideTerritories.Contains(myCurrentTerritory)))
                  {
                     CheckBox cb1 = new CheckBox() { FontSize = 12, IsEnabled = false, IsChecked = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                     myGridHunt.Children.Add(cb1);
                     Grid.SetRow(cb1, rowNum);
                     Grid.SetColumn(cb1, 3);
                  }
               }
            }
            UpdateModifier(rowNum, hunter);
            if (0 < myGridRows[i].myFood)
            {
               Label labelforFood = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myFood.ToString() };
               myGridHunt.Children.Add(labelforFood);
               Grid.SetRow(labelforFood, rowNum);
               Grid.SetColumn(labelforFood, 6);
            }
            Logger.Log(LogEnum.LE_VIEW_SHOW_HUNT, "UpdateGridRows(): myGameInstance.IsPartyRested=" + myGameInstance.IsPartyRested.ToString());
            if (false == myGameInstance.IsPartyRested) // if party is not rested, only show one rectangle
               break;
         }
         return true;
      }
      private void UpdateGridRowPurchases()
      {
         myTextBlock0.Text = "";
         myTextBlock1.Text = "Fed";
         myTextBlock2.Text = "Gold Spent";
         myTextBlock3.Text = "Extra Food";
         if (true == myGameInstance.IsPartyRested)
            myTextBlock3.Visibility = Visibility.Visible;
         else
            myTextBlock3.Visibility = Visibility.Hidden;
         myTextBlock4.Visibility = Visibility.Hidden;
         myTextBlock5.Visibility = Visibility.Hidden;
         myTextBlock5.Visibility = Visibility.Hidden;
         myTextBlock6.Visibility = Visibility.Hidden;
         //-------------------------------------------
         int rowNum = STARTING_ASSIGNED_ROW;
         //-------------------------------------------
         IMapItem prince = myGridRows[0].myAssignable;
         if (1 < myMapItems.Count)
            prince.OverlayImageName = "OParty";
         Button b = CreateButton(prince, false, false, true);
         myGridHunt.Children.Add(b);
         Grid.SetRow(b, rowNum);
         Grid.SetColumn(b, 0);
         //-------------------------------------------
         CheckBox cb = new CheckBox() { IsChecked = false, IsEnabled = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         if (true == myIsHeaderCheckBoxChecked)
         {
            if (true == myGameInstance.IsPartyFed)
               cb.IsChecked = true;
            else
               cb.IsChecked = false;
            if (0 < myNumMounts)
            {
               cb.Checked += CheckBoxPartyFed_Checked;
               cb.Unchecked += CheckBoxPartyFed_Unchecked;
               cb.IsEnabled = true;
            }
         }
         myGridHunt.Children.Add(cb);
         Grid.SetRow(cb, rowNum);
         Grid.SetColumn(cb, 1);
         //-------------------------------------------
         if (true == myGameInstance.IsPartyRested)
         {
            int coinSpent = myCoinSpentForParty + myCoinSpentForMounts;
            StackPanel stackpanelCol1 = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
            Button bMinus = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam, Content = " - " };
            if ((true == myIsHeaderCheckBoxChecked) && (true == cb.IsChecked))
            {
               if ((0 != myCoinSpentForParty) && (0 != myFoodAdded)) // Cannot grow to more than original
               {
                  bMinus.IsEnabled = true;
                  bMinus.Click += ButtonGold_Click;
               }
            }
            stackpanelCol1.Children.Add(bMinus);
            //-------------------------------------------
            Label labelforCoinSpent = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myCoinSpentForParty.ToString() };
            stackpanelCol1.Children.Add(labelforCoinSpent);
            Button bPlus = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam, Content = " + " };
            if ((true == myIsHeaderCheckBoxChecked) && (true == myGameInstance.IsPartyRested) && (true == cb.IsChecked))
            {
               if (0 < myCoinCurrent)
               {
                  bPlus.IsEnabled = true;
                  bPlus.Click += ButtonGold_Click;
               }
            }
            stackpanelCol1.Children.Add(bPlus);
            myGridHunt.Children.Add(stackpanelCol1);
            Grid.SetRow(stackpanelCol1, rowNum);
            Grid.SetColumn(stackpanelCol1, 2);
            //-------------------------------------------
            int extraFoodBought = 0;
            if ((true == myIsHeaderCheckBoxChecked) && (true == cb.IsChecked))
            {
               extraFoodBought = myCoinSpentForParty - myCoinNeededForParty;  // this assumes one gold per extra food
               if (true == myGameInstance.CheapLodgings.Contains(myCurrentTerritory)) // if cheap lodgings, get two food extra per coin
                  extraFoodBought *= 2;
            }
            Label labelForExtraFood = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = extraFoodBought.ToString() };
            myGridHunt.Children.Add(labelForExtraFood);
            Grid.SetRow(labelForExtraFood, rowNum);
            Grid.SetColumn(labelForExtraFood, 3);
         }
         else
         {
            Label labelforCoinSpent = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myCoinSpentForParty.ToString() };
            myGridHunt.Children.Add(labelforCoinSpent);
            Grid.SetRow(labelforCoinSpent, rowNum);
            Grid.SetColumn(labelforCoinSpent, 2);
         }
         //+++++++++++++++++++++++++++++++++++++++++++
         // Fill in Next row if there are mounts in the party
         if (0 < myNumMounts)
         {
            rowNum = STARTING_ASSIGNED_ROW + 1;
            IMapItem mount = myGridRows[1].myAssignable;
            if (1 < myNumMounts)
               mount.OverlayImageName = "OParty";
            Button b1 = CreateButton(mount, false, false, false);
            myGridHunt.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            //-------------------------------------------
            CheckBox cb1 = new CheckBox() { IsChecked = false, IsEnabled = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (true == myIsHeaderCheckBoxChecked)
            {
               if (true == myGameInstance.IsMountsFed)
               {
                  cb1.IsChecked = true;
                  cb1.IsEnabled = true;
               }
               else
               {
                  cb1.IsChecked = false;
                  if (myCoinNeededForMounts <= myCoinCurrent)
                     cb1.IsEnabled = true;
               }
               cb1.Checked += CheckBoxMountsFed_Checked;
               cb1.Unchecked += CheckBoxMountsFed_Unchecked;
            }
            myGridHunt.Children.Add(cb1);
            Grid.SetRow(cb1, rowNum);
            Grid.SetColumn(cb1, 1);
            //-------------------------------------------
            int foodForMounts = 0;
            if ((true == myIsHeaderCheckBoxChecked) && (true == cb1.IsChecked))
               foodForMounts = myCoinSpentForMounts;
            Label labelForMountsFood = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = foodForMounts.ToString() };
            myGridHunt.Children.Add(labelForMountsFood);
            Grid.SetRow(labelForMountsFood, rowNum);
            Grid.SetColumn(labelForMountsFood, 2);
         }
      }
      private void UpdateModifier(int rowNum, IMapItem hunter)
      {
         int i = rowNum - STARTING_ASSIGNED_ROW;
         myGridRows[i].myModifier = 0;
         if (null == hunter)  // null if not assigned yet
            return;
         if (1 == myGridRows[i].myAssignmentCount)
         {
            int endurance = hunter.Endurance;
            endurance -= (hunter.Wound + hunter.Poison);
            myGridRows[i].myModifier = endurance / 2;
            myGridRows[i].myModifier += hunter.Combat;
         }
         else
         {
            ++myGridRows[i].myModifier;
            if ((true == hunter.IsGuide) && (true == hunter.GuideTerritories.Contains(myCurrentTerritory)))
               ++myGridRows[i].myModifier;
         }
         Label labelforModifier = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myModifier.ToString() };
         myGridHunt.Children.Add(labelforModifier);
         Grid.SetRow(labelforModifier, rowNum);
         Grid.SetColumn(labelforModifier, 4);
      }
      //-----------------------------------------------------------------------------------------
      private Button CreateButton(IMapItem mi, bool isRectangleAdded, bool isCursor, bool isAdornments)
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
         b.IsEnabled = true;
         if ((1 < myMaxRowCount) && (false == isCursor) && (HuntEnum.LE_PURCHASE != myState)) // If more than just the prince
            b.Click += this.Button_Click;
         MapItem.SetButtonContent(b, mi, !isCursor, isAdornments); // This sets the image as the button's content
         return b;
      }
      private int GetAssignedCount()
      {
         int count = 0;
         for (int i = 0; i < myMaxRowCount; ++i) // set the default grid data
         {
            IMapItem assignable = myGridRows[i].myAssignable;
            if (null == assignable)
               continue;
            ++count;
         }
         return count;
      }
      private bool DecrementAssignmentCounts(int gridIndex)
      {
         int countNumBeingRemoved = myGridRows[gridIndex].myAssignmentCount;
         IMapItem miBeingRemoved = myGridRows[gridIndex].myAssignable;
         if (null == miBeingRemoved)
         {
            Logger.Log(LogEnum.LE_ERROR, "DecrementAssignmentCounts(): miBeingRemoved=null for row=" + gridIndex.ToString());
            return false;
         }
         // For each number at or above this number, decrement by one number
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            sb.Append(i.ToString());
            if (null == myGridRows[i].myAssignable)
            {
               sb.Append("=null\n");
            }
            else
            {
               if (i == gridIndex)
               {
                  sb.Append("=index count=0\n");
                  myGridRows[gridIndex].myAssignmentCount = 0; // set to zero if button being moved is this row
               }
               else
               {
                  sb.Append("=? n=");
                  sb.Append(miBeingRemoved.Name);
                  sb.Append(" ");
                  sb.Append(myGridRows[i].myAssignable.Name);
                  sb.Append(" ");
                  // If this value is greater than the button being removed, decrement count
                  if (countNumBeingRemoved <= myGridRows[i].myAssignmentCount)
                  {
                     --myGridRows[i].myAssignmentCount;
                     sb.Append(" --");
                  }
                  sb.Append(myGridRows[i].myAssignmentCount.ToString());
                  sb.Append("\n");
               }
            }
         }
         Logger.Log(LogEnum.LE_VIEW_DEC_COUNT_GRID, sb.ToString());
         return true;
      }
      public void ShowDieResults(int dieRoll)
      {
         if (HuntEnum.LE_HUNTER_HURT == myState)
         {
            myState = HuntEnum.LE_SHOW_WOUNDS;
            IMapItem hunter = FindHunter();
            if (null == hunter)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): hunter=null");
               return;
            }
            myHunterWound = dieRoll;
            hunter.SetWounds(dieRoll, 0);
            if ((true == hunter.IsKilled) || (true == hunter.IsUnconscious))
            {
               if(false == myGameInstance.IsPartyRested)
                  myFoodCurrent = myFoodOriginal;
               myGameInstance.RemoveKilledInParty("Died Hunting", false);
            }
            if (true == myIsFarmland)
               myState = HuntEnum.LE_MOB_CHASE;
         }
         else if (HuntEnum.LE_MOB_CHASE == myState)
         {
            myState = HuntEnum.LE_SHOW_MOB;
            if (5 == dieRoll)
               myIsMobPursuit = true;
            if (6 == dieRoll)
               myIsConstabularyPursuit = true;
         }
         else
         {
            int i = myRollResulltRowNum - STARTING_ASSIGNED_ROW;
            myGridRows[i].myResult = dieRoll;
            myFoodAdded = dieRoll;
            if (true == myGameInstance.IsEagleHunt)
               myFoodAdded += 1;
            for (int j = 0; j < myMaxRowCount; ++j)
               myFoodAdded += myGridRows[j].myModifier;
            myGridRows[i].myFood = myFoodAdded;
            myFoodCurrent = myFoodOriginal + myFoodAdded;
            myState = HuntEnum.LE_SHOW_RESULTS;
            if (12 == dieRoll)
               myState = HuntEnum.LE_HUNTER_HURT;
            else if (true == myIsFarmland)
               myState = HuntEnum.LE_MOB_CHASE;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      private IMapItem FindHunter()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            if (1 == myGridRows[i].myAssignmentCount)
               return myGridRows[i].myAssignable;
         }
         return null;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if ((HuntEnum.LE_SHOW_RESULTS == myState) || (HuntEnum.LE_SHOW_WOUNDS == myState) || (HuntEnum.LE_SHOW_MOB == myState))
         {
            myState = HuntEnum.LE_END;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGridHunt, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGridHunt.Children)
         {
            if (null != myMapItemDragged) // Do nothing unless dragging something
            {
               if (ui is StackPanel panel)
               {
                  foreach (UIElement ui1 in panel.Children)
                  {
                     if (ui1 is Rectangle rect)
                     {
                        if (result.VisualHit == rect) // First check all rectangles in the myStackPanelAssignable
                        {
                           myGridHunt.Cursor = Cursors.Arrow;
                           myMapItems.Add(myMapItemDragged);
                           myMapItemDragged = null;
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                     }
                  }
               }
               else if (ui is Rectangle rect) // next check all rectangles after the header row
               {
                  if (result.VisualHit == rect)
                  {
                     myGridHunt.Cursor = Cursors.Arrow;
                     int rowNum = Grid.GetRow(rect);
                     int i = rowNum - STARTING_ASSIGNED_ROW;
                     myGridRows[i].myAssignable = myMapItemDragged;
                     myGridRows[i].myAssignmentCount = GetAssignedCount();
                     string miName = Utilities.RemoveSpaces(myMapItemDragged.Name);
                     myMapItems.Remove(miName);
                     myMapItemDragged = null;
                     if (false == UpdateGrid())
                        Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                     return;
                  }
               }
            } // end if (null != myMapItemDragged)
            else
            {
               if (ui is StackPanel panel)
               {
                  foreach (UIElement ui1 in panel.Children)
                  {
                     if (ui1 is Image img) // Check all images within the myStackPanelAssignable
                     {
                        if (result.VisualHit == img)
                        {
                           string name = (string)img.Tag;
                           if ("Campfire" == name)
                           {
                              myState = HuntEnum.LE_END;
                           }
                           else if ("Lodge" == name)
                           {
                              myState = HuntEnum.LE_END;
                           }
                           else if ("DieRoll" == name)
                           {
                              if (false == myIsRollInProgress) // myStackPanelAssignable image is clicked
                              {
                                 myRollResulltRowNum = Grid.GetRow(img);
                                 myIsRollInProgress = true;
                                 RollEndCallback callback = ShowDieResults;
                                 int dieRoll = myDieRoller.RollMovingDie(myCanvas, callback);
                                 img.Visibility = Visibility.Hidden;
                              }
                              return;
                           }
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                     }
                  }
               }
               if (ui is Image imgRow) // next check all images within the Grid Rows
               {
                  if (result.VisualHit == imgRow)
                  {
                     if (false == myIsRollInProgress)
                     {
                        myRollResulltRowNum = Grid.GetRow(imgRow);
                        myIsRollInProgress = true;
                        RollEndCallback callback = ShowDieResults;
                        int dieRoll = myDieRoller.RollMovingDice(myCanvas, callback);
                        imgRow.Visibility = Visibility.Hidden;
                     }
                     return;
                  }
               }
            }// end else (null != myMapItemDragged)
         } // end foreach
      }
      private void Header_MouseDown(object sender, MouseButtonEventArgs e)
      {

      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         int rowNum = Grid.GetRow(b);
         if (null == myMapItemDragged)
         {
            myMapItemDragged = myMapItems.Remove(b.Name);
            if (null == myMapItemDragged) // If not in Unassigned container, look in grid row
            {
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  IMapItem mi = myGridRows[i].myAssignable;
                  if (null == mi)
                     continue;
                  if (b.Name == Utilities.RemoveSpaces(mi.Name)) // if true, found in Grid Row
                  {
                     myMapItemDragged = myGridRows[i].myAssignable;
                     if (false == DecrementAssignmentCounts(i))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Button_Click(): DecrementAssignmentCounts() returned false");
                        return;
                     }
                     myGridRows[i].myAssignable = null;
                     break;
                  }
               }
            }
            if (null == myMapItemDragged)
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): mi=null for b.Name=" + b.Name);
               return;
            }
            myGridHunt.Cursor = myCursors[myMapItemDragged.Name]; // change cursor of button being dragged
         }
         else
         {
            string miName = Utilities.RemoveSpaces(myMapItemDragged.Name);
            if (STARTING_ASSIGNED_ROW <= rowNum)
            {
               int i = rowNum - STARTING_ASSIGNED_ROW;
               if (miName == b.Name) // If true, do nothing but stop drag operation
               {
                  myGridRows[i].myAssignable = myMapItemDragged; // take position of this assignable mapitem in this row
               }
               else // replace MapItem
               {
                  if (false == DecrementAssignmentCounts(i))
                     Logger.Log(LogEnum.LE_ERROR, "Button_Click(): DecrementAssignmentCounts() returned false");
                  myMapItems.Add(myGridRows[i].myAssignable);
                  myGridRows[i].myAssignable = myMapItemDragged; // take position of this assignable mapitem in this row
                  myGridRows[i].myAssignmentCount = GetAssignedCount();
               }
            }
            else
            {
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  IMapItem mi = myGridRows[i].myAssignable;
                  if (null != mi)
                     continue;
                  myGridRows[i].myAssignable = myMapItemDragged;
                  myGridRows[i].myAssignmentCount = GetAssignedCount();
                  break;
               }
            }
            myMapItemDragged = null; // If dragging and clicked on same map item, end the dragging operation
            myGridHunt.Cursor = Cursors.Arrow;
         }
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): StateCheck() return false");
            return;
         }
      }
      private void CheckBoxHeader_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         myIsHeaderCheckBoxChecked = true;
         if (HuntEnum.LE_PURCHASE == myState)
         {
            myGameInstance.IsPartyFed = true;
            myCoinSpentForParty = myCoinNeededForParty;
            myCoinCurrent = myCoinOriginal - myCoinSpentForParty;
            if (myCoinNeededForMounts <= myCoinCurrent)
            {
               myGameInstance.IsMountsFed = true;
               myCoinSpentForMounts = myCoinNeededForMounts;
               myCoinCurrent -= myCoinSpentForMounts;
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxHeader_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         myIsHeaderCheckBoxChecked = false;
         if (HuntEnum.LE_PURCHASE == myState)
         {
            myGameInstance.IsPartyFed = false;
            myGameInstance.IsMountsFed = false;
            myFoodCurrent = myFoodOriginal;
            myCoinCurrent = myCoinOriginal;
            myCoinSpentForParty = 0;
            myCoinSpentForMounts = 0;
            myFoodAdded = 0;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Unchecked(): UpdateGrid() return false");
      }
      private void CheckBoxPartyFed_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         myGameInstance.IsPartyFed = true;
         myCoinSpentForParty = myCoinNeededForParty;
         myCoinCurrent = myCoinOriginal - myCoinSpentForParty - myCoinSpentForMounts;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxPartyFed_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxPartyFed_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         myGameInstance.IsPartyFed = false;
         myFoodCurrent = myFoodOriginal;
         myCoinCurrent = myCoinOriginal - myCoinSpentForMounts;
         myCoinSpentForParty = 0;
         myFoodAdded = 0;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxPartyFed_Unchecked(): UpdateGrid() return false");
      }
      private void CheckBoxMountsFed_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         myGameInstance.IsMountsFed = true;
         myCoinSpentForMounts = myCoinNeededForMounts;
         myCoinCurrent = myCoinOriginal - myCoinSpentForParty - myCoinSpentForMounts;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMountsFed_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxMountsFed_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         myGameInstance.IsMountsFed = false;
         myCoinCurrent = myCoinOriginal - myCoinSpentForParty;
         myCoinSpentForMounts = 0;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMountsFed_Unchecked(): UpdateGrid() return false");
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
      private void ButtonGold_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         StackPanel sp = (StackPanel)b.Parent;
         int rowNum = Grid.GetRow(sp);
         if (STARTING_ASSIGNED_ROW == rowNum)
         {
            if (" - " == (string)b.Content)
            {
               ++myCoinCurrent;
               --myCoinSpentForParty;
               --myFoodAdded;
               --myFoodCurrent;
               if (true == myGameInstance.CheapLodgings.Contains(myCurrentTerritory)) // lose two food per coin
               {
                  --myFoodAdded;
                  --myFoodCurrent;
               }
            }
            else if (" + " == (string)b.Content)
            {
               --myCoinCurrent;
               ++myCoinSpentForParty;
               ++myFoodAdded;
               ++myFoodCurrent;
               if (true == myGameInstance.CheapLodgings.Contains(myCurrentTerritory)) // get two food per coin
               {
                  ++myFoodAdded;
                  ++myFoodCurrent;
               }
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "ButtonGold_Click(): reached default content=" + (string)b.Content);
               return;
            }
            if (false == UpdateGrid())
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): StateCheck() return false");
               return;
            }
         }
         else
         {
            if (" - " == (string)b.Content)
            {
               --myCoinCurrent;
               ++myFoodAdded;
               ++myFoodCurrent;
            }
            else if (" + " == (string)b.Content)
            {
               ++myCoinCurrent;
               --myFoodAdded;
               --myFoodCurrent;
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "ButtonGold_Click(): reached default content=" + (string)b.Content);
               return;
            }
            if (false == UpdateGrid())
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): StateCheck() return false");
               return;
            }
         }
      }
   }
}
