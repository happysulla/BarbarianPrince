using System;
using System.Collections.Generic;

namespace BarbarianPrince
{
   public interface IGameInstance
   {
      bool CtorError { get; }
      //----------------------------------------------
      IMapItem Prince { set; get; }
      int WitAndWile { set; get; }
      int Days { set; get; }
      //----------------------------------------------
      IOptions Options { get; }
      bool IsGridActive { set; get; } // True if there is some EventViewer manager active
      string EventActive { set; get; }
      string EventDisplayed { set; get; }
      string EventStart { set; get; } // Event ID when encounter starts
      int GameTurn { set; get; }
      bool IsNewDayChoiceMade { set; get; }
      GamePhase GamePhase { set; get; }
      GamePhase SunriseChoice { set; get; } // This is the action chosen by the user for the day
      GameAction DieRollAction { set; get; } // Used in EventViewerPanel when die roll happens to indicate next event for die roll
      Dictionary<string, int[]> DieResults { get; }
      //----------------------------------------------
      List<ITerritory> Territories { get; }
      IStacks Stacks { set; get; }
      ITerritory ActiveHex { set; get; }
      ITerritory NewHex { set; get; } // this is hex moved to if not lost
      List<ITerritory> EnteredTerritories { get; }
      List<ITerritory> DwarfAdviceLocations { get; } // e006
      List<ITerritory> WizardAdviceLocations { get; } // e025
      List<ITerritory> AlcoveOfSendings { get; }  // e042
      List<ITerritory> Arches { get; } // e045
      List<ITerritory> VisitedLocations { get; } // e050
      List<ITerritory> EscapedLocations { get; } // e050
      List<ITerritory> GoblinKeeps { get; }  // e054
      List<ITerritory> OrcTowers { get; }  // e056
      List<ITerritory> DwarvenMines { get; }  // e058
      List<ITerritory> RuinsUnstable { get; }
      List<ITerritory> HiddenRuins { get; } // e064
      List<ITerritory> HiddenTowns { get; } // e065
      List<ITerritory> HiddenTemples { get; } // e066
      List<ITerritory> KilledLocations { get; } // e066
      List<ITerritory> WizardTowers { get; }  // e068b
      List<ITerritory> HalflingTowns { get; }  // e165
      List<ITerritory> EagleLairs { get; } // e115
      List<ITerritory> SecretClues { get; } // e147
      List<ITerritory> LetterOfRecommendations { get; } // e157
      List<ITerritory> Purifications { get; } // e159
      List<ITerritory> ElfTowns { get; }  // e165
      List<ITerritory> ElfCastles { get; }  // e165
      List<ITerritory> FeelAtHomes { get; }   // e209
      List<ITerritory> SecretRites { get; }   // e209
      List<ITerritory> CheapLodgings { get; } // e209
      List<ITerritory> ForbiddenHexes { get; } // e209
      List<ITerritory> AbandonedTemples { get; } // e212
      List<ITerritory> ForbiddenHires { get; }
      //----------------------------------------------
      IMapItems MapItems { get; }
      IMapItems PartyMembers { set; get; }
      IMapItems LostPartyMembers { set; get; }
      IMapItems LostTrueLoves { set; get; }
      IMapItems EncounteredMembers { set; get; }
      IMapItems EncounteredMinstrels { set; get; } // e049
      IMapItems AtRiskMounts { set; get; } // e095 - at risk mounts are killed if decide to travel
      //----------------------------------------------
      IMapItem ActiveMember { set; get; }
      List<int> CapturedWealthCodes { set; get; }
      PegasusTreasureEnum PegasusTreasure { set; get; }
      int FickleCoin { set; get; }
      int LooterCoin { set; get; }
      //----------------------------------------------
      IMapItemMove PreviousMapItemMove { set; get; }
      IMapItemMoves MapItemMoves { set; get; }
      //----------------------------------------------
      List<string> Events { set; get; }
      String EndGameReason { set; get; }
      //----------------------------------------------
      bool IsPartyRested { set; get; }
      bool IsMountsFed { set; get; } // hunt manager
      bool IsMountsStabled { set; get; } // lodge manager
      int Bribe { set; get; }
      //----------------------------------------------
      bool IsGuardEncounteredThisTurn { set; get; } // e002 - Mercenary Guard north of Trogoth
      string DwarvenChoice { set; get; } // e006 - dwarf chooses talk/evade/fight
      bool IsDwarvenBandSizeSet { set; get; } // e006a - size of party 
      bool IsDwarfWarriorJoiningParty { set; get; } // e006 - dwarf warrior gives knowledge of mines
      string ElvenChoice { set; get; } // e007 - elf chooses talk/evade/fight
      bool IsElfWitAndWileActive { set; get; }   // e007 - subtract one from wit and wiles
      bool IsElvenBandSizeSet { set; get; } // e007a - size of party 
      bool IsPartyDisgusted { set; get; } // e010 - Not providing food
      int PurchasedFood { set; get; } // e012 - Amount  of food purchases  
      bool IsFarmerLodging { set; get; } // e013 - Rich farmer provides lodging
      bool IsReaverClanFight { set; get; } // e014b - Reever clan fights after inquiries
      bool IsReaverClanTrade { set; get; } // e015a - Reever clan trades after inquiries
      int PurchasedMount { set; get; } // e015b - Amount  of mount purchases  
      bool IsMagicianProvideGift { set; get; } // e016a - Wizard gift after inquiries
      bool IsHuntedToday { set; get; } // e017, e049, & e050 - Did already hunt once this day
      bool IsMarkOfCain { set; get; } // e018 - killed a priest
      int MonkPleadModifier { set; get; } // e019 - Need a monk to increase modifier
      bool IsWizardJoiningParty { set; get; } // e023 - wizard gives treasure location at evening meal
      bool IsEnslaved { set; get; } // e024 - wizard enslaves
      bool IsSpellBound { set; get; } // e035 - spell of choas has made mindless
      int WanderingDayCount { set; get; } // e035 - days wandering as idiot
      bool IsBlessed { set; get; } // e044 gods have blessed Prince
      int GuardianCount { set; get; }  // e046
      bool IsMerchantWithParty { set; get; }  // e048
      bool IsMinstrelPlaying { set; get; }  //e049 - Minstrel at dinner
      bool IsMinstrelJoining { set; get; }  //e049 - Minstrel conversation to join
      bool IsJailed { set; get; } // e061 - marked for death
      bool IsDungeon { set; get; } // e062 - thrown in dungeon
      int NightsInDungeon { set; get; } // e062 - thrown in dungeon
      bool IsTempleGuardModifer { set; get; } // e066 - subract one on arrested table
      bool IsTempleGuardEncounteredThisHex { set; get; } // e066b - Temple Guard encountered in this hex
      bool IsWoundedWarriorRest { set; get; } // e069 - Rest in hex to heal warrior
      int NumMembersBeingFollowed { set; get; } // e072 - Can continue to follow until get to end
      bool IsTalkActive { set; get; } // e072 - If choose not to follow, then cannot talk on e071
      bool IsWolvesAttack { set; get; } // e074 - Wolves attack at campfire
      bool IsTrainHorse { set; get; } // e077 - Need to train horses the next day
      bool IsBadGoing { set; get; } // e078 - Difficult terrain continues into next day
      bool IsHeavyRain { set; get; } // e079 - Heavy rains requires a check for colds
      bool IsHeavyRainNextDay { set; get; } // e079 - May last into tomorrow
      bool IsHeavyRainContinue { set; get; } // e079 - Rain continues into tomorrow
      bool IsHeavyRainDismount { set; get; } // e079 - Dismounted from mounts due to rain
      bool IsBearAttack { set; get; } // e084 - Bear attack at campfire
      bool IsHighPass { set; get; } // e086 - High Pass may affect travels
      string EventAfterRedistribute { set; get; } // e086 - After performing the high pass event, there may be another event lighted up
      bool IsImpassable { set; get; } // e089 - Must return from direction entered
      bool IsFlood { set; get; } // e092 - Flood 
      bool IsFloodContinue { set; get; } // e092 - Flood
      bool IsPoisonPlant { set; get; } // e093 - Poison Plants prevent hunting at end of day
      bool IsMountsAtRisk { set; get; } // e095 - Set to true if any mounts die
      bool IsMountsSick { set; get; } // e096 - Check for mounts dieing at end of day
      bool IsFalconFed { set; get; } // e107 - Falcon fed
      List<String> AirSpiritLocations { set;  get; }  // e110c - air spirit travel locations
      bool IsEagleHunt { set; get; } // e114 - Eagles help to hunt
      bool IsExhausted { set; get; } // e120 - Party is exhausted
      RaftEnum RaftState { set; get; } // e122 - Party can be rafting for the day
      bool IsRaftDestroyed { set; get; } // e122 - Destroyed on a 12 after use
      bool IsWoundedBlackKnightRest { set; get; } // e123 - Defeated black knight - so may now rest
      int PurchasedPotionCure { set; get; } // e128 - purchased potion cures
      int PurchasedPotionHeal { set; get; } // e128 - purchased potion healing
      int HydraTeethCount { set; get; }   //e141
      int ChagaDrugCount { set; get; }    // e143 Chaga Drug purchased in town - 2gp per serving
      int SeneschalRollModifier { set; get; }   //e148
      bool IsCavalryEscort { set; get; }   // e151
      bool IsNobleAlly { set; get; }   // e152
      IForbiddenAudiences ForbiddenAudiences { get; } // e153
      int DaughterRollModifier { set; get; }   //e154
      int DayOfLastOffering { set; get; } // e155c
      int PriestModifier { set; get; } // e155d
      bool IsPartyFed { set; get; } // e156 & hunt manager
      bool IsPartyLodged { set; get; } // e156
      bool IsPartyContinuouslyLodged { set; get; } // e160d
      bool IsTrueLoveHeartBroken { set; get; } // e160e
      bool IsMustLeaveHex { set; get; } // e161c
      int NumMonsterKill { set; get; } // e161e
      int PurchasedSlavePorter { set; get; } // e163 - purchased porters
      int PurchasedSlaveWarrior { set; get; } // e163 - purchased warrior
      int PurchasedSlaveGirl { set; get; } // e163 - purchased slave girls
      int SlaveGirlIndex { set; get; } // e163 - purchased slave girls
      bool IsSlaveGirlActive { set; get; } // e163 - repeat the evade roll twice more
      bool IsGiftCharmActive { set; get; } // e182 - repeat the evade roll twice more
      bool IsPegasusSkip { set; get; } // e188 - do not convert talisman to pegasus
      bool IsCharismaTalismanActive { set; get; } // e189 - +1 to Wits and Wiles roll for escape
      bool IsAirborne { set; get; }  // r204 - Party may travel by air if all can fly
      bool IsAirborneEnd { set; get; }  // r204 - Party finished traveling for today
      bool IsShortHop { set; get; }  // r204 - Party is flying but using Short Hops
      bool IsSeekNewModifier { set; get; } // e209 - Add one for spending 5gp
      int PurchasedHenchman { set; get; } // e210f - Amount  of henchmen hired  
      int PurchasedPorter { set; get; } // e210i - Amount  of porter hired  
      int PurchasedGuide { set; get; } // e210i - Amount  of local guides hired  
      bool IsOfferingModifier { set; get; } // e212 - Add one for spending 10 gold
      bool IsOmenModifier { set; get; } // e212f - Add for next offering
      bool IsInfluenceModifier { set; get; } // e212l - Add for next offering
      ICaches Caches { get; } // e214 Caches
      bool IsEvadeActive { set; get; } // e318 hiding fails - cannot evade again
      bool IsAssassination { set; get; } // e341 conversation results in assassination attempt
      bool IsDayEnd { set; get; } // e341 conversation kills the day
      //----------------------------------------------
      bool IsSecretTempleKnown { set; get; }  // e143 
      bool IsSecretBaronHuldra { set; get; }  // e144 
      bool IsSecretLadyAeravir { set; get; }  // e145 
      bool IsSecretCountDrogat { set; get; }  // e146 
      bool IsChagaDrugProvided { set; get; }   // e211b - delivered drug to temple
      //----------------------------------------------
      List<IUnitTest> UnitTests { set; get; }
      //----------------------------------------------
      bool IsInTown(ITerritory t);
      bool IsInTemple(ITerritory t);
      bool IsInCastle(ITerritory t);
      bool IsInRuins(ITerritory t);
      bool IsInTownOrCastle(ITerritory t);
      bool IsInStructure(ITerritory t);
      //----------------------------------------------
      int GetFoods();
      bool AddFoods(int foodStore,bool isHunt= false); // if is hunt, all food is added. In Transport Manager, it is winnowed down to what can be carried.
      void ReduceFoods(int foodStore);
      int GetCoins();
      bool AddCoins(int coins, bool isLooterShareIncluded = true);
      void ReduceCoins(int coins);
      bool AddCoinsAuto();
      int GetNonSpecialMountCount(bool isHorseOnly = false);
      bool AddNewMountToParty(MountEnum mount = MountEnum.Horse);
      void ReduceMount(MountEnum mountType);
      void TransferMounts(IMapItems mounts);
      //----------------------------------------------
      bool IsSpecialItemHeld(SpecialEnum item);
      int GetCountSpecialItem(SpecialEnum possession);
      void AddSpecialItem(SpecialEnum possession, IMapItem mi = null);
      void AddSpecialItems(List<SpecialEnum> possessions);
      bool RemoveSpecialItem(SpecialEnum possession, IMapItem mi = null);
      bool IsFedSlaveGirlHeld();
      bool IsPartySizeOne();
      IMapItem RemoveFedSlaveGirl();
      //----------------------------------------------
      bool IsPartyFlying();
      bool PartyReadyToFly();
      bool IsPartyRiding(); // flying or mounted
      bool IsEncounteredFlying();
      bool IsEncounteredRiding(); // flying or mounted
      bool IsSpecialistInParty();
      bool IsMagicInParty(IMapItems mapItems = null);
      bool IsReligionInParty(IMapItems mapItems = null);
      bool IsFalconInParty();
      bool IsMonkInParty();
      bool IsLooterInParty();
      bool IsPixieLoverInParty();
      bool IsHirelingsInParty();
      bool IsMinstrelInParty();
      bool MinstrelStart();
      bool IsInMapItems(string name, IMapItems mapItems = null);
      //----------------------------------------------
      void AddCompanion(IMapItem mi);
      void RemoveKilledInParty(string reason, bool isEscaping = false);
      bool RemoveVictimInParty(IMapItem victim);
      void RemoveAbandonerInParty(IMapItem mi, bool isTrueLoveRemoved = false); // all food/coin/possessons transferred to Party
      void RemoveAbandonedInParty(IMapItem mi, bool isTrueLoveRemoved = false); // no food/coin/possessons given
      bool RemoveBelongingsInParty(bool isMountsRemoved=true);
      int RemoveLeaderlessInParty();
   }
}
