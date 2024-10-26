using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BarbarianPrince
{
   class GameFeats
   {
      bool myIsVisitAllTowns;
      bool myIsVisitAllCastles;
      bool myIsVisitAllTemples;
      string[] myVisitedTowns;
      string[] myVisitedTemples;
      string[] myVisitedCastles;
      bool myIs500GoldWin;              // Nominal 
      bool myIsNobleAllyWin;            // E152NobleAlly
      bool myIsBlessedWin;              // E044HighAltarBlessed
      bool myIsStaffOfCommandWin;       // E212Temple - Roll 12+1 on e212
      bool myIsRoyalHelmWin;            // Treasure Table -  Row C:6 - Row Ca:6
      bool myIsHuldraDefeatedInBattle;  // e144j
      bool myIsHuldraDesposedWin;       // e211g
      bool myHydraTeethVictory;
      bool myIsArchTraveled;
   };
   class GameStats
   {
      int myNumOfResurrection;           // number times the prince is resurrected
      int myNumOfUncounscious;           // number times the prince is unconscious
      int myGoldAtEnd;                   // Gold at end of game
      int myFoodAtEnd;                   // Food at end of game
      int myPartyMemberCount;            // party member count at end of game
      int myTimeInJailorDungeon;
      int myTimeLost;
      int myNumDays;
      int myTravelEncounters;
      int myMaxPartySize;
      int myMaxPartyEndurance;
      int myMaxPartyCombat;
      int myNumOfRestDays;
      int myNumOfAudienceAttempt;
      int myNumOfAudience;
      int myNumOfOffering;
      int myNumOfPrinceHeal;             // Number of Endurance Prince Healed
      int myNumOfPartyHeal;              // Number of Endurance Party Healed
      int myNumOfPrinceKill;
      int myNumOfPartyKill; 
      int myNumOfPartyKillEndurance;     // Total endurance of all killed monsters
      int myNumOfPartyKillCombat;        // Total endurance of all killed monsters
      int myNumOfPartyKilled;
      int myNumStarvationDay;
      int myNumAxeDeath;                 // Times Prince Killed by execution
      int myNumRiverCrossingSuccess;
      int myNumRiverCrossingFailure;
      int myNumDaysOnRaft;
      int myNumDaysAirborne;
      int myNumDaysArchTravel;           // E045ArchOfTravel
   };
}
