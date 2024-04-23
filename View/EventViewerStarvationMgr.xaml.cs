using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Security.RightsManagement;
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
   public partial class EventViewerStarvationMgr : System.Windows.Controls.UserControl
   {
      public delegate bool EndFeedingCallback();
      private const int STARTING_ASSIGNED_ROW = 8;
      private const int LEAVE_AUTO = 10;
      private const int DO_NOT_LEAVE = -10;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public bool myIsDoubleMeal;
         public bool myIsHired;
         public int myResult;
         public int myWages;
         public int myGroupNum;
         public bool myIsPreviouslyRiding;
         public bool myIsPreviouslyFlying;
      };
      public enum StarveEnum
      {
         SE_PAY_HIRELINGS,
         SE_STARVE,
         SE_FEED_ALL_WITH_EXTRA,
         SE_FEED_ALL,
         SE_FEED_PEOPLE,
         SE_STARVE_PARTIAL,
         SE_ROLL_DESERTERS,
         SE_SHOW_FEED_RESULTS,
         SE_SHOW_POTIONS,
         SE_MAGICIAN_GIFT,  // only for e016a - rolling for magician gift
         SE_MAGICIAN_GIFT_SHOW,
         SE_END
      };
      private enum DragStateEnum
      {
         KEEPER_HEAL,
         SHARER_HEAL,
         KEEPER_CURE,
         SHARER_CURE,
         NONE
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      private GridRow[] myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
      //---------------------------------------------
      private StarveEnum myState = StarveEnum.SE_STARVE;
      private IMapItems myPartyMembers = null;
      private int myMaxRowCount = 0;
      private int myNumMountsCount = 0;
      private bool myIsMoreThanOneMountToMapItem = false;
      private int myFoodOriginal = 0;
      private int myFoodCurrent = 0;
      private int myCoinOriginal = 0;
      private int myCoinCurrent = 0;
      private int myTotalWages = 0;  // total wages of hirelings
      private int myNumTrueLove = 0; // if number of true loves is greater than one, it is a triangle. True Loves can leave.
      private ITerritory myTerritory = null;
      //---------------------------------------------
      private readonly Dictionary<string, Cursor> myCursors = new Dictionary<string, Cursor>();
      private DragStateEnum myDragState = DragStateEnum.NONE;
      private int myDragStateRowNum = 0;
      private int myDragStateColNum = 0;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      private EndFeedingCallback myCallback = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResulltRowNum = 0;
      private bool myIsRollInProgress = false;
      private int myRollForMagicianGift = Utilities.NO_RESULT;  // only for e016a - rolling for magician gift
      //---------------------------------------------
      private string myCheckBoxContent = "";
      private bool myIsHeaderCheckBoxChecked = false;
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerStarvationMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerStarvationMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerStarvationMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerStarvationMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerStarvationMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerStarvationMgr(): dr=null");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool FeedParty(EndFeedingCallback callback)
      {
         myPartyMembers = myGameInstance.PartyMembers.SortOnGroupNum();
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "FeedParty(): partyMembers=null");
            return false;
         }
         if (myPartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "FeedParty(): partyMembers=null");
            return false;
         }
         //--------------------------------------------------
         myState = StarveEnum.SE_STARVE;
         Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "FeedParty(): set state=" + myState.ToString());
         myFoodOriginal = 0;
         myFoodCurrent = 0;
         myNumMountsCount = 0;
         myCallback = callback;
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myCoinOriginal = 0;
         myCoinCurrent = 0;
         myTotalWages = 0;
         myIsHeaderCheckBoxChecked = false;
         myIsMoreThanOneMountToMapItem = false;
         myIsRollInProgress = false;
         myRollForMagicianGift = Utilities.NO_RESULT;
         myDragState = DragStateEnum.NONE;
         myDragStateRowNum = 0;
         myDragStateColNum = 0;
         myNumTrueLove = 0;
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myPartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "FeedParty(): mi=null");
               return false;
            }
            if (true == mi.IsSunStroke) // e121 - sunstroke removed at evening meal
            {
               mi.OverlayImageName = "";
               mi.IsSunStroke = false;
            }
            if ("Prince" == mi.Name)
               myTerritory = mi.Territory;
            if (true == mi.Name.Contains("TrueLove")) // if there is more than one true love, all but one may leave
              ++myNumTrueLove;
            if ((true == myGameInstance.IsPartyFed) || (true == mi.Name.Contains("Eagle"))) // Party if Fed if they are in a town and money was paid to feed them during the hunting phase
               mi.StarveDayNumOld = mi.StarveDayNum;
            else
               mi.StarveDayNumOld = mi.StarveDayNum + 1;
            myFoodOriginal += mi.Food;
            myCoinOriginal += mi.Coin;
            myNumMountsCount += mi.Mounts.Count;
            if (1 < mi.Mounts.Count)
               myIsMoreThanOneMountToMapItem = true;
            foreach (IMapItem mount in mi.Mounts)
            {
               if (true == myGameInstance.IsMountsFed) 
                  mount.StarveDayNumOld = mount.StarveDayNum;
               else
                  mount.StarveDayNumOld = mount.StarveDayNum + 1;
            }
            //-------------------------
            myGridRows[i] = new GridRow();
            myGridRows[i].myMapItem = mi;
            myGridRows[i].myResult = Utilities.NO_RESULT;
            myGridRows[i].myWages = 0;
            myGridRows[i].myGroupNum = mi.GroupNum;
            myGridRows[i].myIsPreviouslyRiding = mi.IsRiding;
            myGridRows[i].myIsPreviouslyFlying = mi.IsFlying;
            //-------------------------
            if ( 0 < mi.Wages ) 
            {
               if (myGameInstance.Days < mi.PayDay) 
               {
                  myGridRows[i].myIsHired = true; //Hireling already paid for today or not paid until tommorrow
               }
               else
               {
                  myGridRows[i].myWages += mi.Wages;
                  myTotalWages += mi.Wages;
               }
            }
            ++i;
         }
         myFoodCurrent = myFoodOriginal;
         myCoinCurrent = myCoinOriginal;
         //--------------------------------------------------
         if (0 < myTotalWages)
         {
            myState = StarveEnum.SE_PAY_HIRELINGS;
            if (myTotalWages <= myCoinOriginal)
               myIsHeaderCheckBoxChecked = true;
            else
               myIsHeaderCheckBoxChecked = false;
            for (int j = 0; j < myMaxRowCount; ++j)  // pay hirelings as much as possible
            {
               IMapItem mi = myGridRows[j].myMapItem;
               if ((0 < myGridRows[j].myWages) && (myGridRows[j].myWages <= myCoinCurrent))
               {
                  if( false == myGameInstance.IsMinstrelPlaying )
                     myCoinCurrent -= myGridRows[j].myWages;
                  myGridRows[j].myIsHired = true;
               }
            }
         }
         else
         {
            SetStateStarvation();
         }
         //--------------------------------------------------
         Point hotPoint = new Point(Utilities.theMapItemOffset, Utilities.theMapItemOffset); // set the center of the MapItem as the hot point for the cursor
         if (true == IsPotionHealShown())
         {
            BitmapImage bmi2 = new BitmapImage();
            bmi2.BeginInit();
            bmi2.UriSource = new Uri("../../Images/PotionHeal.gif", UriKind.Relative);
            bmi2.EndInit();
            Image img2 = new Image { Name = "PotionHeal", Source = bmi2, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
            myCursors["PotionHeal"] = Utilities.ConvertToCursor(img2, hotPoint);
         }
         //--------------------------------------------------
         if (true == IsPotionCureShown())
         {
            BitmapImage bmi3 = new BitmapImage();
            bmi3.BeginInit();
            bmi3.UriSource = new Uri("../../Images/PotionCure.gif", UriKind.Relative);
            bmi3.EndInit();
            Image img3 = new Image { Name = "PotionCure", Source = bmi3, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
            myCursors["PotionCure"] = Utilities.ConvertToCursor(img3, hotPoint);
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "FeedParty(): UpdateGrid() return false");
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
         if (StarveEnum.SE_END == myState)
            return true;
         if (1 == myPartyMembers.Count)
         {
            if (false == UpdateUserInstructionsForPrince())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructionsForPrince() returned false");
               return false;
            }
         }
         else
         {
            if (false == UpdateUserInstructionsForParty())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructionsForParty() returned false");
               return false;
            }
         }
         if (false == UpdateHeader())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid():          if (false == UpdateHeader())\r\n() returned false");
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
         if (StarveEnum.SE_END == myState)
         {
            //---------------------------------------------
            foreach (IMapItem mi in myPartyMembers) // remove mounts that have starved to death
            {
               IMapItems mountsToRemove = new MapItems();
               foreach (IMapItem mount in mi.Mounts)
               {
                  if (5 < mount.StarveDayNum)
                     mountsToRemove.Add(mount);
               }
               foreach (IMapItem removed in mountsToRemove)
                  mi.Mounts.Remove(removed.Name);
            }
            //---------------------------------------------
            int diffFood = myFoodOriginal - myFoodCurrent;
            myGameInstance.ReduceFoods(diffFood);
            //---------------------------------------------
            for (int i = 0; i < myMaxRowCount; ++i) // check for run aways
            {
               IMapItem mi = myGridRows[i].myMapItem;
               if (3 < myGridRows[i].myResult)
                  myGameInstance.RemoveAbandonerInParty(mi, true);
               if ((0 < myGridRows[i].myWages) && (false == myGridRows[i].myIsHired))
                  myGameInstance.RemoveAbandonerInParty(mi);
            }
            //---------------------------------------------
            int diffCoin = myCoinOriginal - myCoinCurrent;
            myGameInstance.ReduceCoins(diffCoin);
            //---------------------------------------------
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): myEndHuntCallback=null");
               return false;
            }
            foreach (IMapItem mi in myPartyMembers)
            {
               if ((true == mi.Name.Contains("Slave")) && (5 < mi.StarveDayNum))
                  mi.IsKilled = true;
            }
            myGameInstance.RemoveKilledInParty("Starvation");
            if (false == myCallback())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): myEndHuntCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateUserInstructionsForPrince()
      {
         string clickOption = " campfire to continue.";
         if (true == myGameInstance.IsMagicianProvideGift)
            clickOption = " magician gift to continue.";
         bool isHealingPotion = IsPotionHealShown();
         bool isCurePoisonVial = IsPotionCureShown();
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case StarveEnum.SE_FEED_ALL_WITH_EXTRA:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (0 < myNumMountsCount)
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts fed. Unfeed or click" + clickOption));
                  }
                  else
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Prince fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Prince fed. Unfeed or click" + clickOption));
                  }
               }
               else
               {
                  if (0 < myNumMountsCount)
                  {
                     if (false == myGameInstance.IsMountsFed)
                     {
                        if ((true == isHealingPotion) || (true == isCurePoisonVial))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts not fed. Feed or click potion to continue."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts not fed. Feed or click" + clickOption));
                     }
                     else
                     {
                        if ((true == isHealingPotion) || (true == isCurePoisonVial))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click potion to continue."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click" + clickOption));
                     }
                  }
                  else
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed, click potion, or click campfire."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click" + clickOption));
                  }
               }
               break;
            case StarveEnum.SE_FEED_ALL:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (0 < myNumMountsCount)
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts fed. Unfeed or click\" + clickOption"));
                  }

                  else
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Prince fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Prince fed. Unfeed or click" + clickOption));
                  }
               }
               else
               {
                  if (0 < myNumMountsCount)
                  {
                     if (false == myGameInstance.IsMountsFed)
                     {
                        if ((true == isHealingPotion) || (true == isCurePoisonVial))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts not fed. Feed or click potion to continue."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts not fed. Feed or click" + clickOption));
                     }

                     else
                     {
                        if ((true == isHealingPotion) || (true == isCurePoisonVial))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click potion to continue."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click horse to continue."));
                     }
                  }
                  else
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click" + clickOption));
                  }
               }
               break;
            case StarveEnum.SE_FEED_PEOPLE:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if ((true == isHealingPotion) || (true == isCurePoisonVial))
                     myTextBlockInstructions.Inlines.Add(new Run("Prince fed but mounts unfed. Unfeed or click potion to continue."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Prince fed but mounts unfed. Unfeed or click" + clickOption));
               }
               else
               {
                  if ((true == isHealingPotion) || (true == isCurePoisonVial))
                     myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts not fed. Feed prince or click potion to continue."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts not fed. Feed prince or click" + clickOption));
               }
               break;
            case StarveEnum.SE_STARVE_PARTIAL:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (0 < myNumMountsCount)
                  {
                     if (false == myGameInstance.IsMountsFed)
                     {
                        if ((true == isHealingPotion) || (true == isCurePoisonVial))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts partially fed. Unfeed or click potion to continue."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts partially fed. Unfeed or click" + clickOption));
                     }

                     else
                     {
                        if ((true == isHealingPotion) || (true == isCurePoisonVial))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince partially fed. Unfeed or click potion to continue."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince partially fed. Unfeed or click" + clickOption));
                     }

                  }
                  else
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Prince partially fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Prince partially fed. Unfeed or click" + clickOption));
                  }
               }
               else
               {
                  if (0 < myNumMountsCount)
                  {
                     if (false == myGameInstance.IsMountsFed)
                     {
                        if ((true == isHealingPotion) || (true == isCurePoisonVial))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts not fed. Feed or click potion to continue."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts not fed. Feed or click" + clickOption));
                     }

                     else
                     {
                        if ((true == isHealingPotion) || (true == isCurePoisonVial))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click potion to continue."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click" + clickOption));
                     }
                  }
                  else
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Prince not fed. Feed or click" + clickOption));
                  }
               }
               break;
            case StarveEnum.SE_STARVE:
               if ((true == isHealingPotion) || (true == isCurePoisonVial))
                  myTextBlockInstructions.Inlines.Add(new Run("Click potion to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue."));
               break;
            case StarveEnum.SE_SHOW_POTIONS:
               myTextBlockInstructions.Inlines.Add(new Run("Drag n Drop potion from owned/shared column to drink box."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Owned can only be dropped on same row. Click" + clickOption));
               break;
            case StarveEnum.SE_SHOW_FEED_RESULTS:
               if ((true == isHealingPotion) || (true == isCurePoisonVial))
                  myTextBlockInstructions.Inlines.Add(new Run("Click potion to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click" + clickOption));
               break;
            case StarveEnum.SE_MAGICIAN_GIFT:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die for magician gift"));
               break;
            case StarveEnum.SE_MAGICIAN_GIFT_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue."));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructionsForPrince(): reached default s=" + myState.ToString());
               break;
         }
         return true;
      }
      private bool UpdateUserInstructionsForParty()
      {
         string clickOption = " campfire to continue.";
         if (true == myGameInstance.IsMinstrelPlaying)
            clickOption = " minstrel to continue.";
         if (true == myGameInstance.IsMagicianProvideGift)
            clickOption = " magician gift to continue.";
         bool isHealingPotion = IsPotionHealShown();
         bool isCurePoisonVial = IsPotionCureShown();
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case StarveEnum.SE_PAY_HIRELINGS:
               if (true == myGameInstance.IsMinstrelPlaying)
                  myTextBlockInstructions.Inlines.Add(new Run("Hirelings listening to music. Click minstrel to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Pay hirelings or they leave. Click muscle when satisfied with hireling pay."));
               break;
            case StarveEnum.SE_FEED_ALL_WITH_EXTRA:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (0 < myNumMountsCount)
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Party & mounts fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Party & mounts fed. Unfeed or click" + clickOption));
                  }
                  else
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Party fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Party fed. Unfeed or click" + clickOption));
                  }
               }
               else
               {
                  if (0 < myNumMountsCount)
                  {
                     if (false == myGameInstance.IsMountsFed)
                     {
                        if ((true == isHealingPotion) || (true == isCurePoisonVial))
                           myTextBlockInstructions.Inlines.Add(new Run("Party & mounts not fed. Feed or roll for deserters."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Party & mounts not fed. Feed or roll for deserters."));
                     }
                     else
                     {
                        myTextBlockInstructions.Inlines.Add(new Run("Party not fed. Click to feed or roll for deserters."));
                     }
                  }
                  else
                  {
                     myTextBlockInstructions.Inlines.Add(new Run("Party not fed. Click to feed or roll for deserters."));
                  }
               }
               break;
            case StarveEnum.SE_FEED_ALL:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (0 < myNumMountsCount)
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Party & mounts fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Party & mounts fed. Unfeed or click" + clickOption));
                  }
                  else
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Party fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Party fed. Unfeed or click" + clickOption));
                  }
               }
               else
               {
                  if (0 < myNumMountsCount)
                  {
                     if (false == myGameInstance.IsMountsFed)
                        myTextBlockInstructions.Inlines.Add(new Run("Party & mounts not fed. Feed or roll for deserters."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Party not fed. Feed or roll for deserters."));
                  }
                  else
                  {
                     myTextBlockInstructions.Inlines.Add(new Run("Party not fed. Feed or roll for deserters."));
                  }
               }
               break;
            case StarveEnum.SE_FEED_PEOPLE:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if ((true == isHealingPotion) || (true == isCurePoisonVial))
                     myTextBlockInstructions.Inlines.Add(new Run("Party fed but mounts unfed. Unfeed or click potion to continue."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Party fed but mounts unfed. Unfeed or click" + clickOption));
               }
               else
               {
                  if (false == myGameInstance.IsMountsFed)
                     myTextBlockInstructions.Inlines.Add(new Run("Party & mounts not fed. Feed people or roll for deserters."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Party not fed. Feed people or roll for deserters."));
               }
               break;
            case StarveEnum.SE_STARVE_PARTIAL:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (0 < myNumMountsCount)
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Party & mounts partially fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Party & mounts partially fed. Unfeed or click" + clickOption));
                  }
                  else
                  {
                     if ((true == isHealingPotion) || (true == isCurePoisonVial))
                        myTextBlockInstructions.Inlines.Add(new Run("Party partially fed. Unfeed or click potion to continue."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Party partially fed. Unfeed or click" + clickOption));
                  }
               }
               else
               {
                  if (0 < myNumMountsCount)
                  {
                     if (false == myGameInstance.IsMountsFed)
                        myTextBlockInstructions.Inlines.Add(new Run("Party & mounts not fed. Feed or roll for deserters."));
                     else
                        myTextBlockInstructions.Inlines.Add(new Run("Party not fed. Feed or roll for deserters."));
                  }
                  else
                  {
                     myTextBlockInstructions.Inlines.Add(new Run("Party not fed. Feed or roll for deserters."));
                  }
               }
               break;
            case StarveEnum.SE_STARVE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for deserters."));
               break;
            case StarveEnum.SE_ROLL_DESERTERS:
               myTextBlockInstructions.Inlines.Add(new Run("Continue to roll for deserters."));
               break;
            case StarveEnum.SE_SHOW_POTIONS:
               myTextBlockInstructions.Inlines.Add(new Run("Drag n Drop potion from owned/shared column to drink box."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Owned can only be dropped on same row. Click" + clickOption));
               break;
            case StarveEnum.SE_SHOW_FEED_RESULTS:
               if ((true == isHealingPotion) || (true == isCurePoisonVial))
                  myTextBlockInstructions.Inlines.Add(new Run("Click potion to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click" + clickOption));
               break;
            case StarveEnum.SE_MAGICIAN_GIFT:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die for magician gift"));
               break;
            case StarveEnum.SE_MAGICIAN_GIFT_SHOW:
               if (true == myGameInstance.IsMinstrelPlaying)
                  myTextBlockInstructions.Inlines.Add(new Run("Click minstrel to continue."));
                  else
                  myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue."));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructionsForParty(): reached default s=" + myState.ToString());
               break;
         }
         return true;
      }
      private bool UpdateHeader()
      {
         myStackPanelCheckMarks.Children.Clear();
         CheckBox cb = new CheckBox() { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         if (true == myIsHeaderCheckBoxChecked)
            cb.IsChecked = true;
         else
            cb.IsChecked = false;
         switch (myState)
         {
            case StarveEnum.SE_PAY_HIRELINGS:
               int wagesToPay = myTotalWages;
               if (true == myGameInstance.IsMinstrelPlaying)
                  wagesToPay = 0;
               myCheckBoxContent = "Pay Hirelings " + wagesToPay.ToString();
               if ((myTotalWages <= myCoinOriginal) && (false == myGameInstance.IsMinstrelPlaying) )
               {
                  cb.IsEnabled = true;
                  cb.Checked += CheckBoxHeader_Checked;
                  cb.Unchecked += CheckBoxHeader_Unchecked;
               }
               else
               {
                  cb.IsEnabled = false;
               }
               break;
            case StarveEnum.SE_STARVE:
               myCheckBoxContent = "Feed Party";
               cb.IsEnabled = false;
               break;
            case StarveEnum.SE_FEED_ALL_WITH_EXTRA:
            case StarveEnum.SE_FEED_ALL:
               myCheckBoxContent = "Feed Party";
               if (false == myGameInstance.IsPartyFed) // Party if Fed if they are in a town and money was paid to feed them during the hunting phase
               {
                  cb.Checked += CheckBoxHeader_Checked;
                  cb.Unchecked += CheckBoxHeader_Unchecked;
               }
               else
               {
                  cb.IsEnabled = false;
               }
               break;
            case StarveEnum.SE_FEED_PEOPLE:
            case StarveEnum.SE_STARVE_PARTIAL:
               myCheckBoxContent = "Partial Feed";
               if (false == myGameInstance.IsPartyFed) // Party if Fed if they are in a town and money was paid to feed them during the hunting phase
               {
                  cb.Checked += CheckBoxHeader_Checked;
                  cb.Unchecked += CheckBoxHeader_Unchecked;
               }
               else
               {
                  cb.IsEnabled = false;
               }
               break;
            case StarveEnum.SE_ROLL_DESERTERS:
            case StarveEnum.SE_SHOW_FEED_RESULTS:
               cb.IsEnabled = false;
               break;
            case StarveEnum.SE_SHOW_POTIONS:
               myCheckBoxContent = "Drink Potions";
               cb.IsEnabled = false;
               cb.IsChecked = true;
               break;
            case StarveEnum.SE_MAGICIAN_GIFT:
            case StarveEnum.SE_MAGICIAN_GIFT_SHOW:
               myCheckBoxContent = "Get Magician Gift";
               cb.IsEnabled = false;
               cb.IsChecked = true;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): reached default");
               return false;
         }
         cb.Content = myCheckBoxContent;
         myStackPanelCheckMarks.Children.Add(cb);
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear();
         Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         bool isPartySizeOne = myGameInstance.IsPartySizeOne();
         switch (myState) 
         {
            case StarveEnum.SE_PAY_HIRELINGS:
               UpdateAssignablePanelShowChoices();
               UpdateAssignablePanelPersistent();
               break;
            case StarveEnum.SE_SHOW_POTIONS:
               UpdateAssignablePanelShowChoices();
               UpdateAssignablePanelPersistent();
               break;
            case StarveEnum.SE_FEED_ALL_WITH_EXTRA:
            case StarveEnum.SE_FEED_ALL:
            case StarveEnum.SE_FEED_PEOPLE:
            case StarveEnum.SE_STARVE_PARTIAL:
               if ((true == isPartySizeOne) || (true == myIsHeaderCheckBoxChecked) || (true == myGameInstance.IsMinstrelPlaying) ) // do not show potions if need to roll for desertion
                  UpdateAssignablePanelShowChoices();
               else
                  myStackPanelAssignable.Children.Add(r0);
               UpdateAssignablePanelPersistent();
               break;
            case StarveEnum.SE_STARVE:
            case StarveEnum.SE_ROLL_DESERTERS:
               if ((true == isPartySizeOne) || (true == myGameInstance.IsMinstrelPlaying))
                  UpdateAssignablePanelShowChoices();
               else
                  myStackPanelAssignable.Children.Add(r0);
               UpdateAssignablePanelPersistent();
               break;
            case StarveEnum.SE_SHOW_FEED_RESULTS:
               UpdateAssignablePanelShowChoices();
               UpdateAssignablePanelPersistent();
               break;
            case StarveEnum.SE_MAGICIAN_GIFT:
               BitmapImage bmi11 = new BitmapImage();
               bmi11.BeginInit();
               bmi11.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi11.EndInit();
               Image img11 = new Image { Source = bmi11, Name = "GiftRoll", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img11, bmi11);
               myStackPanelAssignable.Children.Add(img11);
               return true;
            case StarveEnum.SE_MAGICIAN_GIFT_SHOW:
               if (true == myGameInstance.IsMinstrelPlaying)
               {
                  Image img2 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c60Minstrel"), Name = "Campfire", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               else
               {
                  BitmapImage bmi2 = new BitmapImage();
                  bmi2.BeginInit();
                  bmi2.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
                  bmi2.EndInit();
                  Image img2 = new Image { Name = "Campfire", Source = bmi2, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img2, bmi2);
                  myStackPanelAssignable.Children.Add(img2);
               }
               return true;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               break;
         }
         return true;
      }
      private void UpdateAssignablePanelPersistent()
      {
         Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
         myStackPanelAssignable.Children.Add(r1);
         //----------------------------------------------------------------------
         Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("Food"), Name = "Food", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         myStackPanelAssignable.Children.Add(img);
         string sContent = "= " + myFoodCurrent.ToString();
         Label labelforFood = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContent };
         myStackPanelAssignable.Children.Add(labelforFood);
         //--------------------------------------------
         Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
         myStackPanelAssignable.Children.Add(r2);
         //--------------------------------------------
         Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Coin"), Name = "Coin", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         myStackPanelAssignable.Children.Add(img1);
         string sContentCoin = "= " + myCoinCurrent.ToString();
         Label labelforCoin = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContentCoin };
         myStackPanelAssignable.Children.Add(labelforCoin);
         //--------------------------------------------
         if ((false == myGameInstance.IsMinstrelPlaying) && (true == myGameInstance.IsMinstrelInParty()))
         {
            Rectangle r3 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
            myStackPanelAssignable.Children.Add(r3);
            Image img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c60Minstrel"), Name = "MinstrelStart", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            myStackPanelAssignable.Children.Add(img3);
         }
      }
      private void UpdateAssignablePanelShowChoices()
      {
         if ((true == myGameInstance.IsHirelingsInParty()) && (StarveEnum.SE_PAY_HIRELINGS == myState))
         {
            Image img0 = null;
            if (false == myGameInstance.IsMinstrelPlaying)
               img0 = new Image { Name = "Muscle", Source = MapItem.theMapImages.GetBitmapImage("Muscle"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            else
               img0 = new Image { Name = "Muscle", Source = MapItem.theMapImages.GetBitmapImage("c60Minstrel"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            myStackPanelAssignable.Children.Add(img0);
         }
         else if ((true == IsPotionHealShown()) && (StarveEnum.SE_SHOW_POTIONS != myState))
         {
            Image img1 = new Image { Name = "PotionHeal", Source = MapItem.theMapImages.GetBitmapImage("PotionHeal"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            myStackPanelAssignable.Children.Add(img1);
         }
         else if ((true == IsPotionCureShown()) && (StarveEnum.SE_SHOW_POTIONS != myState))
         {
            Image img2 = new Image { Name = "PotionCure", Source = MapItem.theMapImages.GetBitmapImage("PotionCure"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            myStackPanelAssignable.Children.Add(img2);
         }
         else
         {
            if ((false == myGameInstance.IsMagicianProvideGift) || (StarveEnum.SE_MAGICIAN_GIFT_SHOW == myState))
            {
               if (true == myGameInstance.IsMinstrelPlaying)
               {
                  Image img2 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c60Minstrel"), Name = "Campfire", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               else
               {
                  BitmapImage bmi2 = new BitmapImage();
                  bmi2.BeginInit();
                  bmi2.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
                  bmi2.EndInit();
                  Image img2 = new Image { Name = "Campfire", Source = bmi2, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img2, bmi2);
                  myStackPanelAssignable.Children.Add(img2);
               }
            }
            else
            {
               Image img2 = new Image { Name = "MagicianGift", Source = MapItem.theMapImages.GetBitmapImage("Gift"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2);
            }
         }
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
         if ((StarveEnum.SE_MAGICIAN_GIFT == myState) || (StarveEnum.SE_MAGICIAN_GIFT_SHOW == myState))
         {
            UpdateGridRowsForMagicianGift();
            return true;
         }
         if (StarveEnum.SE_PAY_HIRELINGS == myState)
         {
            myTextBlockCol1.Visibility = Visibility.Hidden;
            myTextBlockCol2.Visibility = Visibility.Hidden;
            myTextBlockCol3.Visibility = Visibility.Visible;
            myTextBlockCol4.Visibility = Visibility.Hidden;
            myTextBlockCol5.Visibility = Visibility.Visible;
            myTextBlockCol6.Visibility = Visibility.Hidden;
            myTextBlockCol7.Visibility = Visibility.Visible;
            myTextBlockCol3.Text = "Group";
            myTextBlockCol5.Text = "Wages";
            myTextBlockCol7.Text = "Paid?";
         }
         else if (StarveEnum.SE_SHOW_POTIONS == myState)
         {
            myTextBlockCol1.Visibility = Visibility.Visible;
            myTextBlockCol2.Visibility = Visibility.Visible;
            myTextBlockCol3.Visibility = Visibility.Visible;
            myTextBlockCol4.Visibility = Visibility.Visible;
            myTextBlockCol5.Visibility = Visibility.Hidden;
            myTextBlockCol6.Visibility = Visibility.Visible;
            myTextBlockCol7.Visibility = Visibility.Hidden;
            myTextBlockCol1.Text = "Owned";
            myTextBlockCol2.Text = "Owned";
            myTextBlockCol3.Text = "Shared";
            myTextBlockCol4.Text = "Shared";
            myTextBlockCol6.Text = "Drink";
         }
         else
         {
            myTextBlockCol1.Visibility = Visibility.Visible;
            myTextBlockCol2.Visibility = Visibility.Visible;
            myTextBlockCol3.Visibility = Visibility.Visible;
            myTextBlockCol4.Visibility = Visibility.Visible;
            myTextBlockCol5.Visibility = Visibility.Visible;
            myTextBlockCol6.Visibility = Visibility.Visible;
            myTextBlockCol7.Visibility = Visibility.Visible;
            myTextBlockCol1.Text = "Double?";
            myTextBlockCol2.Text = "Starve Days";
            if (true == myIsMoreThanOneMountToMapItem)
               myTextBlockCol3.Text = "Click to Rotate";
            else
               myTextBlockCol3.Text = "Mount";
            myTextBlockCol4.Text = "Feed Mount?";
            myTextBlockCol5.Text = "Max Loads";
            myTextBlockCol6.Text = "Result";
            myTextBlockCol7.Text = "Desert?";
         }
         //------------------------------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            if (StarveEnum.SE_PAY_HIRELINGS == myState)
               UpdateGridRowsHirelings(i);
            else if (StarveEnum.SE_SHOW_POTIONS == myState)
               UpdateGridRowsPotion(i);
            else
               UpdateGridRowsStarvation(i);
         }
         return true;
      }
      private void UpdateGridRowsForMagicianGift()
      {
         myTextBlockCol0.Visibility = Visibility.Hidden;
         myTextBlockCol1.Visibility = Visibility.Visible;
         myTextBlockCol2.Visibility = Visibility.Hidden;
         myTextBlockCol3.Visibility = Visibility.Visible;
         myTextBlockCol4.Visibility = Visibility.Hidden;
         myTextBlockCol5.Visibility = Visibility.Visible;
         myTextBlockCol6.Visibility = Visibility.Hidden;
         myTextBlockCol7.Visibility = Visibility.Hidden;
         myTextBlockCol0.Text = "";
         myTextBlockCol1.Text = "Die Roll";
         myTextBlockCol2.Text = "";
         myTextBlockCol3.Text = "Rule";
         myTextBlockCol4.Text = "";
         myTextBlockCol5.Text = "Item";
         myTextBlockCol6.Text = "";
         myTextBlockCol7.Text = "";
         //------------------------------------------------------------
         Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "1" };
         myGrid.Children.Add(label);
         Grid.SetRow(label, STARTING_ASSIGNED_ROW + 0);
         Grid.SetColumn(label, 1);
         System.Windows.Controls.Button b = new Button { FontFamily = myFontFam1, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "r180" };
         b.Click += ButtonRule_Click;
         myGrid.Children.Add(b);
         Grid.SetRow(b, STARTING_ASSIGNED_ROW + 0);
         Grid.SetColumn(b, 3);
         Image img = new Image { Name = "Gift", Source = MapItem.theMapImages.GetBitmapImage("PotionHeal"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(img);
         Grid.SetRow(img, STARTING_ASSIGNED_ROW + 0);
         Grid.SetColumn(img, 5);
         //------------------------------------------------------------
         Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "2" };
         myGrid.Children.Add(label1);
         Grid.SetRow(label1, STARTING_ASSIGNED_ROW + 1);
         Grid.SetColumn(label1, 1);
         System.Windows.Controls.Button b1 = new Button { FontFamily = myFontFam1, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "r186" };
         b1.Click += ButtonRule_Click;
         myGrid.Children.Add(b1);
         Grid.SetRow(b1, STARTING_ASSIGNED_ROW + 1);
         Grid.SetColumn(b1, 3);
         Image img1 = new Image { Name = "Gift", Source = MapItem.theMapImages.GetBitmapImage("Sword"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(img1);
         Grid.SetRow(img1, STARTING_ASSIGNED_ROW + 1);
         Grid.SetColumn(img1, 5);
         //------------------------------------------------------------
         Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "3" };
         myGrid.Children.Add(label2);
         Grid.SetRow(label2, STARTING_ASSIGNED_ROW + 2);
         Grid.SetColumn(label2, 1);
         System.Windows.Controls.Button b2 = new Button { FontFamily = myFontFam1, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "r187" };
         b2.Click += ButtonRule_Click;
         myGrid.Children.Add(b2);
         Grid.SetRow(b2, STARTING_ASSIGNED_ROW + 2);
         Grid.SetColumn(b2, 3);
         Image img2 = new Image { Name = "Gift", Source = MapItem.theMapImages.GetBitmapImage("AmuletAntiPoison"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(img2);
         Grid.SetRow(img2, STARTING_ASSIGNED_ROW + 2);
         Grid.SetColumn(img2, 5);
         //------------------------------------------------------------
         Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "4" };
         myGrid.Children.Add(label3);
         Grid.SetRow(label3, STARTING_ASSIGNED_ROW + 3);
         Grid.SetColumn(label3, 1);
         System.Windows.Controls.Button b3 = new Button { FontFamily = myFontFam1, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "r188" };
         b3.Click += ButtonRule_Click;
         myGrid.Children.Add(b3);
         Grid.SetRow(b3, STARTING_ASSIGNED_ROW + 3);
         Grid.SetColumn(b3, 3);
         Image img3 = new Image { Name = "Gift", Source = MapItem.theMapImages.GetBitmapImage("TalismanPegasus"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(img3);
         Grid.SetRow(img3, STARTING_ASSIGNED_ROW + 3);
         Grid.SetColumn(img3, 5);
         //------------------------------------------------------------
         Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "5" };
         myGrid.Children.Add(label4);
         Grid.SetRow(label4, STARTING_ASSIGNED_ROW + 4);
         Grid.SetColumn(label4, 1);
         System.Windows.Controls.Button b4 = new Button { FontFamily = myFontFam1, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "r190" };
         b4.Click += ButtonRule_Click;
         myGrid.Children.Add(b4);
         Grid.SetRow(b4, STARTING_ASSIGNED_ROW + 4);
         Grid.SetColumn(b4, 3);
         Image img4 = new Image { Name = "Gift", Source = MapItem.theMapImages.GetBitmapImage("NerveGasBomb"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = 0.8 * Utilities.theMapItemSize };
         myGrid.Children.Add(img4);
         Grid.SetRow(img4, STARTING_ASSIGNED_ROW + 4);
         Grid.SetColumn(img4, 5);
         //------------------------------------------------------------
         Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "6" };
         myGrid.Children.Add(label5);
         Grid.SetRow(label5, STARTING_ASSIGNED_ROW + 5);
         Grid.SetColumn(label5, 1);
         System.Windows.Controls.Button b5 = new Button { FontFamily = myFontFam1, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "r193" };
         b5.Click += ButtonRule_Click;
         myGrid.Children.Add(b5);
         Grid.SetRow(b5, STARTING_ASSIGNED_ROW + 5);
         Grid.SetColumn(b5, 3);
         Image img5 = new Image { Name = "Gift", Source = MapItem.theMapImages.GetBitmapImage("Shield"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = 0.8 * Utilities.theMapItemSize };
         myGrid.Children.Add(img5);
         Grid.SetRow(img5, STARTING_ASSIGNED_ROW + 5);
         Grid.SetColumn(img5, 5);
      }
      private void UpdateGridRowsHirelings(int i)
      {
         int rowNum = i + STARTING_ASSIGNED_ROW;
         IMapItem follower = myGridRows[i].myMapItem;
         if (null == follower)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): follower=null for rownum=" + rowNum.ToString());
            return;
         }
         //-----------------------------------------------
         if ( (0 == follower.Wages) || (myGameInstance.Days < follower.PayDay) )
            return;
         //-----------------------------------------------
         Button b = CreateButton(follower, true);
         myGrid.Children.Add(b);
         Grid.SetRow(b, rowNum);
         Grid.SetColumn(b, 0);
         //-----------------------------------------------
         if (0 != myGridRows[i].myGroupNum)
         {
            int groupNum = myGridRows[i].myGroupNum;
            if (groupNum < 0)
               groupNum = 99 + Math.Abs(myGridRows[i].myGroupNum);
            Label labelForGroup = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = groupNum.ToString() };
            myGrid.Children.Add(labelForGroup);
            Grid.SetRow(labelForGroup, rowNum);
            Grid.SetColumn(labelForGroup, 3);
         }
         //-----------------------------------------------
         Label labelForWages = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = follower.Wages.ToString() };
         myGrid.Children.Add(labelForWages);
         Grid.SetRow(labelForWages, rowNum);
         Grid.SetColumn(labelForWages, 5);
         //-----------------------------------------------
         CheckBox cb = new CheckBox() { IsEnabled = true, IsChecked = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         if (true == IsFirstMemberOfGroup(i))
         {
            if (true == myGridRows[i].myIsHired)
            {
               cb.IsChecked = true;
               if ( (0 < myGridRows[i].myWages) && (false == myGameInstance.IsMinstrelPlaying) )
                  cb.Unchecked += CheckBoxHire_Unchecked;
               else
                  cb.IsEnabled = false;
            }
            else
            {
               if ((myCoinCurrent < GetGroupWages(follower.GroupNum)) || (true == myGameInstance.IsMinstrelPlaying))
                  cb.IsEnabled = false;
               else
                  cb.Checked += CheckBoxHire_Checked;
            }
         }
         else if (true == IsPartOfGroup(i))
         {
            cb.IsEnabled = false;
            myGridRows[i].myIsHired = IsGroupHired(follower.GroupNum);
            cb.IsChecked = myGridRows[i].myIsHired;
         }
         else
         {
            if (true == myGridRows[i].myIsHired)
            {
               cb.IsChecked = true;
               if ( (0 < myGridRows[i].myWages) && (false == myGameInstance.IsMinstrelPlaying) )
                  cb.Unchecked += CheckBoxHire_Unchecked;
               else
                  cb.IsEnabled = false;
            }
            else
            {
               if ( (myCoinCurrent < myGridRows[i].myWages) || (true == myGameInstance.IsMinstrelPlaying) ) // if not hired and not enough money, disable hiring
                  cb.IsEnabled = false;
               else
                  cb.Checked += CheckBoxHire_Checked;
            }
         }
         myGrid.Children.Add(cb);
         Grid.SetRow(cb, rowNum);
         Grid.SetColumn(cb, 7);
      }
      private void UpdateGridRowsPotion(int i)
      {
         int rowNum = i + STARTING_ASSIGNED_ROW;
         IMapItem follower = myGridRows[i].myMapItem;
         if (null == follower)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): follower=null for rownum=" + rowNum.ToString());
            return;
         }
         Button b = CreateButton(follower, true);
         myGrid.Children.Add(b);
         Grid.SetRow(b, rowNum);
         Grid.SetColumn(b, 0);
         if (DragStateEnum.NONE != myDragState)
         {
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
            myGrid.Children.Add(r2);
            Grid.SetRow(r2, myDragStateRowNum);
            Grid.SetColumn(r2, myDragStateColNum);
         }
         else
         {
            if (true == IsPotionHealOwned(i))
            {
               Image img2 = new Image { Name = "PotionHeal", Source = MapItem.theMapImages.GetBitmapImage("PotionHeal"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myGrid.Children.Add(img2);
               Grid.SetRow(img2, rowNum);
               Grid.SetColumn(img2, 1);
            }
            if (true == IsPotionCureOwned(i))
            {
               Image img3 = new Image { Name = "PotionCure", Source = MapItem.theMapImages.GetBitmapImage("PotionCure"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myGrid.Children.Add(img3);
               Grid.SetRow(img3, rowNum);
               Grid.SetColumn(img3, 2);
            }
            if (true == IsPotionHealShared(i))
            {
               Image img4 = new Image { Name = "PotionHeal", Source = MapItem.theMapImages.GetBitmapImage("PotionHeal"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myGrid.Children.Add(img4);
               Grid.SetRow(img4, rowNum);
               Grid.SetColumn(img4, 3);
            }
            if (true == IsPotionCureShared(i))
            {
               Image img5 = new Image { Name = "PotionCure", Source = MapItem.theMapImages.GetBitmapImage("PotionCure"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myGrid.Children.Add(img5);
               Grid.SetRow(img5, rowNum);
               Grid.SetColumn(img5, 4);
            }
         }
         //-----------------------------------------------
         Rectangle r6 = new Rectangle()
         {
            Visibility = Visibility.Visible,
            Stroke = mySolidColorBrushBlack,
            Fill = Brushes.Transparent,
            StrokeThickness = 2.0,
            StrokeDashArray = myDashArray,
            Width = Utilities.ZOOM * Utilities.theMapItemSize,
            Height = Utilities.ZOOM * Utilities.theMapItemSize
         };
         myGrid.Children.Add(r6);
         Grid.SetRow(r6, rowNum);
         Grid.SetColumn(r6, 6);
      }
      private void UpdateGridRowsStarvation(int i)
      {
         int rowNum = i + STARTING_ASSIGNED_ROW;
         IMapItem follower = myGridRows[i].myMapItem;
         if (null == follower)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsStarvation(): follower=null for rownum=" + rowNum.ToString());
            return;
         }
         Button b = CreateButton(follower, true);
         myGrid.Children.Add(b);
         Grid.SetRow(b, rowNum);
         Grid.SetColumn(b, 0);
         //-----------------------------------------------
         CheckBox cb = new CheckBox() { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         if (false == myIsHeaderCheckBoxChecked) // do nothing if the header is unchecked
         {
            cb.IsEnabled = false;
         }
         else
         {
            if (true == myGridRows[i].myIsDoubleMeal)
            {
               cb.IsChecked = true;
            }
            else
            {
               cb.IsChecked = false;
               int minFoodNeeded = 1;
               if (true == follower.Name.Contains("Giant"))
                  minFoodNeeded += 1;
               else if (true == follower.Name.Contains("Eagle"))
                  minFoodNeeded = 0;
               if (("Desert" == myTerritory.Type) && (false == myTerritory.IsOasis)) // Double food needs if in desert
               {
                  if (true == follower.Name.Contains("Giant"))
                     minFoodNeeded += 2;
                  else if (true == follower.Name.Contains("Eagle"))
                     minFoodNeeded += 0;
                  else
                     minFoodNeeded += 1;
               }
               if ((myFoodCurrent < minFoodNeeded) || (0 == follower.StarveDayNum)) // do not enable if no food to get OR no more starve days
                  cb.IsEnabled = false;
            }
            cb.Checked += CheckBoxDouble_Checked;
            cb.Unchecked += CheckBoxDouble_Unchecked;
         }
         myGrid.Children.Add(cb);
         Grid.SetRow(cb, rowNum);
         Grid.SetColumn(cb, 1);
         //--------------------------------
         Label labelforStarveDayNum = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = follower.StarveDayNum.ToString() };
         myGrid.Children.Add(labelforStarveDayNum);
         Grid.SetRow(labelforStarveDayNum, rowNum);
         Grid.SetColumn(labelforStarveDayNum, 2);
         //--------------------------------
         if (0 == follower.Mounts.Count) // If no mounts assigned
         {
            Label labelforMounts = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "0" };
            myGrid.Children.Add(labelforMounts);
            Grid.SetRow(labelforMounts, rowNum);
            Grid.SetColumn(labelforMounts, 3);
         }
         else
         {
            IMapItem mount = myGridRows[i].myMapItem.Mounts[0]; // Show the first mount in the list
            if (5 < mount.StarveDayNum) // when carry capacity drops to zero, mount dies
               mount.IsKilled = true;
            else
               mount.IsKilled = false;
            Button b1 = CreateButton(mount, false);
            b1.Click += ButtonMount_Click;
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 3);
         }
         //--------------------------------
         CheckBox cb2 = new CheckBox() { IsChecked = false, IsEnabled = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         cb2.IsEnabled = false;
         if (0 < follower.Mounts.Count)
         {
            if (true == myGameInstance.IsMountsFed) // If fed due to grazing, no need to check this
            {
               cb2.IsChecked = true;
            }
            else
            {
               int minFoodNeeded = 2;
               if (("Desert" == myTerritory.Type) && (false == myTerritory.IsOasis)) // Double food needs if in desert
                  minFoodNeeded += 2;
               IMapItem mount = follower.Mounts[0];
               if (0 == mount.StarveDayNum) // already fed
               {
                  cb2.IsChecked = true;
                  cb2.IsEnabled = true;
                  cb2.Checked += CheckBoxMount_Checked;
                  cb2.Unchecked += CheckBoxMount_Unchecked;
               }
               else if (minFoodNeeded <= myFoodCurrent)
               {
                  cb2.IsEnabled = true;
                  cb2.Checked += CheckBoxMount_Checked;
                  cb2.Unchecked += CheckBoxMount_Unchecked;
               }
            }
         }
         myGrid.Children.Add(cb2);
         Grid.SetRow(cb2, rowNum);
         Grid.SetColumn(cb2, 4);
         //--------------------------------
         int maxLoad = Utilities.MaxLoad;
         if (true == follower.IsExhausted)
            maxLoad = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
         int load = maxLoad >> follower.StarveDayNum; // divide by half for each starve day
         if (true == follower.Name.Contains("Eagle"))
            load = 0;
         foreach (IMapItem mount in follower.Mounts)
         {
            int maxMountLoad = Utilities.MaxMountLoad;
            if (true == mount.IsExhausted)
               maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted 
            load += maxMountLoad >> mount.StarveDayNum;
         }
         Label labelforLoads = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = load.ToString() };
         myGrid.Children.Add(labelforLoads);
         Grid.SetRow(labelforLoads, rowNum);
         Grid.SetColumn(labelforLoads, 5);
         //--------------------------------
         if (false == myIsHeaderCheckBoxChecked)
         {
            if (Utilities.NO_RESULT < myGridRows[i].myResult)
            {
               string result = myGridRows[i].myResult.ToString();
               if (LEAVE_AUTO == myGridRows[i].myResult) // hirelings automatically run away if not paid
                  result = "NA";
               if (DO_NOT_LEAVE == myGridRows[i].myResult) // eagles, porters, & true love do not leave
                  result = "NA";
               Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = result };
               myGrid.Children.Add(labelforResult);
               Grid.SetRow(labelforResult, rowNum);
               Grid.SetColumn(labelforResult, 6);
            }
            else
            {
               if ( (true == follower.Name.Contains("TrueLove")) && (1 == myNumTrueLove))
               {
                  Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                  myGrid.Children.Add(labelforResult);
                  Grid.SetRow(labelforResult, rowNum);
                  Grid.SetColumn(labelforResult, 6);
               }
               else
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 6);
               }
            }
         }
         //--------------------------------
         if (Utilities.NO_RESULT < myGridRows[i].myResult)
         {
            string resultStatus = "no";
            if (3 < myGridRows[i].myResult)
               resultStatus = "yes";
            if ((true == follower.Name.Contains("Slave")) && (5 < follower.StarveDayNum) ) // slave girls and porters die when starve to death
               resultStatus = "dies";
            else if(DO_NOT_LEAVE == myGridRows[i].myResult)
               resultStatus = "NA";
            Label labelforDesertion = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = resultStatus };
            myGrid.Children.Add(labelforDesertion);
            Grid.SetRow(labelforDesertion, rowNum);
            Grid.SetColumn(labelforDesertion, 7);
         }
      }
      //-----------------------------------------------------------------------------------------
      private bool IsFirstMemberOfGroup(int i)
      {
         if (0 == myGridRows[i].myGroupNum) // group number zero is never first member of group
            return false;
         if (0 == i)
            return true;
         if (myGridRows[i].myGroupNum != myGridRows[i - 1].myGroupNum)
            return true;
         return false;
      }
      private bool IsPartOfGroup(int i)
      {
         if (0 == myGridRows[i].myGroupNum) // group number zero is never part of a group
            return false;
         if (0 == i) // if 1st row, cannot determine if member of group yet.
            return false;
         if (myGridRows[i].myGroupNum == myGridRows[i - 1].myGroupNum)
            return true;
         return false;
      }
      private bool IsGroupHired(int groupNum)
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            if (myGridRows[i].myGroupNum == groupNum)
               return myGridRows[i].myIsHired;
         }
         return false;
      }
      private int GetGroupWages(int groupNum)
      {
         int groupWages = 0;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            if (myGridRows[i].myGroupNum == groupNum)
               groupWages += myGridRows[i].myWages;
         }
         return groupWages;
      }
      private bool IsPotionHealShown()
      {
         bool isAnybodyWounded = false;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            if (0 < myGridRows[i].myMapItem.Wound)
               isAnybodyWounded = true;
         }
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myMapItem;
            foreach (SpecialEnum possession in mi.SpecialKeeps)
            {
               if ((SpecialEnum.HealingPoition == possession) && (0 < mi.Wound))
                  return true;
            }
            foreach (SpecialEnum possession in mi.SpecialShares)
            {
               if ((SpecialEnum.HealingPoition == possession) && (true == isAnybodyWounded))
                  return true;
            }
         }
         return false;
      }
      private bool IsPotionHealOwned(int i)
      {
         IMapItem mi = myGridRows[i].myMapItem;
         foreach (SpecialEnum possession in mi.SpecialKeeps)
         {
            if ((SpecialEnum.HealingPoition == possession) && (0 < mi.Wound))
               return true;
         }
         return false;
      }
      private bool IsPotionHealShared(int i)
      {
         bool isAnybodyWounded = false;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            if (0 < myGridRows[j].myMapItem.Wound)
               isAnybodyWounded = true;
         }
         IMapItem mi = myGridRows[i].myMapItem;
         foreach (SpecialEnum possession in mi.SpecialShares)
         {
            if ((SpecialEnum.HealingPoition == possession) && (true == isAnybodyWounded))
               return true;
         }
         return false;
      }
      private bool IsPotionCureShown()
      {
         bool isAnybodyPoisoned = false;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            if (0 < myGridRows[j].myMapItem.Poison)
               isAnybodyPoisoned = true;
         }
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myMapItem;
            foreach (SpecialEnum possession in mi.SpecialKeeps)
            {
               if ((SpecialEnum.CurePoisonVial == possession) && (0 < mi.Poison))
                  return true;
            }
            foreach (SpecialEnum possession in mi.SpecialShares)
            {
               if ((SpecialEnum.CurePoisonVial == possession) && (true == isAnybodyPoisoned))
                  return true;
            }
         }
         return false;
      }
      private bool IsPotionCureOwned(int i)
      {
         IMapItem mi = myGridRows[i].myMapItem;
         foreach (SpecialEnum possession in mi.SpecialKeeps)
         {
            if ((SpecialEnum.CurePoisonVial == possession) && (0 < mi.Poison))
               return true;
         }
         return false;
      }
      private bool IsPotionCureShared(int i)
      {
         bool isAnybodyPoisoned = false;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            if (0 < myGridRows[j].myMapItem.Poison)
               isAnybodyPoisoned = true;
         }
         IMapItem mi = myGridRows[i].myMapItem;
         foreach (SpecialEnum possession in mi.SpecialShares)
         {
            if ((SpecialEnum.CurePoisonVial == possession) && (true == isAnybodyPoisoned))
               return true;
         }
         return false;
      }
      private void SetStateStarvation()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myMapItem;
            if ((0 < myGridRows[i].myWages) || (0 != myGridRows[i].myGroupNum) ) // This mapitem is a hireling
            {
               if ((mi.PayDay <= myGameInstance.Days) && (false == myGridRows[i].myIsHired)) // they are leaving regardless of being fed
                  myGridRows[i].myResult = LEAVE_AUTO;
            }
            if (true == mi.Name.Contains("Prince"))
               myGridRows[i].myResult = DO_NOT_LEAVE;
            else if (true == mi.Name.Contains("Eagle"))
               myGridRows[i].myResult = DO_NOT_LEAVE;
            else if ( true == mi.Name.Contains("PorterSlave") )
               myGridRows[i].myResult = DO_NOT_LEAVE;
            else if ((true == mi.Name.Contains("TrueLove")) && (1 == myNumTrueLove) ) // If thre is only one true love, she will not leave
               myGridRows[i].myResult = DO_NOT_LEAVE;
            else if( true == myGameInstance.IsMinstrelPlaying )
               myGridRows[i].myResult = DO_NOT_LEAVE;
         }
         //-------------------------------------------------
         int foodNeededForPeople = 0;
         if (false == myGameInstance.IsPartyFed) // Party if Fed if they are in a town and money was paid to feed them during the hunting phase
         {
            foreach (IMapItem mi in myGameInstance.PartyMembers)
            {
               if (true == mi.Name.Contains("Giant"))
                  foodNeededForPeople += 2;
               else if (true == mi.Name.Contains("Eagle"))
                  foodNeededForPeople += 0;
               else
                  foodNeededForPeople += 1;
            }
         }
         int foodNeededWithExtra = 0;
         for (int i = 0; i < myMaxRowCount; ++i)  //  If have extra starve days, can remove two if extra food - only applies to people and not mounts
         {
            IMapItem mi = myGridRows[i].myMapItem;
            if (1 < mi.StarveDayNum)
            {
               if (true == mi.Name.Contains("Giant"))
                  foodNeededWithExtra += 2;
               else if (true == mi.Name.Contains("Eagle"))
                  foodNeededForPeople += 0;
               else
                  foodNeededWithExtra += 1;
            }
         }
         int foodNeededForMounts = 0;
         if (false == myGameInstance.IsMountsFed)
            foodNeededForMounts = myNumMountsCount << 1;
         if (("Desert" == myTerritory.Type) && (false == myTerritory.IsOasis)) // Double food needs if in desert
         {
            foodNeededWithExtra = foodNeededWithExtra << 1;
            foodNeededForPeople = foodNeededForPeople << 1;
            foodNeededForMounts = foodNeededForMounts << 1;
         }
         int foodNeededTotalWithExtra = foodNeededWithExtra + foodNeededForMounts + foodNeededForPeople;
         //*************************************************
         // Set State based on food needs
         if (foodNeededTotalWithExtra <= myFoodCurrent) // Feed all and eliminate extra starve days
         {
            myState = StarveEnum.SE_FEED_ALL_WITH_EXTRA;
            Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "SetStateStarvation(): set state=" + myState.ToString());
            myIsHeaderCheckBoxChecked = true;
            myFoodCurrent -= foodNeededTotalWithExtra;
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               IMapItem mi = myGridRows[i].myMapItem;
               if (1 < mi.StarveDayNum) // feeding extra removes one more starve day
               {
                  --mi.StarveDayNum;
                  myGridRows[i].myIsDoubleMeal = true;
               }
               if (0 < mi.StarveDayNum) // feeding today removes one starve day if it exists
                  --mi.StarveDayNum;
               foreach (IMapItem mount in mi.Mounts)
                  mount.StarveDayNum = 0;
            }
            return;
         }
         //-------------------------------------------------
         int foodNeededTotal = foodNeededForPeople + foodNeededForMounts;
         if (foodNeededTotal <= myFoodCurrent) // no extra feeding for people
         {
            myState = StarveEnum.SE_FEED_ALL;
            myIsHeaderCheckBoxChecked = true;
            myFoodCurrent -= foodNeededTotal;
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               IMapItem mi = myGridRows[i].myMapItem;
               if (0 < mi.StarveDayNum) // feeding today removes one stare day if it exists
                  --mi.StarveDayNum;
               foreach (IMapItem mount in mi.Mounts)
                  mount.StarveDayNum = 0;
            }
            return;
         }
         //-------------------------------------------------
         if (foodNeededForPeople <= myFoodCurrent) // Feed people but do not feed mounts
         {
            myState = StarveEnum.SE_FEED_PEOPLE;
            Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "SetStateStarvation(): set state=" + myState.ToString());
            myIsHeaderCheckBoxChecked = true;
            myFoodCurrent -= foodNeededForPeople;
            foreach (IMapItem mi in myPartyMembers)
            {
               if (0 < mi.StarveDayNum) // feeding today removes one starve day if it exists
                  --mi.StarveDayNum;
               foreach (IMapItem mount in mi.Mounts)
               {
                  if (true == myGameInstance.IsMountsFed)
                     mount.StarveDayNum = 0;
                  else
                     ++mount.StarveDayNum;
               }
            }
            return;
         }
         //-------------------------------------------------
         if (0 < myFoodCurrent) // Partial Feed
         {
            myState = StarveEnum.SE_STARVE_PARTIAL;
            myIsHeaderCheckBoxChecked = true;
            myFoodCurrent = 0;
            foreach (IMapItem mi in myPartyMembers)
            {
               foreach (IMapItem mount in mi.Mounts)
               {
                  if (true == myGameInstance.IsMountsFed)
                     mount.StarveDayNum = 0;
                  else
                     ++mount.StarveDayNum;
               }
            }
            return;
         }
         //-------------------------------------------------
         myState = StarveEnum.SE_STARVE;
         Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "SetStateStarvation(): set state=" + myState.ToString());
         myIsHeaderCheckBoxChecked = false;
         foreach (IMapItem mi in myPartyMembers)
         {
            if ((false == myGameInstance.IsPartyFed) && ( false == mi.Name.Contains("Ealge")) ) // Party if Fed if they are in a town and money was paid to feed them during the hunting phase
               ++mi.StarveDayNum;
            foreach (IMapItem mount in mi.Mounts)
            {
               if (true == myGameInstance.IsMountsFed)
                  mount.StarveDayNum = 0;
               else
                  ++mount.StarveDayNum;
            }
         }
         return;
      }
      private Button CreateButton(IMapItem mi, bool isAdornmentsShown)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         b.IsEnabled = true;
         MapItem.SetButtonContent(b, mi, true, isAdornmentsShown); // This sets the image as the button's content
         return b;
      }
      public void ShowDieResults(int dieRoll)
      {
         if (StarveEnum.SE_MAGICIAN_GIFT == myState)
         {
            myState = StarveEnum.SE_MAGICIAN_GIFT_SHOW;
            myRollForMagicianGift = dieRoll;
            SpecialEnum possession = SpecialEnum.None;
            switch (dieRoll)
            {
               case 1: possession = SpecialEnum.HealingPoition; break;
               case 2: possession = SpecialEnum.MagicSword; break;
               case 3: possession = SpecialEnum.AntiPoisonAmulet; break;
               case 4: possession = SpecialEnum.PegasusMount; break;
               case 5: possession = SpecialEnum.NerveGasBomb; break;
               case 6: possession = SpecialEnum.ShieldOfLight; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): dieRoll=" + dieRoll.ToString()); break;
            }
            if (SpecialEnum.None == possession)
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): possession=SpecialEnum.None");
            else
               myGameInstance.AddSpecialItem(possession);
         }
         else
         {
            int i = myRollResulltRowNum - STARTING_ASSIGNED_ROW;
            if (i < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
               return;
            }
            int result = dieRoll - myGameInstance.WitAndWile;
            IMapItem mi = myGridRows[i].myMapItem;
            if (true == mi.Name.Contains("TrueLove"))
            {
               dieRoll += myGameInstance.WitAndWile;
               ++dieRoll; // normalize so that a roll of < 3 causes true love to depart - normally departs when die is greater than 3. For true loves, it is when greater than 2
               if (3 < dieRoll)
                  --myNumTrueLove;
            }
            myGridRows[i].myResult = result;
            //----------------------------------------------
            if (1 == myNumTrueLove)  // If down to one true love, she does not leave
            {
               for (int k = 0; k < myMaxRowCount; ++k)
               {
                  IMapItem trueLove = myGridRows[k].myMapItem;
                  if ( (true == trueLove.Name.Contains("TrueLove")) && (Utilities.NO_RESULT == myGridRows[k].myResult) )
                     myGridRows[k].myResult = DO_NOT_LEAVE;
               }
            }
            //----------------------------------------------
            myState = StarveEnum.SE_SHOW_FEED_RESULTS;  // Assume all rolls performed unless one row shows no results
            Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "ShowDieResults(): init state=" + myState.ToString());
            for (int k = 0; k < myMaxRowCount; ++k)
            {
               IMapItem mi1 = myGridRows[k].myMapItem;
               Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "ShowDieResults(): mi=" + mi1.Name + " result=" + myGridRows[k].myResult.ToString());
               if (Utilities.NO_RESULT == myGridRows[k].myResult)  
               {
                  myState = StarveEnum.SE_ROLL_DESERTERS;
                  Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "ShowDieResults(): change state=" + myState.ToString());
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
         if (StarveEnum.SE_SHOW_FEED_RESULTS == myState)
         {
            if ((true == IsPotionHealShown()) && (true == IsPotionCureShown()))
               myState = StarveEnum.SE_SHOW_POTIONS;
            else
               myState = StarveEnum.SE_END;
            Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "Grid_MouseDown(): set state=" + myState.ToString());
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel) // Assignable Panel
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img)
                  {
                     if (result.VisualHit == img)
                     {
                        string name = (string)img.Name;
                        if ("Campfire" == name)
                        {
                           myState = StarveEnum.SE_END;
                           Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "Grid_MouseDown(): set state=" + myState.ToString());
                        }
                        else if (true == name.Contains("Potion"))
                        {
                           myState = StarveEnum.SE_SHOW_POTIONS;
                           Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "Grid_MouseDown(): set state=" + myState.ToString());
                        }
                        else if (true == name.Contains("Muscle"))
                        {
                           SetStateStarvation();
                           Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "Grid_MouseDown(): set state=" + myState.ToString());
                        }
                        else if ("MagicianGift" == name)
                        {
                           myState = StarveEnum.SE_MAGICIAN_GIFT;
                           Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "Grid_MouseDown(): set state=" + myState.ToString());
                        }
                        else if ("GiftRoll" == name)
                        {
                           myRollResulltRowNum = Grid.GetRow(img);
                           myIsRollInProgress = true;
                           int dieRoll = myDieRoller.RollMovingDie(myCanvas, ShowDieResults);
                           img.Visibility = Visibility.Hidden;
                           return;
                        }
                        else if ("MinstrelStart" == name)
                        {
                           myGameInstance.MinstrelStart();
                           for (int i = 0; i < myMaxRowCount; ++i) // all hired muscle is free
                           {
                              if (0 < myGridRows[i].myWages)
                              {
                                 if (true == myGridRows[i].myIsHired)
                                    myCoinCurrent += myGridRows[i].myWages;
                                 else
                                    myGridRows[i].myIsHired = false;
                              }
                           }
                        }
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
               }
            }
            else if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {  
                  if ("Gift" == img1.Name)
                     return;
                  if (StarveEnum.SE_SHOW_POTIONS == myState) // only images in this state are potions
                  {
                     string name = (string)img1.Name;
                     if (true == name.Contains("Potion"))
                     {
                        int rowNum = Grid.GetRow(img1);
                        int colNum = Grid.GetColumn(img1);
                        int i = rowNum - STARTING_ASSIGNED_ROW;
                        if (i < 0)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid index=" + i.ToString());
                           return;
                        }
                        IMapItem mi = myGridRows[i].myMapItem;
                        if (DragStateEnum.NONE == myDragState)
                        {
                           myDragStateRowNum = rowNum;
                           myDragStateColNum = colNum;
                           if ("PotionHeal" == name)
                           {
                              if (1 == colNum)
                              {
                                 mi.SpecialKeeps.Remove(SpecialEnum.HealingPoition);
                                 myDragState = DragStateEnum.KEEPER_HEAL;
                              }
                              else if (3 == colNum)
                              {
                                 mi.SpecialShares.Remove(SpecialEnum.HealingPoition);
                                 myDragState = DragStateEnum.SHARER_HEAL;
                              }
                              else
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid colNum=" + colNum.ToString());
                                 return;
                              }
                              myGrid.Cursor = myCursors["PotionHeal"]; // change cursor of button being dragged
                           }
                           else if ("PotionCure" == name)
                           {
                              if (2 == colNum)
                              {
                                 mi.SpecialKeeps.Remove(SpecialEnum.CurePoisonVial);
                                 myDragState = DragStateEnum.KEEPER_CURE;
                              }
                              else if (4 == colNum)
                              {
                                 mi.SpecialShares.Remove(SpecialEnum.CurePoisonVial);
                                 myDragState = DragStateEnum.SHARER_CURE;
                              }
                              else
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid colNum=" + colNum.ToString());
                                 return;
                              }
                              myGrid.Cursor = myCursors["PotionCure"]; // change cursor of button being dragged
                           }
                        }
                        else
                        {
                           switch (myDragState) // return back to original state
                           {
                              case DragStateEnum.KEEPER_HEAL:
                                 mi.AddSpecialItemToKeep(SpecialEnum.HealingPoition);
                                 break;
                              case DragStateEnum.KEEPER_CURE:
                                 mi.AddSpecialItemToKeep(SpecialEnum.CurePoisonVial);
                                 break;
                              case DragStateEnum.SHARER_HEAL:
                                 mi.AddSpecialItemToShare(SpecialEnum.HealingPoition);
                                 break;
                              case DragStateEnum.SHARER_CURE:
                                 mi.AddSpecialItemToShare(SpecialEnum.CurePoisonVial);
                                 break;
                              default:
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): reached default myDragState=" + myDragState.ToString());
                                 return;
                           }
                           myDragState = DragStateEnum.NONE;
                           myGrid.Cursor = Cursors.Arrow;
                        }
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
                  else
                  {
                     if (StarveEnum.SE_MAGICIAN_GIFT != myState)
                     {
                        if(StarveEnum.SE_ROLL_DESERTERS != myState )
                        {
                           myState = StarveEnum.SE_ROLL_DESERTERS;
                           //if (false == UpdateGrid())
                           //   Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        }
                        Logger.Log(LogEnum.LE_STARVATION_STATE_CHANGE, "Grid_MouseDown(): set state=" + myState.ToString());
                        if (false == myIsRollInProgress)
                        {
                           myRollResulltRowNum = Grid.GetRow(img1);
                           myIsRollInProgress = true;
                           int dieRoll = myDieRoller.RollMovingDice(myCanvas, ShowDieResults);
                           img1.Visibility = Visibility.Hidden;
                           return;
                        }
                     }
                     return;
                  } // end if (StarveEnum.SE_SHOW_POTIONS == myState  )
               } // end if (result.VisualHit == img1)
            }
            else if (ui is Rectangle rect) // next check all rectangles after the header row
            {
               if ((result.VisualHit == rect) && (DragStateEnum.NONE != myDragState))
               {
                  int rowNum = Grid.GetRow(rect);
                  int colNum = Grid.GetColumn(rect);
                  int i = rowNum - STARTING_ASSIGNED_ROW;
                  if (i < 0)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): 1-invalid index=" + i.ToString());
                     return;
                  }
                  IMapItem mi = myGridRows[i].myMapItem;
                  int j = myDragStateRowNum - STARTING_ASSIGNED_ROW;
                  if (j < 0)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): 2-invalid index=" + j.ToString());
                     return;
                  }
                  IMapItem miReturn = myGridRows[j].myMapItem;
                  switch (myDragState)
                  {
                     case DragStateEnum.KEEPER_HEAL:
                        if ((rowNum == myDragStateRowNum) && (6 == colNum))
                           mi.HealWounds(mi.Wound, 0);
                        else
                           miReturn.AddSpecialItemToKeep(SpecialEnum.HealingPoition);
                        break;
                     case DragStateEnum.KEEPER_CURE:
                        if ((rowNum == myDragStateRowNum) && (6 == colNum))
                           mi.HealWounds(0, mi.Poison);
                        else
                           miReturn.AddSpecialItemToKeep(SpecialEnum.CurePoisonVial);
                        break;
                     case DragStateEnum.SHARER_HEAL:
                        if (6 == colNum)
                           mi.HealWounds(mi.Wound, 0);
                        else
                           miReturn.AddSpecialItemToShare(SpecialEnum.HealingPoition);
                        break;
                     case DragStateEnum.SHARER_CURE:
                        if (6 == colNum)
                           mi.HealWounds(0, mi.Poison);
                        else
                           miReturn.AddSpecialItemToShare(SpecialEnum.CurePoisonVial);
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): reached default myDragState=" + myDragState.ToString());
                        return;
                  }
                  myGrid.Cursor = Cursors.Arrow;
                  myDragState = DragStateEnum.NONE;
                  if (false == UpdateGrid())
                     Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                  return;
               }
            }
         } // end foreach
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
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
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false key=" + key);
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
         if (1 < mi.Mounts.Count)
         {
            IMapItem mountBeingRotated = mi.Mounts[0];
            if (true == mountBeingRotated.Name.Contains("Griffon"))
            {
               mountBeingRotated.Rider.Mounts.Remove(mountBeingRotated);
               mountBeingRotated.Rider = null;
            }
         }
         mi.Mounts.Rotate(1);
         if (0 == mi.Mounts.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonMount_Click(): mi.Mounts.Count=0 for rowNum=" + rowNum);
            return;
         }
         IMapItem newMount = mi.Mounts[0];
         mi.IsRiding = myGridRows[i].myIsPreviouslyRiding;
         if (true == newMount.Name.Contains("Pegasus"))
            mi.IsFlying = myGridRows[i].myIsPreviouslyFlying;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Click(): UpdateGrid() return false");
      }
      private void CheckBoxHeader_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         myIsHeaderCheckBoxChecked = true;
         //-------------------------------------------------
         if (StarveEnum.SE_PAY_HIRELINGS == myState)
         {
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               if (0 < myGridRows[i].myWages)
               {
                  if (false == myGridRows[i].myIsHired)
                  {
                     myGridRows[i].myIsHired = true;
                     myCoinCurrent -= myGridRows[i].myWages;
                  }
               }
            }
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Checked(): UpdateGrid() return false");
            return;
         }
         //-------------------------------------------------
         int foodNeededForPeople = 0;
         if (false == myGameInstance.IsPartyFed) // Party if Fed if they are in a town and money was paid to feed them during the hunting phase
         {
            foreach (IMapItem mi in myGameInstance.PartyMembers)
            {
               if (true == mi.Name.Contains("Giant"))
                  foodNeededForPeople += 2;
               else if (true == mi.Name.Contains("Eagle"))
                  foodNeededForPeople += 0;
               else
                  foodNeededForPeople += 1;
            }
         }
         for (int i = 0; i < myMaxRowCount; ++i)  //  If have extra starve days, can remove two if extra food - only applies to people and not mounts
         {
            IMapItem mi = myGridRows[i].myMapItem;
            if (2 < mi.StarveDayNum)
            {
               if (true == mi.Name.Contains("Giant"))
                  foodNeededForPeople += 2;
               else if (true == mi.Name.Contains("Eagle"))
                  foodNeededForPeople += 0;
               else
                  foodNeededForPeople += 1;
            }
         }
         int foodNeededForMounts = 0;
         if (false == myGameInstance.IsMountsFed)
            foodNeededForMounts = myNumMountsCount << 1;
         //--------------------------------------------------------------------
         if (("Desert" == myTerritory.Type) && (false == myTerritory.IsOasis)) // Double food needs if in desert
         {
            foodNeededForPeople = foodNeededForPeople << 1;
            foodNeededForMounts = foodNeededForMounts << 1;
         }
         int foodNeededTotalWithExtra = foodNeededForMounts + foodNeededForPeople;
         int foodNeededTotal = foodNeededForPeople + foodNeededForMounts;
         //--------------------------------------------------------------------
         myFoodCurrent = myFoodOriginal;
         switch (myState)
         {
            case StarveEnum.SE_FEED_ALL_WITH_EXTRA:
               myFoodCurrent -= foodNeededTotalWithExtra;
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  IMapItem mi = myGridRows[i].myMapItem;
                  if (2 < mi.StarveDayNum) // feeding extra removes one more starve day
                  {
                     --mi.StarveDayNum;
                     myGridRows[i].myIsDoubleMeal = true;
                  }
                  if (1 < mi.StarveDayNum)
                     --mi.StarveDayNum;
                  if( false == mi.Name.Contains("Eagle"))
                     --mi.StarveDayNum;
                  foreach (IMapItem mount in mi.Mounts)
                     mount.StarveDayNum = 0;
               }
               break;
            case StarveEnum.SE_FEED_ALL:
               myFoodCurrent -= foodNeededTotal;
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  IMapItem mi = myGridRows[i].myMapItem;
                  if (1 < mi.StarveDayNum)
                     --mi.StarveDayNum;
                  if (false == mi.Name.Contains("Eagle"))
                     --mi.StarveDayNum;
                  foreach (IMapItem mount in mi.Mounts)
                     mount.StarveDayNum = 0;
               }
               break;
            case StarveEnum.SE_FEED_PEOPLE:
               myFoodCurrent -= foodNeededForPeople;
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  IMapItem mi = myGridRows[i].myMapItem;
                  if (1 < mi.StarveDayNum)
                     --mi.StarveDayNum;
                  if (false == mi.Name.Contains("Eagle"))
                     --mi.StarveDayNum;
                  foreach (IMapItem mount in mi.Mounts)
                     mount.StarveDayNum = mount.StarveDayNumOld;
               }
               break;
            case StarveEnum.SE_STARVE_PARTIAL:
               myFoodCurrent = 0;
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  IMapItem mi = myGridRows[i].myMapItem;
                  if (1 < mi.StarveDayNum)
                     --mi.StarveDayNum;
                  if (false == mi.Name.Contains("Eagle"))
                     --mi.StarveDayNum;
                  foreach (IMapItem mount in mi.Mounts)
                     mount.StarveDayNum = mount.StarveDayNumOld;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Checked(): reached default s=" + myState.ToString());
               return;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxHeader_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         myIsHeaderCheckBoxChecked = false;
         cb.IsChecked = false;
         //----------------------------------------
         switch (myState)
         {
            case StarveEnum.SE_PAY_HIRELINGS:
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  if (0 < myGridRows[i].myWages)
                  {
                     if (true == myGridRows[i].myIsHired)
                     {
                        myGridRows[i].myIsHired = false;
                        myCoinCurrent += myGridRows[i].myWages;
                     }
                  }
               }
               break;
            case StarveEnum.SE_FEED_ALL_WITH_EXTRA:
            case StarveEnum.SE_FEED_ALL:
            case StarveEnum.SE_FEED_PEOPLE:
            case StarveEnum.SE_STARVE_PARTIAL:
               myFoodCurrent = myFoodOriginal;
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  myGridRows[i].myIsDoubleMeal = false;
                  IMapItem mi = myGridRows[i].myMapItem;
                  mi.StarveDayNum = mi.StarveDayNumOld;
                  foreach (IMapItem mount in mi.Mounts)
                     mount.StarveDayNum = mount.StarveDayNumOld;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Unchecked(): reached default s=" + myState.ToString());
               return;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Unchecked(): UpdateGrid() return false");
      }
      private void CheckBoxDouble_Checked(object sender, RoutedEventArgs e)
      {
         if (false == myIsHeaderCheckBoxChecked) // do nothing if the header is unchecked
            return;
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
            return;
         int i = rowNum - STARTING_ASSIGNED_ROW;
         myGridRows[i].myIsDoubleMeal = true;
         IMapItem mi = myGridRows[i].myMapItem;
         --mi.StarveDayNum;
         if (true == mi.Name.Contains("Giant"))
            myFoodCurrent -= 2;
         else if (true == mi.Name.Contains("Eagle"))
            myFoodCurrent -= 0;
         else
            myFoodCurrent -= 1;
         if (("Desert" == myTerritory.Type) && (false == myTerritory.IsOasis)) // Double food needs if in desert
         {
            if (true == mi.Name.Contains("Giant"))
               myFoodCurrent -= 2;
            else if (true == mi.Name.Contains("Eagle"))
               myFoodCurrent -= 0;
            else
               myFoodCurrent -= 1;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxDouble_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxDouble_Unchecked(object sender, RoutedEventArgs e)
      {
         if (false == myIsHeaderCheckBoxChecked) // do nothing if the header is unchecked
            return;
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)  
            return;
         int i = rowNum - STARTING_ASSIGNED_ROW;
         myGridRows[i].myIsDoubleMeal = false;
         IMapItem mi = myGridRows[i].myMapItem;
         ++mi.StarveDayNum;
         if (true == mi.Name.Contains("Giant"))
            myFoodCurrent += 2;
         else if (true == mi.Name.Contains("Eagle"))
            myFoodCurrent += 0;
         else
            myFoodCurrent += 1;
         if (("Desert" == myTerritory.Type) && (false == myTerritory.IsOasis)) // Double food needs if in desert
         {
            if (true == mi.Name.Contains("Giant"))
               myFoodCurrent += 2;
            else if (true == mi.Name.Contains("Eagle"))
               myFoodCurrent += 0;
            else
               myFoodCurrent += 1;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxDouble_Unchecked(): UpdateGrid() return false");
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
         mount.StarveDayNum = 0;
         myFoodCurrent -= 2;
         if (("Desert" == myTerritory.Type) && (false == myTerritory.IsOasis)) // Double food needs if in desert
            myFoodCurrent -= 2;
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
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         if (0 == mi.Mounts.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMount_Unchecked(): mounts count=0 for mi=" + mi.Name);
            return;
         }
         IMapItem mount = mi.Mounts[0];
         mount.StarveDayNum = mount.StarveDayNumOld;
         myFoodCurrent += 2;
         if (("Desert" == myTerritory.Type) && (false == myTerritory.IsOasis)) // Double food needs if in desert
            myFoodCurrent += 2;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMount_Unchecked(): UpdateGrid() return false");
      }
      private void CheckBoxHire_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHire_Checked(): rowNum=" + rowNum.ToString());
            return;
         }
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         if (0 != mi.GroupNum)
         {
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               if (mi.GroupNum == myGridRows[j].myGroupNum)
                  myGridRows[i].myIsHired = true;
            }
            myCoinCurrent -= GetGroupWages(mi.GroupNum);
         }
         else
         {
            myGridRows[i].myIsHired = true;
            myCoinCurrent -= mi.Wages;
         }
         int coinDiff = myCoinOriginal - myCoinCurrent;
         if (coinDiff == myTotalWages)
            myIsHeaderCheckBoxChecked = true;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHire_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxHire_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHire_Unchecked(): rowNum=" + rowNum.ToString());
            return;
         }
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         if (0 != mi.GroupNum)
         {
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               if (mi.GroupNum == myGridRows[j].myGroupNum)
                  myGridRows[j].myIsHired = false;
            }
            myCoinCurrent += GetGroupWages(mi.GroupNum);
         }
         else
         {
            myGridRows[i].myIsHired = false;
            myCoinCurrent += mi.Wages;
         }
         myIsHeaderCheckBoxChecked = false;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHire_Unchecked(): UpdateGrid() return false");
      }
   }
}