
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BarbarianPrince
{
   [XmlRootAttribute("GameInstance",IsNullable = false)]
   [Serializable]
   public class GameInstance : IGameInstance
   {
      [NonSerialized] static public Logger Logger = new Logger();
      public bool IsTalkRoll { get; set; } = false;
      public Options Options { get; set; } = new Options();
      public GameStat Statistic { get; set; } = new GameStat();
      //------------------------------------------------
      public bool CtorError { get; } = false;
      public GameInstance() // Constructor - set log levels
      {
         if( false == Logger.SetInitial()) // tsetup logger
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): SetInitial() returned false");
            CtorError = true;
            return;
         }
         try
         {
            // Create the territories and the regions marking the territories.
            // Keep a list of Territories used in the game.  All the information 
            // of Territories is static and does not change.
            Territory.theTerritories = ReadTerritoriesXml();
            if (null == Territory.theTerritories)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() returned null");
               CtorError = true;
               return;
            }
         }
         catch (Exception e)
         {
            MessageBox.Show("Exception in GameEngine() e=" + e.ToString());
         }
         //------------------------------------------------------------------------------------
         ITerritory territory = Territory.theTerritories.Find("0101");
         myPrince= new MapItem("Prince", 1.0, false, false, false, "c07Prince", "c07Prince", territory, 9, 8, 0);
         PartyMembers.Add(myPrince);
      }
      public GameInstance(Options newGameOptions) // Constructor - set log levels
      {
         //------------------------------------------------------------------------------------
         ITerritory territory = Territory.theTerritories.Find("0101");
         myPrince= new MapItem("Prince", 1.0, false, false, false, "c07Prince", "c07Prince", territory, 9, 8, 0);
         PartyMembers.Add(myPrince);
         this.Options = newGameOptions;
      }
      //----------------------------------------------
      private IMapItem myPrince = null;
      public IMapItem Prince { set => myPrince = value; get => myPrince; }
      public IMapItem ActiveMember { set; get; } = null;
      public int WitAndWile { get; set; } = 0;
      public int Days { get; set; } = 0;
      //----------------------------------------------
      public bool IsGridActive { set; get; } = false;
      //----------------------------------------------
      public string EventActive { get; set; } = "e000";
      public string EventDisplayed { set; get; } = "e000";
      public string EventStart { set; get; } = "e000";
      //----------------------------------------------
      public int GameTurn { get; set; } = 0;
      public bool IsNewDayChoiceMade { set; get; } = false;
      public bool IsUndoCommandAvailable { set; get; } = false;
      public GamePhase GamePhase { get; set; } = GamePhase.GameSetup;
      public GamePhase SunriseChoice { set; get; } = GamePhase.StartGame;
      public GameAction DieRollAction { get; set; } = GameAction.DieRollActionNone;
      //----------------------------------------------
      public ITerritory TargetHex { set; get; } = null;
      public ITerritory NewHex { set; get; } = null;
      //----------------------------------------------
      public List<int> CapturedWealthCodes { set; get; } = new List<int>();
      public PegasusTreasureEnum PegasusTreasure { set; get; } = PegasusTreasureEnum.Mount;
      public int FickleCoin { set; get; } = 0;
      public int LooterCoin { get; set; } = 0;
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
      public bool IsAlcoveOfSendingAudience { set; get; } = false;
      public bool IsBlessed { set; get; } = false;
      public bool IsArchTravelKnown { set; get; } = false;
      public int GuardianCount { set; get; } = 0;
      public bool IsMerchantWithParty { set; get; } = false;
      public bool IsMinstrelPlaying { set; get; } = false;
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
      public List<String> AirSpiritLocations { get; set; } = null;
      public bool IsEagleHunt { set; get; } = false;
      public bool IsExhausted { set; get; } = false;
      public RaftEnum RaftState { set; get; } = RaftEnum.RE_NO_RAFT; // e122 - Party can be rafting for the day
      public RaftEnum RaftStatePrevUndo { set; get; } = RaftEnum.RE_NO_RAFT;
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
      public bool IsArrestedByDrogat { set; get; } = false;
      public int HydraTeethCount { set; get; } = 0;
      public bool IsHuldraHeirFight { set; get; } = false; // e144b
      public bool IsHuldraHeirKilled { set; get; } = false; // e144e
      public bool IsLadyAeravirRerollActive { set; get; } = false;
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
      public bool IsResurrectedThisTurn { get; set; } = false;
      public bool IsCharismaTalismanActive { set; get; } = false;
      public bool IsSeekNewModifier { set; get; } = false;
      public int PurchasedHenchman { set; get; } = 0;// e210f - Amount  of henchmen hired  
      public int PurchasedPorter { set; get; } = 0; // e210i - Amount  of porter purchases  
      public int PurchasedGuide { set; get; } = 0; // e210i - Amount  of local guides purchases  
      public bool IsMagicUserDismissed { set; get; } = false;
      public bool IsOfferingModifier { set; get; } = false; // e212 - add +1 due to spending 10 gold
      public bool IsOmenModifier { set; get; } = false;  // e212f
      public bool IsInfluenceModifier { set; get; } = false; // e212l
      private ICaches myCaches = new Caches();
      public ICaches Caches { get => myCaches; }
      public bool IsAssassination { set; get; } = false;
      public bool IsDayEnd { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsSecretTempleKnown { set; get; } = false;    // e143 
      public int ChagaDrugCount { set; get; } = 0;              // e143 Chaga Drug purchased in town - 2gp per serving
      public bool IsChagaDrugProvided { set; get; } = false;    // e211b
      public bool IsSecretBaronHuldra { set; get; } = false;    // e144 
      public bool IsSecretLadyAeravir { set; get; } = false;    // e145 
      public bool IsSecretCountDrogat { set; get; } = false;    // e146 
      public bool IsFoulBaneUsedThisTurn { set; get; } = false; // e146 FoulBane purchased in Duffyd Temple - 1gp per serving
      //---------------------------------------------------------------
      public IMapItems PartyMembers { get; set; } = new MapItems();
      public IMapItems LostPartyMembers { get; set; } = new MapItems();
      public IMapItems LostTrueLoves { set; get; } = new MapItems();
      public IMapItems EncounteredMembers { get; set; } = new MapItems();
      public IMapItems EncounteredMinstrels { get; set; } = new MapItems();
      public IMapItems AtRiskMounts { get; set; } = new MapItems();
      public IMapItems ResurrectedMembers { set; get; } = new MapItems();
      //---------------------------------------------------------------
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IMapItemMove PreviousMapItemMove { get; set; } = new MapItemMove();
      private List<EnteredHex> myEnteredHexes = new List<EnteredHex>();
      public List<EnteredHex> EnteredHexes { get => myEnteredHexes; }
      //---------------------------------------------------------------
      private ITerritories myDwarfAdviceLocations = new Territories();
      public ITerritories DwarfAdviceLocations { get => myDwarfAdviceLocations; }
      private ITerritories myWizardAdviceLocations = new Territories();
      public ITerritories WizardAdviceLocations { get => myWizardAdviceLocations; }
      private ITerritories myArches = new Territories();
      public ITerritories Arches { get => myArches; }
      private ITerritories myVisitedLoctions = new Territories();
      public ITerritories VisitedLocations { get => myVisitedLoctions; }
      private ITerritories myEscapedLoctions = new Territories();
      public ITerritories EscapedLocations { get => myEscapedLoctions; }
      private ITerritories myGoblinKeeps = new Territories();
      public ITerritories GoblinKeeps { get => myGoblinKeeps; }
      private ITerritories myDwarvenMines = new Territories();
      public ITerritories DwarvenMines { get => myDwarvenMines; }
      private ITerritories myOrcTowers = new Territories();
      public ITerritories OrcTowers { get => myOrcTowers; }
      private ITerritories myWizardTowers = new Territories();
      public ITerritories WizardTowers { get => myWizardTowers; }
      private ITerritories myPixiedviceLocations = new Territories();
      public ITerritories PixieAdviceLocations { get => myPixiedviceLocations; }
      private ITerritories myHalflingTowns = new Territories();
      public ITerritories HalflingTowns { get => myHalflingTowns; } // e070
      private ITerritories myRuinsUnstable = new Territories();
      public ITerritories RuinsUnstable { get => myRuinsUnstable; }
      private ITerritories myHiddenRuins = new Territories();
      public ITerritories HiddenRuins { get => myHiddenRuins; }
      private ITerritories myHiddenTowns = new Territories();
      public ITerritories HiddenTowns { get => myHiddenTowns; }
      private ITerritories myHiddenTemples = new Territories();
      public ITerritories HiddenTemples { get => myHiddenTemples; }
      private ITerritories myKilledLoctions = new Territories();
      public ITerritories KilledLocations { get => myKilledLoctions; }
      private ITerritories myEagleLairs = new Territories();
      public ITerritories EagleLairs { get => myEagleLairs; } // e115
      private ITerritories mySecretClues = new Territories();
      public ITerritories SecretClues { get => mySecretClues; } // e147
      private ITerritories myLetterOfRecommendations = new Territories();
      public ITerritories LetterOfRecommendations { get => myLetterOfRecommendations; } // e157
      private ITerritories myPurifications = new Territories();
      public ITerritories Purifications { get => myPurifications; } // e159
      private ITerritories myElfTowns = new Territories();
      public ITerritories ElfTowns { get => myElfTowns; } // e165
      private ITerritories myElfCastles = new Territories();
      public ITerritories ElfCastles { get => myElfCastles; } // e166
      private ITerritories myFeelAtHomes = new Territories();
      public ITerritories FeelAtHomes { get => myFeelAtHomes; } // e209
      private ITerritories mySecretRites = new Territories();
      public ITerritories SecretRites { get => mySecretRites; } // e209
      private ITerritories myCheapLodgings = new Territories();
      public ITerritories CheapLodgings { get => myCheapLodgings; } // e209
      private ITerritories myForbiddenHexes = new Territories();
      public ITerritories ForbiddenHexes { get => myForbiddenHexes; } // e209
      private ITerritories myAbandonedTemples = new Territories();
      public ITerritories AbandonedTemples { get => myAbandonedTemples; }
      private ITerritories myForbiddenHires = new Territories();
      public ITerritories ForbiddenHires { get => myForbiddenHires; }
      //---------------------------------------------------------------
      public IStacks Stacks { get; set; } = new Stacks();
      [NonSerialized] private List<IUnitTest> myUnitTests = new List<IUnitTest>();
      public List<IUnitTest> UnitTests { get => myUnitTests; }
      //---------------------------------------------------------------
      private Dictionary<string, int[]> myDieResults = new Dictionary<string, int[]>();
      public Dictionary<string, int[]> DieResults { get => myDieResults; }
      //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
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
         RestoreMapItemAttribute(companion); // if this is transfer of monster to party member, return attributes to proper values 
         companion.Territory = Prince.Territory;
         companion.IsSecretGatewayToDarknessKnown = true;  // e046 
         foreach (string s in Utilities.theNorthOfTragothHexes) // if added south of river, secret of gateway known
         {
            if (s == Prince.Territory.Name)
               companion.IsSecretGatewayToDarknessKnown = false;
         }
         //--------------------------------
         Logger.Log(LogEnum.LE_PARTYMEMBER_ADD, "AddCompanion(): mi=[" + companion.ToString() + "]");
         PartyMembers.Add(companion);
         //--------------------------------
         if (PartyMembers.Count < this.Statistic.myMaxPartySize)
            this.Statistic.myMaxPartySize = PartyMembers.Count;
         int partyEndurance = 0;
         int partyCombat = 0;
         foreach (IMapItem mi in PartyMembers)
         {
            partyEndurance += mi.Endurance;
            partyCombat += mi.Combat;
         }
         if (this.Statistic.myMaxPartyEndurance < partyEndurance)
            this.Statistic.myMaxPartyEndurance = partyEndurance;
         if ( this.Statistic.myMaxPartyCombat < partyCombat)
            this.Statistic.myMaxPartyCombat = partyCombat;
         //--------------------------------
         if (true == companion.Name.Contains("TrueLove"))
         {
            int numTrueLoves = 0; // new true love added to party above step
            foreach (IMapItem member in PartyMembers)
            {
               if (true == member.Name.Contains("TrueLove"))
                  ++numTrueLoves;
            }
            if ((2 == numTrueLoves) && (0 != this.Days)) // remove effects of true love due to the eternal triangle if not first day
            {
               int witandwile = this.WitAndWile;
               --this.WitAndWile;
               Logger.Log(LogEnum.LE_WIT_AND_WILES_INIT, "AddCompanion(): numTrueLoves=2 original=" + witandwile.ToString() + " ww=" + this.WitAndWile.ToString());
            }
            else if (1 == numTrueLoves)  // add effects of true love
            {
               int witandwile = this.WitAndWile;
               ++this.WitAndWile;
               Logger.Log(LogEnum.LE_WIT_AND_WILES_INIT, "AddCompanion(): numTrueLoves=1 original=" + witandwile.ToString() + " ww=" + this.WitAndWile.ToString());
            }
         }
         if( (0 < this.Arches.Count) && (true == IsMagicInParty()) ) // if there is magician, witch, or wizard added to party, adn arches exist, the secret becomes known 
            this.IsArchTravelKnown = true;
         //--------------------------------
         if (true == companion.Name.Contains("Merchant"))
            IsMerchantWithParty = true;
         //--------------------------------
         if (true == companion.Name.Contains("Wizard"))
            IsWizardJoiningParty = true;
         //--------------------------------
         if (true == companion.Name.Contains("DwarfWarrior"))
            IsDwarfWarriorJoiningParty = true;
         //--------------------------------
         if (true == companion.Name.Contains("ElfWarrior"))
         {
            int witandwile = this.WitAndWile;
            ++this.WitAndWile;
            Logger.Log(LogEnum.LE_WIT_AND_WILES_INIT, "AddCompanion(): ElfWarrior original=" + witandwile.ToString() + " ww=" + this.WitAndWile.ToString());
         }
         //--------------------------------
         if ((true == companion.Name.Contains("Minstrel")) && (1 < Days) )
            GameEngine.theFeatsInGame.myIsMinstelAdded = true;
         if ((true == companion.Name.Contains("Eagle")) && (1 < Days))
            GameEngine.theFeatsInGame.myIsEagleAdded = true;
         if ((true == companion.Name.Contains("Falcon")) && (1 < Days))
            GameEngine.theFeatsInGame.myIsFalconAdded = true;
         if ((true == companion.Name.Contains("Merchant")) && (1 < Days))
            GameEngine.theFeatsInGame.myIsMerchantAdded = true;
      }
      private void RestoreMapItemAttribute(IMapItem mi)
      {
         if (true == mi.Name.Contains("TrueLoveSwordwoman"))
         {
            mi.Endurance = 7;
            mi.Combat = 7;
            return;
         }
         if (true == mi.Name.Contains("TrueLovePriestDaughter"))
         {
            mi.Endurance = 4;
            mi.Combat = 2;
            return;
         }
         if (true == mi.Name.Contains("TrueLoveSlave"))
         {
            mi.Endurance = 4;
            mi.Combat = 2;
            return;
         }
         if (true == mi.Name.Contains("Amazon"))
         {
            mi.Endurance = 5;
            mi.Combat = 6;
            return;
         }
         if (true == mi.Name.Contains("Deserter"))
         {
            mi.Endurance = 4;
            mi.Combat = 4;
            return;
         }
         if (true == mi.Name.Contains("DwarfLead"))
         {
            mi.Endurance = 7;
            mi.Combat = 6;
            return;
         }
         if (true == mi.Name.Contains("DwarfW"))
         {
            mi.Endurance = 6;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("Dwarf"))
         {
            mi.Endurance = 6;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("Eagle"))
         {
            mi.Endurance = 3;
            mi.Combat = 4;
            return;
         }
         if (true == mi.Name.Contains("ElfAssistant"))
         {
            mi.Endurance = 3;
            mi.Combat = 3;
            return;
         }
         if (true == mi.Name.Contains("ElfFriend"))
         {
            mi.Endurance = 4;
            mi.Combat = 4;
            return;
         }
         if (true == mi.Name.Contains("ElfWarrior"))
         {
            mi.Endurance = 5;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("Elf"))
         {
            mi.Endurance = 4;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("FarmerBoy"))
         {
            mi.Endurance = 4;
            mi.Combat = 3;
            return;
         }
         if (true == mi.Name.Contains("Freeman"))
         {
            mi.Endurance = 4;
            mi.Combat = 3;
            return;
         }
         if (true == mi.Name.Contains("Giant"))
         {
            mi.Endurance = 8;
            mi.Combat = 9;
            return;
         }
         if (true == mi.Name.Contains("Griffon"))
         {
            mi.Endurance = 6;
            mi.Combat = 7;
            return;
         }
         if (true == mi.Name.Contains("Guide"))
         {
            mi.Endurance = 3;
            mi.Combat = 2;
            return;
         }
         if (true == mi.Name.Contains("HalflingLead"))
         {
            mi.Endurance = 6;
            mi.Combat = 3;
            return;
         }
         if (true == mi.Name.Contains("Harpy"))
         {
            mi.Endurance = 4;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("Henchman"))
         {
            mi.Endurance = 3;
            mi.Combat = 2;
            return;
         }
         if (true == mi.Name.Contains("KnightBlack"))
         {
            mi.Endurance = 8;
            mi.Combat = 8;
            return;
         }
         if (true == mi.Name.Contains("Knight"))
         {
            mi.Endurance = 6;
            mi.Combat = 7;
            return;
         }
         if (true == mi.Name.Contains("Lancer"))
         {
            mi.Endurance = 5;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("MagicianWeak"))
         {
            mi.Endurance = 2;
            mi.Combat = 3;
            return;
         }
         if (true == mi.Name.Contains("Magician"))
         {
            mi.Endurance = 5;
            mi.Combat = 3;
            return;
         }
         if (true == mi.Name.Contains("Lancer"))
         {
            mi.Endurance = 5;
            mi.Combat = 3;
            return;
         }
         if (true == mi.Name.Contains("MercenaryLead"))
         {
            mi.Endurance = 6;
            mi.Combat = 6;
            return;
         }
         if (true == mi.Name.Contains("Mercenary"))
         {
            mi.Endurance = 4;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("Merchant"))
         {
            mi.Endurance = 3;
            mi.Combat = 2;
            return;
         }
         if (true == mi.Name.Contains("MonkWarrior"))
         {
            mi.Endurance = 6;
            mi.Combat = 6;
            return;
         }
         if (true == mi.Name.Contains("MonkTraveling"))
         {
            mi.Endurance = 3;
            mi.Combat = 2;
            return;
         }
         if (true == mi.Name.Contains("MonkHermit"))
         {
            mi.Endurance = 6;
            mi.Combat = 3;
            return;
         }
         if (true == mi.Name.Contains("MonkGuide"))
         {
            mi.Endurance = 3;
            mi.Combat = 2;
            return;
         }
         if (true == mi.Name.Contains("Monk"))
         {
            mi.Endurance = 5;
            mi.Combat = 4;
            return;
         }
         if (true == mi.Name.Contains("Patrol"))
         {
            mi.Endurance = 5;
            mi.Combat = 6;
            return;
         }
         if (true == mi.Name.Contains("Priest"))
         {
            mi.Endurance = 3;
            mi.Combat = 3;
            return;
         }
         if (true == mi.Name.Contains("ReaverBoss"))
         {
            mi.Endurance = 5;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("ReaverLead"))
         {
            mi.Endurance = 4;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("Reaver"))
         {
            mi.Endurance = 4;
            mi.Combat = 4;
            return;
         }
         if (true == mi.Name.Contains("Runaway"))
         {
            mi.Endurance = 4;
            mi.Combat = 4;
            return;
         }
         if (true == mi.Name.Contains("Swordsman"))
         {
            mi.Endurance = 6;
            mi.Combat = 6;
            return;
         }
         if (true == mi.Name.Contains("Swordswoman"))
         {
            mi.Endurance = 7;
            mi.Combat = 7;
            return;
         }
         if (true == mi.Name.Contains("TrustedAssistant"))
         {
            mi.Endurance = 4;
            mi.Combat = 4;
            return;
         }
         if (true == mi.Name.Contains("WarriorBoy"))
         {
            mi.Endurance = 7;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("WarriorOld"))
         {
            return;
         }
         if (true == mi.Name.Contains("Warrior"))
         {
            mi.Endurance = 6;
            mi.Combat = 7;
            return;
         }
         if (true == mi.Name.Contains("Witch"))
         {
            mi.Endurance = 3;
            mi.Combat = 1;
            return;
         }
         if (true == mi.Name.Contains("WizardHenchman"))
         {
            mi.Endurance = 4;
            mi.Combat = 5;
            return;
         }
         if (true == mi.Name.Contains("Wizard"))
         {
            mi.Endurance = 4;
            mi.Combat = 4;
            return;
         }
      }
      public int GetFoods()
      {
         int foods = 0;
         foreach (IMapItem mi in PartyMembers)
            foods += mi.Food;
         return foods;
      }
      public bool AddFoods(int foodStore, bool isHunt=false)
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
            int totalFreeLoad = 0;
            foreach (IMapItem mi in sortedMapItems)
            {
               if ((true == mi.IsUnconscious) || (true == mi.IsKilled) || (true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Falcon")))
                  continue;
               int freeLoad = 0;
               if (false == isHunt)
                  freeLoad = mi.GetFreeLoad();
               else
                  freeLoad = 1000; // keep adding without regard to limit when there is a hunt - it is fixed after the hunt is over
               if (freeLoad < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "AddFoods(): GetFreeLoad() returned fl=" + freeLoad.ToString());
                  return false;
               }
               totalFreeLoad += freeLoad;
               if (0 < freeLoad)
               {
                  ++mi.Food;
                  --foodStore;
                  Logger.Log(LogEnum.LE_ADD_FOOD, "AddFoods(): HUNT=" + isHunt.ToString() + " mi=" + mi.Name + " ++++>>> f=" + mi.Food.ToString() + " foodStore=" + foodStore.ToString() + " fl=" + freeLoad.ToString());
                  if (0 == foodStore)
                     return true;
               }
            }
            if (0 == totalFreeLoad)
               return true;
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
      public bool AddCoins(string caller, int coins, bool isCoinsShared = true)
      {
         if (0 == coins)
            return true;
         Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): " + caller + ":++++++++++++++++++" + coins.ToString() + "++++++++++++++++++++++++++++++++++++++++++++");
         StringBuilder sb = new StringBuilder("AddCoins():");
         foreach ( IMapItem mi in PartyMembers)
         {
            sb.Append("\n     mi.Name=");
            sb.Append(mi.Name);
            sb.Append(" c=");
            sb.Append(mi.Coin.ToString());
            sb.Append(" f=");
            sb.Append(mi.Food.ToString());
         }
         Logger.Log(LogEnum.LE_ADD_COIN, sb.ToString());
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
         Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): ls=" + looterShare.ToString() + " rc=" + remainingCoins.ToString());
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
         int afterLooterCoins = remainingCoins;
         remainingCoins = (int)Math.Ceiling((decimal)afterLooterCoins / (decimal)fickleShare); // fickle get equal share as Prince
         this.FickleCoin += (afterLooterCoins - remainingCoins);
         Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): fs=" + fickleShare.ToString()+ " rc=" + remainingCoins.ToString());
         //---------------------------------
         IMapItems sortedMapItems = PartyMembers.SortOnCoin();
         sortedMapItems.Reverse();
         foreach (IMapItem mi in sortedMapItems) // add to party members to get to 100 increment - starting with most poor
         {
            int miRemainder = mi.Coin % 100;
            if ( (0 != miRemainder) && (false == mi.IsUnconscious) && (false == mi.IsKilled) && (false == mi.Name.Contains("Eagle")) && (false == mi.Name.Contains("Falcon")))
            {
               if (0 == remainingCoins)
                  return true;
               int diffToGetTo100 = 100 - miRemainder;
               if (remainingCoins <= diffToGetTo100)
               {
                  int miCoin = mi.Coin;
                  mi.Coin += remainingCoins;
                  Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): returning after add to poorest " + mi.Name + "++++>>>" + miCoin.ToString() + " + " + remainingCoins.ToString() + " = " + mi.Coin.ToString());
                  return true;
               }
               remainingCoins -= diffToGetTo100;  // remainingCoins reduced by how much added to this MapItem
               int miCoin1 = mi.Coin;
               mi.Coin += diffToGetTo100;
               Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): add to poorest first to reach 100 " + mi.Name + "++++>>>" + miCoin1.ToString() + " + " + diffToGetTo100.ToString() + " = " + mi.Coin.ToString() + " rc=" + remainingCoins.ToString());

            }
         }
         //--------------------------------- 
         int remainder = remainingCoins % 100; // First try to get remainder removed
         int hundreds = (int)((remainingCoins - remainder)/100.0);
         foreach (IMapItem mi in sortedMapItems) // take care of remainder first
         {
            if ((true == mi.IsUnconscious) || (true == mi.IsKilled) || (true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Falcon")))
               continue;
            int freeLoad = mi.GetFreeLoadWithoutModify(); // AddCoins() trying to add remainder of coins by subtracting a food
            if (0 < freeLoad)
            {
               remainingCoins -= remainder;
               int miCoin = mi.Coin;
               mi.Coin += remainder;
               Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): 3 remainder=" + remainder.ToString() + "-->" + mi.Name + "++++>>>" + miCoin.ToString() + " + " + remainder.ToString() + " = " + mi.Coin.ToString() + " rc=" + remainingCoins.ToString());
               remainder = 0;
            }
            else if( 0 < mi.Food )
            {
               mi.Food -= 1; // remove one food to add one load of coins
               remainingCoins -= remainder;
               int miCoin = mi.Coin;
               mi.Coin += remainder;
               Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): 4 remainder=" + remainder.ToString() + "-->" + mi.Name + "++++>>>" + miCoin.ToString() + " + " + remainder.ToString() + " = " + mi.Coin.ToString() + " remove one food rc=" + remainingCoins.ToString());
               remainder = 0;
            }
         }
         if ((0 == hundreds) || (0 == remainingCoins) ) // at this point, only left with 100s
            return true;
         //--------------------------------- 
         int princeFreeLoad = Prince.GetFreeLoadWithoutModify(); // AddCoins() - Add to prince if prince free load over zero
         if ((0 < princeFreeLoad) && (false == Prince.IsUnconscious) && (false == Prince.IsKilled))
         {
            int c100 = (hundreds * 100);
            if (hundreds <= princeFreeLoad)
            {
               int miPrinceCoin = Prince.Coin;
               Prince.Coin += c100;
               Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): return after 100s--> Prince++++>>>" + miPrinceCoin.ToString() + " + " + c100.ToString() + " = " + Prince.Coin.ToString() + "rc=" + remainingCoins.ToString());
               return true;
            }
            int diff = hundreds - princeFreeLoad; // prince Free load greater than new coin load
            if (diff <= Prince.Food)
            {
               int miPrinceCoin = Prince.Coin;
               Prince.Coin += c100;
               Prince.Food -= diff;
               Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): return after 100s--> Prince++++>>>" + miPrinceCoin.ToString() + " + " + c100.ToString() + " = " + Prince.Coin.ToString() + "rc=" + remainingCoins.ToString() + " minus food=" + diff.ToString());
               return true;
            }
            c100 = (diff * 100);
            Prince.Food = 0;
            int miPrinceCoin1 = Prince.Coin;
            Prince.Coin += c100; // prince removes all food and gets a portion of the remaining coins
            Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): 7 100s--> Prince++++>>>" + miPrinceCoin1.ToString() + " + " + c100.ToString() + " = " + Prince.Coin.ToString() + " minus all food");
            hundreds -= diff;
         }
         //--------------------------------- 
         foreach (IMapItem mi in sortedMapItems) 
         {
            int freeLoad = mi.GetFreeLoadWithoutModify(); // AddCoins() -  Add to others party members if free load over zero
            if ((0 < freeLoad) && (false == mi.IsUnconscious) && (false == mi.IsKilled) && (false == mi.Name.Contains("Eagle")) && (false == mi.Name.Contains("Falcon")))
            {
               int c100 = (hundreds * 100);
               if (hundreds <= freeLoad)
               {
                  int miCoin = mi.Coin;
                  mi.Coin += c100;
                  Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): 8 100s--> mi=" + mi.Name + "++++>>>" + mi.Coin.ToString() + " + " + c100.ToString() + " = " + mi.Coin.ToString());
                  return true;
               }
               int diff = hundreds - freeLoad;
               if (diff <= mi.Food)
               {
                  int miCoin = mi.Coin;
                  mi.Coin += c100;
                  mi.Food -= diff;
                  Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): 9 100s--> mi=" + mi.Name + "++++>>>" + miCoin.ToString() + " + " + c100.ToString() + " = " + mi.Coin.ToString() + " minus food=1");
                  return true;
               }
               c100 = (diff * 100);
               mi.Food = 0;
               int miCoin1 = mi.Coin;
               mi.Coin += c100; // mi removes all food and gets a portion of the remaining coins
               Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): 10 100s--> mi=" + mi.Name + "++++>>>" + miCoin1.ToString() + " + " + c100.ToString() + " = " + mi.Coin.ToString() + " minus all food");
               hundreds -= diff;
            }
         }
         //--------------------------------- 
         foreach (IMapItem mi in sortedMapItems)
         {
            if ((0 < hundreds) && (false == mi.IsUnconscious) && (false == mi.IsKilled) && (false == mi.Name.Contains("Eagle")) && (false == mi.Name.Contains("Falcon")))
            {
               int c100 = (hundreds * 100);
               int diff = mi.Food - hundreds;
               if (hundreds < diff)
               {
                  Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): 11 100s--> mi=" + mi.Name + "++++>>>" + mi.Coin.ToString() + " + " + c100.ToString() + " minus food=" + diff.ToString());
                  mi.Coin += (hundreds * 100);
                  mi.Food -= hundreds;
                  return true;
               }
               else
               {
                  c100 = (mi.Food * 100);
                  Logger.Log(LogEnum.LE_ADD_COIN, "AddCoins(): 12 100s--> mi=" + mi.Name + "++++>>>" + mi.Coin.ToString() + " + " + c100.ToString() + " minus all food=" + mi.Food.ToString());
                  mi.Food = 0;
                  mi.Coin += c100; // mi removes all food and gets a portion of the remaining coins
                  hundreds -= mi.Food;
               }
            }
         }
         return true;
      }
      public bool AddCoinsAuto()
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
                  Logger.Log(LogEnum.LE_ERROR, "AddCoinsAuto(): GetCoin()=" + coin.ToString() + " wc=" + wc.ToString());
                  return false;
               }
               capturedCoins += coin;
               Logger.Log(LogEnum.LE_ADD_COIN_AUTO, "AddCoinsAuto(): coin=" + coin.ToString() +  " capturedCoins=" + capturedCoins.ToString());
            }
         }
         if (false == AddCoins("AddCoinsAuto", capturedCoins))
         {
            Logger.Log(LogEnum.LE_ERROR, "AddCoinsAuto(): AddCoins()=" + capturedCoins.ToString());
            return false;
         }
         CapturedWealthCodes = wealthCodes;
         return true;
      }
      public void ReduceCoins(string caller, int coins)
      {
         if (coins < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): invalid parameter coins=" + coins.ToString());
            coins = 0;
         }
         if (0 == coins)
            return;
         Logger.Log(LogEnum.LE_REDUCE_COIN, "ReduceCoins(): " + caller + " ------------------" + coins.ToString() + "--------------------------------------------");
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
                  int miCoin0 = mi.Coin;
                  mi.Coin -= coins;
                  Logger.Log(LogEnum.LE_REDUCE_COIN, "ReduceCoins(): return reduce from rich " + mi.Name + " ++++>>> " + miCoin0.ToString() + " - " + coins.ToString() + " = " + mi.Coin.ToString());
                  if (mi.Coin < 0)
                     Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): e - invalid state (mi.Coin=" + mi.Coin.ToString() + ")<0 coins=" + coins.ToString());
                  StringBuilder sb = new StringBuilder("ReduceCoins():");
                  foreach (IMapItem mi11 in PartyMembers)
                  {
                     sb.Append("\n     mi.Name=");
                     sb.Append(mi11.Name);
                     sb.Append(" c=");
                     sb.Append(mi11.Coin.ToString());
                     sb.Append(" f=");
                     sb.Append(mi11.Food.ToString());
                  }
                  Logger.Log(LogEnum.LE_REDUCE_COIN, sb.ToString());
                  return;
               }
               coins -= remainder;
               int miCoin = mi.Coin;
               mi.Coin -= remainder;
               Logger.Log(LogEnum.LE_REDUCE_COIN, "ReduceCoins(): reduce from rich " + mi.Name + " ++++>>> " + miCoin.ToString() + " - " + remainder.ToString() + " = " + mi.Coin.ToString() + " coins=" + coins.ToString());
               if (mi.Coin < 0)
                  Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): f - invalid state (mi.Coin=" + mi.Coin.ToString() + ")<0 coins=" + remainder.ToString());
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
                     int miCoin = mi.Coin;
                     mi.Coin -= coins;
                     Logger.Log(LogEnum.LE_REDUCE_COIN, "ReduceCoins(): reduce from rich " + mi.Name + " ---->>> " + miCoin.ToString() + " - " + coins.ToString() + " = " + mi.Coin.ToString());
                     if (mi.Coin < 0)
                        Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): a - invalid state (mi.Coin=" + mi.Coin.ToString() + ")<0 coins=" + coins.ToString());
                     StringBuilder sb = new StringBuilder("ReduceCoins():");
                     foreach (IMapItem mi11 in PartyMembers)
                     {
                        sb.Append("\n     mi.Name=");
                        sb.Append(mi11.Name);
                        sb.Append(" c=");
                        sb.Append(mi11.Coin.ToString());
                        sb.Append(" f=");
                        sb.Append(mi11.Food.ToString());
                     }
                     Logger.Log(LogEnum.LE_REDUCE_COIN, sb.ToString());
                     return;
                  }
                  int miCoin3 = mi.Coin;
                  mi.Coin -= remainder2;
                  Logger.Log(LogEnum.LE_REDUCE_COIN, "ReduceCoins(): reduce from rich " + mi.Name + " ---->>> " + miCoin3.ToString() + " - " + remainder2.ToString() + " = " + mi.Coin.ToString() + " coins=" + coins.ToString());
                  if (mi.Coin < 0)
                     Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): b - invalid state (mi.Coin=" + mi.Coin.ToString() + ")<0 coins=" + remainder2.ToString());
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
               int miCoinPrince = myPrince.Coin;
               myPrince.Coin -= coins;
               Logger.Log(LogEnum.LE_REDUCE_COIN, "ReduceCoins(): reduce mi=Prince ***>>> " + miCoinPrince.ToString() + " - " + coins.ToString() + " = " + myPrince.Coin.ToString() );
               if (myPrince.Coin < 0)
                  Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): c - invalid state (myPrince.Coin=" + myPrince.Coin.ToString() + ")<0 coins=" + coins.ToString());
               StringBuilder sb = new StringBuilder("ReduceCoins():");
               foreach (IMapItem mi11 in PartyMembers)
               {
                  sb.Append("\n     mi.Name=");
                  sb.Append(mi11.Name);
                  sb.Append(" c=");
                  sb.Append(mi11.Coin.ToString());
                  sb.Append(" f=");
                  sb.Append(mi11.Food.ToString());
               }
               Logger.Log(LogEnum.LE_REDUCE_COIN, sb.ToString());
               return;
            }
            int miCoinPrince1 = myPrince.Coin;
            myPrince.Coin -= remainder1;
            Logger.Log(LogEnum.LE_ADD_COIN, "ReduceCoins(): reduce mi=Prince ***>>> " + miCoinPrince1.ToString() + " - " + remainder1.ToString() + " = " + myPrince.Coin.ToString() + " coins=" + coins.ToString());
            if (myPrince.Coin < 0)
               Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): d - invalid state (myPrince.Coin=" + myPrince.Coin.ToString() + ")<0 coins=" + coins.ToString());
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
                  int miCoinL = mi.Coin;
                  --mi.Coin;
                  Logger.Log(LogEnum.LE_REDUCE_COIN, "ReduceCoins(): mi=" + mi.Name + ">>>>>> " + miCoinL.ToString() + " - 1 = " + myPrince.Coin.ToString() + " coins=" + coins.ToString());
                  --coins;
               }
               if (coins <= 0 )
               {
                  if (count < 0)
                     Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): 1 - invalid state count<0 coins=" + coins.ToString());
                  StringBuilder sb = new StringBuilder("ReduceCoins():");
                  foreach (IMapItem mi11 in PartyMembers)
                  {
                     sb.Append("\n     mi.Name=");
                     sb.Append(mi11.Name);
                     sb.Append(" c=");
                     sb.Append(mi11.Coin.ToString());
                     sb.Append(" f=");
                     sb.Append(mi11.Food.ToString());
                  }
                  Logger.Log(LogEnum.LE_REDUCE_COIN, sb.ToString());
                  return;
               }
            }
         }
         if (count < 0)
            Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): 2 - invalid state count<0 coins=" + coins.ToString());
         if (0 != coins)
            Logger.Log(LogEnum.LE_ERROR, "ReduceCoins(): not reduced to zero coins=" + coins.ToString());
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
            Logger.Log(LogEnum.LE_MOUNT_CHANGE, "TransferMounts(): Nobody conscious to receive mount. Lose mount as it wonders off to feed.");
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
         if ((false == myPrince.IsSpecialItemHeld(possession)) && (false == myPrince.IsUnconscious) && (false == myPrince.IsKilled)) // give to prince if he does not own this special item
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
            if ( (true == member.Name.Contains("Porter")) || (true == member.Name.Contains("Slave")) || (true == member.Name.Contains("Minstrel")) || (true == member.Name.Contains("Eagle"))  )
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
         {
            if( SpecialEnum.ResurrectionNecklace != possession) // Special Items do not transfer
               AddSpecialItem(possession);
         }
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
      //---------------------------------------------------------------
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
      public bool IsPartyFlying()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if (false == mi.IsFlying)
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
            Logger.Log(LogEnum.LE_ERROR, "IsEncounteredRiding() Invalid State b/c  EncounteredMembers is empty for ae=" + EventActive + " es=" + EventStart);
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
      public bool IsDuplicateMount()
      {
         IMapItems mounts = new MapItems();
         foreach( IMapItem mi in this.PartyMembers)
         {
            foreach(IMapItem mount in mi.Mounts)
               mounts.Add(mount);
         }
         IMapItems duplicates = new MapItems(mounts);
         foreach (IMapItem mount in mounts)
         {
            duplicates.Remove(mount);
            if (null != duplicates.Find(mount.Name))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsDuplicateMapItem(): duplicate=" + mount.Name + " in mounts=" + mounts.ToString());
               foreach(IMapItem partyMember in PartyMembers)
               {
                  foreach(IMapItem mount1 in partyMember.Mounts)
                  {
                     if( mount.Name == mount1.Name )
                     {
                        partyMember.Mounts.Remove(mount1); // remove the duplicate mount
                        return true;
                     }
                  }
               }
            }
         }
         return false;
      }
      //---------------------------------------------------------------
      public bool PartyReadyToFly()
      {
         foreach (IMapItem mi in PartyMembers)
         {
            if (mi.GetFlyLoad() < 0)
               return false;
         }
         return true;
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
      public void ProcessIncapacitedPartyMembers(string reason, bool isEscaping = false)
      {
         if (true == Prince.IsKilled) // If prince killed, no need to look at other members
         {
            return;
         }
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
         foreach (IMapItem member in PartyMembers) // do initial processing before removal
         {
            if (true == member.Name.Contains("TrueLove"))
               ++numTrueLovesBefore;
            if (true == member.IsKilled)
            {
               isMemberKilled = true;
               if (true == member.Name.Contains("ElfWarrior"))
                  --this.WitAndWile;
               if (true == member.Name.Contains("WarriorBoy"))
                  IsHuldraHeirKilled = true;
               if (true == member.Name.Contains("Minstel"))
                  IsMinstrelPlaying = false;
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
         if (true == isMemberKilled) // if anybody killed, all fickle members disappear
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
         IMapItems members = new MapItems(); // make a copy of MapItems 
         foreach (IMapItem mi in PartyMembers)
            members.Add(mi);
         IMapItems killedMembers0 = new MapItems();
         foreach (IMapItem mi in members)
         {
            if (true == mi.Name.Contains("Undead")) // remove any undead warriors that were created by Hydra Teeth - See e140
               killedMembers0.Add(mi);
            if (true == mi.IsKilled)
            {
               Logger.Log(LogEnum.LE_REMOVE_KIA, "ProcessIncapacitedPartyMembers(): ================"+mi.Name+"=KIA c=" + mi.Coin.ToString() + " f=" + mi.Food.ToString() + "=========================");
               isMemberKilled = true;
               killedMembers0.Add(mi);
               if( true == mi.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace))
               {
                  mi.RemoveSpecialItem(SpecialEnum.ResurrectionNecklace);
                  this.ResurrectedMembers.Add(mi);
               }
               if (false == isEscaping)
               {
                  TransferMounts(mi.Mounts);
                  AddSpecialItems(mi.SpecialKeeps);  // ProcessIncapacitedPartyMembers() - IsKilled
                  AddSpecialItems(mi.SpecialShares); // ProcessIncapacitedPartyMembers() - IsKilled
                  if (false == AddFoods(mi.Food))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ProcessIncapacitedPartyMembers(): AddFoods() returned false");
                     return;
                  }
                  int coin = GameEngine.theTreasureMgr.GetCoin(mi.WealthCode); // add the wealth code of character to character's coin pile
                  if( coin < -1 )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ProcessIncapacitedPartyMembers(): AddCoins() returned false");
                     return;
                  }
                  Logger.Log(LogEnum.LE_ADD_COIN, "ProcessIncapacitedPartyMembers(): Wealth Code Dead Companion=" + coin.ToString());
                  mi.Coin += coin;
                  if ( false == AddCoins("ProcessIncapacitedPartyMembers-0", mi.Coin, false))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ProcessIncapacitedPartyMembers(): AddCoins() returned false");
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
               if ((0 == mi.Mounts.Count) && (0 == mi.Food) && (0 == mi.Food) && (0 == mi.SpecialShares.Count)) // nothing to give away - nothing to process
                  continue;
               Logger.Log(LogEnum.LE_PROCESS_MIA, "ProcessIncapacitedPartyMembers(): ----------------" + mi.Name + "=MIA c=" + mi.Coin.ToString() + " f=" + mi.Food.ToString() + "-------------------------");
               if (false == isEscaping)
               {
                  TransferMounts(mi.Mounts);
                  AddSpecialItems(mi.SpecialShares); // ProcessIncapacitedPartyMembers() - IsUnconscious
                  if (false == AddFoods(mi.Food))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ProcessIncapacitedPartyMembers(): AddFoods() returned false");
                     return;
                  }
                  if (false == AddCoins("ProcessIncapacitedPartyMembers-1",mi.Coin, false))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ProcessIncapacitedPartyMembers(): AddCoins() returned false");
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
         //---------------------------------------------
         if (true == victim.Name.Contains("WarriorBoy"))
            IsHuldraHeirKilled = true;
         //--------------------------------
         if (true == victim.Name.Contains("Minstel"))
            IsMinstrelPlaying = false;
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
            Logger.Log(LogEnum.LE_ERROR, "RemoveVictimInParty(): RemoveVictimMountAndLoad() returned false");
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
         if (false == AddCoins("RemoveVictimInParty", victim.Coin, false))
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
         //--------------------------------
         if (true == victim.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace))
         {
            victim.RemoveSpecialItem(SpecialEnum.ResurrectionNecklace);
            this.ResurrectedMembers.Add(victim);
         }
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
            mi.CarriedMembers.Clear();
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
         this.RaftStatePrevUndo = this.RaftState = RaftEnum.RE_NO_RAFT;
         this.HydraTeethCount = 0;
         this.ChagaDrugCount = 0;
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
         IsMinstrelPlaying = false;
         //--------------------------------
         int count = partyMembers.Count;
         foreach (IMapItem mi in partyMembers)
         {
            if (true == mi.Name.Contains("ElfWarrior"))
               --this.WitAndWile;
            if (true == mi.Name.Contains("WarriorBoy"))
               IsHuldraHeirKilled = true;
            if (true == mi.Name.Contains("TrueLove"))
               LostTrueLoves.Add(mi);
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
         if (true == mi.Name.Contains("WarriorBoy"))
            IsHuldraHeirKilled = true;
         //--------------------------------
         if (true == mi.Name.Contains("Minstel"))
            IsMinstrelPlaying = false;
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
         AddSpecialItems(mi.SpecialShares); // RemoveAbandonerInParty()
         TransferMounts(mi.Mounts);
         if (false == AddFoods(mi.Food))
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveAbandonerInParty(): AddFoods() returned false");
            return;
         }
         if (false == AddCoins("RemoveAbandonerInParty", mi.Coin, false))
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
         if (true == mi.Name.Contains("WarriorBoy"))
            IsHuldraHeirKilled = true;
         //--------------------------------
         if (true == mi.Name.Contains("Minstel"))
            IsMinstrelPlaying = false;
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
      public ITerritories ReadTerritoriesXml()
      {
         ITerritories territories = new Territories();
         XmlTextReader reader = null;
         try
         {
            // Load the reader with the data file and ignore all white space nodes.
            string filename = ConfigFileReader.theConfigDirectory + "Territories.xml";
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
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
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("[");
         sb.Append("t=");
         sb.Append(this.GameTurn.ToString());
         sb.Append(",p=");
         sb.Append(this.GamePhase.ToString());
         sb.Append(",c=");
         sb.Append(this.SunriseChoice.ToString());
         sb.Append(",st=");
         sb.Append(this.Prince.TerritoryStarting.Name);
         sb.Append(",t=");
         sb.Append(this.Prince.Territory.Name);
         sb.Append("]");
         return sb.ToString();
      }
   }
}

