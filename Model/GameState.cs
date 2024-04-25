﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;

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
         if (RaftEnum.RE_RAFT_CHOSEN == gi.RaftState)
            gi.ReduceCoins(1); // pay one to raftsmen
         else                  // e122 - if travel some other way other than raft
            gi.RaftState = RaftEnum.RE_NO_RAFT;
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
               gi.EventDisplayed = gi.EventActive = "e213"; // next screen to show
            else if (true == gi.IsShortHop)
               gi.EventDisplayed = gi.EventActive = "e204s"; // next screen to show
            else if (true == gi.IsAirborne)
               gi.EventDisplayed = gi.EventActive = "e204a"; // next screen to show
            else if (true == gi.IsPartyRiding())
               gi.EventDisplayed = gi.EventActive = "e204m"; // next screen to show
            else
               gi.EventDisplayed = gi.EventActive = "e204u"; // next screen to show
         }
      }
      protected bool SetHuntState(IGameInstance gi, ref GameAction action)
      {
         ITerritory t = gi.NewHex;
         bool isStructure = gi.IsInTownOrCastle(t);
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
         if ((true == IsNorthofTragothRiver(princeTerritory)) && (false == gi.IsGuardEncounteredThisTurn) && ("e061" != gi.EventStart) && ("e062" != gi.EventStart) && ("e063" != gi.EventStart) && ("e024" != gi.EventStart))
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
               if (false == SetPlagueStateCheck(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "SetEndOfDayState(): SetPlagueStateCheck() returned false");
                  return false;
               }
            }
         }
         else
         {
            if (false == SetPlagueStateCheck(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "SetEndOfDayState(): SetPlagueStateCheck() returned false");
               return false;
            }
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
         // Characters joining party may have knowledge of other locations
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
            if (false == SetCampfireFinalConditionState(gi, ref action))
            {
               Logger.Log(LogEnum.LE_ERROR, "SetCampfireEncounterState(): SetCampfireFinalConditionState() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool SetCampfireFinalConditionState(IGameInstance gi, ref GameAction action)
      {
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
            gi.IsGridActive = true;   // SetCampfireFinalConditionState()
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
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "Wakeup():  gi.MapItemMoves.Clear()  a=" + action.ToString());
         gi.MapItemMoves.Clear();
         gi.IsNewDayChoiceMade = false;
         gi.IsHeavyRainDismount = false;  // reset to indicate user needs to choose dismount if e079 heavy rains shown at beginning of day
         gi.ForbiddenAudiences.RemoveTimeConstraints(gi.Days);
         if (true == PerformEndCheck(gi, ref action))
            return true;
         if (true == gi.IsJailed)
         {
            gi.GamePhase = GamePhase.Campfire;
            gi.EventDisplayed = gi.EventActive = "e203a"; // next screen to show
            gi.DieRollAction = GameAction.E203NightInPrison;
         }
         else if (true == gi.IsDungeon)
         {
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
            gi.GamePhase = GamePhase.SunriseChoice;      // Nominal - Wakeup()
            gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         return true;
      }
      protected bool PerformEndCheck(IGameInstance gi, ref GameAction action)
      {
         if ((6 == gi.DieResults["e203a"][0]) && ("e061" == gi.EventStart)) // need to show the battle axe chopping off head before ending game
            return false;
         if (true == gi.Prince.IsKilled)
         {
            action = GameAction.EndGameLost;
            gi.GamePhase = GamePhase.EndGame;
            if ("e203b" == gi.EventActive)
               gi.EndGameReason = "Beheaded in gory execution";
            else
               gi.EndGameReason = "Prince killed";
            return true;
         }
         if ((true == gi.Prince.IsUnconscious) && (1 == gi.PartyMembers.Count))
         {
            action = GameAction.EndGameLost;
            gi.GamePhase = GamePhase.EndGame;
            gi.EndGameReason = "Prince unconscious and alone to die";
            return true;
         }
         if (Utilities.MaxDays < gi.Days)
         {
            action = GameAction.EndGameLost;
            gi.GamePhase = GamePhase.EndGame;
            gi.EndGameReason = "Time Limit Reached";
            return true;
         }
         if (true == IsNorthofTragothRiver(gi.Prince.Territory))
         {
            if (true == gi.IsBlessed)
            {
               action = GameAction.EndGameWin;
               gi.GamePhase = GamePhase.EndGame;
               gi.EndGameReason = "Blessed by Gods and North of Tragoth River";
               return true;
            }
            else if (true == gi.IsSpecialItemHeld(SpecialEnum.StaffOfCommand))
            {
               action = GameAction.EndGameWin;
               gi.GamePhase = GamePhase.EndGame;
               gi.EndGameReason = "Hold the Staff of Command North of Tragoth River";
               return true;
            }
            else if (499 < gi.GetCoins())
            {
               action = GameAction.EndGameWin;
               gi.GamePhase = GamePhase.EndGame;
               gi.EndGameReason = "500+ gold and North of Tragoth River";
               return true;
            }
            else if (true == gi.IsSpecialItemHeld(SpecialEnum.RoyalHelmOfNorthlands))
            {
               if ("0101" == gi.Prince.Territory.Name)  // In Ogon 
               {
                  action = GameAction.EndGameWin;
                  gi.GamePhase = GamePhase.EndGame;
                  gi.EndGameReason = "Hold the Royal Helm of Northlands when in Ogon";
                  return true;
               }
               else if ("1501" == gi.Prince.Territory.Name) // In Weshor
               {
                  action = GameAction.EndGameWin;
                  gi.GamePhase = GamePhase.EndGame;
                  gi.EndGameReason = "Hold the Royal Helm of Northlands when in Weshor";
                  return true;
               }
            }
         }
         return false;
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
               Logger.Log(LogEnum.LE_ERROR, "EncounterEscape(): no adjacent territories w/o crossing river");
               return false;
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
         ITerritory adjacentTerritory = gi.Territories.Find(adjacentName);
         if (null == adjacentTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterEscape(): invalid param adjacent=null");
            return false;
         }
         gi.Prince.TerritoryStarting = gi.Prince.Territory;
         if (false == AddMapItemMove(gi, adjacentTerritory))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): AddMapItemMove() return false");
            return false;
         }
         if ("e036" == gi.EventStart) // e036 Party members not discovered again
            gi.LostPartyMembers.Clear();
         Logger.Log(LogEnum.LE_MOVE_COUNT, "EncounterEscape(): MovementUsed=Movement for a=" + action.ToString());
         gi.Prince.MovementUsed = gi.Prince.Movement; // no more travel or today
         // !!!!!!Must call EncounterEnd() in the calling routine if this is end of encounter b/c of EncounterEscapeFly and EncounterEscapeMounted take user to different screen to end encounter
         return true;
      }
      protected bool EncounterFollow(IGameInstance gi, ref GameAction action)
      {
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
         ITerritory adjacentTerritory = gi.Territories.Find(adjacentName);
         if (null == adjacentTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterFollow(): invalid param adjacent=null");
            return false;
         }
         //---------------------------------------------
         gi.IsMustLeaveHex = false;
         gi.IsImpassable = false;
         gi.IsTrueLoveHeartBroken = false;
         gi.IsTempleGuardEncounteredThisHex = false;
         gi.NewHex = adjacentTerritory;
         gi.EnteredTerritories.Add(gi.NewHex);
         if ((true == gi.IsExhausted) && ((true == gi.NewHex.IsOasis) || ("Desert" != gi.NewHex.Type))) // e120
            gi.IsExhausted = false;
         //---------------------------------------------
         gi.Prince.TerritoryStarting = gi.Prince.Territory;
         if (false == AddMapItemMove(gi, adjacentTerritory))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): AddMapItemMove() return false" );
            return false;
         }
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
      protected ITerritory FindRandomHexAdjacent(IGameInstance gi)
      {

         if (0 == gi.Prince.Territory.Adjacents.Count)
            return gi.Prince.Territory;
         int randomNum = Utilities.RandomGenerator.Next(gi.Prince.Territory.Adjacents.Count); // get a random index into the masterList
         string tName = gi.Prince.Territory.Adjacents[randomNum];
         ITerritory adjacent = gi.Territories.Find(tName);
         return adjacent;
      }
      protected ITerritory FindRandomHex(IGameInstance gi, int range)
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
            ITerritory t = gi.Territories.Find(name);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "FindRandomHex(): t=null for " + name);
               return null;
            }
            foreach (string adj in t.Adjacents)
            {
               ITerritory adjacent = gi.Territories.Find(adj);
               if (null == adjacent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "FindRandomHex(): adjacent=null for " + adj);
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
         ITerritory selected = gi.Territories.Find(masterList[randomNum]); // Find the territory of this random index
         if (null == selected)
         {
            Logger.Log(LogEnum.LE_ERROR, "FindRandomHex(): selected=null for " + masterList[randomNum]);
            return null;
         }
         return selected;
      }
      protected ITerritory FindClueHex(IGameInstance gi, int direction, int range)
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
               Logger.Log(LogEnum.LE_ERROR, "FindClueHex(): Reached default direction=" + direction.ToString());
               return null;
         }
         string hex = colNum.ToString("D2") + rowNum.ToString("D2");
         ITerritory selected = gi.Territories.Find(hex); // Find the territory of this random index
         if (null == selected)
         {
            Logger.Log(LogEnum.LE_ERROR, "FindClueHex(): selected=null for " + hex);
            return null;
         }
         return selected;
      }
      protected bool SetSubstitutionEvent(IGameInstance gi, ITerritory princeTerritory, bool isTravel = false)
      {
         if ((true == gi.IsHighPass) && (true == isTravel))
         {
            gi.IsHighPass = false;
            int tCount = gi.EnteredTerritories.Count;
            if (tCount < 2)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetSubstitutionEvent(): Invalid state with tCount=" + tCount.ToString());
               return false;
            }
            else
            {
               ITerritory previousTerritory = gi.EnteredTerritories[tCount - 2]; // If do not return from hex entered, must do a High Pass Check for deaths
               if (gi.NewHex.Name != previousTerritory.Name)
               {
                  gi.EventAfterRedistribute = gi.EventActive; // encounter this event after high pass check
                  gi.EventDisplayed = gi.EventActive = "e086a";
                  gi.GamePhase = GamePhase.Encounter;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
            }
            return true;
         }
         if (false == isTravel)
            gi.RaftState = RaftEnum.RE_NO_RAFT;
         // if traveling and flying into eagle lairs, show e115. If not traveling and in eagle lair, assume already there.
         if ((((true == gi.IsPartyFlying()) && (true == isTravel)) || (false == isTravel)) && (true == gi.EagleLairs.Contains(gi.NewHex))) // e115 - return to eagle lair for free food
         {
            gi.EventDisplayed = gi.EventActive = "e115";
            Logger.Log(LogEnum.LE_MOVE_COUNT, "SetSubstitutionEvent(): MovementUsed=Movement for ae=e115");
            gi.Prince.MovementUsed = gi.Prince.Movement; // end the day
            gi.IsPartyFed = true;
            gi.IsMountsFed = true;
         }
         else if (("e075" == gi.EventActive) && (true == gi.IsInStructure(princeTerritory))) // wolves encounter do not happen in structure
            gi.EventDisplayed = gi.EventActive = "e075a";
         else if (("e076" == gi.EventActive) && (true == gi.IsInStructure(princeTerritory))) // no hunting cat encounter if in structure
            gi.EventDisplayed = gi.EventActive = "e076a";
         else if (("e077" == gi.EventActive) && (true == gi.IsInStructure(princeTerritory))) // no herd of wild horses encounter if in structure
            gi.EventDisplayed = gi.EventActive = "e077a";
         else if (("e085" == gi.EventActive) && (true == gi.IsInStructure(princeTerritory))) // bear encounter does not happen in structure
            gi.EventDisplayed = gi.EventActive = "e085a";
         else if (("e095" == gi.EventActive) && (0 == gi.GetMountCount())) // Lost mounts do not happen if have none
            gi.EventDisplayed = gi.EventActive = "e095a";
         else if (("e096" == gi.EventActive) && (0 == gi.GetMountCount())) // Lost mounts do not happen if have none
            gi.EventDisplayed = gi.EventActive = "e096a";
         else if ("e078" == gi.EventActive)
         {
            if ((true == gi.IsInStructure(princeTerritory)) || (0 < princeTerritory.Roads.Count))
               gi.EventDisplayed = gi.EventActive = "e078a"; // majestic view from road
            else if (0 == gi.GetMountCount(true))
               gi.EventDisplayed = gi.EventActive = "e078b"; // majestic view with no horses
            else if (gi.Prince.MovementUsed == gi.Prince.Movement)
               gi.EventDisplayed = gi.EventActive = "e078c"; // bad going for horses
         }
         return true;
      } // Before showing Encounter Event, show another event based on hex contents
      protected bool SetEncounterOptions(IGameInstance gi, bool isNoLost, bool isForceLost, bool isForceLostEvent, bool isNoEvent, bool isForceEvent)
      {
         IOption optionNoLost = gi.Options.Find("NoLost");
         if (null == optionNoLost)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEncounterOptions(): gi.Options.Find(NoLost) returned null");
            return false;
         }
         optionNoLost.IsEnabled = isNoLost;
         //-------------------------------------------
         IOption optionForceLost = gi.Options.Find("ForceLost");
         if (null == optionForceLost)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEncounterOptions(): gi.Options.Find(ForceLost) returned null");
            return false;
         }
         optionForceLost.IsEnabled = isForceLost;
         //-------------------------------------------
         IOption optionForceLostEvent = gi.Options.Find("ForceLostEvent");
         if (null == optionForceLostEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEncounterOptions(): gi.Options.Find(ForceLostEvent) returned null");
            return false;
         }
         optionForceLostEvent.IsEnabled = isForceLostEvent;
         //-------------------------------------------
         IOption optionNoEvent = gi.Options.Find("NoEvent");
         if (null == optionNoEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEncounterOptions(): gi.Options.Find(NoEvent) returned null");
            return false;
         }
         optionNoEvent.IsEnabled = isNoEvent;
         //-------------------------------------------
         IOption optionForceEvent = gi.Options.Find("ForceEvent");
         if (null == optionForceEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEncounterOptions(): gi.Options.Find(ForceEvent) returned null");
            return false;
         }
         optionForceEvent.IsEnabled = isForceEvent;
         return true;
      }
      protected bool AddMapItemMove(IGameInstance gi, ITerritory newT)
      {
         //-------------------------------
         if (0 == gi.PartyMembers.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMapItemMove(): gi.PartyMembers.Count=0");
            return false;
         }
         gi.Prince.TerritoryStarting = gi.Prince.Territory;
         MapItemMove mim = new MapItemMove(gi.Territories, gi.Prince, newT);
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
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "AddMapItemMove(): gi.MapItemMoves.Add() mim=" + mim.ToString());
         gi.MapItemMoves.Add(mim);
         return true;
      }
   }
   //-----------------------------------------------------
   class GameStateSetup : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         switch (action)
         {
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               if ("e000a" == gi.EventDisplayed)
                  gi.EventActive = gi.EventDisplayed; // next screen to show
               break;
            case GameAction.RemoveSplashScreen:
               IOption option = gi.Options.Find("AutoSetup");
               if (null == option)
               {
                  returnStatus = "gi.Options.Find(AutoSetup) returned null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else
               {
                  if (true == option.IsEnabled)
                  {
                     if (false == PerformAutoSetup(ref gi, ref action))
                     {
                        returnStatus = "PerformAutoSetup() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
                     }
                  }
                  else
                  {
                     gi.DieRollAction = GameAction.EncounterLootStart;
                  }
               }
               break;
            case GameAction.EncounterLootStart:
               gi.CapturedWealthCodes.Add(2);
               gi.ActiveMember = gi.Prince;
               break;
            case GameAction.EncounterLootStartEnd:
               gi.EventDisplayed = gi.EventActive = "e000b"; // next screen to show
               gi.DieRollAction = GameAction.SetupRollWitsWiles;
               break;
            case GameAction.SetupRollWitsWiles:
               gi.WitAndWile = dieRoll;
               gi.EventDisplayed = gi.EventActive = "e000c"; // next screen to show
               gi.DieRollAction = GameAction.SetupManualWitsWiles;
               break;
            case GameAction.SetupManualWitsWiles:
               if (0 == gi.WitAndWile) // Die Roll for random wits and wiles handled here
               {
                  if (1 == dieRoll)
                     gi.WitAndWile = 2; // cannot be less than two
                  else
                     gi.WitAndWile = dieRoll;
               }
               else
               {
                  gi.WitAndWile += dieRoll; // manual changes to wits and wiles handled here
                  if (gi.WitAndWile < 2)
                     gi.WitAndWile = 2;
                  else if (6 < gi.WitAndWile)
                     gi.WitAndWile = 6;
               }
               gi.EventDisplayed = gi.EventActive = "e000c"; // next screen to show
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.SetupStartingLocation:
               gi.EventDisplayed = gi.EventActive = "e001"; // next screen to show
               gi.DieRollAction = GameAction.SetupFinalize;
               break;
            case GameAction.SetupFinalize:
               if (false == SetStartingLocation(ref gi, dieRoll))
               {
                  returnStatus = "SetStartingLocation() returned error";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               gi.GamePhase = GamePhase.SunriseChoice;      // GameStateSetup.PerformAction()
               gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            default:
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateSetup.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
            case 1: starting = gi.Territories.Find("0101"); break;
            case 2: starting = gi.Territories.Find("0701"); break;
            case 3: starting = gi.Territories.Find("0801"); break;
            case 4: starting = gi.Territories.Find("1301"); break;
            case 5: starting = gi.Territories.Find("1501"); break;
            case 6: starting = gi.Territories.Find("1801"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "SetStartingLocation() reached default dr=" + dieRoll.ToString()); return false;
         }
         // <cgs> TEST
         //starting = gi.Territories.Find("0711"); //Town=0109 Ruins=0206 Temple=0711 Castle=1212 Castle=0323 Castle=1923 Cache=0505   
         //starting = gi.Territories.Find("0409"); //Farmland=0418 CountrySide=0410 Forest=0409 Hills=0406 Mountains=0405 Swamp=0411 Desert=0407 
         //starting = gi.Territories.Find("0411"); //ForestTemple=1021 HillsTemple=2009 MountainTemple=1021 
         //starting = gi.Territories.Find("0207"); //Road Travel=0207->0208
         starting = gi.Territories.Find("0707"); //Cross River=0707->0708
         if (null == starting)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartingLocation() starting territory=null");
            return false;
         }
         gi.EnteredTerritories.Add(starting);
         int counterCount = 0;
         foreach (IMapItem mi in gi.PartyMembers)
         {
            mi.SetLocation(counterCount);
            mi.TerritoryStarting = starting;
            mi.Territory = starting;
            mi.IsHidden = false;
            ++counterCount;
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
         gi.WitAndWile = Utilities.RandomGenerator.Next(6);
         ++gi.WitAndWile;
         if (1 == gi.WitAndWile) // cannot start with one 
            ++gi.WitAndWile;
         dr = Utilities.RandomGenerator.Next(6);
         ++dr;
         if (false == SetStartingLocation(ref gi, dr))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetup():  SetStartingLocation() return false");
            return false;
         }
         gi.GamePhase = GamePhase.SunriseChoice;      // PerformAutoSetup()
         gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
         gi.DieRollAction = GameAction.DieRollActionNone;
         return true;
      }
   }
   //-----------------------------------------------------
   class GameStateSunriseChoice : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         if(false == gi.IsNewDayChoiceMade)
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
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (action)
         {
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
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
                  if (null != mi.Rider) // mi = griffon
                  {
                     mi.Rider.Mounts.Remove(mi);  // Griffon Rider removes griffon as mount
                     mi.Rider = null;             // Griffon removes its rider
                  }
                  if ((true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Griffon")))
                     continue;
                  mi.IsRiding = false;
                  mi.IsFlying = false;
               }
               action = GameAction.UpdateEventViewerDisplay;
               break;
            case GameAction.RestEncounterCheck:
               ResetDayForNonTravelChoice(gi);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = gi.GamePhase = GamePhase.Rest;
               if (true == gi.IsInStructure(princeTerritory))
               {
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     mi.HealWounds(1, 0); // RestEncounterCheck - InStructure()=true
                     mi.IsExhausted = false;
                  }
                  if (false == SetHuntState(gi, ref action)) // Resting in same hex
                  {
                     returnStatus = "SetHuntState() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateRest.PerformAction(): " + returnStatus);
                  }
               }
               else // might have a travel encounter
               {
                  action = GameAction.TravelLostCheck;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateRest.PerformAction(RestEncounterCheck): gi.MapItemMoves.Clear()");
                  gi.MapItemMoves.Clear();
                  MapItemMove mim = new MapItemMove(gi.Territories, gi.Prince, princeTerritory);   // Travel to same hex if rest encounter
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
               TravelAction(gi, ref action); // GameAction.Travel
               break;
            case GameAction.TravelAir:
               gi.IsAirborne = true;
               gi.IsAirborneEnd = false;
               gi.IsShortHop = false;
               if ( true == gi.PartyReadyToFly()) // mount to fly returns false if anybody is left or possessions are left
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
               if (false == gi.PartyReadyToFly()) // mount to fly returns false if anybody is left or possessions are left
               {
                  gi.IsAirborne = false;
                  returnStatus = "gi.PartyReadyToFly() returned false for " + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateRest.PerformAction(): " + returnStatus);
               }
               else
               {
                  TravelAction(gi, ref action); // GameAction.TransportRedistributeEnd
               }
               break;
            case GameAction.SeekNews:
               ResetDayForNonTravelChoice(gi);
               gi.NumMembersBeingFollowed = 0;
               gi.EventDisplayed = gi.EventActive = "e209"; // next screen to show
               gi.SunriseChoice = gi.GamePhase = GamePhase.SeekNews;
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.SeekHire:
               ResetDayForNonTravelChoice(gi);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.SeekHire;
               gi.EventDisplayed = gi.EventActive = "e210"; // next screen to show
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case GameAction.SeekAudience:
               ResetDayForNonTravelChoice(gi);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.SeekAudience;
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
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211c";
                  else if ("0323" == princeTerritory.Name)
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211d";
                  else if ("1923" == princeTerritory.Name)
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211e";
                  else if (true == gi.DwarvenMines.Contains(princeTerritory))
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211f";
                  else
                     gi.EventDisplayed = gi.EventActive = "e211b"; // treat audience in other castles as temple audience
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
               ResetDayForNonTravelChoice(gi);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.SeekOffering;
               gi.ReduceCoins(1); // must spend one gold to make offering
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e212";
               if (0 < gi.ChagaDrugCount)
                  gi.EventDisplayed = gi.EventActive = "e143a";
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case GameAction.SearchRuins:
               gi.RemoveKilledInParty("e134");
               ResetDayForNonTravelChoice(gi);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.SearchRuins;
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
               ResetDayForNonTravelChoice(gi);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = gi.GamePhase = GamePhase.SearchCache;
               action = GameAction.TravelLostCheck;
               gi.DieRollAction = GameAction.DieRollActionNone;
               Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateSunriseChoice.PerformAction(SearchCacheCheck): gi.MapItemMoves.Clear()");
               gi.MapItemMoves.Clear();
               MapItemMove mimSearch = new MapItemMove(gi.Territories, gi.Prince, princeTerritory);   // Travel to same hex if rest encounter
               if ((0 == mimSearch.BestPath.Territories.Count) || (null == mimSearch.NewTerritory))
               {
                  returnStatus = "Unable to Find Path";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               }
               gi.MapItemMoves.Add(mimSearch);
               Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateSunriseChoice.PerformAction(SearchCacheCheck): oT=" + princeTerritory.Name + " nT=" + mimSearch.NewTerritory.Name);
               break;
            case GameAction.SearchTreasure:
               ResetDayForNonTravelChoice(gi);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = gi.GamePhase = GamePhase.SearchTreasure;
               action = GameAction.TravelLostCheck;
               gi.DieRollAction = GameAction.DieRollActionNone;
               Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateSunriseChoice.PerformAction(SearchTreasure): gi.MapItemMoves.Clear()");
               gi.MapItemMoves.Clear();
               MapItemMove mimSearch1 = new MapItemMove(gi.Territories, gi.Prince, princeTerritory);   // Travel to same hex if rest encounter
               if ((0 == mimSearch1.BestPath.Territories.Count) || (null == mimSearch1.NewTerritory))
               {
                  returnStatus = "Unable to Find Path";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               }
               gi.MapItemMoves.Add(mimSearch1);
               Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateSunriseChoice.PerformAction(SearchCacheCheck): oT=" + princeTerritory.Name + " nT=" + mimSearch1.NewTerritory.Name);
               break;
            case GameAction.ArchTravel:
               ResetDayForNonTravelChoice(gi);
               gi.NumMembersBeingFollowed = 0;
               gi.SunriseChoice = GamePhase.Travel;
               gi.GamePhase = GamePhase.Encounter;
               gi.EventDisplayed = gi.EventActive = "e045b";
               gi.DieRollAction = GameAction.EncounterRoll;
               action = GameAction.UpdateEventViewerActive;
               break;
            case GameAction.EncounterFollow:
               gi.IsBadGoing = false;                 // e078
               gi.IsHeavyRain = false;                // e079
               gi.IsFloodContinue = false;            // e092
               gi.IsEagleHunt = false;                // e114
               gi.RaftState = RaftEnum.RE_NO_RAFT;    // e122
               gi.IsCavalryEscort = false;            // e151
               if (false == EncounterFollow(gi, ref action))
               {
                  returnStatus = "EncounterFollow() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSunriseChoice.PerformAction(): " + returnStatus);
               }
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e072d"; // follow elven band
               gi.DieRollAction = GameAction.EncounterRoll;
               //-------------------------------------------
               IOption isEasyMonstersOption = gi.Options.Find("EasyMonsters");
               if (null == isEasyMonstersOption)
               {
                  returnStatus = "Reached Default ERROR with a=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               gi.EncounteredMembers.Clear();
               for (int i = 0; i < gi.NumMembersBeingFollowed; ++i) // if following elves, repopulate EncounterMembers container
               {
                  string elfName = "Elf" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", gi.Prince.Territory, 4, 5, 7);
                  if (true == isEasyMonstersOption.IsEnabled)
                     elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", gi.Prince.Territory, 1, 1, 7);
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
         sb12.Append("\t\tm="); sb12.Append(gi.Prince.Movement.ToString());
         sb12.Append("\t\tmu="); sb12.Append(gi.Prince.MovementUsed.ToString());
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
         gi.ActiveHex = null;
         gi.ActiveMember = null;               // TreausreTable lookup values
         gi.CapturedWealthCodes.Clear();
         gi.PegasusTreasure = PegasusTreasureEnum.Mount;
         gi.Bribe = 0;
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
         gi.IsTalkActive = true;                // e072
         gi.IsWolvesAttack = false;             // e075
         gi.IsHeavyRainNextDay = false;         // e079
         gi.EventAfterRedistribute = "";        // e086
         gi.IsMountsAtRisk = false;             // e095
         gi.PurchasedPotionCure = 0;            // e128b
         gi.PurchasedPotionHeal = 0;            // e128e
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
      protected void ResetDayForNonTravelChoice(IGameInstance gi)
      {
         gi.NumMembersBeingFollowed = 0;
         gi.IsAirborne = false;
         gi.IsWoundedWarriorRest = false;       // e069
         gi.IsTrainHorse = false;               // e077
         gi.IsBadGoing = false;                 // e078
         gi.IsFloodContinue = false;            // e092
         gi.AtRiskMounts.Clear();               // e095
         gi.IsEagleHunt = false;                // e114
         gi.IsWoundedBlackKnightRest = false;   // e123
         gi.IsCavalryEscort = false;            // e151
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
         if (true == PerformEndCheck(gi, ref action))
            return returnStatus;
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         switch (action)
         {
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateEventViewerDisplay:
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
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateHunt.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
         if (true == PerformEndCheck(gi, ref action))
            return returnStatus;
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateCampfire.PerformAction(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         switch (action)
         {
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.CampfirePlagueDustEnd:
               gi.RemoveKilledInParty("Plague Dust");
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
               if (false == EncounterEscape(gi, ref action))
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
               bool isPartySizeOne = gi.IsPartySizeOne();
               if ((true == gi.IsPartyDisgusted) && (false == isPartySizeOne)) // e010 - party is disgusted if ignore starving farmer
               {
                  action = GameAction.CampfireDisgustCheck;
               }
               else if (false == Wakeup(gi, ref action))
               {
                  returnStatus = "Wakeup() return false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): " + returnStatus);
               }
               gi.IsPartyDisgusted = false;
               break;
            default:
               returnStatus = "Reached Default ERROR for a=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateCampfire.PerformAction(): 567 - " + returnStatus);
               break;
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      protected bool PerformJailBreak(IGameInstance gi, ref GameAction action, int dieRoll)
      {
         switch (gi.EventActive)
         {
            case "e203a":
               if (1 == dieRoll)
               {
                  gi.IsJailed = false;
               }
               else
               {
                  dieRoll = 6;
                  if (("e061" == gi.EventStart) && (6 == dieRoll)) // only e061 does die roll = 6 cause death
                  {
                     gi.DieResults["e203a"][0] = 6;
                     gi.Prince.IsKilled = true;
                     gi.Prince.Wound = gi.Prince.Endurance;
                  }
                  else
                  {
                     if (false == SetEndOfDayState(gi, ref action)) // no hunting in prison so go straight to plague state
                     {
                        Logger.Log(LogEnum.LE_ERROR, "SetHuntState(): SetEndOfDayState() returned false");
                        return false;
                     }
                  }
               }
               break;
            case "e203c":
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
                     action = GameAction.EndGameLost;
                     gi.GamePhase = GamePhase.EndGame;
                     gi.EndGameReason = "Prince starves to dead as Wizard's slave";
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
                        action = GameAction.EndGameLost;
                        gi.GamePhase = GamePhase.EndGame;
                        gi.EndGameReason = "Prince beaten to death as Wizard's slave";
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
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (action)
         {
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.RestHealingEncounter:
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterStart;
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateRest.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
               gi.Prince.MovementUsed = gi.Prince.Movement; // End of the day
               if ( false == SetSubstitutionEvent(gi, princeTerritory))           // GameStateRest.PerformAction()      - RestHealingEncounter
               {
                  returnStatus = "SetSubstitutionEvent() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateRest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.RestHealing:
               foreach (IMapItem mi in gi.PartyMembers)
               {
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
         if (true == PerformEndCheck(gi, ref action))
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
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.TravelLostCheck:
               gi.IsGridActive = false; // GameAction.TravelLostCheck
               if (0 == gi.MapItemMoves.Count)
               {
                  returnStatus = "Invalid state: gi.MapItemMoves.Count for a=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
               }
               if (null == gi.MapItemMoves[0].NewTerritory)
               {
                  returnStatus = "Invalid state: gi.NewHex=null for a=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
               }
               else
               {
                  gi.NewHex = gi.MapItemMoves[0].NewTerritory;
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
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateTravel.PerformAction(): gi.MapItemMoves.Clear() a=TravelShowLost");
                  gi.MapItemMoves.Clear();
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
               Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateTravel.PerformAction():  a=TravelShowLostEncounter gi.MapItemMoves.Clear() a=TravelShowLostEncounter");
               gi.MapItemMoves.Clear();
               gi.IsGridActive = false;   // GameAction.TravelShowLostEncounter
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterStart;
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateTravel.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
               gi.Prince.MovementUsed = gi.Prince.Movement; // End of the day
               if (0 < gi.AtRiskMounts.Count)  // e095 - if traveling -- at risk mounts die. Need to redistribute load
               {
                  gi.AtRiskMounts.Clear();
                  action = GameAction.UpdateEventViewerActive;
                  gi.EventAfterRedistribute = gi.EventActive; // encounter this event after high pass check
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
               else if ( true == gi.IsHighPass ) // high pass event can happen even if no travel encounter
               {

                  gi.IsHighPass = false;
                  int tCount = gi.EnteredTerritories.Count;
                  if( tCount < 2 )
                  {
                     returnStatus = "Invalid state with tCount=" + tCount.ToString() + " for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTravel.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     ITerritory previousTerritory = gi.EnteredTerritories[tCount-2];
                     if (gi.NewHex.Name == previousTerritory.Name)
                     {
                        ShowMovementScreenViewer(gi); // show user instructions in the ScreenViewer
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e086a";
                        gi.GamePhase = GamePhase.Encounter;
                        gi.DieRollAction = GameAction.EncounterRoll;
                     }
                  }
               }
               else
               {
                  ShowMovementScreenViewer(gi);
               }
               if(gi.NewHex.Name != gi.EnteredTerritories.Last().Name )
                  gi.EnteredTerritories.Add(gi.NewHex); // NewHex set in the TravelShowLost action
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
               gi.EnteredTerritories.Add(gi.NewHex);
               break;
            case GameAction.TravelEndMovement: // Prince clicked when still movement left ends movement phase
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
            sb12.Append("<<<<ERROR2::::::GameStateTravel.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tes="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
         sb12.Append("\t\tm="); sb12.Append(gi.Prince.Movement.ToString());
         sb12.Append("\t\tmu="); sb12.Append(gi.Prince.MovementUsed.ToString());
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
            gi.RaftState = RaftEnum.RE_RAFT_ENDS_TODAY;
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
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.SeekNewsNoPay:
               gi.IsSeekNewModifier = false;
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e209a";
               gi.SunriseChoice = GamePhase.SeekNews;
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case GameAction.SeekNewsWithPay:
               gi.IsSeekNewModifier = true;
               gi.ReduceCoins(5);
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e209a";
               gi.SunriseChoice = GamePhase.SeekNews;
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tes="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.SeekHire:
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e210";
               gi.SunriseChoice = GamePhase.SeekHire;
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tes="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
         switch (action)
         {
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
         switch (action)
         {
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (action)
         {
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.SearchEncounter:
               gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterStart;
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
               Logger.Log(LogEnum.LE_NEXT_ACTION, ":GameStateSearch.PerformAction(): SearchCache action");
               gi.EventActive = gi.EventDisplayed = "e214";
               break;
            case GameAction.SearchTreasure:
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               if (true == gi.SecretClues.Contains(princeTerritory))
                  gi.EventDisplayed = gi.EventActive = "e147a";
               else if (true == gi.WizardAdviceLocations.Contains(princeTerritory))
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
         switch (action)
         {
            case GameAction.EndGameLost:
               break;
            case GameAction.EndGameWin:
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
         IUnitTest ut = gi.UnitTests[gi.GameTurn];
         switch (action)
         {
            case GameAction.RemoveSplashScreen: // do nothing...the unit test is updated in GameViewerWindow:updateView()
               break;
            case GameAction.UpdateGameViewer: // do nothing...the unit test is updated in GameViewerWindow:updateView()
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
            case GameAction.UnitTestStart: // do nothing...the unit test is updated in GameViewerWindow:updateView()
               break;
            case GameAction.UnitTestCommand: // call the unit test's Command() function
               if (false == ut.Command(ref gi))
               {
                  returnStatus = "Command() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UnitTestNext: // call the unit test's NextTest() function
               if (false == ut.NextTest(ref gi))
               {
                  returnStatus = "NextTest() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UnitTestCleanup: // Call the unit test's NextTest() function
               if (false == ut.Cleanup(ref gi))
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tes="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
         { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
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
      static private int theNumHydraTeeth = 0;
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         if (true == PerformEndCheck(gi, ref action))
            return returnStatus;
         IOption isEasyMonstersOption = gi.Options.Find("EasyMonsters");
         if (null == isEasyMonstersOption)
         {
            returnStatus = "isEasyMonstersOption=null";
            Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
         }
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (action)
         {
            case GameAction.UpdateEventViewerDisplay:
               break;
            case GameAction.UpdateEventViewerActive:
               gi.EventDisplayed = gi.EventActive;
               break;
            case GameAction.ShowDienstalBranch:
            case GameAction.ShowLargosRiver:
            case GameAction.ShowNesserRiver:
            case GameAction.ShowTrogothRiver:
               break;
            case GameAction.EncounterAbandon: // Remove all party members that are not riding
               if (false == EncounterAbandon(gi, ref action))
               {
                  returnStatus = "EncounterAbandon() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.EncounterBribe:
               gi.ReduceCoins(gi.Bribe);
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd(EncounterBribe) returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.EncounterCombat:
               break;
            case GameAction.EncounterEnd:
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(EncounterEnd): " + returnStatus);
               }
               break;
            case GameAction.EncounterEscape:
               gi.RemoveKilledInParty("Escape", true);
               if (false == EncounterEscape(gi, ref action))
               {
                  returnStatus = "EncounterEscape() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               action = GameAction.UpdateEventViewerActive;
               gi.EventDisplayed = gi.EventActive = "e218";
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.EncounterEscapeFly:
               if (false == EncounterEscape(gi, ref action))
               {
                  returnStatus = "EncounterEscape() returned false";
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
                  returnStatus = "EncounterEnd() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.EncounterEscapeMounted:
               if (false == EncounterEscape(gi, ref action))
               {
                  returnStatus = "EncounterEscape() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               action = GameAction.UpdateEventViewerActive;
               gi.EventDisplayed = gi.EventActive = "e312c";
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.EncounterFollow:
               if (false == EncounterFollow(gi, ref action))
               {
                  returnStatus = "EncounterFollow() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.EncounterHide:
               if (false == SetEndOfDayState(gi, ref action)) // Hiding so cannot hunt
               {
                  returnStatus = "SetEndOfDayState() returned false";
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
                  gi.IsMagicianProvideGift = true;
                  string magicianName = "Magician" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c26Magician", "c26Magician", princeTerritory, 5, 3, 0);
                  gi.EncounteredMembers.Add(magician);
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
                  returnStatus = "EncounterLootStart() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(EncounterLootStart): " + returnStatus);
               }
               break;
            case GameAction.EncounterLoot:
               break;
            case GameAction.EncounterLootStartEnd:
               if (false == EncounterLootStartEnd(gi, ref action))
               {
                  returnStatus = "EncounterLootStartEnd() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(EncounterLootStartEnd): " + returnStatus);
               }
               break;
            case GameAction.EncounterRoll:
               if (false == EncounterRoll(gi, ref action, dieRoll))
               {
                  returnStatus = "EncounterRoll() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.EncounterStart:
               if (false == EncounterStart(gi, ref action, dieRoll))
               {
                  returnStatus = "EncounterStart() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.EncounterSurrender:
               if ("e054a" == gi.EventActive)
               {
                  if (false == MoveToClosestGoblinKeep(gi))  // return back to keep 
                  {
                     returnStatus = "MoveToClosestGoblinKeep() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
               }
               if (false == MarkedForDeath(gi))
               {
                  returnStatus = "MarkedForDeath() returned false";
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
                  ITerritory tRamdom = FindClueHex(gi, directionLost, dieRoll);// Find a random hex at the range set by die roll
                  if (null == tRamdom)
                  {
                     returnStatus = "tRamdom=null for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.DwarfAdviceLocations.Add(tRamdom);
                     if (false == SetCampfireFinalConditionState(gi, ref action))
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
                     gi.AddCoins(1, false);
                     int food = 4;
                     if (true == gi.IsMerchantWithParty)
                        food = (int)Math.Ceiling((double)food * 2);
                     gi.ReduceFoods(food);
                     gi.PurchasedFood -= food;
                  }
                  else if ("e012a" == gi.EventActive)
                  {
                     gi.AddCoins(1, false);
                     int food = 2;
                     if (true == gi.IsMerchantWithParty)
                        food = (int)Math.Ceiling((double)food * 2);
                     gi.ReduceFoods(food);
                     gi.PurchasedFood -= food;
                  }
                  else if (("e015b" == gi.EventActive) || ("e128c" == gi.EventActive))
                  {
                     gi.AddCoins(1, false);
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
                     gi.ReduceCoins(1);
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
                     gi.ReduceCoins(1);
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
                     gi.ReduceCoins(1);
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
                  returnStatus = "EncounterEnd() returned false for E044HighAltarEnd";
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
                     gi.AddCoins(cost, false);
                  }
                  else if (("e129c" == gi.EventActive) || ("e210g" == gi.EventActive))
                  {
                     int cost = 7;
                     if (true == gi.IsMerchantWithParty)
                        cost = (int)Math.Ceiling((double)cost * 0.5);
                     gi.AddCoins(cost, false);
                  }
                  else if ("e210d" == gi.EventActive)
                  {
                     int cost = 10;
                     if (true == gi.IsMerchantWithParty)
                        cost = (int)Math.Ceiling((double)cost * 0.5);
                     gi.AddCoins(cost, false);
                  }
                  else
                  {
                     returnStatus = "1-Invalid event=" + gi.EventActive + " for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
               }
               else
               {
                  gi.AddNewMountToParty();
                  ++gi.PurchasedMount;
                  if (("e015b" == gi.EventActive) || ("e128f" == gi.EventActive))
                  {
                     int cost = 6;
                     if (true == gi.IsMerchantWithParty)
                        cost = (int)Math.Ceiling((double)cost * 0.5);
                     gi.ReduceCoins(cost);
                  }
                  else if (("e129c" == gi.EventActive) || ("e210g" == gi.EventActive))
                  {
                     int cost = 7;
                     if (true == gi.IsMerchantWithParty)
                        cost = (int)Math.Ceiling((double)cost * 0.5);
                     gi.ReduceCoins(cost);
                  }
                  else if ("e210d" == gi.EventActive)
                  {
                     int cost = 10;
                     if (true == gi.IsMerchantWithParty)
                        cost = (int)Math.Ceiling((double)cost * 0.5);
                     gi.ReduceCoins(cost);
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
               foreach (ITerritory t in gi.Territories) // all audiences with high priest is forbidden
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
               else
               {
                  int directionAdvice = gi.DieResults["e025"][0];
                  ITerritory tRamdom = FindClueHex(gi, directionAdvice, dieRoll);// Find a random hex at the range set by die roll
                  if (null == tRamdom)
                  {
                     returnStatus = "tRamdom=null for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.WizardAdviceLocations.Add(tRamdom);
                     if (false == SetCampfireFinalConditionState(gi, ref action))
                     {
                        returnStatus = "SetCampfireFinalConditionState() returned false for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
               }
               break;
            case GameAction.E024WizardFight:
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.EventDisplayed = gi.EventActive = "e330";
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
               if (false == Wakeup(gi, ref action))
               {
                  returnStatus = "Wakeup() returned false for ae=" + gi.EventActive;
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E035IdiotContinue:
               if (false == Wakeup(gi, ref action))
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
            case GameAction.E043SmallAltar:
               break;
            case GameAction.E043SmallAltarEnd:
               gi.RemoveKilledInParty("E043");
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for E044HighAltarEnd";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E044HighAltar:
               break;
            case GameAction.E044HighAltarEnd:
               gi.RemoveKilledInParty("E044");
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
               gi.DieResults["e045b"][0] = Utilities.NO_RESULT;
               Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateEncounter.PerformAction(E045ArchOfTravelEnd): gi.MapItemMoves.Clear()");
               gi.MapItemMoves.Clear();
               MapItemMove mimArchTravel = new MapItemMove(gi.Territories, gi.Prince, princeTerritory);   // Travel to same hex is no lost check
               if ((0 == mimArchTravel.BestPath.Territories.Count) || (null == mimArchTravel.NewTerritory))
                  returnStatus = "Unable to Find Path to mim=" + mimArchTravel.ToString();
               gi.MapItemMoves.Add(mimArchTravel);
               Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "GameStateEncounter.PerformAction(E045ArchOfTravelEnd): oT =" + princeTerritory.Name + " nT=" + mimArchTravel.NewTerritory.Name);
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
               gi.Prince.MovementUsed = gi.Prince.Movement;
               gi.NewHex = princeTerritory;
               gi.EnteredTerritories.Add(gi.NewHex);
               if ((true == gi.IsExhausted) && ((true == gi.NewHex.IsOasis) || ("Desert" != gi.NewHex.Type)) ) // e120
                  gi.IsExhausted = false;
               break;
            case GameAction.E045ArchOfTravelEndEncounter:
               action = GameAction.UpdateEventViewerDisplay;
               break;
            case GameAction.E045ArchOfTravelSkip:
               if (false == gi.Arches.Contains(princeTerritory))
                  gi.Arches.Add(princeTerritory);
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
                           case 1: gi.AddCoins(3, false); break;
                           case 2: gi.AddCoins(4, false); break;
                           case 3: gi.AddCoins(5, false); break;
                           case 4: gi.AddCoins(6, false); break;
                           case 5: gi.AddCoins(6, false); break;
                           case 6: gi.AddCoins(7, false); break;
                           default:
                              returnStatus = "EncounterRoll(): reached default randomWealthRoll=" + randomWealthRoll.ToString() + " for ae=" + gi.EventActive;
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
                              returnStatus = " AddGuideTerritories() returned false for ae=" + gi.EventActive;
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
               gi.AddCoins(gi.FickleCoin, false);  // do not need to pay coin if you play a song
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
                  gi.RaftState = RaftEnum.RE_NO_RAFT;
                  gi.Prince.ResetPartial();
                  if (false == gi.RemoveBelongingsInParty())
                  {
                     returnStatus = " RemoveBelongingsInParty() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
               }
               else
               {
                  gi.ReduceCoins(10);
                  if( 1 == gi.PartyMembers.Count ) // if only yourself, encounter ends with you let out of jail paying your fine
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
               string woundedWarriorName = "Warrior" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem warriorWounded = new MapItem(woundedWarriorName, 1.0, false, false, false, "c79Warrior", "c79Warrior", princeTerritory, 6, 7, 0);
               warriorWounded.SetWounds(5, 0);
               warriorWounded.IsAlly = true;
               gi.PartyMembers.Add(warriorWounded);
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false a=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E069WoundedWarriorRemain:
               string woundedWarriorName1 = "Warrior" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem warriorWounded1 = new MapItem(woundedWarriorName1, 1.0, false, false, false, "c79Warrior", "c79Warrior", princeTerritory, 6, 7, 0);
               warriorWounded1.SetWounds(5, 0);
               warriorWounded1.IsAlly = true;
               gi.PartyMembers.Add(warriorWounded1);
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
            case GameAction.E073WitchMeet: break;
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
                  action = GameAction.EndGameLost;
                  gi.GamePhase = GamePhase.EndGame;
                  gi.EndGameReason = "Prince is a Frog";
               }
               break;
            case GameAction. E075WolvesEncounter:
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
               string huntingCatName = "Cat" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem huntingCat = new MapItem(huntingCatName, 1.0, false, false, false, "c59HuntingCat", "c59HuntingCat", princeTerritory, 3, 6, 0);
               if (true == isEasyMonstersOption.IsEnabled)
                  huntingCat = new MapItem(huntingCatName, 1.0, false, false, false, "c59HuntingCat", "c59HuntingCat", princeTerritory, 1, 1, 0);
               gi.EncounteredMembers.Add(huntingCat);
               break;
            case GameAction.E077HerdCapture: // herd of wild horses
               foreach (IMapItem e077Mi in gi.PartyMembers)
                  e077Mi.AddNewMount();
               if (false == gi.IsMagicInParty())
               {
                  gi.IsTrainHorse = true;
                  if (GamePhase.Travel == gi.SunriseChoice)
                     gi.SunriseChoice = GamePhase.Encounter;  // must spend next day training horses
               }
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd(E077HerdCapture) returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E078BadGoingHalt: // bad going
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
                  int directionPixie = gi.DieResults["e025b"][0];
                  ITerritory tRamdom = FindClueHex(gi, directionPixie, dieRoll);// Find a random hex at the range set by die roll
                  if (null == tRamdom)
                  {
                     returnStatus = "tRamdom=null for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.WizardAdviceLocations.Add(tRamdom);
                     if( false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
               }
               break;
            case GameAction.E082SpectreMagic:
               gi.EventStart = "e082";
               break;
            case GameAction.E085Falling:
               break;
            case GameAction.E086HighPass:
               gi.IsHighPass = true;
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.TransportRedistributeEnd:
               if( "e079b" == gi.EventAfterRedistribute ) // Need to set movement based on current conditions after Heavy Rains continues into next day
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
               }
               else if( "" == gi.EventAfterRedistribute )
               {
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
               break;
            case GameAction.E088FallingRocks:
               break;
            case GameAction.E082SpectreMagicEnd:
               gi.RemoveKilledInParty("Spectre takes person", true);
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E083WildBoar:
               gi.EventStart = "e083";
               gi.EventDisplayed = gi.EventActive = "e310";
               string boarName = "Boar" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem boar = new MapItem(boarName, 1.0, false, false, false, "c58Boar", "c58Boar", princeTerritory, 5, 8, 0);
               if (true == isEasyMonstersOption.IsEnabled)
                  boar = new MapItem(boarName, 1.0, false, false, false, "c58Boar", "c58Boar", princeTerritory, 1, 1, 0);
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
               gi.RemoveKilledInParty("Snake takes person", true);
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
               //SetEncounterOptions(gi, false, false, false, false, true); // no lost event and no event -- (gi, isNoLost, isForceLost, isForceLostEvent, isNoEvent, isForceEvent)
               break;
            case GameAction.E096MountsDie:
               gi.IsMountsSick = true;
               foreach(IMapItem mi in gi.PartyMembers)
               {
                  foreach( IMapItem mount in mi.Mounts )
                  {
                     if (true == mount.Name.Contains("Griffon"))
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
               gi.RemoveKilledInParty("Marsh Flesh Rot");
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
               if(2 < gi.Prince.MovementUsed) // if traveled to third hex in air, need to move back to previous hex
               {
                  gi.IsAirborne = false;
                  int tCount = gi.EnteredTerritories.Count;
                  if (tCount < 2)
                  {
                     returnStatus = " Invalid state with tCount=" + tCount.ToString() + " a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else // move back to previous hex
                  {
                     ITerritory previousTerritory = gi.EnteredTerritories[tCount - 2]; // If do not return from hex entered, must do a High Pass Check for deaths
                     --gi.Prince.MovementUsed;
                     gi.Prince.TerritoryStarting = gi.Prince.Territory;
                     if (false == AddMapItemMove(gi, previousTerritory))
                     {
                        returnStatus = " AddMapItemMove() return false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     ++gi.Prince.MovementUsed;
                  }
               }
               else
               {
                  --gi.Prince.Movement; // reduce hexes by one
                  if(gi.Prince.Movement <= gi.Prince.MovementUsed)
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
               int woundsvw = gi.DieResults["e105a"][2];
               if ((woundsvw < 1) || (6 < woundsvw))
               {
                  returnStatus = " invalid direction=" + woundsvw.ToString() + " for  action =" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               //--------------------------------------------------------
               gi.Prince.SetWounds(woundsvw, 0);  // prince is wounded one die
               if( true == gi.Prince.IsKilled )
               {
                  action = GameAction.EndGameLost;
                  gi.GamePhase = GamePhase.EndGame;
               }
               else
               {
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
                  rangevw = (int)Math.Ceiling((double)rangevw*0.5); // half rounded up
                  //--------------------------------------------------------
                  IMapItems lostInStormMembers = new MapItems();
                  foreach (IMapItem mi in gi.PartyMembers)
                     lostInStormMembers.Add(mi);
                  foreach (IMapItem mi in lostInStormMembers)
                     gi.RemoveAbandonedInParty(mi);
                  //--------------------------------------------------------
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateEncounter.PerformAction(): gi.MapItemMoves.Clear() for a=" + action.ToString());
                  gi.MapItemMoves.Clear();
                  //--------------------------------------------------------
                  ITerritory blowToTerritory = FindClueHex(gi, directionvw, rangevw);// Find a random hex at random direction and range 1
                  if (null == blowToTerritory)
                  {
                     returnStatus = " blowToTerritory=null action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  //--------------------------------------------------------
                  gi.Prince.MovementUsed = 0; // must have movement left to be blown off course
                  gi.Prince.TerritoryStarting = gi.NewHex;
                  if (false == AddMapItemMove(gi, blowToTerritory))
                  {
                     returnStatus = " AddMapItemMove() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  //--------------------------------------------------------
                  gi.IsAirborne = false;
                  Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
                  gi.Prince.MovementUsed = gi.Prince.Movement; // end movement after being blown off course
                  if (false == EncounterEnd(gi, ref action))
                  {
                     returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  gi.DieResults["e105a"][0] = Utilities.NO_RESULT;
                  gi.DieResults["e105a"][1] = Utilities.NO_RESULT;
                  gi.DieResults["e105a"][2] = Utilities.NO_RESULT;
               }
               break;
            case GameAction.E106OvercastLost:
                  int direction = gi.DieResults["e106"][0];
                  ITerritory adjacentTerritory = FindClueHex(gi, direction, 1);// Find a random hex at random direction and range 1
                  if (null == adjacentTerritory)
                  {
                     returnStatus = " adjacentTerritory=null action=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  gi.Prince.TerritoryStarting = gi.NewHex;
                  if (false == AddMapItemMove(gi, adjacentTerritory))
                  {
                     returnStatus = " AddMapItemMove() return false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
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
               string falconName = "Falcon" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem falcon = new MapItem(falconName, 1.0, false, false, false, "c82Falcon", "c82Falcon", princeTerritory, 0, 0, 0);
               falcon.IsRiding = true;
               falcon.IsFlying = true;
               falcon.IsGuide = true;
               falcon.GuideTerritories = gi.Territories;
               gi.PartyMembers.Add(falcon);
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
            case GameAction.E120Exhausted:
               gi.IsExhausted = true;
               foreach (IMapItem mi in gi.PartyMembers)
               {
                  if (null != mi.Rider)
                  {
                     mi.Rider.Mounts.Remove(mi);  // Griffon removes its rider
                     mi.Rider = null;
                  }
               }
               foreach (IMapItem mi in gi.PartyMembers)
               {
                  mi.IsExhausted = true;
                  mi.SetWounds(1, 0); // each party member suffers one wound
                  foreach(IMapItem mount in mi.Mounts)
                     mount.IsExhausted = true;
                  if ((false == mi.Name.Contains("Griffon")) && (false == mi.Name.Contains("Eagle")))
                     mi.IsRiding = false;
                  mi.IsFlying = false;
               }
               gi.RemoveKilledInParty("E120 Exhausted");
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E121SunStroke:
            case GameAction.E121SunStrokeEnd:
               break;
            case GameAction.E122RaftingEndsForDay:
               action = GameAction.TravelLostCheck;
               gi.GamePhase = GamePhase.Travel;
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.RaftState = RaftEnum.RE_RAFT_ENDS_TODAY;
               break;
            case GameAction.E123WoundedBlackKnightRemain:
               string woundedBlackKnightName1 = "Warrior" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem blackKnight = new MapItem(woundedBlackKnightName1, 1.0, false, false, false, "c80BlackKnight", "c80BlackKnight", princeTerritory, 8, 8, 0);
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
               gi.ReduceCoins(1);
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
               gi.RaftState = RaftEnum.RE_NO_RAFT;
               Logger.Log(LogEnum.LE_MOVE_COUNT, "GameStateEncounter.PerformAction(): MovementUsed=Movement for a=" + action.ToString());
               gi.Prince.MovementUsed = gi.Prince.Movement; // stop movement
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E126RaftInCurrentEnd:
               Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateEncounter.PerformAction(): gi.MapItemMoves.Clear() for a=" + action.ToString());
               gi.MapItemMoves.Clear();
               ITerritory downRiverT1 = gi.Territories.Find(gi.Prince.Territory.DownRiver);
               if( null == downRiverT1)
               {
                  returnStatus = " downRiverT1=null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               gi.Prince.TerritoryStarting = gi.Prince.Territory;
               if (false == AddMapItemMove(gi, downRiverT1))
               {
                  returnStatus = " AddMapItemMove() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               gi.NewHex = downRiverT1;
               gi.EnteredTerritories.Add(gi.NewHex);
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E126RaftInCurrentRedistribute:
               Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateEncounter.PerformAction(): gi.MapItemMoves.Clear() for a=" + action.ToString());
               gi.MapItemMoves.Clear();

               ITerritory downRiverT = gi.Territories.Find(gi.Prince.Territory.DownRiver);
               if (null == downRiverT)
               {
                  returnStatus = " downRiverT1=null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               gi.Prince.TerritoryStarting = gi.Prince.Territory;
               if (false == AddMapItemMove(gi, downRiverT))
               {
                  returnStatus = " AddMapItemMove() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               gi.NewHex = downRiverT;
               gi.EnteredTerritories.Add(gi.NewHex);
               break;
            case GameAction.E128aBuyPegasus:
               gi.AddNewMountToParty(MountEnum.Pegasus);
               int pegasusCost = 50;
               if (true == gi.IsMerchantWithParty)
                  pegasusCost = (int)Math.Ceiling((double)pegasusCost * 0.5);
               gi.ReduceCoins(pegasusCost);
               gi.EventDisplayed = gi.EventActive = "e188";
               break;
            case GameAction.E128bPotionCureChange:
               int costCurePotion = 10;
               if (true == gi.IsMerchantWithParty)
                  costCurePotion = (int)Math.Ceiling((double)costCurePotion * 0.5);
               if (dieRoll < 0)
               {
                  gi.AddCoins(costCurePotion, false);
                  if (false == gi.RemoveSpecialItem(SpecialEnum.CurePoisonVial))
                  {
                     returnStatus = "RemoveSpecialItem() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  --gi.PurchasedPotionCure;
               }
               else
               {
                  gi.ReduceCoins(costCurePotion);
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
                  gi.AddCoins(costHealingPotion, false);
                  if (false == gi.RemoveSpecialItem(SpecialEnum.HealingPoition))
                  {
                     returnStatus = "RemoveSpecialItem() returned false a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  --gi.PurchasedPotionHeal;
               }
               else
               {
                  gi.ReduceCoins(costHealingPotion);
                  gi.AddSpecialItem(SpecialEnum.HealingPoition);
                  ++gi.PurchasedPotionHeal;
               }
               break;
            case GameAction.E129aBuyAmulet:
               int costAmulet = 25;
               if (true == gi.IsMerchantWithParty)
                  costAmulet = (int)Math.Ceiling((double)costAmulet * 0.5);
               gi.AddSpecialItem(SpecialEnum.AntiPoisonAmulet);
               gi.ReduceCoins(costAmulet);
               gi.EventDisplayed = gi.EventActive = "e187";
               break;
            case GameAction.E130JailedOnTravels: 
               switch (gi.DieResults["e130"][1])
               {
                  case 1: gi.NewHex = gi.Territories.Find("1212"); break;
                  case 2: gi.NewHex = gi.Territories.Find("0323"); break;
                  case 3: gi.NewHex = gi.Territories.Find("1923"); break;
                  case 4: gi.NewHex = FindClosestTemple(gi); break;
                  case 5: case 6: gi.NewHex = FindClosestTown(gi); break;
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
                  ++gi.Days;  // advance the day by one day
                  gi.EnteredTerritories.Add(gi.NewHex);
                  if ((true == gi.IsExhausted) && ((true == gi.NewHex.IsOasis) || ("Desert" != gi.NewHex.Type))) // e120
                     gi.IsExhausted = false;
               }
               gi.EventDisplayed = gi.EventActive = "e060"; 
               gi.DieRollAction = GameAction.EncounterRoll; 
               break;
            case GameAction.E130BribeGuard:
               int costBribeGard = 10;
               if (true == gi.IsMerchantWithParty)
                  costBribeGard = (int)Math.Ceiling((double)costBribeGard * 0.5);
               gi.ReduceCoins(costBribeGard);
               gi.EventDisplayed = gi.EventActive = "e130e";
               break;
            case GameAction.E130RobGuard:
               if (false == gi.AddCoins(100))
               {
                  returnStatus = "AddCoins() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               ITerritory t130 = null;
               switch (gi.DieResults["e130"][1]) // castle is forbidden after robbing lord
               {
                  case 1: t130 = gi.Territories.Find("1212"); break;
                  case 2: t130 = gi.Territories.Find("0323"); break;
                  case 3: t130 = gi.Territories.Find("1923"); break;
                  default: break; // do nothing
               }
               if( null != t130 )
               {
                  if (false == gi.ForbiddenHexes.Contains(t130)) // cannot return to this hex
                     gi.ForbiddenHexes.Add(t130);
               }
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E133PlaguePrince:
               ++gi.Prince.StarveDayNum;
               gi.Prince.IsRiding = false;
               gi.Prince.IsFlying = false;
               gi.Prince.IsPlagued = false;
               gi.RemoveKilledInParty("E133");
               //-----------------------------
               int partyCount = gi.PartyMembers.Count;
               int countOfPersons = gi.RemoveLeaderlessInParty();
               if (0 < countOfPersons)  // If there are any surviving party members, they run away and take all mounts and treasures
               {
                  gi.RaftState = RaftEnum.RE_NO_RAFT;
                  gi.Prince.ResetPartial();
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
               //-----------------------------
               if (false == Wakeup(gi, ref action))
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
               gi.RemoveKilledInParty("E133");
               action = GameAction.EncounterEscape;
               if (false == EncounterEscape(gi, ref action))
               {
                  returnStatus = "EncounterEscape() returned false for a=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E134ShakyWalls:
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.E136FallingCoins:
               if (false == gi.AddCoins(500))
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
            case GameAction.E147ClueToTreasure:
               if (Utilities.NO_RESULT == gi.DieResults["e147"][0])
               {
                  gi.DieResults["e147"][0] = dieRoll;
               }
               else
               {
                  int directionClue = gi.DieResults["e147"][0];
                  ITerritory tRamdom = FindClueHex(gi, directionClue, dieRoll);// Find a random hex at the range set by die roll
                  if (null == tRamdom)
                  {
                     returnStatus = "tRamdom=null for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.SecretClues.Add(tRamdom);
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                  }
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
               gi.ReduceCoins(gi.Bribe);
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
               action = GameAction.EndGameWin;
               gi.GamePhase = GamePhase.EndGame;
               gi.EndGameReason = "Noble Ally marches on Northlands!";
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
               gi.ReduceCoins(10);
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
            case GameAction.E156MayorAudience:
               switch (gi.DieResults["e156"][0]) // Based on the die roll, implement event 
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; break;          // insulted
                  case 2:                                                                                                           // stone faced
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     break;                                                                                                         // free food and lodging
                  case 3:
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
                     break;
                  case 4:                                                                                                          // letter of recommendation nearest castle
                     gi.EventDisplayed = gi.EventActive = "e157";
                     ITerritory closetCastle0 = FindClosestCastle(gi);
                     if (null == closetCastle0)
                     {
                        returnStatus = "FindClosestCastle() returned null ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     gi.ActiveHex = closetCastle0;
                     gi.LetterOfRecommendations.Add(closetCastle0);
                     gi.ForbiddenAudiences.RemoveLetterGivenConstraints(closetCastle0); // if a letter is given for a Drogat Castle, remove the constraint to have audience
                     ITerritory t156 = FindClosestTown(gi);
                     gi.ForbiddenAudiences.AddTimeConstraint(t156, gi.Days + 6);
                     break;
                  case 5:                                                                                                          // letter of recommendation nearest castle
                     gi.EventDisplayed = gi.EventActive = "e157";
                     if (false == gi.AddCoins(50))
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
                     gi.ActiveHex = closetCastle1;
                     gi.LetterOfRecommendations.Add(closetCastle1);
                     gi.ForbiddenAudiences.RemoveLetterGivenConstraints(closetCastle1); // if a letter is given for a Drogat Castle, remove the constraint to have audience
                     ITerritory t156b = FindClosestTown(gi);
                     gi.ForbiddenAudiences.AddLetterConstraint(t156b, closetCastle1);
                     break;
                  case 6:                                                                                                          // letter of recommendation nearest castle
                     if (true == gi.IsReligionInParty())
                     {
                        gi.EventDisplayed = gi.EventActive = "e157";
                        if (false == gi.AddCoins(100))
                        {
                           returnStatus = "AddCoins() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                     }
                     else
                     {
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                     }
                     break;
                  default:
                     returnStatus = "Reach Default ae=" + action.ToString() + " dr=" + gi.DieResults["e156"][0].ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     break;
               }
               break;
            case GameAction.E155HighPriestAudience:
               ITerritory t155 = FindClosestTemple(gi);
               switch (gi.DieResults["e155"][0]) // Based on the die roll, implement event
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; break;  // arrested
                  case 2:                                                                                                   // stone faced
                     if( true == gi.IsInTemple(t155) )
                        gi.ForbiddenAudiences.AddTimeConstraint(t155, gi.Days + 6);
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e155"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     break;
                  case 3:                                                                                                  // hears pleas
                     if ((gi.DayOfLastOffering - gi.Days < 4) && (gi.Days <= gi.DayOfLastOffering))
                     {
                        gi.EventStart = gi.EventDisplayed = gi.EventActive = "e211b";
                        gi.DieRollAction = GameAction.EncounterRoll;
                        gi.DieResults["e211b"][0] = Utilities.NO_RESULT;
                     }
                     else
                     {
                        if (true == gi.IsInTemple(t155))
                           gi.ForbiddenAudiences.AddOfferingConstaint(t155, Utilities.FOREVER);
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e155"][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                     }
                     break;
                  case 4:
                     if (true == gi.IsInTemple(t155))
                        gi.ForbiddenAudiences.AddOfferingConstaint(t155, gi.Days + 1);
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e155"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
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
                     if (false == gi.AddCoins(200))
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
            case GameAction.E158HostileGuardPay:
               gi.ReduceCoins(20);
               if (false == ResetDieResultsForAudience(gi))
               {
                  returnStatus = "ResetDieResultsForAudience() returned false ae=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               action = GameAction.UpdateEventViewerActive;
               break;
            case GameAction.E160GBrokenLove:
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
               gi.Days += gi.DieResults["e160e"][0];
               if (false == PerformEndCheck(gi, ref action))
               {
                  gi.DieResults["e160"][0] = Utilities.NO_RESULT;
                  gi.DieResults["e160e"][0] = Utilities.NO_RESULT;
                  gi.EventDisplayed = gi.EventActive = "e160";
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               break;
            case GameAction.E160LadyAudience:  // e160
               ITerritory t160 = gi.Territories.Find("1923");
               switch (gi.DieResults["e160"][0]) // Based on the die roll, implement event
               {
                  case 1:                                                                                                        // not interested
                     gi.ForbiddenAudiences.AddTimeConstraint(t160, Utilities.FOREVER);
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e160"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     break;
                  case 2:                                                                                                        // distracted listening
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e160"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
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
                     returnStatus = "Reach Default ae=" + action.ToString() + " dr=" + gi.DieResults["e160"][0].ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     break;
               }
               break;
            case GameAction.E161CountAudience: 
               ITerritory t161 = gi.Territories.Find("0323");
               switch (gi.DieResults["e161"][0]) // Based on the die roll, implement event
               {
                  case 1:                                                                                                           // count victim
                     if (false == MarkedForDeath(gi))
                     {
                        returnStatus = "ResetDieResultsForAudience() returned false ae=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     break;
                  case 2:                                                                                                           // half listens
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e161"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     break;
                  case 3:                                                                                                         // flippant advice
                     gi.ForbiddenAudiences.AddLetterGivenConstraint(t161); // must have letter given for this territory to hold audience  
                     gi.IsMustLeaveHex = true;
                     if (false == EncounterEnd(gi, ref action))
                     {
                        returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e161"][0].ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                     }
                     break;
                  case 4:                                                                                                          // interested
                     if (false == gi.AddCoins(100))
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
                        if (false == gi.AddCoins(500))
                        {
                           returnStatus = "AddCoins() returned false for action=" + action.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                        gi.CapturedWealthCodes.Add(110);
                        gi.AddNewMountToParty(MountEnum.Pegasus);
                        gi.AddNewMountToParty(MountEnum.Pegasus);
                        gi.ForbiddenAudiences.AddTimeConstraint(t161, Utilities.FOREVER);
                        action = GameAction.EncounterLootStart;
                     }
                     else
                     {
                        gi.ForbiddenAudiences.AddMonsterKillConstraint(t161);
                        if (false == EncounterEnd(gi, ref action))
                        {
                           returnStatus = "EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e161"][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
                        }
                     }
                     break;
                  case 6: gi.IsNobleAlly = true; break;
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
                  gi.AddCoins(porterCost, false);
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
                  gi.ReduceCoins(porterCost);
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
                  gi.AddCoins(girlCost, false);
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
                  gi.ReduceCoins(girlCost);
                  ++gi.PurchasedSlaveGirl;
               }
               break;
            case GameAction.E163SlaveWarriorChange:
               int warriorCost = gi.DieResults["e163"][2];
               if (true == gi.IsMerchantWithParty)
                  warriorCost = (int)Math.Ceiling((double)warriorCost * 0.5);
               if ((0 == gi.PurchasedSlavePorter) && (0 == gi.PurchasedSlaveGirl)) // when buying warrior, if no porter or slave girl is purchased, the warrior cost 2gp extra
               {
                  if( true == gi.IsMerchantWithParty)
                     warriorCost += 1;
                  else
                     warriorCost += 2;
               }
               if (dieRoll < 0)
               {
                  gi.AddCoins(warriorCost, false);
                  --gi.PurchasedSlaveWarrior;
               }
               else
               {
                  gi.ReduceCoins(warriorCost);
                  ++gi.PurchasedSlaveWarrior;
               }
               break;
            case GameAction.E163SlaveGirlSelected:
               IMapItem slavegirl = gi.RemoveFedSlaveGirl();
               if (null == null)
               {
                  returnStatus = "Invalid State - Slave Girl not found in SpecialItems";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               //-----------------------------------------------
               gi.EncounteredMembers.Add(slavegirl);
               gi.IsSlaveGirlActive = true;
               gi.DieRollAction = GameAction.E182CharmGiftRoll;
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
            case GameAction.E188TalismanPegasusConversion:
               if (false == gi.RemoveSpecialItem(SpecialEnum.PegasusMountTalisman))
               {
                  returnStatus = "RemoveSpecialItem(PegasusMountTalisman) returned false for ae=" + gi.EventActive;
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               if (false == gi.Prince.AddNewMount(MountEnum.Pegasus))
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
               if (false == gi.AddCoins(20))
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
               string freemanName = "Freeman";
               if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                  freemanName += "Elf";
               if (true == gi.DwarvenMines.Contains(princeTerritory)) 
                  freemanName += "Dwarf";
               if (true == gi.HalflingTowns.Contains(princeTerritory))
                  freemanName += "Halfling";
               freemanName += Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem freeman = new MapItem(freemanName, 1.0, false, false, false, "c46Freeman", "c46Freeman", princeTerritory, 4, 4, 0);
               gi.AddCompanion(freeman);
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E210HireLancer:
               string lancerName = "Lancer";
               if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                  lancerName += "Elf";
               if (true == gi.DwarvenMines.Contains(princeTerritory))
                  lancerName += "Dwarf";
               if (true == gi.HalflingTowns.Contains(princeTerritory))
                  lancerName += "Halfling";
               lancerName += Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem lancer = new MapItem(lancerName, 1.0, false, false, false, "c47Lancer", "c47Lancer", princeTerritory, 5, 5, 0);
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
               string merc0Name = "Mercenary";
               if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                  merc0Name += "Elf";
               if (true == gi.DwarvenMines.Contains(princeTerritory))
                  merc0Name += "Dwarf";
               if (true == gi.HalflingTowns.Contains(princeTerritory))
                  merc0Name += "Halfling";
               merc0Name += Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem merc0 = new MapItem(merc0Name, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", princeTerritory, 4, 4, 0);
               merc0.Wages = 2;
               gi.AddCompanion(merc0);
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E210HireMerc2:
               string merc1Name = "Mercenary";
               if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                  merc1Name += "Elf";
               if (true == gi.DwarvenMines.Contains(princeTerritory))
                  merc1Name += "Dwarf";
               if (true == gi.HalflingTowns.Contains(princeTerritory))
                  merc1Name += "Halfling";
               merc1Name += Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem merc1 = new MapItem(merc1Name, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", princeTerritory, 4, 4, 0);
               merc1.Wages = 2;
               gi.AddCompanion(merc1);
               string merc2Name = "Mercenary";
               if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                  merc2Name += "Elf";
               if (true == gi.DwarvenMines.Contains(princeTerritory))
                  merc2Name += "Dwarf";
               if (true == gi.HalflingTowns.Contains(princeTerritory))
                  merc2Name += "Halfling";
               merc2Name += Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem merc2 = new MapItem(merc2Name, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", princeTerritory, 4, 4, 0);
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
                     string henchmanName = "Henchman";
                     if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                        henchmanName += "Elf";
                     if (true == gi.DwarvenMines.Contains(princeTerritory))
                        henchmanName += "Dwarf";
                     if (true == gi.HalflingTowns.Contains(princeTerritory))
                        henchmanName += "Halfling";
                     henchmanName += Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem henchman = new MapItem(henchmanName, 1.0, false, false, false, "c49Henchman", "c49Henchman", princeTerritory, 3, 2, 0);
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
               string localGuideName = "Guide";
               if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                  localGuideName += "Elf";
               if (true == gi.DwarvenMines.Contains(princeTerritory))
                  localGuideName += "Dwarf";
               if (true == gi.HalflingTowns.Contains(princeTerritory))
                  localGuideName += "Halfling";
               localGuideName += Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem hiredLocalGuide = new MapItem(localGuideName, 1.0, false, false, false, "c48Guide", "c48Guide", princeTerritory, 3, 2, 0);
               hiredLocalGuide.Wages = 2;
               hiredLocalGuide.IsGuide = true;
               if (false == AddGuideTerritories(gi, hiredLocalGuide, 2))
               {
                  returnStatus = "AddGuideTerritories() returned false for action=" + action.ToString();
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
               string runawayName = "Runaway";
               if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                  runawayName += "Elf";
               if (true == gi.DwarvenMines.Contains(princeTerritory))
                  runawayName += "Dwarf";
               if (true == gi.HalflingTowns.Contains(princeTerritory))
                  runawayName += "Halfling";
               runawayName += Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem runaway = new MapItem(runawayName, 1.0, false, false, false, "c09Runaway", "c09Runaway", princeTerritory, 4, 4, 0);
               gi.AddCompanion(runaway);
               if (false == EncounterEnd(gi, ref action))
               {
                  returnStatus = "EncounterEnd() returned false for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.E210HirePorter:
               for (int i = 0; i < gi.PurchasedPorter; ++i)
               {
                  string porterName = "Porter";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     porterName += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     porterName += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     porterName += "Halfling";
                  porterName += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem porter = new MapItem(porterName, 1.0, false, false, false, "c11Porter", "c11Porter", princeTerritory, 0, 0, 0);
                  porter.GroupNum = --Utilities.PorterNum;  // porter group must be zero or lower
                  porter.Wages = 1;
                  gi.AddCompanion(porter);
                  porterName = "Porter";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     porterName += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     porterName += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     porterName += "Halfling";
                  porterName += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  porter = new MapItem(porterName, 1.0, false, false, false, "c11Porter", "c11Porter", princeTerritory, 0, 0, 0);
                  porter.GroupNum = Utilities.PorterNum; // belong to the same group num
                  porter.Wages = 0;  // a pair cost 1 gp
                  gi.AddCompanion(porter);
               }
               if (0 < gi.PurchasedGuide)
               {
                  string hiredGuideName1 = "Guide";
                  if ((true == gi.ElfCastles.Contains(princeTerritory)) || (true == gi.ElfTowns.Contains(princeTerritory)))
                     hiredGuideName1 += "Elf";
                  if (true == gi.DwarvenMines.Contains(princeTerritory))
                     hiredGuideName1 += "Dwarf";
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                     hiredGuideName1 += "Halfling";
                  hiredGuideName1 += Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem hiredLocalGuideWithPorter = new MapItem(hiredGuideName1, 1.0, false, false, false, "c48Guide", "c48Guide", princeTerritory, 3, 2, 0);
                  hiredLocalGuideWithPorter.Wages = 2;
                  hiredLocalGuideWithPorter.IsGuide = true;
                  if (false == AddGuideTerritories(gi, hiredLocalGuideWithPorter, 2))
                  {
                     returnStatus = "AddGuideTerritories() returned false for action=" + action.ToString();
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
                  case 4: action = GameAction.E212TempleCurse; gi.ForbiddenHires.Add(t212); break;
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
                        string monkGuideName = "Monk" + Utilities.MapItemNum.ToString();
                        ++Utilities.MapItemNum;
                        IMapItem monkGuide = new MapItem(monkGuideName, 1.0, false, false, false, "c19Monk", "c19Monk", t212, 3, 2, 0);
                        monkGuide.IsGuide = true;
                        if (false == AddGuideTerritories(gi, monkGuide, 1))
                        {
                           returnStatus = "AddGuideTerritories() returned false for action=" + action.ToString();
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
                        string monkName1 = "WarriorMonk" + Utilities.MapItemNum.ToString();
                        ++Utilities.MapItemNum;
                        IMapItem warriorMonk1 = new MapItem(monkName1, 1.0, false, false, false, "c19Monk", "c19Monk", t212, 6, 5, 0);
                        warriorMonk1.AddNewMount();
                        gi.AddCompanion(warriorMonk1);
                        string monkName2 = "WarriorMonk" + Utilities.MapItemNum.ToString();
                        ++Utilities.MapItemNum;
                        IMapItem warriorMonk2 = new MapItem(monkName2, 1.0, false, false, false, "c19Monk", "c19Monk", t212, 6, 5, 0);
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
               gi.ReduceCoins(10);
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
               gi.AddCoins(gi.FickleCoin, false);  // Return the fickle share to the party
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
                  gi.ReduceCoins(gi.Bribe);
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
                  gi.ReduceCoins(gi.Bribe);
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
                     gi.ReduceCoins(2);
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
                     gi.ReduceCoins(2);
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
         sb12.Append("\t\ta="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
            { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append("\t\tdra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
            { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append("\t\te="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
             { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append("\t\tes="); sb12.Append(previousStartEvent);
         if (previousStartEvent != gi.EventStart)
             { sb12.Append("=>"); sb12.Append(gi.EventStart); }
         sb12.Append("\t\tdr="); sb12.Append(dieRoll.ToString());
         sb12.Append("\t\tm="); sb12.Append(gi.Prince.Movement.ToString());
         sb12.Append("\t\tmu="); sb12.Append(gi.Prince.MovementUsed.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      protected bool EncounterStart(IGameInstance gi, ref GameAction action, int dieRoll)
      {
         IOption isEasyMonstersOption = gi.Options.Find("EasyMonsters");
         if (null == isEasyMonstersOption)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): returned option=null");
            return false;
         }
         //--------------------------------------
         ITerritory princeTerritory = gi.Prince.Territory;
         string key = gi.EventStart = gi.EventActive;
         switch (key)
         {
            case "e002a": // Mercenaries
               for (int i = 0; i < dieRoll; ++i)
               {
                  string miName = "Mercenary" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem guard = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", princeTerritory, 4, 5, 4);
                  if (true == isEasyMonstersOption.IsEnabled)
                     guard = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", princeTerritory, 1, 1, 4);
                  gi.EncounteredMembers.Add(guard);
               }
               gi.DieResults[key][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e003a": // Swordsman
            case "e003b": // Swordsman
            case "e003c": // Swordsman
               gi.EventStart = "e003";
               gi.EncounteredMembers.Clear();
               string swordsmanName = "Swordsman" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem swordsman = new MapItem(swordsmanName, 1.0, false, false, false, "c53Swordsman", "c53Swordsman", princeTerritory, 6, 6, 7);
               swordsman.AddNewMount();
               gi.EncounteredMembers.Add(swordsman);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e004": // Mercenaries
               gi.EncounteredMembers.Clear();
               string mercenaryLeadName = "Mercenary" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem mercenaryLead = new MapItem(mercenaryLeadName, 1.0, false, false, false, "c65MercLead", "c65MercLead", princeTerritory, 6, 6, 50);
               if (true == isEasyMonstersOption.IsEnabled)
                  mercenaryLead = new MapItem(mercenaryLeadName, 1.0, false, false, false, "c65MercLead", "c65MercLead", princeTerritory, 1, 1, 50);
               mercenaryLead.AddNewMount();
               gi.EncounteredMembers.Add(mercenaryLead);
               if (true == isEasyMonstersOption.IsEnabled)
                  dieRoll = 1;
               for (int i = 0; i < dieRoll; ++i)
               {
                  string mercenaryName = "Mercenary" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem mercenary = new MapItem(mercenaryName, 1.0, false, false, false, "c18Mercenary", "c18Mercenary", princeTerritory, 4, 5, 4);
                  if (true == isEasyMonstersOption.IsEnabled)
                     mercenary = new MapItem(mercenaryName, 1.0, false, false, false, "c18Mercenary", "c18Mercenary", princeTerritory, 1, 1, 4);
                  if (dieRoll < 3)
                     mercenary.AddNewMount();
                  gi.EncounteredMembers.Add(mercenary);
               }
               gi.DieResults[key][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e005": // Amazons
               gi.EncounteredMembers.Clear();
               if (true == isEasyMonstersOption.IsEnabled)
                  dieRoll = 2;
               int amazonGroupNum = Utilities.GroupNum;
               ++Utilities.GroupNum;
               for (int i = 0; i < dieRoll + 1; ++i)
               {
                  string amazonName = "Amazon" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem amazon = new MapItem(amazonName, 1.0, false, false, false, "c57Amazon", "c57Amazon", princeTerritory, 5, 6, 4);
                  if (true == isEasyMonstersOption.IsEnabled)
                     amazon = new MapItem(amazonName, 1.0, false, false, false, "c57Amazon", "c57Amazon", princeTerritory, 1, 1, 4);
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
                  string dwarfLeaderName = "DwarfWarrior" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem dwarfLeader = new MapItem(dwarfLeaderName, 1.0, false, false, false, "c68DwarfLead", "c68DwarfLead", princeTerritory, 7, 6, 21);
                  if (true == isEasyMonstersOption.IsEnabled)
                     dwarfLeader = new MapItem(dwarfLeaderName, 1.0, false, false, false, "c68DwarfLead", "c68DwarfLead", princeTerritory, 1, 1, 21);
                  gi.EncounteredMembers.Add(dwarfLeader);
               }
               else
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 1: case 2: case 3:
                        switch (gi.DwarvenChoice)
                        {
                           case "Talk":  gi.EventDisplayed = gi.EventActive = "e006c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Evade": gi.EventDisplayed = gi.EventActive = "e006d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           case "Fight": gi.EventDisplayed = gi.EventActive = "e006e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                           default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default choice=" + gi.DwarvenChoice + " ae=" + gi.EventActive); return false;
                        }
                        break;
                     case 4:
                        if( false == gi.IsDwarvenBandSizeSet)
                        {
                           string dwarfFriendName = "Dwarf" + Utilities.MapItemNum.ToString();
                           ++Utilities.MapItemNum;
                           IMapItem dwarfFriend = new MapItem(dwarfFriendName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 6, 5, 12);
                           if (true == isEasyMonstersOption.IsEnabled)
                              dwarfFriend = new MapItem(dwarfFriendName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 1, 1, 12);
                           gi.EncounteredMembers.Add(dwarfFriend);
                        }
                        switch (gi.DwarvenChoice)
                        {
                           case "Talk":  gi.EventDisplayed = gi.EventActive = "e006c"; gi.DieRollAction = GameAction.EncounterRoll; break;
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
                              case "Talk":  gi.EventDisplayed = gi.EventActive = "e006c"; gi.DieRollAction = GameAction.EncounterRoll; break;
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
                  string elfLeaderName = "ElfWarrior" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem elfLeader = new MapItem(elfLeaderName, 1.0, false, false, false, "c69ElfLead", "c69ElfLead", princeTerritory, 7, 6, 21);
                  if (true == isEasyMonstersOption.IsEnabled)
                     elfLeader = new MapItem(elfLeaderName, 1.0, false, false, false, "c69ElfLead", "c69ElfLead", princeTerritory, 1, 1, 21);
                  gi.EncounteredMembers.Add(elfLeader);
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
                           if ( 0 == gi.EncounteredMembers.Count )
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
                           string elfAssistantName = "Elf" + Utilities.MapItemNum.ToString();
                           ++Utilities.MapItemNum;
                           IMapItem elfAssistant= new MapItem(elfAssistantName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 3, 3, 2);
                           if (true == isEasyMonstersOption.IsEnabled)
                              elfAssistant = new MapItem(elfAssistantName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 1, 1, 2);
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
                           string elfFriendName = "Elf" + Utilities.MapItemNum.ToString();
                           ++Utilities.MapItemNum;
                           IMapItem elfFriend = new MapItem(elfFriendName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 4, 4, 7);
                           if (true == isEasyMonstersOption.IsEnabled)
                              elfFriend = new MapItem(elfFriendName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 1, 1, 7);
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
               string halflingWarriorName = "HalflingWarrior" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem halflingWarrior = new MapItem(halflingWarriorName, 1.0, false, false, false, "c70HalflingLead", "c70HalflingLead", princeTerritory, 6, 3, 4);
               gi.EncounteredMembers.Add(halflingWarrior);
               gi.EventDisplayed = gi.EventActive = "e304";
               break;
            case "e008a": // Halfling
               gi.EventStart = "e008";
               gi.EncounteredMembers.Clear();
               string halflingWarriorName1 = "HalflingWarrior" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem halflingWarrior1 = new MapItem(halflingWarriorName1, 1.0, false, false, false, "c70HalflingLead", "c70HalflingLead", princeTerritory, 6, 3, 4);
               gi.EncounteredMembers.Add(halflingWarrior1);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e011b": // peaceful farmer - raid 
               string farmerPeacefulName = "Farmer" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem farmePeaceful = new MapItem(farmerPeacefulName, 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 7, 4, 1);
               if (true == isEasyMonstersOption.IsEnabled)
                  farmePeaceful = new MapItem(farmerPeacefulName, 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 1, 1, 1);
               gi.EncounteredMembers.Add(farmePeaceful);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e012b": // farmer with protector - raid 
               string farmerWithProtectorName = "Farmer" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem farmerWithProtector = new MapItem(farmerWithProtectorName, 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 7, 4, 2);
               if (true == isEasyMonstersOption.IsEnabled)
                  farmerWithProtector = new MapItem(farmerWithProtectorName, 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 1, 1, 2);
               gi.EncounteredMembers.Add(farmerWithProtector);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e013b": // rich farmer retainer - raid 
               int numRetainers = 4;
               if (true == isEasyMonstersOption.IsEnabled)
                  numRetainers = 1;
               for (int i = 0; i < numRetainers; ++i)
               {
                  string farmerRetainerhName = "Farmer" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem farmerRetainer = new MapItem(farmerRetainerhName, 1.0, false, false, false, "c40Retainer", "c40Retainer", princeTerritory, 4, 4, 1);
                  if (true == isEasyMonstersOption.IsEnabled)
                     farmerRetainer = new MapItem(farmerRetainerhName, 1.0, false, false, false, "c40Retainer", "c40Retainer", princeTerritory, 1, 1, 1);
                  gi.EncounteredMembers.Add(farmerRetainer);
               }
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e013c": // rich farmer - raid 
               gi.EncounteredMembers.Clear();
               string farmerRichName = "Farmer" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem farmerRich = new MapItem(farmerRichName, 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 6, 5, 30);
               if (true == isEasyMonstersOption.IsEnabled)
                  farmerRich = new MapItem(farmerRichName, 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 1, 1, 30);
               gi.EncounteredMembers.Add(farmerRich);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e014a": // hostile reapers - freadly approach
               string reaverHostileBossName0 = "ReaverBoss" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem reaverHostileBoss0 = new MapItem(reaverHostileBossName0, 1.0, false, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 5, 5, 10);
               if (true == isEasyMonstersOption.IsEnabled)
                  reaverHostileBoss0 = new MapItem(reaverHostileBossName0, 1.0, false, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 1, 1, 10);
               gi.EncounteredMembers.Add(reaverHostileBoss0);
               int numReapers0 = dieRoll + 1;
               for (int i = 0; i < numReapers0; ++i)
               {
                  string miName = "Reaver" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem reaver = new MapItem(miName, 1.0, false, false, false, "C36Reaver", "C36Reaver", princeTerritory, 4, 4, 4);
                  if (true == isEasyMonstersOption.IsEnabled)
                     reaver = new MapItem(miName, 1.0, false, false, false, "C36Reaver", "C36Reaver", princeTerritory, 1, 1, 4);
                  gi.EncounteredMembers.Add(reaver);
               }
               gi.DieResults["e014a"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e014c": // hostile reapers
               string reaverHostileBossName1 = "ReaverBoss" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem reaverHostlieBoss1 = new MapItem(reaverHostileBossName1, 1.0, false, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 4, 5, 7);
               if (true == isEasyMonstersOption.IsEnabled)
                  reaverHostlieBoss1 = new MapItem(reaverHostileBossName1, 1.0, false, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 1, 1, 7);
               gi.EncounteredMembers.Add(reaverHostlieBoss1);
               int numReapers1 = dieRoll + 1;
               for (int i = 0; i < numReapers1; ++i)
               {
                  string miName = "Reaver" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem reaver = new MapItem(miName, 1.0, false, false, false, "C36Reaver", "C36Reaver", princeTerritory, 4, 4, 4);
                  if (true == isEasyMonstersOption.IsEnabled)
                     reaver = new MapItem(miName, 1.0, false, false, false, "C36Reaver", "C36Reaver", princeTerritory, 1, 1, 4);
                  gi.EncounteredMembers.Add(reaver);
               }
               gi.DieResults["e014c"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e015a": // Friendly reapers
               string reaverRriendlyBossName0 = "ReaverBoss" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem reaverFriendlyBoss0 = new MapItem(reaverRriendlyBossName0, 1.0, false, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 4, 5, 7);
               if (true == isEasyMonstersOption.IsEnabled)
                  reaverFriendlyBoss0 = new MapItem(reaverRriendlyBossName0, 1.0, false, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 1, 1, 7);
               gi.EncounteredMembers.Add(reaverFriendlyBoss0);
               for (int i = 0; i < dieRoll; ++i)
               {
                  string miName = "Reaver" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem reaver = new MapItem(miName, 1.0, false, false, false, "C36Reaver", "C36Reaver", princeTerritory, 4, 4, 4);
                  if (true == isEasyMonstersOption.IsEnabled)
                     reaver = new MapItem(miName, 1.0, false, false, false, "C36Reaver", "C36Reaver", princeTerritory, 1, 1, 4);
                  gi.EncounteredMembers.Add(reaver);
               }
               gi.IsReaverClanTrade = true;
               gi.DieResults["e015a"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e015c": // frien reapers raid
               string reaverFriendlyBossName1 = "ReaverBoss" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem reaverFriendlyBoss1 = new MapItem(reaverFriendlyBossName1, 1.0, false, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 4, 5, 7);
               if (true == isEasyMonstersOption.IsEnabled)
                  reaverFriendlyBoss1 = new MapItem(reaverFriendlyBossName1, 1.0, false, false, false, "C37ReaverBoss", "C37ReaverBoss", princeTerritory, 1, 1, 7);
               gi.EncounteredMembers.Add(reaverFriendlyBoss1);
               int numReapers11 = dieRoll + 1;
               for (int i = 0; i < numReapers11; ++i)
               {
                  string miName = "Reaver" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem reaver = new MapItem(miName, 1.0, false, false, false, "C36Reaver", "C36Reaver", princeTerritory, 4, 4, 4);
                  if (true == isEasyMonstersOption.IsEnabled)
                     reaver = new MapItem(miName, 1.0, false, false, false, "C36Reaver", "C36Reaver", princeTerritory, 1, 1, 4);
                  gi.EncounteredMembers.Add(reaver);
               }
               gi.DieResults["e015c"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e016a": // Friendly magician
               gi.IsMagicianProvideGift = true;
               string magicianName = "Magician" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c26Magician", "c26Magician", princeTerritory, 5, 3, 0);
               gi.EncounteredMembers.Add(magician);
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e017": // Peasant Mob
               IMapItem leader = new MapItem("FarmerLeader", 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 3, 2, 2);
               if (true == isEasyMonstersOption.IsEnabled)
                  leader = new MapItem("FarmerLeader", 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 1, 1, 2);
               gi.EncounteredMembers.Add(leader);
               int numFarmers = 2 * dieRoll - 1;
               if (true == isEasyMonstersOption.IsEnabled)
                  numFarmers = 3;
               for (int i = 0; i < numFarmers; ++i)
               {
                  string miName = "Farmer" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem farmer = new MapItem(miName, 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 2, 2, 0);
                  if (true == isEasyMonstersOption.IsEnabled)
                     farmer = new MapItem(miName, 1.0, false, false, false, "c17Farmer", "c17Farmer", princeTerritory, 1, 1, 0);
                  gi.EncounteredMembers.Add(farmer);
               }
               gi.EventDisplayed = gi.EventActive = "e017";
               gi.DieResults["e017"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e018a": // priest
            case "e018b": // priest
               gi.EventStart = "e018";
               string priestName = "Priest" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem priest = new MapItem(priestName, 1.0, false, false, false, "c14Priest", "c14Priest", princeTerritory, 3, 3, 25);
               if (true == isEasyMonstersOption.IsEnabled)
                  priest = new MapItem(priestName, 1.0, false, false, false, "c14Priest", "c14Priest", princeTerritory, 1, 1, 25);
               priest.AddNewMount();
               gi.EncounteredMembers.Add(priest);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e020": // Traveling Monk
               string monkName1 = "TravelingMonk" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem travelingMonk1 = new MapItem(monkName1, 1.0, false, false, false, "c19Monk", "c19Monk", princeTerritory, 5, 4, 4);
               if (true == isEasyMonstersOption.IsEnabled)
                  travelingMonk1 = new MapItem(monkName1, 1.0, false, false, false, "c19Monk", "c19Monk", princeTerritory, 1, 1, 4);
               gi.EncounteredMembers.Add(travelingMonk1);
               if (4 < dieRoll) // add a second one on die of 5 or 6
               {
                  string monkName2 = "TravelingMonk" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem travelingMonk2 = new MapItem(monkName2, 1.0, false, false, false, "c19Monk", "c19Monk", princeTerritory, 5, 4, 4);
                  if (true == isEasyMonstersOption.IsEnabled)
                     travelingMonk2 = new MapItem(monkName2, 1.0, false, false, false, "c19Monk", "c19Monk", princeTerritory, 1, 1, 4);
                  gi.EncounteredMembers.Add(travelingMonk2);
               }
               gi.DieResults["e020"][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e021": // Warrior Monks
               int numMonkWarriors = (int)Math.Ceiling((double)dieRoll * 0.5);
               for (int i = 0; i < numMonkWarriors; ++i)
               {
                  string miName = "WarriorMonk" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem warriorMonk = new MapItem(miName, 1.0, false, false, false, "c19Monk", "c19Monk", princeTerritory, 4, 5, 10);
                  if (true == isEasyMonstersOption.IsEnabled)
                     warriorMonk = new MapItem(miName, 1.0, false, false, false, "c19Monk", "c19Monk", princeTerritory, 1, 1, 10);
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
                     string monkName = "HermitMonk" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem hermitMonk = new MapItem(monkName, 1.0, false, false, false, "c19Monk", "c19Monk", princeTerritory, 6, 3, 0);
                     if (true == isEasyMonstersOption.IsEnabled)
                        hermitMonk = new MapItem(monkName, 1.0, false, false, false, "c19Monk", "c19Monk", princeTerritory, 1, 1, 0);
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
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e023":
               gi.DieResults[key][0] = dieRoll;
               gi.EncounteredMembers.Clear();
               string wizardName = "Wizard" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem wizard = new MapItem(wizardName, 1.0, false, false, false, "c12Wizard", "c12Wizard", princeTerritory, 4, 4, 60);
               if (true == isEasyMonstersOption.IsEnabled)
                  wizard = new MapItem(wizardName, 1.0, false, false, false, "c12Wizard", "c12Wizard", princeTerritory, 1, 1, 60);
               if (3 < dieRoll)
                  wizard.AddNewMount();
               gi.EncounteredMembers.Add(wizard);
               string henchmanName = "Henchman" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem henchman = new MapItem(henchmanName, 1.0, false, false, false, "c49Henchman", "c49Henchman", princeTerritory, 4, 5, 4);
               if (true == isEasyMonstersOption.IsEnabled)
                  henchman = new MapItem(henchmanName, 1.0, false, false, false, "c49Henchman", "c49Henchman", princeTerritory, 1, 1, 4);
               if (3 < dieRoll)
                  henchman.AddNewMount();
               gi.EncounteredMembers.Add(henchman);
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
               gi.EncounteredMembers.Clear();
               int numGhosts = dieRoll + 1;
               if (true == isEasyMonstersOption.IsEnabled)
                  numGhosts = 2;
               for (int i = 0; i < numGhosts; ++i)
               {
                  string miName = "Ghost" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem ghost = new MapItem(miName, 1.0, false, false, false, "c20Ghost", "c20Ghost", princeTerritory, 2, 4, 0);
                  gi.EncounteredMembers.Add(ghost);
               }
               gi.EventDisplayed = gi.EventActive = "e310";  // party is surprised
               break;
            case "e033":  // Warrior Wraiths
               gi.EncounteredMembers.Clear();
               int numWraiths = dieRoll;
               if (1 == numWraiths) // always at least two
                  ++numWraiths;
               for (int i = 0; i < numWraiths; ++i)
               {
                  string miName = "Wraith" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem wraith = new MapItem(miName, 1.0, false, false, false, "c24Wraith", "c24Wraith", princeTerritory, 9, 6, 0);
                  if (true == isEasyMonstersOption.IsEnabled)
                     wraith = new MapItem(miName, 1.0, false, false, false, "c24Wraith", "c24Wraith", princeTerritory, 1, 1, 0);
                  gi.EncounteredMembers.Add(wraith);
               }
               gi.EventDisplayed = gi.EventActive = "e307"; // wraiths attack first
               break;
            case "e034":  // Spectre of Inner Tomb
               gi.EncounteredMembers.Clear();
               IMapItem spectre = new MapItem("Spectre", 1.0, false, false, false, "c25Spectre", "c25Spectre", princeTerritory, 3, 7, 0);
               if (true == isEasyMonstersOption.IsEnabled)
                  spectre = new MapItem("Spectre", 1.0, false, false, false, "c25Spectre", "c25Spectre", princeTerritory, 1, 1, 0);
               gi.EncounteredMembers.Add(spectre);
               gi.EventDisplayed = gi.EventActive = "e034a";
               break;
            case "e036":  // golem at the gate
               gi.EncounteredMembers.Clear();
               IMapItem golem = new MapItem("Golem", 1.0, false, false, false, "c27Golem", "c27Golem", princeTerritory, 8, 6, 0);
               if (true == isEasyMonstersOption.IsEnabled)
                  golem = new MapItem("Golem", 1.0, false, false, false, "c27Golem", "c27Golem", princeTerritory, 1, 1, 0);
               gi.EncounteredMembers.Add(golem);
               foreach (IMapItem mi in gi.PartyMembers)  // Prince fights golem alone
               {
                  if ("Prince" != mi.Name)
                     gi.LostPartyMembers.Add(mi);
               }
               gi.PartyMembers.Clear();
               gi.PartyMembers.Add(gi.Prince);
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e045":  // arch of travel
               gi.EventDisplayed = gi.EventActive = "e045b";
               gi.DieRollAction = GameAction.EncounterRoll;
               if (false == gi.Arches.Contains(princeTerritory)) // add arch icon to canvas when UpdateCanvas() is called in GameViewerWindow.cs
                  gi.Arches.Add(princeTerritory);
               break;
            case "e046":  // gateway to darkness
               gi.EncounteredMembers.Clear();
               ++gi.GuardianCount;
               IMapItem guardian = new MapItem("Guardian", 1.0, false, false, false, "c28Guardian", "c28Guardian", princeTerritory, 7, 7, 0);
               if (true == isEasyMonstersOption.IsEnabled)
                  guardian = new MapItem("Guardian", 1.0, false, false, false, "c28Guardian", "c28Guardian", princeTerritory, 1, 1, 0);
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
                  IMapItem guardian1 = new MapItem("Guardian", 1.0, false, false, false, "c28Guardian", "c28Guardian", princeTerritory, 7, 7, 0);
                  if (true == isEasyMonstersOption.IsEnabled)
                     guardian1 = new MapItem("Guardian", 1.0, false, false, false, "c28Guardian", "c28Guardian", princeTerritory, 1, 1, 0);
                  gi.EncounteredMembers.Add(guardian1);
               }
               gi.EventDisplayed = gi.EventActive = "e307";
               break;
            case "e047":  // mirror of reversal
               gi.EncounteredMembers.Clear();
               IMapItem mirror = new MapItem(gi.Prince);
               mirror.Name = "Mirror";
               mirror.TopImageName = "c34PrinceMirror";
               mirror.BottomImageName = "c34PrinceMirror";
               mirror.OverlayImageName = "";
               if (true == isEasyMonstersOption.IsEnabled)
               {
                  mirror.Endurance = 1;
                  mirror.Combat = 1;
               }
               theNumHydraTeeth = gi.HydraTeethCount;
               gi.EncounteredMembers.Add(mirror);
               gi.EventDisplayed = gi.EventActive = "e307";
               break;
            case "e048":  // Fugitive 
               gi.DieResults[key][0] = dieRoll;
               gi.EncounteredMembers.Clear();
               switch (gi.DieResults["e048"][0])
               {
                  case 1:
                     string miName1 = "Swordswoman" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem swordswoman = new MapItem(miName1, 1.0, false, false, false, "c76Swordswoman", "c76Swordswoman", princeTerritory, 7, 7, 4);
                     gi.EncounteredMembers.Add(swordswoman);
                     break;
                  case 2:
                     string miName2 = "Runaway" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem runaway = new MapItem(miName2, 1.0, false, false, false, "c09Runaway", "c09Runaway", princeTerritory, 4, 2, 0);
                     gi.EncounteredMembers.Add(runaway);
                     break;
                  case 3:
                     if (true == gi.IsMarkOfCain)
                     {
                        action = GameAction.E018MarkOfCain;
                     }
                     else
                     {
                        string miName31 = "Priest" + Utilities.MapItemNum.ToString();
                        ++Utilities.MapItemNum;
                        IMapItem priest3 = new MapItem(miName31, 1.0, false, false, false, "c14Priest", "c14Priest", princeTerritory, 3, 3, 10);
                        gi.EncounteredMembers.Add(priest3);
                     }
                     break;
                  case 4:
                     string miName3 = "Magician" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem magician4 = new MapItem(miName3, 1.0, false, false, false, "c16Magician", "c16Magician", princeTerritory, 2, 3, 5);
                     gi.EncounteredMembers.Add(magician4);
                     break;
                  case 5:
                     string miName5 = "Merchant" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem merchant5 = new MapItem(miName5, 1.0, false, false, false, "c77Merchant", "c77Merchant", princeTerritory, 3, 2, 0);
                     merchant5.IsFugitive = true;
                     gi.EncounteredMembers.Add(merchant5);
                     break;
                  case 6:
                     string miName6 = "Deserter" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem deserter = new MapItem(miName6, 1.0, false, false, false, "c78Deserter", "c78Deserter", princeTerritory, 4, 4, 2);
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
               string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", princeTerritory, 0, 0, 0);
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
                     if (false == gi.EscapedLocations.Contains(princeTerritory)) //TODO Add escape location for every town, temple, castle
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
                     string constabularyName = "Constabulary" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem constabulary = new MapItem(constabularyName, 1.0, false, false, false, "c45Constabulary", "c45Constabulary", princeTerritory, 4, 5, 4);
                     if (true == isEasyMonstersOption.IsEnabled)
                        constabulary = new MapItem(constabularyName, 1.0, false, false, false, "c45Constabulary", "c45Constabulary", princeTerritory, 1, 1, 4);
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
               string miName0 = "BanditLeader";
               IMapItem banditLeader = new MapItem(miName0, 1.0, false, false, false, "c21Bandit", "c21Bandit", princeTerritory, 6, 6, 15);
               if (true == isEasyMonstersOption.IsEnabled)
                  banditLeader = new MapItem(miName0, 1.0, false, false, false, "c21Bandit", "c21Bandit", princeTerritory, 1, 1, 15);
               gi.EncounteredMembers.Add(banditLeader);
               int numBandits = gi.PartyMembers.Count + 1; // leader plus one extra exceeds party count by two
               if (true == isEasyMonstersOption.IsEnabled)
                  numBandits = 1;
               for (int i = 0; i < numBandits; ++i)
               {
                  string miName1 = "Bandit" + i.ToString();
                  IMapItem bandit1 = new MapItem(miName1, 1.0, false, false, false, "c21Bandit", "c21Bandit", princeTerritory, 4, 5, 1);
                  if (true == isEasyMonstersOption.IsEnabled)
                     bandit1 = new MapItem(miName1, 1.0, false, false, false, "c21Bandit", "c21Bandit", princeTerritory, 1, 1, 1);
                  gi.EncounteredMembers.Add(bandit1);
               }
               gi.EventDisplayed = gi.EventActive = "e310"; // party is surprised
               break;
            case "e052": // Goblins
               IMapItem hobgoblin = new MapItem("Hobgoblin", 1.0, false, false, false, "c23Hobgoblin", "c23Hobgoblin", princeTerritory, 5, 6, 5);
               if (true == isEasyMonstersOption.IsEnabled)
                  hobgoblin = new MapItem("Hobgoblin", 1.0, false, false, false, "c23Hobgoblin", "c23Hobgoblin", princeTerritory, 1, 1, 5);
               gi.EncounteredMembers.Add(hobgoblin);
               gi.DieResults["e052"][0] = dieRoll;
               int numGoblins = dieRoll - 1;
               for (int i = 0; i < numGoblins; ++i)
               {
                  string miName = "Goblin" + i.ToString();
                  IMapItem goblin = new MapItem(miName, 1.0, false, false, false, "c22Goblin", "c22Goblin", princeTerritory, 3, 3, 1);
                  if (true == isEasyMonstersOption.IsEnabled)
                     goblin = new MapItem(miName, 1.0, false, false, false, "c22Goblin", "c22Goblin", princeTerritory, 1, 1, 1);
                  gi.EncounteredMembers.Add(goblin);
               }
               break;
            case "e054b": // Goblin Tower Fight - EncounterStart()
               if (null == gi.GoblinKeeps.Find(princeTerritory.Name))
                  gi.GoblinKeeps.Add(princeTerritory);
               gi.EncounteredMembers.Clear();
               IMapItem hobgoblin1 = new MapItem("Hobgoblin", 1.0, false, false, false, "c23Hobgoblin", "c23Hobgoblin", princeTerritory, 5, 6, 4);
               if (true == isEasyMonstersOption.IsEnabled)
                  hobgoblin1 = new MapItem("Hobgoblin", 1.0, false, false, false, "c23Hobgoblin", "c23Hobgoblin", princeTerritory, 1, 1, 5);
               gi.EncounteredMembers.Add(hobgoblin1);
               gi.DieResults["e054b"][0] = dieRoll;
               int numGoblins1 = dieRoll - 1;
               for (int i = 0; i < numGoblins1; ++i)
               {
                  string miName = "Goblin" + i.ToString();
                  IMapItem goblin = new MapItem(miName, 1.0, false, false, false, "c22Goblin", "c22Goblin", princeTerritory, 3, 3, 1);
                  if (true == isEasyMonstersOption.IsEnabled)
                     goblin = new MapItem(miName, 1.0, false, false, false, "c22Goblin", "c22Goblin", princeTerritory, 1, 1, 1);
                  gi.EncounteredMembers.Add(goblin);
               }
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e055": // Orcs
               gi.EncounteredMembers.Clear();
               IMapItem chieftain = new MapItem("Orc", 1.0, false, false, false, "c64OrcChief", "c64OrcChief", princeTerritory, 6, 5, 7);
               if (true == isEasyMonstersOption.IsEnabled)
                  chieftain = new MapItem("Orc", 1.0, false, false, false, "c64OrcChief", "c64OrcChief", princeTerritory, 1, 1, 7);
               gi.EncounteredMembers.Add(chieftain);
               gi.DieResults["e055"][0] = dieRoll;
               int numOrcsInBand = dieRoll - 1;
               if (true == isEasyMonstersOption.IsEnabled)
                  numOrcsInBand = 1;
               for (int i = 0; i < numOrcsInBand; ++i)
               {
                  string miName = "Orc" + i.ToString();
                  IMapItem orc = new MapItem(miName, 1.0, false, false, false, "c30Orc", "c30Orc", princeTerritory, 5, 4, 1);
                  if (true == isEasyMonstersOption.IsEnabled)
                     orc = new MapItem(miName, 1.0, false, false, false, "c30Orc", "c30Orc", princeTerritory, 1, 1, 1);
                  gi.EncounteredMembers.Add(orc);
               }
               break;
            case "e056a": // Orc Tower
               if (null == gi.OrcTowers.Find(princeTerritory.Name))
                  gi.OrcTowers.Add(princeTerritory);
               gi.EncounteredMembers.Clear();
               IMapItem demiTroll = new MapItem("DemiTroll", 1.0, false, false, false, "c29DemiTroll", "c29DemiTroll", princeTerritory, 7, 8, 10);
               if (true == isEasyMonstersOption.IsEnabled)
                  demiTroll = new MapItem("DemiTroll", 1.0, false, false, false, "c29DemiTroll", "c29DemiTroll", princeTerritory, 1, 1, 10);
               gi.EncounteredMembers.Add(demiTroll);
               gi.DieResults["e056a"][0] = dieRoll;
               int numOrcs = dieRoll + 1;
               if (true == isEasyMonstersOption.IsEnabled)
                  numOrcs = 1;
               for (int i = 0; i < numOrcs; ++i)
               {
                  string miName = "Orc" + i.ToString();
                  IMapItem orc = new MapItem(miName, 1.0, false, false, false, "c30Orc", "c30Orc", princeTerritory, 5, 5, 2);
                  if (true == isEasyMonstersOption.IsEnabled)
                     orc = new MapItem(miName, 1.0, false, false, false, "c30Orc", "c30Orc", princeTerritory, 1, 1, 2);
                  gi.EncounteredMembers.Add(orc);
               }
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e057": // troll
               gi.EncounteredMembers.Clear();
               IMapItem troll = new MapItem("Troll", 1.0, false, false, false, "c31Troll", "c31Troll", princeTerritory, 8, 8, 15);
               if (true == isEasyMonstersOption.IsEnabled)
                  troll = new MapItem("Troll", 1.0, false, false, false, "c31Troll", "c31Troll", princeTerritory, 5, 5, 15);
               gi.EncounteredMembers.Add(troll);
               gi.DieRollAction = GameAction.DieRollActionNone;
               if (gi.WitAndWile < dieRoll)
                  gi.EventDisplayed = gi.EventActive = "e307";
               else
                  gi.EventDisplayed = gi.EventActive = "e304";
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
                     string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 6, 5, 10);
                     if (true == isEasyMonstersOption.IsEnabled)
                        dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 1, 1, 10);
                     gi.EncounteredMembers.Add(dwarf);
                  }
                  if (gi.PartyMembers.Count < numDwarves)
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e058a";
                  else
                     gi.EventStart = gi.EventDisplayed = gi.EventActive = "e058b";
               }
               break;
            case "e059": // dwarven mines
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  if (gi.WitAndWile <= gi.DieResults[key][0])
                  {
                     gi.EventDisplayed = gi.EventActive = "e060";   // arrested
                     gi.DieRollAction = GameAction.EncounterRoll;
                  }
                  else
                  {
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false w/ ae=" + gi.EventActive);
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
                           gi.EventDisplayed = gi.EventActive = "e045";
                        else
                           gi.EventDisplayed = gi.EventActive = "e045a";
                        break;  
                     case 5: case 6: gi.EventDisplayed = gi.EventActive = "e028"; break;                                      // cave tombs
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e068": // wizard abode
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.EncounteredMembers.Clear();
                  string wizardName2 = "Wizard" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem wizard2 = new MapItem(wizardName2, 1.0, false, false, false, "c12Wizard", "c12Wizard", princeTerritory, 4, 4, 60);
                  if (true == isEasyMonstersOption.IsEnabled)
                     wizard2 = new MapItem(wizardName2, 1.0, false, false, false, "c12Wizard", "c12Wizard", princeTerritory, 1, 1, 60);
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
                     case 5: case 6: 
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
                  if (true == isEasyMonstersOption.IsEnabled)
                     dieRoll = 1;
                  gi.DieResults["e071"][0] = dieRoll;
                  gi.NumMembersBeingFollowed = dieRoll;
                  for (int i = 0; i < dieRoll; ++i)
                  {
                     string elfName = "Elf" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 4, 5, 7);
                     if (true == isEasyMonstersOption.IsEnabled)
                        elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 1, 1, 7);
                     gi.EncounteredMembers.Add(elf);
                  }
               }
               break;
            case "e071a": // elves
            case "e071b": // elves
            case "e071c": // elves
               gi.DieResults["e071"][0] = Utilities.NO_RESULT; // avoid problem if two encounters in the same day
               gi.EventStart = "e071";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e072": // elven band - EncounterStart()
               gi.IsTalkActive = false;
               gi.EventStart = gi.EventDisplayed = gi.EventActive = "e071";
               gi.DieRollAction = GameAction.EncounterStart;
               break;
            case "e072a": // elves - EncounterStart()
               if (0 == gi.NumMembersBeingFollowed)
               {
                  gi.EncounteredMembers.Clear();
                  ++dieRoll;
                  if (true == isEasyMonstersOption.IsEnabled)
                     dieRoll = 1;
                  gi.DieResults["e072a"][0] = dieRoll;
                  gi.NumMembersBeingFollowed = dieRoll;
                  for (int i = 0; i < dieRoll; ++i)
                  {
                     string elfName = "Elf" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 4, 5, 7);
                     if (true == isEasyMonstersOption.IsEnabled)
                        elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 1, 1, 7);
                     gi.EncounteredMembers.Add(elf);
                  }
               }
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e073":  // witch
               gi.EncounteredMembers.Clear();
               string witchName = "Witch" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem witch = new MapItem(witchName, 1.0, false, false, false, "c13Witch", "c13Witch", princeTerritory, 3, 1, 5);
               gi.EncounteredMembers.Add(witch);
               if (gi.WitAndWile < dieRoll)
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e073a";
               else if (gi.WitAndWile == dieRoll)
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e073b";
               else
                  gi.EventStart = gi.EventDisplayed = gi.EventActive = "e073c";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e074":  // spiders
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.EncounteredMembers.Clear();
                  int numSpiders = gi.DieResults[key][0];
                  if (true == isEasyMonstersOption.IsEnabled)
                     numSpiders = 1;
                  for (int i = 0; i < numSpiders; ++i)
                  {
                     string miName = "Spider" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem spider = new MapItem(miName, 1.0, false, false, false, "c54Spider", "c54Spider", princeTerritory, 3, 4, 0);
                     if (true == isEasyMonstersOption.IsEnabled)
                        spider = new MapItem(miName, 1.0, false, false, false, "c54Spider", "c54Spider", princeTerritory, 1, 1, 0);
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
                  if (true == isEasyMonstersOption.IsEnabled)
                     numWolves = 1;
                  for (int i = 0; i < numWolves; ++i)
                  {
                     string miName = "Wolf" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem wolf = new MapItem(miName, 1.0, false, false, false, "c71Wolf", "c71Wolf", princeTerritory, 3, 3, 0);
                     if (true == isEasyMonstersOption.IsEnabled)
                        wolf = new MapItem(miName, 1.0, false, false, false, "c71Wolf", "c71Wolf", princeTerritory, 1, 1, 0);
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
                     gi.ReduceMount(MountEnum.Horse);
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
                  if( 0 < numLostMounts )
                  {
                     action = GameAction.E078BadGoingRedistribute;
                  }
                  else if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventStart);
                     return false;
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
               if( true == gi.IsPixieLoverInParty() )
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
               string patrolLeadName = "PatrolLead" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem patrolLead = new MapItem(patrolLeadName, 1.0, false, false, false, "c75MountedPatrolLead", "c75MountedPatrolLead", princeTerritory, 5, 6, 10);
               if (true == isEasyMonstersOption.IsEnabled)
                  patrolLead = new MapItem(patrolLeadName, 1.0, false, false, false, "c75MountedPatrolLead", "c75MountedPatrolLead", princeTerritory, 1, 1, 10);
               patrolLead.AddNewMount();
               gi.EncounteredMembers.Add(patrolLead);
               if (true == isEasyMonstersOption.IsEnabled)
                  dieRoll = 1;
               for (int i = 0; i < dieRoll; ++i)
               {
                  string patrolName = "Patrol" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem patrol = new MapItem(patrolName, 1.0, false, false, false, "c74MountedPatrol", "c74MountedPatrol", princeTerritory, 5, 6, 4);
                  if (true == isEasyMonstersOption.IsEnabled)
                     patrol = new MapItem(patrolName, 1.0, false, false, false, "c74MountedPatrol", "c74MountedPatrol", princeTerritory, 1, 1, 4);
                  patrol.AddNewMount();
                  gi.EncounteredMembers.Add(patrol);
               }
               gi.DieResults[key][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e084b":  // bear
               gi.EncounteredMembers.Clear();
               string bearName = "Bear" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem bear = new MapItem(bearName, 1.0, false, false, false, "c72Bear", "c72Bear", princeTerritory, 5, 5, 0);
               if (true == isEasyMonstersOption.IsEnabled)
                  bear = new MapItem(bearName, 1.0, false, false, false, "c72Bear", "c72Bear", princeTerritory, 1, 1, 0);
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
                     case 5: case 6:                                                                                           // nothing
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): EncounterEnd() returned false ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e094": // crocodiles in swamp
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == isEasyMonstersOption.IsEnabled)
                     dieRoll = 1;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EncounteredMembers.Clear();
                  for (int i = 0; i < gi.DieResults[key][0]; ++i)
                  {
                     string crocName = "Croc" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem crocodile = new MapItem(crocName, 1.0, false, false, false, "c73Crocodile", "c73Crocodile", princeTerritory, 6, 4, 0);
                     if (true == isEasyMonstersOption.IsEnabled)
                        crocodile = new MapItem(crocName, 1.0, false, false, false, "c73Crocodile", "c73Crocodile", princeTerritory, 1, 1, 0);
                     gi.EncounteredMembers.Add(crocodile);
                  }
                  gi.EventDisplayed = gi.EventActive = "e310";
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e094a": // crocodiles in river
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  if (true == isEasyMonstersOption.IsEnabled)
                     dieRoll = 1;
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.EncounteredMembers.Clear();
                  for (int i = 0; i < gi.DieResults[key][0]; ++i)
                  {
                     string crocName = "Croc" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem crocodile = new MapItem(crocName, 1.0, false, false, false, "c73Crocodile", "c73Crocodile", princeTerritory, 6, 4, 0);
                     if (true == isEasyMonstersOption.IsEnabled)
                        crocodile = new MapItem(crocName, 1.0, false, false, false, "c73Crocodile", "c73Crocodile", princeTerritory, 1, 1, 0);
                     gi.EncounteredMembers.Add(crocodile);
                  }
                  gi.EventDisplayed = gi.EventActive = "e307";
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e098": // Dragon
               IMapItem dragon = new MapItem("Dragon", 1.0, false, false, false, "c33Dragon", "c33Dragon", princeTerritory, 11, 10, 0);
               if (true == isEasyMonstersOption.IsEnabled)
                  dragon = new MapItem("Dragon", 1.0, false, false, false, "c33Dragon", "c33Dragon", princeTerritory, 1, 1, 0);
               gi.EncounteredMembers.Add(dragon);
               gi.DieResults[key][0] = dieRoll;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e099a": // Roc
            case "e099b": // Roc
               gi.EncounteredMembers.Clear();
               gi.EventStart = "e099";
               IMapItem roc = new MapItem("Roc", 1.0, false, false, false, "c55Roc", "c55Roc", princeTerritory, 8, 9, 10);
               if (true == isEasyMonstersOption.IsEnabled)
                  roc = new MapItem("Roc", 1.0, false, false, false, "c55Roc", "c55Roc", princeTerritory, 1, 1, 10);
               gi.EncounteredMembers.Add(roc);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e100a": // griffon
            case "e100b": // griffon
            case "e100c": // griffon
               gi.EventStart = "e100"; // assign for loot purposes
               gi.EncounteredMembers.Clear();
               string griffonName = "GriffonMount" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem griffon = new MapItem(griffonName, 1.0, false, false, false, "c63Griffon", "c63Griffon", princeTerritory, 6, 7, 12);
               if (true == isEasyMonstersOption.IsEnabled)
                  griffon = new MapItem(griffonName, 1.0, false, false, false, "c63Griffon", "c63Griffon", princeTerritory, 1, 1, 12);
               griffon.IsFlying = true;
               griffon.IsRiding = true;
               gi.EncounteredMembers.Add(griffon);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e105":
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e105a":
               gi.DieRollAction = GameAction.EncounterStart;
               if( Utilities.NO_RESULT == gi.DieResults[key][0] )
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
                  if (true == isEasyMonstersOption.IsEnabled)
                     dieRoll = 1;
                  for (int i = 0; i < dieRoll; ++i)
                  {
                     string hawkManName = "Hawkman" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem hawkman = new MapItem(hawkManName, 1.0, false, false, false, "c81Hawkman", "c81Hawkman", princeTerritory, 5, 7, 7);
                     if (true == isEasyMonstersOption.IsEnabled)
                        hawkman = new MapItem(hawkManName, 1.0, false, false, false, "c81Hawkman", "c81Hawkman", princeTerritory, 1, 1, 7);
                     hawkman.IsFlying = true;
                     hawkman.IsRiding = true;
                     gi.EncounteredMembers.Add(hawkman);
                  }
                  gi.EventDisplayed = gi.EventActive = "e310";
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
               }
               break;
            case "e112": // eagles - EncounterStart()
               gi.EncounteredMembers.Clear();
               ++dieRoll;
               if (true == isEasyMonstersOption.IsEnabled)
                  dieRoll = 1;
               gi.DieResults["e112"][0] = dieRoll;
               for (int i = 0; i < dieRoll; ++i)
               {
                  string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", princeTerritory, 3, 4, 1);
                  if (true == isEasyMonstersOption.IsEnabled)
                     eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", princeTerritory, 1, 1, 1);
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
               string giantName = "Giant" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem giant = new MapItem(giantName, 1.0, false, false, false, "c61Giant", "c61Giant", princeTerritory, 8, 9, 10);
               if (true == isEasyMonstersOption.IsEnabled)
                  giant = new MapItem(giantName, 1.0, false, false, false, "c61Giant", "c61Giant", princeTerritory, 1, 1, 10);
               gi.EncounteredMembers.Add(giant);
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e123b": // Black Knight
               gi.EncounteredMembers.Clear();
               string blackKnightName = "BlackKnight" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem blackKnight = new MapItem(blackKnightName, 1.0, false, false, false, "c80BlackKnight", "c80BlackKnight", princeTerritory, 8, 8, 30);
               if (true == isEasyMonstersOption.IsEnabled)
                  blackKnight = new MapItem(blackKnightName, 1.0, false, false, false, "c80BlackKnight", "c80BlackKnight", princeTerritory, 2, 8, 30);
               gi.EncounteredMembers.Add(blackKnight);
               gi.EventDisplayed = gi.EventActive = "e304";
               break;
            case "e124": // make raft
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e125": // Raft Overtuns
               if( false == gi.RemoveBelongingsInParty(false))
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
                  if (true == isEasyMonstersOption.IsEnabled)
                     gi.DieResults[key][0] = 2;
                  else
                     gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  gi.DieResults[key][1] = dieRoll;  // This die roll determines high lord
                  int highLordGuardNum = gi.DieResults[key][0] + 1;
                  for (int i = 0; i < highLordGuardNum; ++i)
                  {
                     string highLordGuardName = "Guard" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem guard = new MapItem(highLordGuardName, 1.0, false, false, false, "c66Guard", "c66Guard", princeTerritory, 5, 6, 4);
                     if (true == isEasyMonstersOption.IsEnabled)
                        guard = new MapItem(highLordGuardName, 1.0, false, false, false, "c66Guard", "c66Guard", princeTerritory, 1, 1, 4);
                     gi.EncounteredMembers.Add(guard);
                  }
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               break;
            case "e158": // hostile guards
               gi.EncounteredMembers.Clear();
               IMapItem hostileGuard1 = new MapItem("HostileGuard1", 1.0, false, false, false, "c50GuardHostile", "c50GuardHostile", princeTerritory, 6, 5, 7);
               IMapItem hostileGuard2 = new MapItem("HostileGuard2", 1.0, false, false, false, "c50GuardHostile", "c50GuardHostile", princeTerritory, 6, 5, 7);
               if (true == isEasyMonstersOption.IsEnabled)
               {
                  hostileGuard1 = new MapItem("HostileGuard1", 1.0, false, false, false, "c50GuardHostile", "c50GuardHostile", princeTerritory, 1, 1, 7);
                  hostileGuard2 = new MapItem("HostileGuard2", 1.0, false, false, false, "c50GuardHostile", "c50GuardHostile", princeTerritory, 1, 1, 7);
               }
               gi.EncounteredMembers.Add(hostileGuard1);
               gi.EncounteredMembers.Add(hostileGuard2);
               gi.EventDisplayed = gi.EventActive = "e307";
               //-----------------------------------------
               foreach (IMapItem mi in gi.PartyMembers) // Prince in Atrium fighting hostile guards alone
               {
                  if ("Prince" != mi.Name)
                     gi.LostPartyMembers.Add(mi);
               }
               gi.PartyMembers.Clear();
               gi.PartyMembers.Add(gi.Prince);
               break;
            case "e164": // giant lizard
               gi.EncounteredMembers.Clear();
               string lizardName = "Lizard" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem lizard = new MapItem(lizardName, 1.0, false, false, false, "c67Lizard", "c67Lizard", princeTerritory, 12, 10, 0);
               if (true == isEasyMonstersOption.IsEnabled)
                  lizard = new MapItem(lizardName, 1.0, false, false, false, "c67Lizard", "c67Lizard", princeTerritory, 1, 1, 0);
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
            default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): Reached default ae=" + gi.EventActive); return false;
         }
         return true;
      }
      //--------------------------------------------
      protected bool EncounterAbandon(IGameInstance gi, ref GameAction action)
      {
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
                  if ((false == mi.Name.Contains("Eagle")) && (false == mi.IsFlying) && (false == mi.IsRiding))
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
         IOption autoWealthOption = gi.Options.Find("AutoWealthRollForUnderFive");
         if (null == autoWealthOption)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): returned option=null");
            return false;
         }
         gi.RemoveKilledInParty(gi.EventActive);
         //----------------------------------------------
         string key = gi.EventStart;
         ITerritory princeTerritory = gi.Prince.Territory;
         switch (key) // End of combat always causes EncounterLootStart Action
         {
            case "e002a": // defeated mercenaries 
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e003": break; // swordman
            case "e004": // Mercenaries
            case "e005": // Amazons
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
            case "e007":  // Elf Warrior
            case "e007a": // Elf Warrior
            case "e007c": // Elf Warrior
            case "e007d": // Elf Warrior
            case "e007e": // Elf Warrior
               break;
            case "e008": // halfing warrior
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e011b": // farmer with protector
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e027": // defeated ghosts
               gi.PegasusTreasure = PegasusTreasureEnum.Mount;
               gi.CapturedWealthCodes.Add(110);
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
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
               gi.AddCoins(gi.Prince.Coin); // double coins - possesions and mounts are already doubled
               gi.HydraTeethCount = theNumHydraTeeth;
               theNumHydraTeeth = 0;
               gi.CapturedWealthCodes.Clear();
               break;
            case "e048": // fugitive 
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e049": // defeat minstrel
            case "e050": // defeat constalubary
            case "e051":
            case "e052":  // defeated goblins
            case "e054b": // defeated goblin keep - EncounterLootStart()
            case "e055":  // defeated orcs
            case "e056a": // defeated orc tower
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
               break;
            case "e073": // witch 
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e074": break;// defeated spiders
            case "e075b":  // defeated wolves
               if (false == SetCampfireFinalConditionState(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
                  return false;
               }
               return true; //<<<<<<<<<<<<<<<<<<<<<
            case "e076": break; // hunting cat
            case "e081": // mounted patrol
               if (true == autoWealthOption.IsEnabled)
               {
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
               if (false == SetCampfireFinalConditionState(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
            case "e099": // defeated roc
               gi.AddSpecialItem(SpecialEnum.RocBeak); // helps in Count Drogat
               break;
            case "e100": break; // griffon
            case "e108": break; // hawkmen 
            case "e112": // defeated eagles
               if (true == autoWealthOption.IsEnabled)  // automatically perform wealth code rolls if enabled.
               {
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
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
                  if (false == gi.StripCoins())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): gi.StripCoins()() returned false w/ es=" + gi.EventStart);
                     return false;
                  }
               }
               break;
            case "e142":
               gi.CapturedWealthCodes.Add(100);
               gi.CapturedWealthCodes.Add(100);
               break;
            case "e154e": // lords daughter
               gi.CapturedWealthCodes.Add(100);
               gi.CapturedWealthCodes.Add(100);
               break;
            case "e155": // high priest audience
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
                     string porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", princeTerritory, 0, 0, 0);
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
               gi.AddCoins(gi.LooterCoin, false); // Steal looter coin and return back to fold
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
               if (1 != gi.EncounteredMembers.Count)
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): invalid state gi.EncounteredMembers.Count=" + gi.EncounteredMembers.Count.ToString() + " w / es=" + gi.EventStart);
                  return false;
               }
               else if (false == gi.EncounteredMembers[0].IsKilled)
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
               action = GameAction.UpdateEventViewerActive;
               string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "LordsDaughter", princeTerritory, 0, 0, 0);
               gi.AddCompanion(trueLove);
               foreach (IMapItem e154Mi in gi.PartyMembers) // if no mount, add a mount
               {
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
         IOption isEasyMonstersOption = gi.Options.Find("EasyMonsters");
         if (null == isEasyMonstersOption)
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterLootStart(): returned option=null");
            return false;
         }
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
               if ((true == mi.IsRiding) || (true == mi.Name.Contains("Eagle")) || (true == mi.Name.Contains("Griffon")) )
                  ++numRiding;
            }
         }
         ITerritory princeTerritory = gi.Prince.Territory;
         //--------------------------------------------------------
         gi.DieRollAction = GameAction.DieRollActionNone;
         string key = gi.EventActive;
         switch (key)
         {
            case "e002b": // talk
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
                     gi.EventDisplayed = gi.EventActive = "e323";
                     break;
                  case 5:
                     gi.DieRollAction = GameAction.EncounterRoll;
                     gi.Bribe = 25;
                     if (true == gi.IsMerchantWithParty)
                        gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                     gi.EventDisplayed = gi.EventActive = "e323";
                     break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               for (int i = 0; i < 3; ++i)
                  gi.DieResults[key][i] = Utilities.NO_RESULT;
               break;
            case "e002c": // Evade
               if (true == gi.IsPartyRiding())
                  dieRoll += 1;
               switch (dieRoll) // Based on the die roll, implement the correct screen
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
               break;
            case "e002d": // fight mercenary
               switch (dieRoll) // Based on the die roll, implement the attack case
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e300"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e301"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e303"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e304"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e003a": // talk to swordsman
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
                     break; 
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               for (int i = 0; i < 3; ++i)
                  gi.DieResults[key][i] = Utilities.NO_RESULT;
               break;
            case "e003b": // evade to swordsman
               switch (dieRoll) // Based on the die roll, implement the correct screen
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
                     break;
                  case 5: case 6: gi.EventDisplayed = gi.EventActive = "e325"; break;                 // pass with dignity
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e003c": // fight swordsman
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the attack case
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e303"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e304"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e307"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e004a": // talk to mercenaries
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                  case 2: 
                     gi.EventDisplayed = gi.EventActive = "e332"; 
                     gi.Bribe = 10;
                     if (true == gi.IsMerchantWithParty)
                        gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
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
               break;
            case "e004b": // evade mercenaries
               foreach( IMapItem mi in gi.PartyMembers ) // if there is at least one mount in party, add one to evade
               {
                  if( 0 < mi.Mounts.Count)
                  {
                     dieRoll += 1;
                     break;
                  }
               }
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide party size                                        // escape
                  case 2: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                  case 3: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass charm
                  case 4: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                  case 5: gi.EventDisplayed = gi.EventActive = "e327"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass dummies
                  case 6: case 7:                                                                                           // escape mounted
                     if (0 == numRiding)                                 // no escape
                        gi.EventDisplayed = gi.EventActive = "e312b";
                     else if (numRiding < gi.PartyMembers.Count)         // partial escape
                        gi.EventDisplayed = gi.EventActive = "e312a";
                     else                                                // full escape
                        gi.EventDisplayed = gi.EventActive = "e312";
                     break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               gi.DieResults["e004"][0] = Utilities.NO_RESULT;
               break;
            case "e004c": // fight mercenaries
               switch (dieRoll)
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
               break;
            case "e005a": // talk to amazon
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
               break;
            case "e005b": // evade amazon
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e311"; break;                                               // escape
                  case 2: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape talking
                  case 3: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape talking
                  case 4: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide party size
                  case 5: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                  case 6: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               gi.DieResults["e005"][0] = Utilities.NO_RESULT;
               break;
            case "e005c": // fight amazon
               switch (dieRoll)
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
               break;
            case "e006b": // number of dwarf friends
               for (int i = 0; i < dieRoll; ++i)
               {
                  string dwarfFriendName = "Dwarf" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem dwarfFriend = new MapItem(dwarfFriendName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 6, 5, 12);
                  if (true == isEasyMonstersOption.IsEnabled)
                     dwarfFriend = new MapItem(dwarfFriendName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 1, 1, 12);
                  gi.EncounteredMembers.Add(dwarfFriend);
               }
               switch (gi.DwarvenChoice)
               {
                  case "Talk":  gi.EventDisplayed = gi.EventActive = "e006c"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case "Evade": gi.EventDisplayed = gi.EventActive = "e006d"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case "Fight": gi.EventDisplayed = gi.EventActive = "e006e"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default choice=" + gi.DwarvenChoice + " ae=" + gi.EventActive); return false;
               }
               break;
            case "e006c": // talk to dwarf warrior
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
               break;
            case "e006d": // evade dwarf warrior 
               if (gi.DieResults["e006a"][0] < 4) // if alone add one to die roll
                  ++dieRoll;
               switch (dieRoll)
               {
                  case 1:                                                                                                   // bribe to pass
                     gi.EventDisplayed = gi.EventActive = "e322"; 
                     gi.Bribe = 5; 
                     if (true == gi.IsMerchantWithParty)
                        gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5); 
                     break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass rough
                  case 3: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e006"; break; // escape talking
                  case 4: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e006"; break; // escape begging 
                  case 5: gi.EventDisplayed = gi.EventActive = "e311"; break;                                              // escape
                  case 6: gi.EventDisplayed = gi.EventActive = "e306"; gi.DieRollAction = GameAction.EncounterRoll; break; // attacked
                  case 7: gi.EventDisplayed = gi.EventActive = "e311"; break;                                              // escape
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               gi.DieResults["e006a"][0] = Utilities.NO_RESULT;
               break;
            case "e006e": // fight dwarf warrior
               if (gi.DieResults["e006a"][0] < 4) // if alone add one to die roll
                  ++dieRoll;
               switch (dieRoll)
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
               break;
            case "e006g": // Search for treasure
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
            case "e007c": // talk to elf warrior
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
               break;
            case "e007d": // evade elf warrior 
               if ("Forest" == princeTerritory.Type) // if in forest, add two
                  dieRoll += 2;
                switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e007"; break;  // hide quickly
                  case 2: gi.EventDisplayed = gi.EventActive = "e318"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e007"; break;  // hide
                  case 3: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e007"; break;  // escape talking
                  case 4: case 5: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; gi.EventStart = "e007"; break; // escape begging 
                  case 6: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass suspiciously
                  case 7: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass charm
                  case 8: gi.EventDisplayed = gi.EventActive = "e310"; break;                                               // surprised
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               gi.DieResults["e007a"][0] = Utilities.NO_RESULT;
               break;
            case "e007e": // fight elf warrior
               if ("Forest" == princeTerritory.Type) // if in forest, add two
                  dieRoll += 2;
               switch (dieRoll)
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
               break;
            case "e008a": // talk to halfling
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
                     ITerritory adjacent = FindRandomHexAdjacent(gi);
                     if( false == gi.HalflingTowns.Contains(adjacent) )
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
               break;
            case "e008b": // halfing gossip
               if (dieRoll < 4)
               {
                  gi.EventDisplayed = gi.EventActive = "e147"; // secret clue
                  gi.DieRollAction = GameAction.E147ClueToTreasure;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e162"; // secrets
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               break;
            case "e009a": // farm friendly approach
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
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
                        if( 0 < gi.MapItemMoves.Count )
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
                     case 11: case 12:
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
                  if (6 == gi.DieResults[key][0])
                  {
                     string miName = "FarmerBoy" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem farmBoy = new MapItem(miName, 1.0, false, false, false, "c35FarmerBoy", "c35FarmerBoy", princeTerritory, 4, 3, 0);
                     if (false == AddGuideTerritories(gi, farmBoy, 2))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddGuideTerritories() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
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
               if (gi.EncounteredMembers.Count < gi.PartyMembers.Count)
                  gi.EventDisplayed = gi.EventActive = "e014b";
               else
                  gi.EventDisplayed = gi.EventActive = "e307";
               gi.DieResults[key][0] = Utilities.NO_RESULT;
               break;
            case "e014c": // Hostile reapers
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               gi.DieResults[key][0] = Utilities.NO_RESULT;
               break;
            case "e015a": // Friendly reapers
               gi.EventDisplayed = gi.EventActive = "e342";
               gi.DieResults["e342"][0] = Utilities.NO_RESULT;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e015c": // Hostile reapers
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e016b": // Hostile Magician wins
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
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
               gi.DieResults[key][0] = dieRoll;
               if (false == gi.RemoveSpecialItem(SpecialEnum.ResistanceTalisman))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): RemoveSpecialItem(ResistanceTalisman) invalid state ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e017": // Peasant Mob
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e018a": // talk to priest
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: case 2: gi.EventDisplayed = gi.EventActive = "e336"; gi.DieRollAction = GameAction.EncounterRoll; break;  // please commrades - sympathic
                  case 3: gi.EventDisplayed = gi.EventActive = "e337"; gi.DieRollAction = GameAction.EncounterRoll; break; // please comrades - unsavory
                  case 4: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // converse
                  case 5: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                  case 6: gi.EventDisplayed = gi.EventActive = "e325";  break;  // pass with dignity
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               for (int i = 0; i < 3; ++i)
                  gi.DieResults[key][i] = Utilities.NO_RESULT;
               break;
            case "e018b": // fight priest
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the attack case
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e300"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e018c": // killed priest
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
               break;
            case "e019b": // evade hermit monk
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: case 2: case 3: gi.EventDisplayed = gi.EventActive = "e325"; break; // pass with dignity
                  case 4: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e310"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e022"; gi.DieRollAction = GameAction.EncounterStart; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e019c":  // fight hermit monk
               action = GameAction.UpdateEventViewerActive;
               gi.DieRollAction = GameAction.EncounterRoll;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e307"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e020a": // talk traveling monk
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
               break;
            case "e020b": // evade traveling monk
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: case 2: gi.EventDisplayed = gi.EventActive = "e325"; break; // pass with dignity
                  case 3: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e311"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e020c":  // fight traveling monk
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 3: case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e307"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e021": // Warrior Monks
               if (Utilities.NO_RESULT == gi.DieResults[key][1])
               {
                  gi.DieResults[key][1] = dieRoll;
                  if (3 < dieRoll) // if above 3, add mounts to each warrior monk
                  {
                     foreach (IMapItem mi in gi.EncounteredMembers)
                     {
                        string name = "Mount" + Utilities.MapItemNum.ToString();
                        ++Utilities.MapItemNum;
                        MapItem horse = new MapItem(name, 1.0, false, false, false, "MHorse", "", princeTerritory, 0, 0, 0);
                        mi.Mounts.Add(horse);
                     }
                  }
               }
               break;
            case "e021a": // talk to warrior monks
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
               break;
            case "e021b": // evade warrior monks
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e325"; break; // pass with dignity
                  case 2: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                  case 3: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                  case 5: gi.EventDisplayed = gi.EventActive = "e324"; gi.DieRollAction = GameAction.EncounterRoll; gi.Bribe = 10; break; // bribe to pass with their threat
                  case 6: gi.EventDisplayed = gi.EventActive = "e329"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass charm
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e021c": // evade warrior monks
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e301"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e302"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 5: case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e023a": // talk to wizard 
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
               break;
            case "e023b": // evade to wizard
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: case 2: gi.EventDisplayed = gi.EventActive = "e325"; break; // pass with dignity
                  case 3:                                                 // bribe to pass
                     gi.EventDisplayed = gi.EventActive = "e321";
                     gi.Bribe = 5;
                     if (true == gi.IsMerchantWithParty)
                        gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
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
               break;
            case "e023c": // fight wizard
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the attack case
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 3: case 4:
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
               break;
            case "e024": // wizard attack
               action = GameAction.UpdateEventViewerActive;
               if (gi.WitAndWile <= dieRoll)
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
               break;
            case "e024a": // wizard attack
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
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
                  action = GameAction.UpdateEventViewerActive;
                  gi.WizardAdviceLocations.Remove(princeTerritory);
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
                  {
                     case 1: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e027"; break;  // ancient treasure
                     case 2: gi.EventDisplayed = gi.EventActive = "e028"; break;                  // cave tombs
                     case 3: gi.EventDisplayed = gi.EventActive = "e029"; gi.DieRollAction = GameAction.EncounterRoll; break;  // danger and treasure 
                     case 4: case 5: gi.EventDisplayed = gi.EventActive = "e401"; break;          // nothing to see
                     case 6:
                        ITerritory adjacent = FindRandomHexAdjacent(gi);
                        gi.WizardAdviceLocations.Add(adjacent);
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                           return false;
                        }
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
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e030"; gi.AddCoins(1); break;                               // 1 gold with mummies
                  case 2: gi.EventDisplayed = gi.EventActive = "e031"; gi.DieRollAction = GameAction.EncounterRoll; break; // looted tomb
                  case 3: gi.EventDisplayed = gi.EventActive = "e032"; gi.DieRollAction = GameAction.EncounterStart; break; // ghosts
                  case 4: gi.EventDisplayed = gi.EventActive = "e033"; gi.DieRollAction = GameAction.EncounterStart; break; // warrior wraiths
                  case 5: gi.EventDisplayed = gi.EventActive = "e034"; break;                                               // spectre of the inner tomb
                  case 6: gi.EventDisplayed = gi.EventActive = "e029"; gi.DieRollAction = GameAction.EncounterRoll; break;  // danger and treasure
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e029": // danger and treasure
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
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
               break;
            case "e032a": // hidden altar 
               switch (dieRoll) // Based on the die roll, implement the attack case
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e037"; gi.DieRollAction = GameAction.EncounterRoll; break; // broken chest
                  case 2: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e039"; break;                              // Small Treasure Chest
                  case 3: gi.EventDisplayed = gi.EventActive = "e041"; gi.DieRollAction = GameAction.EncounterRoll; break; // Vision Gem
                  case 4: gi.EventDisplayed = gi.EventActive = "e042"; break; // Alcove of Sending
                  case 5:                                                     // High Altar
                     if (true == gi.IsReligionInParty())
                        gi.EventDisplayed = gi.EventActive = "e044";
                     else
                        gi.EventDisplayed = gi.EventActive = "e044a";
                     break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e033a": // defeated warrior wraiths
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
               ++gi.Prince.StarveDayNum;
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
               //-------------------------------------------
               if (gi.WanderingDayCount < dieRoll)
               {
                  ++gi.WanderingDayCount;
                  gi.EventDisplayed = gi.EventActive = "e035b"; // continue to wander
               }
               else
               {
                  gi.IsSpellBound = false;
                  if (false == Wakeup(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll: Wakeup() returned false ae=" + gi.EventActive);
                     return false;
                  }
               }
               break;
            case "e036a": // golem defeated
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
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
               break;
            case "e037": // broken chest
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, give the artifact
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e180"; gi.AddSpecialItem(SpecialEnum.HealingPoition); break; // healing potion
                  case 2: gi.EventDisplayed = gi.EventActive = "e181"; gi.AddSpecialItem(SpecialEnum.CurePoisonVial); break; // cure vial
                  case 3: gi.EventDisplayed = gi.EventActive = "e182"; gi.AddSpecialItem(SpecialEnum.GiftOfCharm); break; // lucky charm
                  case 4: gi.EventDisplayed = gi.EventActive = "e184"; gi.AddSpecialItem(SpecialEnum.ResistanceTalisman); break; // resistence talisman
                  case 5: gi.EventDisplayed = gi.EventActive = "e186"; gi.AddSpecialItem(SpecialEnum.MagicSword); break; // magic sword
                  case 6: gi.EventDisplayed = gi.EventActive = "e189"; gi.AddSpecialItem(SpecialEnum.CharismaTalisman); break; // charisma talisman
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e038": // cache under stone
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, give the artifact
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e180"; gi.AddSpecialItem(SpecialEnum.HealingPoition); break; // healing potion
                  case 2: gi.EventDisplayed = gi.EventActive = "e181"; gi.AddSpecialItem(SpecialEnum.CurePoisonVial); break; // cure vial
                  case 3: gi.EventDisplayed = gi.EventActive = "e182"; gi.AddSpecialItem(SpecialEnum.GiftOfCharm); break; // lucky charm
                  case 4: gi.EventDisplayed = gi.EventActive = "e185"; gi.AddSpecialItem(SpecialEnum.EnduranceSash); break; // sash
                  case 5: gi.EventDisplayed = gi.EventActive = "e187"; gi.AddSpecialItem(SpecialEnum.AntiPoisonAmulet); break; // anti-poison amulet
                  case 6: gi.EventDisplayed = gi.EventActive = "e190"; gi.AddSpecialItem(SpecialEnum.NerveGasBomb); break; // Nerve gas bomb
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e041": // vision gem
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e143"; gi.IsSecretTempleKnown = true; break; // Secret of Temples
                  case 2: gi.EventDisplayed = gi.EventActive = "e144"; gi.IsSecretBaronHuldra = true; break; // Secret of Baron Huldra
                  case 3: gi.EventDisplayed = gi.EventActive = "e145"; gi.IsSecretLadyAeravir = true; break; // Secret of Lady Aeravir
                  case 4: gi.EventDisplayed = gi.EventActive = "e146"; gi.IsSecretCountDrogat = true; break; // Secret of Count Drogat
                  case 5: gi.EventDisplayed = gi.EventActive = "e147"; gi.DieRollAction = GameAction.E147ClueToTreasure; break; // Clue to Treasure
                  case 6: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e045b":
               gi.DieResults["e045b"][0] = dieRoll;
               int remainingDays = Utilities.MaxDays - gi.Days;
               int daysToAdvance = Math.Min(dieRoll, remainingDays);
               gi.Days += daysToAdvance;
               action = GameAction.E045ArchOfTravel;
               break;
            case "e046a": // gateway to darkness - finished combat against guardians
               action = GameAction.UpdateEventViewerActive;
               int maxDaysToRemove = Math.Min(gi.Days, dieRoll);
               gi.Days -= maxDaysToRemove;
               gi.DieResults["e046a"][0] = dieRoll;
               break;
            case "e048a": // fugitive swordswoman
               if( 6 == dieRoll )
               {
                  gi.EncounteredMembers.Clear();
                  gi.EventDisplayed = gi.EventActive = "e048i";
                  string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "Swordswoman", princeTerritory, 7, 7, 4);
                  trueLove.IsFugitive = true;
                  trueLove.IsTownCastleTempleLeave = true;
                  gi.AddCompanion(trueLove);
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
               if (6 == dieRoll)
               {
                  gi.EncounteredMembers.Clear();
                  gi.EventDisplayed = gi.EventActive = "e048j";
                  string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "SlaveWoman", princeTerritory, 4, 2, 0);
                  trueLove.IsFugitive = true;
                  trueLove.IsTownCastleTempleLeave = true;
                  trueLove.IsGuide = true;
                  if (false == AddGuideTerritories(gi, trueLove, 5))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddGuideTerritories() returned false for ae=" + gi.EventActive);
                     return false;
                  }
                  gi.AddCompanion(trueLove);
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
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddGuideTerritories() returned false for ae=" + gi.EventActive);
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
               if( 0 == gi.EncounteredMembers.Count )
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): gi.EncounteredMembers.Count=0 for ae=" + gi.EventActive); 
                  return false;
               }
               if( 4 < dieRoll )
               {
                  IMapItem encountered = gi.EncounteredMembers[0];
                  encountered.AddNewMount();
               }
               gi.EventDisplayed = gi.EventActive = "e300";
               break;
            case "e048h": // fugitive deserter
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
                     break;                             
                  case 4:                                                                                                  // bribe to pass
                     gi.EventDisplayed = gi.EventActive = "e323"; 
                     gi.Bribe = 15;
                     if (true == gi.IsMerchantWithParty)
                        gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
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
               break;
            case "e050c": // Evade with Constabulary
               action = GameAction.UpdateEventViewerActive;
               int resulte050c = theConstableRollModifier + dieRoll;
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
               break;
            case "e050d": // Fight with Constabulary
               action = GameAction.UpdateEventViewerActive;
               int resulte050d = theConstableRollModifier + dieRoll;
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
               break;
            case "e052a": // following goblins
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
                              string elfName = "Elf" + i.ToString();
                              IMapItem elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 4, 5, 7);
                              if (true == isEasyMonstersOption.IsEnabled)
                                 elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 1, 1, 7);
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
                        Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "EncounterRoll(e053): gi.MapItemMoves.Clear()");
                        gi.MapItemMoves.Clear();
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
               gi.DieResults[key][0] = dieRoll;
               int start = gi.EncounteredMembers.Count;
               int end = 2 * gi.EncounteredMembers.Count;
               int maxCount = Math.Min(Utilities.MAX_GRID_ROW - 8, end); // cannot grow over number that can be shown
               for (int i = start; i < maxCount; ++i) // double number of goblins
               {
                  if ("e052" == gi.EventStart)
                  {
                     string miName = "Goblin" + i.ToString();
                     IMapItem goblin = new MapItem(miName, 1.0, false, false, false, "c22Goblin", "c22Goblin", princeTerritory, 3, 3, 1);
                     if (true == isEasyMonstersOption.IsEnabled)
                        goblin = new MapItem(miName, 1.0, false, false, false, "c22Goblin", "c22Goblin", princeTerritory, 1, 1, 1);
                     gi.EncounteredMembers.Add(goblin);
                  }
                  else if ("e055" == gi.EventStart)
                  {
                     string miName = "Orc" + i.ToString();
                     IMapItem orc = new MapItem(miName, 1.0, false, false, false, "c30Orc", "c30Orc", princeTerritory, 5, 4, 1);
                     if (true == isEasyMonstersOption.IsEnabled)
                        orc = new MapItem(miName, 1.0, false, false, false, "c30Orc", "c30Orc", princeTerritory, 1, 1, 1);
                     gi.EncounteredMembers.Add(orc);
                  }
                  else if ("e058a" == gi.EventStart)
                  {
                     string miName = "Dwarf" + i.ToString();
                     IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 6, 5, 10);
                     if (true == isEasyMonstersOption.IsEnabled)
                        dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 1, 1, 10);
                     gi.EncounteredMembers.Add(dwarf);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default double number ae=" + gi.EventActive + " es=" + gi.EventStart );
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
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e042"; break; // alcove of sending
                  case 2: gi.EventDisplayed = gi.EventActive = "e037"; gi.DieRollAction = GameAction.EncounterRoll; break; // broken chest
                  case 3: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break; // cache under stone
                  case 4: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e039"; break;                              // treasure chest
                  case 5: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                  case 6: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e053e": // campsite near magic
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
                  case 5:
                     if (true == gi.IsMagicInParty())
                        gi.EventDisplayed = gi.EventActive = "e045";
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
                        gi.EventDisplayed = gi.EventActive = "e045";
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
               if (null == gi.GoblinKeeps.Find(princeTerritory.Name))
                  gi.GoblinKeeps.Add(princeTerritory);
               gi.EventDisplayed = gi.EventActive = "e304"; // party attacks first
               break;
            case "e055a": // following orcs
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
               action = GameAction.UpdateEventViewerActive;
               gi.EventDisplayed = gi.EventActive = "e330";
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            case "e058c": // following dwarves
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
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                  case 2:                                                                                                   // bride to join - 30gp
                     gi.EventDisplayed = gi.EventActive = "e331"; 
                     gi.Bribe = 30;
                     if (true == gi.IsMerchantWithParty)
                        gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                     break; 
                  case 3:                                                                                                   // bride to hire
                     gi.EventDisplayed = gi.EventActive = "e332"; 
                     gi.Bribe = 10;
                     if (true == gi.IsMerchantWithParty)
                        gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                     break; 
                  case 4: gi.EventDisplayed = gi.EventActive = "e339"; break;                                               // convince to hire
                  case 5: gi.EventDisplayed = gi.EventActive = "e328"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass rough
                  case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;                                               // attacked
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               for (int i = 0; i < 3; ++i)
                  gi.DieResults[key][i] = Utilities.NO_RESULT;
               break;
            case "e058f": // evade dwarves
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e311"; break;                                               // escape
                  case 2: gi.EventDisplayed = gi.EventActive = "e314"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape talking
                  case 3: gi.EventDisplayed = gi.EventActive = "e315"; gi.DieRollAction = GameAction.EncounterRoll; break;  // escape begging
                  case 4: gi.EventDisplayed = gi.EventActive = "e317"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide quickly
                  case 5: gi.EventDisplayed = gi.EventActive = "e319"; gi.DieRollAction = GameAction.EncounterRoll; break;  // hide party size
                  case 6: gi.EventDisplayed = gi.EventActive = "e306"; break; // attacked
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e058g": // fight dwarves
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e058h": // Band of Dwarves
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  int numDwarves = gi.DieResults[key][0] + 1;
                  for (int i = 0; i < numDwarves; ++i)
                  {
                     string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 6, 5, 10);
                     if (true == isEasyMonstersOption.IsEnabled)
                        dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", princeTerritory, 1, 1, 10);
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
                  switch (gi.DieResults[key][0])
                  {
                     case 1: case 3: case 5: // raid approach
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
                     case 2: case 4: case 6: // friendly approach
                        gi.EventDisplayed = gi.EventActive = "e016a"; 
                        break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterStart(): reached default dieroll=" + dieRoll.ToString() + " ae=" + gi.EventActive); return false;
                  }
               }
               break;
            case "e071a": // talk to elves    
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
                  case 4: gi.EventDisplayed = gi.EventActive = "e325";  break;                      // pass with dignity
                  case 6: case 7: gi.EventDisplayed = gi.EventActive = "e309"; break;               // suprised
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               for (int i = 0; i < 3; ++i)
                  gi.DieResults[key][i] = Utilities.NO_RESULT;
               break;
            case "e071b": // evade elves
               if (true == gi.IsInMapItems("Elf"))
                  --dieRoll;
               if (true == gi.IsInMapItems("Dwarf"))
                  ++dieRoll;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 0: gi.EventDisplayed = gi.EventActive = "e325";  break;                               // pass with dignity
                  case 1: case 7:                                                                            // escape fly
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
               break;
            case "e071c": // fight elves
               if (true == gi.IsInMapItems("Elf"))
                  --dieRoll;
               if (true == gi.IsInMapItems("Dwarf"))
                  ++dieRoll;
               switch (dieRoll) // Based on the die roll, implement the correct screen
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
               break;
            case "e071d": // Band of Elves
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else
               {
                  int numElves = gi.DieResults[key][0] + 1;
                  for (int i = 0; i < numElves; ++i)
                  {
                     string elfName = "Elf" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 4, 5, 7);
                     if (true == isEasyMonstersOption.IsEnabled)
                        elf = new MapItem(elfName, 1.0, false, false, false, "c56Elf", "c56Elf", princeTerritory, 1, 1, 7);
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
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e165"; gi.DieRollAction = GameAction.EncounterRoll; break;   // town
                  case 2: gi.EventDisplayed = gi.EventActive = "e166"; gi.DieRollAction = GameAction.EncounterRoll; break;   // castle
                  default: gi.EventDisplayed = gi.EventActive = "e053"; gi.DieRollAction = GameAction.EncounterRoll; break; // campfire
               }
               break;
            case "e073c": // friendly witch
               switch (dieRoll)
               {
                  case 1: case 2: gi.EventDisplayed = gi.EventActive = "e334"; break;
                  case 3: case 4: gi.EventDisplayed = gi.EventActive = "e333"; break;
                  case 5: case 6: gi.EventDisplayed = gi.EventActive = "e195"; gi.DieRollAction = GameAction.EncounterRoll; break;  // roll for possessions
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e079a": // heavy rains
               gi.IsHeavyRainNextDay = false;  // rain is today - need EncounterEnd() to be called to end the day in ShowE079ColdCheckResult->ShowE079ColdCheckResult()
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.GamePhase = GamePhase.SunriseChoice;      // e079a - Finish Heavy Rains
               gi.DieResults[key][0] = dieRoll;
               if( 3 < dieRoll )
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
                  switch (gi.DieResults[key][0]) 
                  {
                     case 1:
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     case 2: gi.EventDisplayed = gi.EventActive = "e025b"; gi.DieRollAction = GameAction.E080PixieAdvice; break; // pixie advice
                     case 3: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break;  // stone slabe
                     case 4: case 5: gi.EventDisplayed = gi.EventActive = "e195"; gi.DieRollAction = GameAction.EncounterRoll; break; // roll for possessions
                     case 6: gi.EventDisplayed = gi.EventActive = "e188"; gi.AddNewMountToParty(MountEnum.Pegasus); break; // pegasus mount
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
                  }
               }
               else
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e081a": // talk to mounted patrol 
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
                     break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;  // attacked
                  case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;  // surprised
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               for (int i = 0; i < 3; ++i)
                  gi.DieResults[key][i] = Utilities.NO_RESULT;
               break;
            case "e081b": // evade mounted patrol
               switch (dieRoll)
               {
                  case 1:case 2:                                                                                            // escape mounted
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
               break;
            case "e081c": // fight mounted patrol
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e083a": // roasted boar
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
                     action = GameAction.EndGameLost;
                     gi.GamePhase = GamePhase.EndGame;
                  }
                  else if ( (true==isMemberIncapacited) || (8 < gi.DieResults[key][0]) ) // if anybody is incapacitied or the mounts are lost, redistribute belongings
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
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.GamePhase = GamePhase.SunriseChoice;      // e092a - Finish Flood
               gi.DieResults[key][0] = dieRoll;
               if (dieRoll < 5 )
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
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
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
               break;
            case "e098b": // fight dragon
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e308"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e309"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e099a": // evade roc
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
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
               break;
            case "e099b": // fight roc
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e301"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e100a": // talk to griffon
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
               break;
            case "e100b": // evade to griffon
               switch (dieRoll) // Based on the die roll, implement the correct screen
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
               break;
            case "e100c": // fight griffon
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the attack case
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e303"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e308"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e105": // storm clouds
               action = GameAction.UpdateEventViewerActive;
               switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e103"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e102"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e079"; break;
                  case 4:  case 5:
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
            case "e112a": // follow eagles
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
                     string eagleHelpName = "EagleHelp" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem eagleHelp = new MapItem(eagleHelpName, 1.0, false, false, false, "c62Eagle", "c62Eagle", princeTerritory, 3, 4, 1);
                     eagleHelp.IsGuide = true;
                     eagleHelp.GuideTerritories = gi.Territories;
                     eagleHelp.IsTownCastleTempleLeave = true;
                     eagleHelp.IsFlying = true;
                     eagleHelp.IsRiding = true;
                     gi.AddCompanion(eagleHelp);
                     break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e117"; gi.DieRollAction = GameAction.EncounterRoll; break;  // eagle allies
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e112b": // evade eagles 
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: case 2: gi.EventDisplayed = gi.EventActive = "e325"; break;                                               // pass with dignity
                  case 3: case 4: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;  // pass suspiciously
                  case 5: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // conversation  
                  case 6: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry  
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e112c": // fight eagles
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e304"; break;  // attack
                  case 2: gi.EventDisplayed = gi.EventActive = "e305"; break;  // attack
                  case 3: gi.EventDisplayed = gi.EventActive = "e306"; break;  // attacked
                  case 4: gi.EventDisplayed = gi.EventActive = "e306"; break;  // attacked
                  case 5: gi.EventDisplayed = gi.EventActive = "e308"; break;  // suprised
                  case 6: gi.EventDisplayed = gi.EventActive = "e309"; break;  // suprised
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e113": // eagle ambush
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  for (int i = 0; i < gi.DieResults[key][0]; ++i) // additional eagles arrive
                  {
                     string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", princeTerritory, 3, 4, 1);
                     if (true == isEasyMonstersOption.IsEnabled)
                        eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", princeTerritory, 1, 1, 1);
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
                  gi.IsPartyFed = true;
                  gi.IsMountsFed = true;
                  if (false == gi.AddCoins(50))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins() return false for ae=" + gi.EventActive);
                     return false;
                  }
                  for (int i = 0; i < gi.DieResults[key][0]; ++i) // additional eagles arrive
                  {
                     string eagleAllyName = "EagleAlly" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem eagleAlly = new MapItem(eagleAllyName, 1.0, false, false, false, "c62Eagle", "c62Eagle", princeTerritory, 3, 4, 1);
                     eagleAlly.IsGuide = true;
                     eagleAlly.GuideTerritories = gi.Territories;
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
               break;
            case "e118b": // evade to giant
               switch (dieRoll) // Based on the die roll, implement the correct screen
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
               break;
            case "e118c": // fight giant
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the attack case
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e301"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e302"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e124": // make raft
               action = GameAction.UpdateEventViewerActive;
               switch (gi.DieResults[key][0]) // Based on the die roll, implement the attack case
               {
                  case 1:case 2:
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                     break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e094a"; gi.DieRollAction = GameAction.EncounterStart;  break;  // Crocs in River
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
                  if (GamePhase.Travel == gi.GamePhase)
                     gi.SunriseChoice = GamePhase.Encounter; // if talk with merchant, no more travel
                  switch (gi.DieResults[key][0])
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e128a"; break; // paasus sale
                     case 3: gi.EventDisplayed = gi.EventActive = "e028"; break;  // cave tombs
                     case 4: gi.EventDisplayed = gi.EventActive = "e128b"; break; // cure-poison sale
                     case 5: gi.EventDisplayed = gi.EventActive = "e009"; gi.DieRollAction = GameAction.EncounterRoll; break;  // farms
                     case 6: gi.EventDisplayed = gi.EventActive = "e128c"; break; // food for sale
                     case 7: gi.EventDisplayed = gi.EventActive = "e128d"; gi.DieRollAction = GameAction.EncounterRoll; break;  // merchant outwits you
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
               if (gi.WitAndWile < dieRoll)
               {
                  int maxLoss = Math.Min(gi.GetCoins(), 10);
                  gi.ReduceCoins(maxLoss);
               }
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false for ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                  return false;
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
                        ITerritory t = FindRandomHexAdjacent(gi);
                        if (null == t)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): FindRandomHexAdjacent() returned null");
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
            case "e130a": // talk to high lord 
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
               break;
            case "e130b": // evade high lord 
               switch (dieRoll)
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
               break;
            case "e130c": // fight high lord
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e302"; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e303"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e304"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e305"; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e306"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
               }
               break;
            case "e130e": // Audience with high lord
               switch (gi.DieResults["e130"][1])
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e130f"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 2: gi.EventDisplayed = gi.EventActive = "e161"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e160"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e155"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e156"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e156"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default dr=" + gi.DieResults["e130"][1].ToString() + " ae=" + gi.EventActive); return false;
               }
               break;
            case "e130f": // Audience with Baron
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll; 
               }
               else
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 1:case 2:
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false for ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                           return false;
                        }
                        break;
                     case 3: case 4: gi.EventDisplayed = gi.EventActive = "e150"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 5: gi.EventDisplayed = gi.EventActive = "e151"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     case 6: gi.EventDisplayed = gi.EventActive = "e152"; gi.DieRollAction = GameAction.EncounterRoll; break;
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default dr=" + gi.DieResults["e130"][1].ToString() + " ae=" + gi.EventActive); return false;
                  }
               }
               break;
            // ========================Search Ruin Results================================
            case "e132":
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
            case "e133": action = GameAction.E133Plague; gi.DieRollAction = GameAction.DieRollActionNone; break;
            case "e135": // broken columns in ruins
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the attack case
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
                  case 4:                                                     // arch of travel
                     if (true == gi.IsMagicInParty())
                        gi.EventDisplayed = gi.EventActive = "e045";
                     else
                        gi.EventDisplayed = gi.EventActive = "e045a";
                     break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e046"; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e047"; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e136": // hidden treasures
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, the proper hidden treasures
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
                  case 5: gi.EventDisplayed = gi.EventActive = "e500"; break;  // Horde of 500 gp
                  case 6:
                     if (false == EncounterEnd(gi, ref action))                // nothing found
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false for ae=e136 dr=5");
                        return false;
                     }
                     break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e137": // inhabitants
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement event
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
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e138": // unclean creatures
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement event
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
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e139": // minor treasures
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement event
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
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e140":  // magic box
            case "e140b": // magic box
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement event
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e141"; gi.DieRollAction = GameAction.EncounterRoll; break;                               // hydra teeth
                  case 2: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e142"; break;                                                            // gems
                  case 3: gi.CapturedWealthCodes.Add(60); action = GameAction.EncounterLoot; gi.PegasusTreasure = PegasusTreasureEnum.Talisman; break;   // treasure
                  case 4: gi.CapturedWealthCodes.Add(110); action = GameAction.EncounterLoot; gi.PegasusTreasure = PegasusTreasureEnum.Talisman; break;  // treasure 
                  case 5: gi.EventDisplayed = gi.EventActive = "e195"; gi.DieRollAction = GameAction.EncounterRoll; break;                               // roll for possessions
                  case 6: gi.EventDisplayed = gi.EventActive = "e401"; break;                                                                            // nothing
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e141": // hydra teeth
               gi.AddSpecialItem(SpecialEnum.HydraTeeth);
               gi.HydraTeethCount += dieRoll;
               if (false == EncounterEnd(gi, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false w/ ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e147a": // found treasure from clues
               action = GameAction.UpdateEventViewerActive;
               gi.SecretClues.Remove(princeTerritory);
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {

                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e066"; gi.DieRollAction = GameAction.EncounterRoll; break;   // hidden temple
                     case 3: gi.EventDisplayed = gi.EventActive = "e037"; gi.DieRollAction = GameAction.EncounterRoll; break;   // broken chest
                     case 4: gi.EventDisplayed = gi.EventActive = "e038"; gi.DieRollAction = GameAction.EncounterRoll; break;   // cache under stone
                     case 5: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e039"; break;                                // Small Treasure Chest        
                     case 6: gi.EventDisplayed = gi.EventActive = "e040"; gi.DieRollAction = GameAction.EncounterStart; break;  // treasure chest
                     case 7: gi.EventDisplayed = gi.EventActive = "e030"; gi.AddCoins(1); break;                                // 1 gold with mummies
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
               gi.DieResults[key][0] = dieRoll;
               gi.Bribe = dieRoll * 10;
               if (true == gi.IsMerchantWithParty)
                  gi.Bribe = (int) ((double)gi.Bribe * 0.5);
               break;
            case "e151": // lord finds favor
               gi.DieResults[key][0] = dieRoll;
               if (false == gi.AddCoins(dieRoll * 100))
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): AddCoins() returned false for ae=" + gi.EventActive);
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
                  string cavalryName = "Cavalry" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem cavalry = new MapItem(cavalryName, 1.0, false, false, false, "Cavalry", "Cavalry", princeTerritory, 0, 0, 0);
                  cavalry.IsGuide = true;
                  if (false == AddGuideTerritories(gi, cavalry, 3))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddGuideTerritories() returned false ae=" + action.ToString() + " dr=" + dieRoll.ToString());
                     return false;
                  }
                  gi.AddCompanion(cavalry);
               }
               break;
            // ========================Miscellaneous Events================================
            case "e154": // meet lords daughter
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
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
                  action = GameAction.UpdateEventViewerActive;
                  switch (gi.DieResults[key][0]) // Based on the die roll, implement event
                  {
                     case 1: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; break;         // arrested
                     case 2: gi.EventDisplayed = gi.EventActive = "e156a"; break;                                                     // stone faced
                     case 3: gi.EventDisplayed = gi.EventActive = "e156b"; break;                                                     // lodging and food   
                     case 4: gi.EventDisplayed = gi.EventActive = "e156c"; break;                                                     // favor
                     case 5: gi.EventDisplayed = gi.EventActive = "e156d"; break;                                                     // interest
                     case 6:                                                                                                          // religious interest
                        if (true == gi.IsReligionInParty())
                        {
                           action = GameAction.E156MayorTerritorySelection;
                           gi.EventDisplayed = gi.EventActive = "e156e";
                           string trustedAssistantName = "TrustedAssistant" + Utilities.MapItemNum.ToString();
                           ++Utilities.MapItemNum;
                           IMapItem trustedAssistant = new MapItem(trustedAssistantName, 1.0, false, false, false, "c51TrustedAssistant", "c51TrustedAssistant", princeTerritory, 4, 4, 0);
                           gi.AddCompanion(trustedAssistant);
                           ITerritory t156a = FindClosestTown(gi);
                           gi.ForbiddenAudiences.AddAssistantConstraint(t156a, trustedAssistant);
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e156f";
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
            case "e160e": // audience with lady
               gi.DieResults[key][0] = dieRoll;
               break;
            case "e160f":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  gi.AddCoins(gi.DieResults[key][0] * 150);
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
                     e160Mi.HealWounds(wound, poision);
                     if (0 == e160Mi.Mounts.Count)  // add mount if do not have one
                        e160Mi.AddNewMount();
                  }
                  //-----------------------------------------------
                  gi.AddNewMountToParty(); // add a spare pack horse
                  for (int k = 0; k < 3; ++k) // add three knights
                  {
                     string knightName = "Knight" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     IMapItem knight = new MapItem(knightName, 1.0, false, false, false, "c52Knight", "c52Knight", princeTerritory, 6, 7, 0);
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
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e161": // audience with count drogat
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
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
                     case 6: gi.EventDisplayed = gi.EventActive = "e161f"; break;        // noble ally
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
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: case 2: gi.EventDisplayed = gi.EventActive = "e143"; break;
                  case 3: case 4: gi.EventDisplayed = gi.EventActive = "e144"; gi.IsSecretBaronHuldra = true; break;
                  case 5: gi.EventDisplayed = gi.EventActive = "e145"; gi.IsSecretLadyAeravir = true; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e146"; gi.IsSecretCountDrogat = true; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive); return false;
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
                  gi.DieResults[key][2] = dieRoll;
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive);
                  return false;
               }
               break;
            case "e163c": //buy slave girls
               gi.DieResults[key][0] = dieRoll;
               if (12 == dieRoll)
               {
                  string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  string imageSource = "SlaveGirlFace" + gi.SlaveGirlIndex.ToString();
                  IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", imageSource, princeTerritory, 0, 0, 0);
                  gi.AddCompanion(trueLove);
                  action = GameAction.E228ShowTrueLove;
               }
               else
               {
                  string slaveGirlName = "SlaveGirl" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  IMapItem slaveGirl = new MapItem(slaveGirlName, 1.0, false, false, false, "c41SlaveGirl", "c41SlaveGirl", princeTerritory, 0, 0, 0);
                  gi.AddCompanion(slaveGirl);
               }
               --gi.PurchasedSlaveGirl;
               break;
            case "e163d": //buy warrior
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults["e163d"][0] = dieRoll;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               else if (Utilities.NO_RESULT == gi.DieResults[key][1])
               {
                  gi.DieResults[key][1] = dieRoll;
                  string warriorName = "Warrior" + Utilities.MapItemNum.ToString();
                  ++Utilities.MapItemNum;
                  int endurance = gi.DieResults[key][0];
                  IMapItem oldWarrior = new MapItem(warriorName, 1.0, false, false, false, "c43OldWarrior", "c43OldWarrior", princeTerritory, endurance, dieRoll, 0);
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
               action = GameAction.UpdateEventViewerActive;
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
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll)
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
                     case 3: gi.EventDisplayed = gi.EventActive = "e209b"; gi.AddCoins(50); break;
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
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false w/ ae=" + gi.EventActive);
                           return false;
                        }
                        break;
                     case 8:
                        gi.Prince.Coin /= 2;
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
                  if( true == gi.IsSeekNewModifier )
                     dieRoll += 1;
                  if (4 < gi.WitAndWile)
                     dieRoll += 1;
                  if (true == gi.HalflingTowns.Contains(princeTerritory))
                  {
                     if ( (true == gi.KilledLocations.Contains(princeTerritory)) || (true == gi.EscapedLocations.Contains(princeTerritory)) )
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
               if (false == gi.AddCoins(180))
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
               gi.ReduceCoins(10);
               switch (dieRoll)
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e401"; break; // nothing
                  case 2: gi.EventDisplayed = gi.EventActive = "e147"; gi.DieRollAction = GameAction.E147ClueToTreasure; break; // Clue to Treasure
                  case 3: gi.EventDisplayed = gi.EventActive = "e143"; gi.IsSecretTempleKnown = true; break; // Secret of Temples
                  case 4: gi.EventDisplayed = gi.EventActive = "e144"; gi.IsSecretBaronHuldra = true; break; // Secret of Baron Huldra
                  case 5: gi.EventDisplayed = gi.EventActive = "e145"; gi.IsSecretLadyAeravir = true; break; // Secret of Lady Aeravir
                  case 6: gi.EventDisplayed = gi.EventActive = "e146"; gi.IsSecretCountDrogat = true; break; // Secret of Count Drogat
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            // ========================See Hire================================
            case "e210":
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
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
            case "e211a": // see audience in town
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 2: ThrownInDungeon(gi); break;                                                                       // thrown in dungeon
                     case 3: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; break;  // arrested
                     case 4: gi.EventDisplayed = gi.EventActive = "e158"; break;                                               // hostile guards
                     case 5: gi.EventDisplayed = gi.EventActive = "e153"; break;                                               // master of house hold
                     case 6:
                     case 7:
                     case 8:                                                                                   // do nothing
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + gi.DieResults["e211a"][0].ToString());
                           return false;
                        }
                        break;
                     case 9: case 10: gi.EventDisplayed = gi.EventActive = "e156"; gi.DieRollAction = GameAction.EncounterRoll; break; // audience permitted
                     case 11: gi.EventDisplayed = gi.EventActive = "e154"; gi.DieRollAction = GameAction.EncounterRoll; break; // mayor's daughter
                     case 12: case 13: case 14: case 15: case 16: gi.EventDisplayed = gi.EventActive = "e156"; gi.DieRollAction = GameAction.EncounterRoll; break; //  audience permitted
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               else
               {
                  if (true == gi.FeelAtHomes.Contains(princeTerritory))
                     ++dieRoll;
                  //--------------------------------
                  dieRoll += gi.DaughterRollModifier;
                  gi.DaughterRollModifier = 0;
                  //--------------------------------
                  if ((true == gi.HalflingTowns.Contains(gi.Prince.Territory)) && ((true == gi.KilledLocations.Contains(gi.Prince.Territory)) || (true == gi.EscapedLocations.Contains(gi.Prince.Territory))) )
                     --dieRoll;
                  //--------------------------------
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e211b":
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
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
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e155";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e153"; // master of household
                        }
                        break;
                     case 9:                                                                                                   // pay your respect
                        gi.EventDisplayed = gi.EventActive = "e150";
                        if (false == gi.AddCoins(50))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, gi.Days + 1);
                        break;
                     case 10: gi.EventDisplayed = gi.EventActive = "e159"; gi.ForbiddenAudiences.AddPurifyConstaint(princeTerritory); break;   // purify self
                     default: gi.EventStart = gi.EventDisplayed = gi.EventActive = "e155"; gi.DieRollAction = GameAction.EncounterRoll; break; //  audience permitted
                  }
               }
               else
               {
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
                  foreach (ITerritory t in letters)
                     gi.LetterOfRecommendations.Remove(t);
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
            case "e211c":
            case "e211f":
               action = GameAction.UpdateEventViewerActive;
               int resultOfDie = gi.DieResults[key][0];
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
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
                     case 3: gi.EventDisplayed = gi.EventActive = "e154"; gi.DieRollAction = GameAction.EncounterRoll; break;                      // meet king's daughter
                     case 4: gi.EventDisplayed = gi.EventActive = "e149"; gi.ForbiddenAudiences.AddClothesConstraint(princeTerritory); break;  // learn court manners
                     case 5: gi.EventDisplayed = gi.EventActive = "e158"; break;                                                                   // hostile guards
                     case 6:
                     case 7:                                                                                                               // nothing happens
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
                        gi.EventDisplayed = gi.EventActive = "e150";
                        if (false == gi.AddCoins(50))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): AddCoins() return false for ae=" + gi.EventActive);
                           return false;
                        }
                        gi.ForbiddenAudiences.AddTimeConstraint(princeTerritory, gi.Days + 1);
                        break;
                     case 12: gi.EventDisplayed = gi.EventActive = "e151"; gi.DieRollAction = GameAction.EncounterRoll; break;                     // find favor
                     default: gi.EventDisplayed = gi.EventActive = "e152"; gi.IsNobleAlly = true; break;                    // ally                       
                  }
               }
               else
               {
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  foreach (ITerritory t in letters)
                     gi.LetterOfRecommendations.Remove(t);
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
            case "e211d":
               action = GameAction.UpdateEventViewerActive;
               int resultOfDie1 = gi.DieResults["e211d"][0];
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 2:                                                                                                                        // next victim                                                               
                        if (false == MarkedForDeath(gi))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): MarkedForDeath() returned false ae=" + gi.EventActive + " dr=" + resultOfDie1.ToString());
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
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): RemoveSpecialItem() returned false ae=" + action.ToString() + " dr=" + resultOfDie1.ToString());
                              return false;
                           }
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
                     case 9: gi.EventDisplayed = gi.EventActive = "e149"; gi.ForbiddenAudiences.AddClothesConstraint(princeTerritory); break;  // learn court manners
                     case 10: gi.EventDisplayed = gi.EventActive = "e151"; gi.DieRollAction = GameAction.EncounterRoll; break;                     // find favor
                     default: gi.EventDisplayed = gi.EventActive = "e161"; gi.DieRollAction = GameAction.EncounterRoll; break;                     // gain audience
                  }
               }
               else
               {
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  foreach (ITerritory t in letters)
                     gi.LetterOfRecommendations.Remove(t);
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
            case "e211e":
               action = GameAction.UpdateEventViewerActive;
               int resultOfDie2 = gi.DieResults["e211e"][0];
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  switch (gi.DieResults[key][0])
                  {
                     case 2: gi.EventDisplayed = gi.EventActive = "e060"; gi.DieRollAction = GameAction.EncounterRoll; break;                      // arrested
                     case 3: gi.EventDisplayed = gi.EventActive = "e159"; gi.ForbiddenAudiences.AddPurifyConstaint(princeTerritory); break;        // purify self
                     case 4: gi.EventDisplayed = gi.EventActive = "e158"; break;                                                                   // hostile guards
                     case 5: gi.EventDisplayed = gi.EventActive = "e149"; gi.ForbiddenAudiences.AddClothesConstraint(princeTerritory); break;  // learn court manners
                     case 6: gi.EventDisplayed = gi.EventActive = "e153"; break;                                                                   // master of household
                     case 7:                                                                                                                       // audience if have griffon claw   
                        if (true == gi.IsSpecialItemHeld(SpecialEnum.GriffonClaws))
                        {
                           if (false == gi.RemoveSpecialItem(SpecialEnum.GriffonClaws))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): RemoveSpecialItem() returned false ae=" + action.ToString() + " dr=" + resultOfDie2.ToString());
                              return false;
                           }
                           gi.EventStart = gi.EventDisplayed = gi.EventActive = "e160";
                           gi.DieRollAction = GameAction.EncounterRoll;
                        }
                        else
                        {
                           if (false == EncounterEnd(gi, ref action))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + resultOfDie2.ToString());
                              return false;
                           }
                        }
                        break;
                     case 8:                                                                                                                        // do nothing   
                        if (false == EncounterEnd(gi, ref action))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() returned false ae=" + action.ToString() + " dr=" + resultOfDie2.ToString());
                           return false;
                        }
                        break;
                     case 9:                                                                                                                      // seneschal requires a bribe
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
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
                     case 10: gi.EventDisplayed = gi.EventActive = "e160"; gi.DieRollAction = GameAction.EncounterRoll; break;                     // gain audience
                     case 11: gi.EventDisplayed = gi.EventActive = "e154"; gi.DieRollAction = GameAction.EncounterRoll; break;                     // meet lord's daughter
                     default: gi.EventDisplayed = gi.EventActive = "e160"; gi.DieRollAction = GameAction.EncounterRoll; break;                     // gain audience
                  }
               }
               else
               {
                  List<ITerritory> letters = new List<ITerritory>();
                  foreach (ITerritory t in gi.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letters.Add(t);
                  }
                  dieRoll += (letters.Count * 2);
                  foreach (ITerritory t in letters)
                     gi.LetterOfRecommendations.Remove(t);
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
                  gi.DieResults[key][0] = dieRoll;
               }
               break;
            case "e212":
               action = GameAction.UpdateEventViewerActive;
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
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
                        string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
                        ++Utilities.MapItemNum;
                        IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "LordsDaughter", princeTerritory, 4, 2, 0);
                        gi.AddCompanion(trueLove);
                        break;
                     case 11: gi.EventDisplayed = gi.EventActive = "e212j"; break;
                     case 12: gi.EventDisplayed = gi.EventActive = "e212k"; break;
                     default: gi.EventDisplayed = gi.EventActive = "e212m"; break;
                  }
               }
               else
               {
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
                  foreach (ITerritory t in letters)
                     gi.LetterOfRecommendations.Remove(t);
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
            case "e212l": //learn secrets
               action = GameAction.UpdateEventViewerActive;
               switch (dieRoll) // Based on the die roll, implement the correct screen
               {
                  case 1: case 2: gi.EventDisplayed = gi.EventActive = "e144"; gi.IsSecretBaronHuldra = true; break;
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
                           gi.AddCoins(targetCache.Coin, false);
                           gi.Caches.Remove(targetCache);
                           break;
                        case 5:
                           break;
                        case 6:
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
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll <= gi.WitAndWile)
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
               break;
            case "e315": // escape hard
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll < gi.WitAndWile)
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
               break;
            // ========================Hiding================================
            case "e317": // Hiding
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll <= gi.WitAndWile)
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
               break;
            case "e318": // Hiding
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll < gi.WitAndWile)
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
               break;
            case "e319": // Hiding
               if (gi.PartyMembers.Count <= dieRoll)
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
               break;
            case "e320": // Hiding
               if (gi.PartyMembers.Count < dieRoll)
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
               break;
            // ========================Bribe================================
            case "e321": action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; theCombatModifer = 1; break;
            case "e322": action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
            case "e323": action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; theCombatModifer = -1; break;
            case "e324": action = GameAction.UpdateEventViewerActive; gi.EventDisplayed = gi.EventActive = "e307"; break;
            // ========================Pass================================
            case "e326": // Pass encounter
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll <= gi.WitAndWile)
               {
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
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
               break;
            case "e327": // Pass encounter
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll <= gi.WitAndWile)
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
               break;
            case "e328": // Pass encounter
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll < gi.WitAndWile)
               {
                  if (false == EncounterEnd(gi, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEncounter.PerformAction(): EncounterEnd() returned false for ae=" + gi.EventActive + " dr=" + dieRoll.ToString());
                     return false;
                  }
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e330";
                  action = GameAction.UpdateEventViewerActive;
                  gi.DieRollAction = GameAction.EncounterRoll;
               }
               break;
            case "e329": // Pass encounter
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll < gi.WitAndWile)
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
               break;
            // ========================Roll for Fight================================
            case "e330":
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
               switch (dieRoll) // Based on the die roll, implement event
               {
                  case 1: case 2: case 3: gi.EventDisplayed = gi.EventActive = "e401"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e321"; break; // bribe to pass
                  case 5: gi.EventDisplayed = gi.EventActive = "e322"; break; // bribe to pass 
                  case 6: gi.EventDisplayed = gi.EventActive = "e323"; break; // bribe to pass
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e332a": // failed to bribe to hire
               switch (dieRoll) // Based on the die roll, implement event
               {
                  case 1: case 2: gi.EventDisplayed = gi.EventActive = "e401"; break;
                  case 3: gi.EventDisplayed = gi.EventActive = "e321"; break;
                  case 4: gi.EventDisplayed = gi.EventActive = "e322"; break; // bribe to pass
                  case 5: gi.EventDisplayed = gi.EventActive = "e323"; break; // bribe to pass
                  case 6: gi.EventDisplayed = gi.EventActive = "e324"; gi.DieRollAction = GameAction.EncounterRoll; break; // bribe to pass with their threat
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e333a": // failed to hire 
               switch (dieRoll) // Based on the die roll, implement event
               {
                  case 1: case 2: case 3: case 4: gi.EventDisplayed = gi.EventActive = "e325"; break;                      // pass with dignity
                  case 5: gi.EventDisplayed = gi.EventActive = "e326"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  case 6: gi.EventDisplayed = gi.EventActive = "e327"; gi.DieRollAction = GameAction.EncounterRoll; break; // pass dummies
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e336": // Plead Comrades
               if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
               {
                  action = GameAction.E018MarkOfCain;
               }
               else
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  if (dieRoll <= (gi.WitAndWile + gi.MonkPleadModifier))
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
               break;
            case "e337": // Plead Comrades
               if ((true == gi.IsMarkOfCain) && (true == gi.IsReligionInParty(gi.EncounteredMembers)))
               {
                  action = GameAction.E018MarkOfCain;
               }
               else
               {
                  if (true == gi.IsCharismaTalismanActive)
                     --dieRoll;
                  if (true == gi.IsElfWitAndWileActive)
                     ++dieRoll;
                  if (dieRoll <= (gi.WitAndWile + gi.MonkPleadModifier))
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
               break;
            case "e337a": // Plead
               switch (dieRoll) // Based on the die roll, implement event
               {
                  case 1: gi.EventDisplayed = gi.EventActive = "e325"; break;                                               // pass with dignity
                  case 2: gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;  // combat
                  case 3: gi.EventDisplayed = gi.EventActive = "e340"; gi.DieRollAction = GameAction.EncounterRoll; break;  // looters
                  case 4: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // conversation 
                  case 5: gi.EventDisplayed = gi.EventActive = "e341"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e341"][0] = Utilities.NO_RESULT; break; // conversation       
                  case 6: gi.EventDisplayed = gi.EventActive = "e342"; gi.DieRollAction = GameAction.EncounterRoll; gi.DieResults["e342"][0] = Utilities.NO_RESULT; break; // inquiry
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e338a": // convince hirelings
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll <= gi.WitAndWile)
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
                           gi.ReduceCoins(wage);
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
               break;
            case "e338c": // convince hirelings
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll <= gi.WitAndWile)
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
                           gi.ReduceCoins(wage);
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
               break;
            case "e339a": // convince hirelings
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll < gi.WitAndWile)
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
                        gi.ReduceCoins(2);
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
               break;
            case "e339b": // failed to hire 
               switch (dieRoll) // Based on the die roll, implement event
               {
                  case 1: case 2: case 3: gi.EventDisplayed = gi.EventActive = "e325"; break;  // pass with dignity
                  case 4: case 5: case 6: gi.EventDisplayed = gi.EventActive = "e330"; gi.DieRollAction = GameAction.EncounterRoll; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
               }
               break;
            case "e339d": // convince hirelings
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll < gi.WitAndWile)
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
                        gi.ReduceCoins(2);
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
               break;
            case "e340": // looters
               if (true == gi.IsCharismaTalismanActive)
                  --dieRoll;
               if (true == gi.IsElfWitAndWileActive)
                  ++dieRoll;
               if (dieRoll <= (gi.WitAndWile + gi.MonkPleadModifier))
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
               break;
            case "e340a": // looters hostile
               switch (dieRoll) // Based on the die roll, implement event
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
                     gi.AddCoins(gi.LooterCoin, false); // Steal looter coin and return back to fold
                     gi.LooterCoin = 0;
                     if (false == EncounterEnd(gi, ref action))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): EncounterEnd() return false for ae=" + gi.EventActive);
                        return false;
                     }
                     break;
                  default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
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
                  int result = gi.DieResults["e341"][0];
                  if ("e016a" == gi.EventStart) // magician's home - if combat, use e016b
                  {
                     gi.IsMagicianProvideGift = false;
                     if ((2 == result) || (3 == result) || (4 == result))
                     {
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
                     case 2: gi.EventDisplayed = gi.EventActive = "e310"; gi.DieRollAction = GameAction.EncounterRoll; gi.IsAssassination = true;  break;  // attack
                     case 3: gi.EventDisplayed = gi.EventActive = "e308"; gi.DieRollAction = GameAction.EncounterRoll; break;  // attack
                     case 4: gi.EventDisplayed = gi.EventActive = "e305"; gi.DieRollAction = GameAction.EncounterRoll; break;  // attack 
                     case 5:                                                                                                   // bride to join - 10gp
                        gi.EventDisplayed = gi.EventActive = "e331";
                        gi.Bribe = 10;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
                        foreach (IMapItem mi in gi.EncounteredMembers)
                           mi.IsTownCastleTempleLeave = true;
                        break;
                     case 6:                                                                                                   // bride to hire
                        gi.EventDisplayed = gi.EventActive = "e332"; 
                        gi.Bribe = 5;
                        if (true == gi.IsMerchantWithParty)
                           gi.Bribe = (int)Math.Ceiling((double)gi.Bribe * 0.5);
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
                     case 12:                                                                                                  // plead comrades
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
                     default: Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): Reached default ae=" + gi.EventActive + " dr=" + dieRoll.ToString()); return false;
                  }
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "EncounterRoll(): 123 - Reached default ae=" + gi.EventActive);
               return false;
         }
         return true;
      }
      protected bool EncounterEnd(IGameInstance gi, ref GameAction action)
      {
         ITerritory princeTerritory = gi.Prince.Territory;
         gi.RemoveKilledInParty(gi.EventActive);
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
         gi.AddCoins(capturedCoin);
         if (false == gi.AddCoins(capturedCoin))
         {
            Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): AddCoins() returned false for es=" + gi.EventStart);
            return false;
         }
         gi.CapturedWealthCodes.Clear();
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
                  gi.RaftState = RaftEnum.RE_RAFT_ENDS_TODAY;
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
                  gi.IsShortHop = false;
                  gi.IsAirborne = false;
                  gi.IsAirborneEnd = true;
                  action = GameAction.TravelLostCheck;
                  gi.GamePhase = GamePhase.Travel;
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               else // used up movement
               {
                  isEndOfDay = true;
                  gi.IsShortHop = false;
                  foreach (IMapItem mi in gi.PartyMembers) // e151 - Cavalry departs at end of day after first day of travel
                  {
                     if (true == mi.Name.Contains("Cavalry"))
                     {
                        gi.RemoveAbandonerInParty(mi);
                        break;
                     }
                  }
                  if ("e114" != gi.EventActive)  // e114 - eagle hunt only helps one day of travel after event "e114"
                     gi.IsEagleHunt = false;
               }
               break;
            case GamePhase.SeekNews:
            case GamePhase.SeekHire:
            case GamePhase.SeekAudience:
            case GamePhase.SeekOffering:
            case GamePhase.SearchRuins:
               isEndOfDay = true;
               break;
            case GamePhase.Encounter:
               if (("e134" == gi.EventActive) && (null == gi.RuinsUnstable.Find(princeTerritory.Name))) // if this is unstable ruins, it stays unstable ruins
                  gi.RuinsUnstable.Add(princeTerritory);
               isEndOfDay = true;
               break;
            case GamePhase.Rest:
               if (false == gi.IsExhausted)
               {
                  foreach (IMapItem mi in gi.PartyMembers)
                  {
                     mi.HealWounds(1, 0); // EncounterEnd()
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
               if (true == gi.SecretClues.Contains(princeTerritory))
               {
                  gi.EventDisplayed = gi.EventActive = "e147a";
               }
               else if (true == gi.WizardAdviceLocations.Contains(princeTerritory))
               {
                  gi.EventDisplayed = gi.EventActive = "e026";
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): invalid state t=" + princeTerritory.Name + " gi.SunriseChoice=" + gi.SunriseChoice.ToString());
                  return false;
               }
               gi.SunriseChoice = gi.GamePhase = GamePhase.Encounter;
               gi.DieRollAction = GameAction.EncounterRoll;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "EncounterEnd(): reached default gi.SunriseChoice=" + gi.SunriseChoice.ToString());
               return false;
         }
         if ((true == isEndOfDay) || (true == gi.IsDayEnd))
         {
            if (RaftEnum.RE_RAFT_ENDS_TODAY == gi.RaftState)
            {
               if ((true == gi.IsRaftDestroyed) || (0==gi.GetCoins()) ) // e122 - raft destroyed on 12 
                  gi.RaftState = RaftEnum.RE_NO_RAFT;
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
            ITerritory t = gi.Territories.Find(name);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "AddGuideTerritories(): t=null for " + name);
               return false;
            }
            foreach (string adj in t.Adjacents)
            {
               ITerritory adjacent = gi.Territories.Find(adj);
               if (null == adjacent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "AddGuideTerritories(): adjacent=null for " + adj);
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
            ITerritory t = gi.Territories.Find(tName);
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
            else if( true == gi.DwarvenMines.Contains(t))
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
         gi.RaftState = RaftEnum.RE_NO_RAFT;
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
         gi.RaftState = RaftEnum.RE_NO_RAFT;
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
         gi.RaftState = RaftEnum.RE_NO_RAFT;
         gi.HydraTeethCount = 0;
         gi.ChagaDrugCount = 0;
         gi.Prince.ResetPartial();
      }
      protected void Imprisoned(IGameInstance gi)
      {
         gi.EventStart = gi.EventDisplayed = gi.EventActive = "e063";
         gi.IsJailed = true;
         gi.RaftState = RaftEnum.RE_NO_RAFT;
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
         foreach (ITerritory t in gi.Territories)
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
         foreach (ITerritory t in gi.Territories)
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
         foreach (ITerritory t in gi.Territories)
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
            foreach (ITerritory t in gi.Territories)
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
         if( null == minT )
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveToClosestGoblinKeep(): minT=null finding GoblinKeep closest to t=" + startT.Name);
            return false;
         }
         if (false == AddMapItemMove(gi, minT) )
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveToClosestGoblinKeep(): AddMapItemMove() returned error for t=" + minT.Name);
            return false;
         }
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