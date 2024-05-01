
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;

namespace BarbarianPrince
{
   public class GameInstance : IGameInstance
   {
      public bool CtorError { get; } = false;
      //----------------------------------------------
      private readonly IOptions myOptions = null;
      public IOptions Options { get => myOptions; }
      public bool IsGridActive { set; get; } = false;
      //----------------------------------------------
      public string EventActive { get; set; } = "e000";
      public string EventDisplayed { set; get; } = "e000";
      public string EventStart { set; get; } = "e000";
      //----------------------------------------------
      public int GameTurn { get; set; } = 0;
      public bool IsNewDayChoiceMade { set; get; } = false;
      public GamePhase GamePhase { get; set; } = GamePhase.Error;
      public GamePhase SunriseChoice { set; get; } = GamePhase.Error;
      public GameAction DieRollAction { get; set; } = GameAction.DieRollActionNone;
      private Dictionary<string, int[]> myDieResults = new Dictionary<string, int[]>();
      public Dictionary<string, int[]> DieResults { get => myDieResults; }
      //----------------------------------------------
      private List<ITerritory> myTerritories = null;
      public List<ITerritory> Territories { get => myTerritories; }
      public ITerritory ActiveHex { set; get; } = null;
      public ITerritory NewHex { set; get; } = null;
      private List<ITerritory> myEnteredTerritories = new List<ITerritory>();
      public List<ITerritory> EnteredTerritories { get => myEnteredTerritories; }
      private List<ITerritory> myDwarfAdviceLocations = new List<ITerritory>();
      public List<ITerritory> DwarfAdviceLocations { get => myDwarfAdviceLocations; }
      private List<ITerritory> myWizardAdviceLocations = new List<ITerritory>();
      public List<ITerritory> WizardAdviceLocations { get => myWizardAdviceLocations; }
      private List<ITerritory> myAlcoveOfSendings = new List<ITerritory>();
      public List<ITerritory> AlcoveOfSendings { get => myAlcoveOfSendings; }
      private List<ITerritory> myArches = new List<ITerritory>();
      public List<ITerritory> Arches { get => myArches; }
      private List<ITerritory> myVisitedLoctions = new List<ITerritory>();
      public List<ITerritory> VisitedLocations { get => myVisitedLoctions; }
      private List<ITerritory> myEscapedLoctions = new List<ITerritory>();
      public List<ITerritory> EscapedLocations { get => myEscapedLoctions; }
      private List<ITerritory> myGoblinKeeps = new List<ITerritory>();
      public List<ITerritory> GoblinKeeps { get => myGoblinKeeps; }
      private List<ITerritory> myDwarvenMines = new List<ITerritory>();
      public List<ITerritory> DwarvenMines { get => myDwarvenMines; }
      private List<ITerritory> myOrcTowers = new List<ITerritory>();
      public List<ITerritory> OrcTowers { get => myOrcTowers; }
      private List<ITerritory> myWizardTowers = new List<ITerritory>();
      public List<ITerritory> WizardTowers { get => myWizardTowers; }
      private List<ITerritory> myHalflingTowns = new List<ITerritory>();
      public List<ITerritory> HalflingTowns { get => myHalflingTowns; } // e070
      private List<ITerritory> myRuinsUnstable = new List<ITerritory>();
      public List<ITerritory> RuinsUnstable { get => myRuinsUnstable; }
      private List<ITerritory> myHiddenRuins = new List<ITerritory>();
      public List<ITerritory> HiddenRuins { get => myHiddenRuins; }
      private List<ITerritory> myHiddenTowns = new List<ITerritory>();
      public List<ITerritory> HiddenTowns { get => myHiddenTowns; }
      private List<ITerritory> myHiddenTemples = new List<ITerritory>();
      public List<ITerritory> HiddenTemples { get => myHiddenTemples; }
      private List<ITerritory> myKilledLoctions = new List<ITerritory>();
      public List<ITerritory> KilledLocations { get => myKilledLoctions; }
      private List<ITerritory> myEagleLairs = new List<ITerritory>();
      public List<ITerritory> EagleLairs { get => myEagleLairs; } // e115
      private List<ITerritory> mySecretClues = new List<ITerritory>();
      public List<ITerritory> SecretClues { get => mySecretClues; } // e147
      private List<ITerritory> myLetterOfRecommendations = new List<ITerritory>();
      public List<ITerritory> LetterOfRecommendations { get => myLetterOfRecommendations; } // e157
      private List<ITerritory> myPurifications = new List<ITerritory>();
      public List<ITerritory> Purifications { get => myPurifications; } // e159
      private List<ITerritory> myElfTowns = new List<ITerritory>();
      public List<ITerritory> ElfTowns { get => myElfTowns; } // e165
      private List<ITerritory> myElfCastles = new List<ITerritory>();
      public List<ITerritory> ElfCastles { get => myElfCastles; } // e166
      private List<ITerritory> myFeelAtHomes = new List<ITerritory>();
      public List<ITerritory> FeelAtHomes { get => myFeelAtHomes; } // e209
      private List<ITerritory> mySecretRites = new List<ITerritory>();
      public List<ITerritory> SecretRites { get => mySecretRites; } // e209
      private List<ITerritory> myCheapLodgings = new List<ITerritory>();
      public List<ITerritory> CheapLodgings { get => myCheapLodgings; } // e209
      private List<ITerritory> myThievesGuilds = new List<ITerritory>();
      public List<ITerritory> ForbiddenHexes { get => myThievesGuilds; } // e209
      private List<ITerritory> myAbandonedTemples = new List<ITerritory>();
      public List<ITerritory> AbandonedTemples { get => myAbandonedTemples; }
      private List<ITerritory> myForbiddenHires = new List<ITerritory>();
      public List<ITerritory> ForbiddenHires { get => myForbiddenHires; }
      //----------------------------------------------
      private IMapItem myPrince = null;
      public IMapItem Prince { set => myPrince = value; get => myPrince; }
      public int WitAndWile { get; set; } = 0;
      public int Days { get; set; } = 0;
      //----------------------------------------------
      public IMapItem ActiveMember { set; get; } = null;
      public List<int> CapturedWealthCodes { set; get; } = new List<int>();
      public PegasusTreasureEnum PegasusTreasure { set; get; } = PegasusTreasureEnum.Mount;
      public int FickleCoin { set; get; } = 0;
      public int LooterCoin { get; set; } = 0;
      //----------------------------------------------
      private readonly IMapItems myMapItems = null;
      public IMapItems MapItems { get => myMapItems; }
      public IMapItems PartyMembers { get; set; } = new MapItems();
      public IMapItems LostPartyMembers { get; set; } = new MapItems();
      public IMapItems LostTrueLoves { set; get; } = new MapItems();
      public IMapItems EncounteredMembers { get; set; } = new MapItems();
      public IMapItems EncounteredMinstrels { get; set; } = new MapItems();
      public IMapItems AtRiskMounts { get; set; } = new MapItems();
      //----------------------------------------------
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IMapItemMove PreviousMapItemMove { get; set; } = new MapItemMove();
      //----------------------------------------------
      public List<string> Events { set; get; } = new List<string>();
      public String EndGameReason { set; get; } = "";
      //----------------------------------------------
      public bool IsPartyRested { set; get; } = false;
      public bool IsAirborne { set; get; } = false;
      public bool IsAirborneEnd { set; get; } = false;
      public bool IsShortHop { set; get; } = false;
      public bool IsMountsFed { set; get; } = false;
      public bool IsMountsStabled { set; get; } = false;
      public int Bribe { set; get; } = 0;
      //----------------------------------------------
      public bool IsGuardEncounteredThisTurn { set; get; } = false;
      public string DwarvenChoice { set; get; } = "";
      public bool IsDwarvenBandSizeSet { set; get; } = false;
      public bool IsDwarfWarriorJoiningParty { set; get; } = false;
      public string ElvenChoice { set; get; } = "";
      public bool IsElfWitAndWileActive { set; get; } = false;
      public bool IsElvenBandSizeSet { set; get; } = false;
      public bool IsPartyDisgusted { set; get; } = false;
      public int PurchasedFood { set; get; } = 0;
      public bool IsFarmerLodging { set; get; } = false;
      public bool IsReaverClanFight { set; get; } = false;
      public bool IsReaverClanTrade { set; get; } = false;
      public bool IsMagicianProvideGift { set; get; } = false;
      public bool IsHuntedToday { set; get; } = false;
      public bool IsMarkOfCain { set; get; } = false;
      public int PurchasedMount { set; get; } = 0;
      public int MonkPleadModifier { set; get; } = 0;
      public bool IsWizardJoiningParty { set; get; } = false;
      public bool IsEnslaved { set; get; } = false;
      public bool IsSpellBound { set; get; } = false;
      public int WanderingDayCount { set; get; } = 1;
      public bool IsBlessed { set; get; } = false;
      public int GuardianCount { set; get; } = 0;
      public bool IsMerchantWithParty { set; get; } = false;
      public bool IsMinstrelPlaying { set; get; } = false;
      public bool IsMinstrelJoining { set; get; } = false;
      public bool IsJailed { set; get; } = false;
      public bool IsDungeon { set; get; } = false;
      public int NightsInDungeon { set; get; } = 0;
      public bool IsTempleGuardModifer { set; get; } = false;
      public bool IsTempleGuardEncounteredThisHex { set; get; } = false;
      public bool IsWoundedWarriorRest { set; get; } = false;
      public int NumMembersBeingFollowed { set; get; } = 0;
      public bool IsTalkActive { set; get; } = true;
      public bool IsWolvesAttack { set; get; } = false;
      public bool IsBearAttack { set; get; } = false;
      public bool IsHighPass { set; get; } = false;
      public string EventAfterRedistribute { set; get; } = "";
      public bool IsImpassable { set; get; } = false;
      public bool IsFlood { set; get; } = false;
      public bool IsFloodContinue { set; get; } = false;
      public bool IsPoisonPlant { set; get; } = false;
      public bool IsMountsAtRisk { set; get; } = false;
      public bool IsMountsSick { set; get; } = false;
      public bool IsFalconFed { set; get; } = false;
      public bool IsEagleHunt { set; get; } = false;
      public bool IsExhausted { set; get; } = false;
      public RaftEnum RaftState { set; get; } = RaftEnum.RE_NO_RAFT; // e122 - Party can be rafting for the day
      public bool IsRaftDestroyed { set; get; } = false;
      public bool IsWoundedBlackKnightRest { set; get; } = false;
      public bool IsTrainHorse { set; get; } = false;
      public bool IsBadGoing { set; get; } = false;
      public bool IsHeavyRain { set; get; } = false;
      public bool IsHeavyRainNextDay { set; get; } = false;
      public bool IsHeavyRainContinue { set; get; } = false;
      public bool IsHeavyRainDismount { set; get; } = false;
      public bool IsEvadeActive { set; get; } = true;
      public int PurchasedPotionCure { set; get; } = 0;
      public int PurchasedPotionHeal { set; get; } = 0;
      public int HydraTeethCount { set; get; } = 0;
      public bool IsCavalryEscort { set; get; } = false;  // e151
      public bool IsNobleAlly { set; get; } = false;  // e152
      public int SeneschalRollModifier { set; get; } = 0;
      private IForbiddenAudiences myForbiddenAudiences = new ForbiddenAudiences();
      public IForbiddenAudiences ForbiddenAudiences { get => myForbiddenAudiences; } // e153
      public int DaughterRollModifier { set; get; } = 0;
      public int DayOfLastOffering { set; get; } = Utilities.FOREVER;
      public int PriestModifier { set; get; } = 0;
      public bool IsPartyFed { set; get; } = false;
      public bool IsPartyLodged { set; get; } = false;
      public bool IsPartyContinuouslyLodged { set; get; } = false;
      public bool IsTrueLoveHeartBroken { set; get; } = false;
      public bool IsMustLeaveHex { set; get; } = false;
      public int NumMonsterKill { set; get; } = 0;
      public int PurchasedSlavePorter { set; get; } = 0;
      public int PurchasedSlaveWarrior { set; get; } = 0;
      public int PurchasedSlaveGirl { set; get; } = 0;
      public int SlaveGirlIndex { set; get; } = 0;
      public bool IsSlaveGirlActive { set; get; } = false;
      public bool IsGiftCharmActive { set; get; } = false;
      public bool IsPegasusSkip { set; get; } = false;
      public bool IsCharismaTalismanActive { set; get; } = false;
      public bool IsSeekNewModifier { set; get; } = false;
      public int PurchasedHenchman { set; get; } = 0;// e210f - Amount  of henchmen hired  
      public int PurchasedPorter { set; get; } = 0; // e210i - Amount  of porter purchases  
      public int PurchasedGuide { set; get; } = 0; // e210i - Amount  of local guides purchases  
      public bool IsOfferingModifier { set; get; } = false; // e212 - add +1 due to spending 10 gold
      public bool IsOmenModifier { set; get; } = false;  // e212f
      public bool IsInfluenceModifier { set; get; } = false; // e212l
      private ICaches myCaches = new Caches();
      public ICaches Caches { get => myCaches; }
      public bool IsAssassination { set; get; } = false;
      public bool IsDayEnd { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsSecretGatewayToDarknessKnown { set; get; } = false;  // e046
      public bool IsSecretTempleKnown { set; get; } = false;   // e143 
      public bool IsSecretBaronHuldra { set; get; } = false;   // e144 
      public bool IsSecretLadyAeravir { set; get; } = false;   // e145 
      public bool IsSecretCountDrogat { set; get; } = false;   // e146 
      public int ChagaDrugCount { set; get; } = 0;             // e143 Chaga Drug purchased in town - 2gp per serving
      public bool IsChagaDrugProvided { set; get; } = false;   // e211b
      //---------------------------------------------------------------
      public IStacks Stacks { get; set; } = new Stacks();
      public List<IUnitTest> UnitTests { set; get; } = null;
      //---------------------------------------------------------------
      [NonSerialized] static public Logger Logger = new Logger();
      public GameInstance() // Constructor - set log levels
      {
         Logger.SetOn(LogEnum.LE_ERROR);
         //Logger.SetOn(LogEnum.LE_GAME_INIT);
         Logger.SetOn(LogEnum.LE_NEXT_ACTION);
         //Logger.SetOn(LogEnum.LE_GAME_PARTYMEMBER_COUNT);
         Logger.SetOn(LogEnum.LE_REMOVE_KILLED);
         //Logger.SetOn(LogEnum.LE_MOVE_STACKING);
         Logger.SetOn(LogEnum.LE_MOVE_COUNT);
         //Logger.SetOn(LogEnum.LE_RESET_ROLL_STATE);
         //Logger.SetOn(LogEnum.LE_GET_COIN);
         //Logger.SetOn(LogEnum.LE_MOUNT_CHANGE);
         Logger.SetOn(LogEnum.LE_COMBAT_STATE);
         //Logger.SetOn(LogEnum.LE_COMBAT_STATE_ESCAPE);
         //Logger.SetOn(LogEnum.LE_COMBAT_STATE_ROUTE);
         Logger.SetOn(LogEnum.LE_COMBAT_RESULT);
         //Logger.SetOn(LogEnum.LE_COMBAT_TROLL_HEAL);
         //Logger.SetOn(LogEnum.LE_MAPITEM_WOUND);
         //Logger.SetOn(LogEnum.LE_MAPITEM_POISION);
         Logger.SetOn(LogEnum.LE_END_ENCOUNTER);
         //Logger.SetOn(LogEnum.LE_STARVATION_STATE_CHANGE);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_WINDOW);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_MENU);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_STATUS_BAR);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_ACTION_PANEL);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_ACTION_PANEL_CLEAR);
         //Logger.SetOn(LogEnum.LE_RETURN_TO_START);
         //Logger.SetOn(LogEnum.LE_VIEW_DICE_MOVING);
         //Logger.SetOn(LogEnum.LE_VIEW_RESET_BATTLE_GRID);
         //Logger.SetOn(LogEnum.LE_VIEW_DEC_COUNT_GRID);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_DAILY_ACTIONS);
         //Logger.SetOn(LogEnum.LE_VIEW_MIM);
         //Logger.SetOn(LogEnum.LE_VIEW_MIM_ADD);
         Logger.SetOn(LogEnum.LE_VIEW_MIM_CLEAR);
         Logger.SetOn(LogEnum.LE_VIEW_SHOW_LOADS);
         //Logger.SetOn(LogEnum.LE_VIEW_SHOW_HUNT);
         try
         {
            // Create the territories and the regions marking the territories.
            // Keep a list of Territories used in the game.  All the information 
            // of Territories is static and does not change.
            myTerritories = ReadTerritoriesXml();
            if (null == myTerritories)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() returned null");
               CtorError = true;
               return;
            }
            //---------------------------------------------------------
            myMapItems = ReadMapItemsXml(myTerritories);
            if (null == myMapItems)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadMapItemsXml() returned null");
               CtorError = true;
               return;
            }
            //---------------------------------------------------------
            myOptions = ReadOptionsXml();
            if (null == myOptions)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadOptionsXml() returned null");
               CtorError = true;
               return;
            }
         }
         catch (Exception e)
         {
            MessageBox.Show("Exception in GameEngine() e=" + e.ToString());
         }
         IMapItem prince = MapItems.Find("Prince");
         if (null == prince)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): prince=null");
            CtorError = true;
            return;
         }
         myPrince = new MapItem(prince);
         PartyMembers.Add(myPrince);
         if( false == AddStartingPartyMembers())
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): AddStartingPartyMembers() returned false");
            CtorError = true;
            return;
         }
         #if UT1
                  AddUnitTests();
         #endif
         Logger.Log(LogEnum.LE_GAME_PARTYMEMBER_COUNT, "GameInstance() c=" + PartyMembers.Count.ToString());
      }
      private bool AddStartingPartyMembers()
      {
         IOption option = null;
         String memberToAdd = "";
         //---------------------------------------------------------
         memberToAdd = "Dwarf";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", Prince.Territory, 5, 5, 0);
            member.IsFlying = true;
            member.IsRiding = true;
            member.Food = 25;
            member.Coin = 301;
            member.IsFickle = true;
            member.AddNewMount(); // riding
            AddCompanion(member);
         }
         //---------------------------------------------------------
         memberToAdd = "Eagle";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c62Eagle", "c62Eagle", Prince.Territory, 3, 4, 1);
            member.IsFlying = true;
            member.IsRiding = true;
            AddCompanion(member);
         }
         //---------------------------------------------------------
         memberToAdd = "Elf";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c56Elf", "c56Elf", Prince.Territory, 5, 5, 0);
            member.Food = 5;
            member.Coin = 201;
            member.AddNewMount(MountEnum.Horse);
            AddCompanion(member);
         }
         //---------------------------------------------------------
         memberToAdd = "Falcon";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c82Falcon", "c82Falcon", Prince.Territory, 0, 0, 0);
            member.IsFlying = true;
            member.IsRiding = true;
            member.IsGuide = true;
            member.GuideTerritories = Territories;
            AddCompanion(member);
            IsFalconFed = true;
         }
         //---------------------------------------------------------
         memberToAdd = "Griffon";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c63Griffon", "c63Griffon", Prince.Territory, 3, 4, 1);
            member.IsFlying = true;
            member.IsRiding = true;
            AddCompanion(member);
            //---------------------
            memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem rider = new MapItem(memberName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", Prince.Territory, 5, 5, 0);
            member.Rider = member;
            rider.Mounts.Insert(0, member);
            rider.IsRiding = true;
            rider.IsFlying = true;
            AddCompanion(rider);
         }
         //---------------------------------------------------------
         memberToAdd = "Magician";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if( true == option.IsEnabled )
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c16Magician", "c16Magician", Prince.Territory, 5, 5, 0);
            AddCompanion(member);
         }
         //---------------------------------------------------------
         memberToAdd = "Mercenary";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", Prince.Territory, 5, 5, 0);
            member.Food = 5;
            member.Coin = 98;
            member.AddNewMount();  // riding
            member.AddNewMount(MountEnum.Pegasus); // flying
            member.SetWounds(4, 0); // make unconscious
            member.IsGuide = true;
            foreach (string adj in Prince.TerritoryStarting.Adjacents)
            {
               ITerritory t = Territories.Find(adj);
               member.GuideTerritories.Add(t);
            }
            AddCompanion(member);
         }
         //---------------------------------------------------------
         memberToAdd = "Merchant";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c16Magician", "c16Magician", Prince.Territory, 5, 5, 0);
            AddCompanion(member);
            IsMerchantWithParty = true;
         }
         //---------------------------------------------------------
         memberToAdd = "Minstrel";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", Prince.Territory, 0, 0, 0);
            AddCompanion(member);
            IsMinstrelPlaying = true; // e049 - minstrel
         }
         //---------------------------------------------------------
         memberToAdd = "Monk";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c19Monk", "c19Monk", Prince.Territory, 5, 5, 0);
            AddCompanion(member);
         }
         //---------------------------------------------------------
         memberToAdd = "PorterSlave";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", Prince.Territory, 0, 0, 0);
            AddCompanion(member);
         }
         //---------------------------------------------------------
         memberToAdd = "TrueLove";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", Prince.Territory, 0, 0, 0);
            AddCompanion(member);
         }
         //---------------------------------------------------------
         memberToAdd = "Wizard";
         option = myOptions.Find(memberToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + memberToAdd + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            string memberName = memberToAdd + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem member = new MapItem(memberName, 1.0, false, false, false, "c12Wizard", "c12Wizard", Prince.Territory, 4, 4, 0);
            AddCompanion(member);
         }
         return true;
      }
      void AddUnitTests()
      {
         Days = 40;
         myPrince.Food = 5;
         myPrince.Coin = 30;
         //myPrince.SetWounds(7, 0);
         //myPrince.PlagueDustWound = 1; 
         //myPrince.AddNewMount();
         myPrince.AddNewMount(MountEnum.Pegasus);
         //this.AddUnitTestTiredMount(myPrince);
         //myPrince.AddNewMount();
         //myPrince.AddNewMount();
         //---------------------
         AddSpecialItem(SpecialEnum.GiftOfCharm);
         AddSpecialItem(SpecialEnum.ResistanceTalisman);
         AddSpecialItem(SpecialEnum.CharismaTalisman);
         //AddSpecialItem(SpecialEnum.DragonEye);
         //AddSpecialItem(SpecialEnum.RocBeak);
         //AddSpecialItem(SpecialEnum.GriffonClaws);
         //AddSpecialItem(SpecialEnum.HealingPoition);
         //AddSpecialItem(SpecialEnum.CurePoisonVial);
         //AddSpecialItem(SpecialEnum.EnduranceSash);
         //AddSpecialItem(SpecialEnum.PoisonDrug);
         //AddSpecialItem(SpecialEnum.MagicSword);
         //AddSpecialItem(SpecialEnum.AntiPoisonAmulet);
         //AddSpecialItem(SpecialEnum.PegasusMountTalisman);
         //AddSpecialItem(SpecialEnum.NerveGasBomb);
         //AddSpecialItem(SpecialEnum.ResistanceRing);
         //AddSpecialItem(SpecialEnum.ResurrectionNecklace);
         //AddSpecialItem(SpecialEnum.ShieldOfLight);
         //AddSpecialItem(SpecialEnum.RoyalHelmOfNorthlands);
         //myPrince.AddSpecialItemToShare(SpecialEnum.HydraTeeth);
         //this.HydraTeethCount = 5;
         //---------------------
         //ITerritory visited = Territories.Find("0109");
         //this.myVisitedLoctions.Add(visited);
         //---------------------
         //ITerritory escapeLocation = Territories.Find("0605");
         //EscapedLocations.Add(escapeLocation);
         //---------------------
         //ITerritory cacheHex = Territories.Find("0504");
         //Caches.Add(cacheHex, 66);
         //cacheHex = Territories.Find("0505");
         //Caches.Add(cacheHex, 333);
         //Caches.Add(cacheHex, 100);
         //Caches.Add(cacheHex, 500);
         //Caches.Add(cacheHex, 33);
         //---------------------
         //ITerritory secretClueHex = Territories.Find("0504");
         //SecretClues.Add(secretClueHex);
         //---------------------
         //ITerritory secretClueHex2 = Territories.Find("0706");
         //SecretClues.Add(secretClueHex2);
         //---------------------
         //ITerritory hiddenTemple = Territories.Find("0605");
         //HiddenTemples.Add(hiddenTemple);
         //---------------------
         //ITerritory hiddenRuin = Territories.Find("0606");
         //HiddenRuins.Add(hiddenRuin);
         //---------------------
         //ITerritory elfTown = Territories.Find("0607");
         //ElfTowns.Add(elfTown);
         //---------------------
         //ITerritory eagleLair = Territories.Find("0407");
         //EagleLairs.Add(eagleLair);
         //---------------------
         //ITerritory dwarvenMine = Territories.Find("0408");  
         //DwarvenMines.Add(dwarvenMine);
         //---------------------
         //ITerritory dwarfAdviceHex = Territories.Find("0319");
         //DwarfAdviceLocations.Add(dwarfAdviceHex);
         //---------------------
         //ITerritory halflingTown = Territories.Find("0303");
         //HalflingTowns.Add(halflingTown);
         //---------------------
         //ITerritory elfCastle  = Territories.Find("0608");
         //ElfCastles.Add(elfCastle);
         //---------------------
         //ITerritory wizarTower = Territories.Find("0404");  //mountain
         //WizardTowers.Add(wizarTower);
         //---------------------
         //ITerritory wizardAdviceHex = Territories.Find("1005");
         //WizardAdviceLocations.Add(wizardAdviceHex);
         //---------------------
         //ITerritory t11 = Territories.Find("0306"); // e114 - verify that eagle hunt can happen in structure
         //HiddenTemples.Add(t11);
         //t11 = Territories.Find("0307"); // e114 - verify that eagle hunt can happen in structure
         //HiddenTemples.Add(t11);
         //t11 = Territories.Find("0407"); // e114 - verify that eagle hunt can happen in structure
         //HiddenTemples.Add(t11);
         //t11 = Territories.Find("0405"); // e114 - verify that eagle hunt can happen in structure
         //HiddenTemples.Add(t11);
         //t11 = Territories.Find("0406"); // e114 - verify that eagle hunt can happen in structure
         //HiddenTemples.Add(t11);
         //t11 = Territories.Find("0506"); // e114 - verify that eagle hunt can happen in structure
         //HiddenTemples.Add(t11);
         //t11 = Territories.Find("0507"); // e114 - verify that eagle hunt can happen in structure
         //HiddenTemples.Add(t11);
         //---------------------
         //ITerritory forbiddenHex = Territories.Find("0705");
         //ForbiddenHexes.Add(forbiddenHex);
         //---------------------
         //ITerritory forbiddenAudience = Territories.Find("0101");
         //ITerritory lt1 = Territories.Find("0109");
         //ITerritory lt2 = Territories.Find("0711");
         //ITerritory lt3 = Territories.Find("1212");
         //LetterOfRecommendations.Add(lt1);
         //LetterOfRecommendations.Add(lt1);
         //ForbiddenAudiences.AddLetterConstraint(forbiddenAudience, lt1);
         //LetterOfRecommendations.Add(lt2);
         //ForbiddenAudiences.AddLetterConstraint(forbiddenAudience, lt2);
         //LetterOfRecommendations.Add(lt3);
         //ForbiddenAudiences.AddLetterConstraint(forbiddenAudience, lt3);
         //---------------------
         //DayOfLastOffering = Days + 4;
         //IsSecretTempleKnown = true;
         //IsMarkOfCain = true; // e018
         //NumMonsterKill = 5; // e161e - kill 5 monsters
         //ChagaDrugCount = 2;
         //RaftState = RaftEnum.RE_RAFT_SHOWN;
      }
      //----------------------------------------------
      public bool IsInTown(ITerritory t)
      {
         return ((true == t.IsTown) || (true == HiddenTowns.Contains(t)) || (true == ElfTowns.Contains(t)) || (true == HalflingTowns.Contains(t)));
      }
      public bool IsInTemple(ITerritory t)
      {
         return (((true == t.IsTemple) || (true == HiddenTemples.Contains(t))) && (false == AbandonedTemples.Contains(t)));
      }
      public bool IsInCastle(ITerritory t)
      {
         return ((true == t.IsCastle) || (true == ElfCastles.Contains(t)) || (true == DwarvenMines.Contains(t)) || (true == WizardTowers.Contains(t)));
      }
      public bool IsInRuins(ITerritory t)
      {
         return ((true == t.IsRuin) || (true == HiddenRuins.Contains(t)));
      }
      public bool IsInTownOrCastle(ITerritory t)
      {
         return ((true == IsInTown(t)) || (true == IsInCastle(t)));
      }
      public bool IsInStructure(ITerritory t)
      {
         return ((true == IsInTown(t)) || (true == IsInTemple(t)) || (true == IsInCastle(t)));
      }
      //---------------------------------------------------------------
      public void AddCompanion(IMapItem companion)
      {
         companion.Territory = Prince.Territory;
         companion.IsSecretGatewayToDarknessKnown = true;  // e046 
         foreach (string s in Utilities.theNorthOfTragothHexes) // if added south of river, secret of gateway known
         {
            if (s == Prince.Territory.Name)
               IsSecretGatewayToDarknessKnown = false;
         }
         //--------------------------------
         PartyMembers.Add(companion);
         //--------------------------------
         if (true == companion.Name.Contains("TrueLove"))
         {
            int numTrueLoves = 0;
            foreach (IMapItem member in PartyMembers)
            {
               if (true == member.Name.Contains("TrueLove"))
                  ++numTrueLoves;
            }
            if (2 == numTrueLoves)  // remove effects of true love due to the eternal triangle
               --this.WitAndWile;
            else if (1 == numTrueLoves)  // add effects of true love
               ++this.WitAndWile;
         }
         //--------------------------------
         if (true == companion.Name.Contains("Wizard"))
            IsWizardJoiningParty = true;
         //--------------------------------
         if (true == companion.Name.Contains("DwarfWarrior"))
            IsDwarfWarriorJoiningParty = true;
         //--------------------------------
         if (true == companion.Name.Contains("ElfWarrior"))
            WitAndWile += 1;
      }
      public int GetFoods()
      {
         int foods = 0;
         foreach (IMapItem mi in PartyMembers)
            foods += mi.Food;
         return foods;
      }
      public bool AddFoods(int foodStore)
      {
         if (foodStore < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddFoods(): invalid parameter foodStore=" + foodStore.ToString());
            return false;
         }
         if (0 == foodStore)
            return true;
         int count = 1000;
         IMapItems sortedMapItems = PartyMembers.SortOnFreeLoad();
         while (0 < --count)
         {
            foreach (IMapItem mi in sortedMapItems)
            {
               if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
                  continue;
               int freeLoad = mi.GetFreeLoad();
               if (freeLoad < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "AddFoods(): GetFreeLoad() returned fl=" + freeLoad.ToString());
                  return false;
               }
               if (0 < freeLoad)
               {
                  ++mi.Food;
                  --foodStore;
                  Logger.Log(LogEnum.LE_REMOVE_KILLED, "AddFoods(): " + mi.Name + " ++++>>> f=" + mi.Food.ToString() + " foodStore=" + foodStore.ToString() + " fl=" + freeLoad.ToString());
                  if (0 == foodStore)
                     return true;
               }
            }
         }
         if (count < 0)
            Logger.Log(LogEnum.LE_ERROR, "AddFoods(): invalid state count<0 fs=" + foodStore.ToString());
         if ((false == myPrince.IsUnconscious) && (false == myPrince.IsKilled))
            myPrince.Food += foodStore; // remaining is given to Prince
         return true;
      }
      public void ReduceFoods(int foodStore)
      {
         if (foodStore < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReduceFoods(): invalid parameter foodStore=" + foodStore.ToString());
            return;
         }
         if (0 == foodStore)
            return;
         IMapItems sortedMapItems = PartyMembers.SortOnFreeLoad();
         sortedMapItems.Reverse();
         //---------------------------------
         int count = 100;
         while (0 < --count)
         {
            foreach (IMapItem mi in sortedMapItems)
            {
               if (mi.Food < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReduceFoods(): invalid parameter 0 > mi.Food=" + mi.Food.ToString() + " for n=" + mi.Name);
                  mi.Food = 0;
               }
               if (0 < mi.Food)
               {
                  --mi.Food;
                  --foodStore;
                  if (0 == foodStore)
                     return;
               }
            }
         }
         if (count < 0)
            Logger.Log(LogEnum.LE_ERROR, "ReduceFoods(): invalid state count<0 fs=" + foodStore.ToString());
      }
      public int GetCoins()
      {
         int coins = 0;
         foreach (IMapItem mi in PartyMembers)
            coins += mi.Coin;
         return coins;
      }
      public bool AddCoins(int coins, bool isCoinsShared = true)
      {
         if (0 == coins)
            return true;
         //---------------------------------
         int looterShare = 1;
         if (true == isCoinsShared) // need to give equal share for each looter
         {
            foreach (IMapItem mi in PartyMembers)
            {
               if (true == mi.IsLooter)
                  ++looterShare;
            }
         }
         int remainingCoins = (int)Math.Ceiling((decimal)coins / (decimal)looterShare); // looters get their share and it disappears forever
         LooterCoin += (coins - remainingCoins);
         //---------------------------------
         int fickleShare = 1;
         if (true == isCoinsShared) // need to give equal share for each looter
         {
            foreach (IMapItem mi in PartyMembers)
            {
               if (true == mi.IsFickle)
               {
                  ++fickleShare;
                  break;
               }
            }
         }
         remainingCoins = (int)Math.Ceiling((decimal)coins / (decimal)fickleShare); // fickle get equal share as Prince
         this.FickleCoin += (coins - remainingCoins);
         //---------------------------------
         IMapItems sortedMapItems = PartyMembers.SortOnCoin();
         sortedMapItems.Reverse();
         foreach (IMapItem mi in sortedMapItems) // add to party members to get to 100 increment
         {
            if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
               continue;
            int remainder1 = mi.Coin % 100;
            if (0 != remainder1)
            {
               int freeLoad = mi.GetFreeLoad();
               if (freeLoad < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "AddCoins(): GetFreeLoad() returned fl=" + freeLoad.ToString());
                  return false;
               }
               if (0 < freeLoad)
               {
                  int diff1 = 100 - remainder1;
                  if (remainingCoins <= diff1)
                  {
                     int total0 = remainingCoins + mi.Coin;
                     Logger.Log(LogEnum.LE_REMOVE_KILLED, "AddCoins(): " + mi.Name + " ++++>>> " + mi.Coin.ToString() + " + " + remainingCoins.ToString() + " = " + total0.ToString() + " fl=" + freeLoad.ToString());
                     mi.Coin += remainingCoins;
                     return true;
                  }
                  int total = diff1 + mi.Coin;
                  Logger.Log(LogEnum.LE_REMOVE_KILLED, "AddCoins(): " + mi.Name + " ++++>>> " + mi.Coin.ToString() + " + " + diff1.ToString() + " = " + total.ToString() + " fl=" + freeLoad.ToString());
                  mi.Coin += diff1;
                  remainingCoins -= diff1;
               }
            }
         }
         //---------------------------------
         int count = 100;
         while (0 < --count) // repeat adding in 100 increments
         {
            IMapItems sortedMapItems1 = PartyMembers.SortOnCoin();
            sortedMapItems1.Reverse();
            foreach (IMapItem mi in sortedMapItems1)
            {
               if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
                  continue;
               int freeLoad = mi.GetFreeLoad();
               if (freeLoad < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "AddCoins(): GetFreeLoad() returned fl=" + freeLoad.ToString());
                  return false;
               }
               if (0 < freeLoad)
               {
                  int remainder3 = mi.Coin % 100;
                  int diff3 = 100 - remainder3;
                  if (remainingCoins <= diff3)
                  {
                     int total0 = remainingCoins + mi.Coin;
                     Logger.Log(LogEnum.LE_REMOVE_KILLED, "AddCoins(): " + mi.Name + " ++++>>> " + mi.Coin.ToString() + " + " + remainingCoins.ToString() + " = " + total0.ToString());
                     mi.Coin += remainingCoins;
                     return true;
                  }
                  int total = diff3 + mi.Coin;
                  Logger.Log(LogEnum.LE_REMOVE_KILLED, "AddCoins(): " + mi.Name + " ++++>>> " + mi.Coin.ToString() + " + " + diff3.ToString() + " = " + total.ToString());
                  mi.Coin += diff3;
                  remainingCoins -= diff3;
               }
            }
         }
         if (count < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddCoins(): invalid state count<0 coins=" + remainingCoins.ToString());
            return false;
         }
         return true;
      }
      public void ReduceCoins(int coins)
      {
         if (coins < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): invalid parameter coins=" + coins.ToString());
            coins = 0;
         }
         if (0 == coins)
            return;
         IMapItems sortedMapItems = PartyMembers.SortOnCoin();
         foreach (IMapItem mi in sortedMapItems)  // now from party members to get to 100 increments
         {
            if ("Prince" == mi.Name)
               continue;
            if (0 < mi.Coin)
            {
               int remainder = mi.Coin % 100;
               if (coins <= remainder)
               {
                  mi.Coin -= coins;
                  return;
               }
               coins -= remainder;
               mi.Coin -= remainder;
            }
         }
         //---------------------------------
         int count = 100;
         while (0 < --count)
         {
            IMapItems sortedMapItems1 = PartyMembers.SortOnCoin();
            foreach (IMapItem mi in sortedMapItems1)
            {
               if (0 < mi.Coin)
               {
                  int remainder2 = mi.Coin % 100;
                  if ((0 == remainder2) && (99 < mi.Coin))
                     remainder2 = 100;
                  if (coins <= remainder2)
                  {
                     mi.Coin -= coins;
                     return;
                  }
                  mi.Coin -= remainder2;
                  coins -= remainder2;
               }
            }
         }
         if (count < 0)
            Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): 1 - invalid state count<0 coins=" + coins.ToString());
         //---------------------------------
         if (0 < myPrince.Coin)
         {
            int remainder1 = myPrince.Coin % 100; // now remove from prince
            if (coins <= remainder1)
            {
               myPrince.Coin -= coins;
               return;
            }
            myPrince.Coin -= remainder1;
            coins -= remainder1;
         }
         //---------------------------------
         count = 1000;
         while (0 < --count)
         {
            foreach (IMapItem mi in PartyMembers)  // now from any party members 
            {
               if (0 < mi.Coin)
               {
                  --mi.Coin;
                  --coins;
               }
               if (0 == coins)
                  return;
            }
         }
         if (count < 0)
            Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): 2 - invalid state count<0 coins=" + coins.ToString());
         if (0 != coins)
            Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): not reduced to zero coins=" + coins.ToString());
      }
      public bool StripCoins()
      {
         int capturedCoins = 0;
         List<int> wealthCodes = new List<int>();
         foreach (int wc in this.CapturedWealthCodes)
         {
            if (4 < wc)
            {
               wealthCodes.Add(wc);
            }
            else
            {
               int coin = GameEngine.theTreasureMgr.GetCoin(wc);
               if (coin < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "StripCoins(): GetCoin()=" + coin.ToString() + " wc=" + wc.ToString());
                  return false;
               }
               capturedCoins += coin;
            }
         }
         if( false == AddCoins(capturedCoins) )
         {
            Logger.Log(LogEnum.LE_ERROR, "StripCoins(): AddCoins() returned false");
            return false;
         }
         CapturedWealthCodes = wealthCodes;
         return true;
      }
      public int GetNonSpecialMountCount(bool isHorseOnly = false)
      {
         int mountCount = 0;
         foreach (IMapItem mi in PartyMembers)
         {
            foreach (IMapItem mount in mi.Mounts)
            {
               if (true == mount.IsFlyingMountCarrier())
                  continue;
               if ( true == isHorseOnly )
               {
                  if (true == mount.Name.Contains("Horse"))
                     ++mountCount;
               }
               else
               {
                  ++mountCount;
               }
            }
         }
         return mountCount;
      }
      public bool AddNewMountToParty(MountEnum mt = MountEnum.Horse)
      {
         //--------------------------------
         // Add to Prince first if he does not have one
         bool isGriffonOwned = false;
         bool isHarpyOwned = false;
         bool isPegasusOwned = false;
         bool isHorseOwned = false;
         foreach (IMapItem mount in myPrince.Mounts) // If not owned, Add to Prince first
         {
            if (true == mount.Name.Contains("Griffon"))
               isGriffonOwned = true;
            if (true == mount.Name.Contains("Harpy"))
               isHarpyOwned = true;
            else if (true == mount.Name.Contains("Pegasus"))
               isPegasusOwned = true;
            else
               isHorseOwned = true;
         }
         if ((MountEnum.Griffon == mt) && (false == isGriffonOwned))
         {
            if( false == myPrince.AddNewMount(mt) )
            {
               Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): AddNewMount() returned false for mi=" + myPrince.Name + " for mount=" + mt.ToString());
               return false;
            }
            return true;
         }
         if ((MountEnum.Harpy == mt) && (false == isHarpyOwned))
         {
            if (false == myPrince.AddNewMount(mt))
            {
               Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): AddNewMount() returned false for mi=" + myPrince.Name + " for mount=" + mt.ToString());
               return false;
            }
            return true;
         }
         if ((MountEnum.Pegasus == mt) && (false == isGriffonOwned) && (false == isHarpyOwned) && (false == isPegasusOwned))
         {
            if (false == myPrince.AddNewMount(mt))
            {
               Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): AddNewMount() returned false for mi=" + myPrince.Name + " for mount=" + mt.ToString());
               return false;
            }
            return true;
         }
         if ((false == isGriffonOwned) && (false == isHarpyOwned)  && (false == isPegasusOwned) && (false == isHorseOwned))
         {
            if (false == myPrince.AddNewMount(mt))
            {
               Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): AddNewMount() returned false for mi=" + myPrince.Name + " for mount=" + mt.ToString());
               return false;
            }
            return true;
         }
         //--------------------------------
         // Add to member with least number of mounts
         IMapItems sortedMapItems = PartyMembers.SortOnMount();
         sortedMapItems.Reverse();
         foreach (IMapItem partyMember in sortedMapItems) // add to partymember without a mount
         {
            if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || (0 < partyMember.Mounts.Count) || (true == partyMember.IsFlyer()) )
               continue;
            if (false == partyMember.AddNewMount(mt))
            {
               Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): AddNewMount() returned false for mi=" + partyMember.Name + " for mount=" + mt.ToString());
               return false;
            }
            return true;
         }
         //--------------------------------
         if (MountEnum.Griffon == mt)
         {
            foreach (IMapItem partyMember in sortedMapItems) // If this is a pegasus, add to partymember that has no pegasus
            {
               if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || ("Prince" == partyMember.Name) || (true == partyMember.IsFlyer()) )
                  continue;
               isGriffonOwned = false;
               foreach (IMapItem mount in partyMember.Mounts)
               {
                  if (true == mount.Name.Contains("Griffon"))
                     isGriffonOwned = true;
               }
               if (false == isGriffonOwned)
               {
                  if (false == partyMember.AddNewMount(mt))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): AddNewMount() returned false for mi=" + partyMember.Name + " for mount=" + mt.ToString());
                     return false;
                  }
                  return true;
               }
            }
            return true; // Cannot be added since every character already riding griffon
         }
         //--------------------------------
         if (MountEnum.Harpy == mt)
         {
            foreach (IMapItem partyMember in sortedMapItems) // If this is a pegasus, add to partymember that has no pegasus
            {
               if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || ("Prince" == partyMember.Name) || (true == partyMember.IsFlyer()))
                  continue;
               isHarpyOwned = false;
               foreach (IMapItem mount in partyMember.Mounts)
               {
                  if (true == mount.Name.Contains("Harpy"))
                     isHarpyOwned = true;
               }
               if (false == isHarpyOwned)
               {
                  if (false == partyMember.AddNewMount(mt))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): AddNewMount() returned false for mi=" + partyMember.Name + " for mount=" + mt.ToString());
                     return false;
                  }
                  return true;
               }
            }
            return true; // Harpy cannot be added since every character already riding harpy
         }
         //--------------------------------
         if (MountEnum.Pegasus == mt) // If this is a pegasus, add to partymember that has none
         {
            foreach (IMapItem partyMember in sortedMapItems)
            {
               if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || ("Prince" == partyMember.Name) || (true == partyMember.IsFlyer()) )
                  continue;
               isPegasusOwned = false;
               foreach (IMapItem mount in partyMember.Mounts)
               {
                  if (true == mount.Name.Contains("Pegasus"))
                     isPegasusOwned = true;
               }
               if (false == isPegasusOwned)
               {
                  if (false == partyMember.AddNewMount(mt))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): AddNewMount() returned false for mi=" + partyMember.Name + " for mount=" + mt.ToString());
                     return false;
                  }
                  return true;
               }
            }
         }
         //--------------------------------
         while ("Prince" != PartyMembers[0].Name) // get to top
            PartyMembers.Rotate(1);
         foreach (IMapItem partyMember in PartyMembers) // add to first conscious person which is hopefully the prince
         {
            if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || (true == partyMember.IsFlyer()))
               continue;
            if (false == partyMember.AddNewMount(mt))
            {
               Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): AddNewMount() returned false for mi=" + partyMember.Name + " for mount=" + mt.ToString());
               return false;
            }
            return true;
         }
         Logger.Log(LogEnum.LE_ERROR, "AddNewMountToParty(): reached default for mount=" + mt.ToString());
         return true;
      }
      public void ReduceMount(MountEnum mt)
      {
         IMapItems sortedMapItems = PartyMembers.SortOnMount();
         foreach (IMapItem mi in sortedMapItems)
         {
            int mountCount = 0;
            foreach (IMapItem mount in mi.Mounts) // First remove from MapItem that has multiple mounts
            {
               if ( (MountEnum.Any == mt) && ((false == mount.Name.Contains("Griffon")) && (false == mount.Name.Contains("Harpy"))) )
                  ++mountCount;
               else if ((MountEnum.Horse == mt) && (true == mount.Name.Contains("Horse")))
                  ++mountCount;
               else if ((MountEnum.Pegasus == mt) && (true == mount.Name.Contains("Pegasus")))
                  ++mountCount;
               if (1 < mountCount)
               {
                  Logger.Log(LogEnum.LE_MOUNT_CHANGE, "ReduceMount(): remove=" + mount.Name + " from mi=" + mi.Name);
                  mi.Mounts.Remove(mount.Name);
                  return;
               }
            }
            //----------------------------
            foreach (IMapItem mount in mi.Mounts) // Next remove from MapItem that has mount of proper type
            {
               if ("Prince" == mi.Name)
                  continue;
               if ((MountEnum.Any == mt) && ((false == mount.Name.Contains("Griffon")) && (false == mount.Name.Contains("Harpy"))) )
               {
                  Logger.Log(LogEnum.LE_MOUNT_CHANGE, "ReduceMount(): remove=" + mount.Name + " from mi=" + mi.Name);
                  mount.Rider = null;
                  mi.Mounts.Remove(mount.Name);
                  return;
               }
               else if ((MountEnum.Horse == mt) && (true == mount.Name.Contains("Horse")))
               {
                  Logger.Log(LogEnum.LE_MOUNT_CHANGE, "ReduceMount(): remove=" + mount.Name + " from mi=" + mi.Name);
                  mi.Mounts.Remove(mount.Name);
                  return;
               }
               else if ((MountEnum.Pegasus == mt) && (true == mount.Name.Contains("Pegasus")))
               {
                  Logger.Log(LogEnum.LE_MOUNT_CHANGE, "ReduceMount(): remove=" + mount.Name + " from mi=" + mi.Name);
                  mi.Mounts.Remove(mount.Name);
                  return;
               }
            }
         }
         //----------------------------
         foreach (IMapItem mount in myPrince.Mounts) // Finally, remove from Prince last
         {
            if ((MountEnum.Any == mt) && ((false == mount.Name.Contains("Griffon")) && (false == mount.Name.Contains("Harpy"))))
            {
               Logger.Log(LogEnum.LE_MOUNT_CHANGE, "ReduceMount(): remove=" + mount.Name + " from mi=Prince");
               mount.Rider = null;
               myPrince.Mounts.Remove(mount.Name);
               return;
            }
            else if ((MountEnum.Horse == mt) && (true == mount.Name.Contains("Horse")))
            {
               Logger.Log(LogEnum.LE_MOUNT_CHANGE, "ReduceMount(): remove=" + mount.Name + " from mi=Prince");
               myPrince.Mounts.Remove(mount.Name);
               return;
            }
            else if ((MountEnum.Pegasus == mt) && (true == mount.Name.Contains("Pegasus")))
            {
               Logger.Log(LogEnum.LE_MOUNT_CHANGE, "ReduceMount(): remove=" + mount.Name + " from mi=Prince");
               myPrince.Mounts.Remove(mount.Name);
               return;
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "ReduceMount() - unable to remove mount");
      }
      public void TransferMounts(IMapItems mounts)
      {
         List<IMapItem> griffons = new List<IMapItem>();
         List<IMapItem> harpies = new List<IMapItem>();
         List<IMapItem> pegasuses = new List<IMapItem>();
         List<IMapItem> horses = new List<IMapItem>();
         foreach (IMapItem mount in mounts)
         {
            if (true == mount.Name.Contains("Griffon"))
               griffons.Add(mount);
            else if (true == mount.Name.Contains("Harpy"))
               harpies.Add(mount);
            else if (true == mount.Name.Contains("Pegasus"))
               pegasuses.Add(mount);
            else if (true == mount.Name.Contains("Horse"))
               horses.Add(mount);
            else
               Logger.Log(LogEnum.LE_ERROR, "TransferMounts(): Invalid state m.Name=" + mount.Name);
         }
         mounts.Clear();
         //---------------------------------------
         while ("Prince" != PartyMembers[0].Name) // get Prince to top
            PartyMembers.Rotate(1);
         //---------------------------------------
         IMapItem firstConsciousMapItem = null;
         foreach (IMapItem partyMember in PartyMembers) // Find the first conscious party member that can get mounts. Hopefully this is Prince
         {
            if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || (true == partyMember.IsFlyer()))
               continue;
            if (null == firstConsciousMapItem)
               firstConsciousMapItem = partyMember;
         }
         if (null == firstConsciousMapItem) // if there is no conscious member to assign mounts, mount disappears
         {
            Logger.Log(LogEnum.LE_ERROR, "TransferMounts(): assigning mounts to unconscious Prince");
            return; 
         }
         //---------------------------------------
         int assignedGriffonCount = 0;
         int maxGriffonCount = griffons.Count;
         foreach (IMapItem partyMember in PartyMembers) 
         {
            if (assignedGriffonCount == maxGriffonCount)
               break;
            if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || (true == partyMember.IsFlyer()) )
               continue;
            partyMember.AddMount(griffons[assignedGriffonCount]); // Griffon gets assigned a rider
            assignedGriffonCount++;
         }
         int unassignedCount = maxGriffonCount - assignedGriffonCount; // Any unassigned got to first conscious member which is hopefully Prince
         for (int i = 0; i < unassignedCount; i++)
            firstConsciousMapItem.AddMount(griffons[i]);
         //---------------------------------------
         int assignedHarpyCount = 0;
         int maxHarpyCount = harpies.Count;
         foreach (IMapItem partyMember in PartyMembers) 
         {
            if (assignedHarpyCount == maxHarpyCount)
               break;
            if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || (true == partyMember.IsFlyer()))
               continue;
            partyMember.AddMount(harpies[assignedHarpyCount]); // Griffon gets assigned a rider
            assignedHarpyCount++;
         }
         unassignedCount = maxHarpyCount - assignedHarpyCount; // Any unassigned got to first conscious member which is hopefully Prince
         for (int i = 0; i < unassignedCount; i++)
            firstConsciousMapItem.AddMount(harpies[i]);
         //---------------------------------------
         int assignedPegasusCount = 0;
         int maxPegasusCount = pegasuses.Count;
         foreach (IMapItem partyMember in PartyMembers) 
         {
            if (assignedPegasusCount == maxPegasusCount)
               break;
            if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || (true == partyMember.IsFlyer()) || (0 < partyMember.Mounts.Count))
               continue;
            partyMember.AddMount(pegasuses[assignedPegasusCount]);
            assignedPegasusCount++;
         }
         unassignedCount = maxPegasusCount - assignedPegasusCount; // Any unassigned got to first conscious member which is hopefully Prince
         for (int i = 0; i < unassignedCount; i++)
            firstConsciousMapItem.AddMount(pegasuses[i]);
         //---------------------------------------
         int assignedHorseCount = 0;
         int maxHorseCount = horses.Count;
         foreach (IMapItem partyMember in PartyMembers)
         {
            if (assignedHorseCount == maxHorseCount)
               break;
            if ((true == partyMember.IsUnconscious) || (true == partyMember.IsKilled) || (true == partyMember.IsFlyer()) || (0 < partyMember.Mounts.Count))
               continue;
            partyMember.AddMount(horses[assignedHorseCount]);
            assignedHorseCount++;
         }
         unassignedCount = maxHorseCount - assignedHorseCount;
         for (int i = 0; i < unassignedCount; i++)
            firstConsciousMapItem.AddMount(horses[i]);
      }
      //---------------------------------------------------------------
      public bool IsSpecialItemHeld(SpecialEnum item)
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if (true == mi.IsSpecialItemHeld(item))
               return true;
         }
         return false;
      }
      public int GetCountSpecialItem(SpecialEnum possession)
      {
         int count = 0;
         foreach (IMapItem mi in PartyMembers)
         {
            foreach (SpecialEnum p in mi.SpecialKeeps)
            {
               if (p == possession)
                  ++count;
            }
            foreach (SpecialEnum p in mi.SpecialShares)
            {
               if (p == possession)
                  ++count;
            }
         }
         return count;
      }
      public void AddSpecialItem(SpecialEnum possession, IMapItem mi = null)
      {
         bool IsRoyalHelmHeldStart = IsSpecialItemHeld(SpecialEnum.RoyalHelmOfNorthlands);
         if (null != mi) // Add to this guys special keepers list as he did the work for it
         {
            if ("Prince" != mi.Name)
            {
               mi.AddSpecialItemToKeep(possession);
               if ((false == IsRoyalHelmHeldStart) && (SpecialEnum.RoyalHelmOfNorthlands == possession))
                  WitAndWile += 1;
               return;
            }
         }
         //--------------------------------
         // Adding to Share list instead of Keep List
         if ((false == myPrince.IsSpecialItemHeld(possession)) && (false == myPrince.IsUnconscious) && (false == myPrince.IsKilled))
         {
            myPrince.AddSpecialItemToShare(possession);
            if ((false == IsRoyalHelmHeldStart) && (SpecialEnum.RoyalHelmOfNorthlands == possession))
               WitAndWile += 1;
            return;
         }
         foreach (IMapItem member in PartyMembers)
         {
            if ((true == member.IsKilled) || (true == member.IsUnconscious))
               continue;
            if (true == member.IsSpecialItemHeld(possession))
               continue;
            member.AddSpecialItemToShare(possession);
            if ((false == IsRoyalHelmHeldStart) && (SpecialEnum.RoyalHelmOfNorthlands == possession))
               WitAndWile += 1;
            return;
         }
         if((false == myPrince.IsUnconscious) && (false == myPrince.IsKilled))
         {
            myPrince.AddSpecialItemToShare(possession); // If nobody adds it, add to prince
            if ((false == IsRoyalHelmHeldStart) && (SpecialEnum.RoyalHelmOfNorthlands == possession))
               WitAndWile += 1;
            return;
         }
      }
      public void AddSpecialItems(List<SpecialEnum> possessions)
      {
         foreach (SpecialEnum possession in possessions)
            AddSpecialItem(possession);
      }
      public bool RemoveSpecialItem(SpecialEnum possession, IMapItem mi = null)
      {
         bool IsRoyalHelmHeldStart = IsSpecialItemHeld(SpecialEnum.RoyalHelmOfNorthlands);
         bool isPossessionRemoved = false;
         if (null != mi)
         {
            if (true == mi.RemoveSpecialItem(possession))
               isPossessionRemoved = true;
         }
         else
         {
            foreach (IMapItem member in PartyMembers)
            {
               if (true == member.RemoveSpecialItem(possession))
               {
                  isPossessionRemoved = true;
                  break;
               }
            }
         }
         //-------------------------------------
         if ((true == IsRoyalHelmHeldStart) && (false == IsSpecialItemHeld(SpecialEnum.RoyalHelmOfNorthlands)))
            WitAndWile -= 1;
         return isPossessionRemoved;
      }
      public bool IsFedSlaveGirlHeld()
      {
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("SlaveGirl"))
            {
               if (0 == member.StarveDayNum)
                  return true;
            }
         }
         return false;
      }
      public bool IsPartySizeOne()
      {
         // Party Size may be affected by how many porter slaves
         // They do not count when determining party size.
         // Additionally, True Loves do not count if equal to one.
         if (1 == PartyMembers.Count)
            return true;
         int numTrueLoves = 0;
         foreach (IMapItem mi in PartyMembers)
         {
            if ("Prince" == mi.Name)
               continue;
            bool isTrueLove = mi.Name.Contains("TrueLove");
            if ((false == mi.Name.Contains("PorterSlave")) && (false == isTrueLove))
               return false;
            if (true == isTrueLove)
               ++numTrueLoves;
         }
         if (1 != numTrueLoves)
            return false;
         return true;
      }
      public IMapItem RemoveFedSlaveGirl()
      {
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("SlaveGirl"))
            {
               if (0 == member.StarveDayNum)
               {
                  this.RemoveAbandonerInParty(member);
                  return member;
               }
            }
         }
         return null;
      }
      //---------------------------------------------------------------
      public bool IsPartyFlying()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if (false == mi.IsFlying)
               return false;
         }
         return true;
      }
      public bool PartyReadyToFly()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if (mi.GetFlyLoad() < 0)
               return false;
         }
         return true;
      }
      public bool IsPartyRiding()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if ( (false == mi.IsRiding) && (false == mi.IsFlyer()) )
               return false;
         }
         return true;
      }
      public bool IsEncounteredFlying()
      {
         if (0 == EncounteredMembers.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEncounteredFlying() Invalid State b/c  EncounteredMembers is empty");
            return false;
         }
         foreach (IMapItem mi in EncounteredMembers)
         {
            if (false == mi.IsFlying)
               return false;
         }
         return true;
      }
      public bool IsEncounteredRiding()
      {
         if (0 == EncounteredMembers.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEncounteredRiding() Invalid State b/c  EncounteredMembers is empty");
            return false;
         }
         foreach (IMapItem mi in EncounteredMembers)
         {
            if ( (false == mi.IsRiding) && (false == mi.IsFlyer()) )
               return false;
         }
         return true;
      }
      public bool IsSpecialistInParty()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if ((true == mi.Name.Contains("Magician")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Wizard")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Witch")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Priest")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Monk")) && (false == mi.IsUnconscious))
               return true;
         }
         return false;
      }
      public bool IsMagicInParty(IMapItems mapItems = null)
      {
         if (null == mapItems)
            mapItems = PartyMembers;
         foreach (IMapItem mi in mapItems)
         {
            if ((true == mi.Name.Contains("Magician")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Wizard")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Witch")) && (false == mi.IsUnconscious))
               return true;
         }
         return false;
      }
      public bool IsReligionInParty(IMapItems mapItems = null)
      {
         if (null == mapItems)
            mapItems = PartyMembers;
         foreach (IMapItem mi in mapItems)
         {
            if ((true == mi.Name.Contains("Monk")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Priest")) && (false == mi.IsUnconscious))
               return true;
         }
         return false;
      }
      public bool IsFalconInParty()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if ( true == mi.Name.Contains("Falcon") )
               return true;
         }
         return false;
      }
      public bool IsMonkInParty()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if ((true == mi.Name.Contains("Monk")) && (false == mi.IsUnconscious))
               return true;
         }
         return false;
      }
      public bool IsPixieLoverInParty()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if ((true == mi.Name.Contains("Magician")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Wizard")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Witch")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Elf")) && (false == mi.IsUnconscious))
               return true;
            if ((true == mi.Name.Contains("Halfling")) && (false == mi.IsUnconscious))
               return true;
         }
         return false;
      }
      public bool IsLooterInParty() 
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if (true == mi.IsLooter)
               return true;
         }
         return false;
      }
      public bool IsHirelingsInParty()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if (0 < mi.Wages)
               return true;
         }
         return false;
      }
      public bool IsMinstrelInParty()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if ((true == mi.Name.Contains("Minstrel")) && (false == mi.IsPlayedMusic))
               return true;
         }
         return false;
      }
      public bool MinstrelStart()
      {
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("Minstrel"))
            {
               if (false == member.IsPlayedMusic)
               {
                  this.IsMinstrelPlaying = true;
                  this.IsPartyLodged = true;
                  member.IsPlayedMusic = true;
                  member.OverlayImageName = "Deny";
                  return true;
               }
            }
         }
         return false;
      }
      public bool IsInMapItems(string name, IMapItems mapItems = null)
      {
         if (null == mapItems)
            mapItems = PartyMembers;
         foreach (IMapItem mi in mapItems)
         {
            if (true == mi.Name.Contains(name))
               return true;
         }
         return false;
      }
      //---------------------------------------------------------------
      public void RemoveKilledInParty(string reason, bool isEscaping = false)
      {
         if (true == Prince.IsKilled) // If prince killed, no need to look at other members
            return;
         //--------------------------------
         int numTrueLovesBefore = 0;
         bool isMemberKilled = false;
         //--------------------------------
         foreach (IMapItem member in PartyMembers) // If Griffon/Harpy rider is killed, remove it from Griffon/Harpy
         {
            if (null != member.Rider)
            {
               if (true == member.Rider.IsKilled) // If killed, remove from griffon/harpy
               {
                  member.Rider.Mounts.Remove(member);
                  member.Rider = null;
               }
            }
         }
         //--------------------------------
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("TrueLove"))
               ++numTrueLovesBefore;
            if (true == member.IsKilled)
            {
               isMemberKilled = true;
               if (true == member.Name.Contains("ElfWarrior"))
                  --this.WitAndWile;
            }
            if( null != member.Rider ) // If Griffon/Harpy is killed, and it has a rider, must remove
            {
               member.Rider.IsFlying = false;
               member.Rider.IsRiding = false;
               member.Rider.Mounts.Remove(member);
               member.Rider = null;
            }
         }
         //--------------------------------
         if (true == isMemberKilled)
         {
            IMapItems fickleMembers = new MapItems();
            foreach (IMapItem mi in PartyMembers) // the fickle members disappear
            {
               if (true == mi.IsFickle)
                  fickleMembers.Add(mi);
            }
            foreach (IMapItem mi in fickleMembers)
               RemoveAbandonerInParty(mi);
         }
         //---------------------------------------------
         IMapItems members = new MapItems();
         foreach (IMapItem mi in PartyMembers)
            members.Add(mi);
         IMapItems killedMembers0 = new MapItems();
         foreach (IMapItem mi in members)
         {
            if (true == mi.Name.Contains("Undead")) // remove any undead warriors that were crated by Hydra Teeth - See e140
               killedMembers0.Add(mi);
            if (true == mi.IsKilled)
            {
               Logger.Log(LogEnum.LE_REMOVE_KILLED, "RemoveKilledInParty(): ================"+mi.Name+"=KIA c=" + mi.Coin.ToString() + " f=" + mi.Food.ToString() + "=========================");
               isMemberKilled = true;
               killedMembers0.Add(mi);
               if (false == isEscaping)
               {
                  TransferMounts(mi.Mounts);
                  AddSpecialItems(mi.SpecialKeeps);
                  AddSpecialItems(mi.SpecialShares);
                  if (false == AddFoods(mi.Food))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "RemoveKilledInParty(): AddFoods() returned false");
                     return;
                  }
                  if( false == AddCoins(mi.Coin, false))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "RemoveKilledInParty(): AddCoins() returned false");
                     return;
                  }
                  mi.Food = 0;
                  mi.Coin = 0;
                  mi.Mounts.Clear();
                  mi.SpecialShares.Clear();
                  mi.SpecialKeeps.Clear();
               }
            }
            if (true == mi.IsUnconscious)
            {
               Logger.Log(LogEnum.LE_REMOVE_KILLED, "RemoveKilledInParty(): ----------------" + mi.Name + "=MIA c=" + mi.Coin.ToString() + " f=" + mi.Food.ToString() + "-------------------------");
               if (false == isEscaping)
               {
                  TransferMounts(mi.Mounts);
                  AddSpecialItems(mi.SpecialShares);
                  if (false == AddFoods(mi.Food))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "RemoveKilledInParty(): AddFoods() returned false");
                     return;
                  }
                  if (false == AddCoins(mi.Coin, false))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "RemoveKilledInParty(): AddCoins() returned false");
                     return;
                  }
                  mi.Food = 0;
                  mi.Coin = 0;
                  mi.Mounts.Clear();
                  mi.SpecialShares.Clear();
               }
            }
         }
         foreach (IMapItem mi in killedMembers0)
            PartyMembers.Remove(mi);
         //--------------------------------
         int numTrueLovesAfter = 0;
         IsMerchantWithParty = false;
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("TrueLove"))
               ++numTrueLovesAfter;
            if (true == member.Name.Contains("Merchant"))
               IsMerchantWithParty = true;
         }
         if ((1 == numTrueLovesAfter) && (1 < numTrueLovesBefore))      // If the number of True Loves is one and there was no triangle, increase wit and wiles
            ++this.WitAndWile;
         else if ((1 != numTrueLovesAfter) && (1 == numTrueLovesBefore)) // If the number of True Loves changed from one, decrease wit and wiles
            --this.WitAndWile;
      }
      public bool RemoveVictimInParty(IMapItem victim)
      {
         if ("Prince" == victim.Name)
         {
            int remainingHealth = victim.Endurance - victim.Wound - victim.Poison;
            victim.SetWounds(remainingHealth, 0);
            return true;
         }
         //---------------------------------------------
         int numTrueLovesBefore = 0;
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("TrueLove"))
               ++numTrueLovesBefore;
         }
         //---------------------------------------------
         if (true == victim.Name.Contains("ElfWarrior"))
            --this.WitAndWile;
         //--------------------------------
         IMapItems fickleMembers = new MapItems();
         foreach (IMapItem mi in PartyMembers) // the fickle members disappear
         {
            if (true == mi.IsFickle)
               fickleMembers.Add(mi);
         }
         foreach (IMapItem mi in fickleMembers)
            RemoveAbandonerInParty(mi);
         //--------------------------------
         if (true == victim.Name.Contains("Griffon"))
         {
            IMapItem griffon = victim;
            if (null != griffon.Rider) // If Griffon has a rider, it is the victim that is being removed
               victim = griffon.Rider;
            griffon.Rider = null;
         }
         //--------------------------------
         if (true == victim.Name.Contains("Harpy"))
         {
            IMapItem harpy = victim;
            if (null != harpy.Rider) // If Harpy has a rider, it is the victim that is being removed
               victim = harpy.Rider;
            harpy.Rider = null;
         }
         //---------------------------------------------
         if (false == victim.RemoveVictimMountAndLoad()) // remove mount, coin, food that victim is carrying
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveVictimInParty(): RemoveVictimLoads() returned false");
            return false;
         }
         IMapItems mounts = new MapItems();
         foreach (IMapItem mount in victim.Mounts)
            mounts.Add(mount);
         PartyMembers.Remove(victim);
         victim.Mounts.Clear();
         TransferMounts(mounts);
         if (false == AddFoods(victim.Food)) // all other food, coin, mounts transfer to other party members
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveVictimInParty(): AddFoods() returned false");
            return false;
         }
         if (false == AddCoins(victim.Coin, false))
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveVictimInParty(): AddCoins() returned false");
            return false;
         }
         //--------------------------------
         int numTrueLovesAfter = 0;
         IsMerchantWithParty = false;
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("TrueLove"))
               ++numTrueLovesAfter;
            if (true == member.Name.Contains("Merchant"))
               IsMerchantWithParty = true;
         }
         if ((1 == numTrueLovesAfter) && (1 < numTrueLovesBefore))      // If the number of True Loves is one and there was no triangle, increase wit and wiles
            ++this.WitAndWile;
         else if ((1 != numTrueLovesAfter) && (1 == numTrueLovesBefore)) // If the number of True Loves changed from one, decrease wit and wiles
            --this.WitAndWile;
         return true;
      }
      public bool RemoveBelongingsInParty(bool isMountsLost=true)
      {
         this.LetterOfRecommendations.Clear();
         foreach (IMapItem mi in PartyMembers)
         {
            mi.Food = 0;
            mi.Coin = 0;
            mi.WealthCode = 0;
            if (null != mi.Rider) // mi = griffon/harpy
            {
               mi.Rider.Mounts.Remove(mi);  // Griffon/Harpy Rider removes griffon/harpy as mount
               mi.Rider = null;            
            }
         }
         if (true == isMountsLost)
         {
            foreach (IMapItem mi in PartyMembers)
            {
               if (false == mi.IsFlyer())
               {
                  mi.IsRiding = false;
                  mi.IsFlying = false;
               }
               mi.Mounts.Clear();
            }
         }
         //--------------------------------------------------
         int count = 1000;
         bool isModified = true;
         while ((true == isModified) && (0 < --count))
         {
            isModified = false;
            foreach (IMapItem mi in PartyMembers)
            {
               foreach (SpecialEnum possession in mi.SpecialKeeps)
               {
                  isModified = true;
                  mi.RemoveSpecialItem(possession);
                  break;
               }
            }
         }
         if (count < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveBelongingsInParty(): 1-invalid state count < 0");
            return false;
         }
         //--------------------------------------------------
         count = 1000;
         isModified = true;
         while ((true == isModified) && (0 < --count))
         {
            isModified = false;
            foreach (IMapItem mi in PartyMembers)
            {
               foreach (SpecialEnum possession in mi.SpecialShares)
               {
                  isModified = true;
                  mi.RemoveSpecialItem(possession);
                  break;
               }
            }
         }
         if (count < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveBelongingsInParty(): 2-invalid state count < 0");
            return false;
         }
         return true;
      }
      public int RemoveLeaderlessInParty()
      {
         IMapItems partyMembers = new MapItems();
         int numTrueLovesBefore = 0;
         foreach (IMapItem mi in PartyMembers)
         {
            if ("Prince" != mi.Name)
               partyMembers.Add(mi);
            if (true == mi.Name.Contains("TrueLove"))
               ++numTrueLovesBefore;
         }
         if (1 == numTrueLovesBefore)  // remove effects of true love if only had one true love
            --this.WitAndWile;
         //--------------------------------
         IsMerchantWithParty = false; // remove effects of having a merchant
         //--------------------------------
         int count = partyMembers.Count;
         foreach (IMapItem mi in partyMembers)
         {
            if (true == mi.Name.Contains("ElfWarrior"))
               --this.WitAndWile;
            PartyMembers.Remove(mi);
         }
         return count;
      } // Party leaves Prince assuming he is dead
      public void RemoveAbandonerInParty(IMapItem mi, bool isTrueLoveRemoved = false)
      {
         if ("Prince" == mi.Name) // the prince never runs away
            return;
         //--------------------------------
         int numTrueLoves = 0;
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("TrueLove"))
               ++numTrueLoves;
         }
         if (true == mi.Name.Contains("TrueLove"))
         {
            if (false == isTrueLoveRemoved) // true love may find a way to return if this is false
               LostTrueLoves.Add(mi);
            if (2 == numTrueLoves)       // add the effects of true love - going to one true love
               ++this.WitAndWile;
            else if (1 == numTrueLoves)  // remove effects of true love  - going to no true love
               --this.WitAndWile;
         }
         //--------------------------------
         if ( true == mi.IsFlyingMountCarrier() )
         {
            if( null != mi.Rider )
               mi.Rider.Mounts.Remove(mi);
            mi.Rider = null;
         }
         if( 0 < mi.Mounts.Count )
         {
            IMapItem mount = mi.Mounts[0];
            if( true == mount.IsFlyingMountCarrier() )
            {
               if (null != mi.Rider)
                  mi.Mounts.Remove(mount);
               mount.Rider = null;
            }
         }
         //--------------------------------
         if (true == mi.Name.Contains("ElfWarrior"))
            --this.WitAndWile;
         //--------------------------------
         PartyMembers.Remove(mi);
         //--------------------------------
         IsMerchantWithParty = false; // remove the effects of merchant unless one still exists
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("Merchant"))
               IsMerchantWithParty = true;
         }
         //--------------------------------
         AddSpecialItems(mi.SpecialShares);
         TransferMounts(mi.Mounts);
         if (false == AddFoods(mi.Food))
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveAbandonerInParty(): AddFoods() returned false");
            return;
         }
         if (false == AddCoins(mi.Coin, false))
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveAbandonerInParty(): AddCoins() returned false");
            return;
         }
      }
      public void RemoveAbandonedInParty(IMapItem mi, bool isTrueLoveRemoved = false) // no food/coin/possessons given to Prince
      {
         if ("Prince" == mi.Name) // the prince never runs away
            return;
         //--------------------------------
         int numTrueLoves = 0;
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("TrueLove"))
               ++numTrueLoves;
         }
         if (true == mi.Name.Contains("TrueLove"))
         {
            if (false == isTrueLoveRemoved) // true love may find a way to return if this is false
               LostTrueLoves.Add(mi);
            if (2 == numTrueLoves)       // add the effects of true love - going to one true love
               ++this.WitAndWile;
            else if (1 == numTrueLoves)  // remove effects of true love  - going to no true love
               --this.WitAndWile;
         }
         //--------------------------------
         if ( true == mi.IsFlyingMountCarrier() )
         {
            if (null != mi.Rider)
               mi.Rider.Mounts.Remove(mi);
            mi.Rider = null;
         }
         if (0 < mi.Mounts.Count)
         {
            IMapItem mount = mi.Mounts[0];
            if ( true == mount.IsFlyingMountCarrier() )
            {
               if (null != mi.Rider)
                  mi.Mounts.Remove(mount);
               mount.Rider = null;
            }
         }
         //--------------------------------
         if (true == mi.Name.Contains("ElfWarrior"))
            --this.WitAndWile;
         //--------------------------------
         PartyMembers.Remove(mi);
         //--------------------------------
         IsMerchantWithParty = false; // remove the effects of merchant unless one still exists
         foreach (IMapItem member in PartyMembers)
         {
            if (true == member.Name.Contains("Merchant"))
               IsMerchantWithParty = true;
         }
      }
      //---------------------------------------------------------------
      public List<ITerritory> ReadTerritoriesXml()
      {
         List<ITerritory> territories = new List<ITerritory>();
         XmlTextReader reader = null;
         try
         {
            // Load the reader with the data file and ignore all white space nodes.         
            reader = new XmlTextReader("../../Config/Territories.xml") { WhitespaceHandling = WhitespaceHandling.None };
            while (reader.Read())
            {
               if (reader.Name == "Territory")
               {
                  if (reader.IsStartElement())
                  {
                     string name = reader.GetAttribute("value");
                     Territory t = new Territory(name);
                     reader.Read(); // read the type
                     string typeOfTerritory = reader.GetAttribute("value");
                     t.Type = typeOfTerritory;
                     reader.Read(); // read the sector
                     string coin = reader.GetAttribute("value");
                     t.Coin = Int32.Parse(coin);
                     reader.Read(); // read the center point
                     string value = reader.GetAttribute("X");
                     Double X = Double.Parse(value);
                     value = reader.GetAttribute("Y");
                     Double Y = Double.Parse(value);
                     t.CenterPoint = new MapPoint(X, Y);
                     reader.Read();
                     value = reader.GetAttribute("value");
                     t.IsTown = Boolean.Parse(value);
                     reader.Read();
                     value = reader.GetAttribute("value");
                     t.IsCastle = Boolean.Parse(value);
                     reader.Read();
                     value = reader.GetAttribute("value");
                     t.IsRuin = Boolean.Parse(value);
                     reader.Read();
                     value = reader.GetAttribute("value");
                     t.IsTemple = Boolean.Parse(value);
                     reader.Read();
                     value = reader.GetAttribute("value");
                     t.IsOasis = Boolean.Parse(value);
                     reader.Read();
                     value = reader.GetAttribute("value");
                     t.DownRiver = value;
                     while (reader.Read())
                     {
                        if ((reader.Name == "road" && (reader.IsStartElement())))
                        {
                           value = reader.GetAttribute("value");
                           t.Roads.Add(value);
                        }
                        else if ((reader.Name == "river" && (reader.IsStartElement())))
                        {
                           value = reader.GetAttribute("value");
                           t.Rivers.Add(value);
                        }
                        else if ((reader.Name == "adjacent" && (reader.IsStartElement())))
                        {
                           value = reader.GetAttribute("value");
                           t.Adjacents.Add(value);
                        }
                        else if ((reader.Name == "raft" && (reader.IsStartElement())))
                        {
                           value = reader.GetAttribute("value");
                           t.Rafts.Add(value);
                        }
                        else if ((reader.Name == "regionPoint" && (reader.IsStartElement())))
                        {
                           value = reader.GetAttribute("X");
                           Double X1 = Double.Parse(value);
                           value = reader.GetAttribute("Y");
                           Double Y1 = Double.Parse(value);
                           t.Points.Add(new MapPoint(X1, Y1));
                        }
                        else
                        {
                           break;
                        }
                     }  // end while
                     territories.Add(t);
                  } // end if
               } // end if
            } // end while
            return territories;
         } // try
         catch (Exception e)
         {
            Console.WriteLine("ReadTerritoriesXml(): Exception:  e.Message={0} while reading reader.Name={1}", e.Message, reader.Name);
            return territories;
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      public IMapItems ReadMapItemsXml(List<ITerritory> territories)
      {
         IMapItems mapItems = new MapItems();
         XmlTextReader reader = null;
         try
         {
            // Load the reader with the data file and ignore all white space nodes.         
            reader = new XmlTextReader("../../Config/MapItems.xml") { WhitespaceHandling = WhitespaceHandling.None };
            while (reader.Read())
            {
               if (reader.Name == "MapItem")
               {
                  if (reader.IsStartElement())
                  {
                     string name = reader.GetAttribute("value");
                     //---------------------------------------------------------
                     reader.Read();
                     string zoomStr = reader.GetAttribute("value");
                     Double zoom = Double.Parse(zoomStr);
                     //---------------------------------------------------------
                     reader.Read();
                     string isHiddenStr = reader.GetAttribute("value");
                     bool isHidden = Boolean.Parse(isHiddenStr);
                     //---------------------------------------------------------
                     reader.Read();
                     string isAnimatedStr = reader.GetAttribute("value");
                     bool isAnimated = Boolean.Parse(isAnimatedStr);
                     //---------------------------------------------------------
                     reader.Read();
                     string isGuideStr = reader.GetAttribute("value");
                     bool isGuide = Boolean.Parse(isGuideStr);
                     //---------------------------------------------------------
                     reader.Read();
                     string enduranceStr = reader.GetAttribute("value");
                     Int32 endurance = Int32.Parse(enduranceStr);
                     //---------------------------------------------------------
                     reader.Read();
                     string movementStr = reader.GetAttribute("value");
                     Int32 movement = Int32.Parse(movementStr);
                     //---------------------------------------------------------
                     reader.Read();
                     string combatStr = reader.GetAttribute("value");
                     Int32 combat = Int32.Parse(combatStr);
                     //---------------------------------------------------------
                     reader.Read();
                     string coinStr = reader.GetAttribute("value");
                     int coin = Int32.Parse(coinStr);
                     //---------------------------------------------------------
                     reader.Read();
                     string topImageName = reader.GetAttribute("value");
                     reader.Read();
                     string bottomImageName = reader.GetAttribute("value");
                     reader.Read();
                     string overlapImageName = reader.GetAttribute("value");
                     //---------------------------------------------------------
                     reader.Read();
                     string value = reader.GetAttribute("X");
                     Double X = Double.Parse(value);
                     value = reader.GetAttribute("Y");
                     Double Y = Double.Parse(value);
                     MapPoint location = new MapPoint(X, Y);
                     //---------------------------------------------------------
                     reader.Read();
                     string enduranceUsedStr = reader.GetAttribute("value");
                     Int32 enduranceUsed = Int32.Parse(enduranceUsedStr);
                     reader.Read();
                     string movementUsedStr = reader.GetAttribute("value");
                     Int32 movementUsed = Int32.Parse(movementUsedStr);
                     //---------------------------------------------------------
                     reader.Read();
                     string territoryName = reader.GetAttribute("value");
                     ITerritory matchingTerritory = null;
                     foreach (ITerritory t in territories)
                     {
                        if (territoryName == Utilities.RemoveSpaces(t.ToString()))
                        {
                           matchingTerritory = t;
                           break;
                        }
                     }
                     if (null == matchingTerritory)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadMapItemsXml(): matchingTerritory=null");
                        return null;
                     }
                     //---------------------------------------------------------
                     reader.Read();
                     string territoryStartingName = reader.GetAttribute("value");
                     ITerritory matchingTerritoryStarting = null;
                     foreach (ITerritory t in territories)
                     {
                        if (territoryStartingName == Utilities.RemoveSpaces(t.ToString()))
                        {
                           matchingTerritoryStarting = t;
                           break;
                        }
                     }
                     if (null == matchingTerritoryStarting)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadMapItemsXml(): matchingTerritoryStarting=null");
                        return null;
                     }
                     //---------------------------------------------------------
                     reader.Read();
                     string isKilledStr = reader.GetAttribute("value");
                     bool isKilled = Boolean.Parse(isKilledStr);
                     //---------------------------------------------------------
                     MapItem mi = new MapItem(name, zoom, isHidden, isAnimated, isGuide, topImageName, bottomImageName, matchingTerritory, endurance, combat, coin);
                     mapItems.Add(mi);
                  } // end if
               } // end if
            } // end while
            return mapItems;
         } // try
         catch (Exception e)
         {
            Console.WriteLine("ReadMapItemsXml(): Exception:  e.Message={0} while reading reader.Name={1}", e.Message, reader.Name);
            return null;
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      public IOptions ReadOptionsXml()
      {
         IOptions options = new Options();
         XmlTextReader reader = null;
         try
         {
            // Load the reader with the data file and ignore all white space nodes.         
            reader = new XmlTextReader("../../Config/Options.xml") { WhitespaceHandling = WhitespaceHandling.None };
            while (reader.Read())
            {
               if (reader.Name == "Option")
               {
                  if (reader.IsStartElement())
                  {
                     string name = reader.GetAttribute("value");
                     //---------------------------------------------------------
                     reader.Read();
                     string isEnabledStr = reader.GetAttribute("value");
                     bool isEnabled = Boolean.Parse(isEnabledStr);
                     //---------------------------------------------------------
                     IOption option = new Option(name, isEnabled);
                     options.Add(option);
                  } // end if
               } // end if
            } // end while
            return options;
         } // try
         catch (Exception e)
         {
            Console.WriteLine("ReadOptionsXml(): Exception:  e.Message={0} while reading reader.Name={1}", e.Message, reader.Name);
            return null;
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
   }
}

