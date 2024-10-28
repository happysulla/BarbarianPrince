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
      public int myNumOfResurrection;           // number times the prince is resurrected
      public int myNumOfUncounscious;           // number times the prince is unconscious
      public int myDaysInJailorDungeon;
      public int myDaysLost;
      public int myNumEncounters;
      public int myMaxPartySize;
      public int myMaxPartyEndurance;
      public int myMaxPartyCombat;
      public int myNumOfRestDays;
      public int myNumOfAudienceAttempt;
      public int myNumOfAudience;
      public int myNumOfOffering;
      public int myNumOfPrinceHeal;             // Number of Endurance Prince Healed
      public int myNumOfPartyHeal;              // Number of Endurance Party Healed
      public int myNumOfPrinceKill;
      public int myNumOfPartyKill;
      public int myNumOfPartyKillEndurance;     // Total endurance of all killed monsters
      public int myNumOfPartyKillCombat;        // Total endurance of all killed monsters
      public int myNumOfPartyKilled;
      public int myNumStarvationDay;
      public int myNumAxeDeath;                 // Times Prince Killed by execution
      public int myNumRiverCrossingSuccess;
      public int myNumRiverCrossingFailure;
      public int myNumDaysOnRaft;
      public int myNumDaysAirborne;
      public int myNumDaysArchTravel;           // E045ArchOfTravel
      public int myGoldAtEnd;                   // Gold at end of game
      public int myFoodAtEnd;                   // Food at end of game
      public int myPartyCountEnd;               // Party member count at end of game
      public int myNumDaysEnd;
      public int myMaxNumDaysEnd;
      public int myMinNumDaysEnd;
      public int myMinNumCoinEnd;
      public int myMaxNumCoinEnd;
   };
}
