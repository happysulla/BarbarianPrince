using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace BarbarianPrince
{
   public abstract class GameState : IGameState
   {
      static protected int theCombatModifer = 0;
      static protected int theConstableRollModifier = 0;
      static protected int theTempleRollModifier = 0;
      abstract public string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll); // abstract function...GameEngine calls PerformAction() 
      static public IGameState GetGameState(GamePhase phase) // static method that returns the next GameState object based on GamePhase
      {
         switch (phase)
         {
            case GamePhase.UnitTest: return new GameStateUnitTest();
            case GamePhase.GameSetup: return new GameStateSetup();
            case GamePhase.SunriseChoice: return new GameStateSunriseChoice();
            case GamePhase.Rest: return new GameStateRest();
            case GamePhase.Travel: return new GameStateTravel();
            case GamePhase.SeekNews: return new GameStateSeekNews();
            case GamePhase.SeekHire: return new GameStateSeekHire();
            case GamePhase.SeekOffering: return new GameStateSeekOffering();
            case GamePhase.SearchRuins: return new GameStateSearchRuins();
            case GamePhase.SearchCache: return new GameStateSearch();
            case GamePhase.SearchTreasure: return new GameStateSearch();
            case GamePhase.Encounter: return new GameStateEncounter();
            case GamePhase.Hunt: return new GameStateHunt();
            case GamePhase.Campfire: return new GameStateCampfire();
            case GamePhase.EndGame: return new GameStateEnded();
            default: Logger.Log(LogEnum.LE_ERROR, "GetGameState(): reached default p=" + phase.ToString()); return null;
         }
      }
      protected ITerritory GetPreviousHex(IGameInstance gi)
      {
         for (int i = gi.EnteredHexes.Count - 1; -1 < i; --i)
         {
            if (gi.Prince.Territory.Name != gi.EnteredHexes[i].HexName)
            {
               ITerritory t = Territory.theTerritories.Find(gi.EnteredHexes[i].HexName);
               if (null == t)
                  Logger.Log(LogEnum.LE_ERROR, "GetPreviousHex(): theTerritories.Find() returned null for n=" + gi.EnteredHexes[i].HexName);
               return t;
            }
         }
         return null;
      }
      protected void TravelAction(IGameInstance gi, ref GameAction action)
      {
         action = GameAction.Travel;
         gi.NumMembersBeingFollowed = 0;
         gi.IsFloodContinue = false; // e092
         gi.SunriseChoice = gi.GamePhase = GamePhase.Travel;
         //----------------------------------------------
         if (RaftEnum.RE_RAFT_CHOSEN == gi.RaftState) // Set Movement value that is allowed this turn
            gi.Prince.Movement = 1;
         else if (true == gi.IsShortHop)
            gi.Prince.Movement = 2;
         else if (true == gi.IsAirborne)
            gi.Prince.Movement = 3;
         else if (true == gi.IsPartyRiding())
            gi.Prince.Movement = 2;
         else
            gi.Prince.Movement = 1;
         //----------------------------------------------
         gi.RaftStatePrevUndo = gi.RaftState;
         if (RaftEnum.RE_RAFT_CHOSEN == gi.RaftState)
            gi.ReduceCoins("TravelAction(pay rafsman)", 1); // pay one to raftsmen
         else                  // e122 - if travel some other way other than raft
            gi.RaftState = RaftEnum.RE_NO_RAFT; // TravelAction() with no money
                                                //----------------------------------------------
         if (0 < gi.AtRiskMounts.Count)  // e095 - if traveling -- at risk mounts die. Need to redistribute load
         {
            gi.IsMountsAtRisk = true; // this gets set to false in the EventViewerTransportMgr->UpdateEndState() - allows for display proper exit icon in the user assignment panel
            foreach (IMapItem deadMount in gi.AtRiskMounts)
            {
               IMapItem owner = null;
               foreach (IMapItem partyMember in gi.PartyMembers)
               {
                  foreach (IMapItem mount in partyMember.Mounts)
                  {
                     if (mount.Name == deadMount.Name)
                     {
                        owner = partyMember;
                        break;
                     }
                  }
               }
               if (null != owner)
               {
                  owner.Mounts.Remove(deadMount);
                  break;
               }
            }
            gi.AtRiskMounts.Clear();
            gi.GamePhase = GamePhase.Encounter;
            action = GameAction.UpdateEventViewerActive;
            gi.EventDisplayed = gi.EventActive = "e095b";
         }
         else if (true == gi.IsBadGoing)
         {
            gi.IsBadGoing = false;                 // e078
            gi.GamePhase = GamePhase.Encounter;
            gi.EventDisplayed = gi.EventActive = "e078";
            gi.DieRollAction = GameAction.EncounterStart;
            action = GameAction.UpdateEventViewerActive;
         }
         else
         {
            gi.IsGridActive = true; // case GameAction.Travel
            gi.DieRollAction = GameAction.DieRollActionNone;
            if (RaftEnum.RE_RAFT_CHOSEN == gi.RaftState)
            {
               GameEngine.theFeatsInGame.myIsRaftTravel = true;
               ++gi.Statistic.myNumDaysOnRaft;
               gi.EventDisplayed = gi.EventActive = "e213"; // next screen to show
            }
            else if (true == gi.IsShortHop)
            {
               gi.EventDisplayed = gi.EventActive = "e204s"; // next screen to show
            }
            else if (true == gi.IsAirborne)
            {
               GameEngine.theFeatsInGame.myIsAirTravel = true;
               ++gi.Statistic.myNumDaysAirborne;
               gi.EventDisplayed = gi.EventActive = "e204a"; // next screen to show
            }
            else if (true == gi.IsPartyRiding())
            {
               gi.EventDisplayed = gi.EventActive = "e204m"; // next screen to show
            }
            else
            {
               gi.EventDisplayed = gi.EventActive = "e204u"; // next screen to show
            }
         }
      }
      protected bool SetHuntState(IGameInstance gi, ref GameAction action)
      {
         ITerritory t = gi.NewHex;
         if (null == t)
            t = gi.Prince.Territory;
         bool isStructure = gi.IsInStructure(t);
         bool isHunting = (false == isStructure) && (("Countryside" == t.Type) || ("Hills" == t.Type) || ("Forest" == t.Type) || ("Farmland" == t.Type) || ("Swamp" == t.Type)) || (true == gi.IsEagleHunt); // no hunting in mountain or desert w/o eagle
         bool isBuyingFood = ((true == isStructure) && (0 < gi.GetCoins()));
         if (((true == isBuyingFood) || (true == isHunting)) && (false == gi.IsJailed) && (false == gi.IsEnslaved) && (false == gi.IsGuardEncounteredThisTurn) && (false == gi.IsHuntedToday) && (false == gi.IsPoisonPlant))
         {
            action = GameAction.Hunt;
            gi.GamePhase = GamePhase.Hunt;
            gi.DieRollAction = GameAction.DieRollActionNone;
            gi.IsGridActive = true; // SetHuntState()
         }
         else
         {
            gi.IsPoisonPlant = false; // e093 - cannot hunt if encountered poison plants
            if (false == SetEndOfDayState(gi, ref action)) // Finished hunting so go to End of Day state
            {
               Logger.Log(LogEnum.LE_ERROR, "SetHuntState(): SetEndOfDayState() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool SetEndOfDayState(IGameInstance gi, ref GameAction action)
      {
         ITerritory princeTerritory = gi.Prince.Territory;
         if ((true == IsNorthofTragothRiver(princeTerritory)) && (false == gi.IsEnslaved) && (false == gi.IsGuardEncounteredThisTurn) && ("e061" != gi.EventStart) && ("e062" != gi.EventStart) && ("e063" != gi.EventStart) && ("e024" != gi.EventStart))
         {
            gi.IsGuardEncounteredThisTurn = true;
            gi.GamePhase = GamePhase.Hunt;
            gi.EventDisplayed = gi.EventActive = "e002"; // next screen to show
            gi.DieRollAction = GameAction.HuntE002aEncounterRoll; // If north of Tragforth river
         }
         else if ((true == gi.HiddenTemples.Contains(princeTerritory)) && (false == gi.IsTempleGuardEncounteredThisHex))
         {
            gi.IsTempleGuardEncounteredThisHex = true;
            if ((true == gi.EscapedLocations.Contains(princeTerritory)) || (true == gi.KilledLocations.Contains(princeTerritory)))
            {
               gi.GamePhase = GamePhase.Encounter;
               gi.EventDisplayed = gi.EventActive = "e066b";
               gi.DieRollAction = GameAction.EncounterRoll;
            }
            else
            {
               if (false == SetResurrectionStateCheck(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "SetEndOfDayState(): SetResurrectionStateCheck() returned false");
                  return false;
               }
            }
         }
         else
         {
            if (false == SetResurrectionStateCheck(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "SetEndOfDayState(): SetResurrectionStateCheck() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool SetResurrectionStateCheck(IGameInstance gi, ref GameAction action)
      {
         foreach (IMapItem mi in gi.ResurrectedMembers)
         {
            mi.Reset();
            gi.AddCompanion(mi);
            mi.Endurance = Math.Max(1, mi.Endurance - 1);
            mi.IsResurrected = true;
         }
         gi.ResurrectedMembers.Clear();
         if (false == SetPlagueStateCheck(gi, ref action))
         {
            Logger.Log(LogEnum.LE_ERROR, "SetResurrectionStateCheck(): SetPlagueStateCheck() returned false");
            return false;
         }
         return true;
      }
      protected bool SetPlagueStateCheck(IGameInstance gi, ref GameAction action)
      {
         foreach (IMapItem mi in gi.LostPartyMembers) // return lost party members at end of day
            gi.PartyMembers.Add(mi);
         gi.LostPartyMembers.Clear();
         //------------------------------------
         bool isPlagueDustActive = false;
         foreach (IMapItem mi in gi.PartyMembers)
         {
            if (0 < mi.PlagueDustWound)
               isPlagueDustActive = true;
         }
         if (true == isPlagueDustActive) // Perform Plague Dust Affects if necessary
         {
            gi.IsGridActive = true; // SetPlagueStateCheck()
            gi.GamePhase = GamePhase.Campfire;
            action = GameAction.CampfirePlagueDust;
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         else
         {
            if (false == SetTalismanCheckState(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "SetPlagueStateCheck(): SetTalismanCheckState() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool SetTalismanCheckState(IGameInstance gi, ref GameAction action)
      {
         if ((true == gi.IsCharismaTalismanActive) && (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman)))
         {
            gi.IsGridActive = true; // SetTalismanCheckState()
            gi.IsCharismaTalismanActive = false;
            gi.GamePhase = GamePhase.Campfire;
            action = GameAction.CampfireTalismanDestroy;
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         else
         {
            if (false == SetMountDieCheckState(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "SetTalismanCheckState(): SetMountDieCheckState() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool SetMountDieCheckState(IGameInstance gi, ref GameAction action)
      {
         if (true == gi.IsMountsSick)
         {
            gi.IsGridActive = true; // SetMountDieCheckState()
            gi.GamePhase = GamePhase.Campfire;
            action = GameAction.CampfireMountDieCheck;
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         else
         {
            if (false == SetCampfireEncounterState(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "SetMountDieCheckState(): SetCampfireEncounterState() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool SetCampfireEncounterState(IGameInstance gi, ref GameAction action)
      {
         // This function perform encounter checks that happen in the Campfire state
         if ((true == gi.IsJailed) || (true == gi.IsDungeon) || (true == gi.IsEnslaved))  // short circuit and got to falcon check state if jailed or enslaved
         {
            if (false == SetCampfireFalconCheckState(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "SetCampfireEncounterState(): SetCampfireFinalConditionState() returned false");
               return false;
            }
            return true;
         }
         //--------------------------------------------------------------
         // Characters joining party may have knowledge of other locations
         if (false == gi.IsInMapItems("Wizard")) // Wizard still needs to be in party to provide advice
            gi.IsWizardJoiningParty = false;
         if (false == gi.IsInMapItems("DwarfWarrior")) // Dwarf Warrior still needs to be in party to provide advice
            gi.IsDwarfWarriorJoiningParty = false;
         if (true == gi.IsWizardJoiningParty)
         {
            action = GameAction.UpdateEventViewerActive;
            gi.IsWizardJoiningParty = false;
            gi.GamePhase = GamePhase.Encounter;
            gi.EventDisplayed = gi.EventActive = "e025";
            gi.DieRollAction = GameAction.E023WizardAdvice;
         }
         else if (true == gi.IsDwarfWarriorJoiningParty)
         {
            action = GameAction.UpdateEventViewerActive;
            gi.IsDwarfWarriorJoiningParty = false;
            gi.GamePhase = GamePhase.Encounter;
            gi.EventDisplayed = gi.EventActive = "e006f";
            gi.DieRollAction = GameAction.E006DwarfAdvice;
         }
         else if (0 < gi.EncounteredMinstrels.Count)
         {
            gi.IsHuntedToday = true;
            action = GameAction.UpdateEventViewerActive;
            IMapItem minstrel = gi.EncounteredMinstrels[0];
            gi.EncounteredMembers.Clear();
            gi.EncounteredMembers.Add(minstrel);
            gi.EncounteredMinstrels.Remove(minstrel);
            gi.GamePhase = GamePhase.Encounter;
            gi.EventDisplayed = gi.EventActive = "e341";
            gi.DieRollAction = GameAction.EncounterRoll;
            gi.DieResults["e341"][0] = Utilities.NO_RESULT;
            gi.DieRollAction = GameAction.EncounterRoll;
         }
         else if (true == gi.IsWolvesAttack)
         {
            action = GameAction.UpdateEventViewerActive;
            gi.IsWolvesAttack = false;
            gi.GamePhase = GamePhase.Encounter;
            gi.EventDisplayed = gi.EventActive = "e075b";
            gi.DieRollAction = GameAction.EncounterStart;
         }
         else if (true == gi.IsBearAttack)
         {
            action = GameAction.UpdateEventViewerActive;
            gi.IsBearAttack = false;
            gi.GamePhase = GamePhase.Encounter;
            gi.EventDisplayed = gi.EventActive = "e084b";
            gi.DieRollAction = GameAction.EncounterStart;
         }
         else
         {
            if (false == SetCampfireFalconCheckState(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "SetCampfireEncounterState(): SetCampfireFinalConditionState() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool SetCampfireFalconCheckState(IGameInstance gi, ref GameAction action)
      {
         if (true == gi.IsFalconInParty())
         {
            if (0 == gi.GetFoods()) // if no food to feed falcon(s), remove from party
            {
               gi.IsFalconFed = false;
               IMapItems toRemoveFalcons = new MapItems();
               foreach (IMapItem mi in gi.PartyMembers)
               {
                  if (true == mi.Name.Contains("Falcon"))
                     toRemoveFalcons.Add(mi);
               }
               foreach (IMapItem mi in toRemoveFalcons)
                  gi.RemoveAbandonerInParty(mi);
            }
         }
         else
         {
            gi.IsFalconFed = false;
         }
         if (true == gi.IsFalconFed)  // If there is a fed falcon in the party, see if can feed with food
         {
            gi.IsFalconFed = false;
            gi.IsGridActive = true;
            action = GameAction.CampfireFalconCheck;
            gi.GamePhase = GamePhase.Campfire;
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         else
         {
            if (false == CampfireShowFeatState(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "SetCampfireEncounterState(): CampfireShowFeatState() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool CampfireShowFeatState(IGameInstance gi, ref GameAction action)
      {
         if (false == GameEngine.theFeatsInGame.IsEqual(GameEngine.theFeatsInGameStarting))  // If feats exist to be shown
         {
            action = GameAction.CampfireShowFeat;
            gi.GamePhase = GamePhase.Campfire;
            gi.DieRollAction = GameAction.DieRollActionNone;
            gi.EventDisplayed = gi.EventActive = "e503a";
         }
         else
         {
            if (false == SetCampfireFinalConditionState(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "CampfireShowFeatState(): SetCampfireFinalConditionState() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool SetCampfireFinalConditionState(IGameInstance gi, ref GameAction action)
      {
         // Perform different action if jailed.
         if (true == gi.IsJailed)
         {
            action = GameAction.UpdateEventViewerActive;
            gi.EventDisplayed = gi.EventActive = "e203a";
            gi.DieRollAction = GameAction.CampfireWakeup;
         }
         else if (true == gi.IsDungeon)
         {
            action = GameAction.UpdateEventViewerActive;
            gi.EventDisplayed = gi.EventActive = "e203c";
            gi.DieRollAction = GameAction.CampfireWakeup;
         }
         else if (true == gi.IsEnslaved) // Show e203e which does die roll. Die roll generates Wakeup()/PerformJailBreak(). EventViewer decided path based on die roll.
         {
            gi.EventDisplayed = gi.EventActive = "e203e";
            gi.DieRollAction = GameAction.CampfireWakeup;
         }
         else
         {
            gi.IsGridActive = true;
            gi.GamePhase = GamePhase.Campfire;
            action = GameAction.CampfireStarvationCheck;
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         return true;
      }
      protected bool SetCampfireStarvationEndState(IGameInstance gi, ref GameAction action)
      {
         gi.IsGridActive = true; // SetCampfireStarvationEndState()
         gi.GamePhase = GamePhase.Campfire;
         gi.DieRollAction = GameAction.DieRollActionNone;
         //------------------------------------------------
         ITerritory t = gi.Prince.Territory; // If there is a movement to a new hex, check hunting in new hex
         if (0 < gi.MapItemMoves.Count)
            t = gi.MapItemMoves[0].NewTerritory;
         //------------------------------------------------
         bool isInEagleNest = ((true == gi.IsEagleHunt) || ((true == gi.EagleLairs.Contains(t)) && (true == gi.IsPartyFlying()))); // e114, e115, e116
         if (((true == gi.IsInStructure(t)) || (true == gi.IsFarmerLodging)) && (false == isInEagleNest)) // May lodge in town/castle/temple
         {
            action = GameAction.CampfireLodgingCheck;
            return true;
         }
         // True Love returns at start of new day. Otherwise wake up and arrange transport options for new day.
         if ((0 < gi.LostTrueLoves.Count) && (false == gi.IsTrueLoveHeartBroken)) // true love does not return until no longer heartbroken
            action = GameAction.CampfireTrueLoveCheck;
         else
            action = GameAction.CampfireLoadTransport;
         return true;
      } // Set to Transport loads or lodging check screen
      protected bool Wakeup(IGameInstance gi, ref GameAction action)
      {
         ++gi.Days; // advance the days before showing daily choices - this is the last action for the day
         gi.IsMinstrelPlaying = false; // e049 - at beginning of day, music stops
         foreach (IMapItem mi in gi.PartyMembers) // e049 - if minstrel played music, he leaves party
         {
            if (true == mi.IsPlayedMusic)
            {
               gi.RemoveAbandonerInParty(mi);
               break;
            }
         }
         gi.IsNewDayChoiceMade = false;
         gi.IsHeavyRainDismount = false;  // reset to indicate user needs to choose dismount if e079 heavy rains shown at beginning of day
         gi.ForbiddenAudiences.RemoveTimeConstraints(gi.Days);
         if (true == PerformEndCheck(gi, ref action)) // Wakeup()
            return true;
         if (true == gi.IsJailed)
         {
            GameEngine.theFeatsInGame.myNumNightsInJail++;
            if ((0 < GameEngine.theFeatsInGame.myNumNightsInJail) && (0 == (GameEngine.theFeatsInGame.myNumNightsInJail % 40))) // report every 40 times
            {
               GameEngine.theFeatsInGame.myIsNightsInJail = true;
               GameEngine.theFeatsInGameStarting.myIsNightsInJail = false;
            }
            gi.GamePhase = GamePhase.Campfire;
            gi.EventDisplayed = gi.EventActive = "e203a"; // next screen to show
            gi.DieRollAction = GameAction.E203NightInPrison;
         }
         else if (true == gi.IsDungeon)
         {
            GameEngine.theFeatsInGame.myNumNightsInJail++;
            if ((0 < GameEngine.theFeatsInGame.myNumNightsInJail) && (0 == (GameEngine.theFeatsInGame.myNumNightsInJail % 40)) ) // report every 40 times
            {
               GameEngine.theFeatsInGame.myIsNightsInJail = true;
               GameEngine.theFeatsInGameStarting.myIsNightsInJail = false;
            }
            gi.GamePhase = GamePhase.Campfire;
            gi.EventDisplayed = gi.EventActive = "e203c"; // next screen to show
            gi.DieRollAction = GameAction.E203NightInDungeon;
         }
         else if (true == gi.IsEnslaved)
         {
            gi.GamePhase = GamePhase.Campfire;
            gi.EventDisplayed = gi.EventActive = "e203e"; // next screen to show
            gi.DieRollAction = GameAction.E203NightEnslaved;
         }
         else if (true == gi.IsSpellBound)
         {
            gi.GamePhase = GamePhase.Encounter;
            gi.EventDisplayed = gi.EventActive = "e035a"; // next screen to show
            gi.DieRollAction = GameAction.EncounterRoll;
         }
         else if (true == gi.IsHeavyRain) // Wakeup() - at start of day, check to see if rains continue
         {
            gi.EventDisplayed = gi.EventActive = "e079a"; // next screen to show
            gi.GamePhase = GamePhase.Encounter;           // Need an encounter roll
            gi.DieRollAction = GameAction.EncounterRoll;
            gi.IsHeavyRain = false; // Wakeup()
         }
         else if (true == gi.IsFlood) // at start of day, check to see if flood continue
         {
            gi.EventDisplayed = gi.EventActive = "e092a"; // next screen to show
            gi.GamePhase = GamePhase.Encounter;           // Need an encounter roll
            gi.DieRollAction = GameAction.EncounterRoll;
            gi.IsFlood = false;
         }
         else
         {
            Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "Wakeup():  gi.MapItemMoves.Clear()  a=" + action.ToString());
            gi.MapItemMoves.Clear();
            gi.GamePhase = GamePhase.SunriseChoice;      // Nominal - Wakeup()
            Option optionSteadyIncome = gi.Options.Find("SteadyIncome");
            if (null == optionSteadyIncome)
            {
               Logger.Log(LogEnum.LE_ERROR, "MenuItemSave_Click(): = gi.Options.Find(SteadyIncome) returned null");
               return false;
            }
            if (true == optionSteadyIncome.IsEnabled)
            {
               int dailyCoin = 3 + Utilities.RandomGenerator.Next(3);
               gi.AddCoins("Wakeup", dailyCoin);
            }
            gi.DieRollAction = GameAction.DieRollActionNone;
            if (499 < gi.GetCoins())
            {
               action = GameAction.EndGameWin; // PerformEndCheck()
               gi.GamePhase = GamePhase.EndGame;
               gi.EndGameReason = "500+ gold";
               gi.EventDisplayed = gi.EventActive = "e501";
               gi.Statistic.myNumWins++;
               gi.Statistic.myEndDaysCount = gi.Days;
               gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
               gi.Statistic.myEndCoinCount = gi.GetCoins();
               gi.Statistic.myEndFoodCount = gi.GetFoods();
               GameEngine.theFeatsInGame.myIs500GoldWin = true;
            }
            else
            {
               gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
            }
         }
         GameLoadMgr loadMgr = new GameLoadMgr();
         if (false == loadMgr.SaveGameToFile(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemSave_Click(): SaveGameToMemory() returned false");
            return false;
         }
         return true;
      }
      protected bool PerformEndCheck(IGameInstance gi, ref GameAction action)
      {
         Logger.Log(LogEnum.LE_END_GAME_CHECK, "PerformEndCheck(): ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
         if ((6 == gi.DieResults["e203a"][0]) && ("e061" == gi.EventStart)) // need to show the battle axe chopping off head before ending game
         {
            ++gi.Statistic.myNumOfPrinceAxeDeath;
            return false;
         }
         bool isGameEnd = false;
         if (true == gi.Prince.IsKilled)
         {
            gi.GamePhase = GamePhase.EndGame;
            bool isNecklass = gi.Prince.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace);
            Logger.Log(LogEnum.LE_END_GAME, "PerformEndCheck(): 1-EndGameLost ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString() + " isNecklass=" + isNecklass.ToString());
            if (true == isNecklass)
            {
               action = GameAction.EndGameResurrect;  // PerformEndCheck()
               gi.EventDisplayed = gi.EventActive = "e192a";
               gi.DieRollAction = GameAction.DieRollActionNone;
            }
            else
            {
               action = GameAction.EndGameLost;  // PerformEndCheck() - Prince Killed
               if ("e203b" == gi.EventActive)
               {
                  GameEngine.theFeatsInGame.myIsLostAxeDeath = true;
                  gi.EndGameReason = "Beheaded in gory execution";
               }
               else
               {
                  gi.EndGameReason = "Prince killed";
               }
            }
            isGameEnd = true;
         }
         else if ((true == gi.Prince.IsUnconscious) && (1 == gi.PartyMembers.Count))
         {
            gi.GamePhase = GamePhase.EndGame;
            bool isNecklass = gi.Prince.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace);
            Logger.Log(LogEnum.LE_END_GAME, "PerformEndCheck(): 2-EndGameLost ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString() + " isNecklass=" + isNecklass.ToString());
            if (true == isNecklass)
            {
               action = GameAction.EndGameResurrect;  // PerformEndCheck()
               gi.EventDisplayed = gi.EventActive = "e192a";
               gi.DieRollAction = GameAction.DieRollActionNone;
            }
            else
            {
               action = GameAction.EndGameLost; // PerformEndCheck() - Unconscious and alone
               gi.EndGameReason = "Prince unconscious and alone to die";
            }
            isGameEnd = true;
         }
         else if (Utilities.MaxDays < gi.Days)
         {
            Logger.Log(LogEnum.LE_END_GAME, "PerformEndCheck(): EndGameLost-3 ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
            action = GameAction.EndGameLost;  // PerformEndCheck() - reached end time limit
            gi.GamePhase = GamePhase.EndGame;
            gi.EndGameReason = "Time Limit Reached";
            isGameEnd = true;
            GameEngine.theFeatsInGame.myIsLostOnTime = true;   
         }
         else if (499 < gi.GetCoins())
         {
            Logger.Log(LogEnum.LE_END_GAME, "PerformEndCheck(): EndGameWon 500+ gold ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
            action = GameAction.EndGameWin; // PerformEndCheck()
            gi.GamePhase = GamePhase.EndGame;
            gi.EndGameReason = "500+ gold";
            isGameEnd = true;
            GameEngine.theFeatsInGame.myIs500GoldWin = true;
         }
         else if (true == IsNorthofTragothRiver(gi.Prince.Territory))
         {
            if (true == gi.IsBlessed)
            {
               Logger.Log(LogEnum.LE_END_GAME, "PerformEndCheck(): EndGameWon Blessed ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
               action = GameAction.EndGameWin; // PerformEndCheck()
               gi.GamePhase = GamePhase.EndGame;
               gi.EndGameReason = "Blessed by Gods and North of Tragoth River";
               isGameEnd = true;
               GameEngine.theFeatsInGame.myIsBlessedWin = true;
            }
            else if (true == gi.IsSpecialItemHeld(SpecialEnum.StaffOfCommand))
            {
               Logger.Log(LogEnum.LE_END_GAME, "PerformEndCheck(): EndGameWon Staff ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
               action = GameAction.EndGameWin; // PerformEndCheck()
               gi.GamePhase = GamePhase.EndGame;
               gi.EndGameReason = "Hold the Staff of Command North of Tragoth River";
               isGameEnd = true;
               GameEngine.theFeatsInGame.myIsStaffOfCommandWin = true;
            }
            else if (true == gi.IsSpecialItemHeld(SpecialEnum.RoyalHelmOfNorthlands))
            {
               if ("0101" == gi.Prince.Territory.Name)  // In Ogon 
               {
                  Logger.Log(LogEnum.LE_END_GAME, "PerformEndCheck(): EndGameWon Helm1 ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
                  action = GameAction.EndGameWin; // PerformEndCheck()
                  gi.GamePhase = GamePhase.EndGame;
                  gi.EndGameReason = "Hold the Royal Helm of Northlands when in Ogon";
                  isGameEnd = true;
                  GameEngine.theFeatsInGame.myIsRoyalHelmWin = true;
               }
               else if ("1501" == gi.Prince.Territory.Name) // In Weshor
               {
                  Logger.Log(LogEnum.LE_END_GAME, "PerformEndCheck(): EndGameWon Helm2 ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
                  action = GameAction.EndGameWin; // PerformEndCheck()
                  gi.GamePhase = GamePhase.EndGame;
                  gi.EndGameReason = "Hold the Royal Helm of Northlands when in Weshor";
                  isGameEnd = true;
                  GameEngine.theFeatsInGame.myIsRoyalHelmWin = true;
               }
            }
         }
         if (true == gi.Prince.IsUnconscious)
            ++gi.Statistic.myNumOfPrinceUncounscious;
         if (GameAction.EndGameWin == action)  // PerformEndCheck()
         {
            gi.EventDisplayed = gi.EventActive = "e501";
            gi.DieRollAction = GameAction.DieRollActionNone;
            gi.Statistic.myNumWins++;
            gi.Statistic.myEndDaysCount = gi.Days;
            gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
            gi.Statistic.myEndCoinCount = gi.GetCoins();
            gi.Statistic.myEndFoodCount = gi.GetFoods();
            if (0 == gi.Options.GetGameIndex())
               GameEngine.theFeatsInGame.myIsOriginalGameWin = true;
            else if (1 == gi.Options.GetGameIndex())
               GameEngine.theFeatsInGame.myIsRandomPartyGameWin = true;
            else if (2 == gi.Options.GetGameIndex())
               GameEngine.theFeatsInGame.myIsRandomHexGameWin = true;
            else if (3 == gi.Options.GetGameIndex())
               GameEngine.theFeatsInGame.myIsRandomGameWin = true;
            else if (4 == gi.Options.GetGameIndex())
               GameEngine.theFeatsInGame.myIsFunGameWin = true;
            if (2 == gi.WitAndWileInitial)
               GameEngine.theFeatsInGame.myIsLowWitWin = true;
            Logger.Log(LogEnum.LE_SERIALIZE_FEATS, "PerformEndCheck(): 1-feats=" + GameEngine.theFeatsInGame.ToString());
         }
         else if (GameAction.EndGameLost == action)
         {
            gi.EventDisplayed = gi.EventActive = "e502";
            gi.DieRollAction = GameAction.DieRollActionNone;
            gi.Statistic.myEndDaysCount = gi.Days;
            gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
            gi.Statistic.myEndCoinCount = gi.GetCoins();
            gi.Statistic.myEndFoodCount = gi.GetFoods();
            Logger.Log(LogEnum.LE_SERIALIZE_FEATS, "PerformEndCheck(): 2-feats=" + GameEngine.theFeatsInGame.ToString());
         }
         return isGameEnd;
      }
      protected bool IsNorthofTragothRiver(ITerritory t)
      {
         foreach (string s in Utilities.theNorthOfTragothHexes)
         {
            if (s == t.Name)
               return true;
         }
         return false;
      }
      protected bool EncounterEscape(IGameInstance gi, ref GameAction action)
      {
         gi.IsAirborne = false;  // do not have event when landing
                                 //---------------------------------------------
         IMapItems abandonedPartyMembers = new MapItems();
         if (GameAction.EncounterEscapeFly == action) // remove all party members that are not flying
         {
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (false == mi.IsFlying)
                  abandonedPartyMembers.Add(mi);
            }
         }
         else if (GameAction.EncounterEscapeMounted == action) // remove all party members that are not flying
         {
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (false == mi.IsRiding)
                  abandonedPartyMembers.Add(mi);
            }
         }
         foreach (IMapItem mi in abandonedPartyMembers)
            gi.RemoveAbandonedInParty(mi);
         //---------------------------------------------
         ITerritory t = gi.Prince.Territory;
         if (true == gi.IsInStructure(t)) // need to track escaped locations for e050 if it is a structure
         {
            if (false == gi.EscapedLocations.Contains(t))
               gi.EscapedLocations.Add(t);
         }
         //---------------------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "EncounterEscape(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterEscape(): MovementUsed=0 for a=" + action.ToString());
         gi.Prince.MovementUsed = 0; // need this to construct MapItemMove when no movement is left
         if (0 == t.Adjacents.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterEscape(): no adjacent territories");
            return false;
         }
         List<string> adjNotCrossingRiverNames = new List<string>();
         //---------------------------------------------
         // Cannot escape accross river unless all flying or if there is a bridge. 
         bool isAllFlying = true;
         foreach (IMapItem mi in gi.PartyMembers)
         {
            if (false == mi.IsFlying)
               isAllFlying = false;
         }
         if (true == isAllFlying)
         {
            foreach (string territoryName in t.Adjacents)
               adjNotCrossingRiverNames.Add(territoryName);
         }
         else
         {
            foreach (string territoryName in t.Adjacents)
            {
               if ((true == t.Rivers.Contains(territoryName)) && (false == t.Roads.Contains(territoryName))) // cannot cross rivers unless there is a road
                  continue;
               adjNotCrossingRiverNames.Add(territoryName);
            }
            if (0 == adjNotCrossingRiverNames.Count)
            {
               foreach (string territoryName in t.Adjacents) // pick random territory regardless of river
                  adjNotCrossingRiverNames.Add(territoryName);
            }
         }
         //---------------------------------------------
         int randomNum = Utilities.RandomGenerator.Next(0, adjNotCrossingRiverNames.Count);
         if ((randomNum < 0) || (adjNotCrossingRiverNames.Count <= randomNum))
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterEscape(): invalid param randomNum=" + randomNum.ToString());
            return false;
         }
         string adjacentName = adjNotCrossingRiverNames[randomNum];
         ITerritory adjacentTerritory = Territory.theTerritories.Find(adjacentName);
         if (null == adjacentTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterEscape(): invalid param adjacent=null");
            return false;
         }
         gi.Prince.TerritoryStarting = gi.Prince.Territory;
         gi.NewHex = adjacentTerritory;         // EncounterEscape()
         gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_ESCAPE)); // EncounterEscape()
         this.AddVisitedLocation(gi); // EncounterEscape()
         if (false == AddMapItemMove(gi, adjacentTerritory))
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterEscape(): AddMapItemMove() return false");
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "EncounterEscape(): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
         if ("e036" == gi.EventStart) // e036 Party members not discovered again
            gi.LostPartyMembers.Clear();
         Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterEscape(): MovementUsed=Movement for a=" + action.ToString());
         gi.Prince.MovementUsed = gi.Prince.Movement; // no more travel or today
         if (0 == adjacentTerritory.Rafts.Count) // if there are no raft hexes, destroy the raft
            gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT; // EncounterEscape()
         // !!!!!!Must call EncounterEnd() in the calling routine if this is end of encounter b/c of EncounterEscapeFly and EncounterEscapeMounted take user to different screen to end encounter
         Logger.Log(LogEnum.LE_ENCOUNTER_ESCAPE, "EncounterEscape(): prince move=" + gi.MapItemMoves[0].ToString());
         return true;
      }
      protected bool EncounterFollow(IGameInstance gi, ref GameAction action)
      {
         gi.IsAirborne = false; // do not have event when landing
                                //---------------------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "EncounterFollow(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterFollow(): MovementUsed=0 for a=" + action.ToString());
         gi.Prince.MovementUsed = 0;
         ITerritory t = gi.Prince.Territory;
         if (0 == t.Adjacents.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterFollow(): no adjacent territories");
            return false;
         }
         //---------------------------------------------
         int randomNum = Utilities.RandomGenerator.Next(0, t.Adjacents.Count);
         if ((randomNum < 0) || (t.Adjacents.Count <= randomNum))
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterFollow(): invalid param randomNum=" + randomNum.ToString());
            return false;
         }
         string adjacentName = t.Adjacents[randomNum];
         ITerritory adjacentTerritory = Territory.theTerritories.Find(adjacentName);
         if (null == adjacentTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterFollow(): invalid param adjacent=null");
            return false;
         }
         //---------------------------------------------
         if ((true == gi.IsExhausted) && ((true == gi.NewHex.IsOasis) || ("Desert" != gi.NewHex.Type))) // e120
            gi.IsExhausted = false;
         gi.IsMustLeaveHex = false;
         gi.IsImpassable = false;
         gi.IsTrueLoveHeartBroken = false;
         gi.IsTempleGuardEncounteredThisHex = false;
         //---------------------------------------------
         gi.Prince.TerritoryStarting = gi.Prince.Territory;
         gi.NewHex = adjacentTerritory;         // EncounterFollow()
         gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_FOLLOW)); // EncounterFollow()
         this.AddVisitedLocation(gi); // EncounterFollow()
         if (false == AddMapItemMove(gi, adjacentTerritory))
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterFollow(): AddMapItemMove() return false");
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "EncounterFollow(): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
         switch (gi.EventActive)
         {
            case "e052": gi.EventDisplayed = gi.EventActive = "e052a"; gi.DieRollAction = GameAction.EncounterRoll; break;
            case "e055": gi.EventDisplayed = gi.EventActive = "e055a"; gi.DieRollAction = GameAction.EncounterRoll; break;
            case "e058a": gi.EventDisplayed = gi.EventActive = "e058c"; gi.DieRollAction = GameAction.EncounterRoll; break;
            case "e072": gi.EventStart = gi.EventDisplayed = gi.EventActive = "e072a"; gi.DieRollAction = GameAction.EncounterStart; break;
            case "e112": gi.EventDisplayed = gi.EventActive = "e112a"; gi.DieRollAction = GameAction.EncounterRoll; break;
            default: break;
         }
         Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterFollow(): MovementUsed=Movement for a=" + action.ToString());
         gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
         gi.DieResults[gi.EventActive][0] = Utilities.NO_RESULT;
         return true;
      }
      protected List<String> GetHexesWithinRange(IGameInstance gi, int range)
      {
         //------------------------------------------------------
         List<string> masterList = new List<string>();
         Queue<string> tStack = new Queue<string>();
         Queue<int> depthStack = new Queue<int>();
         Dictionary<string, bool> visited = new Dictionary<string, bool>();
         tStack.Enqueue(gi.Prince.Territory.Name);
         depthStack.Enqueue(0);
         visited[gi.Prince.Territory.Name] = false;
         masterList.Add(gi.Prince.Territory.Name);
         while (0 < tStack.Count)
         {
            String name = tStack.Dequeue();
            int depth = depthStack.Dequeue();
            if (true == visited[name])
               continue;
            if (range <= depth)
               continue;
            visited[name] = true;
            ITerritory t = Territory.theTerritories.Find(name);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "FindRandomHexRange(): t=null for " + name);
               return null;
            }
            foreach (string adj in t.Adjacents)
            {
               ITerritory adjacent = Territory.theTerritories.Find(adj);
               if (null == adjacent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "FindRandomHexRange(): adjacent=null for " + adj);
                  return null;
               }
               tStack.Enqueue(adjacent.Name);
               depthStack.Enqueue(depth + 1);
               if (false == masterList.Contains(adj))
               {
                  masterList.Add(adj);
                  visited[adj] = false;
               }
            }
         }
         return masterList;
      }
      protected ITerritory FindRandomHexRange(IGameInstance gi, int range)
      {
         //------------------------------------------------------
         List<string> masterList = new List<string>();
         Queue<string> tStack = new Queue<string>();
         Queue<int> depthStack = new Queue<int>();
         Dictionary<string, bool> visited = new Dictionary<string, bool>();
         tStack.Enqueue(gi.Prince.Territory.Name);
         depthStack.Enqueue(0);
         visited[gi.Prince.Territory.Name] = false;
         masterList.Add(gi.Prince.Territory.Name);
         while (0 < tStack.Count)
         {
            String name = tStack.Dequeue();
            int depth = depthStack.Dequeue();
            if (true == visited[name])
               continue;
            if (range <= depth)
               continue;
            visited[name] = true;
            ITerritory t = Territory.theTerritories.Find(name);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "FindRandomHexRange(): t=null for " + name);
               return null;
            }
            foreach (string adj in t.Adjacents)
            {
               ITerritory adjacent = Territory.theTerritories.Find(adj);
               if (null == adjacent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "FindRandomHexRange(): adjacent=null for " + adj);
                  return adjacent;
               }
               tStack.Enqueue(adjacent.Name);
               depthStack.Enqueue(depth + 1);
               if (false == masterList.Contains(adj))
               {
                  masterList.Add(adj);
                  visited[adj] = false;
               }
            }
         }
         //-------------------------------------------------
         masterList.Remove(gi.Prince.Territory.Name);
         int randomNum = Utilities.RandomGenerator.Next(masterList.Count); // get a random index into the masterList
         ITerritory selected = Territory.theTerritories.Find(masterList[randomNum]); // Find the territory of this random index
         if (null == selected)
         {
            Logger.Log(LogEnum.LE_ERROR, "FindRandomHexRange(): selected=null for " + masterList[randomNum]);
            return null;
         }
         return selected;
      }
      protected ITerritory FindRandomHexRangeAdjacent(IGameInstance gi)
      {

         if (0 == gi.Prince.Territory.Adjacents.Count)
            return gi.Prince.Territory;
         int randomNum = Utilities.RandomGenerator.Next(gi.Prince.Territory.Adjacents.Count); // get a random index into the masterList
         string tName = gi.Prince.Territory.Adjacents[randomNum];
         ITerritory adjacent = Territory.theTerritories.Find(tName);
         return adjacent;
      }
      protected ITerritory FindRandomHexRangeDirectionAndRange(IGameInstance gi, int direction, int range)
      {
         // Stop at the hex just prior to going off the board
         bool isColNumEven = false;
         int colNum = Int32.Parse(gi.Prince.Territory.Name.Substring(0, 2)); // (start index, length)
         int rowNum = Int32.Parse(gi.Prince.Territory.Name.Substring(2));
         switch (direction)
         {
            case 1: // North
               rowNum = Math.Max(1, rowNum - range);
               break;
            case 2: // NorthEast
               for (int i = 0; i < range; ++i)
               {
                  if (20 == colNum)
                     break;
                  if (1 == rowNum)
                     break;
                  isColNumEven = (0 == colNum % 2);
                  if (false == isColNumEven)
                  {
                     if (1 == rowNum)
                        break;
                     --rowNum;
                  }
                  ++colNum;
               }
               break;
            case 3: // SouthEast
               for (int i = 0; i < range; ++i)
               {
                  if (20 == colNum)
                     break;
                  if (23 == rowNum)
                     break;
                  isColNumEven = (0 == colNum % 2);
                  if (true == isColNumEven)
                  {
                     if (23 == rowNum)
                        break;
                     ++rowNum;
                  }
                  ++colNum;
               }
               break;
            case 4: // South
               isColNumEven = (0 == colNum % 2);
               if (false == isColNumEven)
                  rowNum = Math.Min(23, rowNum + range);
               else
                  rowNum = Math.Min(22, rowNum + range);
               break;
            case 5: // SouthWest
               for (int i = 0; i < range; ++i)
               {
                  if (1 == colNum)
                     break;
                  if (23 == rowNum)
                     break;
                  isColNumEven = (0 == colNum % 2);
                  if (true == isColNumEven)
                  {
                     if (23 == rowNum)
                        break;
                     ++rowNum;
                  }
                  --colNum;
               }
               break;
            case 6: // NorhWest
               for (int i = 0; i < range; ++i)
               {
                  if (1 == colNum)
                     break;
                  if (1 == rowNum)
                     break;
                  isColNumEven = (0 == colNum % 2);
                  if (false == isColNumEven)
                  {
                     if (1 == rowNum)
                        break;
                     --rowNum;
                  }
                  --colNum;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "FindRandomHexRangeDirectionAndRange(): Reached default direction=" + direction.ToString());
               return null;
         }
         string hex = colNum.ToString("D2") + rowNum.ToString("D2");
         ITerritory selected = Territory.theTerritories.Find(hex); // Find the territory of this random index
         if (null == selected)
         {
            Logger.Log(LogEnum.LE_ERROR, "FindRandomHexRangeDirectionAndRange(): selected=null for " + hex + " starting=" + gi.Prince.Territory.Name + " direction=" + direction.ToString() + " range=" + range.ToString());
            return null;
         }
         return selected;
      }
      protected bool SetSubstitutionEvent(IGameInstance gi, ITerritory princeTerritory, bool isTravel = false)
      {
         if ((true == gi.IsHighPass) && (true == isTravel))
         {
            gi.IsHighPass = false;
            ITerritory previousTerritory = GetPreviousHex(gi);
            if (null == previousTerritory)
            {
               if( 0 < gi.Days ) // on first day, might occur
                  Logger.Log(LogEnum.LE_ERROR, "SetSubstitutionEvent(): previousTerritory=null for t=" + gi.Prince.Territory.Name);
               previousTerritory = gi.Prince.Territory;
            }
            if (gi.NewHex.Name != previousTerritory.Name)
            {
               gi.EventAfterRedistribute = gi.EventActive; // encounter this event after high pass check
               gi.EventDisplayed = gi.EventActive = "e086a";
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
            }
            return true;
         }
         gi.RaftStatePrevUndo = gi.RaftState;
         if (false == isTravel)
            gi.RaftState = RaftEnum.RE_NO_RAFT; // SetSubstitutionEvent() and not traveling
         // if traveling and flying into eagle lairs, show e115. If not traveling and in eagle lair, assume already there.
         if ((((true == gi.IsPartyFlying()) && (true == isTravel)) || (false == isTravel)) && (true == gi.EagleLairs.Contains(gi.NewHex))) // e115 - return to eagle lair for free food
         {
            gi.EventDisplayed = gi.EventActive = "e115";
            Logger.Log(LogEnum.LE_MOVE_COUNT, "SetSubstitutionEvent(): MovementUsed=Movement for ae=e115");
            gi.Prince.MovementUsed = gi.Prince.Movement; // end the day
            gi.IsPartyFed = true;
            gi.IsMountsFed = true;
         }
         else if (("e075" == gi.EventActive) && (true == gi.IsInStructure(gi.NewHex))) // wolves encounter do not happen in structure
            gi.EventDisplayed = gi.EventActive = "e075a";
         else if (("e076" == gi.EventActive) && (true == gi.IsInStructure(gi.NewHex))) // no hunting cat encounter if in structure
            gi.EventDisplayed = gi.EventActive = "e076a";
         else if (("e077" == gi.EventActive) && (true == gi.IsInStructure(gi.NewHex))) // no herd of wild horses encounter if in structure
            gi.EventDisplayed = gi.EventActive = "e077a";
         else if (("e084" == gi.EventActive) && (true == gi.IsInStructure(gi.NewHex))) // bear encounter does not happen in structure
            gi.EventDisplayed = gi.EventActive = "e084a";
         else if (("e085" == gi.EventActive) && (true == gi.IsInStructure(gi.NewHex))) // narrow ledges does not happen in structure
            gi.EventDisplayed = gi.EventActive = "e085a";
         else if (("e095" == gi.EventActive) && (0 == gi.GetNonSpecialMountCount())) // Mounts at risk - Lost mounts do not happen if have none - Griffons/Harpy do not count
            gi.EventDisplayed = gi.EventActive = "e095a";
         else if (("e096" == gi.EventActive) && (0 == gi.GetNonSpecialMountCount())) // Mounts die - Lost mounts do not happen if have none - Griffons/Harpy do not count
            gi.EventDisplayed = gi.EventActive = "e096a";
         else if ("e078" == gi.EventActive)
         {
            if ((true == gi.IsInStructure(gi.NewHex)) || (0 < gi.NewHex.Roads.Count) || (RaftEnum.RE_NO_RAFT != gi.RaftState)) // if in structure, on roads, or rafting, do not implement bad going
               gi.EventDisplayed = gi.EventActive = "e078a"; // majestic view from road
            else if (0 == gi.GetNonSpecialMountCount(true))
               gi.EventDisplayed = gi.EventActive = "e078b"; // majestic view with no horses
            else if (gi.Prince.MovementUsed == gi.Prince.Movement)
               gi.EventDisplayed = gi.EventActive = "e078c"; // bad going for horses
         }
         return true;
      } // Before showing Encounter Event, show another event based on hex contents
      protected bool AddMapItemMove(IGameInstance gi, ITerritory newT)
      {
         //-------------------------------
         if (0 == gi.PartyMembers.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMapItemMove(): gi.PartyMembers.Count=0");
            gi.PartyMembers.Add(gi.Prince);
         }
         gi.Prince.TerritoryStarting = gi.Prince.Territory;
         MapItemMove mim = new MapItemMove(Territory.theTerritories, gi.Prince, newT);
         if (true == mim.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMapItemMove(): mim.CtorError=true for start=" + gi.Prince.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + gi.Prince.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + gi.Prince.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         gi.MapItemMoves.Insert(0, mim); // add at front
         return true;
      }
      protected IMapItem CreateCharacter(IGameInstance gi, string cName)
      {
         ITerritory princeTerritory = gi.Prince.Territory;
         string miName = cName + Utilities.MapItemNum.ToString();
         ++Utilities.MapItemNum;
         IMapItem character = new MapItem(miName, 1.0, false, false, "c84Rat", "c84Rat", gi.Prince.Territory, 1, 1, 0); ; // default character if something does not work
         //------------------------------------------------------------
         switch (cName)
         {
            case "Amazon": character = new MapItem(miName, 1.0, false, false, "c57Amazon", "c57Amazon", princeTerritory, 5, 6, 4); break;
            case "Bandit": character = new MapItem(miName, 1.0, false, false, "c21Bandit", "c21Bandit", princeTerritory, 4, 5, 1); break;
            case "BanditLeader": character = new MapItem(miName, 1.0, false, false, "c85BanditLead", "c85BanditLead", princeTerritory, 6, 6, 15); break;
            case "Bear": character = new MapItem(miName, 1.0, false, false, "c72Bear", "c72Bear", princeTerritory, 5, 5, 0); break;
            case "Boar": character = new MapItem(miName, 1.0, false, false, "c58Boar", "c58Boar", princeTerritory, 5, 8, 0); break;
            case "Cavalry": character = new MapItem(miName, 1.0, false, false, "Cavalry", "Cavalry", princeTerritory, 0, 0, 0); break;
            case "Cat": character = new MapItem(miName, 1.0, false, false, "c59HuntingCat", "c59HuntingCat", princeTerritory, 3, 6, 0); break;
            case "Constabulary": character = new MapItem(miName, 1.0, false, false, "c45Constabulary", "c45Constabulary", princeTerritory, 4, 5, 4); break;
            case "Croc": character = new MapItem(miName, 1.0, false, false, "c73Crocodile", "c73Crocodile", princeTerritory, 6, 4, 0); break;
            case "Deserter": character = new MapItem(miName, 1.0, false, false, "c78Deserter", "c78Deserter", princeTerritory, 4, 4, 2); break;
            case "Dragon": character = new MapItem(miName, 1.0, false, false, "c33Dragon", "c33Dragon", princeTerritory, 11, 10, 0); break;
            case "Dwarf": character = new MapItem(miName, 1.0, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 6, 5, 10); break;
            case "DwarfW": character = new MapItem(miName, 1.0, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 6, 5, 12); break;
            case "DwarfLead": character = new MapItem(miName, 1.0, false, false, "c68DwarfLead", "c68DwarfLead", princeTerritory, 7, 6, 21); break;
            case "Eagle": character = new MapItem(miName, 1.0, false, false, "c62Eagle", "c62Eagle", princeTerritory, 3, 4, 1); break;
            case "Elf": character = new MapItem(miName, 1.0, false, false, "c56Elf", "c56Elf", princeTerritory, 4, 5, 7); break;
            case "ElfAssistant": character = new MapItem(miName, 1.0, false, false, "c56Elf", "c56Elf", princeTerritory, 3, 3, 2); break;
            case "ElfFriend": character = new MapItem(miName, 1.0, false, false, "c56Elf", "c56Elf", princeTerritory, 4, 4, 7); break;
            case "ElfWarrior": character = new MapItem(miName, 1.0, false, false, "c69ElfLead", "c69ElfLead", princeTerritory, 5, 5, 15); break;
            case "Falcon": character = new MapItem(miName, 1.0, false, false, "c82Falcon", "c82Falcon", princeTerritory, 0, 0, 0); break;
            case "Farmer": character = new MapItem(miName, 1.0, false, false, "c17Farmer", "c17Farmer", princeTerritory, 7, 4, 1); break;
            case "FarmerW": character = new MapItem(miName, 1.0, false, false, "c17Farmer", "c17Farmer", princeTerritory, 7, 4, 2); break;
            case "FarmerBoy": character = new MapItem(miName, 1.0, false, false, "c35FarmerBoy", "c35FarmerBoy", princeTerritory, 4, 3, 0); break;
            case "FarmerLeader": character = new MapItem(miName, 1.0, false, false, "c17Farmer", "c17Farmer", princeTerritory, 3, 2, 2); break;
            case "FarmerMobLeader": character = new MapItem(miName, 1.0, false, false, "c17Farmer", "c17Farmer", princeTerritory, 3, 2, 0); break;
            case "FarmerMob": character = new MapItem(miName, 1.0, false, false, "c17Farmer", "c17Farmer", princeTerritory, 2, 2, 0); break;
            case "FarmerRetainer": character = new MapItem(miName, 1.0, false, false, "c40Retainer", "c40Retainer", princeTerritory, 4, 4, 1); break;
            case "FarmerRich": character = new MapItem(miName, 1.0, false, false, "c17Farmer", "c17Farmer", princeTerritory, 6, 5, 30); break;
            case "Freeman": character = new MapItem(miName, 1.0, false, false, "c46Freeman", "c46Freeman", princeTerritory, 4, 3, 0); break;
            case "Ghost": character = new MapItem(miName, 1.0, false, false, "c20Ghost", "c20Ghost", princeTerritory, 2, 4, 0); break;
            case "Giant": character = new MapItem(miName, 1.0, false, false, "c61Giant", "c61Giant", princeTerritory, 8, 9, 10); break;
            case "Goblin": character = new MapItem(miName, 1.0, false, false, "c22Goblin", "c22Goblin", princeTerritory, 3, 3, 1); break;
            case "Golem": character = new MapItem(miName, 1.0, false, false, "c27Golem", "c27Golem", princeTerritory, 8, 6, 0); break;
            case "Griffon": character = new MapItem(miName, 1.0, false, false, "c63Griffon", "c63Griffon", princeTerritory, 6, 7, 12); break;
            case "Guard": character = new MapItem(miName, 1.0, false, false, "c66Guard", "c66Guard", princeTerritory, 5, 6, 4); break;
            case "GuardBody": character = new MapItem(miName, 1.0, false, false, "c87BodyGuard", "c87BodyGuard", princeTerritory, 6, 6, 0); break;
            case "GuardHeir": character = new MapItem(miName, 1.0, false, false, "c66Guard", "c66Guard", princeTerritory, 5, 4, 0); break;
            case "GuardHostile": character = new MapItem(miName, 1.0, false, false, "c50GuardHostile", "c50GuardHostile", princeTerritory, 6, 5, 7); break;
            case "Guardian": character = new MapItem(miName, 1.0, false, false, "c28Guardian", "c28Guardian", princeTerritory, 7, 7, 0); break;
            case "Guide": character = new MapItem(miName, 1.0, false, false, "c48Guide", "c48Guide", princeTerritory, 3, 2, 0); break;
            case "HalflingLead": character = new MapItem(miName, 1.0, false, false, "c70HalflingLead", "c70HalflingLead", princeTerritory, 6, 3, 4); break;
            case "Harpy": character = new MapItem(miName, 1.0, false, false, "c83Harpy", "c83Harpy", princeTerritory, 4, 5, 4); break;
            case "Hawkman": character = new MapItem(miName, 1.0, false, false, "c81Hawkman", "c81Hawkman", princeTerritory, 5, 7, 7); break;
            case "Hobgoblin": character = new MapItem(miName, 1.0, false, false, "c23Hobgoblin", "c23Hobgoblin", princeTerritory, 5, 6, 4); break;
            case "HobgoblinW": character = new MapItem(miName, 1.0, false, false, "c23Hobgoblin", "c23Hobgoblin", princeTerritory, 5, 6, 5); break;
            case "Henchman": character = new MapItem(miName, 1.0, false, false, "c49Henchman", "c49Henchman", princeTerritory, 3, 2, 0); break;
            case "Huldra": character = new MapItem(miName, 1.0, false, false, "c89Huldra", "c89Huldra", princeTerritory, 8, 6, 0); break;
            case "Knight": character = new MapItem(miName, 1.0, false, false, "c52Knight", "c52Knight", princeTerritory, 6, 7, 0); break;
            case "KnightBlack": character = new MapItem(miName, 1.0, false, false, "c80BlackKnight", "c80BlackKnight", princeTerritory, 8, 8, 30); break;
            case "Lancer": character = new MapItem(miName, 1.0, false, false, "c47Lancer", "c47Lancer", princeTerritory, 5, 5, 0); break;
            case "Lizard": character = new MapItem(miName, 1.0, false, false, "c67Lizard", "c67Lizard", princeTerritory, 12, 10, 0); break;
            case "Magician": character = new MapItem(miName, 1.0, false, false, "c16Magician", "c16Magician", princeTerritory, 5, 3, 0); break;
            case "MagicianWeak": character = new MapItem(miName, 1.0, false, false, "c16Magician", "c16Magician", princeTerritory, 2, 3, 5); break;
            case "Mercenary": character = new MapItem(miName, 1.0, false, false, "c10Mercenary", "c10Mercenary", princeTerritory, 4, 5, 4); break;
            case "MercenaryLead": character = new MapItem(miName, 1.0, false, false, "c65MercLead", "c65MercLead", princeTerritory, 6, 6, 50); break;
            case "Merchant": character = new MapItem(miName, 1.0, false, false, "Negotiator1", "c77Merchant", princeTerritory, 3, 2, 5); break;
            case "Minstrel": character = new MapItem(miName, 1.0, false, false, "c60Minstrel", "c60Minstrel", princeTerritory, 0, 0, 0); break;
            case "Mirror":
               character = new MapItem(gi.Prince);
               character.Name = "Mirror";
               character.TopImageName = "c34PrinceMirror";
               character.BottomImageName = "c34PrinceMirror";
               character.OverlayImageName = "";
               break;
            case "Monk": character = new MapItem(miName, 1.0, false, false, "c19Monk", "c19Monk", princeTerritory, 5, 4, 4); break;
            case "MonkGuide": character = new MapItem(miName, 1.0, false, false, "c19Monk", "c19Monk", princeTerritory, 3, 2, 0); break;
            case "MonkHermit": character = new MapItem(miName, 1.0, false, false, "c19Monk", "c19Monk", princeTerritory, 6, 3, 0); break;
            case "MonkTraveling": character = new MapItem(miName, 1.0, false, false, "c19Monk", "c19Monk", princeTerritory, 3, 2, 4); break;
            case "MonkWarrior": character = new MapItem(miName, 1.0, false, false, "c19Monk", "c19Monk", princeTerritory, 6, 6, 10); break;
            case "Orc": character = new MapItem(miName, 1.0, false, false, "c30Orc", "c30Orc", princeTerritory, 5, 5, 2); break;
            case "OrcW": character = new MapItem(miName, 1.0, false, false, "c30Orc", "c30Orc", princeTerritory, 5, 5, 2); break;
            case "OrcWeak": character = new MapItem(miName, 1.0, false, false, "c30Orc", "c30Orc", princeTerritory, 5, 4, 1); break;
            case "OrcChief": character = new MapItem(miName, 1.0, false, false, "c64OrcChief", "c64OrcChief", princeTerritory, 6, 5, 7); break;
            case "PatrolMounted": character = new MapItem(miName, 1.0, false, false, "c74MountedPatrol", "c74MountedPatrol", princeTerritory, 5, 6, 4); break;
            case "PatrolMountedLead": character = new MapItem(miName, 1.0, false, false, "c75MountedPatrolLead", "c75MountedPatrolLead", princeTerritory, 5, 6, 10); break;
            case "Priest": character = new MapItem(miName, 1.0, false, false, "c14Priest", "c14Priest", princeTerritory, 3, 3, 25); break;
            case "Porter": character = new MapItem(miName, 1.0, false, false, "c11Porter", "c11Porter", princeTerritory, 0, 0, 0); break;
            case "PorterSlave": character = new MapItem(miName, 1.0, false, false, "c42SlavePorter", "c42SlavePorter", princeTerritory, 0, 0, 0); break;
            case "Reaver": character = new MapItem(miName, 1.0, false, false, "C36Reaver", "C36Reaver", princeTerritory, 4, 4, 4); break;
            case "ReaverBoss": character = new MapItem(miName, 1.0, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 5, 5, 10); break;
            case "ReaverLead": character = new MapItem(miName, 1.0, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 4, 5, 7); break;
            case "Roc": character = new MapItem(miName, 1.0, false, false, "c55Roc", "c55Roc", princeTerritory, 8, 9, 10); break;
            case "Runaway": character = new MapItem(miName, 1.0, false, false, "c09Runaway", "c09Runaway", princeTerritory, 4, 4, 0); break;
            case "Spectre": character = new MapItem(miName, 1.0, false, false, "c25Spectre", "c25Spectre", princeTerritory, 3, 7, 0); break;
            case "SlaveGirl": character = new MapItem(miName, 1.0, false, false, "c41SlaveGirl", "c41SlaveGirl", princeTerritory, 0, 0, 0); break;
            case "Spider": character = new MapItem(miName, 1.0, false, false, "c54Spider", "c54Spider", princeTerritory, 3, 4, 0); break;
            case "Swordsman": character = new MapItem(miName, 1.0, false, false, "c53Swordsman", "c53Swordsman", princeTerritory, 6, 6, 7); break;
            case "Swordswoman": character = new MapItem(miName, 1.0, false, false, "c76Swordswoman", "c76Swordswoman", princeTerritory, 7, 7, 4); break;
            case "Troll": character = new MapItem(miName, 1.0, false, false, "c31Troll", "c31Troll", princeTerritory, 8, 8, 15); break;
            case "OrcDemi": character = new MapItem(miName, 1.0, false, false, "c29DemiTroll", "c29DemiTroll", princeTerritory, 7, 8, 10); break;
            case "TrueLoveLordsDaughter": character = new MapItem(miName, 1.0, false, false, "c44TrueLove", "c44TrueLove", princeTerritory, 0, 0, 0); break;
            case "TrueLovePriestDaughter": character = new MapItem(miName, 1.0, false, false, "c44TrueLove", "c44TrueLove", princeTerritory, 4, 2, 0); break;
            case "TrueLoveSlave": character = new MapItem(miName, 1.0, false, false, "c44TrueLove", "SlaveWoman", princeTerritory, 4, 2, 0); break;
            case "TrueLoveSwordwoman": character = new MapItem(miName, 1.0, false, false, "c44TrueLove", "c44TrueLove", princeTerritory, 7, 7, 4); break;
            case "TrustedAssistant": character = new MapItem(miName, 1.0, false, false, "c51TrustedAssistant", "c51TrustedAssistant", princeTerritory, 4, 4, 0); break;
            case "Warrior": character = new MapItem(miName, 1.0, false, false, "c79Warrior", "c79Warrior", princeTerritory, 6, 7, 0); break;
            case "WarriorBoy": character = new MapItem(miName, 1.0, false, false, "c86WarriorBoy", "c86WarriorBoy", princeTerritory, 7, 5, 0); break;
            case "WarriorOld": character = new MapItem(miName, 1.0, false, false, "c43OldWarrior", "c43OldWarrior", princeTerritory, 0, 0, 0); break;
            case "Witch": character = new MapItem(miName, 1.0, false, false, "c13Witch", "c13Witch", princeTerritory, 3, 1, 5); break;
            case "Wizard": character = new MapItem(miName, 1.0, false, false, "c12Wizard", "c12Wizard", princeTerritory, 4, 4, 60); break;
            case "WHenchman": character = new MapItem(miName, 1.0, false, false, "c49Henchman", "c49Henchman", princeTerritory, 4, 5, 4); break;
            case "Wolf": character = new MapItem(miName, 1.0, false, false, "c71Wolf", "c71Wolf", princeTerritory, 3, 3, 0); break;
            case "Wraith": character = new MapItem(miName, 1.0, false, false, "c24Wraith", "c24Wraith", princeTerritory, 9, 6, 0); break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "CreateCharacter(): Reached default character=" + cName);
               break;
         }
         //------------------------------------------------------------
         Option isEasiestMonstersOption = gi.Options.Find("EasiestMonsters");
         if (null == isEasiestMonstersOption)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateCharacter(): unknwon option=EasiestMonsters");
            gi.EncounteredMembers.Add(character);
            return character;
         }
         Option isEasyMonstersOption = gi.Options.Find("EasyMonsters");
         if (null == isEasyMonstersOption)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateCharacter(): unknwon option=EasyMonsters");
            gi.EncounteredMembers.Add(character);
            return character;
         }
         Option isLessHardMonstersOption = gi.Options.Find("LessHardMonsters");
         if (null == isLessHardMonstersOption)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateCharacter(): unknwon option=LessHardMonsters");
            gi.EncounteredMembers.Add(character);
            return character;
         }
         //------------------------------------------------------------
         if (true == isEasiestMonstersOption.IsEnabled)
         {
            character.Endurance = 1;
            character.Combat = 1;
         }
         else if (true == isEasyMonstersOption.IsEnabled)
         {
            int newEndurance = character.Endurance - 2;
            character.Endurance = Math.Max(newEndurance, 1);
            int newCombat = character.Combat - 2;
            character.Combat = Math.Max(newCombat, 1);
         }
         else if (true == isLessHardMonstersOption.IsEnabled)
         {
            int newEndurance = character.Endurance - 1;
            character.Endurance = Math.Max(newEndurance, 1);
            int newCombat = character.Combat - 1;
            character.Combat = Math.Max(newCombat, 1);
         }
         //------------------------------------------------------------
         Logger.Log(LogEnum.LE_PARTYMEMBER_ADD, "CreateCharacter(): mi=[" + character.ToString() + "]");
         return character;
      }
      protected bool LoadGame(ref IGameInstance gi, ref GameAction action)
      {
         Option.LogGameType("LoadGame(): ", gi.Options);
         foreach ( IMapItem mi in gi.PartyMembers ) // If loading game with bad content, fix it
         {
            if (mi.Coin < 0)
               mi.Coin = 0;
            if (mi.Food < 0)
               mi.Food = 0;
         }
         //---------------------------------------------------------
         Option option = gi.Options.Find("ExtendEndTime");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadGame(): Options.Find(ExtendEndTime) returned null");
            option = new Option("ExtendEndTime", false);
         }
         if (true == option.IsEnabled)
            Utilities.MaxDays = 105;
         else
            Utilities.MaxDays = 70;
         //---------------------------------------------------------
         if ((true == gi.IsJailed) || (true == gi.IsDungeon) || (true == gi.IsEnslaved))
         {
            gi.GamePhase = GamePhase.Campfire;
            gi.EventDisplayed = gi.EventActive = "e203f";
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         else
         {
            gi.GamePhase = GamePhase.Encounter;      // GameStateSetup.PerformAction()
            gi.EventDisplayed = gi.EventActive = "e401"; // next screen to show
            gi.DieRollAction = GameAction.DieRollActionNone;
            if (0 == gi.MapItemMoves.Count)
            {
               --gi.Prince.MovementUsed;
               if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
               {
                  Logger.Log(LogEnum.LE_ERROR, "LoadGame(): AddMapItemMove() return false");
                  return false;
               }
               ++gi.Prince.MovementUsed;
            }
            gi.Prince.MovementUsed = gi.Prince.Movement; // no more movement when reverting to daybreak
         }
         ITerritory st = Territory.theTerritories.Find(gi.Prince.TerritoryStarting.Name);
         gi.Prince.TerritoryStarting = st;
         ITerritory t = Territory.theTerritories.Find(gi.Prince.Territory.Name);
         gi.Prince.Territory = t;
         Double x = t.CenterPoint.X - (gi.Prince.Zoom * Utilities.theMapItemOffset);
         Double y = t.CenterPoint.Y - (gi.Prince.Zoom * Utilities.theMapItemOffset);
         Logger.Log(LogEnum.LE_VIEW_MAPITEM_LOCATION, "LoadGame(): prince=(" + x.ToString("0.0") + "," + y.ToString("0.0") + ") t=" + t.Name + "=(" + gi.Prince.Territory.CenterPoint.X.ToString("0.0") + "," + gi.Prince.Territory.CenterPoint.Y.ToString("0.0") + ")"); ;
         return true;
      }
      protected void UndoCommand(ref IGameInstance gi, ref GameAction action)
      {
         Logger.Log(LogEnum.LE_UNDO_COMMAND, "UndoCommand(): cmd=" + gi.IsUndoCommandAvailable.ToString() + "-->false  a=" + action.ToString());
         gi.IsUndoCommandAvailable = false;
         gi.IsAirborne = false;
         foreach(IMapItem mi in gi.PartyMembers) // return mounts left on ground
         {
            foreach( IMapItem mount in mi.LeftOnGroundMounts)
               mi.AddMount(mount);
            mi.LeftOnGroundMounts.Clear();
         }
         gi.SunriseChoice = GamePhase.StartGame;
         gi.GamePhase = GamePhase.SunriseChoice;
         gi.NewHex = gi.Prince.Territory;
         if ((RaftEnum.RE_RAFT_CHOSEN == gi.RaftStatePrevUndo) || (RaftEnum.RE_RAFT_SHOWN == gi.RaftStatePrevUndo))
            gi.RaftState = RaftEnum.RE_RAFT_SHOWN;
         gi.EventDisplayed = gi.EventActive = "e203";
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "UndoCommand():  gi.MapItemMoves.Clear()  a=" + action.ToString());
         gi.MapItemMoves.Clear();
         //-----------------------------
         foreach(string s in gi.UndoHeal) // undo rest when in structure
         {
            IMapItem mi = gi.PartyMembers.Find(s);
            mi.SetWounds(1, 0); 
         }
         gi.UndoHeal.Clear();
         foreach (string s in gi.UndoExhaust)
         {
            IMapItem mi = gi.PartyMembers.Find(s);
            mi.IsExhausted = true;
         }
         gi.UndoExhaust.Clear();
      }
      protected void AddVisitedLocation(IGameInstance gi)
      {
         if (true == gi.NewHex.IsRuin)
         {
            if (false == GameEngine.theFeatsInGame.myVisitedRuins.Contains(gi.NewHex.Name))
            {
               GameEngine.theFeatsInGame.myVisitedRuins.Add(gi.NewHex.Name);
               if (GameFeat.LOCATIONS_RUIN <= GameEngine.theFeatsInGame.myVisitedRuins.Count)
                  GameEngine.theFeatsInGame.myIsVisitAllRuins = true;
            }
         }
         if (true == gi.NewHex.IsOasis)
         {
            if (false == GameEngine.theFeatsInGame.myVisitedOasises.Contains(gi.NewHex.Name))
            {
               GameEngine.theFeatsInGame.myVisitedOasises.Add(gi.NewHex.Name);
               if (GameFeat.LOCATIONS_OASIS <= GameEngine.theFeatsInGame.myVisitedOasises.Count)
                  GameEngine.theFeatsInGame.myIsVisitAllOasis = true;
            }
         }
         if ((true == gi.IsInStructure(gi.NewHex)) && (false == gi.VisitedLocations.Contains(gi.NewHex)))
         {
            gi.VisitedLocations.Add(gi.NewHex);
            if( true == gi.NewHex.IsCastle )
            {
               if( false == GameEngine.theFeatsInGame.myVisitedCastles.Contains(gi.NewHex.Name))
               {
                  GameEngine.theFeatsInGame.myVisitedCastles.Add(gi.NewHex.Name);
                  if (GameFeat.LOCATIONS_CASTLE <= GameEngine.theFeatsInGame.myVisitedCastles.Count)
                     GameEngine.theFeatsInGame.myIsVisitAllCastles = true; 
               }
            }
            else if (true == gi.NewHex.IsTemple)
            {
               if (false == GameEngine.theFeatsInGame.myVisitedTemples.Contains(gi.NewHex.Name))
               {
                  GameEngine.theFeatsInGame.myVisitedTemples.Add(gi.NewHex.Name);
                  if (GameFeat.LOCATIONS_TEMPLE <= GameEngine.theFeatsInGame.myVisitedTemples.Count)
                     GameEngine.theFeatsInGame.myIsVisitAllTemples = true;
               }
            }
            else if (true == gi.NewHex.IsTown)
            {
               if (false == GameEngine.theFeatsInGame.myVisitedTowns.Contains(gi.NewHex.Name))
               {
                  GameEngine.theFeatsInGame.myVisitedTowns.Add(gi.NewHex.Name);
                  if (GameFeat.LOCATIONS_TOWN <= GameEngine.theFeatsInGame.myVisitedTowns.Count)
                     GameEngine.theFeatsInGame.myIsVisitAllTowns = true;
               }
            }
         }
      }
   }
   //-----------------------------------------------------
   class GameStateSetup : GameState
   {
      private static bool theIsGameSetup = false;
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.UpdateNewGame:
            case GameAction.RemoveSplashScreen:
               int diagInfoLevel = (int)LogEnum.LE_GAME_INIT_VERSION;
               if (true == Logger.theLogLevel[diagInfoLevel])
                  PrintDiagnosticInfoToLog();
               // Logger.Log(LogEnum.LE_VIEW_MAPITEM_LOCATION, "PerformAction(RemoveSplashScreen): territories=" + Territory.theTerritories.ToString());
               //-------------------------------------------------
               theIsGameSetup = false;
               gi.Statistic.Clear();         // Clear any current statitics
               gi.Statistic.myNumGames = 1;  // Set played games to 1
               Option option = gi.Options.Find("AutoSetup");
               if (null == option)
               {
                  option = new Option("AutoSetup", false);
                  gi.Options.Add(option);
               }
               if (true == option.IsEnabled)
               {
                  if (false == PerformAutoSetup(ref gi, ref action))
                  {
                     returnStatus = "PerformAutoSetup() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
                  }
                  gi.GamePhase = GamePhase.SunriseChoice;      // RemoveSplashScreen
                  gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               else
               {
                  gi.Options.SetOriginalGameOptions();
                  gi.GamePhase = GamePhase.GameSetup;
                  gi.EventDisplayed = gi.EventActive = "e000"; // next screen to show
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               if (false == AddStartingPrinceOption(gi))
               {
                  returnStatus = "AddStartingPrinceOption() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               if (false == AddStartingOptions(gi))
               {
                  returnStatus = "AddStartingOptions() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               AddStartingTestingOptions(gi); // RemoveSplashScreen
               //------------------------------------------
               Logger.Log(LogEnum.LE_SERIALIZE_FEATS, "GameStateSetup.PerformAction(RemoveSplashScreen): \n feats=" + GameEngine.theFeatsInGame.ToString() );
               break;
            case GameAction.SetupShowCalArath:
               gi.EventDisplayed = gi.EventActive = "e000a";
               break;
            case GameAction.SetupShowStartingWealth:
               gi.EventDisplayed = gi.EventActive = "e000b";
               gi.DieRollAction = GameAction.EncounterLootStart;
               break;
            case GameAction.EncounterLootStart:
               gi.CapturedWealthCodes.Add(2);
               gi.ActiveMember = gi.Prince;
               break;
            case GameAction.EncounterLootStartEnd:
               gi.EventDisplayed = gi.EventActive = "e000c"; // next screen to show
               gi.DieRollAction = GameAction.SetupRollWitsWiles;
               break;
            case GameAction.SetupRollWitsWiles:
               if (dieRoll < 2)
                  gi.WitAndWile = 2; // cannot be less than two
               else
                  gi.WitAndWile = dieRoll;
               gi.WitAndWileInitial = gi.WitAndWile; // GameStateSetup.PerformAction(SetupRollWitsWiles)
               Logger.Log(LogEnum.LE_WIT_AND_WILES_INIT, "GameStateSetup.PerformAction(SetupRollWitsWiles): dr=" + dieRoll.ToString() + " ww=" + gi.WitAndWile.ToString());
               gi.EventDisplayed = gi.EventActive = "e000d"; // next screen to show
               break;
            case GameAction.SetupManualWitsWiles:
               if (0 != gi.WitAndWile) // if zero, user selected no number - otherwise, user added or subtracted one from wit and wiles
               {
                  gi.WitAndWile += dieRoll; // manual changes to wits and wiles handled here
                  if (gi.WitAndWile < 2)
                     gi.WitAndWile = 2;
                  else if (6 < gi.WitAndWile)
                     gi.WitAndWile = 6;
                  gi.WitAndWileInitial = gi.WitAndWile; // GameStateSetup.PerformAction(SetupManualWitsWiles)
                  Logger.Log(LogEnum.LE_WIT_AND_WILES_INIT, "GameStateSetup.PerformAction(SetupManualWitsWiles): dr=" + dieRoll.ToString() + " ww=" + gi.WitAndWile.ToString());
               }
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.SetupGameOptionChoice:
               gi.EventDisplayed = gi.EventActive = "e001"; // next screen to show
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.SetupStartingLocation:
               gi.EventDisplayed = gi.EventActive = "e001a"; // next screen to show
               gi.DieRollAction = GameAction.SetupLocationRoll;
               break;
            case GameAction.SetupChooseFunOptions:
               gi.Options.SelectFunGameOptions();
               if (false == SetStartingLocation(ref gi, 100))
               {
                  returnStatus = "SetStartingLocation() returned error";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               if (false == AddStartingPrinceOption(gi))
               {
                  returnStatus = "AddStartingPrinceOption() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else if (false == AddStartingOptions(gi))
               {
                  returnStatus = "AddStartingOptions() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               AddStartingTestingOptions(gi); // SetupChooseFunOptions
               gi.GamePhase = GamePhase.SunriseChoice;      // GameStateSetup.PerformAction()
               gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.SetupLocationRoll:
               if (Utilities.NO_RESULT == gi.DieResults["e001a"][0])
               {
                  gi.DieResults["e001a"][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               break;
            case GameAction.SetupFinalize:
               if (false == SetStartingLocation(ref gi, gi.DieResults["e001a"][0]))
               {
                  returnStatus = "SetStartingLocation() returned error";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               gi.GamePhase = GamePhase.SunriseChoice;      // GameStateSetup.PerformAction()
               gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            default:
               returnStatus = "Reached Default ERROR with a=" + action.ToString();
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateSetup.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      private bool SetStartingLocation(ref IGameInstance gi, int dieRoll)
      {
         ITerritory starting = null;
         switch (dieRoll)
         {
            case 1: starting = Territory.theTerritories.Find("0101"); break;
            case 2: starting = Territory.theTerritories.Find("0701"); break;
            case 3: starting = Territory.theTerritories.Find("0901"); break;
            case 4: starting = Territory.theTerritories.Find("1301"); break;
            case 5: starting = Territory.theTerritories.Find("1501"); break;
            case 6: starting = Territory.theTerritories.Find("1801"); break;
            case 100: break; // when skipping to Fun Game Options instead of Original Setup
            default: Logger.Log(LogEnum.LE_ERROR, "SetStartingLocation() reached default dr=" + dieRoll.ToString()); return false;
         }
         if (false == SetStartingLocationOption(gi, ref starting))
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocation()  SetStartingLocationOption() returned false");
            return false;
         }
         if (null == starting)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocation() starting territory=null");
            return false;
         }
         gi.NewHex = gi.Prince.Territory = gi.Prince.TerritoryStarting = starting;
         gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_START));
         this.AddVisitedLocation(gi); // SetStartingLocation()
         gi.Prince.Territory = starting;  
         gi.Prince.TerritoryStarting = starting;
         foreach (IMapItem mi in gi.PartyMembers)
         {
            mi.TerritoryStarting = starting;
            mi.Territory = starting;
            Double x = starting.CenterPoint.X - (gi.Prince.Zoom * Utilities.theMapItemOffset);
            Double y = starting.CenterPoint.Y - (gi.Prince.Zoom * Utilities.theMapItemOffset);
         }
         return true;
      }
      private bool PerformAutoSetup(ref IGameInstance gi, ref GameAction action)
      {
         int dr = Utilities.RandomGenerator.Next(6);
         ++dr;
         switch (dr) // Setup Wealth, Wits and Wiles, Starting Location
         {
            case 1: gi.Prince.Coin += 0; break;
            case 2: gi.Prince.Coin += 1; break;
            case 3: gi.Prince.Coin += 2; break;
            case 4: gi.Prince.Coin += 2; break;
            case 5: gi.Prince.Coin += 3; break;
            case 6: gi.Prince.Coin += 4; break;
            default: Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetup(): reached default dr=" + dr.ToString()); return false;
         }
         dr = Utilities.RandomGenerator.Next(6);
         ++dr;
         gi.WitAndWile = dr;
         if (1 == gi.WitAndWile) // cannot start with one 
            gi.WitAndWile = 2;
         gi.WitAndWileInitial = gi.WitAndWile; // PerformAutoSetup()
         Logger.Log(LogEnum.LE_WIT_AND_WILES_INIT, "PerformAutoSetup(): dr=" + dr.ToString() + " ww=" + gi.WitAndWile.ToString());
         dr = Utilities.RandomGenerator.Next(6);
         ++dr;
         if (false == SetStartingLocation(ref gi, dr))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetup():  SetStartingLocation() return false");
            return false;
         }
         return true;
      }
      private bool AddStartingPrinceOption(IGameInstance gi)
      {
         int coin = gi.Prince.Coin;
         gi.Prince.Reset(); // clear if this is run twice as party of setup - user selects Fun Options
         gi.Prince.Coin = coin;
         Options options = gi.Options;
         Option option = null;
         String itemToAdd = "";
         //---------------------------------------------------------
         itemToAdd = "PrinceHorse";
         option = options.Find(itemToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + itemToAdd + ") returned null");
            option = new Option(itemToAdd, false);
         }
         if (true == option.IsEnabled)
         {
            gi.Prince.AddNewMount();
            gi.Prince.AddNewMount();
            gi.Prince.AddNewMount();
         }
         //---------------------------------------------------------
         itemToAdd = "PrincePegasus";
         option = options.Find(itemToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + itemToAdd + ") returned null");
            option = new Option(itemToAdd, false);
         }
         if (true == option.IsEnabled)
            gi.Prince.AddNewMount(MountEnum.Pegasus);
         //---------------------------------------------------------
         itemToAdd = "PrinceCoin";
         option = options.Find(itemToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + itemToAdd + ") returned null");
            option = new Option(itemToAdd, false);
         }
         if (true == option.IsEnabled)
            gi.Prince.Coin += (Utilities.RandomGenerator.Next(50) + 50);
         //---------------------------------------------------------
         itemToAdd = "PrinceFood";
         option = options.Find(itemToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + itemToAdd + ") returned null");
            option = new Option(itemToAdd, false);
         }
         if (true == option.IsEnabled)
            gi.Prince.Food += 5;
         //---------------------------------------------------------
         itemToAdd = "StartWithNerveGame";
         option = options.Find(itemToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + itemToAdd + ") returned null");
            option = new Option(itemToAdd, false);
         }
         if (true == option.IsEnabled)
            gi.AddSpecialItem(SpecialEnum.NerveGasBomb);
         //---------------------------------------------------------
         itemToAdd = "StartWithNecklass";
         option = options.Find(itemToAdd);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(" + itemToAdd + ") returned null");
            option = new Option(itemToAdd, false);
         }
         if (true == option.IsEnabled)
            gi.AddSpecialItem(SpecialEnum.ResurrectionNecklace);
         return true;
      }
      private bool AddStartingOptions(IGameInstance gi)
      {
         Utilities.MaxDays = 70;
         gi.PartyMembers.Clear(); // clear if this is run twice in game startup - happens when user choose fun option
         gi.PartyMembers.Add(gi.Prince);
         Options options = gi.Options;
         int numPartyMembers = 0;
         Option option = options.Find("PartyCustom");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingOptions(): myOptions.Find(PartyCustom) returned null");
            return false;
         }
         if (true == option.IsEnabled)  // If this is a custom party, choose based on checked boxes in options
         {
            for (int i = 0; i < Options.MEMBER_COUNT; i++)
            {
               string memberToAdd = Options.theDefaults[i];
               option = options.Find(memberToAdd);
               if (null == option)
               {
                  Logger.Log(LogEnum.LE_ERROR, "AddStartingOptions(): myOptions.Find(" + memberToAdd + ") returned null");
                  option = new Option(memberToAdd, false);
               }
               if (true == option.IsEnabled)
               {
                  if (false == AddStartingPartyMemberOption(gi, memberToAdd))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AddStartingOptions(): AddStartingPartyMemberOption() returned false");
                     option = new Option(memberToAdd, false);
                  }
               }
            }
         }
         else // check if random party
         {
            //-----------------------------------------------
            string itemToAdd1 = "RandomParty10";
            option = options.Find("RandomParty10");
            if (null == option)
            {
               Logger.Log(LogEnum.LE_ERROR, "AddStartingOptions(): myOptions.Find(RandomParty10) returned null");
               option = new Option(itemToAdd1, false);
            }
            if (true == option.IsEnabled)
               numPartyMembers = 10;
            //-----------------------------------------------
            itemToAdd1 = "RandomParty08";
            option = options.Find("RandomParty08");
            if (null == option)
            {
               Logger.Log(LogEnum.LE_ERROR, "AddStartingOptions(): myOptions.Find(RandomParty08) returned null");
               option = new Option(itemToAdd1, false);
            }
            if (true == option.IsEnabled)
               numPartyMembers = 8;
            //-----------------------------------------------
            itemToAdd1 = "RandomParty05";
            option = options.Find("RandomParty05");
            if (null == option)
            {
               Logger.Log(LogEnum.LE_ERROR, "AddStartingOptions(): myOptions.Find(RandomParty05) returned null");
               option = new Option(itemToAdd1, false);
            }
            if (true == option.IsEnabled)
               numPartyMembers = 5;
            //-----------------------------------------------
            itemToAdd1 = "RandomParty03";
            option = options.Find("RandomParty03");
            if (null == option)
            {
               Logger.Log(LogEnum.LE_ERROR, "AddStartingOptions(): myOptions.Find(RandomParty03) returned null");
               option = new Option(itemToAdd1, false);
            }
            if (true == option.IsEnabled)
               numPartyMembers = 3;
            //-----------------------------------------------
            itemToAdd1 = "RandomParty01";
            option = options.Find("RandomParty01");
            if (null == option)
            {
               Logger.Log(LogEnum.LE_ERROR, "AddStartingOptions(): myOptions.Find(RandomParty01) returned null");
               option = new Option(itemToAdd1, false);
            }
            if (true == option.IsEnabled)
               numPartyMembers = 1;
            //-----------------------------------------------
            if (0 < numPartyMembers)
            {
               for (int i = 0; i < numPartyMembers; ++i)
               {
                  int index = Utilities.RandomGenerator.Next(Options.MEMBER_COUNT);
                  string memberToAdd = Options.theDefaults[index];
                  if (false == AddStartingPartyMemberOption(gi, memberToAdd))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AddStartingOptions(): AddStartingPartyMemberOption(" + memberToAdd + ") returned false");
                     return false;
                  }
               }
            }
         }
         //---------------------------------------------------------
         string itemToAdd = "PartyMounted";
         option = options.Find("PartyMounted");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(PartyMounted) returned null");
            option = new Option(itemToAdd, false);
         }
         if (true == option.IsEnabled)
         {
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (false == mi.IsFlyer())
                  mi.AddNewMount();
            }
         }
         //---------------------------------------------------------
         itemToAdd = "PartyAirborne";
         option = options.Find("PartyAirborne");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(PartyAirborne) returned null");
            option = new Option(itemToAdd, false);
         }
         if (true == option.IsEnabled)
         {
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (false == mi.IsFlyer())
                  mi.AddNewMount(MountEnum.Pegasus);
            }
         }
         //---------------------------------------------------------
         itemToAdd = "ExtendEndTime";
         option = options.Find("ExtendEndTime");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMembers(): myOptions.Find(ExtendEndTime) returned null");
            option = new Option(itemToAdd, false);
         }
         if (true == option.IsEnabled)
            Utilities.MaxDays = 105;
         else
            Utilities.MaxDays = 70;
         return true;
      }
      private bool AddStartingPartyMemberOption(IGameInstance gi, string partyMemberName)
      {
         switch (partyMemberName)
         {
            case "Dwarf":
               {
                  IMapItem member = CreateCharacter(gi, "Dwarf");
                  member.IsFlying = true;
                  member.IsRiding = true;
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  member.IsFickle = true;
                  member.AddNewMount(); // riding
                  gi.AddCompanion(member);
               }
               break;
            case "Eagle":
               {
                  IMapItem member = CreateCharacter(gi, "Eagle");
                  member.IsFlying = true;
                  member.IsRiding = true;
                  member.IsGuide = true;
                  member.GuideTerritories = Territory.theTerritories;
                  gi.AddCompanion(member);
               }
               break;
            case "Elf":
               {
                  IMapItem member = CreateCharacter(gi, "Elf");
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  member.AddNewMount(MountEnum.Horse);
                  gi.AddCompanion(member);
               }
               break;
            case "ElfWarrior":
               {
                  IMapItem member = CreateCharacter(gi, "ElfWarrior");
                  member.AddNewMount(MountEnum.Horse);
                  gi.AddCompanion(member);
               }
               break;
            case "Falcon":
               {
                  IMapItem member = CreateCharacter(gi, "Falcon");
                  member.IsFlying = true;
                  member.IsRiding = true;
                  member.IsGuide = true;
                  member.GuideTerritories = Territory.theTerritories;
                  gi.AddCompanion(member);
                  gi.IsFalconFed = true;
               }
               break;
            case "Griffon":
               {
                  IMapItem griffon = CreateCharacter(gi, "Griffon");
                  griffon.IsFlying = true;
                  griffon.IsRiding = true;
                  gi.AddCompanion(griffon);
                  //---------------------
                  IMapItem rider = CreateCharacter(gi, "Mercenary");
                  griffon.Rider = rider;
                  rider.Mounts.Insert(0, griffon);
                  rider.Food = Utilities.RandomGenerator.Next(5);
                  rider.Coin = Utilities.RandomGenerator.Next(20);
                  rider.IsRiding = true;
                  rider.IsFlying = true;
                  gi.AddCompanion(rider);
               }
               break;
            case "Harpy":
               {
                  IMapItem harpy = CreateCharacter(gi, "Harpy");
                  harpy.IsFlying = true;
                  harpy.IsRiding = true;
                  gi.AddCompanion(harpy);
                  //---------------------
                  IMapItem rider = CreateCharacter(gi, "Monk");
                  harpy.Rider = rider;
                  rider.Mounts.Insert(0, harpy);
                  rider.Food = Utilities.RandomGenerator.Next(5);
                  rider.Coin = Utilities.RandomGenerator.Next(20);
                  rider.IsRiding = true;
                  rider.IsFlying = true;
                  gi.AddCompanion(rider);
               }
               break;
            case "Magician":
               {
                  IMapItem member = CreateCharacter(gi, "Magician");
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  gi.AddCompanion(member);
               }
               break;
            case "Mercenary":
               {
                  IMapItem member = CreateCharacter(gi, "Mercenary");
                  member.Food = 5;
                  member.Coin = 98;
                  member.AddNewMount();  // riding
                  member.IsGuide = true;
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  foreach (string adj in gi.Prince.TerritoryStarting.Adjacents)
                  {
                     ITerritory t = Territory.theTerritories.Find(adj);
                     member.GuideTerritories.Add(t);
                  }
                  gi.AddCompanion(member);
               }
               break;
            case "Merchant":
               {
                  IMapItem member = CreateCharacter(gi, "Merchant");
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  gi.AddCompanion(member);
                  gi.IsMerchantWithParty = true;
               }
               break;
            case "Minstrel":
               {
                  IMapItem member = CreateCharacter(gi, "Minstrel");
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  gi.AddCompanion(member);
                  gi.IsMinstrelPlaying = true; // e049 - minstrel
               }
               break;
            case "Monk":
               {
                  IMapItem member = CreateCharacter(gi, "Monk");
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  gi.AddCompanion(member);
               }
               break;
            case "PorterSlave":
               {
                  IMapItem member = CreateCharacter(gi, "PorterSlave");
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  gi.AddCompanion(member);
               }
               break;
            case "Priest":
               {
                  IMapItem member = CreateCharacter(gi, "Priest");
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  gi.AddCompanion(member);
               }
               break;
            case "TrueLove":
               {
                  IMapItem trueLove = CreateCharacter(gi, "TrueLovePriestDaughter");
                  trueLove.Food = Utilities.RandomGenerator.Next(5);
                  trueLove.Coin = Utilities.RandomGenerator.Next(20);
                  gi.AddCompanion(trueLove);
               }
               break;
            case "Wizard":
               {
                  IMapItem member = CreateCharacter(gi, "Wizard");
                  member.Food = Utilities.RandomGenerator.Next(5);
                  member.Coin = Utilities.RandomGenerator.Next(20);
                  gi.AddCompanion(member);
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "AddStartingPartyMemberOption(): reached default name=" + partyMemberName);
               return false;
         }
         return true;
      }
      private bool SetStartingLocationOption(IGameInstance gi, ref ITerritory starting)
      {
         Option option = null;
         string hex = "";
         //---------------------------------------------------------
         hex = "RandomHex";
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            int index = Utilities.RandomGenerator.Next(Territory.theTerritories.Count); // returns [0,count)
            starting = Territory.theTerritories[index];
            return true;
         }
         //---------------------------------------------------------
         hex = "RandomTown";
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            List<ITerritory> townHexes = new List<ITerritory>();
            foreach (ITerritory t in Territory.theTerritories)
            {
               if (true == t.IsTown)
                  townHexes.Add(t);
            }
            int index = Utilities.RandomGenerator.Next(townHexes.Count); // returns [0,count)
            starting = townHexes[index];
            return true;
         }
         //---------------------------------------------------------
         hex = "RandomLeft";
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            List<ITerritory> leftEdges = new List<ITerritory>();
            foreach (ITerritory t in Territory.theTerritories)
            {
               if ("offboard" != t.Name)
               {
                  int colNum = Int32.Parse(t.Name.Substring(0, 2)); // (start index, length)
                  if (01 == colNum)
                     leftEdges.Add(t);
               }
            }
            int index = Utilities.RandomGenerator.Next(leftEdges.Count); // returns [0,count)
            starting = leftEdges[index];
            return true;
         }
         //---------------------------------------------------------
         hex = "RandomRight";
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            List<ITerritory> rightEdges = new List<ITerritory>();
            foreach (ITerritory t in Territory.theTerritories)
            {
               if ("offboard" != t.Name)
               {
                  int colNum = Int32.Parse(t.Name.Substring(0, 2)); // (start index, length)
                  if (20 == colNum)
                     rightEdges.Add(t);
               }
            }
            int index = Utilities.RandomGenerator.Next(rightEdges.Count); // returns [0,count)
            starting = rightEdges[index];
            return true;
         }
         //---------------------------------------------------------
         hex = "RandomBottom";
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            List<ITerritory> bottomEdges = new List<ITerritory>();
            foreach (ITerritory t in Territory.theTerritories)
            {
               if ("offboard" != t.Name)
               {
                  int colNum = Int32.Parse(t.Name.Substring(0, 2)); // (start index, length)
                  int rowNum = Int32.Parse(t.Name.Substring(2));
                  if (((22 == rowNum) && (0 == colNum % 2)) || (23 == rowNum))
                     bottomEdges.Add(t);
               }
            }
            if (0 == bottomEdges.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): bottomEdges.Count=0");
               return false;
            }
            int index = Utilities.RandomGenerator.Next(bottomEdges.Count); // returns [0,count)
            starting = bottomEdges[index];
            return true;
         }
         //---------------------------------------------------------
         hex = "0109";  // Town
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "0206";  // Ruins
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "0708";  // River
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
         {
            starting = Territory.theTerritories.Find(hex);
            gi.RaftState = RaftEnum.RE_RAFT_SHOWN; // party has a raft
         }
         //---------------------------------------------------------
         hex = "0711";  // Temple
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "1212";  // Castle
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "0323"; // Castle
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "1923"; // Castle
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "0418";  // farmland
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "0722";  // CountrySide
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "0409";  // Forrest
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "0406";  // hills
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "1611";  // Mountains
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "0411";  // Swamp
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "1507";  // Desert
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "1905";  // Road
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         //---------------------------------------------------------
         hex = "1723"; // Bottom Board
         option = gi.Options.Find(hex);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocationOption(): myOptions.Find(" + hex + ") returned null");
            return false;
         }
         if (true == option.IsEnabled)
            starting = Territory.theTerritories.Find(hex);
         return true;
      }
      private void AddStartingTestingOptions(IGameInstance gi)
      {
         if( false == theIsGameSetup) //This function can be run twice if user selects fun option
         {
            theIsGameSetup = true;
            //gi.Prince.Territory = Territory.theTerritories.Find("1722"); // 
            //gi.Days = 40;
            //gi.Prince.SetWounds(6, 0); // 
            //gi.Prince.PlagueDustWound = 1; 
            //gi.Prince.IsResurrected = true;
            //gi.AddUnitTestTiredMount(myPrince);
            //gi.Prince.Coin = 501;
            //gi.Prince.Food = 9;
            //---------------------
         //   gi.AddSpecialItem(SpecialEnum.GiftOfCharm);
         //   gi.AddSpecialItem(SpecialEnum.ResistanceTalisman);
         //   gi.AddSpecialItem(SpecialEnum.CharismaTalisman);
         //   gi.AddSpecialItem(SpecialEnum.DragonEye);
         //   gi.AddSpecialItem(SpecialEnum.RocBeak);
         //   gi.AddSpecialItem(SpecialEnum.GriffonClaws);
         //   gi.Prince.AddSpecialItemToShare(SpecialEnum.Foulbane);
         //   gi.AddSpecialItem(SpecialEnum.HealingPoition);
         //   gi.AddSpecialItem(SpecialEnum.CurePoisonVial);
         //   gi.AddSpecialItem(SpecialEnum.EnduranceSash);
         //   gi.AddSpecialItem(SpecialEnum.PoisonDrug);
         //   gi.AddSpecialItem(SpecialEnum.MagicSword);
         //   gi.AddSpecialItem(SpecialEnum.AntiPoisonAmulet);
         //   gi.AddSpecialItem(SpecialEnum.PegasusMountTalisman);
         //   gi.AddSpecialItem(SpecialEnum.NerveGasBomb);
         //   gi.AddSpecialItem(SpecialEnum.ResistanceRing);
         //   gi.AddSpecialItem(SpecialEnum.ResurrectionNecklace);
         //   gi.AddSpecialItem(SpecialEnum.ShieldOfLight);
         //   gi.AddSpecialItem(SpecialEnum.RoyalHelmOfNorthlands);
         //   gi.Prince.AddSpecialItemToShare(SpecialEnum.MagicBox);
         //   gi.Prince.AddSpecialItemToShare(SpecialEnum.HydraTeeth);
         //   //---------------------
         //   gi.HydraTeethCount = 5;
         //   gi.Prince.AddSpecialItemToShare(SpecialEnum.StaffOfCommand);
         //   ITerritory visited = Territory.theTerritories.Find("0109");
         //   gi.VisitedLocations.Add(visited);
         //   ITerritory escapeLocation = Territory.theTerritories.Find("0605");
         //   gi.EscapedLocations.Add(escapeLocation);
         //   ITerritory cacheHex = Territory.theTerritories.Find("0504");
         //   gi.Caches.Add(cacheHex, 66);
         //   cacheHex = Territory.theTerritories.Find("0505");
         //   gi.Caches.Add(cacheHex, 333);
         //   gi.Caches.Add(cacheHex, 100);
         //   gi.Caches.Add(cacheHex, 500);
         //   gi.Caches.Add(cacheHex, 33);
         //   //---------------------
         //   ITerritory secretClueHex = Territory.theTerritories.Find("0507");
         //   gi.SecretClues.Add(secretClueHex);
         //   ITerritory secretClueHex2 = Territory.theTerritories.Find("0406");
         //   gi.SecretClues.Add(secretClueHex2);
         //   ////---------------------
         //   ITerritory hiddenTemple = Territory.theTerritories.Find("0605");
         //   gi.HiddenTemples.Add(hiddenTemple);
         //   ITerritory hiddenRuin = Territory.theTerritories.Find("0606");
         //   gi.HiddenRuins.Add(hiddenRuin);
         //   ////---------------------
         //   ITerritory elfTown = Territory.theTerritories.Find("0607");
         //   gi.ElfTowns.Add(elfTown);
         //   ITerritory eagleLair = Territory.theTerritories.Find("1507");
         //   gi.EagleLairs.Add(eagleLair);
         //   ITerritory dwarvenMine = Territory.theTerritories.Find("0408");
         //   gi.DwarvenMines.Add(dwarvenMine);
         //   ITerritory dwarfAdviceHex = Territory.theTerritories.Find("0319");
         //   gi.DwarfAdviceLocations.Add(dwarfAdviceHex);
         //   ITerritory halflingTown = Territory.theTerritories.Find("0303");
         //   gi.HalflingTowns.Add(halflingTown);
         //   ITerritory elfCastle = Territory.theTerritories.Find("0608");
         //   gi.ElfCastles.Add(elfCastle);
         //   ////---------------------
         //   ITerritory wizardTower = Territory.theTerritories.Find("0404");  //mountain
         //   gi.WizardTowers.Add(wizardTower);
         //   ITerritory wizardAdviceHex = Territory.theTerritories.Find("1005");
         //   gi.WizardAdviceLocations.Add(wizardAdviceHex);
         //   ITerritory wizardAdviceHex2 = Territory.theTerritories.Find("0406");
         //   gi.WizardAdviceLocations.Add(wizardAdviceHex2);
         //   //---------------------
         //   ITerritory pixieAdviceHex = Territory.theTerritories.Find("0406");
         //   gi.PixieAdviceLocations.Add(pixieAdviceHex);
         //   //---------------------
         //   ITerritory t11 = Territory.theTerritories.Find("0306"); // e114 - verify that eagle hunt can happen in structure
         //   gi.HiddenTemples.Add(t11);
         //   t11 = Territory.theTerritories.Find("0307"); // e114 - verify that eagle hunt can happen in structure
         //   gi.HiddenTemples.Add(t11);
         //   t11 = Territory.theTerritories.Find("1507"); // e114 - verify that eagle hunt can happen in structure
         //   gi.HiddenTemples.Add(t11);
         //   t11 = Territory.theTerritories.Find("0405"); // e114 - verify that eagle hunt can happen in structure
         //   gi.HiddenTemples.Add(t11);
         //   t11 = Territory.theTerritories.Find("0406"); // e114 - verify that eagle hunt can happen in structure
         //   gi.HiddenTemples.Add(t11);
         //   t11 = Territory.theTerritories.Find("0506"); // e114 - verify that eagle hunt can happen in structure
         //   gi.HiddenTemples.Add(t11);
         //   t11 = Territory.theTerritories.Find("0507"); // e114 - verify that eagle hunt can happen in structure
         //   gi.HiddenTemples.Add(t11);
         //   gi.Purifications.Add(t11);
         //   //---------------------
         //   ITerritory forbiddenHex = Territory.theTerritories.Find("0705");
         //   gi.ForbiddenHexes.Add(forbiddenHex);
         //   //---------------------
         //   ITerritory purificationHex = Territory.theTerritories.Find("1805");
         //   gi.ForbiddenAudiences.AddPurifyConstaint(purificationHex);
         //   //---------------------
         //   ITerritory forbiddenAudienceOffering= Territory.theTerritories.Find("1021");
         //   gi.ForbiddenAudiences.AddOfferingConstaint(forbiddenAudienceOffering, Utilities.FOREVER);
         //   //---------------------
         //   ITerritory forbiddenAudience = Territory.theTerritories.Find("0101");
         //   ITerritory lt1 = Territory.theTerritories.Find("0109");
         //   ITerritory lt2 = Territory.theTerritories.Find("0711");
         //   ITerritory lt3 = Territory.theTerritories.Find("1212");
         //   gi.LetterOfRecommendations.Add(lt1);
         //   gi.LetterOfRecommendations.Add(lt1);
         //   gi.ForbiddenAudiences.AddLetterConstraint(forbiddenAudience, lt1);
         //   gi.LetterOfRecommendations.Add(lt2);
         //   gi.ForbiddenAudiences.AddLetterConstraint(forbiddenAudience, lt2);
         //   gi.LetterOfRecommendations.Add(lt3);
         //   gi.ForbiddenAudiences.AddLetterConstraint(forbiddenAudience, lt3);
         //   //---------------------
         //   ITerritory forbiddenAudienceAssistant = Territory.theTerritories.Find("0216");
         //   IMapItem trustedAssistant = CreateCharacter(gi, "TrustedAssistant");
         //   gi.AddCompanion(trustedAssistant);
         //   gi.ForbiddenAudiences.AddAssistantConstraint(forbiddenAudienceAssistant, trustedAssistant);
         //   ITerritory lt4 = Territory.theTerritories.Find("1212");
         //   gi.ForbiddenAudiences.UpdateLetterLocation(lt4); // need to assign target territory after construction
         //   //---------------------
         //   ITerritory forbiddenAudienceTime = Territory.theTerritories.Find("0323");
         //   gi.ForbiddenAudiences.AddTimeConstraint(forbiddenAudienceTime, 10);
         //   //---------------------
         //   ITerritory forbiddenAudienceClothes= Territory.theTerritories.Find("0109");
         //   gi.ForbiddenAudiences.AddClothesConstraint(forbiddenAudienceClothes);
         //   //---------------------
         //   ITerritory forbiddenAudienceReligion = Territory.theTerritories.Find("1004");
         //   gi.ForbiddenAudiences.AddReligiousConstraint(forbiddenAudienceReligion);
         //   //---------------------
         //   ITerritory forbiddenAudienceKills = Territory.theTerritories.Find("0323");
         //   gi.ForbiddenAudiences.AddMonsterKillConstraint(forbiddenAudienceKills);
         //   //---------------------
         //   gi.IsArchTravelKnown = true;
         //   ITerritory arch1 = Territory.theTerritories.Find("0418");  //
         //   gi.Arches.Add(arch1); // AddStartingTestingOptions()
         //   ITerritory arch2 = Territory.theTerritories.Find("0517");
         //   gi.Arches.Add(arch2); // AddStartingTestingOptions()
         //   //---------------------
         //   gi.DayOfLastOffering = gi.Days - 4;
         //   gi.IsSecretTempleKnown = true;
         //   gi.ChagaDrugCount = 2;
         //   gi.IsMarkOfCain = true; // e018
         //   gi.NumMonsterKill = 5; // e161e - kill 5 monsters
         //   //---------------------
         //   gi.IsSecretBaronHuldra = true; // e144
         //   gi.IsSecretLadyAeravir = true; // e145
         //   gi.IsSecretCountDrogat = true; // e146
         //   IMapItem trueHeir = CreateCharacter(gi, "WarriorBoy");
         //   gi.AddCompanion(trueHeir);
         //   //---------------------
         //   foreach (IMapItem mi11 in gi.PartyMembers)
         //      mi11.AddSpecialItemToKeep(SpecialEnum.ResurrectionNecklace);
         //   //---------------------
         //   IMapItem mi111 = this.CreateCharacter(gi, "Porter");
         //   mi111.PlagueDustWound = 2;
         //   gi.AddCompanion(mi111);
         //   //---------------------
         //   GameEngine.theFeatsInGame.myIsEagleAdded = true;
         //   GameEngine.theFeatsInGame.myIsPurchaseFoulbane = true;
         //   GameEngine.theFeatsInGame.myIsRescueHeir = true;
         //   //---------------------
         //   foreach (IMapItem partyMember in gi.PartyMembers)
         //   {
         //      foreach (IMapItem mount in partyMember.Mounts)
         //      {
         //         if ((true == mount.Name.Contains("Griffon")) || (true == mount.Name.Contains("Harpy")))
         //            continue;
         //         gi.AtRiskMounts.Add(mount);
         //      }
         //   }
         }
      }
      private void PrintDiagnosticInfoToLog()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("\n\tGameVersion=");
         Version version = Assembly.GetExecutingAssembly().GetName().Version;
         sb.Append(version.ToString());
         sb.Append("\n\tOsVersion=");
         sb.Append(System.Environment.OSVersion.Version.Build.ToString());
         sb.Append("\n\tOS Desc=");
         sb.Append(RuntimeInformation.OSDescription.ToString());
         sb.Append("\n\tOS Arch=");
         sb.Append(RuntimeInformation.OSArchitecture.ToString());
         sb.Append("\n\tProcessorArch=");
         sb.Append(RuntimeInformation.ProcessArchitecture.ToString());
         sb.Append("\n\tnetVersion=");
         sb.Append(Environment.Version.ToString());
         uint dpiX = 0;
         uint dpiY = 0;
         ScreenExtensions.GetDpi(System.Windows.Forms.Screen.PrimaryScreen, ScreenExtensions.DpiType.Effective, out dpiX, out dpiY);
         sb.Append("\n\tDPI=(");
         sb.Append(dpiX.ToString());
         sb.Append(",");
         sb.Append(dpiY.ToString());
         sb.Append(")\n\tAppDir=");
         sb.Append(MainWindow.theAssemblyDirectory);
         Logger.Log(LogEnum.LE_GAME_INIT_VERSION, sb.ToString());
      }
   }
   //-----------------------------------------------------
   class GameStateSunriseChoice : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         if (false == gi.IsNewDayChoiceMade)
         {
            if (false == ResetDayAfterChoice(gi))
            {
               returnStatus = "ResetDayAfterChoice() returned error";
               Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               return returnStatus;
            }
         }
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateUndo:
               UndoCommand(ref gi, ref action);
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E079HeavyRainsStartDayCheck:
               gi.IsAirborne = false;
               gi.IsAirborneEnd = false;
               gi.IsShortHop = false;
               //----------------------------
               gi.SunriseChoice = GamePhase.Travel;
               gi.GamePhase = GamePhase.Encounter;
               gi.EventAfterRedistribute = gi.EventDisplayed = gi.EventActive = "e079b";
               gi.IsHeavyRainNextDay = true;
               break;
            case GameAction.E079HeavyRainsStartDayCheckInAir:
               gi.IsAirborne = true;
               gi.IsAirborneEnd = false;
               gi.IsShortHop = false;
               //----------------------------
               gi.SunriseChoice = GamePhase.Travel;
               gi.GamePhase = GamePhase.Encounter;
               gi.EventAfterRedistribute = gi.EventDisplayed = gi.EventActive = "e079b";
               gi.IsHeavyRainNextDay = true;
               break;
            case GameAction.E079HeavyRainsDismount:
               gi.IsAirborne = false;
               gi.IsHeavyRainDismount = true;
               foreach (IMapItem mi in gi.PartyMembers)
               {
                  if (null != mi.Rider) // mi = griffon/harpy
                  {
                     mi.Rider.Mounts.Remove(mi);  // Griffon/Harpy Rider removed as mount
                     mi.Rider = null;
                  }
                  if (true == mi.IsFlyer()) // flyers do not need to dismount
                     continue;
                  mi.IsRiding = false;
                  mi.IsFlying = false;
               }
               action = GameAction.UpdateEventViewerDisplay;
               break;
            case GameAction.E144RescueHeir:
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.EventDisplayed = gi.EventActive = "e144a"; // next screen to show
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.E144SneakAttack:
               GameEngine.theFeatsInGame.myIsSneakAttack = true;
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.EventDisplayed = gi.EventActive = "e144i"; // next screen to show
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.E146StealGems:
               GameEngine.theFeatsInGame.myIsStealGems = true;
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.EventDisplayed = gi.EventActive = "e146a"; // next screen to show
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.RestEncounterCheck:
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = gi.GamePhase = GamePhase.Rest;
               if (true == gi.IsInStructure(princeTerritory))
               {
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     if (0 < mi.Wound)
                     {
                        if (true == mi.Name.Contains("Prince"))
                           gi.Statistic.myNumOfPrinceHeal++;
                        else
                           gi.Statistic.myNumOfPartyHeal++;
                        mi.HealWounds(1, 0); // RestEncounterCheck - InStructure()=true - Resting cures one wound
                        gi.UndoHeal.Add(mi.Name);
                     }
                     if( true == mi.IsExhausted)
                     {
                        mi.IsExhausted = false;
                        gi.UndoExhaust.Add(mi.Name);
                     }
                  }
                  if (false == SetHuntState(gi, ref action)) // Resting in same hex
                  {
                     returnStatus = "SetHuntState() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
                  }
               }
               else // might have a travel encounter
               {
                  action = GameAction.TravelLostCheck;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateSunriseChoice.PerformAction(RestEncounterCheck): gi.MapItemMoves.Clear()");
                  gi.MapItemMoves.Clear();
                  MapItemMove mim = new MapItemMove(Territory.theTerritories, gi.Prince, princeTerritory);   // Travel to same hex if rest encounter
                  if ((0 == mim.BestPath.Territories.Count) || (null == mim.NewTerritory))
                  {
                     returnStatus = "Unable to Find Path";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
                  }
                  gi.MapItemMoves.Add(mim);
                  Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateSunriseChoice.PerformAction(RestEncounterCheck): oT=" + princeTerritory.Name + " nT=" + mim.NewTerritory.Name);
               }
               break;
            case GameAction.Travel:
               Logger.Log(LogEnum.LE_UNDO_COMMAND, "GameStateSunriseChoice.PerformAction(): cmd=" + gi.IsUndoCommandAvailable.ToString() + "-->true  a=" + action.ToString());
               gi.IsUndoCommandAvailable = true;
               TravelAction(gi, ref action); // GameAction.Travel
               break;
            case GameAction.TravelAir:
               gi.IsAirborne = true;
               gi.IsAirborneEnd = false;
               gi.IsShortHop = false;
               gi.RaftStatePrevUndo = gi.RaftState;
               gi.RaftState = RaftEnum.RE_NO_RAFT;    // e122
               Logger.Log(LogEnum.LE_UNDO_COMMAND, "GameStateSunriseChoice.PerformAction(): cmd=" + gi.IsUndoCommandAvailable.ToString() + "-->true  a=" + action.ToString());
               gi.IsUndoCommandAvailable = true;
               if (true == gi.PartyReadyToFly()) // mount to fly returns false if anybody is left or possessions are left
               {
                  TravelAction(gi, ref action); // GameAction.TravelAir
               }
               else
               {
                  gi.EventAfterRedistribute = "";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  action = GameAction.TravelAirRedistribute;
               }
               break;
            case GameAction.TransportRedistributeEnd:
               Logger.Log(LogEnum.LE_UNDO_COMMAND, "GameStateSunriseChoice.PerformAction(): cmd=" + gi.IsUndoCommandAvailable.ToString() + "-->false for a=" + action.ToString());
               gi.IsUndoCommandAvailable = false; // cannot undo after redistribute happens
               gi.UndoHeal.Clear();
               gi.UndoExhaust.Clear();
               if (false == gi.PartyReadyToFly()) // mount to fly returns false if anybody is left or possessions are left
               {
                  gi.IsAirborne = false;
                  returnStatus = "gi.PartyReadyToFly() returned false for " + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               }
               else
               {
                  TravelAction(gi, ref action); // GameAction.TransportRedistributeEnd
               }
               break;
            case GameAction.SeekNews:
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.EventDisplayed = gi.EventActive = "e209"; // next screen to show
               gi.SunriseChoice = gi.GamePhase = GamePhase.SeekNews;
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.SeekHire:
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.SeekHire;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_HIRE));
               gi.EventDisplayed = gi.EventActive = "e210"; // next screen to show
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case GameAction.SeekAudience:
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.SeekAudience;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_AUDIENCE));
               if (true == gi.IsInTown(princeTerritory))
               {
                  gi.EventDisplayed = gi.EventActive = "e211a";
               }
               else if (true == gi.IsInTemple(princeTerritory))
               {
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211b";
                  if (0 < gi.ChagaDrugCount)
                     gi.EventDisplayed = gi.EventActive = "e143a";
               }
               else if (true == gi.IsInCastle(princeTerritory))
               {
                  if ("1212" == princeTerritory.Name)
                  {
                     if (true == gi.IsInMapItems("WarriorBoy"))
                        gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211g";
                     else
                        gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211c";
                  }
                  else if ("0323" == princeTerritory.Name)
                  {
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211d";
                  }
                  else if ("1923" == princeTerritory.Name)
                  {
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211e";
                  }
                  else if (true == gi.DwarvenMines.Contains(princeTerritory))
                  {
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211f";
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e211a"; // treat audience in other castles as town audience
                  }
               }
               else
               {
                  returnStatus = "Reached Default ae=" + action.ToString() + " t=" + princeTerritory.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               }
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case GameAction.SeekOffering:
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.SeekOffering;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_OFFERING));
               gi.ReduceCoins("SeekOffering", 1); // must spend one gold to make offering
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e212";
               if (0 < gi.ChagaDrugCount)
                  gi.EventDisplayed = gi.EventActive = "e143a";
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case GameAction.SearchRuins:
               gi.ProcessIncapacitedPartyMembers("e134");
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.SearchRuins;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_SEARCH_RUINS));
               if (true == gi.RuinsUnstable.Contains(princeTerritory)) // once a ruins is discovered to be unstable, it is always unstable
               {
                  gi.EventDisplayed = gi.EventActive = "e134";
                  gi.GamePhase = GamePhase.Encounter;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e208";
                  gi.GamePhase = GamePhase.Encounter;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               break;
            case GameAction.SearchCacheCheck:
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = gi.GamePhase = GamePhase.SearchCache;
               action = GameAction.TravelLostCheck;
               gi.DieRollAction = GameAction.DieRollActionNone;
               Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateSunriseChoice.PerformAction(SearchCacheCheck): gi.MapItemMoves.Clear()");
               gi.MapItemMoves.Clear();
               MapItemMove mimSearch = new MapItemMove(Territory.theTerritories, gi.Prince, princeTerritory);   // Travel to same hex if search cache encounter
               if ((0 == mimSearch.BestPath.Territories.Count) || (null == mimSearch.NewTerritory))
               {
                  returnStatus = "Unable to Find Path";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               }
               gi.MapItemMoves.Add(mimSearch);
               Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateSunriseChoice.PerformAction(SearchCacheCheck): oT=" + princeTerritory.Name + " nT=" + mimSearch.NewTerritory.Name);
               break;
            case GameAction.SearchTreasure:
               ResetDayForNonTravelChoice(gi, action);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = gi.GamePhase = GamePhase.SearchTreasure;
               action = GameAction.TravelLostCheck;
               gi.DieRollAction = GameAction.DieRollActionNone;
               Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateSunriseChoice.PerformAction(SearchTreasure): gi.MapItemMoves.Clear()");
               gi.MapItemMoves.Clear();
               MapItemMove mimSearch1 = new MapItemMove(Territory.theTerritories, gi.Prince, princeTerritory);   // Travel to same hex if search treasure
               if ((0 == mimSearch1.BestPath.Territories.Count) || (null == mimSearch1.NewTerritory))
               {
                  returnStatus = "Unable to Find Path";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               }
               gi.MapItemMoves.Add(mimSearch1);
               Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateSunriseChoice.PerformAction(SearchTreasure): oT=" + princeTerritory.Name + " nT=" + mimSearch1.NewTerritory.Name);
               break;
            case GameAction.ArchTravel:
               ResetDayForNonTravelChoice(gi, action);
               gi.Statistic.myNumDaysArchTravel++;
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.Travel;
               gi.GamePhase = GamePhase.Encounter;
               gi.EventDisplayed = gi.EventActive = "e045b";
               gi.DieRollAction = GameAction.EncounterRoll;
               action = GameAction.UpdateEventViewerActive;
               break;
            case GameAction.EncounterFollow:
               Logger.Log(LogEnum.LE_UNDO_COMMAND, "GameStateSunriseChoice.PerformAction(): cmd=" + gi.IsUndoCommandAvailable.ToString() + "-->true for a=" + action.ToString());
               gi.IsUndoCommandAvailable = true;
               gi.IsBadGoing = false;                 // e078
               gi.IsHeavyRain = false;                // e079
               gi.IsFloodContinue = false;            // e092
               gi.IsEagleHunt = false;                // e114
               gi.RaftStatePrevUndo = gi.RaftState;
               gi.RaftState = RaftEnum.RE_NO_RAFT;    // e122
               gi.IsCavalryEscort = false;            // e151
               if (false == EncounterFollow(gi, ref action)) // GameStateSunriseChoice
               {
                  returnStatus = "EncounterFollow() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               }
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e072d"; // follow elven band
               gi.DieRollAction = GameAction.EncounterRoll;
               //-------------------------------------------
               gi.EncounteredMembers.Clear();
               for (int i = 0; i < gi.NumMembersBeingFollowed; ++i) // if following elves, repopulate EncounterMembers container
               {
                  IMapItem elf = CreateCharacter(gi, "Elf");
                  gi.EncounteredMembers.Add(elf);
               }
               break;
            default:
               returnStatus = "Reached Default ERROR with a=" + action.ToString();
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
         {
            sb12.Append("<<<<ERROR2::::::GameStateSunriseChoice.PerformAction(): ");
            sb12.Append(returnStatus);
            sb12.Append("  ???  ");
         }
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      protected bool ResetDayAfterChoice(IGameInstance gi)
      {
         gi.IsNewDayChoiceMade = true;
         theCombatModifer = 0;
         theConstableRollModifier = 0;
         //----------------------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "ResetDayAfterChoice(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         Logger.Log(LogEnum.LE_MOVE_COUNT, "ResetDayAfterChoice(): MovementUsed=0");
         foreach (IMapItem mi in gi.PartyMembers)
            mi.MovementUsed = 0;
         //----------------------------------------------
         gi.EncounteredMembers.Clear();
         gi.LostPartyMembers.Clear();
         gi.TargetHex = null;
         gi.ActiveMember = null;               // TreausreTable lookup values
         gi.CapturedWealthCodes.Clear();
         gi.PegasusTreasure = PegasusTreasureEnum.Mount;
         gi.Bribe = 0;
         Logger.Log(LogEnum.LE_BRIBE, "ResetDayAfterChoice(): bribe=" + gi.Bribe.ToString());
         gi.FickleCoin = 0;
         gi.LooterCoin = 0;
         //----------------------------------------------
         gi.IsGuardEncounteredThisTurn = false; // e002
         gi.DwarvenChoice = "";                 // e006
         gi.IsDwarvenBandSizeSet = false;       // e006a
         gi.IsElfWitAndWileActive = false;      // e007
         gi.IsElvenBandSizeSet = false;         // e007a
         gi.PurchasedFood = 0;                  // e012
         gi.IsReaverClanFight = false;          // e014b
         gi.IsReaverClanTrade = false;          // e015a
         gi.PurchasedMount = 0;                 // e015b
         gi.IsMagicianProvideGift = false;      // e016a
         gi.IsHuntedToday = false;              // e017, e049, or e050
         gi.MonkPleadModifier = 0;              // e019a
         gi.GuardianCount = 0;                  // e046
         gi.IsTempleGuardModifer = false;       // e066
         gi.IsElfTalkActive = true;                // e072
         gi.IsWolvesAttack = false;             // e075
         gi.IsHeavyRainNextDay = false;         // e079
         gi.EventAfterRedistribute = "";        // e086
         gi.IsMountsAtRisk = false;             // e095
         gi.PurchasedPotionCure = 0;            // e128b
         gi.PurchasedPotionHeal = 0;            // e128e
         gi.IsArrestedByDrogat = false;         // e130d
         gi.IsLadyAeravirRerollActive = false;  // e145
         gi.IsFoulBaneUsedThisTurn = false;     // e146
         gi.SeneschalRollModifier = 0;          // e148
         gi.DaughterRollModifier = 0;           // e154
         gi.IsPartyFed = false;                 // e156
         gi.IsMountsFed = false;                // e156
         gi.IsPartyLodged = false;              // e156
         gi.IsMountsStabled = false;            // e156
         gi.PurchasedSlavePorter = 0;           // e163
         gi.PurchasedSlaveWarrior = 0;          // e163
         gi.PurchasedSlaveGirl = 0;             // e163
         gi.IsSlaveGirlActive = false;          // e163
         gi.IsGiftCharmActive = false;          // e182
         gi.IsPegasusSkip = false;              // e188  
         gi.IsCharismaTalismanActive = false;   // e189
         gi.PurchasedHenchman = 0;              // e210f
         gi.PurchasedPorter = 0;                // e210i 
         gi.PurchasedGuide = 0;                 // e210i 
         gi.IsChagaDrugProvided = false;        // e211b
         gi.IsMagicUserDismissed = false;       // e211c
         gi.IsAssassination = false;            // e341
         gi.IsDayEnd = false;                   // e341
         gi.IsEvadeActive = true;
         gi.IsGridActive = false;  // Can show active event button in status bar
         try
         {
            Logger.Log(LogEnum.LE_RESET_ROLL_STATE, "ResetDayAfterChoice(): resetting die rolls");
            foreach (KeyValuePair<string, int[]> kvp in gi.DieResults)
            {
               for (int i = 0; i < 3; ++i)
                  kvp.Value[i] = Utilities.NO_RESULT;
            }
         }
         catch (Exception)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResetDayAfterChoice(): reset rolls");
            return false;
         }
         return true;
      } // Reset all variables after user chooses the acton for today
      protected void ResetDayForNonTravelChoice(IGameInstance gi, GameAction action)
      {
         Logger.Log(LogEnum.LE_UNDO_COMMAND, "ResetDayForNonTravelChoice(): cmd=" + gi.IsUndoCommandAvailable.ToString() + "-->true  a=" + action.ToString());
         gi.IsUndoCommandAvailable = true;
         gi.NumMembersBeingFollowed = 0;
         gi.IsAirborne = false;
         gi.IsWoundedWarriorRest = false;          // e069
         gi.IsTrainHorse = false;                  // e077
         gi.IsBadGoing = false;                    // e078
         gi.IsHeavyRainContinue = false;           // e079
         gi.IsFloodContinue = false;               // e092
         gi.AtRiskMounts.Clear();                  // e095
         gi.IsEagleHunt = false;                   // e114
         gi.RaftStatePrevUndo = gi.RaftState;
         gi.RaftState = RaftEnum.RE_NO_RAFT;       // e122
         gi.IsWoundedBlackKnightRest = false;      // e123
         gi.IsCavalryEscort = false;               // e151
         foreach (IMapItem mi in gi.PartyMembers)
            mi.IsCatchCold = false;
      }
   }
   //-----------------------------------------------------
   class GameStateHunt : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         if (false == PerformEndCheck(gi, ref action)) // GameStateHunt.PerformAction()
         {
            switch (action)
            {
               case GameAction.ShowInventory:
               case GameAction.ShowGameFeats:
               case GameAction.ShowAllRivers:
               case GameAction.ShowRuleListing:
               case GameAction.ShowCharacterDescription:
               case GameAction.ShowEventListing:
               case GameAction.ShowPartyPath:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.UpdateEventViewerDisplay:
                  break;
               case GameAction.UpdateEventViewerActive:
                  gi.EventDisplayed = gi.EventActive;
                  break;
               case GameAction.UpdateUndo:
                  UndoCommand(ref gi, ref action);
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               case GameAction.UpdateGameOptions:
                  break;
               case GameAction.UpdateLoadingGame:
                  if (false == LoadGame(ref gi, ref action))
                  {
                     returnStatus = "LoadGame() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateHunt.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.Hunt:
                  break;
               case GameAction.HuntPeasantMobPursuit:
                  if (false == gi.IsHuntedToday)
                  {
                     gi.IsHuntedToday = true;
                     gi.EventDisplayed = gi.EventActive = "e017"; // next screen to show
                     gi.GamePhase = GamePhase.Encounter;
                     gi.DieRollAction = GameAction.EncounterStart;
                  }
                  else
                  {
                     gi.DieResults["e017"][0] = Utilities.NO_RESULT;
                     if (false == SetEndOfDayState(gi, ref action)) // Hunting End of Day Check
                     {
                        returnStatus = "SetEndOfDayState() return false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateHunt.PerformAction(HuntEndOfDayCheck): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.HuntConstabularyPursuit:
                  if (false == gi.IsHuntedToday)
                  {
                     gi.IsHuntedToday = true;
                     gi.EventDisplayed = gi.EventActive = "e050"; // next screen to show
                     gi.GamePhase = GamePhase.Encounter;
                     gi.DieRollAction = GameAction.EncounterStart;
                  }
                  else
                  {
                     for (int i = 0; i < 3; ++i)
                        gi.DieResults["e050"][i] = Utilities.NO_RESULT;
                     if (false == SetEndOfDayState(gi, ref action)) // Hunting End of Day Check
                     {
                        returnStatus = "SetEndOfDayState() return false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateHunt.PerformAction(HuntEndOfDayCheck): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.HuntEndOfDayCheck:
                  if (false == SetEndOfDayState(gi, ref action)) // Hunting End of Day Check
                  {
                     returnStatus = "SetEndOfDayState() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateHunt.PerformAction(HuntEndOfDayCheck): " + returnStatus);
                  }
                  break;
               case GameAction.HuntE002aEncounterRoll:
                  int encounterResult = dieRoll - 3;
                  if ("0101" == gi.Prince.Territory.Name)
                     ++encounterResult;
                  if ("1501" == gi.Prince.Territory.Name)
                     ++encounterResult;
                  if (0 < encounterResult)
                  {
                     --gi.Prince.MovementUsed;
                     if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
                     {
                        returnStatus = " AddMapItemMove() return false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateTravel.PerformAction(HuntE002aEncounterRoll): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                     }
                     ++gi.Prince.MovementUsed;
                     gi.EventDisplayed = gi.EventActive = "e002a"; // next screen to show
                     gi.GamePhase = GamePhase.Encounter;
                     gi.DieRollAction = GameAction.EncounterStart;
                  }
                  else
                  {
                     if (false == SetEndOfDayState(gi, ref action)) // Did not encounter Mercenaries North of Tragoth
                     {
                        returnStatus = "SetEndOfDayState() return false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateHunt.PerformAction(HuntE002aEncounterRoll): " + returnStatus);
                     }
                  }
                  break;
               default:
                  returnStatus = "Reached Default ERROR";
                  break;
            }
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateHunt.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateCampfire : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateCampfire.PerformAction(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         if (false == PerformEndCheck(gi, ref action)) // GameStateCampfire.PerformAction()
         {
            switch (action)
            {
               case GameAction.ShowInventory:
               case GameAction.ShowGameFeats:
               case GameAction.ShowAllRivers:
               case GameAction.ShowRuleListing:
               case GameAction.ShowCharacterDescription:
               case GameAction.ShowEventListing:
               case GameAction.ShowPartyPath:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.UpdateEventViewerDisplay:
                  break;
               case GameAction.UpdateEventViewerActive:
                  gi.EventDisplayed = gi.EventActive;
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               case GameAction.UpdateGameOptions:
                  break;
               case GameAction.UpdateUndo:
                  UndoCommand(ref gi, ref action);
                  break;
               case GameAction.UpdateLoadingGame:
                  if (false == LoadGame(ref gi, ref action))
                  {
                     returnStatus = "LoadGame() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.UpdateLoadingGameReturnToJail:
                  action = GameAction.CampfireWakeup;
                  gi.EventDisplayed = gi.EventActive = "e203a";
                  gi.DieRollAction = GameAction.E203NightInPrison;
                  break;
               case GameAction.CampfirePlagueDustEnd:
                  gi.ProcessIncapacitedPartyMembers("Plague Dust");
                  gi.IsGridActive = false;   // GameAction.CampfirePlagueDustEnd
                  if (false == SetTalismanCheckState(gi, ref action))
                  {
                     returnStatus = "SetTalismanCheckState() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.CampfireTalismanDestroyEnd:
                  if (false == SetMountDieCheckState(gi, ref action))
                  {
                     returnStatus = "SetMountDieCheckState() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.CampfireMountDieCheckEnd:
                  if (false == SetCampfireEncounterState(gi, ref action))
                  {
                     returnStatus = "SetCampfireEncounterState() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E203NightEnslaved:
                  if (false == EncounterEscape(gi, ref action)) // use this function to randomly move one hex direction
                  {
                     returnStatus = "EncounterEscape() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  gi.DieResults["e203e"][0] = dieRoll;
                  if (false == PerformJailBreak(gi, ref action, dieRoll))
                  {
                     returnStatus = "PerformJailBreak() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E203EscapeEnslaved:
                  if (false == EncounterEscape(gi, ref action))
                  {
                     returnStatus = "EncounterEscape() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  if (false == SetHuntState(gi, ref action)) // Escape from Dungeion
                  {
                     returnStatus = "SetHuntState() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E203NightInDungeon:
                  gi.DieResults["e203c"][0] = dieRoll;
                  if (false == PerformJailBreak(gi, ref action, dieRoll))
                  {
                     returnStatus = "PerformJailBreak() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E203EscapeFromDungeon:
                  if (false == EncounterEscape(gi, ref action))
                  {
                     returnStatus = "EncounterEscape() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  if (false == SetHuntState(gi, ref action)) // Escape from Dungeion
                  {
                     returnStatus = "SetHuntState() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E203NightInPrison:
                  gi.DieResults["e203a"][0] = dieRoll;
                  if (false == PerformJailBreak(gi, ref action, dieRoll))
                  {
                     returnStatus = "PerformJailBreak() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E203EscapeFromPrison:
                  if (false == EncounterEscape(gi, ref action))
                  {
                     returnStatus = "EncounterEscape() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  if (1 == gi.PartyMembers.Count) // If only Prince in jail, no need to check for party members escaping
                  {
                     if (false == SetHuntState(gi, ref action)) // Escape from Prison
                     {
                        returnStatus = "SetHuntState() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     gi.IsGridActive = true; // GameAction.E203EscapeFromPrison
                  }
                  break;
               case GameAction.E203EscapeFromPrisonEnd:
                  if (false == SetHuntState(gi, ref action)) // Escape from Prison
                  {
                     returnStatus = "SetHuntState() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.CampfireFalconCheckEnd:
                  if (false == CampfireShowFeatState(gi, ref action)) 
                  {
                     returnStatus = "CampfireShowFeatState() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.CampfireShowFeat:
                  gi.EventDisplayed = gi.EventActive = "e503a";
                  break;
               case GameAction.CampfireShowFeatEnd:
                  if (false == SetCampfireFinalConditionState(gi, ref action)) 
                  {
                     returnStatus = "SetCampfireFinalConditionState() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.CampfireStarvationCheck:
                  gi.IsGridActive = true;  // CampfireStarvationCheck
                  break;
               case GameAction.CampfireStarvationEnd:
                  if (false == SetCampfireStarvationEndState(gi, ref action))
                  {
                     returnStatus = "SetCampfireStarvationEndState() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.CampfireLodgingCheck:
               case GameAction.CampfireTrueLoveCheck:
               case GameAction.CampfireLoadTransport:
                  break;
               case GameAction.CampfireWakeup:
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.PegasusMount)) // if character has a Pegasus possession, convert to mount
                  {
                     if (false == gi.RemoveSpecialItem(SpecialEnum.PegasusMount))
                     {
                        returnStatus = "RemoveSpecialItem(PegasusMount) returned false for ae=" + gi.EventActive + " action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                     }
                     else if (false == gi.AddNewMountToParty(MountEnum.Pegasus))
                     {
                        returnStatus = "AddMount(Pegasus) returned false for ae=" + gi.EventActive + " action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                     }
                  }
                  //--------------------------------------
                  bool isPartySizeOne = gi.IsPartySizeOne();
                  if ((true == gi.IsPartyDisgusted) && (false == isPartySizeOne)) // e010 - party is disgusted if ignore starving farmer
                  {
                     action = GameAction.CampfireDisgustCheck;
                     gi.IsPartyDisgusted = false;
                  }
                  else if ((true == gi.IsSpecialItemHeld(SpecialEnum.PegasusMountTalisman)) && (true == gi.IsSpecialistInParty()) && (false == gi.IsPegasusSkip))
                  {
                     action = GameAction.UpdateEventViewerActive;
                     gi.EventDisplayed = gi.EventActive = "e188b";
                  }
                  else if (false == Wakeup(gi, ref action))
                  {
                     returnStatus = "Wakeup() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E188TalismanPegasusConversion:
                  if (false == gi.RemoveSpecialItem(SpecialEnum.PegasusMountTalisman))
                  {
                     returnStatus = "RemoveSpecialItem(PegasusMountTalisman) returned false for ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  if (false == gi.AddNewMountToParty(MountEnum.Pegasus))
                  {
                     returnStatus = "AddMount(Pegasus) returned false for ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  if (false == Wakeup(gi, ref action))
                  {
                     returnStatus = "Wakeup() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E188TalismanPegasusSkip:
                  gi.IsPegasusSkip = true;
                  if (false == Wakeup(gi, ref action))
                  {
                     returnStatus = "Wakeup() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
                  }
                  break;
               default:
                  returnStatus = "Reached Default ERROR for a=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): 567 - " + returnStatus);
                  break;
            }
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
         {
            sb12.Append("<<<<ERROR2::::::GameStateCampfire.PerformAction(): ");
            sb12.Append(returnStatus);
            sb12.Append("  ???   ");
         }
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      protected bool PerformJailBreak(IGameInstance gi, ref GameAction action, int dieRoll)
      {
         gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT; // PerformJailBreak()
         gi.IsAirborne = false;
         EnteredHex hex = new EnteredHex(gi, ColorActionEnum.CAE_JAIL);
         hex.IsEncounter = true;
         ++gi.Statistic.myDaysInJailorDungeon;
         switch (gi.EventActive)
         {
            case "e203a":
               gi.EnteredHexes.Add(hex); // show staying in hex when in jail
               this.AddVisitedLocation(gi); // PerformJailBreak(e203a)
               if (1 == dieRoll)
               {
                  gi.IsJailed = false;
               }
               else
               {
                  if (("e061" == gi.EventStart) && (6 == dieRoll)) // only e061 does die roll = 6 cause death
                  {
                     gi.DieResults["e203a"][0] = 6;
                     gi.Prince.SetWounds(gi.Prince.Endurance - gi.Prince.Poison, 0); // kill the prince
                  }
                  else
                  {
                     if (false == SetEndOfDayState(gi, ref action)) // no hunting in prison so go straight to plague state
                     {
                        Logger.Log(LogEnum.LE_ERROR, "PerformJailBreak(): SetEndOfDayState() returned false");
                        return false;
                     }
                  }
               }
               break;
            case "e203c":
               gi.EnteredHexes.Add(hex); // show staying in hex when in jail
               this.AddVisitedLocation(gi); // PerformJailBreak(e203c)
               switch (dieRoll) // Based on the die roll, implement event
               {
                  case 2:
                  case 3:
                     gi.IsDungeon = false;
                     gi.NightsInDungeon = 0;
                     break;
                  case 4:
                  case 5:
                  case 6:
                  case 7:
                  case 8:
                  case 9:
                  case 10:
                  case 11:
                  case 12:
                     gi.NightsInDungeon++;
                     if (0 == (gi.NightsInDungeon % 7))
                        gi.Prince.SetWounds(0, 1);
                     break;
                  default: Logger.Log(LogEnum.LE_ERROR, "PerformJailBreak(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e203e":
               ++gi.WanderingDayCount;
               if (0 == gi.WanderingDayCount % 3)
               {
                  gi.Prince.SetWounds(1, 0);
                  int healthRemaining = gi.Prince.Endurance - gi.Prince.Wound - gi.Prince.Poison;
                  if (1 == healthRemaining) // left for dead
                  {
                     gi.GamePhase = GamePhase.EndGame;
                     if (true == gi.Prince.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace))
                     {
                        action = GameAction.EndGameResurrect;  // wizard slave - overworked and starved
                        gi.EventDisplayed = gi.EventActive = "e192a";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_END_GAME, "PerformJailBreak(): EndGameLost-1 ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
                        action = GameAction.EndGameLost; // wizard slave - overworked and starved
                        gi.EndGameReason = "Prince starves to dead as Wizard's slave";
                        gi.EventDisplayed = gi.EventActive = "e502";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                        gi.Statistic.myEndDaysCount = gi.Days;
                        gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
                        gi.Statistic.myEndCoinCount = gi.GetCoins();
                        gi.Statistic.myEndFoodCount = gi.GetFoods();
                     }
                     return true;
                  }
               }
               //-------------------------------------------
               if (6 == dieRoll)
               {
                  gi.IsEnslaved = false;
               }
               else
               {
                  if (1 == dieRoll)
                  {
                     gi.Prince.SetWounds(1, 0);
                     int healthRemaining = gi.Prince.Endurance - gi.Prince.Wound - gi.Prince.Poison;
                     if (1 == healthRemaining) // left for dead
                     {
                        gi.GamePhase = GamePhase.EndGame;
                        if (true == gi.Prince.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace))
                        {
                           action = GameAction.EndGameResurrect;  // wizard slave - beaten for escaping
                           gi.EventDisplayed = gi.EventActive = "e192a";
                           gi.DieRollAction = GameAction.DieRollActionNone;
                        }
                        else
                        {
                           Logger.Log(LogEnum.LE_END_GAME, "PerformJailBreak(): EndGameLost-2 ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
                           action = GameAction.EndGameLost; // wizard slave - beaten for escaping
                           gi.EndGameReason = "Prince beaten to death as Wizard's slave";
                           gi.EventDisplayed = gi.EventActive = "e502";
                           gi.DieRollAction = GameAction.DieRollActionNone;
                           gi.Statistic.myEndDaysCount = gi.Days;
                           gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
                           gi.Statistic.myEndCoinCount = gi.GetCoins();
                           gi.Statistic.myEndFoodCount = gi.GetFoods();
                        }
                        return true;
                     }
                  }
                  if (false == SetEndOfDayState(gi, ref action)) // no hunting as slave so go straight to plague state
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SetHuntState(): SetEndOfDayState() returned false");
                     return false;
                  }
               }
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "PerformJailBreak(): Reached Default with ae=" + gi.EventActive + " a=" + action.ToString()); return false;
         }
         return true;
      }
   }
   //-----------------------------------------------------
   class GameStateRest : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateUndo:
               UndoCommand(ref gi, ref action);
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateRest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.RestHealingEncounter:
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterStart;
               gi.NewHex = gi.Prince.Territory;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_REST));
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateRest.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
               gi.Prince.MovementUsed = gi.Prince.Movement; // End of the day
               if (false == SetSubstitutionEvent(gi, princeTerritory))           // GameStateRest.PerformAction()      - RestHealingEncounter
               {
                  returnStatus = "SetSubstitutionEvent() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateRest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.RestHealing:
               gi.NewHex = gi.Prince.Territory;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_REST));
               foreach (IMapItem mi in gi.PartyMembers)
               {
                  if( 0 < mi.Wound )
                  {
                     if (true == mi.Name.Contains("Prince"))
                        gi.Statistic.myNumOfPrinceHeal++;
                     else
                        gi.Statistic.myNumOfPartyHeal++;
                  }
                  mi.HealWounds(1, 0);  // RestHealing 
                  mi.IsExhausted = false;
               }
               if (false == SetHuntState(gi, ref action)) // Resting in same hex
               {
                  returnStatus = "SetHuntState() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateRest.PerformAction(): " + returnStatus);
               }
               break;
            default:
               returnStatus = "Reached Default ERROR";
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateRest.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateTravel : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         if (true == PerformEndCheck(gi, ref action))  // GameStateTravel.PerformAction()
            return returnStatus;
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         foreach (IMapItem mi in gi.PartyMembers)
            mi.IsCatchCold = false;
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
            case GameAction.ShowDienstalBranch:
            case GameAction.ShowLargosRiver:
            case GameAction.ShowNesserRiver:
            case GameAction.ShowTrogothRiver:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateUndo:
               UndoCommand(ref gi, ref action);
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.TravelLostCheck:
               gi.IsGridActive = false; // GameAction.TravelLostCheck
               if (0 == gi.MapItemMoves.Count)
               {
                  returnStatus = "Invalid state: gi.MapItemMoves.Count for a=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
               }
               else
               {
                  if (null == gi.MapItemMoves[0].NewTerritory)
                  {
                     returnStatus = "Invalid state: gi.NewHex=null for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.NewHex = gi.MapItemMoves[0].NewTerritory;  // GameStateTravel.PerformAction(TravelLostCheck) - Not added to gi.EnteredHexes[]
                  }
               }
               break;
            case GameAction.TravelShortHop:
               gi.IsAirborne = false;
               gi.IsShortHop = true;
               if (false == gi.PartyReadyToFly()) // mount to fly returns false if anybody is left or possessions are left
               {
                  returnStatus = "gi.PartyReadyToFly() returned false for " + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
               }
               else
               {
                  TravelAction(gi, ref action); // GameAction.TravelShortHop
               }
               break;
            case GameAction.TravelShowLost:
               if (true == gi.IsAirborne)
               {
                  gi.AtRiskMounts.Clear();
                  gi.IsGridActive = false;
                  gi.GamePhase = GamePhase.Encounter;
                  gi.DieRollAction = GameAction.EncounterStart;
                  gi.EventDisplayed = gi.EventActive = "e205c";
               }
               else
               {
                  gi.NewHex = gi.Prince.Territory;
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_LOST)); // GameStateTravel.PerformAction(TravelShowLost)
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateTravel.PerformAction(): gi.MapItemMoves.Clear() a=TravelShowLost");
                  gi.MapItemMoves.Clear();
                  --gi.Prince.MovementUsed;
                  if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
                  {
                     returnStatus = " AddMapItemMove() return false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateTravel.PerformAction(TravelShowLost): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                  }
                  ++gi.Prince.MovementUsed;
                  gi.IsGridActive = false; // GameAction.TravelShowLost
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateTravel.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // End of the day
                  Logger.Log(LogEnum.LE_MOVE_STACKING, "GameStateTravel.PerformAction(): a=TravelShowLost m=" + gi.Prince.Movement.ToString() + " mu=" + gi.Prince.MovementUsed.ToString());
                  if (false == SetHuntState(gi, ref action)) // Lost in this hex
                  {
                     returnStatus = "SetHuntState() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
                  }
               }
               break;
            case GameAction.TravelShowLostEncounter:
               if (0 == gi.MapItemMoves.Count)
               {
                  returnStatus = "gi.MapItemMoves.Count=0 for a=" + action.ToString();
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateTravel.PerformAction(): " + returnStatus);
               }
               else
               {
                  if (RiverCrossEnum.TC_CROSS_YES_SHOWN != gi.MapItemMoves[0].RiverCross)
                  {
                     gi.NewHex = gi.Prince.Territory;
                     gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_LOST)); // GameStateTravel.PerformAction(TravelShowLostEncounter)
                     Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateTravel.PerformAction(): gi.MapItemMoves.Clear() a=TravelShowLostEncounter");
                     gi.MapItemMoves.Clear();
                     --gi.Prince.MovementUsed;
                     if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex - no longer moving to new territory unless this is check after river crossing
                     {
                        returnStatus = " AddMapItemMove() return false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateTravel.PerformAction(TravelShowLostEncounter): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                     }
                     ++gi.Prince.MovementUsed;
                     Logger.Log(LogEnum.LE_VIEW_MIM, "GameStateTravel.PerformAction(): a=" + action.ToString() + " RESET mim=(" + gi.MapItemMoves[0].ToString() + " m=" + gi.Prince.MovementUsed + "/" + gi.Prince.Movement);
                  }
               }
               gi.IsGridActive = false;   // GameAction.TravelShowLostEncounter
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterStart;
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateTravel.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
               gi.Prince.MovementUsed = gi.Prince.Movement; // End of the day
               if (0 < gi.AtRiskMounts.Count)  // e095 - if traveling -- at risk mounts die. Need to redistribute load
               {
                  gi.AtRiskMounts.Clear();
                  action = GameAction.UpdateEventViewerActive;
                  gi.EventAfterRedistribute = gi.EventActive; // encounter this event after risk mount check
                  gi.EventDisplayed = gi.EventActive = "e095b";
               }
               else if (false == SetSubstitutionEvent(gi, princeTerritory))           // GameStateTravel.PerformAction()   - TravelShowLostEncounter
               {
                  returnStatus = "SetSubstitutionEvent() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.TravelShowMovement:  // no encounter
               action = GameAction.Travel;
               gi.IsGridActive = false; // GameAction.TravelShowMovement
               gi.IsMustLeaveHex = false;
               gi.IsImpassable = false;
               gi.IsTrueLoveHeartBroken = false;
               gi.IsTempleGuardEncounteredThisHex = false;
               if ((true == gi.IsExhausted) && ((true == gi.NewHex.IsOasis) || ("Desert" != gi.NewHex.Type))) // e120
                  gi.IsExhausted = false;
               ++gi.Prince.MovementUsed;
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateTravel(): ++MovementUsed=" + gi.Prince.MovementUsed.ToString() + " for a=" + action.ToString());
               //--------------------------------------------
               if (true == gi.DwarfAdviceLocations.Contains(gi.NewHex)) // this only applies when moving to a new hex
               {
                  action = GameAction.UpdateEventViewerActive;
                  gi.EventDisplayed = gi.EventActive = "e006g";
                  gi.GamePhase = GamePhase.Encounter;
                  gi.DieRollAction = GameAction.EncounterRoll;
                  ShowMovementScreenViewer(gi); // show user instructions in the ScreenViewer
               }
               else if (true == gi.IsHighPass) // high pass event can happen even if no travel encounter
               {
                  gi.IsHighPass = false;
                  ITerritory previousTerritory = GetPreviousHex(gi);
                  if (null == previousTerritory)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): GetPreviousHex() returned null for t=" + gi.Prince.Territory.Name);
                     previousTerritory = gi.Prince.Territory;
                  }
                  if (gi.NewHex.Name == previousTerritory.Name)
                  {
                     ShowMovementScreenViewer(gi); // show user instructions in the ScreenViewer
                  }
                  else
                  {
                     gi.EventAfterRedistribute = gi.EventDisplayed = gi.EventActive = "e086a";
                     gi.GamePhase = GamePhase.Encounter;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
               }
               else
               {
                  ShowMovementScreenViewer(gi);
               }
               if (true == gi.IsAirborne)
               {
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_AIR)); // TravelShowMovement 
               }
               else if (RaftEnum.RE_RAFT_CHOSEN == gi.RaftState)
               {
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_RAFT)); // TravelShowMovement()
                  this.AddVisitedLocation(gi);  // GameStateTravel.PerformAction(TravelShowMovement) - RaftEnum.RE_RAFT_CHOSEN 
               }
               else if (RaftEnum.RE_RAFT_ENDS_TODAY == gi.RaftState) // TravelShowMovement()
               {
                  //do nothing
               }
               else
               {
                  if (false == gi.IsAirborneEnd)
                     gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL)); // TravelShowMovement 
                  this.AddVisitedLocation(gi);  // GameStateTravel.PerformAction(TravelShowMovement)  
               }
               break;
            case GameAction.TravelShowRiverEncounter:
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterStart;
               break;
            case GameAction.TravelShowMovementEncounter:
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterStart;
               gi.IsGridActive = false; // GameAction.TravelShowMovementEncounter
               gi.IsMustLeaveHex = false;
               gi.IsImpassable = false;
               gi.IsTrueLoveHeartBroken = false;
               gi.IsTempleGuardEncounteredThisHex = false;
               if ((true == gi.IsExhausted) && (true == gi.NewHex.IsOasis) || ("Desert" != gi.NewHex.Type)) // e120
                  gi.IsExhausted = false;
               ++gi.Prince.MovementUsed;
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateTravel(): ++MovementUsed=" + gi.Prince.MovementUsed.ToString() + " for a=" + action.ToString());
               if (false == SetSubstitutionEvent(gi, princeTerritory, true))  // GameStateTravel.PerformAction()   - TravelShowMovementEncounter
               {
                  returnStatus = "SetSubstitutionEvent() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
               }
               //------------------------
               if (true == gi.IsAirborne)
               {
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_AIR));
               }
               else if (RaftEnum.RE_RAFT_CHOSEN == gi.RaftState)
               {
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_RAFT)); // TravelShowMovementEncounter()
                  this.AddVisitedLocation(gi);  // GameStateTravel.PerformAction()   - TravelShowMovementEncounter - RE_RAFT_CHOSEN
               }
               else if (RaftEnum.RE_RAFT_ENDS_TODAY == gi.RaftState)
               {
                  EnteredHex enteredHex = gi.EnteredHexes.Last();
                  enteredHex.EventNames.Add(gi.EventActive);
               }
               else
               {
                  if (false == gi.IsAirborneEnd)
                  {
                     gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL));
                  }
                  else
                  {
                     EnteredHex enteredHex = gi.EnteredHexes.Last();
                     enteredHex.EventNames.Add(gi.EventActive);
                     this.AddVisitedLocation(gi);  // GameStateTravel.PerformAction()   - TravelShowMovementEncounter
                  }
               }
               break;
            case GameAction.TravelEndMovement: // Prince clicked when still movement left ends movement phase
               gi.NewHex = gi.Prince.Territory;
               --gi.Prince.MovementUsed;
               gi.Prince.TerritoryStarting = gi.Prince.Territory;
               if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
               {
                  returnStatus = " AddMapItemMove() return false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
               }
               else
               {
                  Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateTravel.PerformAction(TravelEndMovement): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
               }
               ++gi.Prince.MovementUsed;
               //--------------------------------------------------------
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateTravel.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
               gi.Prince.MovementUsed = gi.Prince.Movement;
               gi.GamePhase = GamePhase.Encounter;
               gi.EventDisplayed = gi.EventActive = "e401"; // end of day
               break;
            default:
               returnStatus = "Reached Default ERROR";
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
         {
            sb12.Append("<<<<ERROR2::::::GameStateEncounter.PerformAction(): ");
            sb12.Append(returnStatus);
         }
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      protected void ShowMovementScreenViewer(IGameInstance gi)
      {
         if (RaftEnum.RE_RAFT_CHOSEN == gi.RaftState)
         {
            gi.EventDisplayed = gi.EventActive = "e213a";
            gi.GamePhase = GamePhase.Encounter;
            gi.DieRollAction = GameAction.EncounterRoll;
         }
         else if (true == gi.IsShortHop)
         {
            if (true == gi.EagleLairs.Contains(gi.NewHex)) // e115 - return to eagle lair for free food
            {
               gi.EventDisplayed = gi.EventActive = "e115";
               gi.GamePhase = GamePhase.Encounter;
               Logger.Log(LogEnum.LE_MOVE_COUNT, "ShowMovementScreenViewer(): MovementUsed=Movement for EagleLairs");
               gi.Prince.MovementUsed = gi.Prince.Movement; // end the day
               gi.IsPartyFed = true;
               gi.IsMountsFed = true;
            }
            else
            {
               gi.IsGridActive = true; // ShowMovementScreenViewer(flying)
               gi.EventDisplayed = gi.EventActive = "e204s"; // next screen to show
               if (gi.Prince.Movement <= gi.Prince.MovementUsed) // Finished Movement
               {
                  gi.GamePhase = GamePhase.Encounter;
                  gi.EventDisplayed = gi.EventActive = "e401"; // end of day
               }
            }
         }
         else if (true == gi.IsAirborne)
         {
            if (true == gi.EagleLairs.Contains(gi.NewHex)) // e115 - return to eagle lair for free food
            {
               gi.EventDisplayed = gi.EventActive = "e115";
               gi.GamePhase = GamePhase.Encounter;
               Logger.Log(LogEnum.LE_MOVE_COUNT, "ShowMovementScreenViewer(): MovementUsed=Movement for EagleLairs");
               gi.Prince.MovementUsed = gi.Prince.Movement; // end the day
               gi.IsPartyFed = true;
               gi.IsMountsFed = true;
            }
            else
            {
               gi.IsGridActive = true; // ShowMovementScreenViewer(flying)
               gi.EventDisplayed = gi.EventActive = "e204a"; // next screen to show
               if (gi.Prince.Movement <= gi.Prince.MovementUsed) // Finished Movement
               {
                  gi.GamePhase = GamePhase.Encounter;
                  gi.EventDisplayed = gi.EventActive = "e401"; // end of day
               }
            }
         }
         else if (true == gi.IsPartyRiding())
         {
            gi.IsGridActive = true; // ShowMovementScreenViewer(riding)
            gi.EventDisplayed = gi.EventActive = "e204m"; // next screen to show
            if (gi.Prince.Movement <= gi.Prince.MovementUsed) // Finished Movement
            {
               gi.GamePhase = GamePhase.Encounter;
               gi.EventDisplayed = gi.EventActive = "e401"; // end of day
            }
         }
         else
         {
            gi.IsGridActive = true; // ShowMovementScreenViewer(walking)
            gi.EventDisplayed = gi.EventActive = "e204u"; // next screen to show
            if (gi.Prince.Movement <= gi.Prince.MovementUsed) // Finished Movement
            {
               gi.GamePhase = GamePhase.Encounter;
               gi.EventDisplayed = gi.EventActive = "e401"; // end of day
            }
         }
      }  // Show on the ScreenViewer panel the instructions for movement
   }
   //-----------------------------------------------------
   class GameStateSeekNews : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateUndo:
               UndoCommand(ref gi, ref action);
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSeekNews.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.SeekNewsNoPay:
               gi.IsSeekNewModifier = false;
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e209a";
               gi.SunriseChoice = GamePhase.SeekNews;
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_SEEK_NEWS));
               break;
            case GameAction.SeekNewsWithPay:
               gi.IsSeekNewModifier = true;
               gi.ReduceCoins("SeekNewsWithPay", 5);
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e209a";
               gi.SunriseChoice = GamePhase.SeekNews;
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_SEEK_NEWS));
               break;
            default:
               returnStatus = "Reached Default ERROR";
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
         {
            sb12.Append("<<<<ERROR2::::::GameStateSeekNews.PerformAction(): ");
            sb12.Append(returnStatus);
         }
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateSeekHire : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateUndo:
               UndoCommand(ref gi, ref action);
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSeekHire.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.SeekHire:
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e210";
               gi.SunriseChoice = GamePhase.SeekHire;
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_HIRE));
               break;
            default:
               returnStatus = "Reached Default ERROR";
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
         {
            sb12.Append("<<<<ERROR2::::::GameStateSeekHire.PerformAction(): ");
            sb12.Append(returnStatus);
         }
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateSeekOffering : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateUndo:
               UndoCommand(ref gi, ref action);
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSeekOffering.PerformAction(): " + returnStatus);
               }
               break;
            default:
               returnStatus = "Reached Default ERROR";
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateSeekOffering.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateSearchRuins : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateUndo:
               UndoCommand(ref gi, ref action);
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSearchRuins.PerformAction(): " + returnStatus);
               }
               break;
            default:
               returnStatus = "Reached Default ERROR";
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateSearchRuins.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateSearch : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateUndo:
               UndoCommand(ref gi, ref action);
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSearch.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.SearchEncounter:
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterStart;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_SEARCH));
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateSearch.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
               gi.Prince.MovementUsed = gi.Prince.Movement; // End of the day
               if (false == SetSubstitutionEvent(gi, princeTerritory))           // GameStateSearch.PerformAction() - SearchEncounter
               {
                  returnStatus = "SetSubstitutionEvent() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSearch.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.SearchCache:
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_SEARCH));
               Logger.Log(LogEnum.LE_NEXT_ACTION, ":GameStateSearch.PerformAction(): SearchCache action");
               gi.EventActive = gi.EventDisplayed = "e214";
               break;
            case GameAction.SearchTreasure:
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_SEARCH));
               if (true == gi.SecretClues.Contains(princeTerritory))
                  gi.EventDisplayed = gi.EventActive = "e147a";
               else if ((true == gi.WizardAdviceLocations.Contains(princeTerritory)) || (true == gi.PixieAdviceLocations.Contains(princeTerritory)) )
                  gi.EventDisplayed = gi.EventActive = "e026";
               else
                  returnStatus = "invald state - p.t=" + princeTerritory.Name;
               break;
            default:
               returnStatus = "Reached Default ERROR";
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateSearch.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateEnded : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowGameFeats:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
            case GameAction.EndGameShowFeats:
               break;
            case GameAction.EndGameResurrect:
               ++gi.Statistic.myNumOfPrinceResurrection;
               gi.EventDisplayed = gi.EventActive = "e192a";
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.EndGameWin:
               if (0 == gi.Options.GetGameIndex())
                  GameEngine.theFeatsInGame.myIsOriginalGameWin = true;
               else if (1 == gi.Options.GetGameIndex())
                  GameEngine.theFeatsInGame.myIsRandomPartyGameWin = true;
               else if (2 == gi.Options.GetGameIndex())
                  GameEngine.theFeatsInGame.myIsRandomHexGameWin = true;
               else if (3 == gi.Options.GetGameIndex())
                  GameEngine.theFeatsInGame.myIsRandomGameWin = true;
               else if (4 == gi.Options.GetGameIndex())
                  GameEngine.theFeatsInGame.myIsFunGameWin = true;
               if (2 == gi.WitAndWileInitial)
                  GameEngine.theFeatsInGame.myIsLowWitWin = true;
               gi.EventDisplayed = gi.EventActive = "e501";
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.EndGameLost:
               gi.EventDisplayed = gi.EventActive = "e502";
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.Statistic.myEndDaysCount = gi.Days;
               gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
               gi.Statistic.myEndCoinCount = gi.GetCoins();
               gi.Statistic.myEndFoodCount = gi.GetFoods();
               break;
            case GameAction.EndGameShowStats:
               gi.EventDisplayed = gi.EventActive = "e503";
               break;
            case GameAction.EndGameClose:
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.EndGameFinal:
               gi.EventDisplayed = gi.EventActive = "e504";
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEnded.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.EndGameExit:
               if (null != System.Windows.Application.Current)
                  System.Windows.Application.Current.Shutdown();
               break;
            default:
               returnStatus = "Reached Default ERROR";
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateEnded.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateUnitTest : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         switch (action)
         {
            case GameAction.RemoveSplashScreen:
            case GameAction.ShowGameFeats:
            case GameAction.ShowInventory:
            case GameAction.ShowAllRivers:
            case GameAction.ShowRuleListing:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowEventListing:
            case GameAction.ShowPartyPath:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateLoadingGame:
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.E123BlackKnightCombatEnd:
               action = GameAction.UpdateEventViewerActive;
               gi.EventDisplayed = gi.EventActive = "e000a";
               break;
            case GameAction.EncounterEscape:
               action = GameAction.UpdateEventViewerActive;
               gi.EventDisplayed = gi.EventActive = "e000a";
               break;
            case GameAction.EncounterLootStart:
               if (0 == gi.CapturedWealthCodes.Count)
               {
                  action = GameAction.UpdateEventViewerActive;
                  gi.EventDisplayed = gi.EventActive = "e000a";
               }
               break;
            case GameAction.EncounterLoot:
               if (0 == gi.CapturedWealthCodes.Count)
               {
                  action = GameAction.UpdateEventViewerActive;
                  gi.EventDisplayed = gi.EventActive = "e000a";
               }
               break;
            case GameAction.EncounterLootStartEnd:
               action = GameAction.UpdateEventViewerActive;
               gi.EventDisplayed = gi.EventActive = "e000a";
               break;
            case GameAction.UnitTestCommand: // call the unit test's Command() function
               IUnitTest ut = gi.UnitTests[gi.GameTurn];
               if (false == ut.Command(ref gi))
               {
                  returnStatus = "Command() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UnitTestNext: // call the unit test's NextTest() function
               IUnitTest ut1 = gi.UnitTests[gi.GameTurn];
               if (false == ut1.NextTest(ref gi))
               {
                  returnStatus = "NextTest() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UnitTestCleanup: // Call the unit test's NextTest() function
               IUnitTest ut2 = gi.UnitTests[gi.GameTurn];
               if (false == ut2.Cleanup(ref gi))
               {
                  returnStatus = "Cleanup() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            default:
               returnStatus = "Reached Default ERROR";
               Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
         {
            sb12.Append("<<<<ERROR2::::::GameStateUnitTest.PerformAction(): ");
            sb12.Append(returnStatus);
         }
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateEncounter : GameState
   {
      //private static bool theIsFirstTime = true;
      private static int theNumHydraTeeth = 0;
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         Logger.Log(LogEnum.LE_UNDO_COMMAND, "GameStateEncounter.PerformAction(): cmd=" + gi.IsUndoCommandAvailable.ToString() + "-->false for a=" + action.ToString());
         gi.IsUndoCommandAvailable = false;
         gi.UndoHeal.Clear();
         gi.UndoExhaust.Clear();
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         //------------------------------------------------
         bool isGameEnded = false;
         if (GameAction.E192PrinceResurrected != action)    // if resurrected, game does not end
         {
            isGameEnded = PerformEndCheck(gi, ref action);  // check if game ended
            if( true == isGameEnded)
               Logger.Log(LogEnum.LE_END_GAME, "GameStateEncounter.PerformAction(): a=" + action.ToString() + " end?=" + isGameEnded.ToString() + " ae=" + gi.EventActive);
         }
         if (false == isGameEnded) // GameStateEncounter.PerformAction()
         {
            switch (action)
            {
               case GameAction.ShowInventory:
               case GameAction.ShowGameFeats:
               case GameAction.ShowRuleListing:
               case GameAction.ShowCharacterDescription:
               case GameAction.ShowEventListing:
               case GameAction.ShowPartyPath:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.ShowAllRivers:
               case GameAction.ShowDienstalBranch:
               case GameAction.ShowLargosRiver:
               case GameAction.ShowNesserRiver:
               case GameAction.ShowTrogothRiver:
               case GameAction.UpdateEventViewerDisplay:
                  break;
               case GameAction.UpdateEventViewerActive:
                  gi.EventDisplayed = gi.EventActive;
                  break;
               case GameAction.UpdateUndo:
                  UndoCommand(ref gi, ref action);
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               case GameAction.UpdateGameOptions:
                  break;
               case GameAction.UpdateLoadingGame:
                  if (false == LoadGame(ref gi, ref action))
                  {
                     returnStatus = "LoadGame() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterAbandon: // Remove all party members that are not riding
                  if (false == EncounterAbandon(gi, ref action))
                  {
                     returnStatus = "EncounterAbandon() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterBribe:
                  gi.ReduceCoins("EncounterBribe", gi.Bribe);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd(EncounterBribe) returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterCombat:
                  break;
               case GameAction.EncounterEnd:
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(EncounterEnd): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterEscape:
                  gi.ProcessIncapacitedPartyMembers("Escape", true);
                  if (false == EncounterEscape(gi, ref action))
                  {
                     returnStatus = "EncounterEscape() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  action = GameAction.UpdateEventViewerActive;
                  gi.EventDisplayed = gi.EventActive = "e218";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.EncounterEscapeFly:
                  if (false == EncounterEscape(gi, ref action))
                  {
                     returnStatus = "EncounterEscape() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  if ("e313c" != gi.EventActive)
                  {
                     action = GameAction.UpdateEventViewerActive;
                     gi.EventDisplayed = gi.EventActive = "e313";
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterEscapeMounted:
                  if (false == EncounterEscape(gi, ref action))
                  {
                     returnStatus = "EncounterEscape() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  action = GameAction.UpdateEventViewerActive;
                  gi.EventDisplayed = gi.EventActive = "e312c";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.EncounterFollow: // GameStateEncounter
                  gi.RaftStatePrevUndo = gi.RaftState;
                  gi.RaftState = RaftEnum.RE_NO_RAFT;    // e122 - GameStateEncounter(GameAction.EncounterFollow)
                  if (false == EncounterFollow(gi, ref action))
                  {
                     returnStatus = "EncounterFollow() returned false for action = " + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterHide:
                  if (false == SetEndOfDayState(gi, ref action)) // Hiding so cannot hunt
                  {
                     returnStatus = "SetEndOfDayState() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): SetEndOfDayState() returned false");
                  }
                  break;
               case GameAction.EncounterInquiry:
                  if ("e014b" == gi.EventActive)
                  {
                     gi.IsReaverClanFight = true;
                  }
                  else if ("e016a" == gi.EventActive)
                  {
                     gi.EncounteredMembers.Clear();
                     gi.EventStart = gi.EventActive;
                     IMapItem magician = CreateCharacter(gi, "Magician");
                     gi.EncounteredMembers.Add(magician);
                     gi.IsMagicianProvideGift = true;
                  }
                  else
                  {
                     returnStatus = "Invalid parameter ae=" + gi.EventDisplayed;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): SetEndOfDayState() returned false");
                  }
                  gi.EventDisplayed = gi.EventActive = "e342";
                  gi.DieResults["e342"][0] = Utilities.NO_RESULT;
                  gi.DieRollAction = GameAction.EncounterRoll;
                  action = GameAction.UpdateEventViewerActive;
                  break;
               case GameAction.EncounterLootStart:
                  if (false == EncounterLootStart(gi, ref action, dieRoll))
                  {
                     returnStatus = "EncounterLootStart() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(EncounterLootStart): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterLoot:
                  break;
               case GameAction.EncounterLootStartEnd:
                  if (false == EncounterLootStartEnd(gi, ref action))
                  {
                     returnStatus = "EncounterLootStartEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(EncounterLootStartEnd): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterRoll:
                  if (false == EncounterRoll(gi, ref action, dieRoll))
                  {
                     returnStatus = "EncounterRoll() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterStart:
                  if (false == EncounterStart(gi, ref action, dieRoll))
                  {
                     returnStatus = "EncounterStart() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EncounterSurrender:
                  if ("e054a" == gi.EventActive)
                  {
                     if (false == MoveToClosestGoblinKeep(gi))  // return back to keep 
                     {
                        returnStatus = "MoveToClosestGoblinKeep() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  if (false == MarkedForDeath(gi))
                  {
                     returnStatus = "MarkedForDeath() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.SearchRuins:
                  gi.SunriseChoice = GamePhase.SearchRuins;
                  gi.GamePhase = GamePhase.Encounter;
                  gi.EventDisplayed = gi.EventActive = "e208";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E006DwarfTalk:
                  gi.DwarvenChoice = "Talk";
                  gi.EventDisplayed = gi.EventActive = "e006a";
                  gi.DieRollAction = GameAction.EncounterStart;
                  break;
               case GameAction.E006DwarfEvade:
                  gi.DwarvenChoice = "Evade";
                  gi.EventDisplayed = gi.EventActive = "e006a";
                  gi.DieRollAction = GameAction.EncounterStart;
                  break;
               case GameAction.E006DwarfFight:
                  gi.DwarvenChoice = "Fight";
                  gi.EventDisplayed = gi.EventActive = "e006a";
                  gi.DieRollAction = GameAction.EncounterStart;
                  break;
               case GameAction.E006DwarfAdvice:
                  if (Utilities.NO_RESULT == gi.DieResults["e006f"][0])
                  {
                     gi.DieResults["e006f"][0] = dieRoll;
                  }
                  else
                  {
                     int directionLost = gi.DieResults["e006f"][0];
                     ITerritory tRamdom = FindRandomHexRangeDirectionAndRange(gi, directionLost, dieRoll);// Find a random hex at the range set by die roll
                     if (null == tRamdom)
                     {
                        returnStatus = "tRamdom=null for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        gi.DwarfAdviceLocations.Add(tRamdom);
                        if (false == SetCampfireEncounterState(gi, ref action))
                        {
                           returnStatus = "SetCampfireFinalConditionState() returned false for a=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                     }
                  }
                  break;
               case GameAction.E007ElfTalk:
                  gi.ElvenChoice = "Talk";
                  gi.EventDisplayed = gi.EventActive = "e007a";
                  gi.DieRollAction = GameAction.EncounterStart;
                  break;
               case GameAction.E007ElfEvade:
                  gi.ElvenChoice = "Evade";
                  gi.EventDisplayed = gi.EventActive = "e007a";
                  gi.DieRollAction = GameAction.EncounterStart;
                  break;
               case GameAction.E007ElfFight:
                  gi.ElvenChoice = "Fight";
                  gi.EventDisplayed = gi.EventActive = "e007a";
                  gi.DieRollAction = GameAction.EncounterStart;
                  break;
               case GameAction.E009FarmDetour:
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // detour consume rest of day
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E010FoodDeny:
                  if (4 < gi.GetFoods())
                     gi.IsPartyDisgusted = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E010FoodGive:
                  gi.ReduceFoods(5);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E011FarmerPurchaseEnd:
                  action = GameAction.UpdateEventViewerActive;
                  gi.EventDisplayed = gi.EventActive = "e011d";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E012FoodChange:
                  if (dieRoll < 0)
                  {
                     if ("e011a" == gi.EventActive)
                     {
                        gi.AddCoins("GameStateEncounter(E012FoodChange)-e011a", 1, false);
                        int food = 4;
                        if (true == gi.IsMerchantWithParty)
                           food = (int)Math.Ceiling((double)food * 2);
                        gi.ReduceFoods(food);
                        gi.PurchasedFood -= food;
                     }
                     else if ("e012a" == gi.EventActive)
                     {
                        gi.AddCoins("GameStateEncounter(E012FoodChange)-e012a", 1, false);
                        int food = 2;
                        if (true == gi.IsMerchantWithParty)
                           food = (int)Math.Ceiling((double)food * 2);
                        gi.ReduceFoods(food);
                        gi.PurchasedFood -= food;
                     }
                     else if (("e015b" == gi.EventActive) || ("e128c" == gi.EventActive))
                     {
                        gi.AddCoins("GameStateEncounter(E012FoodChange)-e015b", 1, false);
                        int food = 2;
                        if (true == gi.IsMerchantWithParty)
                           food = (int)Math.Ceiling((double)food * 2);
                        gi.ReduceFoods(food);
                        gi.PurchasedFood -= food;
                     }
                     else
                     {
                        returnStatus = "1-Invalid event=" + gi.EventActive + " for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     if ("e011a" == gi.EventActive)
                     {
                        gi.ReduceCoins("GameStateEncounter(e011a)", 1);
                        int food = 4;
                        if (true == gi.IsMerchantWithParty)
                           food = (int)Math.Ceiling((double)food * 2);
                        if (false == gi.AddFoods(food))
                        {
                           returnStatus = "AddFoods() returned false for adding 1";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        gi.PurchasedFood += food;
                     }
                     else if ("e012a" == gi.EventActive)
                     {
                        gi.ReduceCoins("GameStateEncounter(e012a)", 1);
                        int food = 2;
                        if (true == gi.IsMerchantWithParty)
                           food = (int)Math.Ceiling((double)food * 2);
                        if (false == gi.AddFoods(food))
                        {
                           returnStatus = "AddFoods() returned false for adding 1";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        gi.PurchasedFood += food;
                     }
                     else if (("e015b" == gi.EventActive) || ("e128c" == gi.EventActive))
                     {
                        gi.ReduceCoins("GameStateEncounter(e015b)", 1);
                        int food = 2;
                        if (true == gi.IsMerchantWithParty)
                           food = (int)Math.Ceiling((double)food * 2);
                        if (false == gi.AddFoods(food))
                        {
                           returnStatus = "AddFoods() returned false for adding 2";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        gi.PurchasedFood += food;
                     }
                     else
                     {
                        returnStatus = "2-Invalid event=" + gi.EventActive + " for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E013Lodging:
                  gi.IsFarmerLodging = true;
                  gi.IsHuntedToday = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for E013Lodging";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E015MountChange:
                  if (dieRoll < 0)
                  {
                     gi.ReduceMount(MountEnum.Horse);
                     --gi.PurchasedMount;
                     if (("e015b" == gi.EventActive) || ("e128f" == gi.EventActive))
                     {
                        int cost = 6;
                        if (true == gi.IsMerchantWithParty)
                           cost = (int)Math.Ceiling((double)cost * 0.5);
                        gi.AddCoins("GameStateEncounter(E015MountChange)-e015b", cost, false);
                     }
                     else if (("e129c" == gi.EventActive) || ("e210g" == gi.EventActive))
                     {
                        int cost = 7;
                        if (true == gi.IsMerchantWithParty)
                           cost = (int)Math.Ceiling((double)cost * 0.5);
                        gi.AddCoins("GameStateEncounter(E015MountChange)-e129c", cost, false);
                     }
                     else if ("e210d" == gi.EventActive)
                     {
                        int cost = 10;
                        if (true == gi.IsMerchantWithParty)
                           cost = (int)Math.Ceiling((double)cost * 0.5);
                        gi.AddCoins("GameStateEncounter(E015MountChange)-e210d", cost, false);
                     }
                     else
                     {
                        returnStatus = "1-Invalid event=" + gi.EventActive + " for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     if (false == gi.AddNewMountToParty())
                     {
                        returnStatus = "AddNewMountToParty() returned false for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     ++gi.PurchasedMount;
                     if (("e015b" == gi.EventActive) || ("e128f" == gi.EventActive))
                     {
                        int cost = 6;
                        if (true == gi.IsMerchantWithParty)
                           cost = (int)Math.Ceiling((double)cost * 0.5);
                        gi.ReduceCoins("GameStateEncounter(E015MountChange-e015b)", cost);
                     }
                     else if (("e129c" == gi.EventActive) || ("e210g" == gi.EventActive))
                     {
                        int cost = 7;
                        if (true == gi.IsMerchantWithParty)
                           cost = (int)Math.Ceiling((double)cost * 0.5);
                        gi.ReduceCoins("GameStateEncounter(E015MountChange-e129c)", cost);
                     }
                     else if ("e210d" == gi.EventActive)
                     {
                        int cost = 10;
                        if (true == gi.IsMerchantWithParty)
                           cost = (int)Math.Ceiling((double)cost * 0.5);
                        gi.ReduceCoins("GameStateEncounter(E015MountChange-e210d)", cost);
                     }
                     else
                     {
                        returnStatus = "2-Invalid event=" + gi.EventActive + " for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E016TalismanSave:
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.EventDisplayed = gi.EventActive = "e016b";
                  break;
               case GameAction.E018MarkOfCainEnd:
                  gi.IsMarkOfCain = true;
                  foreach (ITerritory t in Territory.theTerritories) // all audiences with high priest is forbidden
                  {
                     if (true == t.IsTemple)
                        gi.ForbiddenAudiences.AddTimeConstraint(t, Utilities.FOREVER);
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for E018MarkOfCainEnd";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E023WizardAdvice:
                  if (Utilities.NO_RESULT == gi.DieResults["e025"][0])
                  {
                     gi.DieResults["e025"][0] = dieRoll;
                  }
                  else if (Utilities.NO_RESULT == gi.DieResults["e025"][1])
                  {
                     gi.DieResults["e025"][1] = dieRoll;
                     ITerritory tRamdom = FindRandomHexRangeDirectionAndRange(gi, gi.DieResults["e025"][0], gi.DieResults["e025"][1]);// Find a random hex at the range set by die roll
                     if (null == tRamdom)
                     {
                        returnStatus = "tRamdom=null for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        gi.WizardAdviceLocations.Add(tRamdom);
                     }
                  }
                  else
                  {
                     if (false == SetCampfireEncounterState(gi, ref action))
                     {
                        returnStatus = "SetCampfireEncounterState() returned false for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     gi.DieResults["e025"][0] = Utilities.NO_RESULT;
                     gi.DieResults["e025"][1] = Utilities.NO_RESULT;
                  }
                  break;
               case GameAction.E024WizardFight:
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.EventDisplayed = gi.EventActive = "e330";
                  break;
               case GameAction.E027AncientTreasure:
                  gi.PegasusTreasure = PegasusTreasureEnum.Mount;
                  gi.CapturedWealthCodes.Add(110);
                  action = GameAction.EncounterLootStart;
                  break;
               case GameAction.E028CaveTombs:
                  gi.EventDisplayed = gi.EventActive = "e028a";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E031LootedTomb: // Handled by EventViewerE031Mgr 
                  break;
               case GameAction.E034CombatSpectre:
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.EventDisplayed = gi.EventActive = "e330";
                  break;
               case GameAction.E035IdiotStartDay:
                  gi.IsSpellBound = true;
                  gi.RemoveLeaderlessInParty(); // keep possessions and mounts
                  gi.WanderingDayCount = 1;
                  ++gi.Prince.StarveDayNum;
                  ++gi.Statistic.myNumOfPrinceStarveDays;
                  IMapItems deadMounts = new MapItems();
                  foreach (IMapItem mount in gi.Prince.Mounts)
                  {
                     ++mount.StarveDayNum;
                     if (5 < mount.StarveDayNum) // when carry capacity drops to zero, mount dies
                        deadMounts.Add(mount);
                  }
                  foreach (IMapItem m in deadMounts)
                     gi.Prince.Mounts.Remove(m);
                  if (false == EncounterEscape(gi, ref action))
                  {
                     returnStatus = "EncounterEscape() returned false for ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  if (false == Wakeup(gi, ref action)) //action == E035IdiotStartDay
                  {
                     returnStatus = "Wakeup() returned false for ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E035IdiotContinue:
                  if (false == Wakeup(gi, ref action)) // acton = E035IdiotContinue
                  {
                     returnStatus = "Wakeup() returned false ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E039TreasureChest: // Handled by EventViewerE039Mgr 
                  break;
               case GameAction.E040TreasureChest: // Handled by EventViewerE040Mgr 
                  gi.EventStart = "e040";
                  break;
               case GameAction.E042HighPriestAudience:
                  gi.IsAlcoveOfSendingAudience = true;
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e155";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E042MayorAudience: // meet mayor from closest town using AlcoveOfSending...
                  gi.IsAlcoveOfSendingAudience = true;
                  ITerritory t156b = FindClosestTown(gi); // this territory is updated by user selecting a castle or temple
                  if ((true == gi.IsReligionInParty()) && (true == gi.ForbiddenAudiences.IsReligiousConstraint(t156b)))
                  {
                     gi.ForbiddenAudiences.RemoveReligionConstraint(t156b); // no trusted advisor is provided
                     action = GameAction.E156MayorTerritorySelection;
                     gi.EventDisplayed = gi.EventActive = "e156g";
                     gi.ForbiddenAudiences.AddLetterConstraint(t156b);
                     if (false == gi.AddCoins("PerformAction(E042MayorAudience)", 100))
                     {
                        returnStatus = "EncounterRoll(): AddCoins()=false ae=" + gi.EventActive + " dr=" + dieRoll.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e156"; // audience with mayor due to alcove of sending
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  break;
               case GameAction.E042LadyAeravirAudience:
                  gi.IsAlcoveOfSendingAudience = true;
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e160";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E042CountDrogatAudience:
                  gi.IsAlcoveOfSendingAudience = true;
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e161";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E043SmallAltar:
                  break;
               case GameAction.E043SmallAltarEnd:
                  gi.ProcessIncapacitedPartyMembers("E043");
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for E044HighAltarEnd";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E044HighAltar:
                  break;
               case GameAction.E044HighAltarEnd:
                  gi.ProcessIncapacitedPartyMembers("E044");
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for E044HighAltarEnd";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E044HighAltarClue:
                  gi.EventDisplayed = gi.EventActive = "e147";
                  gi.DieRollAction = GameAction.E147ClueToTreasure;
                  break;
               case GameAction.E044HighAltarBlessed:
                  gi.IsBlessed = true;
                  --gi.Prince.Endurance;
                  foreach (IMapItem mi in gi.PartyMembers) // Gods reduce coin to zero
                     mi.Coin = 0;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for E044HighAltarBlessed";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E045ArchOfTravel:
                  break;
               case GameAction.E045ArchOfTravelEnd:
                  GameEngine.theFeatsInGame.myIsArchTravel = true;
                  gi.DieResults["e045b"][0] = Utilities.NO_RESULT;
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateEncounter.PerformAction(E045ArchOfTravelEnd): gi.MapItemMoves.Clear()");
                  gi.MapItemMoves.Clear();
                  MapItemMove mimArchTravel = new MapItemMove(Territory.theTerritories, gi.Prince, princeTerritory);   // Travel to same hex is no lost check
                  if ((0 == mimArchTravel.BestPath.Territories.Count) || (null == mimArchTravel.NewTerritory))
                  {
                     returnStatus = "Unable to Find Path to mim=" + mimArchTravel.ToString();
                  }
                  else
                  {
                     gi.MapItemMoves.Add(mimArchTravel);
                     Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(E045ArchOfTravelEnd): oT =" + princeTerritory.Name + " nT=" + mimArchTravel.NewTerritory.Name);
                  }
                  gi.NewHex = princeTerritory; // GameStateEncounter.PerformAction(E045ArchOfTravelEnd)
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL)); // GameStateEncounter.PerformAction(E045ArchOfTravelEnd)
                  //----------------------------------------------
                  if (true == gi.IsPartyRiding())
                     gi.Prince.Movement = 3;
                  else
                     gi.Prince.Movement = 2;
                  break;
               case GameAction.E045ArchOfTravelEndEncounter:
                  action = GameAction.UpdateEventViewerDisplay;
                  break;
               case GameAction.E045ArchOfTravelSkip: // Found an arch but skip traveling through it
                  if (false == gi.Arches.Contains(princeTerritory))
                     gi.Arches.Add(princeTerritory); // E045ArchOfTravelSkip
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E048FugitiveAlly:
                  switch (gi.DieResults["e048"][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e048a"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e048b"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 3:
                        if (true == gi.IsMarkOfCain)
                        {
                           action = GameAction.E018MarkOfCain;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e048c";
                           gi.DieRollAction = GameAction.DieRollActionNone;
                           int randomWealthRoll = Utilities.RandomGenerator.Next(6);
                           ++randomWealthRoll;
                           switch (randomWealthRoll) // priest gives half his wealth code = 10
                           {
                              case 1: gi.AddCoins("GameStateEncounter(E048FugitiveAlly)-1", 3, false); break;
                              case 2: gi.AddCoins("GameStateEncounter(E048FugitiveAlly)-2", 4, false); break;
                              case 3: gi.AddCoins("GameStateEncounter(E048FugitiveAlly)-3", 5, false); break;
                              case 4: gi.AddCoins("GameStateEncounter(E048FugitiveAlly)-4", 6, false); break;
                              case 5: gi.AddCoins("GameStateEncounter(E048FugitiveAlly)-5", 6, false); break;
                              case 6: gi.AddCoins("GameStateEncounter(E048FugitiveAlly)-6", 7, false); break;
                              default:
                                 returnStatus = "E048FugitiveAlly(): reached default randomWealthRoll=" + randomWealthRoll.ToString() + " for ae=" + gi.EventActive;
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                                 break;
                           }
                           if (0 == gi.EncounteredMembers.Count)
                           {
                              returnStatus = "EncounterRoll(): gi.EncounteredMembers.Count=0 for ae=" + gi.EventActive;
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                           else
                           {
                              IMapItem fugitivePriest = gi.EncounteredMembers[0];
                              fugitivePriest.IsFugitive = true;
                              fugitivePriest.IsTownCastleTempleLeave = true;
                              fugitivePriest.IsGuide = true;
                              fugitivePriest.IsFugitive = true;
                              if (false == AddGuideTerritories(gi, fugitivePriest, 2))
                              {
                                 returnStatus = " AddGuideTerritories() returned false for ae=" + gi.EventActive + " mi=" + fugitivePriest.Name + " hex=2";
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                              }
                              gi.AddCompanion(fugitivePriest);
                           }
                        }
                        break;
                     case 4:
                        gi.EventDisplayed = gi.EventActive = "e048d";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                        if (0 == gi.EncounteredMembers.Count)
                        {
                           returnStatus = " gi.EncounteredMembers.Count=0 for ae=" + gi.EventActive;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        else
                        {
                           IMapItem fugitiveMagician = gi.EncounteredMembers[0];
                           fugitiveMagician.IsFugitive = true;
                           fugitiveMagician.IsTownCastleTempleLeave = true;
                           gi.AddCompanion(fugitiveMagician);
                           gi.IsWizardJoiningParty = true;
                        }
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e048e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e048f"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: returnStatus = "reached default dr=" + gi.DieResults["e048"][0].ToString() + " a=" + action.ToString(); Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus); break;
                  }
                  break;
               case GameAction.E048FugitiveFight:
                  switch (gi.DieResults["e048"][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e300"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e300"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e300"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e300"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e048g"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e048h"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: returnStatus = "reached default dr=" + gi.DieResults["e048"][0].ToString() + " a=" + action.ToString(); Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus); break;
                  }
                  break;
               case GameAction.E049MinstrelStart:
                  gi.AddCoins("GameStateEncounter(E049MinstrelStart)", gi.FickleCoin, false);  // do not need to pay coin if you play a song
                  gi.FickleCoin = 0;
                  if (false == gi.IsMinstrelPlaying)
                     gi.MinstrelStart();
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = " EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E053CampsiteFight:
                  if (("e072a" == gi.EventStart) || ("e072d" == gi.EventStart))
                  {
                     gi.EventDisplayed = gi.EventActive = "e307"; // Elves attack first
                     gi.NumMembersBeingFollowed = 0;
                  }
                  else if ("e052" == gi.EventStart)
                  {
                     gi.EventDisplayed = gi.EventActive = "e304"; // Party attacks Encountered
                  }
                  else if ("e055" == gi.EventStart)
                  {
                     gi.EventDisplayed = gi.EventActive = "e304"; // Party attacks Encountered
                  }
                  else if ("e058a" == gi.EventStart)
                  {
                     gi.EventDisplayed = gi.EventActive = "e307"; // Dwarves attack first
                     gi.NumMembersBeingFollowed = 0;
                  }
                  else
                  {
                     returnStatus = " Reached Default with es=" + gi.EventStart + " for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  action = GameAction.UpdateEventViewerActive;
                  break;
               case GameAction.E060JailOvernight:
                  if (gi.GetCoins() < 10) // if greater than 12, open up a EventViewer
                  {
                     action = GameAction.UpdateEventViewerActive;
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e063"; // imprisoned
                     gi.IsJailed = true;
                     gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT; // GameAction.E060JailOvernigh
                     gi.Prince.ResetPartial();
                     if (false == gi.RemoveBelongingsInParty())
                     {
                        returnStatus = " RemoveBelongingsInParty() returned false a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     gi.ReduceCoins("E060JailOvernight", 10);
                     if (1 == gi.PartyMembers.Count) // if only yourself, encounter ends with you let out of jail paying your fine
                     {
                        action = GameAction.UpdateEventViewerActive;
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = " EncounterEnd() returned false a=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                     }
                  }
                  break;
               case GameAction.E064HiddenRuins:
                  ITerritory tRuins = gi.NewHex;
                  if (null == tRuins)
                     tRuins = princeTerritory;
                  if (false == gi.HiddenRuins.Contains(tRuins))  // if this is hidden ruins, save it for future reference
                     gi.HiddenRuins.Add(tRuins);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = " EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E068WizardTower:
                  IMapItems magicUsers = new MapItems();
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     if (true == mi.IsMagicUser())
                        magicUsers.Add(mi);
                  }
                  foreach (IMapItem mi in magicUsers) // the magic users escape or get arrested - leave party
                     gi.RemoveAbandonerInParty(mi);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = " EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E069WoundedWarriorCarry:
                  IMapItem warriorWounded = CreateCharacter(gi, "Warrior");
                  warriorWounded.IsAlly = true;
                  gi.AddCompanion(warriorWounded);
                  int woundsToAdd = warriorWounded.Endurance - 1;
                  warriorWounded.SetWounds(woundsToAdd, 0);
                  int freeLoad = 0;
                  foreach (IMapItem mi in gi.PartyMembers)
                     freeLoad += mi.GetFreeLoadWithoutModify(); // E069WoundedWarriorCarry - Deteremine if can carry wounded warrior
                  if (freeLoad < Utilities.PersonBurden) // if unable to carry without dropping food, need to redistribute
                     action = GameAction.E069WoundedWarriorRedistribute;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E069WoundedWarriorRemain:
                  IMapItem warriorWounded1 = CreateCharacter(gi, "Warrior");
                  warriorWounded1.IsAlly = true;
                  gi.AddCompanion(warriorWounded1);
                  int woundsToAdd1 = warriorWounded1.Endurance - 1;
                  warriorWounded1.SetWounds(woundsToAdd1, 0);
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // no more travel today
                  gi.IsWoundedWarriorRest = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E070HalflingTown:
                  if (false == gi.HalflingTowns.Contains(princeTerritory))
                     gi.HalflingTowns.Add(princeTerritory);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E072DoubleElves:
                  gi.DieResults["e053"][0] = Utilities.NO_RESULT; // reset the die roll b/c will attempt again after doubling  party
                  gi.EventDisplayed = gi.EventActive = "e053";    // return back and roll again for location
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E072FollowElves: // user selected the 'follow' button on e072
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateTravel.PerformAction(TravelEndMovement): gi.MapItemMoves.Clear()");
                  gi.MapItemMoves.Clear();
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  --gi.Prince.MovementUsed;
                  if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
                  {
                     returnStatus = " AddMapItemMove() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = " EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E073WitchCombat:
                  gi.EventDisplayed = gi.EventActive = "e330";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E073WitchMeet:
                  break;
               case GameAction.E073WitchTurnsPrinceIsFrog:
                  if ((true == gi.IsInMapItems("TrueLove")) || (true == gi.IsMagicInParty()))
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_END_GAME, "GameStateEncounter.PerformAction(): EndGameLost ae=" + gi.EventActive + " gp=" + gi.GamePhase.ToString() + " a=" + action.ToString() + " k?=" + gi.Prince.IsKilled.ToString() + " u?=" + gi.Prince.IsUnconscious.ToString() + " pc=" + gi.PartyMembers.Count.ToString());
                     action = GameAction.EndGameLost; // turned into frog
                     gi.GamePhase = GamePhase.EndGame;
                     gi.EndGameReason = "Prince is a Frog";
                     gi.EventDisplayed = gi.EventActive = "e502";
                     gi.DieRollAction = GameAction.DieRollActionNone;
                     gi.Statistic.myEndDaysCount = gi.Days;
                     gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
                     gi.Statistic.myEndCoinCount = gi.GetCoins();
                     gi.Statistic.myEndFoodCount = gi.GetFoods();
                  }
                  break;
               case GameAction.E075WolvesEncounter:
                  gi.IsWolvesAttack = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E076HuntingCat:
                  gi.EventStart = "e076";
                  gi.EventDisplayed = gi.EventActive = "e310";
                  IMapItem huntingCat = CreateCharacter(gi, "Cat");
                  gi.EncounteredMembers.Add(huntingCat);
                  break;
               case GameAction.E077HerdCapture: // herd of wild horses
                  if (true == gi.IsAirborne) // if this is encountered when airborne, need to drop to ground to train horses
                     gi.IsAirborne = false;
                  foreach (IMapItem e077Mi in gi.PartyMembers)
                  {
                     if (true == e077Mi.Name.Contains("Falcon"))
                        continue;
                     if (true == e077Mi.IsFlyer())
                        gi.Prince.AddNewMount();
                     else
                        e077Mi.AddNewMount();
                  }
                  if (false == gi.IsMagicInParty())
                  {
                     gi.IsTrainHorse = true;
                     Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                     gi.Prince.MovementUsed = gi.Prince.Movement; // need to stop to train horses
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd(E077HerdCapture) returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E078BadGoingHalt: // bad going
                  --gi.Prince.MovementUsed;
                  if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
                  {
                     returnStatus = "AddMapItemMove() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(TransportRedistributeEnd): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                  }
                  ++gi.Prince.MovementUsed;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E079HeavyRains: // heavy rain - cause a cold check
                  gi.IsAirborne = false;
                  gi.IsHeavyRain = true;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                  break;
               case GameAction.E079HeavyRainsRedistribute: // mount lost due to heavy rains
                  break;
               case GameAction.E079HeavyRainsContinueTravel:
                  action = GameAction.Travel;
                  gi.NumMembersBeingFollowed = 0;
                  gi.IsFloodContinue = false; // e092
                  gi.SunriseChoice = gi.GamePhase = GamePhase.Travel;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  //----------------------------------------------
                  if (RaftEnum.RE_RAFT_CHOSEN == gi.RaftState) // Set Movement value that is allowed this turn
                     gi.Prince.Movement = 1;
                  else if (true == gi.IsShortHop)
                     gi.Prince.Movement = 2;
                  else if (true == gi.IsPartyFlying())
                     gi.Prince.Movement = 3;
                  else if (true == gi.IsPartyRiding())
                     gi.Prince.Movement = 2;
                  else
                     gi.Prince.Movement = 1;
                  //----------------------------------------------
                  gi.IsGridActive = true;  // E079HeavyRainsContinueTravel
                  if (true == gi.IsShortHop)
                     gi.EventDisplayed = gi.EventActive = "e204s"; // next screen to show
                  else if (true == gi.IsAirborne)
                     gi.EventDisplayed = gi.EventActive = "e204a"; // next screen to show
                  else if (true == gi.IsPartyRiding())
                     gi.EventDisplayed = gi.EventActive = "e204m"; // next screen to show
                  else
                     gi.EventDisplayed = gi.EventActive = "e204u"; // next screen to show
                  break;
               case GameAction.E080PixieAdvice:
                  if (Utilities.NO_RESULT == gi.DieResults["e025b"][0])
                  {
                     gi.DieResults["e025b"][0] = dieRoll;
                  }
                  else
                  {
                     if (Utilities.NO_RESULT == gi.DieResults["e025b"][1])
                     {
                        gi.DieResults["e025b"][1] = dieRoll;
                     }
                     else
                     {
                        ITerritory tRamdom = FindRandomHexRangeDirectionAndRange(gi, gi.DieResults["e025b"][0], gi.DieResults["e025b"][0]);// Find a random hex at the range set by die roll
                        if (null == tRamdom)
                        {
                           returnStatus = "tRamdom=null for a=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        else
                        {
                           gi.PixieAdviceLocations.Add(tRamdom);
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                     }
                     gi.DieResults["e025b"][0] = Utilities.NO_RESULT;
                     gi.DieResults["e025b"][1] = Utilities.NO_RESULT;
                  }
                  break;
               case GameAction.E082SpectreMagic:
                  gi.EventStart = "e082";
                  break;
               case GameAction.E085Falling:
                  break;
               case GameAction.E086HighPass:
                  if (RaftEnum.RE_RAFT_SHOWN != gi.RaftState) // if rafting, can leave by raft hex
                     gi.IsHighPass = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.TransportRedistributeEnd: // GameStateEncounter.PerformAction()
                  if ("e079b" == gi.EventAfterRedistribute) // Need to set movement based on current conditions after Heavy Rains continues into next day
                  {
                     gi.IsGridActive = true;  // TransportRedistributeEnd
                     gi.GamePhase = GamePhase.Travel;
                     action = GameAction.Travel;
                     bool isPartyRiding = gi.IsPartyRiding();
                     if (true == gi.IsHeavyRainDismount)
                        isPartyRiding = false;
                     if (true == gi.IsAirborne)
                     {
                        if (true == gi.PartyReadyToFly()) // mount to fly returns false if anybody is left or possessions are left
                        {
                           gi.EventDisplayed = gi.EventActive = "e204a"; // next screen to show
                           gi.Prince.Movement = 3;
                        }
                        else
                        {
                           gi.IsAirborne = false;
                           if (true == isPartyRiding)
                           {
                              gi.EventDisplayed = gi.EventActive = "e204m"; // next screen to show
                              gi.Prince.Movement = 2;
                           }
                           else
                           {
                              gi.EventDisplayed = gi.EventActive = "e204u"; // next screen to show  
                              gi.Prince.Movement = 1;
                           }
                        }
                     }
                     else
                     {
                        if (true == isPartyRiding)
                        {
                           gi.EventDisplayed = gi.EventActive = "e204m"; // next screen to show
                           gi.Prince.Movement = 2;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e204u"; // next screen to show  
                           gi.Prince.Movement = 1;
                        }
                     }
                     gi.EventAfterRedistribute = "";
                  }
                  else if ("e078" == gi.EventAfterRedistribute)
                  {
                     --gi.Prince.MovementUsed;
                     if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
                     {
                        returnStatus = "AddMapItemMove() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(TransportRedistributeEnd): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                     }
                     ++gi.Prince.MovementUsed;
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     gi.EventAfterRedistribute = "";
                  }
                  else if ("e121" == gi.EventAfterRedistribute)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     gi.EventAfterRedistribute = "";
                  }
                  else if ("e126" == gi.EventAfterRedistribute) // raft caught in current
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     gi.EventAfterRedistribute = "";
                  }
                  else if ("e086a" == gi.EventAfterRedistribute) // high pass results
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     gi.EventAfterRedistribute = "";
                  }
                  else
                  {
                     if ("" == gi.EventAfterRedistribute)
                     {
                        --gi.Prince.MovementUsed;
                        if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
                        {
                           returnStatus = "AddMapItemMove() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        else
                        {
                           Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(TransportRedistributeEnd): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                        }
                        ++gi.Prince.MovementUsed;
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = gi.EventAfterRedistribute;
                        gi.EventAfterRedistribute = "";
                        gi.DieRollAction = GameAction.EncounterStart;
                        action = GameAction.UpdateEventViewerActive;
                     }
                  }
                  break;
               case GameAction.E088FallingRocks:
                  break;
               case GameAction.E082SpectreMagicEnd:
                  gi.ProcessIncapacitedPartyMembers("Spectre takes person", true);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E083WildBoar:
                  gi.EventStart = "e083";
                  gi.EventDisplayed = gi.EventActive = "e310";
                  IMapItem boar = CreateCharacter(gi, "Boar");
                  gi.EncounteredMembers.Add(boar);
                  break;
               case GameAction.E084BearEncounter:
                  gi.IsBearAttack = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E087UnpassableWoods:
               case GameAction.E089UnpassableMorass:
                  gi.IsImpassable = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E090Quicksand: break;
               case GameAction.E091PoisonSnake: break;
               case GameAction.E091PoisonSnakeEnd:
                  gi.DieResults["e091"][0] = Utilities.NO_RESULT;
                  gi.ProcessIncapacitedPartyMembers("Snake takes person", true);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E092Flood: // flood
                  gi.IsFlood = true;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E095MountAtRisk:
                  break;
               case GameAction.E095MountAtRiskEnd:
                  break;
               case GameAction.E096MountsDie:
                  gi.IsMountsSick = true;
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     foreach (IMapItem mount in mi.Mounts)
                     {
                        if (true == mount.IsFlyingMountCarrier()) // Griffons and Harpies not considered mounts in this case
                           continue;
                        mount.IsMountSick = true;
                     }
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E097FleshRot:
                  break;
               case GameAction.E097FleshRotEnd:
                  gi.ProcessIncapacitedPartyMembers("Marsh Flesh Rot");
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E102LowClouds:
                  gi.IsAirborne = false;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // End of the day
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E103BadHeadWinds:
                  if (2 < gi.Prince.MovementUsed) // if traveled to third hex in air, need to move back to previous hex
                  {
                     gi.IsAirborne = false;
                     ITerritory previousTerritory = GetPreviousHex(gi);
                     if (null == previousTerritory)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + "GetPreviousHex() returned null for t=" + gi.Prince.Territory.Name);
                        previousTerritory = gi.Prince.Territory;
                     }
                     --gi.Prince.MovementUsed;
                     gi.Prince.TerritoryStarting = gi.Prince.Territory;
                     gi.NewHex = previousTerritory;
                     gi.EnteredHexes.RemoveAt(gi.EnteredHexes.Count - 1); // remove last entry 
                     gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_AIR));
                     if (false == AddMapItemMove(gi, previousTerritory))
                     {
                        returnStatus = " AddMapItemMove() return false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(E103BadHeadWinds): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                     }
                     ++gi.Prince.MovementUsed;
                  }
                  else
                  {
                     --gi.Prince.Movement; // reduce hexes by one
                     if (gi.Prince.Movement <= gi.Prince.MovementUsed)
                        gi.IsAirborne = false;
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E104TailWinds:
                  ++gi.Prince.Movement; // increases hexes by one
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E105StormCloudLand:
                  gi.IsAirborne = false;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // End of the day
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E105ViolentWeather:
                  //--------------------------------------------------------
                  IMapItems lostInStormMembers = new MapItems();
                  foreach (IMapItem mi in gi.PartyMembers)
                     lostInStormMembers.Add(mi);
                  foreach (IMapItem mi in lostInStormMembers)
                     gi.RemoveAbandonedInParty(mi);
                  gi.Prince.Mounts.Clear(); // remove all mounts
                                            //--------------------------------------------------------
                  int directionvw = gi.DieResults["e105a"][0];
                  int rangevw = gi.DieResults["e105a"][1];
                  if ((directionvw < 1) || (6 < directionvw))
                  {
                     returnStatus = " invalid direction=" + directionvw.ToString() + " for  action =" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  if ((rangevw < 1) || (6 < rangevw))
                  {
                     returnStatus = " invalid direction=" + rangevw.ToString() + " for  action =" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  rangevw = (int)Math.Ceiling((double)rangevw * 0.5); // half rounded up
                  //--------------------------------------------------------
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateEncounter.PerformAction(): gi.MapItemMoves.Clear() for a=" + action.ToString());
                  gi.MapItemMoves.Clear();
                  ITerritory blowToTerritory = FindRandomHexRangeDirectionAndRange(gi, directionvw, rangevw);// Find a random hex at random direction and range 1
                  if (null == blowToTerritory)
                  {
                     returnStatus = " blowToTerritory=null action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  gi.Prince.MovementUsed = 0; // must have movement left to be blown off course
                  gi.Prince.TerritoryStarting = gi.NewHex;
                  gi.NewHex = blowToTerritory;
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_AIR));
                  if (false == AddMapItemMove(gi, blowToTerritory))
                  {
                     returnStatus = " AddMapItemMove() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(E105ViolentWeather): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                  }
                  //--------------------------------------------------------
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // end movement after being blown off course
                  //--------------------------------------------------------
                  int woundsvw = gi.DieResults["e105a"][2];
                  if ((woundsvw < 1) || (6 < woundsvw))
                  {
                     returnStatus = " invalid direction=" + woundsvw.ToString() + " for  action =" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  gi.Prince.SetWounds(woundsvw, 0);  // prince is wounded one die
                  if (true == gi.Prince.IsKilled)
                  {
                     gi.GamePhase = GamePhase.EndGame;
                     if (true == gi.Prince.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace))
                     {
                        action = GameAction.EndGameResurrect;  // E105ViolentWeather
                        gi.EventDisplayed = gi.EventActive = "e192a";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                     else
                     {
                        action = GameAction.EndGameLost;  // E105ViolentWeather
                        gi.EndGameReason = "Prince died in violent crash to ground due to weather";
                        gi.EventDisplayed = gi.EventActive = "e502";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                        gi.Statistic.myEndDaysCount = gi.Days;
                        gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
                        gi.Statistic.myEndCoinCount = gi.GetCoins();
                        gi.Statistic.myEndFoodCount = gi.GetFoods();
                     }
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  gi.DieResults["e105a"][0] = Utilities.NO_RESULT;
                  gi.DieResults["e105a"][1] = Utilities.NO_RESULT;
                  gi.DieResults["e105a"][2] = Utilities.NO_RESULT;
                  break;
               case GameAction.E106OvercastLost:
                  int direction = gi.DieResults["e106"][0];
                  ITerritory adjacentTerritory = FindRandomHexRangeDirectionAndRange(gi, direction, 1);// Find a random hex at random direction and range 1
                  if (null == adjacentTerritory)
                  {
                     returnStatus = " adjacentTerritory=null action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  gi.Prince.TerritoryStarting = gi.NewHex;
                  gi.NewHex = adjacentTerritory;
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_AIR));
                  if (false == AddMapItemMove(gi, adjacentTerritory))
                  {
                     returnStatus = " AddMapItemMove() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(E106OvercastLost): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                  }
                  gi.DieResults["e106"][0] = Utilities.NO_RESULT;
                  break;
               case GameAction.E106OvercastLostEnd:
                  gi.IsAirborne = false;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // no more travel today
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E107FalconAdd:
                  gi.IsFalconFed = true;
                  IMapItem falcon = CreateCharacter(gi, "Falcon");
                  falcon.IsRiding = true;
                  falcon.IsFlying = true;
                  falcon.IsGuide = true;
                  falcon.GuideTerritories = Territory.theTerritories;
                  gi.PartyMembers.Add(falcon);
                  gi.ReduceFoods(1);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E107FalconNoFeed:
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E110AirSpiritConfusedEnd:
                  gi.DieResults["e110b"][0] = Utilities.NO_RESULT; // setup if event occurs again in same day
                  gi.DieResults["e110b"][1] = Utilities.NO_RESULT;
                  if (gi.Prince.MovementUsed < gi.Prince.Movement)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e401";
                  }
                  break;
               case GameAction.E111StormDemonEnd:
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E111StormDemonRepel:
                  break;
               case GameAction.E111StormDemonRepelFail:
                  gi.DieResults["e111"][1] = Utilities.NO_RESULT;
                  gi.EventDisplayed = gi.EventActive = "e111";
                  gi.DieRollAction = GameAction.EncounterStart;
                  break;
               case GameAction.E110AirSpiritTravelEnd:
                  gi.DieResults["e110c"][0] = Utilities.NO_RESULT; // setup if event occurs again in same day
                  gi.AirSpiritLocations = null;
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_AIR)); // NexHex changed to proper hex in GameViewerWindow->MouseDownPolygonTravel()
                  if (gi.Prince.MovementUsed < gi.Prince.Movement)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e401";
                  }
                  break;
               case GameAction.E120Exhausted:
                  gi.IsExhausted = true;
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     if (null != mi.Rider)
                     {
                        mi.Rider.Mounts.Remove(mi);  // Griffon/Harpy removes its rider
                        mi.Rider = null;
                     }
                  }
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     mi.IsExhausted = true;
                     mi.SetWounds(1, 0); // each party member suffers one wound
                     foreach (IMapItem mount in mi.Mounts)
                        mount.IsExhausted = true;
                     if (false == mi.IsFlyer()) // Exhausted must dismount -- flyers are always riding
                        mi.IsRiding = false;
                     mi.IsFlying = false;
                  }
                  gi.ProcessIncapacitedPartyMembers("E120 Exhausted");
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E109PegasusCapture:
                  break;
               case GameAction.E121SunStroke:
                  gi.EventAfterRedistribute = gi.EventStart = gi.EventDisplayed = gi.EventActive = "e121";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.E121SunStrokeEnd:
                  break;
               case GameAction.E123WoundedBlackKnightRemain:
                  IMapItem blackKnight = CreateCharacter(gi, "KnightBlack");
                  blackKnight.SetWounds(7, 0);
                  blackKnight.IsAlly = true;
                  gi.PartyMembers.Add(blackKnight);
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // no more travel today
                  gi.IsWoundedBlackKnightRest = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E122RaftsmenEnd:
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                  if (0 == gi.MapItemMoves.Count)
                  {
                     returnStatus = "Invalid state gi.MapItemMoves.Count=0 for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_FAIL;
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E122RaftsmenCross:
                  gi.ReduceCoins("GameStateEncounter(E122RaftsmenCross)", 1);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E122RaftsmenHire:
                  gi.RaftState = RaftEnum.RE_RAFT_SHOWN;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                  if (0 == gi.MapItemMoves.Count)
                  {
                     returnStatus = "Invalid state gi.MapItemMoves.Count=0 for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_FAIL;
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E123BlackKnightCombatEnd: // Prince elected to end combat with black knight
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                  gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_FAIL;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E123BlackKnightRefuse:
                  break;
               case GameAction.E123BlackKnightRefuseEnd:
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                  if (0 == gi.MapItemMoves.Count)
                  {
                     returnStatus = "Invalid state gi.MapItemMoves.Count=0 for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_FAIL;
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E126RaftInCurrent:
                  break;
               case GameAction.E126RaftInCurrentLostRaft:
                  gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E126RaftInCurrentEnd: // raft in current but nobody lost
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateEncounter.PerformAction(): gi.MapItemMoves.Clear() for a=" + action.ToString());
                  gi.MapItemMoves.Clear();
                  ITerritory downRiverT1 = Territory.theTerritories.Find(gi.Prince.Territory.DownRiver);
                  if (null == downRiverT1)
                  {
                     returnStatus = " downRiverT1=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  gi.Prince.TerritoryStarting = gi.Prince.Territory;
                  gi.NewHex = downRiverT1;
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_DOWNRIVER));  // E126RaftInCurrentEnd
                  if (false == AddMapItemMove(gi, downRiverT1))
                  {
                     returnStatus = " AddMapItemMove() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(E126RaftInCurrentEnd): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E126RaftInCurrentRedistribute: // raft in current and somebody lost
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateEncounter.PerformAction(): gi.MapItemMoves.Clear() for a=" + action.ToString());
                  gi.MapItemMoves.Clear();
                  ITerritory downRiverT = Territory.theTerritories.Find(gi.Prince.Territory.DownRiver);
                  if (null == downRiverT)
                  {
                     returnStatus = " downRiverT1=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  gi.Prince.TerritoryStarting = gi.Prince.Territory;
                  gi.NewHex = downRiverT; //GameStateEncounter.PerformAction(E126RaftInCurrentRedistribute)
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_DOWNRIVER)); // E126RaftInCurrentRedistribute
                  if (false == AddMapItemMove(gi, downRiverT))
                  {
                     returnStatus = " AddMapItemMove() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(E126RaftInCurrentRedistribute): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
                  }
                  gi.EventAfterRedistribute = "e126";
                  break;
               case GameAction.E128aBuyPegasus:
                  if (false == gi.AddNewMountToParty(MountEnum.Pegasus))
                  {
                     returnStatus = " AddNewMountToParty() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  int pegasusCost = 50;
                  if (true == gi.IsMerchantWithParty)
                     pegasusCost = (int)Math.Ceiling((double)pegasusCost * 0.5);
                  gi.ReduceCoins("E128aBuyPegasus", pegasusCost);
                  gi.EventDisplayed = gi.EventActive = "e188";
                  break;
               case GameAction.E128bPotionCureChange:
                  int costCurePotion = 10;
                  if (true == gi.IsMerchantWithParty)
                     costCurePotion = (int)Math.Ceiling((double)costCurePotion * 0.5);
                  if (dieRoll < 0)
                  {
                     gi.AddCoins("GameStateEncounter(E128bPotionCureChange)", costCurePotion, false);
                     if (false == gi.RemoveSpecialItem(SpecialEnum.CurePoisonVial))
                     {
                        returnStatus = "RemoveSpecialItem() returned false a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     --gi.PurchasedPotionCure;
                  }
                  else
                  {
                     gi.ReduceCoins("E128bPotionCureChange", costCurePotion);
                     gi.AddSpecialItem(SpecialEnum.CurePoisonVial);
                     ++gi.PurchasedPotionCure;
                  }
                  break;
               case GameAction.E128ePotionHealChange:
                  int costHealingPotion = 5;
                  if ("e128e" == gi.EventActive)
                     costHealingPotion = 5;
                  else if ("e129b" == gi.EventActive)
                     costHealingPotion = 6;
                  else
                     returnStatus = "Invalid state ++ a=" + action.ToString() + " ae=" + gi.EventActive;
                  if (true == gi.IsMerchantWithParty)
                     costHealingPotion = (int)Math.Ceiling((double)costHealingPotion * 0.5);
                  if (dieRoll < 0)
                  {
                     gi.AddCoins("GameStateEncounter(E128ePotionHealChange)", costHealingPotion, false);
                     if (false == gi.RemoveSpecialItem(SpecialEnum.HealingPoition))
                     {
                        returnStatus = "RemoveSpecialItem() returned false a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     --gi.PurchasedPotionHeal;
                  }
                  else
                  {
                     gi.ReduceCoins("E128ePotionHealChange", costHealingPotion);
                     gi.AddSpecialItem(SpecialEnum.HealingPoition);
                     ++gi.PurchasedPotionHeal;
                  }
                  break;
               case GameAction.E129aBuyAmulet:
                  int costAmulet = 25;
                  if (true == gi.IsMerchantWithParty)
                     costAmulet = (int)Math.Ceiling((double)costAmulet * 0.5);
                  gi.AddSpecialItem(SpecialEnum.AntiPoisonAmulet);
                  gi.ReduceCoins("E129aBuyAmulet", costAmulet);
                  gi.EventDisplayed = gi.EventActive = "e187";
                  break;
               case GameAction.E130JailedOnTravels:
                  switch (gi.DieResults["e130"][1])
                  {
                     case 1: gi.NewHex = Territory.theTerritories.Find("1212"); break; // E130JailedOnTravels - Huldra
                     case 2: gi.NewHex = Territory.theTerritories.Find("0323"); break; // E130JailedOnTravels - Dragot
                     case 3: gi.NewHex = Territory.theTerritories.Find("1923"); break; // E130JailedOnTravels - Aeravir
                     case 4: gi.NewHex = FindClosestTemple(gi); break;                 // E130JailedOnTravels
                     case 5: case 6: gi.NewHex = FindClosestTown(gi); break;           // E130JailedOnTravels
                     default:
                        returnStatus = "Reached default for dr=" + gi.DieResults["e130"][1].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        break;
                  }
                  if (null == gi.NewHex)
                  {
                     returnStatus = "gi.NewHex=null for dr=" + gi.DieResults["e130"][1].ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.Prince.TerritoryStarting = gi.Prince.Territory;
                     gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_JAIL));
                     gi.MapItemMoves.Clear();
                     Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                     --gi.Prince.MovementUsed;
                     if (false == AddMapItemMove(gi, gi.NewHex)) // move to same hex
                     {
                        returnStatus = " AddMapItemMove() returned false a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
                     ++gi.Days;  // advance the day by one day
                     if ((true == gi.IsExhausted) && ((true == gi.NewHex.IsOasis) || ("Desert" != gi.NewHex.Type))) // e120
                        gi.IsExhausted = false;
                  }
                  if (true == gi.IsArrestedByDrogat)
                  {
                     gi.IsArrestedByDrogat = false;
                     if (false == MarkedForDeath(gi))
                     {
                        returnStatus = "ResetDieResultsForAudience() returned false ae=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e060"; // normal arrest
                  }
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E130BribeGuard:
                  int costBribeGard = 10;
                  if (true == gi.IsMerchantWithParty)
                     costBribeGard = (int)Math.Ceiling((double)costBribeGard * 0.5);
                  gi.ReduceCoins("E130BribeGuard", costBribeGard);
                  gi.EventDisplayed = gi.EventActive = "e130e";
                  break;
               case GameAction.E130RobGuard:
                  ITerritory t130 = null;
                  switch (gi.DieResults["e130"][1]) // castle is forbidden after robbing lord
                  {
                     case 1: t130 = Territory.theTerritories.Find("1212"); break;
                     case 2: t130 = Territory.theTerritories.Find("0323"); break;
                     case 3: t130 = Territory.theTerritories.Find("1923"); break;
                     default: break; // do nothing
                  }
                  if (null != t130)
                  {
                     if (false == gi.ForbiddenHexes.Contains(t130)) // cannot return to this hex
                        gi.ForbiddenHexes.Add(t130);
                  }
                  gi.EventStart = "e130";
                  gi.CapturedWealthCodes.Add(100);
                  action = GameAction.EncounterLootStart;
                  break;
               case GameAction.E133PlaguePrince:
                  foreach (IMapItem mi in gi.PartyMembers) // kill all plagued party members
                  {
                     if ((true == mi.IsPlagued) && (false == mi.Name.Contains("Prince")))
                        mi.IsKilled = true;
                  }
                  ++gi.Prince.StarveDayNum;
                  ++gi.Statistic.myNumOfPrinceStarveDays;
                  gi.Prince.IsRiding = false;
                  gi.Prince.IsFlying = false;
                  gi.Prince.IsPlagued = false;
                  gi.ProcessIncapacitedPartyMembers("E133");
                  //-----------------------------
                  int partyCount = gi.PartyMembers.Count;
                  int countOfPersons = gi.RemoveLeaderlessInParty();
                  if (0 < countOfPersons)  // If there are any surviving party members, they run away and take all mounts and treasures
                  {
                     gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT;
                     gi.Prince.ResetPartial(); // remove all food and water and possessions
                  }
                  else
                  {
                     IMapItems deadMounts1 = new MapItems();
                     foreach (IMapItem mount in gi.Prince.Mounts)
                     {
                        ++mount.StarveDayNum;
                        if (5 < mount.StarveDayNum) // when carry capacity drops to zero, mount dies
                           deadMounts1.Add(mount);
                     }
                     foreach (IMapItem m in deadMounts1)
                        gi.Prince.Mounts.Remove(m);
                  }
                  if (false == Wakeup(gi, ref action)) // action = E133PlaguePrince
                  {
                     returnStatus = "Wakeup() return false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E133PlagueParty:
                  foreach (IMapItem mi in gi.PartyMembers) // kill all plagued party members
                  {
                     if (true == mi.IsPlagued)
                        mi.IsKilled = true;
                  }
                  gi.ProcessIncapacitedPartyMembers("E133");
                  if (false == EncounterEscape(gi, ref action))
                  {
                     returnStatus = "EncounterEscape() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E134ShakyWalls:
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.E134ShakyWallsEnd:
                  if (null == gi.RuinsUnstable.Find(princeTerritory.Name)) // if this is unstable ruins, it stays unstable ruins
                     gi.RuinsUnstable.Add(princeTerritory);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E134ShakyWallsSearch:
                  if (null == gi.RuinsUnstable.Find(princeTerritory.Name)) // if this is unstable ruins, it stays unstable ruins
                     gi.RuinsUnstable.Add(princeTerritory);
                  gi.SunriseChoice = GamePhase.SearchRuins;
                  gi.GamePhase = GamePhase.Encounter;
                  gi.EventDisplayed = gi.EventActive = "e208";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E136FallingCoins:
                  if (false == gi.AddCoins("GameStateEncounter(E136FallingCoins)", 500))
                  {
                     returnStatus = "AddCoins() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E143ChagaDrugPay:
                  gi.IsChagaDrugProvided = true;
                  --gi.ChagaDrugCount;
                  gi.EventDisplayed = gi.EventActive = gi.EventStart;
                  break;
               case GameAction.E143ChagaDrugDeny:
                  gi.EventDisplayed = gi.EventActive = gi.EventStart;
                  break;
               case GameAction.E143SecretOfTemple:
                  gi.IsSecretTempleKnown = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E144RescueCast:
               case GameAction.E144RescueImpress:
                  GameEngine.theFeatsInGame.myIsRescueHeir = true;
                  gi.EventDisplayed = gi.EventActive = "e144c";
                  gi.IsSecretBaronHuldra = false;
                  if (false == EncounterEscape(gi, ref action)) // move to random hex
                  {
                     returnStatus = "EncounterRoll(): EncounterEscape() returned false ae=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  IMapItem trueHeir = CreateCharacter(gi, "WarriorBoy");
                  gi.AddCompanion(trueHeir);
                  break;
               case GameAction.E144RescueCharm:
                  GameEngine.theFeatsInGame.myIsRescueHeir = true;
                  gi.EventDisplayed = gi.EventActive = "e144c";
                  gi.IsSecretBaronHuldra = false;
                  if (false == EncounterEscape(gi, ref action)) // move to random hex
                  {
                     returnStatus = "EncounterRoll(): EncounterEscape() returned false ae=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  IMapItem trueHeir1 = CreateCharacter(gi, "WarriorBoy");
                  gi.AddCompanion(trueHeir1);
                  gi.IsCharismaTalismanActive = true;
                  break;
               case GameAction.E144RescueKill:
                  GameEngine.theFeatsInGame.myIsRescueHeir = true;
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e144c";
                  gi.IsSecretBaronHuldra = false;
                  if (false == EncounterEscape(gi, ref action)) // move to random hex
                  {
                     returnStatus = "EncounterRoll(): EncounterEscape() returned false ae=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  IMapItem trueHeir2 = CreateCharacter(gi, "WarriorBoy");
                  gi.AddCompanion(trueHeir2);
                  gi.RemoveSpecialItem(SpecialEnum.NerveGasBomb);
                  break;
               case GameAction.E144RescueFight:
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e144b";
                  gi.DieRollAction = GameAction.EncounterStart;
                  action = GameAction.EncounterStart;
                  break;
               case GameAction.E144ContinueNormalAudienceRoll:
                  gi.EventDisplayed = gi.EventActive = "e211c";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E146CountAudienceReroll:
                  gi.EventDisplayed = gi.EventActive = "e161";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.DieResults["e161"][0] = Utilities.NO_RESULT;
                  gi.IsFoulBaneUsedThisTurn = true;
                  break;
               case GameAction.E146StealGems:
                  gi.ForbiddenHexes.Add(princeTerritory);
                  gi.CapturedWealthCodes.Add(110);
                  action = GameAction.EncounterLootStart;
                  if (false == EncounterEscape(gi, ref action)) // move to random hex
                  {
                     returnStatus = "EncounterRoll(): EncounterEscape() returned false ae=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E147ClueToTreasure:
                  if (Utilities.NO_RESULT == gi.DieResults["e147"][0])
                  {
                     gi.DieResults["e147"][0] = dieRoll;
                  }
                  else if (Utilities.NO_RESULT == gi.DieResults["e147"][1])
                  {
                     gi.DieResults["e147"][1] = dieRoll;
                     ITerritory tRamdom = FindRandomHexRangeDirectionAndRange(gi, gi.DieResults["e147"][0], gi.DieResults["e147"][1]);// Find a random hex at the range set by die roll
                     if (null == tRamdom)
                     {
                        returnStatus = "tRamdom=null for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        gi.SecretClues.Add(tRamdom);
                     }
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     gi.DieResults["e147"][0] = Utilities.NO_RESULT;
                     gi.DieResults["e147"][1] = Utilities.NO_RESULT;
                  }
                  break;
               case GameAction.E148SeneschalDeny:
                  gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, Utilities.FOREVER);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E148SeneschalPay:
                  gi.ReduceCoins("E148SeneschalPay", gi.Bribe);
                  gi.Bribe = 0;
                  gi.SeneschalRollModifier = 10;
                  gi.DieResults["e148"][0] = Utilities.NO_RESULT;
                  if (false == ResetDieResultsForAudience(gi))
                  {
                     returnStatus = "ResetDieResultsForAudience() returned false ae=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E152NobleAlly:
                  action = GameAction.EndGameWin;  // GameAction.E152NobleAlly
                  gi.GamePhase = GamePhase.EndGame;
                  gi.EndGameReason = "Noble Ally marches on Northlands!";
                  gi.EventDisplayed = gi.EventActive = "e501";
                  gi.Statistic.myNumWins++;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.Statistic.myEndDaysCount = gi.Days;
                  gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
                  gi.Statistic.myEndCoinCount = gi.GetCoins();
                  gi.Statistic.myEndFoodCount = gi.GetFoods();
                  GameEngine.theFeatsInGame.myIsNobleAllyWin = true;
                  break;
               case GameAction.E153MasterOfHouseholdDeny:
                  gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, Utilities.FOREVER);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E153MasterOfHouseholdPay:
                  gi.ReduceCoins("E153MasterOfHouseholdPay", 10);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E154LordsDaughter:
                  switch (gi.DieResults["e154"][0]) // Based on the die roll, implement event
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, Utilities.FOREVER); break;         // arrested
                     case 3:                                                                                                                 // dally
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e154"][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        break;
                     case 4:                                                                                                                // reserved & pass on
                        gi.DieResults["e154"][0] = Utilities.NO_RESULT;
                        if (false == ResetDieResultsForAudience(gi))
                        {
                           returnStatus = "ResetDieResultsForAudience() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e154"][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        break;
                     case 5:                                                                                                                // she takes a liking to you
                        gi.DaughterRollModifier += 1;
                        gi.DieResults["e154"][0] = Utilities.NO_RESULT;
                        if (false == ResetDieResultsForAudience(gi))
                        {
                           returnStatus = "ResetDieResultsForAudience() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e154"][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        break;
                     case 6:  // not possbile becuase this results in LordsDaughterLove in EventViewer which results in EventLootStart
                     default:
                        returnStatus = "reached default with ae=" + action.ToString() + " dr=" + gi.DieResults["e154"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        break;
                  }
                  break;
               case GameAction.E156MayorAudienceApplyResults:
                  switch (gi.DieResults["e156"][0]) // Based on the die roll, implement event 
                  {
                     case 1:                                                                            // insulted
                        if (true == gi.IsAlcoveOfSendingAudience)
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        else
                        {
                           if ("e130" == gi.EventStart) // arrested by lords on travel
                              gi.EventDisplayed = gi.EventActive = "e130d";
                           else
                              gi.EventDisplayed = gi.EventActive = "e060";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;
                     case 2:                                                                            // stone faced
                        if (true == gi.IsAlcoveOfSendingAudience)
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        else
                        {
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        break;                                                                          // free food and lodging
                     case 3:
                        if (true == gi.IsAlcoveOfSendingAudience)
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e156a";
                           gi.IsPartyFed = true;
                           gi.IsMountsFed = true;
                           gi.IsPartyLodged = true;
                           gi.IsMountsStabled = true;
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case 4:                                                                           // letter of recommendation nearest castle
                        gi.EventDisplayed = gi.EventActive = "e157";
                        ITerritory closetCastle0 = FindClosestCastle(gi);
                        if (null == closetCastle0)
                        {
                           returnStatus = "FindClosestCastle() returned null ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        gi.TargetHex = closetCastle0;
                        gi.LetterOfRecommendations.Add(closetCastle0);
                        gi.ForbiddenAudiences.RemoveLetterGivenConstraints(closetCastle0); // if a letter is given for a Drogat Castle, remove the constraint to have audience
                        ITerritory t156 = FindClosestTown(gi);
                        gi.ForbiddenAudiences.AddTimeConstraint(t156, gi.Days + 6);
                        break;
                     case 5:                                                                             // letter of recommendation nearest castle
                        gi.EventDisplayed = gi.EventActive = "e157";
                        if (false == gi.AddCoins("GameStateEncounter(E156MayorAudienceApplyResults)", 50))
                        {
                           returnStatus = "gi.AddCoins() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        ITerritory closetCastle1 = FindClosestCastle(gi);
                        if (null == closetCastle1)
                        {
                           returnStatus = "FindClosestCastle() returned null ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        gi.TargetHex = closetCastle1;
                        gi.LetterOfRecommendations.Add(closetCastle1);
                        gi.ForbiddenAudiences.RemoveLetterGivenConstraints(closetCastle1); // if a letter is given for a Drogat Castle, remove the constraint to have audience
                        ITerritory t156cb = FindClosestTown(gi);
                        gi.ForbiddenAudiences.AddLetterConstraint(t156cb, closetCastle1);
                        break;
                     case 6: // This is only executed when there is no religion in party and a six is rolled. 
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        break;
                     default:
                        returnStatus = "Reach Default ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        break;
                  }
                  break;
               case GameAction.E155HighPriestAudienceApplyResults:
                  ITerritory t155 = FindClosestTemple(gi);
                  switch (gi.DieResults["e155"][0]) // Based on the die roll, implement event
                  {
                     case 1:                                                                                                  // arrested
                        if (false == gi.IsAlcoveOfSendingAudience)
                        {
                           if ("e130" == gi.EventStart)  //e130 - arrested by Lords on travel
                              gi.EventDisplayed = gi.EventActive = "e130d";
                           else
                              gi.EventDisplayed = gi.EventActive = "e060";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        break;
                     case 2:
                        if (true == gi.IsInTemple(t155))
                           gi.ForbiddenAudiences.AddTimeConstraint(t155, gi.Days + 6);// stone faced
                        if (false == gi.IsAlcoveOfSendingAudience)
                        {
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e155"][0].ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        break;
                     case 3:                                                                                                  // hears pleas
                        if (gi.Days - gi.DayOfLastOffering < 4)
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e155";
                           gi.DieRollAction = GameAction.EncounterRoll;
                           gi.DieResults["e155"][0] = Utilities.NO_RESULT;
                        }
                        else
                        {
                           if (true == gi.IsInTemple(t155))
                              gi.ForbiddenAudiences.AddOfferingConstaint(t155, Utilities.FOREVER);
                           if (false == gi.IsAlcoveOfSendingAudience)
                           {
                              if (false == EncounterEnd(gi, ref action))
                              {
                                 returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e155"][0].ToString();
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                              }
                           }
                           else
                           {
                              gi.EventDisplayed = gi.EventActive = "e042b";
                           }
                        }
                        break;
                     case 4:
                        if (true == gi.IsInTemple(t155))
                           gi.ForbiddenAudiences.AddOfferingConstaint(t155, gi.Days + 1);
                        if (false == gi.IsAlcoveOfSendingAudience)
                        {
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e155"][0].ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        break;
                     case 5:
                        if (true == gi.IsInTemple(t155))
                           gi.ForbiddenAudiences.AddTimeConstraint(t155, Utilities.FOREVER);
                        gi.CapturedWealthCodes.Add(110);
                        action = GameAction.EncounterLootStart;
                        break;
                     case 6:
                        if (true == gi.IsInTemple(t155))
                           gi.ForbiddenAudiences.AddTimeConstraint(t155, Utilities.FOREVER);
                        if (false == gi.AddCoins("GameStateEncounter(E155HighPriestAudienceApplyResults)", 200))
                        {
                           returnStatus = "gi.AddCoins() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        gi.CapturedWealthCodes.Add(110);
                        action = GameAction.EncounterLootStart;
                        break;
                     default:
                        returnStatus = "Reach Default ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        break;
                  }
                  break;
               case GameAction.E156MayorTerritorySelection:
                  break;
               case GameAction.E156MayorTerritorySelectionEnd:
                  gi.EventDisplayed = gi.EventActive = "e157";
                  break;
               case GameAction.E157LetterEnd:
                  if (true == gi.IsAlcoveOfSendingAudience)
                  {
                     gi.EventDisplayed = gi.EventActive = "e042b";
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e160"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E158HostileGuardPay:
                  gi.ReduceCoins("E158HostileGuardPay", 20);
                  if (false == ResetDieResultsForAudience(gi))
                  {
                     returnStatus = "ResetDieResultsForAudience() returned false ae=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  action = GameAction.UpdateEventViewerActive;
                  break;
               case GameAction.E160BrokenLove:
                  IMapItems trueLoves = new MapItems();
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     mi.PlagueDustWound = 0; // assume that healers cure any plague dust
                     if (true == mi.Name.Contains("TrueLove"))
                        trueLoves.Add(mi);
                  }
                  foreach (IMapItem mi in trueLoves)  // True Loves leave since they are heartbroken.
                     gi.RemoveAbandonerInParty(mi);
                  gi.IsTrueLoveHeartBroken = true;
                  //-------------------------------
                  gi.Days += gi.DieResults["e160e"][0]; // number of days pass with you hardbroken
                  if (false == PerformEndCheck(gi, ref action)) // GameStateEncounter.PerformAction(E160BrokenLove)
                  {
                     gi.DieResults["e160"][0] = Utilities.NO_RESULT;
                     gi.DieResults["e160e"][0] = Utilities.NO_RESULT;
                     if (true == gi.IsAlcoveOfSendingAudience)
                     {
                        gi.EventDisplayed = gi.EventActive = "e042b";
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e160"; // Get Audience after end of Broken Love
                        gi.DieRollAction = GameAction.EncounterRoll;
                     }
                  }
                  break;
               case GameAction.E160LadyAudienceApplyResults:  // e160
                  ITerritory t160 = Territory.theTerritories.Find("1923");
                  if (true == gi.IsLadyAeravirRerollActive)
                  {
                     gi.IsLadyAeravirRerollActive = false;
                     gi.ForbiddenHexes.Add(t160); // can never return to this hex.
                  }
                  switch (gi.DieResults["e160"][0]) // Based on the die roll, implement event
                  {
                     case 1:                                                                                                        // not interested
                        gi.ForbiddenAudiences.AddTimeConstraint(t160, Utilities.FOREVER);
                        if (true == gi.IsAlcoveOfSendingAudience)
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        else
                        {
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e160"][0].ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case 2:                                                                                                       // distracted listening
                        if (true == gi.IsAlcoveOfSendingAudience)
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        else
                        {
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e160"][0].ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case 3:                                                                                                         // takes pity
                        gi.ForbiddenAudiences.AddTimeConstraint(t160, Utilities.FOREVER);
                        gi.CapturedWealthCodes.Add(60);
                        action = GameAction.EncounterLootStart;
                        break;
                     case 4:                                                                                                         // find favorable virtue
                        gi.ForbiddenAudiences.AddTimeConstraint(t160, Utilities.FOREVER);
                        gi.IsPartyFed = true;
                        gi.IsMountsFed = true;
                        gi.IsPartyLodged = true;
                        gi.IsMountsStabled = true;
                        gi.IsPartyContinuouslyLodged = true;
                        gi.CapturedWealthCodes.Add(110);
                        action = GameAction.EncounterLootStart;
                        break;
                     default:
                        returnStatus = "Reach Default ae=" + action.ToString() + " gi.DieResults[e160][0]=" + gi.DieResults["e160"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        break;
                  }
                  break;
               case GameAction.E161CountAudienceApplyResults: // already rolled e161 and have an audience - this is audience results
                  ITerritory t161 = Territory.theTerritories.Find("0323"); // Drogat Castle may have a constraint on killing monsters
                  switch (gi.DieResults["e161"][0]) // Based on the die roll, implement event
                  {
                     case 1:                                                                                                          // count victim
                        if (true == gi.IsAlcoveOfSendingAudience)
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        else
                        {
                           if ("e130" == gi.EventStart)
                           {
                              gi.EventDisplayed = gi.EventActive = "e130d";
                              gi.IsArrestedByDrogat = true;   // Jailed on travels results in MarkedForDeath()
                           }
                           else
                           {
                              if (false == MarkedForDeath(gi))
                              {
                                 returnStatus = "MarkedForDeath() returned false ae=" + action.ToString();
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                              }
                           }
                        }
                        break;
                     case 2:
                        if (true == gi.IsAlcoveOfSendingAudience)
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        else
                        {                                                                                                    // half listens
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e161"][0].ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case 3:
                        gi.ForbiddenAudiences.AddLetterGivenConstraint(t161); // must have letter given for this territory to hold audience  // flippant advice
                        if (true == gi.IsAlcoveOfSendingAudience)
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        else
                        {
                           gi.IsMustLeaveHex = true;
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e161"][0].ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case 4:                                                                                                          // interested
                        if (false == gi.AddCoins("PerformAction(E161CountAudienceApplyResults)", 100))
                        {
                           returnStatus = "AddCoins() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        gi.CapturedWealthCodes.Add(110);
                        action = GameAction.EncounterLootStart;
                        break;
                     case 5:
                        if (4 < gi.NumMonsterKill)// needs trophies
                        {
                           if (false == gi.IsAlcoveOfSendingAudience)
                           {
                              if (false == gi.AddNewMountToParty(MountEnum.Pegasus))
                              {
                                 returnStatus = "AddNewMountToParty() returned false for action=" + action.ToString();
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                              }
                              if (false == gi.AddNewMountToParty(MountEnum.Pegasus))
                              {
                                 returnStatus = "AddNewMountToParty() returned false for action=" + action.ToString();
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                              }
                           }
                           if (false == gi.AddCoins("GameStateEncounter(E161CountAudienceApplyResults)", 500))
                           {
                              returnStatus = "AddCoins() returned false for action=" + action.ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                           gi.CapturedWealthCodes.Add(110);

                           gi.ForbiddenAudiences.AddTimeConstraint(t161, Utilities.FOREVER);
                           action = GameAction.EncounterLootStart;
                        }
                        else
                        {
                           gi.ForbiddenAudiences.AddMonsterKillConstraint(t161);
                           if (true == gi.IsAlcoveOfSendingAudience)
                           {
                              gi.EventDisplayed = gi.EventActive = "e042b";
                           }
                           else
                           {
                              if (false == EncounterEnd(gi, ref action))
                              {
                                 returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e161"][0].ToString();
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                              }
                           }
                        }
                        break;
                     case 6: gi.IsNobleAlly = true; break; // you immediately win
                     default:
                        returnStatus = "Reach Default ae=" + action.ToString() + " dr=" + gi.DieResults["e161"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        break;
                  }
                  break;
               case GameAction.E163SlavePorterChange:
                  int porterCost = gi.DieResults["e163"][0];
                  if (true == gi.IsMerchantWithParty)
                     porterCost = (int)Math.Ceiling((double)porterCost * 0.5);
                  if (dieRoll < 0)
                  {
                     if ((1 == gi.PurchasedSlavePorter) && (0 == gi.PurchasedSlaveGirl) && (0 < gi.PurchasedSlaveWarrior)) // This applies only if warrior is purchased. If this is last porter and no slave girl, cost of warrior is more.
                     {
                        if (true == gi.IsMerchantWithParty)
                           porterCost -= 1;
                        else
                           porterCost -= 2;
                        if (porterCost < 0)
                           porterCost = 0;
                     }
                     gi.AddCoins("RemoveVictimInParty", porterCost, false);
                     --gi.PurchasedSlavePorter;
                  }
                  else
                  {
                     if ((0 == gi.PurchasedSlavePorter) && (0 == gi.PurchasedSlaveGirl) && (0 < gi.PurchasedSlaveWarrior)) // when buying porter, if no other porter or girl has been purchased, the cost of the warrior is cheaper
                     {
                        if (true == gi.IsMerchantWithParty)
                           porterCost -= 1;
                        else
                           porterCost -= 2;
                     }
                     gi.ReduceCoins("E163SlavePorterChange", porterCost);
                     ++gi.PurchasedSlavePorter;
                  }
                  break;
               case GameAction.E163SlaveGirlChange:
                  int girlCost = gi.DieResults["e163"][1] + 2;
                  if (true == gi.IsMerchantWithParty)
                     girlCost = (int)Math.Ceiling((double)girlCost * 0.5);
                  if (dieRoll < 0)
                  {
                     if ((0 == gi.PurchasedSlavePorter) && (1 == gi.PurchasedSlaveGirl) && (0 < gi.PurchasedSlaveWarrior)) // when selling salve girl, if no other porter or girl has been purchased, the cost of the warrior is more expensive
                     {
                        if (true == gi.IsMerchantWithParty)
                           girlCost -= 1;
                        else
                           girlCost -= 2;
                        if (girlCost < 0)
                           girlCost = 0;
                     }
                     gi.AddCoins("GameStateEncounter(E128ePotionHealChange)", girlCost, false);
                     --gi.PurchasedSlaveGirl;
                  }
                  else
                  {
                     if ((0 == gi.PurchasedSlavePorter) && (0 == gi.PurchasedSlaveGirl) && (0 < gi.PurchasedSlaveWarrior)) // when buying slave girl, if no other porter or girl has been purchased, the cost of the warrior is cheaper
                     {
                        if (true == gi.IsMerchantWithParty)
                           girlCost -= 1;
                        else
                           girlCost -= 2;
                     }
                     gi.ReduceCoins("E163SlaveGirlChange", girlCost);
                     ++gi.PurchasedSlaveGirl;
                  }
                  break;
               case GameAction.E163SlaveWarriorChange:
                  int warriorCost = gi.DieResults["e163"][2];
                  if (true == gi.IsMerchantWithParty)
                     warriorCost = (int)Math.Ceiling((double)warriorCost * 0.5);
                  if ((0 == gi.PurchasedSlavePorter) && (0 == gi.PurchasedSlaveGirl)) // when buying warrior, if no porter or slave girl is purchased, the warrior cost 2gp extra
                  {
                     if (true == gi.IsMerchantWithParty)
                        warriorCost += 1;
                     else
                        warriorCost += 2;
                  }
                  if (dieRoll < 0)
                  {
                     gi.AddCoins("GameStateEncounter(E163SlaveWarriorChange)", warriorCost, false);
                     --gi.PurchasedSlaveWarrior;
                  }
                  else
                  {
                     gi.ReduceCoins("GameStateEncounter(E163SlaveWarriorChange)", warriorCost);
                     ++gi.PurchasedSlaveWarrior;
                  }
                  break;
               case GameAction.E163SlaveGirlSelected:
                  IMapItem slavegirl = gi.RemoveFedSlaveGirl();
                  if (null == slavegirl)
                  {
                     returnStatus = "Invalid State - Slave Girl not found in SpecialItems";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.EncounteredMembers.Add(slavegirl);
                     gi.IsSlaveGirlActive = true;
                     gi.DieRollAction = GameAction.E182CharmGiftRoll;
                  }
                  break;
               case GameAction.E182CharmGiftSelected:
                  if (false == gi.RemoveSpecialItem(SpecialEnum.GiftOfCharm))
                  {
                     returnStatus = "Invalid State - Gift Charm not found";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  //-----------------------------------------------
                  if (0 == gi.EncounteredMembers.Count) // Find a random encountered character to give the gift charm to - If they die, can recover it
                  {
                     returnStatus = "Invalid State - EncounteredMembers.Count=0 for ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     int randomNum1 = Utilities.RandomGenerator.Next(gi.EncounteredMembers.Count);
                     gi.EncounteredMembers[randomNum1].AddSpecialItemToShare(SpecialEnum.GiftOfCharm);  // giving to encountered character
                     gi.IsGiftCharmActive = true;
                     gi.DieRollAction = GameAction.E182CharmGiftRoll;
                  }
                  break;
               case GameAction.E182CharmGiftRoll:
                  gi.DieRollAction = GameAction.E182CharmGiftRoll;
                  if (Utilities.NO_RESULT == gi.DieResults[gi.EventActive][0])
                     gi.DieResults[gi.EventActive][0] = dieRoll;
                  else if (Utilities.NO_RESULT == gi.DieResults[gi.EventActive][1])
                     gi.DieResults[gi.EventActive][1] = dieRoll;
                  else if (Utilities.NO_RESULT == gi.DieResults[gi.EventActive][2])
                     gi.DieResults[gi.EventActive][2] = dieRoll;
                  break;
               case GameAction.E192PrinceResurrected:
                  gi.RemoveLeaderlessInParty();
                  gi.Prince.Reset();
                  gi.Prince.Endurance = Math.Max(1, gi.Prince.Endurance - 1);
                  gi.Prince.IsResurrected = true;
                  if (0 == gi.PartyMembers.Count) // if prince is removed due to combat and all alone
                     gi.PartyMembers.Add(gi.Prince);
                  if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
                  {
                     returnStatus = "AddMapItemMove() returned false ae=" + gi.EventActive + " action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(E192PrinceResurrected): MovementUsed=Movement for a=" + action.ToString());
                     gi.Prince.MovementUsed = gi.Prince.Movement; // no more travel or today
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd(E192PrinceResurrected) returned false ae=" + gi.EventActive;
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E188TalismanPegasusConversion:
                  if (false == gi.RemoveSpecialItem(SpecialEnum.PegasusMountTalisman))
                  {
                     returnStatus = "RemoveSpecialItem(PegasusMountTalisman) returned false for ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  if (false == gi.AddNewMountToParty(MountEnum.Pegasus))
                  {
                     returnStatus = "AddMount(Pegasus) returned false for ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd(EncounterBribe) returned false ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E188TalismanPegasusSkip:
                  gi.IsPegasusSkip = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd(EncounterBribe) returned false ae=" + gi.EventActive;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E209ThievesGuiildPay:
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E209ThievesGuiildNoPay:
                  if (false == gi.ForbiddenHexes.Contains(princeTerritory)) // cannot return to this hex
                     gi.ForbiddenHexes.Add(princeTerritory);
                  if (false == gi.AddCoins("GameStateEncounter(E209ThievesGuiildNoPay)", 20))
                  {
                     returnStatus = "gi.AddCoins() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E210HireFreeman:
                  IMapItem freeman = CreateCharacter(gi, "Freeman");
                  string freemanName = "Freeman";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     freemanName += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     freemanName += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     freemanName += "Halfling";
                  freeman.Name = freemanName;
                  freeman.Name += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  gi.AddCompanion(freeman);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E210HireLancer:
                  IMapItem lancer = CreateCharacter(gi, "Lancer");
                  string lancerName = "Lancer";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     lancerName += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     lancerName += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     lancerName += "Halfling";
                  lancer.Name = lancerName;
                  lancer.Name += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  lancer.Wages = 3;
                  lancer.AddNewMount();
                  gi.AddCompanion(lancer);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E210HireMerc1:
                  IMapItem merc0 = CreateCharacter(gi, "Mercenary");
                  string merc0Name = "Mercenary";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     merc0Name += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     merc0Name += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     merc0Name += "Halfling";
                  merc0.Name = merc0Name;
                  merc0.Name += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  merc0.Wages = 2;
                  gi.AddCompanion(merc0);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E210HireMerc2:
                  IMapItem merc1 = CreateCharacter(gi, "Mercenary");
                  string merc1Name = "Mercenary";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     merc1Name += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     merc1Name += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     merc1Name += "Halfling";
                  merc1.Name = merc1Name;
                  merc1.Name += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  merc1.Wages = 2;
                  gi.AddCompanion(merc1);
                  IMapItem merc2 = CreateCharacter(gi, "Mercenary");
                  string merc2Name = "Mercenary";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     merc2Name += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     merc2Name += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     merc2Name += "Halfling";
                  merc2.Name = merc2Name;
                  merc2.Name += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  merc2.Wages = 2;
                  gi.AddCompanion(merc2);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E210HireHenchmen:
                  if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                  {
                     action = GameAction.E018MarkOfCain;
                  }
                  else
                  {
                     for (int i = 0; i < gi.PurchasedHenchman; ++i)
                     {
                        IMapItem henchman = CreateCharacter(gi, "Henchman");
                        string henchmanName = "Henchman";
                        if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                           henchmanName += "Elf";
                        if (true == gi.DwarvenMines.Contains(princeTerritory))
                           henchmanName += "Dwarf";
                        if (true == gi.HalflingTowns.Contains(princeTerritory))
                           henchmanName += "Halfling";
                        henchman.Name = henchmanName;
                        henchman.Name += Utilities.MapItemNum.ToString();
                        ++Utilities.MapItemNum;
                        henchman.Wages = 1;
                        gi.AddCompanion(henchman);
                     }
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E210HireLocalGuide:
                  IMapItem hiredLocalGuide = CreateCharacter(gi, "Guide");
                  string localGuideName = "Guide";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     localGuideName += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     localGuideName += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     localGuideName += "Halfling";
                  hiredLocalGuide.Name = localGuideName;
                  hiredLocalGuide.Name += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  hiredLocalGuide.Wages = 2;
                  hiredLocalGuide.IsGuide = true;
                  if (false == AddGuideTerritories(gi, hiredLocalGuide, 2))
                  {
                     returnStatus = "AddGuideTerritories() returned false for action=" + action.ToString() + " mi=" + hiredLocalGuide.Name + " hexes=2";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  gi.AddCompanion(hiredLocalGuide);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E210HireRunaway:
                  IMapItem runaway = CreateCharacter(gi, "Runaway");
                  string runawayName = "Runaway";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     runawayName += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     runawayName += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     runawayName += "Halfling";
                  runaway.Name = runawayName;
                  runaway.Name += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  gi.AddCompanion(runaway);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E211DismissMagicUser:
                  IMapItems magicUsers1 = new MapItems();
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     if (true == mi.IsMagicUser())
                        magicUsers1.Add(mi);
                  }
                  int randNum = Utilities.RandomGenerator.Next(magicUsers1.Count);
                  IMapItem mapItemToRemove = magicUsers1[randNum];
                  gi.RemoveAbandonedInParty(mapItemToRemove);
                  gi.IsMagicUserDismissed = true;
                  break;
               case GameAction.E210HirePorter:
                  for (int i = 0; i < gi.PurchasedPorter; ++i)
                  {
                     IMapItem porter = CreateCharacter(gi, "Porter");
                     string porterName = "Porter";
                     if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                        porterName += "Elf";
                     if (true == gi.DwarvenMines.Contains(princeTerritory))
                        porterName += "Dwarf";
                     if (true == gi.HalflingTowns.Contains(princeTerritory))
                        porterName += "Halfling";
                     porter.Name = porterName;
                     porter.Name += Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     porter.GroupNum = --Utilities.PorterNum;  // porter group must be zero or lower
                     porter.Wages = 1;
                     gi.AddCompanion(porter);
                     //----------------------------  add second porter
                     porter = CreateCharacter(gi, "Porter");
                     porterName = "Porter";
                     if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                        porterName += "Elf";
                     if (true == gi.DwarvenMines.Contains(princeTerritory))
                        porterName += "Dwarf";
                     if (true == gi.HalflingTowns.Contains(princeTerritory))
                        porterName += "Halfling";
                     porter.Name = porterName;
                     porter.Name += Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     porter.GroupNum = Utilities.PorterNum; // belong to the same group num
                     porter.Wages = 0;  // a pair cost 1 gp
                     gi.AddCompanion(porter);
                  }
                  if (0 < gi.PurchasedGuide)
                  {
                     IMapItem hiredLocalGuideWithPorter = CreateCharacter(gi, "Guide");
                     string hiredGuideName1 = "Guide";
                     if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                        hiredGuideName1 += "Elf";
                     if (true == gi.DwarvenMines.Contains(princeTerritory))
                        hiredGuideName1 += "Dwarf";
                     if (true == gi.HalflingTowns.Contains(princeTerritory))
                        hiredGuideName1 += "Halfling";
                     hiredLocalGuideWithPorter.Name = hiredGuideName1;
                     hiredLocalGuideWithPorter.Name += Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     hiredLocalGuideWithPorter.Wages = 2;
                     hiredLocalGuideWithPorter.IsGuide = true;
                     if (false == AddGuideTerritories(gi, hiredLocalGuideWithPorter, 2))
                     {
                        returnStatus = "AddGuideTerritories() returned false for action=" + action.ToString() + " mi=" + hiredLocalGuideWithPorter.Name + " hexes=2";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     gi.AddCompanion(hiredLocalGuideWithPorter);
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E212Temple:
                  ITerritory t212 = princeTerritory;
                  switch (gi.DieResults["e212"][0]) // Based on the die roll, implement event
                  {
                     case 2:
                        gi.AbandonedTemples.Add(t212);
                        if (false == MarkedForDeath(gi))
                        {
                           returnStatus = "MarkedForDeath() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        break;
                     case 3:
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        break;
                     case 4:
                        gi.ForbiddenHires.Add(t212);
                        if (1 < gi.PartyMembers.Count) // must have more than Prince in Party
                        {
                           action = GameAction.E212TempleCurse;
                        }
                        else
                        {
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; break;         // arrested
                     case 6:
                        gi.IsPartyFed = true;
                        gi.IsMountsFed = true;
                        gi.IsPartyLodged = true;
                        gi.IsMountsStabled = true;
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        break;
                     case 7:
                        gi.IsOmenModifier = true;
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        break;
                     case 8:
                        if (false == gi.IsMarkOfCain)
                        {
                           IMapItem monkGuide = CreateCharacter(gi, "MonkGuide");
                           monkGuide.IsGuide = true;
                           if (false == AddGuideTerritories(gi, monkGuide, 1))
                           {
                              returnStatus = "AddGuieTerritories() returned false for action=" + action.ToString() + " mi=" + monkGuide.Name + " hexes=1";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                           gi.AddCompanion(monkGuide);
                           if (false == EncounterEnd(gi, ref action))
                           {
                              returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                              Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                           }
                        }
                        else
                        {
                           action = GameAction.E018MarkOfCain;
                        }
                        break;
                     case 9: gi.EventDisplayed = gi.EventActive = "e147"; gi.DieRollAction = GameAction.E147ClueToTreasure; break;
                     case 10:
                        returnStatus = "Invalid sate with  gi.DieResults[e212][0]=10 for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        break;
                     case 11: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e155"; gi.DieRollAction = GameAction.EncounterRoll; break; //  audience permitted
                     case 12:
                        returnStatus = "Invalid sate with  gi.DieResults[e212][0]=12 for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        break;
                     default:
                        gi.EventActive = gi.EventDisplayed = "e212n";
                        gi.AddSpecialItem(SpecialEnum.StaffOfCommand);
                        if (false == gi.IsMarkOfCain)
                        {
                           IMapItem warriorMonk1 = CreateCharacter(gi, "MonkWarrior");
                           warriorMonk1.AddNewMount();
                           gi.AddCompanion(warriorMonk1);

                           IMapItem warriorMonk2 = CreateCharacter(gi, "MonkWarrior");
                           warriorMonk2.AddNewMount();
                           gi.AddCompanion(warriorMonk2);
                        }
                        else
                        {
                           action = GameAction.E018MarkOfCain;
                        }
                        break;
                  }
                  break;
               case GameAction.E212TempleTenGold:
                  gi.EventDisplayed = gi.EventActive = "e212";
                  gi.IsOfferingModifier = true;
                  gi.ReduceCoins("E212TempleTenGold", 10);
                  break;
               case GameAction.E212TempleRequestClues:
                  gi.EventDisplayed = gi.EventActive = "e212l";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E212TempleRequestInfluence:
                  gi.IsInfluenceModifier = true;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E228ShowTrueLove:
                  break;
               case GameAction.E331DenyFickle:
                  gi.AddCoins("EncounterEnd(E331DenyFickle)", gi.FickleCoin, false);  // Return the fickle share to the party
                  gi.FickleCoin = 0;
                  IMapItems fickleMembers = new MapItems();
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     if (true == mi.IsFickle)
                        fickleMembers.Add(mi);
                  }
                  foreach (IMapItem mi in fickleMembers) // the fickle members disappear
                     gi.RemoveAbandonerInParty(mi);
                  gi.EventDisplayed = gi.EventActive = "e331c";
                  action = GameAction.UpdateEventViewerActive;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.E331PayFickle:
                  if ("e331b" == gi.EventActive)
                  {
                     gi.FickleCoin = 0; // their share disappears forever
                  }
                  else
                  {
                     gi.ReduceCoins("E331PayFickle", gi.Bribe);
                     foreach (IMapItem fickle in gi.EncounteredMembers) // add encountered members to party - reduce coin by bribe amount
                     {
                        fickle.IsFickle = true;
                        gi.AddCompanion(fickle);
                     }
                     gi.EncounteredMembers.Clear();
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E332PayGroup:
                  if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                  {
                     action = GameAction.E018MarkOfCain;
                  }
                  else
                  {
                     gi.ReduceCoins("E332PayGroup", gi.Bribe);
                     int groupNum = Utilities.GroupNum;
                     ++Utilities.GroupNum;
                     foreach (IMapItem groupMember in gi.EncounteredMembers) // add encountered members to party - reduce coin by bribe amount
                     {
                        groupMember.GroupNum = groupNum;
                        groupMember.PayDay = gi.Days + 1; // the group number is the day of the hire
                        groupMember.Wages = 2;
                        gi.AddCompanion(groupMember);
                     }
                     gi.EncounteredMembers.Clear();
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E333DenyHirelings:
                  gi.EventDisplayed = gi.EventActive = "e333a";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  break;
               case GameAction.E333PayHirelings:
                  if (1 == gi.EncounteredMembers.Count)
                  {
                     if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                     {
                        action = GameAction.E018MarkOfCain;
                     }
                     else
                     {
                        IMapItem hireling = gi.EncounteredMembers[0];
                        hireling.Wages = 2;
                        gi.AddCompanion(hireling);
                        gi.ReduceCoins("E333PayHirelings", 2);
                        gi.EncounteredMembers.Clear();
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e333b";
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  break;
               case GameAction.E333HirelingCount:
                  if (gi.EncounteredMembers.Count < dieRoll)  // die roll is the selected button which corresponds to the count of hires
                  {
                     returnStatus = "Invalid state dr=" + dieRoll.ToString() + " > c=" + gi.EncounteredMembers.Count.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     break;
                  }
                  if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                  {
                     action = GameAction.E018MarkOfCain;
                  }
                  else
                  {
                     IMapItems hiredMembers = new MapItems();
                     for (int i = 0; i < dieRoll; ++i)
                     {
                        IMapItem hireling = gi.EncounteredMembers[i];
                        hireling.Wages = 2;
                        hireling.PayDay = gi.Days + 1;
                        gi.AddCompanion(hireling);
                        gi.ReduceCoins("E333HirelingCount", 2);
                        hiredMembers.Add(hireling);
                     }
                     foreach (IMapItem mi in hiredMembers)
                        gi.EncounteredMembers.Remove(mi);
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E334Ally:
                  if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                  {
                     action = GameAction.E018MarkOfCain;
                  }
                  else
                  {
                     foreach (IMapItem ally in gi.EncounteredMembers)
                     {
                        ally.IsAlly = true;
                        gi.AddCompanion(ally);
                     }
                     gi.EncounteredMembers.Clear();
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.E335Escapee:
                  foreach (IMapItem escapee in gi.EncounteredMembers)
                  {
                     escapee.IsTownCastleTempleLeave = true;
                     gi.AddCompanion(escapee);
                  }
                  gi.EncounteredMembers.Clear();
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.E340DenyLooters:
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e340a";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.EncounteredMembers.Clear();
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     if (true == mi.IsLooter)
                        gi.EncounteredMembers.Add(mi);
                  }
                  foreach (IMapItem mi in gi.EncounteredMembers)
                     gi.PartyMembers.Remove(mi);
                  break;
               case GameAction.E340PayLooters:
                  gi.LooterCoin = 0;
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  break;
               default:
                  returnStatus = "777 Reached Default ERROR for a=" + action.ToString() + " p=" + previousPhase.ToString() + " ae=" + gi.EventActive + " es=" + gi.EventStart + " ?? ";
                  break;
            }
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
         {
            sb12.Append("<<<<ERROR2::::::GameStateEncounter.PerformAction(): ");
            sb12.Append(returnStatus);
         }
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("  a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("  dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("  e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("  es="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("  dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      protected bool EncounterStart(IGameInstance gi, ref GameAction action, int dieRoll)
      {
         //--------------------------------------
         ITerritory princeTerritory = gi.Prince.Territory;
         string key = gi.EventStart = gi.EventActive;
         switch (key)
         {
            case "e002a": // Mercenaries
               gi.EncounteredMembers.Clear();
               for (int i = 0; i < dieRoll; ++i)
               {
                  IMapItem mercenanary11 = CreateCharacter(gi, "Mercenary");
                  gi.EncounteredMembers.Add(mercenanary11);
               }
               gi.DieResults[key][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e003a": // Swordsman
            case "e003b": // Swordsman
            case "e003c": // Swordsman
               gi.EventStart = "e003"; // set for EncounterLootStart()
               gi.EncounteredMembers.Clear();
               IMapItem swordsman = CreateCharacter(gi, "Swordsman");
               swordsman.AddNewMount();
               gi.EncounteredMembers.Add(swordsman);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e004": // Mercenaries - EncounterStart
               gi.EncounteredMembers.Clear();
               IMapItem mercenaryLead = CreateCharacter(gi, "MercenaryLead");
               mercenaryLead.AddNewMount();
               gi.EncounteredMembers.Add(mercenaryLead);
               for (int i = 0; i < dieRoll; ++i)
               {
                  IMapItem mercenary = CreateCharacter(gi, "Mercenary");
                  if (dieRoll < 3)
                     mercenary.AddNewMount();
                  gi.EncounteredMembers.Add(mercenary);
               }
               gi.DieResults[key][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e005": // Amazons
               gi.EncounteredMembers.Clear();
               int amazonGroupNum = Utilities.GroupNum;
               ++Utilities.GroupNum;
               for (int i = 0; i < dieRoll + 1; ++i)
               {
                  IMapItem amazon = CreateCharacter(gi, "Amazon");
                  amazon.GroupNum = amazonGroupNum;
                  gi.EncounteredMembers.Add(amazon);
               }
               gi.DieResults[key][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e006a": // Dwarf
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.EncounteredMembers.Clear();
                  IMapItem dwarfLeader = CreateCharacter(gi, "DwarfLead");
                  gi.EncounteredMembers.Add(dwarfLeader);
               }
               else
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 1:
                     case 2:
                     case 3:
                        switch (gi.DwarvenChoice)
                        {
                           case "Talk": gi.EventDisplayed = gi.EventActive = "e006c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Evade": gi.EventDisplayed = gi.EventActive = "e006d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Fight": gi.EventDisplayed = gi.EventActive = "e006e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default choice=" + gi.DwarvenChoice + " ae=" + gi.EventActive); return false;
                        }
                        break;
                     case 4:
                        if (false == gi.IsDwarvenBandSizeSet)
                        {
                           IMapItem dwarfFriend = CreateCharacter(gi, "DwarfW");
                           gi.EncounteredMembers.Add(dwarfFriend);
                        }
                        switch (gi.DwarvenChoice)
                        {
                           case "Talk": gi.EventDisplayed = gi.EventActive = "e006c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Evade": gi.EventDisplayed = gi.EventActive = "e006d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Fight": gi.EventDisplayed = gi.EventActive = "e006e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default choice=" + gi.DwarvenChoice + " ae=" + gi.EventActive); return false;
                        }
                        break;
                     case 5:
                        if (false == gi.IsDwarvenBandSizeSet)
                        {
                           gi.EventDisplayed = gi.EventActive = "e006b";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           switch (gi.DwarvenChoice)
                           {
                              case "Talk": gi.EventDisplayed = gi.EventActive = "e006c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                              case "Evade": gi.EventDisplayed = gi.EventActive = "e006d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                              case "Fight": gi.EventDisplayed = gi.EventActive = "e006e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                              default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default choice=" + gi.DwarvenChoice + " ae=" + gi.EventActive); return false;
                           }
                        }
                        break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e058h"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default dr=" + gi.DieResults[key][0].ToString() + " ae=" + gi.EventActive); return false;
                  }
                  gi.IsDwarvenBandSizeSet = true;
               }
               break;
            case "e007a": // Elf
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsElfWitAndWileActive = true;
                  gi.EncounteredMembers.Clear();
                  IMapItem elfLead = CreateCharacter(gi, "ElfWarrior");
                  gi.EncounteredMembers.Add(elfLead);
               }
               else
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 1:
                        switch (gi.ElvenChoice)
                        {
                           case "Talk": gi.EventDisplayed = gi.EventActive = "e007c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Evade": gi.EventDisplayed = gi.EventActive = "e007d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Fight": gi.EventDisplayed = gi.EventActive = "e007e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default choice=" + gi.ElvenChoice + " ae=" + gi.EventActive); return false;
                        }
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e071d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 3:
                        if (false == gi.IsElvenBandSizeSet)
                        {
                           if (0 == gi.EncounteredMembers.Count)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): invalid state gi.EncounteredMembers.Count=0 with dr=" + gi.ElvenChoice + " ae=" + gi.EventActive);
                              return false;
                           }
                           gi.EncounteredMembers[0].AddSpecialItemToShare(SpecialEnum.NerveGasBomb);
                        }
                        switch (gi.ElvenChoice)
                        {
                           case "Talk": gi.EventDisplayed = gi.EventActive = "e007c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Evade": gi.EventDisplayed = gi.EventActive = "e007d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Fight": gi.EventDisplayed = gi.EventActive = "e007e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default choice=" + gi.ElvenChoice + " ae=" + gi.EventActive); return false;
                        }
                        break;
                     case 4:
                        if (false == gi.IsElvenBandSizeSet)
                        {
                           if (0 == gi.EncounteredMembers.Count)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): invalid state gi.EncounteredMembers.Count=0 with dr=" + gi.ElvenChoice + " ae=" + gi.EventActive);
                              return false;
                           }
                           gi.EncounteredMembers[0].AddSpecialItemToShare(SpecialEnum.CurePoisonVial);
                           IMapItem elfAssistant = CreateCharacter(gi, "ElfAssistant");
                           gi.EncounteredMembers.Add(elfAssistant);
                        }
                        switch (gi.ElvenChoice)
                        {
                           case "Talk": gi.EventDisplayed = gi.EventActive = "e007c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Evade": gi.EventDisplayed = gi.EventActive = "e007d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Fight": gi.EventDisplayed = gi.EventActive = "e007e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default choice=" + gi.ElvenChoice + " ae=" + gi.EventActive); return false;
                        }
                        break;
                     case 5:
                        if (false == gi.IsElvenBandSizeSet)
                        {
                           if (0 == gi.EncounteredMembers.Count)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): invalid state gi.EncounteredMembers.Count=0 with dr=" + gi.ElvenChoice + " ae=" + gi.EventActive);
                              return false;
                           }
                           gi.EncounteredMembers[0].AddSpecialItemToShare(SpecialEnum.HealingPoition);
                        }
                        switch (gi.ElvenChoice)
                        {
                           case "Talk": gi.EventDisplayed = gi.EventActive = "e007c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Evade": gi.EventDisplayed = gi.EventActive = "e007d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Fight": gi.EventDisplayed = gi.EventActive = "e007e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default choice=" + gi.ElvenChoice + " ae=" + gi.EventActive); return false;
                        }
                        break;
                     case 6:
                        if (false == gi.IsElvenBandSizeSet)
                        {
                           IMapItem elfFriend = CreateCharacter(gi, "ElfFriend");
                           gi.EncounteredMembers.Add(elfFriend);
                        }
                        switch (gi.ElvenChoice)
                        {
                           case "Talk": gi.EventDisplayed = gi.EventActive = "e007c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Evade": gi.EventDisplayed = gi.EventActive = "e007d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Fight": gi.EventDisplayed = gi.EventActive = "e007e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default choice=" + gi.ElvenChoice + " ae=" + gi.EventActive); return false;
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default dr=" + gi.DieResults[key][0].ToString() + " ae=" + gi.EventActive); return false;
                  }
                  gi.IsElvenBandSizeSet = true;
               }
               break;
            case "e008": // Halfling
               gi.EncounteredMembers.Clear();
               IMapItem halflingWarrior = CreateCharacter(gi, "HalflingLead");
               gi.EncounteredMembers.Add(halflingWarrior);
               gi.EventDisplayed = gi.EventActive = "e304";
               break;
            case "e008a": // Halfling
               gi.EventStart = "e008";
               gi.EncounteredMembers.Clear();
               IMapItem halflingWarrior1 = CreateCharacter(gi, "HalflingLead");
               gi.EncounteredMembers.Add(halflingWarrior1);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e011b": // peaceful farmer - raid 
               IMapItem farmerPeaceful = CreateCharacter(gi, "Farmer");
               gi.EncounteredMembers.Add(farmerPeaceful);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e012b": // farmer with protector - raid 
               IMapItem farmerWithProtector = CreateCharacter(gi, "FarmerW");
               gi.EncounteredMembers.Add(farmerWithProtector);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e013b": // rich farmer retainer - raid 
               int numRetainers = 4;
               for (int i = 0; i < numRetainers; ++i)
               {
                  IMapItem farmerRetainer = CreateCharacter(gi, "FarmerRetainer");
                  gi.EncounteredMembers.Add(farmerRetainer);
               }
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e013c": // rich farmer - raid 
               gi.EncounteredMembers.Clear();
               IMapItem farmerRich = CreateCharacter(gi, "FarmerRich");
               gi.EncounteredMembers.Add(farmerRich);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e014a": // hostile reapers - freadly approach
               IMapItem reaverHostileBoss0 = CreateCharacter(gi, "ReaverBoss");
               gi.EncounteredMembers.Add(reaverHostileBoss0);
               int numReapers0 = dieRoll + 1;
               for (int i = 0; i < numReapers0; ++i)
               {
                  IMapItem reaver = CreateCharacter(gi, "Reaver");
                  gi.EncounteredMembers.Add(reaver);
               }
               gi.DieResults["e014a"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e014c": // hostile reapers
               IMapItem reaverLead = CreateCharacter(gi, "ReaverLead");
               gi.EncounteredMembers.Add(reaverLead);
               int numReapers1 = dieRoll + 1;
               for (int i = 0; i < numReapers1; ++i)
               {
                  IMapItem reaver = CreateCharacter(gi, "Reaver");
                  gi.EncounteredMembers.Add(reaver);
               }
               gi.DieResults["e014c"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e015a": // Friendly reapers
               IMapItem reaverLeadFriendly = CreateCharacter(gi, "ReaverLead");
               gi.EncounteredMembers.Add(reaverLeadFriendly);
               for (int i = 0; i < dieRoll; ++i)
               {
                  IMapItem reaver = CreateCharacter(gi, "Reaver");
                  gi.EncounteredMembers.Add(reaver);
               }
               gi.IsReaverClanTrade = true;
               gi.DieResults["e015a"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e015c": // friendly reapers raid
               IMapItem reaverLeadFriendlyRaid = CreateCharacter(gi, "ReaverLead");
               gi.EncounteredMembers.Add(reaverLeadFriendlyRaid);
               int numReapers11 = dieRoll + 1;
               for (int i = 0; i < numReapers11; ++i)
               {
                  IMapItem reaver = CreateCharacter(gi, "Reaver");
                  gi.EncounteredMembers.Add(reaver);
               }
               gi.DieResults["e015c"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e016a": // Friendly magician
               gi.IsMagicianProvideGift = true;
               IMapItem magician = CreateCharacter(gi, "Magician");
               gi.EncounteredMembers.Add(magician);
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e017": // Peasant Mob
               IMapItem farmLeader = CreateCharacter(gi, "FarmerMobLeader");
               gi.EncounteredMembers.Add(farmLeader);
               int numFarmers = 2 * dieRoll - 1;
               for (int i = 0; i < numFarmers; ++i)
               {
                  IMapItem farmerMob = CreateCharacter(gi, "FarmerMob");
                  gi.EncounteredMembers.Add(farmerMob);
               }
               gi.EventDisplayed = gi.EventActive = "e017";
               gi.DieResults["e017"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e018a": // priest
            case "e018b": // priest
               gi.EventStart = "e018";
               IMapItem priest = CreateCharacter(gi, "Priest");
               priest.AddNewMount();
               gi.EncounteredMembers.Add(priest);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e020": // Traveling Monk
               IMapItem travelingMonk1 = CreateCharacter(gi, "MonkTraveling");
               gi.EncounteredMembers.Add(travelingMonk1);
               if (4 < dieRoll) // add a second one on die of 5 or 6
               {
                  IMapItem travelingMonk2 = CreateCharacter(gi, "MonkTraveling");
                  gi.EncounteredMembers.Add(travelingMonk2);
               }
               gi.DieResults["e020"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e021": // Warrior Monks
               int numMonkWarriors = (int)Math.Ceiling((double)dieRoll * 0.5);
               for (int i = 0; i < numMonkWarriors; ++i)
               {
                  IMapItem warriorMonk = CreateCharacter(gi, "MonkWarrior");
                  gi.EncounteredMembers.Add(warriorMonk);
               }
               gi.DieResults["e021"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e022": // monks
               gi.EncounteredMembers.Clear();
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
               {
                  case 1:
                  case 2:                                              // hermit monk
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e019";
                     gi.DieRollAction = GameAction.DieRollActionNone;
                     IMapItem hermitMonk = CreateCharacter(gi, "MonkHermit");
                     gi.EncounteredMembers.Add(hermitMonk);
                     break;
                  case 3:
                  case 4:                                              // traveling monk
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e020";
                     gi.DieRollAction = GameAction.EncounterStart;
                     break;
                  case 5:
                  case 6:                                              // warrior monk
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e021";
                     gi.DieRollAction = GameAction.EncounterStart;
                     break; //
                  default: 
                     Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); 
                     return false;
               }
               break;
            case "e023":
               gi.DieResults[key][0] = dieRoll;
               gi.EncounteredMembers.Clear();
               IMapItem wizard = CreateCharacter(gi, "Wizard");
               if (3 < dieRoll)
                  wizard.AddNewMount();
               gi.EncounteredMembers.Add(wizard);
               //--------------------------------
               IMapItem wizardHenchman = CreateCharacter(gi, "WHenchman");
               if (3 < dieRoll)
                  wizardHenchman.AddNewMount();
               gi.EncounteredMembers.Add(wizardHenchman);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e023a": // Wizard
            case "e023b": // Wizard
            case "e023c": // Wizard
               gi.DieResults["e023"][0] = Utilities.NO_RESULT; // avoid problem if two encounters in the same day
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.EventStart = "e023";
               break;
            case "e032":  // ghosts
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EncounteredMembers.Clear();
                  int numGhosts = gi.DieResults[key][0] + 1;
                  for (int i = 0; i < numGhosts; ++i)
                  {
                     IMapItem ghost = CreateCharacter(gi, "Ghost");
                     gi.EncounteredMembers.Add(ghost);
                  }
                  gi.EventDisplayed = gi.EventActive = "e310";  // party is surprised
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e033":  // Warrior Wraiths
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EncounteredMembers.Clear();
                  int numWraiths = gi.DieResults[key][0];
                  if (1 == numWraiths) // always at least two
                     ++numWraiths;
                  for (int i = 0; i < numWraiths; ++i)
                  {
                     IMapItem wraith = CreateCharacter(gi, "Wraith");
                     gi.EncounteredMembers.Add(wraith);
                  }
                  gi.EventDisplayed = gi.EventActive = "e307"; // wraiths attack first
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e034":  // Spectre of Inner Tomb
               gi.EncounteredMembers.Clear();
               IMapItem spectre = CreateCharacter(gi, "Spectre");
               gi.EncounteredMembers.Add(spectre);
               gi.EventDisplayed = gi.EventActive = "e034a";
               break;
            case "e036":  // golem at the gate
               gi.EncounteredMembers.Clear();
               IMapItem golem = CreateCharacter(gi, "Golem");
               gi.EncounteredMembers.Add(golem);
               foreach (IMapItem mi in gi.PartyMembers)  // Prince fights golem alone
               {
                  if ("Prince" != mi.Name)
                     gi.LostPartyMembers.Add(mi);
               }
               gi.PartyMembers.Clear();
               gi.PartyMembers.Add(gi.Prince);
               gi.EventDisplayed = gi.EventActive = "e307"; // encountered strike first in combat
               break;
            case "e045":  // arch of travel
               gi.EventDisplayed = gi.EventActive = "e045b";
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.IsArchTravelKnown = true;
               if (false == gi.Arches.Contains(princeTerritory)) // add arch icon to canvas when UpdateCanvas() is called in GameViewerWindow.cs
                  gi.Arches.Add(princeTerritory); // EncounterStart(e045)
               break;
            case "e046":  // gateway to darkness
               gi.EncounteredMembers.Clear();
               ++gi.GuardianCount;
               IMapItem guardian = CreateCharacter(gi, "Guardian");
               gi.EncounteredMembers.Add(guardian);
               IMapItems membersLeaving = new MapItems(); // make a copy b/c changing container when member departs
               foreach (IMapItem mi in gi.PartyMembers)
                  membersLeaving.Add(mi);
               foreach (IMapItem mi in membersLeaving)
                  gi.RemoveAbandonerInParty(mi);
               gi.Prince.Mounts.Clear();
               gi.EventDisplayed = gi.EventActive = "e307";
               gi.Prince.IsSecretGatewayToDarknessKnown = true; // once known, always known
               break;
            case "e046a":  // Gateway to Darkness
               gi.DieResults["e046a"][0] = Utilities.NO_RESULT;
               gi.EncounteredMembers.Clear();
               ++gi.GuardianCount;
               for (int i = 0; i < gi.GuardianCount; ++i)
               {
                  IMapItem guardian1 = CreateCharacter(gi, "Guardian");
                  gi.EncounteredMembers.Add(guardian1);
               }
               gi.EventDisplayed = gi.EventActive = "e307"; // attacked - encounter strike first
               break;
            case "e047":  // mirror of reversal
               gi.EncounteredMembers.Clear();
               IMapItem mirror = CreateCharacter(gi, "Mirror");
               theNumHydraTeeth = gi.HydraTeethCount; // save off number of teeth b/c cannot use in this battle - return to this value if defeat mirror
               gi.EncounteredMembers.Add(mirror);
               gi.EventDisplayed = gi.EventActive = "e307";
               break;
            case "e048":  // Fugitive 
               gi.DieResults[key][0] = dieRoll;
               gi.EncounteredMembers.Clear();
               switch (gi.DieResults["e048"][0])
               {
                  case 1:
                     IMapItem swordswoman = CreateCharacter(gi, "Swordswoman");
                     swordswoman.IsFugitive = true;
                     gi.EncounteredMembers.Add(swordswoman);
                     break;
                  case 2:
                     IMapItem runaway = CreateCharacter(gi, "Runaway");
                     runaway.IsFugitive = true;
                     gi.EncounteredMembers.Add(runaway);
                     break;
                  case 3:
                     if (true == gi.IsMarkOfCain)
                     {
                        action = GameAction.E018MarkOfCain;
                     }
                     else
                     {
                        IMapItem priest3 = CreateCharacter(gi, "Priest");
                        priest3.IsFugitive = true;
                        gi.EncounteredMembers.Add(priest3);
                     }
                     break;
                  case 4:
                     IMapItem magician4 = CreateCharacter(gi, "MagicianWeak");
                     magician4.IsFugitive = true;
                     gi.EncounteredMembers.Add(magician4);
                     break;
                  case 5:
                     IMapItem merchant5 = CreateCharacter(gi, "Merchant");
                     merchant5.IsFugitive = true;
                     gi.EncounteredMembers.Add(merchant5);
                     break;
                  case 6:
                     IMapItem deserter = CreateCharacter(gi, "Deserter");
                     deserter.IsFugitive = true;
                     gi.EncounteredMembers.Add(deserter);
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): reached default dieRoll=" + dieRoll.ToString() + " ae=" + gi.EventActive);
                     return false;
               }
               break;
            case "e049":
               if (0 == gi.GetFoods())
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Invalid state GetFoods()=0 for ae=" + gi.EventActive);
                  return false;
               }
               gi.ReduceFoods(1);
               gi.IsMinstrelPlaying = true;
               IMapItem minstrel = CreateCharacter(gi, "Minstrel");
               gi.EncounteredMinstrels.Add(minstrel);
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd(0 returned false ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e050":
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.VisitedLocations.Contains(princeTerritory))
                  {
                     if (false == gi.EscapedLocations.Contains(princeTerritory))
                        theConstableRollModifier = 2;
                  }
                  else
                  {
                     theConstableRollModifier = 1;
                  }
                  gi.DieResults[key][0] = dieRoll;
                  action = GameAction.EncounterStart;
               }
               else if (Utilities.NO_RESULT == gi.DieResults[key][1])
               {
                  gi.EncounteredMembers.Clear();
                  int numOfConstabulary = dieRoll;
                  if (4 < gi.DieResults[key][0])
                     ++numOfConstabulary;    // mounted
                  else
                     numOfConstabulary += 3; // on foot
                  for (int i = 0; i < numOfConstabulary; ++i)
                  {
                     IMapItem constabulary = CreateCharacter(gi, "Constabulary");
                     if (4 < gi.DieResults[key][0])
                        constabulary.AddNewMount();
                     gi.EncounteredMembers.Add(constabulary);
                     gi.DieResults[key][1] = dieRoll;
                  }
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): INvalid State ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e050e": // local constabulary
               gi.IsGuardEncounteredThisTurn = true;
               int encounterResult = dieRoll - 3;
               if (("0101" == princeTerritory.Name) || ("1501" == princeTerritory.Name))
                  ++encounterResult;
               if (0 < encounterResult)
               {
                  gi.EventDisplayed = gi.EventActive = "e002a"; // next screen to show
                  gi.GamePhase = GamePhase.Encounter;
                  gi.DieRollAction = GameAction.EncounterStart;
               }
               else
               {
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventActive);
                     return false;
                  }
               }
               break;
            case "e051":  // Bandits
               gi.EncounteredMembers.Clear();
               IMapItem banditLeader = CreateCharacter(gi, "BanditLeader");
               gi.EncounteredMembers.Add(banditLeader);
               int numBandits = 1; // leader plus one extra exceeds party count by two
               foreach (IMapItem mi in gi.PartyMembers) // non-fighting party members do not bring more bandits
               {
                  if ((true == mi.Name.Contains("TrueLove")) && (0 == mi.Combat))
                     continue;
                  if ((false == mi.Name.Contains("Slave")) && (false == mi.Name.Contains("Porter")) && (false == mi.Name.Contains("Minstrel")) && (false == mi.Name.Contains("Falcon")))
                     numBandits++; // add a bandit for this party member
               }
               for (int i = 0; i < numBandits; ++i)
               {
                  IMapItem bandit1 = CreateCharacter(gi, "Bandit");
                  gi.EncounteredMembers.Add(bandit1);
               }
               gi.EventDisplayed = gi.EventActive = "e310"; // party is surprised
               break;
            case "e052": // Goblins
               IMapItem hobgoblin = CreateCharacter(gi, "HobgoblinW");
               gi.EncounteredMembers.Add(hobgoblin);
               gi.DieResults["e052"][0] = dieRoll;
               int numGoblins = dieRoll - 1;
               for (int i = 0; i < numGoblins; ++i)
               {
                  IMapItem goblin = CreateCharacter(gi, "Goblin");
                  gi.EncounteredMembers.Add(goblin);
               }
               break;
            case "e054b": // Goblin Tower Fight - EncounterStart()
               if (null == gi.GoblinKeeps.Find(princeTerritory.Name))
                  gi.GoblinKeeps.Add(princeTerritory);
               gi.EncounteredMembers.Clear();
               IMapItem hobgoblin1 = CreateCharacter(gi, "Hobgoblin");
               gi.EncounteredMembers.Add(hobgoblin1);
               gi.DieResults["e054b"][0] = dieRoll;
               int numGoblins1 = dieRoll - 1;
               for (int i = 0; i < numGoblins1; ++i)
               {
                  IMapItem goblin = CreateCharacter(gi, "Goblin");
                  gi.EncounteredMembers.Add(goblin);
               }
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e055": // Orcs
               gi.DieResults["e055"][0] = dieRoll;
               gi.EncounteredMembers.Clear();
               IMapItem orcChief = CreateCharacter(gi, "OrcChief");
               gi.EncounteredMembers.Add(orcChief);
               int numOrcsInBand = dieRoll - 1;
               for (int i = 0; i < numOrcsInBand; ++i)
               {
                  IMapItem orc = CreateCharacter(gi, "Orc");
                  gi.EncounteredMembers.Add(orc);
               }
               break;
            case "e056a": // Orc Tower
               if (null == gi.OrcTowers.Find(gi.NewHex.Name))
                  gi.OrcTowers.Add(princeTerritory);
               gi.EncounteredMembers.Clear();
               IMapItem demiTroll = CreateCharacter(gi, "OrcDemi");
               gi.EncounteredMembers.Add(demiTroll);
               gi.DieResults["e056a"][0] = dieRoll;
               int numOrcs = dieRoll + 1;
               for (int i = 0; i < numOrcs; ++i)
               {
                  IMapItem orc = CreateCharacter(gi, "OrcW");
                  gi.EncounteredMembers.Add(orc);
               }
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e057": // troll
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.EncounteredMembers.Clear();
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EncounteredMembers.Clear();
                  IMapItem troll = CreateCharacter(gi, "Troll");
                  gi.EncounteredMembers.Add(troll);
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (gi.WitAndWile < gi.DieResults[key][0])
                     gi.EventDisplayed = gi.EventActive = "e307";
                  else
                     gi.EventDisplayed = gi.EventActive = "e304";
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e058": // Band of Dwarves
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.EncounteredMembers.Clear();
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  int numDwarves = gi.DieResults[key][0] + 1;
                  for (int i = 0; i < numDwarves; ++i)
                  {
                     IMapItem dwarf = CreateCharacter(gi, "Dwarf");
                     gi.EncounteredMembers.Add(dwarf);
                  }
                  if (gi.PartyMembers.Count < numDwarves)
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e058a";
                  else
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e058b";
               }
               break;
            case "e059": // dwarven mines
               if (null == gi.DwarvenMines.Find(princeTerritory.Name))
                  gi.DwarvenMines.Add(princeTerritory);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e065": // hidden town
               ITerritory t065 = gi.NewHex;
               if (null == t065)
                  t065 = princeTerritory;
               if (null == gi.HiddenTowns.Find(t065.Name))  // if this is hidden town, save it for future reference
                  gi.HiddenTowns.Add(t065);
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventStart);
                  return false;
               }
               break;
            case "e066": // secret temple
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e067": // abandoned mines
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e059"; gi.DieRollAction = GameAction.EncounterRoll; break; // dwarven mines
                     case 2: gi.EventDisplayed = gi.EventActive = "e051"; break;                                              // bandits
                     case 3:
                        gi.EventDisplayed = gi.EventActive = "e046b";                                                         // gateway of darkness 
                        foreach (IMapItem mi in gi.PartyMembers) // e046b indicates unrecognized - unless follower knows secret
                        {
                           if (true == mi.IsSecretGatewayToDarknessKnown)
                              gi.EventDisplayed = gi.EventActive = "e046";
                        }
                        break;
                     case 4:                                                                                                 // arch of travel
                        if (true == gi.IsMagicInParty())
                           gi.EventDisplayed = gi.EventActive = "e045";                                                      // arch travel
                        else
                           gi.EventDisplayed = gi.EventActive = "e045a";
                        break;
                     case 5: case 6: gi.EventDisplayed = gi.EventActive = "e028"; break;                                      // cave tombs
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
               }
               break;
            case "e068": // wizard abode
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.EncounteredMembers.Clear();
                  IMapItem wizard2 = CreateCharacter(gi, "Wizard");
                  gi.EncounteredMembers.Add(wizard2);
               }
               else
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e023"; gi.DieRollAction = GameAction.EncounterStart; break;
                     case 3:
                        if (true == gi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e024c";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e024";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;  // wizard attack
                     case 4:
                        gi.EventDisplayed = gi.EventActive = "e068a";
                        gi.DieRollAction = GameAction.EncounterRoll;
                        break;
                     case 5:
                     case 6:
                        gi.EventDisplayed = gi.EventActive = "e068b";
                        if (null == gi.WizardTowers.Find(princeTerritory.Name))
                           gi.WizardTowers.Add(princeTerritory);
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): reached default dieroll=" + dieRoll.ToString() + " ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e071": // elves - EncounterStart()
               if (0 == gi.NumMembersBeingFollowed)
               {
                  gi.EncounteredMembers.Clear();
                  ++dieRoll;
                  gi.DieResults["e071"][0] = dieRoll;
                  gi.NumMembersBeingFollowed = dieRoll;
                  for (int i = 0; i < dieRoll; ++i)
                  {
                     IMapItem elf = CreateCharacter(gi, "Elf");
                     gi.EncounteredMembers.Add(elf);
                  }
               }
               break;
            case "e071a": // elves
            case "e071b": // elves
            case "e071c": // elves
               gi.EventStart = "e071";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e072": // elven band - EncounterStart()
               gi.IsElfTalkActive = false;
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e071";
               gi.DieRollAction = GameAction.EncounterStart;
               break;
            case "e072a": // elves - EncounterStart()
               if (0 == gi.NumMembersBeingFollowed)
               {
                  gi.EncounteredMembers.Clear();
                  ++dieRoll;
                  gi.DieResults["e072a"][0] = dieRoll;
                  gi.NumMembersBeingFollowed = dieRoll;
                  for (int i = 0; i < dieRoll; ++i)
                  {
                     IMapItem elf = CreateCharacter(gi, "Elf");
                     gi.EncounteredMembers.Add(elf);
                  }
               }
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e073":  // witch
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EncounteredMembers.Clear();
                  IMapItem witch = CreateCharacter(gi, "Witch");
                  gi.EncounteredMembers.Add(witch);
                  if (gi.WitAndWile < gi.DieResults[key][0])
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e073a";
                  else if (gi.WitAndWile == gi.DieResults[key][0])
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e073b";
                  else
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e073c";
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e074":  // spiders
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EncounteredMembers.Clear();
                  int numSpiders = gi.DieResults[key][0];
                  for (int i = 0; i < numSpiders; ++i)
                  {
                     IMapItem spider = CreateCharacter(gi, "Spider");
                     gi.EncounteredMembers.Add(spider);
                  }
                  gi.EventDisplayed = gi.EventActive = "e309";  // party may be surprised
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e075b":  // wolves
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EncounteredMembers.Clear();
                  int numWolves = gi.DieResults[key][0];
                  for (int i = 0; i < numWolves; ++i)
                  {
                     IMapItem wolf = CreateCharacter(gi, "Wolf");
                     gi.EncounteredMembers.Add(wolf);
                  }
                  gi.EventDisplayed = gi.EventActive = "e309";  // party may be surprised
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e078":  // bad going
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  int numLostMounts = gi.DieResults[key][0] - 3;
                  for (int i = 0; i < numLostMounts; ++i)
                     gi.ReduceMount(MountEnum.Horse); // only horses are lost
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
                  if (0 < numLostMounts)
                  {
                     action = GameAction.E078BadGoingRedistribute;
                     gi.EventAfterRedistribute = "e078";
                  }
                  else // no horses are lost - end encounter
                  {
                     --gi.Prince.MovementUsed; // ensure there is a MapItemMove in GameInstance so that EncounterEnd() does not error
                     if (false == AddMapItemMove(gi, gi.Prince.Territory)) // move to same hex
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): AddMapItemMove() returned false ae=" + gi.EventStart);
                        return false;
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "EncounterStart(): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString() + " for k=" + key);
                     }
                     ++gi.Prince.MovementUsed;
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventStart);
                        return false;
                     }
                  }
               }
               break;
            case "e078c":  // bad going
               gi.IsBadGoing = true;
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventStart);
                  return false;
               }
               break;
            case "e080":  // pixies
               if (true == gi.IsPixieLoverInParty())
               {
                  gi.EventDisplayed = gi.EventActive = "e080a";
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e080b";
               }
               break;
            case "e081":  // mounted patrol
               gi.EventStart = "e081";
               gi.EncounteredMembers.Clear();
               IMapItem patrolLead = CreateCharacter(gi, "PatrolMountedLead");
               patrolLead.AddNewMount();
               gi.EncounteredMembers.Add(patrolLead);
               for (int i = 0; i < dieRoll; ++i)
               {
                  IMapItem patrol = CreateCharacter(gi, "PatrolMounted");
                  patrol.AddNewMount();
                  gi.EncounteredMembers.Add(patrol);
               }
               gi.DieResults[key][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e084b":  // bear
               gi.EncounteredMembers.Clear();
               IMapItem bear = CreateCharacter(gi, "Bear");
               gi.EncounteredMembers.Add(bear);
               gi.EventDisplayed = gi.EventActive = "e307";  // bear strikes first
               break;
            case "e091": // snake
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e093": // poison plant
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.IsPoisonPlant = true;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e034"; break;                                               // spectre of the inner tomb
                     case 2: gi.EventDisplayed = gi.EventActive = "e032"; gi.DieRollAction = GameAction.EncounterStart; break; // ghosts
                     case 3: gi.EventDisplayed = gi.EventActive = "e033"; gi.DieRollAction = GameAction.EncounterStart; break; // warrior wraiths
                     case 4: gi.EventDisplayed = gi.EventActive = "e074"; gi.DieRollAction = GameAction.EncounterStart; break; // spiders
                     case 5:
                     case 6:                                                                                           // nothing
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
               }
               break;
            case "e094": // crocodiles in swamp
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EncounteredMembers.Clear();
                  for (int i = 0; i < gi.DieResults[key][0]; ++i)
                  {
                     IMapItem crocodile = CreateCharacter(gi, "Croc");
                     gi.EncounteredMembers.Add(crocodile);
                  }
                  gi.EventDisplayed = gi.EventActive = "e310";
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e094a": // crocodiles in river
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EncounteredMembers.Clear();
                  for (int i = 0; i < gi.DieResults[key][0]; ++i)
                  {
                     IMapItem crocodile = CreateCharacter(gi, "Croc");
                     gi.EncounteredMembers.Add(crocodile);
                  }
                  gi.EventDisplayed = gi.EventActive = "e307";
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e098": // Dragon
               IMapItem dragon = CreateCharacter(gi, "Dragon");
               gi.EncounteredMembers.Add(dragon);
               gi.DieResults[key][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e099a": // Roc
            case "e099b": // Roc
               gi.EventStart = "e099";
               gi.EncounteredMembers.Clear();
               IMapItem roc = CreateCharacter(gi, "Roc");
               gi.EncounteredMembers.Add(roc);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e100a": // griffon
            case "e100b": // griffon
            case "e100c": // griffon
               gi.EventStart = "e100"; // assign for loot purposes
               gi.EncounteredMembers.Clear();
               IMapItem griffon = CreateCharacter(gi, "Griffon");
               griffon.IsFlying = true;
               griffon.IsRiding = true;
               gi.EncounteredMembers.Add(griffon);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e101a": // harpy
            case "e101b": // harpy
            case "e101c": // harpy
               gi.EventStart = "e101";  // assign for loot purposes
               gi.EncounteredMembers.Clear();
               IMapItem harpy = CreateCharacter(gi, "Harpy");
               harpy.IsFlying = true;
               harpy.IsRiding = true;
               gi.EncounteredMembers.Add(harpy);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e105":
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e105a":
               gi.DieRollAction = GameAction.EncounterStart;
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  gi.DieResults[key][0] = dieRoll;
               else if (Utilities.NO_RESULT == gi.DieResults[key][1])
                  gi.DieResults[key][1] = dieRoll;
               else
                  gi.DieResults[key][2] = dieRoll;
               break;
            case "e106":
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e108": // hawkmen - EncounterStart()
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.EncounteredMembers.Clear();
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  for (int i = 0; i < gi.DieResults[key][0]; ++i)
                  {
                     IMapItem hawkman = CreateCharacter(gi, "Hawkman");
                     hawkman.IsFlying = true;
                     hawkman.IsRiding = true;
                     gi.EncounteredMembers.Add(hawkman);
                  }
                  gi.EventDisplayed = gi.EventActive = "e310";
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e110a": // air spirit
               gi.EncounteredMembers.Clear();
               gi.DieRollAction = GameAction.EncounterRoll;
               if (dieRoll < gi.WitAndWile)
                  gi.EventDisplayed = gi.EventActive = "e110c"; // succeed
               else
                  gi.EventDisplayed = gi.EventActive = "e110b"; // fail
               break;
            case "e111": // storm demon attack
               gi.IsAirborne = false;
               gi.DieResults[key][0] = dieRoll;
               gi.Prince.SetWounds(dieRoll, 0);
               IMapItems lostInStormMembers = new MapItems();
               foreach (IMapItem mi in gi.PartyMembers)
                  lostInStormMembers.Add(mi);
               foreach (IMapItem mi in lostInStormMembers)
                  gi.RemoveAbandonedInParty(mi);
               gi.Prince.RemoveUnmountedMounts();
               gi.Prince.RemoveMountedMount();
               if (false == EncounterEscape(gi, ref action)) // move to random hex
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEscape() returned false ae=" + action.ToString());
                  return false;
               }
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e111a": // storm demon
               gi.EncounteredMembers.Clear();
               gi.DieRollAction = GameAction.EncounterRoll;

               if (dieRoll < gi.WitAndWile)
                  gi.EventDisplayed = gi.EventActive = "e110c"; // succeed
               else
                  gi.EventDisplayed = gi.EventActive = "e110b"; // fail
               break;
            case "e112": // eagles - EncounterStart()
               gi.EncounteredMembers.Clear();
               gi.DieResults["e112"][0] = dieRoll;
               for (int i = 0; i < dieRoll; ++i)
               {
                  IMapItem eagle = CreateCharacter(gi, "Eagle");
                  eagle.IsFlying = true;
                  eagle.IsRiding = true;
                  gi.EncounteredMembers.Add(eagle);
               }
               break;
            case "e112a": // eagles
            case "e112b": // eagles
            case "e112c": // eagles
               gi.DieResults["e112"][0] = Utilities.NO_RESULT; // avoid problem if two encounters in the same day
               gi.EventStart = "e112";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e118a": // giant
            case "e118b": // giant
            case "e118c": // giant
               gi.DieResults["e118"][0] = Utilities.NO_RESULT; // avoid problem if two encounters in the same day
               gi.EventStart = "e118";
               gi.EncounteredMembers.Clear();
               IMapItem giant = CreateCharacter(gi, "Giant");
               gi.EncounteredMembers.Add(giant);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e123b": // Black Knight
               gi.EncounteredMembers.Clear();
               IMapItem blackKnight = CreateCharacter(gi, "KnightBlack");
               gi.EncounteredMembers.Add(blackKnight);
               gi.EventDisplayed = gi.EventActive = "e304";
               break;
            case "e124": // make raft
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e125": // Raft Overtuns
               if (false == gi.RemoveBelongingsInParty(false))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEscape() return false for ae=" + gi.EventActive);
                  return false;
               }
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e127": // Raft in Rough Water
               foreach (IMapItem mi in gi.PartyMembers) // all food is lost overboard
                  mi.Food = 0;
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e128":
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e129": // merchant caravan
               Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterStart(): MovementUsed=Movement for a=" + action.ToString() + "ae=" + gi.EventActive);
               gi.Prince.MovementUsed = gi.Prince.Movement; // halt movement for the day if talk with merchant caravan
               gi.DieResults[key][0] = dieRoll;
               if (7 == dieRoll) // this is a dice roll of two die
               {
                  action = GameAction.E129EscapeGuards;
                  if (false == EncounterEscape(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEscape() return false for ae=" + gi.EventActive);
                     return false;
                  }
               }
               break;
            case "e130": // high lords 
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.EncounteredMembers.Clear();
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.DieResults[key][1] = dieRoll;  // This die roll determines high lord
                  int highLordGuardNum = gi.DieResults[key][0] + 1;
                  for (int i = 0; i < highLordGuardNum; ++i)
                  {
                     IMapItem guard = CreateCharacter(gi, "Guard");
                     gi.EncounteredMembers.Add(guard);
                  }
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               break;
            case "e144b": // Hill Tribe Fight
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.EncounteredMembers.Clear();
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e300"; // surprise attack
                  int hillTribeGuardNum = gi.DieResults[key][0] + 1; // roll three die and add 1
                  for (int i = 0; i < hillTribeGuardNum; ++i)
                  {
                     IMapItem guard = CreateCharacter(gi, "GuardHeir");
                     gi.EncounteredMembers.Add(guard);
                  }
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               break;
            case "e144i": // Huldra Bodyguard Fight
               gi.EventDisplayed = gi.EventActive = "e300"; // surprise attack
               gi.EncounteredMembers.Clear();
               for (int i = 0; i < 6; ++i)
               {
                  IMapItem guard = CreateCharacter(gi, "GuardBody");
                  gi.EncounteredMembers.Add(guard);
               }
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case "e144j": // Huldra Fight
               gi.EventDisplayed = gi.EventActive = "e307"; // surprise attack
               gi.EncounteredMembers.Clear();
               IMapItem huldra = CreateCharacter(gi, "Huldra"); // Now need to fight Huldra
               gi.EncounteredMembers.Add(huldra);
               break;
            case "e158": // hostile guards
               gi.EncounteredMembers.Clear();
               IMapItem guardHostile1 = CreateCharacter(gi, "GuardHostile");
               IMapItem guardHostile2 = CreateCharacter(gi, "GuardHostile");
               gi.EncounteredMembers.Add(guardHostile1);
               gi.EncounteredMembers.Add(guardHostile2);
               gi.EventDisplayed = gi.EventActive = "e307";
               //-----------------------------------------
               foreach (IMapItem mi in gi.PartyMembers) // Prince in Atrium fighting hostile guards alone
               {
                  if ("Prince" != mi.Name)
                     gi.LostPartyMembers.Add(mi);
               }
               gi.Bribe = 20;
               Logger.Log(LogEnum.LE_BRIBE, "EncounterStart(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
               gi.PartyMembers.Clear();
               gi.PartyMembers.Add(gi.Prince);
               break;
            case "e164": // giant lizard
               gi.EncounteredMembers.Clear();
               IMapItem lizard = CreateCharacter(gi, "Lizard");
               gi.EncounteredMembers.Add(lizard);
               gi.EventDisplayed = gi.EventActive = "e304";
               break;
            case "e165": // elf town
               if (true == gi.IsInMapItems("Elf"))
                  --dieRoll;
               else if (true == gi.IsMagicInParty())
                  --dieRoll;
               if (true == gi.IsInMapItems("Dwarf"))
                  ++dieRoll;
               gi.DieResults[key][0] = dieRoll;
               if (false == gi.ElfTowns.Contains(princeTerritory))
                  gi.ElfTowns.Add(princeTerritory);
               break;
            case "e166": // elf castle - EncounterStart()
               if (true == gi.IsInMapItems("Elf"))
                  --dieRoll;
               else if (true == gi.IsMagicInParty())
                  --dieRoll;
               if (true == gi.IsInMapItems("Dwarf"))
                  ++dieRoll;
               gi.DieResults[key][0] = dieRoll;
               if (false == gi.ElfCastles.Contains(princeTerritory))
                  gi.ElfCastles.Add(princeTerritory);
               break;
            case "e205c": // lost in air
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  if (3 < dieRoll) // if dieroll greater than 3, drift to adjacent hext
                  {
                     if (false == EncounterEscape(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEscape() returned false ae=" + gi.EventActive);
                        return false;
                     }
                  }
               }
               else
               {
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterStart(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // End of the day
                  if (false == EncounterEnd(gi, ref action)) // EncounterAbandon is end of day action. It sets the next state
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventActive);
                     return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): !!!!! Reached default ae=" + gi.EventActive); return false;
         }
         return true;
      }
      //---------------------I-----------------------
      protected bool EncounterAbandon(IGameInstance gi, ref GameAction action)
      {
         gi.IsAirborne = false;
         IMapItems adbandonedMembers = new MapItems();
         switch (gi.EventActive)
         {
            case "e313a": // flying adbandon
               foreach (IMapItem mi in gi.PartyMembers)
               {
                  if (false == mi.IsFlying)
                     adbandonedMembers.Add(mi);
               }
               break;
            default: // do nothing
               foreach (IMapItem mi in gi.PartyMembers)
               {
                  if ((false == mi.IsRiding) && (false == mi.IsFlyer())) // flyers cannot be abandoned due to riding away since they are always riding 
                     adbandonedMembers.Add(mi);
               }
               break;
         }
         foreach (IMapItem abandoned in adbandonedMembers)
            gi.RemoveAbandonedInParty(abandoned);
         //-----------------------------------------
         IMapItems fickleMembers = new MapItems(); // fickle memebers leave if anybody is abandoned
         foreach (IMapItem mi in gi.PartyMembers)
         {
            if (true == mi.IsFickle)
               fickleMembers.Add(mi);
         }
         foreach (IMapItem fickle in fickleMembers)
            gi.RemoveAbandonerInParty(fickle);
         //-----------------------------------------
         if (false == EncounterEscape(gi, ref action))
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterAbandon(): EncounterEscape() returned false ae=" + gi.EventActive);
            return false;
         }
         if (false == EncounterEnd(gi, ref action)) // EncounterAbandon is end of day action. It sets the next state
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterAbandon(): EncounterEnd() returned false ae=" + gi.EventActive);
            return false;
         }
         return true;
      }
      protected bool EncounterLootStart(IGameInstance gi, ref GameAction action, int dieRoll)
      {
         Option autoWealthOption = gi.Options.Find("AutoWealthRollForUnderFive");
         if (null == autoWealthOption)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): returned option=null");
            return false;
         }
         gi.ProcessIncapacitedPartyMembers(gi.EventActive);
         //----------------------------------------------
         string key = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (key) // End of combat always causes EncounterLootStart Action
         {
            case "e002a": // defeated mercenaries 
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e003": break; // swordman
            case "e004": // Mercenaries - EncounterLootStart
            case "e005": // Amazons
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e006":  // Dwarf Warrior
            case "e006a": // Dwarf Warrior
            case "e006c": // Dwarf Warrior
            case "e006d": // Dwarf Warrior
            case "e006e": // Dwarf Warrior
            case "e006i": // Dwarf Warrior
            case "e007":  // Elf Lead
            case "e007a": // Elf Lead
            case "e007c": // Elf Lead
            case "e007d": // Elf Lead
            case "e007e": // Elf Lead
               break;
            case "e008": // halfing warrior
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e011b": // farmer with protector
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               gi.EventDisplayed = gi.EventActive = "e011c";
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterRoll;
               return true;
            case "e012b": // farmer with protector
               gi.EventDisplayed = gi.EventActive = "e040";
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterStart;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e013b": // defeated farm retainers
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               gi.EventDisplayed = gi.EventActive = "e013c";
               action = GameAction.EncounterStart;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e013c": break; // defeated rich farmers
            case "e014a": // defeated reavers
            case "e014c": // defeated reavers
            case "e015a": // defeated reavers
            case "e015c": // defeated reavers
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               gi.DieResults[key][0] = Utilities.NO_RESULT;
               break;
            case "e016a": break; // friendly magician
            case "e016c": // hostile magician               
               switch (gi.DieResults["e016d"][0])
               {
                  case 1: gi.CapturedWealthCodes.Add(5); break; // might have extra possession 
                  case 2: gi.CapturedWealthCodes.Add(25); break;
                  case 3: gi.CapturedWealthCodes.Add(25); break;
                  case 4: gi.CapturedWealthCodes.Add(60); break; // might have extra possession  
                  case 5: gi.CapturedWealthCodes.Add(60); break;
                  case 6: gi.CapturedWealthCodes.Add(110); break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): Reached default dr=" + dieRoll.ToString() + " ae=" + gi.EventActive + " es=" + gi.EventStart.ToString()); return false;
               }
               break;
            case "e017": // defeat peasant mob
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e018": // priest
               break;
            case "e019": // defeated hermit monk
            case "e020": // defeated traveling monk
            case "e021": // defeated warrior monks
            case "e023": // defeated wizard
            case "e024c": // defeated wizard
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e027": // defeated golem or secret clue found treasure - Already added in E027AncientTreasure
               break;
            case "e032": // defeated ghosts
               gi.EventDisplayed = gi.EventActive = "e032a";
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterRoll;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e033": // defeated warrior wraiths
               gi.EventDisplayed = gi.EventActive = "e033a";
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterRoll;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e034": //  defeated spectre
               gi.EventDisplayed = gi.EventActive = "e034b";
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterRoll;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e036": // defeated golem
               gi.EventDisplayed = gi.EventActive = "e036a";
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterRoll;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e039": //  treasure chest opening
               gi.CapturedWealthCodes.Add(60);
               gi.PegasusTreasure = PegasusTreasureEnum.Reroll;
               break;
            case "e040": //  treasure chest opening
               if (true == autoWealthOption.IsEnabled)  // famer with protector might have reached this point without stripping out protector money
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               gi.ActiveMember = gi.Prince;
               gi.PegasusTreasure = PegasusTreasureEnum.Talisman;
               switch (dieRoll)
               {
                  case 1: gi.CapturedWealthCodes.Add(5); break; // might have extra possession 
                  case 2: gi.CapturedWealthCodes.Add(25); break;
                  case 3: gi.CapturedWealthCodes.Add(50); break;
                  case 4: gi.CapturedWealthCodes.Add(60); break; // might have extra possession  
                  case 5: gi.CapturedWealthCodes.Add(70); break;
                  case 6: gi.CapturedWealthCodes.Add(100); break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): Reached default dr=" + dieRoll.ToString() + " ae=" + gi.EventActive + " es=" + gi.EventStart.ToString()); return false;
               }
               break;
            case "e046":  //Gateway to Darkness
            case "e046a":
               gi.EventDisplayed = gi.EventActive = "e046a";
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterRoll;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e047": // defeated mirror
               gi.AddCoins("EncounterLootStart(e047)", gi.Prince.Coin); // double coins - possesions and mounts are already doubled
               gi.HydraTeethCount = theNumHydraTeeth; // return number of Hydrateeth to proper number
               theNumHydraTeeth = 0;
               gi.CapturedWealthCodes.Clear();
               break;
            case "e048": // fugitive 
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e049":  // defeat minstrel
            case "e050":  // defeat constalubary
            case "e051":  // defeat bandits
            case "e052":  // defeated goblins
            case "e054b": // defeated goblin keep - EncounterLootStart()
            case "e055":  // defeated orcs
            case "e056a": // defeated orc tower
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e057": // defeated troll
            case "e058":  // defeated dwarves
            case "e058a":  // defeated dwarves - spotted dwarves
            case "e058b":  // defeated dwarves - spotted you
            case "e058i":  // defeated dwarves - spotted you
            case "e071":   // elves
            case "e072a":  // elves
            case "e072d":  // elves
            case "e071e":  // elves
               break;
            case "e073": // witch 
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e074": break;// defeated spiders
            case "e075b":  // defeated wolves
               if (false == SetCampfireFalconCheckState(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                  return false;
               }
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e076": break; // hunting cat
            case "e081": // mounted patrol
               if (true == autoWealthOption.IsEnabled)
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e083": // wild boar
               gi.EventDisplayed = gi.EventActive = "e083a";
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterRoll;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e084b":  // defeated bear
               if (false == SetCampfireFalconCheckState(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                  return false;
               }
               return true;
            case "e094": break; // crocodiles
            case "e094a": break; // crocodiles
            case "e098": // defeated dragon
               gi.AddSpecialItem(SpecialEnum.DragonEye); // helps in temple
               gi.AddSpecialItem(SpecialEnum.DragonEye); // helps in temple
               if (2 < gi.DieResults["e098"][0])
               {
                  gi.CapturedWealthCodes.Add(30);
               }
               else
               {
                  gi.CapturedWealthCodes.Add(60);
                  gi.CapturedWealthCodes.Add(110);
               }
               break;
            case "e099": break; // defeated roc
            case "e100": break; // griffon
            case "e101": break; // harpy 
            case "e108": break; // hawkmen 
            case "e112": // defeated eagles
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e118": // defeated giant
               break;
            case "e123b": // defeated black knight
               break;
            case "e130": // defeated lord's guards
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.AddCoinsAuto())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.AddCoinsAuto()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e142":
               gi.CapturedWealthCodes.Add(100);
               gi.CapturedWealthCodes.Add(100);
               break;
            case "e144b": // defeated Hill Tribe
               GameEngine.theFeatsInGame.myIsRescueHeir = true;
               gi.EventDisplayed = gi.EventActive = "e144c";
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.IsSecretBaronHuldra = false;
               IMapItem trueHeir = CreateCharacter(gi, "WarriorBoy");
               gi.AddCompanion(trueHeir);
               if (false == EncounterEscape(gi, ref action)) // move to random hex
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): EncounterEscape() returned false ae = " + action.ToString());
                  return false;
               }
               action = GameAction.UpdateEventViewerActive;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e144i": // defeated Huldra Body Guards
               action = GameAction.UpdateEventViewerActive;
               gi.EventDisplayed = gi.EventActive = "e144j";
               gi.DieRollAction = GameAction.DieRollActionNone;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e144j": // defeated Huldra
               action = GameAction.EndGameWin; // Defeated Huldra in battle
               gi.GamePhase = GamePhase.EndGame;
               gi.EndGameReason = "Restored Huldra's Heir to the Throne.";
               gi.EventDisplayed = gi.EventActive = "e501";
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.Statistic.myNumWins++;
               gi.Statistic.myEndDaysCount = gi.Days;
               gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
               gi.Statistic.myEndCoinCount = gi.GetCoins();
               gi.Statistic.myEndFoodCount = gi.GetFoods();
               GameEngine.theFeatsInGame.myIsHuldraDefeatedInBattleWin = true;
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e154e": // lords daughter
               gi.CapturedWealthCodes.Add(100);
               gi.CapturedWealthCodes.Add(100);
               break;
            case "e158": // hostile guards
               if (false == EncounterEscape(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart: EncounterEscape() returned false ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e163a":
               if (0 < gi.PurchasedSlavePorter)
               {
                  gi.EventDisplayed = gi.EventActive = "e163b";
                  action = GameAction.UpdateEventViewerActive;
                  for (int i = 0; i < gi.PurchasedSlavePorter; ++i)
                  {
                     IMapItem porter = CreateCharacter(gi, "PorterSlave");
                     gi.AddCompanion(porter);
                  }
                  gi.PurchasedSlavePorter = 0;
                  return true; //<<<<<<<<<<<<<<<<<<<<<
               }
               if (0 < gi.PurchasedSlaveGirl)
               {
                  gi.DieResults["e163c"][0] = Utilities.NO_RESULT;
                  ++gi.SlaveGirlIndex;
                  if (Utilities.MAX_SLAVE_GIRLS <= gi.SlaveGirlIndex)
                     gi.SlaveGirlIndex = 0;
                  gi.EventDisplayed = gi.EventActive = "e163c";
                  action = GameAction.UpdateEventViewerActive;
                  gi.DieRollAction = GameAction.EncounterRoll;
                  return true; //<<<<<<<<<<<<<<<<<<<<<
               }
               if (0 < gi.PurchasedSlaveWarrior)
               {
                  gi.EventDisplayed = gi.EventActive = "e163d";
                  action = GameAction.UpdateEventViewerActive;
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.PurchasedSlaveWarrior = 0;
                  return true; //<<<<<<<<<<<<<<<<<<<<<
               }
               break;
            case "e164": break;// giant lizard
            case "e212i": // high priestess
               if (false == EncounterEscape(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart: EncounterEscape() returned false ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e333a":
               gi.CapturedWealthCodes.Clear();
               break;
            case "e340a":
               gi.AddCoins("EncounterLootStart(e340a)", gi.LooterCoin, false); // Steal looter coin and return back to fold
               gi.LooterCoin = 0;
               gi.CapturedWealthCodes.Clear();
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart():  reached default for es=" + gi.EventStart);
               return false;
         }
         if (0 == gi.CapturedWealthCodes.Count)
         {
            action = GameAction.UpdateEventViewerActive;
            if (false == EncounterEnd(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): EncounterEnd() returned false w/ es=" + gi.EventStart);
               return false;
            }
         }
         return true;
      }
      protected bool EncounterLootStartEnd(IGameInstance gi, ref GameAction action)
      {
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (gi.EventStart)
         {
            case "e013c":  // rich farmer
               gi.EventDisplayed = gi.EventActive = "e013d";
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e018": // priest
               if (false == gi.IsMarkOfCain)
               {
                  gi.EventDisplayed = gi.EventActive = "e018c";  // mark of cain check
                  action = GameAction.UpdateEventViewerActive;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStartEnd(): EncounterEnd() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e052":   // goblins
            case "e055":   // orcs
            case "e058a":  // dwarves
            case "e072a":  // Elves
            case "e072d":  // Elves
               switch (gi.DieResults["e053"][0]) // e053 - take control of campsite after combat
               {
                  case 2: gi.EventDisplayed = gi.EventActive = "e043"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e064"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e028"; break;  // cave tombs
                  case 9: gi.EventDisplayed = gi.EventActive = "e053c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 10: gi.EventDisplayed = gi.EventActive = "e053d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 11: gi.EventDisplayed = gi.EventActive = "e053e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 12:
                     if (true == gi.IsSpecialistInParty())
                     {
                        gi.EventDisplayed = gi.EventActive = "e053f";
                        gi.DieRollAction = GameAction.EncounterRoll;
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e053g";
                     }
                     break;
                  default: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
               }
               gi.DieResults["e053"][0] = Utilities.NO_RESULT;
               break;
            case "e123b": // defeated black knight
               if (0 < gi.EncounteredMembers.Count)
               {
                  if (false == gi.EncounteredMembers[0].IsKilled) // black knight knocked unconscious - he can join the party
                  {
                     gi.EventDisplayed = gi.EventActive = "e123c";
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterLootStartEnd(): EncounterEnd() returned false w/ es=" + gi.EventStart);
                        return false;
                     }
                  }
               }
               else
               {
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStartEnd(): EncounterEnd() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e130": // Robbery of High Lord on Travels
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStartEnd(): EncounterEnd() returned false w/ es=" + gi.EventStart);
                  return false;
               }
               break;
            case "e142":
               if (true == gi.IsMagicInParty())
               {
                  action = GameAction.UpdateEventViewerActive;
                  gi.EventDisplayed = gi.EventActive = "e041";
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStartEnd(): EncounterEnd() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e154e": // lords daughter 
               IMapItem trueLoveLordsDaughter = CreateCharacter(gi, "TrueLoveLordsDaughter");
               gi.AddCompanion(trueLoveLordsDaughter);
               foreach (IMapItem e154Mi in gi.PartyMembers) // if no mount, add a mount
               {
                  if (true == e154Mi.IsFlyer())
                     continue;
                  if (true == e154Mi.Name.Contains("Giant"))
                     continue;
                  if (0 == e154Mi.Mounts.Count)
                     e154Mi.AddNewMount();
               }
               //------------------------------------------------
               gi.DaughterRollModifier += 4;
               gi.DieResults["e154"][0] = Utilities.NO_RESULT;
               if (false == ResetDieResultsForAudience(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStartEnd(): ResetDieResultsForAudience() returned false w/ es=" + gi.EventStart);
                  return false;
               }
               foreach (ITerritory t in Territory.theTerritories) // Accused of kidnapping and considered as if killed somebody from every town, temple, castle in game south of Tragfor River
               {
                  if (true == gi.IsInStructure(t))
                  {
                     if (false == gi.KilledLocations.Contains(t))
                     {
                        if (false == IsNorthofTragothRiver(gi.Prince.Territory)) // exclude rivers north of Tragoth River
                           gi.KilledLocations.Add(t);
                     }
                  }
               }
               break;
            case "e158":
               foreach (IMapItem mi in gi.LostPartyMembers) // return lost party members at end of day
                  gi.PartyMembers.Add(mi);
               gi.LostPartyMembers.Clear();
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStartEnd(): EncounterEnd() returned false w/ es=" + gi.EventStart);
                  return false;
               }
               break;
            case "e155": // Audience with High Priest
            case "e160": // Audience with Lady Aeravir
            case "e161": // Audience with Count Drogat
               if (true == gi.IsAlcoveOfSendingAudience)
               {
                  gi.EventDisplayed = gi.EventActive = "e042b";
               }
               else
               {
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStartEnd(): EncounterEnd() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            default:
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStartEnd(): EncounterEnd() returned false w/ es=" + gi.EventStart);
                  return false;
               }
               break;
         }
         return true;
      }
      protected bool EncounterRoll(IGameInstance gi, ref GameAction action, int dieRoll)
      {
         //--------------------------------------------------------
         int partyCoin = gi.GetCoins();
         int numFlying = 0; // nobody is considered flying if prince is not
         int numRiding = 0; // nobody is considered riding if prince is not
         if (true == gi.Prince.IsFlying)
         {
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (true == mi.IsFlying)
                  ++numFlying;
            }
         }
         if (true == gi.Prince.IsRiding)
         {
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if ((true == mi.IsRiding) || (true == mi.IsFlyer()))
                  ++numRiding;
            }
         }
         ITerritory princeTerritory = gi.Prince.Territory;
         //--------------------------------------------------------
         bool isResultsPerformed = false;
         gi.DieRollAction = GameAction.DieRollActionNone;
         string key = gi.EventActive;
         Logger.Log(LogEnum.LE_VIEW_APPEND_EVENT, "EncounterRoll(): k=" + key + " d0=" + gi.DieResults[key][0].ToString() + " d1=" + gi.DieResults[key][1].ToString() + " d2=" + gi.DieResults[key][2].ToString());
         switch (key)
         {
            case "e002b": // talk
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if( Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if( true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e327"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass dummies
                     case 2: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass rough
                     case 3: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass charm
                     case 4:
                        gi.DieRollAction = GameAction.EncounterRoll;
                        gi.Bribe = 15;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        gi.EventDisplayed = gi.EventActive = "e323";
                        break;
                     case 5:
                        gi.DieRollAction = GameAction.EncounterRoll;
                        gi.Bribe = 25;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterStart(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        gi.EventDisplayed = gi.EventActive = "e323";
                        break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e002c": // Evade
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsPartyRiding())
                     dieRoll += 1;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e307"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e306"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e304"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e318"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e311"; break;
                     case 6:
                     case 7:
                        if (0 == numRiding)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e312b";
                        else if (numRiding < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e312a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e312";
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e002d": // fight mercenary
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e300"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e301"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e003a": // talk to swordsman
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                     case 3: gi.EventDisplayed = gi.EventActive = "e340"; gi.DieRollAction = GameAction.EncounterRoll; break;  // looter
                     case 4: gi.EventDisplayed = gi.EventActive = "e339"; break;                                               // convince to hire
                     case 5: gi.EventDisplayed = gi.EventActive = "e333"; break;                                               // hirelings
                     case 6:                                                                                                   // bride to hire
                        gi.EventDisplayed = gi.EventActive = "e332";
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterStart(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e003b": // evade to swordsman
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1:
                        if (0 == numRiding)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e312b";
                        else if (numRiding < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e312a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e312";
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                     case 3: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                     case 4:                                                                                                   // bribe to pass
                        gi.EventDisplayed = gi.EventActive = "e322";
                        gi.Bribe = 5;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterStart(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 5: case 6: gi.EventDisplayed = gi.EventActive = "e325"; break;                 // pass with dignity
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e003c": // fight swordsman
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e307"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e004a": // talk to mercenaries
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll)
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                     case 2:
                        gi.EventDisplayed = gi.EventActive = "e332";
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e338"; break;  // convince to hire
                     case 4: gi.EventDisplayed = gi.EventActive = "e339"; break;  // convince to hire
                     case 5: gi.EventDisplayed = gi.EventActive = "e340"; gi.DieRollAction = GameAction.EncounterRoll; break;  // looter
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;                                               // attacked
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults["e004"][0] = Utilities.NO_RESULT;
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e004b": // evade mercenaries
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  foreach (IMapItem mi in gi.PartyMembers) // if there is at least one mount in party, add one to evade
                  {
                     if ((0 < mi.Mounts.Count) || (true == mi.IsFlyingMountCarrier()))
                     {
                        gi.DieResults[key][0] += 1;
                        break;
                     }
                  }
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide party size 
                     case 2: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                     case 3: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass charm
                     case 4: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                     case 5: gi.EventDisplayed = gi.EventActive = "e327"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e004"][0] = Utilities.NO_RESULT;  break;  // pass dummies
                     case 6:
                     case 7:                                                                                           // escape mounted
                        if (0 == numRiding)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e312b";
                        else if (numRiding < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e312a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e312";
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e004c": // fight mercenaries
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e301"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults["e004"][0] = Utilities.NO_RESULT;
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e005a": // talk to amazon
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll)
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                     case 2: gi.EventDisplayed = gi.EventActive = "e338"; break;  // convince to hire
                     case 3: gi.EventDisplayed = gi.EventActive = "e339"; break;  // convince to hire
                     case 4: gi.EventDisplayed = gi.EventActive = "e340"; gi.DieRollAction = GameAction.EncounterRoll; break;  // looters
                     case 5: gi.EventDisplayed = gi.EventActive = "e327"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass dummies
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;                                               // attacked
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults["e005"][0] = Utilities.NO_RESULT;
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e005b": // evade amazon
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e311"; gi.DieResults["e005"][0] = Utilities.NO_RESULT; break;                                               // escape
                     case 2: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieResults["e005"][0] = Utilities.NO_RESULT; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape talking
                     case 3: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieResults["e005"][0] = Utilities.NO_RESULT; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape talking
                     case 4: gi.EventDisplayed = gi.EventActive = "e319";                                                 gi.DieRollAction = GameAction.EncounterRoll; break;  // hide party size
                     case 5: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieResults["e005"][0] = Utilities.NO_RESULT; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                     case 6: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieResults["e005"][0] = Utilities.NO_RESULT; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e005c": // fight amazon
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults["e005"][0] = Utilities.NO_RESULT;
               }
               break;
            case "e006b": // number of dwarf friends
               gi.EnteredHexes.Last().EventNames.Add(key);
               for (int i = 0; i < dieRoll; ++i)
               {
                  IMapItem dwarfFriend = CreateCharacter(gi, "DwarfW");
                  gi.EncounteredMembers.Add(dwarfFriend);
               }
               switch (gi.DwarvenChoice)
               {
                  case "Talk": gi.EventDisplayed = gi.EventActive = "e006c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case "Evade": gi.EventDisplayed = gi.EventActive = "e006d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case "Fight": gi.EventDisplayed = gi.EventActive = "e006e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default choice=" + gi.DwarvenChoice + " ae=" + gi.EventActive); return false;
               }
               break;
            case "e006c": // talk to dwarf warrior
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults["e006a"][0] < 4) // if alone add one to die roll
                     ++dieRoll;
                  switch (dieRoll)
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e308"; break;                                               // attacked
                     case 2:                                                                                                   // bribe to pass
                        gi.EventDisplayed = gi.EventActive = "e322";
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e340"; gi.DieRollAction = GameAction.EncounterRoll; break;  // looters
                     case 4: gi.EventDisplayed = gi.EventActive = "e339"; break;                                               // convince to hire
                     case 5: gi.EventDisplayed = gi.EventActive = "e333"; break;                                               // hirelings
                     case 6: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                     case 7: gi.EventDisplayed = gi.EventActive = "e334"; break;                                               // ally
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults["e006a"][0] = Utilities.NO_RESULT;
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e006d": // evade dwarf warrior 
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults["e006a"][0] < 4) // if alone add one to die roll
                     ++gi.DieResults[key][0];
                  switch (gi.DieResults[key][0])
                  {
                     case 1:                                                                                                   // bribe to pass
                        gi.EventDisplayed = gi.EventActive = "e322";
                        gi.Bribe = 5;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e006a"][0] = Utilities.NO_RESULT; break; // pass rough
                     case 3: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e006"; break; // escape talking
                     case 4: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e006"; break; // escape begging 
                     case 5: gi.EventDisplayed = gi.EventActive = "e311"; gi.DieResults["e006a"][0] = Utilities.NO_RESULT; break;                                              // escape
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; gi.DieResults["e006a"][0] = Utilities.NO_RESULT; gi.DieRollAction = GameAction.EncounterRoll; break; // attacked
                     case 7: gi.EventDisplayed = gi.EventActive = "e311"; gi.DieResults["e006a"][0] = Utilities.NO_RESULT; break;                                              // escape
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e006e": // fight dwarf warrior
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults["e006a"][0] < 4) // if alone add one to die roll
                     ++gi.DieResults[key][0];
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 7: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults["e006a"][0] = Utilities.NO_RESULT;
               }
               break;
            case "e006g": // Search for treasure
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.DwarfAdviceLocations.Remove(gi.NewHex);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e059"; gi.DieRollAction = GameAction.EncounterRoll; break; // dwarven mines
                     case 3: case 4: gi.EventDisplayed = gi.EventActive = "e064"; break;  // hidden ruins
                     case 5:                                                              // orc tower
                        if (null == gi.OrcTowers.Find(gi.NewHex.Name))
                           gi.OrcTowers.Add(gi.NewHex);
                        gi.EventDisplayed = gi.EventActive = "e056";
                        break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e028"; break;          // cave tombs
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e007c": // talk to elf lead
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if ("Forest" == princeTerritory.Type) // if in forest, add two
                     dieRoll += 2;
                  switch (dieRoll)
                  {
                     case 1: case 3: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                     case 2: case 4: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                     case 5: gi.EventDisplayed = gi.EventActive = "e335"; break;                                               // escapee
                     case 6: gi.EventDisplayed = gi.EventActive = "e336"; gi.DieRollAction = GameAction.EncounterRoll; break;  // please comrades - sympathetic
                     case 7: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                     case 8: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass charm
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults["e007a"][0] = Utilities.NO_RESULT;
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e007d": // evade elf lead 
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if ("Forest" == princeTerritory.Type) // if in forest, add two
                     dieRoll += 2;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e007"; break;  // hide quickly
                     case 2: gi.EventDisplayed = gi.EventActive = "e318"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e007"; break;  // hide
                     case 3: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e007"; break;  // escape talking
                     case 4: case 5: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e007"; break; // escape begging 
                     case 6: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e007a"][0] = Utilities.NO_RESULT; break;  // pass suspiciously
                     case 7: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e007a"][0] = Utilities.NO_RESULT; break;  // pass charm
                     case 8: gi.EventDisplayed = gi.EventActive = "e310"; gi.DieResults["e007a"][0] = Utilities.NO_RESULT; break;                                               // surprised
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e007e": // fight elf lead
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if ("Forest" == princeTerritory.Type) // if in forest, add two
                     dieRoll += 2;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 7: gi.EventDisplayed = gi.EventActive = "e309"; break;
                     case 8: gi.EventDisplayed = gi.EventActive = "e310"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults["e007a"][0] = Utilities.NO_RESULT;
               }
               break;
            case "e008a": // talk to halfling
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  gi.EventStart = "e008";
                  if (0 == gi.EncounteredMembers.Count)
                  {
                     IMapItem halflingWarrior1 = CreateCharacter(gi, "HalflingLead");
                     gi.EncounteredMembers.Add(halflingWarrior1);
                  }
                  switch (dieRoll)
                  {
                     case 1:
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                           return false;
                        }
                        break;
                     case 2:
                        ITerritory adjacent = FindRandomHexRangeAdjacent(gi);
                        if (false == gi.HalflingTowns.Contains(adjacent))
                           gi.HalflingTowns.Add(adjacent);
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                           return false;
                        }
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                     case 4: case 5: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                     case 6: gi.EventDisplayed = gi.EventActive = "e008b"; gi.DieRollAction = GameAction.EncounterRoll; break;  // choice gossip
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e008b": // halfing gossip
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] < 4)
                  {
                     gi.EventDisplayed = gi.EventActive = "e147"; // secret clue
                     gi.DieRollAction = GameAction.E147ClueToTreasure;
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e162"; // secrets
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e009a": // farm friendly approach
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2: case 3: gi.EventDisplayed = gi.EventActive = "e012a"; break;                                       // farmer with protector
                     case 5: gi.EventDisplayed = gi.EventActive = "e014a"; gi.DieRollAction = GameAction.EncounterStart; break; // hostile reapers
                     case 6: gi.EventDisplayed = gi.EventActive = "e010a"; gi.DieRollAction = GameAction.EncounterStart; break; // starving farmer
                     case 4:
                     case 7:                                                                                                    // peaceful farmer
                        gi.EventDisplayed = gi.EventActive = "e011a";
                        gi.DieRollAction = GameAction.EncounterRoll;
                        gi.IsPartyFed = true;
                        gi.IsMountsFed = true;
                        gi.IsPartyLodged = true;
                        gi.IsMountsStabled = true;
                        if (0 < gi.MapItemMoves.Count)
                           gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_FAIL;
                        Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterRoll(): MovementUsed=Movement for ae=e011a");
                        gi.Prince.MovementUsed = gi.Prince.Movement;
                        break;
                     case 8:                                                                                                    // rich peasant family
                        gi.EventDisplayed = gi.EventActive = "e013a";
                        if (0 < gi.MapItemMoves.Count)
                           gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_FAIL;
                        Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterRoll(): MovementUsed=Movement for ae=e013a");
                        gi.Prince.MovementUsed = gi.Prince.Movement;
                        break;
                     case 9: gi.EventDisplayed = gi.EventActive = "e015a"; gi.DieRollAction = GameAction.EncounterStart; gi.DieResults["e015a"][0] = Utilities.NO_RESULT; break; // friendly reapers
                     case 10: gi.EventDisplayed = gi.EventActive = "e012a"; break;                                                                                               // farmer with protector 
                     case 11: case 12: gi.EventDisplayed = gi.EventActive = "e016a"; gi.DieResults["e016a"][0] = Utilities.NO_RESULT; break;                                     // magician
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e009b": // farm raid approach
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2: case 3: gi.EventDisplayed = gi.EventActive = "e012b"; gi.DieRollAction = GameAction.EncounterStart; break; // farmer with protector
                     case 4: gi.EventDisplayed = gi.EventActive = "e011b"; gi.DieRollAction = GameAction.EncounterStart; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e014c"; gi.DieRollAction = GameAction.EncounterStart; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e010b"; break;
                     case 7: gi.EventDisplayed = gi.EventActive = "e011b"; gi.DieRollAction = GameAction.EncounterStart; break;
                     case 8: gi.EventDisplayed = gi.EventActive = "e013b"; gi.DieRollAction = GameAction.EncounterStart; break;
                     case 9: gi.EventDisplayed = gi.EventActive = "e015c"; gi.DieRollAction = GameAction.EncounterStart; gi.DieResults["e015c"][0] = Utilities.NO_RESULT; break;
                     case 10: gi.EventDisplayed = gi.EventActive = "e012b"; gi.DieRollAction = GameAction.EncounterStart; break;
                     case 11:
                     case 12:
                        if (false == gi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
                        {
                           gi.EventDisplayed = gi.EventActive = "e016b";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e016c";
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e011d": // FarmerBoy
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (6 == gi.DieResults[key][0])
                  {
                     IMapItem farmBoy = CreateCharacter(gi, "FarmerBoy");
                     farmBoy.IsGuide = true;
                     if (false == AddGuideTerritories(gi, farmBoy, 2))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddGuideTerritories() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString() + " mi=" + farmBoy.Name + " hexes=2");
                        return false;
                     }
                     farmBoy.IsGuide = true;
                     gi.AddCompanion(farmBoy);
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                     return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e011c": // Steal Farmers Food
               gi.EnteredHexes.Last().EventNames.Add(key);
               int foodToAdd = dieRoll * 4;
               if (false == gi.AddFoods(foodToAdd))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddFoods() returned false for food=" + foodToAdd.ToString() + " ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                  return false;
               }
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                  return false;
               }
               break;
            case "e013d": // Steal Rich Farmers Food
               gi.EnteredHexes.Last().EventNames.Add(key);
               int foodToAdd1 = dieRoll * 6;
               if (false == gi.AddFoods(foodToAdd1))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddFoods() returned false for food=" + foodToAdd1.ToString() + "ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                  return false;
               }
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                  return false;
               }
               break;
            case "e014a": // Hostile reapers
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (gi.EncounteredMembers.Count < gi.PartyMembers.Count)
                  gi.EventDisplayed = gi.EventActive = "e014b";
               else
                  gi.EventDisplayed = gi.EventActive = "e307";
               gi.DieResults[key][0] = Utilities.NO_RESULT;
               break;
            case "e014c": // Hostile reapers
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.DieResults[key][0] = Utilities.NO_RESULT;
               break;
            case "e015a": // Friendly reapers
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.EventDisplayed = gi.EventActive = "e342";
               gi.DieResults["e342"][0] = Utilities.NO_RESULT;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e015c": // Hostile reapers
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e016b": // Hostile Magician wins
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  gi.DieResults["e016b"][0] = dieRoll;
                  IMapItems abandonedPartyMembers = new MapItems();
                  foreach (IMapItem mi in gi.PartyMembers)
                     abandonedPartyMembers.Add(mi);
                  foreach (IMapItem mi in abandonedPartyMembers)
                     gi.RemoveAbandonedInParty(mi);
                  gi.Prince.RemoveUnmountedMounts();
                  gi.Prince.SetWounds(dieRoll, 1); // set one poison wound and a die roll for normal wounds
                  if (0 < gi.MapItemMoves.Count)
                     gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_FAIL;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterRoll(): MovementUsed=Movement for ae=e016b");
                  gi.Prince.MovementUsed = gi.Prince.Movement;
               }
               break;
            case "e016d": // Hostile magician
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.DieResults[key][0] = dieRoll;
               if (false == gi.RemoveSpecialItem(SpecialEnum.ResistanceTalisman))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): RemoveSpecialItem(ResistanceTalisman) invalid state ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e017": // Peasant Mob
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e018a": // talk to priest
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e336"; gi.DieRollAction = GameAction.EncounterRoll; break;  // please commrades - sympathic
                     case 3: gi.EventDisplayed = gi.EventActive = "e337"; gi.DieRollAction = GameAction.EncounterRoll; break; // please comrades - unsavory
                     case 4: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                     case 5: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                     case 6: gi.EventDisplayed = gi.EventActive = "e325"; break;  // pass with dignity
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e018b": // fight priest
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e300"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e018c": // killed priest
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               if (4 < dieRoll)
               {
                  action = GameAction.E018MarkOfCain;
               }
               else
               {
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                     return false;
                  }
               }
               break;
            case "e019a": // talk hermit monk
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break;
                     case 3:
                     case 4:
                        if (false == gi.IsReligionInParty())
                           gi.MonkPleadModifier = -1; // temporarily reduce wit & wiles by one if Priest/Monk is not in party
                        gi.EventDisplayed = gi.EventActive = "e336";
                        gi.DieRollAction = GameAction.EncounterRoll;
                        break;
                     case 5: case 6: gi.EventDisplayed = gi.EventActive = "e325"; break; // pass with dignity
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e019b": // evade hermit monk
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: case 2: case 3: gi.EventDisplayed = gi.EventActive = "e325"; break; // pass with dignity
                     case 4: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e310"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e022"; gi.DieRollAction = GameAction.EncounterStart; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e019c":  // fight hermit monk
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  gi.DieRollAction = GameAction.EncounterRoll;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e307"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e020a": // talk traveling monk
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e336"; gi.DieRollAction = GameAction.EncounterRoll; break; // please comrades - sympathetic
                     case 5: gi.EventDisplayed = gi.EventActive = "e337"; gi.DieRollAction = GameAction.EncounterRoll; break; // please comrades - unsavory
                     case 6: gi.EventDisplayed = gi.EventActive = "e309"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e020b": // evade traveling monk
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e325"; break; // pass with dignity
                     case 3: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e311"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e020c":  // fight traveling monk
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 3: case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e307"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e021": // Warrior Monks
               if (Utilities.NO_RESULT == gi.DieResults[key][1])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  gi.DieResults[key][1] = dieRoll;
                  if (3 < dieRoll) // if above 3, add mounts to each warrior monk
                  {
                     foreach (IMapItem mi in gi.EncounteredMembers)
                        mi.AddNewMount();
                  }
               }
               break;
            case "e021a": // talk to warrior monks
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass charm
                     case 4:
                        gi.EventDisplayed = gi.EventActive = "e323";
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6:
                        gi.DieResults["e021a"][0] = Utilities.NO_RESULT;
                        gi.DieResults["e021a"][1] = Utilities.NO_RESULT;
                        gi.DieResults["e021a"][2] = Utilities.NO_RESULT;
                        gi.EventDisplayed = gi.EventActive = "e021a";
                        gi.DieRollAction = GameAction.EncounterRoll;
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e021b": // evade warrior monks
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e325"; break; // pass with dignity
                     case 2: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                     case 3: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                     case 5: gi.EventDisplayed = gi.EventActive = "e324"; gi.DieRollAction = GameAction.EncounterRoll; gi.Bribe = 10; break; // bribe to pass with their threat
                     case 6: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass charm
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e021c": // fight warrior monks
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e301"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e023a": // talk to wizard 
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquriy
                     case 3: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                     case 4:
                        if (true == gi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e024c";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e024";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;  // wizard attack
                     case 5: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                     case 6: gi.EventDisplayed = gi.EventActive = "e334"; break;                                               // ally
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e023b": // evade to wizard
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e325"; break; // pass with dignity
                     case 3:                                                 // bribe to pass
                        gi.EventDisplayed = gi.EventActive = "e321";
                        gi.Bribe = 5;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterStart(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape talking
                     case 5:                                                // escape fly
                        if (0 == numFlying)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e313b";
                        else if (numFlying < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e313a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e313c";
                        break;
                     case 6:
                        if (true == gi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e024c";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e024";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;  // wizard attack
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e023c": // fight wizard
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 3:
                     case 4:
                        if (true == gi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e024c";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e024";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;  // wizard attack
                     case 5: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e307"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e024": // wizard attack
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  if (gi.WitAndWile <= gi.DieResults[key][0])
                  {
                     gi.EventDisplayed = gi.EventActive = "e024a";
                     gi.DieRollAction = GameAction.EncounterRoll;
                     IMapItems abandonedPartyMembers = new MapItems();
                     foreach (IMapItem mi in gi.PartyMembers)
                        abandonedPartyMembers.Add(mi);
                     foreach (IMapItem mi in abandonedPartyMembers)
                        gi.RemoveAbandonedInParty(mi);
                  }
                  else
                  {
                     if (false == EncounterEscape(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: EncounterEscape() returned false ae=" + gi.EventActive);
                        return false;
                     }
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e024a": // wizard attack
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.WitAndWile < gi.DieResults[key][0])
                  {
                     Enslaved(gi);
                  }
                  else
                  {
                     if (false == EncounterEscape(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: EncounterEscape() returned false ae=" + gi.EventActive);
                        return false;
                     }
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e026": // Search for treasure
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  bool isWizardAdvice = gi.WizardAdviceLocations.Contains(princeTerritory);
                  if( true == isWizardAdvice)
                     gi.WizardAdviceLocations.Remove(princeTerritory);
                  else
                     gi.PixieAdviceLocations.Remove(princeTerritory);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e027"; break;  // ancient treasure
                     case 2: gi.EventDisplayed = gi.EventActive = "e028"; break;                  // cave tombs
                     case 3: gi.EventDisplayed = gi.EventActive = "e029"; gi.DieRollAction = GameAction.EncounterRoll; break;  // danger and treasure 
                     case 4: case 5: gi.EventDisplayed = gi.EventActive = "e401"; break;          // nothing to see
                     case 6:
                        ITerritory adjacent = FindRandomHexRangeAdjacent(gi);
                        if (true == isWizardAdvice)
                           gi.WizardAdviceLocations.Add(adjacent);
                        else
                           gi.PixieAdviceLocations.Remove(adjacent);
                        gi.EventDisplayed = gi.EventActive = "e026a";
                        break;          // nothing to see
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e028a": // Cave Tombs
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e030"; gi.AddCoins("EncounterRoll(e028a)", 1); break;       // 1 gold with mummies
                     case 2: gi.EventDisplayed = gi.EventActive = "e031"; gi.DieRollAction = GameAction.EncounterRoll; break;  // looted tomb
                     case 3: gi.EventDisplayed = gi.EventActive = "e032"; gi.DieRollAction = GameAction.EncounterStart; break; // ghosts
                     case 4: gi.EventDisplayed = gi.EventActive = "e033"; gi.DieRollAction = GameAction.EncounterStart; break; // warrior wraiths
                     case 5: gi.EventDisplayed = gi.EventActive = "e034"; break;                                               // spectre of the inner tomb
                     case 6: gi.EventDisplayed = gi.EventActive = "e029"; gi.DieRollAction = GameAction.EncounterRoll; break;  // danger and treasure
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e029": // danger and treasure
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e028"; break;                                               // cave tomb
                     case 2: gi.EventDisplayed = gi.EventActive = "e032"; gi.DieRollAction = GameAction.EncounterStart; break; // ghosts
                     case 3: gi.EventDisplayed = gi.EventActive = "e036"; gi.DieRollAction = GameAction.EncounterStart; break; // golem at the gate
                     case 4: gi.EventDisplayed = gi.EventActive = "e037"; gi.DieRollAction = GameAction.EncounterRoll; break;  // broken chest
                     case 5: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break;  // cache under stone
                     case 6:                                                                                                   // high altar
                        if (true == gi.IsReligionInParty())
                           gi.EventDisplayed = gi.EventActive = "e044";
                        else
                           gi.EventDisplayed = gi.EventActive = "e044a";
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e032a": // hidden altar 
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e037"; gi.DieRollAction = GameAction.EncounterRoll; break; // broken chest
                     case 2: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e039"; break;                              // Small Treasure Chest
                     case 3: gi.EventDisplayed = gi.EventActive = "e041"; gi.DieRollAction = GameAction.EncounterRoll; break; // Vision Gem
                     case 4:                                                                                                  // Alcove of Sending
                        if (true == gi.IsMagicInParty())
                           gi.EventDisplayed = gi.EventActive = "e042";
                        else
                           gi.EventDisplayed = gi.EventActive = "e042a";
                        break;
                     case 5:                                                     // High Altar
                        if (true == gi.IsReligionInParty())
                           gi.EventDisplayed = gi.EventActive = "e044";
                        else
                           gi.EventDisplayed = gi.EventActive = "e044a";
                        break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e033a": // defeated warrior wraiths
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.EncounterLoot;
               gi.ActiveMember = gi.Prince;
               gi.PegasusTreasure = PegasusTreasureEnum.Mount;
               switch (dieRoll)
               {
                  case 1: gi.CapturedWealthCodes.Add(25); break;  // might have extra possession   
                  case 2: gi.CapturedWealthCodes.Add(50); break;
                  case 3: gi.CapturedWealthCodes.Add(60); break;  // might have extra possession  
                  case 4: gi.CapturedWealthCodes.Add(70); break;
                  case 5: gi.CapturedWealthCodes.Add(100); break;
                  case 6: gi.CapturedWealthCodes.Add(100); break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default dr=" + dieRoll.ToString() + " ae=" + gi.EventActive + " es=" + gi.EventStart.ToString()); return false;
               }
               break;
            case "e034b": //  defeated spectre
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.EncounterLoot;
               gi.ActiveMember = gi.Prince;
               gi.PegasusTreasure = PegasusTreasureEnum.Mount;
               switch (dieRoll)
               {
                  case 1: gi.CapturedWealthCodes.Add(5); break;   // might have extra possession 
                  case 2: gi.CapturedWealthCodes.Add(12); break;  // might have extra possession  
                  case 3: gi.CapturedWealthCodes.Add(25); break;
                  case 4: gi.CapturedWealthCodes.Add(25); break;
                  case 5: gi.CapturedWealthCodes.Add(60); break;  // might have extra possession  
                  case 6: gi.CapturedWealthCodes.Add(110); break; // might have extra possession 
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default dr=" + dieRoll.ToString() + " ae=" + gi.EventActive + " es=" + gi.EventStart.ToString()); return false;
               }
               break;
            case "e035a": // wandering idiot
               if (gi.WanderingDayCount < dieRoll)
               {
                  ++gi.Prince.StarveDayNum;
                  ++gi.Statistic.myNumOfPrinceStarveDays;
                  IMapItems deadMounts = new MapItems();
                  foreach (IMapItem mount in gi.Prince.Mounts)
                  {
                     ++mount.StarveDayNum;
                     if (5 < mount.StarveDayNum) // when carry capacity drops to zero, mount dies
                        deadMounts.Add(mount);
                  }
                  foreach (IMapItem m in deadMounts)
                     gi.Prince.Mounts.Remove(m);
                  if (false == EncounterEscape(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: EncounterEscape() returned false ae=" + gi.EventActive);
                     return false;
                  }
                  ++gi.WanderingDayCount;
                  gi.EventDisplayed = gi.EventActive = "e035b"; // continue to wander
               }
               else
               {
                  gi.IsSpellBound = false;
                  if (false == Wakeup(gi, ref action)) // action = EncounterRoll - e035a
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: Wakeup() returned false ae=" + gi.EventActive);
                     return false;
                  }
               }
               break;
            case "e036a": // golem defeated
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break;  // stone slabe
                     case 2: gi.EventDisplayed = gi.EventActive = "e040"; gi.DieRollAction = GameAction.EncounterStart; break; // treasure chest
                     case 3: gi.EventDisplayed = gi.EventActive = "e043"; break; // small altar
                     case 4:                                                     // high altar
                        if (true == gi.IsReligionInParty())
                           gi.EventDisplayed = gi.EventActive = "e044";
                        else
                           gi.EventDisplayed = gi.EventActive = "e044a";
                        break;
                     case 5:                                                    // gateway to darkness
                        gi.EventDisplayed = gi.EventActive = "e046b";
                        foreach (IMapItem mi in gi.PartyMembers)
                        {
                           if (true == mi.IsSecretGatewayToDarknessKnown)
                              gi.EventDisplayed = gi.EventActive = "e046";
                        }
                        break;
                     case 6: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e027"; break;  // ancient treasure    
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e037": // broken chest
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, give the artifact
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e180"; gi.AddSpecialItem(SpecialEnum.HealingPoition); break; // healing potion
                     case 2: gi.EventDisplayed = gi.EventActive = "e181"; gi.AddSpecialItem(SpecialEnum.CurePoisonVial); break; // cure vial
                     case 3: gi.EventDisplayed = gi.EventActive = "e182"; gi.AddSpecialItem(SpecialEnum.GiftOfCharm); break; // lucky charm
                     case 4: gi.EventDisplayed = gi.EventActive = "e184"; gi.AddSpecialItem(SpecialEnum.ResistanceTalisman); break; // resistence talisman
                     case 5: gi.EventDisplayed = gi.EventActive = "e186"; gi.AddSpecialItem(SpecialEnum.MagicSword); break; // magic sword
                     case 6: gi.EventDisplayed = gi.EventActive = "e189"; gi.AddSpecialItem(SpecialEnum.CharismaTalisman); break; // charisma talisman
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e038": // cache under stone
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, give the artifact
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e180"; gi.AddSpecialItem(SpecialEnum.HealingPoition); break; // healing potion
                     case 2: gi.EventDisplayed = gi.EventActive = "e181"; gi.AddSpecialItem(SpecialEnum.CurePoisonVial); break; // cure vial
                     case 3: gi.EventDisplayed = gi.EventActive = "e182"; gi.AddSpecialItem(SpecialEnum.GiftOfCharm); break; // lucky charm
                     case 4: gi.EventDisplayed = gi.EventActive = "e185"; gi.AddSpecialItem(SpecialEnum.EnduranceSash); break; // sash
                     case 5: gi.EventDisplayed = gi.EventActive = "e187"; gi.AddSpecialItem(SpecialEnum.AntiPoisonAmulet); break; // anti-poison amulet
                     case 6: gi.EventDisplayed = gi.EventActive = "e190"; gi.AddSpecialItem(SpecialEnum.NerveGasBomb); break; // Nerve gas bomb
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e041": // vision gem
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e143"; gi.IsSecretTempleKnown = true; break; // Secret of Temples
                     case 2:                                                                                    // Secret of Baron Huldra
                        if (true == gi.IsHuldraHeirKilled)
                        {
                           gi.EventDisplayed = gi.EventActive = "e144e";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e144";
                           gi.IsSecretBaronHuldra = true;
                        }
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e145"; gi.IsSecretLadyAeravir = true; break; // Secret of Lady Aeravir
                     case 4: gi.EventDisplayed = gi.EventActive = "e146"; gi.IsSecretCountDrogat = true; break; // Secret of Count Drogat
                     case 5: gi.EventDisplayed = gi.EventActive = "e147"; gi.DieRollAction = GameAction.E147ClueToTreasure; break; // Clue to Treasure
                     case 6: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e045b":
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.DieResults["e045b"][0] = dieRoll;
               int remainingDays = Utilities.MaxDays - gi.Days;
               int daysToAdvance = Math.Min(dieRoll, remainingDays);
               gi.Days += daysToAdvance;
               action = GameAction.E045ArchOfTravel;
               if (false == gi.Arches.Contains(gi.NewHex))
                  gi.Arches.Add(gi.NewHex); // EncounterRoll(e045b)
               break;
            case "e046a": // gateway to darkness - finished combat against guardians
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               int maxDaysToRemove = Math.Min(gi.Days, dieRoll);
               gi.Days -= maxDaysToRemove;
               gi.DieResults["e046a"][0] = dieRoll;
               break;
            case "e048a": // fugitive swordswoman
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (6 == dieRoll)
               {
                  gi.EncounteredMembers.Clear();
                  gi.EventDisplayed = gi.EventActive = "e048i";
                  IMapItem trueLoveSwordswoman = CreateCharacter(gi, "TrueLoveSwordwoman");
                  trueLoveSwordswoman.IsFugitive = true;
                  trueLoveSwordswoman.IsTownCastleTempleLeave = true;
                  gi.AddCompanion(trueLoveSwordswoman);
                  action = GameAction.E228ShowTrueLove;
               }
               else
               {
                  if (0 == gi.EncounteredMembers.Count)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): gi.EncounteredMembers.Count=0 for ae=" + gi.EventActive);
                     return false;
                  }
                  IMapItem fugitiveSwordsWoman = gi.EncounteredMembers[0];
                  fugitiveSwordsWoman.IsFugitive = true;
                  fugitiveSwordsWoman.IsTownCastleTempleLeave = true;
                  gi.AddCompanion(fugitiveSwordsWoman);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                     return false;
                  }
               }
               break;
            case "e048b": // fugitive slave woman
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (6 == dieRoll)
               {
                  gi.EncounteredMembers.Clear();
                  gi.EventDisplayed = gi.EventActive = "e048j";
                  IMapItem trueLoveSlave = CreateCharacter(gi, "TrueLoveSlave");
                  trueLoveSlave.IsFugitive = true;
                  trueLoveSlave.IsTownCastleTempleLeave = true;
                  trueLoveSlave.IsGuide = true;
                  if (false == AddGuideTerritories(gi, trueLoveSlave, 5))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddGuideTerritories() returned false for ae=" + gi.EventActive + " mi=" + trueLoveSlave.Name + " hexes=5");
                     return false;
                  }
                  gi.AddCompanion(trueLoveSlave);
                  action = GameAction.E228ShowTrueLove;
               }
               else
               {
                  if (0 == gi.EncounteredMembers.Count)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): gi.EncounteredMembers.Count=0 for ae=" + gi.EventActive);
                     return false;
                  }
                  IMapItem fugitiveSlave = gi.EncounteredMembers[0];
                  fugitiveSlave.IsFugitive = true;
                  fugitiveSlave.IsTownCastleTempleLeave = true;
                  fugitiveSlave.IsGuide = true;
                  if (false == AddGuideTerritories(gi, fugitiveSlave, 5))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddGuideTerritories() returned false for ae=" + gi.EventActive + " mi=" + fugitiveSlave.Name + " hexes=5");
                     return false;
                  }
                  gi.AddCompanion(fugitiveSlave);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                     return false;
                  }
               }
               break;
            case "e048e": // fugitive merchant
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (0 == gi.EncounteredMembers.Count)
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): gi.EncounteredMembers.Count=0 for ae=" + gi.EventActive);
                  return false;
               }
               IMapItem fugitiveMerchant = gi.EncounteredMembers[0];
               fugitiveMerchant.TopImageName = "Negotiator1";
               fugitiveMerchant.IsFugitive = true;
               fugitiveMerchant.IsTownCastleTempleLeave = true;
               if (4 < dieRoll)
                  fugitiveMerchant.AddNewMount();
               gi.AddCompanion(fugitiveMerchant);
               gi.IsMerchantWithParty = true;
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e048f": // fugitive deserter
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (0 == gi.EncounteredMembers.Count)
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): gi.EncounteredMembers.Count=0 for ae=" + gi.EventActive);
                  return false;
               }
               IMapItem fugitiveDeserter = gi.EncounteredMembers[0];
               fugitiveDeserter.IsFugitive = true;
               fugitiveDeserter.IsTownCastleTempleLeave = true;
               if (4 < dieRoll)
                  fugitiveDeserter.AddNewMount();
               gi.AddCompanion(fugitiveDeserter);
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e048g": // fugitive deserter
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (0 == gi.EncounteredMembers.Count)
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): gi.EncounteredMembers.Count=0 for ae=" + gi.EventActive);
                  return false;
               }
               if (4 < dieRoll)
               {
                  IMapItem encountered = gi.EncounteredMembers[0];
                  encountered.AddNewMount();
               }
               gi.EventDisplayed = gi.EventActive = "e300";
               break;
            case "e048h": // fugitive deserter
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (0 == gi.EncounteredMembers.Count)
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): gi.EncounteredMembers.Count=0 for ae=" + gi.EventActive);
                  return false;
               }
               if (4 < dieRoll)
               {
                  IMapItem encountered = gi.EncounteredMembers[0];
                  encountered.AddNewMount();
               }
               gi.EventDisplayed = gi.EventActive = "e300";
               break;
            case "e050b": // Talk with Constabulary
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  int resulte050b = theConstableRollModifier + dieRoll;
                  switch (resulte050b)
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 3:                                                                                                   // bribe to pass
                        gi.EventDisplayed = gi.EventActive = "e322";
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 4:                                                                                                  // bribe to pass
                        gi.EventDisplayed = gi.EventActive = "e323";
                        gi.Bribe = 15;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass charm
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 7: gi.EventDisplayed = gi.EventActive = "e325"; break;                                              // pass with dignity
                     case 8: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  //------------------------------------------
                  // Any fugitives depart the party
                  IMapItems fugitives = new MapItems();
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     if (true == mi.IsFugitive)
                        fugitives.Add(mi);
                  }
                  foreach (IMapItem mi in fugitives) // the magic users escape or get arrested - leave party
                     gi.RemoveAbandonerInParty(mi);
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e050c": // Evade with Constabulary
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  int resulte050c = theConstableRollModifier + gi.DieResults[key][0];
                  switch (resulte050c)
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e320"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e311"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                     case 5: gi.EventDisplayed = gi.EventActive = "e318"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 7: case 8: gi.EventDisplayed = gi.EventActive = "e325"; break;                                       // pass with dignity
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  IMapItems evadingFugitives = new MapItems();
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     if (true == mi.IsFugitive)
                        evadingFugitives.Add(mi);
                  }
                  foreach (IMapItem mi in evadingFugitives) // the magic users escape or get arrested - leave party
                     gi.RemoveAbandonerInParty(mi);
               }
               break;
            case "e050d": // Fight with Constabulary
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  int resulte050d = theConstableRollModifier + gi.DieResults[key][0];
                  switch (resulte050d)
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e307"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 7: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 8: gi.EventDisplayed = gi.EventActive = "e301"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e052a": // following goblins
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               if (dieRoll <= gi.WitAndWile)
               {
                  gi.EventDisplayed = gi.EventActive = "e052b";
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e304";
               }
               break;
            case "e052b": // following goblins undiscovered
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               if (1 == dieRoll)
               {
                  if (null == gi.GoblinKeeps.Find(princeTerritory.Name))
                     gi.GoblinKeeps.Add(princeTerritory);
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.EventDisplayed = gi.EventActive = "e054";
               }
               else
               {
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.EventDisplayed = gi.EventActive = "e053";
               }
               break;
            case "e053": // campsite location
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 5: // campsize rendezvous
                        if ("e052" == gi.EventStart)
                        {
                           gi.DieResults["e053b"][0] = Utilities.NO_RESULT;
                           gi.EventDisplayed = gi.EventActive = "e053b";
                           gi.DieRollAction = GameAction.EncounterRoll; // reattempt this roll
                        }
                        else if ("e055" == gi.EventStart)
                        {
                           gi.DieResults["e053b"][0] = Utilities.NO_RESULT;
                           gi.EventDisplayed = gi.EventActive = "e053b";
                           gi.DieRollAction = GameAction.EncounterRoll; // reattempt this roll
                        }
                        if ("e058a" == gi.EventStart)
                        {
                           gi.DieResults["e053b"][0] = Utilities.NO_RESULT;
                           gi.EventDisplayed = gi.EventActive = "e053b";
                           gi.DieRollAction = GameAction.EncounterRoll; // reattempt this roll
                        }
                        else if (("e072a" == gi.EventStart) || ("e072d" == gi.EventStart))
                        {
                           gi.EventDisplayed = gi.EventActive = "e072c";
                           int start1 = gi.EncounteredMembers.Count;
                           int end1 = 2 * gi.EncounteredMembers.Count;
                           int maxCount1 = Math.Min(Utilities.MAX_GRID_ROW - 8, end1); // cannot grow over number that can be shown
                           for (int i = start1; i < maxCount1; ++i) // double the number of elves
                           {
                              IMapItem elf = CreateCharacter(gi, "Elf");
                              gi.EncounteredMembers.Add(elf);
                           }
                        }
                        else
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " es=" + gi.EventStart + " dr=" + dieRoll.ToString());
                           return false;
                        }
                        break;
                     default:
                        if ("e052" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e053a"; // campsite decision with goblins
                        }
                        else if ("e055" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e053a"; // campsite decision with orcs
                        }
                        else if ("e058a" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e053a"; // campsite decision with dwarves
                        }
                        else if (("e072a" == gi.EventStart) || (("e072d" == gi.EventStart)))
                        {
                           gi.EventDisplayed = gi.EventActive = "e072b"; // campsite decision with elves
                        }
                        else
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default at campsite ae=" + gi.EventActive + " es=" + gi.EventStart + " dr=" + dieRoll.ToString());
                           return false;
                        }
                        break;
                  }
               }
               break;
            case "e053b": // campsite rendezvous
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.DieResults[key][0] = dieRoll;
               int start = gi.EncounteredMembers.Count;
               int end = 2 * gi.EncounteredMembers.Count;
               int maxCount = Math.Min(Utilities.MAX_GRID_ROW - 8, end); // cannot grow over number that can be shown
               for (int i = start; i < maxCount; ++i) // double number of goblins
               {
                  if ("e052" == gi.EventStart)
                  {
                     IMapItem goblin = CreateCharacter(gi, "Goblin");
                     gi.EncounteredMembers.Add(goblin);
                  }
                  else if ("e055" == gi.EventStart)
                  {
                     IMapItem orc = CreateCharacter(gi, "Orc");
                     gi.EncounteredMembers.Add(orc);
                  }
                  else if ("e058a" == gi.EventStart)
                  {
                     IMapItem dwarf = CreateCharacter(gi, "Dwarf");
                     gi.EncounteredMembers.Add(dwarf);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default double number ae=" + gi.EventActive + " es=" + gi.EventStart);
                     return false;
                  }
               }
               if (gi.WitAndWile < dieRoll)
               {
                  gi.EventDisplayed = gi.EventActive = "e304"; // attack first in combat
               }
               else
               {
                  gi.DieResults["e053"][0] = Utilities.NO_RESULT; // reset the die roll b/c will attempt again after doubling goblin party
                  gi.EventDisplayed = gi.EventActive = "e053";    // return back and roll again for location
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               break;
            case "e053c":
               gi.EnteredHexes.Last().EventNames.Add(key);
               switch (dieRoll) // e053 - take control of campsite after combat
               {
                  case 2: gi.EventDisplayed = gi.EventActive = "e043"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e064"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e028"; break; // cave tombs
                  case 9: gi.EventDisplayed = gi.EventActive = "e053c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 10: gi.EventDisplayed = gi.EventActive = "e053d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 11: gi.EventDisplayed = gi.EventActive = "e053e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 12:
                     if (true == gi.IsSpecialistInParty())
                     {
                        gi.EventDisplayed = gi.EventActive = "e053f";
                        gi.DieRollAction = GameAction.EncounterRoll;
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e053g";
                     }
                     break;
                  default: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
               }
               break;
            case "e053d": // campsite with small building
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
               {
                  case 1:                                                                                                  // alcove of sending
                     if (true == gi.IsMagicInParty())
                        gi.EventDisplayed = gi.EventActive = "e042";
                     else
                        gi.EventDisplayed = gi.EventActive = "e042a";
                     break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e037"; gi.DieRollAction = GameAction.EncounterRoll; break; // broken chest
                  case 3: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break; // cache under stone
                  case 4: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e039"; break;                              // treasure chest
                  case 5: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                  case 6: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e053e": // campsite near magic
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e032"; gi.DieRollAction = GameAction.EncounterStart; break; // ghosts
                  case 2: gi.EventDisplayed = gi.EventActive = "e036"; gi.DieRollAction = GameAction.EncounterStart; break; // golem at the gate
                  case 3: gi.EventDisplayed = gi.EventActive = "e043"; break; // small altar
                  case 4:                                                     // high alter
                     if (true == gi.IsReligionInParty())
                        gi.EventDisplayed = gi.EventActive = "e044";
                     else
                        gi.EventDisplayed = gi.EventActive = "e044a";
                     break;
                  case 5:                                                      // arch of travel
                     if (true == gi.IsMagicInParty())
                        gi.EventDisplayed = gi.EventActive = "e045"; // campsite near magic
                     else
                        gi.EventDisplayed = gi.EventActive = "e045a";
                     break; // arch of travel
                  case 6:                                                     // gateway to darkness
                     gi.EventDisplayed = gi.EventActive = "e046b";
                     foreach (IMapItem mi in gi.PartyMembers)
                     {
                        if (true == mi.IsSecretGatewayToDarknessKnown)
                           gi.EventDisplayed = gi.EventActive = "e046";
                     }
                     break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e053f": // campsite near strong magic
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e022"; gi.DieRollAction = GameAction.EncounterStart; break; // monks
                  case 2: gi.EventDisplayed = gi.EventActive = "e036"; gi.DieRollAction = GameAction.EncounterStart; break; // golem at the gate
                  case 3: gi.EventDisplayed = gi.EventActive = "e043"; break; // small alter
                  case 4:                                                     // high alter
                     if (true == gi.IsReligionInParty())
                        gi.EventDisplayed = gi.EventActive = "e044";
                     else
                        gi.EventDisplayed = gi.EventActive = "e044a";
                     break;
                  case 5:                                                    // arch of travel
                     if (true == gi.IsMagicInParty())
                        gi.EventDisplayed = gi.EventActive = "e045"; // campsite near strong magic   
                     else
                        gi.EventDisplayed = gi.EventActive = "e045a";
                     break;
                  case 6:                                                     // gateway to darkness
                     gi.EventDisplayed = gi.EventActive = "e046b";
                     foreach (IMapItem mi in gi.PartyMembers)
                     {
                        if (true == mi.IsSecretGatewayToDarknessKnown)
                           gi.EventDisplayed = gi.EventActive = "e046";
                     }
                     break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e054a": // escaping goblin keep
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (null == gi.GoblinKeeps.Find(princeTerritory.Name))
                  gi.GoblinKeeps.Add(princeTerritory);
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  if (gi.DieResults[key][0] < gi.WitAndWile)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                        return false;
                     }
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  if (false == EncounterEscape(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEscape() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                     return false;
                  }
                  action = GameAction.E054EscapeKeep;
               }
               break;
            case "e054b": // fighting goblin keep - EncounterRoll()
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (null == gi.GoblinKeeps.Find(princeTerritory.Name))
                  gi.GoblinKeeps.Add(princeTerritory);
               gi.EventDisplayed = gi.EventActive = "e304"; // party attacks first
               break;
            case "e055a": // following orcs
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               if (dieRoll <= gi.WitAndWile)
               {
                  gi.EventDisplayed = gi.EventActive = "e055b";
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e304";
               }
               break;
            case "e055b": // following orcs undiscovered
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               if (1 == dieRoll)
               {
                  if (null == gi.GoblinKeeps.Find(princeTerritory.Name))
                     gi.GoblinKeeps.Add(princeTerritory);
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.EventDisplayed = gi.EventActive = "e056";
               }
               else
               {
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.EventDisplayed = gi.EventActive = "e053";
               }
               break;
            case "e056a": // fighting orc tower
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e058c": // following dwarves
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               if (dieRoll < gi.WitAndWile)
               {
                  gi.EventDisplayed = gi.EventActive = "e058d";
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e307";
               }
               break;
            case "e058d": // following dwarves undiscovered
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               if (1 == dieRoll)
               {
                  if (null == gi.DwarvenMines.Find(gi.NewHex.Name))
                     gi.DwarvenMines.Add(gi.NewHex);
                  gi.EventDisplayed = gi.EventActive = "e059";
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e053";
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               break;
            case "e058e": // talk to dwarves
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll)
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                     case 2:                                                                                                   // bride to join - 30gp
                        gi.EventDisplayed = gi.EventActive = "e331";
                        gi.Bribe = 30;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 3:                                                                                                   // bride to hire
                        gi.EventDisplayed = gi.EventActive = "e332";
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e339"; break;                                               // convince to hire
                     case 5: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;                                               // attacked
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e058f": // evade dwarves
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e311"; break;                                               // escape
                     case 2: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape talking
                     case 3: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                     case 4: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                     case 5: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide party size
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break; // attacked
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e058g": // fight dwarves
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e058h": // Band of Dwarves
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  int numDwarves = gi.DieResults[key][0] + 1;
                  for (int i = 0; i < numDwarves; ++i)
                  {
                     IMapItem dwarf = CreateCharacter(gi, "Dwarf");
                     gi.EncounteredMembers.Add(dwarf);
                  }
                  gi.EventStart = "e058i"; // show this screen if evade is not a possible option.
                  switch (gi.DwarvenChoice)
                  {
                     case "Talk": gi.EventDisplayed = gi.EventActive = "e058e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case "Evade": gi.EventDisplayed = gi.EventActive = "e058f"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case "Fight": gi.EventDisplayed = gi.EventActive = "e058g"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default choice=" + gi.DwarvenChoice + " ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e059": // dwarven mines
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.WitAndWile <= gi.DieResults[key][0])
                  {
                     gi.EventDisplayed = gi.EventActive = "e060";   // arrested
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false w/ ae=" + gi.EventActive);
                        return false;
                     }
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  if (null == gi.DwarvenMines.Find(princeTerritory.Name))
                     gi.DwarvenMines.Add(princeTerritory);
               }
               break;
            case "e060": // arrested
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 0:
                     case 1: //marked for death
                        if (false == MarkedForDeath(gi))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): MarkedForDeath() returned false ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                           return false;
                        }
                        break;
                     case 2: ThrownInDungeon(gi); break;  // Thrown in Dungeon
                     case 3: case 4: Imprisoned(gi); break; // Imprisoned
                     case 5: case 6: gi.EventDisplayed = gi.EventActive = "e060a"; gi.DieRollAction = GameAction.EncounterRoll; break; // minor offense
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else
               {
                  if (true == gi.IsTempleGuardModifer)
                     dieRoll -= 1;
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e066": // secret temple
            case "e066b": // secret temple unfriendly
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] < gi.WitAndWile)
                  {
                     gi.EventDisplayed = gi.EventActive = "e066a";   // access temple
                     if (false == gi.HiddenTemples.Contains(princeTerritory))
                        gi.HiddenTemples.Add(princeTerritory);
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e060";   // arrested
                     gi.DieRollAction = GameAction.EncounterRoll;
                     gi.IsTempleGuardModifer = true;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e068a": // magician home 
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1:
                     case 3:
                     case 5: // raid approach
                        if (false == gi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
                        {
                           gi.EventDisplayed = gi.EventActive = "e016b";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e016c";
                        }
                        break;
                     case 2:
                     case 4:
                     case 6: // friendly approach
                        gi.EventDisplayed = gi.EventActive = "e016a";
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): reached default dieroll=" + dieRoll.ToString() + " ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e071a": // talk to elves
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (true == gi.IsInMapItems("Elf"))
                     --dieRoll;
                  if (true == gi.IsInMapItems("Dwarf"))
                     ++dieRoll;
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 0:
                     case 5:                                                             // follow
                        gi.EventDisplayed = gi.EventActive = "e072";
                        gi.DieResults["e071a"][0] = Utilities.NO_RESULT; // if there is lucky charm, reset rolls
                        gi.DieResults["e071a"][1] = Utilities.NO_RESULT;
                        gi.DieResults["e071a"][2] = Utilities.NO_RESULT;
                        break;
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break;  // inquiry
                     case 3: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break;          // conversation
                     case 4: gi.EventDisplayed = gi.EventActive = "e325"; break;                      // pass with dignity
                     case 6: case 7: gi.EventDisplayed = gi.EventActive = "e309"; break;               // suprised
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e071b": // evade elves
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (true == gi.IsInMapItems("Elf"))
                     --gi.DieResults[key][0];
                  if (true == gi.IsInMapItems("Dwarf"))
                     ++gi.DieResults[key][0];
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 0: gi.EventDisplayed = gi.EventActive = "e325"; break;                                // pass with dignity
                     case 1:
                     case 7:                                                                                    // escape fly
                        if (0 == numFlying)                                                                     // no escape
                           gi.EventDisplayed = gi.EventActive = "e313b";
                        else if (numFlying < gi.PartyMembers.Count)                                             // partial escape
                           gi.EventDisplayed = gi.EventActive = "e313a";
                        else                                                                                    // full escape
                           gi.EventDisplayed = gi.EventActive = "e313c";
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                     case 3: gi.EventDisplayed = gi.EventActive = "e318"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide
                     case 4: gi.EventDisplayed = gi.EventActive = "e320"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;                                               // attacked
                     case 6: gi.EventDisplayed = gi.EventActive = "e309"; break;                                               // surpised
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e071c": // fight elves
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.NumMembersBeingFollowed = 0;
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (true == gi.IsInMapItems("Elf"))
                     --gi.DieResults[key][0];
                  if (true == gi.IsInMapItems("Dwarf"))
                     ++gi.DieResults[key][0];
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 0: gi.EventDisplayed = gi.EventActive = "e301"; break;  // suprise
                     case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;  // suprise
                     case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;  // attack
                     case 3: gi.EventDisplayed = gi.EventActive = "e306"; break;  // attacked
                     case 4: gi.EventDisplayed = gi.EventActive = "e307"; break;  // attacked
                     case 5: gi.EventDisplayed = gi.EventActive = "e308"; break;  // suprised
                     case 6: gi.EventDisplayed = gi.EventActive = "e310"; break;  // suprised
                     case 7: gi.EventDisplayed = gi.EventActive = "e305"; break;  // attack
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e071d": // Band of Elves
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  int numElves = gi.DieResults[key][0] + 1;
                  for (int i = 0; i < numElves; ++i)
                  {
                     IMapItem elf = CreateCharacter(gi, "Elf");
                     gi.EncounteredMembers.Add(elf);
                  }
                  gi.EventStart = "e071e"; // show this screen if evade is not a possible option.
                  switch (gi.ElvenChoice)
                  {
                     case "Talk": gi.EventDisplayed = gi.EventActive = "e071a"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case "Evade": gi.EventDisplayed = gi.EventActive = "e071b"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case "Fight": gi.EventDisplayed = gi.EventActive = "e071c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default choice=" + gi.DwarvenChoice + " ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e072a": // follow elves
            case "e072d":
               gi.EnteredHexes.Last().EventNames.Add(key);
               switch (dieRoll)
               {
                  case 1:                                              // town
                     gi.NumMembersBeingFollowed = 0;
                     gi.EventDisplayed = gi.EventActive = "e165";
                     gi.DieRollAction = GameAction.EncounterRoll;
                     break;
                  case 2:                                              // castle
                     gi.NumMembersBeingFollowed = 0;
                     gi.EventDisplayed = gi.EventActive = "e166";
                     gi.DieRollAction = GameAction.EncounterRoll;
                     break;
                  default:                                             // campfire
                     gi.EventDisplayed = gi.EventActive = "e053";
                     gi.DieRollAction = GameAction.EncounterRoll;
                     break;
               }
               break;
            case "e073c": // friendly witch
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e334"; break;
                     case 3: case 4: gi.EventDisplayed = gi.EventActive = "e333"; break;
                     case 5: case 6: gi.EventDisplayed = gi.EventActive = "e195"; gi.DieRollAction = GameAction.EncounterRoll; break;  // roll for possessions
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e079a": // heavy rains
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.IsHeavyRainNextDay = false;  // rain is today - need EncounterEnd() to be called to end the day in EventViewer.ShowE079ColdCheckResult()
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.GamePhase = GamePhase.SunriseChoice;      // e079a - Finish Heavy Rains
               gi.DieResults[key][0] = dieRoll;
               if (3 < dieRoll)
               {
                  gi.IsHeavyRainContinue = true;
                  gi.EventDisplayed = gi.EventActive = "e079b"; // next screen to show
               }
               else
               {
                  gi.IsHeavyRainContinue = false;
                  foreach (IMapItem mi in gi.PartyMembers)     // all party members no longer have colds
                     mi.IsCatchCold = false;
                  gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
               }
               break;
            case "e080a": // pixie gift
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1:
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e025b"; gi.DieRollAction = GameAction.E080PixieAdvice; break;      // pixie advice
                     case 3: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break;         // stone slabe
                     case 4: case 5: gi.EventDisplayed = gi.EventActive = "e195"; gi.DieRollAction = GameAction.EncounterRoll; break; // roll for possessions
                     case 6:                                                                                                          // pegasus mount
                        gi.EventDisplayed = gi.EventActive = "e188";
                        if (false == gi.AddNewMountToParty(MountEnum.Pegasus))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddNewMountToParty() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e081a": // talk to mounted patrol 
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll)
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                     case 2: gi.EventDisplayed = gi.EventActive = "e327"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass dummies
                     case 3:                                                                                                  // bribe to pass
                        gi.EventDisplayed = gi.EventActive = "e322";
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        break;
                     case 4:                                                                                                  // bribe to pass with their threat
                        gi.EventDisplayed = gi.EventActive = "e324";
                        gi.DieRollAction = GameAction.EncounterRoll;
                        gi.Bribe = 8;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;  // attacked
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;  // surprised
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e081b": // evade mounted patrol
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1:
                     case 2:                                                                                            // escape mounted
                        if (0 == numRiding)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e312b";
                        else if (numRiding < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e312a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e312";
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape talking
                     case 4: gi.EventDisplayed = gi.EventActive = "e318"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;  // attacked
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;  // surprised
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e081c": // fight mounted patrol
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e083a": // roasted boar
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  if (false == gi.AddFoods(gi.DieResults[key][0]))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                     return false;
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                     return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e086a": // high pass
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  int numWound = 0;
                  int numPoison = 0;
                  switch (gi.DieResults[key][0])
                  {
                     case 8: numWound = 1; break;
                     case 9: numWound = 2; break;
                     case 10: numWound = 3; break;
                     case 11: numWound = 4; break;
                     case 12: numWound = 4; numPoison = 1; break;
                     default: break;
                  }
                  bool isMemberIncapacited = false;
                  foreach (IMapItem partyMember in gi.PartyMembers)
                  {
                     partyMember.SetWounds(numWound, numPoison);
                     if ((true == partyMember.IsKilled) || (true == partyMember.IsUnconscious))
                        isMemberIncapacited = true;
                  }
                  if ((true == gi.Prince.IsKilled)) // if Prince killed, game over
                  {
                     gi.GamePhase = GamePhase.EndGame;
                     if (true == gi.Prince.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace))
                     {
                        action = GameAction.EndGameResurrect;  // High Pass
                        gi.EventDisplayed = gi.EventActive = "e192a";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                     else
                     {
                        action = GameAction.EndGameLost;  // High Pass
                        gi.EndGameReason = "Prince died in gory fall off cliff";
                        gi.EventDisplayed = gi.EventActive = "e502";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                        gi.Statistic.myEndDaysCount = gi.Days;
                        gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
                        gi.Statistic.myEndCoinCount = gi.GetCoins();
                        gi.Statistic.myEndFoodCount = gi.GetFoods();
                     }
                  }
                  else if ((true == isMemberIncapacited) || (8 < gi.DieResults[key][0])) // if anybody is incapacitied or the mounts are lost, redistribute belongings
                  {
                     action = GameAction.E086HighPassRedistribute;
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
               }
               break;
            case "e092a": // flood
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.GamePhase = GamePhase.SunriseChoice;      // e092a - Finish Flood
               gi.DieResults[key][0] = dieRoll;
               if (dieRoll < 5)
               {
                  gi.IsFloodContinue = true;
                  gi.EventDisplayed = gi.EventActive = "e092b"; // next screen to show
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
               }
               break;
            case "e098a": // evade dragon
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1:
                        if (0 == numFlying)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e313b";
                        else if (numFlying < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e313a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e313c";
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                     case 3: gi.EventDisplayed = gi.EventActive = "e318"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e320"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e098b": // fight dragon
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e309"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e099a": // evade roc
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1:
                     case 2:
                        if (0 == numFlying)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e313b";
                        else if (numFlying < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e313a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e313c";
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                     case 4: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e099b": // fight roc
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e301"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e100a": // talk to griffon
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry  
                     case 2: gi.EventDisplayed = gi.EventActive = "e337"; gi.DieRollAction = GameAction.EncounterRoll; break; // plead comrades - unsavory
                     case 3: gi.EventDisplayed = gi.EventActive = "e306"; break;         // attacked  
                     case 4: gi.EventDisplayed = gi.EventActive = "e307"; break;         // attacked
                     case 5: gi.EventDisplayed = gi.EventActive = "e308"; break;         // surprised
                     case 6: gi.EventDisplayed = gi.EventActive = "e309"; break;         // surprised
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e100b": // evade to griffon
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1:
                        if (0 == numFlying)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e313b";
                        else if (numFlying < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e313a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e313c";
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                     case 3: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                     case 4: gi.EventDisplayed = gi.EventActive = "e318"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide
                     case 5: gi.EventDisplayed = gi.EventActive = "e325"; break;                                               // pass with dignity
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;                                               // attacked
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e100c": // fight griffon
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e101a": // talk to harpy
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry  
                     case 2: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass charm
                     case 3: case 4: gi.EventDisplayed = gi.EventActive = "e340"; gi.DieRollAction = GameAction.EncounterRoll; break;  // looters
                     case 5: gi.EventDisplayed = gi.EventActive = "e308"; break;         // possible surprised
                     case 6: gi.EventDisplayed = gi.EventActive = "e310"; break;         // surprised
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e101b": // evade to harpy
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1:
                        if (0 == numFlying)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e313b";
                        else if (numFlying < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e313a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e313c";
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e325"; break;                                               // pass with dignity
                     case 3: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass suspiciously
                     case 4: gi.EventDisplayed = gi.EventActive = "e327"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass dummies
                     case 5: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                     case 6: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass charm
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e101c": // fight harpy
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e309"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e105": // storm clouds
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e103"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e102"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e079"; break;
                  case 4:
                  case 5:
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                     break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e105a"; gi.DieRollAction = GameAction.EncounterStart; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               gi.DieResults[key][0] = Utilities.NO_RESULT;
               break;
            case "e110b": // air spirit - fail - blown off course
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  int directionLost = gi.DieResults[key][0];
                  int range = gi.DieResults[key][1] = dieRoll;
                  ITerritory blowToTerritory = FindRandomHexRangeDirectionAndRange(gi, directionLost, range);
                  if (null == blowToTerritory)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): blowToTerritory=null for a=" + action.ToString());
                     return false;
                  }
                  //--------------------------------------------------------
                  int movementUsed = gi.Prince.MovementUsed;
                  gi.Prince.MovementUsed = 0; // must have movement left to be blown off course
                  gi.Prince.TerritoryStarting = gi.NewHex;
                  gi.NewHex = blowToTerritory;
                  gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_TRAVEL_AIR));
                  this.AddVisitedLocation(gi); // EncounterRoll() - air spirit - fail - blown off course
                  if (false == AddMapItemMove(gi, blowToTerritory))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddMapItemMove() return false for a=" + action.ToString());
                     return false;
                  }
                  Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "EncounterRoll(): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString() + " for k=" + key);
                  gi.Prince.MovementUsed = movementUsed; // return back to original value
               }
               break;
            case "e110c": // air spirit - succeed - choose hex
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.E110AirSpiritTravel;
                  gi.DieResults[key][0] = dieRoll;
                  gi.AirSpiritLocations = GetHexesWithinRange(gi, dieRoll);
                  if (null == gi.AirSpiritLocations)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): GetHexesWithinRange() return null for a=" + action.ToString());
                     return false;
                  }
               }
               break;
            case "e112a": // follow eagles
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e113"; gi.DieRollAction = GameAction.EncounterRoll; break;  // eagle ambush
                     case 2:
                     case 3:                                                                                           // eagle hunt
                        gi.EventDisplayed = gi.EventActive = "e114";
                        Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterRoll(): MovementUsed=Movement for ae=e114 dr=3");
                        gi.Prince.MovementUsed = gi.Prince.Movement; // end the day
                        gi.IsEagleHunt = true;
                        break;
                     case 4:                                                                                                   // eagle lair
                        gi.EventDisplayed = gi.EventActive = "e115";
                        Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterRoll(): MovementUsed=Movement for for ae=e115 dr=4");
                        gi.Prince.MovementUsed = gi.Prince.Movement; // end the day
                        if (false == gi.EagleLairs.Contains(princeTerritory))
                           gi.EagleLairs.Add(princeTerritory);
                        gi.IsPartyFed = true;
                        gi.IsMountsFed = true;
                        break;
                     case 5:                                                                                                   // eagle help
                        gi.EventDisplayed = gi.EventActive = "e116";
                        Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterRoll(): MovementUsed=Movement for for ae=e116 dr=5");
                        gi.Prince.MovementUsed = gi.Prince.Movement; // end the day
                        if (false == gi.EagleLairs.Contains(princeTerritory))
                           gi.EagleLairs.Add(princeTerritory);
                        gi.IsPartyFed = true;
                        gi.IsMountsFed = true;
                        IMapItem eagleHelp = CreateCharacter(gi, "Eagle");
                        eagleHelp.IsGuide = true;
                        eagleHelp.GuideTerritories = Territory.theTerritories;
                        eagleHelp.IsTownCastleTempleLeave = true;
                        eagleHelp.IsFlying = true;
                        eagleHelp.IsRiding = true;
                        gi.AddCompanion(eagleHelp);
                        break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e117"; gi.DieRollAction = GameAction.EncounterRoll; break;  // eagle allies
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e112b": // evade eagles 
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e325"; break;                                               // pass with dignity
                     case 3: case 4: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass suspiciously
                     case 5: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // conversation  
                     case 6: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry  
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e112c": // fight eagles
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e304"; break;  // attack
                     case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;  // attack
                     case 3: gi.EventDisplayed = gi.EventActive = "e306"; break;  // attacked
                     case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;  // attacked
                     case 5: gi.EventDisplayed = gi.EventActive = "e308"; break;  // suprised
                     case 6: gi.EventDisplayed = gi.EventActive = "e309"; break;  // suprised
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e113": // eagle ambush
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  for (int i = 0; i < gi.DieResults[key][0]; ++i) // additional eagles arrive
                  {
                     IMapItem eagle = CreateCharacter(gi, "Eagle");
                     eagle.IsFlying = true;
                     eagle.IsRiding = true;
                     gi.EncounteredMembers.Add(eagle);
                  }
                  gi.EventDisplayed = gi.EventActive = "e310";
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e117":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  gi.IsPartyFed = true;
                  gi.IsMountsFed = true;
                  if (false == gi.AddCoins("EncounterRoll(e117)", 50))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins() return false for ae=" + gi.EventActive);
                     return false;
                  }
                  for (int i = 0; i < gi.DieResults[key][0]; ++i) // additional eagles arrive
                  {
                     IMapItem eagleAlly = CreateCharacter(gi, "Eagle");
                     eagleAlly.IsGuide = true;
                     eagleAlly.GuideTerritories = Territory.theTerritories;
                     eagleAlly.IsFlying = true;
                     eagleAlly.IsRiding = true;
                     gi.AddCompanion(eagleAlly);
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                     return false;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e118a": // talk to giant
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll) // Based on the die roll, implement the correct screen
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                     case 2: gi.EventDisplayed = gi.EventActive = "e337"; gi.DieRollAction = GameAction.EncounterRoll; break;  // please comrades - unsavory
                     case 3: gi.EventDisplayed = gi.EventActive = "e340"; gi.DieRollAction = GameAction.EncounterRoll; break;  // looters
                     case 4: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;                                               // attacked
                     case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;                                               // attacked
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e118b": // evade to giant
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1:
                        if (0 == numFlying)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e313b";
                        else if (numFlying < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e313a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e313c";
                        break;
                     case 2:
                        if (0 == numRiding)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e312b";
                        else if (numRiding < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e312a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e312";
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e316"; break;                                               // auto hide
                     case 4: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                     case 5: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;  // easy group hide   
                     case 6: gi.EventDisplayed = gi.EventActive = "e320"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hard group hide
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e118c": // fight giant
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e301"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e124": // make raft
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
               {
                  case 1:
                  case 2:
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                     break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e094a"; gi.DieRollAction = GameAction.EncounterStart; break;  // Crocs in River
                  case 4: gi.EventDisplayed = gi.EventActive = "e125"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e126"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e127"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               gi.DieResults[key][0] = Utilities.NO_RESULT;
               break;
            case "e128": // merchant caravan
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (GamePhase.Travel == gi.GamePhase)
                     gi.SunriseChoice = GamePhase.Encounter; // if talk with merchant, no more travel
                  switch (gi.DieResults[key][0])
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e128a"; break; // paasus sale
                     case 3: gi.EventDisplayed = gi.EventActive = "e028"; break;  // cave tombs
                     case 4: gi.EventDisplayed = gi.EventActive = "e128b"; break; // cure-poison sale
                     case 5: gi.EventDisplayed = gi.EventActive = "e009"; gi.DieRollAction = GameAction.EncounterRoll; break;  // farms
                     case 6: gi.EventDisplayed = gi.EventActive = "e128c"; break; // food for sale
                     case 7:                                                      // merchant outwits you
                        if (0 < gi.GetCoins())
                        {
                           gi.EventDisplayed = gi.EventActive = "e128d";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e128g";
                        }
                        break;
                     case 8: gi.EventDisplayed = gi.EventActive = "e128e"; break; // healing potion sale
                     case 9: gi.EventDisplayed = gi.EventActive = "e128f"; break; // two horses for sale
                     case 10: gi.EventDisplayed = gi.EventActive = "e163"; gi.DieRollAction = GameAction.EncounterRoll; break; // coffle
                     case 11: gi.EventDisplayed = gi.EventActive = "e147"; gi.DieRollAction = GameAction.E147ClueToTreasure; break; // clue to treasure
                     case 12: gi.EventDisplayed = gi.EventActive = "e162"; gi.DieRollAction = GameAction.EncounterRoll; break; // secrets
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else
               {
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterRoll(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // halt movement for the day if talk with merchant caravan
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e128d": // merchant outwit
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.WitAndWile < gi.DieResults[key][0])
                  {
                     int maxLoss = Math.Min(gi.GetCoins(), 10);
                     gi.ReduceCoins("EncounterRoll(e128d)", maxLoss);
                  }
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false for ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString());
                     return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e129": // merchant caravan
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e162"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e022"; gi.DieRollAction = GameAction.EncounterStart; break; // monks
                     case 4: gi.EventDisplayed = gi.EventActive = "e129a"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e009"; gi.DieRollAction = GameAction.EncounterRoll; break;  // farms
                     case 6: gi.EventDisplayed = gi.EventActive = "e128"; gi.DieRollAction = GameAction.EncounterRoll; break;  // independent merchant
                     case 7:
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     case 8: gi.EventDisplayed = gi.EventActive = "e129b"; break;
                     case 9: gi.EventDisplayed = gi.EventActive = "e129c"; break;
                     case 10: gi.EventDisplayed = gi.EventActive = "e163"; gi.DieRollAction = GameAction.EncounterRoll; break; // coffle
                     case 11: gi.EventDisplayed = gi.EventActive = "e147"; gi.DieRollAction = GameAction.E147ClueToTreasure; break; // clue to treasure
                     case 12:
                        ITerritory t = FindRandomHexRangeAdjacent(gi);
                        if (null == t)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): FindRandomHexRangeAdjacent() returned null");
                           return false;
                        }
                        else if (null == gi.HiddenRuins.Find(t.Name))  // if this is hidden ruins, save it for future reference
                        {
                           gi.HiddenRuins.Add(t);
                        }
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               break;
            case "e130a": // talk to high lord on travels
               isResultsPerformed = false;
               if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive))
               {
                  isResultsPerformed = true;
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  dieRoll = gi.DieResults[key][0];
                  gi.IsTalkRoll = false;
                  isResultsPerformed = true;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.IsTalkRoll = true;
               }
               if (true == isResultsPerformed)
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (dieRoll)
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e327"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass dummies
                     case 2: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass charm
                     case 3: gi.EventDisplayed = gi.EventActive = "e130d"; break; // arrested
                     case 4: gi.EventDisplayed = gi.EventActive = "e130e"; break; // audience
                     case 5: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                     case 6: gi.EventDisplayed = gi.EventActive = "e307"; break;                                              // attacked
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  for (int i = 0; i < 3; ++i)
                     gi.DieResults[key][i] = Utilities.NO_RESULT;
               }
               break;
            case "e130b": // evade high lord on travels
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1:
                        if (0 == numRiding)                                 // no escape
                           gi.EventDisplayed = gi.EventActive = "e312b";
                        else if (numRiding < gi.PartyMembers.Count)         // partial escape
                           gi.EventDisplayed = gi.EventActive = "e312a";
                        else                                                // full escape
                           gi.EventDisplayed = gi.EventActive = "e312";
                        break;                                           // escape
                     case 2: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape talking
                     case 3: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                     case 4: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // quick hide
                     case 5: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide party size
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break; // attacked
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e130c": // fight high lord on travels
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e130e": // Audience with high lord on travels
               gi.EnteredHexes.Last().EventNames.Add(key);
               switch (gi.DieResults["e130"][1])
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e130f"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e161"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e160"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e155"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 5:
                  case 6:   // Audience with mayor on travels from closest twon
                     ITerritory t156b = FindClosestTown(gi); // this territory is updated by user selecting a castle
                     if ((true == gi.IsReligionInParty()) && (true == gi.ForbiddenAudiences.IsReligiousConstraint(t156b)))
                     {
                        gi.ForbiddenAudiences.RemoveReligionConstraint(t156b);
                        action = GameAction.E156MayorTerritorySelection;
                        gi.EventDisplayed = gi.EventActive = "e156g";
                        IMapItem trustedAssistant = CreateCharacter(gi, "TrustedAssistant");
                        gi.AddCompanion(trustedAssistant);
                        ITerritory t156a = FindClosestTown(gi); // this territory is updated by user selecting a castle or temple
                        gi.ForbiddenAudiences.AddAssistantConstraint(t156a, trustedAssistant);
                        if (false == gi.AddCoins("EncounterRoll(e130e)", 100))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins()=false ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                           return false;
                        }
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e156";  // Audience with mayor on travels 
                        gi.DieRollAction = GameAction.EncounterRoll;
                     }
                     break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default dr=" + gi.DieResults["e130"][1].ToString() + " ae=" + gi.EventActive); return false;
               }
               break;
            case "e130f": // Audience with Baron when Meeting on Travels
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 1:
                     case 2:
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false for ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                           return false;
                        }
                        break;
                     case 3: case 4: gi.EventDisplayed = gi.EventActive = "e150"; gi.DieRollAction = GameAction.EncounterRoll; ++gi.Statistic.myNumOfAudience; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e151"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e152"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default dr=" + gi.DieResults["e130"][1].ToString() + " ae=" + gi.EventActive); return false;
                  }
               }
               break;
            // ========================Search Ruin Results================================
            case "e132":
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (dieRoll < gi.PartyMembers.Count)
               {
                  gi.EventDisplayed = gi.EventActive = "e208";
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e132a";
               }
               break;
            case "e133": gi.EnteredHexes.Last().EventNames.Add(key); action = GameAction.E133Plague; gi.DieRollAction = GameAction.DieRollActionNone; break;
            case "e135": // broken columns in ruins
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1:
                        if (true == gi.IsMagicInParty())
                           gi.EventDisplayed = gi.EventActive = "e042";
                        else
                           gi.EventDisplayed = gi.EventActive = "e042a";
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e043"; break;
                     case 3:
                        if (true == gi.IsReligionInParty())
                           gi.EventDisplayed = gi.EventActive = "e044";
                        else
                           gi.EventDisplayed = gi.EventActive = "e044a";
                        break;
                     case 4:                                                       // arch of travel
                        if (true == gi.IsMagicInParty())
                           gi.EventDisplayed = gi.EventActive = "e045";           // broken columns in ruins
                        else
                           gi.EventDisplayed = gi.EventActive = "e045a";
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e046"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e047"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e136": // hidden treasures
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, the proper hidden treasures
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e037"; gi.DieRollAction = GameAction.EncounterRoll; break; // Broken Chest
                     case 2: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break; // Cache Under Stone
                     case 3: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e039"; break;                              // Small Treasure Chest
                     case 4:
                        if (true == gi.IsReligionInParty())                       // High Alter
                           gi.EventDisplayed = gi.EventActive = "e044";
                        else
                           gi.EventDisplayed = gi.EventActive = "e044a";
                        break;
                     case 5:
                        gi.EventDisplayed = gi.EventActive = "e500";              // Horde of 500 gp
                        break;
                     case 6:
                        if (false == EncounterEnd(gi, ref action))                // nothing found
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false for ae=e136 dr=5");
                           return false;
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e137": // inhabitants
               if( Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e032"; gi.DieRollAction = GameAction.EncounterStart; break; // ghosts
                     case 2: gi.EventDisplayed = gi.EventActive = "e051"; break;                                               // bandits
                     case 3: gi.EventDisplayed = gi.EventActive = "e052"; gi.DieRollAction = GameAction.EncounterStart; break; // goblins
                     case 4:                                                                                                   // orc tower
                        if (null == gi.OrcTowers.Find(princeTerritory.Name))
                           gi.OrcTowers.Add(princeTerritory);
                        gi.EventDisplayed = gi.EventActive = "e056";
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e057"; gi.DieRollAction = GameAction.EncounterStart; break;  // troll           
                     case 6: gi.EventDisplayed = gi.EventActive = "e082"; gi.DieRollAction = GameAction.EncounterStart; break;  // unearthly spectre
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e138": // unclean creatures
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e032"; gi.DieRollAction = GameAction.EncounterStart; break; // ghosts
                     case 2: gi.EventDisplayed = gi.EventActive = "e033"; gi.DieRollAction = GameAction.EncounterStart; break; // warrior wraiths
                     case 3: gi.EventDisplayed = gi.EventActive = "e034"; gi.DieRollAction = GameAction.EncounterStart; break; // spectre of inner tomb
                     case 4:                                                                                                   // orc tower
                        if (null == gi.OrcTowers.Find(princeTerritory.Name))
                           gi.OrcTowers.Add(princeTerritory);
                        gi.EventDisplayed = gi.EventActive = "e056";
                        break;                                               // orc tower
                     case 5: gi.EventDisplayed = gi.EventActive = "e082"; gi.DieRollAction = GameAction.EncounterStart; break; // unearthly spectre           
                     case 6: gi.EventDisplayed = gi.EventActive = "e098"; gi.DieRollAction = GameAction.EncounterStart; break; // dragon
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e139": // minor treasures
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: gi.CapturedWealthCodes.Add(25); action = GameAction.EncounterLoot; break;
                     case 2: gi.CapturedWealthCodes.Add(60); action = GameAction.EncounterLoot; gi.PegasusTreasure = PegasusTreasureEnum.Reroll; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break;   // cache under stone
                     case 4: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e039"; break;                                // treasure chest    
                     case 5: gi.EventDisplayed = gi.EventActive = "e040"; gi.DieRollAction = GameAction.EncounterStart; break;  // treasure chest           
                     case 6:
                        if (true == gi.IsMagicInParty())
                        {
                           gi.EventDisplayed = gi.EventActive = "e140";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e140a";
                           gi.AddSpecialItem(SpecialEnum.MagicBox);
                        }
                        break;   // magic box
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e140":  // magic box
            case "e140b": // magic box
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e141"; gi.DieRollAction = GameAction.EncounterRoll; break;                               // hydra teeth
                     case 2: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e142"; break;                                                            // gems
                     case 3: gi.CapturedWealthCodes.Add(60); action = GameAction.EncounterLoot; gi.PegasusTreasure = PegasusTreasureEnum.Talisman; break;   // treasure
                     case 4: gi.CapturedWealthCodes.Add(110); action = GameAction.EncounterLoot; gi.PegasusTreasure = PegasusTreasureEnum.Talisman; break;  // treasure 
                     case 5: gi.EventDisplayed = gi.EventActive = "e195"; gi.DieRollAction = GameAction.EncounterRoll; break;                               // roll for possessions
                     case 6: gi.EventDisplayed = gi.EventActive = "e401"; break;                                                                            // nothing
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e141": // hydra teeth
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.AddSpecialItem(SpecialEnum.HydraTeeth);
                  gi.HydraTeethCount += dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString());
                     return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e147a": // found treasure from clues
               action = GameAction.UpdateEventViewerActive;
               gi.SecretClues.Remove(princeTerritory);
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e066"; gi.DieRollAction = GameAction.EncounterRoll; break;   // hidden temple
                     case 3: gi.EventDisplayed = gi.EventActive = "e037"; gi.DieRollAction = GameAction.EncounterRoll; break;   // broken chest
                     case 4: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break;   // cache under stone
                     case 5: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e039"; break;                                // Small Treasure Chest        
                     case 6: gi.EventDisplayed = gi.EventActive = "e040"; gi.DieRollAction = GameAction.EncounterStart; break;  // treasure chest
                     case 7: gi.EventDisplayed = gi.EventActive = "e030"; gi.AddCoins("EncounterRoll(e147a)", 1); break;                                // 1 gold with mummies
                     case 8: gi.CapturedWealthCodes.Add(110); action = GameAction.EncounterLootStart; break;
                     case 9: gi.EventDisplayed = gi.EventActive = "e139"; gi.DieRollAction = GameAction.EncounterRoll; break;   // minor treasure
                     case 10:                                                                                                   // magic box
                        if (true == gi.IsMagicInParty())
                        {
                           gi.EventDisplayed = gi.EventActive = "e140";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e140a";
                           gi.AddSpecialItem(SpecialEnum.MagicBox);
                        }
                        break;
                     case 11: gi.EventDisplayed = gi.EventActive = "e136"; gi.DieRollAction = GameAction.EncounterRoll; break; // Hidden Treasures
                     case 12:                                                                                                  // Goblin Keep
                        if (null == gi.GoblinKeeps.Find(princeTerritory.Name))
                           gi.GoblinKeeps.Add(princeTerritory);
                        gi.EventDisplayed = gi.EventActive = "e054";
                        gi.DieRollAction = GameAction.EncounterRoll;
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e148": // seneschal requires a bribe
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.DieResults[key][0] = dieRoll;
               gi.Bribe = dieRoll * 10;
               if (true == gi.IsMerchantWithParty)
                  gi.Bribe = (int)((double)gi.Bribe * 0.5);
               Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
               break;
            case "e151": // lord finds favor
               gi.EnteredHexes.Last().EventNames.Add(key);
               ++gi.Statistic.myNumOfAudience;
               gi.DieResults[key][0] = dieRoll;
               if (false == gi.AddCoins("EncounterRoll(e151)", dieRoll * 100))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins() returned false for ae=" + gi.EventActive);
                  return false;
               }
               gi.IsPartyFed = true;
               gi.IsMountsFed = true;
               gi.IsPartyLodged = true;
               gi.IsMountsStabled = true;
               gi.IsCavalryEscort = true;
               //----------------------------------
               bool isCavalryAlreadyInParty = false;  // add cavalry if not already in party
               foreach (IMapItem mi in gi.PartyMembers)
               {
                  if (true == mi.Name.Contains("Cavalry"))
                     isCavalryAlreadyInParty = true;
               }
               if (false == isCavalryAlreadyInParty)
               {
                  IMapItem cavalry = CreateCharacter(gi, "Cavalry");
                  cavalry.IsGuide = true;
                  if (false == AddGuideTerritories(gi, cavalry, 3))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddGuideTerritories() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString() + " mi=" + cavalry.Name + "hexes=3");
                     return false;
                  }
                  gi.AddCompanion(cavalry);
               }
               break;
            // ========================Miscellaneous Events================================
            case "e154": // meet lords daughter
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e154a"; break; // hates you
                     case 3: gi.EventDisplayed = gi.EventActive = "e154b"; break;         // dally
                     case 4: gi.EventDisplayed = gi.EventActive = "e154c"; break;         // reserved
                     case 5: gi.EventDisplayed = gi.EventActive = "e154d"; break;         // likes you           
                     case 6: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e154e"; action = GameAction.E228ShowTrueLove; break; // loves you        
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults["e154"][0].ToString()); return false;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e155": // meet high priest
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e155a"; break; // insulted
                     case 2: gi.EventDisplayed = gi.EventActive = "e155b"; break; // unmoved
                     case 3: gi.EventDisplayed = gi.EventActive = "e155c"; break; // hears pleas
                     case 4: gi.EventDisplayed = gi.EventActive = "e155d"; break; // listens
                     case 5: gi.EventDisplayed = gi.EventActive = "e155e"; break; // helps           
                     case 6: gi.EventDisplayed = gi.EventActive = "e155f"; break; // full support      
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults["e155"][0].ToString()); return false;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e156": // audience with town mayor
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  ++gi.Statistic.myNumOfAudience;
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1:                                                                      // arrested
                        if (true == gi.IsAlcoveOfSendingAudience)
                        {
                           gi.EventDisplayed = gi.EventActive = "e042b";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e060";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;
                     case 2:                                                                      // stone faced
                        gi.EventDisplayed = gi.EventActive = "e156a";
                        break;
                     case 3:                                                                      // lodging and food   
                        gi.EventDisplayed = gi.EventActive = "e156b";
                        break;
                     case 4:                                                                       // favor
                        gi.EventDisplayed = gi.EventActive = "e156c";
                        break;
                     case 5:                                                                       // interest
                        gi.EventDisplayed = gi.EventActive = "e156d";
                        break;
                     case 6:                                                                       // religious interest
                        if (true == gi.IsReligionInParty())
                        {
                           action = GameAction.E156MayorTerritorySelection;
                           gi.EventDisplayed = gi.EventActive = "e156e";
                           if (false == gi.IsAlcoveOfSendingAudience)
                           {
                              IMapItem trustedAssistant = CreateCharacter(gi, "TrustedAssistant");
                              gi.AddCompanion(trustedAssistant);
                              ITerritory t156a = FindClosestTown(gi); // this territory is updated by user selecting a castle
                              gi.ForbiddenAudiences.AddAssistantConstraint(t156a, trustedAssistant);
                           }
                           else // only get letter but no trusted assistant
                           {
                              ITerritory t156b = FindClosestTown(gi); // this territory is updated by user selecting a castle
                              gi.ForbiddenAudiences.AddLetterConstraint(t156b);
                           }
                           if (false == gi.AddCoins("EncounterRoll(e156)", 100))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins()=false ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                              return false;
                           }
                        }
                        else
                        {
                           ITerritory t156b = FindClosestTown(gi); // this territory is updated by user selecting a castle
                           gi.ForbiddenAudiences.AddReligiousConstraint(t156b);
                           gi.EventDisplayed = gi.EventActive = "e156f"; // display event showing need a religous person 
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e160": // audience with lady 
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  ++gi.Statistic.myNumOfAudience;
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e160a"; break;        // not interested
                     case 2: gi.EventDisplayed = gi.EventActive = "e160b"; break;        // distracted listening
                     case 3: gi.EventDisplayed = gi.EventActive = "e160c"; break;        // takes pity
                     case 4: gi.EventDisplayed = gi.EventActive = "e160d"; break;        // supports your virtue
                     case 5: gi.EventDisplayed = gi.EventActive = "e160e"; gi.DieRollAction = GameAction.EncounterRoll; break;        // seductively charmed
                     case 6: gi.EventDisplayed = gi.EventActive = "e160f"; gi.DieRollAction = GameAction.EncounterRoll; break;        // supports your cause
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e160e": // Lady Aeravier Seduction
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e160f":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  gi.AddCoins("EncounterRoll(e160f)", gi.DieResults[key][0] * 150);
                  if (false == gi.IsAlcoveOfSendingAudience)
                  {
                     gi.IsPartyFed = true;
                     gi.IsMountsFed = true;
                     gi.IsPartyLodged = true;
                     gi.IsMountsStabled = true;
                     //-----------------------------------------------
                     foreach (IMapItem e160Mi in gi.PartyMembers)   // heal all wounds
                     {
                        e160Mi.PlagueDustWound = 0; // assume that healers cure any plague dust
                        int wound = e160Mi.Wound;
                        int poision = e160Mi.Poison;
                        e160Mi.HealWounds(wound, poision); // e160f - Lady A cures all wounds - nominal path
                        if (0 == e160Mi.Mounts.Count)  // add mount if do not have one
                           e160Mi.AddNewMount();
                        if (true == e160Mi.Name.Contains("Prince"))
                           gi.Statistic.myNumOfPrinceHeal += (wound + poision);
                        else
                           gi.Statistic.myNumOfPartyHeal += (wound + poision); 
                     }
                     //-----------------------------------------------
                     if (false == gi.AddNewMountToParty()) // add a spare pack horse
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddNewMountToParty() return false for ae=" + gi.EventActive);
                        return false;
                     }
                     for (int k = 0; k < 3; ++k) // add three knights
                     {
                        IMapItem knight = CreateCharacter(gi, "Knight");
                        knight.AddNewMount();
                        gi.AddCompanion(knight);
                     }
                     //-----------------------------------------------
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e160f"][0].ToString());
                        return false;
                     }
                  }
                  else
                  {
                     gi.Prince.PlagueDustWound = 0; // assume that healers cure any plague dust - Heal all wounds for Prince
                     int wound = gi.Prince.Wound;
                     int poision = gi.Prince.Poison;
                     gi.Prince.HealWounds(wound, poision); // e160f - Lady A cures all wounds - Alcove path
                     gi.EventDisplayed = gi.EventActive = "e042b";
                     gi.Statistic.myNumOfPrinceHeal += (wound + poision);
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e160g": // audience with lady 
               if (Utilities.NO_RESULT < gi.DieResults[key][1])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  gi.DieResults["e160"][0] = dieRoll; // when applying results, gi.DieResults["e160"][0] is what is looked at
                  switch (dieRoll) // Based on the die roll, implement event
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e160a"; break;        // not interested
                     case 2: gi.EventDisplayed = gi.EventActive = "e160b"; break;        // distracted listening
                     case 3: gi.EventDisplayed = gi.EventActive = "e160c"; break;        // takes pity
                     case 4: gi.EventDisplayed = gi.EventActive = "e160d"; break;        // supports your virtue
                     case 5: gi.EventDisplayed = gi.EventActive = "e160e"; gi.DieRollAction = GameAction.EncounterRoll; break;        // seductively charmed
                     case 6: gi.EventDisplayed = gi.EventActive = "e160f"; gi.DieRollAction = GameAction.EncounterRoll; break;        // supports your cause
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  GameEngine.theFeatsInGame.myIsLadyAeravirAccused = true;
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else
               {
                  gi.DieResults[key][1] = dieRoll;
               }
               break;
            case "e161": // audience with count drogat
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  ++gi.Statistic.myNumOfAudience;
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e161a"; break;        // count victim
                     case 2: gi.EventDisplayed = gi.EventActive = "e161b"; break;        // half listens
                     case 3: gi.EventDisplayed = gi.EventActive = "e161c"; break;        // flippant advice
                     case 4: gi.EventDisplayed = gi.EventActive = "e161d"; break;        // interested
                     case 5:                                                             // need trophies
                        if (4 < gi.NumMonsterKill)
                           gi.EventDisplayed = gi.EventActive = "e161e";
                        else
                           gi.EventDisplayed = gi.EventActive = "e161g";
                        break;
                     case 6: case 7: gi.EventDisplayed = gi.EventActive = "e161f"; break;        // noble ally
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else
               {
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.TrollSkin))
                  {
                     if (false == gi.RemoveSpecialItem(SpecialEnum.TrollSkin))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): RemoveSpecialItem(skin) returned false ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                        return false;
                     }
                     ++dieRoll;
                  }
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e162": //learn secrets
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the correct screen
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e143"; break;
                     case 3:
                     case 4:
                        if (true == gi.IsHuldraHeirKilled)
                        {
                           gi.EventDisplayed = gi.EventActive = "e144e";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e144";
                           gi.IsSecretBaronHuldra = true;
                        }
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e145"; gi.IsSecretLadyAeravir = true; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e146"; gi.IsSecretCountDrogat = true; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e163": //coffle
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults["e163"][0] = dieRoll;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else if (Utilities.NO_RESULT == gi.DieResults[key][1])
               {
                  gi.DieResults[key][1] = dieRoll;
                  gi.DieRollAction = GameAction.EncounterRoll;
                  gi.SlaveGirlIndex = Utilities.RandomGenerator.Next(Utilities.MAX_SLAVE_GIRLS);
               }
               else if (Utilities.NO_RESULT == gi.DieResults[key][2])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  gi.DieResults[key][2] = dieRoll;
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e163c": //buy slave girls
               gi.EnteredHexes.Last().EventNames.Add(key);
               gi.DieResults[key][0] = dieRoll;
               if (12 == dieRoll)
               {
                  IMapItem slaveGirl = CreateCharacter(gi, "TrueLoveSlave");
                  slaveGirl.BottomImageName = "SlaveGirlFace" + gi.SlaveGirlIndex.ToString();
                  gi.AddCompanion(slaveGirl);
                  action = GameAction.E228ShowTrueLove;
               }
               else
               {
                  IMapItem slaveGirl = CreateCharacter(gi, "SlaveGirl");
                  gi.AddCompanion(slaveGirl);
               }
               --gi.PurchasedSlaveGirl;
               break;
            case "e163d": //buy warrior
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults["e163d"][0] = dieRoll;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else if (Utilities.NO_RESULT == gi.DieResults[key][1])
               {
                  gi.DieResults[key][1] = dieRoll;
                  IMapItem oldWarrior = CreateCharacter(gi, "WarriorOld");
                  oldWarrior.Endurance = gi.DieResults[key][0];
                  oldWarrior.Combat = gi.DieResults[key][1];
                  gi.AddCompanion(oldWarrior);
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e165": // elf town - EncounterRoll()
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.WitAndWile < gi.DieResults[key][0])
                  {
                     gi.EventDisplayed = gi.EventActive = "e060";   // arrested
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false w/ ae=" + gi.EventActive);
                        return false;
                     }
                  }
               }
               else
               {
                  if (true == gi.IsInMapItems("Elf"))
                     --dieRoll;
                  else if (true == gi.IsMagicInParty())
                     --dieRoll;
                  if (true == gi.IsInMapItems("Dwarf"))
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
                  if (false == gi.ElfTowns.Contains(princeTerritory))
                     gi.ElfTowns.Add(princeTerritory);
               }
               break;
            case "e166": // elf castle
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.WitAndWile <= gi.DieResults[key][0])
                  {
                     gi.EventDisplayed = gi.EventActive = "e060";   // arrested
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false w/ ae=" + gi.EventActive);
                        return false;
                     }
                  }
               }
               else
               {
                  if (true == gi.IsInMapItems("Elf"))
                     --dieRoll;
                  else if (true == gi.IsMagicInParty())
                     --dieRoll;
                  if (true == gi.IsInMapItems("Dwarf"))
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
                  if (false == gi.ElfCastles.Contains(princeTerritory))
                     gi.ElfCastles.Add(princeTerritory);
               }
               break;
            case "e195": // possession reference
               gi.EnteredHexes.Last().EventNames.Add(key);
               switch (dieRoll)
               {
                  case 2: gi.EventDisplayed = gi.EventActive = "e191"; gi.AddSpecialItem(SpecialEnum.ResistanceRing); break; // resistence ring
                  case 3: gi.EventDisplayed = gi.EventActive = "e186"; gi.AddSpecialItem(SpecialEnum.MagicSword); break; // magic sword
                  case 4: gi.EventDisplayed = gi.EventActive = "e182"; gi.AddSpecialItem(SpecialEnum.GiftOfCharm); break; // gift of charm
                  case 5: gi.EventDisplayed = gi.EventActive = "e184"; gi.AddSpecialItem(SpecialEnum.ResistanceTalisman); break; // resistence talisman
                  case 6: gi.EventDisplayed = gi.EventActive = "e181"; gi.AddSpecialItem(SpecialEnum.CurePoisonVial); break; // cure poison vial
                  case 7: gi.EventDisplayed = gi.EventActive = "e180"; gi.AddSpecialItem(SpecialEnum.HealingPoition); break; // cure wounds potion
                  case 8: gi.EventDisplayed = gi.EventActive = "e185"; gi.AddSpecialItem(SpecialEnum.PoisonDrug); break; // poison drug
                  case 9: gi.EventDisplayed = gi.EventActive = "e193"; gi.AddSpecialItem(SpecialEnum.ShieldOfLight); break; // shield of light
                  case 10: gi.EventDisplayed = gi.EventActive = "e183"; gi.AddSpecialItem(SpecialEnum.EnduranceSash); break; // endurance sash
                  case 11: gi.EventDisplayed = gi.EventActive = "e189"; gi.AddSpecialItem(SpecialEnum.CharismaTalisman); break; // charisma talisman
                  case 12: gi.EventDisplayed = gi.EventActive = "e192"; gi.AddSpecialItem(SpecialEnum.ResurrectionNecklace); break; // resurrection necklass
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            // ========================Search Ruins================================
            case "e208":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e133"; break;
                     case 3:
                        if (true == gi.IsSpecialistInParty())
                        {
                           gi.EventDisplayed = gi.EventActive = "e135";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e135a";
                        }
                        break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e136"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e137"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e139"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 7: gi.EventDisplayed = gi.EventActive = "e131"; break;
                     case 8:
                        if (1 < gi.PartyMembers.Count) // if not alone
                        {
                           gi.EventDisplayed = gi.EventActive = "e132";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e132a";
                        }
                        break;
                     case 9: gi.EventDisplayed = gi.EventActive = "e134"; break;
                     case 10: gi.EventDisplayed = gi.EventActive = "e138"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 11:
                        if (true == gi.IsSpecialistInParty())
                        {
                           gi.EventDisplayed = gi.EventActive = "e135";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e135a";
                        }
                        break;
                     case 12: gi.EventDisplayed = gi.EventActive = "e035"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            // ========================Seek News================================
            case "e209a":
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 2:
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false w/ ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e209b"; gi.AddCoins("EncounterRoll(e209a)", 50); break;
                     case 4:
                        gi.EventDisplayed = gi.EventActive = "e209c";
                        if (false == gi.FeelAtHomes.Contains(princeTerritory))
                           gi.FeelAtHomes.Add(princeTerritory);
                        break;
                     case 5:
                        gi.EventDisplayed = gi.EventActive = "e209d";
                        ITerritory temple = FindClosestTemple(gi);
                        if (null == temple)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): FindClosestTemple() returned null for ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                           return false;
                        }
                        if (false == gi.SecretRites.Contains(temple))
                           gi.SecretRites.Add(temple);
                        action = GameAction.E209ShowSecretRites;
                        break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e129"; gi.DieRollAction = GameAction.EncounterStart; break;
                     case 7:
                        if (false == gi.CheapLodgings.Contains(princeTerritory))
                           gi.CheapLodgings.Add(princeTerritory);
                        gi.EventDisplayed = gi.EventActive = "e209i";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                        break;
                     case 8:
                        gi.Prince.Coin = (int)((double)gi.Prince.Coin * 0.5);
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false w/ ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     case 9:
                        if (("0101" == princeTerritory.Name) || ("1501" == princeTerritory.Name))
                        {
                           gi.EventDisplayed = gi.EventActive = "e050e";
                           gi.DieRollAction = GameAction.EncounterStart;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e050";
                           gi.DieRollAction = GameAction.EncounterStart;
                        }
                        break;
                     case 10: gi.EventDisplayed = gi.EventActive = "e016"; break;
                     case 11:
                        gi.EventDisplayed = gi.EventActive = "e209e";
                        break;
                     case 12: case 13: case 14: case 15: gi.EventDisplayed = gi.EventActive = "e209g"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               else
               {
                  if (true == gi.IsSeekNewModifier)
                     dieRoll += 1;
                  if (4 < gi.WitAndWile)
                     dieRoll += 1;
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                  {
                     if ((true == gi.KilledLocations.Contains(princeTerritory)) || (true == gi.EscapedLocations.Contains(princeTerritory)))
                     {
                        --dieRoll; // subtract one from die
                     }
                     else
                     {
                        ++dieRoll; // add one from die
                        gi.IsPartyFed = true;
                        gi.IsMountsFed = true;
                        gi.IsPartyLodged = true;
                        gi.IsMountsStabled = true;
                     }
                  }
                  if (true == gi.FeelAtHomes.Contains(princeTerritory))
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e209f":
               gi.EnteredHexes.Last().EventNames.Add(key);
               if (false == gi.AddCoins("EncounterRoll(e209f)", 180))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: AddCoins() returned false for action=" + action.ToString());
                  return false;
               }
               if (false == EncounterEscape(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEscape() returned false w/ ae=" + gi.EventActive + " action=" + action.ToString());
                  return false;
               }
               break;
            case "e209h":
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  gi.ReduceCoins("EncounterRoll(e209h)", 10);
                  switch (gi.DieResults[key][0])
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                     case 2: gi.EventDisplayed = gi.EventActive = "e147"; gi.DieRollAction = GameAction.E147ClueToTreasure; break; // Clue to Treasure
                     case 3: gi.EventDisplayed = gi.EventActive = "e143"; gi.IsSecretTempleKnown = true; break; // Secret of Temples
                     case 4:                                                                               // Secret of Baron Huldra
                        if (true == gi.IsHuldraHeirKilled)
                        {
                           gi.EventDisplayed = gi.EventActive = "e144e";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e144";
                           gi.IsSecretBaronHuldra = true;
                        }
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e145"; gi.IsSecretLadyAeravir = true; break; // Secret of Lady Aeravir
                     case 6: gi.EventDisplayed = gi.EventActive = "e146"; gi.IsSecretCountDrogat = true; break; // Secret of Count Drogat
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            // ========================See Hire================================
            case "e210":
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e210a"; break;  // hire freeman
                     case 3: gi.EventDisplayed = gi.EventActive = "e210b"; break;  // hire lancer
                     case 4: gi.EventDisplayed = gi.EventActive = "e210c"; break;  // hire mercenaries
                     case 5: gi.EventDisplayed = gi.EventActive = "e210d"; break;  // horse dealer
                     case 6: gi.EventDisplayed = gi.EventActive = "e210e"; break;  // hire local guide
                     case 7: gi.EventDisplayed = gi.EventActive = "e210f"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hire henchmen
                     case 8: gi.EventDisplayed = gi.EventActive = "e163"; gi.DieRollAction = GameAction.EncounterRoll; break;   // slave market
                     case 9: gi.EventDisplayed = gi.EventActive = "e209"; break;   // seek news
                     case 10: gi.EventDisplayed = gi.EventActive = "e210g"; break; // honest horse dealer
                     case 11: gi.EventDisplayed = gi.EventActive = "e210h"; break; // runaway
                     case 12: gi.EventDisplayed = gi.EventActive = "e210i"; break; // porters & local guide
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else
               {
                  if (true == gi.FeelAtHomes.Contains(princeTerritory))
                     ++dieRoll;
                  if ((true == gi.HalflingTowns.Contains(gi.Prince.Territory)) && ((true == gi.KilledLocations.Contains(gi.Prince.Territory)) || (true == gi.EscapedLocations.Contains(gi.Prince.Territory))))
                     --dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e210f":
               gi.DieResults[key][0] = dieRoll;
               break;
            // ========================See Audience================================
            case "e211a": // seek audience in town
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2: ThrownInDungeon(gi); break;                                                                       // thrown in dungeon
                     case 3: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; break;  // arrested
                     case 4: gi.EventDisplayed = gi.EventActive = "e158"; break;                                               // hostile guards
                     case 5: gi.EventDisplayed = gi.EventActive = "e153"; break;                                               // master of house hold
                     case 6:
                     case 7:
                     case 8:                                                                                                   // do nothing
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e211a"][0].ToString());
                           return false;
                        }
                        break;
                     case 9:
                     case 10: // audience permitted
                        ++gi.Statistic.myNumOfAudience;
                        ITerritory e211a1 = FindClosestTown(gi); // this territory is updated by user selecting a castle or temple
                        if ((true == gi.IsReligionInParty()) && (true == gi.ForbiddenAudiences.IsReligiousConstraint(e211a1)))
                        {
                           gi.ForbiddenAudiences.RemoveReligionConstraint(e211a1);
                           action = GameAction.E156MayorTerritorySelection;
                           gi.EventDisplayed = gi.EventActive = "e156g";  // e211a - seek audience in town - die roll 9/10
                           if (false == gi.IsAlcoveOfSendingAudience)
                           {
                              IMapItem trustedAssistant = CreateCharacter(gi, "TrustedAssistant");
                              gi.AddCompanion(trustedAssistant);
                              ITerritory t156a = FindClosestTown(gi); // this territory is updated by user selecting a castle
                              gi.ForbiddenAudiences.AddAssistantConstraint(t156a, trustedAssistant);
                           }
                           else // only get letter but no trusted assistant
                           {
                              gi.ForbiddenAudiences.AddLetterConstraint(e211a1);
                           }
                           if (false == gi.AddCoins("EncounterRoll(e211a)-10", 100))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins()=false ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                              return false;
                           }
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e156";  // e211a - seek audience in town 
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;
                     case 11: gi.EventDisplayed = gi.EventActive = "e154"; gi.DieRollAction = GameAction.EncounterRoll; break;  // mayor's daughter
                     case 12:
                     case 13:
                     case 14:
                     case 15:
                     case 16:                                                              //  audience permitted
                        ++gi.Statistic.myNumOfAudience;
                        ITerritory e211a2 = FindClosestTown(gi); // this territory is updated by user selecting a castle or temple
                        if ((true == gi.IsReligionInParty()) && (true == gi.ForbiddenAudiences.IsReligiousConstraint(e211a2)))
                        {
                           gi.ForbiddenAudiences.RemoveReligionConstraint(e211a2);
                           action = GameAction.E156MayorTerritorySelection;
                           gi.EventDisplayed = gi.EventActive = "e156g";  // e211a - seek audience in town - die roll 12+
                           if (false == gi.IsAlcoveOfSendingAudience)
                           {
                              IMapItem trustedAssistant = CreateCharacter(gi, "TrustedAssistant");
                              gi.AddCompanion(trustedAssistant);
                              ITerritory t156a = FindClosestTown(gi); // this territory is updated by user selecting a castle
                              gi.ForbiddenAudiences.AddAssistantConstraint(t156a, trustedAssistant);
                           }
                           else // only get letter but no trusted assistant
                           {
                              gi.ForbiddenAudiences.AddLetterConstraint(e211a2); // this territory is updated by user selecting a castle
                           }
                           if (false == gi.AddCoins("EncounterRoll(e211a)", 100))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins()=false ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                              return false;
                           }
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e156"; // e211a - seek audience in town 
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else
               {
                  ++gi.Statistic.myNumOfAudienceAttempt;
                  if (true == gi.FeelAtHomes.Contains(princeTerritory))
                     ++dieRoll;
                  //--------------------------------
                  dieRoll += gi.DaughterRollModifier;
                  gi.DaughterRollModifier = 0;
                  //--------------------------------
                  if ((true == gi.HalflingTowns.Contains(gi.Prince.Territory)) && ((true == gi.KilledLocations.Contains(gi.Prince.Territory)) || (true == gi.EscapedLocations.Contains(gi.Prince.Territory))))
                     --dieRoll;
                  //--------------------------------
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e211b": // seeking audience with High Priest
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2: Imprisoned(gi); break;                                                                            // imprisoned
                     case 3: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; break;  // arrested
                     case 4: gi.EventDisplayed = gi.EventActive = "e158"; break;                                               // hostile guards
                     case 5:                                                                                                   // do nothing
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e211b"][0].ToString());
                           return false;
                        }
                        break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e159"; gi.ForbiddenAudiences.AddPurifyConstaint(princeTerritory); break;   // purify self                                            
                     case 7:                                                                                                   // do nothing
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e211b"][0].ToString());
                           return false;
                        }
                        break;
                     case 8:                                                                                                   // permitted if have dragon eye or see master of household
                        if (true == gi.IsSpecialItemHeld(SpecialEnum.DragonEye))
                        {
                           if (false == gi.RemoveSpecialItem(SpecialEnum.DragonEye))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): RemoveSpecialItem() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e211b"][0].ToString());
                              return false;
                           }
                           ++gi.Statistic.myNumOfAudience;
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e155";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e153"; // master of household
                        }
                        break;
                     case 9:                                                                                                   // pay your respect
                        ++gi.Statistic.myNumOfAudience;
                        gi.EventDisplayed = gi.EventActive = "e150";
                        if (false == gi.AddCoins("EncounterRoll(e211b)", 50))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, gi.Days + 1);
                        break;
                     case 10: gi.EventDisplayed = gi.EventActive = "e159"; gi.ForbiddenAudiences.AddPurifyConstaint(princeTerritory); break;   // purify self
                     default: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e155"; gi.DieRollAction = GameAction.EncounterRoll; ++gi.Statistic.myNumOfAudience; break; //  audience permitted
                  }
               }
               else
               {
                  ++gi.Statistic.myNumOfAudienceAttempt;
                  if (true == gi.IsChagaDrugProvided)
                     dieRoll += 1;  // Chaga Drug adds one
                  gi.IsChagaDrugProvided = false;
                  //--------------------------------
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  //foreach (ITerritory t in letters)
                  //   gi.LetterOfRecommendations.Remove(t);
                  gi.ForbiddenAudiences.RemoveLetterConstraints(princeTerritory);
                  //--------------------------------
                  if (true == gi.Purifications.Contains(princeTerritory)) // can only be one purification territory in this container
                  {
                     gi.Purifications.Remove(princeTerritory);
                     dieRoll += 2;
                  }
                  //--------------------------------
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e211c": // seeking audience with Baron of Huldra Castle
               action = GameAction.UpdateEventViewerActive;
               int resultOfDie = gi.DieResults[key][0];
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2:                                                                                                                       // no audience ever
                        gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, Utilities.FOREVER);
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + resultOfDie.ToString());
                           return false;
                        }
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e154"; gi.DieRollAction = GameAction.EncounterRoll; break;                  // meet king's daughter
                     case 4: gi.EventDisplayed = gi.EventActive = "e149"; gi.ForbiddenAudiences.AddClothesConstraint(princeTerritory); break;  // learn court manners
                     case 5: gi.EventDisplayed = gi.EventActive = "e158"; break;                                                               // hostile guards
                     case 6:
                     case 7:                                                                                                                   // nothing happens
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + resultOfDie.ToString());
                           return false;
                        }
                        break;
                     case 8: gi.EventDisplayed = gi.EventActive = "e153"; break;                                                          // master of household
                     case 9:                                                                                                              // seneschal requires a bribe 
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        if (gi.Bribe <= gi.GetCoins())
                        {
                           gi.EventDisplayed = gi.EventActive = "e148";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, Utilities.FOREVER);
                           gi.EventDisplayed = gi.EventActive = "e148e";
                        }
                        break;
                     case 10:
                     case 11:                                                                                                             // pay your respects
                        ++gi.Statistic.myNumOfAudience;
                        gi.EventDisplayed = gi.EventActive = "e150";
                        if (false == gi.AddCoins("EncounterRoll(e211c)", 50))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, gi.Days + 1);
                        break;
                     case 12: gi.EventDisplayed = gi.EventActive = "e151"; gi.DieRollAction = GameAction.EncounterRoll; break;             // find favor
                     default: gi.EventDisplayed = gi.EventActive = "e152"; gi.IsNobleAlly = true; ++gi.Statistic.myNumOfAudience; break;                                   // ally                       
                  }
               }
               else
               {
                  ++gi.Statistic.myNumOfAudienceAttempt;
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  //foreach (ITerritory t in letters)
                  //   gi.LetterOfRecommendations.Remove(t);
                  gi.ForbiddenAudiences.RemoveLetterConstraints(princeTerritory);
                  //--------------------------------
                  dieRoll += gi.DaughterRollModifier;
                  gi.DaughterRollModifier = 0;
                  //--------------------------------
                  dieRoll += gi.SeneschalRollModifier;
                  gi.SeneschalRollModifier = 0;
                  //--------------------------------
                  if (true == gi.IsMagicUserDismissed)
                  {
                     ++dieRoll;
                     gi.IsMagicUserDismissed = false;
                  }
                  //--------------------------------
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e211d": // Seek Audience with Count Drogat
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2:                                                                                                                        // next victim                                                               
                        if (false == MarkedForDeath(gi))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): MarkedForDeath() returned false ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString());
                           return false;
                        }
                        break;
                     case 3: Imprisoned(gi); break;                                                                                                // Imprisoned
                     case 4: gi.EventDisplayed = gi.EventActive = "e154"; gi.DieRollAction = GameAction.EncounterRoll; break;                      // meet lord's daughter
                     case 5: gi.EventDisplayed = gi.EventActive = "e153"; break;                                                                   // master of household
                     case 6: gi.EventDisplayed = gi.EventActive = "e158"; break;                                                                   // hostile guards
                     case 7:                                                                                                                       // arrested or audience granted
                        if (true == gi.IsSpecialItemHeld(SpecialEnum.RocBeak))
                        {
                           if (false == gi.RemoveSpecialItem(SpecialEnum.RocBeak))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): RemoveSpecialItem() returned false ae=" + action.ToString() + " dr=" + gi.DieResults[key][0].ToString());
                              return false;
                           }
                           ++gi.Statistic.myNumOfAudience;
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e161";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e060";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;
                     case 8:                     // seneschal requires a bribe
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        if (gi.Bribe <= gi.GetCoins())
                        {
                           gi.EventDisplayed = gi.EventActive = "e148";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, Utilities.FOREVER);
                           gi.EventDisplayed = gi.EventActive = "e148e";
                        }
                        break;
                     case 9: gi.EventDisplayed = gi.EventActive = "e149"; gi.ForbiddenAudiences.AddClothesConstraint(princeTerritory); break;      // learn court manners
                     case 10: gi.EventDisplayed = gi.EventActive = "e151"; gi.DieRollAction = GameAction.EncounterRoll; ++gi.Statistic.myNumOfAudience; break;  // find favor
                     default: gi.EventDisplayed = gi.EventActive = "e161"; gi.DieRollAction = GameAction.EncounterRoll; ++gi.Statistic.myNumOfAudience; break;  // gain audience
                  }
               }
               else
               {
                  ++gi.Statistic.myNumOfAudienceAttempt;
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  //foreach (ITerritory t in letters)
                  //   gi.LetterOfRecommendations.Remove(t);
                  gi.ForbiddenAudiences.RemoveLetterConstraints(princeTerritory);
                  //--------------------------------
                  dieRoll += gi.DaughterRollModifier;
                  gi.DaughterRollModifier = 0;
                  //--------------------------------
                  dieRoll += gi.SeneschalRollModifier;
                  gi.SeneschalRollModifier = 0;
                  //--------------------------------
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.Foulbane))
                     dieRoll += 1;
                  //--------------------------------
                  if (true == gi.Prince.IsResurrected)
                     dieRoll += 1;
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e211e": // Seeking audience with Lady Aeravir
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; break;                      // arrested
                     case 3: gi.EventDisplayed = gi.EventActive = "e159"; gi.ForbiddenAudiences.AddPurifyConstaint(princeTerritory); break;        // purify self
                     case 4: gi.EventDisplayed = gi.EventActive = "e158"; break;                                                                   // hostile guards
                     case 5: gi.EventDisplayed = gi.EventActive = "e149"; gi.ForbiddenAudiences.AddClothesConstraint(princeTerritory); break;      // learn court manners
                     case 6: gi.EventDisplayed = gi.EventActive = "e153"; break;                                                                   // master of household
                     case 7:                                                                                                                       // audience if have griffon claw   
                        if (true == gi.IsSpecialItemHeld(SpecialEnum.GriffonClaws))
                        {
                           if (false == gi.RemoveSpecialItem(SpecialEnum.GriffonClaws))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): RemoveSpecialItem() returned false ae=" + action.ToString() + " dr=" + gi.DieResults[key][0].ToString());
                              return false;
                           }
                           if (false == gi.IsSecretLadyAeravir)
                           {
                              gi.EventStart = gi.EventDisplayed = gi.EventActive = "e160";
                           }
                           else
                           {
                              gi.EventStart = gi.EventDisplayed = gi.EventActive = "e160g";
                              gi.IsLadyAeravirRerollActive = true;
                           }
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           if (false == EncounterEnd(gi, ref action))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults[key][0].ToString());
                              return false;
                           }
                        }
                        break;
                     case 8:                                                                                                                        // do nothing   
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults[key][0].ToString());
                           return false;
                        }
                        break;
                     case 9:                                                                                                                      // seneschal requires a bribe
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        if (gi.Bribe <= gi.GetCoins())
                        {
                           gi.EventDisplayed = gi.EventActive = "e148";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, Utilities.FOREVER);
                           gi.EventDisplayed = gi.EventActive = "e148e";
                        }
                        break;
                     case 10:
                        ++gi.Statistic.myNumOfAudience;
                        if (false == gi.IsSecretLadyAeravir)
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e160";
                        }
                        else
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e160g";
                           gi.IsLadyAeravirRerollActive = true;
                        }
                        gi.DieRollAction = GameAction.EncounterRoll;
                        break;                     // gain audience
                     case 11: gi.EventDisplayed = gi.EventActive = "e154"; gi.DieRollAction = GameAction.EncounterRoll; break;                     // meet lord's daughter
                     default:                                                                                                                      // gain audience
                        ++gi.Statistic.myNumOfAudience;
                        if (false == gi.IsSecretLadyAeravir)
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e160";
                        }
                        else
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e160g";
                           gi.IsLadyAeravirRerollActive = true;
                        }
                        gi.DieRollAction = GameAction.EncounterRoll;
                        break;
                  }
               }
               else
               {
                  ++gi.Statistic.myNumOfAudienceAttempt;
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  //foreach (ITerritory t in letters)
                  //   gi.LetterOfRecommendations.Remove(t);
                  gi.ForbiddenAudiences.RemoveLetterConstraints(princeTerritory);
                  //--------------------------------
                  if (true == gi.Purifications.Contains(princeTerritory)) // can only be one purification territory in this container
                  {
                     gi.Purifications.Remove(princeTerritory);
                     dieRoll += 2;
                  }
                  //--------------------------------
                  dieRoll += gi.DaughterRollModifier;
                  gi.DaughterRollModifier = 0;
                  //--------------------------------
                  dieRoll += gi.SeneschalRollModifier;
                  gi.SeneschalRollModifier = 0;
                  //--------------------------------
                  if (true == gi.IsSecretLadyAeravir) // know the secret of Lady Aeravir
                     ++dieRoll;
                  //--------------------------------
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e211f": // seeking audience with Dwarven King
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2:                                                                                                                   // no audience ever
                        gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, Utilities.FOREVER);
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults[key][0].ToString());
                           return false;
                        }
                        break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e154"; gi.DieRollAction = GameAction.EncounterRoll; break;                  // meet king's daughter
                     case 4: gi.EventDisplayed = gi.EventActive = "e149"; gi.ForbiddenAudiences.AddClothesConstraint(princeTerritory); break;  // learn court manners
                     case 5: gi.EventDisplayed = gi.EventActive = "e158"; break;                                                               // hostile guards
                     case 6:
                     case 7:                                                                                                                   // nothing happens
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults[key][0].ToString());
                           return false;
                        }
                        break;
                     case 8: gi.EventDisplayed = gi.EventActive = "e153"; break;                                                          // master of household
                     case 9:                                                                                                              // seneschal requires a bribe 
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        if (gi.Bribe <= gi.GetCoins())
                        {
                           gi.EventDisplayed = gi.EventActive = "e148";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, Utilities.FOREVER);
                           gi.EventDisplayed = gi.EventActive = "e148e";
                        }
                        break;
                     case 10:
                     case 11:                                                                                                             // pay your respects
                        ++gi.Statistic.myNumOfAudience;
                        gi.EventDisplayed = gi.EventActive = "e150";
                        if (false == gi.AddCoins("EncounterRoll(e211f)", 50))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, gi.Days + 1);
                        break;
                     case 12: gi.EventDisplayed = gi.EventActive = "e151"; gi.DieRollAction = GameAction.EncounterRoll; break; // find favor
                     default: gi.EventDisplayed = gi.EventActive = "e152"; gi.IsNobleAlly = true; ++gi.Statistic.myNumOfAudience; break;                       // ally                       
                  }
               }
               else
               {
                  ++gi.Statistic.myNumOfAudienceAttempt;
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  //foreach (ITerritory t in letters)
                  //   gi.LetterOfRecommendations.Remove(t);
                  gi.ForbiddenAudiences.RemoveLetterConstraints(princeTerritory);
                  //--------------------------------
                  dieRoll += gi.DaughterRollModifier;
                  gi.DaughterRollModifier = 0;
                  //--------------------------------
                  dieRoll += gi.SeneschalRollModifier;
                  gi.SeneschalRollModifier = 0;
                  //--------------------------------
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e211g": // attempt to dispose Baron Huldra with real Heir
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  ++gi.Statistic.myNumOfAudience;
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (9 < gi.DieResults[key][0])
                  {
                     action = GameAction.EndGameWin; // Dispose Huldra with real heir
                     gi.GamePhase = GamePhase.EndGame;
                     gi.EndGameReason = "Restore True Heir to Huldra Throne in Audience.";
                     gi.EventDisplayed = gi.EventActive = "e501";
                     gi.DieRollAction = GameAction.DieRollActionNone;
                     gi.Statistic.myNumWins++;
                     gi.Statistic.myEndDaysCount = gi.Days;
                     gi.Statistic.myEndPartyCount = gi.PartyMembers.Count;
                     gi.Statistic.myEndCoinCount = gi.GetCoins();
                     gi.Statistic.myEndFoodCount = gi.GetFoods();
                     GameEngine.theFeatsInGame.myIsHuldraDesposedWin = true;
                  }
                  else
                  {
                     ITerritory t = Territory.theTerritories.Find("1212");
                     if (null == t)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins() return false for ae=" + gi.EventActive + " for a=" + action.ToString());
                        return false;
                     }
                     gi.ForbiddenHexes.Add(t);
                     if (false == EncounterEscape(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEscape() return false for ae=" + gi.EventActive);
                        return false;
                     }
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
               }
               else
               {
                  ++gi.Statistic.myNumOfAudienceAttempt;
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  //foreach (ITerritory t in letters)
                  //   gi.LetterOfRecommendations.Remove(t);
                  gi.ForbiddenAudiences.RemoveLetterConstraints(princeTerritory);
                  //--------------------------------
                  dieRoll += gi.DaughterRollModifier;
                  gi.DaughterRollModifier = 0;
                  //--------------------------------
                  dieRoll += gi.SeneschalRollModifier;
                  gi.SeneschalRollModifier = 0;
                  //--------------------------------
                  if (true == gi.IsMagicUserDismissed)
                  {
                     ++dieRoll;
                     gi.IsMagicUserDismissed = false;
                  }
                  //--------------------------------
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e212":
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0])
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e212a"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e212b"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e212c"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e212d"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e212e"; break;
                     case 7: gi.EventDisplayed = gi.EventActive = "e212f"; break;
                     case 8: gi.EventDisplayed = gi.EventActive = "e212g"; break;
                     case 9: gi.EventDisplayed = gi.EventActive = "e212h"; break;
                     case 10:
                        action = GameAction.E228ShowTrueLove;
                        gi.EventStart = gi.EventDisplayed = gi.EventActive = "e212i";
                        gi.CapturedWealthCodes.Add(100);
                        gi.ForbiddenHexes.Add(princeTerritory);
                        IMapItem trueLove = CreateCharacter(gi, "TrueLovePriestDaughter");
                        gi.AddCompanion(trueLove);
                        break;
                     case 11: gi.EventDisplayed = gi.EventActive = "e212j"; break;
                     case 12: gi.EventDisplayed = gi.EventActive = "e212k"; break;
                     default: gi.EventDisplayed = gi.EventActive = "e212m"; break;
                  }
               }
               else
               {
                  ++gi.Statistic.myNumOfOffering;
                  if (true == gi.IsOfferingModifier)
                     dieRoll += 1;
                  gi.IsOfferingModifier = false;
                  //--------------------------------
                  if (true == gi.SecretRites.Contains(princeTerritory))
                     dieRoll += 1;
                  //--------------------------------
                  if ((true == gi.IsInfluenceModifier) && ((gi.Days - gi.DayOfLastOffering) < 2))
                     dieRoll += 3;
                  gi.IsInfluenceModifier = false;
                  //--------------------------------
                  if ((true == gi.IsOmenModifier) && ((gi.Days - gi.DayOfLastOffering) < 2))
                     dieRoll += 1;
                  gi.IsOmenModifier = false;
                  //--------------------------------
                  if (true == gi.IsChagaDrugProvided)  // Chaga Drug adds one
                     dieRoll += 1;
                  gi.IsChagaDrugProvided = false;
                  //--------------------------------
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  //foreach (ITerritory t in letters)
                  //   gi.LetterOfRecommendations.Remove(t);
                  gi.ForbiddenAudiences.RemoveLetterConstraints(princeTerritory);
                  //--------------------------------
                  bool isOfferingConstraint = gi.ForbiddenAudiences.IsOfferingsConstraint(princeTerritory, gi.Days);
                  if (true == isOfferingConstraint)
                     dieRoll += 3;
                  gi.ForbiddenAudiences.RemoveOfferingsConstraints(princeTerritory);
                  gi.ForbiddenAudiences.RemovePurifyConstraints(princeTerritory, gi.Purifications);
                  //--------------------------------
                  gi.DieResults[key][0] = dieRoll;
                  gi.DayOfLastOffering = gi.Days;
               }
               break;
            case "e212l": // learn secrets
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1:
                  case 2:
                     if (true == gi.IsHuldraHeirKilled)
                     {
                        gi.EventDisplayed = gi.EventActive = "e144e";
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e144";
                        gi.IsSecretBaronHuldra = true;
                     }
                     break;
                  case 3: case 4: gi.EventDisplayed = gi.EventActive = "e145"; gi.IsSecretLadyAeravir = true; break;
                  case 5: case 6: gi.EventDisplayed = gi.EventActive = "e146"; gi.IsSecretCountDrogat = true; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e213a":
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterRoll(): MovementUsed=Movement for ae=" + key);
                  gi.Prince.MovementUsed = gi.Prince.Movement;
               }
               else
               {
                  if (12 == gi.DieResults[key][0])
                     gi.IsRaftDestroyed = true;
                  else
                     gi.IsRaftDestroyed = false;
                  action = GameAction.TravelLostCheck;
                  gi.GamePhase = GamePhase.Travel;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.RaftState = RaftEnum.RE_RAFT_ENDS_TODAY;
               }
               break;
            case "e214":
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  ICaches caches = gi.Caches.Sort();
                  ICache targetCache = caches.Find(princeTerritory.Name);
                  if (null == targetCache)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): targetCache=null for t=" + princeTerritory.Name);
                     return false;
                  }
                  else
                  {
                     switch (gi.DieResults["e214"][0]) // Based on the die roll, implement the correct screen
                     {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                           Logger.Log(LogEnum.LE_MANAGE_CACHE, "EncounterRoll(): RETREIVE targetCache=" + targetCache.Coin.ToString() + " for t=" + princeTerritory.Name + " dr=" + dieRoll.ToString());
                           gi.Caches.Remove(targetCache);
                           if ( false == gi.AddCoins("EncounterRoll(e214)", targetCache.Coin, false))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(e214): AddCoins() returned false");
                              return false;
                           }
                           break;
                        case 5:
                           break;
                        case 6:
                           Logger.Log(LogEnum.LE_MANAGE_CACHE, "EncounterRoll(): REMOVE targetCache=" + targetCache.Coin.ToString() + " for t=" + princeTerritory.Name + " dr=" + dieRoll.ToString());
                           gi.Caches.Remove(targetCache);
                           break;
                        default:
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): reached default dr=" + dieRoll.ToString());
                           return false;
                     }
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
               }
               break;
            // ========================Combat================================
            // e300 to e310 are are converted to EncounterCombat 
            // ========================Escape================================
            case "e312a": action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
            case "e312b": action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
            case "e313a": action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
            case "e313b": action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
            // ========================Escape================================
            case "e314": // escape easy
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] <= gi.WitAndWile)
                  {
                     if (false == EncounterEscape(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEscape() return false for ae=" + gi.EventActive);
                        return false;
                     }
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  else
                  {
                     gi.IsEvadeActive = false;
                     gi.EventDisplayed = gi.EventActive = gi.EventStart;
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e315": // escape hard
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] < gi.WitAndWile)
                  {
                     if (false == EncounterEscape(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEscape() return false for ae=" + gi.EventActive);
                        return false;
                     }
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  else
                  {
                     gi.IsEvadeActive = false;
                     gi.EventDisplayed = gi.EventActive = gi.EventStart;
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            // ========================Hiding================================
            case "e317": // Hiding
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  if (gi.DieResults[key][0] <= gi.WitAndWile)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  else
                  {
                     gi.IsEvadeActive = false;
                     gi.EventDisplayed = gi.EventActive = gi.EventStart;
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e318": // Hiding
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] < gi.WitAndWile)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  else
                  {
                     gi.IsEvadeActive = false;
                     gi.EventDisplayed = gi.EventActive = gi.EventStart;
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e319": // Hiding
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.PartyMembers.Count <= gi.DieResults[key][0])
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  else
                  {
                     gi.IsEvadeActive = false;
                     gi.EventDisplayed = gi.EventActive = gi.EventStart;
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e320": // Hiding
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.PartyMembers.Count < gi.DieResults[key][0])
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  else
                  {
                     gi.IsEvadeActive = false;
                     gi.EventDisplayed = gi.EventActive = gi.EventStart;
                     if ("e071" == gi.EventStart)
                        gi.DieResults[gi.EventStart][0] = gi.EncounteredMembers.Count; // elf hide fails - repopulate the die roll 
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
               }
               break;
            // ========================Bribe================================
            case "e321": gi.EnteredHexes.Last().EventNames.Add(key); action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; theCombatModifer = 1; break;
            case "e322": gi.EnteredHexes.Last().EventNames.Add(key); action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
            case "e323": gi.EnteredHexes.Last().EventNames.Add(key); action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; theCombatModifer = -1; break;
            case "e324": gi.EnteredHexes.Last().EventNames.Add(key); action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e307"; break;
            // ========================Pass================================
            case "e326": // Pass encounter
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] <= gi.WitAndWile)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString());
                        return false;
                     }
                  }
                  else
                  {
                     theCombatModifer = 1;
                     gi.EventDisplayed = gi.EventActive = "e330";
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e327": // Pass encounter
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] <= gi.WitAndWile)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false ae = " + gi.EventActive);
                        return false;
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e330";
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e328": // Pass encounter
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] < gi.WitAndWile)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): EncounterEnd() returned false for ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString());
                        return false;
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e330";
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e329": // Pass encounter
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] < gi.WitAndWile)
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  else
                  {
                     theCombatModifer = -1;
                     gi.EventDisplayed = gi.EventActive = "e330";
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            // ========================Roll for Fight================================
            case "e330":
               gi.EnteredHexes.Last().EventNames.Add(key);
               action = GameAction.UpdateEventViewerActive;
               dieRoll += theCombatModifer;
               theCombatModifer = 0;
               if (dieRoll < 2)
               {
                  gi.EventDisplayed = gi.EventActive = "e310";
               }
               else if (12 < dieRoll)
               {
                  gi.EventDisplayed = gi.EventActive = "e300";
               }
               else
               {
                  switch (dieRoll) // Based on the die roll, implement the attack case
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e310"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e309"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e308"; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e307"; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                     case 7: gi.EventDisplayed = gi.EventActive = "e305"; break;
                     case 8: gi.EventDisplayed = gi.EventActive = "e304"; break;
                     case 9: gi.EventDisplayed = gi.EventActive = "e303"; break;
                     case 10: gi.EventDisplayed = gi.EventActive = "e302"; break;
                     case 11: gi.EventDisplayed = gi.EventActive = "e301"; break;
                     case 12: gi.EventDisplayed = gi.EventActive = "e300"; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               break;
            // ========================Character Interaction================================
            case "e331a": // failed to bribe to join
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: case 2: case 3: gi.EventDisplayed = gi.EventActive = "e401"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e321"; break; // bribe to pass
                     case 5: gi.EventDisplayed = gi.EventActive = "e322"; break; // bribe to pass 
                     case 6: gi.EventDisplayed = gi.EventActive = "e323"; break; // bribe to pass
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e332a": // failed to bribe to hire
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: case 2: gi.EventDisplayed = gi.EventActive = "e401"; break;
                     case 3: gi.EventDisplayed = gi.EventActive = "e321"; break;
                     case 4: gi.EventDisplayed = gi.EventActive = "e322"; break; // bribe to pass
                     case 5: gi.EventDisplayed = gi.EventActive = "e323"; break; // bribe to pass
                     case 6: gi.EventDisplayed = gi.EventActive = "e324"; gi.DieRollAction = GameAction.EncounterRoll; break; // bribe to pass with their threat
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e333a": // failed to hire 
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: case 2: case 3: case 4: gi.EventDisplayed = gi.EventActive = "e325"; break;                      // pass with dignity
                     case 5: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e327"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass dummies
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e336": // Plead Comrades
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                  {
                     action = GameAction.E018MarkOfCain;
                  }
                  else
                  {
                     if (gi.DieResults[key][0] <= (gi.WitAndWile + gi.MonkPleadModifier))
                     {
                        foreach (IMapItem comrade in gi.EncounteredMembers)
                           gi.AddCompanion(comrade);
                        gi.EncounteredMembers.Clear();
                     }
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e337": // Plead Comrades
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                  {
                     action = GameAction.E018MarkOfCain;
                  }
                  else
                  {
                     if (gi.DieResults[key][0] <= (gi.WitAndWile + gi.MonkPleadModifier))
                     {
                        foreach (IMapItem comrade in gi.EncounteredMembers)
                           gi.AddCompanion(comrade);
                        gi.EncounteredMembers.Clear();
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                           return false;
                        }
                     }
                     else
                     {
                        action = GameAction.UpdateEventViewerActive;
                        gi.EventDisplayed = gi.EventActive = "e337a";
                        gi.DieRollAction = GameAction.EncounterRoll;
                     }
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e337a": // Plead Failed
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e325"; break;                                               // pass with dignity
                     case 2: gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;  // combat
                     case 3: gi.EventDisplayed = gi.EventActive = "e340"; gi.DieRollAction = GameAction.EncounterRoll; break;  // looters
                     case 4: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // conversation 
                     case 5: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // conversation       
                     case 6: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e338a": // convince hirelings
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] <= gi.WitAndWile)
                  {
                     int wage = 1;
                     if (dieRoll == gi.WitAndWile)
                        wage = 2;
                     if (1 == gi.EncounteredMembers.Count)
                     {
                        if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                        {
                           action = GameAction.E018MarkOfCain;
                        }
                        else
                        {
                           if (gi.GetCoins() < wage)
                           {
                              gi.EventDisplayed = gi.EventActive = "e402";  // do not have enough money
                           }
                           else
                           {
                              IMapItem hireling = gi.EncounteredMembers[0];
                              hireling.Wages = wage;
                              hireling.PayDay = gi.Days + 1;
                              gi.AddCompanion(hireling);
                              gi.ReduceCoins("EncounterRoll(e338a)", wage);
                              gi.EncounteredMembers.Clear();
                              if (false == EncounterEnd(gi, ref action))
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: EncounterEnd() returned false for ae=" + gi.EventActive);
                                 return false;
                              }
                           }
                        }
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e338b";
                        action = GameAction.UpdateEventViewerActive;
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e338c": // convince hirelings
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] <= gi.WitAndWile)
                  {
                     int wage = 1;
                     if (dieRoll == gi.WitAndWile)
                        wage = 2;
                     if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                     {
                        action = GameAction.E018MarkOfCain;
                     }
                     else
                     {
                        if (gi.GetCoins() < wage * gi.EncounteredMembers.Count)
                        {
                           gi.EventDisplayed = gi.EventActive = "e402";  // do not have enough money
                        }
                        else
                        {
                           foreach (IMapItem hireling in gi.EncounteredMembers)
                           {
                              hireling.Wages = wage;
                              hireling.PayDay = gi.Days + 1;
                              gi.AddCompanion(hireling);
                              gi.ReduceCoins("EncounterRoll(e338c)", wage);
                           }
                           gi.EncounteredMembers.Clear();
                           if (false == EncounterEnd(gi, ref action))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: EncounterEnd() returned false for ae=" + gi.EventActive);
                              return false;
                           }
                        }
                     }
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e339a": // convince hirelings acting as individuals
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);

                  if (gi.DieResults[key][0] < gi.WitAndWile)
                  {
                     if (1 == gi.EncounteredMembers.Count)
                     {
                        if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                        {
                           action = GameAction.E018MarkOfCain;
                        }
                        else
                        {
                           IMapItem hireling = gi.EncounteredMembers[0];
                           hireling.Wages = 2;
                           hireling.PayDay = gi.Days + 1;
                           gi.AddCompanion(hireling);
                           gi.ReduceCoins("EncounterRoll(e339a)", 2);
                           gi.EncounteredMembers.Clear();
                           if (false == EncounterEnd(gi, ref action))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: EncounterEnd() returned false for ae=" + gi.EventActive);
                              return false;
                           }
                        }
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e339c";
                        action = GameAction.UpdateEventViewerActive;
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e339b";
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e339b": // failed to hire 
               gi.EnteredHexes.Last().EventNames.Add(key);
               switch (dieRoll) // Based on the die roll, implement event
               {
                  case 1: case 2: case 3: gi.EventDisplayed = gi.EventActive = "e325"; break;  // pass with dignity
                  case 4: case 5: case 6: gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e339d": // convince hirelings acting as group - Amazons
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] < gi.WitAndWile)
                  {
                     if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                     {
                        action = GameAction.E018MarkOfCain;
                     }
                     else
                     {
                        foreach (IMapItem hireling in gi.EncounteredMembers)
                        {
                           hireling.Wages = 2;
                           hireling.PayDay = gi.Days + 1;
                           gi.AddCompanion(hireling);
                           gi.ReduceCoins("EncounterRoll(e339d)", 2);
                        }
                        gi.EncounteredMembers.Clear();
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: EncounterEnd() returned false for ae=" + gi.EventActive);
                           return false;
                        }
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e339b";
                     action = GameAction.UpdateEventViewerActive;
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e340": // looters
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  if (gi.DieResults[key][0] <= (gi.WitAndWile + gi.MonkPleadModifier))
                  {
                     if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
                     {
                        action = GameAction.E018MarkOfCain;
                     }
                     else
                     {
                        foreach (IMapItem looter in gi.EncounteredMembers)
                        {
                           looter.IsLooter = true;
                           gi.AddCompanion(looter);
                        }
                        gi.EncounteredMembers.Clear();
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                           return false;
                        }
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e340a";
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e340a": // looters hostile
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1:
                     case 2:
                        foreach (IMapItem mi in gi.PartyMembers) // party members do not participate in fight against looters
                        {
                           if ("Prince" != mi.Name)
                              gi.LostPartyMembers.Add(mi);
                        }
                        gi.PartyMembers.Clear();
                        gi.PartyMembers.Add(gi.Prince);
                        gi.EventDisplayed = gi.EventActive = "e330";
                        action = GameAction.UpdateEventViewerActive;
                        gi.DieRollAction = GameAction.EncounterRoll;
                        break;
                     case 3:
                     case 4:
                        gi.EventDisplayed = gi.EventActive = "e330";
                        action = GameAction.UpdateEventViewerActive;
                        gi.DieRollAction = GameAction.EncounterRoll;
                        break;
                     case 5:
                     case 6: // did not fight. give over looted coin to party
                        gi.AddCoins("EncounterRoll(e340a)", gi.LooterCoin, false); // Steal looter coin and return back to fold
                        gi.LooterCoin = 0;
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + gi.DieResults[key][0].ToString()); return false;
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e341": // Conversation
               gi.IsDayEnd = true;  // conversation ends the day
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (0 == dieRoll)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Invalid state for ae=" + key + " with dr=" + dieRoll.ToString() + " but expecting a value of 2-12");
                     return false;
                  }
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  int result = gi.DieResults["e341"][0];
                  if ("e016a" == gi.EventStart) // magician's home - if combat, use e016b
                  {
                     if ((2 == result) || (3 == result) || (4 == result))
                     {
                        gi.IsMagicianProvideGift = false; // if there is combat, the gift goes away
                        if (false == gi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
                        {
                           gi.EventDisplayed = gi.EventActive = "e016b";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e016c";
                        }
                        break;
                     }
                  }
                  switch (result)
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e310"; gi.DieRollAction = GameAction.EncounterRoll; gi.IsAssassination = true; break;  // attack
                     case 3: gi.EventDisplayed = gi.EventActive = "e308"; gi.DieRollAction = GameAction.EncounterRoll; break;  // attack
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; gi.DieRollAction = GameAction.EncounterRoll; break;  // attack 
                     case 5:                                                                                                   // bride to join - 10gp
                        gi.EventDisplayed = gi.EventActive = "e331";
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        foreach (IMapItem mi in gi.EncounteredMembers)
                           mi.IsTownCastleTempleLeave = true;
                        break;
                     case 6:                                                                                                   // bride to hire
                        gi.EventDisplayed = gi.EventActive = "e332";
                        gi.Bribe = 5;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        Logger.Log(LogEnum.LE_BRIBE, "EncounterRoll(): bribe=" + gi.Bribe.ToString() + " for ae=" + key + " a=" + action.ToString());
                        break;
                     case 7: gi.EventDisplayed = gi.EventActive = "e333"; break;                                               // hirelings
                     case 8: gi.EventDisplayed = gi.EventActive = "e338"; break;                                               // hirelings
                     case 9: gi.EventDisplayed = gi.EventActive = "e335"; break;                                               // escapee
                     case 10: gi.EventDisplayed = gi.EventActive = "e336"; gi.DieRollAction = GameAction.EncounterRoll; break; // please comrades - sympathetic
                     case 11: gi.EventDisplayed = gi.EventActive = "e340"; gi.DieRollAction = GameAction.EncounterRoll; break; // looters
                     case 12: gi.EventDisplayed = gi.EventActive = "e334"; break;                                              // ally
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               break;
            case "e342": // Inquiry
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EnteredHexes.Last().EventNames.Add(key);
                  int result = gi.DieResults[key][0];
                  if ("e016a" == gi.EventStart) // magician's home - if combat, use e016b
                  {
                     if ((2 == result) || (3 == result) || (5 == result))
                     {
                        gi.IsMagicianProvideGift = false;  // no gift given if this is combat
                        if (false == gi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
                        {
                           gi.EventDisplayed = gi.EventActive = "e016b";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e016c";
                        }
                        break;
                     }
                  }
                  switch (result)
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e309"; break;                                               // insult
                     case 3: gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;  // unwilling combat
                     case 4:                                                                                                   // looters
                        if ("e130" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e130g";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e340";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;                                               // characters attack
                     case 6: gi.EventDisplayed = gi.EventActive = "e325"; break;                                               // pass with dignity
                     case 7:                                                                                                   // pass or talk further                                                                                                // pass or talk
                        if ("e130" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e130g";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e342a";
                           gi.DieResults["e341"][0] = Utilities.NO_RESULT;
                        }
                        break;
                     case 8:                                                                                                   // hire
                        if ("e130" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e130g";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e333";
                        }
                        break;
                     case 9:                                                                                                   // convince to hire
                        if ("e130" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e130g";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e339";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;
                     case 10:                                                                                                  // please comrades - unsavory 
                        if ("e130" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e130g";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e337";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break; // 
                     case 11:                                                                                                  // uninterested but may hire 
                        if ("e130" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e130g";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e338";
                        }
                        break;
                     case 12:                                                                                                  // plead comradesFol
                        if ("e130" == gi.EventStart)
                        {
                           gi.EventDisplayed = gi.EventActive = "e130g";
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e336";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString() + " result=" + result.ToString()); return false;
                  }
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EEEE123EEEEE - Reached default ae=" + gi.EventActive);
               return false;
         }
         return true;
      }
      protected bool EncounterEnd(IGameInstance gi, ref GameAction action)
      {
         ITerritory princeTerritory = gi.Prince.Territory;
         gi.ProcessIncapacitedPartyMembers(gi.EventActive);
         //---------------------------------------------------
         if (true == gi.DwarfAdviceLocations.Contains(princeTerritory)) // encounter dwarf advice event if not already encountered
         {
            action = GameAction.UpdateEventViewerActive;
            gi.EventDisplayed = gi.EventActive = "e006g";
            gi.DieRollAction = GameAction.EncounterRoll;
            return true;
         }
         //---------------------------------------------------
         if (true == gi.IsReaverClanFight)  // e014b - If this is reaver encounter, they fight after inquiries
         {
            gi.IsReaverClanFight = false;
            if (0 < gi.EncounteredMembers.Count) // only fight if there are reavers left to fight
            {
               gi.EventDisplayed = gi.EventActive = "e307";
               action = GameAction.UpdateEventViewerActive;
               return true;
            }
         }
         gi.EncounteredMembers.Clear();
         //---------------------------------------------------
         if (true == gi.IsReaverClanTrade)  // e015a - If this is reaver encounter, they trade after inquiries
         {
            gi.IsReaverClanTrade = false;
            gi.EventDisplayed = gi.EventActive = "e015b";
            action = GameAction.UpdateEventViewerActive;
            return true;
         }
         //---------------------------------------------------
         if (0 < gi.CapturedWealthCodes.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): WC WC WC WC WC WC WC WC WC WC WC WC WC WC.Count=" + gi.CapturedWealthCodes.Count.ToString() + " for es=" + gi.EventStart + " ae=" + gi.EventActive + " a=" + action.ToString());
            int capturedCoin = 0;
            foreach (int wc in gi.CapturedWealthCodes) // All wealth should be moved to party
            {
               int coin = GameEngine.theTreasureMgr.GetCoin(wc);
               if (coin < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): GetCoin()=" + coin.ToString() + " es=" + gi.EventStart);
                  return false;
               }
               capturedCoin += coin;
            }
            if (false == gi.AddCoins("EncounterEnd", capturedCoin))
            {
               Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): AddCoins() returned false for es=" + gi.EventStart);
               return false;
            }
            gi.CapturedWealthCodes.Clear();
         }
         //---------------------------------------------------
         if (0 < gi.FickleCoin)
         {
            action = GameAction.UpdateEventViewerActive;
            gi.EventDisplayed = gi.EventActive = "e331b";
            return true;
         }
         //---------------------------------------------------
         if (0 < gi.LooterCoin)
         {
            action = GameAction.UpdateEventViewerActive;
            gi.EventDisplayed = gi.EventActive = "e340b";
            gi.DieRollAction = GameAction.EncounterRoll;
            return true;
         }
         //---------------------------------------------------
         if ((true == gi.IsSpecialItemHeld(SpecialEnum.MagicBox)) && (true == gi.IsMagicInParty()))
         {
            action = GameAction.UpdateEventViewerActive;
            gi.EventDisplayed = gi.EventActive = "e140b";
            gi.DieRollAction = GameAction.EncounterRoll;
            if( false == gi.RemoveSpecialItem(SpecialEnum.MagicBox) )
            {
               Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): gi.RemoveSpecialItem(SpecialEnum.MagicBox) for " + gi.EventStart + " sc=" + gi.SunriseChoice);
               return false;
            }
            return true;
         }
         //---------------------------------------------------
         if ((true == gi.IsSpecialItemHeld(SpecialEnum.PegasusMountTalisman)) && (true == gi.IsSpecialistInParty()) && (false == gi.IsPegasusSkip))
         {
            action = GameAction.UpdateEventViewerActive;
            gi.EventDisplayed = gi.EventActive = "e188b";
            return true;
         }
         //---------------------------------------------------
         string riverState = "NONE";
         if (0 < gi.MapItemMoves.Count)
            riverState = gi.MapItemMoves[0].RiverCross.ToString();
         Logger.Log(LogEnum.LE_END_ENCOUNTER, "EncounterEnd(): ************ae=" + gi.EventActive + " c=" + gi.SunriseChoice.ToString() + " rs=" + riverState + " m=" + gi.Prince.MovementUsed + "/" + gi.Prince.Movement + "****************************");
         bool isEndOfDay = false;
         switch (gi.SunriseChoice)
         {
            case GamePhase.Travel:
               if (0 == gi.MapItemMoves.Count)
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): gi.MapItemMoves.Count=0 for " + gi.EventStart + " sc=" + gi.SunriseChoice);
                  return false;
               }
               else if (RiverCrossEnum.TC_CROSS_FAIL == gi.MapItemMoves[0].RiverCross)
               {
                  isEndOfDay = true;
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "EncounterEnd():  gi.MapItemMoves.Clear()  a=" + action.ToString());
                  gi.MapItemMoves.Clear();
               }
               else if (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == gi.MapItemMoves[0].RiverCross)
               {
                  gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_YES;
                  action = GameAction.TravelLostCheck;
                  gi.GamePhase = GamePhase.Travel;
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               else if (RiverCrossEnum.TC_CROSS_YES == gi.MapItemMoves[0].RiverCross)
               {
                  action = GameAction.TravelLostCheck;
                  gi.GamePhase = GamePhase.Travel;
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               else if (RaftEnum.RE_RAFT_CHOSEN == gi.RaftState)
               {
                  gi.EventDisplayed = gi.EventActive = "e213a";
                  gi.GamePhase = GamePhase.Encounter;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else if (gi.Prince.MovementUsed < gi.Prince.Movement)
               {
                  action = GameAction.Travel;
                  gi.GamePhase = GamePhase.Travel;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.IsGridActive = true;  // EncounterEnd()
                  if (true == gi.IsShortHop)
                     gi.EventDisplayed = gi.EventActive = "e204s"; // next screen to show
                  else if (true == gi.IsAirborne)
                     gi.EventDisplayed = gi.EventActive = "e204a"; // next screen to show
                  else if (true == gi.IsPartyRiding())
                     gi.EventDisplayed = gi.EventActive = "e204m"; // next screen to show
                  else
                     gi.EventDisplayed = gi.EventActive = "e204u"; // next screen to show
               }
               else if (true == gi.IsAirborne) // airborne and used up movement - check for lost encounter in landing hex
               {
                  gi.IsAirborne = false;
                  gi.IsShortHop = false;
                  gi.IsAirborneEnd = true;
                  action = GameAction.TravelLostCheck;
                  gi.GamePhase = GamePhase.Travel;
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               else // used up movement
               {
                  isEndOfDay = true;
                  gi.IsShortHop = false;
                  gi.IsAirborneEnd = false;
                  foreach (IMapItem mi in gi.PartyMembers) // e151 - Cavalry departs at end of day after first day of travel
                  {
                     if (true == mi.Name.Contains("Cavalry"))
                     {
                        gi.RemoveAbandonerInParty(mi);
                        gi.IsCavalryEscort = false;
                        break;
                     }
                  }
                  if ("e114" != gi.EventActive)  // e114 - eagle hunt only helps one day of travel after event "e114"
                     gi.IsEagleHunt = false;
               }
               break;
            case GamePhase.StartGame:
            case GamePhase.SeekNews:
            case GamePhase.SeekHire:
            case GamePhase.SeekAudience:
            case GamePhase.SeekOffering:
            case GamePhase.SearchRuins:
            case GamePhase.Encounter:
               isEndOfDay = true;
               break;
            case GamePhase.Rest:
               if (false == gi.IsExhausted)
               {
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     if (0 < mi.Wound)
                     {
                        if (true == mi.Name.Contains("Prince"))
                           gi.Statistic.myNumOfPrinceHeal++;
                        else
                           gi.Statistic.myNumOfPartyHeal++;
                     }
                     mi.HealWounds(1, 0); // EncounterEnd() - Resting heals one wound
                     mi.IsExhausted = false;
                  }
               }
               isEndOfDay = true;
               break;
            case GamePhase.SearchCache:
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               Logger.Log(LogEnum.LE_NEXT_ACTION, "EncounterEnd(): SearchCache action");
               gi.EventActive = gi.EventDisplayed = "e214";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case GamePhase.SearchTreasure:
               bool isSearchHexAvailable = false;
               if (true == gi.SecretClues.Contains(princeTerritory))
               {
                  gi.EventDisplayed = gi.EventActive = "e147a";
                  isSearchHexAvailable = true;
               }
               else if ( (true == gi.WizardAdviceLocations.Contains(princeTerritory)) || (true == gi.PixieAdviceLocations.Contains(princeTerritory)) )
               {
                  gi.EventDisplayed = gi.EventActive = "e026";
                  isSearchHexAvailable = true;
               }
               else // if escaped during encounter and no longer in the hex with treasure
               {
                  Logger.Log(LogEnum.LE_ENCOUNTER_ESCAPE, "EncounterEnd(): prince=" + princeTerritory.Name + " and unable to search treasure when " + gi.SunriseChoice.ToString());
                  isEndOfDay = true;
               }
               if( true == isSearchHexAvailable )
               {
                  gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): reached default gi.SunriseChoice=" + gi.SunriseChoice.ToString());
               return false;
         }
         if ((true == isEndOfDay) || (true == gi.IsDayEnd))
         {
            Logger.Log(LogEnum.LE_END_ENCOUNTER, "EncounterEnd(): eeeeeeeeeee ae=" + gi.EventActive + " c=" + gi.SunriseChoice.ToString() + " rs=" + riverState + " m=" + gi.Prince.MovementUsed + "/" + gi.Prince.Movement + "eeeeeeeeeeeeeeeeeeeeeeeeeeee");
            if (RaftEnum.RE_RAFT_ENDS_TODAY == gi.RaftState)
            {
               if (0 == gi.Prince.Territory.Rafts.Count) // if there are no raft hexes, destroy the raft
                  gi.IsRaftDestroyed = true;
               if ((true == gi.IsRaftDestroyed) || (0 == gi.GetCoins())) // e122 - raft destroyed on 12 
                  gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT;
               else
                  gi.RaftState = RaftEnum.RE_RAFT_SHOWN;
            }
            gi.IsRaftDestroyed = false;
            if (false == SetHuntState(gi, ref action)) // end of encounter
            {
               Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): SetHuntState() return false for ae=" + gi.EventActive);
               return false;
            }
         }
         return true;
      }
      //--------------------------------------------
      protected bool AddGuideTerritories(IGameInstance gi, IMapItem guide, int range)
      {
         List<string> masterList = new List<string>();
         Queue<string> tStack = new Queue<string>();
         Queue<int> depthStack = new Queue<int>();
         Dictionary<string, bool> visited = new Dictionary<string, bool>();
         ITerritory startT = guide.Territory;
         tStack.Enqueue(startT.Name);
         depthStack.Enqueue(0);
         visited[startT.Name] = false;
         masterList.Add(startT.Name);
         while (0 < tStack.Count)
         {
            String name = tStack.Dequeue();
            int depth = depthStack.Dequeue();
            if (true == visited[name])
               continue;
            if (range <= depth)
               continue;
            visited[name] = true;
            ITerritory t = Territory.theTerritories.Find(name);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "AddGuideTerritories(): t=null for " + name);
               return false;
            }
            foreach (string adj in t.Adjacents)
            {
               ITerritory adjacent = Territory.theTerritories.Find(adj);
               if (null == adjacent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "AddGuideTerritories(): adjacent=null for " + adj + " t=" + t.Name + " name=" + name + " guide=" + guide.Name + " r=" + range.ToString());
                  return false;
               }
               tStack.Enqueue(adjacent.Name);
               depthStack.Enqueue(depth + 1);
               if (false == masterList.Contains(adj))
               {
                  masterList.Add(adj);
                  visited[adj] = false;
               }
            }
         }
         foreach (string tName in masterList)  // add the territories to the guide
         {
            ITerritory t = Territory.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "AddGuideTerritories(): tName=" + tName);
               return false;
            }
            guide.GuideTerritories.Add(t);
         }
         return true;
      }
      protected bool ResetDieResultsForAudience(IGameInstance gi)
      {
         ITerritory t = gi.Prince.Territory;
         ++gi.Statistic.myNumOfAudience;
         gi.DieRollAction = GameAction.EncounterRoll;
         if (true == gi.IsInTown(t))
         {
            gi.EventDisplayed = gi.EventActive = "e211a";
            gi.DieResults["e211a"][0] = Utilities.NO_RESULT;
         }
         else if (true == gi.IsInTemple(t))
         {
            gi.EventDisplayed = gi.EventActive = "e211b";
            gi.DieResults["e211b"][0] = Utilities.NO_RESULT;
         }
         else if (true == gi.IsInCastle(t))
         {
            if ("1212" == t.Name)
            {
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211c";
               gi.DieResults["e211c"][0] = Utilities.NO_RESULT;
            }
            else if ("0323" == t.Name)
            {
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211d";
               gi.DieResults["e211d"][0] = Utilities.NO_RESULT;
            }
            else if ("1923" == t.Name)
            {
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211e";
               gi.DieResults["e211e"][0] = Utilities.NO_RESULT;
            }
            else if (true == gi.DwarvenMines.Contains(t))
            {
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211f";
               gi.DieResults["e211f"][0] = Utilities.NO_RESULT;
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetDieResultsForAudience(): 1-Reached Default territory type t=" + t.ToString());
               return false;
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ResetDieResultsForAudience(): 2-Reached Default territory type t=" + t.ToString());
            return false;
         }
         return true;
      }
      protected bool MarkedForDeath(IGameInstance gi)
      {
         gi.EventStart = gi.EventDisplayed = gi.EventActive = "e061";
         gi.IsJailed = true;
         gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT; // MarkedForDeath()
         gi.HydraTeethCount = 0;
         gi.ChagaDrugCount = 0;
         gi.Prince.ResetPartial();
         if (false == gi.RemoveBelongingsInParty())
         {
            Logger.Log(LogEnum.LE_ERROR, "MarkedForDeath(): RemoveBelongingsInParty() returned false");
            return false;
         }
         return true;
      }
      protected void Enslaved(IGameInstance gi)
      {
         gi.EventStart = gi.EventDisplayed = gi.EventActive = "e024b";
         gi.WanderingDayCount = 0;
         gi.DieRollAction = GameAction.EncounterRoll;
         gi.IsEnslaved = true;
         gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT; // Enslaved()
         gi.HydraTeethCount = 0;
         gi.ChagaDrugCount = 0;
         gi.Prince.ResetPartial();
      }
      protected void ThrownInDungeon(IGameInstance gi)
      {
         gi.EventStart = gi.EventDisplayed = gi.EventActive = "e062";
         gi.IsDungeon = true;
         IMapItems abandonedPartyMembers = new MapItems();
         foreach (IMapItem mi in gi.PartyMembers)
            abandonedPartyMembers.Add(mi);
         foreach (IMapItem mi in abandonedPartyMembers)
            gi.RemoveAbandonedInParty(mi);
         gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT;
         gi.HydraTeethCount = 0;
         gi.ChagaDrugCount = 0;
         gi.Prince.ResetPartial();
      }
      protected void Imprisoned(IGameInstance gi)
      {
         gi.EventStart = gi.EventDisplayed = gi.EventActive = "e063";
         gi.IsJailed = true;
         gi.RaftStatePrevUndo = gi.RaftState = RaftEnum.RE_NO_RAFT;
         gi.HydraTeethCount = 0;
         gi.ChagaDrugCount = 0;
         gi.Prince.ResetPartial();
         if (false == gi.RemoveBelongingsInParty())
            Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): RemoveBelongingsInParty() returned false");
      }
      protected ITerritory FindClosestTown(IGameInstance gi)
      {
         ITerritory startT = gi.Prince.Territory;
         if (true == gi.IsInTown(startT))
            return startT;
         ITerritory minT = null;
         double minMetric = 100000000000.0;  // start with a big number
         foreach (ITerritory t in Territory.theTerritories)
         {
            if (false == gi.IsInTown(t))
               continue;
            double x = t.CenterPoint.X - startT.CenterPoint.X;
            double y = t.CenterPoint.Y - startT.CenterPoint.Y;
            double distance = Math.Sqrt(x * x + y * y);
            if (distance < minMetric)
            {
               minMetric = distance;
               minT = t;
            }
         }
         return minT;
      }
      protected ITerritory FindClosestTemple(IGameInstance gi)
      {
         ITerritory startT = gi.Prince.Territory;
         if (true == gi.IsInTemple(startT))
            return startT;
         ITerritory minT = null;
         double minMetric = 100000000000.0;  // start with a big number
         foreach (ITerritory t in Territory.theTerritories)
         {
            if (false == gi.IsInTemple(t))
               continue;
            double x = t.CenterPoint.X - startT.CenterPoint.X;
            double y = t.CenterPoint.Y - startT.CenterPoint.Y;
            double distance = Math.Sqrt(x * x + y * y);
            if (distance < minMetric)
            {
               minMetric = distance;
               minT = t;
            }
         }
         return minT;
      }
      protected ITerritory FindClosestCastle(IGameInstance gi)
      {
         ITerritory startT = gi.Prince.Territory;
         if (true == gi.IsInCastle(startT))
            return startT;
         ITerritory minT = null;
         double minMetric = 100000000000.0;  // start with a big number
         foreach (ITerritory t in Territory.theTerritories)
         {
            if (false == t.IsCastle)
               continue;
            double x = t.CenterPoint.X - startT.CenterPoint.X;
            double y = t.CenterPoint.Y - startT.CenterPoint.Y;
            double distance = Math.Sqrt(x * x + y * y);
            if (distance < minMetric)
            {
               minMetric = distance;
               minT = t;
            }
         }
         return minT;
      }
      protected bool MoveToClosestGoblinKeep(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "MoveToClosestGoblinKeep(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         ITerritory startT = gi.Prince.Territory;
         ITerritory minT = null;
         if (true == gi.GoblinKeeps.Contains(startT))
         {
            minT = startT;
         }
         else
         {
            double minMetric = 100000000000.0;  // start with a big number
            foreach (ITerritory t in Territory.theTerritories)
            {
               if (false == gi.GoblinKeeps.Contains(t))
                  continue;
               double x = t.CenterPoint.X - startT.CenterPoint.X;
               double y = t.CenterPoint.Y - startT.CenterPoint.Y;
               double distance = Math.Sqrt(x * x + y * y);
               if (distance < minMetric)
               {
                  minMetric = distance;
                  minT = t;
               }
            }
         }
         if (null == minT)
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveToClosestGoblinKeep(): minT=null finding GoblinKeep closest to t=" + startT.Name);
            return false;
         }
         gi.NewHex = minT;
         gi.EnteredHexes.Add(new EnteredHex(gi, ColorActionEnum.CAE_JAIL));
         if (false == AddMapItemMove(gi, minT))
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveToClosestGoblinKeep(): AddMapItemMove() returned error for t=" + minT.Name);
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "MoveToClosestGoblinKeep(): gi.MapItemMoves.Add() mim=" + gi.MapItemMoves[0].ToString());
         return true;
      }
      protected bool RiverCrossCheck(IGameInstance gi, ref GameAction action)
      {
         if (0 < gi.MapItemMoves.Count) // if succeeded in river encounter - cross river
         {
            if (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == gi.MapItemMoves[0].RiverCross)
            {
               gi.MapItemMoves[0].RiverCross = RiverCrossEnum.TC_CROSS_YES;
               action = GameAction.TravelLostCheck;
               gi.GamePhase = GamePhase.Travel;
               gi.DieRollAction = GameAction.DieRollActionNone;
            }
            else if (RiverCrossEnum.TC_CROSS_YES == gi.MapItemMoves[0].RiverCross)
            {
               action = GameAction.TravelLostCheck;
               gi.GamePhase = GamePhase.Travel;
               gi.DieRollAction = GameAction.DieRollActionNone;
            }
         }
         return true;
      }
   }
}