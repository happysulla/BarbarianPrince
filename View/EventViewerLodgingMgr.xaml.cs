using System;
using System.Collections.Generic;
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
   public partial class EventViewerLodgingMgr : System.Windows.Controls.UserControl
   {
      public delegate bool EndLodgingCallback();
      private const int STARTING_ASSIGNED_ROW = 8;
      private const int DO_NOT_LEAVE = -10;
      //---------------------------------------------
      public struct MountRow
      {
         public string myName;
         public bool myIsStabled;
         public int myResult;
         public MountRow(string name, bool isStabled)
         {
            myName = name;
            myIsStabled = isStabled;
            myResult = Utilities.NO_RESULT;
         }
         public MountRow(string name, int result)
         {
            myName = name;
            myIsStabled = false;
            myResult = result;
         }
      };
      public struct GridRow
      {
         public IMapItem myMapItem;
         public bool myIsLodged;
         public int myResult;
         public List<MountRow> myMountRows;
      };
      public enum LodgingEnum
      {
         SE_LODGE_ALL,
         LE_LODGE_PEOPLE,
         LE_LODGE_NOBODY,
         LE_SHOW_DESERTERS,
         LE_SHOW_RESULTS,
         LE_SELL_GOODS,
         LE_END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private LodgingEnum myState = LodgingEnum.LE_SHOW_DESERTERS;
      private IMapItems myPartyMembers = null;
      private int myMaxRowCount = 0;
      private int myNumMountsCount = 0;
      private GridRow[] myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
      private int myCoinOriginal = 0;
      private int myCoinCurrent = 0;
      private bool myIsHeaderCheckBoxChecked = false;
      private bool myIsMoreThanOneMountToMapItem = false;
      private int myTrollSkinsInPartyOriginal = 0;
      private int myTrollSkinsInPartyCurrent = 0;
      private int myRocBeaksInPartyOriginal = 0;
      private int myRocBeaksInPartyCurrent = 0;
      private bool myIsPurchasedCloth = false; // e149 - need to purchase fine clothes 
      private bool myIsPurchasedClothShown = false;
      private int myFoodPurchasedAtFarm = 0;  // only applies to e013a when farmer sells food
      private int myHorsePurchasedAtFarm = 0;  // only applies to e013a when farmer sells food
      private int myNumTrueLove = 0; // if number of true loves is greater than one, it is a triangle. True Loves can leave.
      private int mySuitCost = 10;             // e048 - merchant can reduce cost by 1/2
      private double myFoodCost = 0.5;             // e048 - merchant can reduce cost by 1/2
      private int myChagaCost = 2;             // e048 - merchant can reduce cost by 1/2
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      private EndLodgingCallback myCallback = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResultRowNum = 0;
      private int myRollResultColNum = 0;
      private int myRollForMount = Utilities.NO_RESULT;
      private int myRollForMountNum = Utilities.NO_RESULT;   // number of horses for sale by rich farmer
      private int myRollForMountCost = Utilities.NO_RESULT;  // if horses are for sale by rich farmer, this is cost
      private bool myIsRollInProgress = false;
      private bool myIsHalfLodging = false;
      //---------------------------------------------
      private string myCheckBoxContent = "";
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerLodgingMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerLodgingMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerLodgingMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerLodgingMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerLodgingMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerLodgingMgr(): dr=null");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
      }
      public bool LodgeParty(EndLodgingCallback callback)
      {
         if (null == callback)
         {
            Logger.Log(LogEnum.LE_ERROR, "FeedParty(): callback=null");
            return false;
         }
         myCallback = callback;
         myPartyMembers = myGameInstance.PartyMembers;
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
         Option option =  myGameInstance.Options.Find("ReduceLodgingCosts");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "LodgeParty(): gi.Options.Find(ReduceLodgingCosts) returned null");
            return false;
         }
         myIsHalfLodging = option.IsEnabled;
         //--------------------------------------------------
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myState = LodgingEnum.SE_LODGE_ALL;
         myNumMountsCount = 0;
         myCoinOriginal = 0;
         myCoinCurrent = 0;
         myIsHeaderCheckBoxChecked = false;
         myIsMoreThanOneMountToMapItem = false;
         myIsPurchasedCloth = false;
         myIsPurchasedClothShown = false;
         myFoodPurchasedAtFarm = 0;
         myHorsePurchasedAtFarm = 0;
         myRollForMount = Utilities.NO_RESULT;
         myRollForMountNum = Utilities.NO_RESULT;
         myRollForMountCost = Utilities.NO_RESULT;
         myNumTrueLove = 0;
         //--------------------------------------------------
         mySuitCost = 10;
         myFoodCost = 0.5;
         myChagaCost = 2;
         if (true == myGameInstance.IsMerchantWithParty) // e048 - a negotiator in the party half the costs
         {
            mySuitCost = (int)Math.Ceiling((double)mySuitCost * 0.5);
            myFoodCost = 0.5*myFoodCost;
            myChagaCost = (int)Math.Ceiling((double)myChagaCost * 0.5);
         }
         if ((true == myGameInstance.ForbiddenAudiences.IsClothesConstraint()) && (mySuitCost <= myCoinCurrent))
            myIsPurchasedClothShown = true;
         //--------------------------------------------------
         myTrollSkinsInPartyOriginal = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            foreach (SpecialEnum item in mi.SpecialKeeps)
            {
               if (SpecialEnum.TrollSkin == item)
                  ++myTrollSkinsInPartyOriginal;
            }
            foreach (SpecialEnum item in mi.SpecialShares)
            {
               if (SpecialEnum.TrollSkin == item)
                  ++myTrollSkinsInPartyOriginal;
            }
         }
         myTrollSkinsInPartyCurrent = myTrollSkinsInPartyOriginal;
         //--------------------------------------------------
         myRocBeaksInPartyOriginal = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            foreach (SpecialEnum item in mi.SpecialKeeps)
            {
               if (SpecialEnum.RocBeak == item)
                  ++myRocBeaksInPartyOriginal;
            }
            foreach (SpecialEnum item in mi.SpecialShares)
            {
               if (SpecialEnum.RocBeak == item)
                  ++myRocBeaksInPartyOriginal;
            }
         }
         myRocBeaksInPartyCurrent = myRocBeaksInPartyOriginal;
         //--------------------------------------------------
         int i = 0;
         ITerritory t = null;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "LodgeParty(): mi=null");
               return false;
            }
            //-----------------------------------
            if( false == mi.IsFlyer() )
            {
               mi.IsRiding = false; // dismount all party members
               mi.IsFlying = false;
            }
            if (null != mi.Rider)
            {
               mi.Rider.Mounts.Remove(mi);
               mi.Rider = null;
            }
            //-----------------------------------
            if ("Prince" == mi.Name)
               t = mi.Territory;
            myCoinOriginal += mi.Coin;
            myNumMountsCount += mi.Mounts.Count;
            if (1 < mi.Mounts.Count)
               myIsMoreThanOneMountToMapItem = true;
            myGridRows[i] = new GridRow();
            myGridRows[i].myMapItem = mi;
            myGridRows[i].myResult = Utilities.NO_RESULT;
            if (true == mi.Name.Contains("TrueLove")) // if there is more than one true love, all but one may leave
               ++myNumTrueLove;
            myGridRows[i].myMountRows = new List<MountRow>();
            foreach (IMapItem mount in mi.Mounts)
            {
               MountRow mr = new MountRow(mount.Name, false);
               myGridRows[i].myMountRows.Add(mr);
            }
            ++i;
         }
         //--------------------------------------------------
         if (true == myGameInstance.IsFarmerLodging)
         {
            myButtonR013a.Visibility = Visibility.Visible;
         }
         else if (false == myGameInstance.IsInStructure(t))
         {
            Logger.Log(LogEnum.LE_ERROR, "LodgeParty(): IsInStructure() returned false & isFarmerLodging=false for invalid param t=" + t.ToString());
            return false;
         }
         myCoinCurrent = myCoinOriginal;
         //--------------------------------------------------
         SetStateInitialLodging();
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "FeedParty(): UpdateGrid() return false");
            return false;
         }
         myGrid.MouseDown += Grid_MouseDown;
         myScrollViewer.Content = myGrid;
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateEndState() return false");
            return false;
         }
         if (LodgingEnum.LE_END == myState)
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
         if (LodgingEnum.LE_END == myState)
         {
            //--------------------------------------------
            IMapItems deserters = new MapItems();
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               IMapItem partyMember = myGridRows[i].myMapItem;
               IMapItems stolenMounts = new MapItems();
               for (int k = 0; k < myGridRows[i].myMountRows.Count; ++k)
               {
                  MountRow mr = myGridRows[i].myMountRows[k];
                  if (3 < mr.myResult)
                  {
                     IMapItem mount = partyMember.Mounts.Find(mr.myName);
                     stolenMounts.Add(mount);
                  }
                  foreach (IMapItem sm in stolenMounts)
                     partyMember.Mounts.Remove(sm);
               }
               if ( (true == partyMember.Name.Contains("TrueLove")) && (2 < myGridRows[i].myResult) )  // true loves leave on die roll > 2
                  deserters.Add(partyMember);
               else if (3 < myGridRows[i].myResult)
                  deserters.Add(partyMember);
            }
            //--------------------------------------------
            foreach (IMapItem deserter in deserters)
               myGameInstance.RemoveAbandonerInParty(deserter, true);
            //--------------------------------------------
            if (myCoinCurrent < myCoinOriginal)
            {
               int diffCoin = myCoinOriginal - myCoinCurrent;
               myGameInstance.ReduceCoins(diffCoin);
            }
            else
            {
               int diffCoin = myCoinCurrent - myCoinOriginal;
               myGameInstance.AddCoins(diffCoin);
            }
            //--------------------------------------------
            myGameInstance.AddFoods(myFoodPurchasedAtFarm);
            for (int i = 0; i < myHorsePurchasedAtFarm; ++i)
            {
               if (false == myGameInstance.AddNewMountToParty())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): AddNewMountToParty() return false");
                  return false;
               }
            }
            //--------------------------------------------
            int diffRocBeaks = myRocBeaksInPartyOriginal - myRocBeaksInPartyCurrent;
            for (int k = 0; k < diffRocBeaks; ++k)
            {
               if (false == myGameInstance.RemoveSpecialItem(SpecialEnum.RocBeak))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): RemoveSpecialItem(SpecialEnum.RocBeak) return false");
                  return false;
               }
            }
            //--------------------------------------------
            int diffTrollSkins = myTrollSkinsInPartyOriginal - myTrollSkinsInPartyCurrent;
            for (int k = 0; k < diffTrollSkins; ++k )
            {
               if( false == myGameInstance.RemoveSpecialItem(SpecialEnum.TrollSkin) )
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): RemoveSpecialItem(SpecialEnum.TrollSkin) return false");
                  return false;
               }
            }
            //--------------------------------------------
            if (true == myIsPurchasedCloth)
               myGameInstance.ForbiddenAudiences.RemoveClothesConstraints();
            //--------------------------------------------
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
         string finish = "inn to continue.";
         if (true == myGameInstance.IsFarmerLodging)
            finish = "farmer to continue.";
         switch (myState)
         {
            case LodgingEnum.SE_LODGE_ALL:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (1 == myMaxRowCount)
                  {
                     if (0 < myNumMountsCount)
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent ) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts lodged. Sell/Buy items, unlodge, or click " + finish));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince & mounts lodged. Unlodge or click " + finish));
                     }
                     else
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince lodged. Sell/Buy items, unlodge, or click " + finish));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince lodged. Click " + finish));
                     }
                  }
                  else
                  {
                     if (0 < myNumMountsCount)
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Party/mounts lodged. Sell/Buy items, unlodge, or click " + finish));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Party/mounts lodged. Unlodge or click " + finish));
                     }
                     else
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Party lodged. Sell/Buy items, unlodge, or click " + finish));
                         else
                           myTextBlockInstructions.Inlines.Add(new Run("Party lodged. Unlodge or click " + finish));
                     }
                  }
               }
               else // Not everybody is lodged
               {
                  if (1 == myMaxRowCount)
                  {
                     if (0 < myNumMountsCount)
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince/Mounts not lodged. Sell/Buy items, lodge, or roll for stolen mounts."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince/Mounts not lodged. Lodge or roll for stolen mounts."));
                     }
                     else
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not lodged. Sell/Buy items, lodge, or click " + finish));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not lodged. Click " + finish));
                     }
                  }
                  else
                  {
                     if (0 < myNumMountsCount)
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Party/Mounts not lodged. Sell/Buy items,lodge, or roll for deserters/stolen mounts."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Party/Mounts not lodged. Lodge or roll for deserters/stolen mounts."));
                     }
                     else
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Party not lodged. Sell/Buy items, lodge, or roll for deserters."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Party not lodged. Lodge or roll for deserters."));
                     }
                  }
               }
               break;
            case LodgingEnum.LE_LODGE_PEOPLE:
               if (true == myIsHeaderCheckBoxChecked)
               {
                  if (1 == myMaxRowCount) // Only Prince in party
                  {
                     if (0 < myNumMountsCount)
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Mounts not stabled. Sell/Buy items, unlodge, or roll for stolen mounts."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Mounts not stabled. Unlodge or roll for stolen mounts."));
                     }
                     else
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince lodged. Sell/Buy items, unlodge, or click " + finish));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince lodged. Click " + finish));
                     }
                  }
                  else // Prince has other party members
                  {
                     if (0 < myNumMountsCount)
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Mounts not stabled. Sell/Buy items, unlodge, or roll for stolen mounts."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Mounts not stabled. Unlodge or roll for stolen mounts."));
                     }
                     else
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Party lodged. Sell/Buy items, unlodge, or click " + finish));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Party lodged. Unlodge or click " + finish));
                     }
                  }
               }
               else // people and mounts not lodged
               {
                  if (1 == myMaxRowCount)
                  {
                     if (0 < myNumMountsCount)
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Mounts not stabled. Sell/Buy items, lodge, or roll for stolen mounts."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Mounts not stabled. Lodge or roll for stolen mounts."));
                     }
                     else
                     {
                        if (true == myGameInstance.IsFarmerLodging)
                           finish = "click farmer to continue.";
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not lodged. Sell/Buy items, lodge, or click " + finish));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Prince not lodged. Lodge or click " + finish));
                     }
                  }
                  else
                  {
                     if (0 < myNumMountsCount)
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Mounts not stabled. Sell/Buy items, lodge, or roll for deserters/stolen mounts."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Mounts not stabled. Lodge or roll for deserters/stolen mounts."));
                     }
                     else
                     {
                        if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                           myTextBlockInstructions.Inlines.Add(new Run("Party not lodged. Sell/Buy items, lodge, or roll for deserters."));
                        else
                           myTextBlockInstructions.Inlines.Add(new Run("Party not lodged. Lodge or roll for deserters."));
                     }
                  }
               }
               break;
            case LodgingEnum.LE_LODGE_NOBODY:
               if (0 < myNumMountsCount)
               {
                  if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                     myTextBlockInstructions.Inlines.Add(new Run("Party/mounts not lodged. Sell/Buy items, or roll for deserters/stolen mounts."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Party/mounts not lodged. Roll for deserters or stolen mounts."));
               }
               else
               {
                  if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                     myTextBlockInstructions.Inlines.Add(new Run("Party not lodged. Sell/Buy items, or roll for deserters."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Party not lodged. Roll for deserters."));
               }
               break;
            case LodgingEnum.LE_SHOW_DESERTERS:
               if (0 < myNumMountsCount)
               {
                  if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                     myTextBlockInstructions.Inlines.Add(new Run("Not lodged. Sell/Buy items, or roll for deserters/stolen mounts."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Not lodged. Roll for deserters or stolen mounts."));
               }
               else
               {
                  if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                     myTextBlockInstructions.Inlines.Add(new Run("Party not lodged. Sell/Buy items, or roll for deserters."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Party not lodged. Roll for deserters."));
               }
               break;
            case LodgingEnum.LE_SHOW_RESULTS:
               if (true == myGameInstance.IsFarmerLodging)
                  finish = "click farmer to continue.";
               if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                  myTextBlockInstructions.Inlines.Add(new Run("Sell/Buy items, or click " + finish));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click " + finish));
               break;
            case LodgingEnum.LE_SELL_GOODS:
               if ((true == myGameInstance.IsSecretTempleKnown) || (0 < myTrollSkinsInPartyOriginal) || (0 < myRocBeaksInPartyCurrent) || (true == myIsPurchasedClothShown))
                  myTextBlockInstructions.Inlines.Add(new Run("Sell/Buy items, or click inn to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click inn to continue."));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default myState=" + myState.ToString());
               return false;
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
            case LodgingEnum.SE_LODGE_ALL:
               if (0 < myNumMountsCount)
                  myCheckBoxContent = "Party Lodged & Mounts Stabled";
               else
                  myCheckBoxContent = "Party Lodged";
               if ((false == myGameInstance.IsPartyLodged) || (false == myGameInstance.IsMountsStabled) )
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
            case LodgingEnum.LE_LODGE_PEOPLE:
               myCheckBoxContent = "Party Lodged";
               cb.IsEnabled = true;
               cb.Checked += CheckBoxHeader_Checked;
               cb.Unchecked += CheckBoxHeader_Unchecked;
               break;
            case LodgingEnum.LE_LODGE_NOBODY:
               bool isPartyLodged = true;
               bool isMountsStabled = true;
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  if (false == myGridRows[i].myIsLodged)
                     isPartyLodged = false;
                  foreach (MountRow mr in myGridRows[i].myMountRows)
                  {
                     if (false == mr.myIsStabled)
                        isMountsStabled = false;
                  }
               }
               if ((false == isPartyLodged) && (1 < myPartyMembers.Count))
               {
                  cb.IsChecked = true;
                  cb.IsEnabled = false;
                  if ((0 < myNumMountsCount) && (false == isMountsStabled))
                     myCheckBoxContent = "Roll for Deserters & Stolen Mounts";
                  else
                     myCheckBoxContent = "Roll for Deserters";
               }
               else if ((0 < myNumMountsCount) && (false == isMountsStabled))
               {
                  cb.IsChecked = true;
                  cb.IsEnabled = false;
                  myCheckBoxContent = "Roll for Stolen Mounts";
               }
               if ((true == isPartyLodged) || ((0 < myNumMountsCount) && (true == isMountsStabled)))
               {
                  cb.Checked += CheckBoxHeader_Checked;
                  cb.Unchecked += CheckBoxHeader_Unchecked;
               }
               break;
            case LodgingEnum.LE_SHOW_DESERTERS:
            case LodgingEnum.LE_SHOW_RESULTS:
               cb.IsChecked = true;
               cb.IsEnabled = false;
               if (1 < myPartyMembers.Count)
               {
                  if (0 < myNumMountsCount)
                     myCheckBoxContent = "Roll for Deserters & Stolen Mounts";
                  else
                     myCheckBoxContent = "Roll for Deserters";
               }
               else
               {
                  myCheckBoxContent = "Roll for Stolen Mounts";
               }
               break;
            case LodgingEnum.LE_SELL_GOODS:
               cb.IsChecked = true;
               cb.IsEnabled = false;
               myCheckBoxContent = "Purchase Food/Mounts";
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
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         if (LodgingEnum.LE_SELL_GOODS == myState)
         {
            Image img2 = null;
            if (true == myGameInstance.IsMinstrelPlaying)
               img2 = new Image { Name = "Lodge", Source = MapItem.theMapImages.GetBitmapImage("c60Minstrel"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            else
               img2 = new Image { Name = "Lodge", Source = MapItem.theMapImages.GetBitmapImage("Lodging3"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            myStackPanelAssignable.Children.Add(img2);
         }
         else if (false == IsLodgingRequiredForMembers())
         {
            if (true == myGameInstance.IsFarmerLodging)
            {
               Image img2 = new Image { Name = "FarmerSelling", Source = MapItem.theMapImages.GetBitmapImage("FarmerSelling"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2);
            }
            else
            {
               Image img2 = null;
               if( true == myGameInstance.IsMinstrelPlaying )
                  img2 = new Image { Name = "Lodge", Source = MapItem.theMapImages.GetBitmapImage("c60Minstrel"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
              else
                  img2 = new Image { Name = "Lodge", Source = MapItem.theMapImages.GetBitmapImage("Lodging3"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2);
            }
         }
         else // add hidden rectangle to keep same distance
         {
            Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            myStackPanelAssignable.Children.Add(r0);
         }
         //--------------------------------------------
         Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
         myStackPanelAssignable.Children.Add(r1);
         //--------------------------------------------
         Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("Coin"), Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myStackPanelAssignable.Children.Add(img);
         //--------------------------------------------
         string sContent = "= " + myCoinCurrent.ToString();
         Label labelforCoin = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContent };
         myStackPanelAssignable.Children.Add(labelforCoin);
         //--------------------------------------------
         if (true == myGameInstance.IsSecretTempleKnown)
            UpdateAssignablePanelChagaDrug();
         if (true == myIsPurchasedClothShown)
            UpdateAssignablePanelFineClothes();
         if (true == myGameInstance.IsSpecialItemHeld(SpecialEnum.TrollSkin))
            UpdateAssignablePanelTrollSkin();
         if (true == myGameInstance.IsSpecialItemHeld(SpecialEnum.RocBeak))
            UpdateAssignablePanelRocBeak();
         //--------------------------------------------
         if ((false == myGameInstance.IsMinstrelPlaying) && (true == myGameInstance.IsMinstrelInParty()))
         {
            Rectangle r3 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
            myStackPanelAssignable.Children.Add(r3);
            Image img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c60Minstrel"), Name = "MinstrelStart", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            myStackPanelAssignable.Children.Add(img3);
         }
         return true;
      }
      private void UpdateAssignablePanelTrollSkin()
      {
         //--------------------------------------------
         Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
         myStackPanelAssignable.Children.Add(r0);
         StackPanel stackpanelTrollSkin = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
         Button bMinusSkin = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
         if (0 < myTrollSkinsInPartyCurrent)
         {
            bMinusSkin.Click += ButtonSkin_Click;
            bMinusSkin.IsEnabled = true;
         }
         stackpanelTrollSkin.Children.Add(bMinusSkin);
         Image img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TrollSkin"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         stackpanelTrollSkin.Children.Add(img3);
         Button bPlusSkin = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
         int diff = myTrollSkinsInPartyOriginal - myTrollSkinsInPartyCurrent;
         if ((0 < diff) && (49 < myCoinCurrent))
         {
            bPlusSkin.Click += ButtonSkin_Click;
            bPlusSkin.IsEnabled = true;
         }
         stackpanelTrollSkin.Children.Add(bPlusSkin);
         myStackPanelAssignable.Children.Add(stackpanelTrollSkin);
         //--------------------------------------------
         string sContent = "= " + myTrollSkinsInPartyCurrent.ToString();
         Label labelForSkin = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContent };
         myStackPanelAssignable.Children.Add(labelForSkin);
      }
      private void UpdateAssignablePanelRocBeak()
      {
         //--------------------------------------------
         Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
         myStackPanelAssignable.Children.Add(r0);
         StackPanel stackpanelRocBeak = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
         Button bMinusBeak= new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
         if (0 < myRocBeaksInPartyCurrent)
         {
            bMinusBeak.Click += ButtonBeak_Click;
            bMinusBeak.IsEnabled = true;
         }
         stackpanelRocBeak.Children.Add(bMinusBeak);
         Image img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("RocBeak"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         stackpanelRocBeak.Children.Add(img3);
         Button bPlusBeak = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
         int diff = myRocBeaksInPartyOriginal - myRocBeaksInPartyCurrent;
         if ((0 < diff) && (49 < myCoinCurrent))
         {
            bPlusBeak.Click += ButtonBeak_Click;
            bPlusBeak.IsEnabled = true;
         }
         stackpanelRocBeak.Children.Add(bPlusBeak);
         myStackPanelAssignable.Children.Add(stackpanelRocBeak);
         //--------------------------------------------
         string sContent = "= " + myRocBeaksInPartyCurrent.ToString();
         Label labelForSkin = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContent };
         myStackPanelAssignable.Children.Add(labelForSkin);
      }
      private void UpdateAssignablePanelChagaDrug()
      {
         Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
         myStackPanelAssignable.Children.Add(r0);
         StackPanel stackpanelDrug = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
         Button bMinusDrug = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
         if (0 < myGameInstance.ChagaDrugCount)
         {
            bMinusDrug.Click += ButtonDrug_Click;
            bMinusDrug.IsEnabled = true;
         }
         stackpanelDrug.Children.Add(bMinusDrug);
         Image img3 = new Image { Tag = "DrugChaga", Source = MapItem.theMapImages.GetBitmapImage("DrugChaga"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         stackpanelDrug.Children.Add(img3);
         Button bPlusDrug = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
         if (myChagaCost <= myCoinCurrent)
         {
            bPlusDrug.Click += ButtonDrug_Click;
            bPlusDrug.IsEnabled = true;
         }
         stackpanelDrug.Children.Add(bPlusDrug);
         myStackPanelAssignable.Children.Add(stackpanelDrug);
         //--------------------------------------------
         string sContent = "= " + myGameInstance.ChagaDrugCount.ToString();
         Label labelForDrug = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sContent };
         myStackPanelAssignable.Children.Add(labelForDrug);
      }
      private void UpdateAssignablePanelFineClothes()
      {
         Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
         myStackPanelAssignable.Children.Add(r0);
         StackPanel stackpaneClothes = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
         Button bMinusClothes = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
         if (true == myIsPurchasedCloth)
         {
            bMinusClothes.Click += ButtonClothes_Click;
            bMinusClothes.IsEnabled = true;
         }
         stackpaneClothes.Children.Add(bMinusClothes);
         Image img3 = new Image { Tag = "Clothes", Source = MapItem.theMapImages.GetBitmapImage("FineClothes"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         stackpaneClothes.Children.Add(img3);
         Button bPlusClothes = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
         if ( (mySuitCost <= myCoinCurrent) && (false == myIsPurchasedCloth))
         {
            bPlusClothes.Click += ButtonClothes_Click;
            bPlusClothes.IsEnabled = true;
         }
         stackpaneClothes.Children.Add(bPlusClothes);
         myStackPanelAssignable.Children.Add(stackpaneClothes);
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
         if (LodgingEnum.LE_SELL_GOODS == myState)
         {
            if (false == UpdateGridRowsForSelling())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsForLodging() returned false");
               return false;
            }
         }
         else
         {
            if (false == UpdateGridRowsForLodging())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsForLodging() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateGridRowsForLodging()
      {
         if (true == myIsMoreThanOneMountToMapItem)
            myTextBlock3.Text = "Click to Rotate";
         else
            myTextBlock3.Text = "Mounts";
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): partymember=null for rownum=" + rowNum.ToString());
               return false;
            }
            //--------------------------------
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //--------------------------------
            CheckBox cb1 = new CheckBox() { IsChecked = false, IsEnabled = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            cb1.IsChecked = myGridRows[i].myIsLodged;
            myGrid.Children.Add(cb1);
            Grid.SetRow(cb1, rowNum);
            Grid.SetColumn(cb1, 1);
            //--------------------------------
            // Column #2 - Die Rolls for Deserters
            if (false == cb1.IsChecked)
            {
               if (Utilities.NO_RESULT < myGridRows[i].myResult)
               {
                  string result = myGridRows[i].myResult.ToString();
                  if (DO_NOT_LEAVE == myGridRows[i].myResult) // porters & true love do not leave
                     result = "NA";
                  Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = result };
                  myGrid.Children.Add(labelforResult);
                  Grid.SetRow(labelforResult, rowNum);
                  Grid.SetColumn(labelforResult, 2);
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
                  Grid.SetColumn(img, 2);
               }
            }
            else
            {
               if (DO_NOT_LEAVE == myGridRows[i].myResult)
               {
                  Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                  myGrid.Children.Add(labelforResult);
                  Grid.SetRow(labelforResult, rowNum);
                  Grid.SetColumn(labelforResult, 2);
               }
            }
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
               foreach (MountRow mr in myGridRows[i].myMountRows)
               {
                  if (mr.myName == mount.Name)
                  {
                     if (true == mr.myIsStabled)
                        cb2.IsChecked = true;
                     break;
                  }
               }
               if (((0 < myCoinCurrent) || (true == cb2.IsChecked)) && ((LodgingEnum.LE_SHOW_DESERTERS != myState) && (LodgingEnum.LE_SHOW_RESULTS != myState))) // Cannot change once start rolling
               {
                  if (false == myGameInstance.IsMountsStabled)
                  {
                     cb2.Checked += CheckBoxMount_Checked;
                     cb2.Unchecked += CheckBoxMount_Unchecked;
                     cb2.IsEnabled = true;
                  }
               }
               myGrid.Children.Add(cb2);
               Grid.SetRow(cb2, rowNum);
               Grid.SetColumn(cb2, 4);
               //--------------------------------
               // Column 5 - Die Rolls for Mounts
               if (false == cb2.IsChecked)
               {
                  int mountResult = Utilities.NO_RESULT;
                  foreach (MountRow mr in myGridRows[i].myMountRows)
                  {
                     if (mount.Name == mr.myName)
                        mountResult = mr.myResult;
                  }
                  if (true == cb2.IsChecked)
                  {
                     Label labelforStabled = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                     myGrid.Children.Add(labelforStabled);
                     Grid.SetRow(labelforStabled, rowNum);
                     Grid.SetColumn(labelforStabled, 5);
                  }
                  else if (Utilities.NO_RESULT < mountResult)
                  {
                     Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = mountResult.ToString() };
                     myGrid.Children.Add(labelforResult);
                     Grid.SetRow(labelforResult, rowNum);
                     Grid.SetColumn(labelforResult, 5);
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
                     Grid.SetColumn(img, 5);
                  }
               }
            }
         }
         return true;
      }
      private bool UpdateGridRowsForSelling()
      {
         myTextBlock0.Visibility = Visibility.Visible;
         myTextBlock1.Visibility = Visibility.Visible;
         myTextBlock2.Visibility = Visibility.Visible;
         myTextBlock3.Visibility = Visibility.Visible;
         myTextBlock4.Visibility = Visibility.Hidden;
         myTextBlock5.Visibility = Visibility.Visible;
         myTextBlock0.Text = "Item";
         myTextBlock1.Text = "Stables?";
         myTextBlock2.Text = "# to Sell?";
         myTextBlock3.Text = "Cost";
         myTextBlock4.Text = "";
         myTextBlock5.Text = "Purchases";
         //----------------------------------------
         Image imgFood = new Image { Source = MapItem.theMapImages.GetBitmapImage("Food"), Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(imgFood);
         Grid.SetRow(imgFood, STARTING_ASSIGNED_ROW);
         Grid.SetColumn(imgFood, 0);
         Label labelNotApply = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
         myGrid.Children.Add(labelNotApply);
         Grid.SetRow(labelNotApply, STARTING_ASSIGNED_ROW);
         Grid.SetColumn(labelNotApply, 1);
         Label labelNotApply1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
         myGrid.Children.Add(labelNotApply1);
         Grid.SetRow(labelNotApply1, STARTING_ASSIGNED_ROW);
         Grid.SetColumn(labelNotApply1, 2);
         Label labelforFoodCost = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myFoodCost.ToString() };
         myGrid.Children.Add(labelforFoodCost);
         Grid.SetRow(labelforFoodCost, STARTING_ASSIGNED_ROW);
         Grid.SetColumn(labelforFoodCost, 3);
         //------------------------------------------------
         StackPanel stackpanelFood = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
         Button bMinusFood = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
         if (0 < myFoodPurchasedAtFarm)
         {
            bMinusFood.Click += ButtonFood_Click;
            bMinusFood.IsEnabled = true;
         }
         stackpanelFood.Children.Add(bMinusFood);
         Label labelforFood = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myFoodPurchasedAtFarm.ToString() };
         stackpanelFood.Children.Add(labelforFood);
         Button bPlusFood = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
         int normalizedFoodCost = (int)Math.Ceiling(myFoodCost);
         if (normalizedFoodCost <= myCoinCurrent)
         {
            bPlusFood.Click += ButtonFood_Click;
            bPlusFood.IsEnabled = true;
         }
         stackpanelFood.Children.Add(bPlusFood);
         myGrid.Children.Add(stackpanelFood);
         Grid.SetRow(stackpanelFood, STARTING_ASSIGNED_ROW);
         Grid.SetColumn(stackpanelFood, 5);
         //------------------------------------------------
         Image imgMount = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mount"), Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(imgMount);
         Grid.SetRow(imgMount, STARTING_ASSIGNED_ROW + 1);
         Grid.SetColumn(imgMount, 0);
         if (Utilities.NO_RESULT == myRollForMount)
         {
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
            bmi.EndInit();
            Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
            ImageBehavior.SetAnimatedSource(img, bmi);
            myGrid.Children.Add(img);
            Grid.SetRow(img, STARTING_ASSIGNED_ROW + 1);
            Grid.SetColumn(img, 1);
         }
         else
         {
            Label labelForRollStables = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (3 < myRollForMount)
               labelForRollStables.Content = "yes";
            else
               labelForRollStables.Content = "no";
            myGrid.Children.Add(labelForRollStables);
            Grid.SetRow(labelForRollStables, STARTING_ASSIGNED_ROW + 1);
            Grid.SetColumn(labelForRollStables, 1);
            if (3 < myRollForMount)
            {
               if (Utilities.NO_RESULT == myRollForMountNum)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, STARTING_ASSIGNED_ROW + 1);
                  Grid.SetColumn(img, 2);
               }
               else
               {
                  Label labelForRollNum = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myRollForMountNum.ToString() };
                  myGrid.Children.Add(labelForRollNum);
                  Grid.SetRow(labelForRollNum, STARTING_ASSIGNED_ROW + 1);
                  Grid.SetColumn(labelForRollNum, 2);
                  if (Utilities.NO_RESULT == myRollForMountCost)
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                     bmi.EndInit();
                     Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, STARTING_ASSIGNED_ROW + 1);
                     Grid.SetColumn(img, 3);
                  }
                  else
                  {
                     Label labelForRollCost = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myRollForMountCost.ToString() };
                     myGrid.Children.Add(labelForRollCost);
                     Grid.SetRow(labelForRollCost, STARTING_ASSIGNED_ROW + 1);
                     Grid.SetColumn(labelForRollCost, 3);
                     //------------------------------------------------
                     StackPanel stackpanelHorse = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
                     Button bMinusHorse = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
                     if (0 < myHorsePurchasedAtFarm)
                     {
                        bMinusHorse.Click += ButtonHorse_Click;
                        bMinusHorse.IsEnabled = true;
                     }
                     stackpanelHorse.Children.Add(bMinusHorse);
                     Label labelforHorse = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myHorsePurchasedAtFarm.ToString() };
                     stackpanelHorse.Children.Add(labelforHorse);
                     Button bPlusHorse = new Button() { IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
                     if ((myRollForMountCost <= myCoinCurrent) && (myHorsePurchasedAtFarm < myRollForMountNum))
                     {
                        bPlusHorse.Click += ButtonHorse_Click;
                        bPlusHorse.IsEnabled = true;
                     }
                     stackpanelHorse.Children.Add(bPlusHorse);
                     myGrid.Children.Add(stackpanelHorse);
                     Grid.SetRow(stackpanelHorse, STARTING_ASSIGNED_ROW + 1);
                     Grid.SetColumn(stackpanelHorse, 5);
                  }
               }
            }
            else
            {
               Label labelNumToSell = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "0" };
               myGrid.Children.Add(labelNumToSell);
               Grid.SetRow(labelNumToSell, STARTING_ASSIGNED_ROW + 1);
               Grid.SetColumn(labelNumToSell, 2);
               Label labelForRollCost = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(labelForRollCost);
               Grid.SetRow(labelForRollCost, STARTING_ASSIGNED_ROW + 1);
               Grid.SetColumn(labelForRollCost, 3);
            }
         }
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private void SetStateInitialLodging()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myMapItem;
            if (true == mi.Name.Contains("Prince"))
               myGridRows[i].myResult = DO_NOT_LEAVE;
            else if (true == mi.Name.Contains("PorterSlave"))
               myGridRows[i].myResult = DO_NOT_LEAVE;
            else if ((true == mi.Name.Contains("TrueLove")) && (1 == myNumTrueLove)) // If there is only one true love, she will not leave
               myGridRows[i].myResult = DO_NOT_LEAVE;
            else if ( (true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Falcon")) )
               myGridRows[i].myResult = DO_NOT_LEAVE;
            else if (true == myGameInstance.IsMinstrelPlaying)
               myGridRows[i].myResult = DO_NOT_LEAVE;
         }
         if (true == myGameInstance.IsMinstrelPlaying)
            myGameInstance.IsPartyLodged = true;
         //-----------------------------------------------
         double roomCost = 0;
         double stableCost = 0;
         if (false == myGameInstance.IsPartyLodged)
         {
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               IMapItem mi = myGridRows[i].myMapItem;
               if ((true == mi.Name.Contains("Prince")) || (true == mi.IsSpecialist()))
                  roomCost += 1.0;
               else if ((true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Falcon")))
                  roomCost += 0.0;
               else
                  roomCost += 0.5;
            }
         }
         if (false == myGameInstance.IsMountsStabled)
            stableCost = (double)myNumMountsCount;
         double totalCost = roomCost + stableCost;
         bool isCheapLodging = myGameInstance.CheapLodgings.Contains(myGameInstance.Prince.Territory);
         if ((true == isCheapLodging) && (true == myIsHalfLodging))
         {
            roomCost = roomCost / 4.0;
            totalCost = totalCost / 4.0;
         }
         else if ((true == isCheapLodging) || (true == myIsHalfLodging))
         {
            roomCost = roomCost / 2.0;
            totalCost = totalCost / 2.0;
         }
         int diffTotal = (int)Math.Ceiling(totalCost);
         int diffRoom = (int)Math.Ceiling(roomCost);
         //-----------------------------------------------
         if (totalCost <= myCoinCurrent) // Feed all and eliminate extra starve days
         {
            myState = LodgingEnum.SE_LODGE_ALL;
            myIsHeaderCheckBoxChecked = true;
            myCoinCurrent -= diffTotal;
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               IMapItem mi = myGridRows[i].myMapItem;
               myGridRows[i].myIsLodged = true;
               myGridRows[i].myMountRows.Clear();
               foreach (IMapItem mount in mi.Mounts)
               {
                  MountRow mr = new MountRow(mount.Name, true);
                  myGridRows[i].myMountRows.Add(mr);
               }
            }
         }
         else if (roomCost <= myCoinCurrent)
         {
            myState = LodgingEnum.LE_LODGE_PEOPLE;
            for (int i = 0; i < myMaxRowCount; ++i)
               myGridRows[i].myIsLodged = true;
            myIsHeaderCheckBoxChecked = true;
            myCoinCurrent -= diffRoom;
         }
         else
         {
            myState = LodgingEnum.LE_LODGE_NOBODY;
            for (int i = 0; i < myMaxRowCount; ++i)
               myGridRows[i].myIsLodged = false;
            myIsHeaderCheckBoxChecked = false;
         }
         if( myCoinOriginal < myCoinCurrent )
            myCoinCurrent = myCoinOriginal;
         return;
      }
      private bool SetStateLodging()
      {
         if ((LodgingEnum.LE_SHOW_DESERTERS == myState) || (LodgingEnum.LE_SHOW_RESULTS == myState)) // once the rolling starts, cannot change state
            return true;
         //-----------------------------------------------
         double roomCost = 0;
         double roomCostSpent = 0;
         double stableCost = 0;
         double stableCostSpent = 0;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            if (false == myGameInstance.IsPartyLodged)
            {
               IMapItem mi = myGridRows[i].myMapItem;
               if ((true == mi.Name.Contains("Prince")) || (true == mi.IsSpecialist()))
                  roomCost += 1.0;
               else if ((true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Falcon")))
                  roomCost += 0.0;
               else
                  roomCost += 0.5;
               if (true == myGridRows[i].myIsLodged)
               {
                  if ((true == mi.Name.Contains("Prince")) || (true == mi.IsSpecialist()))
                     roomCostSpent += 1.0;
                  else if ( (true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Falcon")) )
                     roomCostSpent += 0.0;
                  else
                     roomCostSpent += 0.5;
               }
            }
            if (false == myGameInstance.IsMountsStabled)
            {
               foreach (MountRow mountRow in myGridRows[i].myMountRows)
               {
                  stableCost += 1.0;
                  if (true == mountRow.myIsStabled)
                     stableCostSpent += 1.0;
               }
            }
         }
         double totalCost = roomCost + stableCost;
         double totalSpent = roomCostSpent + stableCostSpent;
         bool isCheapLodging = myGameInstance.CheapLodgings.Contains(myGameInstance.Prince.Territory);
         if ((true == isCheapLodging) && (true == myIsHalfLodging))
         {
            roomCost = roomCost/4.0; 
            roomCostSpent = roomCostSpent/4.0;
            stableCost = stableCost / 4.0;
            stableCostSpent = stableCostSpent / 4.0;
            totalCost = totalCost/4.0;
            totalSpent = totalSpent/4.0; 
         }
         else if ((true == isCheapLodging) || (true == myIsHalfLodging))
         {
            roomCost = roomCost / 2.0;
            roomCostSpent = roomCostSpent / 2.0;
            stableCost = stableCost / 2.0;
            stableCostSpent = stableCostSpent / 2.0;
            totalCost = totalCost / 2.0;
            totalSpent = totalSpent / 2.0;
         }
         //-----------------------------------------------
         if (totalCost < totalSpent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStateLodging(): invalid state tc=" + totalCost.ToString() + " < ts=" + totalSpent.ToString());
            return false;
         }
         if (roomCost < roomCostSpent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStateLodging(): invalid state rc=" + roomCost.ToString() + " < rs=" + roomCostSpent.ToString());
            return false;
         }
         int diffTotal = (int)Math.Ceiling(totalCost - totalSpent);
         int diffRoom = (int)Math.Ceiling(roomCost - roomCostSpent);
         int diffStable = (int)Math.Ceiling(stableCost - stableCostSpent);
         if (diffTotal < myCoinCurrent)
         {
            myState = LodgingEnum.SE_LODGE_ALL;
            if (true == myIsHeaderCheckBoxChecked)
            {
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  IMapItem mi = myGridRows[i].myMapItem;
                  if (false == myGridRows[i].myIsLodged)
                     myGridRows[i].myIsLodged = true;
                  myGridRows[i].myMountRows.Clear();
                  foreach (IMapItem mount in mi.Mounts)
                  {
                     MountRow mr = new MountRow(mount.Name, true);
                     myGridRows[i].myMountRows.Add(mr);
                  }
               } // end for
               myCoinCurrent -= diffStable;    // get all mounts stabled
               myCoinCurrent -= diffRoom;      // get all party roomed
            }
         }
         else if (diffRoom <= myCoinCurrent)
         {
            myState = LodgingEnum.LE_LODGE_PEOPLE;
            if (true == myIsHeaderCheckBoxChecked)
            {
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  IMapItem mi = myGridRows[i].myMapItem;
                  if (false == myGridRows[i].myIsLodged)
                     myGridRows[i].myIsLodged = true;
               } // end for
               myCoinCurrent -= diffRoom;            // get all party roomed
            }
         }
         else
         {
            myState = LodgingEnum.LE_LODGE_NOBODY;
         }
         return true;
      }
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         b.IsEnabled = true;
         MapItem.SetButtonContent(b, mi, true, true); // This sets the image as the button's content
         return b;
      }
      private void ShowDieResults(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
            return;
         }
         if (LodgingEnum.LE_SELL_GOODS == myState)
         {
            if (1 == myRollResultColNum)
            {
               myRollForMount = dieRoll;
            }
            else if (2 == myRollResultColNum)
            {
               myRollForMountNum = dieRoll;
            }
            else if (3 == myRollResultColNum)
            {
               myRollForMountCost = dieRoll * 2; // cost is two times die roll
               if (true == myGameInstance.IsMerchantWithParty)
                  myRollForMountCost = (int)Math.Ceiling((double)myRollForMountCost * 0.5);
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): Invalid Param myRollResultColNum=" + myRollResultColNum.ToString());
            }
         }
         else
         {
            IMapItem partyMember = myGridRows[i].myMapItem;
            if (2 == myRollResultColNum)
            {
               myGridRows[i].myResult = dieRoll;
               if (true == partyMember.Name.Contains("TrueLove"))
               {
                  if (2 < myGridRows[i].myResult)
                  {
                     myGridRows[i].myMapItem.OverlayImageName = "OMIA";
                     --myNumTrueLove;
                  }
                  if (1 == myNumTrueLove)  // If down to one true love, she does not leave
                  {
                     for (int k = 0; k < myMaxRowCount; ++k)
                     {
                        IMapItem trueLove = myGridRows[k].myMapItem;
                        if ((true == trueLove.Name.Contains("TrueLove")) && (Utilities.NO_RESULT == myGridRows[k].myResult))
                           myGridRows[k].myResult = DO_NOT_LEAVE;
                     }
                  }
               }
               else
               {
                  myGridRows[i].myResult -= myGameInstance.WitAndWile;
                  if (3 < myGridRows[i].myResult) // if die is four or more, party member deserts
                     myGridRows[i].myMapItem.OverlayImageName = "OMIA";
               }
            }
            else // take care of mount column
            {
               IMapItem mount = partyMember.Mounts[0];
               int k = 0;
               for (k = 0; k < myGridRows[i].myMountRows.Count; ++k)
               {
                  if (myGridRows[i].myMountRows[k].myName == mount.Name)
                     break;
               }
               MountRow mr = new MountRow(mount.Name, dieRoll);
               myGridRows[i].myMountRows[k] = mr;
               if (3 < dieRoll)
                  mount.OverlayImageName = "OStolen";
            }
            if (true == IsLodgingRequiredForMembers())
               myState = LodgingEnum.LE_SHOW_DESERTERS;  // Assume all rolls performed unless one row shows no results
            else
               myState = LodgingEnum.LE_SHOW_RESULTS;  // Assume all rolls performed unless one row shows no results
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      private bool IsLodgingRequiredForMembers()
      {
         bool isLodgingRequred = false;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            if ((false == myGridRows[i].myIsLodged) && (Utilities.NO_RESULT == myGridRows[i].myResult))
               return true;
            foreach (MountRow mr in myGridRows[i].myMountRows)
            {
               if ((false == mr.myIsStabled) && (Utilities.NO_RESULT == mr.myResult))
               {
                  Logger.Log(LogEnum.LE_MOUNT_CHANGE, "IsLodgingRequiredForMembers(): mr=" + mr.myName + " for mi=" + myGridRows[i].myMapItem.Name);
                  isLodgingRequred = true;
               }
            }
         }
         return isLodgingRequred;
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
                  if (ui1 is Image img)
                  {
                     if (result.VisualHit == img)
                     {
                        string name = (string)img.Name;
                        if ("Lodge" == name)
                        {
                           myState = LodgingEnum.LE_END;
                        }
                        else if ("FarmerSelling" == name)
                        {
                           myState = LodgingEnum.LE_SELL_GOODS;
                        }
                        else if ("MinstrelStart" == name)
                        {
                           myGameInstance.MinstrelStart();
                           SetStateInitialLodging();
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
                  if (LodgingEnum.LE_SELL_GOODS == myState)
                  {
                     if (false == myIsRollInProgress)
                     {
                        myIsRollInProgress = true;
                        imgRow.Visibility = Visibility.Hidden;
                        myRollResultRowNum = Grid.GetRow(imgRow);
                        myRollResultColNum = Grid.GetColumn(imgRow);
                        myDieRoller.RollMovingDie(myCanvas, ShowDieResults);
                     }
                  }
                  else
                  {
                     myState = LodgingEnum.LE_SHOW_DESERTERS;
                     if (false == myIsRollInProgress)
                     {
                        myIsRollInProgress = true;
                        imgRow.Visibility = Visibility.Hidden;
                        myRollResultColNum = Grid.GetColumn(imgRow);
                        myRollResultRowNum = Grid.GetRow(imgRow);
                        int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
                        if (i < 0)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): 0 > i=" + i.ToString());
                           return;
                        }
                        IMapItem mi = myGridRows[i].myMapItem;
                        if ( (2 == myRollResultColNum) && (false == mi.Name.Contains("TrueLove")) ) // true love departs on single die > 2
                           myDieRoller.RollMovingDice(myCanvas, ShowDieResults);
                        else
                           myDieRoller.RollMovingDie(myCanvas, ShowDieResults);
                     }
                  }
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
      private void CheckBoxHeader_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         myIsHeaderCheckBoxChecked = false;
         SetStateInitialLodging();
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Checked(): UpdateGrid() return false");
      }
      private void CheckBoxHeader_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         myIsHeaderCheckBoxChecked = false;
         double roomCostSpent = 0;
         double stableCostSpent = 0;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myMapItem;
            if (false == myGameInstance.IsPartyLodged)
            {
               if (true == myGridRows[i].myIsLodged)
               {
                  if ((true == mi.Name.Contains("Prince")) || (true == mi.IsSpecialist()))
                     roomCostSpent += 1.0;
                  else if ((true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Falcon")))
                     roomCostSpent += 0.0;
                  else
                     roomCostSpent += 0.5;
               }
               myGridRows[i].myIsLodged = false;
            }
            if (false == myGameInstance.IsMountsStabled)
            {
               foreach (MountRow mountRow in myGridRows[i].myMountRows)
               {
                  if (true == mountRow.myIsStabled)
                     ++stableCostSpent;
               }
               myGridRows[i].myMountRows.Clear();
               foreach (IMapItem mount in mi.Mounts)
               {
                  MountRow mr = new MountRow(mount.Name, false);
                  myGridRows[i].myMountRows.Add(mr);
               }
            }
         }
         double totalSpent = roomCostSpent + stableCostSpent;
         bool isCheapLodging = myGameInstance.CheapLodgings.Contains(myGameInstance.Prince.Territory);
         if ((true == isCheapLodging) && (true == myIsHalfLodging))
            totalSpent = totalSpent / 4.0;
         else if ((true == isCheapLodging) || (true == myIsHalfLodging))
            totalSpent = totalSpent / 2.0;
         int totalDiff = (int)Math.Ceiling(totalSpent);
         int maxDiff = myCoinOriginal - myCoinCurrent;
         myCoinCurrent += Math.Min(maxDiff, totalDiff);
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxHeader_Unchecked(): UpdateGrid() return false");
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
         //--------------------------------------------
         if (true == myGameInstance.IsPartyLodged)
         {
            myIsHeaderCheckBoxChecked = true;
            for (int k1 = 0; k1 < myMaxRowCount; ++k1)
            {
               for (int k2 = 0; k2 < myGridRows[k1].myMountRows.Count; ++k2)
               {
                  if (false == myGridRows[k1].myMountRows[k2].myIsStabled)
                     myIsHeaderCheckBoxChecked = false; ;
               }
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
         int i = rowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         if (0 == mi.Mounts.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxMount_Unchecked(): mounts count=0 for mi=" + mi.Name);
            return;
         }
         myIsHeaderCheckBoxChecked = false;
         IMapItem mount = mi.Mounts[0];
         if( myCoinCurrent < myCoinOriginal )
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
      private void ButtonDrug_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string content = (String)b.Content;
         if ("-" == content)
         {
            myCoinOriginal += myChagaCost;
            myCoinCurrent += myChagaCost;
            --myGameInstance.ChagaDrugCount;
         }
         else if ("+" == content)
         {
            myCoinOriginal -= myChagaCost;
            myCoinCurrent -= myChagaCost;
            ++myGameInstance.ChagaDrugCount;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonDrug_Click(): Reached default for " + content);
         }
         if (false == SetStateLodging())
            Logger.Log(LogEnum.LE_ERROR, "ButtonDrug_Click(): SetStateLodging() return false");
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonDrug_Click(): UpdateGrid() return false");
      }
      private void ButtonSkin_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string content = (String)b.Content;
         if ("-" == content)
         {
            myCoinCurrent += 50;
            --myTrollSkinsInPartyCurrent;
         }
         else if ("+" == content)
         {
            myCoinCurrent -= 50;
            ++myTrollSkinsInPartyCurrent;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonSkin_Click(): Reached default for " + content);
         }
         if (false == SetStateLodging())
            Logger.Log(LogEnum.LE_ERROR, "ButtonSkin_Click(): SetStateLodging() return false");
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonSkin_Click(): UpdateGrid() return false");
      }
      private void ButtonBeak_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string content = (String)b.Content;
         if ("-" == content)
         {
            myCoinCurrent += 35;
            --myRocBeaksInPartyCurrent;
         }
         else if ("+" == content)
         {
            myCoinCurrent -= 35;
            ++myRocBeaksInPartyCurrent;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonBeak_Click(): Reached default for " + content);
         }
         if (false == SetStateLodging())
            Logger.Log(LogEnum.LE_ERROR, "ButtonBeak_Click(): SetStateLodging() return false");
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonBeak_Click(): UpdateGrid() return false");
      }
      private void ButtonClothes_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string content = (String)b.Content;
         if ("-" == content)
         {
            myCoinOriginal += mySuitCost;
            myCoinCurrent += mySuitCost;
            myIsPurchasedCloth = false;
         }
         else if ("+" == content)
         {
            myCoinOriginal -= mySuitCost;
            myCoinCurrent -= mySuitCost;
            myIsPurchasedCloth = true;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonClothes_Click(): Reached default for " + content);
         }
         if (false == SetStateLodging())
            Logger.Log(LogEnum.LE_ERROR, "ButtonClothes_Click(): SetStateLodging() return false");
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonClothes_Click(): UpdateGrid() return false");
      }
      private void ButtonFood_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         StackPanel sp = (StackPanel)b.Parent;
         int rowNum = Grid.GetRow(sp);
         int i = rowNum - STARTING_ASSIGNED_ROW;
         string content = (String)b.Content;
         int normalizedFoodCost = (int)Math.Ceiling(myFoodCost);
         if (0 == myFoodCost)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonFood_Click(): myFoodCost=0");
            return;
         }
         int normalizedFood = (int)( 1/ myFoodCost);
         if ("-" == content)
         {
            myCoinCurrent += normalizedFoodCost;
            myFoodPurchasedAtFarm -= normalizedFood;
         }
         else if ("+" == content)
         {
            myCoinCurrent -= normalizedFoodCost;
            myFoodPurchasedAtFarm += normalizedFood;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonFood_Click(): Reached default for " + content);
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonFood_Click(): UpdateGrid() return false");
      }
      private void ButtonHorse_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         StackPanel sp = (StackPanel)b.Parent;
         int rowNum = Grid.GetRow(sp);
         int i = rowNum - STARTING_ASSIGNED_ROW;
         string content = (String)b.Content;
         if ("-" == content)
         {
            myCoinCurrent += myRollForMountCost;
            --myHorsePurchasedAtFarm;
         }
         else if ("+" == content)
         {
            myCoinCurrent -= myRollForMountCost;
            ++myHorsePurchasedAtFarm;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonHorse_Click(): Reached default for " + content);
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonHorse_Click(): UpdateGrid() return false");
      }
   }
}
