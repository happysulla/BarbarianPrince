using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbarianPrince
{
   public class GameFeats
   {
      public bool myIs500GoldWin;              // Nominal 
      public bool myIsNobleAllyWin;            // E152NobleAlly
      public bool myIsBlessedWin;             // E044HighAltarBlessed
      public bool myIsStaffOfCommandWin;      // E212Temple - Roll 12+1 on e212
      public bool myIsRoyalHelmWin;            // Treasure Table -  Row C:6 - Row Ca:6
      public bool myIsHuldraDefeatedInBattle;  // e144j
      public bool myIsHuldraDesposedWin;       // e211g
      public bool myHydraTeethVictory;         // use hydra teeth
      public bool myIsAirTravel;
      public bool myIsRaftTravel;
      public bool myIsArchTravel;
      public bool myIsDragonKiller;            // kill a dragon
      public bool myIsBanditKiller;            // kill 20 bandits
      public bool myIsOrcKiller;               // kill 30 orcs
      public bool myIsGoblinKiller;            // kill 40 goblins
      public bool myIsWolfKiller;              // kill 50 wolves
      public string[] myVisitedTowns;
      public string[] myVisitedTemples;
      public string[] myVisitedCastles;
      public int[] myVisitedSidesofBoard;      // visit each side of the board in any hex
      public bool myIsVisitAllTowns;
      public bool myIsVisitAllCastles;
      public bool myIsVisitAllTemples;
   };
}
