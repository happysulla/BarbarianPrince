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
      public int myNumGames = 0;                    // number of games this stat is tracking
      public int myNumWins = 0;                     // number of games won
      public int myEndDaysCount = 0;
      public int myEndCoinCount = 0;                // Coin at end of game
      public int myEndFoodCount = 0;                // Food at end of game
      public int myEndPartyCount = 0;               // Party member count at end of game

      public int myDaysLost = 0;
      public int myNumEncounters = 0;
      public int myNumOfRestDays = 0;
      public int myNumOfAudienceAttempt = 0;
      public int myNumOfAudience = 0;
      public int myNumOfOffering = 0;
      public int myDaysInJailorDungeon = 0;
      public int myNumRiverCrossingSuccess = 0;
      public int myNumRiverCrossingFailure = 0;
      public int myNumDaysOnRaft = 0;
      public int myNumDaysAirborne = 0;
      public int myNumDaysArchTravel = 0;           // E045ArchOfTravel

      public int myMaxPartySize = 0;
      public int myMaxPartyEndurance = 0;
      public int myMaxPartyCombat = 0;
      public int myNumOfPartyKilled = 0;
      public int myNumOfPartyHeal = 0;              // Number of Endurance Party Healed
      public int myNumOfPartyKill = 0;
      public int myNumOfPartyKillEndurance = 0;     // Total endurance of all killed monsters
      public int myNumOfPartyKillCombat = 0;        // Total endurance of all killed monsters

      public int myNumOfPrinceKill = 0;
      public int myNumOfPrinceHeal = 0;             // Number of Endurance Prince Healed
      public int myNumOfPrinceStarveDays = 0;
      public int myNumOfPrinceUncounscious = 0;     // number times the prince is unconscious
      public int myNumOfPrinceResurrection = 0;     // number times the prince is resurrected
      public int myNumOfPrinceAxeDeath = 0;         // Times Prince Killed by execution
      public GameStat()
      {

      }
      public void Clear()
      {
         myNumGames = 0;                    // Number of games this stat is tracking
         myEndDaysCount = 0;
         myEndCoinCount = 0;                // Coin at end of game
         myEndFoodCount = 0;                // Food at end of game
         myEndPartyCount = 0;               // Party member count at end of game

         myDaysLost = 0;
         myNumEncounters = 0;
         myNumOfRestDays = 0;
         myNumOfAudienceAttempt = 0;
         myNumOfAudience = 0;
         myNumOfOffering = 0;
         myDaysInJailorDungeon = 0;
         myNumRiverCrossingSuccess = 0;
         myNumRiverCrossingFailure = 0;
         myNumDaysOnRaft = 0;
         myNumDaysAirborne = 0;
         myNumDaysArchTravel = 0;           // E045ArchOfTravel

         myMaxPartySize = 0;
         myMaxPartyEndurance = 9;           // Prince always starts at 9
         myMaxPartyCombat = 8;              // Prince always starts at 8
         myNumOfPartyKilled = 0;
         myNumOfPartyHeal = 0;              // Number of Endurance Party Healed
         myNumOfPartyKill = 0;
         myNumOfPartyKillEndurance = 0;     // Total endurance of all killed monsters 
         myNumOfPartyKillCombat = 0;        // Total endurance of all killed monsters 

         myNumOfPrinceKill = 0;
         myNumOfPrinceHeal = 0;             // Number of Endurance Prince Healed
         myNumOfPrinceStarveDays = 0;
         myNumOfPrinceUncounscious = 0;     // number times the prince is unconscious
         myNumOfPrinceResurrection = 0;     // number times the prince is resurrected
         myNumOfPrinceAxeDeath = 0;         // Times Prince Killed by execution
      }
   }
}
