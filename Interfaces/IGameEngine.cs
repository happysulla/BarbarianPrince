using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BarbarianPrince
{
   [Serializable]
   public enum PegasusTreasureEnum
   {
      Mount,
      Talisman,
      Reroll
   };
   public enum RiverCrossEnum
   {
      TC_NO_RIVER,                 // no river crossing
      TC_ATTEMPTING_TO_CROSS,      // need to cross river
      TC_CROSS_YES,                // pass checked to cross
      TC_CROSS_YES_SHOWN,          // movement on canvas shown across river
      TC_CROSS_FAIL                // failed to cross river
   };
   public enum RaftEnum
   {
      RE_NO_RAFT,                  // Party does not have raft
      RE_RAFT_SHOWN,               // Rafting option show to user
      RE_RAFT_CHOSEN,              // Rafting option chosen by user
      RE_RAFT_ENCOUNTER,           // pass checked to cross
      RE_RAFT_ENDS_TODAY                 // Finished rafting for today
   };
   public enum SpecialEnum
   {
      None,
      HealingPoition,
      CurePoisonVial,
      GiftOfCharm,
      EnduranceSash,
      ResistanceTalisman,
      PoisonDrug,
      MagicSword,
      AntiPoisonAmulet,
      PegasusMount,
      PegasusMountTalisman,
      CharismaTalisman,
      NerveGasBomb,
      ResistanceRing,
      ResurrectionNecklace,
      ShieldOfLight,
      RoyalHelmOfNorthlands,
      TrollSkin,
      DragonEye,
      RocBeak,
      GriffonClaws,
      MagicBox,
      HydraTeeth,
      StaffOfCommand
   };
   public enum GamePhase
   {
      UnitTest,
      GameSetup,
      SunriseChoice,
      Rest,
      Travel,
      SeekNews,
      SeekHire,
      SeekAudience,
      SeekOffering,
      SearchRuins,
      SearchCache,
      SearchTreasure,
      Hunt,
      Campfire,
      Encounter,
      EndGame,
      Error
   };
   // GameState is a subclass representing the state pattern. For each game state, there can be different
   // game phases and game actions. The GameEngine makes a call that each class can act on..
   // GameEngine.PerformAction() ==> GameState.PerformAction()
   // GameState.PerformAction() ==> GameState.PerformAction()
   [Serializable]
   public enum GameAction
   {

      // Each IUnitTest class may support one of the following actions
      RemoveSplashScreen,
      UpdateStatusBar,
      UpdateEventViewerDisplay,
      UpdateEventViewerActive,
      UpdateGameViewer,
      DieRollActionNone, // The field in IGameInstance indicates what the roll apply. If none expected, it is set to this value.
      TransportRedistributeEnd,

      UnitTestStart,
      UnitTestCommand,
      UnitTestNext,
      UnitTestTest,
      UnitTestCleanup,

      EndGameWin,
      EndGameLost,
      EndGameExit,

      SetupShowCalArath,
      SetupShowStartingWealth,
      SetupRollWitsWiles,
      SetupManualWitsWiles,
      SetupStartingLocation,
      SetupFinalize,

      Hunt,                            // Hunt for Food
      HuntPeasantMobPursuit,           // If hunting in populated area and roll 5
      HuntConstabularyPursuit,         // If hunting in populated area and roll 6
      HuntEndOfDayCheck,               // Check several conditions for possible encounters: HuntE002aEncounterRoll,
      HuntE002aEncounterRoll,          // If north of Tragforth river, check if encounter mercenary guards. If encoutered, move to E002aGuardNumRoll

      CampfirePlagueDust,              // Just before campfire, check if Plague Dust (r227 - trap) causes death
      CampfirePlagueDustEnd,
      CampfireTalismanDestroy,
      CampfireTalismanDestroyEnd,
      CampfireMountDieCheck,       // e096 mounts may die if sick
      CampfireMountDieCheckEnd,
      CampfireFalconCheck,         // e107 falcon may leave if not fed or on dr=6
      CampfireFalconCheckEnd,
      CampfireStarvationCheck,
      CampfireStarvationEnd,
      CampfireLodgingCheck,      // go to sleep at night
      CampfireTrueLoveCheck,     // r228 - True Love attempts to return
      CampfireLoadTransport,     // redistribute the load
      CampfireDisgustCheck,      // check if party members leave
      CampfireWakeup,            // get ready for new day

      RestEncounterCheck,     // Check for encounter in hex when resting - initiate with Daily Action button
      RestHealing,            // Perform Healing
      RestHealingEncounter,   // Perform Healing after encounter

      Travel,
      TravelAir,
      TravelAirRedistribute,
      TravelShortHop,
      TravelLostCheck,
      TravelShowLost,
      TravelShowLostEncounter,
      TravelShowRiverEncounter,
      TravelShowMovement,
      TravelShowMovementEncounter,
      TravelEndMovement,

      SeekNews,
      SeekNewsNoPay,
      SeekNewsWithPay,

      SeekHire,

      SeekAudience,

      SeekOffering,

      SearchRuins,

      SearchCacheCheck,     // Check for encounter in hex when searching - initiate with Daily Action button
      SearchCache,          // Perform Search
      SearchEncounter,      // Perform Search after encounter

      SearchTreasure,
      ArchTravel,

      ShowDienstalBranch,
      ShowLargosRiver,
      ShowNesserRiver,
      ShowTrogothRiver,

      EncounterStart,
      EncounterEnd,
      EncounterEscape,
      EncounterEscapeFly,
      EncounterEscapeMounted,
      EncounterFollow,
      EncounterHide,
      EncounterInquiry,
      EncounterAbandon,        // Abandon party members without mounts
      EncounterCombat,
      EncounterSurrender,
      EncounterBribe,
      EncounterRoll,
      EncounterLootStart,
      EncounterLoot,
      EncounterLootStartEnd,

      E002aGuardNumRoll,
      E002bNegotiateRoll,
      E002cEvadeRoll,
      E002dBattleEnumRoll,
      E006DwarfTalk,
      E006DwarfEvade,
      E006DwarfFight,
      E006DwarfAdvice,
      E007ElfTalk,
      E007ElfEvade,
      E007ElfFight,
      E009FarmDetour,
      E010FoodDeny,
      E010FoodGive,
      E011FarmerPurchaseEnd,
      E012FoodChange,
      E013Lodging,
      E015MountChange,
      E016TalismanSave,
      E018MarkOfCain,
      E018MarkOfCainEnd,
      E023WizardAdvice,
      E024WizardFight,
      E024WizardWander,
      E028CaveTombs,
      E031LootedTomb,
      E032NumberOfGhosts,
      E034CombatSpectre,
      E035IdiotStartDay,
      E035IdiotContinue,
      E039TreasureChest,
      E040TreasureChest,
      E044HighAltar,
      E044HighAltarClue,
      E044HighAltarBlessed,
      E043SmallAltar,
      E043SmallAltarEnd,
      E044HighAltarEnd,
      E045ArchOfTravel,
      E045ArchOfTravelEnd,
      E045ArchOfTravelEndEncounter,
      E045ArchOfTravelSkip,
      E048FugitiveAlly,
      E048FugitiveFight,
      E049MinstrelJoin,
      E049MinstrelStart,
      E053CampsiteFight,
      E054EscapeKeep,
      E060JailOvernight,
      E064HiddenRuins,
      E068WizardTower,
      E069WoundedWarriorCarry,
      E069WoundedWarriorRedistribute,
      E069WoundedWarriorRemain,
      E070HalflingTown,
      E072DoubleElves,
      E072FollowElves,
      E072MeetElves,
      E073WitchCombat,
      E073WitchMeet,
      E073WitchTurnsPrinceIsFrog,
      E075WolvesEncounter,
      E076HuntingCat,
      E077HerdCapture,
      E078BadGoingHalt,
      E078BadGoingRedistribute,
      E079HeavyRains,
      E079HeavyRainsStartDayCheck,
      E079HeavyRainsStartDayCheckInAir,
      E079HeavyRainsDismount,
      E079HeavyRainsContinueTravel,
      E079HeavyRainsRedistribute,
      E080PixieAdvice,
      E082SpectreMagic,
      E082SpectreMagicEnd,
      E083WildBoar,
      E084BearEncounter,
      E085Falling,
      E086HighPass,
      E086HighPassRedistribute,
      E086HighPassRedistributeEnd,
      E087UnpassableWoods,
      E088FallingRocks,
      E089UnpassableMorass,
      E090Quicksand,
      E091PoisonSnake,
      E091PoisonSnakeEnd,
      E092Flood,
      E092FloodContinue,
      E095MountAtRisk,
      E095MountAtRiskEnd,
      E096MountsDie,
      E097FleshRot,
      E097FleshRotEnd,
      E102LowClouds,
      E103BadHeadWinds,
      E104TailWinds,
      E105StormCloudLand,
      E105ViolentWeather,
      E107FalconAdd,
      E107FalconNoFeed,
      E109PegasusCapture,
      E106OvercastLost,
      E106OvercastLostEnd,
      E110AirSpiritConfusedEnd,
      E110AirSpiritTravel,
      E110AirSpiritTravelEnd,
      E111StormDemonEnd,
      E111StormDemonRepel,
      E111StormDemonRepelFail,
      E120Exhausted,
      E121SunStroke,
      E121SunStrokeEnd,
      E122RaftsmenEnd,
      E122RaftsmenCross,
      E122RaftsmenHire,
      E122RaftingEndsForDay,
      E123BlackKnightCombatEnd,
      E123BlackKnightRefuse,
      E123BlackKnightRefuseEnd,
      E123WoundedBlackKnightRemain,
      E126RaftInCurrent,
      E126RaftInCurrentRedistribute,
      E126RaftInCurrentLostRaft,
      E126RaftInCurrentEnd,
      E128aBuyPegasus,
      E128bPotionCureChange,
      E128ePotionHealChange,
      E129EscapeGuards,
      E129aBuyAmulet,
      E130JailedOnTravels,
      E130BribeGuard,
      E130RobGuard,
      E133Plague,
      E133PlaguePrince,
      E133PlagueParty,
      E134ShakyWalls,
      E136FallingCoins,
      E143ChagaDrugPay,
      E143ChagaDrugDeny,
      E143SecretOfTemple,
      E147ClueToTreasure,
      E148SeneschalDeny,
      E148SeneschalPay,
      E152NobleAlly,
      E153MasterOfHouseholdDeny,
      E153MasterOfHouseholdPay,
      E154LordsDaughter,
      E155HighPriestAudience,
      E156MayorAudience,
      E156MayorTerritorySelection,
      E156MayorTerritorySelectionEnd,
      E158HostileGuardPay,
      E160LadyAudience,
      E160GBrokenLove,
      E161CountAudience,
      E163SlavePorterChange,
      E163SlaveGirlChange,
      E163SlaveGirlSelected,
      E163SlaveWarriorChange,
      E182CharmGiftSelected,
      E182CharmGiftRoll,
      E188TalismanPegasusConversion,
      E188TalismanPegasusSkip,
      E203NightInPrison,
      E203EscapeFromPrison,
      E203EscapeFromPrisonEnd,
      E203NightInDungeon,
      E203EscapeFromDungeon,
      E203EscapeFromDungeonEnd,
      E203NightEnslaved,
      E203EscapeEnslaved,
      E209ThievesGuiildNoPay,
      E209ThievesGuiildPay,
      E209ShowSecretRites,
      E210HireFreeman,
      E210HireLancer,
      E210HireMerc1,
      E210HireMerc2,
      E210HireHenchmen,
      E210HireLocalGuide,
      E210HireRunaway,
      E210HirePorter,
      E212Temple,
      E212TempleTenGold,
      E212TempleCurse,
      E212TempleRequestClues,
      E212TempleRequestInfluence,
      E228ShowTrueLove,
      E331DenyFickle,
      E331PayFickle,
      E332DenyGroup,
      E332PayGroup,
      E333DenyHirelings,
      E333PayHirelings,
      E333HirelingCount,
      E334Ally,
      E335Escapee,
      E340DenyLooters,
      E340PayLooters,
      ExitGame,
      Error
   };
   public interface IGameEngine
   {
      List<IView> Views { get; }
      void RegisterForUpdates(IView view);
      void PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll = 0);
      bool CreateUnitTests(IGameInstance gi, DockPanel dp, EventViewer ev, IDieRoller dr);
   }
}
