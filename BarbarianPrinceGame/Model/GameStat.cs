using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BarbarianPrince
{
   [Serializable]
   public class GameStat
   {
      public int myEndDaysCount;
      public int myEndCoinCount;                   // Coin at end of game
      public int myEndFoodCount;                   // Food at end of game
      public int myEndPartyCount;               // Party member count at end of game

      public int myDaysLost;
      public int myNumEncounters;
      public int myNumOfRestDays;
      public int myNumOfAudienceAttempt;
      public int myNumOfAudience;
      public int myNumOfOffering;
      public int myDaysInJailorDungeon;
      public int myNumRiverCrossingSuccess;
      public int myNumRiverCrossingFailure;
      public int myNumDaysOnRaft;
      public int myNumDaysAirborne;
      public int myNumDaysArchTravel;           // E045ArchOfTravel

      public int myMaxPartySize;
      public int myMaxPartyEndurance;
      public int myMaxPartyCombat;
      public int myNumOfPartyKilled;
      public int myNumOfPartyHeal;              // Number of Endurance Party Healed
      public int myNumOfPartyKill;
      public int myNumOfPartyKillEndurance;     // Total endurance of all killed monsters
      public int myNumOfPartyKillCombat;        // Total endurance of all killed monsters

      public int myNumOfPrinceKill;
      public int myNumOfPrinceHeal;             // Number of Endurance Prince Healed
      public int myNumOfPrinceStarveDays;
      public int myNumOfPrinceUncounscious;     // number times the prince is unconscious
      public int myNumOfPrinceResurrection;     // number times the prince is resurrected
      public int myNumOfPrinceAxeDeath;         // Times Prince Killed by execution
   };
}
